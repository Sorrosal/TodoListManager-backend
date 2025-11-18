// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using FluentValidation;
using MediatR;
using TodoListManager.Domain.Common;

namespace TodoListManager.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior that validates requests using FluentValidation.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            var errors = string.Join("; ", failures.Select(e => e.ErrorMessage));
            
            // Use reflection to create the appropriate Result.Failure
            var resultType = typeof(TResponse);
            
            if (resultType.IsGenericType)
            {
                // Result<T>
                var valueType = resultType.GetGenericArguments()[0];
                var method = typeof(Result).GetMethod(nameof(Result.Failure))!
                    .MakeGenericMethod(valueType);
                return (TResponse)method.Invoke(null, new object[] { errors })!;
            }
            else
            {
                // Result
                return (TResponse)(object)Result.Failure(errors);
            }
        }

        return await next();
    }
}
