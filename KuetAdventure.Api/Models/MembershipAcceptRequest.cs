using System.ComponentModel.DataAnnotations;

namespace KuetAdventure.Api.Models;

public class MembershipAcceptRequest
{
    [StringLength(32)]
    public string? ClubMemberId { get; set; }
}