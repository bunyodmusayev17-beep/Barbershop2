using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using KnyazBarber.Api.Entities;
using KnyazBarber.Api.Exceptions;
using KnyazBarber.Api.Repositories;
using KnyazBarber.Api.Services.Telegram;

namespace KnyazBarber.Api.Helpers;

/// <summary>
/// Konsol buyruqlari (sayt ishga tushmaydi, buyruq bajarilib dastur tugaydi):
///   reset-password &lt;login&gt;            — parolni yangilash (yashirin kiritiladi)
///   create-admin &lt;login&gt;              — prod'da birinchi admin (login topilmasa yaratiladi)
///   set-telegram &lt;login&gt; &lt;@username&gt;  — xodimning Telegram'i (yoki raqamli ID)
/// </summary>
public static class CliCommands
{
    /// <returns>true — buyruq bajarildi, sayt ishga tushirilmasin.</returns>
    public static async Task<bool> TryRunAsync(WebApplication app, string[] args)
    {
        switch (args)
        {
            case [var command and ("reset-password" or "create-admin"), var login, ..]:
                await SetPasswordAsync(app.Services, login, createAdmin: command == "create-admin");
                return true;
            case ["set-telegram", var login, var handle, ..]:
                await SetTelegramAsync(app.Services, login, handle);
                return true;
            default:
                return false;
        }
    }

    private static async Task SetPasswordAsync(IServiceProvider services, string login, bool createAdmin)
    {
        await using var scope = services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<IBaseRepository<User>>();
        login = login.Trim().ToLowerInvariant();
        var user = await users.Query().FirstOrDefaultAsync(u => u.Login == login);
        if (user is null && createAdmin)
        {
            if (login.Length is < 3 or > 50) { Console.WriteLine("Login 3–50 belgi bo'lishi kerak."); return; }
            user = new User {Login = login, Role = UserRole.Admin};
            users.Add(user);
            Console.WriteLine($"Yangi admin akkaunti: '{login}'.");
        }
        if (user is null)
        {
            Console.WriteLine($"'{login}' loginli foydalanuvchi topilmadi. Mavjudlari: {string.Join(", ", await users.QueryNoTracking().Select(u => u.Login).ToListAsync())}");
            return;
        }

        Console.Write($"'{user.Login}' uchun yangi parol (kamida 8 belgi): ");
        var first = ReadHidden();
        Console.Write("Parolni qayta kiriting: ");
        var second = ReadHidden();

        if (first.Length < 8) { Console.WriteLine("Parol juda qisqa — o'zgartirilmadi."); return; }
        if (first != second) { Console.WriteLine("Parollar mos kelmadi — o'zgartirilmadi."); return; }

        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, first);
        user.IsActive = true;
        await users.SaveChangesAsync(default);
        Console.WriteLine($"Tayyor: '{user.Login}' paroli yangilandi. Endi shu parol bilan kirishingiz mumkin.");
    }

    private static async Task SetTelegramAsync(IServiceProvider services, string login, string handle)
    {
        await using var scope = services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<IBaseRepository<User>>();
        var user = await users.Query().FirstOrDefaultAsync(u => u.Login == login.Trim().ToLowerInvariant());
        if (user is null) { Console.WriteLine($"'{login}' topilmadi"); return; }
        try
        {
            await scope.ServiceProvider.GetRequiredService<ITelegramAccountService>().ApplyAsync(user, handle, default);
        }
        catch (BadRequestException ex)
        {
            Console.WriteLine(ex.Message);
            return;
        }
        await users.SaveChangesAsync(default);
        Console.WriteLine($"Tayyor: {user.Login} → {TelegramAccountService.Display(user)}. Endi u botga /start yozsin.");
    }

    private static string ReadHidden()
    {
        var sb = new StringBuilder();
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter) break;
            if (key.Key == ConsoleKey.Backspace)
            {
                if (sb.Length == 0) continue;
                sb.Length--;
                Console.Write("\b \b");
            }
            else if (!char.IsControl(key.KeyChar))
            {
                sb.Append(key.KeyChar);
                Console.Write('*');
            }
        }
        Console.WriteLine();
        return sb.ToString();
    }
}
