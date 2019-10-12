using System;
using System.Diagnostics;
using System.Threading;

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
namespace Neo4Net.@unsafe.Impl.Batchimport.staging
{

	using StatsProvider = Neo4Net.@unsafe.Impl.Batchimport.stats.StatsProvider;
	using UnsafeUtil = Neo4Net.@unsafe.Impl.@internal.Dragons.UnsafeUtil;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Integer.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Integer.min;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.nanoTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.getFieldOffset;

	/// <summary>
	/// Executes batches by multiple threads. Each threads only processes its own part, e.g. based on node id,
	/// of a batch such that all threads together fully processes the entire batch.
	/// This is a very useful technique when the code processing a batch uses data structures that aren't,
	/// or cannot trivially or efficiently be synchronized and access order, e.g. per node id, must be preserved.
	/// This is different from <seealso cref="ProcessorStep"/> which has ability of running multiple batches in parallel,
	/// each batch processed by one thread.
	/// </summary>
	public abstract class ForkedProcessorStep<T> : AbstractStep<T>
	{
		 private readonly long _completedProcessorsOffset = getFieldOffset( typeof( Unit ), "completedProcessors" );
		 private readonly long _processingTimeOffset = getFieldOffset( typeof( Unit ), "processingTime" );

		 private readonly object[] _forkedProcessors;
		 private volatile int _numberOfForkedProcessors;
		 private readonly AtomicReference<Unit> _head;
		 private readonly AtomicReference<Unit> _tail;
		 private readonly Thread _downstreamSender;
		 private volatile int _targetNumberOfProcessors = 1;
		 private readonly int _maxProcessors;
		 private readonly int _maxQueueLength;
		 private volatile Thread _receiverThread;
		 private readonly StampedLock _stripingLock;

		 protected internal ForkedProcessorStep( StageControl control, string name, Configuration config, params StatsProvider[] statsProviders ) : base( control, name, config, statsProviders )
		 {
			  this._maxProcessors = config.MaxNumberOfProcessors();
			  this._forkedProcessors = new object[this._maxProcessors];
			  _stripingLock = new StampedLock();

			  Unit noop = new Unit( this, -1, default( T ), 0 );
			  _head = new AtomicReference<Unit>( noop );
			  _tail = new AtomicReference<Unit>( noop );

			  _stripingLock.unlock( ApplyProcessorCount( _stripingLock.readLock() ) );
			  _downstreamSender = new CompletedBatchesSender( this, name + " [CompletedBatchSender]" );
			  _maxQueueLength = 200 + _maxProcessors;
		 }

		 private long ApplyProcessorCount( long @lock )
		 {
			  if ( _numberOfForkedProcessors != _targetNumberOfProcessors )
			  {
					_stripingLock.unlock( @lock );
					@lock = _stripingLock.writeLock();
					AwaitAllCompleted();
					int processors = _targetNumberOfProcessors;
					while ( _numberOfForkedProcessors < processors )
					{
						 if ( _forkedProcessors[_numberOfForkedProcessors] == null )
						 {
							  _forkedProcessors[_numberOfForkedProcessors] = new ForkedProcessor( this, _numberOfForkedProcessors, _tail.get() );
						 }
						 _numberOfForkedProcessors++;
					}
					if ( _numberOfForkedProcessors > processors )
					{
						 _numberOfForkedProcessors = processors;
						 // Excess processors will notice that they are not needed right now, and will park until they are.
						 // The most important thing here is that future Units will have a lower number of processor as expected max.
					}
			  }
			  return @lock;
		 }

		 private void AwaitAllCompleted()
		 {
			  while ( _head.get() != _tail.get() )
			  {
					Park.park( _receiverThread = Thread.CurrentThread );
			  }
		 }

		 public override int Processors( int delta )
		 {
			  _targetNumberOfProcessors = max( 1, min( _targetNumberOfProcessors + delta, _maxProcessors ) );
			  return _targetNumberOfProcessors;
		 }

		 public override void Start( int orderingGuarantees )
		 {
			  base.Start( orderingGuarantees );
			  _downstreamSender.Start();
		 }

		 public override long Receive( long ticket, T batch )
		 {
			  long time = nanoTime();
			  while ( QueuedBatches.get() >= _maxQueueLength )
			  {
					Park.park( _receiverThread = Thread.CurrentThread );
			  }
			  // It is of importance that all items in the queue at the same time agree on the number of processors. We take this lock in order to make sure that we
			  // do not interfere with another thread trying to drain the queue in order to change the processor count.
			  long @lock = ApplyProcessorCount( _stripingLock.readLock() );
			  QueuedBatches.incrementAndGet();
			  Unit unit = new Unit( this, ticket, batch, _numberOfForkedProcessors );

			  // [old head] [unit]
			  //               ^
			  //              head
			  Unit myHead = _head.getAndSet( unit );

			  // [old head] -next-> [unit]
			  myHead.Next = unit;
			  _stripingLock.unlock( @lock );

			  return nanoTime() - time;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract void forkedProcess(int id, int processors, T batch) throws Throwable;
		 protected internal abstract void ForkedProcess( int id, int processors, T batch );

		 internal virtual void SendDownstream( Unit unit )
		 {
			  DownstreamIdleTime.add( DownstreamConflict.receive( unit.Ticket, unit.Batch ) );
		 }

		 // One unit of work. Contains the batch along with ticket and meta state during processing such
		 // as how many processors are done with this batch and link to next batch in the queue.
		 internal class Unit
		 {
			 private readonly ForkedProcessorStep<T> _outerInstance;

			  internal readonly long Ticket;
			  internal readonly T Batch;

			  // Number of processors which is expected to process this batch, this is the number of processors
			  // assigned at the time of enqueueing this unit.
			  internal readonly int Processors;

			  // Updated when a ForkedProcessor have processed this unit.
			  // Atomic since changed by UnsafeUtil#getAndAddInt/Long.
			  // Volatile since read by CompletedBatchesSender.
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") private volatile int completedProcessors;
			  internal volatile int CompletedProcessors;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") private volatile long processingTime;
			  internal volatile long ProcessingTime;

			  // Volatile since assigned by thread enqueueing this unit after changing head of the queue.
			  internal volatile Unit Next;

			  internal Unit( ForkedProcessorStep<T> outerInstance, long ticket, T batch, int processors )
			  {
				  this._outerInstance = outerInstance;
					this.Ticket = ticket;
					this.Batch = batch;
					this.Processors = processors;
			  }

			  internal virtual bool Completed
			  {
				  get
				  {
						return Processors > 0 && Processors == CompletedProcessors;
				  }
			  }

			  internal virtual void ProcessorDone( long time )
			  {
					UnsafeUtil.getAndAddLong( this, outerInstance.PROCESSING_TIME_OFFSET, time );
					int prevCompletedProcessors = UnsafeUtil.getAndAddInt( this, outerInstance.COMPLETED_PROCESSORS_OFFSET, 1 );
					Debug.Assert( prevCompletedProcessors < Processors, prevCompletedProcessors + " vs " + Processors + " for " + Ticket );
			  }

			  public override string ToString()
			  {
					return format( "Unit[%d/%d]", CompletedProcessors, Processors );
			  }
		 }

		 /// <summary>
		 /// Checks tail of queue and sends fully completed units downstream. Since
		 /// <seealso cref="ForkedProcessorStep.receive(long, object)"/> may park on queue bound, this thread will
		 /// unpark the most recent thread calling receive to close that wait gap.
		 /// <seealso cref="ForkedProcessor"/>, the last one processing a unit, will unpark this thread.
		 /// </summary>
		 private sealed class CompletedBatchesSender : Thread
		 {
			 private readonly ForkedProcessorStep<T> _outerInstance;

			  internal CompletedBatchesSender( ForkedProcessorStep<T> outerInstance, string name ) : base( name )
			  {
				  this._outerInstance = outerInstance;
			  }

			  public override void Run()
			  {
					Unit current = outerInstance.tail.get();
					while ( !outerInstance.Completed )
					{
						 Unit candidate = current.Next;
						 if ( candidate != null && candidate.Completed )
						 {
							  if ( outerInstance.DownstreamConflict != null )
							  {
									outerInstance.SendDownstream( candidate );
							  }
							  else
							  {
									outerInstance.Control.recycle( candidate.Batch );
							  }
							  current = candidate;
							  outerInstance.tail.set( current );
							  outerInstance.QueuedBatches.decrementAndGet();
							  outerInstance.DoneBatches.incrementAndGet();
							  outerInstance.TotalProcessingTime.add( candidate.ProcessingTime );
							  outerInstance.CheckNotifyEndDownstream();
						 }
						 else
						 {
							  Thread receiver = _outerInstance.receiverThread;
							  if ( receiver != null )
							  {
									Park.unpark( receiver );
							  }
							  Park.park( this );
						 }
					}
			  }
		 }

		 // Processes units, forever walking the queue looking for more units to process.
		 // If there's no work to do it will park a while, otherwise it will exhaust the queue and process
		 // as far as it can without park. No external thread unparks these forked processors.
		 // So in scenarios where a processor isn't fully saturated there may be short periods of parking,
		 // but should saturate without any park as long as there are units to process.
		 internal class ForkedProcessor : Thread
		 {
			 private readonly ForkedProcessorStep<T> _outerInstance;

			  internal readonly int Id;
			  internal Unit Current;

			  internal ForkedProcessor( ForkedProcessorStep<T> outerInstance, int id, Unit startingUnit ) : base( outerInstance.Name() + "-" + id )
			  {
				  this._outerInstance = outerInstance;
					this.Id = id;
					this.Current = startingUnit;
					start();
			  }

			  public override void Run()
			  {
					try
					{
						 while ( !outerInstance.Completed )
						 {
							  Unit candidate = Current.next;
							  if ( candidate != null )
							  {
									// There's work to do.
									if ( Id < candidate.Processors )
									{
										 // We are expected to take care of this one.
										 long time = nanoTime();
										 outerInstance.ForkedProcess( Id, candidate.Processors, candidate.Batch );
										 candidate.ProcessorDone( nanoTime() - time );
									}
									// Skip to the next.

									Current = candidate;
							  }
							  else
							  {
									// There's no work to be done right now, park a while. When we wake up and work have accumulated
									// we'll plow throw them w/o park in between anyway.
									Park.park( this );
							  }
						 }
					}
					catch ( Exception e )
					{
						 outerInstance.IssuePanic( e, false );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws Exception
		 public override void Close()
		 {
			  Arrays.fill( _forkedProcessors, null );
			  base.Close();
		 }
	}

}