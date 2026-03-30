namespace FlexTradem.Api.Models;

public sealed class Listing
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public DateTime CreatedUtc { get; set; }
}
