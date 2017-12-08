ALTER TABLE SYS_UserFilters ADD UserSID NVARCHAR(200) collate Cyrillic_General_CS_AS NULL
go

UPDATE SYS_UserFilters
SET UserSID = s.[Sid]
FROM SYS_UserFilters uf
JOIN LOG_SidIdentification s ON s.id = uf.refSid
go

drop index AutoIndex_SYS_UserFilters_refSid on SYS_UserFilters
drop index Index_SYS_UserFilters on SYS_UserFilters
alter table SYS_UserFilters drop constraint TableRef_SYS_UserFilters_refSid

ALTER TABLE SYS_UserFilters

ALTER COLUMN UserSID NVARCHAR(200) collate Cyrillic_General_CS_AS NOT NULL

ALTER TABLE SYS_UserFilters DROP COLUMN refSid
GO

ALTER TABLE LOG_TraceTimingRequests ADD UserSID NVARCHAR(200) collate Cyrillic_General_CS_AS NULL
go

UPDATE LOG_TraceTimingRequests
SET UserSID = s.[Sid]
FROM LOG_TraceTimingRequests ttr
JOIN LOG_SidIdentification s ON s.id = ttr.refUser
go

drop index AutoIndex_LOG_TraceTimingRequests_refUser on LOG_TraceTimingRequests
go

drop index NDX_refUser_DateTimeStart on LOG_TraceTimingRequests
alter table LOG_TraceTimingRequests drop constraint TableRef_LOG_TraceTimingRequests_refUser

ALTER TABLE LOG_TraceTimingRequests

ALTER COLUMN UserSID NVARCHAR(200) collate Cyrillic_General_CS_AS NOT NULL

ALTER TABLE LOG_TraceTimingRequests DROP COLUMN refUser
GO

ALTER TABLE RVS_SavedProperties ADD UserSID NVARCHAR(200) collate Cyrillic_General_CS_AS NULL
go

UPDATE RVS_SavedProperties
SET UserSID = s.[Sid]
FROM RVS_SavedProperties ttr
JOIN LOG_SidIdentification s ON s.id = ttr.refSid
go

ALTER TABLE RVS_SavedProperties

ALTER COLUMN UserSID NVARCHAR(200) collate Cyrillic_General_CS_AS NOT NULL

drop index AutoIndex_RVS_SavedProperties_refSid on RVS_SavedProperties
drop index NDX_JournalTypeName_dateTime on RVS_SavedProperties

ALTER TABLE RVS_SavedProperties DROP COLUMN refSid
GO

ALTER TABLE SYS_FilterValues ADD UserSID NVARCHAR(200) collate Cyrillic_General_CS_AS NULL
GO

ALTER TABLE SYS_FileUploads

ALTER COLUMN PersonSID NVARCHAR(200) collate Cyrillic_General_CS_AS NOT NULL
GO

ALTER TABLE SYS_FilterValues ADD BinarySid VARBINARY(64) NULL
go

UPDATE SYS_FilterValues
SET BinarySid = [sid]
go

drop index Index_SidKey on SYS_FilterValues

ALTER TABLE SYS_FilterValues

DROP COLUMN [sid]
GO





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

create index NDX_UserSID_DateTimeStart on LOG_TraceTimingRequests (UserSID, DateTimeStart) include([TimeOfDestinationUser])

CREATE NONCLUSTERED INDEX [NDX_JournalTypeName_dateTime] ON [dbo].[RVS_SavedProperties]
(
	[JournalTypeName] ASC,
	[dateTime] DESC
)
INCLUDE ( 	[id],
	[nameKz],
	[nameRu],
	[refProperties],
	UserSID,
	[isSharedView])
GO



if exists (select 1
          from sysobjects
          where  id = object_id('P_LOG_InsertTraceTimingRequest')
          and type in ('P','PC'))
   drop procedure P_LOG_InsertTraceTimingRequest
go


create procedure P_LOG_InsertTraceTimingRequest
   @Page                 nvarchar(255),
   @Url                  nvarchar(MAX),
   @DateTimeStart        datetime     ,
   @TimeOfCreatingPage   bigint       ,
   @UserSID              nvarchar(200),
   @refRegion            bigint       ,
   @TableName            nvarchar(255),
   @SelectMode           nvarchar(255),
   @PageType             nvarchar(255),
   @Parameters           xml,
   @Trace                xml          ,
   @TraceKey             uniqueidentifier,
   @id bigint output
as
begin
    insert into LOG_TraceTimingRequests (Page, Url, DateTimeStart, TimeOfCreatingPage, UserSID, refRegion, TableName, SelectMode, PageType, Parameters, Trace, TraceKey)
    values (@Page, @Url, @DateTimeStart, @TimeOfCreatingPage, @UserSID, @refRegion, @TableName, @SelectMode, @PageType, @Parameters, @Trace, @TraceKey)
    set @id = Scope_Identity()
end
go
