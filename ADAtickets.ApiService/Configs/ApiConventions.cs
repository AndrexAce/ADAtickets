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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace ADAtickets.ApiService.Configs
{
    /// <summary>
    /// Defines a set of methods with annotations (called conventions) that is used to personalize the behavior of the ASP.NET Core API controller.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "The parameters are not meant to be used, they are only needed to estabilish a pattern to apply the conventions.")]
    static class ApiConventions
    {
        /// <summary>
        /// GET api methods convention (single entity).
        /// </summary>
        /// <param name="id">The identifier of the resource to fetch.</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
        public static void Get(
            [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Exact)]
            [ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.AssignableFrom)]

            Guid id)
        {
            // This method is intentionally left empty. The attributes above are used to define the convention.
        }

        /// <summary>
        /// GET api methods convention (all entities).
        /// </summary>
        /// <param name="filters">A group of key-value pairs defining the property name and value entities should be filtered by.</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
        public static void Get(
            [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Exact)]
            [ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.AssignableFrom)]

            IEnumerable<KeyValuePair<string, string>> filters)
        {
            // This method is intentionally left empty. The attributes above are used to define the convention.
        }

        /// <summary>
        /// GET api methods convention (single entity).
        /// </summary>
        /// <param name="candidateKey">Another possible identifier of the resource to fetch.</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
        public static void Get(
            [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)]
            [ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.AssignableFrom)]

            string candidateKey)
        {
            // This method is intentionally left empty. The attributes above are used to define the convention.
        }

        /// <summary>
        /// POST api methods convention.
        /// </summary>
        /// <param name="model">The resource to create.</param>
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
        public static void Post(
            [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)]
            [ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.AssignableFrom)]
            object model)
        {
            // This method is intentionally left empty. The attributes above are used to define the convention.
        }

        /// <summary>
        /// PUT api methods convention.
        /// </summary>
        /// <param name="id">The identifier of the resource to edit.</param>
        /// <param name="model">The resource to edit.</param>
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
        public static void Put(
            [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Exact)]
            [ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.AssignableFrom)]
            Guid id,

            [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)]
            [ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.AssignableFrom)]
            object model)
        {
            // This method is intentionally left empty. The attributes above are used to define the convention.
        }

        /// <summary>
        /// DELETE api methods convention.
        /// </summary>
        /// <param name="id">The identifier of the resource to delete.</param>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
        public static void Delete(
            [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Exact)]
            [ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.AssignableFrom)]
            Guid id)
        {
            // This method is intentionally left empty. The attributes above are used to define the convention.
        }
    }
}
