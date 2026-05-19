using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace KuetAdventure.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private static readonly string ArchiveFilePath = Path.Combine(AppContext.BaseDirectory, "archived-events.json");

    [HttpGet("archived")]
    public async Task<ActionResult<IEnumerable<string>>> GetArchivedEvents()
    {
        var events = await ReadArchivedEventsAsync();
        return Ok(events);
    }

    [HttpPost("archived/{eventName}")]
    public async Task<IActionResult> ArchiveEvent(string eventName)
    {
        var events = await ReadArchivedEventsAsync();
        if (!events.Contains(eventName, StringComparer.OrdinalIgnoreCase))
        {
            events.Add(eventName);
            await WriteArchivedEventsAsync(events);
        }

        return NoContent();
    }

    [HttpDelete("archived/{eventName}")]
    public async Task<IActionResult> UnarchiveEvent(string eventName)
    {
        var events = await ReadArchivedEventsAsync();
        var updated = events
            .Where(name => !string.Equals(name, eventName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (updated.Count != events.Count)
        {
            await WriteArchivedEventsAsync(updated);
        }

        return NoContent();
    }

    [HttpDelete("archived")]
    public async Task<IActionResult> ClearArchivedEvents()
    {
        await WriteArchivedEventsAsync([]);
        return NoContent();
    }

    private static async Task<List<string>> ReadArchivedEventsAsync()
    {
        if (!System.IO.File.Exists(ArchiveFilePath))
        {
            return [];
        }

        await using var stream = System.IO.File.OpenRead(ArchiveFilePath);
        var events = await JsonSerializer.DeserializeAsync<List<string>>(stream);
        return events ?? [];
    }

    private static async Task WriteArchivedEventsAsync(List<string> events)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(ArchiveFilePath)!);
        await using var stream = System.IO.File.Create(ArchiveFilePath);
        await JsonSerializer.SerializeAsync(stream, events);
    }
}