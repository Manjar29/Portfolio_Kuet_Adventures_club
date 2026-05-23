namespace KuetAdventure.Api.Models;

public class EventRegistrationSummaryDto
{
    public string EventName { get; set; } = string.Empty;
    public int Count { get; set; }
    public int DeletedCount { get; set; }
}