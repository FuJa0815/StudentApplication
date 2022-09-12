using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using StudentApplication.Common.Attributes;
using StudentApplication.Common.Utils;

namespace StudentApplication.Common.Models;

[RestEndpoint("courses")]
public class Course : IModel<int>
{
    [Key, RestKey, RestSortable]
    public int Id { get; set; }

    [MinLength(3), MaxLength(20), Required, RestSearchable, RestSortable]
    public string Name { get; set; } = string.Empty;
    [Required, DisplayName("Starts at"), RestSortable]
    public DateTime StartsAt { get; set; }
    [Required, DisplayName("Ends at"), RestSortable]
    public DateTime EndsAt { get; set; }
    [Required, Range(0, double.MaxValue), RestSortable]
    public decimal Price { get; set; }

    public virtual HashSet<Student> Students { get; set; } = new();
}