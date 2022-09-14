using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace StudentApplication.Server.Controllers.Abstract;

public interface ICreatableController<T, TKey>
    where T : class
    where TKey : IEquatable<TKey>
{
    public Task<ActionResult<TKey>> Create(T body);
}