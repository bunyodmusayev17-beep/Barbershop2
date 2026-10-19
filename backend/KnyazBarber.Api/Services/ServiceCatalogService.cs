using Microsoft.EntityFrameworkCore;
using KnyazBarber.Api.Dtos.Service;
using KnyazBarber.Api.Entities;
using KnyazBarber.Api.Exceptions;
using KnyazBarber.Api.Helpers;
using KnyazBarber.Api.Repositories;

namespace KnyazBarber.Api.Services;

public class ServiceCatalogService(
    IBaseRepository<Service> services,
    IBaseRepository<Barber> barbers,
    IBaseRepository<Appointment> appointments) : IServiceCatalogService
{
    public async Task<List<ServiceAdminDto>> ListAsync(CancellationToken ct) =>
        (await services.QueryNoTracking().Include(s => s.Barbers).OrderBy(s => s.SortOrder).ThenBy(s => s.Id).ToListAsync(ct))
        .Select(s => s.ToDto()).ToList();

    public async Task<ServiceAdminDto> CreateAsync(ServiceInput input, CancellationToken ct)
    {
        var s = new Service();
        await ApplyAsync(s, input, ct);
        await services.AddAsync(s, ct);
        return s.ToDto();
    }

    public async Task<ServiceAdminDto> UpdateAsync(int id, ServiceInput input, CancellationToken ct)
    {
        var s = await services.GetByIdAsync(id, ct, x => x.Barbers) ?? throw new NotFoundException("Xizmat topilmadi");
        await ApplyAsync(s, input, ct);
        await services.SaveChangesAsync(ct);
        return s.ToDto();
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var s = await services.GetByIdAsync(id, ct) ?? throw new NotFoundException("Xizmat topilmadi");
        if (await appointments.QueryNoTracking().AnyAsync(a => a.ServiceId == id, ct))
        {
            s.IsActive = false;
            await services.SaveChangesAsync(ct);
            return;
        }
        await services.DeleteAsync(s, ct);
    }

    private async Task ApplyAsync(Service s, ServiceInput input, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(input.Name.Uz)) throw new BadRequestException("Nom (o'zbekcha) majburiy");
        s.Name = input.Name.Clean(100);
        s.Description = input.Description.Clean(500);
        s.Price = input.Price;
        s.DurationMinutes = input.DurationMinutes;
        s.SortOrder = input.SortOrder;
        s.IsActive = input.IsActive;

        var wanted = (input.BarberIds ?? []).Distinct().ToList();
        var valid = await barbers.QueryNoTracking().Where(b => wanted.Contains(b.Id)).Select(b => b.Id).ToListAsync(ct);
        s.Barbers.RemoveAll(b => !valid.Contains(b.BarberId));
        foreach (var id in valid.Where(id => s.Barbers.All(b => b.BarberId != id)))
            s.Barbers.Add(new BarberService {BarberId = id});
    }
}
