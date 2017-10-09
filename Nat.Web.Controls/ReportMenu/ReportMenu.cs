/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 16 мая 2008 г.
 * Copyright © JSC New Age Technologies 2008
 */

using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using Nat.Web.Controls.Properties;

namespace Nat.Web.Controls.ReportMenu
{
    public class ReportMenu : Menu
    {
        private readonly MenuItem _menuItem;
        private readonly Dictionary<string, string> _valueRedirctes = new Dictionary<string, string>();

        public event EventHandler<ReportMenuEventArgs> ReportMenuClick;

        public ReportMenu()
        {
            _menuItem = new MenuItem(Resources.SReport, "Report");
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Items.Add(_menuItem);
            StaticItemTemplate = new ButtonReport();
            MenuItemClick += menu_MenuItemClick;
        }

        public void AddMenu(string text, string value, string redirect)
        {
            if (_valueRedirctes.ContainsKey(value))
                throw new Exception("Value is not unique in " + ID);
            _valueRedirctes.Add(value, redirect);
            _menuItem.ChildItems.Add(new MenuItem(text, value));
        }

        private void menu_MenuItemClick(object sender, MenuEventArgs e)
        {
            Menu menu = sender as Menu;
            if(menu != null)
            {
                ReportMenuEventArgs args = new ReportMenuEventArgs(e.Item.Text, e.Item.Value, _valueRedirctes[e.Item.Value]);
                OnReportItemClick(args);
                if (!args.Cancel)
                    Redirect(args.Redircet);
            }
        }

        protected virtual void Redirect(string redirect)
        {
            Page.Response.Redirect(redirect);
        }

        protected virtual void OnReportItemClick(ReportMenuEventArgs args)
        {
            if (ReportMenuClick != null)
                ReportMenuClick(this, args);
        }
    }
}