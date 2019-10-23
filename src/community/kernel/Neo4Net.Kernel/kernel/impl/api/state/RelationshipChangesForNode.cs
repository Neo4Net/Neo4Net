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
namespace Neo4Net.Kernel.Impl.Api.state
{
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;
	using IntObjectMap = org.eclipse.collections.api.map.primitive.IntObjectMap;
	using MutableIntObjectMap = org.eclipse.collections.api.map.primitive.MutableIntObjectMap;
	using LongSet = org.eclipse.collections.api.set.primitive.LongSet;
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;
	using ImmutableEmptyLongIterator = org.eclipse.collections.impl.iterator.ImmutableEmptyLongIterator;
	using IntObjectHashMap = org.eclipse.collections.impl.map.mutable.primitive.IntObjectHashMap;
	using LongHashSet = org.eclipse.collections.impl.set.mutable.primitive.LongHashSet;

	using PrimitiveLongCollections = Neo4Net.Collections.PrimitiveLongCollections;
	using RelationshipDirection = Neo4Net.Kernel.Api.StorageEngine.RelationshipDirection;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.toIntExact;

	/// <summary>
	/// Maintains relationships that have been added for a specific node.
	/// <p/>
	/// This class is not a trustworthy source of information unless you are careful - it does not, for instance, remove
	/// rels if they are added and then removed in the same tx. It trusts wrapping data structures for that filtering.
	/// </summary>
	public class RelationshipChangesForNode
	{
		 /// <summary>
		 /// Allows this data structure to work both for tracking removals and additions.
		 /// </summary>
		 public abstract class DiffStrategy
		 {
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           REMOVE { int augmentDegree(int degree, int diff) { return degree - diff; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           ADD { int augmentDegree(int degree, int diff) { return degree + diff; } };

			  private static readonly IList<DiffStrategy> valueList = new List<DiffStrategy>();

			  static DiffStrategy()
			  {
				  valueList.Add( REMOVE );
				  valueList.Add( ADD );
			  }

			  public enum InnerEnum
			  {
				  REMOVE,
				  ADD
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private DiffStrategy( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  internal abstract int augmentDegree( int degree, int diff );


			 public static IList<DiffStrategy> values()
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

			 public static DiffStrategy ValueOf( string name )
			 {
				 foreach ( DiffStrategy enumInstance in DiffStrategy.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

		 private readonly DiffStrategy _diffStrategy;

		 private MutableIntObjectMap<MutableLongSet> _outgoing;
		 private MutableIntObjectMap<MutableLongSet> _incoming;
		 private MutableIntObjectMap<MutableLongSet> _loops;

		 public RelationshipChangesForNode( DiffStrategy diffStrategy )
		 {
			  this._diffStrategy = diffStrategy;
		 }

		 public virtual void AddRelationship( long relId, int typeId, RelationshipDirection direction )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.map.primitive.MutableIntObjectMap<org.eclipse.collections.api.set.primitive.MutableLongSet> relTypeToRelsMap = getTypeToRelMapForDirection(direction);
			  MutableIntObjectMap<MutableLongSet> relTypeToRelsMap = GetTypeToRelMapForDirection( direction );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.set.primitive.MutableLongSet rels = relTypeToRelsMap.getIfAbsentPut(typeId, org.eclipse.collections.impl.set.mutable.primitive.LongHashSet::new);
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  MutableLongSet rels = relTypeToRelsMap.getIfAbsentPut( typeId, LongHashSet::new );

			  rels.add( relId );
		 }

		 public virtual bool RemoveRelationship( long relId, int typeId, RelationshipDirection direction )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.map.primitive.MutableIntObjectMap<org.eclipse.collections.api.set.primitive.MutableLongSet> relTypeToRelsMap = getTypeToRelMapForDirection(direction);
			  MutableIntObjectMap<MutableLongSet> relTypeToRelsMap = GetTypeToRelMapForDirection( direction );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.set.primitive.MutableLongSet rels = relTypeToRelsMap.get(typeId);
			  MutableLongSet rels = relTypeToRelsMap.get( typeId );
			  if ( rels != null && rels.remove( relId ) )
			  {
					if ( rels.Empty )
					{
						 relTypeToRelsMap.remove( typeId );
					}
					return true;
			  }
			  return false;
		 }

		 public virtual int AugmentDegree( RelationshipDirection direction, int degree, int typeId )
		 {
			  switch ( direction )
			  {
			  case RelationshipDirection.INCOMING:
					if ( _incoming != null && _incoming.containsKey( typeId ) )
					{
						 return _diffStrategy.augmentDegree( degree, _incoming.get( typeId ).size() );
					}
					break;
			  case RelationshipDirection.OUTGOING:
					if ( _outgoing != null && _outgoing.containsKey( typeId ) )
					{
						 return _diffStrategy.augmentDegree( degree, _outgoing.get( typeId ).size() );
					}
					break;
			  case RelationshipDirection.LOOP:
					if ( _loops != null && _loops.containsKey( typeId ) )
					{
						 return _diffStrategy.augmentDegree( degree, _loops.get( typeId ).size() );
					}
					break;

			  default:
					throw new System.ArgumentException( "Unknown direction: " + direction );
			  }

			  return degree;
		 }

		 public virtual void Clear()
		 {
			  if ( _outgoing != null )
			  {
					_outgoing.clear();
			  }
			  if ( _incoming != null )
			  {
					_incoming.clear();
			  }
			  if ( _loops != null )
			  {
					_loops.clear();
			  }
		 }

		 private MutableIntObjectMap<MutableLongSet> Outgoing()
		 {
			  if ( _outgoing == null )
			  {
					_outgoing = new IntObjectHashMap<MutableLongSet>();
			  }
			  return _outgoing;
		 }

		 private MutableIntObjectMap<MutableLongSet> Incoming()
		 {
			  if ( _incoming == null )
			  {
					_incoming = new IntObjectHashMap<MutableLongSet>();
			  }
			  return _incoming;
		 }

		 private MutableIntObjectMap<MutableLongSet> Loops()
		 {
			  if ( _loops == null )
			  {
					_loops = new IntObjectHashMap<MutableLongSet>();
			  }
			  return _loops;
		 }

		 private MutableIntObjectMap<MutableLongSet> GetTypeToRelMapForDirection( RelationshipDirection direction )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.map.primitive.MutableIntObjectMap<org.eclipse.collections.api.set.primitive.MutableLongSet> relTypeToRelsMap;
			  MutableIntObjectMap<MutableLongSet> relTypeToRelsMap;
			  switch ( direction )
			  {
					case RelationshipDirection.INCOMING:
						 relTypeToRelsMap = Incoming();
						 break;
					case RelationshipDirection.OUTGOING:
						 relTypeToRelsMap = Outgoing();
						 break;
					case RelationshipDirection.LOOP:
						 relTypeToRelsMap = Loops();
						 break;
					default:
						 throw new System.ArgumentException( "Unknown direction: " + direction );
			  }
			  return relTypeToRelsMap;
		 }

		 public virtual LongIterator Relationships
		 {
			 get
			 {
				  return PrimitiveLongCollections.concat( PrimitiveIds( _incoming ), PrimitiveIds( _outgoing ), PrimitiveIds( _loops ) );
			 }
		 }

		 public virtual LongIterator getRelationships( RelationshipDirection direction, int type )
		 {
			  switch ( direction )
			  {
			  case RelationshipDirection.INCOMING:
					return _incoming != null ? PrimitiveIdsByType( _incoming, type ) : ImmutableEmptyLongIterator.INSTANCE;
			  case RelationshipDirection.OUTGOING:
					return _outgoing != null ? PrimitiveIdsByType( _outgoing, type ) : ImmutableEmptyLongIterator.INSTANCE;
			  case RelationshipDirection.LOOP:
					return _loops != null ? PrimitiveIdsByType( _loops, type ) : ImmutableEmptyLongIterator.INSTANCE;
			  default:
					throw new System.ArgumentException( "Unknown direction: " + direction );
			  }
		 }

		 private static LongIterator PrimitiveIds( IntObjectMap<MutableLongSet> map )
		 {
			  if ( map == null )
			  {
					return ImmutableEmptyLongIterator.INSTANCE;
			  }

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int size = toIntExact(map.sumOfInt(org.eclipse.collections.api.set.primitive.LongSet::size));
			  int size = toIntExact( map.sumOfInt( LongSet.size ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.set.primitive.MutableLongSet ids = new org.eclipse.collections.impl.set.mutable.primitive.LongHashSet(size);
			  MutableLongSet ids = new LongHashSet( size );
			  map.values().forEach(ids.addAll);
			  return ids.longIterator();
		 }

		 private static LongIterator PrimitiveIdsByType( IntObjectMap<MutableLongSet> map, int type )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.set.primitive.LongSet relationships = map.get(type);
			  LongSet relationships = map.get( type );
			  return relationships == null ? ImmutableEmptyLongIterator.INSTANCE : relationships.freeze().longIterator();
		 }
	}

}