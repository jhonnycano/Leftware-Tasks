using Leftware.Common;
using Leftware.Common.Database;
using Leftware.Tasks.Core;
using Leftware.Tasks.Core.Model;
using Leftware.Tasks.Core.TaskParameters;
using Leftware.Tasks.Core.TaskParameters.Conditions;

namespace Leftware.Tasks.Impl.General.Database;

[Descriptor("Database - Execute File Script")]
internal class ExecuteFileScriptTask : CommonTaskBase
{
    private const string SERVER_TYPE = "serverType";
    private const string SERVER = "server";
    private const string FILE = "file";
    private readonly IConnectionProviderFactory _connectionProviderFactory;

    public ExecuteFileScriptTask(IConnectionProviderFactory connectionProviderFactory)
    {
        _connectionProviderFactory = connectionProviderFactory;
    }

    public override IList<TaskParameter> GetTaskParameterDefinition()
    {
        return new List<TaskParameter>
        {
            new SelectEnumTaskParameter(SERVER_TYPE, "Database server type", typeof(DatabaseEngine)),
            new SelectFromCollectionTaskParameter(SERVER, "Oracle server", Defs.Collections.CN_ORACLE).When(new EqualsCondition(SERVER_TYPE, DatabaseEngine.Oracle)),
            new SelectFromCollectionTaskParameter(SERVER, "Sql server", Defs.Collections.CN_MSSQL).When(new EqualsCondition(SERVER_TYPE, DatabaseEngine.SqlServer)),
            new SelectFromCollectionTaskParameter(SERVER, "Postgres server", Defs.Collections.CN_POSTGRES).When(new EqualsCondition(SERVER_TYPE, DatabaseEngine.Postgres)),
            new SelectFromCollectionTaskParameter(SERVER, "Sqlite server", Defs.Collections.CN_SQLITE).When(new EqualsCondition(SERVER_TYPE, DatabaseEngine.Sqlite)),
            new SelectFromCollectionTaskParameter(SERVER, "MySql server", Defs.Collections.CN_MYSQL).When(new EqualsCondition(SERVER_TYPE, DatabaseEngine.MySql)),
            new ReadFileTaskParameter(FILE, "Script file", true),
        };
    }

    public override async Task Execute(IDictionary<string, object> input)
    {
        var serverType = input.Get("serverType", DatabaseEngine.None);
        var colConnection = serverType switch
        {
            DatabaseEngine.Oracle => Defs.Collections.CN_ORACLE,
            DatabaseEngine.SqlServer => Defs.Collections.CN_MSSQL,
            DatabaseEngine.Postgres => Defs.Collections.CN_POSTGRES,
            DatabaseEngine.Sqlite => Defs.Collections.CN_SQLITE,
            DatabaseEngine.MySql => Defs.Collections.CN_MYSQL,
            _ => throw new NotImplementedException()
        };

        var server = input.Get("server", "");
        var serverConnection = Context.CollectionProvider.GetItemContentAs<DatabaseConnectionInfo>(colConnection, server);
        var file = input.Get("file", "");

        ExecuteScriptFile(serverType, serverConnection, file);
    }

    private void ExecuteScriptFile(DatabaseEngine serverType, DatabaseConnectionInfo info, string file)
    {
        var scriptContent = File.ReadAllText(file);

        var provider = _connectionProviderFactory.GetInstance(serverType) ?? throw new InvalidOperationException($"Server type not found: {serverType}");
        using var cn = provider.GetCustomConnection(info.ConnectionString);
        var cmd = cn.CreateCommand();
        cmd.CommandText = scriptContent;
        cmd.Connection = cn;
        cmd.ExecuteNonQuery();
    }
}
