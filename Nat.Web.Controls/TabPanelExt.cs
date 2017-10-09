using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;
using AjaxControlToolkit;
using Nat.Web.Tools;

namespace Nat.Web.Controls
{
    public delegate void TPEChildControlsClearingHandler(object sender, CancelEventArgs e);
    public delegate void TPEChildControlsClearedHandler(object sender, EventArgs e);

    public delegate void TPEChildControlsDisablingViewStateHandler(object sender, CancelEventArgs e);
    public delegate void TPEChildControlsDisabledViewStateHandler(object sender, EventArgs e);

    public class TabPanelExt : TabPanel
    {
        public event TPEChildControlsClearingHandler ChildControlsClearing;
        public event TPEChildControlsClearedHandler ChildControlsCleared;
        public event TPEChildControlsDisablingViewStateHandler DisablingViewState;
        public event TPEChildControlsDisabledViewStateHandler DisabledViewState;

        [DefaultValue(true)]
        public bool NotRenderInactive
        {
            get { return (bool?)ViewState["NotRenderInactive"] ?? true; }
            set { ViewState["NotRenderInactive"] = value; }
        }

        protected override object SaveViewState()
        {
            if (NotRenderInactive)
            {
                TabContainer tabContainer = Parent as TabContainer;
                if (tabContainer != null && tabContainer.ActiveTab != this)
                {
                    CancelEventArgs args = new CancelEventArgs();
                    OnChildControlsDisablingViewState(args);
                    if (!args.Cancel)
                    {
                        foreach (Control control in Controls)
                            control.EnableViewState = false;
                        OnChildControlsDisabledViewState(EventArgs.Empty);
                    }
                }
            }
            return base.SaveViewState();
        }

        private void OnChildControlsDisabledViewState(EventArgs e)
        {
            if (DisabledViewState != null) DisabledViewState(this, e);
        }

        private void OnChildControlsDisablingViewState(CancelEventArgs e)
        {
            if (DisablingViewState != null) DisablingViewState(this, e);
        }

        protected virtual void OnChildControlsClearing(CancelEventArgs e)
        {
            if (ChildControlsClearing != null) ChildControlsClearing(this, e);
        }

        protected virtual void OnChildControlsCleared(EventArgs e)
        {
            if (ChildControlsCleared != null) ChildControlsCleared(this, e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Page.LoadComplete += Page_OnLoadComplete;
        }

        private void Page_OnLoadComplete(object sender, EventArgs e)
        {
            if (NotRenderInactive)
            {
                TabContainer tabContainer = Parent as TabContainer;
                if (tabContainer != null && tabContainer.ActiveTab != this)
                {
                    CancelEventArgs args = new CancelEventArgs();
                    OnChildControlsClearing(args);
                    if (!args.Cancel)
                    {
                        for (int i = 1; i < Controls.Count; i++)
                            Controls[i].Controls.Clear();
                        OnChildControlsCleared(EventArgs.Empty);
                    }
                }
            }
        }
        
        public static TabPanelExt FindInHierarchyUp(Control control)
        {
            return ControlHelper.FindControl<TabPanelExt>(control);
        }

        public static TabPanelExt[] FindAllInHierarchyUp(Control control)
        {
            List<TabPanelExt> panels = new List<TabPanelExt>();
            TabPanelExt panel = ControlHelper.FindControl<TabPanelExt>(control);
            while (panel != null)
            {
                panels.Add(panel);
                panel = ControlHelper.FindControl<TabPanelExt>(panel.Parent);
            }
            return panels.ToArray();
        }

    }
}