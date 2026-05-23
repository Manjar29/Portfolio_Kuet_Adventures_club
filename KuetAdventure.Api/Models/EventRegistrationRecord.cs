namespace KuetAdventure.Api.Models;

public class EventRegistrationRecord
{
    public int Id { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string ClubId { get; set; } = string.Empty;
    public string Roll { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public DateTime SubmittedAtUtc { get; set; } = DateTime.UtcNow;
}