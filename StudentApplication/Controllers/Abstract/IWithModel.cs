using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using StudentApplication.Data;

namespace StudentApplication.Controllers.Abstract;

public interface IWithModel<T>
    where T : class
{
    public DbSet<T> Model { get; }
    
    [Inject]
    public ApplicationDbContext Db { get; }
}