using Data.Contexts;
using Data.Interfaces;
using Domain.Extensions;
using Domain.Responses;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Data.Repositories;

public class BaseRepository<TEntity, TModel> : IBaseRepository<TEntity, TModel>, IBaseRepository<TEntity, TModel> where TEntity : class
{
    protected readonly DataContext _context;
    protected readonly DbSet<TEntity> _table;

    protected BaseRepository(DataContext context)
    {
        _context = context;
        _table = _context.Set<TEntity>();
    }

    public virtual async Task<RepositoryResult<TModel>> AddAsync(TEntity entity)
    {
        if (entity == null)
            return new RepositoryResult<TModel> { Succeeded = false, StatusCode = 400, Error = "Entity can not be null." };

        try
        {
            _table.Add(entity);
            await _context.SaveChangesAsync();
            return new RepositoryResult<TModel> { Succeeded = true, StatusCode = 200 };
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return new RepositoryResult<TModel> { Succeeded = false, StatusCode = 500, Error = ex.Message };
        }
    }

    public virtual async Task<RepositoryResult<IEnumerable<TModel>>> GetAllAsync(bool orderByDescending = false, Expression<Func<TEntity, object>>? sortBy = null, Expression<Func<TEntity, bool>>? where = null, int take = 0, params Expression<Func<TEntity, object>>[] includes)
    {
        try
        {
            IQueryable<TEntity> query = _table;

            if (where != null)
                query = query.Where(where);

            if (includes != null && includes.Length != 0)
                foreach (var include in includes)
                    query = query.Include(include);

            if (sortBy != null)
                query = orderByDescending
                    ? query.OrderByDescending(sortBy)
                    : query.OrderBy(sortBy);

            if (take > 0)
                query = query.Take(take);

            var entities = await query.ToListAsync();
            var result = entities.Select(entity => entity.MapTo<TModel>());
            return new RepositoryResult<IEnumerable<TModel>> { Succeeded = true, StatusCode = 200, Result = result };
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return new RepositoryResult<IEnumerable<TModel>> { Succeeded = false, StatusCode = 500, Error = ex.Message };
        }
    }

    public virtual async Task<RepositoryResult<TModel>> GetAsync(Expression<Func<TEntity, bool>> where, params Expression<Func<TEntity, object>>[] includes)
    {
        try
        {
            IQueryable<TEntity> query = _table;

            if (includes != null && includes.Length != 0)
                foreach (var include in includes)
                    query = query.Include(include);

            var entity = await query.FirstOrDefaultAsync(where);
            if (entity == null)
                return new RepositoryResult<TModel> { Succeeded = false, StatusCode = 404, Error = "Entity not found." };

            var result = entity.MapTo<TModel>();
            return new RepositoryResult<TModel> { Succeeded = true, StatusCode = 200, Result = result };
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return new RepositoryResult<TModel> { Succeeded = false, StatusCode = 500, Error = ex.Message };
        }
    }

    public virtual async Task<RepositoryResult<bool>> ExistsAsync(Expression<Func<TEntity, bool>> findBy)
    {
        if (findBy == null)
            return new RepositoryResult<bool> { Succeeded = false, StatusCode = 400, Error = "Invalid expression." };

        try
        {
            var exists = await _table.AnyAsync(findBy);
            return !exists
                ? new RepositoryResult<bool> { Succeeded = false, StatusCode = 404, Error = "Entity not found." }
                : new RepositoryResult<bool> { Succeeded = true, StatusCode = 200, Result = exists };
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return new RepositoryResult<bool> { Succeeded = false, StatusCode = 500, Error = ex.Message };
        }
    }

    public virtual async Task<RepositoryResult<TModel>> UpdateAsync(TEntity entity)
    {
        if (entity == null)
            return new RepositoryResult<TModel> { Succeeded = false, StatusCode = 400, Error = "Entity can not be null." };

        try
        {
            _table.Update(entity);
            await _context.SaveChangesAsync();
            var updatedModel = entity.MapTo<TModel>();
            return new RepositoryResult<TModel> { Succeeded = true, StatusCode = 200, Result = updatedModel };
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return new RepositoryResult<TModel> { Succeeded = false, StatusCode = 500, Error = ex.Message };
        }
    }

    public virtual async Task<RepositoryResult<bool>> DeleteAsync(TEntity entity)
    {
        if (entity == null)
            return new RepositoryResult<bool> { Succeeded = false, StatusCode = 400, Error = "Entity can not be null." };

        try
        {
            _table.Remove(entity);
            await _context.SaveChangesAsync();
            return new RepositoryResult<bool> { Succeeded = true, StatusCode = 204 };
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return new RepositoryResult<bool> { Succeeded = false, StatusCode = 500, Error = ex.Message };
        }
    }
}
