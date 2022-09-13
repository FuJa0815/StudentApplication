using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DynamicLinq;
using StudentApplication.Common.Models;
using StudentApplication.Server.Data;

namespace StudentApplication.Server.Services;

public interface IStudentsService
{
    Task<bool> RemoveCourse(string id, string courseId);
    Task<bool> AddCourse(string id, string courseId);
}

public class StudentsService : IStudentsService
{
    private ApplicationDbContext _db;
    public StudentsService(ApplicationDbContext db)
    {
        _db = db;
    }
    
    public async Task<bool> RemoveCourse(string id, string courseId)
    {
        var course = await ServiceHelper<Course>.GetByAnyIdAsync(_db.Courses, courseId);
        if (course == null) return false;
        var student = await ServiceHelper<Student>.GetByAnyIdAsync(_db.Students.Include(s => s.Courses), id);
        if (student == null) return false;
        var remove = student.Courses.Remove(course);
        await _db.SaveChangesAsync();
        return remove;
    }

    public async Task<bool> AddCourse(string id, string courseId)
    {
        var course = await ServiceHelper<Course>.GetByAnyIdAsync(_db.Courses, courseId);
        if (course == null) return false;
        var student = await ServiceHelper<Student>.GetByAnyIdAsync(_db.Students.Include(s => s.Courses), id);
        if (student == null) return false;
        var add = student.Courses.Add(course);
        await _db.SaveChangesAsync();
        return add;
    }
}