using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.Cypher.@internal.codegen.profiling
{
	using Id = Neo4Net.Cypher.@internal.v3_5.util.attribution.Id;


	using KernelStatisticProvider = Neo4Net.Cypher.@internal.planner.v3_5.spi.KernelStatisticProvider;
	using QueryExecutionEvent = Neo4Net.Cypher.@internal.runtime.compiled.codegen.QueryExecutionEvent;
	using OperatorProfile = Neo4Net.Cypher.result.OperatorProfile;
	using QueryProfile = Neo4Net.Cypher.result.QueryProfile;

	public class ProfilingTracer : QueryExecutionTracer, QueryProfile
	{
		 public interface Clock
		 {
			  long NanoTime();
		 }

		 public static class Clock_Fields
		 {
			 private readonly ProfilingTracer _outerInstance;

			 public Clock_Fields( ProfilingTracer outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public static readonly Clock SystemTimer = System.nanoTime;
		 }

		 private static readonly Data _zero = new Data();

		 private readonly Clock _clock;
		 private readonly KernelStatisticProvider _statisticProvider;
		 private readonly IDictionary<int, Data> _data = new Dictionary<int, Data>();

		 public ProfilingTracer( KernelStatisticProvider statisticProvider ) : this( Clock_Fields.SystemTimer, statisticProvider )
		 {
		 }

		 internal ProfilingTracer( Clock clock, KernelStatisticProvider statisticProvider )
		 {
			  this._clock = clock;
			  this._statisticProvider = statisticProvider;
		 }

		 public virtual OperatorProfile OperatorProfile( int query )
		 {
			  Data value = _data[query];
			  return value == null ? _zero : value;
		 }

		 public virtual long TimeOf( Id query )
		 {
			  return OperatorProfile( query.x() ).time();
		 }

		 public virtual long DbHitsOf( Id query )
		 {
			  return OperatorProfile( query.x() ).dbHits();
		 }

		 public virtual long RowsOf( Id query )
		 {
			  return OperatorProfile( query.x() ).rows();
		 }

		 public override QueryExecutionEvent ExecuteOperator( Id queryId )
		 {
			  Data queryData = this._data[queryId.x()];
			  if ( queryData == null )
			  {
					queryData = new Data();
					this._data[queryId.x()] = queryData;
			  }
			  return new ExecutionEvent( _clock, _statisticProvider, queryData );
		 }

		 private class ExecutionEvent : QueryExecutionEvent
		 {
			  internal readonly long Start;
			  internal readonly Clock Clock;
			  internal readonly KernelStatisticProvider StatisticProvider;
			  internal readonly Data Data;
			  internal long HitCount;
			  internal long RowCount;

			  internal ExecutionEvent( Clock clock, KernelStatisticProvider statisticProvider, Data data )
			  {
					this.Clock = clock;
					this.StatisticProvider = statisticProvider;
					this.Data = data;
					this.Start = clock.NanoTime();
			  }

			  public override void Close()
			  {
					long executionTime = Clock.nanoTime() - Start;
					long pageCacheHits = StatisticProvider.PageCacheHits;
					long pageCacheFaults = StatisticProvider.PageCacheMisses;
					if ( Data != null )
					{
						 Data.update( executionTime, HitCount, RowCount, pageCacheHits, pageCacheFaults );
					}
			  }

			  public override void DbHit()
			  {
					HitCount++;
			  }

			  public override void Row()
			  {
					RowCount++;
			  }
		 }

		 private class Data : OperatorProfile
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long TimeConflict;
			  internal long Hits;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long RowsConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long PageCacheHitsConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long PageCacheMissesConflict;

			  public virtual void Update( long time, long hits, long rows, long pageCacheHits, long pageCacheMisses )
			  {
					this.TimeConflict += time;
					this.Hits += hits;
					this.RowsConflict += rows;
					this.PageCacheHitsConflict += pageCacheHits;
					this.PageCacheMissesConflict += pageCacheMisses;
			  }

			  public override long Time()
			  {
					return TimeConflict;
			  }

			  public override long DbHits()
			  {
					return Hits;
			  }

			  public override long Rows()
			  {
					return RowsConflict;
			  }

			  public override long PageCacheHits()
			  {
					return PageCacheHitsConflict;
			  }

			  public override long PageCacheMisses()
			  {
					return PageCacheMissesConflict;
			  }
		 }
	}

}