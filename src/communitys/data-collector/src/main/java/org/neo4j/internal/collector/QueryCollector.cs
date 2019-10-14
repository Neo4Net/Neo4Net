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
namespace Neo4Net.@internal.Collector
{

	using InvalidArgumentsException = Neo4Net.Kernel.Api.Exceptions.InvalidArgumentsException;
	using ExecutingQuery = Neo4Net.Kernel.api.query.ExecutingQuery;
	using QuerySnapshot = Neo4Net.Kernel.api.query.QuerySnapshot;
	using QueryExecutionMonitor = Neo4Net.Kernel.impl.query.QueryExecutionMonitor;
	using Group = Neo4Net.Scheduler.Group;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;

	/// <summary>
	/// Thread-safe query collector.
	/// 
	/// Delegates to RingRecentBuffer to hard limit the number of collected queries at any point in time.
	/// </summary>
	internal class QueryCollector : CollectorStateMachine<IEnumerator<TruncatedQuerySnapshot>>, QueryExecutionMonitor
	{
		 private volatile bool _isCollecting;
		 private readonly RingRecentBuffer<TruncatedQuerySnapshot> _queries;
		 private readonly JobScheduler _jobScheduler;
		 private readonly int _maxQueryTextSize;

		 internal QueryCollector( JobScheduler jobScheduler, int maxRecentQueryCount, int maxQueryTextSize ) : base( true )
		 {
			  this._jobScheduler = jobScheduler;
			  this._maxQueryTextSize = maxQueryTextSize;
			  _isCollecting = false;

			  // Round down to the nearest power of 2
			  int queryBufferSize = Integer.highestOneBit( maxRecentQueryCount );
			  _queries = new RingRecentBuffer<TruncatedQuerySnapshot>( queryBufferSize );
		 }

		 internal virtual long NumSilentQueryDrops()
		 {
			  return _queries.numSilentQueryDrops();
		 }

		 // CollectorStateMachine

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected Result doCollect(java.util.Map<String,Object> config, long collectionId) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException
		 protected internal override Result DoCollect( IDictionary<string, object> config, long collectionId )
		 {
			  int collectSeconds = QueryCollectorConfig.Of( config ).CollectSeconds;
			  if ( collectSeconds > 0 )
			  {
					_jobScheduler.schedule( Group.DATA_COLLECTOR, () => QueryCollector.this.Stop(collectionId), collectSeconds, TimeUnit.SECONDS );
			  }
			  _isCollecting = true;
			  return Success( "Collection started." );
		 }

		 protected internal override Result DoStop()
		 {
			  _isCollecting = false;
			  return Success( "Collection stopped." );
		 }

		 protected internal override Result DoClear()
		 {
			  _queries.clear();
			  return Success( "Data cleared." );
		 }

		 protected internal override IEnumerator<TruncatedQuerySnapshot> DoGetData()
		 {
			  IList<TruncatedQuerySnapshot> querySnapshots = new List<TruncatedQuerySnapshot>();
			  _queries.@foreach( querySnapshots.add );
			  return querySnapshots.GetEnumerator();
		 }

		 // QueryExecutionMonitor

		 public override void EndFailure( ExecutingQuery query, Exception failure )
		 {
		 }

		 public override void EndSuccess( ExecutingQuery query )
		 {
			  if ( _isCollecting )
			  {
					QuerySnapshot snapshot = query.Snapshot();
					_queries.produce( new TruncatedQuerySnapshot( snapshot.QueryText(), snapshot.QueryPlanSupplier(), snapshot.QueryParameters(), snapshot.ElapsedTimeMicros(), snapshot.CompilationTimeMicros(), snapshot.StartTimestampMillis(), _maxQueryTextSize ) );
			  }
		 }
	}

}