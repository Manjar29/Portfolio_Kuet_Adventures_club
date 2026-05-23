using System.ComponentModel.DataAnnotations;

namespace KuetAdventure.Api.Models;

public class EventRegistrationCreateRequest
{
    [Required]
    public string EventName { get; set; } = string.Empty;

    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    public string Department { get; set; } = string.Empty;

    [Required]
    public string ClubId { get; set; } = string.Empty;

    [Required]
    public string Roll { get; set; } = string.Empty;

    [Required]
    public string TransactionId { get; set; } = string.Empty;
}