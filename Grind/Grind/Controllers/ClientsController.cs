using Microsoft.AspNetCore.Mvc;
using Grind.Core.Interfaces;
using Grind.Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ClientsController(IClientService clientService, IHttpContextAccessor httpContextAccessor)
    {
        _clientService = clientService;
        _httpContextAccessor = httpContextAccessor;
    }

    [HttpGet("{clientId}")]
    [Authorize(Roles = "Client,Admin,Trainer")]
    public async Task<IActionResult> GetClientProfile(int clientId)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int authenticatedUserId))
        {
            return Unauthorized("מזהה משתמש לא נמצא בטוקן או בפורמט לא תקין.");
        }

        if (authenticatedUserId != clientId && !_httpContextAccessor.HttpContext.User.IsInRole("Admin") && !_httpContextAccessor.HttpContext.User.IsInRole("Trainer"))
        {
            return Forbid();
        }

        var client = await _clientService.GetClientProfileAsync(clientId);
        if (client == null) return NotFound("פרופיל לקוח לא נמצא.");
        return Ok(client);
    }

    [HttpPut("{clientId}/profile")]
    [Authorize(Roles = "Client,Admin")]
    public async Task<IActionResult> UpdateClientProfile(int clientId, [FromBody] ClientProfileUpdateDTO profileDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState); 
        }

        try
        {
            var updatedClientDto = await _clientService.UpdateClientProfileAsync(clientId, profileDto);

            if (updatedClientDto == null)
            {
                return NotFound($"לקוח עם מזהה {clientId} לא נמצא, או שעדכון הפרופיל נכשל מסיבה לא ידועה.");
            }

            return Ok(updatedClientDto); 
        }
        catch (InvalidOperationException ex) 
        {
            return BadRequest(ex.Message); 
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"שגיאה פנימית בשרת בעת עדכון פרופיל הלקוח: {ex.Message}");
        }
    }
    //כרגע לא בשימוש
    [HttpPut("{clientId}/cancel-subscription")]
    [Authorize(Roles = "Client,Admin")]
    public async Task<IActionResult> CancelClientSubscription(int clientId)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int authenticatedUserId))
        {
            return Unauthorized("User ID not found in token or invalid format.");
        }

        if (authenticatedUserId != clientId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var result = await _clientService.CancelSubscriptionAsync(clientId);

        if (result)
        {
            return Ok("המנוי בוטל בהצלחה. כל הרישומים לשיעורים עתידיים הוסרו והלקוח קיבל על כך הודעה.");
        }
        return BadRequest("הפסקת המנוי נכשלה. לקוח לא נמצא או המנוי כבר אינו פעיל.");
    }
    //כרגע לא בשימוש
    [HttpGet("all")] 
    [Authorize(Roles = "Admin")] 
    public async Task<IActionResult> GetAllClients()
    {
        var clients = await _clientService.GetAllClientsAsync();

        if (clients == null || !clients.Any())
        {
            return NotFound("לא נמצאו לקוחות במערכת.");
        }

        return Ok(clients);
    }
}