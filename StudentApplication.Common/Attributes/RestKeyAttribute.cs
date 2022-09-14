using System;

namespace StudentApplication.Common.Attributes;

/// <summary>
///   Declares a property as a REST key. This means that the client can access a resource via this field.
///   Uniqueness is required.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class RestKeyAttribute : Attribute
{
}