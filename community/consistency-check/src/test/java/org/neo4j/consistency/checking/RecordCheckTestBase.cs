﻿using System;

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
namespace Org.Neo4j.Consistency.checking
{
	using MultiPassStore = Org.Neo4j.Consistency.checking.full.MultiPassStore;
	using Stage = Org.Neo4j.Consistency.checking.full.Stage;
	using ConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport;
	using ConsistencyReport_DynamicConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport_DynamicConsistencyReport;
	using ConsistencyReport_NeoStoreConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport_NeoStoreConsistencyReport;
	using ConsistencyReport_NodeConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport_NodeConsistencyReport;
	using ConsistencyReport_PropertyConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport_PropertyConsistencyReport;
	using ConsistencyReport_PropertyKeyTokenConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport_PropertyKeyTokenConsistencyReport;
	using ConsistencyReport_RelationshipConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport_RelationshipConsistencyReport;
	using ConsistencyReport_RelationshipTypeConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport_RelationshipTypeConsistencyReport;
	using RecordAccess = Org.Neo4j.Consistency.store.RecordAccess;
	using RecordAccessStub = Org.Neo4j.Consistency.store.RecordAccessStub;
	using PropertyType = Org.Neo4j.Kernel.impl.store.PropertyType;
	using Org.Neo4j.Kernel.impl.store;
	using AbstractBaseRecord = Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord;
	using DynamicRecord = Org.Neo4j.Kernel.impl.store.record.DynamicRecord;
	using NeoStoreRecord = Org.Neo4j.Kernel.impl.store.record.NeoStoreRecord;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using PropertyBlock = Org.Neo4j.Kernel.impl.store.record.PropertyBlock;
	using PropertyKeyTokenRecord = Org.Neo4j.Kernel.impl.store.record.PropertyKeyTokenRecord;
	using PropertyRecord = Org.Neo4j.Kernel.impl.store.record.PropertyRecord;
	using RelationshipRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipRecord;
	using RelationshipTypeTokenRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipTypeTokenRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;

	public abstract class RecordCheckTestBase<RECORD, REPORT, CHECKER> where RECORD : Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord where REPORT : Org.Neo4j.Consistency.report.ConsistencyReport where CHECKER : RecordCheck<RECORD, REPORT>
	{
		 public const int NONE = -1;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal readonly CHECKER CheckerConflict;
		 private readonly Type<REPORT> _reportClass;
		 protected internal RecordAccessStub Records;
		 private Stage _stage;

		 internal RecordCheckTestBase( CHECKER checker, Type reportClass, int[] cacheFields, params MultiPassStore[] storesToCheck )
		 {
				 reportClass = typeof( REPORT );
			  this( checker, reportClass, new Org.Neo4j.Consistency.checking.full.Stage_Adapter( false, true, "Test stage", cacheFields ), storesToCheck );
		 }

		 internal RecordCheckTestBase( CHECKER checker, Type reportClass, Stage stage, params MultiPassStore[] storesToCheck )
		 {
				 reportClass = typeof( REPORT );
			  this.CheckerConflict = checker;
			  this._reportClass = reportClass;
			  this._stage = stage;
			  Initialize( storesToCheck );
		 }

		 protected internal virtual void Initialize( params MultiPassStore[] storesToCheck )
		 {
			  this.Records = new RecordAccessStub( _stage, storesToCheck );
			  if ( _stage.CacheSlotSizes.Length > 0 )
			  {
					Records.cacheAccess().CacheSlotSizes = _stage.CacheSlotSizes;
			  }
		 }

		 public static PrimitiveRecordCheck<NodeRecord, ConsistencyReport_NodeConsistencyReport> DummyNodeCheck()
		 {
			  return new NodeRecordCheckAnonymousInnerClass();
		 }

		 private class NodeRecordCheckAnonymousInnerClass : NodeRecordCheck
		 {
			 public override void check( NodeRecord record, CheckerEngine<NodeRecord, ConsistencyReport_NodeConsistencyReport> engine, RecordAccess records )
			 {
			 }
		 }

		 public static PrimitiveRecordCheck<RelationshipRecord, ConsistencyReport_RelationshipConsistencyReport> DummyRelationshipChecker()
		 {
			  return new RelationshipRecordCheckAnonymousInnerClass();
		 }

		 private class RelationshipRecordCheckAnonymousInnerClass : RelationshipRecordCheck
		 {
			 public override void check( RelationshipRecord record, CheckerEngine<RelationshipRecord, ConsistencyReport_RelationshipConsistencyReport> engine, RecordAccess records )
			 {
			 }
		 }

		 public static RecordCheck<PropertyRecord, ConsistencyReport_PropertyConsistencyReport> DummyPropertyChecker()
		 {
			  return ( record, engine, Records ) =>
			  {
			  };
		 }

		 public static PrimitiveRecordCheck<NeoStoreRecord, ConsistencyReport_NeoStoreConsistencyReport> DummyNeoStoreCheck()
		 {
			  return new NeoStoreCheckAnonymousInnerClass();
		 }

		 private class NeoStoreCheckAnonymousInnerClass : NeoStoreCheck
		 {
			 public NeoStoreCheckAnonymousInnerClass() : base(new PropertyChain<>(from -> null))
			 {
			 }

			 public override void check( NeoStoreRecord record, CheckerEngine<NeoStoreRecord, ConsistencyReport_NeoStoreConsistencyReport> engine, RecordAccess records )
			 {
			 }
		 }

		 public static RecordCheck<DynamicRecord, ConsistencyReport_DynamicConsistencyReport> DummyDynamicCheck( RecordStore<DynamicRecord> store, DynamicStore dereference )
		 {
			  return new DynamicRecordCheckAnonymousInnerClass( store, dereference );
		 }

		 private class DynamicRecordCheckAnonymousInnerClass : DynamicRecordCheck
		 {
			 public DynamicRecordCheckAnonymousInnerClass( RecordStore<DynamicRecord> store, Org.Neo4j.Consistency.checking.DynamicStore dereference ) : base( store, dereference )
			 {
			 }

			 public override void check( DynamicRecord record, CheckerEngine<DynamicRecord, ConsistencyReport_DynamicConsistencyReport> engine, RecordAccess records )
			 {
			 }
		 }

		 public static RecordCheck<PropertyKeyTokenRecord, ConsistencyReport_PropertyKeyTokenConsistencyReport> DummyPropertyKeyCheck()
		 {
			  return new PropertyKeyTokenRecordCheckAnonymousInnerClass();
		 }

		 private class PropertyKeyTokenRecordCheckAnonymousInnerClass : PropertyKeyTokenRecordCheck
		 {
			 public override void check( PropertyKeyTokenRecord record, CheckerEngine<PropertyKeyTokenRecord, ConsistencyReport_PropertyKeyTokenConsistencyReport> engine, RecordAccess records )
			 {
			 }
		 }

		 public static RecordCheck<RelationshipTypeTokenRecord, ConsistencyReport_RelationshipTypeConsistencyReport> DummyRelationshipLabelCheck()
		 {
			  return new RelationshipTypeTokenRecordCheckAnonymousInnerClass();
		 }

		 private class RelationshipTypeTokenRecordCheckAnonymousInnerClass : RelationshipTypeTokenRecordCheck
		 {
			 public override void check( RelationshipTypeTokenRecord record, CheckerEngine<RelationshipTypeTokenRecord, ConsistencyReport_RelationshipTypeConsistencyReport> engine, RecordAccess records )
			 {
			 }
		 }

		 internal virtual REPORT Check( RECORD record )
		 {
			  return Check( _reportClass, CheckerConflict, record, Records );
		 }

		 internal virtual void Check( REPORT report, RECORD record )
		 {
			  Check( report, CheckerConflict, record, Records );
		 }

		 internal REPORT Check( CHECKER externalChecker, RECORD record )
		 {
			  return Check( _reportClass, externalChecker, record, Records );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <RECORD extends org.neo4j.kernel.impl.store.record.AbstractBaseRecord, REPORT extends org.neo4j.consistency.report.ConsistencyReport> REPORT check(Class<REPORT> reportClass, RecordCheck<RECORD, REPORT> checker, RECORD record, final org.neo4j.consistency.store.RecordAccessStub records)
		 public static REPORT Check<RECORD, REPORT>( Type reportClass, RecordCheck<RECORD, REPORT> checker, RECORD record, RecordAccessStub records ) where RECORD : Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord where REPORT : Org.Neo4j.Consistency.report.ConsistencyReport
		 {
				 reportClass = typeof( REPORT );
			  REPORT report = mock( reportClass );
			  Check( report, checker, record, records );
			  return report;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <RECORD extends org.neo4j.kernel.impl.store.record.AbstractBaseRecord, REPORT extends org.neo4j.consistency.report.ConsistencyReport> void check(REPORT report, RecordCheck<RECORD, REPORT> checker, RECORD record, final org.neo4j.consistency.store.RecordAccessStub records)
		 public static void Check<RECORD, REPORT>( REPORT report, RecordCheck<RECORD, REPORT> checker, RECORD record, RecordAccessStub records ) where RECORD : Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord where REPORT : Org.Neo4j.Consistency.report.ConsistencyReport
		 {
			  checker.Check( record, records.Engine( record, report ), records );
			  records.CheckDeferred();
		 }

		 internal virtual R AddChange<R>( R oldRecord, R newRecord ) where R : Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord
		 {
			  return Records.addChange( oldRecord, newRecord );
		 }

		 internal virtual R Add<R>( R record ) where R : Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord
		 {
			  return Records.add( record );
		 }

		 internal virtual DynamicRecord AddNodeDynamicLabels( DynamicRecord labels )
		 {
			  return Records.addNodeDynamicLabels( labels );
		 }

		 internal virtual DynamicRecord AddKeyName( DynamicRecord name )
		 {
			  return Records.addPropertyKeyName( name );
		 }

		 internal virtual DynamicRecord AddRelationshipTypeName( DynamicRecord name )
		 {
			  return Records.addRelationshipTypeName( name );
		 }

		 internal virtual DynamicRecord AddLabelName( DynamicRecord name )
		 {
			  return Records.addLabelName( name );
		 }

		 public static DynamicRecord String( DynamicRecord record )
		 {
			  record.SetType( PropertyType.STRING.intValue() );
			  return record;
		 }

		 public static DynamicRecord Array( DynamicRecord record )
		 {
			  record.SetType( PropertyType.ARRAY.intValue() );
			  return record;
		 }

		 internal static PropertyBlock PropertyBlock( PropertyKeyTokenRecord key, DynamicRecord value )
		 {
			  PropertyType type = value.getType();
			  if ( value.getType() != PropertyType.STRING && value.getType() != PropertyType.ARRAY )
			  {
					fail( "Dynamic record must be either STRING or ARRAY" );
					return null;
			  }
			  return PropertyBlock( key, type, value.Id );
		 }

		 public static PropertyBlock PropertyBlock( PropertyKeyTokenRecord key, PropertyType type, long value )
		 {
			  PropertyBlock block = new PropertyBlock();
			  block.SingleBlock = key.Id | ( ( ( long ) type.intValue() ) << 24 ) | (value << 28);
			  return block;
		 }

		 public static R InUse<R>( R record ) where R : Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord
		 {
			  record.InUse = true;
			  return record;
		 }

		 public static R NotInUse<R>( R record ) where R : Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord
		 {
			  record.InUse = false;
			  return record;
		 }

		 protected internal virtual CHECKER Checker()
		 {
			  return CheckerConflict;
		 }
	}

}