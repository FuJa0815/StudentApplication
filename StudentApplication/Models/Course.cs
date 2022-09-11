using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentApplication.Models;

public class Course : IIdentifiable<int>
{
    [Key]
    public int CourseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public decimal Price { get; set; }
    
    public virtual List<Student> Students { get; set; }

    [NotMapped]
    int IIdentifiable<int>.Key
    {
        get => CourseId;
        set => CourseId = value;
    }
}