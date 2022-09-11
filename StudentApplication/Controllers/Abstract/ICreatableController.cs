using Microsoft.AspNetCore.Mvc;
using StudentApplication.Models;

namespace StudentApplication.Controllers.Abstract;

public interface ICreatableController<T, TKey> : IWithModel<T, TKey>
    where T : class, IIdentifiable<TKey>
    where TKey : IComparable, IEquatable<TKey>
{
    public Task<ActionResult<TKey>> Create(T body);
}