using System.Linq.Expressions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StudentApplication.Controllers.Abstract;
using StudentApplication.Data;
using StudentApplication.Hub;
using StudentApplication.Models;

namespace StudentApplication.Controllers;

public class CourseController : RestController<Course, int>
{
    public CourseController(ApplicationDbContext db, IHubContext<NotificationHub> hub) : base(db, hub)
    {
    }

    public override DbSet<Course> Model => Db.Courses;

    protected override Expression<Func<Course, object>>[] GetListIgnoreProperties =>
        new Expression<Func<Course, object>>[]
        {
            c => c.Students
        };
}
