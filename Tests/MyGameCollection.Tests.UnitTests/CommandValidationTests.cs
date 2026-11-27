using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MyGameCollection.Application.Abstractions;
using MyGameCollection.Application.Abstractions.Commands.AcquireGame;
using MyGameCollection.Application.Abstractions.Commands.AddConsole;
using MyGameCollection.Application.Abstractions.Commands.AddGame;
using MyGameCollection.Application.Abstractions.Commands.UpdateGame;
using MyGameCollection.Application.Business;
using MyGameCollection.Application.Validation.Validators.AcquireGame;
using MyGameCollection.Application.Validation.Validators.AddConsole;
using MyGameCollection.Application.Validation.Validators.AddGame;
using MyGameCollection.Application.Validation.Validators.UpdateGame;
using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Tests.UnitTests;

public class CommandValidationTests
{
    public static TheoryData<Type> CommandTypes() =>
        new(typeof(ICommand).Assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(ICommand).IsAssignableFrom(t)));

    [Theory]
    [MemberData(nameof(CommandTypes))]
    public void Every_command_has_an_input_validator(Type commandType)
    {
        using var services = new ServiceCollection().AddGameCollectionApplicationServices().BuildServiceProvider();

        Assert.NotNull(services.GetService(typeof(IValidator<>).MakeGenericType(commandType)));
    }

    [Fact]
    public void A_valid_game_passes()
    {
        var result = new AddGameCommandInputValidator().Validate(ValidGame());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Every_invalid_detail_of_a_game_is_reported_once()
    {
        var result = new AddGameCommandInputValidator().Validate(ValidGame() with
        {
            Title = " ",
            Platform = new string('x', CollectionItemLimits.MaxShortTextLength + 1),
            Region = (Region)42,
            ReleaseYear = 1900,
            PurchasePrice = -1m,
            ThumbnailUrl = "ftp://example.com/cover.png"
        });

        Assert.Equal(
            ["Title", "Platform", "Region", "ReleaseYear", "PurchasePrice", "ThumbnailUrl"],
            result.Errors.Select(e => e.PropertyName));
    }

    [Fact]
    public void Text_is_measured_after_trimming_like_the_domain_does()
    {
        var result = new AddGameCommandInputValidator().Validate(ValidGame() with
        {
            Title = new string('x', CollectionItemLimits.MaxNameLength) + "   "
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Empty_optional_details_pass()
    {
        var result = new AddConsoleCommandInputValidator().Validate(new AddConsoleCommand
        {
            Name = "Mega Drive", Manufacturer = "Sega", Model = null, Notes = "", ThumbnailUrl = "  "
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Updating_needs_an_item_key()
    {
        var result = new UpdateGameCommandInputValidator().Validate(new UpdateGameCommand
        {
            Key = Guid.Empty, Title = "Sonic the Hedgehog", Platform = "Mega Drive"
        });

        Assert.Equal("Key", Assert.Single(result.Errors).PropertyName);
    }

    [Fact]
    public void Acquiring_needs_a_purchase_date()
    {
        var result = new AcquireGameCommandInputValidator().Validate(new AcquireGameCommand
        {
            Key = Guid.NewGuid(), PurchaseDate = default
        });

        Assert.Equal("Enter the purchase date.", Assert.Single(result.Errors).ErrorMessage);
    }

    private static AddGameCommand ValidGame() => new()
    {
        Title = "Sonic the Hedgehog",
        Platform = "Mega Drive",
        ReleaseYear = 1991,
        PurchasePrice = 15m,
        ThumbnailUrl = "https://example.com/sonic.png"
    };
}
