using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using StudentApplication.Common.Models;
using StudentApplication.Server.Controllers.Abstract;
using StudentApplication.Server.Data;
using StudentApplication.Server.Services;

namespace StudentApplication.Server.Controllers;

public class StudentsController : RestController<Student, int>
{
    public StudentsController(IRestService<Student, int> service) : base(service)
    {
    }

    public override DbSet<Student> ModelFromDb(ApplicationDbContext db) => db.Students;

    protected override Expression<Func<Student, object>>[] GetListIgnoreProperties => new Expression<Func<Student, object>>[]
    {
        s => s.Courses
    };
}
