using System.ComponentModel.DataAnnotations;
using StudentApplication.Attributes;

namespace StudentApplication.Models;

public class Course : IWithId<int>
{
    [Key, RestKey]
    public int Id { get; set; }

    [MinLength(3), MaxLength(20), Required]
    public string Name { get; set; } = string.Empty;
    [Required]
    public DateTime StartsAt { get; set; }
    [Required]
    public DateTime EndsAt { get; set; }
    [Required, Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    public virtual HashSet<Student> Students { get; set; } = new();
}