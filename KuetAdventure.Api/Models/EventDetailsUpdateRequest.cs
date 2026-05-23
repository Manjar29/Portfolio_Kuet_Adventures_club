using System.ComponentModel.DataAnnotations;

namespace KuetAdventure.Api.Models;

public class EventDetailsUpdateRequest
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Subtitle { get; set; } = string.Empty;

    [Required]
    public string Overview { get; set; } = string.Empty;

    public string? ImageSrc { get; set; }

    public string? ImageAlt { get; set; }

    public List<string> Schedule { get; set; } = [];

    public List<string> Requirements { get; set; } = [];

    public List<string> Payment { get; set; } = [];
}