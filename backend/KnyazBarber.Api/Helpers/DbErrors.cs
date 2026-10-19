using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace KnyazBarber.Api.Helpers;

public static class DbErrors
{
    /// <summary>PostgreSQL EXCLUDE cheklovi buzildi — ustada shu vaqtda boshqa navbat bor.</summary>
    public static bool IsExclusionViolation(this DbUpdateException ex) =>
        ex.InnerException is PostgresException {SqlState: PostgresErrorCodes.ExclusionViolation};
}
