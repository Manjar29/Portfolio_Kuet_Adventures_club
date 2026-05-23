namespace KuetAdventure.Api.Models;

public class EventDetailsDto
{
    public string EventName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Overview { get; set; } = string.Empty;
    public string? ImageSrc { get; set; }
    public string? ImageAlt { get; set; }
    public List<string> Schedule { get; set; } = [];
    public List<string> Requirements { get; set; } = [];
    public List<string> Payment { get; set; } = [];
}