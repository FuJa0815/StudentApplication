using System.ComponentModel.DataAnnotations;
using StudentApplication.Attributes;

namespace StudentApplication.Models;

public class Student : IWithId<int>
{
    [Key, RestKey]
    public int Id { get; set; }
    
    [RestKey, Required, MinLength(6), MaxLength(254)]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string FirstName { get; set; } = string.Empty;
    [Required]
    public string LastName { get; set; } = string.Empty;
    public virtual HashSet<Course> Courses { get; set; } = new();
}