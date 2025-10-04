using Microsoft.EntityFrameworkCore.Query;
using Rent2Read.Application.Common.Models;
using Rent2Read.Domain.Consts;
using System.Linq.Expressions;

namespace Rent2Read.Infrastructure.persistence.Repositories;
internal class BaseRepository<T> : IBaseRepository<T> where T : class
{
    protected readonly ApplicationDbContext _dbContext;

    public BaseRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public IEnumerable<T> GetAll(bool withNoTracking = true)
    {
        IQueryable<T> query = _dbContext.Set<T>();

        if (withNoTracking)
            query = query.AsNoTracking();

        return query.ToList();
    }

    public IQueryable<T> GetQueryable()
    {
        return _dbContext.Set<T>();
    }

    public PaginatedList<T> GetPaginatedList(IQueryable<T> query, int pageNumber, int pageSize)
    {
        return PaginatedList<T>.Create(query, pageNumber, pageSize);
    }

    public T? GetById(int id) => _dbContext.Set<T>().Find(id);

    public T? Find(Expression<Func<T, bool>> predicate) =>
        _dbContext.Set<T>().SingleOrDefault(predicate);

    public T? Find(Expression<Func<T, bool>> predicate, string[]? includes = null)
    {
        IQueryable<T> query = _dbContext.Set<T>();

        if (includes is not null)
            foreach (var include in includes)
                query = query.Include(include);

        return query.SingleOrDefault(predicate);
    }

    public T? Find(Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null)
    {
        IQueryable<T> query = _dbContext.Set<T>().AsQueryable();

        if (include is not null)
            query = include(query);

        return query.SingleOrDefault(predicate);
    }

    public IEnumerable<T> FindAll(Expression<Func<T, bool>> predicate,
        Expression<Func<T, object>>? orderBy = null, string? orderByDirection = OrderBy.Ascending)
    {
        IQueryable<T> query = _dbContext.Set<T>().Where(predicate);

        if (orderBy is not null)
        {
            if (orderByDirection == OrderBy.Ascending)
                query = query.OrderBy(orderBy);
            else
                query = query.OrderByDescending(orderBy);
        }

        return query.ToList();
    }

    public IEnumerable<T> FindAll(Expression<Func<T, bool>> predicate, int? skip = null, int? take = null,
        Expression<Func<T, object>>? orderBy = null, string? orderByDirection = OrderBy.Ascending)
    {
        IQueryable<T> query = _dbContext.Set<T>().Where(predicate);

        if (orderBy is not null)
        {
            if (orderByDirection == OrderBy.Ascending)
                query = query.OrderBy(orderBy);
            else
                query = query.OrderByDescending(orderBy);
        }

        if (skip.HasValue)
            query = query.Skip(skip.Value);

        if (take.HasValue)
            query = query.Take(take.Value);

        return query.ToList();
    }

    public IEnumerable<T> FindAll(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
        Expression<Func<T, object>>? orderBy = null, string? orderByDirection = OrderBy.Ascending)
    {
        IQueryable<T> query = _dbContext.Set<T>().AsQueryable();

        if (include is not null)
            query = include(query);

        query = query.Where(predicate);

        if (orderBy is not null)
        {
            if (orderByDirection == OrderBy.Ascending)
                query = query.OrderBy(orderBy);
            else
                query = query.OrderByDescending(orderBy);
        }

        return query.ToList();
    }

    public T Add(T entity)
    {
        _dbContext.Add(entity);
        return entity;
    }

    public IEnumerable<T> AddRange(IEnumerable<T> entities)
    {
        _dbContext.AddRange(entities);
        return entities;
    }

    public void Update(T entity) => _dbContext.Update(entity);

    //.NET 6
    public void Remove(T entity) => _dbContext.Remove(entity);

    //.NET 6
    public void RemoveRange(IEnumerable<T> entities) => _dbContext.RemoveRange(entities);

    //.NET 7
    public void DeleteBulk(Expression<Func<T, bool>> predicate) =>
        _dbContext.Set<T>().Where(predicate).ExecuteDelete();

    public bool IsExists(Expression<Func<T, bool>> predicate) =>
        _dbContext.Set<T>().Any(predicate);

    public int Count() => _dbContext.Set<T>().Count();

    public int Count(Expression<Func<T, bool>> predicate) => _dbContext.Set<T>().Count(predicate);

    public int Max(Expression<Func<T, bool>> predicate, Expression<Func<T, int>> field) =>
        _dbContext.Set<T>().Any(predicate) ? _dbContext.Set<T>().Where(predicate).Max(field) : 0;
}