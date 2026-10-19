using System.Globalization;
using Microsoft.EntityFrameworkCore;
using KnyazBarber.Api.Dtos.Barber;
using KnyazBarber.Api.Entities;
using KnyazBarber.Api.Exceptions;
using KnyazBarber.Api.Helpers;
using KnyazBarber.Api.Repositories;

namespace KnyazBarber.Api.Services;

public class BarberManagementService(
    IBaseRepository<Barber> barbers,
    IBaseRepository<Service> services,
    IBaseRepository<Appointment> appointments,
    IFileUploadService files,
    BusinessClock clock) : IBarberManagementService
{
    public async Task<List<BarberAdminDto>> ListAsync(CancellationToken ct) =>
        (await Full().AsNoTracking().OrderBy(b => b.SortOrder).ThenBy(b => b.Id).ToListAsync(ct)).Select(b => b.ToDto(clock)).ToList();

    public async Task<BarberAdminDto> CreateAsync(BarberInput input, CancellationToken ct)
    {
        var b = new Barber();
        await ApplyAsync(b, input, ct);
        // Yangi usta uchun standart jadval: har kuni 11:00–23:00, agar berilmagan bo'lsa
        if (b.WorkingHours.Count == 0)
            foreach (var d in Enum.GetValues<DayOfWeek>())
                b.WorkingHours.Add(new WorkingHours {DayOfWeek = d, Start = new TimeOnly(11, 0), End = new TimeOnly(23, 0)});
        await barbers.AddAsync(b, ct);
        return b.ToDto(clock);
    }

    public async Task<BarberAdminDto> UpdateAsync(int id, BarberInput input, CancellationToken ct)
    {
        var b = await FindAsync(id, ct);
        await ApplyAsync(b, input, ct);
        await barbers.SaveChangesAsync(ct);
        return b.ToDto(clock);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var b = await FindAsync(id, ct);
        if (await appointments.QueryNoTracking().AnyAsync(a => a.BarberId == id, ct))
        {
            b.IsActive = false;
            await barbers.SaveChangesAsync(ct);
            return;
        }
        // Ish rasmlari bazadan kaskad bilan o'chadi, fayllarini esa o'zimiz o'chiramiz
        var works = await barbers.QueryNoTracking().Where(x => x.Id == id).SelectMany(x => x.Works.Select(w => w.Url)).ToListAsync(ct);
        files.DeleteImage(b.PhotoUrl);
        await barbers.DeleteAsync(b, ct);
        works.ForEach(files.DeleteImage);
    }

    public async Task<BarberAdminDto> UploadPhotoAsync(int id, IFormFile file, CancellationToken ct)
    {
        var b = await FindAsync(id, ct);
        var url = await files.SaveImageAsync(file, ct);
        files.DeleteImage(b.PhotoUrl);
        b.PhotoUrl = url;
        await barbers.SaveChangesAsync(ct);
        return b.ToDto(clock);
    }

    public async Task<BarberAdminDto> DeletePhotoAsync(int id, CancellationToken ct)
    {
        var b = await FindAsync(id, ct);
        files.DeleteImage(b.PhotoUrl);
        b.PhotoUrl = null;
        await barbers.SaveChangesAsync(ct);
        return b.ToDto(clock);
    }

    public async Task<BarberAdminDto> AddTimeOffAsync(int id, TimeOffInput input, CancellationToken ct)
    {
        var b = await FindAsync(id, ct);
        if (input.To <= input.From) throw new BadRequestException("Tugash vaqti boshlanishdan keyin bo'lishi kerak");

        b.TimeOffs.Add(new TimeOff
        {
            FromUtc = SlotCalculator.ToUtc(DateOnly.FromDateTime(input.From), TimeOnly.FromDateTime(input.From), clock.Zone),
            ToUtc = SlotCalculator.ToUtc(DateOnly.FromDateTime(input.To), TimeOnly.FromDateTime(input.To), clock.Zone),
            Reason = string.IsNullOrWhiteSpace(input.Reason) ? null : input.Reason.Trim(),
        });
        await barbers.SaveChangesAsync(ct);
        return b.ToDto(clock);
    }

    public async Task<BarberAdminDto> DeleteTimeOffAsync(int id, int timeOffId, CancellationToken ct)
    {
        var b = await FindAsync(id, ct);
        b.TimeOffs.RemoveAll(t => t.Id == timeOffId);
        await barbers.SaveChangesAsync(ct);
        return b.ToDto(clock);
    }

    private IQueryable<Barber> Full() =>
        barbers.Query().Include(b => b.Services).Include(b => b.WorkingHours).Include(b => b.TimeOffs);

    private async Task<Barber> FindAsync(int id, CancellationToken ct) =>
        await Full().FirstOrDefaultAsync(x => x.Id == id, ct) ?? throw new NotFoundException("Usta topilmadi");

    private async Task ApplyAsync(Barber b, BarberInput input, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(input.Name)) throw new BadRequestException("Ism majburiy");
        b.Name = input.Name.Trim();
        b.Title = input.Title.Clean(100);
        b.Bio = input.Bio.Clean(1000);
        b.SortOrder = input.SortOrder;
        b.IsActive = input.IsActive;

        if (input.ServiceIds is not null)
        {
            var wanted = input.ServiceIds.Distinct().ToList();
            var valid = await services.QueryNoTracking().Where(s => wanted.Contains(s.Id)).Select(s => s.Id).ToListAsync(ct);
            b.Services.RemoveAll(s => !valid.Contains(s.ServiceId));
            foreach (var sid in valid.Where(sid => b.Services.All(s => s.ServiceId != sid)))
                b.Services.Add(new BarberService {ServiceId = sid});
        }

        if (input.Hours is not null)
        {
            foreach (var h in input.Hours)
            {
                if (!TimeOnly.TryParseExact(h.Start, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var start) ||
                    !TimeOnly.TryParseExact(h.End, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var end))
                    throw new BadRequestException("Vaqt formati HH:mm bo'lishi kerak");
                if (!h.IsDayOff && end <= start) throw new BadRequestException("Ish tugash vaqti boshlanishdan keyin bo'lishi kerak");

                var row = b.WorkingHours.FirstOrDefault(w => w.DayOfWeek == h.Day);
                if (row is null) b.WorkingHours.Add(row = new WorkingHours {DayOfWeek = h.Day});
                row.Start = start;
                row.End = end;
                row.IsDayOff = h.IsDayOff;
            }
        }
    }
}
