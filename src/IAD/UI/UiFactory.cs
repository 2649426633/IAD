using System;
using System.Drawing;
using System.Windows.Forms;

namespace IAD.UI
{
    internal static class UiFactory
    {
        public static Panel Card(string title, Control body)
        {
            Panel card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UiTheme.Surface,
                Padding = new Padding(1),
                Margin = new Padding(0, 0, 10, 10)
            };
            card.Paint += delegate(object sender, PaintEventArgs e)
            {
                using (Pen pen = new Pen(UiTheme.Border))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, Math.Max(0, card.Width - 1), Math.Max(0, card.Height - 1));
                }
            };

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = UiTheme.Surface,
                ColumnCount = 1,
                RowCount = 2,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            Label titleLabel = new Label
            {
                Dock = DockStyle.Fill,
                Text = title,
                Font = UiTheme.Font(10.8F, true),
                ForeColor = UiTheme.Text,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(13, 0, 0, 0),
                BackColor = UiTheme.Surface
            };

            body.Dock = DockStyle.Fill;
            layout.Controls.Add(titleLabel, 0, 0);
            layout.Controls.Add(body, 0, 1);
            card.Controls.Add(layout);
            return card;
        }

        public static Label Label(string text, float size, bool bold, ContentAlignment alignment)
        {
            return new Label
            {
                Dock = DockStyle.Fill,
                Text = text,
                Font = UiTheme.Font(size, bold),
                ForeColor = UiTheme.Text,
                TextAlign = alignment,
                Margin = Padding.Empty
            };
        }

        public static Button Button(string text, int width)
        {
            Button button = new Button
            {
                Text = text,
                Width = width,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = UiTheme.Surface,
                ForeColor = UiTheme.Text,
                Font = UiTheme.Font(9.2F, false),
                Cursor = Cursors.Hand,
                TabStop = false,
                Margin = new Padding(0, 0, 8, 0)
            };
            button.FlatAppearance.BorderColor = Color.FromArgb(175, 175, 175);
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(236, 236, 236);
            return button;
        }

        public static DataGridView Grid(string[] headers, string[][] rows)
        {
            DataGridView grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = UiTheme.Surface,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeight = 31,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                GridColor = UiTheme.SoftBorder,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                EnableHeadersVisualStyles = false,
                ScrollBars = ScrollBars.Vertical
            };
            grid.RowTemplate.Height = 29;
            grid.ColumnHeadersDefaultCellStyle.BackColor = UiTheme.Header;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = UiTheme.Text;
            grid.ColumnHeadersDefaultCellStyle.Font = UiTheme.Font(8.5F, false);
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid.DefaultCellStyle.BackColor = UiTheme.Surface;
            grid.DefaultCellStyle.ForeColor = UiTheme.Text;
            grid.DefaultCellStyle.Font = UiTheme.Font(8.2F, false);
            grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(225, 225, 225);
            grid.DefaultCellStyle.SelectionForeColor = UiTheme.Text;

            for (int i = 0; i < headers.Length; i++)
            {
                grid.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "C" + i,
                    HeaderText = headers[i],
                    SortMode = DataGridViewColumnSortMode.NotSortable
                });
            }
            if (rows != null)
            {
                for (int r = 0; r < rows.Length; r++)
                {
                    grid.Rows.Add(rows[r]);
                }
            }
            return grid;
        }

        public static TableLayoutPanel KeyValues(string[,] data, int labelPercent)
        {
            int count = data.GetLength(0);
            TableLayoutPanel table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = count,
                BackColor = UiTheme.Surface,
                Padding = new Padding(12, 5, 12, 8),
                Margin = Padding.Empty
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, labelPercent));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100 - labelPercent));
            for (int i = 0; i < count; i++)
            {
                table.RowStyles.Add(new RowStyle(SizeType.Percent, 100F / count));
                Label key = Label(data[i, 0], 9F, false, ContentAlignment.MiddleLeft);
                key.ForeColor = UiTheme.Muted;
                Label value = Label(data[i, 1], 9.2F, true, ContentAlignment.MiddleLeft);
                table.Controls.Add(key, 0, i);
                table.Controls.Add(value, 1, i);
            }
            return table;
        }

        public static Panel Stat(string title, string value)
        {
            Panel panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UiTheme.Surface,
                Margin = new Padding(0, 0, 10, 0),
                Padding = new Padding(1)
            };
            panel.Paint += delegate(object sender, PaintEventArgs e)
            {
                using (Pen pen = new Pen(UiTheme.Border))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, Math.Max(0, panel.Width - 1), Math.Max(0, panel.Height - 1));
                }
            };

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = UiTheme.Surface,
                Padding = new Padding(8, 8, 8, 8)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 28F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 48F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 24F));
            layout.Controls.Add(Label(title, 10.2F, true, ContentAlignment.MiddleCenter), 0, 0);
            layout.Controls.Add(Label(value, 18F, false, ContentAlignment.MiddleCenter), 0, 1);
            Label time = Label("数据截止：2025-05-16 24:00", 7.8F, false, ContentAlignment.BottomCenter);
            time.ForeColor = UiTheme.Muted;
            layout.Controls.Add(time, 0, 2);
            panel.Controls.Add(layout);
            return panel;
        }

        public static Panel Stat(string title, string value, string note)
        {
            return Stat(title, value);
        }

        public static FlowLayoutPanel Toolbar(params string[] names)
        {
            FlowLayoutPanel bar = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = UiTheme.Page,
                Padding = new Padding(0, 6, 0, 6),
                Margin = Padding.Empty
            };
            for (int i = 0; i < names.Length; i++)
            {
                bar.Controls.Add(Button(names[i], Math.Max(78, 18 + names[i].Length * 15)));
            }
            return bar;
        }

        public static Panel PendingRow(string text, string count)
        {
            Panel panel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 41,
                BackColor = Color.FromArgb(252, 252, 252),
                Padding = new Padding(10, 0, 10, 0),
                Margin = new Padding(0, 0, 0, 6)
            };
            panel.Paint += delegate(object sender, PaintEventArgs e)
            {
                using (Pen pen = new Pen(UiTheme.SoftBorder))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, Math.Max(0, panel.Width - 1), Math.Max(0, panel.Height - 1));
                }
            };
            Label left = Label(text, 9.2F, false, ContentAlignment.MiddleLeft);
            left.Dock = DockStyle.Left;
            left.Width = 180;
            Label right = Label(count, 9.6F, false, ContentAlignment.MiddleRight);
            right.Dock = DockStyle.Right;
            right.Width = 70;
            panel.Controls.Add(right);
            panel.Controls.Add(left);
            return panel;
        }
    }
}
