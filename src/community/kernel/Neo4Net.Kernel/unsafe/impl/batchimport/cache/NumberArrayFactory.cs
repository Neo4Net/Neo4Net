using System;
using System.Collections.Generic;

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

	using Exceptions = Neo4Net.Helpers.Exceptions;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using GlobalMemoryTracker = Neo4Net.Memory.GlobalMemoryTracker;
	using NativeMemoryAllocationRefusedError = Neo4Net.@unsafe.Impl.Internal.Dragons.NativeMemoryAllocationRefusedError;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.Format.bytes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.Numbers.safeCastLongToInt;

	/// <summary>
	/// Factory of <seealso cref="LongArray"/>, <seealso cref="IntArray"/> and <seealso cref="ByteArray"/> instances. Users can select in which type of
	/// memory the arrays will be placed, either in <seealso cref="HEAP"/>, <seealso cref="OFF_HEAP"/>, or use an auto allocator which
	/// will have each instance placed where it fits best, favoring the primary candidates.
	/// </summary>
	public interface NumberArrayFactory
	{

		 /// <summary>
		 /// Puts arrays inside the heap.
		 /// </summary>
	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 NumberArrayFactory HEAP = new Adapter()
	//	 {
	//		  @@Override public IntArray newIntArray(long length, int defaultValue, long @base)
	//		  {
	//				return new HeapIntArray(safeCastLongToInt(length), defaultValue, @base);
	//		  }
	//
	//		  @@Override public LongArray newLongArray(long length, long defaultValue, long @base)
	//		  {
	//				return new HeapLongArray(safeCastLongToInt(length), defaultValue, @base);
	//		  }
	//
	//		  @@Override public ByteArray newByteArray(long length, byte[] defaultValue, long @base)
	//		  {
	//				return new HeapByteArray(safeCastLongToInt(length), defaultValue, @base);
	//		  }
	//
	//		  @@Override public String toString()
	//		  {
	//				return "HEAP";
	//		  }
	//	 };

		 /// <summary>
		 /// Puts arrays off-heap, using unsafe calls.
		 /// </summary>
	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 NumberArrayFactory OFF_HEAP = new Adapter()
	//	 {
	//		  @@Override public IntArray newIntArray(long length, int defaultValue, long @base)
	//		  {
	//				return new OffHeapIntArray(length, defaultValue, @base, GlobalMemoryTracker.INSTANCE);
	//		  }
	//
	//		  @@Override public LongArray newLongArray(long length, long defaultValue, long @base)
	//		  {
	//				return new OffHeapLongArray(length, defaultValue, @base, GlobalMemoryTracker.INSTANCE);
	//		  }
	//
	//		  @@Override public ByteArray newByteArray(long length, byte[] defaultValue, long @base)
	//		  {
	//				return new OffHeapByteArray(length, defaultValue, @base, GlobalMemoryTracker.INSTANCE);
	//		  }
	//
	//		  @@Override public String toString()
	//		  {
	//				return "OFF_HEAP";
	//		  }
	//	 };

		 /// <summary>
		 /// Used as part of the fallback strategy for <seealso cref="Auto"/>. Tries to split up fixed-size arrays
		 /// (<seealso cref="newLongArray(long, long)"/> and <seealso cref="newIntArray(long, int)"/> into smaller chunks where
		 /// some can live on heap and some off heap.
		 /// </summary>

		 /// <summary>
		 /// <seealso cref="Auto"/> factory which uses JVM stats for gathering information about available memory.
		 /// </summary>

		 /// <summary>
		 /// <seealso cref="Auto"/> factory which has a page cache backed number array as final fallback, in order to prevent OOM
		 /// errors. </summary>
		 /// <param name="pageCache"> <seealso cref="PageCache"/> to fallback allocation into, if no more memory is available. </param>
		 /// <param name="dir"> directory where cached files are placed. </param>
		 /// <param name="allowHeapAllocation"> whether or not to allow allocation on heap. Otherwise allocation is restricted
		 /// to off-heap and the page cache fallback. This to be more in control of available space in the heap at all times. </param>
		 /// <param name="monitor"> for monitoring successful and failed allocations and which factory was selected. </param>
		 /// <returns> a <seealso cref="NumberArrayFactory"/> which tries to allocation off-heap, then potentially on heap
		 /// and lastly falls back to allocating inside the given {@code pageCache}. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static NumberArrayFactory auto(org.Neo4Net.io.pagecache.PageCache pageCache, java.io.File dir, boolean allowHeapAllocation, NumberArrayFactory_Monitor monitor)
	//	 {
	//		  PageCachedNumberArrayFactory pagedArrayFactory = new PageCachedNumberArrayFactory(pageCache, dir);
	//		  ChunkedNumberArrayFactory chunkedArrayFactory = new ChunkedNumberArrayFactory(monitor, allocationAlternatives(allowHeapAllocation, pagedArrayFactory));
	//		  return new Auto(monitor, allocationAlternatives(allowHeapAllocation, chunkedArrayFactory));
	//	 }

		 /// <param name="allowHeapAllocation"> whether or not to include heap allocation as an alternative. </param>
		 /// <param name="additional"> other means of allocation to try after the standard off/on heap alternatives. </param>
		 /// <returns> an array of <seealso cref="NumberArrayFactory"/> with the desired alternatives. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static NumberArrayFactory[] allocationAlternatives(boolean allowHeapAllocation, NumberArrayFactory... additional)
	//	 {
	//		  List<NumberArrayFactory> result = new ArrayList<>(Collections.singletonList(OFF_HEAP));
	//		  if (allowHeapAllocation)
	//		  {
	//				result.add(HEAP);
	//		  }
	//		  result.addAll(asList(additional));
	//		  return result.toArray(new NumberArrayFactory[result.size()]);
	//	 }

		 /// <summary>
		 /// Looks at available memory and decides where the requested array fits best. Tries to allocate the whole
		 /// array with the first candidate, falling back to others as needed.
		 /// </summary>

		 /// <param name="length"> size of the array. </param>
		 /// <param name="defaultValue"> value which will represent unset values. </param>
		 /// <returns> a fixed size <seealso cref="IntArray"/>. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default IntArray newIntArray(long length, int defaultValue)
	//	 {
	//		  return newIntArray(length, defaultValue, 0);
	//	 }

		 /// <param name="length"> size of the array. </param>
		 /// <param name="defaultValue"> value which will represent unset values. </param>
		 /// <param name="base"> base index to rebase all requested indexes with. </param>
		 /// <returns> a fixed size <seealso cref="IntArray"/>. </returns>
		 IntArray NewIntArray( long length, int defaultValue, long @base );

		 /// <param name="chunkSize"> the size of each array (number of items). Where new chunks are added when needed. </param>
		 /// <param name="defaultValue"> value which will represent unset values. </param>
		 /// <returns> dynamically growing <seealso cref="IntArray"/>. </returns>
		 IntArray NewDynamicIntArray( long chunkSize, int defaultValue );

		 /// <param name="length"> size of the array. </param>
		 /// <param name="defaultValue"> value which will represent unset values. </param>
		 /// <returns> a fixed size <seealso cref="LongArray"/>. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default LongArray newLongArray(long length, long defaultValue)
	//	 {
	//		  return newLongArray(length, defaultValue, 0);
	//	 }

		 /// <param name="length"> size of the array. </param>
		 /// <param name="defaultValue"> value which will represent unset values. </param>
		 /// <param name="base"> base index to rebase all requested indexes with. </param>
		 /// <returns> a fixed size <seealso cref="LongArray"/>. </returns>
		 LongArray NewLongArray( long length, long defaultValue, long @base );

		 /// <param name="chunkSize"> the size of each array (number of items). Where new chunks are added when needed. </param>
		 /// <param name="defaultValue"> value which will represent unset values. </param>
		 /// <returns> dynamically growing <seealso cref="LongArray"/>. </returns>
		 LongArray NewDynamicLongArray( long chunkSize, long defaultValue );

		 /// <param name="length"> size of the array. </param>
		 /// <param name="defaultValue"> value which will represent unset values. </param>
		 /// <returns> a fixed size <seealso cref="ByteArray"/>. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default ByteArray newByteArray(long length, byte[] defaultValue)
	//	 {
	//		  return newByteArray(length, defaultValue, 0);
	//	 }

		 /// <param name="length"> size of the array. </param>
		 /// <param name="defaultValue"> value which will represent unset values. </param>
		 /// <param name="base"> base index to rebase all requested indexes with. </param>
		 /// <returns> a fixed size <seealso cref="ByteArray"/>. </returns>
		 ByteArray NewByteArray( long length, sbyte[] defaultValue, long @base );

		 /// <param name="chunkSize"> the size of each array (number of items). Where new chunks are added when needed. </param>
		 /// <param name="defaultValue"> value which will represent unset values. </param>
		 /// <returns> dynamically growing <seealso cref="ByteArray"/>. </returns>
		 ByteArray NewDynamicByteArray( long chunkSize, sbyte[] defaultValue );

		 /// <summary>
		 /// Implements the dynamic array methods, because they are the same in most implementations.
		 /// </summary>
	}

	public static class NumberArrayFactory_Fields
	{
		 public static readonly Monitor NoMonitor = ( memory, successfulFactory, attemptedAllocationFailures ) =>
		 {
		 };

		 public static readonly NumberArrayFactory ChunkedFixedSize = new ChunkedNumberArrayFactory( NumberArrayFactory_Fields.NoMonitor );
		 public static readonly NumberArrayFactory AutoWithoutPagecache = new NumberArrayFactory_Auto( NumberArrayFactory_Fields.NoMonitor, OFF_HEAP, HEAP, ChunkedFixedSize );
	}

	 public interface NumberArrayFactory_Monitor
	 {
		  /// <summary>
		  /// Notifies about a successful allocation where information about both successful and failed attempts are included.
		  /// </summary>
		  /// <param name="memory"> amount of memory for this allocation. </param>
		  /// <param name="successfulFactory"> the <seealso cref="NumberArrayFactory"/> which proved successful allocating this amount of memory. </param>
		  /// <param name="attemptedAllocationFailures"> list of failed attempts to allocate this memory in other allocators. </param>
		  void AllocationSuccessful( long memory, NumberArrayFactory successfulFactory, IEnumerable<NumberArrayFactory_AllocationFailure> attemptedAllocationFailures );
	 }

	 public class NumberArrayFactory_AllocationFailure
	 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		  internal readonly Exception FailureConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		  internal readonly NumberArrayFactory FactoryConflict;

		  internal NumberArrayFactory_AllocationFailure( Exception failure, NumberArrayFactory factory )
		  {
				this.FailureConflict = failure;
				this.FactoryConflict = factory;
		  }

		  public virtual Exception Failure
		  {
			  get
			  {
					return FailureConflict;
			  }
		  }

		  public virtual NumberArrayFactory Factory
		  {
			  get
			  {
					return FactoryConflict;
			  }
		  }
	 }

	 public class NumberArrayFactory_Auto : NumberArrayFactory_Adapter
	 {
		  internal readonly NumberArrayFactory_Monitor Monitor;
		  internal readonly NumberArrayFactory[] Candidates;

		  public NumberArrayFactory_Auto( NumberArrayFactory_Monitor monitor, params NumberArrayFactory[] candidates )
		  {
				Objects.requireNonNull( monitor );
				this.Monitor = monitor;
				this.Candidates = candidates;
		  }

		  public override LongArray NewLongArray( long length, long defaultValue, long @base )
		  {
				return TryAllocate( length, 8, f => f.newLongArray( length, defaultValue, @base ) );
		  }

		  public override IntArray NewIntArray( long length, int defaultValue, long @base )
		  {
				return TryAllocate( length, 4, f => f.newIntArray( length, defaultValue, @base ) );
		  }

		  public override ByteArray NewByteArray( long length, sbyte[] defaultValue, long @base )
		  {
				return TryAllocate( length, defaultValue.Length, f => f.newByteArray( length, defaultValue, @base ) );
		  }

		  internal virtual T TryAllocate<T>( long length, int itemSize, System.Func<NumberArrayFactory, T> allocator )
		  {
				IList<NumberArrayFactory_AllocationFailure> failures = new List<NumberArrayFactory_AllocationFailure>();
				Exception error = null;
				foreach ( NumberArrayFactory candidate in Candidates )
				{
					 try
					 {
						  try
						  {
								T array = allocator( candidate );
								Monitor.allocationSuccessful( length * itemSize, candidate, failures );
								return array;
						  }
						  catch ( ArithmeticException e )
						  {
								throw new System.OutOfMemoryException( e.Message );
						  }
					 }
					 catch ( Exception e ) when ( e is System.OutOfMemoryException || e is NativeMemoryAllocationRefusedError )
					 { // Alright let's try the next one
						  if ( error == null )
						  {
								error = e;
						  }
						  else
						  {
								e.addSuppressed( error );
								error = e;
						  }
						  failures.Add( new NumberArrayFactory_AllocationFailure( e, candidate ) );
					 }
				}
				throw error( length, itemSize, error );
		  }

		  internal virtual Exception Error( long length, int itemSize, Exception error )
		  {
				return Exceptions.withMessage( error, format( "%s: Not enough memory available for allocating %s, tried %s", error.Message, bytes( length * itemSize ), Arrays.ToString( Candidates ) ) );
		  }

	 }

	 public abstract class NumberArrayFactory_Adapter : NumberArrayFactory
	 {
		 public abstract ByteArray NewByteArray( long length, sbyte[] defaultValue, long @base );
		 public abstract ByteArray NewByteArray( long length, sbyte[] defaultValue );
		 public abstract LongArray NewLongArray( long length, long defaultValue, long @base );
		 public abstract LongArray NewLongArray( long length, long defaultValue );
		 public abstract IntArray NewIntArray( long length, int defaultValue, long @base );
		 public abstract IntArray NewIntArray( long length, int defaultValue );
		 public abstract NumberArrayFactory[] AllocationAlternatives( bool allowHeapAllocation, params NumberArrayFactory[] additional );
		 public abstract NumberArrayFactory Auto( PageCache pageCache, File dir, bool allowHeapAllocation, NumberArrayFactory_Monitor monitor );
		  public override IntArray NewDynamicIntArray( long chunkSize, int defaultValue )
		  {
				return new DynamicIntArray( this, chunkSize, defaultValue );
		  }

		  public override LongArray NewDynamicLongArray( long chunkSize, long defaultValue )
		  {
				return new DynamicLongArray( this, chunkSize, defaultValue );
		  }

		  public override ByteArray NewDynamicByteArray( long chunkSize, sbyte[] defaultValue )
		  {
				return new DynamicByteArray( this, chunkSize, defaultValue );
		  }
	 }

}