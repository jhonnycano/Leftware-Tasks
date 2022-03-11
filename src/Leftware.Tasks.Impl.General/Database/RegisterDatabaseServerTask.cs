using Leftware.Common;
using Leftware.Tasks.Core;
using Leftware.Tasks.Core.Model;
using Leftware.Tasks.Core.TaskParameters;
using Leftware.Tasks.Core.TaskParameters.Conditions;
using Newtonsoft.Json;

namespace Leftware.Tasks.Impl.General.Database;

[Descriptor("Database - Register database server")]
internal class RegisterDatabaseServerTask : CommonTaskBase
{
    private const string TEMPLATE_SQL_CONN_INFO = "{{Server}};{{Database}};{{User}};{{Password}}";
    private const string TEMPLATE_ORACLE_CONN_INFO = "{{Server}};{{User}};{{Password}}";
    private const string TEMPLATE_POSTGRES_CONN_INFO = "{{Server}};{{Port}};{{Database}};{{User}};{{Password}}";
    private const string TEMPLATE_MYSQL_CONN_INFO = "{{Server}};{{Port}};{{Database}};{{User}};{{Password}}";
    private const string TEMPLATE_SQLITE_CONN_INFO = "{{Database}};";
    private const string KEY = "key";
    private const string LABEL = "label";
    private const string SERVER_TYPE = "serverType";
    private const string SERVER_IDENTIFIER = "server";
    private const string DATABASE = "database";
    private const string PORT = "port";
    private const string USER = "user";
    private const string PASSWORD = "password";
    private const string BACKUP_SOURCE = "serverBackupSource";
    private const string RESTORE_SOURCE = "serverRestoreSource";
    private const string SERVER_TARGET_PATH = "serverTargetPath";

    public override IList<TaskParameter> GetTaskParameterDefinition()
    {
        return new List<TaskParameter>
        {
            new ReadStringTaskParameter(KEY, "Server key"),
            new ReadStringTaskParameter(LABEL, "Server label (for user selection)"),
            new SelectEnumTaskParameter(SERVER_TYPE, "Database server type", typeof(DatabaseEngine)),
            // Sql Server
            new SelectGroupTaskParameter(
                new ReadStringTaskParameter(SERVER_IDENTIFIER, "Server identifier (\"server\" or \"server\\instance\""),
                new ReadStringTaskParameter(DATABASE, "Database"),
                new ReadStringTaskParameter(USER, "User"),
                new ReadPasswordTaskParameter(PASSWORD, "Password", 3, 120),
                new ReadFolderTaskParameter(BACKUP_SOURCE, "Folder for storing generated backups"),
                new ReadFolderTaskParameter(RESTORE_SOURCE, "Folder for reading backup files to restore"),
                new ReadFolderTaskParameter(SERVER_TARGET_PATH, "Folder for placing mdf files")
                ).When(new EqualsCondition(SERVER_TYPE, DatabaseEngine.SqlServer)),
            // Oracle
            new SelectGroupTaskParameter(
                new ReadStringTaskParameter(SERVER_IDENTIFIER, "Server identifier (\"server:port\\service\")"),
                new ReadStringTaskParameter(USER, "User"),
                new ReadPasswordTaskParameter(PASSWORD, "Password", 3, 120),
                new ReadFolderTaskParameter(BACKUP_SOURCE, "Folder for storing generated backups"),
                new ReadFolderTaskParameter(RESTORE_SOURCE, "Folder for reading backup files to restore")
                ).When(new EqualsCondition(SERVER_TYPE, DatabaseEngine.Oracle)),
            // Postgres
            new SelectGroupTaskParameter(
                new ReadStringTaskParameter(SERVER_IDENTIFIER, "Server identifier (\"server\")"),
                new ReadIntegerTaskParameter(PORT, "Port").WithDefaultValue(5432),
                new ReadStringTaskParameter(DATABASE, "Database"),
                new ReadStringTaskParameter(USER, "User"),
                new ReadPasswordTaskParameter(PASSWORD, "Password", 3, 120),
                new ReadFolderTaskParameter(BACKUP_SOURCE, "Folder for storing generated backups"),
                new ReadFolderTaskParameter(RESTORE_SOURCE, "Folder for reading backup files to restore")
                ).When(new EqualsCondition(SERVER_TYPE, DatabaseEngine.Postgres)),
            // Sqlite
            new SelectGroupTaskParameter(
                new ReadStringTaskParameter(DATABASE, "Database")
                ).When(new EqualsCondition(SERVER_TYPE, DatabaseEngine.Sqlite)),
        };
    }

    public override async Task Execute(IDictionary<string, object> input)
    {
        var key = input.Get(KEY, "");
        var label = input.Get(LABEL, "");
        var serverType = input.Get(SERVER_TYPE, DatabaseEngine.None);
        var server = input.Get(SERVER_IDENTIFIER, "");
        var port = input.Get(PORT, "");
        var database = input.Get(DATABASE, "");
        var user = input.Get(USER, "");
        var password = input.Get(PASSWORD, "");
        var serverBackupSource = input.Get(BACKUP_SOURCE, default(string));
        var serverRestoreSource = input.Get(RESTORE_SOURCE, default(string));
        var serverTargetPath = input.Get(SERVER_TARGET_PATH, default(string));

        var template = serverType switch
        {
            DatabaseEngine.Oracle => TEMPLATE_ORACLE_CONN_INFO,
            DatabaseEngine.SqlServer => TEMPLATE_SQL_CONN_INFO,
            DatabaseEngine.Postgres => TEMPLATE_POSTGRES_CONN_INFO,
            DatabaseEngine.Sqlite => TEMPLATE_SQLITE_CONN_INFO,
            DatabaseEngine.MySql => TEMPLATE_MYSQL_CONN_INFO,
            _ => throw new NotImplementedException()
        };
        var source = new { Server = server, Port = port, Database = database, User = user, Password = password };
        var cn = template.FormatLiquid(source);

        var colConnection = serverType switch
        {
            DatabaseEngine.Oracle => Defs.Collections.CN_ORACLE,
            DatabaseEngine.SqlServer => Defs.Collections.CN_MSSQL,
            DatabaseEngine.Postgres => Defs.Collections.CN_POSTGRES,
            DatabaseEngine.Sqlite => Defs.Collections.CN_SQLITE,
            DatabaseEngine.MySql => Defs.Collections.CN_MYSQL,
            _ => throw new NotImplementedException()
        };

        var info = new DatabaseConnectionInfo
        {
            ConnectionString = cn,
            BackupSource = serverBackupSource,
            RestoreSource = serverRestoreSource,
            TargetPath = serverTargetPath,
        };
        var json = JsonConvert.SerializeObject(info);
        await Context.CollectionProvider.AddItemAsync(colConnection, key, label, json);
    }
}
