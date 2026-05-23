using KuetAdventure.Api.Data;
using KuetAdventure.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace KuetAdventure.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MembershipsController(AppDbContext dbContext) : ControllerBase
{
    private static readonly string ReviewsFilePath = Path.Combine(AppContext.BaseDirectory, "membership-reviews.json");
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true, WriteIndented = true };

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MembershipCreateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var entity = new MembershipApplication
        {
            FullName = request.FullName.Trim(),
            MemberType = request.MemberType.Trim(),
            Department = request.Department.Trim(),
            RollId = request.RollId.Trim(),
            Batch = request.Batch.Trim(),
            Mailbox = request.Mailbox.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            HasPassport = request.HasPassport,
            Message = request.Message.Trim(),
            SubmittedAtUtc = DateTime.UtcNow
        };

        dbContext.MembershipApplications.Add(entity);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, new { entity.Id });
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MembershipAdminDto>>> GetAll()
    {
        var data = await dbContext.MembershipApplications
            .OrderByDescending(x => x.SubmittedAtUtc)
            .ToListAsync();

        var reviews = await ReadReviewsAsync();

        return Ok(data.Select(row => ToAdminDto(row, reviews.TryGetValue(row.Id, out var review) ? review : null)));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MembershipAdminDto>> GetById(int id)
    {
        var row = await dbContext.MembershipApplications.FindAsync(id);
        if (row is null)
        {
            return NotFound();
        }

        var reviews = await ReadReviewsAsync();
        reviews.TryGetValue(id, out var review);
        return Ok(ToAdminDto(row, review));
    }

    [HttpPost("{id:int}/accept")]
    public async Task<ActionResult<MembershipAdminDto>> Accept(int id, [FromBody] MembershipAcceptRequest? request)
    {
        var row = await dbContext.MembershipApplications.FindAsync(id);
        if (row is null)
        {
            return NotFound();
        }

        var reviews = await ReadReviewsAsync();
        reviews.TryGetValue(id, out var review);

        var clubMemberId = string.IsNullOrWhiteSpace(request?.ClubMemberId)
            ? review?.ClubMemberId ?? $"KAC-{DateTime.UtcNow:yy}-{id:0000}"
            : request!.ClubMemberId.Trim();

        review = new MembershipReviewRecord
        {
            Status = "Accepted",
            ClubMemberId = clubMemberId,
            ReviewedAtUtc = DateTime.UtcNow
        };

        reviews[id] = review;
        await WriteReviewsAsync(reviews);

        return Ok(ToAdminDto(row, review));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var row = await dbContext.MembershipApplications.FindAsync(id);
        if (row is null)
        {
            return NotFound();
        }

        dbContext.MembershipApplications.Remove(row);
        await dbContext.SaveChangesAsync();

        var reviews = await ReadReviewsAsync();
        if (reviews.Remove(id))
        {
            await WriteReviewsAsync(reviews);
        }

        return NoContent();
    }

    private static MembershipAdminDto ToAdminDto(MembershipApplication row, MembershipReviewRecord? review)
    {
        return new MembershipAdminDto
        {
            Id = row.Id,
            FullName = row.FullName,
            MemberType = row.MemberType,
            Department = row.Department,
            RollId = row.RollId,
            Batch = row.Batch,
            Mailbox = row.Mailbox,
            PhoneNumber = row.PhoneNumber,
            HasPassport = row.HasPassport,
            Message = row.Message,
            SubmittedAtUtc = row.SubmittedAtUtc,
            ReviewStatus = review?.Status ?? "Pending",
            ClubMemberId = review?.ClubMemberId,
            ReviewedAtUtc = review?.ReviewedAtUtc
        };
    }

    private static async Task<Dictionary<int, MembershipReviewRecord>> ReadReviewsAsync()
    {
        if (!System.IO.File.Exists(ReviewsFilePath))
        {
            return new Dictionary<int, MembershipReviewRecord>();
        }

        await using var stream = System.IO.File.OpenRead(ReviewsFilePath);
        var data = await JsonSerializer.DeserializeAsync<Dictionary<int, MembershipReviewRecord>>(stream, JsonOptions);
        return data ?? new Dictionary<int, MembershipReviewRecord>();
    }

    private static async Task WriteReviewsAsync(Dictionary<int, MembershipReviewRecord> reviews)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(ReviewsFilePath)!);
        await using var stream = System.IO.File.Create(ReviewsFilePath);
        await JsonSerializer.SerializeAsync(stream, reviews, JsonOptions);
    }
}
