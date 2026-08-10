using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using IAD.Models;
using IAD.Security;
using IAD.Services;

namespace IAD.Pages
{
    public partial class RulesRecipePage : UserControl
    {
        private sealed class ModelChoice
        {
            public InferenceModel Model { get; set; }
            public override string ToString() { return Model == null ? "（请选择 ONNX 模型）" : Model.Version + " · " + Model.ModelName + (Model.IsActive ? "（启用）" : ""); }
        }

        private bool runtimeInitialized;
        private InspectionRecipe currentRecipe;
        private ComboBox cboModel;
        private Button btnNewRecipe;
        private Button btnSaveRecipe;
        private Button btnActivateRecipe;
        private long loadedProductId;
        private long loadedRevision = -1;

        public RulesRecipePage() { InitializeComponent(); }

        public void InitializeRuntime()
        {
            if (runtimeInitialized) return;
            runtimeInitialized = true;
            BuildActions();
            ConfigureGrids();
            BindEvents();
            LoadRecipe();
        }

        private void BuildActions()
        {
            FlowLayoutPanel actions = new FlowLayoutPanel { Dock=DockStyle.Bottom, Height=68, Padding=new Padding(7,5,7,4), BackColor=Color.WhiteSmoke, AutoScroll=true };
            actions.Controls.Add(new Label { Text="推理模型", AutoSize=true, Margin=new Padding(3,8,4,0) });
            cboModel = new ComboBox { DropDownStyle=ComboBoxStyle.DropDownList, Width=210, Margin=new Padding(0,4,10,0) };
            btnNewRecipe = CreateButton("新建", 70);
            btnSaveRecipe = CreateButton("保存", 70);
            btnActivateRecipe = CreateButton("保存并启用", 100);
            actions.Controls.AddRange(new Control[] { cboModel, btnNewRecipe, btnSaveRecipe, btnActivateRecipe });
            grpRecipe.Controls.Add(actions);
            actions.BringToFront();
        }

        private static Button CreateButton(string text, int width) { return new Button { Text=text, Width=width, Height=29, Margin=new Padding(3,3,3,0) }; }

        private void ConfigureGrids()
        {
            dgvRules.AllowUserToAddRows = false;
            dgvRules.AllowUserToDeleteRows = false;
            dgvRules.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvRulesCol1.HeaderText="瑕疵类别"; dgvRulesCol2.HeaderText="区域"; dgvRulesCol3.HeaderText="最低置信度";
            dgvRulesCol4.HeaderText="最小面积"; dgvRulesCol5.HeaderText="最小宽度"; dgvRulesCol6.HeaderText="最小高度";
            dgvRulesCol7.HeaderText="允许数量"; dgvRulesCol8.HeaderText="超限判定";
            dgvRules.Columns[0].ReadOnly=true; dgvRules.Columns[1].ReadOnly=true;
            dgvThresholds.ReadOnly = true;
            dgvRecipeVersions.ReadOnly = true;
            dgvRecipeVersions.MultiSelect = false;
            dgvRecipeVersions.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            txtModelVersion.ReadOnly = true;
        }

        private void BindEvents()
        {
            btnNewRecipe.Click += delegate { NewRecipe(); };
            btnSaveRecipe.Click += delegate { SaveRecipe(false); };
            btnActivateRecipe.Click += delegate { SaveRecipe(true); };
            cboModel.SelectedIndexChanged += delegate
            {
                ModelChoice choice = cboModel.SelectedItem as ModelChoice;
                txtModelVersion.Text = choice == null || choice.Model == null ? string.Empty : choice.Model.Version;
            };
            dgvRecipeVersions.SelectionChanged += delegate
            {
                if (dgvRecipeVersions.SelectedRows.Count > 0)
                {
                    InspectionRecipe selected = dgvRecipeVersions.SelectedRows[0].Tag as InspectionRecipe;
                    if (selected != null && (currentRecipe == null || selected.Id != currentRecipe.Id)) ShowRecipe(selected);
                }
            };
            AppSession.CurrentProductChanged += delegate { if (!IsDisposed) { loadedProductId=0; LoadRecipe(); } };
            VisibleChanged += delegate { if (Visible && (loadedProductId!=AppSession.CurrentProductId || loadedRevision!=InspectionConfigurationRevisionTracker.GetRevision(AppSession.CurrentProductId))) LoadRecipe(); };
        }

        public void LoadRecipe()
        {
            if (!runtimeInitialized || IsDisposed) return;
            long productId = AppSession.CurrentProductId;
            loadedProductId=productId;
            loadedRevision=InspectionConfigurationRevisionTracker.GetRevision(productId);
            cboModel.Items.Clear();
            cboModel.Items.Add(new ModelChoice());
            dgvRecipeVersions.Rows.Clear();
            if (productId <= 0) { ClearEditor(); return; }
            foreach (InferenceModel model in AppServices.Models.GetModels(productId)) cboModel.Items.Add(new ModelChoice { Model=model });
            IList<InspectionRecipe> recipes = AppServices.Recipes.GetRecipes(productId);
            foreach (InspectionRecipe recipe in recipes)
            {
                int row = dgvRecipeVersions.Rows.Add(recipe.RecipeCode, recipe.IsActive ? "已启用" : "草稿", recipe.UpdatedAtUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm"), "本机");
                dgvRecipeVersions.Rows[row].Tag = recipe;
            }
            InspectionRecipe active = recipes.FirstOrDefault(r => r.IsActive) ?? recipes.FirstOrDefault();
            if (active == null) NewRecipe(); else ShowRecipe(active);
        }

        private void NewRecipe()
        {
            long productId = AppSession.CurrentProductId;
            if (productId <= 0) { ClearEditor(); return; }
            currentRecipe = new InspectionRecipe
            {
                ProductId=productId, RecipeCode="RCP-" + DateTime.Now.ToString("yyyyMMdd-HHmmss"), RecipeName="离线检测 Recipe",
                RuleVersion="RULE-" + DateTime.Now.ToString("yyyyMMddHHmm"), ThresholdVersion="TH-1.0.0"
            };
            ProductDefinitionSettings settings = AppServices.Products.GetDefinitionSettings(productId);
            currentRecipe.LocalizationTemplateVersion = settings.TemplateVersion;
            currentRecipe.CalibrationVersion = settings.CalibrationVersion;
            foreach (DefectCategory category in AppServices.Products.GetDefectCategories(productId).Where(c => c.IsEnabled).OrderBy(c => c.DisplayOrder))
            {
                currentRecipe.Rules.Add(new RecipeRule
                {
                    CategoryId=category.Id, CategoryCode=category.CategoryCode, CategoryName=category.CategoryName, RoiName="全图",
                    MinConfidence=category.DefaultThreshold, MinArea=category.MinArea, MinWidth=category.MinLength,
                    MinHeight=0, MaxAllowedCount=0, Decision="NG", IsEnabled=true
                });
            }
            ShowRecipe(currentRecipe);
        }

        private void ShowRecipe(InspectionRecipe recipe)
        {
            if (recipe.Rules.Count == 0 && recipe.ProductId > 0)
            {
                foreach (DefectCategory category in AppServices.Products.GetDefectCategories(recipe.ProductId).Where(c => c.IsEnabled).OrderBy(c => c.DisplayOrder))
                    recipe.Rules.Add(new RecipeRule { CategoryId=category.Id, CategoryCode=category.CategoryCode, CategoryName=category.CategoryName, RoiName="全图", MinConfidence=category.DefaultThreshold, MinArea=category.MinArea, MinWidth=category.MinLength, MaxAllowedCount=0, Decision="NG", IsEnabled=true });
            }
            currentRecipe = recipe;
            txtRecipeName.Text=recipe.RecipeName ?? ""; txtDatasetVersion.Text=recipe.DatasetVersion ?? "";
            txtLocalizationVersion.Text=recipe.LocalizationTemplateVersion ?? ""; txtModelVersion.Text=recipe.ModelVersion ?? "";
            txtRuleVersion.Text=recipe.RuleVersion ?? ""; txtCalibrationVersion.Text=recipe.CalibrationVersion ?? ""; txtThresholdVersion.Text=recipe.ThresholdVersion ?? "";
            SelectModel(recipe.ModelId);
            dgvRules.Rows.Clear(); dgvThresholds.Rows.Clear();
            foreach (RecipeRule rule in recipe.Rules)
            {
                int row = dgvRules.Rows.Add(rule.CategoryName + " [" + rule.CategoryCode + "]", rule.RoiName ?? "全图", rule.MinConfidence.ToString("0.00"),
                    rule.MinArea.ToString("0.###"), rule.MinWidth.ToString("0.###"), rule.MinHeight.ToString("0.###"), rule.MaxAllowedCount, rule.Decision);
                dgvRules.Rows[row].Tag = rule;
                dgvThresholds.Rows.Add(rule.CategoryName, rule.MinConfidence.ToString("0.00"), rule.MinArea.ToString("0.###"), rule.MaxAllowedCount, rule.MinWidth.ToString("0.###"), rule.Decision);
            }
        }

        private void SelectModel(long? modelId)
        {
            cboModel.SelectedIndex = 0;
            if (!modelId.HasValue) return;
            for (int i=1; i<cboModel.Items.Count; i++)
            {
                ModelChoice choice = cboModel.Items[i] as ModelChoice;
                if (choice != null && choice.Model.Id == modelId.Value) { cboModel.SelectedIndex=i; return; }
            }
        }

        private void SaveRecipe(bool activate)
        {
            if (currentRecipe == null || AppSession.CurrentProductId <= 0) return;
            try
            {
                ModelChoice choice = cboModel.SelectedItem as ModelChoice;
                if (choice == null || choice.Model == null) throw new InvalidOperationException("请选择 Recipe 使用的 ONNX 模型。");
                currentRecipe.RecipeName=txtRecipeName.Text; currentRecipe.DatasetVersion=txtDatasetVersion.Text;
                currentRecipe.LocalizationTemplateVersion=txtLocalizationVersion.Text; currentRecipe.ModelId=choice.Model.Id;
                currentRecipe.ModelVersion=choice.Model.Version; currentRecipe.RuleVersion=txtRuleVersion.Text;
                currentRecipe.CalibrationVersion=txtCalibrationVersion.Text; currentRecipe.ThresholdVersion=txtThresholdVersion.Text;
                currentRecipe.IsActive=activate || currentRecipe.IsActive;
                ReadRulesFromGrid(currentRecipe.Rules);
                AppServices.Recipes.SaveRecipe(currentRecipe);
                LoadRecipe();
                MessageBox.Show(this, activate ? "Recipe 已保存并启用，可开始离线检测。" : "Recipe 已保存。", "Recipe", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "保存 Recipe 失败", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void ReadRulesFromGrid(IList<RecipeRule> rules)
        {
            foreach (DataGridViewRow row in dgvRules.Rows)
            {
                RecipeRule rule = row.Tag as RecipeRule;
                if (rule == null) continue;
                rule.MinConfidence=ParseDouble(row.Cells[2].Value, "最低置信度");
                rule.MinArea=ParseDouble(row.Cells[3].Value, "最小面积");
                rule.MinWidth=ParseDouble(row.Cells[4].Value, "最小宽度");
                rule.MinHeight=ParseDouble(row.Cells[5].Value, "最小高度");
                rule.MaxAllowedCount=ParseInt(row.Cells[6].Value, "允许数量");
                rule.Decision=Convert.ToString(row.Cells[7].Value).Trim().ToUpperInvariant();
                if (rule.Decision != "NG" && rule.Decision != "OK" && rule.Decision != "IGNORE") throw new ArgumentException("超限判定只能填写 NG、OK 或 IGNORE。");
            }
        }

        private static double ParseDouble(object value, string name)
        {
            double result;
            if (!double.TryParse(Convert.ToString(value), NumberStyles.Float, CultureInfo.InvariantCulture, out result) && !double.TryParse(Convert.ToString(value), out result)) throw new ArgumentException(name + "不是有效数字。");
            return result;
        }
        private static int ParseInt(object value, string name) { int result; if (!int.TryParse(Convert.ToString(value), out result)) throw new ArgumentException(name + "不是有效整数。"); return result; }
        private void ClearEditor() { currentRecipe=null; dgvRules.Rows.Clear(); dgvThresholds.Rows.Clear(); txtRecipeName.Text="请先选择产品"; }
    }
}
