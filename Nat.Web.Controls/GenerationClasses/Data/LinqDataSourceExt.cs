using System.Web.UI.WebControls;

namespace Nat.Web.Controls.GenerationClasses.Data
{
    /*[DefaultProperty("ContextTypeName"), 
    //Designer("System.Web.UI.Design.WebControls.LinqDataSourceDesigner, System.Web.Extensions.Design, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"), ParseChildren(true), ResourceDescription("LinqDataSource_Description"), ResourceDisplayName("LinqDataSource_DisplayName"), ToolboxItemFilter("System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", ToolboxItemFilterType.Require), 
    PersistChildren(false), ToolboxBitmap(typeof(LinqDataSource), "LinqDataSource.ico"), DefaultEvent("Selecting"), AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal), AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    */

    public class LinqDataSourceExt : LinqDataSource
    {
        protected override LinqDataSourceView CreateView()
        {
            return new LinqDataSourceViewExt(this, "DefaultView", Context);
        }

        /// <summary>
        /// Для решения проблемы обновления поля, при использовании процедуры.
        /// </summary>
        public bool UpdateNewObject
        {
            get { return View.UpdateNewObject; }
            set { View.UpdateNewObject = value; }
        }

        internal LinqDataSourceViewExt View
        {
            get { return (LinqDataSourceViewExt)base.GetView("DefaultView"); }
        }
    }
}
