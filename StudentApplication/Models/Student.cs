using System.ComponentModel.DataAnnotations;
using StudentApplication.Attributes;

namespace StudentApplication.Models;

public class Student : IWithId<int>
{
    [Key, RestKey]
    public int Id { get; set; }
    
    [RestKey]
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public virtual List<Course> Courses { get; set; } = new();
}