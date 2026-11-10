namespace MyGameCollection.Application.Common;

public sealed class ValidationException(string message) : Exception(message);
