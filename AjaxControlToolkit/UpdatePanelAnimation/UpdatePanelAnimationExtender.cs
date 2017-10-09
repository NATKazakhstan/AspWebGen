// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Web.UI;

[assembly: System.Web.UI.WebResource("AjaxControlToolkit.UpdatePanelAnimation.UpdatePanelAnimationBehavior.js", "text/javascript")]

namespace AjaxControlToolkit
{
    /// <summary>
    /// Extender used to play animations just before and after an UpdatePanel updates
    /// </summary>
    [Designer("AjaxControlToolkit.UpdatePanelAnimationDesigner, AjaxControlToolkit")]
    [RequiredScript(typeof(CommonToolkitScripts), 0)]
    [RequiredScript(typeof(AnimationScripts), 1)]
    [RequiredScript(typeof(AnimationExtender), 2)]
    [ClientScriptResource("AjaxControlToolkit.Animation.UpdatePanelAnimationBehavior", "AjaxControlToolkit.UpdatePanelAnimation.UpdatePanelAnimationBehavior.js")]
    [TargetControlType(typeof(UpdatePanel))]
    [ToolboxItem("System.Web.UI.Design.WebControlToolboxItem, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    [ToolboxBitmap(typeof(UpdatePanelAnimationExtender), "UpdatePanelAnimation.UpdatePanelAnimation.ico")]
    public class UpdatePanelAnimationExtender : AnimationExtenderControlBase
    {
        // Animations
        private Animation _updating;
        private Animation _updated;

        /// <summary>
        /// Animation played when an update begins
        /// </summary>
        [DefaultValue(null)]
        [Browsable(false)]
        [ExtenderControlProperty]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Animation OnUpdating
        {
            get { return GetAnimation(ref _updating, "OnUpdating"); }
            set { SetAnimation(ref _updating, "OnUpdating", value); }
        }

        /// <summary>
        /// Animation played when an update completes
        /// </summary>
        [DefaultValue(null)]
        [Browsable(false)]
        [ExtenderControlProperty]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Animation OnUpdated
        {
            get { return GetAnimation(ref _updated, "OnUpdated"); }
            set { SetAnimation(ref _updated, "OnUpdated", value); }
        }

        /// <summary>
        /// Change any AnimationTarget references from server control IDs into the ClientIDs
        /// that the animation scripts are expecting.  We also replace any static AnimationTargets
        /// of the Updated animation with dynamic properties.
        /// </summary>
        /// <param name="e">EventArgs</param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            ResolveControlIDs(_updating);
            ResolveControlIDs(_updated);
            ReplaceStaticAnimationTargets(_updating);
            ReplaceStaticAnimationTargets(_updated);
        }

        /// <summary>
        /// Replace any statically defined AnimationTarget properties with a corresponding
        /// TargetScript dynamic animation property
        /// </summary>
        /// <param name="animation">Animation</param>
        private void ReplaceStaticAnimationTargets(Animation animation)
        {
            if (animation == null)
            {
                return;
            }

            // Check if the Animation has an AnimationTarget property, but not AnimationTargetScript
            // or TargetScript properties
            string id;
            string script;
            if ((animation.Properties.TryGetValue("AnimationTarget", out id) && !string.IsNullOrEmpty(id)) &&
                (!animation.Properties.TryGetValue("AnimationTargetScript", out script) || string.IsNullOrEmpty(script)) &&
                (!animation.Properties.TryGetValue("TargetScript", out script) || string.IsNullOrEmpty(script)))
            {
                // Remove the AnimationTarget property and replace it with a dynamic wrapper
                animation.Properties.Remove("AnimationTarget");
                animation.Properties["TargetScript"] = string.Format(CultureInfo.InvariantCulture, "$get('{0}')", id);
            }

            // Replace any static animation targets on this Animation's children
            foreach (Animation child in animation.Children)
            {
                ReplaceStaticAnimationTargets(child);
            }
        }
    }
}