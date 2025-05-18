using Microsoft.AspNetCore.Mvc;
using TestWebApp.Models;
using TestWebApp.Services;

namespace TestWebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(UserService userService) : ControllerBase
{
    // 1. Create (Admin only)
    [HttpPost("create")]
    public IActionResult Create([FromBody] User userDto, [FromQuery] string whoChangeByLogin)
    {
        var admin = userService.GetByLogin(whoChangeByLogin);
        if (admin is not { Admin: true }) return Forbid("You are not administrator");
        if (userService.LoginExists(userDto.Login)) return BadRequest("Login exists");

        userDto.Id = Guid.NewGuid();
        userDto.CreatedOn = DateTime.UtcNow;
        userDto.ModifiedOn = DateTime.UtcNow;
        userDto.CreatedBy = whoChangeByLogin;
        userDto.ModifiedBy = whoChangeByLogin;
        userDto.RevokedBy = null;
        userDto.RevokedOn = null;
        userService.Add(userDto);

        return Ok(userDto);
    }

    // Update name, gender, birthday
    [HttpPut("update/info")]
    public IActionResult UpdateInfo(string login, string whoChangeByLogin, string? name, int? gender, DateTime? birthday)
    {
        var user = userService.GetByLogin(login);
        if (user == null) return NotFound();
        if (!userService.CanEdit(whoChangeByLogin, user))
            return Forbid();

        if (name != null) user.Name = name;
        if (gender is 0 or 1 or 2) user.Gender = gender.Value;
        if (birthday != null) user.Birthday = birthday;

        user.ModifiedOn = DateTime.UtcNow;
        user.ModifiedBy = whoChangeByLogin;

        userService.Update(user);
        return Ok(user);
    }

    // Update password
    [HttpPut("update/password")]
    public IActionResult UpdatePassword(string login, string whoChangeByLogin, string newPassword)
    {
        var user = userService.GetByLogin(login);
        if (user == null) return NotFound();
        if (!userService.CanEdit(whoChangeByLogin, user))
            return Forbid();

        user.Password = newPassword;
        user.ModifiedOn = DateTime.UtcNow;
        user.ModifiedBy = whoChangeByLogin;
        userService.Update(user);
        return Ok("Password updated");
    }

    // Update login
    [HttpPut("update/login")]
    public IActionResult UpdateLogin(string login, string whoChangeByLogin, string newLogin)
    {
        var user = userService.GetByLogin(login);
        if (user == null) return NotFound();
        if (!userService.CanEdit(whoChangeByLogin, user)) return Forbid();
        if (userService.LoginExists(newLogin)) return BadRequest("Login exists");

        user.Login = newLogin;
        user.ModifiedOn = DateTime.UtcNow;
        user.ModifiedBy = whoChangeByLogin;
        userService.Update(user);
        return Ok("Login updated");
    }

    // Get all active (admin only)
    [HttpGet("active")]
    public IActionResult GetActive([FromQuery] string whoGetByLogin)
    {
        if (!userService.LoginExists(whoGetByLogin)) return NotFound("Login not found");
        var admin = userService.GetByLogin(whoGetByLogin);
        if (admin is not { Admin: true }) return Forbid();
        return Ok(userService.GetAll().Where(u => u.RevokedOn == null).OrderBy(u => u.CreatedOn));
    }

    // Get user by login
    [HttpGet("get/by-login")]
    public IActionResult GetByLogin(string login, string whoGetByLogin)
    {
        if (!userService.LoginExists(login)) return NotFound("Login not found");
        
        var admin = userService.GetByLogin(whoGetByLogin);
        if (admin is not { Admin: true }) return Forbid();
        
        var user = userService.GetByLogin(login);
        if (user == null) return NotFound();

        return Ok(new
        {
            user.Name,
            user.Gender,
            user.Birthday,
            Active = user.RevokedOn == null
        });
    }

    // Get self by login+password
    [HttpGet("get/self")]
    public IActionResult GetSelf(string login, string password)
    {   if (!userService.LoginExists(login)) return NotFound("Login not found");
        var user = userService.GetByLogin(login);
        if (user is not { RevokedOn: null } || user.Password != password) return Forbid("Wrong password");
        return Ok(user);
    }

    // Get older than X (admin only)
    [HttpGet("get/older-than")]
    public IActionResult GetOlderThan(int age, string whoGetByLogin)
    {
        var admin = userService.GetByLogin(whoGetByLogin);
        if (admin == null || !admin.Admin) return Forbid();
        var threshold = DateTime.UtcNow.AddYears(-age);
        var result = userService.GetAll().Where(u => u.Birthday < threshold);
        return Ok(result);
    }

    // Delete (soft or full)
    [HttpDelete("delete")]
    public IActionResult Delete(string login, string whoGetByLogin, bool soft = true)
    {
        if (!userService.LoginExists(login)) return NotFound("Admin login not found");
        var admin = userService.GetByLogin(whoGetByLogin);
        if (admin is not { Admin: true }) return Forbid("You are not admin");
        var user = userService.GetByLogin(login);
        if (user == null) return NotFound("login not found");

        if (soft)
        {
            user.RevokedOn = DateTime.UtcNow;
            user.RevokedBy = whoGetByLogin;
            userService.Update(user);
        }
        else
        {
            userService.Remove(user);
        }

        return Ok("Deleted");
    }

    // Restore
    [HttpPut("restore")]
    public IActionResult Restore(string login, string whoGetByLogin)
    {
        if (!userService.LoginExists(login)) return NotFound("Admin login not found");
        var admin = userService.GetByLogin(whoGetByLogin);
        if (admin is not { Admin: true }) return Forbid();
        var user = userService.GetByLogin(login);
        if (user == null) return NotFound("Restored user not found");

        user.RevokedOn = null;
        user.RevokedBy = null;
        userService.Update(user);
        return Ok("Restored");
    }
}
