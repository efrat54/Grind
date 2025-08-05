using Microsoft.AspNetCore.Mvc;
using Grind.Core.Interfaces;
using Grind.Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class ClassesController : ControllerBase
{
    private readonly IClassService _classService;

    public ClassesController(IClassService classService)
    {
        _classService = classService;
    }

    [HttpGet("client-schedule")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> GetClientSchedule()
    {
        var clientIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (clientIdClaim == null || !int.TryParse(clientIdClaim.Value, out int clientId))
        {
            return Unauthorized("Client ID not found in token or invalid format.");
        }

        var schedule = await _classService.GetClientClassesAsync(clientId);
        if (schedule == null || !schedule.Any())
        {
            return NotFound("No schedule found for this client, or client does not exist or subscription is not active.");
        }
        return Ok(schedule);
    }

    [HttpGet("all")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllClasses()
    {
        var classes = await _classService.GetAllClassesAsync();
        return Ok(classes);
    }

    [HttpPost("{classId}/register")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> RegisterForClass(int classId)
    {
        var clientIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (clientIdClaim == null || !int.TryParse(clientIdClaim.Value, out int authenticatedClientId))
        {
            return Unauthorized("Client ID not found in token or invalid format.");
        }

        var result = await _classService.RegisterClientForClassAsync(authenticatedClientId, classId);
        if (result.IsSuccess)
        {
            if (result.IsInWaitingList)
            {
                return Ok("Successfully added to waiting list.");
            }
            return Ok("Successfully registered for class.");
        }
        return BadRequest(result.Message);
    }

    [HttpDelete("{classId}/unregister")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> UnregisterClientFromClass(int classId)
    {
        var clientIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (clientIdClaim == null || !int.TryParse(clientIdClaim.Value, out int authenticatedClientId))
        {
            return Unauthorized("Client ID not found in token or invalid format.");
        }

        var result = await _classService.CancelClientClassRegistrationAsync(authenticatedClientId, classId);
        if (result)
        {
            return Ok("Class registration cancelled.");
        }
        return BadRequest("Failed to cancel registration or you were not registered for this class.");
    }

    [HttpPost("add")]
    [Authorize(Roles = "Trainer")]
    public async Task<IActionResult> AddClass([FromBody] ClassCreateDTO classDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var trainerUsernameClaim = User.FindFirst(ClaimTypes.Name);
        if (trainerUsernameClaim == null)
        {
            return Unauthorized("Trainer username not found in token.");
        }

        classDto.TrainerUsername = trainerUsernameClaim.Value;

        var result = await _classService.AddClassAsync(classDto);
        if (result)
        {
            return Ok("Class added successfully.");
        }
        return BadRequest("Failed to add class. Trainer not found or other issue.");
    }
    //כרגע לא בשימוש
    [HttpGet("{classId}")]
    [Authorize(Roles = "Client,Trainer,Admin")]
    public async Task<IActionResult> GetClassById(int classId)
    {
        var classItem = await _classService.GetClassByIdAsync(classId);
        if (classItem == null)
        {
            return NotFound($"Class with ID {classId} not found.");
        }
        return Ok(classItem);
    }

    [HttpPut("edit/{classId}")]
    [Authorize(Roles = "Trainer,Admin")]
    public async Task<IActionResult> EditClass(int classId, [FromBody] ClassDTO classDto)
    {
        if (classId != classDto.Id)
        {
            return BadRequest("Class ID in URL does not match ID in body.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userRoleClaim = User.FindFirst(ClaimTypes.Role);
        var usernameClaim = User.FindFirst(ClaimTypes.Name);

        if (userRoleClaim == null || usernameClaim == null)
        {
            return Unauthorized("User role or username not found in token.");
        }

        if (userRoleClaim.Value == "Trainer")
        {
            var classToVerify = await _classService.GetClassByIdAsync(classId);
            if (classToVerify == null || classToVerify.TrainerUsername != usernameClaim.Value)
            {
                return Forbid("You are not authorized to edit this class as you are not its trainer.");
            }
            if (!string.IsNullOrEmpty(classDto.TrainerUsername) && classDto.TrainerUsername != usernameClaim.Value)
            {
                return Forbid("You cannot set another trainer's username for this class.");
            }
            if (string.IsNullOrEmpty(classDto.TrainerUsername))
            {
                classDto.TrainerUsername = usernameClaim.Value;
            }
        }
        else if (userRoleClaim.Value == "Admin")
        {
            if (string.IsNullOrEmpty(classDto.TrainerUsername))
            {
                return BadRequest("When an Admin edits a class, TrainerUsername must be provided in the DTO.");
            }
        }

        var result = await _classService.UpdateClassAsync(classId, classDto);

        if (result)
        {
            return Ok("Class updated successfully.");
        }
        return BadRequest("Failed to update class. Class not found or invalid trainer.");
    }

    [HttpDelete("{classId}")]
    [Authorize(Roles = "Trainer,Admin")]
    public async Task<IActionResult> DeleteClass(int classId)
    {
        var userRoleClaim = User.FindFirst(ClaimTypes.Role);
        var usernameClaim = User.FindFirst(ClaimTypes.Name);

        if (userRoleClaim == null || usernameClaim == null)
        {
            return Unauthorized("User role or username not found in token.");
        }

        if (userRoleClaim.Value == "Trainer")
        {
            var classToVerify = await _classService.GetClassByIdAsync(classId);
            if (classToVerify == null || classToVerify.TrainerUsername != usernameClaim.Value)
            {
                return Forbid("You are not authorized to delete this class as you are not its trainer.");
            }
        }

        var result = await _classService.CancelClassAsync(classId); 
        if (result)
        {
            return NoContent();
        }
        return BadRequest("Failed to cancel/delete class.");
    }
    [HttpGet("trainer-schedule")]
    [Authorize(Roles = "Trainer")]
    public async Task<IActionResult> GetTrainerSchedule()
    {
        var trainerUsernameClaim = User.FindFirst(ClaimTypes.Name);
        if (trainerUsernameClaim == null)
        {
            return Unauthorized("Trainer username not found in token.");
        }

        var trainerUsername = trainerUsernameClaim.Value;
        var schedule = await _classService.GetClassesByTrainerUsernameAsync(trainerUsername);

        if (schedule == null || !schedule.Any())
        {
            return NotFound("No classes found for this trainer.");
        }

        return Ok(schedule);
    }

}