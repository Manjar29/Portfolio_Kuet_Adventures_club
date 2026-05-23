namespace KuetAdventure.Api.Models;

public class MembershipAdminDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string MemberType { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string RollId { get; set; } = string.Empty;
    public string Batch { get; set; } = string.Empty;
    public string Mailbox { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool HasPassport { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime SubmittedAtUtc { get; set; }
    public string ReviewStatus { get; set; } = "Pending";
    public string? ClubMemberId { get; set; }
    public DateTime? ReviewedAtUtc { get; set; }
}