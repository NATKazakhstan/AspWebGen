using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Resources;
using System.Web.UI;

namespace Nat.Web.Controls.GenerationClasses.Navigator
{
    public class BaseMenu : Control
    {
        public BaseMenu()
        {
            MenuItems = new List<BaseMenuItem>();
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public List<BaseMenuItem> MenuItems { get; set; }

        public ResourceManager ResourceManager { get; protected set; }
        public object CurrentObject { get; set; }
        public object[] ListObjects { get; set; }
        public MainPageUrlBuilder Url { get; set; }
        public BaseNavigatorControl BaseNavigatorControl { get; set; }
        public string HeaderResourceKey { get; set; }

        public string Header
        {
            get
            {
                return ResourceManager.GetString(HeaderResourceKey);
            }
        }

        public string HeaderRu
        {
            get
            {
                return ResourceManager.GetString(HeaderResourceKey, new CultureInfo("ru-ru"));
            }
        }

        public string HeaderKz
        {
            get
            {
                return ResourceManager.GetString(HeaderResourceKey, new CultureInfo("kk-kz"));
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            foreach (var menuItem in MenuItems)
                menuItem.InitMenu();
        }

        protected override void Render(HtmlTextWriter writer)
        {
            foreach (var menuItem in MenuItems)
                menuItem.Render(writer, 0);
        }

        public string GetMenuButton(string selectedKey)
        {
            return null;
        }
    }
}
