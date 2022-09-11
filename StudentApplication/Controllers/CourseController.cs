using System.Linq.Expressions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StudentApplication.Controllers.Abstract;
using StudentApplication.Data;
using StudentApplication.Hub;
using StudentApplication.Models;
using StudentApplication.Services;

namespace StudentApplication.Controllers;

public class CourseController : RestController<Course, int>
{
    public CourseController(IRestService<Course, int> service) : base(service)
    {
    }

    public override DbSet<Course> ModelFromDb(ApplicationDbContext db) => db.Courses;

    protected override Expression<Func<Course, object>>[] GetListIgnoreProperties =>
        new Expression<Func<Course, object>>[]
        {
            c => c.Students
        };
}
