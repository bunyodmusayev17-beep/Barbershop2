using System.Linq.Expressions;

namespace KnyazBarber.Api.Repositories;

public interface IBaseRepository<T> where T : class
{
    /// <summary>O'zgartirish uchun so'rov (EF kuzatadi).</summary>
    IQueryable<T> Query();

    /// <summary>Faqat o'qish uchun so'rov (kuzatilmaydi, tezroq).</summary>
    IQueryable<T> QueryNoTracking();

    Task<List<T>> GetAllAsync(CancellationToken ct, params Expression<Func<T, object>>[] includes);
    Task<T?> GetByIdAsync(int id, CancellationToken ct, params Expression<Func<T, object>>[] includes);

    /// <summary>Qo'shadi va darhol saqlaydi.</summary>
    Task<T> AddAsync(T entity, CancellationToken ct);
    /// <summary>Saqlaydi (kuzatilmagan entity bo'lsa — avval biriktiradi).</summary>
    Task UpdateAsync(T entity, CancellationToken ct);
    /// <summary>O'chiradi va darhol saqlaydi.</summary>
    Task DeleteAsync(T entity, CancellationToken ct);

    /// <summary>Saqlamasdan qo'shish — bir nechta o'zgarishni bitta SaveChangesAsync bilan saqlash uchun.</summary>
    void Add(T entity);
    /// <summary>Saqlamasdan o'chirish (yangi qo'shilgan entity uchun — kontekstdan ajratadi).</summary>
    void Remove(T entity);

    /// <summary>Kontekstdagi barcha o'zgarishlarni saqlaydi (barcha repository'lar bitta DbContext'da).</summary>
    Task SaveChangesAsync(CancellationToken ct);
}
