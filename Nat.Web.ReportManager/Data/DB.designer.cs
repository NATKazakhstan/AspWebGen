﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Nat.Web.ReportManager.Data
{
	using System.Data.Linq;
	using System.Data.Linq.Mapping;
	using System.Data;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Linq;
	using System.Linq.Expressions;
	using System.ComponentModel;
	using System;
	
	
	[global::System.Data.Linq.Mapping.DatabaseAttribute(Name="Default")]
	public partial class DBDataContext : System.Data.Linq.DataContext
	{
		
		private static System.Data.Linq.Mapping.MappingSource mappingSource = new AttributeMappingSource();
		
    #region Определения метода расширяемости
    partial void OnCreated();
    partial void InsertReportSubscriptions_Param(ReportSubscriptions_Param instance);
    partial void UpdateReportSubscriptions_Param(ReportSubscriptions_Param instance);
    partial void DeleteReportSubscriptions_Param(ReportSubscriptions_Param instance);
    partial void InsertReportSubscription(ReportSubscription instance);
    partial void UpdateReportSubscription(ReportSubscription instance);
    partial void DeleteReportSubscription(ReportSubscription instance);
    partial void InsertDIC_ReportTimePeriodsParameter(DIC_ReportTimePeriodsParameter instance);
    partial void UpdateDIC_ReportTimePeriodsParameter(DIC_ReportTimePeriodsParameter instance);
    partial void DeleteDIC_ReportTimePeriodsParameter(DIC_ReportTimePeriodsParameter instance);
    #endregion
		
		public DBDataContext() : 
				base(global::System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnectionString"].ConnectionString, mappingSource)
		{
			OnCreated();
		}
		
		public DBDataContext(string connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public DBDataContext(System.Data.IDbConnection connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public DBDataContext(string connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public DBDataContext(System.Data.IDbConnection connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public System.Data.Linq.Table<ReportSubscriptions_Param> ReportSubscriptions_Params
		{
			get
			{
				return this.GetTable<ReportSubscriptions_Param>();
			}
		}
		
		public System.Data.Linq.Table<ReportSubscription> ReportSubscriptions
		{
			get
			{
				return this.GetTable<ReportSubscription>();
			}
		}
		
		public System.Data.Linq.Table<DIC_ReportTimePeriodsParameter> DIC_ReportTimePeriodsParameters
		{
			get
			{
				return this.GetTable<DIC_ReportTimePeriodsParameter>();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.ReportSubscriptions_Params")]
	public partial class ReportSubscriptions_Param : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private long _id;
		
		private long _refReportSubscriptions;
		
		private string _ParamName;
		
		private string _ParamCaptionKz;
		
		private string _ParamCaptionRu;
		
		private string _ParamTextValuesKz;
		
		private string _ParamTextValuesRu;
		
		private System.Nullable<int> _DynamicAttributeIndex;
		
		private string _ParamDataType;
		
		private string _ParamFilterType;
		
		private System.Nullable<long> _refReportTimePeriodsParameters;
		
		private bool _CreateOnTheDayPublication;
		
		private System.Nullable<short> _ExceptDaysFromTheDatePublication;
		
		private System.Nullable<short> _CreateOnTheLastDayPublication;
		
		private System.Nullable<short> _DeviationsFromThePeriodYear;
		
		private System.Nullable<short> _DeviationsFromThePeriodMonth;
		
		private System.Nullable<short> _DeviationsFromThePeriodDay;
		
		private EntityRef<ReportSubscription> _ReportSubscription;
		
		private EntityRef<ReportSubscription> _ReportSubscription_refReportSubscriptions;
		
		private EntityRef<DIC_ReportTimePeriodsParameter> _DIC_ReportTimePeriodsParameter_refReportTimePeriodsParameters;
		
    #region Определения метода расширяемости
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnidChanging(long value);
    partial void OnidChanged();
    partial void OnrefReportSubscriptionsChanging(long value);
    partial void OnrefReportSubscriptionsChanged();
    partial void OnParamNameChanging(string value);
    partial void OnParamNameChanged();
    partial void OnParamCaptionKzChanging(string value);
    partial void OnParamCaptionKzChanged();
    partial void OnParamCaptionRuChanging(string value);
    partial void OnParamCaptionRuChanged();
    partial void OnParamTextValuesKzChanging(string value);
    partial void OnParamTextValuesKzChanged();
    partial void OnParamTextValuesRuChanging(string value);
    partial void OnParamTextValuesRuChanged();
    partial void OnDynamicAttributeIndexChanging(System.Nullable<int> value);
    partial void OnDynamicAttributeIndexChanged();
    partial void OnParamDataTypeChanging(string value);
    partial void OnParamDataTypeChanged();
    partial void OnParamFilterTypeChanging(string value);
    partial void OnParamFilterTypeChanged();
    partial void OnrefReportTimePeriodsParametersChanging(System.Nullable<long> value);
    partial void OnrefReportTimePeriodsParametersChanged();
    partial void OnCreateOnTheDayPublicationChanging(bool value);
    partial void OnCreateOnTheDayPublicationChanged();
    partial void OnExceptDaysFromTheDatePublicationChanging(System.Nullable<short> value);
    partial void OnExceptDaysFromTheDatePublicationChanged();
    partial void OnCreateOnTheLastDayPublicationChanging(System.Nullable<short> value);
    partial void OnCreateOnTheLastDayPublicationChanged();
    partial void OnDeviationsFromThePeriodYearChanging(System.Nullable<short> value);
    partial void OnDeviationsFromThePeriodYearChanged();
    partial void OnDeviationsFromThePeriodMonthChanging(System.Nullable<short> value);
    partial void OnDeviationsFromThePeriodMonthChanged();
    partial void OnDeviationsFromThePeriodDayChanging(System.Nullable<short> value);
    partial void OnDeviationsFromThePeriodDayChanged();
    #endregion
		
		public ReportSubscriptions_Param()
		{
			this._ReportSubscription = default(EntityRef<ReportSubscription>);
			this._ReportSubscription_refReportSubscriptions = default(EntityRef<ReportSubscription>);
			this._DIC_ReportTimePeriodsParameter_refReportTimePeriodsParameters = default(EntityRef<DIC_ReportTimePeriodsParameter>);
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_id", AutoSync=AutoSync.OnInsert, DbType="bigint", IsPrimaryKey=true, IsDbGenerated=true)]
		public long id
		{
			get
			{
				return this._id;
			}
			set
			{
				if ((this._id != value))
				{
					this.OnidChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("id");
					this.OnidChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_refReportSubscriptions", DbType="bigint")]
		public long refReportSubscriptions
		{
			get
			{
				return this._refReportSubscriptions;
			}
			set
			{
				if ((this._refReportSubscriptions != value))
				{
					if ((this._ReportSubscription.HasLoadedOrAssignedValue || this._ReportSubscription_refReportSubscriptions.HasLoadedOrAssignedValue))
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnrefReportSubscriptionsChanging(value);
					this.SendPropertyChanging();
					this._refReportSubscriptions = value;
					this.SendPropertyChanged("refReportSubscriptions");
					this.OnrefReportSubscriptionsChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ParamName", DbType="nvarchar(255)", CanBeNull=false)]
		public string ParamName
		{
			get
			{
				return this._ParamName;
			}
			set
			{
				if ((this._ParamName != value))
				{
					this.OnParamNameChanging(value);
					this.SendPropertyChanging();
					this._ParamName = value;
					this.SendPropertyChanged("ParamName");
					this.OnParamNameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ParamCaptionKz", DbType="nvarchar(255)", CanBeNull=false)]
		public string ParamCaptionKz
		{
			get
			{
				return this._ParamCaptionKz;
			}
			set
			{
				if ((this._ParamCaptionKz != value))
				{
					this.OnParamCaptionKzChanging(value);
					this.SendPropertyChanging();
					this._ParamCaptionKz = value;
					this.SendPropertyChanged("ParamCaptionKz");
					this.OnParamCaptionKzChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ParamCaptionRu", DbType="nvarchar(255)", CanBeNull=false)]
		public string ParamCaptionRu
		{
			get
			{
				return this._ParamCaptionRu;
			}
			set
			{
				if ((this._ParamCaptionRu != value))
				{
					this.OnParamCaptionRuChanging(value);
					this.SendPropertyChanging();
					this._ParamCaptionRu = value;
					this.SendPropertyChanged("ParamCaptionRu");
					this.OnParamCaptionRuChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ParamTextValuesKz", DbType="nvarchar(255)")]
		public string ParamTextValuesKz
		{
			get
			{
				return this._ParamTextValuesKz;
			}
			set
			{
				if ((this._ParamTextValuesKz != value))
				{
					this.OnParamTextValuesKzChanging(value);
					this.SendPropertyChanging();
					this._ParamTextValuesKz = value;
					this.SendPropertyChanged("ParamTextValuesKz");
					this.OnParamTextValuesKzChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ParamTextValuesRu", DbType="nvarchar(255)")]
		public string ParamTextValuesRu
		{
			get
			{
				return this._ParamTextValuesRu;
			}
			set
			{
				if ((this._ParamTextValuesRu != value))
				{
					this.OnParamTextValuesRuChanging(value);
					this.SendPropertyChanging();
					this._ParamTextValuesRu = value;
					this.SendPropertyChanged("ParamTextValuesRu");
					this.OnParamTextValuesRuChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_DynamicAttributeIndex", DbType="int")]
		public System.Nullable<int> DynamicAttributeIndex
		{
			get
			{
				return this._DynamicAttributeIndex;
			}
			set
			{
				if ((this._DynamicAttributeIndex != value))
				{
					this.OnDynamicAttributeIndexChanging(value);
					this.SendPropertyChanging();
					this._DynamicAttributeIndex = value;
					this.SendPropertyChanged("DynamicAttributeIndex");
					this.OnDynamicAttributeIndexChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ParamDataType", DbType="varchar(255)")]
		public string ParamDataType
		{
			get
			{
				return this._ParamDataType;
			}
			set
			{
				if ((this._ParamDataType != value))
				{
					this.OnParamDataTypeChanging(value);
					this.SendPropertyChanging();
					this._ParamDataType = value;
					this.SendPropertyChanged("ParamDataType");
					this.OnParamDataTypeChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ParamFilterType", DbType="varchar(255)")]
		public string ParamFilterType
		{
			get
			{
				return this._ParamFilterType;
			}
			set
			{
				if ((this._ParamFilterType != value))
				{
					this.OnParamFilterTypeChanging(value);
					this.SendPropertyChanging();
					this._ParamFilterType = value;
					this.SendPropertyChanged("ParamFilterType");
					this.OnParamFilterTypeChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_refReportTimePeriodsParameters", DbType="bigint")]
		public System.Nullable<long> refReportTimePeriodsParameters
		{
			get
			{
				return this._refReportTimePeriodsParameters;
			}
			set
			{
				if ((this._refReportTimePeriodsParameters != value))
				{
					if (this._DIC_ReportTimePeriodsParameter_refReportTimePeriodsParameters.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnrefReportTimePeriodsParametersChanging(value);
					this.SendPropertyChanging();
					this._refReportTimePeriodsParameters = value;
					this.SendPropertyChanged("refReportTimePeriodsParameters");
					this.OnrefReportTimePeriodsParametersChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_CreateOnTheDayPublication", DbType="bit")]
		public bool CreateOnTheDayPublication
		{
			get
			{
				return this._CreateOnTheDayPublication;
			}
			set
			{
				if ((this._CreateOnTheDayPublication != value))
				{
					this.OnCreateOnTheDayPublicationChanging(value);
					this.SendPropertyChanging();
					this._CreateOnTheDayPublication = value;
					this.SendPropertyChanged("CreateOnTheDayPublication");
					this.OnCreateOnTheDayPublicationChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ExceptDaysFromTheDatePublication", DbType="smallint")]
		public System.Nullable<short> ExceptDaysFromTheDatePublication
		{
			get
			{
				return this._ExceptDaysFromTheDatePublication;
			}
			set
			{
				if ((this._ExceptDaysFromTheDatePublication != value))
				{
					this.OnExceptDaysFromTheDatePublicationChanging(value);
					this.SendPropertyChanging();
					this._ExceptDaysFromTheDatePublication = value;
					this.SendPropertyChanged("ExceptDaysFromTheDatePublication");
					this.OnExceptDaysFromTheDatePublicationChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_CreateOnTheLastDayPublication", DbType="smallint")]
		public System.Nullable<short> CreateOnTheLastDayPublication
		{
			get
			{
				return this._CreateOnTheLastDayPublication;
			}
			set
			{
				if ((this._CreateOnTheLastDayPublication != value))
				{
					this.OnCreateOnTheLastDayPublicationChanging(value);
					this.SendPropertyChanging();
					this._CreateOnTheLastDayPublication = value;
					this.SendPropertyChanged("CreateOnTheLastDayPublication");
					this.OnCreateOnTheLastDayPublicationChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_DeviationsFromThePeriodYear", DbType="smallint")]
		public System.Nullable<short> DeviationsFromThePeriodYear
		{
			get
			{
				return this._DeviationsFromThePeriodYear;
			}
			set
			{
				if ((this._DeviationsFromThePeriodYear != value))
				{
					this.OnDeviationsFromThePeriodYearChanging(value);
					this.SendPropertyChanging();
					this._DeviationsFromThePeriodYear = value;
					this.SendPropertyChanged("DeviationsFromThePeriodYear");
					this.OnDeviationsFromThePeriodYearChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_DeviationsFromThePeriodMonth", DbType="smallint")]
		public System.Nullable<short> DeviationsFromThePeriodMonth
		{
			get
			{
				return this._DeviationsFromThePeriodMonth;
			}
			set
			{
				if ((this._DeviationsFromThePeriodMonth != value))
				{
					this.OnDeviationsFromThePeriodMonthChanging(value);
					this.SendPropertyChanging();
					this._DeviationsFromThePeriodMonth = value;
					this.SendPropertyChanged("DeviationsFromThePeriodMonth");
					this.OnDeviationsFromThePeriodMonthChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_DeviationsFromThePeriodDay", DbType="smallint")]
		public System.Nullable<short> DeviationsFromThePeriodDay
		{
			get
			{
				return this._DeviationsFromThePeriodDay;
			}
			set
			{
				if ((this._DeviationsFromThePeriodDay != value))
				{
					this.OnDeviationsFromThePeriodDayChanging(value);
					this.SendPropertyChanging();
					this._DeviationsFromThePeriodDay = value;
					this.SendPropertyChanged("DeviationsFromThePeriodDay");
					this.OnDeviationsFromThePeriodDayChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="ReportSubscription_ReportSubscriptions_Param", Storage="_ReportSubscription", ThisKey="refReportSubscriptions", OtherKey="id", IsForeignKey=true, DeleteOnNull=true, DeleteRule="CASCADE")]
		public ReportSubscription ReportSubscription
		{
			get
			{
				return this._ReportSubscription.Entity;
			}
			set
			{
				ReportSubscription previousValue = this._ReportSubscription.Entity;
				if (((previousValue != value) 
							|| (this._ReportSubscription.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._ReportSubscription.Entity = null;
						previousValue.ReportSubscriptions_Params.Remove(this);
					}
					this._ReportSubscription.Entity = value;
					if ((value != null))
					{
						value.ReportSubscriptions_Params.Add(this);
						this._refReportSubscriptions = value.id;
					}
					else
					{
						this._refReportSubscriptions = default(long);
					}
					this.SendPropertyChanged("ReportSubscription");
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="ReportSubscription_ReportSubscriptions_Param1", Storage="_ReportSubscription_refReportSubscriptions", ThisKey="refReportSubscriptions", OtherKey="id", IsForeignKey=true)]
		public ReportSubscription ReportSubscription_refReportSubscriptions
		{
			get
			{
				return this._ReportSubscription_refReportSubscriptions.Entity;
			}
			set
			{
				ReportSubscription previousValue = this._ReportSubscription_refReportSubscriptions.Entity;
				if (((previousValue != value) 
							|| (this._ReportSubscription_refReportSubscriptions.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._ReportSubscription_refReportSubscriptions.Entity = null;
						previousValue.ReportSubscriptions_Params_refReportSubscriptions.Remove(this);
					}
					this._ReportSubscription_refReportSubscriptions.Entity = value;
					if ((value != null))
					{
						value.ReportSubscriptions_Params_refReportSubscriptions.Add(this);
						this._refReportSubscriptions = value.id;
					}
					else
					{
						this._refReportSubscriptions = default(long);
					}
					this.SendPropertyChanged("ReportSubscription_refReportSubscriptions");
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="DIC_ReportTimePeriodsParameter_ReportSubscriptions_Param", Storage="_DIC_ReportTimePeriodsParameter_refReportTimePeriodsParameters", ThisKey="refReportTimePeriodsParameters", OtherKey="id", IsForeignKey=true)]
		public DIC_ReportTimePeriodsParameter DIC_ReportTimePeriodsParameter_refReportTimePeriodsParameters
		{
			get
			{
				return this._DIC_ReportTimePeriodsParameter_refReportTimePeriodsParameters.Entity;
			}
			set
			{
				DIC_ReportTimePeriodsParameter previousValue = this._DIC_ReportTimePeriodsParameter_refReportTimePeriodsParameters.Entity;
				if (((previousValue != value) 
							|| (this._DIC_ReportTimePeriodsParameter_refReportTimePeriodsParameters.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._DIC_ReportTimePeriodsParameter_refReportTimePeriodsParameters.Entity = null;
						previousValue.ReportSubscriptions_Params_refReportTimePeriodsParameters.Remove(this);
					}
					this._DIC_ReportTimePeriodsParameter_refReportTimePeriodsParameters.Entity = value;
					if ((value != null))
					{
						value.ReportSubscriptions_Params_refReportTimePeriodsParameters.Add(this);
						this._refReportTimePeriodsParameters = value.id;
					}
					else
					{
						this._refReportTimePeriodsParameters = default(Nullable<long>);
					}
					this.SendPropertyChanged("DIC_ReportTimePeriodsParameter_refReportTimePeriodsParameters");
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.ReportSubscriptions")]
	public partial class ReportSubscription : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private long _id;
		
		private string _reportName;
		
		private System.Data.Linq.Binary _values;
		
		private System.Data.Linq.Binary _constants;
		
		private EntitySet<ReportSubscriptions_Param> _ReportSubscriptions_Params;
		
		private EntitySet<ReportSubscriptions_Param> _ReportSubscriptions_Params_refReportSubscriptions;
		
    #region Определения метода расширяемости
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnidChanging(long value);
    partial void OnidChanged();
    partial void OnreportNameChanging(string value);
    partial void OnreportNameChanged();
    partial void OnvaluesChanging(System.Data.Linq.Binary value);
    partial void OnvaluesChanged();
    partial void OnconstantsChanging(System.Data.Linq.Binary value);
    partial void OnconstantsChanged();
    #endregion
		
		public ReportSubscription()
		{
			this._ReportSubscriptions_Params = new EntitySet<ReportSubscriptions_Param>(new Action<ReportSubscriptions_Param>(this.attach_ReportSubscriptions_Params), new Action<ReportSubscriptions_Param>(this.detach_ReportSubscriptions_Params));
			this._ReportSubscriptions_Params_refReportSubscriptions = new EntitySet<ReportSubscriptions_Param>(new Action<ReportSubscriptions_Param>(this.attach_ReportSubscriptions_Params_refReportSubscriptions), new Action<ReportSubscriptions_Param>(this.detach_ReportSubscriptions_Params_refReportSubscriptions));
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_id", AutoSync=AutoSync.OnInsert, DbType="BigInt NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true)]
		public long id
		{
			get
			{
				return this._id;
			}
			set
			{
				if ((this._id != value))
				{
					this.OnidChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("id");
					this.OnidChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_reportName", DbType="VarChar(255) NOT NULL", CanBeNull=false)]
		public string reportName
		{
			get
			{
				return this._reportName;
			}
			set
			{
				if ((this._reportName != value))
				{
					this.OnreportNameChanging(value);
					this.SendPropertyChanging();
					this._reportName = value;
					this.SendPropertyChanged("reportName");
					this.OnreportNameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Name="[values]", Storage="_values", DbType="VarBinary(MAX)", UpdateCheck=UpdateCheck.Never)]
		public System.Data.Linq.Binary values
		{
			get
			{
				return this._values;
			}
			set
			{
				if ((this._values != value))
				{
					this.OnvaluesChanging(value);
					this.SendPropertyChanging();
					this._values = value;
					this.SendPropertyChanged("values");
					this.OnvaluesChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_constants", DbType="VarBinary(MAX)", UpdateCheck=UpdateCheck.Never)]
		public System.Data.Linq.Binary constants
		{
			get
			{
				return this._constants;
			}
			set
			{
				if ((this._constants != value))
				{
					this.OnconstantsChanging(value);
					this.SendPropertyChanging();
					this._constants = value;
					this.SendPropertyChanged("constants");
					this.OnconstantsChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="ReportSubscription_ReportSubscriptions_Param", Storage="_ReportSubscriptions_Params", ThisKey="id", OtherKey="refReportSubscriptions")]
		public EntitySet<ReportSubscriptions_Param> ReportSubscriptions_Params
		{
			get
			{
				return this._ReportSubscriptions_Params;
			}
			set
			{
				this._ReportSubscriptions_Params.Assign(value);
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="ReportSubscription_ReportSubscriptions_Param1", Storage="_ReportSubscriptions_Params_refReportSubscriptions", ThisKey="id", OtherKey="refReportSubscriptions")]
		public EntitySet<ReportSubscriptions_Param> ReportSubscriptions_Params_refReportSubscriptions
		{
			get
			{
				return this._ReportSubscriptions_Params_refReportSubscriptions;
			}
			set
			{
				this._ReportSubscriptions_Params_refReportSubscriptions.Assign(value);
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		
		private void attach_ReportSubscriptions_Params(ReportSubscriptions_Param entity)
		{
			this.SendPropertyChanging();
			entity.ReportSubscription = this;
		}
		
		private void detach_ReportSubscriptions_Params(ReportSubscriptions_Param entity)
		{
			this.SendPropertyChanging();
			entity.ReportSubscription = null;
		}
		
		private void attach_ReportSubscriptions_Params_refReportSubscriptions(ReportSubscriptions_Param entity)
		{
			this.SendPropertyChanging();
			entity.ReportSubscription_refReportSubscriptions = this;
		}
		
		private void detach_ReportSubscriptions_Params_refReportSubscriptions(ReportSubscriptions_Param entity)
		{
			this.SendPropertyChanging();
			entity.ReportSubscription_refReportSubscriptions = null;
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.DIC_ReportTimePeriodsParameters")]
	public partial class DIC_ReportTimePeriodsParameter : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private long _id;
		
		private string _code;
		
		private string _nameKz;
		
		private string _nameRu;
		
		private System.DateTime _dateStart;
		
		private System.Nullable<System.DateTime> _dateEnd;
		
		private System.Nullable<long> _refHistory;
		
		private EntitySet<ReportSubscriptions_Param> _ReportSubscriptions_Params_refReportTimePeriodsParameters;
		
		private EntitySet<DIC_ReportTimePeriodsParameter> _DIC_ReportTimePeriodsParameters_refHistory;
		
		private EntityRef<DIC_ReportTimePeriodsParameter> _DIC_ReportTimePeriodsParameter_refHistory;
		
    #region Определения метода расширяемости
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnidChanging(long value);
    partial void OnidChanged();
    partial void OncodeChanging(string value);
    partial void OncodeChanged();
    partial void OnnameKzChanging(string value);
    partial void OnnameKzChanged();
    partial void OnnameRuChanging(string value);
    partial void OnnameRuChanged();
    partial void OndateStartChanging(System.DateTime value);
    partial void OndateStartChanged();
    partial void OndateEndChanging(System.Nullable<System.DateTime> value);
    partial void OndateEndChanged();
    partial void OnrefHistoryChanging(System.Nullable<long> value);
    partial void OnrefHistoryChanged();
    #endregion
		
		public DIC_ReportTimePeriodsParameter()
		{
			this._ReportSubscriptions_Params_refReportTimePeriodsParameters = new EntitySet<ReportSubscriptions_Param>(new Action<ReportSubscriptions_Param>(this.attach_ReportSubscriptions_Params_refReportTimePeriodsParameters), new Action<ReportSubscriptions_Param>(this.detach_ReportSubscriptions_Params_refReportTimePeriodsParameters));
			this._DIC_ReportTimePeriodsParameters_refHistory = new EntitySet<DIC_ReportTimePeriodsParameter>(new Action<DIC_ReportTimePeriodsParameter>(this.attach_DIC_ReportTimePeriodsParameters_refHistory), new Action<DIC_ReportTimePeriodsParameter>(this.detach_DIC_ReportTimePeriodsParameters_refHistory));
			this._DIC_ReportTimePeriodsParameter_refHistory = default(EntityRef<DIC_ReportTimePeriodsParameter>);
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_id", AutoSync=AutoSync.OnInsert, DbType="bigint", IsPrimaryKey=true, IsDbGenerated=true)]
		public long id
		{
			get
			{
				return this._id;
			}
			set
			{
				if ((this._id != value))
				{
					this.OnidChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("id");
					this.OnidChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_code", DbType="varchar(2)", CanBeNull=false)]
		public string code
		{
			get
			{
				return this._code;
			}
			set
			{
				if ((this._code != value))
				{
					this.OncodeChanging(value);
					this.SendPropertyChanging();
					this._code = value;
					this.SendPropertyChanged("code");
					this.OncodeChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_nameKz", DbType="nvarchar(255)", CanBeNull=false)]
		public string nameKz
		{
			get
			{
				return this._nameKz;
			}
			set
			{
				if ((this._nameKz != value))
				{
					this.OnnameKzChanging(value);
					this.SendPropertyChanging();
					this._nameKz = value;
					this.SendPropertyChanged("nameKz");
					this.OnnameKzChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_nameRu", DbType="nvarchar(255)", CanBeNull=false)]
		public string nameRu
		{
			get
			{
				return this._nameRu;
			}
			set
			{
				if ((this._nameRu != value))
				{
					this.OnnameRuChanging(value);
					this.SendPropertyChanging();
					this._nameRu = value;
					this.SendPropertyChanged("nameRu");
					this.OnnameRuChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_dateStart", DbType="datetime")]
		public System.DateTime dateStart
		{
			get
			{
				return this._dateStart;
			}
			set
			{
				if ((this._dateStart != value))
				{
					this.OndateStartChanging(value);
					this.SendPropertyChanging();
					this._dateStart = value;
					this.SendPropertyChanged("dateStart");
					this.OndateStartChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_dateEnd", DbType="datetime")]
		public System.Nullable<System.DateTime> dateEnd
		{
			get
			{
				return this._dateEnd;
			}
			set
			{
				if ((this._dateEnd != value))
				{
					this.OndateEndChanging(value);
					this.SendPropertyChanging();
					this._dateEnd = value;
					this.SendPropertyChanged("dateEnd");
					this.OndateEndChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_refHistory", DbType="bigint")]
		public System.Nullable<long> refHistory
		{
			get
			{
				return this._refHistory;
			}
			set
			{
				if ((this._refHistory != value))
				{
					if (this._DIC_ReportTimePeriodsParameter_refHistory.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnrefHistoryChanging(value);
					this.SendPropertyChanging();
					this._refHistory = value;
					this.SendPropertyChanged("refHistory");
					this.OnrefHistoryChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="DIC_ReportTimePeriodsParameter_ReportSubscriptions_Param", Storage="_ReportSubscriptions_Params_refReportTimePeriodsParameters", ThisKey="id", OtherKey="refReportTimePeriodsParameters")]
		public EntitySet<ReportSubscriptions_Param> ReportSubscriptions_Params_refReportTimePeriodsParameters
		{
			get
			{
				return this._ReportSubscriptions_Params_refReportTimePeriodsParameters;
			}
			set
			{
				this._ReportSubscriptions_Params_refReportTimePeriodsParameters.Assign(value);
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="DIC_ReportTimePeriodsParameter_DIC_ReportTimePeriodsParameter", Storage="_DIC_ReportTimePeriodsParameters_refHistory", ThisKey="id", OtherKey="refHistory")]
		public EntitySet<DIC_ReportTimePeriodsParameter> DIC_ReportTimePeriodsParameters_refHistory
		{
			get
			{
				return this._DIC_ReportTimePeriodsParameters_refHistory;
			}
			set
			{
				this._DIC_ReportTimePeriodsParameters_refHistory.Assign(value);
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="DIC_ReportTimePeriodsParameter_DIC_ReportTimePeriodsParameter", Storage="_DIC_ReportTimePeriodsParameter_refHistory", ThisKey="refHistory", OtherKey="id", IsForeignKey=true)]
		public DIC_ReportTimePeriodsParameter DIC_ReportTimePeriodsParameter_refHistory
		{
			get
			{
				return this._DIC_ReportTimePeriodsParameter_refHistory.Entity;
			}
			set
			{
				DIC_ReportTimePeriodsParameter previousValue = this._DIC_ReportTimePeriodsParameter_refHistory.Entity;
				if (((previousValue != value) 
							|| (this._DIC_ReportTimePeriodsParameter_refHistory.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._DIC_ReportTimePeriodsParameter_refHistory.Entity = null;
						previousValue.DIC_ReportTimePeriodsParameters_refHistory.Remove(this);
					}
					this._DIC_ReportTimePeriodsParameter_refHistory.Entity = value;
					if ((value != null))
					{
						value.DIC_ReportTimePeriodsParameters_refHistory.Add(this);
						this._refHistory = value.id;
					}
					else
					{
						this._refHistory = default(Nullable<long>);
					}
					this.SendPropertyChanged("DIC_ReportTimePeriodsParameter_refHistory");
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		
		private void attach_ReportSubscriptions_Params_refReportTimePeriodsParameters(ReportSubscriptions_Param entity)
		{
			this.SendPropertyChanging();
			entity.DIC_ReportTimePeriodsParameter_refReportTimePeriodsParameters = this;
		}
		
		private void detach_ReportSubscriptions_Params_refReportTimePeriodsParameters(ReportSubscriptions_Param entity)
		{
			this.SendPropertyChanging();
			entity.DIC_ReportTimePeriodsParameter_refReportTimePeriodsParameters = null;
		}
		
		private void attach_DIC_ReportTimePeriodsParameters_refHistory(DIC_ReportTimePeriodsParameter entity)
		{
			this.SendPropertyChanging();
			entity.DIC_ReportTimePeriodsParameter_refHistory = this;
		}
		
		private void detach_DIC_ReportTimePeriodsParameters_refHistory(DIC_ReportTimePeriodsParameter entity)
		{
			this.SendPropertyChanging();
			entity.DIC_ReportTimePeriodsParameter_refHistory = null;
		}
	}
}
#pragma warning restore 1591
