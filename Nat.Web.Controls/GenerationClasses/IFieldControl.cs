namespace Nat.Web.Controls.GenerationClasses
{
    using System;

    public interface IFieldControl
    {
        /// <summary>
        /// ���������� ��������� �������� (����������� �� ������ ���������).
        /// </summary>
        event EventHandler<BrowseFilterParameters> GetFilterParameters;

        /// <summary>
        /// �������� ��������.
        /// </summary>
        object Value { get; set; }

        /// <summary>
        /// ���������� ��������.
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// ������� ������ ��� ������.
        /// </summary>
        bool ReadOnly { get; set; }

        /// <summary>
        /// ��� ���� �������.
        /// </summary>
        string FieldName { get; set; }

        /// <summary>
        /// ��� ������������ �������, ���� ���� ������.
        /// </summary>
        string ParentTableName { get; set; }

        /// <summary>
        /// ������ ������������ �������, ���� ���� ������.
        /// </summary>
        string ParentTableProject { get; set; }

        /// <summary>
        /// ��� ������������ �������, ���� ���� ������.
        /// </summary>
        string ParentTableType { get; set; }

        /// <summary>
        /// ������ �� ������������ ������� (LINQ TO SQL), ���� ���� ������.
        /// </summary>
        string NameRefToParentTable { get; set; }

        /// <summary>
        /// ��������� �������� ��������.
        /// </summary>
        /// <returns></returns>
        string GetTextValue();

        /// <summary>
        /// ��� ��������� ����������� ID ��������, �������� ��� �� ��������� ���������, ����������, �������� �������� ����.
        /// </summary>
        /// <returns>���������� ID.</returns>
        string GetClientID();

        void InitEnableControls(EnableItem item);

        //string ClientMethodForSetDisabled { get; set; }
        //string ClientMethodForSetReadOnly { get; set; }
    }

    public interface IFieldControl<T> : IFieldControl
    {
        /// <summary>
        /// �������� ��������.
        /// </summary>
        T GValue { get; set; }
    }

    /*
    public interface IFieldControlClassType<T> : IFieldControl<T>
        where T : class
    {
    }

    public interface IFieldControlStructType<T> : IFieldControl<T>
        where T : struct
    {
        /// <summary>
        /// �������� ��������
        /// </summary>
        T? NullableValue { get; set; }
    }*/
}