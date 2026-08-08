using System.Reflection;
using System.Windows.Forms;

namespace IAD.UI
{
    /// <summary>
    /// 统一保证所有页面根容器立即填满宿主区域；
    /// 对高密度页面再补充明确的 TableLayoutPanel 行定义。
    /// </summary>
    internal static class PageFillLayoutManager
    {
        public static void Apply(UserControl page)
        {
            if (page == null) return;

            page.Dock = DockStyle.Fill;
            page.Margin = Padding.Empty;
            page.AutoSize = false;

            FillRoot(Field<TableLayoutPanel>(page, "rootLayout"));

            string pageName = page.GetType().Name;
            if (pageName == "TrainingModelsPage")
            {
                ApplyTrainingModels(page);
            }
            else if (pageName == "SystemSettingsPage")
            {
                ApplySystemSettings(page);
            }
        }

        private static void ApplyTrainingModels(UserControl page)
        {
            FillSingleRow(Field<TableLayoutPanel>(page, "topLayout"));
            FillSingleRow(Field<TableLayoutPanel>(page, "middleLayout"));
            FillSingleRow(Field<TableLayoutPanel>(page, "bottomLayout"));

            FillRows(Field<TableLayoutPanel>(page, "configLayout"), 7);
            FillRows(Field<TableLayoutPanel>(page, "acceptanceLayout"), 5);
        }

        private static void ApplySystemSettings(UserControl page)
        {
            FillSingleRow(Field<TableLayoutPanel>(page, "row1"));
            FillSingleRow(Field<TableLayoutPanel>(page, "row2"));
            FillSingleRow(Field<TableLayoutPanel>(page, "row3"));

            FillRows(Field<TableLayoutPanel>(page, "runtimeLayout"), 6);
            FillRows(Field<TableLayoutPanel>(page, "storageLayout"), 6);
            FillRows(Field<TableLayoutPanel>(page, "offlineLayout"), 6);
            FillRows(Field<TableLayoutPanel>(page, "loggingLayout"), 5);
            FillRows(Field<TableLayoutPanel>(page, "backupLayout"), 5);
            FillRows(Field<TableLayoutPanel>(page, "versionLayout"), 6);
        }

        private static void FillRoot(TableLayoutPanel layout)
        {
            if (layout == null) return;
            layout.Dock = DockStyle.Fill;
            layout.Margin = Padding.Empty;
            layout.Padding = Padding.Empty;
            layout.AutoSize = false;
        }

        private static void FillSingleRow(TableLayoutPanel layout)
        {
            if (layout == null) return;

            layout.Dock = DockStyle.Fill;
            layout.Margin = Padding.Empty;
            layout.Padding = Padding.Empty;
            layout.AutoSize = false;
            layout.RowCount = 1;

            while (layout.RowStyles.Count < 1)
            {
                layout.RowStyles.Add(new RowStyle());
            }

            layout.RowStyles[0].SizeType = SizeType.Percent;
            layout.RowStyles[0].Height = 100F;

            while (layout.RowStyles.Count > 1)
            {
                layout.RowStyles.RemoveAt(layout.RowStyles.Count - 1);
            }
        }

        private static void FillRows(TableLayoutPanel layout, int rowCount)
        {
            if (layout == null || rowCount <= 0) return;

            layout.Dock = DockStyle.Fill;
            layout.Margin = Padding.Empty;
            layout.Padding = new Padding(4, 2, 4, 2);
            layout.AutoSize = false;
            layout.RowCount = rowCount;

            while (layout.RowStyles.Count < rowCount)
            {
                layout.RowStyles.Add(new RowStyle());
            }

            float rowHeight = 100F / rowCount;
            for (int i = 0; i < rowCount; i++)
            {
                layout.RowStyles[i].SizeType = SizeType.Percent;
                layout.RowStyles[i].Height = rowHeight;
            }

            while (layout.RowStyles.Count > rowCount)
            {
                layout.RowStyles.RemoveAt(layout.RowStyles.Count - 1);
            }
        }

        private static T Field<T>(object instance, string fieldName) where T : class
        {
            if (instance == null) return null;

            FieldInfo field = instance.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);

            return field == null ? null : field.GetValue(instance) as T;
        }
    }
}
