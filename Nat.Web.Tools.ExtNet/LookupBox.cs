namespace Nat.Web.Controls.ExtNet
{
    using Ext.Net;

    public class LookupBox : ComboBox
    {
        private string tableName;
        private string windowID;
        private string lookupFiltersID;
        private string lookupAddFieldsID;
        private string viewMode;
        private string selectMode;
        private string nullValue;
        private string nullText;
        private string windowTitle;
        private bool multipleSelect;
        private string frameInitializationFunction;

        public LookupBox()
            : base(GetConfig())
        {
            AddTriggers();
        }

        public LookupBox(Config config)
            : base(InitializeConfig(config))
        {
            AddTriggers();
        }

        private static Config GetConfig()
        {
            return InitializeConfig(new Config());
        }

        private static Config InitializeConfig(Config config)
        {
            config.QueryParam = "prefixText";
            config.QueryMode = DataLoadMode.Remote;
            config.PageSize = 10;
            config.MinChars = 1;
            config.HideBaseTrigger = true;
            config.ListConfig = new BoundList
                                {
                                    LoadingText = Properties.Resources.SSearchLoading,
                                    ItemTpl = new XTemplate
                                              {
                                                  Html = "<div class=\"search-item\">{RowName}</div>",
                                              }
                                };
            return config;
        }

        private void AddTriggers()
        {
            Triggers.Add(
               new FieldTrigger
               {
                   Icon = TriggerIcon.Search,
                   Qtip = Tools.ExtNet.Properties.Resources.SOpen,
                   Tag = "open",
               });
            Triggers.Add(
                new FieldTrigger
                {
                    Icon = TriggerIcon.Clear,
                    Qtip = Tools.ExtNet.Properties.Resources.SClear,
                    Tag = "remove",
                });
        }

        public string TableName
        {
            get { return tableName; }
            set
            {
                tableName = value;
                InitClickHandler();
            }
        }

        public string WindowID
        {
            get { return windowID; }
            set
            {
                windowID = value;
                InitClickHandler();
            }
        }

        public string LookupFiltersID
        {
            get { return lookupFiltersID; }
            set
            {
                lookupFiltersID = value;
                InitClickHandler();
            }
        }

        public string LookupAddFieldsID
        {
            get { return lookupAddFieldsID; }
            set
            {
                lookupAddFieldsID = value;
                InitClickHandler();
            }
        }

        public string ViewMode
        {
            get { return viewMode; }
            set
            {
                viewMode = value;
                InitClickHandler();
            }
        }

        public string SelectMode
        {
            get { return selectMode; }
            set
            {
                selectMode = value;
                InitClickHandler();
            }
        }

        public string NullValue
        {
            get { return nullValue; }
            set
            {
                nullValue = value;
                InitClickHandler();
            }
        }

        public string NullText
        {
            get { return nullText; }
            set
            {
                nullText = value;
                InitClickHandler();
            }
        }

        public string WindowTitle
        {
            get { return windowTitle; }
            set
            {
                windowTitle = value;
                InitClickHandler();
            }
        }

        public bool MultipleSelect
        {
            get { return multipleSelect; }
            set
            {
                multipleSelect = value;
                InitClickHandler();
            }
        }

        public string FrameInitializationFunction
        {
            get { return frameInitializationFunction; }
            set
            {
                frameInitializationFunction = value;
                InitClickHandler();
            }
        }

        public string AddNewFunctionName { get; private set; }

        private void InitClickHandler()
        {
            Listeners.TriggerClick.Handler =
                string.Format(
                    "LookupBoxSearchClick(this, trigger, tag, true, {0}, {1}, {2}, '{7}', '{3}', '{4}', {5}, '{6}', {8}, {9});",
                    LookupAddFieldsID == null ? "null" : "#{" + LookupAddFieldsID + "}",
                    LookupFiltersID == null ? "null" : "#{" + LookupFiltersID + "}",
                    "#{" + (WindowID ?? "ModalWindow") + "}",
                    GetUrl(),
                    "ref" + TableName,
                    NullValue ?? "null",
                    NullText,
                    WindowTitle,
                    AddNewFunctionName ?? "function(){}",
                    FrameInitializationFunction ?? "null");
            Listeners.Blur.Handler = "return !#{" + (WindowID ?? "ModalWindow") + "}.isVisible();";
        }

        private string GetUrl()
        {
            var url = new MainPageUrlBuilder
                      {
                          Page = "MainPage",
                          UserControl = TableName + "Journal",
                          IsDataControl = true,
                          IsSelect = true,
                          IsMultipleSelect = MultipleSelect,
                          SelectMode = SelectMode ?? "none",
                          ViewMode = ViewMode ?? "none",
                      };
            return url.CreateUrl();
        }

        public void AddNewRowToDictionary(string addToolTip, string addFunctionName)
        {
            Triggers.Insert(
                0,
                new FieldTrigger
                    {
                        Icon = TriggerIcon.SimplePlus,
                        Qtip = addToolTip,
                        Tag = "add",
                    });
            AddNewFunctionName = addFunctionName;
            InitClickHandler();
        }
    }
}
