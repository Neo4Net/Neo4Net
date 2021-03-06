﻿using System.Diagnostics;

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
	using Org.Neo4j.Consistency.checking;
	using CacheAccess = Org.Neo4j.Consistency.checking.cache.CacheAccess;
	using Org.Neo4j.Consistency.checking.full;
	using ConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport;
	using ConsistencyReport_DynamicLabelConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport_DynamicLabelConsistencyReport;
	using ConsistencyReport_RelationshipGroupConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport_RelationshipGroupConsistencyReport;
	using Counts = Org.Neo4j.Consistency.statistics.Counts;
	using Org.Neo4j.Graphdb;
	using Org.Neo4j.Graphdb;
	using ProgressListener = Org.Neo4j.Helpers.progress.ProgressListener;
	using Org.Neo4j.Kernel.impl.store;
	using AbstractBaseRecord = Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord;
	using DynamicRecord = Org.Neo4j.Kernel.impl.store.record.DynamicRecord;
	using LabelTokenRecord = Org.Neo4j.Kernel.impl.store.record.LabelTokenRecord;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using PropertyKeyTokenRecord = Org.Neo4j.Kernel.impl.store.record.PropertyKeyTokenRecord;
	using PropertyRecord = Org.Neo4j.Kernel.impl.store.record.PropertyRecord;
	using RelationshipGroupRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipGroupRecord;
	using RelationshipRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipRecord;
	using RelationshipTypeTokenRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipTypeTokenRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.cache.DefaultCacheAccess.DEFAULT_QUEUE_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.full.CloningRecordIterator.cloned;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.full.RecordDistributor.distributeRecords;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.Scanner.scan;

	/// <summary>
	/// Full check works by spawning StoreProcessorTasks that call StoreProcessor. StoreProcessor.applyFiltered()
	/// then scans the store and in turn calls down to store.accept which then knows how to check the given record.
	/// </summary>
	public class StoreProcessor : AbstractStoreProcessor
	{
		 private readonly int _qSize = DEFAULT_QUEUE_SIZE;
		 protected internal readonly CacheAccess CacheAccess;
		 private readonly Org.Neo4j.Consistency.report.ConsistencyReport_Reporter _report;
		 private SchemaRecordCheck _schemaRecordCheck;
		 private readonly Stage _stage;

		 public StoreProcessor( CheckDecorator decorator, Org.Neo4j.Consistency.report.ConsistencyReport_Reporter report, Stage stage, CacheAccess cacheAccess ) : base( decorator )
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
			  CacheAccess.client().incAndGetCount(node.Dense ? Org.Neo4j.Consistency.statistics.Counts_Type.NodeDense : Org.Neo4j.Consistency.statistics.Counts_Type.NodeSparse);
			  base.ProcessNode( store, node );
		 }

		 protected internal virtual void CheckSchema( RecordType type, RecordStore<DynamicRecord> store, DynamicRecord schema, RecordCheck<DynamicRecord, Org.Neo4j.Consistency.report.ConsistencyReport_SchemaConsistencyReport> checker )
		 {
			  _report.forSchema( schema, checker );
		 }

		 protected internal override void CheckNode( RecordStore<NodeRecord> store, NodeRecord node, RecordCheck<NodeRecord, Org.Neo4j.Consistency.report.ConsistencyReport_NodeConsistencyReport> checker )
		 {
			  _report.forNode( node, checker );
		 }

		 public virtual void CountLinks( long id1, long id2, Org.Neo4j.Consistency.checking.cache.CacheAccess_Client client )
		 {
			  Org.Neo4j.Consistency.statistics.Counts_Type type = null;
			  if ( id2 == -1 )
			  {
					type = Org.Neo4j.Consistency.statistics.Counts_Type.NullLinks;
			  }
			  else if ( id2 > id1 )
			  {
					type = Org.Neo4j.Consistency.statistics.Counts_Type.ForwardLinks;
			  }
			  else
			  {
					type = Org.Neo4j.Consistency.statistics.Counts_Type.BackLinks;
			  }
			  client.IncAndGetCount( type );
		 }

		 protected internal override void CheckRelationship( RecordStore<RelationshipRecord> store, RelationshipRecord rel, RecordCheck<RelationshipRecord, Org.Neo4j.Consistency.report.ConsistencyReport_RelationshipConsistencyReport> checker )
		 {
			  if ( _stage != null && ( _stage == CheckStage.Stage6RSForward || _stage == CheckStage.Stage7RSBackward ) )
			  {
					long id = rel.Id;
					Org.Neo4j.Consistency.checking.cache.CacheAccess_Client client = CacheAccess.client();
					CountLinks( id, rel.FirstNextRel, client );
					CountLinks( id, rel.FirstPrevRel, client );
					CountLinks( id, rel.SecondNextRel, client );
					CountLinks( id, rel.SecondPrevRel, client );
			  }
			  _report.forRelationship( rel, checker );
		 }

		 protected internal override void CheckProperty( RecordStore<PropertyRecord> store, PropertyRecord property, RecordCheck<PropertyRecord, Org.Neo4j.Consistency.report.ConsistencyReport_PropertyConsistencyReport> checker )
		 {
			  _report.forProperty( property, checker );
		 }

		 protected internal override void CheckRelationshipTypeToken( RecordStore<RelationshipTypeTokenRecord> store, RelationshipTypeTokenRecord relationshipType, RecordCheck<RelationshipTypeTokenRecord, Org.Neo4j.Consistency.report.ConsistencyReport_RelationshipTypeConsistencyReport> checker )
		 {
			  _report.forRelationshipTypeName( relationshipType, checker );
		 }

		 protected internal override void CheckLabelToken( RecordStore<LabelTokenRecord> store, LabelTokenRecord label, RecordCheck<LabelTokenRecord, Org.Neo4j.Consistency.report.ConsistencyReport_LabelTokenConsistencyReport> checker )
		 {
			  _report.forLabelName( label, checker );
		 }

		 protected internal override void CheckPropertyKeyToken( RecordStore<PropertyKeyTokenRecord> store, PropertyKeyTokenRecord key, RecordCheck<PropertyKeyTokenRecord, Org.Neo4j.Consistency.report.ConsistencyReport_PropertyKeyTokenConsistencyReport> checker )
		 {
			  _report.forPropertyKey( key, checker );
		 }

		 protected internal override void CheckDynamic( RecordType type, RecordStore<DynamicRecord> store, DynamicRecord @string, RecordCheck<DynamicRecord, Org.Neo4j.Consistency.report.ConsistencyReport_DynamicConsistencyReport> checker )
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
//ORIGINAL LINE: public <R extends org.neo4j.kernel.impl.store.record.AbstractBaseRecord> void applyFilteredParallel(final org.neo4j.kernel.impl.store.RecordStore<R> store, final org.neo4j.helpers.progress.ProgressListener progressListener, int numberOfThreads, long recordsPerCpu, final org.neo4j.consistency.checking.full.QueueDistribution_QueueDistributor<R> distributor)
		 public virtual void ApplyFilteredParallel<R>( RecordStore<R> store, ProgressListener progressListener, int numberOfThreads, long recordsPerCpu, QueueDistribution_QueueDistributor<R> distributor ) where R : Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord
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