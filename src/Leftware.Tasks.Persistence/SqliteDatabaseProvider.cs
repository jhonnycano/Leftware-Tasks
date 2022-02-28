using Dapper;
using Leftware.Injection.Attributes;
using Leftware.Tasks.Persistence.Resources;
using Microsoft.Data.Sqlite;

namespace Leftware.Tasks.Persistence;

[Service]
public class SqliteDatabaseProvider
{
    private const string CONNECTION_TEMPLATE = "Data Source = {0}";
    private const string DATABASE_FILE = "leftwareTasks.db";
    internal string FilePath { get; }

    public SqliteDatabaseProvider()
    {
        var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        FilePath = Path.Combine(folder, DATABASE_FILE);
    }

    /// <summary>
    /// Gets a list of strings given a SQL Query
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public IList<string> GetList(string sql, object? parameters = null)
    {
        var result = new List<string>();
        using var cn = GetOpenConnection();
        var rdr = cn.ExecuteReader(sql, parameters);
        while (rdr.Read())
        {
            result.Add(rdr.GetString(0));
        }

        return result;
    }

    /// <summary>
    /// Gets a list of objects given a SQL Query
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public IList<T> GetObjects<T>(string sql, object? parameters = null)
    {
        using var cn = GetOpenConnection();
        var enumerable = cn.Query<T>(sql, parameters);
        var list = enumerable.ToList();
        return list;
    }

    public T? GetObject<T>(string sql, object? parameters = null)
    {
        using var cn = GetOpenConnection();
        var enumerable = cn.Query<T>(sql, parameters);
        var item = enumerable.FirstOrDefault();
        return item;
    }

    /// <summary>
    /// Executes a SQL Query
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="parameters"></param>
    public void Execute(string sql, object? parameters = null)
    {
        using var cn = GetOpenConnection();
        using var trx = cn.BeginTransaction();
        cn.Execute(sql, parameters);
        trx.Commit();
    }

    public SqliteConnection GetOpenConnection()
    {
        var connection = new SqliteConnection(string.Format(CONNECTION_TEMPLATE, FilePath));
        connection.Open();
        connection.EnableExtensions(true);
        //connection.LoadExtension("SQLite.Interop.dll", "sqlite3_json_init");
        return connection;
    }

    public void Validate()
    {
        if (File.Exists(FilePath))
        {
            if (CheckDatabase()) return;
            File.Delete(FilePath);
            Thread.Sleep(500);
        }

        var sql = FileResources.Db_Create;
        Execute(sql, null);
    }

    public bool CheckDatabase()
    {
        var sql = "SELECT name FROM sqlite_master WHERE type='table' AND name='col_header';";
        var list = GetList(sql);
        return list.Count > 0;
    }


}
