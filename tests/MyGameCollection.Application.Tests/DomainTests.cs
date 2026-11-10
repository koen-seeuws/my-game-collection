using MyGameCollection.Domain.Entities;

namespace MyGameCollection.Application.Tests;

public class DomainTests
{
    [Fact]
    public void FirstPhotoBecomesPrimary_AndPrimaryMovesWhenRemoved()
    {
        var game = new Game { Title = "Earthbound" };
        var first = game.AddPhoto("a.jpg", "a_t.jpg", "image/jpeg", null, 10);
        var second = game.AddPhoto("b.jpg", "b_t.jpg", "image/jpeg", null, 10);

        Assert.True(first.IsPrimary);
        Assert.False(second.IsPrimary);

        game.SetPrimaryPhoto(second.Id);
        Assert.False(first.IsPrimary);
        Assert.True(second.IsPrimary);

        game.RemovePhoto(second.Id);
        Assert.True(first.IsPrimary);
    }

    [Fact]
    public void IsComplete_WhenBoxAndManualOrSealed()
    {
        Assert.False(new Game { Title = "x", HasBox = true }.IsComplete);
        Assert.True(new Game { Title = "x", HasBox = true, HasManual = true }.IsComplete);
        Assert.True(new Game { Title = "x", IsSealed = true }.IsComplete);
    }

    [Fact]
    public void TitleIsRequired() =>
        Assert.Throws<ArgumentException>(() => new Game { Title = "  " });

    [Fact]
    public void RestoredEntitiesKeepTheirIdentity()
    {
        var id = Guid.NewGuid();
        var platform = new Platform(id, " SNES ", "Nintendo", 1990, ["Super Famicom", "super famicom", " "]);
        Assert.Equal((id, "SNES"), (platform.Id, platform.Name));
        Assert.Equal(["Super Famicom"], platform.Aliases);

        var photo = ItemPhoto.Restore(Guid.NewGuid(), id, "f", "t", "image/jpeg", null, 1, sortOrder: 3, isPrimary: true);
        var game = new Game(id) { Title = "x" };
        game.RestorePhotos([photo]);
        Assert.Same(photo, Assert.Single(game.Photos));
    }
}
