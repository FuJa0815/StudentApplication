using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentApplication.Attributes;
using StudentApplication.Controllers.Abstract;
using StudentApplication.Data;
using StudentApplication.Models;

namespace StudentApplication.Controllers;

[ControllerName("Student")]
public class StudentControllerEmail : KeyRestController<Student, string>
{
    public StudentControllerEmail(ApplicationDbContext db) : base(db)
    {
    }

    [HttpGet("by-email/{email}")]
    public override Task<ActionResult<Student>> GetOne(string email) => base.GetOne(email);

    [HttpDelete("by-email/{email}")]
    public override Task<ActionResult> Delete(string email) => base.Delete(email);

    public override DbSet<Student> Model => Db.Students;
}

[ControllerName("Student")]
public class StudentControllerId : RestController<Student, int>
{
    public StudentControllerId(ApplicationDbContext db) : base(db)
    {
    }

    public override DbSet<Student> Model => Db.Students;

    protected override Expression<Func<Student, object>>[] GetListIgnoreProperties => new Expression<Func<Student, object>>[]
    {
        s => s.Courses
    };
}
