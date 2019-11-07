using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

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
	using Neo4Net.Consistency.checking;
	using Neo4Net.Consistency.checking;
	using Neo4Net.Consistency.checking;
	using ConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport;
	using ConsistencyReport_DynamicConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport;
	using ConsistencyReport_LabelTokenConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport_LabelTokenConsistencyReport;
	using ConsistencyReport_NeoStoreConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport_NeoStoreConsistencyReport;
	using ConsistencyReport_NodeConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport;
	using ConsistencyReport_PropertyConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport;
	using ConsistencyReport_PropertyKeyTokenConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport_PropertyKeyTokenConsistencyReport;
	using ConsistencyReport_RelationshipConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport_RelationshipConsistencyReport;
	using ConsistencyReport_RelationshipGroupConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport_RelationshipGroupConsistencyReport;
	using ConsistencyReport_RelationshipTypeConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport_RelationshipTypeConsistencyReport;
	using RecordAccess = Neo4Net.Consistency.Store.RecordAccess;
	using ProgressListener = Neo4Net.Helpers.progress.ProgressListener;
	using ProgressMonitorFactory = Neo4Net.Helpers.progress.ProgressMonitorFactory;
	using PropertyType = Neo4Net.Kernel.impl.store.PropertyType;
	using DynamicRecord = Neo4Net.Kernel.Impl.Store.Records.DynamicRecord;
	using LabelTokenRecord = Neo4Net.Kernel.Impl.Store.Records.LabelTokenRecord;
	using NeoStoreRecord = Neo4Net.Kernel.Impl.Store.Records.NeoStoreRecord;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using PrimitiveRecord = Neo4Net.Kernel.Impl.Store.Records.PrimitiveRecord;
	using PropertyBlock = Neo4Net.Kernel.Impl.Store.Records.PropertyBlock;
	using PropertyKeyTokenRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyKeyTokenRecord;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;
	using Record = Neo4Net.Kernel.Impl.Store.Records.Record;
	using RelationshipGroupRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;
	using RelationshipTypeTokenRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipTypeTokenRecord;
	using TokenRecord = Neo4Net.Kernel.Impl.Store.Records.TokenRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.consistency.RecordType.ARRAY_PROPERTY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.consistency.RecordType.PROPERTY_KEY_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.consistency.RecordType.RELATIONSHIP_TYPE_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.consistency.RecordType.STRING_PROPERTY;

	internal class OwnerCheck : CheckDecorator
	{
		 private readonly ConcurrentMap<long, PropertyOwner> _owners;
		 private readonly IDictionary<RecordType, ConcurrentMap<long, DynamicOwner>> _dynamics;

		 internal OwnerCheck( bool active, params DynamicStore[] stores )
		 {
			  this._owners = active ? new ConcurrentDictionary<long, PropertyOwner>( 16, 0.75f, 4 ) : null;
			  this._dynamics = active ? Initialize( stores ) : null;
		 }

		 private static IDictionary<RecordType, ConcurrentMap<long, DynamicOwner>> Initialize( DynamicStore[] stores )
		 {
			  Dictionary<RecordType, ConcurrentMap<long, DynamicOwner>> map = new Dictionary<RecordType, ConcurrentMap<long, DynamicOwner>>( typeof( RecordType ) );
			  foreach ( DynamicStore store in stores )
			  {
					map[store.type] = new ConcurrentDictionary<long, DynamicOwner>( 16, 0.75f, 4 );
			  }
			  return unmodifiableMap( map );
		 }

		 internal virtual void ScanForOrphanChains( ProgressMonitorFactory progressFactory )
		 {
			  IList<ThreadStart> tasks = new List<ThreadStart>();
			  ProgressMonitorFactory.MultiPartBuilder progress = progressFactory.MultipleParts( "Checking for orphan chains" );
			  if ( _owners != null )
			  {
					tasks.Add( new OrphanCheck( RecordType.PROPERTY, _owners, progress ) );
			  }
			  if ( _dynamics != null )
			  {
					foreach ( KeyValuePair<RecordType, ConcurrentMap<long, DynamicOwner>> entry in _dynamics.SetOfKeyValuePairs() )
					{
						 tasks.Add( new OrphanCheck( entry.Key, entry.Value, progress ) );
					}
			  }
			  foreach ( ThreadStart task in tasks )
			  {
					task.run();
			  }
		 }

		 private class OrphanCheck : ThreadStart
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.concurrent.ConcurrentMap<long, ? extends Owner> owners;
			  internal readonly ConcurrentMap<long, ? extends Owner> Owners;
			  internal readonly ProgressListener Progress;

			  internal OrphanCheck<T1>( RecordType property, ConcurrentMap<T1> owners, ProgressMonitorFactory.MultiPartBuilder progress ) where T1 : Owner
			  {
					this.Owners = owners;
					this.Progress = progress.ProgressForPart( "Checking for orphan " + property.name() + " chains", owners.size() );
			  }

			  public override void Run()
			  {
					foreach ( Owner owner in Owners.values() )
					{
						 owner.CheckOrphanage();
						 Progress.add( 1 );
					}
					Progress.done();
			  }
		 }

		 public override void Prepare()
		 {
		 }

		 public override OwningRecordCheck<NeoStoreRecord, ConsistencyReport_NeoStoreConsistencyReport> DecorateNeoStoreChecker( OwningRecordCheck<NeoStoreRecord, ConsistencyReport_NeoStoreConsistencyReport> checker )
		 {
			  if ( _owners == null )
			  {
					return checker;
			  }
			  return new PrimitiveCheckerDecoratorAnonymousInnerClass( this, checker );
		 }

		 private class PrimitiveCheckerDecoratorAnonymousInnerClass : PrimitiveCheckerDecorator<NeoStoreRecord, ConsistencyReport_NeoStoreConsistencyReport>
		 {
			 private readonly OwnerCheck _outerInstance;

			 public PrimitiveCheckerDecoratorAnonymousInnerClass( OwnerCheck outerInstance, OwningRecordCheck<NeoStoreRecord, ConsistencyReport_NeoStoreConsistencyReport> checker ) : base( outerInstance, checker )
			 {
				 this.outerInstance = outerInstance;
			 }

			 internal override PropertyOwner owner( NeoStoreRecord record )
			 {
				  return PropertyOwner.OWNING_GRAPH;
			 }
		 }

		 public override OwningRecordCheck<NodeRecord, ConsistencyReport_NodeConsistencyReport> DecorateNodeChecker( OwningRecordCheck<NodeRecord, ConsistencyReport_NodeConsistencyReport> checker )
		 {
			  if ( _owners == null )
			  {
					return checker;
			  }
			  return new PrimitiveCheckerDecoratorAnonymousInnerClass2( this, checker );
		 }

		 private class PrimitiveCheckerDecoratorAnonymousInnerClass2 : PrimitiveCheckerDecorator<NodeRecord, ConsistencyReport_NodeConsistencyReport>
		 {
			 private readonly OwnerCheck _outerInstance;

			 public PrimitiveCheckerDecoratorAnonymousInnerClass2( OwnerCheck outerInstance, OwningRecordCheck<NodeRecord, ConsistencyReport_NodeConsistencyReport> checker ) : base( outerInstance, checker )
			 {
				 this.outerInstance = outerInstance;
			 }

			 internal override PropertyOwner owner( NodeRecord record )
			 {
				  return new PropertyOwner.OwningNode( record );
			 }
		 }

		 public override OwningRecordCheck<RelationshipRecord, ConsistencyReport_RelationshipConsistencyReport> DecorateRelationshipChecker( OwningRecordCheck<RelationshipRecord, ConsistencyReport_RelationshipConsistencyReport> checker )
		 {
			  if ( _owners == null )
			  {
					return checker;
			  }
			  return new PrimitiveCheckerDecoratorAnonymousInnerClass3( this, checker );
		 }

		 private class PrimitiveCheckerDecoratorAnonymousInnerClass3 : PrimitiveCheckerDecorator<RelationshipRecord, ConsistencyReport_RelationshipConsistencyReport>
		 {
			 private readonly OwnerCheck _outerInstance;

			 public PrimitiveCheckerDecoratorAnonymousInnerClass3( OwnerCheck outerInstance, OwningRecordCheck<RelationshipRecord, ConsistencyReport_RelationshipConsistencyReport> checker ) : base( outerInstance, checker )
			 {
				 this.outerInstance = outerInstance;
			 }

			 internal override PropertyOwner owner( RelationshipRecord record )
			 {
				  return new PropertyOwner.OwningRelationship( record );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public Neo4Net.consistency.checking.RecordCheck<Neo4Net.kernel.impl.store.record.PropertyRecord, Neo4Net.consistency.report.ConsistencyReport_PropertyConsistencyReport> decoratePropertyChecker(final Neo4Net.consistency.checking.RecordCheck<Neo4Net.kernel.impl.store.record.PropertyRecord, Neo4Net.consistency.report.ConsistencyReport_PropertyConsistencyReport> checker)
		 public override RecordCheck<PropertyRecord, ConsistencyReport_PropertyConsistencyReport> DecoratePropertyChecker( RecordCheck<PropertyRecord, ConsistencyReport_PropertyConsistencyReport> checker )
		 {
			  if ( _owners == null && _dynamics == null )
			  {
					return checker;
			  }
			  return ( record, engine, records ) =>
			  {
				if ( record.inUse() )
				{
					 if ( _owners != null && Record.NO_PREVIOUS_PROPERTY.@is( record.PrevProp ) )
					 { // this record is first in a chain
						  PropertyOwner.UnknownOwner owner = new PropertyOwner.UnknownOwner();
						  engine.comparativeCheck( owner, _orphanChecker );
						  if ( null == _owners.putIfAbsent( record.Id, owner ) )
						  {
								owner.MarkInCustody();
						  }
					 }
					 if ( _dynamics != null )
					 {
						  foreach ( PropertyBlock block in record )
						  {
								RecordType type = RecordType( block.forceGetType() );
								if ( type != null )
								{
									 ConcurrentMap<long, DynamicOwner> dynamicOwners = _dynamics[type];
									 if ( dynamicOwners != null )
									 {
										  long id = block.SingleValueLong;
										  DynamicOwner.Property owner = new DynamicOwner.Property( type, record );
										  DynamicOwner prev = dynamicOwners.put( id, owner );
										  if ( prev != null )
										  {
												engine.comparativeCheck( prev.record( records ), owner );
										  }
									 }
								}
						  }
					 }
				}
				checker.Check( record, engine, records );
			  };
		 }

		 private RecordType RecordType( PropertyType type )
		 {
			  if ( type == null )
			  {
					return null;
			  }

			  switch ( type.innerEnumValue )
			  {
			  case PropertyType.InnerEnum.STRING:
					return STRING_PROPERTY;
			  case PropertyType.InnerEnum.ARRAY:
					return ARRAY_PROPERTY;
			  default:
					return null;
			  }
		 }

		 public override RecordCheck<PropertyKeyTokenRecord, ConsistencyReport_PropertyKeyTokenConsistencyReport> DecoratePropertyKeyTokenChecker( RecordCheck<PropertyKeyTokenRecord, ConsistencyReport_PropertyKeyTokenConsistencyReport> checker )
		 {
			  ConcurrentMap<long, DynamicOwner> dynamicOwners = dynamicOwners( PROPERTY_KEY_NAME );
			  if ( dynamicOwners == null )
			  {
					return checker;
			  }
			  return new NameCheckerDecoratorAnonymousInnerClass( this, checker, dynamicOwners );
		 }

		 private class NameCheckerDecoratorAnonymousInnerClass : NameCheckerDecorator<PropertyKeyTokenRecord, ConsistencyReport_PropertyKeyTokenConsistencyReport>
		 {
			 private readonly OwnerCheck _outerInstance;

			 public NameCheckerDecoratorAnonymousInnerClass( OwnerCheck outerInstance, RecordCheck<PropertyKeyTokenRecord, ConsistencyReport_PropertyKeyTokenConsistencyReport> checker, ConcurrentMap<long, DynamicOwner> dynamicOwners ) : base( checker, dynamicOwners )
			 {
				 this.outerInstance = outerInstance;
			 }

			 internal override DynamicOwner.NameOwner owner( PropertyKeyTokenRecord record )
			 {
				  return new DynamicOwner.PropertyKey( record );
			 }
		 }

		 public override RecordCheck<RelationshipTypeTokenRecord, ConsistencyReport_RelationshipTypeConsistencyReport> DecorateRelationshipTypeTokenChecker( RecordCheck<RelationshipTypeTokenRecord, ConsistencyReport_RelationshipTypeConsistencyReport> checker )
		 {
			  ConcurrentMap<long, DynamicOwner> dynamicOwners = dynamicOwners( RELATIONSHIP_TYPE_NAME );
			  if ( dynamicOwners == null )
			  {
					return checker;
			  }
			  return new NameCheckerDecoratorAnonymousInnerClass2( this, checker, dynamicOwners );
		 }

		 private class NameCheckerDecoratorAnonymousInnerClass2 : NameCheckerDecorator<RelationshipTypeTokenRecord, ConsistencyReport_RelationshipTypeConsistencyReport>
		 {
			 private readonly OwnerCheck _outerInstance;

			 public NameCheckerDecoratorAnonymousInnerClass2( OwnerCheck outerInstance, RecordCheck<RelationshipTypeTokenRecord, ConsistencyReport_RelationshipTypeConsistencyReport> checker, ConcurrentMap<long, DynamicOwner> dynamicOwners ) : base( checker, dynamicOwners )
			 {
				 this.outerInstance = outerInstance;
			 }

			 internal override DynamicOwner.NameOwner owner( RelationshipTypeTokenRecord record )
			 {
				  return new DynamicOwner.RelationshipTypeToken( record );
			 }
		 }

		 public override RecordCheck<LabelTokenRecord, ConsistencyReport_LabelTokenConsistencyReport> DecorateLabelTokenChecker( RecordCheck<LabelTokenRecord, ConsistencyReport_LabelTokenConsistencyReport> checker )
		 {
			  ConcurrentMap<long, DynamicOwner> dynamicOwners = dynamicOwners( RELATIONSHIP_TYPE_NAME );
			  if ( dynamicOwners == null )
			  {
					return checker;
			  }
			  return new NameCheckerDecoratorAnonymousInnerClass3( this, checker, dynamicOwners );
		 }

		 private class NameCheckerDecoratorAnonymousInnerClass3 : NameCheckerDecorator<LabelTokenRecord, ConsistencyReport_LabelTokenConsistencyReport>
		 {
			 private readonly OwnerCheck _outerInstance;

			 public NameCheckerDecoratorAnonymousInnerClass3( OwnerCheck outerInstance, RecordCheck<LabelTokenRecord, ConsistencyReport_LabelTokenConsistencyReport> checker, ConcurrentMap<long, DynamicOwner> dynamicOwners ) : base( checker, dynamicOwners )
			 {
				 this.outerInstance = outerInstance;
			 }

			 internal override DynamicOwner.NameOwner owner( LabelTokenRecord record )
			 {
				  return new DynamicOwner.LabelToken( record );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: Neo4Net.consistency.checking.RecordCheck<Neo4Net.kernel.impl.store.record.DynamicRecord, Neo4Net.consistency.report.ConsistencyReport_DynamicConsistencyReport> decorateDynamicChecker(final Neo4Net.consistency.RecordType type, final Neo4Net.consistency.checking.RecordCheck<Neo4Net.kernel.impl.store.record.DynamicRecord, Neo4Net.consistency.report.ConsistencyReport_DynamicConsistencyReport> checker)
		 internal virtual RecordCheck<DynamicRecord, ConsistencyReport_DynamicConsistencyReport> DecorateDynamicChecker( RecordType type, RecordCheck<DynamicRecord, ConsistencyReport_DynamicConsistencyReport> checker )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.ConcurrentMap<long, DynamicOwner> dynamicOwners = dynamicOwners(type);
			  ConcurrentMap<long, DynamicOwner> dynamicOwners = dynamicOwners( type );
			  if ( dynamicOwners == null )
			  {
					return checker;
			  }
			  return ( record, engine, records ) =>
			  {
				if ( record.inUse() )
				{
					 DynamicOwner.Unknown owner = new DynamicOwner.Unknown();
					 engine.comparativeCheck( owner, DynamicOwner.OrphanCheck );
					 if ( null == dynamicOwners.putIfAbsent( record.Id, owner ) )
					 {
						  owner.MarkInCustody();
					 }
					 if ( !Record.NO_NEXT_BLOCK.@is( record.NextBlock ) )
					 {
						  DynamicOwner.Dynamic nextOwner = new DynamicOwner.Dynamic( type, record );
						  DynamicOwner prevOwner = dynamicOwners.put( record.NextBlock, nextOwner );
						  if ( prevOwner != null )
						  {
								engine.comparativeCheck( prevOwner.record( records ), nextOwner );
						  }
					 }
				}
				checker.Check( record, engine, records );
			  };
		 }

		 public override RecordCheck<RelationshipGroupRecord, ConsistencyReport_RelationshipGroupConsistencyReport> DecorateRelationshipGroupChecker( RecordCheck<RelationshipGroupRecord, ConsistencyReport_RelationshipGroupConsistencyReport> checker )
		 {
			  return checker;
		 }

		 private ConcurrentMap<long, DynamicOwner> DynamicOwners( RecordType type )
		 {
			  return _dynamics == null ? null : _dynamics[type];
		 }

		 private abstract class PrimitiveCheckerDecorator<RECORD, REPORT> : OwningRecordCheck<RECORD, REPORT> where RECORD : Neo4Net.Kernel.Impl.Store.Records.PrimitiveRecord where REPORT : Neo4Net.Consistency.report.ConsistencyReport_PrimitiveConsistencyReport
		 {
			 private readonly OwnerCheck _outerInstance;

			  internal readonly OwningRecordCheck<RECORD, REPORT> Checker;

			  internal PrimitiveCheckerDecorator( OwnerCheck outerInstance, OwningRecordCheck<RECORD, REPORT> checker )
			  {
				  this._outerInstance = outerInstance;
					this.Checker = checker;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("unchecked") public void check(RECORD record, Neo4Net.consistency.checking.CheckerEngine<RECORD, REPORT> engine, Neo4Net.consistency.store.RecordAccess records)
			  public override void Check( RECORD record, CheckerEngine<RECORD, REPORT> engine, RecordAccess records )
			  {
					if ( record.inUse() )
					{
						 long prop = record.NextProp;
						 if ( !Record.NO_NEXT_PROPERTY.@is( prop ) )
						 {
							  PropertyOwner previous = outerInstance.owners.put( prop, Owner( record ) );
							  if ( previous != null )
							  {
									engine.ComparativeCheck( previous.record( records ), Checker.ownerCheck() );
							  }
						 }
					}
					Checker.check( record, engine, records );
			  }

			  public override ComparativeRecordChecker<RECORD, PrimitiveRecord, REPORT> OwnerCheck()
			  {
					return Checker.ownerCheck();
			  }

			  internal abstract PropertyOwner Owner( RECORD record );
		 }

		 private abstract class NameCheckerDecorator <RECORD, REPORT> : RecordCheck<RECORD, REPORT> where RECORD : Neo4Net.Kernel.Impl.Store.Records.TokenRecord where REPORT : Neo4Net.Consistency.report.ConsistencyReport_NameConsistencyReport
		 {
			  internal readonly RecordCheck<RECORD, REPORT> Checker;
			  internal readonly ConcurrentMap<long, DynamicOwner> Owners;

			  internal NameCheckerDecorator( RecordCheck<RECORD, REPORT> checker, ConcurrentMap<long, DynamicOwner> owners )
			  {
					this.Checker = checker;
					this.Owners = owners;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Override public void check(RECORD record, Neo4Net.consistency.checking.CheckerEngine<RECORD, REPORT> engine, Neo4Net.consistency.store.RecordAccess records)
			  public override void Check( RECORD record, CheckerEngine<RECORD, REPORT> engine, RecordAccess records )
			  {
					if ( record.inUse() )
					{
						 DynamicOwner.NameOwner owner = owner( record );
						 DynamicOwner prev = Owners.put( ( long )record.NameId, owner );
						 if ( prev != null )
						 {
							  engine.ComparativeCheck( prev.record( records ), owner );
						 }
					}
					Checker.check( record, engine, records );
			  }

			  internal abstract DynamicOwner.NameOwner Owner( RECORD record );
		 }

		 private static readonly ComparativeRecordChecker<PropertyRecord, PrimitiveRecord, ConsistencyReport_PropertyConsistencyReport> _orphanChecker = ( record, primitiveRecord, engine, records ) => engine.report().orphanPropertyChain();
	}

}