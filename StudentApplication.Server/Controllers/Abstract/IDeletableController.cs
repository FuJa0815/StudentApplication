using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace StudentApplication.Server.Controllers.Abstract;

public interface IDeletableController<T>
    where T : class
{
    public Task<ActionResult> Delete(string id);
}