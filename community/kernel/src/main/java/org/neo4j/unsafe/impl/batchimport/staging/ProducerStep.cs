using System;
using System.Collections.Generic;
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
namespace Org.Neo4j.@unsafe.Impl.Batchimport.staging
{

	using Key = Org.Neo4j.@unsafe.Impl.Batchimport.stats.Key;
	using Keys = Org.Neo4j.@unsafe.Impl.Batchimport.stats.Keys;
	using Stat = Org.Neo4j.@unsafe.Impl.Batchimport.stats.Stat;
	using StatsProvider = Org.Neo4j.@unsafe.Impl.Batchimport.stats.StatsProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Exceptions.SILENT_UNCAUGHT_EXCEPTION_HANDLER;

	/// <summary>
	/// Step that generally sits first in a <seealso cref="Stage"/> and produces batches that will flow downstream
	/// to other <seealso cref="Step steps"/>.
	/// </summary>
	public abstract class ProducerStep : AbstractStep<Void>, StatsProvider
	{
		public override abstract int Processors( int delta );
		 protected internal readonly int BatchSize;

		 public ProducerStep( StageControl control, Configuration config ) : base( control, ">", config )
		 {
			  this.BatchSize = config.BatchSize();
		 }

		 /// <summary>
		 /// Merely receives one call, like a start signal from the staging framework.
		 /// </summary>
		 public override long Receive( long ticket, Void batch )
		 {
			  // It's fine to not store a reference to this thread here because either it completes and exits
			  // normally, notices a panic and exits via an exception.
			  Thread thread = new Thread(Name()() =>
			  {
				 AssertHealthy();
				 try
				 {
					  Process();
					  EndOfUpstream();
				 }
				 catch ( Exception e )
				 {
					  IssuePanic( e, false );
				 }
			  });
			  thread.UncaughtExceptionHandler = SILENT_UNCAUGHT_EXCEPTION_HANDLER;
			  thread.Start();
			  return 0;
		 }

		 protected internal abstract void Process();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") protected void sendDownstream(Object batch)
		 protected internal virtual void SendDownstream( object batch )
		 {
			  long time = DownstreamConflict.receive( DoneBatches.AndIncrement, batch );
			  DownstreamIdleTime.add( time );
		 }

		 protected internal override void CollectStatsProviders( ICollection<StatsProvider> into )
		 {
			  base.CollectStatsProviders( into );
			  into.Add( this );
		 }

		 public override Stat Stat( Key key )
		 {
			  if ( key == Keys.io_throughput )
			  {
					return new IoThroughputStat( StartTime, EndTime, Position() );
			  }
			  return null;
		 }

		 public override Key[] Keys()
		 {
			  return new Key[] { Keys.io_throughput };
		 }

		 /// <returns> progress in terms of bytes. </returns>
		 protected internal abstract long Position();
	}

}