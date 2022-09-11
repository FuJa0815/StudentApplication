using Microsoft.AspNetCore.Mvc;

namespace StudentApplication.Controllers.Abstract;

public interface IDeletableController<T> : IWithModel<T>
    where T : class
{
    public Task<ActionResult> Delete(string id);
}