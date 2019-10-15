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

namespace Neo4Net.Hashing
{
   /// <summary>
   /// A hash function, as per this interface, will produce a deterministic value based on its input.
   /// <para>
   /// Hash functions are first initialized with a seed, which may be zero, and then updated with a succession of values
   /// that are mixed into the hash state in sequence.
   /// </para>
   /// <para>
   /// Hash functions may have internal state, but can also be stateless, if their complete state can be represented by the
   /// 64-bit intermediate hash state.
   ///
   /// </para>
   /// </summary>
   /// <seealso cref= #IncrementalXXH64() </seealso>
   /// <seealso cref= #dotnetUtilHashing() </seealso>
   /// <seealso cref= #XorShift32() </seealso>
   public interface IHashFunction
   {
      /// <summary>
      /// Initialize the hash function with the given seed.
      /// <para>
      /// Different seeds should produce different final hash values.
      ///
      /// </para>
      /// </summary>
      /// <param name="seed"> The initialization seed for the hash function. </param>
      /// <returns> An initialized intermediate hash state. </returns>
      long Initialize(long seed);

      /// <summary>
      /// Update the hash state by mixing the given value into the given intermediate hash state.
      /// </summary>
      /// <param name="intermediateHash"> The intermediate hash state given either by <seealso cref="initialise(long)"/>, or by a previous
      /// call to this function. </param>
      /// <param name="value"> The value to add to the hash state. </param>
      /// <returns> a new intermediate hash state with the value mixed in. </returns>
      long Update(long intermediateHash, long value);

      /// <summary>
      /// Produce a final hash value from the given intermediate hash state.
      /// </summary>
      /// <param name="intermediateHash"> the intermediate hash state from which to produce a final hash value. </param>
      /// <returns> the final hash value. </returns>
      long Finalize(long intermediateHash);
   }
}