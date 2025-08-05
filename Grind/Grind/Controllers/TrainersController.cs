using Microsoft.AspNetCore.Mvc;
using Grind.Core.Interfaces;
using Grind.Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize] 
public class TrainersController : ControllerBase
{
    private readonly ITrainerService _trainerService;
    private readonly IHttpContextAccessor _httpContextAccessor; 

    public TrainersController(ITrainerService trainerService, IHttpContextAccessor httpContextAccessor)
    {
        _trainerService = trainerService;
        _httpContextAccessor = httpContextAccessor;
    }
    //כרגע לא בשימוש
    [HttpGet("all")]
    [AllowAnonymous] 
    public async Task<IActionResult> GetAllTrainers()
    {
        var trainers = await _trainerService.GetAllTrainersAsync();

        if (trainers == null || !trainers.Any())
        {
            return NotFound("לא נמצאו מאמנים במערכת.");
        }

        return Ok(trainers);
    }
    //כרגע לא בשימוש
    [HttpGet("{trainerId}")]
    public async Task<IActionResult> GetTrainerProfile(int trainerId)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int authenticatedUserId))
        {
            return Unauthorized("מזהה משתמש לא נמצא בטוקן או בפורמט לא תקין.");
        }

        if (authenticatedUserId != trainerId && !_httpContextAccessor.HttpContext.User.IsInRole("Admin")) 
        {
            return Forbid(); 
        }

        var trainer = await _trainerService.GetTrainerProfileAsync(trainerId);
        if (trainer == null)
        {
            return NotFound("פרופיל המאמן לא נמצא.");
        }
        return Ok(trainer);
    }

    [HttpPut("{trainerId}/profile")]
    [Authorize(Roles = "Trainer,Admin")] 
    public async Task<IActionResult> UpdateTrainerProfile(int trainerId, [FromBody] TrainerProfileUpdateDTO profileDto)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int authenticatedUserId))
        {
            return Unauthorized("מזהה משתמש לא נמצא בטוקן או בפורמט לא תקין.");
        }

        if (authenticatedUserId != trainerId && !_httpContextAccessor.HttpContext.User.IsInRole("Admin")) 
        {
            return Forbid(); 
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState); 
        }

        var result = await _trainerService.UpdateTrainerProfileAsync(trainerId, profileDto);

        if (result!=null)
        {
            return Ok("פרופיל המאמן עודכן בהצלחה.");
        }
        return BadRequest("עדכון פרופיל המאמן נכשל. ייתכן שהמאמן לא נמצא או המייל כבר בשימוש.");
    }

    //[HttpGet("{trainerId}/salary")]
    //// [Authorize(Roles = "Trainer")] // 👈 יש הערה
    //public async Task<IActionResult> GetTrainerMonthlySalary(int trainerId)
    //{
    //    // קבלת מזהה המשתמש המחובר מהטוקן
    //    var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
    //    if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int authenticatedUserId))
    //    {
    //        return Unauthorized("מזהה משתמש לא נמצא בטוקן או בפורמט לא תקין.");
    //    }

    //    // בדיקת הרשאות: רק המאמן עצמו או מנהל יכולים לראות את השכר
    //    if (authenticatedUserId != trainerId && !_httpContextAccessor.HttpContext.User.IsInRole("Admin"))
    //    {
    //        return Forbid(); // 403 Forbidden
    //    }

    //    var salary = await _trainerService.GetTrainerMonthlySalaryAsync(trainerId);
    //    if (salary == null) return NotFound();
    //    return Ok(salary);
    //}
    //כרגע לא בשימוש
    [HttpPost("admin/add")]
    [Authorize(Roles = "Client")] 
    public async Task<IActionResult> RegisterTrainer([FromBody] TrainerRegistrationDTO trainerDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _trainerService.AddTrainerAsync(trainerDto);

        if (result == null)
        {
            return Ok("המאמן נרשם בהצלחה.");
        }
        else
        {
            return BadRequest(result);
        }
    }

    //[HttpDelete("{trainerId}/terminate")]
    ////[Authorize(Roles = "Admin")] // 👈 רק מנהל יכול לפטר מאמן!
    //public async Task<IActionResult> TerminateTrainer(int trainerId)
    //{
    //    var result = await _trainerService.TerminateTrainerAsync(trainerId);

    //    if (result == null) // הצלחה - השירות החזיר null
    //    {
    //        return Ok($"המאמן בעל מזהה {trainerId} פוטר בהצלחה. כל השיעורים העתידיים שלו בוטלו והלקוחות עודכנו.");
    //    }
    //    else // כישלון - השירות החזיר הודעת שגיאה
    //    {
    //        return BadRequest(result); // מחזיר את הודעת השגיאה הספציפית
    //    }
    //}
}