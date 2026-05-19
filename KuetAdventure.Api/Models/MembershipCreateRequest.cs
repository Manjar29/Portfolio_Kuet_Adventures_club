using System.ComponentModel.DataAnnotations;

namespace KuetAdventure.Api.Models;

public class MembershipCreateRequest
{
    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    public string MemberType { get; set; } = string.Empty;

    [Required]
    public string Department { get; set; } = string.Empty;

    [Required]
    public string RollId { get; set; } = string.Empty;

    [Required]
    public string Batch { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Mailbox { get; set; } = string.Empty;

    [Required]
    public string PhoneNumber { get; set; } = string.Empty;

    public bool HasPassport { get; set; }

    [Required]
    public string Message { get; set; } = string.Empty;
}
