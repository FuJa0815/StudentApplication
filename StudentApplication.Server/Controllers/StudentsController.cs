using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentApplication.Common.Models;
using StudentApplication.Server.Controllers.Abstract;
using StudentApplication.Server.Data;
using StudentApplication.Server.Services;

namespace StudentApplication.Server.Controllers;

/// <summary>
///   Endpoints for students
/// </summary>
public class StudentsController : RestController<Student, int>
{
    /// <summary>
    ///   Additional services besides normal CRUD operations for students are defined in this service.
    /// </summary>
    private readonly IStudentsService _studentsService;
    public StudentsController(IRestService<Student, int> service, IStudentsService studentsService) : base(service)
    {
        _studentsService = studentsService;
    }

    public override DbSet<Student> ModelFromDb(ApplicationDbContext db) => db.Students;
    public override IQueryable<Student> QueryableFromModel(DbSet<Student> model) => model.Include(s => s.Courses);

    protected override Expression<Func<Student, object>>[] GetListIgnoreProperties => new Expression<Func<Student, object>>[]
    {
        s => s.Courses
    };

    protected override Expression<Func<Student, object>>[] GetOneIgnoreProperties =>
        new Expression<Func<Student, object>>[]
        {
            s => s.Courses.First().Students
        };
    
    [HttpPost("{id}/courses/{courseId}")]
    public async Task<ActionResult> AddCourse(string id, string courseId)
    {
        if (await _studentsService.AddCourse(id, courseId))
            return Ok();
        return NotFound();
    }

    [HttpDelete("{id}/courses/{courseId}")]
    public async Task<ActionResult> RemoveStudent(string id, string courseId)
    {
        if (await _studentsService.RemoveCourse(id, courseId))
            return Ok();
        return NotFound();
    }
}
