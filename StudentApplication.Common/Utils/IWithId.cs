using System.ComponentModel.DataAnnotations;
using StudentApplication.Common.Attributes;

namespace StudentApplication.Common.Utils;

public interface IWithId<T>
    where T : IEquatable<T>
{
    [Key, RestKey]
    public T Id { get; set; }
}