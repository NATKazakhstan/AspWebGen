using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Nat.Web.ReportManager.Kendo.Properties;

namespace Nat.Web.ReportManager.Kendo.Areas.Reports.ViewModels
{
    public class PluginViewModel
    {
        public PluginViewModel(string key, string reportGroup, IDictionary<string, int> dicIDs, IDictionary<int, string> hasHash)
        {
            Key = key;
            if (!dicIDs.ContainsKey(key))
            {
                id = key.GetHashCode();
                while (hasHash.ContainsKey(id))
                    id++;

                dicIDs[key] = id;
                hasHash[id] = key;
            }
            else
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
        public string PluginType { get; set; }
        public bool Visible { get; set; }
        public int? parentID { get; }
        public bool AllowRtfCustomExport { get; set; }
        public bool AllowWordExport { get; set; }
        public bool AllowExcelExport { get; set; }
        public bool AllowPdfExport { get; set; }

        public static IEnumerable<PluginViewModel> ParseGroups(string valueReportGroup, IDictionary<string, int> dicIDs, IDictionary<int, string> hasHash)
        {
            var list = valueReportGroup.Split('\\').Select(r => r.Replace("<0>", "")).ToList();
            var groups = new List<PluginViewModel>();
            for (var i = 0; i < list.Count; i++)
            {
                var model = new PluginViewModel(list[i], i == 0 ? null : list[i - 1], dicIDs, hasHash);
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