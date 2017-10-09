/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 23 марта 2009 г.
 * Copyright © JSC New Age Technologies 2009
 */

using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;
using Nat.Web.Tools;

[assembly: WebResource("Nat.Web.Controls.CopyValue.js", "text/javascript")]

namespace Nat.Web.Controls.BaseLogic
{
    [ClientScriptResource("Nat.Web.Controls.CopyValue", "Nat.Web.Controls.CopyValue.js")]
    public class NameCopy : BaseLogic, IScriptControl
    {
        public NameCopy()
        {
            SkipProperties = new List<string>();
            AdditionalProperties = new List<Props>();
        }

        public NameCopy(IEnumerable<Props> additionalProperties)
        {
            SkipProperties = new List<string>();
            AdditionalProperties = new List<Props>(additionalProperties);
        }

        public NameCopy(IEnumerable<Props> additionalProperties, IEnumerable<string> skipProperties)
        {
            SkipProperties = new List<string>(skipProperties);
            AdditionalProperties = new List<Props>(additionalProperties);
        }

        public IList<string> SkipProperties { get; private set; }
        public IList<Props> AdditionalProperties { get; private set; }
        public bool SkipAutoDetect { get; set; }
        private bool _registerScripts;
        private static readonly Dictionary<Type, IList<Props>> props = new Dictionary<Type, IList<Props>>();

        public override void Logic()
        {
            _registerScripts = IsNew || IsEdit;
        }

        private IList<Props> GetProps()
        {
            var type = ControlInfo.GetType();
            lock (props)
            {
                if (props.ContainsKey(type))
                    return props[type];
                var properties = type.GetProperties();
                var list = new List<Props>();
                if(!SkipAutoDetect)
                    foreach (var property in properties)
                    {
                        if (SkipProperties.Contains(property.Name)) continue;
                        if (property.Name.Equals("nameRuControl") && typeof(TextBox).IsAssignableFrom(property.PropertyType))
                        {
                            foreach (var stringProp in (new[] { "nameRuRodControl", "nameRuDatControl", "nameRuTvorControl", "nameRuVinControl", "nameRuToControl", "nameRuComeControl", "nameRuPredControl" }))
                            {
                                var propertyTo = type.GetProperty(stringProp);
                                if (propertyTo != null)
                                    list.Add(new Props { ChangeValueIfNull = true, From = property, To = propertyTo });
                            }
                        }
                        else if (property.Name.Equals("nameKzControl") && typeof(TextBox).IsAssignableFrom(property.PropertyType))
                        {
                            foreach (var stringProp in (new[] { "nameKzRodControl", "nameKzDatControl", "nameKzTvorControl", "nameKzVinControl", "nameKzToControl", "nameKzComeControl", "nameKzPredControl", "nameKzIshControl" }))
                            {
                                var propertyTo = type.GetProperty(stringProp);
                                if (propertyTo != null)
                                    list.Add(new Props { ChangeValueIfNull = true, From = property, To = propertyTo });
                            }
                        }
                        if (property.Name.StartsWith("nameRu") && property.Name.EndsWith("Control") && typeof(TextBox).IsAssignableFrom(property.PropertyType))
                        {
                            var propertyTo = type.GetProperty("nameKz" + property.Name.Substring(6));
                            if (propertyTo != null)
                                list.Add(new Props { ChangeValueIfNull = true, From = property, To = propertyTo });
                        }
                        else if (property.Name.EndsWith("RuControl") && typeof(TextBox).IsAssignableFrom(property.PropertyType))
                        {
                            var propertyTo = type.GetProperty(property.Name.Substring(0, property.Name.Length - 9) + "KzControl");
                            if (propertyTo != null)
                                list.Add(new Props { ChangeValueIfNull = true, From = property, To = propertyTo });
                        }
                    }
                foreach (var property in AdditionalProperties)
                {
                    if (property.From == null && property.FromProperty != null)
                        property.From = type.GetProperty(property.FromProperty);
                    if (property.To == null && property.ToProperty != null)
                        property.To = type.GetProperty(property.ToProperty);
                    if (property.From != null && property.To != null)
                        list.Add(property);
                }
                //list.AddRange(AdditionalProperties);
                props[type] = list;
                return list;
            }
        }

        public class Props
        {
            public Props()
            {
            }

            public Props(string fromProperty, string toProperty) : this()
            {
                FromProperty = fromProperty;
                ToProperty = toProperty;
                ChangeValueIfNull = true;
            }

            public Props(string fromProperty, string toProperty, bool changeValueIfNull)
                : this()
            {
                ChangeValueIfNull = changeValueIfNull;
                FromProperty = fromProperty;
                ToProperty = toProperty;
            }

            public PropertyInfo From;
            public PropertyInfo To;
            public bool ChangeValueIfNull;
            public string FromProperty;
            public string ToProperty;
        }

        IEnumerable<ScriptDescriptor> IScriptControl.GetScriptDescriptors()
        {
            if(_registerScripts)
            {
                var enumerable = GetProps()/*.Where(p => !SkipProperties.Contains(p.From.Name))*/;
                int i = 0;
                foreach (var prop in enumerable)
                {
                    if (prop.From == null || prop.To == null) continue;
                    
                    var fromControl = (Control) prop.From.GetValue(ControlInfo, null);
                    var toControl = (Control)prop.To.GetValue(ControlInfo, null);
                    if (fromControl == null || toControl == null) continue;

                    var fromClientElement = fromControl as IClientElementProvider;
                    string fromClientID;
                    if (fromClientElement != null)
                        fromClientID = fromClientElement.GetInputElements().First();
                    else 
                        fromClientID = fromControl.ClientID;

                    var toClientElement = toControl as IClientElementProvider;
                    string toClientID;
                    if (toClientElement != null)
                        toClientID = toClientElement.GetInputElements().First();
                    else
                        toClientID = toControl.ClientID;

                    var desc = new ScriptBehaviorDescriptor("Nat.Web.Controls.CopyValue", fromClientID)
                                   {
                                       ID = String.Format("CopyValue_{0}_{1}", i++, fromControl.ID)
                                   };
                    desc.AddProperty("kzId", toClientID);
                    desc.AddProperty("changeValueIfNull", prop.ChangeValueIfNull);
                    yield return desc;
                }
            }
        }

        IEnumerable<ScriptReference> IScriptControl.GetScriptReferences()
        {
            var references = new List<ScriptReference>();
            references.AddRange(ScriptObjectBuilder.GetScriptReferences(GetType()));
            return references;
        }
    }
}