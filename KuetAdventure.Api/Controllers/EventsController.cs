using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using KuetAdventure.Api.Models;

namespace KuetAdventure.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private static readonly string EventCatalogFilePath = Path.Combine(AppContext.BaseDirectory, "events-catalog.json");
    private static readonly string LegacyArchiveFilePath = Path.Combine(AppContext.BaseDirectory, "archived-events.json");
    private static readonly string LegacyEventDetailsOverridesFilePath = Path.Combine(AppContext.BaseDirectory, "event-details-overrides.json");
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true, WriteIndented = true };

    private sealed class EventCatalogRecord
    {
        public int Id { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string Overview { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public DateTime? EventDateUtc { get; set; }
        public DateTime? DeadlineUtc { get; set; }
        public bool IsArchived { get; set; }
        public string? ImageSrc { get; set; }
        public string? ImageAlt { get; set; }
        public List<string> Schedule { get; set; } = [];
        public List<string> Requirements { get; set; } = [];
        public List<string> Payment { get; set; } = [];
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EventCatalogItemDto>>> GetEvents([FromQuery] bool includeArchived = true)
    {
        var records = await ReadCatalogAsync();
        var now = DateTime.UtcNow;
        var query = includeArchived ? records : records.Where(record => !IsArchived(record, now));
        return Ok(query.OrderBy(record => record.EventDateUtc ?? DateTime.MaxValue).ThenBy(record => record.EventName).Select(record => ToCatalogDto(record, now)).ToList());
    }

    [HttpGet("catalog")]
    public async Task<ActionResult<IEnumerable<EventCatalogItemDto>>> GetCatalog([FromQuery] bool includeArchived = true)
    {
        return await GetEvents(includeArchived);
    }

    [HttpGet("catalog/{id:int}")]
    public async Task<ActionResult<EventCatalogItemDto>> GetCatalogById(int id)
    {
        var records = await ReadCatalogAsync();
        var record = records.FirstOrDefault(item => item.Id == id);
        if (record is null)
        {
            return NotFound();
        }

        var now = DateTime.UtcNow;
        return Ok(ToCatalogDto(record, now));
    }

    [HttpGet("by-name/{eventName}")]
    public async Task<ActionResult<EventCatalogItemDto>> GetByName(string eventName)
    {
        var records = await ReadCatalogAsync();
        var record = records.FirstOrDefault(item => string.Equals(item.EventName, eventName, StringComparison.OrdinalIgnoreCase));
        if (record is null)
        {
            return NotFound();
        }

        var now = DateTime.UtcNow;
        return Ok(ToCatalogDto(record, now));
    }

    [HttpPost("catalog")]
    public async Task<ActionResult<EventCatalogItemDto>> CreateCatalogEvent([FromBody] EventCatalogUpsertRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var records = await ReadCatalogAsync();
        if (records.Any(item => string.Equals(item.EventName, request.EventName, StringComparison.OrdinalIgnoreCase)))
        {
            return Conflict(new { message = "An event with the same name already exists." });
        }

        var nextId = records.Count == 0 ? 1 : records.Max(item => item.Id) + 1;
        var record = ToRecord(nextId, request);
        records.Add(record);
        await WriteCatalogAsync(records);

        return CreatedAtAction(nameof(GetCatalogById), new { id = record.Id }, ToCatalogDto(record, DateTime.UtcNow));
    }

    [HttpPut("catalog/{id:int}")]
    public async Task<ActionResult<EventCatalogItemDto>> UpdateCatalogEvent(int id, [FromBody] EventCatalogUpsertRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var records = await ReadCatalogAsync();
        var existing = records.FirstOrDefault(item => item.Id == id);
        if (existing is null)
        {
            return NotFound();
        }

        if (records.Any(item => item.Id != id && string.Equals(item.EventName, request.EventName, StringComparison.OrdinalIgnoreCase)))
        {
            return Conflict(new { message = "An event with the same name already exists." });
        }

        var updated = ToRecord(id, request);
        var index = records.FindIndex(item => item.Id == id);
        records[index] = updated;
        await WriteCatalogAsync(records);

        return Ok(ToCatalogDto(updated, DateTime.UtcNow));
    }

    [HttpDelete("catalog/{id:int}")]
    public async Task<IActionResult> DeleteCatalogEvent(int id)
    {
        var records = await ReadCatalogAsync();
        var updated = records.Where(item => item.Id != id).ToList();
        if (updated.Count == records.Count)
        {
            return NotFound();
        }

        await WriteCatalogAsync(updated);
        return NoContent();
    }

    [HttpPost("catalog/{id:int}/archive")]
    public async Task<IActionResult> ArchiveById(int id)
    {
        var records = await ReadCatalogAsync();
        var record = records.FirstOrDefault(item => item.Id == id);
        if (record is null)
        {
            return NotFound();
        }

        record.IsArchived = true;
        await WriteCatalogAsync(records);
        return NoContent();
    }

    [HttpDelete("catalog/{id:int}/archive")]
    public async Task<IActionResult> UnarchiveById(int id)
    {
        var records = await ReadCatalogAsync();
        var record = records.FirstOrDefault(item => item.Id == id);
        if (record is null)
        {
            return NotFound();
        }

        if (IsExpired(record, DateTime.UtcNow))
        {
            return BadRequest(new { message = "Expired events remain archived." });
        }

        record.IsArchived = false;
        await WriteCatalogAsync(records);
        return NoContent();
    }

    [HttpGet("archived")]
    public async Task<ActionResult<IEnumerable<string>>> GetArchivedEvents()
    {
        var records = await ReadCatalogAsync();
        var now = DateTime.UtcNow;
        return Ok(records.Where(record => IsArchived(record, now)).Select(record => record.EventName).ToList());
    }

    [HttpPost("archived/{eventName}")]
    public async Task<IActionResult> ArchiveEvent(string eventName)
    {
        var records = await ReadCatalogAsync();
        var record = records.FirstOrDefault(item => string.Equals(item.EventName, eventName, StringComparison.OrdinalIgnoreCase));
        if (record is null)
        {
            return NotFound();
        }

        record.IsArchived = true;
        await WriteCatalogAsync(records);

        return NoContent();
    }

    [HttpDelete("archived/{eventName}")]
    public async Task<IActionResult> UnarchiveEvent(string eventName)
    {
        var records = await ReadCatalogAsync();
        var record = records.FirstOrDefault(item => string.Equals(item.EventName, eventName, StringComparison.OrdinalIgnoreCase));
        if (record is null)
        {
            return NotFound();
        }

        if (IsExpired(record, DateTime.UtcNow))
        {
            return BadRequest(new { message = "Expired events remain archived." });
        }

        record.IsArchived = false;
        await WriteCatalogAsync(records);

        return NoContent();
    }

    [HttpDelete("archived")]
    public async Task<IActionResult> ClearArchivedEvents()
    {
        var records = await ReadCatalogAsync();
        var now = DateTime.UtcNow;
        foreach (var record in records)
        {
            if (!IsExpired(record, now))
            {
                record.IsArchived = false;
            }
        }

        await WriteCatalogAsync(records);
        return NoContent();
    }

    [HttpGet("details/{eventName}")]
    public async Task<ActionResult<EventDetailsDto>> GetEventDetails(string eventName)
    {
        var records = await ReadCatalogAsync();
        var record = records.FirstOrDefault(item => string.Equals(item.EventName, eventName, StringComparison.OrdinalIgnoreCase));
        if (record is null)
        {
            return NotFound();
        }

        return Ok(ToDetailsDto(record));
    }

    [HttpPut("details/{eventName}")]
    public async Task<IActionResult> SaveEventDetails(string eventName, [FromBody] EventDetailsUpdateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var records = await ReadCatalogAsync();
        var record = records.FirstOrDefault(item => string.Equals(item.EventName, eventName, StringComparison.OrdinalIgnoreCase));
        if (record is null)
        {
            var nextId = records.Count == 0 ? 1 : records.Max(item => item.Id) + 1;
            record = CreateGenericRecord(nextId, eventName);
            records.Add(record);
        }

        ApplyDetails(record, request);
        await WriteCatalogAsync(records);

        return NoContent();
    }

    [HttpDelete("details/{eventName}")]
    public async Task<IActionResult> ClearEventDetails(string eventName)
    {
        var records = await ReadCatalogAsync();
        var record = records.FirstOrDefault(item => string.Equals(item.EventName, eventName, StringComparison.OrdinalIgnoreCase));
        if (record is null)
        {
            return NotFound();
        }

        var defaults = GetDefaultCatalog().FirstOrDefault(item => string.Equals(item.EventName, eventName, StringComparison.OrdinalIgnoreCase));
        if (defaults is null)
        {
            record.Title = eventName;
            record.Subtitle = "Event details are available below.";
            record.Overview = "Please review the event information and safety instructions before registering.";
            record.ShortDescription = "Custom event managed by admin.";
            record.ImageSrc = null;
            record.ImageAlt = null;
            record.Schedule = ["Schedule will be announced."];
            record.Requirements = ["Requirements will be announced."];
            record.Payment = ["Payment details will be announced."];
        }
        else
        {
            record.Title = defaults.Title;
            record.Subtitle = defaults.Subtitle;
            record.Overview = defaults.Overview;
            record.ShortDescription = defaults.ShortDescription;
            record.ImageSrc = defaults.ImageSrc;
            record.ImageAlt = defaults.ImageAlt;
            record.Schedule = defaults.Schedule.ToList();
            record.Requirements = defaults.Requirements.ToList();
            record.Payment = defaults.Payment.ToList();
        }

        await WriteCatalogAsync(records);

        return NoContent();
    }

    private static bool IsExpired(EventCatalogRecord record, DateTime nowUtc)
    {
        return record.DeadlineUtc.HasValue && record.DeadlineUtc.Value < nowUtc;
    }

    private static bool IsArchived(EventCatalogRecord record, DateTime nowUtc)
    {
        return record.IsArchived || IsExpired(record, nowUtc);
    }

    private static EventCatalogItemDto ToCatalogDto(EventCatalogRecord record, DateTime nowUtc)
    {
        return new EventCatalogItemDto
        {
            Id = record.Id,
            EventName = record.EventName,
            Title = record.Title,
            Subtitle = record.Subtitle,
            Overview = record.Overview,
            ShortDescription = record.ShortDescription,
            EventDateUtc = record.EventDateUtc,
            DeadlineUtc = record.DeadlineUtc,
            IsArchived = IsArchived(record, nowUtc),
            IsExpired = IsExpired(record, nowUtc),
            ImageSrc = record.ImageSrc,
            ImageAlt = record.ImageAlt,
            Schedule = record.Schedule.ToList(),
            Requirements = record.Requirements.ToList(),
            Payment = record.Payment.ToList()
        };
    }

    private static EventDetailsDto ToDetailsDto(EventCatalogRecord record)
    {
        return new EventDetailsDto
        {
            EventName = record.EventName,
            Title = record.Title,
            Subtitle = record.Subtitle,
            Overview = record.Overview,
            ImageSrc = record.ImageSrc,
            ImageAlt = record.ImageAlt,
            Schedule = record.Schedule.ToList(),
            Requirements = record.Requirements.ToList(),
            Payment = record.Payment.ToList()
        };
    }

    private static EventCatalogRecord ToRecord(int id, EventCatalogUpsertRequest request)
    {
        var record = new EventCatalogRecord
        {
            Id = id,
            EventName = request.EventName.Trim(),
            Title = request.Title.Trim(),
            Subtitle = request.Subtitle.Trim(),
            Overview = request.Overview.Trim(),
            ShortDescription = request.ShortDescription.Trim(),
            EventDateUtc = request.EventDateUtc,
            DeadlineUtc = request.DeadlineUtc,
            IsArchived = request.IsArchived,
            ImageSrc = string.IsNullOrWhiteSpace(request.ImageSrc) ? null : request.ImageSrc.Trim(),
            ImageAlt = string.IsNullOrWhiteSpace(request.ImageAlt) ? null : request.ImageAlt.Trim(),
            Schedule = NormalizeLines(request.Schedule, ["Schedule will be announced."]),
            Requirements = NormalizeLines(request.Requirements, ["Requirements will be announced."]),
            Payment = NormalizeLines(request.Payment, ["Payment details will be announced."])
        };

        return NormalizeRecord(record);
    }

    private static void ApplyDetails(EventCatalogRecord record, EventDetailsUpdateRequest request)
    {
        record.Title = request.Title.Trim();
        record.Subtitle = request.Subtitle.Trim();
        record.Overview = request.Overview.Trim();
        record.ShortDescription = request.Overview.Trim().Length > 120 ? request.Overview.Trim()[..120] + "..." : request.Overview.Trim();
        record.ImageSrc = string.IsNullOrWhiteSpace(request.ImageSrc) ? null : request.ImageSrc.Trim();
        record.ImageAlt = string.IsNullOrWhiteSpace(request.ImageAlt) ? null : request.ImageAlt.Trim();
        record.Schedule = NormalizeLines(request.Schedule, ["Schedule will be announced."]);
        record.Requirements = NormalizeLines(request.Requirements, ["Requirements will be announced."]);
        record.Payment = NormalizeLines(request.Payment, ["Payment details will be announced."]);
    }

    private static List<string> NormalizeLines(IEnumerable<string> lines, List<string> fallback)
    {
        var cleaned = lines
            .Select(value => value?.Trim() ?? string.Empty)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToList();

        return cleaned.Count == 0 ? fallback : cleaned;
    }

    private static EventCatalogRecord NormalizeRecord(EventCatalogRecord record)
    {
        record.EventName = record.EventName.Trim();
        record.Title = record.Title.Trim();
        record.Subtitle = record.Subtitle.Trim();
        record.Overview = record.Overview.Trim();
        record.ShortDescription = record.ShortDescription.Trim();
        record.ImageSrc = string.IsNullOrWhiteSpace(record.ImageSrc) ? null : record.ImageSrc.Trim();
        record.ImageAlt = string.IsNullOrWhiteSpace(record.ImageAlt) ? null : record.ImageAlt.Trim();
        record.Schedule = NormalizeLines(record.Schedule, ["Schedule will be announced."]);
        record.Requirements = NormalizeLines(record.Requirements, ["Requirements will be announced."]);
        record.Payment = NormalizeLines(record.Payment, ["Payment details will be announced."]);
        if (string.IsNullOrWhiteSpace(record.ShortDescription))
        {
            record.ShortDescription = record.Overview.Length > 120 ? record.Overview[..120] + "..." : record.Overview;
        }

        return record;
    }

    private static EventCatalogRecord CreateGenericRecord(int id, string eventName)
    {
        return new EventCatalogRecord
        {
            Id = id,
            EventName = eventName.Trim(),
            Title = eventName.Trim(),
            Subtitle = "Event details are available below.",
            Overview = "Please review the event information and safety instructions before registering.",
            ShortDescription = "Custom event managed by admin.",
            EventDateUtc = null,
            DeadlineUtc = null,
            IsArchived = false,
            ImageSrc = null,
            ImageAlt = null,
            Schedule = ["Schedule will be announced."],
            Requirements = ["Requirements will be announced."],
            Payment = ["Payment details will be announced."]
        };
    }

    private static List<EventCatalogRecord> GetDefaultCatalog()
    {
        return
        [
            new EventCatalogRecord
            {
                Id = 1,
                EventName = "Sundarbans Eco Exploration",
                Title = "Sundarbans Eco Exploration",
                Subtitle = "Mangrove ecosystem learning camp with guided exploration and nature safety orientation.",
                Overview = "A one-day eco-focused expedition for observation, awareness, and team-based field learning.",
                ShortDescription = "Nature observation, responsible travel workshop, and mangrove awareness camp.",
                EventDateUtc = new DateTime(2026, 5, 3, 0, 0, 0, DateTimeKind.Utc),
                DeadlineUtc = new DateTime(2026, 5, 1, 18, 0, 0, DateTimeKind.Utc),
                IsArchived = false,
                ImageSrc = "sundarban.webp",
                ImageAlt = "Sundarbans nature scene for KUET Adventure Club event",
                Schedule =
                [
                    "Reporting at KUET gate: 5:30 AM",
                    "Departure by bus: 6:00 AM",
                    "Guided exploration and workshop: 10:00 AM - 3:00 PM",
                    "Return to campus: 9:00 PM"
                ],
                Requirements =
                [
                    "Student ID and club ID",
                    "Comfortable trekking shoes",
                    "Reusable water bottle",
                    "Basic personal medicine"
                ],
                Payment =
                [
                    "Registration fee: 800 BDT",
                    "bKash number: 01712-345678",
                    "Bank account number: 123456789012",
                    "Payment method: bKash (send money) or bank transfer",
                    "Transaction format: bk + last 4 digits (example: bk1298)",
                    "Use that transaction ID in registration form"
                ]
            },
            new EventCatalogRecord
            {
                Id = 2,
                EventName = "KUET to Bagerhat Cycling Run",
                Title = "KUET to Bagerhat Cycling Run",
                Subtitle = "Long-distance group cycling event focusing endurance, road discipline, and hydration planning.",
                Overview = "A 70 km controlled route ride with mentor checkpoints and pace groups.",
                ShortDescription = "70 km endurance ride with hydration checkpoints and riding discipline drills.",
                EventDateUtc = new DateTime(2026, 5, 17, 0, 0, 0, DateTimeKind.Utc),
                DeadlineUtc = new DateTime(2026, 5, 15, 18, 0, 0, DateTimeKind.Utc),
                IsArchived = false,
                ImageSrc = "cycling.webp",
                ImageAlt = "KUET Adventure Club cycling event photo",
                Schedule =
                [
                    "Bike check and briefing: 5:00 AM",
                    "Ride start: 5:45 AM",
                    "Checkpoint breaks every 20 km",
                    "Expected return: 2:00 PM"
                ],
                Requirements =
                [
                    "Helmet and front-back lights",
                    "Roadworthy cycle with brakes",
                    "Two water bottles",
                    "Emergency contact number"
                ],
                Payment =
                [
                    "Registration fee: 500 BDT",
                    "bKash number: 01718-112233",
                    "Bank account number: 123456789013",
                    "Payment method: bKash (merchant) or bank transfer",
                    "Transaction format: bk + last 4 digits (example: bk4455)",
                    "Provide transaction ID during registration"
                ]
            },
            new EventCatalogRecord
            {
                Id = 3,
                EventName = "Adventure Bootcamp 3.0",
                Title = "Adventure Bootcamp 3.0",
                Subtitle = "Two-day intensive bootcamp with team challenges, map reading, and survival practice.",
                Overview = "Hands-on field training to improve leadership, planning, and outdoor emergency response.",
                ShortDescription = "Two-day training on map reading, shelter building, and team challenge circuits.",
                EventDateUtc = new DateTime(2026, 6, 6, 0, 0, 0, DateTimeKind.Utc),
                DeadlineUtc = new DateTime(2026, 6, 4, 18, 0, 0, DateTimeKind.Utc),
                IsArchived = false,
                ImageSrc = "shelter.webp",
                ImageAlt = "Adventure Bootcamp shelter building photo",
                Schedule =
                [
                    "Day 1 reporting: 8:00 AM",
                    "Shelter and knot workshops: Day 1",
                    "Night camp drills: Day 1 evening",
                    "Final challenge and wrap-up: Day 2"
                ],
                Requirements =
                [
                    "Sleeping bag and light backpack",
                    "Torch and power bank",
                    "Personal utensils",
                    "Sports shoes and extra clothing"
                ],
                Payment =
                [
                    "Registration fee: 1200 BDT",
                    "bKash number: 01722-334455",
                    "Bank account number: ibbl 123456789014",
                    "Payment method: bKash (send money) or bank transfer",
                    "Transaction format: bk + last 4 digits (example: bk7721)",
                    "Submit valid transaction ID in the registration form"
                ]
            }
        ];
    }

    private static async Task<List<EventCatalogRecord>> ReadCatalogAsync()
    {
        if (!System.IO.File.Exists(EventCatalogFilePath))
        {
            var records = GetDefaultCatalog();
            await ApplyLegacyDataAsync(records);
            if (EnsureExpiredArchived(records))
            {
                // no-op: persisted below
            }
            await WriteCatalogAsync(records);
            return records;
        }

        await using var stream = System.IO.File.OpenRead(EventCatalogFilePath);
        var data = await JsonSerializer.DeserializeAsync<List<EventCatalogRecord>>(stream, JsonOptions) ?? [];
        var normalizedRecords = data.Select(NormalizeRecord).ToList();
        if (EnsureExpiredArchived(normalizedRecords))
        {
            await WriteCatalogAsync(normalizedRecords);
        }

        return normalizedRecords;
    }

    private static async Task ApplyLegacyDataAsync(List<EventCatalogRecord> records)
    {
        var archivedNames = await ReadLegacyArchivedEventsAsync();
        var overrides = await ReadLegacyEventDetailsOverridesAsync();

        foreach (var record in records)
        {
            if (archivedNames.Contains(record.EventName, StringComparer.OrdinalIgnoreCase))
            {
                record.IsArchived = true;
            }

            if (overrides.TryGetValue(record.EventName, out var details))
            {
                record.Title = details.Title;
                record.Subtitle = details.Subtitle;
                record.Overview = details.Overview;
                record.ShortDescription = details.Overview.Length > 120 ? details.Overview[..120] + "..." : details.Overview;
                record.ImageSrc = details.ImageSrc;
                record.ImageAlt = details.ImageAlt;
                record.Schedule = details.Schedule.ToList();
                record.Requirements = details.Requirements.ToList();
                record.Payment = details.Payment.ToList();
            }
        }

        foreach (var pair in overrides)
        {
            if (records.Any(item => string.Equals(item.EventName, pair.Key, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            var nextId = records.Count == 0 ? 1 : records.Max(item => item.Id) + 1;
            var created = CreateGenericRecord(nextId, pair.Key);
            ApplyDetails(created, new EventDetailsUpdateRequest
            {
                Title = pair.Value.Title,
                Subtitle = pair.Value.Subtitle,
                Overview = pair.Value.Overview,
                ImageSrc = pair.Value.ImageSrc,
                ImageAlt = pair.Value.ImageAlt,
                Schedule = pair.Value.Schedule,
                Requirements = pair.Value.Requirements,
                Payment = pair.Value.Payment
            });
            created.IsArchived = archivedNames.Contains(created.EventName, StringComparer.OrdinalIgnoreCase);
            records.Add(created);
        }
    }

    private static bool EnsureExpiredArchived(List<EventCatalogRecord> records)
    {
        var now = DateTime.UtcNow;
        var changed = false;
        foreach (var record in records)
        {
            if (IsExpired(record, now) && !record.IsArchived)
            {
                record.IsArchived = true;
                changed = true;
            }
        }

        return changed;
    }

    private static async Task<List<string>> ReadLegacyArchivedEventsAsync()
    {
        if (!System.IO.File.Exists(LegacyArchiveFilePath))
        {
            return [];
        }

        await using var stream = System.IO.File.OpenRead(LegacyArchiveFilePath);
        var events = await JsonSerializer.DeserializeAsync<List<string>>(stream, JsonOptions);
        return events ?? [];
    }

    private static async Task<Dictionary<string, EventDetailsDto>> ReadLegacyEventDetailsOverridesAsync()
    {
        if (!System.IO.File.Exists(LegacyEventDetailsOverridesFilePath))
        {
            return new Dictionary<string, EventDetailsDto>(StringComparer.OrdinalIgnoreCase);
        }

        await using var stream = System.IO.File.OpenRead(LegacyEventDetailsOverridesFilePath);
        var data = await JsonSerializer.DeserializeAsync<Dictionary<string, EventDetailsDto>>(stream, JsonOptions);
        return data ?? new Dictionary<string, EventDetailsDto>(StringComparer.OrdinalIgnoreCase);
    }

    private static async Task WriteCatalogAsync(List<EventCatalogRecord> records)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(EventCatalogFilePath)!);
        await using var stream = System.IO.File.Create(EventCatalogFilePath);
        await JsonSerializer.SerializeAsync(stream, records.OrderBy(item => item.Id).ToList(), JsonOptions);
    }
}