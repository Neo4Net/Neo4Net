using System;
using System.Collections.Generic;
using System.Diagnostics;

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
namespace Neo4Net.@unsafe.Impl.Batchimport.staging
{

	using MovingAverage = Neo4Net.Kernel.impl.util.MovingAverage;
	using ParkStrategy = Neo4Net.@unsafe.Impl.Batchimport.executor.ParkStrategy;
	using ProcessingStats = Neo4Net.@unsafe.Impl.Batchimport.stats.ProcessingStats;
	using StatsProvider = Neo4Net.@unsafe.Impl.Batchimport.stats.StatsProvider;
	using StepStats = Neo4Net.@unsafe.Impl.Batchimport.stats.StepStats;
	using Neo4Net.Utils.Concurrent;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.commons.lang3.SystemUtils.IS_OS_WINDOWS;

	/// <summary>
	/// Basic implementation of a <seealso cref="Step"/>. Does the most plumbing job of building a step implementation.
	/// </summary>
	public abstract class AbstractStep<T> : Step<T>
	{
		public abstract int Processors( int delta );
		public abstract long Receive( long ticket, T batch );
		 public static readonly ParkStrategy Park = new Neo4Net.@unsafe.Impl.Batchimport.executor.ParkStrategy_Park( IS_OS_WINDOWS ? 10_000 : 500, MICROSECONDS );

		 protected internal readonly StageControl Control;
		 private volatile string _name;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("rawtypes") protected volatile Step downstream;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal volatile Step DownstreamConflict;
		 protected internal volatile WorkSync<Downstream, SendDownstream> DownstreamWorkSync;
		 private volatile bool _endOfUpstream;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal volatile Exception PanicConflict;
		 private readonly System.Threading.CountdownEvent _completed = new System.Threading.CountdownEvent( 1 );
		 protected internal int OrderingGuarantees;

		 // Milliseconds awaiting downstream to process batches so that its queue size goes beyond the configured threshold
		 // If this is big then it means that this step is faster than downstream.
		 protected internal readonly LongAdder DownstreamIdleTime = new LongAdder();
		 // Milliseconds awaiting upstream to hand over batches to this step.
		 // If this is big then it means that this step is faster than upstream.
		 protected internal readonly LongAdder UpstreamIdleTime = new LongAdder();
		 // Number of batches received, but not yet processed.
		 protected internal readonly AtomicInteger QueuedBatches = new AtomicInteger();
		 // Number of batches fully processed
		 protected internal readonly AtomicLong DoneBatches = new AtomicLong();
		 // Milliseconds spent processing all received batches.
		 protected internal readonly MovingAverage TotalProcessingTime;
		 protected internal long StartTime;
		 protected internal long EndTime;
		 protected internal readonly IList<StatsProvider> AdditionalStatsProvider;
		 protected internal readonly Configuration Config;

		 public AbstractStep( StageControl control, string name, Configuration config, params StatsProvider[] additionalStatsProvider )
		 {
			  this.Control = control;
			  this._name = name;
			  this.Config = config;
			  this.TotalProcessingTime = new MovingAverage( config.MovingAverageSize() );
			  this.AdditionalStatsProvider = new IList<StatsProvider> { additionalStatsProvider };
		 }

		 public override void Start( int orderingGuarantees )
		 {
			  this.OrderingGuarantees = orderingGuarantees;
			  ResetStats();
		 }

		 protected internal virtual bool Guarantees( int orderingGuaranteeFlag )
		 {
			  return ( OrderingGuarantees & orderingGuaranteeFlag ) != 0;
		 }

		 public override string Name()
		 {
			  return _name;
		 }

		 public override void ReceivePanic( Exception cause )
		 {
			  this.PanicConflict = cause;
		 }

		 protected internal virtual bool StillWorking()
		 {
			  if ( Panic )
			  { // There has been a panic, so we'll just stop working
					return false;
			  }

			  if ( _endOfUpstream && QueuedBatches.get() == 0 )
			  { // Upstream has run out and we've processed everything upstream sent us
					return false;
			  }

			  // We're still working
			  return true;
		 }

		 protected internal virtual bool Panic
		 {
			 get
			 {
				  return PanicConflict != null;
			 }
		 }

		 public virtual bool Completed
		 {
			 get
			 {
				  return _completed.CurrentCount == 0;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void awaitCompleted() throws InterruptedException
		 public override void AwaitCompleted()
		 {
			  _completed.await();
		 }

		 protected internal virtual void IssuePanic( Exception cause )
		 {
			  IssuePanic( cause, true );
		 }

		 protected internal virtual void IssuePanic( Exception cause, bool rethrow )
		 {
			  Control.panic( cause );
			  if ( rethrow )
			  {
					throw new Exception( cause );
			  }
		 }

		 protected internal virtual void AssertHealthy()
		 {
			  if ( Panic )
			  {
					throw new Exception( PanicConflict );
			  }
		 }

		 public virtual Step<T1> Downstream<T1>
		 {
			 set
			 {
				  Debug.Assert( value != this );
				  this.DownstreamConflict = value;
				  //noinspection unchecked
				  this.DownstreamWorkSync = new WorkSync<Downstream, SendDownstream>( new Downstream( ( Step<object> ) value, DoneBatches ) );
			 }
		 }

		 public override StepStats Stats()
		 {
			  ICollection<StatsProvider> providers = new List<StatsProvider>();
			  CollectStatsProviders( providers );
			  return new StepStats( _name, StillWorking(), providers );
		 }

		 protected internal virtual void CollectStatsProviders( ICollection<StatsProvider> into )
		 {
			  into.Add( new ProcessingStats( DoneBatches.get() + QueuedBatches.get(), DoneBatches.get(), TotalProcessingTime.total(), TotalProcessingTime.average() / Processors(0), UpstreamIdleTime.sum(), DownstreamIdleTime.sum() ) );
			  into.addAll( AdditionalStatsProvider );
		 }

		 public override void EndOfUpstream()
		 {
			  _endOfUpstream = true;
			  CheckNotifyEndDownstream();
		 }

		 protected internal virtual void CheckNotifyEndDownstream()
		 {
			  if ( !StillWorking() && !Completed )
			  {
					lock ( this )
					{
						 // Only allow a single thread to notify that we've ended our stream as well as calling done()
						 // stillWorking(), once false cannot again return true so no need to check
						 if ( !Completed )
						 {
							  Done();
							  if ( DownstreamConflict != null )
							  {
									DownstreamConflict.endOfUpstream();
							  }
							  EndTime = currentTimeMillis();
							  _completed.Signal();
						 }
					}
			  }
		 }

		 /// <summary>
		 /// Called once, when upstream has run out of batches to send and all received batches have been
		 /// processed successfully.
		 /// </summary>
		 protected internal virtual void Done()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws Exception
		 public override void Close()
		 { // Do nothing by default
		 }

		 protected internal virtual void ChangeName( string name )
		 {
			  this._name = name;
		 }

		 protected internal virtual void ResetStats()
		 {
			  DownstreamIdleTime.reset();
			  UpstreamIdleTime.reset();
			  QueuedBatches.set( 0 );
			  DoneBatches.set( 0 );
			  TotalProcessingTime.reset();
			  StartTime = currentTimeMillis();
			  EndTime = 0;
		 }

		 public override string ToString()
		 {
			  return format( "%s[%s, processors:%d, batches:%d", this.GetType().Name, _name, Processors(0), DoneBatches.get() );
		 }
	}

}