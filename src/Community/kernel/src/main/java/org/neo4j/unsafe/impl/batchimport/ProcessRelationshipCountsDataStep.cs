using System.Collections.Concurrent;
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
namespace Neo4Net.@unsafe.Impl.Batchimport
{

	using CountsAccessor = Neo4Net.Kernel.Impl.Api.CountsAccessor;
	using RelationshipRecord = Neo4Net.Kernel.impl.store.record.RelationshipRecord;
	using ProgressReporter = Neo4Net.Kernel.impl.util.monitoring.ProgressReporter;
	using GatheringMemoryStatsVisitor = Neo4Net.@unsafe.Impl.Batchimport.cache.GatheringMemoryStatsVisitor;
	using NodeLabelsCache = Neo4Net.@unsafe.Impl.Batchimport.cache.NodeLabelsCache;
	using NumberArrayFactory = Neo4Net.@unsafe.Impl.Batchimport.cache.NumberArrayFactory;
	using BatchSender = Neo4Net.@unsafe.Impl.Batchimport.staging.BatchSender;
	using Neo4Net.@unsafe.Impl.Batchimport.staging;
	using StageControl = Neo4Net.@unsafe.Impl.Batchimport.staging.StageControl;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.toIntExact;

	/// <summary>
	/// Processes relationship records, feeding them to <seealso cref="RelationshipCountsProcessor"/> which keeps
	/// the accumulated counts per thread. Aggregated in <seealso cref="done()"/>.
	/// </summary>
	public class ProcessRelationshipCountsDataStep : ProcessorStep<RelationshipRecord[]>
	{
		 private readonly NodeLabelsCache _cache;
		 private readonly IDictionary<Thread, RelationshipCountsProcessor> _processors = new ConcurrentDictionary<Thread, RelationshipCountsProcessor>();
		 private readonly int _highLabelId;
		 private readonly int _highRelationshipTypeId;
		 private readonly Neo4Net.Kernel.Impl.Api.CountsAccessor_Updater _countsUpdater;
		 private readonly NumberArrayFactory _cacheFactory;
		 private readonly ProgressReporter _progressMonitor;

		 public ProcessRelationshipCountsDataStep( StageControl control, NodeLabelsCache cache, Configuration config, int highLabelId, int highRelationshipTypeId, Neo4Net.Kernel.Impl.Api.CountsAccessor_Updater countsUpdater, NumberArrayFactory cacheFactory, ProgressReporter progressReporter ) : base( control, "COUNT", config, NumberOfProcessors( config, cache, highLabelId, highRelationshipTypeId ) )
		 {
			  this._cache = cache;
			  this._highLabelId = highLabelId;
			  this._highRelationshipTypeId = highRelationshipTypeId;
			  this._countsUpdater = countsUpdater;
			  this._cacheFactory = cacheFactory;
			  this._progressMonitor = progressReporter;
		 }

		 /// <summary>
		 /// Keeping all counts for all combinations of label/reltype can require a lot of memory if there are lots of those tokens.
		 /// Each processor will allocate such a data structure and so in extreme cases the number of processors will have to
		 /// be limited to not surpass the available memory limits.
		 /// </summary>
		 /// <param name="config"> <seealso cref="Configuration"/> holding things like max number of processors and max memory. </param>
		 /// <param name="cache"> <seealso cref="NodeLabelsCache"/> which is the only other data structure occupying memory at this point. </param>
		 /// <param name="highLabelId"> high label id for this store. </param>
		 /// <param name="highRelationshipTypeId"> high relationship type id for this store. </param>
		 /// <returns> number of processors suitable for this step. In most cases this will be 0, which is the typical value used
		 /// when just allowing the importer to grab up to <seealso cref="Configuration.maxNumberOfProcessors()"/>. The returned value
		 /// will at least be 1. </returns>
		 private static int NumberOfProcessors( Configuration config, NodeLabelsCache cache, int highLabelId, int highRelationshipTypeId )
		 {
			  GatheringMemoryStatsVisitor memVisitor = new GatheringMemoryStatsVisitor();
			  cache.AcceptMemoryStatsVisitor( memVisitor );

			  long availableMem = config.MaxMemoryUsage() - memVisitor.TotalUsage;
			  long threadMem = RelationshipCountsProcessor.CalculateMemoryUsage( highLabelId, highRelationshipTypeId );
			  long possibleThreads = availableMem / threadMem;
			  return possibleThreads >= config.MaxNumberOfProcessors() ? 0 : toIntExact(max(1, possibleThreads));
		 }

		 protected internal override void Process( RelationshipRecord[] batch, BatchSender sender )
		 {
			  RelationshipCountsProcessor processor = processor();
			  foreach ( RelationshipRecord record in batch )
			  {
					if ( record.InUse() )
					{
						 processor.Process( record );
					}
			  }
			  _progressMonitor.progress( batch.Length );
		 }

		 private RelationshipCountsProcessor Processor()
		 {
			  // This is OK since in this step implementation we use TaskExecutor which sticks to its threads deterministically.
			  return _processors.computeIfAbsent( Thread.CurrentThread, k => new RelationshipCountsProcessor( _cache, _highLabelId, _highRelationshipTypeId, _countsUpdater, _cacheFactory ) );
		 }

		 protected internal override void Done()
		 {
			  base.Done();
			  RelationshipCountsProcessor all = null;
			  foreach ( RelationshipCountsProcessor processor in _processors.Values )
			  {
					if ( all == null )
					{
						 all = processor;
					}
					else
					{
						 all.AddCountsFrom( processor );
					}
			  }
			  if ( all != null )
			  {
					all.Done();
			  }

			  foreach ( RelationshipCountsProcessor processor in _processors.Values )
			  {
					processor.Close();
			  }
		 }
	}

}