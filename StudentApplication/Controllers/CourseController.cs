using Microsoft.EntityFrameworkCore;
using StudentApplication.Controllers.Abstract;
using StudentApplication.Data;
using StudentApplication.Models;

namespace StudentApplication.Controllers;

public class CourseController : RestController<Course>
{
    public CourseController(ApplicationDbContext db) : base(db)
    {
    }

    public override DbSet<Course> Model => Db.Courses;
}
