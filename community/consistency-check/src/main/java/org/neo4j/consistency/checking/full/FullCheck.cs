﻿using System;
using System.Collections.Generic;

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
namespace Org.Neo4j.Consistency.checking.full
{

	using CacheAccess = Org.Neo4j.Consistency.checking.cache.CacheAccess;
	using DefaultCacheAccess = Org.Neo4j.Consistency.checking.cache.DefaultCacheAccess;
	using IndexAccessors = Org.Neo4j.Consistency.checking.index.IndexAccessors;
	using ConsistencyReporter = Org.Neo4j.Consistency.report.ConsistencyReporter;
	using Monitor = Org.Neo4j.Consistency.report.ConsistencyReporter.Monitor;
	using ConsistencySummaryStatistics = Org.Neo4j.Consistency.report.ConsistencySummaryStatistics;
	using InconsistencyMessageLogger = Org.Neo4j.Consistency.report.InconsistencyMessageLogger;
	using InconsistencyReport = Org.Neo4j.Consistency.report.InconsistencyReport;
	using Statistics = Org.Neo4j.Consistency.statistics.Statistics;
	using CacheSmallStoresRecordAccess = Org.Neo4j.Consistency.store.CacheSmallStoresRecordAccess;
	using DirectRecordAccess = Org.Neo4j.Consistency.store.DirectRecordAccess;
	using RecordAccess = Org.Neo4j.Consistency.store.RecordAccess;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using ProgressListener = Org.Neo4j.Helpers.progress.ProgressListener;
	using ProgressMonitorFactory = Org.Neo4j.Helpers.progress.ProgressMonitorFactory;
	using DirectStoreAccess = Org.Neo4j.Kernel.api.direct.DirectStoreAccess;
	using IndexAccessor = Org.Neo4j.Kernel.Api.Index.IndexAccessor;
	using LabelScanStore = Org.Neo4j.Kernel.api.labelscan.LabelScanStore;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using ReporterFactory = Org.Neo4j.Kernel.Impl.Annotations.ReporterFactory;
	using CountsAccessor = Org.Neo4j.Kernel.Impl.Api.CountsAccessor;
	using IndexSamplingConfig = Org.Neo4j.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using Org.Neo4j.Kernel.impl.store;
	using StoreAccess = Org.Neo4j.Kernel.impl.store.StoreAccess;
	using CountsTracker = Org.Neo4j.Kernel.impl.store.counts.CountsTracker;
	using Org.Neo4j.Kernel.impl.store.kvstore;
	using AbstractBaseRecord = Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord;
	using LabelTokenRecord = Org.Neo4j.Kernel.impl.store.record.LabelTokenRecord;
	using PropertyKeyTokenRecord = Org.Neo4j.Kernel.impl.store.record.PropertyKeyTokenRecord;
	using RelationshipTypeTokenRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipTypeTokenRecord;
	using Log = Org.Neo4j.Logging.Log;
	using StoreIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.StoreIndexDescriptor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.report.ConsistencyReporter.NO_MONITOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.RecordLoad.FORCE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.cache.NumberArrayFactory_Fields.AUTO_WITHOUT_PAGECACHE;

	public class FullCheck
	{
		 private readonly bool _checkPropertyOwners;
		 private readonly bool _checkLabelScanStore;
		 private readonly bool _checkIndexes;
		 private readonly bool _checkIndexStructure;
		 private readonly ProgressMonitorFactory _progressFactory;
		 private readonly IndexSamplingConfig _samplingConfig;
		 private readonly bool _checkGraph;
		 private readonly int _threads;
		 private readonly Statistics _statistics;
		 private readonly bool _startCountsStore;

		 public FullCheck( Config config, ProgressMonitorFactory progressFactory, Statistics statistics, int threads, bool startCountsStore ) : this( progressFactory, statistics, threads, new ConsistencyFlags( config ), config, startCountsStore )
		 {
		 }

		 public FullCheck( ProgressMonitorFactory progressFactory, Statistics statistics, int threads, ConsistencyFlags consistencyFlags, Config config, bool startCountsStore )
		 {
			  this._statistics = statistics;
			  this._threads = threads;
			  this._progressFactory = progressFactory;
			  this._samplingConfig = new IndexSamplingConfig( config );
			  this._checkGraph = consistencyFlags.CheckGraph;
			  this._checkIndexes = consistencyFlags.CheckIndexes;
			  this._checkIndexStructure = consistencyFlags.CheckIndexStructure;
			  this._checkLabelScanStore = consistencyFlags.CheckLabelScanStore;
			  this._checkPropertyOwners = consistencyFlags.CheckPropertyOwners;
			  this._startCountsStore = startCountsStore;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.consistency.report.ConsistencySummaryStatistics execute(org.neo4j.kernel.api.direct.DirectStoreAccess stores, org.neo4j.logging.Log log) throws ConsistencyCheckIncompleteException
		 public virtual ConsistencySummaryStatistics Execute( DirectStoreAccess stores, Log log )
		 {
			  return Execute( stores, log, NO_MONITOR );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.consistency.report.ConsistencySummaryStatistics execute(org.neo4j.kernel.api.direct.DirectStoreAccess stores, org.neo4j.logging.Log log, org.neo4j.consistency.report.ConsistencyReporter.Monitor reportMonitor) throws ConsistencyCheckIncompleteException
		 internal virtual ConsistencySummaryStatistics Execute( DirectStoreAccess stores, Log log, ConsistencyReporter.Monitor reportMonitor )
		 {
			  ConsistencySummaryStatistics summary = new ConsistencySummaryStatistics();
			  InconsistencyReport report = new InconsistencyReport( new InconsistencyMessageLogger( log ), summary );

			  OwnerCheck ownerCheck = new OwnerCheck( _checkPropertyOwners );
			  CountsBuilderDecorator countsBuilder = new CountsBuilderDecorator( stores.NativeStores() );
			  CheckDecorator decorator = new Org.Neo4j.Consistency.checking.CheckDecorator_ChainCheckDecorator( ownerCheck, countsBuilder );
			  CacheAccess cacheAccess = new DefaultCacheAccess( AUTO_WITHOUT_PAGECACHE.newByteArray( stores.NativeStores().NodeStore.HighId, new sbyte[ByteArrayBitsManipulator.MAX_BYTES] ), _statistics.Counts, _threads );
			  RecordAccess records = RecordAccess( stores.NativeStores(), cacheAccess );
			  Execute( stores, decorator, records, report, cacheAccess, reportMonitor );
			  ownerCheck.ScanForOrphanChains( _progressFactory );

			  if ( _checkGraph )
			  {
					CountsAccessor countsAccessor = stores.NativeStores().Counts;
					bool checkCounts = true;
					if ( _startCountsStore && countsAccessor is CountsTracker )
					{
						 CountsTracker tracker = ( CountsTracker ) countsAccessor;
						 // Perhaps other read-only use cases thinks it's fine to just rebuild an in-memory counts store,
						 // but the consistency checker should instead prevent rebuild and report that the counts store is broken or missing
						 tracker.Initializer = new RebuildPreventingCountsInitializer( this );
						 try
						 {
							  tracker.Start();
						 }
						 catch ( Exception e )
						 {
							  log.Error( "Counts store is missing, broken or of an older format and will not be consistency checked", e );
							  summary.Update( RecordType.COUNTS, 1, 0 );
							  checkCounts = false;
						 }
					}

					if ( checkCounts )
					{
						 countsBuilder.CheckCounts( countsAccessor, new ConsistencyReporter( records, report ), _progressFactory );
					}
			  }

			  if ( !summary.Consistent )
			  {
					log.Warn( "Inconsistencies found: " + summary );
			  }
			  return summary;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void execute(final org.neo4j.kernel.api.direct.DirectStoreAccess directStoreAccess, final org.neo4j.consistency.checking.CheckDecorator decorator, final org.neo4j.consistency.store.RecordAccess recordAccess, final org.neo4j.consistency.report.InconsistencyReport report, org.neo4j.consistency.checking.cache.CacheAccess cacheAccess, org.neo4j.consistency.report.ConsistencyReporter.Monitor reportMonitor) throws ConsistencyCheckIncompleteException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 internal virtual void Execute( DirectStoreAccess directStoreAccess, CheckDecorator decorator, RecordAccess recordAccess, InconsistencyReport report, CacheAccess cacheAccess, ConsistencyReporter.Monitor reportMonitor )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.consistency.report.ConsistencyReporter reporter = new org.neo4j.consistency.report.ConsistencyReporter(recordAccess, report, reportMonitor);
			  ConsistencyReporter reporter = new ConsistencyReporter( recordAccess, report, reportMonitor );
			  StoreProcessor processEverything = new StoreProcessor( decorator, reporter, Stage_Fields.SequentialForward, cacheAccess );
			  ProgressMonitorFactory.MultiPartBuilder progress = _progressFactory.multipleParts( "Full Consistency Check" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.store.StoreAccess nativeStores = directStoreAccess.nativeStores();
			  StoreAccess nativeStores = directStoreAccess.NativeStores();
			  try
			  {
					  using ( IndexAccessors indexes = new IndexAccessors( directStoreAccess.Indexes(), nativeStores.SchemaStore, _samplingConfig ) )
					  {
						MultiPassStore.Factory multiPass = new MultiPassStore.Factory( decorator, recordAccess, cacheAccess, report, reportMonitor );
						ConsistencyCheckTasks taskCreator = new ConsistencyCheckTasks( progress, processEverything, nativeStores, _statistics, cacheAccess, directStoreAccess.LabelScanStore(), indexes, directStoreAccess.TokenHolders(), multiPass, reporter, _threads );
      
						if ( _checkIndexStructure )
						{
							 ConsistencyCheckIndexStructure( directStoreAccess.LabelScanStore(), indexes, reporter, _progressFactory );
						}
      
						IList<ConsistencyCheckerTask> tasks = taskCreator.CreateTasksForFullCheck( _checkLabelScanStore, _checkIndexes, _checkGraph );
						progress.Build();
						TaskExecutor.Execute( tasks, decorator.prepare );
					  }
			  }
			  catch ( Exception e )
			  {
					throw new ConsistencyCheckIncompleteException( e );
			  }
		 }

		 internal static RecordAccess RecordAccess( StoreAccess store, CacheAccess cacheAccess )
		 {
			  return new CacheSmallStoresRecordAccess( new DirectRecordAccess( store, cacheAccess ), ReadAllRecords( typeof( PropertyKeyTokenRecord ), store.PropertyKeyTokenStore ), ReadAllRecords( typeof( RelationshipTypeTokenRecord ), store.RelationshipTypeTokenStore ), ReadAllRecords( typeof( LabelTokenRecord ), store.LabelTokenStore ) );
		 }

		 private static void ConsistencyCheckIndexStructure( LabelScanStore labelScanStore, IndexAccessors indexes, ConsistencyReporter report, ProgressMonitorFactory progressMonitorFactory )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long schemaIndexCount = org.neo4j.helpers.collection.Iterables.count(indexes.onlineRules());
			  long schemaIndexCount = Iterables.count( indexes.OnlineRules() );
			  const long additionalCount = 1; // LabelScanStore
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long totalCount = schemaIndexCount + additionalCount;
			  long totalCount = schemaIndexCount + additionalCount;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.helpers.progress.ProgressListener listener = progressMonitorFactory.singlePart("Index structure consistency check", totalCount);
			  ProgressListener listener = progressMonitorFactory.SinglePart( "Index structure consistency check", totalCount );
			  listener.Started();

			  ConsistencyCheckLabelScanStore( labelScanStore, report, listener );
			  ConsistencyCheckSchemaIndexes( indexes, report, listener );

			  listener.Done();
		 }

		 private static void ConsistencyCheckLabelScanStore( LabelScanStore labelScanStore, ConsistencyReporter report, ProgressListener listener )
		 {
			  ConsistencyReporter.FormattingDocumentedHandler handler = report.FormattingHandler( RecordType.LABEL_SCAN_DOCUMENT );
			  ReporterFactory proxyFactory = new ReporterFactory( handler );
			  labelScanStore.ConsistencyCheck( proxyFactory );
			  handler.UpdateSummary();
			  listener.Add( 1 );
		 }

		 private static void ConsistencyCheckSchemaIndexes( IndexAccessors indexes, ConsistencyReporter report, ProgressListener listener )
		 {
			  IList<StoreIndexDescriptor> rulesToRemove = new List<StoreIndexDescriptor>();
			  foreach ( StoreIndexDescriptor onlineRule in indexes.OnlineRules() )
			  {
					ConsistencyReporter.FormattingDocumentedHandler handler = report.FormattingHandler( RecordType.INDEX );
					ReporterFactory reporterFactory = new ReporterFactory( handler );
					IndexAccessor accessor = indexes.AccessorFor( onlineRule );
					if ( !accessor.ConsistencyCheck( reporterFactory ) )
					{
						 rulesToRemove.Add( onlineRule );
					}
					handler.UpdateSummary();
					listener.Add( 1 );
			  }
			  foreach ( StoreIndexDescriptor toRemove in rulesToRemove )
			  {
					indexes.Remove( toRemove );
			  }
		 }

		 private static T[] ReadAllRecords<T>( Type type, RecordStore<T> store ) where T : Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord
		 {
				 type = typeof( T );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") T[] records = (T[]) Array.newInstance(type, (int) store.getHighId());
			  T[] records = ( T[] ) Array.CreateInstance( type, ( int ) store.HighId );
			  for ( int i = 0; i < records.Length; i++ )
			  {
					records[i] = store.GetRecord( i, store.NewRecord(), FORCE );
			  }
			  return records;
		 }

		 private class RebuildPreventingCountsInitializer : DataInitializer<Org.Neo4j.Kernel.Impl.Api.CountsAccessor_Updater>
		 {
			 private readonly FullCheck _outerInstance;

			 public RebuildPreventingCountsInitializer( FullCheck outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public override void Initialize( Org.Neo4j.Kernel.Impl.Api.CountsAccessor_Updater updater )
			  {
					throw new System.NotSupportedException( "Counts store needed rebuild, consistency checker will instead report broken or missing counts store" );
			  }

			  public override long InitialVersion()
			  {
					return 0;
			  }
		 }
	}

}