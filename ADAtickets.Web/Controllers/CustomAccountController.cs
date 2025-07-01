using ADAtickets.Shared.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace ADAtickets.Web.Controllers
{
    [Route("[controller]/[action]")]
    public class CustomAccountController(IConfiguration configuration) : Microsoft.AspNetCore.Mvc.Controller
    {
        [HttpGet]
        public new IActionResult SignOut()
        {
            var aud = HttpContext.User.FindFirst("aud");

            var scheme = aud?.Value == configuration["Entra:TenantId"] ? Scheme.OpenIdConnectDefault : Scheme.ExternalOpenIdConnectDefault;

            var callbackUrl = Url.Page("/Account/SignedOut", pageHandler: null, values: null, protocol: Request.Scheme);
            return SignOut(
                 new AuthenticationProperties
                 {
                     RedirectUri = callbackUrl,
                 },
                 scheme == Scheme.OpenIdConnectDefault ? Scheme.CookieDefault : Scheme.ExternalCookieDefault,
                 scheme);
        }
    }
}
