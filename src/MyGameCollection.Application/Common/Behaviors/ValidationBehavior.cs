using System.ComponentModel.DataAnnotations;
using MediatR;
using ValidationException = MyGameCollection.Application.Common.ValidationException;

namespace MyGameCollection.Application.Common.Behaviors;

/// <summary>
/// Validates DataAnnotations on every request, and on input models it carries (e.g. ItemEditModel),
/// so the rules hold no matter which UI or endpoint sends the request.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var errors = new List<ValidationResult>();
        Validate(request, errors);

        foreach (var property in typeof(TRequest).GetProperties())
        {
            var type = property.PropertyType;
            if (type.IsClass && type != typeof(string) && IsOwnContract(type) && property.GetValue(request) is { } model)
            {
                Validate(model, errors);
            }
        }

        if (errors.Count > 0)
            throw new ValidationException(string.Join(" ", errors.Select(e => e.ErrorMessage).Distinct()));

        return next(cancellationToken);
    }

    /// <summary>Input models from our own contracts (e.g. ItemEditModel); skips streams, collections, framework types.</summary>
    private static bool IsOwnContract(Type type) =>
        type.Namespace?.StartsWith("MyGameCollection.Application.Abstractions", StringComparison.Ordinal) == true;

    private static void Validate(object instance, List<ValidationResult> errors) =>
        Validator.TryValidateObject(instance, new ValidationContext(instance), errors, validateAllProperties: true);
}
