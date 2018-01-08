/*
 * Created by: Alexey V. Vorochsak
 * Created: 2013.06.12
 * Copyright © JSC NAT Kazakhstan 2013
 */

namespace Nat.Web.Controls.ExtNet.Generation.Filter
{
    using GenerationClasses;

    /// <summary>
    /// Репозиотрий JavaScript'ов,
    /// предназначенных для обработки клиентской
    /// бизнеслогики контролов фильтра.
    /// </summary>
    internal static class FilterJs
    {
        #region Скрипты FilterControlBase

        #region Данные для формирования данных клиентского репозитория

        /// <summary>
        /// Начальная часть массива.
        /// </summary>
        internal static readonly string InitialPartRepositoryMember = @"{ ClientFilterTypeControl: undefined, ClientFirstFilterValueControl: undefined, ClientSecondFilterValueControl: undefined, LookupValues: undefined";

        /// <summary>
        /// Финальная часть массива.
        /// </summary>
        internal const string FinalPartJsMember = "}";

        /// <summary>
        /// Сепаратор объектов массива.
        /// </summary>
        internal const string MemberJsSeparator = ",";

        /// <summary>
        /// Обработчик события по нажатию клавиши "Enter".
        /// </summary>
        internal const string ClickApplyButtonByEnterPress =
            @"window.applyFilterButton = this;
              $('body').keydown(function(event) {
                  if ( event.which == 13 ) {
                     window.applyFilterButton.fireEvent('click', window.applyFilterButton);
                     event.preventDefault();
                  }
                });";

        /// <summary>
        /// Метод формирует скрипт, 
        /// регистрации на клиенте репозитория данных фильтра.
        /// </summary>
        /// <param name="clientReposiotry">
        /// Строковый массив данных.
        /// </param>
        /// <param name="panelId">
        /// Id панели фильтра.
        /// </param>
        /// <param name="defaultValuesJson">
        /// JSON значений по умолчанию.
        /// </param>
        /// <returns>
        /// JS инициализация репозитория(массива) объектов фильтра.
        /// </returns>
        internal static string GetFilterRepository(string clientReposiotry, string panelId, string defaultValuesJson)
        {
            return string.Format(@"window.defaultValuesFilterRepository = $.parseJSON({0}()); 
                                   if (window.defaultValuesFilterRepository == null) window.defaultValuesFilterRepository = [];
                                   window.filterRepository = [{1}];{2}",
                                                              defaultValuesJson,
                                                              clientReposiotry,
                                                              AutoResize(panelId));
        }

        #endregion

        #region Скрипты регистрации контролов фильтра в клиентском репозитории

        /// <summary>
        /// Скрипт вызывает метод регистрации контрола,
        /// отвечающего за выбора операции фильтрации,
        /// в клиентском репозитории.
        /// </summary>
        internal const string JsCallRegistrateFilterOperationControl =
            @"window.filterRepository.RegistrateFilterOperationControl(this);";

        /// <summary>
        /// Скрипт вызывает метод очистки всего фильтра.
        /// </summary>
        internal const string JsCallCleanupFilter =
           @"window.filterRepository.CleanupFilter();";

        /// <summary>
        /// Скрипт вызывает метод применения значений фильтра.
        /// </summary>
        internal const string JsCallApplyFilter =
           @"window.filterRepository.ApplyFilter('{0}');";
        
        /// <summary>
        /// Метод формирует скрипт, 
        /// который вызывает метод применения данных фильтра. 
        /// </summary>
        /// <param name="applyFunctionName">
        /// Наименование JS функции, 
        /// в которую будет передан результирующий набор данных фильтра.2
        /// </param>
        /// <returns>
        /// JS.
        /// </returns>
        internal static string GetApplyFilterHandler(string applyFunctionName)
        {
            return string.Format(JsCallApplyFilter, applyFunctionName);
        }
        
        #endregion

        #region Скрипты переключения видимости диапазонных контролов

        /// <summary>
        /// Скрипт переключает видимость второго контрола в диапазонной группе.
        /// </summary>
        internal const string JsCallToggleVisibleSecondRangeControl =
@" var isShow = true;
   this.getValue() == 'Between' 
     ? window.filterRepository.ToggleVisibleRangeControl(this, isShow) 
     : window.filterRepository.ToggleVisibleRangeControl(this, !isShow);";
        
        #endregion

        #endregion

        #region Скрипты FilterControlSelect

        /// <summary>
        /// Скрипт скрывает триггер очистки после очищения поля.
        /// </summary>
        internal const string JsCallClearTriggerHide = @"var value = this.getValue(); value == undefined || value.length == 0  ? this.getTrigger(0).hide() : this.getTrigger(0).show();";

        /// <summary>
        /// Скрипт отображает триггер очистки при выборе значения.
        /// </summary>
        internal const string JsCallShowClearTrigger = @"this.getTrigger(0).show(); var value = this.getValue(); window.filterRepository.SetLookupValue(this.id, value != undefined && !Array.isArray(value) ? value.split(',') : value);";

        internal const string JsCallSelectTriggerClick = @"window.lookuprefTypeClick(this, trigger, tag, true, #{ModalWindow});";

        /// <summary>
        /// Скрипт устанавливает toolTip контролу (MultiBox, ComboBox) при изменении значения.
        /// </summary>
        internal const string JsSetContextToolTipByChagneValueHandler =
@"
var value = this.getValue(); 
if (value == undefined || value.length == 0)
    this.getTrigger(0).hide();
else
    this.getTrigger(0).show();

var idMultiSelect = '#' + this.id;
var multiSelect = $(idMultiSelect);
if ($(multiSelect).length > 0) {
    if (this.displayTplData != undefined && this.displayTplData.length > 0) {
        var textToolTip = '';
        for (var i = 0; i < this.displayTplData.length; i++) {
            if (this.displayTplData[i].RowName != undefined && this.displayTplData[i].RowName.length > 0) {
                textToolTip += getToolTipContent(i, this.displayTplData[i].RowName);
            }
            if (this.displayTplData[i].Name != undefined && this.displayTplData[i].Name.length > 0) {
                textToolTip += getToolTipContent(i, this.displayTplData[i].Name);
                window.filterRepository.AddLookupValue(this.id, this.displayTplData[i].id);
            }
        }
        $(multiSelect).attr('data-qtip', textToolTip);
    }
    else {
        $(multiSelect).attr('data-qtip', '');
    }
}

function getToolTipContent(counter, itemText) {
    return counter % 2
     ? '<div style=background-color:#FFFFFF; width: 100%; height: 100%;>' + itemText + '</div>'
     : '<div style=background-color:#E0E0E0; width: 100%; height: 100%;>' + itemText + '</div>'
};";
        
        #endregion

        #region Скрипты для переключения видимости дополнительных фильтров

        /// <summary>
        /// Скрипт переключает видимость дополнительных фильтров.
        /// </summary>
        internal const string JsCallToogleVisibleAdditionalFilters = @" window.filterRepository.ToggleVisibleAdditionalFilters()";
        
        #endregion

        /// <summary>
        /// Метод предоставляет скрипт автоматического изменения размеров контрола,
        /// при изменении размеров окна.
        /// </summary>
        /// <param name="controlId">
        /// Id контрола.
        /// </param>
        /// <returns>
        /// JS автоматического изменения размеров контрола.
        /// </returns>
        internal static string AutoResize(string controlId)
        {
            return string.Format("Ext.EventManager.onWindowResize(function(){{ #{{{0}}}.doComponentLayout();}});", controlId);
        }

        internal const string FieldSetCollapse = @"this.collapse();";
    }
}