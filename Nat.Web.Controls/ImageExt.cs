using System.Web.UI.WebControls;
using Nat.Web.Tools;

namespace Nat.Web.Controls
{
    public class ImageExt : Image
    {
        public override string ImageUrl
        {
            get { return base.ImageUrl; }
            set
            {
                var newWidth = ImageUtils.SaveProportion(value, (int)base.Height.Value, base.Page.Request);
                base.ImageUrl =
                    ImageUtils.GetThumbPath(value, (int)newWidth.Value, (int)base.Height.Value, base.Page.Request);
                base.Width = newWidth;
            }
        }
    }
}