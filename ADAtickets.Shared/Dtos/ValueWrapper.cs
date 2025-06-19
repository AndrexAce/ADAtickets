/*
 * ADAtickets is a simple, lightweight, open source ticketing system
 * interacting with your enterprise's repositories on Azure DevOps 
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

namespace ADAtickets.Shared.Dtos
{
    /// <summary>
    /// Wraps a value of type <typeparamref name="TValue"/> to allow its use in HTTP exchanges.
    /// </summary>
    /// <typeparam name="TValue">The value type the class contains.</typeparam>
    /// <param name="value">The value to wrap.</param>
    public class ValueWrapper<TValue> where TValue : struct
    {
        private ValueWrapper()
        {
            Value = default;
        }

        public ValueWrapper(TValue value)
        {
            Value = value;
        }

        /// <summary>
        /// The wrapped <typeparamref name="TValue"/>.
        /// </summary>
        public TValue Value { get; set; }
    }
}
