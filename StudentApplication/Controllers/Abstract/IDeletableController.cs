using Microsoft.AspNetCore.Mvc;

namespace StudentApplication.Controllers.Abstract;

public interface IDeletableController<T>
    where T : class
{
    public Task<ActionResult> Delete(string id);
}