using System;

namespace StudentApplication.Common.Attributes;

/// <summary>
///   This field is sortable via the sortBy parameter of the RestController
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class RestSortableAttribute : Attribute
{
}