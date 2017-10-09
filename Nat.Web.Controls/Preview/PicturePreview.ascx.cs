using System;
using System.Collections;
using System.Web.UI;
using System.Drawing;

namespace Nat.Web.Controls.Preview
{
    public partial class PicturePreview : UserControl
    {
        protected override void OnInit(System.EventArgs e)
        {
            base.OnInit(e);
            //инициализация списка размеров изображения
            var arSizes = new ArrayList();
            arSizes.Add(Properties.Resources.SNotSpecified);
            arSizes.Add("3х4");
            arSizes.Add(string.Format("3х4 {0}", Properties.Resources.SWithCorner));
            arSizes.Add("3.5х4.5");
            arSizes.Add(string.Format("3.5х4.5 {0}", Properties.Resources.SWithCorner));
            arSizes.Add("9х12");                       
            cbThumbSize.DataSource = arSizes;
            cbThumbSize.DataBind();
            //
            _ppcPreview.CancelControlID = ClientID + "_Close";
            cbThumbSize.Attributes["onchange"] = string.Format("SetSizeImgUrl(\"{0}\");return false;", ControlID);
            btnThumbSize.OnClientClick = string.Format("SetSizeImgEditUrl(\"{0}\");return false;", ControlID);
            btnThumbSize.Text = Properties.Resources.SSCApplyViewSettings;
        }

        protected override void OnPreRender(System.EventArgs e)
        {
            base.OnPreRender(e);            
            _img.Attributes["onclick"] = string.Format("$find('{0}').hide();", _ppcPreview.ModalPopupBehaviorID);
        }

        public string ControlID
        {
            get { return ClientID + "_div"; }
        }
    }
}