namespace Leftware.Tasks.Core.Model
{
    public class DatabaseConnectionInfo
    {
        public string ConnectionString { get; set; }
        public string? BackupSource { get; set; }
        public string? RestoreSource { get; set; }
        public string? TargetPath { get; set; }
    }
}
