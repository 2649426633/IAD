using System;
using System.Collections.Generic;
using System.Linq;
using IAD.Models;

namespace IAD.Services
{
    internal sealed class RecipeRuleEngine
    {
        public string Evaluate(IList<DefectInstance> defects, IList<RecipeRule> rules)
        {
            IList<DefectInstance> items = defects ?? new List<DefectInstance>();
            IList<RecipeRule> activeRules = (rules ?? new List<RecipeRule>()).Where(r => r.IsEnabled).ToList();
            foreach (DefectInstance defect in items)
            {
                RecipeRule rule = activeRules.FirstOrDefault(r => string.Equals(r.CategoryCode, defect.CategoryCode, StringComparison.OrdinalIgnoreCase));
                if (rule == null)
                {
                    defect.Result = "NG";
                    defect.RuleDecision = "未配置规则，按 NG 处理";
                    continue;
                }
                bool qualified = defect.Confidence >= rule.MinConfidence && defect.Area >= rule.MinArea &&
                                 defect.Width >= rule.MinWidth && defect.Height >= rule.MinHeight;
                defect.Result = qualified ? "PENDING" : "OK";
                defect.RuleDecision = qualified ? "达到规则阈值" : "低于规则阈值";
            }

            foreach (RecipeRule rule in activeRules)
            {
                List<DefectInstance> matched = items.Where(d => d.Result == "PENDING" && string.Equals(d.CategoryCode, rule.CategoryCode, StringComparison.OrdinalIgnoreCase)).ToList();
                bool exceeded = matched.Count > Math.Max(0, rule.MaxAllowedCount);
                foreach (DefectInstance defect in matched)
                {
                    bool ng = exceeded && !string.Equals(rule.Decision, "IGNORE", StringComparison.OrdinalIgnoreCase) && !string.Equals(rule.Decision, "OK", StringComparison.OrdinalIgnoreCase);
                    defect.Result = ng ? "NG" : "OK";
                    defect.RuleDecision = ng
                        ? "命中 " + rule.CategoryName + " 规则：数量 " + matched.Count + " > 允许 " + rule.MaxAllowedCount
                        : "规则允许：数量 " + matched.Count + " ≤ " + rule.MaxAllowedCount;
                }
            }
            return items.Any(d => string.Equals(d.Result, "NG", StringComparison.OrdinalIgnoreCase)) ? "NG" : "OK";
        }
    }
}
