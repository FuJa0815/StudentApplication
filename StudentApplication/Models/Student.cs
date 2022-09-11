using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentApplication.Models;

public class Student : IIdentifiable<int>, IIdentifiable<string>
{
    [Key]
    public int StudentId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public virtual List<Course> Courses { get; set; } = new();

    [NotMapped]
    int IIdentifiable<int>.Key
    {
        get => StudentId;
        set => StudentId = value;
    }

    [NotMapped]
    string IIdentifiable<string>.Key
    {
        get => Email;
        set => Email = value;
    }
}