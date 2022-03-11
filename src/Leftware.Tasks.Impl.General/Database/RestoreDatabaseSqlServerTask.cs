using Leftware.Common;
using Leftware.Tasks.Core;
using Leftware.Tasks.Core.Model;
using Leftware.Tasks.Core.TaskParameters;

namespace Leftware.Tasks.Impl.General.Database;

[Descriptor("Database - Restore Sql Server database")]
internal class RestoreDatabaseSqlServerTask : CommonTaskBase
{
    private const string SERVER = "server";
    private const string BACKUP_NAME = "backupName";
    private const string WITH_MOVE = "withMove";
    //private readonly IDbManagerFactory _dbManagerFactory;

    public RestoreDatabaseSqlServerTask(
        //IDbManagerFactory dbManagerFactory
        )
    {
        //_dbManagerFactory = dbManagerFactory;
    }

    public override IList<TaskParameter> GetTaskParameterDefinition()
    {
        return new List<TaskParameter>
        {
            new SelectFromCollectionTaskParameter(SERVER, "Select Sql Connection", Defs.Collections.CN_MSSQL),
            new SelectFromCollectionTaskParameter(BACKUP_NAME, "Backup name", Defs.Collections.DB_MSSQL, true),
            new ReadBoolTaskParameter(WITH_MOVE, "With move"),
        };
    }

    public override async Task Execute(IDictionary<string, object> input)
    {
        var serverAlias = input.Get<string>(SERVER);
        var backupName = input.Get<string>(BACKUP_NAME);
        var withMove = input.Get<bool>(WITH_MOVE);

        var connectionInfo = Context.CollectionProvider.GetItemContentAs<DatabaseConnectionInfo>(Defs.Collections.CN_MSSQL, serverAlias);
        var serverRestorePath = connectionInfo.RestoreSource;
        if (string.IsNullOrEmpty(serverRestorePath))
        {
            Console.WriteLine("Restore path setting not found for server alias {0}", serverAlias);
            return;
        }

        var backupFile = backupName;
        if (!backupFile.EndsWith(".bak")) backupFile += ".bak";
        var backupPath = Path.Combine(serverRestorePath, backupFile);

        var restoreMode = withMove
            ? SqlServerRestoreMode.MoveFiles
            : SqlServerRestoreMode.Replace;

        var folderForDataFile = withMove
            ? connectionInfo.TargetPath
            : "";
        /*
        var sqlServerRestoreInput = new SqlServerRestoreParameters
        {
            Connection = new SqlConnectionParameters
            {
                Instance = server,
                Database = defaultDb,
                UserId = user,
                Password = password
            },
            BackupPath = backupPath,
            TargetDatabase = backupName,
            Mode = restoreMode,
            FolderForDataFile = folderForDataFile
        };
        */

        /*
        var dbManager = dbManagerFactory.Get(dbManagerType.SqlServer);
        var result = dbManager.Execute(sqlServerRestoreInput);

        if (!result.IsSuccess)
        {
            var error = Context.Logger.Errors.Last();
            _console.WriteLine("Error restoring database: ");
            _console.WriteLine(error.Text);
        }
        
         */
    }
}
