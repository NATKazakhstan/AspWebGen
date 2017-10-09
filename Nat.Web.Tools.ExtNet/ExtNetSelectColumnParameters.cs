namespace Nat.Web.Tools.ExtNet
{
    using System;
    using System.Web.UI;

    using Ext.Net;

    using Nat.Web.Controls.GenerationClasses;

    [Serializable]
    public class ExtNetSelectColumnParameters : SelectColumnParameters
    {
        [Serializable]
        public class ExtNetFieldInfo : FieldInfo
        {
            public ExtNetFieldInfo()
            {
            }

            public ExtNetFieldInfo(string fieldName, Control control)
                : base(fieldName, control)
            {
            }
        }

        [Serializable]
        public class ExtNetControlInfo : ControlInfo
        {
            public string GetValueScript { get; set; }
            public string SetValueScript { get; set; }
            public string SetLabelScript { get; set; }

            public Hidden HiddenField
            {
                set
                {
                    GetValueScript = value.ClientID + ".getValue()";
                    SetValueScript = value.ClientID + ".setValue({0})";
                }
            }

            public TextField Label
            {
                set
                {
                    SetLabelScript = value.ClientID + ".setValue({0})";
                }
            }
        }
    }
}
