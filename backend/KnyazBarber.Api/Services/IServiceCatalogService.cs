using KnyazBarber.Api.Dtos.Service;

namespace KnyazBarber.Api.Services;

/// <summary>Sartaroshxona xizmatlari ro'yxati (narx, davomiylik, qaysi ustalar bajaradi).</summary>
public interface IServiceCatalogService
{
    Task<List<ServiceAdminDto>> ListAsync(CancellationToken ct);
    Task<ServiceAdminDto> CreateAsync(ServiceInput input, CancellationToken ct);
    Task<ServiceAdminDto> UpdateAsync(int id, ServiceInput input, CancellationToken ct);
    /// <summary>Navbatlari bor xizmat o'chirilmaydi, faqat nofaol qilinadi (tarix saqlanishi uchun).</summary>
    Task DeleteAsync(int id, CancellationToken ct);
}
