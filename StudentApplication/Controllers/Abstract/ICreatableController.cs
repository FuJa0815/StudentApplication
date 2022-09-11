using Microsoft.AspNetCore.Mvc;

namespace StudentApplication.Controllers.Abstract;

public interface ICreatableController<T> : IWithModel<T>
    where T : class
{
    public Task<ActionResult<object>> Create(T body);
}