using KuetAdventure.Api.Data;
using KuetAdventure.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KuetAdventure.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MembershipsController(AppDbContext dbContext) : ControllerBase
{
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
    public async Task<ActionResult<IEnumerable<MembershipApplication>>> GetAll()
    {
        var data = await dbContext.MembershipApplications
            .OrderByDescending(x => x.SubmittedAtUtc)
            .ToListAsync();

        return Ok(data);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MembershipApplication>> GetById(int id)
    {
        var row = await dbContext.MembershipApplications.FindAsync(id);
        if (row is null)
        {
            return NotFound();
        }

        return Ok(row);
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

        return NoContent();
    }
}
