using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using StudentApplication.Data;
using StudentApplication.Models;

namespace StudentApplication.Controllers.Abstract;

public interface IWithModel<T, TKey>
    where T : class, IIdentifiable<TKey>
    where TKey : IEquatable<TKey>
{
    public DbSet<T> Model { get; }
    
    [Inject]
    public ApplicationDbContext Db { get; }
}