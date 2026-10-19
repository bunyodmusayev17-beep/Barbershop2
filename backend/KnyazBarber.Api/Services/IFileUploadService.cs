namespace KnyazBarber.Api.Services;

public interface IFileUploadService
{
    /// <summary>Rasmni saqlaydi va "/uploads/..." manzilini qaytaradi. Noto'g'ri fayl — BadRequestException.</summary>
    Task<string> SaveImageAsync(IFormFile file, CancellationToken ct);

    /// <summary>"/uploads/..." faylini o'chiradi (boshqa manzil bo'lsa hech narsa qilmaydi).</summary>
    void DeleteImage(string? url);
}
