using System.ComponentModel.DataAnnotations;

namespace TestWebApp.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [RegularExpression("^[a-zA-Z0-9]+$")]
    public string Login { get; set; } = string.Empty;
    
    [Required]
    [RegularExpression("^[a-zA-Z0-9]+$")]
    public string Password { get; set; } = string.Empty;
    
    [Required]
    [RegularExpression("^[a-zA-Zа-яА-Я]+$")]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [Range(0, 2)]
    public int Gender { get; set; } = 2;
    
    public DateTime? Birthday { get; set; }
    
    public bool Admin { get; set; }
    
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = "Admin";

    public DateTime? ModifiedOn { get; set; } = DateTime.UtcNow;
    public string? ModifiedBy { get; set; }

    public DateTime? RevokedOn { get; set; }
    public string? RevokedBy { get; set; }
}
