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
namespace Neo4Net.Consistency.checking.full
{

	using Neo4Net.Consistency.checking;
	using CacheAccess = Neo4Net.Consistency.checking.cache.CacheAccess;
	using CacheTask = Neo4Net.Consistency.checking.cache.CacheTask;
	using IndexAccessors = Neo4Net.Consistency.checking.index.IndexAccessors;
	using IndexEntryProcessor = Neo4Net.Consistency.checking.index.IndexEntryProcessor;
	using IndexIterator = Neo4Net.Consistency.checking.index.IndexIterator;
	using LabelScanCheck = Neo4Net.Consistency.checking.labelscan.LabelScanCheck;
	using LabelScanDocumentProcessor = Neo4Net.Consistency.checking.labelscan.LabelScanDocumentProcessor;
	using ConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport;
	using ConsistencyReporter = Neo4Net.Consistency.report.ConsistencyReporter;
	using Statistics = Neo4Net.Consistency.statistics.Statistics;
	using IndexRecord = Neo4Net.Consistency.store.synthetic.IndexRecord;
	using LabelScanIndex = Neo4Net.Consistency.store.synthetic.LabelScanIndex;
	using Neo4Net.Helpers.Collection;
	using Iterables = Neo4Net.Helpers.Collection.Iterables;
	using ProgressMonitorFactory = Neo4Net.Helpers.progress.ProgressMonitorFactory;
	using TokenNameLookup = Neo4Net.@internal.Kernel.Api.TokenNameLookup;
	using LabelScanStore = Neo4Net.Kernel.api.labelscan.LabelScanStore;
	using NonTransactionalTokenNameLookup = Neo4Net.Kernel.Impl.Api.NonTransactionalTokenNameLookup;
	using TokenHolders = Neo4Net.Kernel.impl.core.TokenHolders;
	using NativeLabelScanStore = Neo4Net.Kernel.impl.index.labelscan.NativeLabelScanStore;
	using Neo4Net.Kernel.impl.store;
	using Scanner = Neo4Net.Kernel.impl.store.Scanner;
	using SchemaStorage = Neo4Net.Kernel.impl.store.SchemaStorage;
	using StoreAccess = Neo4Net.Kernel.impl.store.StoreAccess;
	using AbstractBaseRecord = Neo4Net.Kernel.impl.store.record.AbstractBaseRecord;
	using EntityType = Neo4Net.Storageengine.Api.EntityType;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.full.MultiPassStore.ARRAYS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.full.MultiPassStore.LABELS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.full.MultiPassStore.NODES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.full.MultiPassStore.PROPERTIES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.full.MultiPassStore.RELATIONSHIPS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.full.MultiPassStore.RELATIONSHIP_GROUPS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.full.MultiPassStore.STRINGS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.full.QueueDistribution.ROUND_ROBIN;

	public class ConsistencyCheckTasks
	{
		 private readonly ProgressMonitorFactory.MultiPartBuilder _multiPartBuilder;
		 private readonly StoreProcessor _defaultProcessor;
		 private readonly StoreAccess _nativeStores;
		 private readonly Statistics _statistics;
		 private readonly TokenHolders _tokenHolders;
		 private readonly MultiPassStore.Factory _multiPass;
		 private readonly ConsistencyReporter _reporter;
		 private readonly LabelScanStore _labelScanStore;
		 private readonly IndexAccessors _indexes;
		 private readonly CacheAccess _cacheAccess;
		 private readonly int _numberOfThreads;

		 internal ConsistencyCheckTasks( ProgressMonitorFactory.MultiPartBuilder multiPartBuilder, StoreProcessor defaultProcessor, StoreAccess nativeStores, Statistics statistics, CacheAccess cacheAccess, LabelScanStore labelScanStore, IndexAccessors indexes, TokenHolders tokenHolders, MultiPassStore.Factory multiPass, ConsistencyReporter reporter, int numberOfThreads )
		 {
			  this._multiPartBuilder = multiPartBuilder;
			  this._defaultProcessor = defaultProcessor;
			  this._nativeStores = nativeStores;
			  this._statistics = statistics;
			  this._cacheAccess = cacheAccess;
			  this._tokenHolders = tokenHolders;
			  this._multiPass = multiPass;
			  this._reporter = reporter;
			  this._labelScanStore = labelScanStore;
			  this._indexes = indexes;
			  this._numberOfThreads = numberOfThreads;
		 }

		 public virtual IList<ConsistencyCheckerTask> CreateTasksForFullCheck( bool checkLabelScanStore, bool checkIndexes, bool checkGraph )
		 {
			  IList<ConsistencyCheckerTask> tasks = new List<ConsistencyCheckerTask>();
			  if ( checkGraph )
			  {
					MandatoryProperties mandatoryProperties = new MandatoryProperties( _nativeStores );
					StoreProcessor processor = _multiPass.processor( CheckStage.Stage1NSPropsLabels, PROPERTIES );
					tasks.Add( Create( CheckStage.Stage1NSPropsLabels.name(), _nativeStores.NodeStore, processor, ROUND_ROBIN ) );
					//RelationshipStore pass - check label counts using cached labels, check properties, skip nodes and relationships
					processor = _multiPass.processor( CheckStage.Stage2RSLabels, LABELS );
					_multiPass.reDecorateRelationship( processor, RelationshipRecordCheck.relationshipRecordCheckForwardPass() );
					tasks.Add( Create( CheckStage.Stage2RSLabels.name(), _nativeStores.RelationshipStore, processor, ROUND_ROBIN ) );
					//NodeStore pass - just cache nextRel and inUse
					tasks.Add( new CacheTask.CacheNextRel( CheckStage.Stage3NSNextRel, _cacheAccess, Scanner.scan( _nativeStores.NodeStore ) ) );
					//RelationshipStore pass - check nodes inUse, FirstInFirst, FirstInSecond using cached info
					processor = _multiPass.processor( CheckStage.Stage4RSNextRel, NODES );
					_multiPass.reDecorateRelationship( processor, RelationshipRecordCheck.relationshipRecordCheckBackwardPass( new PropertyChain<>( mandatoryProperties.ForRelationships( _reporter ) ) ) );
					tasks.Add( Create( CheckStage.Stage4RSNextRel.name(), _nativeStores.RelationshipStore, processor, ROUND_ROBIN ) );
					//NodeStore pass - just cache nextRel and inUse
					_multiPass.reDecorateNode( processor, NodeRecordCheck.toCheckNextRel(), true );
					_multiPass.reDecorateNode( processor, NodeRecordCheck.toCheckNextRelationshipGroup(), false );
					tasks.Add( new CacheTask.CheckNextRel( CheckStage.Stage5CheckNextRel, _cacheAccess, _nativeStores, processor ) );
					// source chain
					//RelationshipStore pass - forward scan of source chain using the cache.
					processor = _multiPass.processor( CheckStage.Stage6RSForward, RELATIONSHIPS );
					_multiPass.reDecorateRelationship( processor, RelationshipRecordCheck.relationshipRecordCheckSourceChain() );
					tasks.Add( Create( CheckStage.Stage6RSForward.name(), _nativeStores.RelationshipStore, processor, QueueDistribution.RELATIONSHIPS ) );
					//RelationshipStore pass - reverse scan of source chain using the cache.
					processor = _multiPass.processor( CheckStage.Stage7RSBackward, RELATIONSHIPS );
					_multiPass.reDecorateRelationship( processor, RelationshipRecordCheck.relationshipRecordCheckSourceChain() );
					tasks.Add( Create( CheckStage.Stage7RSBackward.name(), _nativeStores.RelationshipStore, processor, QueueDistribution.RELATIONSHIPS ) );

					//relationshipGroup
					StoreProcessor relGrpProcessor = _multiPass.processor( Stage_Fields.ParallelForward, RELATIONSHIP_GROUPS );
					tasks.Add( Create( "RelationshipGroupStore-RelGrp", _nativeStores.RelationshipGroupStore, relGrpProcessor, ROUND_ROBIN ) );

					PropertyReader propertyReader = new PropertyReader( _nativeStores );
					tasks.Add( RecordScanner( CheckStage.Stage8PSProps.name(), new IterableStore<>(_nativeStores.NodeStore, true), new PropertyAndNode2LabelIndexProcessor(_reporter, checkIndexes ? _indexes : null, propertyReader, _cacheAccess, mandatoryProperties.ForNodes(_reporter)), CheckStage.Stage8PSProps, ROUND_ROBIN, new IterableStore<>(_nativeStores.PropertyStore, true) ) );

					// Checking that relationships are in their expected relationship indexes.
					IList<StoreIndexDescriptor> relationshipIndexes = Iterables.stream( _indexes.onlineRules() ).filter(rule => rule.schema().entityType() == EntityType.RELATIONSHIP).collect(Collectors.toList());
					if ( checkIndexes && relationshipIndexes.Count > 0 )
					{
						 tasks.Add( RecordScanner( CheckStage.Stage9RSIndexes.name(), new IterableStore<>(_nativeStores.RelationshipStore, true), new RelationshipIndexProcessor(_reporter, _indexes, propertyReader, relationshipIndexes), CheckStage.Stage9RSIndexes, ROUND_ROBIN, new IterableStore<>(_nativeStores.PropertyStore, true) ) );
					}

					tasks.Add( Create( "StringStore-Str", _nativeStores.StringStore, _multiPass.processor( Stage_Fields.SequentialForward, STRINGS ), ROUND_ROBIN ) );
					tasks.Add( Create( "ArrayStore-Arrays", _nativeStores.ArrayStore, _multiPass.processor( Stage_Fields.SequentialForward, ARRAYS ), ROUND_ROBIN ) );
			  }
			  // The schema store is verified in multiple passes that share state since it fits into memory
			  // and we care about the consistency of back references (cf. SemanticCheck)
			  // PASS 1: Dynamic record chains
			  tasks.Add( Create( "SchemaStore", _nativeStores.SchemaStore, ROUND_ROBIN ) );
			  // PASS 2: Rule integrity and obligation build up
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.consistency.checking.SchemaRecordCheck schemaCheck = new org.neo4j.consistency.checking.SchemaRecordCheck(new org.neo4j.kernel.impl.store.SchemaStorage(nativeStores.getSchemaStore()), indexes);
			  SchemaRecordCheck schemaCheck = new SchemaRecordCheck( new SchemaStorage( _nativeStores.SchemaStore ), _indexes );
			  tasks.Add( new SchemaStoreProcessorTask<>( "SchemaStoreProcessor-check_rules", _statistics, _numberOfThreads, _nativeStores.SchemaStore, _nativeStores, "check_rules", schemaCheck, _multiPartBuilder, _cacheAccess, _defaultProcessor, ROUND_ROBIN ) );
			  // PASS 3: Obligation verification and semantic rule uniqueness
			  tasks.Add( new SchemaStoreProcessorTask<>( "SchemaStoreProcessor-check_obligations", _statistics, _numberOfThreads, _nativeStores.SchemaStore, _nativeStores, "check_obligations", schemaCheck.ForObligationChecking(), _multiPartBuilder, _cacheAccess, _defaultProcessor, ROUND_ROBIN ) );
			  if ( checkGraph )
			  {
					tasks.Add( Create( "RelationshipTypeTokenStore", _nativeStores.RelationshipTypeTokenStore, ROUND_ROBIN ) );
					tasks.Add( Create( "PropertyKeyTokenStore", _nativeStores.PropertyKeyTokenStore, ROUND_ROBIN ) );
					tasks.Add( Create( "LabelTokenStore", _nativeStores.LabelTokenStore, ROUND_ROBIN ) );
					tasks.Add( Create( "RelationshipTypeNameStore", _nativeStores.RelationshipTypeNameStore, ROUND_ROBIN ) );
					tasks.Add( Create( "PropertyKeyNameStore", _nativeStores.PropertyKeyNameStore, ROUND_ROBIN ) );
					tasks.Add( Create( "LabelNameStore", _nativeStores.LabelNameStore, ROUND_ROBIN ) );
					tasks.Add( Create( "NodeDynamicLabelStore", _nativeStores.NodeDynamicLabelStore, ROUND_ROBIN ) );
			  }

			  ConsistencyReporter filteredReporter = _multiPass.reporter( NODES );
			  if ( checkLabelScanStore )
			  {
					long highId = _nativeStores.NodeStore.HighId;
					tasks.Add( new LabelIndexDirtyCheckTask( this ) );
					tasks.Add( RecordScanner( "LabelScanStore", new GapFreeAllEntriesLabelScanReader( _labelScanStore.allNodeLabelRanges(), highId ), new LabelScanDocumentProcessor(filteredReporter, new LabelScanCheck()), Stage_Fields.SequentialForward, ROUND_ROBIN ) );
			  }
			  if ( checkIndexes )
			  {
					tasks.Add( new IndexDirtyCheckTask( this ) );
					TokenNameLookup tokenNameLookup = new NonTransactionalTokenNameLookup( _tokenHolders, true );
					foreach ( StoreIndexDescriptor indexRule in _indexes.onlineRules() )
					{
						 tasks.Add( RecordScanner( format( "Index_%d", indexRule.Id ), new IndexIterator( _indexes.accessorFor( indexRule ) ), new IndexEntryProcessor( filteredReporter, new IndexCheck( indexRule ), indexRule, tokenNameLookup ), Stage_Fields.SequentialForward, ROUND_ROBIN ) );
					}
			  }
			  return tasks;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: private <RECORD> RecordScanner<RECORD> recordScanner(String name, org.neo4j.helpers.collection.BoundedIterable<RECORD> store, RecordProcessor<RECORD> processor, Stage stage, QueueDistribution distribution, @SuppressWarnings("rawtypes") IterableStore... warmupStores)
		 private RecordScanner<RECORD> RecordScanner<RECORD>( string name, BoundedIterable<RECORD> store, RecordProcessor<RECORD> processor, Stage stage, QueueDistribution distribution, params IterableStore[] warmupStores )
		 {
			  return stage.Parallel ? new ParallelRecordScanner<RECORD>( name, _statistics, _numberOfThreads, store, _multiPartBuilder, processor, _cacheAccess, distribution, warmupStores ) : new SequentialRecordScanner<RECORD>( name, _statistics, _numberOfThreads, store, _multiPartBuilder, processor, warmupStores );
		 }

		 private StoreProcessorTask<RECORD> Create<RECORD>( string name, RecordStore<RECORD> input, QueueDistribution distribution ) where RECORD : Neo4Net.Kernel.impl.store.record.AbstractBaseRecord
		 {
			  return new StoreProcessorTask<RECORD>( name, _statistics, _numberOfThreads, input, _nativeStores, name, _multiPartBuilder, _cacheAccess, _defaultProcessor, distribution );
		 }

		 private StoreProcessorTask<RECORD> Create<RECORD>( string name, RecordStore<RECORD> input, StoreProcessor processor, QueueDistribution distribution ) where RECORD : Neo4Net.Kernel.impl.store.record.AbstractBaseRecord
		 {
			  return new StoreProcessorTask<RECORD>( name, _statistics, _numberOfThreads, input, _nativeStores, name, _multiPartBuilder, _cacheAccess, processor, distribution );
		 }

		 private class LabelIndexDirtyCheckTask : ConsistencyCheckerTask
		 {
			 private readonly ConsistencyCheckTasks _outerInstance;

			  internal LabelIndexDirtyCheckTask( ConsistencyCheckTasks outerInstance ) : base( "Label index dirty check", Statistics.NONE, 1 )
			  {
				  this._outerInstance = outerInstance;
			  }

			  public override void Run()
			  {
					if ( outerInstance.labelScanStore is NativeLabelScanStore )
					{
						 if ( ( ( NativeLabelScanStore )outerInstance.labelScanStore ).Dirty )
						 {
							  outerInstance.reporter.Report( new LabelScanIndex( outerInstance.labelScanStore.LabelScanStoreFile ), typeof( Neo4Net.Consistency.report.ConsistencyReport_LabelScanConsistencyReport ), RecordType.LABEL_SCAN_DOCUMENT ).dirtyIndex();
						 }
					}
			  }
		 }

		 private class IndexDirtyCheckTask : ConsistencyCheckerTask
		 {
			 private readonly ConsistencyCheckTasks _outerInstance;

			  internal IndexDirtyCheckTask( ConsistencyCheckTasks outerInstance ) : base( "Indexes dirty check", Statistics.NONE, 1 )
			  {
				  this._outerInstance = outerInstance;
			  }

			  public override void Run()
			  {
					foreach ( StoreIndexDescriptor indexRule in outerInstance.indexes.OnlineRules() )
					{
						 if ( outerInstance.indexes.AccessorFor( indexRule ).Dirty )
						 {
							  outerInstance.reporter.Report( new IndexRecord( indexRule ), typeof( Neo4Net.Consistency.report.ConsistencyReport_IndexConsistencyReport ), RecordType.INDEX ).dirtyIndex();
						 }
					}

			  }
		 }
	}

}