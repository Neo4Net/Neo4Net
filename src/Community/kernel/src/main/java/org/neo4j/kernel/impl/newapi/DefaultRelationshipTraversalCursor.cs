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
namespace Neo4Net.Kernel.Impl.Newapi
{
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;
	using ImmutableEmptyLongIterator = org.eclipse.collections.impl.iterator.ImmutableEmptyLongIterator;

	using NodeCursor = Neo4Net.@internal.Kernel.Api.NodeCursor;
	using RelationshipTraversalCursor = Neo4Net.@internal.Kernel.Api.RelationshipTraversalCursor;
	using RelationshipDirection = Neo4Net.Storageengine.Api.RelationshipDirection;
	using StorageRelationshipTraversalCursor = Neo4Net.Storageengine.Api.StorageRelationshipTraversalCursor;
	using NodeState = Neo4Net.Storageengine.Api.txstate.NodeState;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.Read_Fields.ANY_RELATIONSHIP_TYPE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.newapi.References.clearEncoding;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.AbstractBaseRecord.NO_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.RelationshipDirection.INCOMING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.RelationshipDirection.LOOP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.RelationshipDirection.OUTGOING;

	internal class DefaultRelationshipTraversalCursor : DefaultRelationshipCursor<StorageRelationshipTraversalCursor>, RelationshipTraversalCursor
	{
		 private abstract class FilterState
		 {
			  // need filter, and need to read filter state from first store relationship
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           NOT_INITIALIZED(org.neo4j.storageengine.api.RelationshipDirection.ERROR) { boolean check(long source, long target, long origin) { throw new IllegalStateException("Cannot call check on uninitialized filter"); } },
			  // allow only incoming relationships
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           INCOMING(org.neo4j.storageengine.api.RelationshipDirection.INCOMING) { boolean check(long source, long target, long origin) { return origin == target && source != target; } },
			  // allow only outgoing relationships
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           OUTGOING(org.neo4j.storageengine.api.RelationshipDirection.OUTGOING) { boolean check(long source, long target, long origin) { return origin == source && source != target; } },
			  // allow only loop relationships
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           LOOP(org.neo4j.storageengine.api.RelationshipDirection.LOOP) { boolean check(long source, long target, long origin) { return source == target; } },
			  // no filtering required
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           NONE(org.neo4j.storageengine.api.RelationshipDirection.ERROR) { boolean check(long source, long target, long origin) { return true; } };

			  private static readonly IList<FilterState> valueList = new List<FilterState>();

			  static FilterState()
			  {
				  valueList.Add( NOT_INITIALIZED );
				  valueList.Add( INCOMING );
				  valueList.Add( OUTGOING );
				  valueList.Add( LOOP );
				  valueList.Add( NONE );
			  }

			  public enum InnerEnum
			  {
				  NOT_INITIALIZED,
				  INCOMING,
				  OUTGOING,
				  LOOP,
				  NONE
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private FilterState( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  internal abstract bool check( long source, long target, long origin );

			  internal readonly Neo4Net.Storageengine.Api.RelationshipDirection direction;

			  internal FilterState( string name, InnerEnum innerEnum, Neo4Net.Storageengine.Api.RelationshipDirection direction )
			  {
					this._direction = direction;

				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  internal static FilterState FromRelationshipDirection( Neo4Net.Storageengine.Api.RelationshipDirection direction )
			  {
					switch ( direction )
					{
					case RelationshipDirection.OUTGOING:
						 return FilterState.Outgoing;
					case RelationshipDirection.INCOMING:
						 return FilterState.Incoming;
					case RelationshipDirection.LOOP:
						 return FilterState.Loop;
					case RelationshipDirection.ERROR:
						 throw new System.ArgumentException( "There has been a RelationshipDirection.ERROR" );
					default:
						 throw new System.InvalidOperationException( format( "Still poking my eye, dear checkstyle... (cannot filter on direction '%s')", direction ) );
					}
			  }

			 public static IList<FilterState> values()
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

			 public static FilterState valueOf( string name )
			 {
				 foreach ( FilterState enumInstance in FilterState.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

		 private FilterState _filterState;
		 private bool _filterStore;
		 private int _filterType = NO_ID;
		 private LongIterator _addedRelationships;

		 internal DefaultRelationshipTraversalCursor( DefaultCursors pool, StorageRelationshipTraversalCursor storeCursor ) : base( pool, storeCursor )
		 {
		 }

		 internal virtual void Init( long nodeReference, long reference, Read read )
		 {
			  /* There are 5 different ways a relationship traversal cursor can be initialized:
			   *
			   * 1. From a batched group in a detached way. This happens when the user manually retrieves the relationships
			   *    references from the group cursor and passes it to this method and if the group cursor was based on having
			   *    batched all the different types in the single (mixed) chain of relationships.
			   *    In this case we should pass a reference marked with some flag to the first relationship in the chain that
			   *    has the type of the current group in the group cursor. The traversal cursor then needs to read the type
			   *    from that first record and use that type as a filter for when reading the rest of the chain.
			   *    - NOTE: we probably have to do the same sort of filtering for direction - so we need a flag for that too.
			   *
			   * 2. From a batched group in a DIRECT way. This happens when the traversal cursor is initialized directly from
			   *    the group cursor, in this case we can simply initialize the traversal cursor with the buffered state from
			   *    the group cursor, so this method here does not have to be involved, and things become pretty simple.
			   *
			   * 3. Traversing all relationships - regardless of type - of a node that has grouped relationships. In this case
			   *    the traversal cursor needs to traverse through the group records in order to get to the actual
			   *    relationships. The initialization of the cursor (through this here method) should be with a FLAGGED
			   *    reference to the (first) group record.
			   *
			   * 4. Traversing a single chain - this is what happens in the cases when
			   *    a) Traversing all relationships of a node without grouped relationships.
			   *    b) Traversing the relationships of a particular group of a node with grouped relationships.
			   *
			   * 5. There are no relationships - i.e. passing in NO_ID to this method.
			   *
			   * This means that we need reference encodings (flags) for cases: 1, 3, 4, 5
			   */

			  RelationshipReferenceEncoding encoding = RelationshipReferenceEncoding.parseEncoding( reference );

			  switch ( encoding.innerEnumValue )
			  {
			  case Neo4Net.Kernel.Impl.Newapi.RelationshipReferenceEncoding.InnerEnum.NONE:
			  case Neo4Net.Kernel.Impl.Newapi.RelationshipReferenceEncoding.InnerEnum.GROUP:
					StoreCursor.init( nodeReference, reference );
					InitFiltering( FilterState.None, false );
					break;

			  case Neo4Net.Kernel.Impl.Newapi.RelationshipReferenceEncoding.InnerEnum.FILTER_TX_STATE:
					// The relationships in tx-state needs to be filtered according to the first relationship we discover,
					// but let's not have the store cursor bother with this detail.
					StoreCursor.init( nodeReference, clearEncoding( reference ) );
					InitFiltering( FilterState.NotInitialized, false );
					break;

			  case Neo4Net.Kernel.Impl.Newapi.RelationshipReferenceEncoding.InnerEnum.FILTER:
					// The relationships needs to be filtered according to the first relationship we discover
					StoreCursor.init( nodeReference, clearEncoding( reference ) );
					InitFiltering( FilterState.NotInitialized, true );
					break;

			  case Neo4Net.Kernel.Impl.Newapi.RelationshipReferenceEncoding.InnerEnum.NO_OUTGOING_OF_TYPE: // nothing in store, but proceed to check tx-state changes
					StoreCursor.init( nodeReference, NO_ID );
					InitFiltering( FilterState.fromRelationshipDirection( OUTGOING ), false );
					this._filterType = ( int ) clearEncoding( reference );
					break;

			  case Neo4Net.Kernel.Impl.Newapi.RelationshipReferenceEncoding.InnerEnum.NO_INCOMING_OF_TYPE: // nothing in store, but proceed to check tx-state changes
					StoreCursor.init( nodeReference, NO_ID );
					InitFiltering( FilterState.fromRelationshipDirection( INCOMING ), false );
					this._filterType = ( int ) clearEncoding( reference );
					break;

			  case Neo4Net.Kernel.Impl.Newapi.RelationshipReferenceEncoding.InnerEnum.NO_LOOP_OF_TYPE: // nothing in store, but proceed to check tx-state changes
					StoreCursor.init( nodeReference, NO_ID );
					InitFiltering( FilterState.fromRelationshipDirection( LOOP ), false );
					this._filterType = ( int ) clearEncoding( reference );
					break;

			  default:
					throw new System.InvalidOperationException( "Unknown encoding " + encoding );
			  }

			  Init( read );
			  this._addedRelationships = ImmutableEmptyLongIterator.INSTANCE;
		 }

		 private void InitFiltering( FilterState filterState, bool filterStore )
		 {
			  this._filterState = filterState;
			  this._filterStore = filterStore;
		 }

		 public override Neo4Net.@internal.Kernel.Api.RelationshipTraversalCursor_Position Suspend()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override void Resume( Neo4Net.@internal.Kernel.Api.RelationshipTraversalCursor_Position position )
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override void Neighbour( NodeCursor cursor )
		 {
			  Read.singleNode( NeighbourNodeReference(), cursor );
		 }

		 public override long NeighbourNodeReference()
		 {
			  return StoreCursor.neighbourNodeReference();
		 }

		 public override long OriginNodeReference()
		 {
			  return StoreCursor.originNodeReference();
		 }

		 public override bool Next()
		 {
			  bool hasChanges;

			  if ( _filterState == FilterState.NotInitialized )
			  {
					hasChanges = hasChanges(); // <- will setup filter state if needed

					if ( _filterState == FilterState.NotInitialized && _filterStore )
					{
						 StoreCursor.next();
						 SetupFilterState();
					}

					if ( _filterState != FilterState.NotInitialized && !( hasChanges && Read.txState().relationshipIsDeletedInThisTx(RelationshipReference()) ) )
					{
						 return true;
					}
			  }
			  else
			  {
					hasChanges = hasChanges();
			  }

			  // tx-state relationships
			  if ( hasChanges && _addedRelationships.hasNext() )
			  {
					Read.txState().relationshipVisit(_addedRelationships.next(), StoreCursor);
					return true;
			  }

			  while ( StoreCursor.next() )
			  {
					bool skip = ( _filterStore && !CorrectTypeAndDirection() ) || (hasChanges && Read.txState().relationshipIsDeletedInThisTx(StoreCursor.entityReference()));
					if ( !skip )
					{
						 return true;
					}
			  }
			  return false;
		 }

		 private void SetupFilterState()
		 {
			  _filterType = StoreCursor.type();
			  const long source = SourceNodeReference(), target = TargetNodeReference();
			  if ( source == target )
			  {
					_filterState = FilterState.Loop;
			  }
			  else if ( source == StoreCursor.originNodeReference() )
			  {
					_filterState = FilterState.Outgoing;
			  }
			  else if ( target == StoreCursor.originNodeReference() )
			  {
					_filterState = FilterState.Incoming;
			  }
		 }

		 private bool CorrectTypeAndDirection()
		 {
			  return ( _filterType == ANY_RELATIONSHIP_TYPE || _filterType == StoreCursor.type() ) && _filterState.check(SourceNodeReference(), TargetNodeReference(), StoreCursor.originNodeReference());
		 }

		 public override void Close()
		 {
			  if ( !Closed )
			  {
					Read = null;
					_filterState = FilterState.None;
					_filterType = NO_ID;
					_filterStore = false;
					StoreCursor.close();

					Pool.accept( this );
			  }
		 }

		 protected internal override void CollectAddedTxStateSnapshot()
		 {
			  if ( _filterState == FilterState.NotInitialized )
			  {
					StoreCursor.next();
					SetupFilterState();
			  }

			  NodeState nodeState = Read.txState().getNodeState(StoreCursor.originNodeReference());
			  _addedRelationships = HasTxStateFilter() ? nodeState.GetAddedRelationships(_filterState.direction, _filterType) : nodeState.AddedRelationships;
		 }

		 private bool HasTxStateFilter()
		 {
			  return _filterState != FilterState.None;
		 }

		 public override bool Closed
		 {
			 get
			 {
				  return Read == null;
			 }
		 }

		 public virtual void Release()
		 {
			  StoreCursor.close();
		 }

		 public override string ToString()
		 {
			  if ( Closed )
			  {
					return "RelationshipTraversalCursor[closed state]";
			  }
			  else
			  {
					string mode = "mode=";
					if ( _filterStore )
					{
						 mode = mode + "filterStore";
					}
					else
					{
						 mode = mode + "regular";
					}
					return "RelationshipTraversalCursor[id=" + StoreCursor.entityReference() +
							  ", " + mode +
							  ", " + StoreCursor.ToString() + "]";
			  }
		 }
	}

}