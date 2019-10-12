/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Hashing
{

	/// <summary>
	/// A hash function, as per this interface, will produce a deterministic value based on its input.
	/// <para>
	/// Hash functions are first initialised with a seed, which may be zero, and then updated with a succession of values
	/// that are mixed into the hash state in sequence.
	/// </para>
	/// <para>
	/// Hash functions may have internal state, but can also be stateless, if their complete state can be represented by the
	/// 64-bit intermediate hash state.
	/// 
	/// </para>
	/// </summary>
	/// <seealso cref= #incrementalXXH64() </seealso>
	/// <seealso cref= #javaUtilHashing() </seealso>
	/// <seealso cref= #xorShift32() </seealso>
	public interface HashFunction
	{
		 /// <summary>
		 /// Initialise the hash function with the given seed.
		 /// <para>
		 /// Different seeds should produce different final hash values.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="seed"> The initialisation seed for the hash function. </param>
		 /// <returns> An initialised intermediate hash state. </returns>
		 long Initialise( long seed );

		 /// <summary>
		 /// Update the hash state by mixing the given value into the given intermediate hash state.
		 /// </summary>
		 /// <param name="intermediateHash"> The intermediate hash state given either by <seealso cref="initialise(long)"/>, or by a previous
		 /// call to this function. </param>
		 /// <param name="value"> The value to add to the hash state. </param>
		 /// <returns> a new intermediate hash state with the value mixed in. </returns>
		 long Update( long intermediateHash, long value );

		 /// <summary>
		 /// Produce a final hash value from the given intermediate hash state.
		 /// </summary>
		 /// <param name="intermediateHash"> the intermediate hash state from which to produce a final hash value. </param>
		 /// <returns> the final hash value. </returns>
		 long Finalise( long intermediateHash );

		 /// <summary>
		 /// Reduce the given 64-bit hash value to a 32-bit value.
		 /// </summary>
		 /// <param name="hash"> The hash value to reduce. </param>
		 /// <returns> The 32-bit representation of the given hash value. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default int toInt(long hash)
	//	 {
	//		  return (int)((hash >> 32) ^ hash);
	//	 }

		 /// <summary>
		 /// Produce a 64-bit hash value from a single long value.
		 /// </summary>
		 /// <param name="value"> The single value to hash. </param>
		 /// <returns> The hash of the given value. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default long hashSingleValue(long value)
	//	 {
	//		  return finalise(update(initialise(0), value));
	//	 }

		 /// <summary>
		 /// Produce a 32-bit hash value from a single long value.
		 /// </summary>
		 /// <param name="value"> The single value to hash. </param>
		 /// <returns> The hash of the given value. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default int hashSingleValueToInt(long value)
	//	 {
	//		  return toInt(hashSingleValue(value));
	//	 }

		 /// <summary>
		 /// Update the hash state by mixing in the given array and all of its elements, in order. This even works if the array is null, in which case a special value
		 /// will be mixed in. Each element must be projected to a {@code long} value, which is why a projection function is required.
		 /// </summary>
		 /// <param name="intermediateHash"> The intermediate hash state given either by <seealso cref="initialise(long)"/>, or by a previous call to this or the
		 /// <seealso cref="update(long, long)"/> method. </param>
		 /// <param name="array"> The array whose length and elements should be added to the hash state. </param>
		 /// <param name="projectionToLong"> The mechanism by which each element is transformed into a {@code long} value, prior to being mixed into the hash state. </param>
		 /// @param <T> The type of array elements. </param>
		 /// <returns> the new intermediate hash state with the array mixed in. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default <T> long updateWithArray(long intermediateHash, T[] array, System.Func<T, long> projectionToLong)
	//	 {
	//		  if (array == null)
	//		  {
	//				// Even if the array is null, we still need to permute the hash, so we leave a trace of this step in the hashing.
	//				return update(intermediateHash, -1);
	//		  }
	//
	//		  long hash = update(intermediateHash, array.length);
	//		  for (T obj : array)
	//		  {
	//				hash = update(hash, projectionToLong.applyAsLong(obj));
	//		  }
	//		  return hash;
	//	 }

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
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static HashFunction incrementalXXH64()
	//	 {
	//		  return IncrementalXXH64.INSTANCE;
	//	 }

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
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static HashFunction javaUtilHashing()
	//	 {
	//		  return JavaUtilHashFunction.INSTANCE;
	//	 }

		 /// <summary>
		 /// The default hash function is based on a pseudo-random number generator, which uses the input value as a seed
		 /// to the generator. This is very fast, and performs well for most input data. However, it is not guaranteed to
		 /// generate a superb distribution, only a "decent" one.
		 /// <para>
		 /// This hash function is stateless, so the returned instance can be freely cached and accessed concurrently by
		 /// multiple threads.
		 /// </para>
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static HashFunction xorShift32()
	//	 {
	//		  return XorShift32HashFunction.INSTANCE;
	//	 }
	}

}