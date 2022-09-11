using System.Linq.Expressions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StudentApplication.Controllers.Abstract;
using StudentApplication.Data;
using StudentApplication.Hub;
using StudentApplication.Models;

namespace StudentApplication.Controllers;

public class StudentController : RestController<Student, int>
{
    public StudentController(ApplicationDbContext db, IHubContext<NotificationHub> hub, ILogger<StudentController> logger) : base(db, hub, logger)
    {
    }

    public override DbSet<Student> Model => Db.Students;

    protected override Expression<Func<Student, object>>[] GetListIgnoreProperties => new Expression<Func<Student, object>>[]
    {
        s => s.Courses
    };
}
