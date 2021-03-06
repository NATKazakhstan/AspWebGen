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

namespace Nat.Web.Controls.Data
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
	internal partial class DBUploadFilesDataContext : System.Data.Linq.DataContext
	{
		
		private static System.Data.Linq.Mapping.MappingSource mappingSource = new AttributeMappingSource();
		
    #region Определения метода расширяемости
    partial void OnCreated();
    partial void InsertSYS_FileUpload(SYS_FileUpload instance);
    partial void UpdateSYS_FileUpload(SYS_FileUpload instance);
    partial void DeleteSYS_FileUpload(SYS_FileUpload instance);
    #endregion
		
		public DBUploadFilesDataContext() : 
				base(global::System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnectionString1"].ConnectionString, mappingSource)
		{
			OnCreated();
		}
		
		public DBUploadFilesDataContext(string connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public DBUploadFilesDataContext(System.Data.IDbConnection connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public DBUploadFilesDataContext(string connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public DBUploadFilesDataContext(System.Data.IDbConnection connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public System.Data.Linq.Table<SYS_FileUpload> SYS_FileUploads
		{
			get
			{
				return this.GetTable<SYS_FileUpload>();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.SYS_FileUploads")]
	public partial class SYS_FileUpload : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private long _id;
		
		private System.Data.Linq.Binary _data;
		
		private string _dataFileName;
		
		private System.Nullable<System.DateTime> _UploadDate;
		
		private string _SubSystemName;
		
		private string _PersonSID;
		
    #region Определения метода расширяемости
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnidChanging(long value);
    partial void OnidChanged();
    partial void OndataChanging(System.Data.Linq.Binary value);
    partial void OndataChanged();
    partial void OndataFileNameChanging(string value);
    partial void OndataFileNameChanged();
    partial void OnUploadDateChanging(System.Nullable<System.DateTime> value);
    partial void OnUploadDateChanged();
    partial void OnSubSystemNameChanging(string value);
    partial void OnSubSystemNameChanged();
    partial void OnPersonSIDChanging(string value);
    partial void OnPersonSIDChanged();
    #endregion
		
		public SYS_FileUpload()
		{
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
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_data", DbType="VarBinary(MAX)", UpdateCheck=UpdateCheck.Never)]
		public System.Data.Linq.Binary data
		{
			get
			{
				return this._data;
			}
			set
			{
				if ((this._data != value))
				{
					this.OndataChanging(value);
					this.SendPropertyChanging();
					this._data = value;
					this.SendPropertyChanged("data");
					this.OndataChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_dataFileName", DbType="NVarChar(MAX)")]
		public string dataFileName
		{
			get
			{
				return this._dataFileName;
			}
			set
			{
				if ((this._dataFileName != value))
				{
					this.OndataFileNameChanging(value);
					this.SendPropertyChanging();
					this._dataFileName = value;
					this.SendPropertyChanged("dataFileName");
					this.OndataFileNameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_UploadDate", DbType="DateTime")]
		public System.Nullable<System.DateTime> UploadDate
		{
			get
			{
				return this._UploadDate;
			}
			set
			{
				if ((this._UploadDate != value))
				{
					this.OnUploadDateChanging(value);
					this.SendPropertyChanging();
					this._UploadDate = value;
					this.SendPropertyChanged("UploadDate");
					this.OnUploadDateChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_SubSystemName", DbType="NVarChar(MAX)")]
		public string SubSystemName
		{
			get
			{
				return this._SubSystemName;
			}
			set
			{
				if ((this._SubSystemName != value))
				{
					this.OnSubSystemNameChanging(value);
					this.SendPropertyChanging();
					this._SubSystemName = value;
					this.SendPropertyChanged("SubSystemName");
					this.OnSubSystemNameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_PersonSID", DbType="NVarChar(200)")]
		public string PersonSID
		{
			get
			{
				return this._PersonSID;
			}
			set
			{
				if ((this._PersonSID != value))
				{
					this.OnPersonSIDChanging(value);
					this.SendPropertyChanging();
					this._PersonSID = value;
					this.SendPropertyChanged("PersonSID");
					this.OnPersonSIDChanged();
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
}
#pragma warning restore 1591
