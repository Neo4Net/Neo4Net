using System;
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
namespace Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage
{

	using IdRange = Org.Neo4j.Kernel.impl.store.id.IdRange;
	using IdSequence = Org.Neo4j.Kernel.impl.store.id.IdSequence;
	using AbstractBaseRecord = Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using Record = Org.Neo4j.Kernel.impl.store.record.Record;
	using RelationshipGroupRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipGroupRecord;
	using RelationshipRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipRecord;
	using Org.Neo4j.Kernel.impl.transaction.state;
	using RecordChangeSet = Org.Neo4j.Kernel.impl.transaction.state.RecordChangeSet;

	/// <summary>
	/// Test utility DSL for creating store records </summary>
	public class RecordBuilders
	{
		 public static IList<R> Records<R, A>( IEnumerable<Org.Neo4j.Kernel.impl.transaction.state.RecordAccess_RecordProxy<R, A>> changes ) where R : Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return StreamSupport.stream( changes.spliterator(), false ).map(Org.Neo4j.Kernel.impl.transaction.state.RecordAccess_RecordProxy::forChangingData).collect(Collectors.toList());
		 }

		 public static NodeRecord Node( long id, params System.Action<NodeRecord>[] modifiers )
		 {
			  NodeRecord record = new NodeRecord( id );
			  record.Initialize( true, Record.NO_NEXT_PROPERTY.intValue(), false, Record.NO_NEXT_RELATIONSHIP.intValue(), Record.NO_LABELS_FIELD.intValue() );
			  foreach ( System.Action<NodeRecord> modifier in modifiers )
			  {
					modifier( record );
			  }

			  return record;
		 }

		 public static RelationshipRecord Rel( long id, params System.Action<RelationshipRecord>[] modifiers )
		 {
			  RelationshipRecord record = new RelationshipRecord( id );
			  record.Initialize( true, Record.NO_NEXT_PROPERTY.intValue(), -1, -1, 0, Record.NO_PREV_RELATIONSHIP.longValue(), Record.NO_NEXT_RELATIONSHIP.longValue(), Record.NO_PREV_RELATIONSHIP.longValue(), Record.NO_NEXT_RELATIONSHIP.longValue(), true, true );
			  foreach ( System.Action<RelationshipRecord> modifier in modifiers )
			  {
					modifier( record );
			  }

			  return record;
		 }

		 public static RelationshipGroupRecord RelGroup( long id, params System.Action<RelationshipGroupRecord>[] modifiers )
		 {
			  RelationshipGroupRecord record = new RelationshipGroupRecord( id );
			  record.Initialize( true, 0, Record.NO_NEXT_RELATIONSHIP.longValue(), Record.NO_NEXT_RELATIONSHIP.longValue(), Record.NO_NEXT_RELATIONSHIP.longValue(), -1, Record.NO_NEXT_RELATIONSHIP.longValue() );
			  foreach ( System.Action<RelationshipGroupRecord> modifier in modifiers )
			  {
					modifier( record );
			  }

			  return record;
		 }

		 // Below is a set of static "modifier" functions, that are meant to act as a DSL for building
		 // records. It's a first stab at it - it has the clear issue that currently each function name
		 // can only map to one record type (since the Consumers are typed), which means field names
		 // class. Refactor as needed!

		 public static System.Action<NodeRecord> NextRel( long nextRelId )
		 {
			  return n => n.setNextRel( nextRelId );
		 }

		 public static System.Action<NodeRecord> Group( long groupId )
		 {
			  return n =>
			  {
				n.Dense = true;
				n.NextRel = groupId;
			  };
		 }

		 public static System.Action<RelationshipRecord> From( long fromNodeId )
		 {
			  return n => n.setFirstNode( fromNodeId );
		 }

		 public static System.Action<RelationshipRecord> To( long toNodeId )
		 {
			  return n => n.setSecondNode( toNodeId );
		 }

		 public static System.Action<RelationshipRecord> SCount( long count )
		 {
			  return n =>
			  {
				n.FirstInFirstChain = true;
				n.FirstPrevRel = count;
			  };
		 }

		 public static System.Action<RelationshipRecord> TCount( long count )
		 {
			  return n =>
			  {
				n.FirstInSecondChain = true;
				n.SecondPrevRel = count;
			  };
		 }

		 public static System.Action<RelationshipRecord> SPrev( long id )
		 {
			  return n =>
			  {
				n.FirstInFirstChain = false;
				n.FirstPrevRel = id;
			  };
		 }

		 public static System.Action<RelationshipRecord> SNext( long id )
		 {
			  return n => n.setFirstNextRel( id );
		 }

		 public static System.Action<RelationshipRecord> TPrev( long id )
		 {
			  return n =>
			  {
				n.FirstInSecondChain = false;
				n.SecondPrevRel = id;
			  };
		 }

		 public static System.Action<RelationshipRecord> TNext( long id )
		 {
			  return n => n.setSecondNextRel( id );
		 }

		 public static System.Action<RelationshipGroupRecord> FirstLoop( long id )
		 {
			  return g => g.setFirstLoop( id );
		 }

		 public static System.Action<RelationshipGroupRecord> FirstOut( long id )
		 {
			  return g => g.setFirstOut( id );
		 }

		 public static System.Action<RelationshipGroupRecord> FirstIn( long id )
		 {
			  return g => g.setFirstIn( id );
		 }

		 public static System.Action<RelationshipGroupRecord> OwningNode( long id )
		 {
			  return g => g.setOwningNode( id );
		 }

		 public static Stream<R> FilterType<R>( object[] @in, Type type )
		 {
				 type = typeof( R );
			  return FilterType( Stream.of( @in ), type );
		 }

		 public static Stream<R> FilterType<R, T1>( Stream<T1> @in, Type type )
		 {
				 type = typeof( R );
			  return @in.filter( type.isInstance ).map( type.cast );
		 }

		 public static RecordChangeSet NewChangeSet( params AbstractBaseRecord[] records )
		 {
			  return new RecordChangeSet(new Loader(FilterType(records, typeof(NodeRecord)).collect(Collectors.toList()), (System.Func<long, object, NodeRecord>)(key, extra) => new NodeRecord(key)), null, new Loader(FilterType(records, typeof(RelationshipRecord)).collect(Collectors.toList()), (System.Func<long, object, RelationshipRecord>)(key, extra) => new RelationshipRecord(key)), new Loader(FilterType(records, typeof(RelationshipGroupRecord)).collect(Collectors.toList()), (System.Func<long, int, RelationshipGroupRecord>)(key, extra) =>
			  {
			  RelationshipGroupRecord group = new RelationshipGroupRecord( key );
			  group.Type = extra;
			  return group;
			  }), null, null, null, null);
		 }

		 public static RelationshipGroupGetter NewRelGroupGetter( params AbstractBaseRecord[] records )
		 {
			  return new RelationshipGroupGetter( new IdSequenceAnonymousInnerClass( records ) );
		 }

		 private class IdSequenceAnonymousInnerClass : IdSequence
		 {
			 private AbstractBaseRecord[] _records;

			 public IdSequenceAnonymousInnerClass( AbstractBaseRecord[] records )
			 {
				 this._records = records;
			 }

			 private long nextId = FilterType( _records, typeof( RelationshipGroupRecord ) ).count();

			 public long nextId()
			 {
				  return nextId++;
			 }

			 public IdRange nextIdBatch( int size )
			 {
				  throw new System.NotSupportedException();
			 }
		 }

		 private class Loader<T, E> : Org.Neo4j.Kernel.impl.transaction.state.RecordAccess_Loader<T, E> where T : Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord
		 {
			  internal readonly IList<T> Records;
			  internal readonly System.Func<long, E, T> NewRecord;

			  internal Loader( IList<T> records, System.Func<long, E, T> newRecord )
			  {
					this.Records = records;
					this.NewRecord = newRecord;
			  }

			  public override T NewUnused( long key, E additionalData )
			  {
					return NewRecord.apply( key, additionalData );
			  }

			  public override T Load( long key, E additionalData )
			  {
					return Records.Where( r => r.Id == key ).First().get();
			  }

			  public override void EnsureHeavy( T relationshipRecord )
			  {

			  }

			  public override T Clone( T record )
			  {
					return ( T )record.clone();
			  }
		 }
	}

}