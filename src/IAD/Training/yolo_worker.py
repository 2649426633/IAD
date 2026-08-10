"""IAD YOLO26 training worker.

This process is intentionally isolated from WinForms so CUDA/PyTorch failures do
not terminate the inspection application. It writes one compact JSON result that
the .NET side validates before importing the ONNX model.
"""

from __future__ import annotations

import argparse
import json
import math
import os
import platform
import sys
import traceback
from pathlib import Path
from typing import Any


def write_json(path: str | Path, value: dict[str, Any]) -> None:
    target = Path(path)
    target.parent.mkdir(parents=True, exist_ok=True)
    temporary = target.with_suffix(target.suffix + ".tmp")
    temporary.write_text(json.dumps(value, ensure_ascii=False, indent=2), encoding="utf-8")
    temporary.replace(target)


def environment_status() -> dict[str, Any]:
    status: dict[str, Any] = {
        "IsReady": False,
        "PythonVersion": platform.python_version(),
        "UltralyticsVersion": "",
        "TorchVersion": "",
        "CudaAvailable": False,
        "DeviceName": "CPU",
        "ErrorMessage": "",
    }
    try:
        import torch
        import ultralytics

        status["TorchVersion"] = str(torch.__version__)
        status["UltralyticsVersion"] = str(ultralytics.__version__)
        status["CudaAvailable"] = bool(torch.cuda.is_available())
        if status["CudaAvailable"]:
            status["DeviceName"] = str(torch.cuda.get_device_name(0))
        status["IsReady"] = True
    except Exception as exc:  # reported to the desktop UI
        status["ErrorMessage"] = f"{type(exc).__name__}: {exc}"
    return status


def metric(metrics: dict[str, Any], *names: str) -> float:
    for name in names:
        value = metrics.get(name)
        if value is not None:
            try:
                number = float(value)
                return number if math.isfinite(number) else 0.0
            except (TypeError, ValueError):
                pass
    return 0.0


def train(args: argparse.Namespace) -> dict[str, Any]:
    from ultralytics import YOLO

    device: str | int | None
    if args.device.lower() == "auto":
        device = None
    elif args.device.isdigit():
        device = int(args.device)
    else:
        device = args.device

    weights_dir = Path(args.weights_dir or Path.cwd()).resolve()
    weights_dir.mkdir(parents=True, exist_ok=True)
    model_path = Path(args.model)
    if not model_path.is_absolute():
        model_path = weights_dir / model_path
    print(f"IAD: loading pretrained model {model_path}", flush=True)
    previous_directory = Path.cwd()
    try:
        os.chdir(weights_dir)
        model = YOLO(str(model_path))
    finally:
        os.chdir(previous_directory)
    result = model.train(
        data=str(Path(args.data).resolve()),
        epochs=args.epochs,
        imgsz=args.imgsz,
        batch=args.batch,
        lr0=args.lr0,
        device=device,
        seed=args.seed,
        deterministic=True,
        workers=0,
        cache=False,
        project=str(Path(args.project).resolve()),
        name=args.name,
        exist_ok=True,
        pretrained=True,
        optimizer="AdamW",
        plots=True,
        verbose=True,
    )

    save_dir = Path(str(result.save_dir)).resolve()
    best_path = save_dir / "weights" / "best.pt"
    if not best_path.exists():
        raise FileNotFoundError(f"best.pt was not generated: {best_path}")

    print("IAD: exporting best.pt to raw-output ONNX", flush=True)
    best_model = YOLO(str(best_path))
    export_kwargs: dict[str, Any] = {
        "format": "onnx",
        "imgsz": args.imgsz,
        "batch": 1,
        "dynamic": False,
        "simplify": True,
        "opset": 17,
        "nms": False,
        "device": device,
    }
    # YOLO26 defaults to an end-to-end [x1,y1,x2,y2,score,class] output.
    # IAD uses its own Recipe-aware NMS, so request the compatible raw tensor.
    export_kwargs["end2end"] = False
    onnx_path = Path(str(best_model.export(**export_kwargs))).resolve()
    if not onnx_path.exists():
        raise FileNotFoundError(f"ONNX export was not generated: {onnx_path}")

    names = best_model.names or {}
    labels = [str(names[index]) for index in sorted(names)] if isinstance(names, dict) else [str(item) for item in names]
    metrics = dict(getattr(result, "results_dict", {}) or {})
    speed = dict(getattr(result, "speed", {}) or {})
    return {
        "Success": True,
        "ErrorMessage": "",
        "BestWeightsPath": str(best_path),
        "OnnxPath": str(onnx_path),
        "Labels": labels,
        "Precision": metric(metrics, "metrics/precision(B)", "metrics/precision"),
        "Recall": metric(metrics, "metrics/recall(B)", "metrics/recall"),
        "Map50": metric(metrics, "metrics/mAP50(B)", "metrics/mAP50"),
        "Map5095": metric(metrics, "metrics/mAP50-95(B)", "metrics/mAP50-95"),
        "InferenceMilliseconds": metric(speed, "inference"),
    }


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="IAD Ultralytics YOLO training worker")
    parser.add_argument("--check", action="store_true")
    parser.add_argument("--model", default="yolo26n.pt")
    parser.add_argument("--data")
    parser.add_argument("--epochs", type=int, default=100)
    parser.add_argument("--imgsz", type=int, default=640)
    parser.add_argument("--batch", type=int, default=8)
    parser.add_argument("--lr0", type=float, default=0.01)
    parser.add_argument("--device", default="auto")
    parser.add_argument("--seed", type=int, default=42)
    parser.add_argument("--project")
    parser.add_argument("--weights-dir")
    parser.add_argument("--name", default="train")
    parser.add_argument("--result", required=True)
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    if args.check:
        status = environment_status()
        write_json(args.result, status)
        print(
            "IAD: Python {PythonVersion}, Ultralytics {UltralyticsVersion}, "
            "Torch {TorchVersion}, device {DeviceName}".format(**status),
            flush=True,
        )
        return 0 if status["IsReady"] else 2

    try:
        result = train(args)
        write_json(args.result, result)
        print("IAD: training and ONNX export completed", flush=True)
        return 0
    except Exception as exc:
        traceback.print_exc()
        write_json(
            args.result,
            {
                "Success": False,
                "ErrorMessage": f"{type(exc).__name__}: {exc}",
                "BestWeightsPath": "",
                "OnnxPath": "",
                "Labels": [],
                "Precision": 0.0,
                "Recall": 0.0,
                "Map50": 0.0,
                "Map5095": 0.0,
                "InferenceMilliseconds": 0.0,
            },
        )
        return 1


if __name__ == "__main__":
    sys.exit(main())
