using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentApplication.Common.Models;
using StudentApplication.Server.Controllers.Abstract;
using StudentApplication.Server.Data;
using StudentApplication.Server.Services;

namespace StudentApplication.Server.Controllers;

/// <summary>
///   Endpoints for courses
/// </summary>
public class CoursesController : RestController<Course, int>
{
    /// <summary>
    ///   Additional services besides normal CRUD operations for courses are defined in this service.
    /// </summary>
    private readonly ICoursesService _coursesService;
    public CoursesController(IRestService<Course, int> service, ICoursesService coursesService) : base(service)
    {
        _coursesService = coursesService;
    }

    public override DbSet<Course> ModelFromDb(ApplicationDbContext db) => db.Courses;
    public override IQueryable<Course> QueryableFromModel(DbSet<Course> model) => model.Include(c => c.Students);

    protected override Expression<Func<Course, object>>[] GetListIgnoreProperties =>
        new Expression<Func<Course, object>>[]
        {
            c => c.Students
        };

    protected override Expression<Func<Course, object>>[] GetOneIgnoreProperties =>
        new Expression<Func<Course, object>>[]
        {
            c => c.Students.First().Courses
        };

    [HttpPost("{id}/students/{studentId}")]
    public async Task<ActionResult> AddStudent(string id, string studentId)
    {
        if (await _coursesService.AddStudent(id, studentId))
            return Ok();
        return NotFound();
    }

    [HttpDelete("{id}/students/{studentId}")]
    public async Task<ActionResult> RemoveStudent(string id, string studentId)
    {
        if (await _coursesService.RemoveStudent(id, studentId))
            return Ok();
        return NotFound();
    }
}
