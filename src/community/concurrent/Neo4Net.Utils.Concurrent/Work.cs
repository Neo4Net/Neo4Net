/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Utils.Concurrent
{
   /// <summary>
   /// <para>
   /// A unit of work that can be applied to the given type of material, or combined with other like-typed units of
   /// work.
   /// </para>
   /// <para>
   /// These types of work must exhibit a number of properties, for their use in the WorkSync to be correct:
   /// </para>
   /// <ul>
   /// <li>
   /// <strong>Commutativity</strong><br>
   /// The order of operands must not matter:
   /// <code>a.combine(b)  =  b.combine(a)</code>
   /// </li>
   /// <li>
   /// <strong>Associativity</strong><br>
   /// The order of operations must not matter:
   /// <code>a.combine(b.combine(c))  =  a.combine(b).combine(c)</code>
   /// </li>
   /// <li>
   /// <strong>Effect Transitivity</strong><br>
   /// Work-combining must not change work outcome:
   /// <code>a.combine(b).apply(m)  =  { a.apply(m) ; b.apply(m) }</code>
   /// </li>
   /// </ul>
   /// </summary>
   /// @param <Material> The type of material to work with. </param>
   /// @param <W> The concrete type of work being performed. </param>
   /// <seealso cref= WorkSync </seealso>
   public interface Work<Material, W> where W : Work<Material, W>
   {
      /// <summary>
      /// <para>
      /// Combine this unit of work with the given unit of work, and produce a unit of work that represents the
      /// aggregate of the work.
      /// </para>
      /// <para>
      /// It is perfectly fine for a unit to build up internal state that aggregates the work it is combined with,
      /// and then return itself. This is perhaps useful for reducing the allocation rate a little.
      /// </para>
      /// </summary>
      W Combine(W work);

      /// <summary>
      /// Apply this unit of work to the given material.
      /// </summary>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: void apply(Material material) throws Exception;
      void Apply(Material material);
   }
}