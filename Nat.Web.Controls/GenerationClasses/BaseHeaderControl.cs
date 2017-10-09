/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 23 февраля 2009 г.
 * Copyright © JSC New Age Technologies 2009
 */

using System.Web.UI;

namespace Nat.Web.Controls.GenerationClasses
{
    public class BaseHeaderControl : UserControl, IHeaderControl
    {
        public virtual string Header { get; protected set; }
        public virtual string HeaderKz { get; protected set; }
        public virtual string HeaderRu { get; protected set; }

        public virtual string TableHeader
        {
            get
            {
                return Header;
            }
        }

        public virtual string TableHeaderRu
        {
            get
            {
                return HeaderRu;
            }
        }

        public virtual string TableHeaderKz
        {
            get
            {
                return HeaderKz;
            }
        }

        public virtual string ProjectHeader
        {
            get
            {
                return "";
            }
        }

        public virtual string ProjectHeaderRu
        {
            get
            {
                return "";
            }
        }

        public virtual string ProjectHeaderKz
        {
            get
            {
                return "";
            }
        }

        public string GetHtmlHeader()
        {
            return "<h2>" + ProjectHeader + " -&gt; " + TableHeader + "</h2>";
        }

        public string GetHtmlHeaderText()
        {
            return ProjectHeader + " -&gt; " + TableHeader;
        }
    }
}