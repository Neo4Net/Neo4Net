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
namespace Neo4Net.Consistency.checking.full
{
	using CacheAccess = Neo4Net.Consistency.checking.cache.CacheAccess;
	using Statistics = Neo4Net.Consistency.statistics.Statistics;
	using ProgressMonitorFactory = Neo4Net.Helpers.progress.ProgressMonitorFactory;
	using Neo4Net.Kernel.impl.store;
	using StoreAccess = Neo4Net.Kernel.impl.store.StoreAccess;
	using AbstractBaseRecord = Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord;

	public class SchemaStoreProcessorTask<R> : StoreProcessorTask<R> where R : Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord
	{
		 private readonly SchemaRecordCheck _schemaRecordCheck;

		 public SchemaStoreProcessorTask( string name, Statistics statistics, int threads, RecordStore<R> store, StoreAccess storeAccess, string builderPrefix, SchemaRecordCheck schemaRecordCheck, ProgressMonitorFactory.MultiPartBuilder builder, CacheAccess cacheAccess, StoreProcessor processor, QueueDistribution distribution ) : base( name, statistics, threads, store, storeAccess, builderPrefix, builder, cacheAccess, processor, distribution )
		 {
			  this._schemaRecordCheck = schemaRecordCheck;
		 }

		 protected internal override void BeforeProcessing( StoreProcessor processor )
		 {
			  processor.SchemaRecordCheck = _schemaRecordCheck;
		 }

		 protected internal override void AfterProcessing( StoreProcessor processor )
		 {
			  processor.SchemaRecordCheck = null;
		 }
	}

}