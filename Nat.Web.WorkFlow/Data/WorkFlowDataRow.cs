using System.Globalization;
using System.Resources;

using Nat.Web.Controls.GenerationClasses;

namespace Nat.Web.WorkFlow.Data
{
    using Nat.Web.Tools;

    public class WorkFlowDataRow<TStatus> : IDataRow
    {
        private string _nameRu;
        private string _nameKz;

        public WorkFlowDataRow()
        {
        }

        public WorkFlowDataRow(ResourceManager resourceManager, string resourceKey, TStatus status)
        {
            ResourceManager = resourceManager;
            ResourceKey = resourceKey;
            Status = status;
        }

        protected ResourceManager ResourceManager { get; set; }
        protected string ResourceKey { get; set; }
        public TStatus Status { get; set; }
        public TStatus id { get { return Status; } }
        public bool CanAddChild { get { return false; } }
        public bool CanEdit { get { return false; } }
        public bool CanDelete { get { return false; } }

        public string nameRu
        {
            get { return _nameRu ?? (_nameRu = ResourceManager.GetString(ResourceKey, new CultureInfo("ru-ru"))); }
            set { _nameRu = value; }
        }

        public string nameKz
        {
            get { return _nameKz ?? (_nameKz = ResourceManager.GetString(ResourceKey, new CultureInfo("kk-kz"))); }
            set { _nameKz = value; }
        }

        public string Name
        {
            get
            {
                if (ResourceManager == null)
                    return LocalizationHelper.IsCultureKZ ? _nameKz : _nameRu;
                return ResourceManager.GetString(ResourceKey);
            }
        }

        public string Value
        {
            get { return Status == null ? "" : Status.ToString(); }
        }

        public string[] GetAdditionalValues(SelectParameters selectParameters)
        {
            throw new System.NotImplementedException();
        }
    }
}