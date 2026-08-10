using System;

namespace IAD.Models
{
    public sealed class YoloTrainingRequest
    {
        public long ProductId { get; set; }
        public string ModelVariant { get; set; }
        public int ImageSize { get; set; }
        public int BatchSize { get; set; }
        public int Epochs { get; set; }
        public double LearningRate { get; set; }
        public string Device { get; set; }
        public int Seed { get; set; }
    }

    public sealed class YoloTrainingRun
    {
        public string RunCode { get; set; }
        public long ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ModelVariant { get; set; }
        public int ImageSize { get; set; }
        public int BatchSize { get; set; }
        public int Epochs { get; set; }
        public double LearningRate { get; set; }
        public string Device { get; set; }
        public string Status { get; set; }
        public string RunDirectory { get; set; }
        public string DatasetDirectory { get; set; }
        public string BestWeightsPath { get; set; }
        public string OnnxPath { get; set; }
        public long? ModelId { get; set; }
        public int? ExitCode { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? StartedAtUtc { get; set; }
        public DateTime? CompletedAtUtc { get; set; }
        public double Precision { get; set; }
        public double Recall { get; set; }
        public double Map50 { get; set; }
        public double Map5095 { get; set; }
        public double F1 { get; set; }
        public double InferenceMilliseconds { get; set; }
    }

    public sealed class YoloTrainingProgress
    {
        public string RunCode { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public int? Epoch { get; set; }
        public int? TotalEpochs { get; set; }
    }

    public sealed class YoloEnvironmentStatus
    {
        public bool IsReady { get; set; }
        public string PythonExecutable { get; set; }
        public string PythonVersion { get; set; }
        public string UltralyticsVersion { get; set; }
        public string TorchVersion { get; set; }
        public bool CudaAvailable { get; set; }
        public string DeviceName { get; set; }
        public string ErrorMessage { get; set; }
    }
}
