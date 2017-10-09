if exists (select 1
          from sysobjects
          where  id = object_id('SYS_DeleteUserFilter')
          and type in ('P','PC'))
   drop procedure SYS_DeleteUserFilter
go

if exists (select 1
          from sysobjects
          where  id = object_id('SYS_GetDefaultUserFilter')
          and type in ('P','PC'))
   drop procedure SYS_GetDefaultUserFilter
go

if exists (select 1
          from sysobjects
          where  id = object_id('SYS_GetFilterValues')
          and type in ('P','PC'))
   drop procedure SYS_GetFilterValues
go

if exists (select 1
          from sysobjects
          where  id = object_id('SYS_GetUserFilters')
          and type in ('P','PC'))
   drop procedure SYS_GetUserFilters
go

if exists (select 1
          from sysobjects
          where  id = object_id('SYS_SetDefaultUserFilter')
          and type in ('P','PC'))
   drop procedure SYS_SetDefaultUserFilter
go

if exists (select 1
          from sysobjects
          where  id = object_id('SYS_SetFilterValues')
          and type in ('P','PC'))
   drop procedure SYS_SetFilterValues
go

if exists (select 1
          from sysobjects
          where  id = object_id('SYS_SetIsDangerousUserFilter')
          and type in ('P','PC'))
   drop procedure SYS_SetIsDangerousUserFilter
go

if exists (select 1
          from sysobjects
          where  id = object_id('SYS_SetUserFilters')
          and type in ('P','PC'))
   drop procedure SYS_SetUserFilters
go

if exists (select 1
   from sys.sysreferences r join sys.sysobjects o on (o.id = r.constid and o.type = 'F')
   where r.fkeyid = object_id('RVS_SavedProperties') and o.name = 'FK_RVS_SavedProperties_RVS_Properties_refProperties')
alter table RVS_SavedProperties
   drop constraint FK_RVS_SavedProperties_RVS_Properties_refProperties
go

if exists (select 1
   from sys.sysreferences r join sys.sysobjects o on (o.id = r.constid and o.type = 'F')
   where r.fkeyid = object_id('SYS_UserFilterValues') and o.name = 'FK_SYS_UserFilterValues_SYS_UserFilters_refUserFilter')
alter table SYS_UserFilterValues
   drop constraint FK_SYS_UserFilterValues_SYS_UserFilters_refUserFilter
go

if exists (select 1
   from sys.sysreferences r join sys.sysobjects o on (o.id = r.constid and o.type = 'F')
   where r.fkeyid = object_id('SYS_UserFilters') and o.name = 'FK_SYS_UserFilters_SYS_UserFilterValues_refUserFilterValueDefault')
alter table SYS_UserFilters
   drop constraint FK_SYS_UserFilters_SYS_UserFilterValues_refUserFilterValueDefault
go

if exists (select 1
            from  sysindexes
           where  id    = object_id('LOG_TraceTimingRequests')
            and   name  = 'IND_TraceKey'
            and   indid > 0
            and   indid < 255)
   drop index LOG_TraceTimingRequests.IND_TraceKey
go

if exists (select 1
            from  sysobjects
           where  id = object_id('LOG_TraceTimingRequests')
            and   type = 'U')
   drop table LOG_TraceTimingRequests
go

if exists (select 1
            from  sysobjects
           where  id = object_id('RVS_Properties')
            and   type = 'U')
   drop table RVS_Properties
go

if exists (select 1
            from  sysobjects
           where  id = object_id('RVS_SavedProperties')
            and   type = 'U')
   drop table RVS_SavedProperties
go

if exists (select 1
            from  sysobjects
           where  id = object_id('SYS_FileUploads')
            and   type = 'U')
   drop table SYS_FileUploads
go

if exists (select 1
            from  sysindexes
           where  id    = object_id('SYS_FilterValues')
            and   name  = 'Index_SidKey'
            and   indid > 0
            and   indid < 255)
   drop index SYS_FilterValues.Index_SidKey
go

if exists (select 1
            from  sysobjects
           where  id = object_id('SYS_FilterValues')
            and   type = 'U')
   drop table SYS_FilterValues
go

if exists (select 1
            from  sysindexes
           where  id    = object_id('SYS_ReferencesConflictResolver')
            and   name  = 'NDX_Code'
            and   indid > 0
            and   indid < 255)
   drop index SYS_ReferencesConflictResolver.NDX_Code
go

if exists (select 1
            from  sysobjects
           where  id = object_id('SYS_ReferencesConflictResolver')
            and   type = 'U')
   drop table SYS_ReferencesConflictResolver
go

if exists (select 1
            from  sysobjects
           where  id = object_id('SYS_UserFilterValues')
            and   type = 'U')
   drop table SYS_UserFilterValues
go

if exists (select 1
            from  sysindexes
           where  id    = object_id('SYS_UserFilters')
            and   name  = 'Index_SYS_UserFilters'
            and   indid > 0
            and   indid < 255)
   drop index SYS_UserFilters.Index_SYS_UserFilters
go

if exists (select 1
            from  sysobjects
           where  id = object_id('SYS_UserFilters')
            and   type = 'U')
   drop table SYS_UserFilters
go

/*==============================================================*/
/* Table: LOG_TraceTimingRequests                               */
/*==============================================================*/
create table LOG_TraceTimingRequests (
   id                   bigint               not null,
   TraceKey             uniqueidentifier     not null,
   Page                 nvarchar(255)        not null,
   Url                  nvarchar(MAX)        not null,
   DateTimeStart        datetime             not null,
   TimeOfCreatingPage   bigint               not null,
   TimeOfDestinationUser bigint               null,
   UserSID              nvarchar(200)        collate Cyrillic_General_CS_AS null,
   refRegion            bigint               null,
   TableName            nvarchar(255)        null,
   SelectMode           nvarchar(255)        null,
   PageType             nvarchar(255)        null,
   Parameters           xml                  null,
   Trace                xml                  null,
   constraint PK_LOG_TRACETIMINGREQUESTS primary key (id)
)
go

if exists (select 1 from  sys.extended_properties
           where major_id = object_id('LOG_TraceTimingRequests') and minor_id = 0)
begin 
   declare @CurrentUser sysname 
select @CurrentUser = user_name() 
execute sp_dropextendedproperty 'MS_Description',  
   'user', @CurrentUser, 'table', 'LOG_TraceTimingRequests' 
 
end 


select @CurrentUser = user_name() 
execute sp_addextendedproperty 'MS_Description',  
   'Трассировка времени запросов', 
   'user', @CurrentUser, 'table', 'LOG_TraceTimingRequests'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('LOG_TraceTimingRequests')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'id')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'LOG_TraceTimingRequests', 'column', 'id'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Идентификатор',
   'user', @CurrentUser, 'table', 'LOG_TraceTimingRequests', 'column', 'id'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('LOG_TraceTimingRequests')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'TraceKey')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'LOG_TraceTimingRequests', 'column', 'TraceKey'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Ключь для поиска записи',
   'user', @CurrentUser, 'table', 'LOG_TraceTimingRequests', 'column', 'TraceKey'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('LOG_TraceTimingRequests')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'Page')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'LOG_TraceTimingRequests', 'column', 'Page'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Страница',
   'user', @CurrentUser, 'table', 'LOG_TraceTimingRequests', 'column', 'Page'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('LOG_TraceTimingRequests')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'Url')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'LOG_TraceTimingRequests', 'column', 'Url'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Адрес страницы',
   'user', @CurrentUser, 'table', 'LOG_TraceTimingRequests', 'column', 'Url'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('LOG_TraceTimingRequests')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'DateTimeStart')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'LOG_TraceTimingRequests', 'column', 'DateTimeStart'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Время начала выполнения запроса',
   'user', @CurrentUser, 'table', 'LOG_TraceTimingRequests', 'column', 'DateTimeStart'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('LOG_TraceTimingRequests')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'TimeOfCreatingPage')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'LOG_TraceTimingRequests', 'column', 'TimeOfCreatingPage'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Время выполнения страницы, мс',
   'user', @CurrentUser, 'table', 'LOG_TraceTimingRequests', 'column', 'TimeOfCreatingPage'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('LOG_TraceTimingRequests')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'TimeOfDestinationUser')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'LOG_TraceTimingRequests', 'column', 'TimeOfDestinationUser'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Время достижения информации до пользователя, мс',
   'user', @CurrentUser, 'table', 'LOG_TraceTimingRequests', 'column', 'TimeOfDestinationUser'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('LOG_TraceTimingRequests')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'UserSID')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'LOG_TraceTimingRequests', 'column', 'UserSID'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Пользователь',
   'user', @CurrentUser, 'table', 'LOG_TraceTimingRequests', 'column', 'UserSID'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('LOG_TraceTimingRequests')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'refRegion')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'LOG_TraceTimingRequests', 'column', 'refRegion'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Регион',
   'user', @CurrentUser, 'table', 'LOG_TraceTimingRequests', 'column', 'refRegion'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('LOG_TraceTimingRequests')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'TableName')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'LOG_TraceTimingRequests', 'column', 'TableName'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Наименование таблицы',
   'user', @CurrentUser, 'table', 'LOG_TraceTimingRequests', 'column', 'TableName'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('LOG_TraceTimingRequests')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'SelectMode')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'LOG_TraceTimingRequests', 'column', 'SelectMode'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Режим выбора записей',
   'user', @CurrentUser, 'table', 'LOG_TraceTimingRequests', 'column', 'SelectMode'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('LOG_TraceTimingRequests')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'PageType')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'LOG_TraceTimingRequests', 'column', 'PageType'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Тип страницы (просмотр, редактирование, журнал)',
   'user', @CurrentUser, 'table', 'LOG_TraceTimingRequests', 'column', 'PageType'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('LOG_TraceTimingRequests')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'Parameters')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'LOG_TraceTimingRequests', 'column', 'Parameters'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Параметры страницы',
   'user', @CurrentUser, 'table', 'LOG_TraceTimingRequests', 'column', 'Parameters'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('LOG_TraceTimingRequests')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'Trace')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'LOG_TraceTimingRequests', 'column', 'Trace'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Трасировка страницы',
   'user', @CurrentUser, 'table', 'LOG_TraceTimingRequests', 'column', 'Trace'
go

/*==============================================================*/
/* Index: IND_TraceKey                                          */
/*==============================================================*/
create unique index IND_TraceKey on LOG_TraceTimingRequests (
TraceKey ASC
)
go

/*==============================================================*/
/* Table: RVS_Properties                                        */
/*==============================================================*/
create table RVS_Properties (
   id                   bigint               identity(1, 1),
   nameKz               nvarchar(255)        not null,
   nameRu               nvarchar(255)        not null,
   JournalTypeName      nvarchar(300)        not null,
   Grouping             xml                  null,
   Filter               xml                  null,
   ColumnsVisible       xml                  null,
   ColumnsStyle         xml                  null,
   RowsStyle            xml                  null,
   CellsStyle           xml                  null,
   FixedHeader          xml                  null,
   StorageValues        varbinary(MAX)       null,
   ReportPluginName     nvarchar(255)        null,
   OrderByColumns       xml                  null,
   OtherParameters      xml                  null,
   constraint PK_RVS_PROPERTIES1 primary key (id)
)
go

if exists (select 1 from  sys.extended_properties
           where major_id = object_id('RVS_Properties') and minor_id = 0)
begin 
   declare @CurrentUser sysname 
select @CurrentUser = user_name() 
execute sp_dropextendedproperty 'MS_Description',  
   'user', @CurrentUser, 'table', 'RVS_Properties' 
 
end 


select @CurrentUser = user_name() 
execute sp_addextendedproperty 'MS_Description',  
   'Сохраненные свойства табличных представлений', 
   'user', @CurrentUser, 'table', 'RVS_Properties'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('RVS_Properties')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'id')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'RVS_Properties', 'column', 'id'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Идентификатор',
   'user', @CurrentUser, 'table', 'RVS_Properties', 'column', 'id'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('RVS_Properties')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'nameKz')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'RVS_Properties', 'column', 'nameKz'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Наименование представления (КАЗ)',
   'user', @CurrentUser, 'table', 'RVS_Properties', 'column', 'nameKz'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('RVS_Properties')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'nameRu')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'RVS_Properties', 'column', 'nameRu'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Наименование представления (РУС)',
   'user', @CurrentUser, 'table', 'RVS_Properties', 'column', 'nameRu'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('RVS_Properties')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'JournalTypeName')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'RVS_Properties', 'column', 'JournalTypeName'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Наименование имени класса журнала',
   'user', @CurrentUser, 'table', 'RVS_Properties', 'column', 'JournalTypeName'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('RVS_Properties')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'Grouping')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'RVS_Properties', 'column', 'Grouping'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Группировки',
   'user', @CurrentUser, 'table', 'RVS_Properties', 'column', 'Grouping'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('RVS_Properties')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'Filter')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'RVS_Properties', 'column', 'Filter'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Фильтры',
   'user', @CurrentUser, 'table', 'RVS_Properties', 'column', 'Filter'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('RVS_Properties')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'ColumnsVisible')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'RVS_Properties', 'column', 'ColumnsVisible'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Видимость колонок',
   'user', @CurrentUser, 'table', 'RVS_Properties', 'column', 'ColumnsVisible'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('RVS_Properties')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'ColumnsStyle')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'RVS_Properties', 'column', 'ColumnsStyle'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Стили колонок',
   'user', @CurrentUser, 'table', 'RVS_Properties', 'column', 'ColumnsStyle'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('RVS_Properties')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'RowsStyle')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'RVS_Properties', 'column', 'RowsStyle'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Стили строк',
   'user', @CurrentUser, 'table', 'RVS_Properties', 'column', 'RowsStyle'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('RVS_Properties')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'CellsStyle')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'RVS_Properties', 'column', 'CellsStyle'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Стили ячеек',
   'user', @CurrentUser, 'table', 'RVS_Properties', 'column', 'CellsStyle'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('RVS_Properties')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'FixedHeader')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'RVS_Properties', 'column', 'FixedHeader'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Настройки фиксации заголовка',
   'user', @CurrentUser, 'table', 'RVS_Properties', 'column', 'FixedHeader'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('RVS_Properties')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'StorageValues')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'RVS_Properties', 'column', 'StorageValues'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Параметры отчета',
   'user', @CurrentUser, 'table', 'RVS_Properties', 'column', 'StorageValues'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('RVS_Properties')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'ReportPluginName')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'RVS_Properties', 'column', 'ReportPluginName'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Имя плагина',
   'user', @CurrentUser, 'table', 'RVS_Properties', 'column', 'ReportPluginName'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('RVS_Properties')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'OrderByColumns')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'RVS_Properties', 'column', 'OrderByColumns'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Сортировка журнала',
   'user', @CurrentUser, 'table', 'RVS_Properties', 'column', 'OrderByColumns'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('RVS_Properties')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'OtherParameters')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'RVS_Properties', 'column', 'OtherParameters'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Дополнительные параметры',
   'user', @CurrentUser, 'table', 'RVS_Properties', 'column', 'OtherParameters'
go

/*==============================================================*/
/* Table: RVS_SavedProperties                                   */
/*==============================================================*/
create table RVS_SavedProperties (
   id                   bigint               not null,
   nameKz               nvarchar(255)        not null,
   nameRu               nvarchar(255)        not null,
   refProperties        bigint               not null,
   dateTime             datetime             not null,
   UserSID              nvarchar(200)        collate Cyrillic_General_CS_AS not null,
   JournalTypeName      nvarchar(300)        not null,
   isDefaultView        bit                  not null,
   isSharedView         bit                  not null,
   context              nvarchar(200)        not null,
   constraint PK_RVS_SAVEDPROPERTIES primary key (id)
)
go

if exists (select 1 from  sys.extended_properties
           where major_id = object_id('RVS_SavedProperties') and minor_id = 0)
begin 
   declare @CurrentUser sysname 
select @CurrentUser = user_name() 
execute sp_dropextendedproperty 'MS_Description',  
   'user', @CurrentUser, 'table', 'RVS_SavedProperties' 
 
end 


select @CurrentUser = user_name() 
execute sp_addextendedproperty 'MS_Description',  
   'Сохраненные настройки представления пользователем', 
   'user', @CurrentUser, 'table', 'RVS_SavedProperties'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('RVS_SavedProperties')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'id')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'RVS_SavedProperties', 'column', 'id'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Идентификатор',
   'user', @CurrentUser, 'table', 'RVS_SavedProperties', 'column', 'id'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('RVS_SavedProperties')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'nameKz')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'RVS_SavedProperties', 'column', 'nameKz'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Наименование представления (КАЗ)',
   'user', @CurrentUser, 'table', 'RVS_SavedProperties', 'column', 'nameKz'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('RVS_SavedProperties')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'nameRu')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'RVS_SavedProperties', 'column', 'nameRu'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Наименование представления (РУС)',
   'user', @CurrentUser, 'table', 'RVS_SavedProperties', 'column', 'nameRu'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('RVS_SavedProperties')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'refProperties')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'RVS_SavedProperties', 'column', 'refProperties'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Свойства представления',
   'user', @CurrentUser, 'table', 'RVS_SavedProperties', 'column', 'refProperties'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('RVS_SavedProperties')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'dateTime')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'RVS_SavedProperties', 'column', 'dateTime'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Дата сохранения',
   'user', @CurrentUser, 'table', 'RVS_SavedProperties', 'column', 'dateTime'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('RVS_SavedProperties')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'UserSID')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'RVS_SavedProperties', 'column', 'UserSID'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'SID пользователя',
   'user', @CurrentUser, 'table', 'RVS_SavedProperties', 'column', 'UserSID'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('RVS_SavedProperties')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'JournalTypeName')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'RVS_SavedProperties', 'column', 'JournalTypeName'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Наименование имени класса журнала',
   'user', @CurrentUser, 'table', 'RVS_SavedProperties', 'column', 'JournalTypeName'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('RVS_SavedProperties')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'isDefaultView')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'RVS_SavedProperties', 'column', 'isDefaultView'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Представление по умолчанию',
   'user', @CurrentUser, 'table', 'RVS_SavedProperties', 'column', 'isDefaultView'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('RVS_SavedProperties')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'isSharedView')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'RVS_SavedProperties', 'column', 'isSharedView'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Общее представление',
   'user', @CurrentUser, 'table', 'RVS_SavedProperties', 'column', 'isSharedView'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('RVS_SavedProperties')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'context')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'RVS_SavedProperties', 'column', 'context'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Контекст сохраненного представления',
   'user', @CurrentUser, 'table', 'RVS_SavedProperties', 'column', 'context'
go

/*==============================================================*/
/* Table: SYS_FileUploads                                       */
/*==============================================================*/
create table SYS_FileUploads (
   id                   bigint               identity,
   data                 varbinary(max)       not null,
   dataFileName         varchar(max)         not null,
   UploadDate           datetime             not null,
   SubSystemName        varchar(max)         not null,
   PersonSID            nvarchar(200)        collate Cyrillic_General_CS_AS not null,
   constraint PK_SYS_FILEUPLOADS primary key (id)
)
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('SYS_FileUploads')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'id')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'SYS_FileUploads', 'column', 'id'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'идентификатор',
   'user', @CurrentUser, 'table', 'SYS_FileUploads', 'column', 'id'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('SYS_FileUploads')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'data')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'SYS_FileUploads', 'column', 'data'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'содержимое файла',
   'user', @CurrentUser, 'table', 'SYS_FileUploads', 'column', 'data'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('SYS_FileUploads')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'dataFileName')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'SYS_FileUploads', 'column', 'dataFileName'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Имя файла',
   'user', @CurrentUser, 'table', 'SYS_FileUploads', 'column', 'dataFileName'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('SYS_FileUploads')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'UploadDate')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'SYS_FileUploads', 'column', 'UploadDate'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Дата загрузки',
   'user', @CurrentUser, 'table', 'SYS_FileUploads', 'column', 'UploadDate'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('SYS_FileUploads')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'SubSystemName')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'SYS_FileUploads', 'column', 'SubSystemName'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Наименование подсистемы',
   'user', @CurrentUser, 'table', 'SYS_FileUploads', 'column', 'SubSystemName'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('SYS_FileUploads')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'PersonSID')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'SYS_FileUploads', 'column', 'PersonSID'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'SID сотрудника',
   'user', @CurrentUser, 'table', 'SYS_FileUploads', 'column', 'PersonSID'
go

/*==============================================================*/
/* Table: SYS_FilterValues                                      */
/*==============================================================*/
create table SYS_FilterValues (
   id                   bigint               identity,
   "key"                nvarchar(400)        not null,
   "values"             varbinary(MAX)       not null,
   BinarySid            varbinary(64)        not null default 0x0,
   UserSID              nvarchar(200)        collate Cyrillic_General_CS_AS not null default '',
   constraint PK_SYS_FILTERVALUES primary key (id)
)
go

if exists (select 1 from  sys.extended_properties
           where major_id = object_id('SYS_FilterValues') and minor_id = 0)
begin 
   declare @CurrentUser sysname 
select @CurrentUser = user_name() 
execute sp_dropextendedproperty 'MS_Description',  
   'user', @CurrentUser, 'table', 'SYS_FilterValues' 
 
end 


select @CurrentUser = user_name() 
execute sp_addextendedproperty 'MS_Description',  
   'Выбранные значения пользователями системы', 
   'user', @CurrentUser, 'table', 'SYS_FilterValues'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('SYS_FilterValues')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'id')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'SYS_FilterValues', 'column', 'id'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Идентификатор',
   'user', @CurrentUser, 'table', 'SYS_FilterValues', 'column', 'id'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('SYS_FilterValues')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'key')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'SYS_FilterValues', 'column', 'key'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Ключ элемента значений',
   'user', @CurrentUser, 'table', 'SYS_FilterValues', 'column', 'key'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('SYS_FilterValues')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'values')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'SYS_FilterValues', 'column', 'values'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Заполненные значения пользователем',
   'user', @CurrentUser, 'table', 'SYS_FilterValues', 'column', 'values'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('SYS_FilterValues')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'BinarySid')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'SYS_FilterValues', 'column', 'BinarySid'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'SID пользователя',
   'user', @CurrentUser, 'table', 'SYS_FilterValues', 'column', 'BinarySid'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('SYS_FilterValues')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'UserSID')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'SYS_FilterValues', 'column', 'UserSID'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'SID пользователя',
   'user', @CurrentUser, 'table', 'SYS_FilterValues', 'column', 'UserSID'
go

/*==============================================================*/
/* Index: Index_SidKey                                          */
/*==============================================================*/
create index Index_SidKey on SYS_FilterValues (
BinarySid ASC,
"key" ASC
)
go

/*==============================================================*/
/* Table: SYS_ReferencesConflictResolver                        */
/*==============================================================*/
create table SYS_ReferencesConflictResolver (
   id                   bigint               identity,
   Code                 nvarchar(250)        not null,
   RegexSearch          nvarchar(250)        null,
   ConstraintName       nvarchar(250)        null,
   TableName            nvarchar(250)        null,
   ReferencesConflictResolverClass nvarchar(500)        null,
   ErrorMessageRu       nvarchar(250)        null,
   ErrorMessageKz       nvarchar(250)        null,
   constraint PK_SYS_REFERENCESCONFLICTRESOL primary key (id)
)
go

/*==============================================================*/
/* Index: NDX_Code                                              */
/*==============================================================*/
create unique index NDX_Code on SYS_ReferencesConflictResolver (
Code ASC
)
go

/*==============================================================*/
/* Table: SYS_UserFilterValues                                  */
/*==============================================================*/
create table SYS_UserFilterValues (
   id                   bigint               not null,
   refUserFilter        bigint               not null,
   Name                 nvarchar(100)        not null,
   FilterValues         nvarchar(MAX)        not null,
   isDangerous          bit                  not null,
   constraint PK_SYS_USERFILTERVALUES primary key (id)
)
go

if exists (select 1 from  sys.extended_properties
           where major_id = object_id('SYS_UserFilterValues') and minor_id = 0)
begin 
   declare @CurrentUser sysname 
select @CurrentUser = user_name() 
execute sp_dropextendedproperty 'MS_Description',  
   'user', @CurrentUser, 'table', 'SYS_UserFilterValues' 
 
end 


select @CurrentUser = user_name() 
execute sp_addextendedproperty 'MS_Description',  
   'Список настроенных фильтров', 
   'user', @CurrentUser, 'table', 'SYS_UserFilterValues'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('SYS_UserFilterValues')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'id')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'SYS_UserFilterValues', 'column', 'id'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Идентификатор',
   'user', @CurrentUser, 'table', 'SYS_UserFilterValues', 'column', 'id'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('SYS_UserFilterValues')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'refUserFilter')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'SYS_UserFilterValues', 'column', 'refUserFilter'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Фильтр журнала',
   'user', @CurrentUser, 'table', 'SYS_UserFilterValues', 'column', 'refUserFilter'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('SYS_UserFilterValues')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'Name')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'SYS_UserFilterValues', 'column', 'Name'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Имя фильтра',
   'user', @CurrentUser, 'table', 'SYS_UserFilterValues', 'column', 'Name'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('SYS_UserFilterValues')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'FilterValues')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'SYS_UserFilterValues', 'column', 'FilterValues'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Значения фильтра',
   'user', @CurrentUser, 'table', 'SYS_UserFilterValues', 'column', 'FilterValues'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('SYS_UserFilterValues')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'isDangerous')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'SYS_UserFilterValues', 'column', 'isDangerous'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Опаный фильтр (возникал timeout)',
   'user', @CurrentUser, 'table', 'SYS_UserFilterValues', 'column', 'isDangerous'
go

/*==============================================================*/
/* Table: SYS_UserFilters                                       */
/*==============================================================*/
create table SYS_UserFilters (
   id                   bigint               not null,
   TableName            nvarchar(250)        not null,
   UserSID              nvarchar(200)        collate Cyrillic_General_CS_AS not null,
   refUserFilterValueDefault bigint               null,
   constraint PK_SYS_USERFILTERS primary key (id)
)
go

if exists (select 1 from  sys.extended_properties
           where major_id = object_id('SYS_UserFilters') and minor_id = 0)
begin 
   declare @CurrentUser sysname 
select @CurrentUser = user_name() 
execute sp_dropextendedproperty 'MS_Description',  
   'user', @CurrentUser, 'table', 'SYS_UserFilters' 
 
end 


select @CurrentUser = user_name() 
execute sp_addextendedproperty 'MS_Description',  
   'Фильтры пользователей к журналам', 
   'user', @CurrentUser, 'table', 'SYS_UserFilters'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('SYS_UserFilters')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'id')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'SYS_UserFilters', 'column', 'id'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Идентификатор',
   'user', @CurrentUser, 'table', 'SYS_UserFilters', 'column', 'id'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('SYS_UserFilters')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'TableName')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'SYS_UserFilters', 'column', 'TableName'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Наименования таблицы',
   'user', @CurrentUser, 'table', 'SYS_UserFilters', 'column', 'TableName'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('SYS_UserFilters')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'UserSID')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'SYS_UserFilters', 'column', 'UserSID'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'SID пользователя',
   'user', @CurrentUser, 'table', 'SYS_UserFilters', 'column', 'UserSID'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('SYS_UserFilters')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'refUserFilterValueDefault')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'SYS_UserFilters', 'column', 'refUserFilterValueDefault'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Фильтр по умолчанию',
   'user', @CurrentUser, 'table', 'SYS_UserFilters', 'column', 'refUserFilterValueDefault'
go

/*==============================================================*/
/* Index: Index_SYS_UserFilters                                 */
/*==============================================================*/
create index Index_SYS_UserFilters on SYS_UserFilters (
TableName ASC,
UserSID ASC
)
include (id)
go

alter table RVS_SavedProperties
   add constraint FK_RVS_SavedProperties_RVS_Properties_refProperties foreign key (refProperties)
      references RVS_Properties (id)
         on delete cascade
go

alter table SYS_UserFilterValues
   add constraint FK_SYS_UserFilterValues_SYS_UserFilters_refUserFilter foreign key (refUserFilter)
      references SYS_UserFilters (id)
         on delete cascade
go

alter table SYS_UserFilters
   add constraint FK_SYS_UserFilters_SYS_UserFilterValues_refUserFilterValueDefault foreign key (refUserFilterValueDefault)
      references SYS_UserFilterValues (id)
go


create procedure SYS_DeleteUserFilter
    @refUserFilterValues bigint
as
begin
    update SYS_UserFilters set refUserFilterValueDefault = null
    where refUserFilterValueDefault = @refUserFilterValues

    delete from SYS_UserFilterValues
    where id = @refUserFilterValues
end
go


create procedure SYS_GetDefaultUserFilter
    @TableName nvarchar(250),
    @sid nvarchar(400)
as
begin
    select ufv.id, ufv.Name, ufv.isDangerous, ufv.FilterValues
    from SYS_UserFilters uf
    join SYS_UserFilterValues ufv on ufv.id = uf.refUserFilterValueDefault
    where uf.TableName = @TableName and uf.[UserSid] = @sid
	order by ufv.Name
end
go


set ANSI_NULLS ON
set QUOTED_IDENTIFIER ON
go

-- =============================================
-- Author:		sergey.shpakovskiy
-- Create date: 23.01.2008
-- Description:	Получение условий фильтра
-- =============================================
CREATE PROCEDURE [dbo].[SYS_GetFilterValues]
	@key nvarchar (500),
	@binarySid varbinary(64),
    @sid nvarchar (400)
AS 
BEGIN
	declare @values varbinary(MAX)
	set @key = lower(@key)

	select @values = [values]
	from SYS_FilterValues
	where (binarySid = @binarySid or UserSid = @sid) and [key] = @key

	select @values
END
go


create procedure SYS_GetUserFilters
    @TableName nvarchar(250),
    @sid nvarchar(400)
as
begin
    select ufv.id, ufv.Name, ufv.isDangerous, ufv.FilterValues
    from SYS_UserFilters uf
        join SYS_UserFilterValues ufv on ufv.refUserFilter = uf.id
    where uf.TableName = @TableName and uf.[UserSID] = @sid
	order by ufv.Name
end
go


create procedure SYS_SetDefaultUserFilter
    @refUserFilterValues bigint,
    @TableName nvarchar(250),
    @sid nvarchar(400)
as
begin
    update SYS_UserFilters set refUserFilterValueDefault = @refUserFilterValues
    from SYS_UserFilters uf
    where uf.TableName = @TableName and uf.[UserSID] = @sid
end
go


set ANSI_NULLS ON
set QUOTED_IDENTIFIER ON
go

-- =============================================
-- Author:		sergey.shpakovskiy
-- Create date: 23.01.2008
-- Description:	Установление условий фильтра
-- =============================================
CREATE PROCEDURE [dbo].[SYS_SetFilterValues]
	@key nvarchar (500),
	@BinarySid varbinary(64),
    @UserSid nvarchar(400),
	@values varbinary(MAX)
AS 
BEGIN
	declare @id bigint
	set @key = lower(@key)

	select @id = id
	from SYS_FilterValues
	where (BinarySid = @BinarySid OR UserSid = @UserSid) and [key] = @key

	if @id is null
	begin
		INSERT INTO SYS_FilterValues (BinarySid, UserSid, [key], [values]) Values(@BinarySid, @UserSid, @key, @values)
	end else begin
		UPDATE SYS_FilterValues
		SET [values] = @values, UserSid = IsNull(@UserSid, UserSid), BinarySid = IsNull(@BinarySid, BinarySid)
		where id = @id
	end

	return 0
END
go


create procedure SYS_SetIsDangerousUserFilter
    @refUserFilterValues bigint
as
begin
    update SYS_UserFilterValues set isDangerous = 1
    where id = @refUserFilterValues
end
go


create procedure SYS_SetUserFilters
    @refUserFilterValues bigint,
    @TableName nvarchar(250),
    @sid nvarchar(400),
    @Name nvarchar(100),
    @FilterValues nvarchar(MAX)
as
begin
    if @refUserFilterValues is not null
        update SYS_UserFilterValues set Name = @Name, FilterValues = @FilterValues, isDangerous = 0
        where id = @refUserFilterValues
    else
    begin
        if not exists
            (
                select * 
                from SYS_UserFilters uf
                where uf.TableName = @TableName and uf.[UserSID] = @sid
            )
            insert into SYS_UserFilters(TableName, UserSID)
            select @TableName, @sid
        insert into SYS_UserFilterValues(refUserFilter, Name, FilterValues, isDangerous)
        select uf.id, @Name, @FilterValues, 0
            from SYS_UserFilters uf
        where uf.TableName = @TableName and uf.[UserSID] = @sid
    end
    select isnull(@refUserFilterValues, Scope_Identity()) as refUserFilterValues
end
go
