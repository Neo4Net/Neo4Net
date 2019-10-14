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
namespace Neo4Net.Consistency.checking.full
{
	using MutableObjectLongMap = org.eclipse.collections.api.map.primitive.MutableObjectLongMap;
	using ObjectLongHashMap = org.eclipse.collections.impl.map.mutable.primitive.ObjectLongHashMap;


	using Neo4Net.Consistency.checking;
	using Neo4Net.Consistency.checking;
	using Neo4Net.Consistency.checking;
	using CacheAccess = Neo4Net.Consistency.checking.cache.CacheAccess;
	using ConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport;
	using ConsistencyReport_NodeConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport;
	using ConsistencyReport_RelationshipConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport_RelationshipConsistencyReport;
	using ConsistencyReporter = Neo4Net.Consistency.report.ConsistencyReporter;
	using RecordAccess = Neo4Net.Consistency.store.RecordAccess;
	using CountsEntry = Neo4Net.Consistency.store.synthetic.CountsEntry;
	using ProgressListener = Neo4Net.Helpers.progress.ProgressListener;
	using ProgressMonitorFactory = Neo4Net.Helpers.progress.ProgressMonitorFactory;
	using CountsAccessor = Neo4Net.Kernel.Impl.Api.CountsAccessor;
	using CountsVisitor = Neo4Net.Kernel.Impl.Api.CountsVisitor;
	using NodeLabelsField = Neo4Net.Kernel.impl.store.NodeLabelsField;
	using NodeStore = Neo4Net.Kernel.impl.store.NodeStore;
	using Neo4Net.Kernel.impl.store;
	using StoreAccess = Neo4Net.Kernel.impl.store.StoreAccess;
	using CountsKey = Neo4Net.Kernel.impl.store.counts.keys.CountsKey;
	using AbstractBaseRecord = Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using PrimitiveRecord = Neo4Net.Kernel.Impl.Store.Records.PrimitiveRecord;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.cache.CacheSlots_NodeLabel_Fields.SLOT_IN_USE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.cache.CacheSlots_NodeLabel_Fields.SLOT_LABEL_FIELD;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.full.NodeLabelReader.getListOfLabels;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.counts.keys.CountsKeyFactory.nodeKey;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.counts.keys.CountsKeyFactory.relationshipKey;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.RecordLoad.FORCE;

	internal class CountsBuilderDecorator : Neo4Net.Consistency.checking.CheckDecorator_Adapter
	{
		 private const int WILDCARD = -1;
		 private readonly MutableObjectLongMap<CountsKey> _nodeCounts = new ObjectLongHashMap<CountsKey>();
		 private readonly MutableObjectLongMap<CountsKey> _relationshipCounts = new ObjectLongHashMap<CountsKey>();
		 private readonly MultiPassAvoidanceCondition<NodeRecord> _nodeCountBuildCondition;
		 private readonly MultiPassAvoidanceCondition<RelationshipRecord> _relationshipCountBuildCondition;
		 private readonly NodeStore _nodeStore;
		 private readonly StoreAccess _storeAccess;
		 private readonly CountsEntry.CheckAdapter CHECK_NODE_COUNT = new CheckAdapterAnonymousInnerClass();

		 private class CheckAdapterAnonymousInnerClass : CountsEntry.CheckAdapter
		 {
			 public override void check( CountsEntry record, CheckerEngine<CountsEntry, Neo4Net.Consistency.report.ConsistencyReport_CountsConsistencyReport> engine, RecordAccess records )
			 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long expectedCount = nodeCounts.get(record.getCountsKey());
				  long expectedCount = outerInstance.nodeCounts.get( record.CountsKey );
				  if ( expectedCount != record.Count )
				  {
						engine.Report().inconsistentNodeCount(expectedCount);
				  }
			 }
		 }
		 private readonly CountsEntry.CheckAdapter CHECK_RELATIONSHIP_COUNT = new CheckAdapterAnonymousInnerClass2();

		 private class CheckAdapterAnonymousInnerClass2 : CountsEntry.CheckAdapter
		 {
			 public override void check( CountsEntry record, CheckerEngine<CountsEntry, Neo4Net.Consistency.report.ConsistencyReport_CountsConsistencyReport> engine, RecordAccess records )
			 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long expectedCount = relationshipCounts.get(record.getCountsKey());
				  long expectedCount = outerInstance.relationshipCounts.get( record.CountsKey );
				  if ( expectedCount != record.Count )
				  {
						engine.Report().inconsistentRelationshipCount(expectedCount);
				  }
			 }
		 }
		 private readonly CountsEntry.CheckAdapter CHECK_NODE_KEY_COUNT = new CheckAdapterAnonymousInnerClass3();

		 private class CheckAdapterAnonymousInnerClass3 : CountsEntry.CheckAdapter
		 {
			 public override void check( CountsEntry record, CheckerEngine<CountsEntry, Neo4Net.Consistency.report.ConsistencyReport_CountsConsistencyReport> engine, RecordAccess records )
			 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int expectedCount = nodeCounts.size();
				  int expectedCount = outerInstance.nodeCounts.size();
				  if ( record.Count != expectedCount )
				  {
						engine.Report().inconsistentNumberOfNodeKeys(expectedCount);
				  }
			 }
		 }
		 private readonly CountsEntry.CheckAdapter CHECK_RELATIONSHIP_KEY_COUNT = new CheckAdapterAnonymousInnerClass4();

		 private class CheckAdapterAnonymousInnerClass4 : CountsEntry.CheckAdapter
		 {
			 public override void check( CountsEntry record, CheckerEngine<CountsEntry, Neo4Net.Consistency.report.ConsistencyReport_CountsConsistencyReport> engine, RecordAccess records )
			 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int expectedCount = relationshipCounts.size();
				  int expectedCount = outerInstance.relationshipCounts.size();
				  if ( record.Count != expectedCount )
				  {
						engine.Report().inconsistentNumberOfRelationshipKeys(expectedCount);
				  }
			 }
		 }

		 internal CountsBuilderDecorator( StoreAccess storeAccess )
		 {
			  this._storeAccess = storeAccess;
			  this._nodeStore = storeAccess.RawNeoStores.NodeStore;
			  this._nodeCountBuildCondition = new MultiPassAvoidanceCondition<NodeRecord>( 0 );
			  this._relationshipCountBuildCondition = new MultiPassAvoidanceCondition<RelationshipRecord>( 1 );
		 }

		 public override void Prepare()
		 {
			  this._nodeCountBuildCondition.prepare();
			  this._relationshipCountBuildCondition.prepare();
		 }

		 public override OwningRecordCheck<NodeRecord, ConsistencyReport_NodeConsistencyReport> DecorateNodeChecker( OwningRecordCheck<NodeRecord, ConsistencyReport_NodeConsistencyReport> checker )
		 {
			  return new NodeCounts( _nodeStore, _nodeCounts, _nodeCountBuildCondition, checker );
		 }

		 public override OwningRecordCheck<RelationshipRecord, ConsistencyReport_RelationshipConsistencyReport> DecorateRelationshipChecker( OwningRecordCheck<RelationshipRecord, ConsistencyReport_RelationshipConsistencyReport> checker )
		 {
			  return new RelationshipCounts( _storeAccess, _relationshipCounts, _relationshipCountBuildCondition, checker );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public void checkCounts(org.neo4j.kernel.impl.api.CountsAccessor counts, final org.neo4j.consistency.report.ConsistencyReporter reporter, org.neo4j.helpers.progress.ProgressMonitorFactory progressFactory)
		 public virtual void CheckCounts( CountsAccessor counts, ConsistencyReporter reporter, ProgressMonitorFactory progressFactory )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int nodes = nodeCounts.size();
			  int nodes = _nodeCounts.size();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int relationships = relationshipCounts.size();
			  int relationships = _relationshipCounts.size();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int total = nodes + relationships;
			  int total = nodes + relationships;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicInteger nodeEntries = new java.util.concurrent.atomic.AtomicInteger(0);
			  AtomicInteger nodeEntries = new AtomicInteger( 0 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicInteger relationshipEntries = new java.util.concurrent.atomic.AtomicInteger(0);
			  AtomicInteger relationshipEntries = new AtomicInteger( 0 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.helpers.progress.ProgressListener listener = progressFactory.singlePart("Checking node and relationship counts", total);
			  ProgressListener listener = progressFactory.SinglePart( "Checking node and relationship counts", total );
			  listener.Started();
			  Counts.accept( new CountsVisitor_AdapterAnonymousInnerClass( this, reporter, nodeEntries, relationshipEntries, listener ) );
			  reporter.ForCounts( new CountsEntry( nodeKey( WILDCARD ), nodeEntries.get() ), CHECK_NODE_KEY_COUNT );
			  reporter.ForCounts( new CountsEntry( relationshipKey( WILDCARD, WILDCARD, WILDCARD ), relationshipEntries.get() ), CHECK_RELATIONSHIP_KEY_COUNT );
			  listener.Done();
		 }

		 private class CountsVisitor_AdapterAnonymousInnerClass : Neo4Net.Kernel.Impl.Api.CountsVisitor_Adapter
		 {
			 private readonly CountsBuilderDecorator _outerInstance;

			 private ConsistencyReporter _reporter;
			 private AtomicInteger _nodeEntries;
			 private AtomicInteger _relationshipEntries;
			 private ProgressListener _listener;

			 public CountsVisitor_AdapterAnonymousInnerClass( CountsBuilderDecorator outerInstance, ConsistencyReporter reporter, AtomicInteger nodeEntries, AtomicInteger relationshipEntries, ProgressListener listener )
			 {
				 this.outerInstance = outerInstance;
				 this._reporter = reporter;
				 this._nodeEntries = nodeEntries;
				 this._relationshipEntries = relationshipEntries;
				 this._listener = listener;
			 }

			 public override void visitNodeCount( int labelId, long count )
			 {
				  _nodeEntries.incrementAndGet();
				  _reporter.forCounts( new CountsEntry( nodeKey( labelId ), count ), CHECK_NODE_COUNT );
				  _listener.add( 1 );
			 }

			 public override void visitRelationshipCount( int startLabelId, int relTypeId, int endLabelId, long count )
			 {
				  _relationshipEntries.incrementAndGet();
				  _reporter.forCounts( new CountsEntry( relationshipKey( startLabelId, relTypeId, endLabelId ), count ), CHECK_RELATIONSHIP_COUNT );
				  _listener.add( 1 );
			 }
		 }

		 private class NodeCounts : OwningRecordCheck<NodeRecord, ConsistencyReport_NodeConsistencyReport>
		 {
			  internal readonly RecordStore<NodeRecord> NodeStore;
			  internal readonly MutableObjectLongMap<CountsKey> Counts;
			  internal readonly System.Predicate<NodeRecord> CountUpdateCondition;
			  internal readonly OwningRecordCheck<NodeRecord, ConsistencyReport_NodeConsistencyReport> Inner;

			  internal NodeCounts( RecordStore<NodeRecord> nodeStore, MutableObjectLongMap<CountsKey> counts, System.Predicate<NodeRecord> countUpdateCondition, OwningRecordCheck<NodeRecord, ConsistencyReport_NodeConsistencyReport> inner )
			  {
					this.NodeStore = nodeStore;
					this.Counts = counts;
					this.CountUpdateCondition = countUpdateCondition;
					this.Inner = inner;
			  }

			  public override ComparativeRecordChecker<NodeRecord, PrimitiveRecord, ConsistencyReport_NodeConsistencyReport> OwnerCheck()
			  {
					return Inner.ownerCheck();
			  }

			  public override void Check( NodeRecord record, CheckerEngine<NodeRecord, ConsistencyReport_NodeConsistencyReport> engine, RecordAccess records )
			  {
					if ( CountUpdateCondition.test( record ) )
					{
						 if ( record.InUse() )
						 {
							  Neo4Net.Consistency.checking.cache.CacheAccess_Client client = records.CacheAccess().client();
							  client.PutToCacheSingle( record.Id, SLOT_IN_USE, 1 );
							  client.PutToCacheSingle( record.Id, SLOT_LABEL_FIELD, record.LabelField );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Set<long> labels = labelsFor(nodeStore, engine, records, record.getId());
							  ISet<long> labels = LabelsFor( NodeStore, engine, records, record.Id );
							  lock ( Counts )
							  {
									Counts.addToValue( nodeKey( WILDCARD ), 1 );
									foreach ( long label in labels )
									{
										 Counts.addToValue( nodeKey( ( int ) label ), 1 );
									}
							  }
						 }
					}
					Inner.check( record, engine, records );
			  }
		 }

		 private class RelationshipCounts : OwningRecordCheck<RelationshipRecord, ConsistencyReport_RelationshipConsistencyReport>
		 {
			  /// <summary>
			  /// Don't support these counts at the moment so don't compute them </summary>
			  internal const bool COMPUTE_DOUBLE_SIDED_RELATIONSHIP_COUNTS = false;
			  internal readonly NodeStore NodeStore;
			  internal readonly MutableObjectLongMap<CountsKey> Counts;
			  internal readonly System.Predicate<RelationshipRecord> CountUpdateCondition;
			  internal readonly OwningRecordCheck<RelationshipRecord, ConsistencyReport_RelationshipConsistencyReport> Inner;

			  internal RelationshipCounts( StoreAccess storeAccess, MutableObjectLongMap<CountsKey> counts, System.Predicate<RelationshipRecord> countUpdateCondition, OwningRecordCheck<RelationshipRecord, ConsistencyReport_RelationshipConsistencyReport> inner )
			  {
					this.NodeStore = storeAccess.RawNeoStores.NodeStore;
					this.Counts = counts;
					this.CountUpdateCondition = countUpdateCondition;
					this.Inner = inner;
			  }

			  public override ComparativeRecordChecker<RelationshipRecord, PrimitiveRecord, ConsistencyReport_RelationshipConsistencyReport> OwnerCheck()
			  {
					return Inner.ownerCheck();
			  }

			  public override void Check( RelationshipRecord record, CheckerEngine<RelationshipRecord, ConsistencyReport_RelationshipConsistencyReport> engine, RecordAccess records )
			  {
					if ( CountUpdateCondition.test( record ) )
					{
						 if ( record.InUse() )
						 {
							  Neo4Net.Consistency.checking.cache.CacheAccess_Client cacheAccess = records.CacheAccess().client();
							  ISet<long> firstNodeLabels;
							  ISet<long> secondNodeLabels;
							  long firstLabelsField = cacheAccess.GetFromCache( record.FirstNode, SLOT_LABEL_FIELD );
							  if ( NodeLabelsField.fieldPointsToDynamicRecordOfLabels( firstLabelsField ) )
							  {
									firstNodeLabels = LabelsFor( NodeStore, engine, records, record.FirstNode );
							  }
							  else
							  {
									firstNodeLabels = NodeLabelReader.GetListOfLabels( firstLabelsField );
							  }
							  long secondLabelsField = cacheAccess.GetFromCache( record.SecondNode, SLOT_LABEL_FIELD );
							  if ( NodeLabelsField.fieldPointsToDynamicRecordOfLabels( secondLabelsField ) )
							  {
									secondNodeLabels = LabelsFor( NodeStore, engine, records, record.SecondNode );
							  }
							  else
							  {
									secondNodeLabels = NodeLabelReader.GetListOfLabels( secondLabelsField );
							  }
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int type = record.getType();
							  int type = record.Type;
							  lock ( Counts )
							  {
									Counts.addToValue( relationshipKey( WILDCARD, WILDCARD, WILDCARD ), 1 );
									Counts.addToValue( relationshipKey( WILDCARD, type, WILDCARD ), 1 );
									if ( firstNodeLabels != null )
									{
										 foreach ( long firstLabel in firstNodeLabels )
										 {
											  Counts.addToValue( relationshipKey( ( int ) firstLabel, WILDCARD, WILDCARD ), 1 );
											  Counts.addToValue( relationshipKey( ( int ) firstLabel, type, WILDCARD ), 1 );
										 }
									}
									if ( secondNodeLabels != null )
									{
										 foreach ( long secondLabel in secondNodeLabels )
										 {
											  Counts.addToValue( relationshipKey( WILDCARD, WILDCARD, ( int ) secondLabel ), 1 );
											  Counts.addToValue( relationshipKey( WILDCARD, type, ( int ) secondLabel ), 1 );
										 }
									}
									if ( COMPUTE_DOUBLE_SIDED_RELATIONSHIP_COUNTS )
									{
										 foreach ( long firstLabel in firstNodeLabels )
										 {
											  foreach ( long secondLabel in secondNodeLabels )
											  {
													Counts.addToValue( relationshipKey( ( int ) firstLabel, WILDCARD, ( int ) secondLabel ), 1 );
													Counts.addToValue( relationshipKey( ( int ) firstLabel, type, ( int ) secondLabel ), 1 );
											  }
										 }
									}
							  }
						 }
					}
					Inner.check( record, engine, records );
			  }
		 }

		 private class MultiPassAvoidanceCondition<T> : System.Predicate<T> where T : Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord
		 {
			  // Stage which this condition is active, starting from 0, mimicking the CheckStage ordinal
			  internal readonly int ActiveStage;
			  // The same thread updates this every time, the TaskExecutor. Other threads read it
			  internal volatile int Stage = -1;

			  internal MultiPassAvoidanceCondition( int activeStage )
			  {
					this.ActiveStage = activeStage;
			  }

			  public virtual void Prepare()
			  {
					Stage++;
			  }

			  public override bool Test( T record )
			  {
					return Stage == ActiveStage;
			  }
		 }

		 private static ISet<long> LabelsFor<T1>( RecordStore<NodeRecord> nodeStore, CheckerEngine<T1> engine, RecordAccess recordAccess, long nodeId ) where T1 : Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord
		 {
			  return getListOfLabels( nodeStore.GetRecord( nodeId, nodeStore.NewRecord(), FORCE ), recordAccess, engine );
		 }
	}

}