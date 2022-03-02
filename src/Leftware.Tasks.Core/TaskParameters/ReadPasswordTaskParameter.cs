namespace Leftware.Tasks.Core.TaskParameters;

public class ReadPasswordTaskParameter : TaskParameter<string>
{
    public ReadPasswordTaskParameter(string name, string label, int minLength, int maxLength, string cancelString = "?") : base(name, label)
    {
        Type = TaskParameterType.ReadPassword;
        CharMask = '*';
        MinLength = minLength;
        MaxLength = maxLength;
        CancelString = cancelString;
    }

    public int MinLength { get; set; }
    public int MaxLength { get; set; }
    public string CancelString { get; set; }
    public char CharMask { get; internal set; }
}
