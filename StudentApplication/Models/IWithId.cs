using System.ComponentModel.DataAnnotations;
using StudentApplication.Attributes;

namespace StudentApplication.Models;

public interface IWithId<T>
    where T : IEquatable<T>
{
    [Key, RestKey]
    public T Id { get; set; }
}