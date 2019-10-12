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
namespace Neo4Net.Consistency.store
{
	using InvocationOnMock = org.mockito.invocation.InvocationOnMock;
	using Answer = org.mockito.stubbing.Answer;


	using Neo4Net.Consistency.checking;
	using Neo4Net.Consistency.checking;
	using CacheAccess = Neo4Net.Consistency.checking.cache.CacheAccess;
	using CacheTask = Neo4Net.Consistency.checking.cache.CacheTask;
	using DefaultCacheAccess = Neo4Net.Consistency.checking.cache.DefaultCacheAccess;
	using CheckStage = Neo4Net.Consistency.checking.full.CheckStage;
	using MultiPassStore = Neo4Net.Consistency.checking.full.MultiPassStore;
	using Stage = Neo4Net.Consistency.checking.full.Stage;
	using ConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport;
	using Neo4Net.Consistency.report;
	using Counts = Neo4Net.Consistency.statistics.Counts;
	using ArrayUtil = Neo4Net.Helpers.ArrayUtil;
	using Neo4Net.Helpers.Collection;
	using Neo4Net.Helpers.Collection;
	using PropertyType = Neo4Net.Kernel.impl.store.PropertyType;
	using AbstractBaseRecord = Neo4Net.Kernel.impl.store.record.AbstractBaseRecord;
	using DynamicRecord = Neo4Net.Kernel.impl.store.record.DynamicRecord;
	using LabelTokenRecord = Neo4Net.Kernel.impl.store.record.LabelTokenRecord;
	using NeoStoreRecord = Neo4Net.Kernel.impl.store.record.NeoStoreRecord;
	using NodeRecord = Neo4Net.Kernel.impl.store.record.NodeRecord;
	using PropertyKeyTokenRecord = Neo4Net.Kernel.impl.store.record.PropertyKeyTokenRecord;
	using PropertyRecord = Neo4Net.Kernel.impl.store.record.PropertyRecord;
	using Record = Neo4Net.Kernel.impl.store.record.Record;
	using RelationshipGroupRecord = Neo4Net.Kernel.impl.store.record.RelationshipGroupRecord;
	using RelationshipRecord = Neo4Net.Kernel.impl.store.record.RelationshipRecord;
	using RelationshipTypeTokenRecord = Neo4Net.Kernel.impl.store.record.RelationshipTypeTokenRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.isNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doAnswer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.resourceIterable;

	public class RecordAccessStub : RecordAccess
	{
		 public const int SCHEMA_RECORD_TYPE = 255;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public <RECORD extends org.neo4j.kernel.impl.store.record.AbstractBaseRecord, REPORT extends org.neo4j.consistency.report.ConsistencyReport> org.neo4j.consistency.checking.CheckerEngine<RECORD, REPORT> engine(final RECORD record, final REPORT report)
		 public virtual CheckerEngine<RECORD, REPORT> Engine<RECORD, REPORT>( RECORD record, REPORT report ) where RECORD : Neo4Net.Kernel.impl.store.record.AbstractBaseRecord where REPORT : Neo4Net.Consistency.report.ConsistencyReport
		 {
			  return new EngineAnonymousInnerClass( this, report, record );
		 }

		 private class EngineAnonymousInnerClass : Engine<RECORD, REPORT>
		 {
			 private readonly RecordAccessStub _outerInstance;

			 private AbstractBaseRecord _record;

			 public EngineAnonymousInnerClass( RecordAccessStub outerInstance, ConsistencyReport report, AbstractBaseRecord record ) : base( outerInstance, report )
			 {
				 this.outerInstance = outerInstance;
				 this._record = record;
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("unchecked") void checkReference(org.neo4j.consistency.checking.ComparativeRecordChecker checker, org.neo4j.kernel.impl.store.record.AbstractBaseRecord oldReference, org.neo4j.kernel.impl.store.record.AbstractBaseRecord newReference)
			 internal override void checkReference( ComparativeRecordChecker checker, AbstractBaseRecord oldReference, AbstractBaseRecord newReference )
			 {
				  checker.checkReference( _record, newReference, this, _outerInstance );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public <RECORD extends org.neo4j.kernel.impl.store.record.AbstractBaseRecord, REPORT extends org.neo4j.consistency.report.ConsistencyReport> org.neo4j.consistency.checking.CheckerEngine<RECORD, REPORT> engine(final RECORD oldRecord, final RECORD newRecord, REPORT report)
		 public virtual CheckerEngine<RECORD, REPORT> Engine<RECORD, REPORT>( RECORD oldRecord, RECORD newRecord, REPORT report ) where RECORD : Neo4Net.Kernel.impl.store.record.AbstractBaseRecord where REPORT : Neo4Net.Consistency.report.ConsistencyReport
		 {
			  return new EngineAnonymousInnerClass2( this, report, newRecord );
		 }

		 private class EngineAnonymousInnerClass2 : Engine<RECORD, REPORT>
		 {
			 private readonly RecordAccessStub _outerInstance;

			 private AbstractBaseRecord _newRecord;

			 public EngineAnonymousInnerClass2( RecordAccessStub outerInstance, ConsistencyReport report, AbstractBaseRecord newRecord ) : base( outerInstance, report )
			 {
				 this.outerInstance = outerInstance;
				 this._newRecord = newRecord;
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("unchecked") void checkReference(org.neo4j.consistency.checking.ComparativeRecordChecker checker, org.neo4j.kernel.impl.store.record.AbstractBaseRecord oldReference, org.neo4j.kernel.impl.store.record.AbstractBaseRecord newReference)
			 internal override void checkReference( ComparativeRecordChecker checker, AbstractBaseRecord oldReference, AbstractBaseRecord newReference )
			 {
				  checker.checkReference( _newRecord, newReference, this, _outerInstance );
			 }
		 }

		 private abstract class Engine<RECORD, REPORT> : CheckerEngine<RECORD, REPORT> where RECORD : Neo4Net.Kernel.impl.store.record.AbstractBaseRecord where REPORT : Neo4Net.Consistency.report.ConsistencyReport
		 {
			 private readonly RecordAccessStub _outerInstance;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly REPORT ReportConflict;

			  protected internal Engine( RecordAccessStub outerInstance, REPORT report )
			  {
				  this._outerInstance = outerInstance;
					this.ReportConflict = report;
			  }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <REFERRED extends org.neo4j.kernel.impl.store.record.AbstractBaseRecord> void comparativeCheck(final RecordReference<REFERRED> other, final org.neo4j.consistency.checking.ComparativeRecordChecker<RECORD, ? super REFERRED, REPORT> checker)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
			  public override void ComparativeCheck<REFERRED, T1>( RecordReference<REFERRED> other, ComparativeRecordChecker<T1> checker ) where REFERRED : Neo4Net.Kernel.impl.store.record.AbstractBaseRecord
			  {
					outerInstance.deferredTasks.AddLast(() =>
					{
					 PendingReferenceCheck mock = mock( typeof( PendingReferenceCheck ) );
					 DeferredReferenceCheck check = new DeferredReferenceCheck( Engine.this, checker );
					 doAnswer( check ).when( mock ).checkReference( Null, Null );
					 doAnswer( check ).when( mock ).checkReference( any( typeof( AbstractBaseRecord ) ), any( typeof( RecordAccess ) ) );
					 doAnswer( check ).when( mock ).checkDiffReference( any( typeof( AbstractBaseRecord ) ), any( typeof( AbstractBaseRecord ) ), any( typeof( RecordAccess ) ) );
					 other.Dispatch( mock );
					});
			  }

			  public override REPORT Report()
			  {
					return ReportConflict;
			  }

			  internal abstract void CheckReference( ComparativeRecordChecker checker, AbstractBaseRecord oldReference, AbstractBaseRecord newReference );
		 }

		 private class DeferredReferenceCheck : Answer<Void>
		 {
			  internal readonly Engine Dispatch;
			  internal readonly ComparativeRecordChecker Checker;

			  internal DeferredReferenceCheck( Engine dispatch, ComparativeRecordChecker checker )
			  {
					this.Dispatch = dispatch;
					this.Checker = checker;
			  }

			  public override Void Answer( InvocationOnMock invocation )
			  {
					object[] arguments = invocation.Arguments;
					AbstractBaseRecord oldReference = null;
					AbstractBaseRecord newReference;
					if ( arguments.Length == 3 )
					{
						 oldReference = ( AbstractBaseRecord ) arguments[0];
						 newReference = ( AbstractBaseRecord ) arguments[1];
					}
					else
					{
						 newReference = ( AbstractBaseRecord ) arguments[0];
					}
					Dispatch.checkReference( Checker, oldReference, newReference );
					return null;
			  }
		 }

		 private readonly LinkedList<ThreadStart> _deferredTasks = new LinkedList<ThreadStart>();

		 public virtual void CheckDeferred()
		 {
			  for ( ThreadStart task; null != ( task = _deferredTasks.RemoveFirst() ); )
			  {
					task.run();
			  }
		 }

		 private readonly IDictionary<long, Delta<DynamicRecord>> _schemata = new Dictionary<long, Delta<DynamicRecord>>();
		 private readonly IDictionary<long, Delta<NodeRecord>> _nodes = new Dictionary<long, Delta<NodeRecord>>();
		 private readonly IDictionary<long, Delta<RelationshipRecord>> _relationships = new Dictionary<long, Delta<RelationshipRecord>>();
		 private readonly IDictionary<long, Delta<PropertyRecord>> _properties = new Dictionary<long, Delta<PropertyRecord>>();
		 private readonly IDictionary<long, Delta<DynamicRecord>> _strings = new Dictionary<long, Delta<DynamicRecord>>();
		 private readonly IDictionary<long, Delta<DynamicRecord>> _arrays = new Dictionary<long, Delta<DynamicRecord>>();
		 private readonly IDictionary<long, Delta<RelationshipTypeTokenRecord>> _relationshipTypeTokens = new Dictionary<long, Delta<RelationshipTypeTokenRecord>>();
		 private readonly IDictionary<long, Delta<LabelTokenRecord>> _labelTokens = new Dictionary<long, Delta<LabelTokenRecord>>();
		 private readonly IDictionary<long, Delta<PropertyKeyTokenRecord>> _propertyKeyTokens = new Dictionary<long, Delta<PropertyKeyTokenRecord>>();
		 private readonly IDictionary<long, Delta<DynamicRecord>> _relationshipTypeNames = new Dictionary<long, Delta<DynamicRecord>>();
		 private readonly IDictionary<long, Delta<DynamicRecord>> _nodeDynamicLabels = new Dictionary<long, Delta<DynamicRecord>>();
		 private readonly IDictionary<long, Delta<DynamicRecord>> _labelNames = new Dictionary<long, Delta<DynamicRecord>>();
		 private readonly IDictionary<long, Delta<DynamicRecord>> _propertyKeyNames = new Dictionary<long, Delta<DynamicRecord>>();
		 private readonly IDictionary<long, Delta<RelationshipGroupRecord>> _relationshipGroups = new Dictionary<long, Delta<RelationshipGroupRecord>>();
		 private Delta<NeoStoreRecord> _graph;
		 private readonly CacheAccess _cacheAccess = new DefaultCacheAccess( Counts.NONE, 1 );
		 private readonly MultiPassStore[] _storesToCheck;

		 public RecordAccessStub() : this(org.neo4j.consistency.checking.full.Stage_Fields.SequentialForward, MultiPassStore.values())
		 {
		 }

		 public RecordAccessStub( Stage stage, params MultiPassStore[] storesToCheck )
		 {
			  this._storesToCheck = storesToCheck;
			  if ( stage.CacheSlotSizes.Length > 0 )
			  {
					_cacheAccess.CacheSlotSizes = stage.CacheSlotSizes;
			  }
		 }

		 public virtual void PopulateCache()
		 {
			  CacheTask action = new CacheTask.CacheNextRel( CheckStage.Stage3_NS_NextRel, _cacheAccess, resourceIterable( new IterableWrapperAnonymousInnerClass( this, _nodes.Values ) ) );
			  action.Run();
		 }

		 private class IterableWrapperAnonymousInnerClass : IterableWrapper<NodeRecord, Delta<NodeRecord>>
		 {
			 private readonly RecordAccessStub _outerInstance;

			 public IterableWrapperAnonymousInnerClass( RecordAccessStub outerInstance, UnknownType values ) : base( values )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override NodeRecord underlyingObjectToObject( Delta<NodeRecord> node )
			 {
				  return node.NewRecord;
			 }
		 }

		 private class Delta<R> where R : Neo4Net.Kernel.impl.store.record.AbstractBaseRecord
		 {
			  internal readonly R OldRecord;
			  internal readonly R NewRecord;

			  internal Delta( R record )
			  {
					this.OldRecord = null;
					this.NewRecord = record;
			  }

			  internal Delta( R oldRecord, R newRecord )
			  {
					this.OldRecord = oldRecord;
					this.NewRecord = newRecord;
			  }
		 }

		 private abstract class Version
		 {
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           PREV { <R> R get(Delta<R> delta) { return delta.oldRecord == null ? delta.newRecord : delta.oldRecord; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           LATEST { <R> R get(Delta<R> delta) { return delta.newRecord; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           NEW { <R> R get(Delta<R> delta) { return delta.oldRecord == null ? null : delta.newRecord; } };

			  private static readonly IList<Version> valueList = new List<Version>();

			  static Version()
			  {
				  valueList.Add( PREV );
				  valueList.Add( LATEST );
				  valueList.Add( NEW );
			  }

			  public enum InnerEnum
			  {
				  PREV,
				  LATEST,
				  NEW
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private Version( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  internal abstract R get<R>( Delta<R> delta );

			 public static IList<Version> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static Version valueOf( string name )
			 {
				 foreach ( Version enumInstance in Version.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

		 private static R Add<R>( IDictionary<long, Delta<R>> records, R record ) where R : Neo4Net.Kernel.impl.store.record.AbstractBaseRecord
		 {
			  records[record.Id] = new Delta<R>( record );
			  return record;
		 }

		 private static void Add<R>( IDictionary<long, Delta<R>> records, R oldRecord, R newRecord ) where R : Neo4Net.Kernel.impl.store.record.AbstractBaseRecord
		 {
			  records[newRecord.Id] = new Delta<R>( oldRecord, newRecord );
		 }

		 public virtual DynamicRecord AddSchema( DynamicRecord schema )
		 {
			  return Add( _schemata, schema );
		 }

		 public virtual DynamicRecord AddString( DynamicRecord @string )
		 {
			  return Add( _strings, @string );
		 }

		 public virtual DynamicRecord AddArray( DynamicRecord array )
		 {
			  return Add( _arrays, array );
		 }

		 public virtual DynamicRecord AddNodeDynamicLabels( DynamicRecord array )
		 {
			  return Add( _nodeDynamicLabels, array );
		 }

		 public virtual DynamicRecord AddPropertyKeyName( DynamicRecord name )
		 {
			  return Add( _propertyKeyNames, name );
		 }

		 public virtual DynamicRecord AddRelationshipTypeName( DynamicRecord name )
		 {
			  return Add( _relationshipTypeNames, name );
		 }

		 public virtual DynamicRecord AddLabelName( DynamicRecord name )
		 {
			  return Add( _labelNames, name );
		 }

		 public virtual R AddChange<R>( R oldRecord, R newRecord ) where R : Neo4Net.Kernel.impl.store.record.AbstractBaseRecord
		 {
			  if ( newRecord is NodeRecord )
			  {
					Add( _nodes, ( NodeRecord ) oldRecord, ( NodeRecord ) newRecord );
			  }
			  else if ( newRecord is RelationshipRecord )
			  {
					Add( _relationships, ( RelationshipRecord ) oldRecord, ( RelationshipRecord ) newRecord );
			  }
			  else if ( newRecord is PropertyRecord )
			  {
					Add( _properties, ( PropertyRecord ) oldRecord, ( PropertyRecord ) newRecord );
			  }
			  else if ( newRecord is DynamicRecord )
			  {
					DynamicRecord dyn = ( DynamicRecord ) newRecord;
					if ( dyn.getType() == PropertyType.STRING )
					{
						 Add( _strings, ( DynamicRecord ) oldRecord, dyn );
					}
					else if ( dyn.getType() == PropertyType.ARRAY )
					{
						 Add( _arrays, ( DynamicRecord ) oldRecord, dyn );
					}
					else if ( dyn.TypeAsInt == SCHEMA_RECORD_TYPE )
					{
						 Add( _schemata, ( DynamicRecord ) oldRecord, dyn );
					}
					else
					{
						 throw new System.ArgumentException( "Invalid dynamic record type" );
					}
			  }
			  else if ( newRecord is RelationshipTypeTokenRecord )
			  {
					Add( _relationshipTypeTokens, ( RelationshipTypeTokenRecord ) oldRecord, ( RelationshipTypeTokenRecord ) newRecord );
			  }
			  else if ( newRecord is PropertyKeyTokenRecord )
			  {
					Add( _propertyKeyTokens, ( PropertyKeyTokenRecord ) oldRecord, ( PropertyKeyTokenRecord ) newRecord );
			  }
			  else if ( newRecord is NeoStoreRecord )
			  {
					this._graph = new Delta<NeoStoreRecord>( ( NeoStoreRecord ) oldRecord, ( NeoStoreRecord ) newRecord );
			  }
			  else
			  {
					throw new System.ArgumentException( "Invalid record type" );
			  }
			  return newRecord;
		 }

		 public virtual R Add<R>( R record ) where R : Neo4Net.Kernel.impl.store.record.AbstractBaseRecord
		 {
			  if ( record is NodeRecord )
			  {
					Add( _nodes, ( NodeRecord ) record );
			  }
			  else if ( record is RelationshipRecord )
			  {
					Add( _relationships, ( RelationshipRecord ) record );
			  }
			  else if ( record is PropertyRecord )
			  {
					Add( _properties, ( PropertyRecord ) record );
			  }
			  else if ( record is DynamicRecord )
			  {
					DynamicRecord dyn = ( DynamicRecord ) record;
					if ( dyn.getType() == PropertyType.STRING )
					{
						 AddString( dyn );
					}
					else if ( dyn.getType() == PropertyType.ARRAY )
					{
						 AddArray( dyn );
					}
					else if ( dyn.TypeAsInt == SCHEMA_RECORD_TYPE )
					{
						 AddSchema( dyn );
					}
					else
					{
						 throw new System.ArgumentException( "Invalid dynamic record type" );
					}
			  }
			  else if ( record is RelationshipTypeTokenRecord )
			  {
					Add( _relationshipTypeTokens, ( RelationshipTypeTokenRecord ) record );
			  }
			  else if ( record is PropertyKeyTokenRecord )
			  {
					Add( _propertyKeyTokens, ( PropertyKeyTokenRecord ) record );
			  }
			  else if ( record is LabelTokenRecord )
			  {
					Add( _labelTokens, ( LabelTokenRecord ) record );
			  }
			  else if ( record is NeoStoreRecord )
			  {
					this._graph = new Delta<NeoStoreRecord>( ( NeoStoreRecord ) record );
			  }
			  else if ( record is RelationshipGroupRecord )
			  {
					Add( _relationshipGroups, ( RelationshipGroupRecord ) record );
			  }
			  else
			  {
					throw new System.ArgumentException( "Invalid record type" );
			  }
			  return record;
		 }

		 private DirectRecordReference<R> Reference<R>( IDictionary<long, Delta<R>> records, long id, Version version ) where R : Neo4Net.Kernel.impl.store.record.AbstractBaseRecord
		 {
			  return new DirectRecordReference<R>( Record( records, id, version ), this );
		 }

		 private static R Record<R>( IDictionary<long, Delta<R>> records, long id, Version version ) where R : Neo4Net.Kernel.impl.store.record.AbstractBaseRecord
		 {
			  Delta<R> delta = records[id];
			  if ( delta == null )
			  {
					if ( version == Version.New )
					{
						 return default( R );
					}
					throw new AssertionError( string.Format( "Access to record with id={0:D} not expected.", id ) );
			  }
			  return version.get( delta );
		 }

		 public override RecordReference<DynamicRecord> Schema( long id )
		 {
			  return Reference( _schemata, id, Version.Latest );
		 }

		 public override RecordReference<NodeRecord> Node( long id )
		 {
			  return Reference( _nodes, id, Version.Latest );
		 }

		 public override RecordReference<RelationshipRecord> Relationship( long id )
		 {
			  return Reference( _relationships, id, Version.Latest );
		 }

		 public override RecordReference<PropertyRecord> Property( long id )
		 {
			  return Reference( _properties, id, Version.Latest );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public java.util.Iterator<org.neo4j.kernel.impl.store.record.PropertyRecord> rawPropertyChain(final long firstId)
		 public override IEnumerator<PropertyRecord> RawPropertyChain( long firstId )
		 {
			  return new PrefetchingIteratorAnonymousInnerClass( this, firstId );
		 }

		 private class PrefetchingIteratorAnonymousInnerClass : PrefetchingIterator<PropertyRecord>
		 {
			 private readonly RecordAccessStub _outerInstance;

			 private long _firstId;

			 public PrefetchingIteratorAnonymousInnerClass( RecordAccessStub outerInstance, long firstId )
			 {
				 this.outerInstance = outerInstance;
				 this._firstId = firstId;
				 next = firstId;
			 }

			 private long next;

			 protected internal override PropertyRecord fetchNextOrNull()
			 {
				  if ( Record.NO_NEXT_PROPERTY.@is( next ) )
				  {
						return null;
				  }
				  PropertyRecord record = _outerInstance.reference( _outerInstance.properties, next, Version.Latest ).record();
				  next = record.NextProp;
				  return record;
			 }
		 }

		 public override RecordReference<RelationshipTypeTokenRecord> RelationshipType( int id )
		 {
			  return Reference( _relationshipTypeTokens, id, Version.Latest );
		 }

		 public override RecordReference<PropertyKeyTokenRecord> PropertyKey( int id )
		 {
			  return Reference( _propertyKeyTokens, id, Version.Latest );
		 }

		 public override RecordReference<DynamicRecord> String( long id )
		 {
			  return Reference( _strings, id, Version.Latest );
		 }

		 public override RecordReference<DynamicRecord> Array( long id )
		 {
			  return Reference( _arrays, id, Version.Latest );
		 }

		 public override RecordReference<DynamicRecord> RelationshipTypeName( int id )
		 {
			  return Reference( _relationshipTypeNames, id, Version.Latest );
		 }

		 public override RecordReference<DynamicRecord> NodeLabels( long id )
		 {
			  return Reference( _nodeDynamicLabels, id, Version.Latest );
		 }

		 public override RecordReference<LabelTokenRecord> Label( int id )
		 {
			  return Reference( _labelTokens, id, Version.Latest );
		 }

		 public override RecordReference<DynamicRecord> LabelName( int id )
		 {
			  return Reference( _labelNames, id, Version.Latest );
		 }

		 public override RecordReference<DynamicRecord> PropertyKeyName( int id )
		 {
			  return Reference( _propertyKeyNames, id, Version.Latest );
		 }

		 public override RecordReference<NeoStoreRecord> Graph()
		 {
			  return Reference( singletonMap( -1L, _graph ), -1, Version.Latest );
		 }

		 public override RecordReference<RelationshipGroupRecord> RelationshipGroup( long id )
		 {
			  return Reference( _relationshipGroups, id, Version.Latest );
		 }

		 public override bool ShouldCheck( long id, MultiPassStore store )
		 {
			  return ArrayUtil.contains( _storesToCheck, store );
		 }

		 public override CacheAccess CacheAccess()
		 {
			  return _cacheAccess;
		 }
	}

}