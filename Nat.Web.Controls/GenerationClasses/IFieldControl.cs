namespace Nat.Web.Controls.GenerationClasses
{
    using System;

    public interface IFieldControl
    {
        /// <summary>
        /// ”становить параметры контролу (зависимость от других контролов).
        /// </summary>
        event EventHandler<BrowseFilterParameters> GetFilterParameters;

        /// <summary>
        /// «начение контрола.
        /// </summary>
        object Value { get; set; }

        /// <summary>
        /// јктивность контрола.
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        ///  онтрол только дл€ чтени€.
        /// </summary>
        bool ReadOnly { get; set; }

        /// <summary>
        /// »м€ пол€ таблицы.
        /// </summary>
        string FieldName { get; set; }

        /// <summary>
        /// »м€ родительской таблицы, если поле ссылка.
        /// </summary>
        string ParentTableName { get; set; }

        /// <summary>
        /// ѕроект родительской таблицы, если поле ссылка.
        /// </summary>
        string ParentTableProject { get; set; }

        /// <summary>
        /// “ип родительской таблицы, если поле ссылка.
        /// </summary>
        string ParentTableType { get; set; }

        /// <summary>
        /// —сылка на родительскую таблицу (LINQ TO SQL), если поле ссылка.
        /// </summary>
        string NameRefToParentTable { get; set; }

        /// <summary>
        /// “екстовое значение контрола.
        /// </summary>
        /// <returns></returns>
        string GetTextValue();

        /// <summary>
        /// ƒл€ получени€ клиентского ID контрола, например что бы настроить видимость, активность, получить значение пол€.
        /// </summary>
        /// <returns> лиентский ID.</returns>
        string GetClientID();

        void InitEnableControls(EnableItem item);

        //string ClientMethodForSetDisabled { get; set; }
        //string ClientMethodForSetReadOnly { get; set; }
    }

    public interface IFieldControl<T> : IFieldControl
    {
        /// <summary>
        /// «начение контрола.
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
        /// «начение контрола
        /// </summary>
        T? NullableValue { get; set; }
    }*/
}