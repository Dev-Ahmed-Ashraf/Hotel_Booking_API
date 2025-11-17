using System.Security.Claims;

namespace Hotel_Booking.Infrastructure.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Gets the UserId claim from the authenticated user.
        /// </summary>
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var idClaim = user.FindFirst(ClaimTypes.NameIdentifier);

            if (idClaim == null)
                throw new UnauthorizedAccessException("UserId claim is missing from token.");

            return int.Parse(idClaim.Value);
        }

        /// <summary>
        /// Returns the user's email from the JWT Token.
        /// </summary>
        public static string GetEmail(this ClaimsPrincipal user)
        {
            var claim = user.FindFirst(ClaimTypes.Email);

            if (claim == null)
                throw new UnauthorizedAccessException("Email claim is missing from token.");

            return claim.Value;
        }

        /// <summary>
        /// Returns the user's full name from the JWT Token.
        /// </summary>
        public static string GetFullName(this ClaimsPrincipal user)
        {
            var claim = user.FindFirst(ClaimTypes.Name);

            if (claim == null)
                throw new UnauthorizedAccessException("Name claim is missing from token.");

            return claim.Value;
        }

        /// <summary>
        /// Returns the user's role from the JWT Token (Admin, Customer, etc).
        /// </summary>
        public static string GetRole(this ClaimsPrincipal user)
        {
            var claim = user.FindFirst(ClaimTypes.Role);

            if (claim == null)
                throw new UnauthorizedAccessException("Role claim is missing from token.");

            return claim.Value;
        }

        /// <summary>
        /// Checks if the authenticated user has the Admin role.
        /// </summary>
        public static bool IsAdmin(this ClaimsPrincipal user)
        {
            return user.GetRole().Equals("Admin", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks if the authenticated user has the Customer role.
        /// </summary>
        public static bool IsCustomer(this ClaimsPrincipal user)
        {
            return user.GetRole().Equals("Customer", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns any claim value safely; throws if missing.
        /// </summary>
        public static string GetClaimValue(this ClaimsPrincipal user, string key)
        {
            var claim = user.FindFirst(key);

            if (claim == null)
                throw new UnauthorizedAccessException($"Claim '{key}' is missing from token.");

            return claim.Value;
        }
    }
}
