using Microsoft.AspNetCore.Mvc;
using StudentApplication.Models;

namespace StudentApplication.Controllers.Abstract;

public interface IDeletableController<T, TKey> : IWithModel<T, TKey>
    where T : class, IIdentifiable<TKey>
    where TKey : IComparable, IEquatable<TKey>
{
    public Task<ActionResult> Delete(TKey id);
}