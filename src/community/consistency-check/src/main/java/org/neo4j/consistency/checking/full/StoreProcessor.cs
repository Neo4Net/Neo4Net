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
namespace Neo4Net.Consistency.checking.full
{
	using Neo4Net.Consistency.checking;
	using CacheAccess = Neo4Net.Consistency.checking.cache.CacheAccess;
	using Neo4Net.Consistency.checking.full;
	using ConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport;
	using ConsistencyReport_DynamicLabelConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport_DynamicLabelConsistencyReport;
	using ConsistencyReport_RelationshipGroupConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport_RelationshipGroupConsistencyReport;
	using Counts = Neo4Net.Consistency.statistics.Counts;
	using Neo4Net.GraphDb;
	using Neo4Net.GraphDb;
	using ProgressListener = Neo4Net.Helpers.progress.ProgressListener;
	using Neo4Net.Kernel.impl.store;
	using AbstractBaseRecord = Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord;
	using DynamicRecord = Neo4Net.Kernel.Impl.Store.Records.DynamicRecord;
	using LabelTokenRecord = Neo4Net.Kernel.Impl.Store.Records.LabelTokenRecord;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using PropertyKeyTokenRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyKeyTokenRecord;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;
	using RelationshipGroupRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;
	using RelationshipTypeTokenRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipTypeTokenRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.consistency.checking.cache.DefaultCacheAccess.DEFAULT_QUEUE_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.consistency.checking.full.CloningRecordIterator.cloned;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.consistency.checking.full.RecordDistributor.distributeRecords;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.Scanner.scan;

	/// <summary>
	/// Full check works by spawning StoreProcessorTasks that call StoreProcessor. StoreProcessor.applyFiltered()
	/// then scans the store and in turn calls down to store.accept which then knows how to check the given record.
	/// </summary>
	public class StoreProcessor : AbstractStoreProcessor
	{
		 private readonly int _qSize = DEFAULT_QUEUE_SIZE;
		 protected internal readonly CacheAccess CacheAccess;
		 private readonly Neo4Net.Consistency.report.ConsistencyReport_Reporter _report;
		 private SchemaRecordCheck _schemaRecordCheck;
		 private readonly Stage _stage;

		 public StoreProcessor( CheckDecorator decorator, Neo4Net.Consistency.report.ConsistencyReport_Reporter report, Stage stage, CacheAccess cacheAccess ) : base( decorator )
		 {
			  Debug.Assert( stage != null );
			  this._report = report;
			  this._stage = stage;
			  this.CacheAccess = cacheAccess;
		 }

		 public virtual Stage Stage
		 {
			 get
			 {
				  return _stage;
			 }
		 }

		 public override void ProcessNode( RecordStore<NodeRecord> store, NodeRecord node )
		 {
			  CacheAccess.client().incAndGetCount(node.Dense ? Neo4Net.Consistency.statistics.Counts_Type.NodeDense : Neo4Net.Consistency.statistics.Counts_Type.NodeSparse);
			  base.ProcessNode( store, node );
		 }

		 protected internal virtual void CheckSchema( RecordType type, RecordStore<DynamicRecord> store, DynamicRecord schema, RecordCheck<DynamicRecord, Neo4Net.Consistency.report.ConsistencyReport_SchemaConsistencyReport> checker )
		 {
			  _report.forSchema( schema, checker );
		 }

		 protected internal override void CheckNode( RecordStore<NodeRecord> store, NodeRecord node, RecordCheck<NodeRecord, Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport> checker )
		 {
			  _report.forNode( node, checker );
		 }

		 public virtual void CountLinks( long id1, long id2, Neo4Net.Consistency.checking.cache.CacheAccess_Client client )
		 {
			  Neo4Net.Consistency.statistics.Counts_Type type = null;
			  if ( id2 == -1 )
			  {
					type = Neo4Net.Consistency.statistics.Counts_Type.NullLinks;
			  }
			  else if ( id2 > id1 )
			  {
					type = Neo4Net.Consistency.statistics.Counts_Type.ForwardLinks;
			  }
			  else
			  {
					type = Neo4Net.Consistency.statistics.Counts_Type.BackLinks;
			  }
			  client.IncAndGetCount( type );
		 }

		 protected internal override void CheckRelationship( RecordStore<RelationshipRecord> store, RelationshipRecord rel, RecordCheck<RelationshipRecord, Neo4Net.Consistency.report.ConsistencyReport_RelationshipConsistencyReport> checker )
		 {
			  if ( _stage != null && ( _stage == CheckStage.Stage6RSForward || _stage == CheckStage.Stage7RSBackward ) )
			  {
					long id = rel.Id;
					Neo4Net.Consistency.checking.cache.CacheAccess_Client client = CacheAccess.client();
					CountLinks( id, rel.FirstNextRel, client );
					CountLinks( id, rel.FirstPrevRel, client );
					CountLinks( id, rel.SecondNextRel, client );
					CountLinks( id, rel.SecondPrevRel, client );
			  }
			  _report.forRelationship( rel, checker );
		 }

		 protected internal override void CheckProperty( RecordStore<PropertyRecord> store, PropertyRecord property, RecordCheck<PropertyRecord, Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport> checker )
		 {
			  _report.forProperty( property, checker );
		 }

		 protected internal override void CheckRelationshipTypeToken( RecordStore<RelationshipTypeTokenRecord> store, RelationshipTypeTokenRecord relationshipType, RecordCheck<RelationshipTypeTokenRecord, Neo4Net.Consistency.report.ConsistencyReport_RelationshipTypeConsistencyReport> checker )
		 {
			  _report.forRelationshipTypeName( relationshipType, checker );
		 }

		 protected internal override void CheckLabelToken( RecordStore<LabelTokenRecord> store, LabelTokenRecord label, RecordCheck<LabelTokenRecord, Neo4Net.Consistency.report.ConsistencyReport_LabelTokenConsistencyReport> checker )
		 {
			  _report.forLabelName( label, checker );
		 }

		 protected internal override void CheckPropertyKeyToken( RecordStore<PropertyKeyTokenRecord> store, PropertyKeyTokenRecord key, RecordCheck<PropertyKeyTokenRecord, Neo4Net.Consistency.report.ConsistencyReport_PropertyKeyTokenConsistencyReport> checker )
		 {
			  _report.forPropertyKey( key, checker );
		 }

		 protected internal override void CheckDynamic( RecordType type, RecordStore<DynamicRecord> store, DynamicRecord @string, RecordCheck<DynamicRecord, Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport> checker )
		 {
			  _report.forDynamicBlock( type, @string, checker );
		 }

		 protected internal override void CheckDynamicLabel( RecordType type, RecordStore<DynamicRecord> store, DynamicRecord @string, RecordCheck<DynamicRecord, ConsistencyReport_DynamicLabelConsistencyReport> checker )
		 {
			  _report.forDynamicLabelBlock( type, @string, checker );
		 }

		 protected internal override void CheckRelationshipGroup( RecordStore<RelationshipGroupRecord> store, RelationshipGroupRecord record, RecordCheck<RelationshipGroupRecord, ConsistencyReport_RelationshipGroupConsistencyReport> checker )
		 {
			  _report.forRelationshipGroup( record, checker );
		 }

		 internal virtual SchemaRecordCheck SchemaRecordCheck
		 {
			 set
			 {
				  this._schemaRecordCheck = value;
			 }
		 }

		 public override void ProcessSchema( RecordStore<DynamicRecord> store, DynamicRecord schema )
		 {
			  if ( null == _schemaRecordCheck )
			  {
					base.ProcessSchema( store, schema );
			  }
			  else
			  {
					CheckSchema( RecordType.SCHEMA, store, schema, _schemaRecordCheck );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public <R extends org.Neo4Net.kernel.impl.store.record.AbstractBaseRecord> void applyFilteredParallel(final org.Neo4Net.kernel.impl.store.RecordStore<R> store, final org.Neo4Net.helpers.progress.ProgressListener progressListener, int numberOfThreads, long recordsPerCpu, final org.Neo4Net.consistency.checking.full.QueueDistribution_QueueDistributor<R> distributor)
		 public virtual void ApplyFilteredParallel<R>( RecordStore<R> store, ProgressListener progressListener, int numberOfThreads, long recordsPerCpu, QueueDistribution_QueueDistributor<R> distributor ) where R : Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord
		 {
			  CacheAccess.prepareForProcessingOfSingleStore( recordsPerCpu );
			  RecordProcessor<R> processor = new RecordProcessor_AdapterAnonymousInnerClass( this, store );

			  ResourceIterable<R> scan = scan( store, _stage.Forward );
			  using ( ResourceIterator<R> records = scan.GetEnumerator() )
			  {
					distributeRecords( numberOfThreads, this.GetType().Name, _qSize, cloned(records), progressListener, processor, distributor );
			  }
		 }

		 private class RecordProcessor_AdapterAnonymousInnerClass : RecordProcessor_Adapter<R>
		 {
			 private readonly StoreProcessor _outerInstance;

			 private RecordStore<R> _store;

			 public RecordProcessor_AdapterAnonymousInnerClass( StoreProcessor outerInstance, RecordStore<R> store )
			 {
				 this.outerInstance = outerInstance;
				 this._store = store;
			 }

			 public override void init( int id )
			 {
				  // Thread id assignment happens here, so do this before processing. Calles to this init
				  // method is ordered externally.
				  _outerInstance.cacheAccess.client();
			 }

			 public override void process( R record )
			 {
				  _store.accept( _outerInstance, record );
			 }
		 }
	}

}