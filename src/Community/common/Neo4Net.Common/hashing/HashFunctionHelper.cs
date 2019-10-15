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
   /// A hash function helper class
   /// </summary>
   /// <seealso cref= #IncrementalXXH64() </seealso>
   /// <seealso cref= #dotnetUtilHashing() </seealso>
   /// <seealso cref= #XorShift32() </seealso>
   public class HashFunctionHelper
   {
      /// <summary>
      /// Our incremental XXH64 based hash function.
      /// <para>
      /// This hash function is based on xxHash (XXH64 variant), but modified to work incrementally on 8-byte blocks
      /// instead of on 32-byte blocks. Basically, the 32-byte block hash loop has been removed, so we use the 8-byte
      /// block tail-loop for the entire input.
      /// </para>
      /// <para>
      /// This hash function is roughly twice as fast as the hash function used for index entries since 2.2.0, about 30%
      /// faster than optimised murmurhash3 implementations though not as fast as optimised xxHash implementations due to
      /// the smaller block size. It is allocation free, unlike its predecessor. And it retains most of the excellent
      /// statistical properties of xxHash, failing only the "TwoBytes" and "Zeroes" keyset tests in SMHasher, passing 12
      /// out of 14 tests. According to <a href="https://twitter.com/Cyan4973/status/899995095549698049">Yann Collet on
      /// twitter</a>, this modification is expected to mostly cause degraded performance, and worsens some of the
      /// avalanche statistics.
      /// </para>
      /// <para>
      /// This hash function is stateless, so the returned instance can be freely cached and accessed concurrently by
      /// multiple threads.
      /// </para>
      /// <para>
      /// The <a href="http://cyan4973.github.io/xxHash/">xxHash</a> algorithm is originally by Yann Collet, and this
      /// implementation is with inspiration from Vsevolod Tolstopyatovs implementation in the
      /// <a href="https://github.com/OpenHFT/Zero-Allocation-Hashing">Zero Allocation Hashing</a> library.
      /// Credit for <a href="https://github.com/aappleby/smhasher">SMHasher</a> goes to Austin Appleby.
      /// </para>
      /// </summary>

      public static IHashFunction IncrementalXXH64()
      {
         return Neo4Net.Hashing.IncrementalXXH64.Instance;
      }

      /// <summary>
      /// Same hash function as that used by the standard library hash collections. It generates a hash by splitting the
      /// input value into segments, and then re-distributing those segments, so the end result is effectively a striped
      /// and then jumbled version of the input data. For randomly distributed keys, this has a good chance at generating
      /// an even hash distribution over the full hash space.
      /// <para>
      /// It performs exceptionally poorly for sequences of numbers, as the sequence increments all end up in the same
      /// stripe, generating hash values that will end up in the same buckets in collections.
      /// </para>
      /// <para>
      /// This hash function is stateless, so the returned instance can be freely cached and accessed concurrently by
      /// multiple threads.
      /// </para>
      /// </summary>
      public static IHashFunction dotnetUtilHashing()
      {
         return dotnetUtilHashFunction.Instance;
      }

      /// <summary>
      /// The default hash function is based on a pseudo-random number generator, which uses the input value as a seed
      /// to the generator. This is very fast, and performs well for most input data. However, it is not guaranteed to
      /// generate a superb distribution, only a "decent" one.
      /// <para>
      /// This hash function is stateless, so the returned instance can be freely cached and accessed concurrently by
      /// multiple threads.
      /// </para>
      /// </summary>
      public static IHashFunction XorShift32()
      {
         return XorShift32HashFunction.Instance;
      }
   }
}