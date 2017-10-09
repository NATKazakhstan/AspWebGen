/*
 * Created by: Alexey V. Vorochsak
 * Created: 2013.07.03
 * Copyright © JSC NAT Kazakhstan 2013
 */

namespace Nat.Web.Controls.ExtNet.Validators
{
    internal class ValidatorsJs
    {
        #region Scripts client controls range validation

        #region Scripts range validation number fields

        internal const string RangeValidationTypeNumberFields = "numberrangefields";
        internal const string JsSetStartNumberField = "this.startNumberField = #{{{0}}};";
        internal const string JsSetEndNumberField = "this.endNumberField = #{{{0}}};";

        internal const string JsRangeNumberFieldsValidationHandler =
            @" if (!Ext.form.VTypes.numberrangefields){
                  Ext.apply(Ext.form.VTypes, {
                      numberrangefields : function (val, field) {
                          if (field.vtype == 'numberrangefields'){
                              if (!val){ return; }
                              var beginNumberValue, endNumberValue; isVisible = undefined;
                              var validate = false;                     
                              if (field.startNumberField) {
                                  validate = true;
                                  beginNumberValue = field.startNumberField.getValue();
                                  endNumberValue = field.getValue();
                                  if (beginNumberValue == null) field.startNumberField.setValue('0');
                                  if (beginNumberValue != null && 
                                      endNumberValue != null && 
                                      endNumberValue < beginNumberValue){
                                         field.setValue(beginNumberValue);
                                  }
                              } else if (field.endNumberField) {
                                  var controlId = '#' + field.endNumberField.id + ':visible';
                                  isVisible = $(controlId) != undefined && $(controlId).length > 0;                                  
                                  if (isVisible){
                                      validate = true;
                                      beginNumberValue = field.getValue();
                                      endNumberValue = field.endNumberField.getValue();  
                                      if (endNumberValue == null) field.endNumberField.setValue('0');                              
                                      if (beginNumberValue != null && 
                                          endNumberValue != null && 
                                          endNumberValue < beginNumberValue){
                                             field.setValue(endNumberValue);
                                      }
                                  }
                              }
                          }

                          return true;
                     }});};";

        #endregion

        #region Scripts range validation date fields

        internal const string RangeValidationTypeDateFields = "daterangefields";
        internal const string JsSetStartDateField = "this.startDateField = #{{{0}}}.id;";
        internal const string JsSetEndDateField = "this.endDateField = #{{{0}}}.id;";

        internal const string JsRangeDateFieldsValidationHandler =
            @" if (!Ext.form.VTypes.daterangefields){
                  Ext.apply(Ext.form.VTypes, {
                      daterangefields : function (val, field) {
                        var date = field.parseDate(val);
                        var isVisible = undefined;
                        if (date) {
                            if (field.startDateField 
                                && (!this.dateRangeMax || (date.getTime() !== this.dateRangeMax.getTime())) 
                                && !this.startValidating) {
                                var start = Ext.getCmp(field.startDateField);
                                this.dateRangeMax = date;
                                start.setMaxValue(date);
                                this.startValidating = true;
                                start.validate();
                                if (start.getValue() == undefined){ start.validateValue('00.00.0000'); }
                                delete this.startValidating;
                            } else if (field.endDateField 
                                       && (!this.dateRangeMin || (date.getTime() !== this.dateRangeMin.getTime())) 
                                       && !this.endValidating) {
                                var controlId = '#' + field.endDateField + ':visible';
                                  isVisible = $(controlId) != undefined && $(controlId).length > 0;
                                  if (isVisible){
                                        var end = Ext.getCmp(field.endDateField);
                                        this.dateRangeMin = date;
                                        end.setMinValue(date);
                                        this.endValidating = true;
                                        end.validate();
                                        delete this.endValidating;
                                  }
                            } 
                        }

                        return true;
                    }});};";

        #endregion

        #endregion

        #region Scripts client compare validation

        internal const string SetClientTypeValidation = "this.vtype = 'compareValidation';";
        
        internal const string SetControlToCompare =
            @"var controlToCompare = #{{{0}}};
               if (controlToCompare != undefined){{
                  this.controlToCompare = controlToCompare;
               }};";

        internal const string SetValueToCompare =
            @" var valueToCompare = '{0}';
               if (valueToCompare != undefined && valueToCompare.length > 0){{
                  this.valueToCompare = valueToCompare;
               }};";

        internal const string SetCompareOperator =
            @" var compareOperator = '{0}';
               if (compareOperator != undefined && compareOperator.length > 0){{
                  this.compareOperator = eval('Ext.form.EnumValidationCompareOperator.' + compareOperator);
               }};";

        internal const string JsCompareValidationHandler =
            @"if (!Ext.form.VTypes.compareValidation){
                  Ext.apply(Ext.form.VTypes, {
                      compareValidation : function (val, field) {
                          if (field.compareOperator != undefined 
                              && field.compareOperator == Ext.form.EnumValidationCompareOperator.DataTypeCheck){
                              return Ext.form.convert(val, field.dataType) != undefined;
                          }

                          var valueToCompare = undefined;

                          if (field.controlToCompare != undefined){ valueToCompare = field.controlToCompare.rawValue; }
                          if (field.valueToCompare != undefined){ valueToCompare = field.valueToCompare; }

                          if (valueToCompare != undefined && field.compareOperator != undefined && field.dataType != undefined){
                             var convertFieldValue = Ext.form.convert(val, field.dataType);
                             var convertCompareValue = Ext.form.convert(valueToCompare, field.dataType);
                             
                             if (convertFieldValue != undefined && !isNaN(convertFieldValue) 
                                 && convertCompareValue != undefined && !isNaN(convertCompareValue)){
                                
                                if (field.dataType == Ext.form.EnumValidationDataType.Date){
                                   var fieldDateStr = 'new Date(\'' + convertFieldValue.toDateString() + '\')';
                                   var compareDateStr = 'new Date(\'' + convertCompareValue.toDateString() + '\')';
                                   return eval(fieldDateStr + field.compareOperator + compareDateStr);
                                }

                                return eval(String(convertFieldValue) + field.compareOperator + String(convertCompareValue));
                             }
                          }
                          return true;
                      }
                  });
               };";

        #endregion

        #region Scripts client regex validation

        internal static string SetRegularExpression =
            @"debugger; var regexValidationArr = $(this).data('regexValidationArr'); 
              if (Object.prototype.toString.call(regexValidationArr) !== '[object Array]'){{
                 regexValidationArr = new Array();
                 this.vtype = 'regexValidation';
              }}

              var hasExperssion = false;

              for(var i = 0; i < regexValidationArr.length; i++){{
                 hasExperssion = regexValidationArr[i][0] == '{0}';
                 if (hasExperssion){{
                    break;
                 }}
              }}

              if (!hasExperssion){{
                 regexValidationArr.push(['{0}', '{1}']);
                 $(this).data('regexValidationArr', regexValidationArr);
              }}";

        internal const string JsRegexValidationHandler =
          @"debugger;
            if (!Ext.form.VTypes.regexValidation) {
                Ext.apply(Ext.form.VTypes, {
                    regexValidation: function (val, field) {
                        var regexValidationArr = $(field).data('regexValidationArr');
                        if (Object.prototype.toString.call(regexValidationArr) !== '[object Array]' || regexValidationArr.length <= 0) {
                            return true;
                        }

                        var result = true;

                        for (var i = 0; i < regexValidationArr.length; i++) {
                            var regexData = regexValidationArr[i];
                            var regexValidator = new RegExp(regexData[0]);
                            result = regexValidator.test(val);

                            if (!result) {
                                field.vtypeText = regexData[1];
                                break;
                            }
                        }

                        return result;
                    }
                });
            }";

        #endregion

        #region Scripts client range validation

        internal static string SetRangeValidationType = "this.vtype = 'rangeValidation';";

        internal static string SetMinimumValue =
            @" var minimumValue = '{0}';
               if(minimumValue != undefined && minimumValue.length > 0){{
                  this.MinValue = minimumValue;
               }};";

        internal static string SetMaximumValue = 
            @"var maximumValue = '{0}';
              if(maximumValue != undefined && maximumValue.length > 0){{
                 this.MaxValue = maximumValue;
              }};";

        internal static string JsRangeValidationHandler =
             @"if (!Ext.form.VTypes.rangeValidation){
                  Ext.apply(Ext.form.VTypes, {
                      rangeValidation : function (val, field) {
                          if (field.dataType != undefined && field.dataType.toString().length > 0 
                              && ((field.MinValue != undefined && field.MinValue.toString().length > 0)
                                   || (field.MaxValue != undefined && field.MaxValue.length > 0))
                              && val != undefined && val.toString().length > 0)
                          {
                              var value, maxValue, minValue = undefined;
                              value = Ext.form.convert(val, field.dataType);
                              maxValue = Ext.form.convert(field.MaxValue, field.dataType);
                              minValue = Ext.form.convert(field.MinValue, field.dataType);
                              return Ext.form.rangeValidate(minValue, value, maxValue);
                          }
                     }
                });
            };";

        #endregion

        #region Common validation scripts

        internal const string JsRangeValidationFunction =
            @"Ext.form.rangeValidate = function rangeValidate(minValue, value, maxValue){
                    var result = false;
                              
                    if(value == undefined){
                        return result;
                    }

                    if(minValue == undefined && maxValue == undefined){
                        return result;
                    }

                    if(minValue != undefined && value < minValue) return result;
                    if(maxValue != undefined && value > maxValue) return result;
                    return true;
                 };";

        internal const string JsConvertFunction =
            @"Ext.form.convert = function convert(val, dataType){
                    var regex, value;
                    switch(dataType)
                    {
                        case Ext.form.EnumValidationDataType.Currency:
                             value = val.replace(/[^0-9\.]+/g, '');
                             if(value){
                                return parseFloat(value);
                             }
                             break;

                        case Ext.form.EnumValidationDataType.Date:
                             value = new Date(val.replace(/(\d{2})\.(\d{2})\.(\d{4})/, 
                                              navigator.userAgent.search('MSIE') >= 0 ? '$1-$2-$3' : '$3-$2-$1'));

                             if (value != 'Invalid Date'){  
                                return value;
                             }
                             break;

                        case Ext.form.EnumValidationDataType.Double:
                             regex = RegExp('^[0-9]+\.([0-9]+)$'); 
                             if (regex.test(val)){
                                return parseFloat(val);
                             }
                             break;

                        case Ext.form.EnumValidationDataType.Integer:
                             regex = RegExp('^\\d+$'); 
                             if (regex.test(val)){
                                return parseInt(val);
                             }
                             break;

                        case Ext.form.EnumValidationDataType.String:
                                //todo: не реализовано.
                                break;
                    }
                  };";

        internal const string SetEnumValidationCompareOperator =
            @"if (Ext.form.EnumValidationCompareOperator == undefined){
                  var EnumValidationCompareOperator = {'DataTypeCheck':'DataTypeCheck',
                                                       'Equal': '==',
                                                       'GreaterThan':'>',
                                                       'GreaterThanEqual':'>=',
                                                       'LessThan':'<',
                                                       'LessThanEqual':'<=',
                                                       'NotEqual':'!='}; 

                  Ext.form.EnumValidationCompareOperator = EnumValidationCompareOperator;
              };";

        internal const string SetEnumValidationDataType =
            @"if (Ext.form.EnumValidationDataType == undefined){
                  var EnumValidationDataType = {'Currency':'Currency',
                                                'Date': 'Date',
                                                'Double':'Double',
                                                'Integer':'Integer',
                                                'String':'String'};

                  Ext.form.EnumValidationDataType = EnumValidationDataType;
              };";

        internal const string SetDataType =
           @" var dataType = '{0}';  
               if (dataType != undefined && dataType.length > 0){{
                    this.dataType = dataType;
               }};";

        #endregion
    }
}