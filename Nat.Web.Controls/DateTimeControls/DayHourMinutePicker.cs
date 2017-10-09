/*
 * Created by : Roman V. Kurbangaliev
 * Created    : 18.09.2008
 * Copyright © NAT Kazahstan
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using AjaxControlToolkit;

namespace Nat.Web.Controls.DateTimeControls
{
    public class DayHourMinutePicker : DataBoundControl
    {
        #region Fields

        private TextBox textBox;
        private MaskedEditExtender maskedEditExtender;

        private const string dbNullValue = "__:__:__";

        private readonly Regex regexValues = new Regex(@"\A(?<day>\d\d)\:(?<hour>\d\d)\:(?<minute>\d\d)", RegexOptions.Compiled);

        #endregion

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            textBox = new TextBox { ID = "textBoxID", ReadOnly = ReadOnly };
            Controls.Add(textBox);

            if (ReadOnly)
                textBox.BorderWidth = Unit.Pixel(0);
            else
            {
                maskedEditExtender = new MaskedEditExtender
                                         {
                                             ID = "maskedEditExtenderID",
                                             Enabled = true,
                                             Mask = "99:99:99",
                                             ClearTextOnInvalid = false,
                                             TargetControlID = textBox.ID,
                                             ClearMaskOnLostFocus = false
                                         };
                Controls.Add(maskedEditExtender);
            }
        }

        #region Properties

        [DefaultValue(false)]
        [Browsable(true)]
        [Localizable(false)]
        public bool ReadOnly
        {
            get; set;
        }

        [Browsable(false)]
        [Bindable(true, BindingDirection.TwoWay)]
        public Object Value
        {
            get
            {
                if (textBox.Text == dbNullValue)
                    return null;
                
                var values = GetValuesFromValue(textBox.Text);

                return new DateTime(2000, 1, values[0], values[1], values[2], 0);
            }
            set
            {
                if (String.IsNullOrEmpty(value.ToString()) || dbNullValue == value.ToString())
                    textBox.Text = null;
                else
                    textBox.Text = GetMaskDateFromDateTime(Convert.ToDateTime(value));
            }
        }

        #endregion

        #region Private methods

        private List<int> GetValuesFromValue(string stringValue)
        {
            var values = new List<int>();

            Match match = regexValues.Matches(stringValue)[0];
            if (!match.Success)
                throw new ArgumentException();

            int day = Convert.ToInt32(match.Groups[1].Value);
            int hour = Convert.ToInt32(match.Groups[2].Value);
            int minute = Convert.ToInt32(match.Groups[3].Value);

            //Валидация
            if (day > 30 || hour > 23 || minute > 59)
                throw new ArgumentOutOfRangeException();

            values.Add(day);
            values.Add(hour);
            values.Add(minute);

            return values;
        }

        private static string GetMaskDateFromDateTime(DateTime dateTime)
        {
            string day = dateTime.Day.ToString();
            string hour = dateTime.Hour.ToString();
            string minute = dateTime.Minute.ToString();

            string result = day.Length > 1 ? day : "0" + day;
            result += ":";
            result += day.Length > 1 ? hour : "0" + hour;
            result += ":";
            result += day.Length > 1 ? minute : "0" + minute;
            return result;
        }

        #endregion
    }
}