using Microsoft.EntityFrameworkCore;
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
    public CoursesService(ApplicationDbContext db)
    {
        _db = db;
    }
    
    public async Task<bool> RemoveStudent(string id, string studentId)
    {
        var course = await ServiceHelper<Course>.GetByAnyIdAsync(_db.Courses.Include(c => c.Students), id);
        if (course == null) return false;
        var student = await ServiceHelper<Student>.GetByAnyIdAsync(_db.Students, studentId);
        if (student == null) return false;
        var remove = course.Students.Remove(student);
        await _db.SaveChangesAsync();
        return remove;
    }

    public async Task<bool> AddStudent(string id, string studentId)
    {
        var course = await ServiceHelper<Course>.GetByAnyIdAsync(_db.Courses.Include(c => c.Students), id);
        if (course == null) return false;
        var student = await ServiceHelper<Student>.GetByAnyIdAsync(_db.Students, studentId);
        if (student == null) return false;
        var add = course.Students.Add(student);
        await _db.SaveChangesAsync();
        return add;
    }
}