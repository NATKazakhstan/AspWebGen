using System.Collections.Generic;
using System.Drawing;
using System.Web.UI;
using Nat.ReportManager.QueryGeneration;
using Nat.Web.ReportManager.CustomExport;

namespace Nat.Web.ReportManager
{
    public interface IWebReportPlugin : IReportPlugin
    {
        /// <summary>
        /// ����� �������.
        /// </summary>
        /// <returns>������ �����, ������� �������� �����.</returns>
        string[] Roles();

        /// <summary>
        /// ��������� �������� �� ���������. 
        /// � �������, ����� ��� ������� � ������� �� ������ �� ������, 
        /// �� ����������� ��������� id �������
        /// </summary>
        string DefaultValue { set; }

        /// <summary>
        /// �������� �� ������� ������������ �����.
        /// </summary>
        Page Page { get; set; }

        /// <summary>
        /// ��������� ��������, ��� �������� ������.
        /// </summary>
        Dictionary<string, object> Constants { get; set; }

        /// <summary>
        /// ���������������� �������� ��������� ������� ������, ������������ ���������� � �������� ������������ ������.
        /// </summary>
        bool InitSavedValuesInvisibleConditions { get; }

        /// <summary>
        /// ��������� �������� ������� ������������ ������.
        /// </summary>
        /// <remarks>���� false, �� InitSavedValuesInvisibleConditions ��������� ���� false</remarks>
        bool AllowSaveValuesConditions { get; }

        string ImageUrl { get; }

        CustomExportType CustomExportType { get; }
    }
}