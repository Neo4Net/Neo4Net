using System;
using System.Diagnostics;

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

	using Neo4Net.@unsafe.Impl.Batchimport.executor;
	using Neo4Net.@unsafe.Impl.Batchimport.executor;
	using StatsProvider = Neo4Net.@unsafe.Impl.Batchimport.stats.StatsProvider;
	using VisibleForTesting = Neo4Net.Util.VisibleForTesting;
	using AsyncApply = Neo4Net.Util.concurrent.AsyncApply;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.nanoTime;

	/// <summary>
	/// <seealso cref="Step"/> that uses <seealso cref="TaskExecutor"/> as a queue and execution mechanism.
	/// Supports an arbitrary number of threads to execute batches in parallel.
	/// Subclasses implement <seealso cref="process(object, BatchSender)"/> receiving the batch to process
	/// and an <seealso cref="BatchSender"/> for sending the modified batch, or other batches downstream.
	/// 
	/// </summary>
	public abstract class ProcessorStep<T> : AbstractStep<T>
	{
		 private TaskExecutor<Sender> _executor;
		 // max processors for this step, zero means unlimited, or rather config.maxNumberOfProcessors()
		 private readonly int _maxProcessors;

		 // Time stamp for when we processed the last queued batch received from upstream.
		 // Useful for tracking how much time we spend waiting for batches from upstream.
		 private readonly AtomicLong _lastBatchEndTime = new AtomicLong();

		 protected internal ProcessorStep( StageControl control, string name, Configuration config, int maxProcessors, params StatsProvider[] additionalStatsProviders ) : base( control, name, config, additionalStatsProviders )
		 {
			  this._maxProcessors = maxProcessors;
		 }

		 public override void Start( int orderingGuarantees )
		 {
			  base.Start( orderingGuarantees );
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  this._executor = new DynamicTaskExecutor<Sender>( 1, _maxProcessors, Config.maxNumberOfProcessors(), Park, Name(), Sender::new );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public long receive(final long ticket, final T batch)
		 public override long Receive( long ticket, T batch )
		 {
			  // Don't go too far ahead
			  IncrementQueue();
			  long nanoTime = nanoTime();
			  _executor.submit(sender =>
			  {
				AssertHealthy();
				sender.initialize( ticket );
				try
				{
					 long startTime = nanoTime();
					 Process( batch, sender );
					 if ( DownstreamConflict == null )
					 {
						  // No batches were emitted so we couldn't track done batches in that way.
						  // We can see that we're the last step so increment here instead
						  DoneBatches.incrementAndGet();
						  Control.recycle( batch );
					 }
					 TotalProcessingTime.add( nanoTime() - startTime - sender.sendTime );

					 DecrementQueue();
					 CheckNotifyEndDownstream();
				}
				catch ( Exception e )
				{
					 IssuePanic( e );
				}
			  });
			  return nanoTime() - nanoTime;
		 }

		 private void DecrementQueue()
		 {
			  // Even though queuedBatches is built into AbstractStep, in that number of received batches
			  // is number of done + queued batches, this is the only implementation changing queuedBatches
			  // since it's the only implementation capable of such. That's why this code is here
			  // and not in AbstractStep.
			  int queueSizeAfterThisJobDone = QueuedBatches.decrementAndGet();
			  Debug.Assert( queueSizeAfterThisJobDone >= 0, "Negative queue size " + queueSizeAfterThisJobDone );
			  if ( queueSizeAfterThisJobDone == 0 )
			  {
					_lastBatchEndTime.set( currentTimeMillis() );
			  }
		 }

		 private void IncrementQueue()
		 {
			  if ( QueuedBatches.AndIncrement == 0 )
			  { // This is the first batch after we last drained the queue.
					long lastBatchEnd = _lastBatchEndTime.get();
					if ( lastBatchEnd != 0 )
					{
						 UpstreamIdleTime.add( currentTimeMillis() - lastBatchEnd );
					}
			  }
		 }

		 /// <summary>
		 /// Processes a <seealso cref="receive(long, object) received"/> batch. Any batch that should be sent downstream
		 /// as part of processing the supplied batch should be done so using <seealso cref="BatchSender.send(object)"/>.
		 /// 
		 /// The most typical implementation of this method is to process the received batch, either by
		 /// creating a new batch object containing some derivative of the received batch, or the batch
		 /// object itself with some modifications and <seealso cref="BatchSender.send(object) emit"/> that in the end of the method.
		 /// </summary>
		 /// <param name="batch"> batch to process. </param>
		 /// <param name="sender"> <seealso cref="BatchSender"/> for sending zero or more batches downstream. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract void process(T batch, BatchSender sender) throws Throwable;
		 protected internal abstract void Process( T batch, BatchSender sender );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws Exception
		 public override void Close()
		 {
			  base.Close();
			  _executor.close();
		 }

		 public override int Processors( int delta )
		 {
			  return _executor.processors( delta );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private org.neo4j.util.concurrent.AsyncApply sendDownstream(long ticket, Object batch, org.neo4j.util.concurrent.AsyncApply downstreamAsync)
		 private AsyncApply SendDownstream( long ticket, object batch, AsyncApply downstreamAsync )
		 {
			  if ( Guarantees( Step_Fields.ORDER_SEND_DOWNSTREAM ) )
			  {
					AsyncApply async = DownstreamWorkSync.applyAsync( new SendDownstream( ticket, batch, DownstreamIdleTime ) );
					if ( downstreamAsync != null )
					{
						 try
						 {
							  downstreamAsync.Await();
							  async.Await();
							  return null;
						 }
						 catch ( ExecutionException e )
						 {
							  IssuePanic( e.InnerException );
						 }
					}
					else
					{
						 return async;
					}
			  }
			  else
			  {
					DownstreamIdleTime.add( DownstreamConflict.receive( ticket, batch ) );
					DoneBatches.incrementAndGet();
			  }
			  return null;
		 }

		 protected internal override void Done()
		 {
			  LastCallForEmittingOutstandingBatches( new Sender( this ) );
			  if ( DownstreamWorkSync != null )
			  {
					try
					{
						 DownstreamWorkSync.apply( new SendDownstream( -1, null, DownstreamIdleTime ) );
					}
					catch ( ExecutionException e )
					{
						 IssuePanic( e.InnerException );
					}
			  }
			  base.Done();
		 }

		 protected internal virtual void LastCallForEmittingOutstandingBatches( BatchSender sender )
		 { // Nothing to emit, subclasses might have though
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting public int getMaxProcessors()
		 public virtual int MaxProcessors
		 {
			 get
			 {
				  return _maxProcessors;
			 }
		 }

		 private class Sender : BatchSender
		 {
			 private readonly ProcessorStep<T> _outerInstance;

			 public Sender( ProcessorStep<T> outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal long SendTime;
			  internal long Ticket;
			  internal AsyncApply DownstreamAsync;

			  public override void Send( object batch )
			  {
					long time = nanoTime();
					try
					{
						 DownstreamAsync = outerInstance.sendDownstream( Ticket, batch, DownstreamAsync );
					}
					finally
					{
						 SendTime += nanoTime() - time;
					}
			  }

			  public virtual void Initialize( long ticket )
			  {
					this.Ticket = ticket;
					this.SendTime = 0;
			  }
		 }
	}

}