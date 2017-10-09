using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;

namespace Nat.Web.Controls.GenerationClasses.Navigator
{
    using System.Web;

    public class BaseMenuItem
    {
        private string _url;
        private bool _isCurrentTable;

        public BaseMenuItem()
        {
            Childs = new List<BaseMenuItem>();
        }

        public List<BaseMenuItem> Childs { get; private set; }
        public BaseMenuItem Parent { get; set; }
        public BaseMenu Menu { get; set; }
        public string ResourceKey { get; set; }
        public string TableName { get; set; }
        public Type TableType { get; set; }
        public string ReferenceName { get; set; }
        //todo: сделать наполнение количества записей
        public Dictionary<string, string> CountValueProperties { get; set; }
        
        public MenuDefaultDestination DefaultDestination { get; set; }

        public string Header
        {
            get
            {
                return Menu.ResourceManager.GetString(ResourceKey);
            }
        }

        public string HeaderRu
        {
            get
            {
                return Menu.ResourceManager.GetString(ResourceKey, new CultureInfo("ru-ru"));
            }
        }

        public string HeaderKz
        {
            get
            {
                return Menu.ResourceManager.GetString(ResourceKey, new CultureInfo("kk-kz"));
            }
        }

        public void Render(HtmlTextWriter writer, int level)
        {
            writer.AddStyleAttribute(HtmlTextWriterStyle.PaddingTop, "5px");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            
            int i = 0;
            while (i++ < level)
                writer.Write("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");

            RenderLink(writer);
            if (Childs.Count > 0)
            {
                foreach (var menuItem in Childs)
                {
                    menuItem.Render(writer, level + 1);
                }
            }

            writer.RenderEndTag();
        }

        private void RenderLink(HtmlTextWriter writer)
        {
            if (string.IsNullOrEmpty(TableName))
            {
                writer.Write(Header);
                writer.WriteBreak();
                return;
            }
            if (_isCurrentTable)
            {
                writer.RenderBeginTag(HtmlTextWriterTag.B);
                writer.Write(Header);
                writer.RenderEndTag();
                return;
            }

            writer.AddAttribute(HtmlTextWriterAttribute.Href, _url);
            writer.RenderBeginTag(HtmlTextWriterTag.A);
            writer.Write(Header);
            if(false)//todo: определится с условием на наличие количества
            {
                var count = 0;//todo: выставить значние
                writer.Write(" (");
                writer.Write(count);
                writer.Write(")");
            }
            writer.RenderEndTag();

            if (DetectedUrlBuilder != null && Menu.BaseNavigatorControl.BaseNavigatorValues.ContainsKey(TableType))
            {
                var referenceName = GetFullReferenceName();
                if (!string.IsNullOrEmpty(referenceName))
                {
                    var rowName = Menu.BaseNavigatorControl.GetNavigatorInfoRowName(TableType, referenceName, Menu.Url);
                    writer.Write(" <i>");
                    writer.Write(HttpUtility.HtmlEncode(rowName));
                    writer.Write("</i>");
                }
            }
        }

        private string GetFullReferenceName()
        {
            if (_isCurrentTable)
                return "";
            foreach (var item in Childs)
            {
                var value = item.GetFullReferenceName();
                if(value == null) continue;
                if (value == "")
                    return item.ReferenceName;

                if (string.IsNullOrEmpty(item.ReferenceName))
                    return value;
                return value + "." + item.ReferenceName;
            }
            return null;
        }

        public void InitMenu()
        {
            InitMenu(Menu.Url.Clone(), false);
        }

        protected bool SetUserControl(MainPageUrlBuilder urlBuilder)
        {
            if (string.IsNullOrEmpty(TableName))
                return false;
            urlBuilder.IsRead = false;
            urlBuilder.IsNew = false;
            urlBuilder.IsSelect = false;
            urlBuilder.IsFilterWindow = false;
            switch (DefaultDestination)
            {
                case MenuDefaultDestination.Journal:
                    urlBuilder.UserControl = TableName + "Journal";
                    break;
                case MenuDefaultDestination.Read:
                    if (Menu.BaseNavigatorControl.BaseNavigatorValues.ContainsKey(TableType))
                    {
                        urlBuilder.UserControl = TableName + "Edit";
                        urlBuilder.IsRead = true;
                    }
                    else
                        urlBuilder.UserControl = TableName + "Journal";
                    break;
                case MenuDefaultDestination.Edit:
                    if (Menu.BaseNavigatorControl.BaseNavigatorValues.ContainsKey(TableType))
                        urlBuilder.UserControl = TableName + "Edit";
                    else
                        urlBuilder.UserControl = TableName + "Journal";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return true;
        }

        /// <summary>
        /// Используется для оптимизации обхода рекурсии. Что бы не было вызова обхода дочки от которой получен урл
        /// </summary>
        protected MainPageUrlBuilder DetectedUrlBuilder { get; set; }

        /// <summary>
        /// Метод инициализации ссылок на журналы
        /// </summary>
        /// <param name="urlBuilder"></param>
        /// <param name="detectedMenu"></param>
        /// <returns></returns>
        protected MainPageUrlBuilder InitMenu(MainPageUrlBuilder urlBuilder, bool detectedMenu)
        {
            //если урл не определен, ты определяем урл для данной таблицы
            if (!detectedMenu)
            {
                //проверяем, урл по данной таблице используется или нет
                if (urlBuilder.UserControl.Equals(TableName + "Edit", StringComparison.OrdinalIgnoreCase)
                    || urlBuilder.UserControl.Equals(TableName + "Journal", StringComparison.OrdinalIgnoreCase))
                {
                    //проверяем что имеющиеся ссылки подходят именно этой части меню
                    var keys = urlBuilder.QueryParameters.Keys.Where(r => r.EndsWith(".id")).ToList();
                    //смотрим либо отсутствуют параметры таблиц, либо они начинаются с текущего референса, тогда считаем текущей таблицей
                    if (keys.Count == 0 || ReferenceName == null ||
                        keys.FirstOrDefault(r => r.StartsWith(ReferenceName)) != null)
                    {
                        detectedMenu = true;
                        _isCurrentTable = true;
                        InitMenuByChilds(urlBuilder, true);
                    }
                }
                else
                {
                    //обходим по дочкам и пытаемся определить урл для данной таблицы
                    var detectedUrlBuilder = InitMenuByChilds(urlBuilder, false);
                    if (detectedUrlBuilder != null)
                    {
                        urlBuilder = detectedUrlBuilder;
                        detectedMenu = true;
                    }
                }
            }
            else
            {
                //если урл определен, то передаем его дочкам
                InitMenuByChilds(urlBuilder, true);
            }
            //указываем журнал ссылке
            var setUrl = SetUserControl(urlBuilder);
            if (detectedMenu)
            {
                //выставляем текущему меню ссылку
                if (setUrl) _url = urlBuilder.CreateUrl();
                //укарачиваем ссылки по референсам
                MoveReferenceToParent(urlBuilder, this, Parent);
                return DetectedUrlBuilder = urlBuilder;
            }
            //если урл не был определен, то убираем все параметры родителей
            RemoveAllTableReferences(urlBuilder);
            //выставляем текущему меню ссылку
            if (setUrl) _url = urlBuilder.CreateUrl();
            return null;
        }

        /// <summary>
        /// Обход по дочерним для определения ссылок на журналы
        /// </summary>
        /// <param name="urlBuilder"></param>
        /// <param name="detectedMenu"></param>
        /// <returns></returns>
        protected MainPageUrlBuilder InitMenuByChilds(MainPageUrlBuilder urlBuilder, bool detectedMenu)
        {
            if (!detectedMenu)
            {
                //если урл не определен, то обходим по дочкам и смотрим более подходящий
                var list = new List<MainPageUrlBuilder>();
                foreach (var item in Childs)
                {
                    item.Parent = this;
                    var itemUrl = item.InitMenu(urlBuilder.Clone(), false);
                    if (itemUrl != null)
                        list.Add(itemUrl);
                }
                MainPageUrlBuilder resultUrl = null;
                if (list.Count == 1) resultUrl = list[0];
                else if (list.Count > 0)
                {
                    #region определение более подходящего меню
                    MainPageUrlBuilder maxUrl = null;
                    var maxValue = 0;
                    foreach (var itemUrl in list)
                    {
                        var countData = itemUrl.QueryParameters.Keys.
                            Where(key => key.EndsWith(".id"));
                        if (countData.FirstOrDefault() == null) continue;
                        var maxInner = countData.
                            Select(key => key.Count(c => c == '.')).
                            Max();
                        if (maxInner > maxValue)
                        {
                            maxValue = maxInner;
                            maxUrl = itemUrl;
                        }
                    }
                    resultUrl = maxUrl ?? list[0];
                    #endregion
                }
                if (resultUrl == null)
                    return null;
                //если определяется нормальная ссылка, то обходим дочек еще раз
                foreach (var item in Childs)
                {
                    if (item.DetectedUrlBuilder != resultUrl)//пропускаем тот журнал что вернул урл
                    {
                        var url = resultUrl.Clone();
                        MoveReferenceToChilds(item, GetMenuItem(), url);
                        item.InitMenu(url, true);
                    }
                }
                return resultUrl;
            }
            //если урл определено, то обходим по дочкам и отдаем им урл, изменяя параметры родительских таблиц (учитываем референс)
            foreach (var item in Childs)
            {
                item.Parent = this;
                var url = urlBuilder.Clone();
                MoveReferenceToChilds(item, GetMenuItem(), url);
                item.InitMenu(url, true);
            }
            return null;
        }

        /// <summary>
        /// Определяет ближайшую таблицу, т.е. либо сам либо один из родителей который имеет заполненное свойство TableName
        /// </summary>
        /// <returns></returns>
        private BaseMenuItem GetMenuItem()
        {
            if (string.IsNullOrEmpty(TableName))
                return Parent == null ? null : Parent.GetMenuItem();
            return this;
        }

        /// <summary>
        /// Удаляем все рефренсы журналов
        /// </summary>
        /// <param name="urlBuilder"></param>
        private static void RemoveAllTableReferences(MainPageUrlBuilder urlBuilder)
        {
            var keys = urlBuilder.QueryParameters.Keys.Where(r => r.EndsWith(".id")).ToList();
            foreach (var key in keys)
                urlBuilder.QueryParameters.Remove(key);
        }

        /// <summary>
        /// Изменяем урл приводя его к урлу родительского журнала.
        /// </summary>
        /// <param name="urlBuilder"></param>
        /// <param name="menuItem"></param>
        private static void MoveReferenceToParent(MainPageUrlBuilder urlBuilder, BaseMenuItem menuItem, BaseMenuItem parentItem)
        {
            if (string.IsNullOrEmpty(menuItem.ReferenceName)) return;
            var keys = urlBuilder.QueryParameters.Keys.Where(r => r.EndsWith(".id")).ToList();
            if (keys.Count == 0) return;

            var newValues = new Dictionary<string, string>();
            foreach (var key in keys)
            {
                if (key == menuItem.ReferenceName + ".id")
                {
                    var value = urlBuilder.QueryParameters[key];
                    urlBuilder.QueryParameters.Remove(key);
                    urlBuilder.QueryParameters["ref" + parentItem.TableName] = value;
                }
                else if (key.StartsWith(menuItem.ReferenceName))
                {
                    var value = urlBuilder.QueryParameters[key];
                    urlBuilder.QueryParameters.Remove(key);
                    var newKey = key.Substring(menuItem.ReferenceName.Length + 1);
                    newValues[newKey] = value;
                }
                else
                    urlBuilder.QueryParameters.Remove(key);
                foreach (var newValue in newValues)
                    urlBuilder.QueryParameters[newValue.Key] = newValue.Value;
            }
        }

        /// <summary>
        /// Изменяем урл приводя его к урлу дочернего журнала.
        /// </summary>
        /// <param name="menuItem"></param>
        /// <param name="parentMenuItem"></param>
        /// <param name="url"></param>
        private static void MoveReferenceToChilds(BaseMenuItem menuItem, BaseMenuItem parentMenuItem, MainPageUrlBuilder url)
        {
            if (string.IsNullOrEmpty(menuItem.ReferenceName)) return;
            var keys = url.QueryParameters.Keys.Where(r => r.EndsWith(".id")).ToList();
            var refKey = "ref" + parentMenuItem.TableName;
            if (!parentMenuItem._isCurrentTable
                && menuItem.Menu.BaseNavigatorControl.BaseNavigatorValues.ContainsKey(parentMenuItem.TableType)
                && menuItem.Menu.BaseNavigatorControl.BaseNavigatorValues[parentMenuItem.TableType] != null)
            {
                //проставление референса по родительской таблице, если она выбрана
                //смотрим в навигаторе потому как навигатор хранит выбранные записи по референсам (в урле хранится лишнее)
                var value = menuItem.Menu.BaseNavigatorControl.BaseNavigatorValues[parentMenuItem.TableType].ToString();
                url.QueryParameters[refKey] = value;
                url.QueryParameters[menuItem.ReferenceName + ".id"] = value;
            }
            if (keys.Count == 0 && !parentMenuItem._isCurrentTable)
                return;

            foreach (var key in keys)
            {
                var value = url.QueryParameters[key];
                url.QueryParameters.Remove(key);
                url.QueryParameters[menuItem.ReferenceName + "." + key] = value;
            }
            //проставляем референс по выбранной записи
            //смотрим в этой коллекции потому как в текущем журнале изменяется выбранная в урле
            if (parentMenuItem._isCurrentTable && url.QueryParameters.ContainsKey(refKey))
            {
                var value = url.QueryParameters[refKey];
                url.QueryParameters[menuItem.ReferenceName + ".id"] = value;
            }

        }
    }
}
