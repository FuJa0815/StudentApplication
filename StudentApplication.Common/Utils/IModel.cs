using System;
using System.ComponentModel.DataAnnotations;
using StudentApplication.Common.Attributes;

namespace StudentApplication.Common.Utils;

public interface IModel<T>
    where T : IEquatable<T>
{
    [Key, RestKey]
    public T Id { get; set; }
}