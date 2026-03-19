namespace PLATEAU.Snap.Models.Exceptions;

public class LambdaOperationException : Exception
{
    public LambdaOperationException(string message) : base(message)
    {
    }

    public LambdaOperationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
