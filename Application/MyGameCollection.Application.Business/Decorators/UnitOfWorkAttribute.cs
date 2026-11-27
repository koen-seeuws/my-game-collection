using MyGameCollection.Application.Abstractions.Persistence;

namespace MyGameCollection.Application.Business.Decorators;

/// <summary>
/// Runs the command handler in a transaction of another unit of work than the game collections'
/// <see cref="IUnitOfWork"/>, e.g. <see cref="IAccountsUnitOfWork"/> for the account store.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class UnitOfWorkAttribute : Attribute
{
    public UnitOfWorkAttribute(Type unitOfWorkType)
    {
        if (!typeof(IUnitOfWork).IsAssignableFrom(unitOfWorkType))
            throw new ArgumentException($"{unitOfWorkType} is not an {nameof(IUnitOfWork)}.", nameof(unitOfWorkType));

        UnitOfWorkType = unitOfWorkType;
    }

    public Type UnitOfWorkType { get; }
}
