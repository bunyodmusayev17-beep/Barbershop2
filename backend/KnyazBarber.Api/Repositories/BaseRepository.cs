using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using KnyazBarber.Api.Data;

namespace KnyazBarber.Api.Repositories;

public class BaseRepository<T>(AppDbContext context) : IBaseRepository<T> where T : class
{
    protected readonly AppDbContext Context = context;
    protected readonly DbSet<T> DbSet = context.Set<T>();

    public IQueryable<T> Query() => DbSet;

    public IQueryable<T> QueryNoTracking() => DbSet.AsNoTracking();

    public Task<List<T>> GetAllAsync(CancellationToken ct, params Expression<Func<T, object>>[] includes) =>
        WithIncludes(includes).ToListAsync(ct);

    public Task<T?> GetByIdAsync(int id, CancellationToken ct, params Expression<Func<T, object>>[] includes)
    {
        // Kalit nomi model'dan olinadi — har bir entity uchun bir xil ishlaydi
        var key = Context.Model.FindEntityType(typeof(T))!.FindPrimaryKey()!.Properties[0].Name;
        return WithIncludes(includes).FirstOrDefaultAsync(e => EF.Property<int>(e, key) == id, ct);
    }

    public async Task<T> AddAsync(T entity, CancellationToken ct)
    {
        DbSet.Add(entity);
        await Context.SaveChangesAsync(ct);
        return entity;
    }

    public async Task UpdateAsync(T entity, CancellationToken ct)
    {
        if (Context.Entry(entity).State == EntityState.Detached) DbSet.Update(entity);
        await Context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(T entity, CancellationToken ct)
    {
        DbSet.Remove(entity);
        await Context.SaveChangesAsync(ct);
    }

    public void Add(T entity) => DbSet.Add(entity);

    public void Remove(T entity) => DbSet.Remove(entity);

    public Task SaveChangesAsync(CancellationToken ct) => Context.SaveChangesAsync(ct);

    private IQueryable<T> WithIncludes(Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = DbSet;
        foreach (var include in includes) query = query.Include(include);
        return query;
    }
}
