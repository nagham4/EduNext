using backend.DTOs.Guest;
using backend.Services.Guest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/contact-messages")]
public class ContactMessagesController : ControllerBase
{
    private readonly IContactMessageService _contactMessageService;

    public ContactMessagesController(IContactMessageService contactMessageService)
    {
        _contactMessageService = contactMessageService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateContactMessageDto dto,
        CancellationToken cancellationToken
    )
    {
        if (
            string.IsNullOrWhiteSpace(dto.Name) ||
            string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.Message)
        )
        {
            return BadRequest(new
            {
                message = "Name, email, and message are required."
            });
        }

        var result = await _contactMessageService.CreateAsync(dto, cancellationToken);

        return CreatedAtAction(nameof(Create), new { id = result.Id }, new
        {
            message = "Contact message received successfully.",
            contactMessage = result
        });
    }

    [Authorize(Roles = "admin")]
    [HttpGet("/api/admin/contact-messages")]
    public async Task<ActionResult<IReadOnlyList<ContactMessageDto>>> GetAll(
        CancellationToken cancellationToken
    )
    {
        var messages = await _contactMessageService.GetAllAsync(cancellationToken);
        return Ok(messages);
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("/api/admin/contact-messages/{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        var deleted = await _contactMessageService.DeleteAsync(id, cancellationToken);

        if (!deleted)
        {
            return NotFound(new { message = "Contact message was not found." });
        }

        return NoContent();
    }
}
