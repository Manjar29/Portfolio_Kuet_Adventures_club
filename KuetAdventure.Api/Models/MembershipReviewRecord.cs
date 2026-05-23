namespace KuetAdventure.Api.Models;

public class MembershipReviewRecord
{
    public string Status { get; set; } = "Pending";
    public string? ClubMemberId { get; set; }
    public DateTime? ReviewedAtUtc { get; set; }
}