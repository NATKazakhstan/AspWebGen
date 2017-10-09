namespace JS.LinqToJavaScript.Attributes
{
    using System;

    public class JavaScriptFunctionAttribute : Attribute
    {
        #region Public Properties

        /// <summary>
        /// Метод объявлен глобальным методом
        /// </summary>
        public bool DeclaredAsGlobal { get; set; }

        /// <summary>
        /// Метод объявлен в базовом классе
        /// </summary>
        public bool DeclaredInBaseClass { get; set; }

        /// <summary>
        /// Свойство описывающее метод в виде Expression
        /// </summary>
        public string ExpressionProperty { get; set; }

        /// <summary>
        /// Название функции в классе
        /// </summary>
        public string FunctionName { get; set; }

        #endregion
    }
}