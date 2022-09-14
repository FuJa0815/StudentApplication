using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudentApplication.Common.Models;
using StudentApplication.Server.Data;

namespace StudentApplication.Server.Services;

public interface ICoursesService
{
    Task<bool> RemoveStudent(string id, string studentId);
    Task<bool> AddStudent(string id, string studentId);
}

public class CoursesService : ICoursesService
{
    private ApplicationDbContext _db;
    private ILogger _logger;
    public CoursesService(ApplicationDbContext db, ILogger<CoursesService> logger)
    {
        _db = db;
        _logger = logger;
    }
    
    public async Task<bool> RemoveStudent(string id, string studentId)
    {
        var course = await ServiceHelper<Course>.GetByAnyIdAsync(_db.Courses.Include(c => c.Students), id);
        if (course == null)
        {
            _logger.LogWarning($"No course with id {id} found");
            return false;
        }
        var student = await ServiceHelper<Student>.GetByAnyIdAsync(_db.Students, studentId);
        if (student == null)
        {
            _logger.LogWarning($"No student with id {studentId} found");
            return false;
        }
        var remove = course.Students.Remove(student);
        await _db.SaveChangesAsync();
        _logger.LogInformation($"{student.Email} has been removed from {course.Name}");
        return remove;
    }

    public async Task<bool> AddStudent(string id, string studentId)
    {
        var course = await ServiceHelper<Course>.GetByAnyIdAsync(_db.Courses.Include(c => c.Students), id);
        if (course == null)
        {
            _logger.LogWarning($"No course with id {id} found");
            return false;
        }
        var student = await ServiceHelper<Student>.GetByAnyIdAsync(_db.Students, studentId);
        if (student == null)
        {
            _logger.LogWarning($"No student with id {studentId} found");
            return false;
        }
        var add = course.Students.Add(student);
        await _db.SaveChangesAsync();
        _logger.LogInformation($"{student.Email} has been added to {course.Name}");
        return add;
    }
}