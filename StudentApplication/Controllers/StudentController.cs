using System.Linq.Expressions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StudentApplication.Controllers.Abstract;
using StudentApplication.Data;
using StudentApplication.Hub;
using StudentApplication.Models;
using StudentApplication.Services;

namespace StudentApplication.Controllers;

public class StudentController : RestController<Student, int>
{
    public StudentController(IRestService<Student, int> service) : base(service)
    {
    }

    public override DbSet<Student> ModelFromDb(ApplicationDbContext db) => db.Students;

    protected override Expression<Func<Student, object>>[] GetListIgnoreProperties => new Expression<Func<Student, object>>[]
    {
        s => s.Courses
    };
}
