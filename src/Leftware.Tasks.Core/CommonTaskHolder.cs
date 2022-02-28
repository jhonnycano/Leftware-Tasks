namespace Leftware.Tasks.Core;

public record CommonTaskHolder
(
    string Key,
    string Name,
    int SortOrder,
    bool ConfirmBeforeExecution,
    Type TaskType
);
