using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace IAD.UI
{
    /// <summary>
    /// 统一处理页面在不同分辨率和 DPI 下的布局。
    /// 页面控件仍由 WinForms Designer 原生字段保存；这里只在运行时调整尺寸策略。
    /// </summary>
    internal static class ResponsiveLayoutManager
    {
        private const int CompactWidth = 1180;
        private const int CompactHeight = 720;
        private const int MinimumContentWidth = 1000;
        private const int MinimumContentHeight = 600;

        public static void Apply(UserControl page, Size hostSize)
        {
            if (page == null) return;

            int width = Math.Max(1, hostSize.Width);
            int height = Math.Max(1, hostSize.Height);
            bool narrow = width < CompactWidth;
            bool compactHeight = height < CompactHeight;
            bool veryCompact = width < 1020 || height < 620;

            page.SuspendLayout();
            try
            {
                page.AutoScroll = true;
                page.AutoScrollMinSize = veryCompact
                    ? new Size(MinimumContentWidth, MinimumContentHeight)
                    : Size.Empty;
                page.Padding = narrow
                    ? new Padding(8, 8, 8, 8)
                    : new Padding(12, 10, 8, 10);

                TableLayoutPanel root = Field<TableLayoutPanel>(page, "rootLayout");
                if (root != null)
                {
                    root.Margin = Padding.Empty;
                    root.Padding = Padding.Empty;
                    root.MinimumSize = veryCompact
                        ? new Size(
                            Math.Max(1, MinimumContentWidth - page.Padding.Horizontal),
                            Math.Max(1, MinimumContentHeight - page.Padding.Vertical))
                        : Size.Empty;
                }

                ApplyCommonControlRules(page, width);

                switch (page.GetType().Name)
                {
                    case "DashboardPage":
                        OptimizeDashboard(page, narrow, compactHeight);
                        break;
                    case "ProductDefinitionPage":
                        OptimizeProductDefinition(page, narrow, compactHeight);
                        break;
                    case "DatasetAnnotationPage":
                        OptimizeDatasetAnnotation(page, narrow, compactHeight);
                        break;
                    case "TemplateRecognitionPage":
                        OptimizeTemplateRecognition(page, narrow, compactHeight);
                        break;
                    case "TrainingModelsPage":
                        OptimizeTraining(page, narrow, compactHeight);
                        break;
                    case "RulesRecipePage":
                        OptimizeRulesRecipe(page, narrow, compactHeight);
                        break;
                    case "OnlineInspectionPage":
                        OptimizeOnlineInspection(page, narrow, compactHeight);
                        break;
                    case "TraceabilityPage":
                        OptimizeTraceability(page, narrow, compactHeight);
                        break;
                    case "SystemSettingsPage":
                        OptimizeSystemSettings(page, narrow, compactHeight);
                        break;
                }
            }
            finally
            {
                page.ResumeLayout(true);
            }
        }

        private static void OptimizeDashboard(UserControl page, bool narrow, bool compactHeight)
        {
            TableLayoutPanel root = Field<TableLayoutPanel>(page, "rootLayout");
            SetRow(root, 0, SizeType.Absolute, compactHeight ? 148F : 168F);
            SetRow(root, 1, SizeType.Absolute, compactHeight ? 112F : 132F);
            SetRow(root, 2, SizeType.Percent, 100F);

            TableLayoutPanel bottom = Field<TableLayoutPanel>(page, "bottomLayout");
            if (narrow)
            {
                SetColumn(bottom, 0, SizeType.Percent, 36F);
                SetColumn(bottom, 1, SizeType.Percent, 36F);
                SetColumn(bottom, 2, SizeType.Percent, 28F);
            }
            else
            {
                SetColumn(bottom, 0, SizeType.Percent, 38F);
                SetColumn(bottom, 1, SizeType.Percent, 38F);
                SetColumn(bottom, 2, SizeType.Percent, 24F);
            }

            TableLayoutPanel process = Field<TableLayoutPanel>(page, "processLayout");
            if (process != null)
            {
                process.Padding = narrow ? new Padding(2) : new Padding(6, 4, 6, 4);
            }

            SetRowsEvenly(Field<TableLayoutPanel>(page, "pendingLayout"));
        }

        private static void OptimizeProductDefinition(UserControl page, bool narrow, bool compactHeight)
        {
            TableLayoutPanel root = Field<TableLayoutPanel>(page, "rootLayout");
            SetRow(root, 0, SizeType.Absolute, compactHeight ? 42F : 48F);
            SetRow(root, 1, SizeType.Absolute, compactHeight ? 82F : 96F);
            SetRow(root, 2, SizeType.Percent, 62F);
            SetRow(root, 3, SizeType.Percent, 38F);

            // Keep the field row close to its label row. The designer's taller label
            // row combined with vertically centered editors left a conspicuous empty
            // band between the labels and their inputs on high-DPI displays.
            TableLayoutPanel basic = Field<TableLayoutPanel>(page, "basicLayout");
            SetRow(basic, 0, SizeType.Absolute, compactHeight ? 22F : 26F);
            SetRow(basic, 1, SizeType.Percent, 100F);

            string[] basicFieldNames =
            {
                "txtProductName",
                "txtProductCode",
                "txtImageSize",
                "txtProductsPerImage",
                "cboPose",
                "txtAcquisitionCondition"
            };
            foreach (string fieldName in basicFieldNames)
            {
                Control field = Field<Control>(page, fieldName);
                if (field == null) continue;
                field.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                field.Margin = new Padding(4, 2, 4, 2);
            }

            TableLayoutPanel command = Field<TableLayoutPanel>(page, "commandLayout");
            SetColumn(command, 0, SizeType.Percent, 100F);
            SetColumn(command, 1, SizeType.Absolute, narrow ? 285F : 400F);

            TableLayoutPanel middle = Field<TableLayoutPanel>(page, "middleLayout");
            SetColumn(middle, 0, SizeType.Percent, narrow ? 60F : 64F);
            SetColumn(middle, 1, SizeType.Percent, narrow ? 40F : 36F);

            TableLayoutPanel parameterStack = Field<TableLayoutPanel>(page, "parameterStack");
            SetRow(parameterStack, 0, SizeType.Percent, compactHeight ? 56F : 58F);
            SetRow(parameterStack, 1, SizeType.Percent, compactHeight ? 44F : 42F);

            SetFlowScroll(page, "commandButtons");
            SetFlowScroll(page, "templateButtons");
            SetFlowScroll(page, "defectButtons");
        }

        private static void OptimizeDatasetAnnotation(UserControl page, bool narrow, bool compactHeight)
        {
            TableLayoutPanel root = Field<TableLayoutPanel>(page, "rootLayout");
            SetRow(root, 0, SizeType.Absolute, compactHeight ? 42F : 46F);
            SetRow(root, 1, SizeType.Percent, 100F);
            SetRow(root, 2, SizeType.Absolute, compactHeight ? 108F : 128F);

            TableLayoutPanel body = Field<TableLayoutPanel>(page, "bodyLayout");
            SetColumn(body, 0, SizeType.Absolute, narrow ? 205F : 235F);
            SetColumn(body, 1, SizeType.Percent, 100F);
            SetColumn(body, 2, SizeType.Absolute, narrow ? 275F : 305F);

            TableLayoutPanel right = Field<TableLayoutPanel>(page, "rightLayout");
            SetRow(right, 0, SizeType.Percent, 29F);
            SetRow(right, 1, SizeType.Percent, 23F);
            SetRow(right, 2, SizeType.Percent, 24F);
            SetRow(right, 3, SizeType.Percent, 24F);

            SetRowsEvenly(Field<TableLayoutPanel>(page, "currentClassLayout"));
            SetRowsEvenly(Field<TableLayoutPanel>(page, "qualityLayout"));

            SetFlowScroll(page, "toolbar");
        }

        private static void OptimizeTemplateRecognition(UserControl page, bool narrow, bool compactHeight)
        {
            TableLayoutPanel root = Field<TableLayoutPanel>(page, "rootLayout");
            SetRow(root, 0, SizeType.Absolute, compactHeight ? 70F : 82F);
            SetRow(root, 1, SizeType.Percent, 100F);
            SetRow(root, 2, SizeType.Absolute, compactHeight ? 90F : 108F);

            TableLayoutPanel body = Field<TableLayoutPanel>(page, "bodyLayout");
            SetColumn(body, 0, SizeType.Absolute, narrow ? 230F : 270F);
            SetColumn(body, 1, SizeType.Percent, 100F);
            SetColumn(body, 2, SizeType.Absolute, narrow ? 285F : 320F);

            TableLayoutPanel center = Field<TableLayoutPanel>(page, "centerLayout");
            SetRow(center, 0, SizeType.Absolute, compactHeight ? 200F : 238F);
            SetRow(center, 1, SizeType.Percent, 100F);

            TableLayoutPanel right = Field<TableLayoutPanel>(page, "rightLayout");
            SetRow(right, 0, SizeType.Percent, compactHeight ? 64F : 68F);
            SetRow(right, 1, SizeType.Percent, compactHeight ? 36F : 32F);

            TableLayoutPanel fewShot = Field<TableLayoutPanel>(page, "fewShotLayout");
            if (fewShot != null && fewShot.ColumnStyles.Count >= 10)
            {
                if (narrow)
                {
                    SetColumn(fewShot, 0, SizeType.Absolute, 76F);
                    SetColumn(fewShot, 1, SizeType.Percent, 100F);
                    SetColumn(fewShot, 2, SizeType.Absolute, 60F);
                    SetColumn(fewShot, 3, SizeType.Absolute, 55F);
                    SetColumn(fewShot, 4, SizeType.Absolute, 88F);
                    SetColumn(fewShot, 5, SizeType.Absolute, 55F);
                    SetColumn(fewShot, 6, SizeType.Absolute, 82F);
                    SetColumn(fewShot, 7, SizeType.Absolute, 90F);
                    SetColumn(fewShot, 8, SizeType.Absolute, 48F);
                    SetColumn(fewShot, 9, SizeType.Absolute, 65F);
                }
                else
                {
                    SetColumn(fewShot, 0, SizeType.Absolute, 90F);
                    SetColumn(fewShot, 1, SizeType.Percent, 25F);
                    SetColumn(fewShot, 2, SizeType.Absolute, 75F);
                    SetColumn(fewShot, 3, SizeType.Absolute, 70F);
                    SetColumn(fewShot, 4, SizeType.Absolute, 105F);
                    SetColumn(fewShot, 5, SizeType.Absolute, 70F);
                    SetColumn(fewShot, 6, SizeType.Absolute, 95F);
                    SetColumn(fewShot, 7, SizeType.Absolute, 110F);
                    SetColumn(fewShot, 8, SizeType.Absolute, 55F);
                    SetColumn(fewShot, 9, SizeType.Absolute, 80F);
                }
            }

            SetFlowScroll(page, "actionPanel");
        }

        private static void OptimizeTraining(UserControl page, bool narrow, bool compactHeight)
        {
            TableLayoutPanel root = Field<TableLayoutPanel>(page, "rootLayout");
            SetRow(root, 0, SizeType.Percent, compactHeight ? 35F : 34F);
            SetRow(root, 1, SizeType.Percent, compactHeight ? 39F : 38F);
            SetRow(root, 2, SizeType.Percent, compactHeight ? 26F : 28F);

            TableLayoutPanel top = Field<TableLayoutPanel>(page, "topLayout");
            if (narrow)
            {
                SetColumn(top, 0, SizeType.Percent, 30F);
                SetColumn(top, 1, SizeType.Percent, 29F);
                SetColumn(top, 2, SizeType.Percent, 41F);
            }
            else
            {
                SetColumn(top, 0, SizeType.Percent, 27F);
                SetColumn(top, 1, SizeType.Percent, 27F);
                SetColumn(top, 2, SizeType.Percent, 46F);
            }

            TableLayoutPanel middle = Field<TableLayoutPanel>(page, "middleLayout");
            SetColumn(middle, 0, SizeType.Percent, narrow ? 43F : 40F);
            SetColumn(middle, 1, SizeType.Percent, narrow ? 57F : 60F);

            TableLayoutPanel bottom = Field<TableLayoutPanel>(page, "bottomLayout");
            SetColumn(bottom, 0, SizeType.Percent, narrow ? 32F : 31F);
            SetColumn(bottom, 1, SizeType.Percent, narrow ? 68F : 69F);
        }

        private static void OptimizeRulesRecipe(UserControl page, bool narrow, bool compactHeight)
        {
            TableLayoutPanel root = Field<TableLayoutPanel>(page, "rootLayout");
            SetRow(root, 0, SizeType.Percent, compactHeight ? 35F : 36F);
            SetRow(root, 1, SizeType.Percent, compactHeight ? 40F : 39F);
            SetRow(root, 2, SizeType.Percent, 25F);

            TableLayoutPanel top = Field<TableLayoutPanel>(page, "topLayout");
            SetColumn(top, 0, SizeType.Percent, narrow ? 60F : 58F);
            SetColumn(top, 1, SizeType.Percent, narrow ? 40F : 42F);

            TableLayoutPanel middle = Field<TableLayoutPanel>(page, "middleLayout");
            if (narrow)
            {
                SetColumn(middle, 0, SizeType.Percent, 36F);
                SetColumn(middle, 1, SizeType.Percent, 36F);
                SetColumn(middle, 2, SizeType.Percent, 28F);
            }
            else
            {
                SetColumn(middle, 0, SizeType.Percent, 38F);
                SetColumn(middle, 1, SizeType.Percent, 34F);
                SetColumn(middle, 2, SizeType.Percent, 28F);
            }

            SetRowsEvenly(Field<TableLayoutPanel>(page, "recipeLayout"));
            SetRowsEvenly(Field<TableLayoutPanel>(page, "acceptanceLayout"));
            SetRowsEvenly(Field<TableLayoutPanel>(page, "estimateLayout"));
        }

        private static void OptimizeOnlineInspection(UserControl page, bool narrow, bool compactHeight)
        {
            TableLayoutPanel root = Field<TableLayoutPanel>(page, "rootLayout");
            SetRow(root, 0, SizeType.Absolute, compactHeight ? 46F : 52F);
            SetRow(root, 1, SizeType.Percent, 100F);
            SetRow(root, 2, SizeType.Absolute, compactHeight ? 145F : 160F);

            TableLayoutPanel toolbarLayout = Field<TableLayoutPanel>(page, "toolbarLayout");
            SetColumn(toolbarLayout, 0, SizeType.Percent, 100F);
            SetColumn(toolbarLayout, 1, SizeType.Absolute, narrow ? 340F : 450F);

            TableLayoutPanel body = Field<TableLayoutPanel>(page, "bodyLayout");
            SetColumn(body, 0, SizeType.Absolute, narrow ? 200F : 235F);
            SetColumn(body, 1, SizeType.Percent, 100F);
            SetColumn(body, 2, SizeType.Absolute, narrow ? 270F : 305F);

            TableLayoutPanel right = Field<TableLayoutPanel>(page, "rightLayout");
            SetRow(right, 0, SizeType.Percent, compactHeight ? 66F : 70F);
            SetRow(right, 1, SizeType.Percent, compactHeight ? 34F : 30F);

            TableLayoutPanel bottom = Field<TableLayoutPanel>(page, "bottomLayout");
            if (narrow)
            {
                SetColumn(bottom, 0, SizeType.Percent, 44F);
                SetColumn(bottom, 1, SizeType.Percent, 26F);
                SetColumn(bottom, 2, SizeType.Percent, 30F);
            }
            else
            {
                SetColumn(bottom, 0, SizeType.Percent, 48F);
                SetColumn(bottom, 1, SizeType.Percent, 24F);
                SetColumn(bottom, 2, SizeType.Percent, 28F);
            }

            SetRowsEvenly(Field<TableLayoutPanel>(page, "pipelineLayout"));
            SetRowsEvenly(Field<TableLayoutPanel>(page, "backendLayout"));

            SetFlowScroll(page, "toolbar");
        }

        private static void OptimizeTraceability(UserControl page, bool narrow, bool compactHeight)
        {
            TableLayoutPanel root = Field<TableLayoutPanel>(page, "rootLayout");
            SetRow(root, 0, SizeType.Absolute, compactHeight ? 72F : 84F);
            SetRow(root, 1, SizeType.Percent, 100F);
            SetRow(root, 2, SizeType.Absolute, compactHeight ? 165F : 195F);

            TableLayoutPanel body = Field<TableLayoutPanel>(page, "bodyLayout");
            if (narrow)
            {
                SetColumn(body, 0, SizeType.Percent, 42F);
                SetColumn(body, 1, SizeType.Percent, 32F);
                SetColumn(body, 2, SizeType.Percent, 26F);
            }
            else
            {
                SetColumn(body, 0, SizeType.Percent, 43F);
                SetColumn(body, 1, SizeType.Percent, 35F);
                SetColumn(body, 2, SizeType.Percent, 22F);
            }

            TableLayoutPanel filter = Field<TableLayoutPanel>(page, "filterLayout");
            if (filter != null && filter.ColumnStyles.Count >= 11)
            {
                if (narrow)
                {
                    SetColumn(filter, 0, SizeType.Absolute, 60F);
                    SetColumn(filter, 1, SizeType.Percent, 26F);
                    SetColumn(filter, 2, SizeType.Absolute, 46F);
                    SetColumn(filter, 3, SizeType.Percent, 12F);
                    SetColumn(filter, 4, SizeType.Absolute, 46F);
                    SetColumn(filter, 5, SizeType.Percent, 12F);
                    SetColumn(filter, 6, SizeType.Absolute, 76F);
                    SetColumn(filter, 7, SizeType.Percent, 14F);
                    SetColumn(filter, 8, SizeType.Absolute, 88F);
                    SetColumn(filter, 9, SizeType.Percent, 36F);
                    SetColumn(filter, 10, SizeType.Absolute, 74F);
                }
                else
                {
                    SetColumn(filter, 0, SizeType.Absolute, 70F);
                    SetColumn(filter, 1, SizeType.Percent, 26F);
                    SetColumn(filter, 2, SizeType.Absolute, 55F);
                    SetColumn(filter, 3, SizeType.Percent, 12F);
                    SetColumn(filter, 4, SizeType.Absolute, 55F);
                    SetColumn(filter, 5, SizeType.Percent, 12F);
                    SetColumn(filter, 6, SizeType.Absolute, 90F);
                    SetColumn(filter, 7, SizeType.Percent, 14F);
                    SetColumn(filter, 8, SizeType.Absolute, 105F);
                    SetColumn(filter, 9, SizeType.Percent, 36F);
                    SetColumn(filter, 10, SizeType.Absolute, 90F);
                }
            }

            TableLayoutPanel bottom = Field<TableLayoutPanel>(page, "bottomLayout");
            SetColumn(bottom, 0, SizeType.Percent, narrow ? 36F : 38F);
            SetColumn(bottom, 1, SizeType.Percent, 42F);
            SetColumn(bottom, 2, SizeType.Percent, narrow ? 22F : 20F);

            SetRowsEvenly(Field<TableLayoutPanel>(page, "detailLayout"));

            SetFlowScroll(page, "exportPanel");
        }

        private static void OptimizeSystemSettings(UserControl page, bool narrow, bool compactHeight)
        {
            TableLayoutPanel root = Field<TableLayoutPanel>(page, "rootLayout");
            SetRow(root, 0, SizeType.Percent, compactHeight ? 35F : 34F);
            SetRow(root, 1, SizeType.Percent, compactHeight ? 34F : 33F);
            SetRow(root, 2, SizeType.Percent, compactHeight ? 31F : 33F);

            TableLayoutPanel row1 = Field<TableLayoutPanel>(page, "row1");
            SetColumn(row1, 0, SizeType.Percent, narrow ? 31F : 33F);
            SetColumn(row1, 1, SizeType.Percent, narrow ? 37F : 34F);
            SetColumn(row1, 2, SizeType.Percent, narrow ? 32F : 33F);

            TableLayoutPanel row2 = Field<TableLayoutPanel>(page, "row2");
            SetColumn(row2, 0, SizeType.Percent, narrow ? 31F : 33F);
            SetColumn(row2, 1, SizeType.Percent, narrow ? 36F : 34F);
            SetColumn(row2, 2, SizeType.Percent, 33F);

            TableLayoutPanel row3 = Field<TableLayoutPanel>(page, "row3");
            SetColumn(row3, 0, SizeType.Percent, narrow ? 73F : 78F);
            SetColumn(row3, 1, SizeType.Percent, narrow ? 27F : 22F);
        }

        private static void ApplyCommonControlRules(Control parent, int pageWidth)
        {
            foreach (Control control in parent.Controls)
            {
                GroupBox groupBox = control as GroupBox;
                if (groupBox != null)
                {
                    groupBox.BackColor = UiTheme.Surface;
                    groupBox.ForeColor = UiTheme.Text;
                }

                Label label = control as Label;
                if (label != null && control.Parent is TableLayoutPanel)
                {
                    // Designer-created labels keep their small design-time bounds by default.
                    // When a TableLayoutPanel is resized for the current DPI, that stale bound
                    // can collapse the text to a single character. Let the table cell own the
                    // label bounds and preserve any explicit horizontal alignment.
                    label.AutoSize = false;
                    label.AutoEllipsis = true;
                    label.Dock = DockStyle.Fill;
                    label.Margin = new Padding(4, 0, 4, 0);
                    label.TextAlign = WithMiddleVerticalAlignment(label.TextAlign);

                    if (string.Equals(label.Text, "→", StringComparison.Ordinal))
                    {
                        label.AutoEllipsis = false;
                        label.Margin = Padding.Empty;
                        label.TextAlign = ContentAlignment.MiddleCenter;
                    }
                }

                DataGridView grid = control as DataGridView;
                if (grid != null)
                {
                    ConfigureGrid(grid, pageWidth);
                }

                FlowLayoutPanel flow = control as FlowLayoutPanel;
                if (flow != null)
                {
                    flow.AutoScroll = true;
                }

                TextBox textBox = control as TextBox;
                if (textBox != null && !textBox.Multiline)
                {
                    textBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                }

                ComboBox comboBox = control as ComboBox;
                if (comboBox != null)
                {
                    comboBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                }

                NumericUpDown numeric = control as NumericUpDown;
                if (numeric != null)
                {
                    numeric.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                }

                Button button = control as Button;
                if (button != null)
                {
                    button.FlatStyle = FlatStyle.Flat;
                    button.BackColor = UiTheme.Surface;
                    button.ForeColor = UiTheme.Text;
                    button.Cursor = Cursors.Hand;
                    button.FlatAppearance.BorderColor = Color.FromArgb(178, 178, 178);
                    button.FlatAppearance.BorderSize = 1;
                    button.FlatAppearance.MouseOverBackColor = Color.FromArgb(236, 236, 236);

                    if (control.Parent is FlowLayoutPanel)
                    {
                        button.AutoSize = true;
                        button.MinimumSize = new Size(82, 30);
                        button.Margin = new Padding(3, 3, 3, 3);
                    }
                }

                if (control.HasChildren)
                {
                    ApplyCommonControlRules(control, pageWidth);
                }
            }
        }

        private static void ConfigureGrid(DataGridView grid, int pageWidth)
        {
            grid.ScrollBars = ScrollBars.Both;
            grid.BackgroundColor = UiTheme.Surface;
            grid.BorderStyle = BorderStyle.FixedSingle;
            grid.GridColor = UiTheme.SoftBorder;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.EnableHeadersVisualStyles = false;
            grid.RowHeadersVisible = false;
            grid.AllowUserToResizeRows = false;
            grid.MultiSelect = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.ColumnHeadersHeight = 32;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.RowTemplate.Height = 28;
            grid.ColumnHeadersDefaultCellStyle.BackColor = UiTheme.Header;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = UiTheme.Text;
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(4, 0, 4, 0);
            grid.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
            grid.DefaultCellStyle.BackColor = UiTheme.Surface;
            grid.DefaultCellStyle.ForeColor = UiTheme.Text;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(222, 232, 242);
            grid.DefaultCellStyle.SelectionForeColor = UiTheme.Text;
            grid.DefaultCellStyle.Padding = new Padding(4, 0, 4, 0);
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);

            int availableWidth = grid.ClientSize.Width;
            bool crampedGrid = grid.Columns.Count >= 4 &&
                               availableWidth > 0 &&
                               availableWidth / grid.Columns.Count < 88;
            if ((pageWidth < CompactWidth && grid.Columns.Count >= 7) || crampedGrid)
            {
                grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                foreach (DataGridViewColumn column in grid.Columns)
                {
                    column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    column.MinimumWidth = 72;
                    string header = column.HeaderText ?? string.Empty;
                    int width = 88;
                    if (header.Contains("批次") || header.Contains("时间") || header.Contains("版本") ||
                        header.Contains("模型") || header.Contains("路径") || header.Contains("Recipe") ||
                        header.Contains("Confidence"))
                    {
                        width = 108;
                    }
                    if (header.Contains("检测策略") || header.Contains("来源图像") || header.Contains("创建时间"))
                    {
                        width = 120;
                    }
                    column.Width = width;
                }
            }
            else
            {
                foreach (DataGridViewColumn column in grid.Columns)
                {
                    column.MinimumWidth = 5;
                    column.AutoSizeMode = DataGridViewAutoSizeColumnMode.NotSet;
                }
                grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
        }

        private static ContentAlignment WithMiddleVerticalAlignment(ContentAlignment alignment)
        {
            switch (alignment)
            {
                case ContentAlignment.TopCenter:
                case ContentAlignment.MiddleCenter:
                case ContentAlignment.BottomCenter:
                    return ContentAlignment.MiddleCenter;
                case ContentAlignment.TopRight:
                case ContentAlignment.MiddleRight:
                case ContentAlignment.BottomRight:
                    return ContentAlignment.MiddleRight;
                default:
                    return ContentAlignment.MiddleLeft;
            }
        }

        private static void SetFlowScroll(UserControl page, string fieldName)
        {
            FlowLayoutPanel flow = Field<FlowLayoutPanel>(page, fieldName);
            if (flow != null)
            {
                flow.AutoScroll = true;
                flow.WrapContents = false;
            }
        }

        private static void SetRowsEvenly(TableLayoutPanel layout)
        {
            if (layout == null || layout.RowCount <= 0) return;

            float height = 100F / layout.RowCount;
            for (int i = 0; i < layout.RowCount; i++)
            {
                SetRow(layout, i, SizeType.Percent, height);
            }
        }

        private static void SetRow(TableLayoutPanel layout, int index, SizeType type, float value)
        {
            if (layout == null || index < 0) return;
            while (layout.RowStyles.Count <= index)
            {
                layout.RowStyles.Add(new RowStyle());
            }
            layout.RowStyles[index].SizeType = type;
            layout.RowStyles[index].Height = value;
        }

        private static void SetColumn(TableLayoutPanel layout, int index, SizeType type, float value)
        {
            if (layout == null || index < 0) return;
            while (layout.ColumnStyles.Count <= index)
            {
                layout.ColumnStyles.Add(new ColumnStyle());
            }
            layout.ColumnStyles[index].SizeType = type;
            layout.ColumnStyles[index].Width = value;
        }

        private static T Field<T>(object instance, string name) where T : class
        {
            if (instance == null) return null;
            FieldInfo field = instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null) return null;
            return field.GetValue(instance) as T;
        }
    }
}
