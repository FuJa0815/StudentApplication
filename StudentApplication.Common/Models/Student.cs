using System.ComponentModel.DataAnnotations;
using StudentApplication.Common.Attributes;

namespace StudentApplication.Common.Models;

public class Student : IWithId<int>
{
    [Key, RestKey]
    public int Id { get; set; }
    
    [RestKey, Required, MinLength(6), MaxLength(254), RestSearchable]
    public string Email { get; set; } = string.Empty;
    
    [Required, RestSearchable]
    public string FirstName { get; set; } = string.Empty;
    [Required, RestSearchable]
    public string LastName { get; set; } = string.Empty;

    public virtual HashSet<Course> Courses { get; set; } = new();
}