using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Nat.Web.ReportManager.Kendo.Properties;

namespace Nat.Web.ReportManager.Kendo.Areas.Reports.ViewModels
{
    public class PluginViewModel
    {
        public PluginViewModel(string key, string reportGroup, IDictionary<string, int> dicIDs)
        {
            Key = key;
            if (!dicIDs.ContainsKey(key))
                dicIDs[key] = dicIDs.Count + 1;

            id = dicIDs[key];

            reportGroup = string.IsNullOrEmpty(reportGroup)
                ? null
                : reportGroup.Split('\\').Last().Replace("<0>", "");
            if (!string.IsNullOrEmpty(reportGroup) && dicIDs.ContainsKey(reportGroup))
                parentID = dicIDs[reportGroup];
        }

        public int id { get; }

        public string Key { get; }
        [Display(ResourceType = typeof(Resources), Name = nameof(Resources.SReportName))]
        public string Name { get; set; }
        public string PluginName { get; set; }
        public bool Visible { get; set; }
        public int? parentID { get; }

        public static IEnumerable<PluginViewModel> ParseGroups(string valueReportGroup, IDictionary<string, int> dicIDs)
        {
            var list = valueReportGroup.Split('\\').Select(r => r.Replace("<0>", "")).ToList();
            var groups = new List<PluginViewModel>();
            for (var i = 0; i < list.Count; i++)
            {
                var model = new PluginViewModel(list[i], i == 0 ? null : list[i - 1], dicIDs);
                model.Name = model.Key;
                model.Visible = true;
                groups.Add(model);
            }

            return groups;
        }

        public override string ToString()
        {
            return Key;
        }

        public override bool Equals(object obj)
        {
            var model = obj as PluginViewModel;
            if (model == null)
                return false;
            return model.Key == Key;
        }

        public override int GetHashCode()
        {
            return (Key ?? "").GetHashCode();
        }
    }
}