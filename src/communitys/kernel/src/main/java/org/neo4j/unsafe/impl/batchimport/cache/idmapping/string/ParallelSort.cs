using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

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
namespace Neo4Net.@unsafe.Impl.Batchimport.cache.idmapping.@string
{
	using MutableLongStack = org.eclipse.collections.api.stack.primitive.MutableLongStack;
	using LongArrayStack = org.eclipse.collections.impl.stack.mutable.primitive.LongArrayStack;


	using ProgressListener = Neo4Net.Helpers.progress.ProgressListener;
	using CompareType = Neo4Net.@unsafe.Impl.Batchimport.Utils.CompareType;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Numbers.safeCastLongToInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.cache.idmapping.@string.EncodingIdMapper.clearCollision;

	/// <summary>
	/// Sorts input data by dividing up into chunks and sort each chunk in parallel. Each chunk is sorted
	/// using a quick sort method, whereas the dividing of the data is first sorted using radix sort.
	/// </summary>
	public class ParallelSort
	{
		 private readonly int[] _radixIndexCount;
		 private readonly RadixCalculator _radixCalculator;
		 private readonly LongArray _dataCache;
		 private readonly long _highestSetIndex;
		 private readonly Tracker _tracker;
		 private readonly int _threads;
		 private long[][] _sortBuckets;
		 private readonly ProgressListener _progress;
		 private readonly Comparator _comparator;

		 public ParallelSort( Radix radix, LongArray dataCache, long highestSetIndex, Tracker tracker, int threads, ProgressListener progress, Comparator comparator )
		 {
			  this._progress = progress;
			  this._comparator = comparator;
			  this._radixIndexCount = radix.RadixIndexCounts;
			  this._radixCalculator = radix.Calculator();
			  this._dataCache = dataCache;
			  this._highestSetIndex = highestSetIndex;
			  this._tracker = tracker;
			  this._threads = threads;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized long[][] run() throws InterruptedException
		 public virtual long[][] Run()
		 {
			 lock ( this )
			 {
				  long[][] sortParams = SortRadix();
				  int threadsNeeded = 0;
				  for ( int i = 0; i < _threads; i++ )
				  {
						if ( sortParams[i][1] == 0 )
						{
							 break;
						}
						threadsNeeded++;
				  }
      
				  Workers<SortWorker> sortWorkers = new Workers<SortWorker>( "SortWorker" );
				  _progress.started( "SORT" );
				  for ( int i = 0; i < threadsNeeded; i++ )
				  {
						if ( sortParams[i][1] == 0 )
						{
							 break;
						}
						sortWorkers.Start( new SortWorker( this, sortParams[i][0], sortParams[i][1] ) );
				  }
				  try
				  {
						sortWorkers.AwaitAndThrowOnError();
				  }
				  finally
				  {
						_progress.done();
				  }
				  return _sortBuckets;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long[][] sortRadix() throws InterruptedException
		 private long[][] SortRadix()
		 {
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: long[][] rangeParams = new long[_threads][2];
			  long[][] rangeParams = RectangularArrays.RectangularLongArray( _threads, 2 );
			  int[] bucketRange = new int[_threads];
			  Workers<TrackerInitializer> initializers = new Workers<TrackerInitializer>( "TrackerInitializer" );
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: _sortBuckets = new long[_threads][2];
			  _sortBuckets = RectangularArrays.RectangularLongArray( _threads, 2 );
			  long dataSize = _highestSetIndex + 1;
			  long bucketSize = dataSize / _threads;
			  long count = 0;
			  long fullCount = 0;
			  _progress.started( "SPLIT" );
			  for ( int i = 0, threadIndex = 0; i < _radixIndexCount.Length && threadIndex < _threads; i++ )
			  {
					if ( ( count + _radixIndexCount[i] ) > bucketSize )
					{
						 bucketRange[threadIndex] = count == 0 ? i : i - 1;
						 rangeParams[threadIndex][0] = fullCount;
						 if ( count != 0 )
						 {
							  rangeParams[threadIndex][1] = count;
							  fullCount += count;
							  _progress.add( count );
							  count = _radixIndexCount[i];
						 }
						 else
						 {
							  rangeParams[threadIndex][1] = _radixIndexCount[i];
							  fullCount += _radixIndexCount[i];
							  _progress.add( _radixIndexCount[i] );
						 }
						 initializers.Start( new TrackerInitializer( this, threadIndex, rangeParams[threadIndex], threadIndex > 0 ? bucketRange[threadIndex - 1] : -1, bucketRange[threadIndex], _sortBuckets[threadIndex] ) );
						 threadIndex++;
					}
					else
					{
						 count += _radixIndexCount[i];
					}
					if ( threadIndex == _threads - 1 || i == _radixIndexCount.Length - 1 )
					{
						 bucketRange[threadIndex] = _radixIndexCount.Length;
						 rangeParams[threadIndex][0] = fullCount;
						 rangeParams[threadIndex][1] = dataSize - fullCount;
						 initializers.Start( new TrackerInitializer( this, threadIndex, rangeParams[threadIndex], threadIndex > 0 ? bucketRange[threadIndex - 1] : -1, bucketRange[threadIndex], _sortBuckets[threadIndex] ) );
						 break;
					}
			  }
			  _progress.done();

			  // In the loop above where we split up radixes into buckets, we start one thread per bucket whose
			  // job is to populate trackerCache and sortBuckets where each thread will not touch the same
			  // data indexes as any other thread. Here we wait for them all to finish.
			  Exception error = initializers.Await();
			  long[] bucketIndex = new long[_threads];
			  int i = 0;
			  foreach ( TrackerInitializer initializer in initializers )
			  {
					bucketIndex[i++] = initializer.BucketIndex;
			  }
			  if ( error != null )
			  {
					throw new AssertionError( error.Message + "\n" + DumpBuckets( rangeParams, bucketRange, bucketIndex ), error );
			  }
			  return rangeParams;
		 }

		 private string DumpBuckets( long[][] rangeParams, int[] bucketRange, long[] bucketIndex )
		 {
			  StringBuilder builder = new StringBuilder();
			  builder.Append( "rangeParams:\n" );
			  foreach ( long[] range in rangeParams )
			  {
					builder.Append( "  " ).Append( Arrays.ToString( range ) ).Append( "\n" );
			  }
			  builder.Append( "bucketRange:\n" );
			  foreach ( int range in bucketRange )
			  {
					builder.Append( "  " ).Append( range ).Append( "\n" );
			  }
			  builder.Append( "bucketIndex:\n" );
			  foreach ( long index in bucketIndex )
			  {
					builder.Append( "  " ).Append( index ).Append( "\n" );
			  }
			  return builder.ToString();
		 }

		 /// <summary>
		 /// Pluggable comparator for the comparisons that quick-sort needs in order to function.
		 /// </summary>
		 public interface Comparator
		 {
			  /// <returns> {@code true} if {@code left} is less than {@code pivot}. </returns>
			  bool Lt( long left, long pivot );

			  /// <returns> {@code true} if {@code right} is greater than or equal to {@code pivot}. </returns>
			  bool Ge( long right, long pivot );

			  /// <param name="dataValue"> the data value in the used dataCache for a given tracker index. </param>
			  /// <returns> actual data value given the data value retrieved from the dataCache at a given index.
			  /// This is exposed to be able to introduce an indirection while preparing the tracker indexes
			  /// just like the other methods on this interface does. </returns>
			  long DataValue( long dataValue );
		 }

		 public static readonly Comparator DEFAULT = new ComparatorAnonymousInnerClass();

		 private class ComparatorAnonymousInnerClass : Comparator
		 {
			 public bool lt( long left, long pivot )
			 {
				  return Utils.unsignedCompare( left, pivot, Utils.CompareType.LT );
			 }

			 public bool ge( long right, long pivot )
			 {
				  return Utils.unsignedCompare( right, pivot, Utils.CompareType.GE );
			 }

			 public long dataValue( long dataValue )
			 {
				  return dataValue;
			 }
		 }

		 /// <summary>
		 /// Sorts a part of data in dataCache covered by trackerCache. Values in data cache doesn't change location,
		 /// instead trackerCache is updated to point to the right indexes. Only touches a designated part of trackerCache
		 /// so that many can run in parallel on their own part without synchronization.
		 /// </summary>
		 private class SortWorker : ThreadStart
		 {
			 private readonly ParallelSort _outerInstance;

			  internal readonly long Start;
			  internal readonly long Size;
			  internal int ThreadLocalProgress;
			  internal readonly long[] PivotChoice = new long[10];
			  internal readonly ThreadLocalRandom Random = ThreadLocalRandom.current();

			  internal SortWorker( ParallelSort outerInstance, long startRange, long size )
			  {
				  this._outerInstance = outerInstance;
					this.Start = startRange;
					this.Size = size;
			  }

			  internal virtual void IncrementProgress( long diff )
			  {
					ThreadLocalProgress += ( int )diff;
					if ( ThreadLocalProgress >= 10_000 )
					{ // Update the total progress
						 ReportProgress();
					}
			  }

			  internal virtual void ReportProgress()
			  {
					outerInstance.progress.Add( ThreadLocalProgress );
					ThreadLocalProgress = 0;
			  }

			  public override void Run()
			  {
					Qsort( Start, Start + Size );
					ReportProgress();
			  }

			  internal virtual long Partition( long leftIndex, long rightIndex, long pivotIndex )
			  {
					long li = leftIndex;
					long ri = rightIndex - 2;
					long pi = pivotIndex;
					long pivot = clearCollision( outerInstance.dataCache.Get( outerInstance.tracker.Get( pi ) ) );
					// save pivot in last index
					outerInstance.tracker.Swap( pi, rightIndex - 1 );
					long left = clearCollision( outerInstance.dataCache.Get( outerInstance.tracker.Get( li ) ) );
					long right = clearCollision( outerInstance.dataCache.Get( outerInstance.tracker.Get( ri ) ) );
					while ( li < ri )
					{
						 if ( outerInstance.comparator.Lt( left, pivot ) )
						 { // this value is on the correct side of the pivot, moving on
							  left = clearCollision( outerInstance.dataCache.Get( outerInstance.tracker.Get( ++li ) ) );
						 }
						 else if ( outerInstance.comparator.Ge( right, pivot ) )
						 { // this value is on the correct side of the pivot, moving on
							  right = clearCollision( outerInstance.dataCache.Get( outerInstance.tracker.Get( --ri ) ) );
						 }
						 else
						 { // this value is on the wrong side of the pivot, swapping
							  outerInstance.tracker.Swap( li, ri );
							  long temp = left;
							  left = right;
							  right = temp;
						 }
					}
					long partingIndex = ri;
					if ( outerInstance.comparator.Lt( right, pivot ) )
					{
						 partingIndex++;
					}
					// restore pivot
					outerInstance.tracker.Swap( rightIndex - 1, partingIndex );
					return partingIndex;
			  }

			  internal virtual void Qsort( long initialStart, long initialEnd )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.stack.primitive.MutableLongStack stack = new org.eclipse.collections.impl.stack.mutable.primitive.LongArrayStack();
					MutableLongStack stack = new LongArrayStack();
					stack.push( initialStart );
					stack.push( initialEnd );
					while ( !stack.Empty )
					{
						 long end = stack.Empty ? -1 : stack.pop();
						 long start = stack.Empty ? -1 : stack.pop();
						 long diff = end - start;
						 if ( diff < 2 )
						 {
							  IncrementProgress( 2 );
							  continue;
						 }

						 IncrementProgress( 1 );

						 // choose a random pivot between start and end
						 long pivot = start + Random.nextLong( diff );
						 pivot = InformedPivot( start, end, pivot );

						 // partition, given that pivot
						 pivot = Partition( start, end, pivot );
						 if ( pivot > start )
						 { // there are elements to left of pivot
							  stack.push( start );
							  stack.push( pivot );
						 }
						 if ( pivot + 1 < end )
						 { // there are elements to right of pivot
							  stack.push( pivot + 1 );
							  stack.push( end );
						 }
					}
			  }

			  internal virtual long InformedPivot( long start, long end, long randomIndex )
			  {
					if ( end - start < PivotChoice.Length )
					{
						 return randomIndex;
					}

					long low = Math.Max( start, randomIndex - 5 );
					long high = Math.Min( low + 10, end );
					int length = safeCastLongToInt( high - low );

					int j = 0;
					for ( long i = low; i < high; i++, j++ )
					{
						 PivotChoice[j] = clearCollision( outerInstance.dataCache.Get( outerInstance.tracker.Get( i ) ) );
					}
					Arrays.sort( PivotChoice, 0, length );

					long middle = PivotChoice[length / 2];
					for ( long i = low; i <= high; i++ )
					{
						 if ( clearCollision( outerInstance.dataCache.Get( outerInstance.tracker.Get( i ) ) ) == middle )
						 {
							  return i;
						 }
					}
					throw new System.InvalidOperationException( "The middle value somehow disappeared in front of our eyes" );
			  }
		 }

		 /// <summary>
		 /// Sets the initial tracker indexes pointing to data indexes. Only touches a designated part of trackerCache
		 /// so that many can run in parallel on their own part without synchronization.
		 /// </summary>
		 private class TrackerInitializer : ThreadStart
		 {
			 private readonly ParallelSort _outerInstance;

			  internal readonly long[] RangeParams;
			  internal readonly int LowRadixRange;
			  internal readonly int HighRadixRange;
			  internal readonly int ThreadIndex;
			  internal long BucketIndex;
			  internal readonly long[] Result;

			  internal TrackerInitializer( ParallelSort outerInstance, int threadIndex, long[] rangeParams, int lowRadixRange, int highRadixRange, long[] result )
			  {
				  this._outerInstance = outerInstance;
					this.ThreadIndex = threadIndex;
					this.RangeParams = rangeParams;
					this.LowRadixRange = lowRadixRange;
					this.HighRadixRange = highRadixRange;
					this.Result = result;
			  }

			  public override void Run()
			  {
					for ( long i = 0; i <= outerInstance.highestSetIndex; i++ )
					{
						 int rIndex = outerInstance.radixCalculator.RadixOf( outerInstance.comparator.DataValue( outerInstance.dataCache.Get( i ) ) );
						 if ( rIndex > LowRadixRange && rIndex <= HighRadixRange )
						 {
							  long trackerIndex = RangeParams[0] + BucketIndex++;
							  Debug.Assert( outerInstance.tracker.Get( trackerIndex ) == -1, "Overlapping buckets i:" + i + ", k:" + ThreadIndex + ", index:" + trackerIndex );
							  outerInstance.tracker.Set( trackerIndex, i );
							  if ( BucketIndex == RangeParams[1] )
							  {
									Result[0] = HighRadixRange;
									Result[1] = RangeParams[0];
							  }
						 }
					}
			  }
		 }
	}

}