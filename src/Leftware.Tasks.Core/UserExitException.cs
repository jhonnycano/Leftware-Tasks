using System.Runtime.Serialization;

namespace Leftware.Tasks.Core;

[Serializable]
public class UserExitException : Exception
{
    public UserExitException()
    {
    }

    public UserExitException(string message) : base(message)
    {
    }

    public UserExitException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected UserExitException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
