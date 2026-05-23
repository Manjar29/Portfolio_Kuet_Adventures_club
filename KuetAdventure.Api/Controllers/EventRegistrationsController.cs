using System.Text.Json;
using KuetAdventure.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace KuetAdventure.Api.Controllers;

[ApiController]
[Route("api/event-registrations")]
public class EventRegistrationsController : ControllerBase
{
    private static readonly string StorageFilePath = Path.Combine(AppContext.BaseDirectory, "event-registrations.json");
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true, WriteIndented = true };

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] EventRegistrationCreateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var records = await ReadRecordsAsync();
        var nextId = records.Count == 0 ? 1 : records.Max(item => item.Id) + 1;

        records.Add(new EventRegistrationRecord
        {
            Id = nextId,
            EventName = request.EventName.Trim(),
            FullName = request.FullName.Trim(),
            Department = request.Department.Trim(),
            ClubId = request.ClubId.Trim(),
            Roll = request.Roll.Trim(),
            TransactionId = request.TransactionId.Trim(),
            SubmittedAtUtc = DateTime.UtcNow
        });

        await WriteRecordsAsync(records);
        return CreatedAtAction(nameof(GetById), new { id = nextId }, new { id = nextId });
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EventRegistrationAdminDto>>> GetAll([FromQuery] string? eventName = null)
    {
        var records = await ReadRecordsAsync();
        var filtered = FilterRecords(records, eventName);
        return Ok(filtered.Select(ToAdminDto));
    }

    [HttpGet("summary")]
    public async Task<ActionResult<IEnumerable<EventRegistrationSummaryDto>>> GetSummary()
    {
        var records = await ReadRecordsAsync();
        var deletedRecords = await ReadDeletedRecordsAsync();

        var activeByEvent = records
            .GroupBy(record => record.EventName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

        var deletedByEvent = deletedRecords
            .GroupBy(record => record.EventName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

        var allEventNames = new HashSet<string>(activeByEvent.Keys, StringComparer.OrdinalIgnoreCase);
        foreach (var name in deletedByEvent.Keys) allEventNames.Add(name);

        var summary = allEventNames
            .Select(name => new EventRegistrationSummaryDto
            {
                EventName = name,
                Count = activeByEvent.ContainsKey(name) ? activeByEvent[name] : 0,
                DeletedCount = deletedByEvent.ContainsKey(name) ? deletedByEvent[name] : 0
            })
            .OrderBy(item => item.EventName)
            .ToList();

        return Ok(summary);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EventRegistrationAdminDto>> GetById(int id)
    {
        var records = await ReadRecordsAsync();
        var record = records.FirstOrDefault(item => item.Id == id);
        if (record is null)
        {
            return NotFound();
        }

        return Ok(ToAdminDto(record));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var records = await ReadRecordsAsync();
        var idx = records.FindIndex(item => item.Id == id);
        if (idx < 0)
        {
            return NoContent();
        }

        var removedRecord = records[idx];
        records.RemoveAt(idx);

        // persist remaining active records
        await WriteRecordsAsync(records);

        // append removed record to deleted storage for audit
        var deleted = await ReadDeletedRecordsAsync();
        deleted.Add(removedRecord);
        await WriteDeletedRecordsAsync(deleted);

        return NoContent();
    }

    private static IEnumerable<EventRegistrationRecord> FilterRecords(List<EventRegistrationRecord> records, string? eventName)
    {
        if (string.IsNullOrWhiteSpace(eventName))
        {
            return records.OrderByDescending(item => item.SubmittedAtUtc);
        }

        return records
            .Where(item => string.Equals(item.EventName, eventName, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(item => item.SubmittedAtUtc);
    }

    private static EventRegistrationAdminDto ToAdminDto(EventRegistrationRecord record)
    {
        return new EventRegistrationAdminDto
        {
            Id = record.Id,
            EventName = record.EventName,
            FullName = record.FullName,
            Department = record.Department,
            ClubId = record.ClubId,
            Roll = record.Roll,
            TransactionId = record.TransactionId,
            SubmittedAtUtc = record.SubmittedAtUtc
        };
    }

    private static async Task<List<EventRegistrationRecord>> ReadRecordsAsync()
    {
        if (!System.IO.File.Exists(StorageFilePath))
        {
            return new List<EventRegistrationRecord>();
        }

        await using var stream = System.IO.File.OpenRead(StorageFilePath);
        var data = await JsonSerializer.DeserializeAsync<List<EventRegistrationRecord>>(stream, JsonOptions);
        return data ?? new List<EventRegistrationRecord>();
    }

    private static async Task WriteRecordsAsync(List<EventRegistrationRecord> records)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(StorageFilePath)!);
        await using var stream = System.IO.File.Create(StorageFilePath);
        await JsonSerializer.SerializeAsync(stream, records, JsonOptions);
    }

    // Deleted records are kept separately so we can report deletion counts per event
    private static readonly string DeletedStorageFilePath = Path.Combine(AppContext.BaseDirectory, "event-registrations-deleted.json");

    private static async Task<List<EventRegistrationRecord>> ReadDeletedRecordsAsync()
    {
        if (!System.IO.File.Exists(DeletedStorageFilePath))
        {
            return new List<EventRegistrationRecord>();
        }

        await using var stream = System.IO.File.OpenRead(DeletedStorageFilePath);
        var data = await JsonSerializer.DeserializeAsync<List<EventRegistrationRecord>>(stream, JsonOptions);
        return data ?? new List<EventRegistrationRecord>();
    }

    private static async Task WriteDeletedRecordsAsync(List<EventRegistrationRecord> records)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(DeletedStorageFilePath)!);
        await using var stream = System.IO.File.Create(DeletedStorageFilePath);
        await JsonSerializer.SerializeAsync(stream, records, JsonOptions);
    }
}