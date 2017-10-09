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

namespace Nat.Web.Tools.MailMessageContent
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
	internal partial class DBDataContext : System.Data.Linq.DataContext
	{
		
		private static System.Data.Linq.Mapping.MappingSource mappingSource = new AttributeMappingSource();
		
    #region Определения метода расширяемости
    partial void OnCreated();
    partial void InsertMSC_Module(MSC_Module instance);
    partial void UpdateMSC_Module(MSC_Module instance);
    partial void DeleteMSC_Module(MSC_Module instance);
    partial void InsertMSC_Module_Configuration(MSC_Module_Configuration instance);
    partial void UpdateMSC_Module_Configuration(MSC_Module_Configuration instance);
    partial void DeleteMSC_Module_Configuration(MSC_Module_Configuration instance);
    partial void InsertMSC_Module_ConfigurationField(MSC_Module_ConfigurationField instance);
    partial void UpdateMSC_Module_ConfigurationField(MSC_Module_ConfigurationField instance);
    partial void DeleteMSC_Module_ConfigurationField(MSC_Module_ConfigurationField instance);
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
		
		public System.Data.Linq.Table<MSC_Module> MSC_Modules
		{
			get
			{
				return this.GetTable<MSC_Module>();
			}
		}
		
		public System.Data.Linq.Table<MSC_Module_Configuration> MSC_Module_Configurations
		{
			get
			{
				return this.GetTable<MSC_Module_Configuration>();
			}
		}
		
		public System.Data.Linq.Table<MSC_Module_ConfigurationField> MSC_Module_ConfigurationFields
		{
			get
			{
				return this.GetTable<MSC_Module_ConfigurationField>();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.MSC_Modules")]
	public partial class MSC_Module : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private long _id;
		
		private string _Code;
		
		private string _Name;
		
		private bool _Enabled;
		
		private EntitySet<MSC_Module_Configuration> _MSC_Module_Configurations_refModule;
		
		private EntitySet<MSC_Module_ConfigurationField> _MSC_Module_ConfigurationFields_refModule;
		
    #region Определения метода расширяемости
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnidChanging(long value);
    partial void OnidChanged();
    partial void OnCodeChanging(string value);
    partial void OnCodeChanged();
    partial void OnNameChanging(string value);
    partial void OnNameChanged();
    partial void OnEnabledChanging(bool value);
    partial void OnEnabledChanged();
    #endregion
		
		public MSC_Module()
		{
			this._MSC_Module_Configurations_refModule = new EntitySet<MSC_Module_Configuration>(new Action<MSC_Module_Configuration>(this.attach_MSC_Module_Configurations_refModule), new Action<MSC_Module_Configuration>(this.detach_MSC_Module_Configurations_refModule));
			this._MSC_Module_ConfigurationFields_refModule = new EntitySet<MSC_Module_ConfigurationField>(new Action<MSC_Module_ConfigurationField>(this.attach_MSC_Module_ConfigurationFields_refModule), new Action<MSC_Module_ConfigurationField>(this.detach_MSC_Module_ConfigurationFields_refModule));
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
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Code", DbType="nvarchar(255)", CanBeNull=false)]
		public string Code
		{
			get
			{
				return this._Code;
			}
			set
			{
				if ((this._Code != value))
				{
					this.OnCodeChanging(value);
					this.SendPropertyChanging();
					this._Code = value;
					this.SendPropertyChanged("Code");
					this.OnCodeChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Name", DbType="nvarchar(255)", CanBeNull=false)]
		public string Name
		{
			get
			{
				return this._Name;
			}
			set
			{
				if ((this._Name != value))
				{
					this.OnNameChanging(value);
					this.SendPropertyChanging();
					this._Name = value;
					this.SendPropertyChanged("Name");
					this.OnNameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Enabled", DbType="bit")]
		public bool Enabled
		{
			get
			{
				return this._Enabled;
			}
			set
			{
				if ((this._Enabled != value))
				{
					this.OnEnabledChanging(value);
					this.SendPropertyChanging();
					this._Enabled = value;
					this.SendPropertyChanged("Enabled");
					this.OnEnabledChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="MSC_Module_MSC_Module_Configuration", Storage="_MSC_Module_Configurations_refModule", ThisKey="id", OtherKey="refModule")]
		public EntitySet<MSC_Module_Configuration> MSC_Module_Configurations_refModule
		{
			get
			{
				return this._MSC_Module_Configurations_refModule;
			}
			set
			{
				this._MSC_Module_Configurations_refModule.Assign(value);
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="MSC_Module_MSC_Module_ConfigurationField", Storage="_MSC_Module_ConfigurationFields_refModule", ThisKey="id", OtherKey="refModule")]
		public EntitySet<MSC_Module_ConfigurationField> MSC_Module_ConfigurationFields_refModule
		{
			get
			{
				return this._MSC_Module_ConfigurationFields_refModule;
			}
			set
			{
				this._MSC_Module_ConfigurationFields_refModule.Assign(value);
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
		
		private void attach_MSC_Module_Configurations_refModule(MSC_Module_Configuration entity)
		{
			this.SendPropertyChanging();
			entity.MSC_Module_refModule = this;
		}
		
		private void detach_MSC_Module_Configurations_refModule(MSC_Module_Configuration entity)
		{
			this.SendPropertyChanging();
			entity.MSC_Module_refModule = null;
		}
		
		private void attach_MSC_Module_ConfigurationFields_refModule(MSC_Module_ConfigurationField entity)
		{
			this.SendPropertyChanging();
			entity.MSC_Module_refModule = this;
		}
		
		private void detach_MSC_Module_ConfigurationFields_refModule(MSC_Module_ConfigurationField entity)
		{
			this.SendPropertyChanging();
			entity.MSC_Module_refModule = null;
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.MSC_Module_Configurations")]
	public partial class MSC_Module_Configuration : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private long _id;
		
		private long _refModule;
		
		private long _refField;
		
		private bool _Enabled;
		
		private string _ContentKz;
		
		private string _ContentRu;
		
		private EntityRef<MSC_Module> _MSC_Module_refModule;
		
		private EntityRef<MSC_Module_ConfigurationField> _MSC_Module_ConfigurationField_refField;
		
    #region Определения метода расширяемости
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnidChanging(long value);
    partial void OnidChanged();
    partial void OnrefModuleChanging(long value);
    partial void OnrefModuleChanged();
    partial void OnrefFieldChanging(long value);
    partial void OnrefFieldChanged();
    partial void OnEnabledChanging(bool value);
    partial void OnEnabledChanged();
    partial void OnContentKzChanging(string value);
    partial void OnContentKzChanged();
    partial void OnContentRuChanging(string value);
    partial void OnContentRuChanged();
    #endregion
		
		public MSC_Module_Configuration()
		{
			this._MSC_Module_refModule = default(EntityRef<MSC_Module>);
			this._MSC_Module_ConfigurationField_refField = default(EntityRef<MSC_Module_ConfigurationField>);
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
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_refModule", DbType="bigint")]
		public long refModule
		{
			get
			{
				return this._refModule;
			}
			set
			{
				if ((this._refModule != value))
				{
					if (this._MSC_Module_refModule.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnrefModuleChanging(value);
					this.SendPropertyChanging();
					this._refModule = value;
					this.SendPropertyChanged("refModule");
					this.OnrefModuleChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_refField", DbType="bigint")]
		public long refField
		{
			get
			{
				return this._refField;
			}
			set
			{
				if ((this._refField != value))
				{
					if (this._MSC_Module_ConfigurationField_refField.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnrefFieldChanging(value);
					this.SendPropertyChanging();
					this._refField = value;
					this.SendPropertyChanged("refField");
					this.OnrefFieldChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Enabled", DbType="bit")]
		public bool Enabled
		{
			get
			{
				return this._Enabled;
			}
			set
			{
				if ((this._Enabled != value))
				{
					this.OnEnabledChanging(value);
					this.SendPropertyChanging();
					this._Enabled = value;
					this.SendPropertyChanged("Enabled");
					this.OnEnabledChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ContentKz", DbType="nvarchar(MAX)", CanBeNull=false)]
		public string ContentKz
		{
			get
			{
				return this._ContentKz;
			}
			set
			{
				if ((this._ContentKz != value))
				{
					this.OnContentKzChanging(value);
					this.SendPropertyChanging();
					this._ContentKz = value;
					this.SendPropertyChanged("ContentKz");
					this.OnContentKzChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ContentRu", DbType="nvarchar(MAX)", CanBeNull=false)]
		public string ContentRu
		{
			get
			{
				return this._ContentRu;
			}
			set
			{
				if ((this._ContentRu != value))
				{
					this.OnContentRuChanging(value);
					this.SendPropertyChanging();
					this._ContentRu = value;
					this.SendPropertyChanged("ContentRu");
					this.OnContentRuChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="MSC_Module_MSC_Module_Configuration", Storage="_MSC_Module_refModule", ThisKey="refModule", OtherKey="id", IsForeignKey=true)]
		public MSC_Module MSC_Module_refModule
		{
			get
			{
				return this._MSC_Module_refModule.Entity;
			}
			set
			{
				MSC_Module previousValue = this._MSC_Module_refModule.Entity;
				if (((previousValue != value) 
							|| (this._MSC_Module_refModule.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._MSC_Module_refModule.Entity = null;
						previousValue.MSC_Module_Configurations_refModule.Remove(this);
					}
					this._MSC_Module_refModule.Entity = value;
					if ((value != null))
					{
						value.MSC_Module_Configurations_refModule.Add(this);
						this._refModule = value.id;
					}
					else
					{
						this._refModule = default(long);
					}
					this.SendPropertyChanged("MSC_Module_refModule");
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="MSC_Module_ConfigurationField_MSC_Module_Configuration", Storage="_MSC_Module_ConfigurationField_refField", ThisKey="refField", OtherKey="id", IsForeignKey=true)]
		public MSC_Module_ConfigurationField MSC_Module_ConfigurationField_refField
		{
			get
			{
				return this._MSC_Module_ConfigurationField_refField.Entity;
			}
			set
			{
				MSC_Module_ConfigurationField previousValue = this._MSC_Module_ConfigurationField_refField.Entity;
				if (((previousValue != value) 
							|| (this._MSC_Module_ConfigurationField_refField.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._MSC_Module_ConfigurationField_refField.Entity = null;
						previousValue.MSC_Module_Configurations_refField.Remove(this);
					}
					this._MSC_Module_ConfigurationField_refField.Entity = value;
					if ((value != null))
					{
						value.MSC_Module_Configurations_refField.Add(this);
						this._refField = value.id;
					}
					else
					{
						this._refField = default(long);
					}
					this.SendPropertyChanged("MSC_Module_ConfigurationField_refField");
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
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.MSC_Module_ConfigurationFields")]
	public partial class MSC_Module_ConfigurationField : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private long _id;
		
		private long _refModule;
		
		private string _Code;
		
		private string _NameKz;
		
		private string _NameRu;
		
		private bool _isDel;
		
		private EntitySet<MSC_Module_Configuration> _MSC_Module_Configurations_refField;
		
		private EntityRef<MSC_Module> _MSC_Module_refModule;
		
    #region Определения метода расширяемости
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnidChanging(long value);
    partial void OnidChanged();
    partial void OnrefModuleChanging(long value);
    partial void OnrefModuleChanged();
    partial void OnCodeChanging(string value);
    partial void OnCodeChanged();
    partial void OnNameKzChanging(string value);
    partial void OnNameKzChanged();
    partial void OnNameRuChanging(string value);
    partial void OnNameRuChanged();
    partial void OnisDelChanging(bool value);
    partial void OnisDelChanged();
    #endregion
		
		public MSC_Module_ConfigurationField()
		{
			this._MSC_Module_Configurations_refField = new EntitySet<MSC_Module_Configuration>(new Action<MSC_Module_Configuration>(this.attach_MSC_Module_Configurations_refField), new Action<MSC_Module_Configuration>(this.detach_MSC_Module_Configurations_refField));
			this._MSC_Module_refModule = default(EntityRef<MSC_Module>);
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
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_refModule", DbType="bigint")]
		public long refModule
		{
			get
			{
				return this._refModule;
			}
			set
			{
				if ((this._refModule != value))
				{
					if (this._MSC_Module_refModule.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnrefModuleChanging(value);
					this.SendPropertyChanging();
					this._refModule = value;
					this.SendPropertyChanged("refModule");
					this.OnrefModuleChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Code", DbType="nvarchar(100)", CanBeNull=false)]
		public string Code
		{
			get
			{
				return this._Code;
			}
			set
			{
				if ((this._Code != value))
				{
					this.OnCodeChanging(value);
					this.SendPropertyChanging();
					this._Code = value;
					this.SendPropertyChanged("Code");
					this.OnCodeChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_NameKz", DbType="nvarchar(200)", CanBeNull=false)]
		public string NameKz
		{
			get
			{
				return this._NameKz;
			}
			set
			{
				if ((this._NameKz != value))
				{
					this.OnNameKzChanging(value);
					this.SendPropertyChanging();
					this._NameKz = value;
					this.SendPropertyChanged("NameKz");
					this.OnNameKzChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_NameRu", DbType="nvarchar(200)", CanBeNull=false)]
		public string NameRu
		{
			get
			{
				return this._NameRu;
			}
			set
			{
				if ((this._NameRu != value))
				{
					this.OnNameRuChanging(value);
					this.SendPropertyChanging();
					this._NameRu = value;
					this.SendPropertyChanged("NameRu");
					this.OnNameRuChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_isDel", DbType="bit")]
		public bool isDel
		{
			get
			{
				return this._isDel;
			}
			set
			{
				if ((this._isDel != value))
				{
					this.OnisDelChanging(value);
					this.SendPropertyChanging();
					this._isDel = value;
					this.SendPropertyChanged("isDel");
					this.OnisDelChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="MSC_Module_ConfigurationField_MSC_Module_Configuration", Storage="_MSC_Module_Configurations_refField", ThisKey="id", OtherKey="refField")]
		public EntitySet<MSC_Module_Configuration> MSC_Module_Configurations_refField
		{
			get
			{
				return this._MSC_Module_Configurations_refField;
			}
			set
			{
				this._MSC_Module_Configurations_refField.Assign(value);
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="MSC_Module_MSC_Module_ConfigurationField", Storage="_MSC_Module_refModule", ThisKey="refModule", OtherKey="id", IsForeignKey=true)]
		public MSC_Module MSC_Module_refModule
		{
			get
			{
				return this._MSC_Module_refModule.Entity;
			}
			set
			{
				MSC_Module previousValue = this._MSC_Module_refModule.Entity;
				if (((previousValue != value) 
							|| (this._MSC_Module_refModule.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._MSC_Module_refModule.Entity = null;
						previousValue.MSC_Module_ConfigurationFields_refModule.Remove(this);
					}
					this._MSC_Module_refModule.Entity = value;
					if ((value != null))
					{
						value.MSC_Module_ConfigurationFields_refModule.Add(this);
						this._refModule = value.id;
					}
					else
					{
						this._refModule = default(long);
					}
					this.SendPropertyChanged("MSC_Module_refModule");
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
		
		private void attach_MSC_Module_Configurations_refField(MSC_Module_Configuration entity)
		{
			this.SendPropertyChanging();
			entity.MSC_Module_ConfigurationField_refField = this;
		}
		
		private void detach_MSC_Module_Configurations_refField(MSC_Module_Configuration entity)
		{
			this.SendPropertyChanging();
			entity.MSC_Module_ConfigurationField_refField = null;
		}
	}
}
#pragma warning restore 1591
