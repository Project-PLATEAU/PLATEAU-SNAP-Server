namespace PLATEAU.Snap.Models;

public class SnapServerException : Exception
{
    public SnapServerException()
    {
    }

    public SnapServerException(string message) : base(message)
    {
    }

    public SnapServerException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
