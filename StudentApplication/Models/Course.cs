using System.ComponentModel.DataAnnotations;
using StudentApplication.Attributes;

namespace StudentApplication.Models;

public class Course : IWithId<int>
{
    [Key, RestKey]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public decimal Price { get; set; }
    
    public virtual List<Student> Students { get; set; }
}