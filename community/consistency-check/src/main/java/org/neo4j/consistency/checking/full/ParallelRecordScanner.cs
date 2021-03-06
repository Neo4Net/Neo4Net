﻿/*
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
namespace Org.Neo4j.Consistency.checking.full
{
	using CacheAccess = Org.Neo4j.Consistency.checking.cache.CacheAccess;
	using Org.Neo4j.Consistency.checking.full;
	using Statistics = Org.Neo4j.Consistency.statistics.Statistics;
	using Org.Neo4j.Helpers.Collection;
	using MultiPartBuilder = Org.Neo4j.Helpers.progress.ProgressMonitorFactory.MultiPartBuilder;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.cache.DefaultCacheAccess.DEFAULT_QUEUE_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.full.RecordDistributor.distributeRecords;

	public class ParallelRecordScanner<RECORD> : RecordScanner<RECORD>
	{
		 private readonly CacheAccess _cacheAccess;
		 private readonly QueueDistribution _distribution;

		 public ParallelRecordScanner( string name, Statistics statistics, int threads, BoundedIterable<RECORD> store, MultiPartBuilder builder, RecordProcessor<RECORD> processor, CacheAccess cacheAccess, QueueDistribution distribution, params IterableStore[] warmUpStores ) : base( name, statistics, threads, store, builder, processor, warmUpStores )
		 {
			  this._cacheAccess = cacheAccess;
			  this._distribution = distribution;
		 }

		 protected internal override void Scan()
		 {
			  long recordsPerCPU = RecordDistributor.CalculateRecordsPerCpu( Store.maxCount(), NumberOfThreads );
			  _cacheAccess.prepareForProcessingOfSingleStore( recordsPerCPU );

			  QueueDistribution_QueueDistributor<RECORD> distributor = _distribution.distributor( recordsPerCPU, NumberOfThreads );
			  distributeRecords( NumberOfThreads, this.GetType().Name + "-" + Name, DEFAULT_QUEUE_SIZE, Store.GetEnumerator(), Progress, Processor, distributor );
		 }
	}

}