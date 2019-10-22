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
	using IntIterable = org.eclipse.collections.api.IntIterable;
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;
	using IntSets = org.eclipse.collections.impl.factory.primitive.IntSets;
	using ImmutableEmptyLongIterator = org.eclipse.collections.impl.iterator.ImmutableEmptyLongIterator;


	using DiffStrategy = Neo4Net.Kernel.Impl.Api.state.RelationshipChangesForNode.DiffStrategy;
	using CollectionsFactory = Neo4Net.Kernel.impl.util.collection.CollectionsFactory;
	using MutableLongDiffSets = Neo4Net.Kernel.impl.util.diffsets.MutableLongDiffSets;
	using MutableLongDiffSetsImpl = Neo4Net.Kernel.impl.util.diffsets.MutableLongDiffSetsImpl;
	using RelationshipDirection = Neo4Net.Storageengine.Api.RelationshipDirection;
	using StorageProperty = Neo4Net.Storageengine.Api.StorageProperty;
	using LongDiffSets = Neo4Net.Storageengine.Api.txstate.LongDiffSets;
	using NodeState = Neo4Net.Storageengine.Api.txstate.NodeState;
	using Value = Neo4Net.Values.Storable.Value;

	internal class NodeStateImpl : IPropertyContainerStateImpl, NodeState
	{
		 internal const NodeState org;

		 private class NodeStateAnonymousInnerClass : NodeState
		 {
			 public IEnumerator<StorageProperty> addedProperties()
			 {
				  return emptyIterator();
			 }

			 public IEnumerator<StorageProperty> changedProperties()
			 {
				  return emptyIterator();
			 }

			 public IntIterable removedProperties()
			 {
				  return IntSets.immutable.empty();
			 }

			 public IEnumerator<StorageProperty> addedAndChangedProperties()
			 {
				  return emptyIterator();
			 }

			 public bool hasPropertyChanges()
			 {
				  return false;
			 }

			 public LongDiffSets labelDiffSets()
			 {
				  return LongDiffSets.EMPTY;
			 }

			 public int augmentDegree( RelationshipDirection direction, int degree, int typeId )
			 {
				  return degree;
			 }

			 public long Id
			 {
				 get
				 {
					  throw new System.NotSupportedException( "id not defined" );
				 }
			 }

			 public bool isPropertyChangedOrRemoved( int propertyKey )
			 {
				  return false;
			 }

			 public Value propertyValue( int propertyKey )
			 {
				  return null;
			 }

			 public LongIterator AddedRelationships
			 {
				 get
				 {
					  return ImmutableEmptyLongIterator.INSTANCE;
				 }
			 }

			 public LongIterator getAddedRelationships( RelationshipDirection direction, int relType )
			 {
				  return ImmutableEmptyLongIterator.INSTANCE;
			 }
		 }

		 private MutableLongDiffSets _labelDiffSets;
		 private RelationshipChangesForNode _relationshipsAdded;
		 private RelationshipChangesForNode _relationshipsRemoved;

		 private ISet<MutableLongDiffSets> _indexDiffs;

		 internal NodeStateImpl( long id, CollectionsFactory collectionsFactory ) : base( id, collectionsFactory )
		 {
		 }

		 public override LongDiffSets LabelDiffSets()
		 {
			  return _labelDiffSets == null ? LongDiffSets.EMPTY : _labelDiffSets;
		 }

		 internal virtual MutableLongDiffSets OrCreateLabelDiffSets
		 {
			 get
			 {
				  if ( _labelDiffSets == null )
				  {
						_labelDiffSets = new MutableLongDiffSetsImpl( CollectionsFactory );
				  }
				  return _labelDiffSets;
			 }
		 }

		 public virtual void AddRelationship( long relId, int typeId, RelationshipDirection direction )
		 {
			  if ( !HasAddedRelationships() )
			  {
					_relationshipsAdded = new RelationshipChangesForNode( DiffStrategy.ADD );
			  }
			  _relationshipsAdded.addRelationship( relId, typeId, direction );
		 }

		 public virtual void RemoveRelationship( long relId, int typeId, RelationshipDirection direction )
		 {
			  if ( HasAddedRelationships() )
			  {
					if ( _relationshipsAdded.removeRelationship( relId, typeId, direction ) )
					{
						 // This was a rel that was added in this tx, no need to add it to the remove list, instead we just
						 // remove it from added relationships.
						 return;
					}
			  }
			  if ( !HasRemovedRelationships() )
			  {
					_relationshipsRemoved = new RelationshipChangesForNode( DiffStrategy.REMOVE );
			  }
			  _relationshipsRemoved.addRelationship( relId, typeId, direction );
		 }

		 public override void Clear()
		 {
			  base.Clear();
			  if ( _relationshipsAdded != null )
			  {
					_relationshipsAdded.clear();
			  }
			  if ( _relationshipsRemoved != null )
			  {
					_relationshipsRemoved.clear();
			  }
			  if ( _labelDiffSets != null )
			  {
					_labelDiffSets = null;
			  }
			  if ( _indexDiffs != null )
			  {
					_indexDiffs.Clear();
			  }
		 }

		 public override int AugmentDegree( RelationshipDirection direction, int degree, int typeId )
		 {
			  if ( HasAddedRelationships() )
			  {
					degree = _relationshipsAdded.augmentDegree( direction, degree, typeId );
			  }
			  if ( HasRemovedRelationships() )
			  {
					degree = _relationshipsRemoved.augmentDegree( direction, degree, typeId );
			  }
			  return degree;
		 }

		 private bool HasAddedRelationships()
		 {
			  return _relationshipsAdded != null;
		 }

		 private bool HasRemovedRelationships()
		 {
			  return _relationshipsRemoved != null;
		 }

		 internal virtual void AddIndexDiff( MutableLongDiffSets diff )
		 {
			  if ( _indexDiffs == null )
			  {
					_indexDiffs = Collections.newSetFromMap( new IdentityHashMap<>() );
			  }
			  _indexDiffs.Add( diff );
		 }

		 internal virtual void RemoveIndexDiff( MutableLongDiffSets diff )
		 {
			  if ( _indexDiffs != null )
			  {
					_indexDiffs.remove( diff );
			  }
		 }

		 internal virtual void ClearIndexDiffs( long nodeId )
		 {
			  if ( _indexDiffs != null )
			  {
					foreach ( MutableLongDiffSets diff in _indexDiffs )
					{
						 if ( diff.Added.contains( nodeId ) )
						 {
							  diff.Remove( nodeId );
						 }
						 else if ( diff.Removed.contains( nodeId ) )
						 {
							  diff.Add( nodeId );
						 }
					}
			  }
		 }

		 public virtual LongIterator AddedRelationships
		 {
			 get
			 {
				  return _relationshipsAdded != null ? _relationshipsAdded.Relationships : ImmutableEmptyLongIterator.INSTANCE;
			 }
		 }

		 public virtual LongIterator getAddedRelationships( RelationshipDirection direction, int relType )
		 {
			  return _relationshipsAdded != null ? _relationshipsAdded.getRelationships( direction, relType ) : ImmutableEmptyLongIterator.INSTANCE;
		 }
	}

}