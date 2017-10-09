using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using Nat.Tools.Classes;
using Nat.Tools.Data.DataContext;
using Nat.Tools.Specific;
using Nat.Web.Controls.GenerationClasses.Data;
using Nat.Web.Tools.Initialization;
using Nat.Web.Tools.Security;

namespace Nat.Web.Controls.Data
{
    public static class DataContextExtender
    {
        public static FileData GetSysFileUpload(long id, string subsystem)
        {
            WebInitializer.Initialize();
            using (var db = new DBUploadFilesDataContext(SpecificInstances.DbFactory.CreateConnection()))
            {
                return CacheQueries.ExecuteFunction<SYS_FileUpload, Triplet<long, string, string>, FileData>(
                    db, new Triplet<long, string, string>(id, User.GetSID(), subsystem),
                    (r, value) =>
                    r.id == value.First && r.PersonSID == value.Second && r.SubSystemName == value.Third,
                    r => new FileData
                             {
                                 FileName = r.dataFileName,
                                 Binary = r.data,
                             });
            }
        }
    }
}