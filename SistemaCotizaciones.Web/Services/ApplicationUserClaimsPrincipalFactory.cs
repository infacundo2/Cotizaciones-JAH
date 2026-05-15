using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SistemaCotizaciones.Web.Data.Entities;
using System.Security.Claims;

namespace SistemaCotizaciones.Web.Services;

public class ApplicationUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
{
    public ApplicationUserClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        if (!string.IsNullOrWhiteSpace(user.NombreCompleto))
        {
            identity.AddClaim(new Claim("NombreCompleto", user.NombreCompleto));
        }

        return identity;
    }
}
