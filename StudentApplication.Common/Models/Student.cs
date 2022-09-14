using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using StudentApplication.Common.Attributes;
using StudentApplication.Common.Utils;

namespace StudentApplication.Common.Models;

[CrudEndpoint("students")]
[Index(nameof(Email), IsUnique = true)]
public class Student : IModel<int>
{
    [Key, RestKey, RestSortable]
    public int Id { get; set; }
    
    [RestKey, Required, MinLength(6), MaxLength(254), RestSearchable, RestSortable, EmailAddress, DisplayName("E-Mail")]
    public string Email { get; set; } = string.Empty;
    
    [Required, RestSearchable, RestSortable, DisplayName("Firstname")]
    public string FirstName { get; set; } = string.Empty;
    [Required, RestSearchable, RestSortable, DisplayName("Lastname")]
    public string LastName { get; set; } = string.Empty;

    public virtual HashSet<Course> Courses { get; set; } = new();

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Id == ((Student)obj).Id;
    }

    public override int GetHashCode() => Id;
}