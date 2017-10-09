/*
 * Created by : Eugene P. Kolesnikov
 * Created    : 31.10.2007
 * Copyright © New Age Technologies
 */

using System;
using System.ComponentModel;
using System.Data;


namespace Nat.Web.Tools
{
   
    public class AddTestRecord
    {
        private static bool _isTest = false;
        
        [DefaultValue(false)]
        public static bool IsTest
        {
            get { return _isTest; }
            set { _isTest = value; }
        }

        /// <summary>
        /// ƒобавление случайных данных в строку. ƒл€ срабатывани€ метода в строке адреса добавить атрибут test="текущее число" (напр. test=31)
        /// </summary>
        /// <param name="_dataRow">—трока, к которую производитс€ добавление данных</param>
        public static void AddRecord(DataRow _dataRow)
        {
            if (!_isTest) return;
            Random rnd = new Random();
            foreach (DataColumn dc in _dataRow.Table.Columns)
            {
               
                if (dc.ColumnName == "id") continue;
                if (dc.DataType == typeof (bool)) continue;
                if (dc.ReadOnly) continue;

                if (dc.DataType == typeof (string))
                {
                    _dataRow[dc] = Guid.NewGuid().ToString().Substring(1, 7);
                }
                else if (dc.DataType == typeof (long))
                {
                    _dataRow[dc] = rnd.Next(1, 3);
                }
                else if (dc.DataType == typeof (int))
                {
                    _dataRow[dc] = rnd.Next(1, 12);
                }

                else if (dc.DataType == typeof (DateTime))
                {
//                    _dataRow[dc] = DateTime.Now.Date;
                }
                else _dataRow[dc] = rnd.Next(1, 5);
            }
        }
    }
}