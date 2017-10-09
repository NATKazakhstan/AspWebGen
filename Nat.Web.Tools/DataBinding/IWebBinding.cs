/*
 * Created by: Denis M. Silkov
 * Created: 9 октября 2007 г.
 * Copyright © JSC New Age Technologies 2007
 */

using System.Web.UI;
using Nat.Web.Controls.DataBinding;

namespace Nat.Web.Tools.DataBinding
{
    public interface IWebBinding
    {
        /// <summary>
        /// Reference to the WebBinder parent object.
        /// </summary>
        IWebBinder Binder { get; set; }

        /// <summary>
        /// The ID of the control to that is bound.
        /// </summary>
        string ControlId { get; set; }

        /// <summary>
        /// An optional instance of the control that can be assigned. Used internally
        /// by the wwDatBindiner to assign the control whenever possible as the instance
        /// is more efficient and reliable than the string name.
        /// </summary>
        Control ControlInstance { get; set; }

        /// <summary>
        /// The binding source object that is the source for databinding.
        /// This is an object of some sort and can be either a real object
        /// or a DataRow/DataTable/DataSet. If a DataTable is used the first row 
        /// is assumed. If a DataSet is used the first table and first row are assumed.
        ///
        /// The object reference is always Page relative, so binding doesn't work
        /// against local variables, only against properties of the form. Form
        /// properties that are bound should be marked public or protected, but
        /// not private as Reflection is used to get the values. 
        /// 
        /// This or me is implicit, but can be specified so
        ///  "Customer" or "this.Customer" is equivalent. 
        /// </summary>
        /// <example>
        /// // *** Bind a DataRow Item
        /// bi.BindingSource = "Customer.DataRow";
        /// bi.BindingSourceMember = "LastName";
        ///
        /// // *** Bind a DataRow within a DataSet  - not recommended though
        /// bi.BindingSource = "this.Customer.Tables["TCustomers"].Rows[0]";
        /// bi.BindingSourceMember = "LastName";
        ///
        /// // *** Bind an Object
        /// bi.BindingSource = "InventoryItem.Entity";
        /// bi.BindingSourceMember = "ItemPrice";
        /// 
        /// // *** Bind a form property
        /// bi.BindingSource = "this";   // also "me" 
        /// bi.BindingSourceMember = "CustomerPk";
        /// </example>
        string BindingSource { get; set; }

        /// <summary>
        /// An instance of the object that the control is bound to
        /// Optional - can be passed instead of a BindingSource string. Using
        /// a reference is more efficient. Declarative use in the designer
        /// always uses strings, code base assignments should use instances
        /// with BindingSourceObject.
        /// </summary>
        object BindingSourceObject { get; set; }

        /// <summary>
        /// The property or field on the Binding Source that is
        /// bound. Example: BindingSource: Customer.Entity BindingSourceMember: Company
        /// </summary>
        string BindingSourceMember { get; set; }

        /// <summary>
        /// Property that is bound on the target controlId
        /// </summary>
        string BindingProperty { get; set; }

        /// <summary>
        /// Format Expression ( {0:c) ) applied to the binding source when it's displayed.
        /// Watch out for two way conversion issues when formatting this way. If you create
        /// expressions and you are also saving make sure the format used can be saved back.
        /// </summary>
        string DisplayFormat { get; set; }

        /// <summary>
        /// A descriptive name for the field used for error display
        /// </summary>
        string UserFieldName { get; set; }

        /// <summary>
        /// Determines how binding and validation errors display on the control
        /// </summary>
        BindingErrorMessageLocations ErrorMessageLocation { get; set; }

        /// <summary>
        /// Internal property that lets you know if there was binding error
        /// on this control after binding occurred
        /// </summary>
        bool IsBindingError { get; set; }

        /// <summary>
        /// An error message that gets set if there is a binding error
        /// on the control.
        /// </summary>
        string BindingErrorMessage { get; set; }

        /// <summary>
        /// Determines how databinding and unbinding is done on the target control. 
        /// One way only fires ReadValue() and ignores WriteValue() calls. 
        /// Two-way does both. None effectively turns off binding.
        /// </summary>
        BindingModes BindingMode { get; set; }

        /// <summary>
        /// Считывать значение из источника в контрол при наличии ошибки байндинга.
        /// </summary>
        bool ReadValueOnBindingError { get; set; }

        /// <summary>
        /// Binds a source object and property to a control's property. For example
        /// you can bind a business object to a the text property of a text box, or 
        /// a DataRow field to a text box field. You specify a binding source object 
        /// (Customer.Entity or Customer.DataRow) and property or field(Company, FirstName)
        /// and bind it to the control and the property specified (Text).
        /// </summary>
        void ReadValue();

        /// <summary>
        /// Binds a source object and property to a control's property. For example
        /// you can bind a business object to a the text property of a text box, or 
        /// a DataRow field to a text box field. You specify a binding source object 
        /// (Customer.Entity or Customer.DataRow) and property or field(Company, FirstName)
        /// and bind it to the control and the property specified (Text).
        /// </summary>
        /// <param name="root">the Base control that binding source objects are attached to</param>
        void ReadValue(Control root);

        /// <summary>
        /// Unbinds control properties back into the control source.
        /// 
        /// This method uses reflection to lift the data out of the control, then 
        /// parses the string value back into the type of the data source. If an error 
        /// occurs the exception is not caught internally, but generally the 
        /// FormUnbindData method captures the error and assigns an error message to 
        /// the BindingErrorMessage property of the control.
        /// </summary>
        void WriteValue();

        /// <summary>
        /// Unbinds control properties back into the control source.
        /// 
        /// This method uses reflection to lift the data out of the control, then 
        /// parses the string value back into the type of the data source. If an error 
        /// occurs the exception is not caught internally, but generally the 
        /// FormUnbindData method captures the error and assigns an error message to 
        /// the BindingErrorMessage property of the control.
        /// <seealso>Class wwWebDataHelper</seealso>
        /// </summary>
        /// <param name="root">
        /// The base control that binding sources are based on.
        /// </param>
        void WriteValue(Control root);

        /// <summary>
        /// Инициализирует байндинг: устанавливает BindingSourceObject, ControlInstance и др.
        /// </summary>
        /// <param name="root">Контейнер контролов.</param>
        void InitBinding(Control root);
    }
}