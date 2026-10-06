using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Firmeza.API.Problems;

public static class UniqueConstraintHelper
{
    public static bool IsUniqueViolation(DbUpdateException exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (current is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                return true;
            }
        }

        return false;
    }
}