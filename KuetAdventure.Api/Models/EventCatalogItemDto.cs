namespace KuetAdventure.Api.Models;

public class EventCatalogItemDto
{
    public int Id { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Overview { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public DateTime? EventDateUtc { get; set; }
    public DateTime? DeadlineUtc { get; set; }
    public bool IsArchived { get; set; }
    public bool IsExpired { get; set; }
    public string? ImageSrc { get; set; }
    public string? ImageAlt { get; set; }
    public List<string> Schedule { get; set; } = [];
    public List<string> Requirements { get; set; } = [];
    public List<string> Payment { get; set; } = [];
}
