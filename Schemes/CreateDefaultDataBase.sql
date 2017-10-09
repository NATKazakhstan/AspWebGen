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
          from sysobjects
          where  id = object_id('SYS_getFilterValues')
          and type in ('P','PC'))
   drop procedure SYS_getFilterValues
go

if exists (select 1
          from sysobjects
          where  id = object_id('SYS_setFilterValues')
          and type in ('P','PC'))
   drop procedure SYS_setFilterValues
go

if exists (select 1
   from sys.sysreferences r join sys.sysobjects o on (o.id = r.constid and o.type = 'F')
   where r.fkeyid = object_id('LOG_TraceTimingRequests') and o.name = 'FK_LOG_TraceTimingRequests_LOG_SidIdentification_refUser')
alter table LOG_TraceTimingRequests
   drop constraint FK_LOG_TraceTimingRequests_LOG_SidIdentification_refUser
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
   where r.fkeyid = object_id('SYS_UserFilters') and o.name = 'FK_SYS_UserFilters_LOG_SidIdentification_refSid')
alter table SYS_UserFilters
   drop constraint FK_SYS_UserFilters_LOG_SidIdentification_refSid
go

if exists (select 1
   from sys.sysreferences r join sys.sysobjects o on (o.id = r.constid and o.type = 'F')
   where r.fkeyid = object_id('SYS_UserFilters') and o.name = 'FK_SYS_UserFilters_SYS_UserFilterValues_refUserFilterValueDefault')
alter table SYS_UserFilters
   drop constraint FK_SYS_UserFilters_SYS_UserFilterValues_refUserFilterValueDefault
go

if exists (select 1
            from  sysindexes
           where  id    = object_id('dbo.LOG_SidIdentification')
            and   name  = 'NDX_SidInBase64'
            and   indid > 0
            and   indid < 255)
   drop index dbo.LOG_SidIdentification.NDX_SidInBase64
go

if exists (select 1
            from  sysindexes
           where  id    = object_id('dbo.LOG_SidIdentification')
            and   name  = 'Index_Sid'
            and   indid > 0
            and   indid < 255)
   drop index dbo.LOG_SidIdentification.Index_Sid
go

if exists (select 1
            from  sysobjects
           where  id = object_id('dbo.LOG_SidIdentification')
            and   type = 'U')
   drop table dbo.LOG_SidIdentification
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
/* Table: LOG_SidIdentification                                 */
/*==============================================================*/
create table dbo.LOG_SidIdentification (
   id                   bigint               identity,
   Sid                  nvarchar(255)        not null,
   Name                 nvarchar(255)        null,
   email                nvarchar(255)        null,
   isDisabled           bit                  not null default 0,
   RowName              AS (coalesce(Name, Sid)),
   LoginName            nvarchar(255)        null,
   SidInBase64          varchar(450)         null,
   constraint PK_LOG_SIDIDENTIFICATION primary key (id)
)
go

if exists (select 1 from  sys.extended_properties
           where major_id = object_id('dbo.LOG_SidIdentification') and minor_id = 0)
begin 
   execute sp_dropextendedproperty 'MS_Description',  
   'user', 'dbo', 'table', 'LOG_SidIdentification' 
 
end 


execute sp_addextendedproperty 'MS_Description',  
   'Пользователи', 
   'user', 'dbo', 'table', 'LOG_SidIdentification'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('dbo.LOG_SidIdentification')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'id')
)
begin
   execute sp_dropextendedproperty 'MS_Description', 
   'user', 'dbo', 'table', 'LOG_SidIdentification', 'column', 'id'

end


execute sp_addextendedproperty 'MS_Description', 
   'Идентификатор',
   'user', 'dbo', 'table', 'LOG_SidIdentification', 'column', 'id'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('dbo.LOG_SidIdentification')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'Sid')
)
begin
   execute sp_dropextendedproperty 'MS_Description', 
   'user', 'dbo', 'table', 'LOG_SidIdentification', 'column', 'Sid'

end


execute sp_addextendedproperty 'MS_Description', 
   'SID пользователя AD',
   'user', 'dbo', 'table', 'LOG_SidIdentification', 'column', 'Sid'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('dbo.LOG_SidIdentification')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'Name')
)
begin
   execute sp_dropextendedproperty 'MS_Description', 
   'user', 'dbo', 'table', 'LOG_SidIdentification', 'column', 'Name'

end


execute sp_addextendedproperty 'MS_Description', 
   'Имя учетки пользователя AD',
   'user', 'dbo', 'table', 'LOG_SidIdentification', 'column', 'Name'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('dbo.LOG_SidIdentification')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'email')
)
begin
   execute sp_dropextendedproperty 'MS_Description', 
   'user', 'dbo', 'table', 'LOG_SidIdentification', 'column', 'email'

end


execute sp_addextendedproperty 'MS_Description', 
   'Адрес электронной почты',
   'user', 'dbo', 'table', 'LOG_SidIdentification', 'column', 'email'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('dbo.LOG_SidIdentification')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'isDisabled')
)
begin
   execute sp_dropextendedproperty 'MS_Description', 
   'user', 'dbo', 'table', 'LOG_SidIdentification', 'column', 'isDisabled'

end


execute sp_addextendedproperty 'MS_Description', 
   'Учетна запись заблокирована',
   'user', 'dbo', 'table', 'LOG_SidIdentification', 'column', 'isDisabled'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('dbo.LOG_SidIdentification')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'RowName')
)
begin
   execute sp_dropextendedproperty 'MS_Description', 
   'user', 'dbo', 'table', 'LOG_SidIdentification', 'column', 'RowName'

end


execute sp_addextendedproperty 'MS_Description', 
   'Наименование',
   'user', 'dbo', 'table', 'LOG_SidIdentification', 'column', 'RowName'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('dbo.LOG_SidIdentification')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'LoginName')
)
begin
   execute sp_dropextendedproperty 'MS_Description', 
   'user', 'dbo', 'table', 'LOG_SidIdentification', 'column', 'LoginName'

end


execute sp_addextendedproperty 'MS_Description', 
   'Имя входа',
   'user', 'dbo', 'table', 'LOG_SidIdentification', 'column', 'LoginName'
go

if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('dbo.LOG_SidIdentification')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'SidInBase64')
)
begin
   execute sp_dropextendedproperty 'MS_Description', 
   'user', 'dbo', 'table', 'LOG_SidIdentification', 'column', 'SidInBase64'

end


execute sp_addextendedproperty 'MS_Description', 
   'SID пользователя AD в формате base64',
   'user', 'dbo', 'table', 'LOG_SidIdentification', 'column', 'SidInBase64'
go

/*==============================================================*/
/* Index: Index_Sid                                             */
/*==============================================================*/
create unique index Index_Sid on dbo.LOG_SidIdentification (
Sid ASC
)
include (id,SidInBase64)
go

/*==============================================================*/
/* Index: NDX_SidInBase64                                       */
/*==============================================================*/
create unique index NDX_SidInBase64 on dbo.LOG_SidIdentification (
SidInBase64 ASC
)
include (id)
where (SidInBase64 is not null)
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
   refUser              bigint               null,
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
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'refUser')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'LOG_TraceTimingRequests', 'column', 'refUser'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'Пользователь',
   'user', @CurrentUser, 'table', 'LOG_TraceTimingRequests', 'column', 'refUser'
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
   refSid               bigint               not null,
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
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'refSid')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'RVS_SavedProperties', 'column', 'refSid'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'SID пользователя',
   'user', @CurrentUser, 'table', 'RVS_SavedProperties', 'column', 'refSid'
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
   PersonSID            varchar(max)         not null,
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
   refSid               bigint               not null,
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
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'refSid')
)
begin
   declare @CurrentUser sysname
select @CurrentUser = user_name()
execute sp_dropextendedproperty 'MS_Description', 
   'user', @CurrentUser, 'table', 'SYS_UserFilters', 'column', 'refSid'

end


select @CurrentUser = user_name()
execute sp_addextendedproperty 'MS_Description', 
   'SID пользователя',
   'user', @CurrentUser, 'table', 'SYS_UserFilters', 'column', 'refSid'
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
refSid ASC
)
include (id)
go

alter table LOG_TraceTimingRequests
   add constraint FK_LOG_TraceTimingRequests_LOG_SidIdentification_refUser foreign key (refUser)
      references dbo.LOG_SidIdentification (id)
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
   add constraint FK_SYS_UserFilters_LOG_SidIdentification_refSid foreign key (refSid)
      references dbo.LOG_SidIdentification (id)
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
    @sid varchar(250)
as
begin
    select ufv.id, ufv.Name, ufv.isDangerous, ufv.FilterValues
    from SYS_UserFilters uf
        join SYS_UserFilterValues ufv on ufv.id = uf.refUserFilterValueDefault
        join LOG_SidIdentification sidi on sidi.id = uf.refSid
    where uf.TableName = @TableName and sidi.[sid] = @sid
	order by ufv.Name
end
go


create procedure SYS_GetUserFilters
    @TableName nvarchar(250),
    @sid varchar(250)
as
begin
    select ufv.id, ufv.Name, ufv.isDangerous, ufv.FilterValues
    from SYS_UserFilters uf
        join SYS_UserFilterValues ufv on ufv.refUserFilter = uf.id
        join LOG_SidIdentification sidi on sidi.id = uf.refSid
    where uf.TableName = @TableName and sidi.[sid] = @sid
	order by ufv.Name
end
go


create procedure SYS_SetDefaultUserFilter
    @refUserFilterValues bigint,
    @TableName nvarchar(250),
    @sid varchar(250)
as
begin
    update SYS_UserFilters set refUserFilterValueDefault = @refUserFilterValues
    from SYS_UserFilters uf
        join LOG_SidIdentification sidi on sidi.id = uf.refSid
    where uf.TableName = @TableName and sidi.[sid] = @sid
end
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
    @sid varchar(250),
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
                    join LOG_SidIdentification sidi on sidi.id = uf.refSid
                where uf.TableName = @TableName and sidi.[sid] = @sid
            )
            insert into SYS_UserFilters(TableName, refSid)
            select @TableName, sidi.id
            from LOG_SidIdentification sidi
            where sidi.[sid] = @sid
        insert into SYS_UserFilterValues(refUserFilter, Name, FilterValues, isDangerous)
        select uf.id, @Name, @FilterValues, 0
            from SYS_UserFilters uf
            join LOG_SidIdentification sidi on sidi.id = uf.refSid
        where uf.TableName = @TableName and sidi.[sid] = @sid
    end
    select isnull(@refUserFilterValues, Scope_Identity()) as refUserFilterValues
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
CREATE PROCEDURE [dbo].[SYS_getFilterValues]
	@key nvarchar (500),
	@sid varbinary(64)
AS 
BEGIN
	declare @values varbinary(MAX)
	set @key = lower(@key)

	select @values = [values]
	from SYS_FilterValues
	where sid = @sid and [key] = @key

	select @values
END
go


set ANSI_NULLS ON
set QUOTED_IDENTIFIER ON
go

-- =============================================
-- Author:		sergey.shpakovskiy
-- Create date: 23.01.2008
-- Description:	Установление условий фильтра
-- =============================================
CREATE PROCEDURE [dbo].[SYS_setFilterValues]
	@key nvarchar (500),
	@sid varbinary(64),
	@values varbinary(MAX)
AS 
BEGIN
	declare @id bigint
	set @key = lower(@key)

	select @id = id
	from SYS_FilterValues
	where sid = @sid and [key] = @key

	if @id is null
	begin
		INSERT INTO SYS_FilterValues (sid, [key], [values]) Values(@sid, @key, @values)
	end else begin
		UPDATE SYS_FilterValues
		SET [values] = @values
		where id = @id
	end

	return 0
END
go
