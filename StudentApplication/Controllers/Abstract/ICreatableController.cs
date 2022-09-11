using Microsoft.AspNetCore.Mvc;

namespace StudentApplication.Controllers.Abstract;

public interface ICreatableController<T, TKey>
    where T : class
    where TKey : IEquatable<TKey>
{
    public Task<ActionResult<TKey>> Create(T body);
}