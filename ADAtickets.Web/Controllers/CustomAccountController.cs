/*
 * ADAtickets is a simple, lightweight, open source ticketing system
 * interacting with your enterprise repositories on Azure DevOps 
 * with a two-way synchronization.
 * Copyright (C) 2025  Andrea Lucchese
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
using ADAtickets.Shared.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.UI.Areas.MicrosoftIdentity.Controllers;
using System.Diagnostics.CodeAnalysis;
using Controller = Microsoft.AspNetCore.Mvc.Controller;

namespace ADAtickets.Web.Controllers
{
    /// <summary>
    /// Handles personalized account actions.
    /// </summary>
    /// <param name="configuration"></param>
    [Route("[controller]/[action]")]
    public class CustomAccountController(IConfiguration configuration) : Controller
    {
        /// <summary>
        /// Signs out the user and redirects to the signed-out page.
        /// </summary>
        /// <remarks>The <see cref="AccountController.SignOut(string)"/> method is not used since it does not allow to specify a custom cookie signout scheme.</remarks>
        /// <returns></returns>
        [Experimental("ADAticketsWeb001")]
        [HttpGet]
        public new IActionResult SignOut()
        {
            var aud = HttpContext.User.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid");

            var scheme = aud?.Value == configuration["Entra:TenantId"] ? Scheme.OpenIdConnectDefault : Scheme.ExternalOpenIdConnectDefault;

            return SignOut(
                 scheme == Scheme.OpenIdConnectDefault ? Scheme.CookieDefault : Scheme.ExternalCookieDefault,
                 scheme);
        }
    }
}
