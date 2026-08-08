using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using IAD.Models;

namespace IAD.Services
{
    internal static class DefectCategoryCsv
    {
        private static readonly string[] Headers =
        {
            "CategoryCode",
            "CategoryName",
            "DefectType",
            "DetectionStrategy",
            "DefaultThreshold",
            "MinArea",
            "MinLength",
            "DisplayOrder",
            "IsEnabled"
        };

        public static void Write(string filePath, IList<DefectCategory> categories)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("导出文件路径不能为空。", "filePath");
            if (categories == null) throw new ArgumentNullException("categories");

            StringBuilder output = new StringBuilder();
            output.AppendLine(string.Join(",", Headers));
            foreach (DefectCategory category in categories)
            {
                string[] values =
                {
                    category.CategoryCode,
                    category.CategoryName,
                    category.DefectType,
                    category.DetectionStrategy,
                    category.DefaultThreshold.ToString("0.######", CultureInfo.InvariantCulture),
                    category.MinArea.ToString("0.######", CultureInfo.InvariantCulture),
                    category.MinLength.ToString("0.######", CultureInfo.InvariantCulture),
                    category.DisplayOrder.ToString(CultureInfo.InvariantCulture),
                    category.IsEnabled ? "true" : "false"
                };

                for (int i = 0; i < values.Length; i++) values[i] = Escape(values[i]);
                output.AppendLine(string.Join(",", values));
            }

            File.WriteAllText(filePath, output.ToString(), new UTF8Encoding(true));
        }

        public static IList<DefectCategory> Read(string filePath, long productId)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("导入文件路径不能为空。", "filePath");
            if (productId <= 0) throw new ArgumentException("产品Id无效。", "productId");

            string content = File.ReadAllText(filePath, Encoding.UTF8);
            IList<string[]> rows = ParseRows(content);
            if (rows.Count == 0) throw new InvalidOperationException("CSV 文件为空。");
            ValidateHeaders(rows[0]);

            List<DefectCategory> result = new List<DefectCategory>();
            HashSet<string> codes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int rowIndex = 1; rowIndex < rows.Count; rowIndex++)
            {
                string[] row = rows[rowIndex];
                if (IsBlankRow(row)) continue;
                if (row.Length != Headers.Length)
                    throw new InvalidOperationException("CSV 第" + (rowIndex + 1) + "行应包含" + Headers.Length + "列，实际为" + row.Length + "列。");

                string code = Required(row[0], "CategoryCode", rowIndex + 1);
                if (!codes.Add(code))
                    throw new InvalidOperationException("CSV 中存在重复的缺陷类别编码：" + code);

                double threshold = ParseDouble(row[4], "DefaultThreshold", rowIndex + 1);
                double minArea = ParseDouble(row[5], "MinArea", rowIndex + 1);
                double minLength = ParseDouble(row[6], "MinLength", rowIndex + 1);
                int displayOrder = ParseInt(row[7], "DisplayOrder", rowIndex + 1);
                if (threshold < 0 || threshold > 1)
                    throw new InvalidOperationException("CSV 第" + (rowIndex + 1) + "行 DefaultThreshold 必须在0到1之间。");
                if (minArea < 0 || minLength < 0)
                    throw new InvalidOperationException("CSV 第" + (rowIndex + 1) + "行 MinArea 和 MinLength 不能小于0。");
                if (displayOrder <= 0)
                    throw new InvalidOperationException("CSV 第" + (rowIndex + 1) + "行 DisplayOrder 必须大于0。");

                result.Add(new DefectCategory
                {
                    ProductId = productId,
                    CategoryCode = code,
                    CategoryName = Required(row[1], "CategoryName", rowIndex + 1),
                    DefectType = Required(row[2], "DefectType", rowIndex + 1),
                    DetectionStrategy = Required(row[3], "DetectionStrategy", rowIndex + 1),
                    DefaultThreshold = threshold,
                    MinArea = minArea,
                    MinLength = minLength,
                    DisplayOrder = displayOrder,
                    IsEnabled = ParseBoolean(row[8], rowIndex + 1)
                });
            }

            return result;
        }

        private static IList<string[]> ParseRows(string content)
        {
            List<string[]> rows = new List<string[]>();
            List<string> row = new List<string>();
            StringBuilder field = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < content.Length; i++)
            {
                char value = content[i];
                if (inQuotes)
                {
                    if (value == '"')
                    {
                        if (i + 1 < content.Length && content[i + 1] == '"')
                        {
                            field.Append('"');
                            i++;
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        field.Append(value);
                    }
                    continue;
                }

                if (value == '"')
                {
                    if (field.Length > 0) throw new InvalidOperationException("CSV 引号格式不正确。");
                    inQuotes = true;
                }
                else if (value == ',')
                {
                    row.Add(field.ToString());
                    field.Clear();
                }
                else if (value == '\r' || value == '\n')
                {
                    row.Add(field.ToString());
                    field.Clear();
                    rows.Add(row.ToArray());
                    row.Clear();
                    if (value == '\r' && i + 1 < content.Length && content[i + 1] == '\n') i++;
                }
                else
                {
                    field.Append(value);
                }
            }

            if (inQuotes) throw new InvalidOperationException("CSV 存在未闭合的引号。");
            if (field.Length > 0 || row.Count > 0)
            {
                row.Add(field.ToString());
                rows.Add(row.ToArray());
            }
            return rows;
        }

        private static void ValidateHeaders(string[] row)
        {
            if (row.Length != Headers.Length)
                throw new InvalidOperationException("CSV 表头列数不正确，应为：" + string.Join(",", Headers));
            for (int i = 0; i < Headers.Length; i++)
            {
                if (!string.Equals(row[i].Trim(), Headers[i], StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("CSV 第" + (i + 1) + "列表头应为 " + Headers[i] + "。");
            }
        }

        private static bool IsBlankRow(string[] row)
        {
            foreach (string value in row)
            {
                if (!string.IsNullOrWhiteSpace(value)) return false;
            }
            return true;
        }

        private static string Required(string value, string fieldName, int rowNumber)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException("CSV 第" + rowNumber + "行 " + fieldName + " 不能为空。");
            return value.Trim();
        }

        private static double ParseDouble(string value, string fieldName, int rowNumber)
        {
            double result;
            if ((!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result) && !double.TryParse(value, out result)) ||
                double.IsNaN(result) || double.IsInfinity(result))
            {
                throw new InvalidOperationException("CSV 第" + rowNumber + "行 " + fieldName + " 不是有效数值。");
            }
            return result;
        }

        private static int ParseInt(string value, string fieldName, int rowNumber)
        {
            int result;
            if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result) && !int.TryParse(value, out result))
                throw new InvalidOperationException("CSV 第" + rowNumber + "行 " + fieldName + " 不是有效整数。");
            return result;
        }

        private static bool ParseBoolean(string value, int rowNumber)
        {
            string normalized = (value ?? string.Empty).Trim();
            if (string.Equals(normalized, "true", StringComparison.OrdinalIgnoreCase) || normalized == "1" || normalized == "启用") return true;
            if (string.Equals(normalized, "false", StringComparison.OrdinalIgnoreCase) || normalized == "0" || normalized == "停用") return false;
            throw new InvalidOperationException("CSV 第" + rowNumber + "行 IsEnabled 应为 true/false、1/0 或 启用/停用。");
        }

        private static string Escape(string value)
        {
            value = value ?? string.Empty;
            if (value.IndexOfAny(new[] { ',', '"', '\r', '\n' }) < 0) return value;
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
    }
}
