namespace MyGameCollection.Infrastructure.EFC.DataModel;

public class Game : CollectionItem
{
    public required string Title { get; set; }
    public required string Platform { get; set; }
}
