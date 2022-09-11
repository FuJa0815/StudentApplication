using System.ComponentModel.DataAnnotations;
using StudentApplication.Attributes;

namespace StudentApplication.Models;

public class Course
{
    [Key, RestKey]
    public int CourseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public decimal Price { get; set; }
    
    public virtual List<Student> Students { get; set; }
}