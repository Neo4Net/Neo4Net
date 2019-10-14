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
namespace Neo4Net.@unsafe.Impl.Batchimport.cache
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Long.min;

	/// <summary>
	/// Used as part of the fallback strategy for <seealso cref="Auto"/>. Tries to split up fixed-size arrays
	/// (<seealso cref="newLongArray(long, long)"/> and <seealso cref="newIntArray(long, int)"/> into smaller chunks where
	/// some can live on heap and some off heap.
	/// </summary>
	public class ChunkedNumberArrayFactory : NumberArrayFactory_Adapter
	{
		 internal const int MAGIC_CHUNK_COUNT = 10;
		 // This is a safe bet on the maximum number of items the JVM can store in an array. It is commonly slightly less
		 // than Integer.MAX_VALUE
		 private static readonly int _maxArraySize = int.MaxValue - short.MaxValue;
		 private readonly NumberArrayFactory @delegate;

		 internal ChunkedNumberArrayFactory( NumberArrayFactory_Monitor monitor ) : this( monitor, OFF_HEAP, HEAP )
		 {
		 }

		 internal ChunkedNumberArrayFactory( NumberArrayFactory_Monitor monitor, params NumberArrayFactory[] delegateList )
		 {
			  @delegate = new NumberArrayFactory_Auto( monitor, delegateList );
		 }

		 public override LongArray NewLongArray( long length, long defaultValue, long @base )
		 {
			  // Here we want to have the property of a dynamic array so that some parts of the array
			  // can live on heap, some off.
			  return NewDynamicLongArray( FractionOf( length ), defaultValue );
		 }

		 public override IntArray NewIntArray( long length, int defaultValue, long @base )
		 {
			  // Here we want to have the property of a dynamic array so that some parts of the array
			  // can live on heap, some off.
			  return NewDynamicIntArray( FractionOf( length ), defaultValue );
		 }

		 public override ByteArray NewByteArray( long length, sbyte[] defaultValue, long @base )
		 {
			  // Here we want to have the property of a dynamic array so that some parts of the array
			  // can live on heap, some off.
			  return NewDynamicByteArray( FractionOf( length ), defaultValue );
		 }

		 private long FractionOf( long length )
		 {
			  if ( length < MAGIC_CHUNK_COUNT )
			  {
					return length;
			  }
			  return min( length / MAGIC_CHUNK_COUNT, _maxArraySize );
		 }

		 public override IntArray NewDynamicIntArray( long chunkSize, int defaultValue )
		 {
			  return new DynamicIntArray( @delegate, chunkSize, defaultValue );
		 }

		 public override LongArray NewDynamicLongArray( long chunkSize, long defaultValue )
		 {
			  return new DynamicLongArray( @delegate, chunkSize, defaultValue );
		 }

		 public override ByteArray NewDynamicByteArray( long chunkSize, sbyte[] defaultValue )
		 {
			  return new DynamicByteArray( @delegate, chunkSize, defaultValue );
		 }

		 public override string ToString()
		 {
			  return "ChunkedNumberArrayFactory with delegate " + @delegate;
		 }
	}

}