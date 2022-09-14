using System;

namespace StudentApplication.Common.Attributes;

/// <summary>
///   This field is searchable via the query parameter of the RestController
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class RestSearchableAttribute : Attribute
{
}