using System.Security.Claims;
using KnowledgeBase.Identity.Api.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace KnowledgeBase.Identity.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly SignInManager<ApplicationUser> signInManager;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
    }

    /// <summary>
    /// Initiates the Google OAuth login flow.
    /// </summary>
    [HttpGet("login/google")]
    public IActionResult LoginGoogle([FromQuery] string? returnUrl = null)
    {
        var properties = signInManager.ConfigureExternalAuthenticationProperties(
            GoogleDefaults.AuthenticationScheme,
            Url.Action(nameof(GoogleCallback), new { returnUrl }));

        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Google redirect callback — creates or updates the local user and issues
    /// an OpenIddict access token.
    /// </summary>
    [HttpGet("callback/google")]
    public async Task<IActionResult> GoogleCallback([FromQuery] string? returnUrl = null)
    {
        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info is null)
        {
            return BadRequest("External login info not available.");
        }

        var email = info.Principal.FindFirstValue(ClaimTypes.Email)
            ?? info.Principal.FindFirstValue("email");
        if (string.IsNullOrEmpty(email))
        {
            return BadRequest("Email claim is missing from Google response.");
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                DisplayName = info.Principal.FindFirstValue(ClaimTypes.Name) ?? email,
                PictureUrl = info.Principal.FindFirstValue("picture")
            };

            var createResult = await userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                return BadRequest(createResult.Errors);
            }

            await userManager.AddLoginAsync(user, info);
            await userManager.AddToRoleAsync(user, KnowledgeBase.Auth.Roles.Employee);
        }
        else
        {
            user.DisplayName = info.Principal.FindFirstValue(ClaimTypes.Name) ?? user.DisplayName;
            user.PictureUrl = info.Principal.FindFirstValue("picture") ?? user.PictureUrl;
            await userManager.UpdateAsync(user);
        }

        var roles = await userManager.GetRolesAsync(user);

        var identity = new ClaimsIdentity(
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            Claims.Name,
            Claims.Role);

        identity.SetClaim(Claims.Subject, user.Id);
        identity.SetClaim(Claims.Name, user.DisplayName);
        identity.SetClaim(Claims.Email, user.Email);
        identity.SetClaim("picture", user.PictureUrl ?? string.Empty);

        foreach (var role in roles)
        {
            identity.AddClaim(Claims.Role, role);
        }

        identity.SetDestinations(claim => claim.Type switch
        {
            Claims.Name or Claims.Email or Claims.Role or "picture"
                => [Destinations.AccessToken, Destinations.IdentityToken],
            _ => [Destinations.AccessToken]
        });

        var principal = new ClaimsPrincipal(identity);

        principal.SetScopes(
            Scopes.OpenId,
            Scopes.Profile,
            Scopes.Email,
            Scopes.Roles);

        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
}
