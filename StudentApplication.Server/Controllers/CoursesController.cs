using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using StudentApplication.Common.Models;
using StudentApplication.Server.Controllers.Abstract;
using StudentApplication.Server.Data;
using StudentApplication.Server.Services;

namespace StudentApplication.Server.Controllers;

public class CoursesController : RestController<Course, int>
{
    public CoursesController(IRestService<Course, int> service) : base(service)
    {
    }

    public override DbSet<Course> ModelFromDb(ApplicationDbContext db) => db.Courses;

    protected override Expression<Func<Course, object>>[] GetListIgnoreProperties =>
        new Expression<Func<Course, object>>[]
        {
            c => c.Students
        };
}
