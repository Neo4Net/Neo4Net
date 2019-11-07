using System;
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
	using MutableLongObjectMap = org.eclipse.collections.api.map.primitive.MutableLongObjectMap;
	using MutableObjectLongMap = org.eclipse.collections.api.map.primitive.MutableObjectLongMap;
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;
	using UnmodifiableMap = org.eclipse.collections.impl.UnmodifiableMap;
	using LongObjectHashMap = org.eclipse.collections.impl.map.mutable.primitive.LongObjectHashMap;
	using ObjectLongHashMap = org.eclipse.collections.impl.map.mutable.primitive.ObjectLongHashMap;


	using Iterables = Neo4Net.Collections.Helpers.Iterables;
	using ConstraintValidationException = Neo4Net.Kernel.Api.Internal.Exceptions.Schema.ConstraintValidationException;
	using CreateConstraintFailureException = Neo4Net.Kernel.Api.Internal.Exceptions.Schema.CreateConstraintFailureException;
	using SchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor;
	using SchemaDescriptorPredicates = Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptorPredicates;
	using ConstraintDescriptor = Neo4Net.Kernel.Api.Internal.Schema.constraints.ConstraintDescriptor;
	using IndexBackedConstraintDescriptor = Neo4Net.Kernel.Api.schema.constraints.IndexBackedConstraintDescriptor;
	using TransactionState = Neo4Net.Kernel.Api.txstate.TransactionState;
	using CollectionsFactory = Neo4Net.Kernel.impl.util.collection.CollectionsFactory;
	using OnHeapCollectionsFactory = Neo4Net.Kernel.impl.util.collection.OnHeapCollectionsFactory;
	using Neo4Net.Kernel.impl.util.diffsets;
	using Neo4Net.Kernel.impl.util.diffsets;
	using MutableLongDiffSets = Neo4Net.Kernel.impl.util.diffsets.MutableLongDiffSets;
	using MutableLongDiffSetsImpl = Neo4Net.Kernel.impl.util.diffsets.MutableLongDiffSetsImpl;
	using RelationshipDirection = Neo4Net.Kernel.Api.StorageEngine.RelationshipDirection;
	using Neo4Net.Kernel.Api.StorageEngine;
	using IndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor;
	using Neo4Net.Kernel.Api.StorageEngine.TxState;
	using GraphState = Neo4Net.Kernel.Api.StorageEngine.TxState.GraphState;
	using LongDiffSets = Neo4Net.Kernel.Api.StorageEngine.TxState.LongDiffSets;
	using NodeState = Neo4Net.Kernel.Api.StorageEngine.TxState.NodeState;
	using RelationshipState = Neo4Net.Kernel.Api.StorageEngine.TxState.RelationshipState;
	using TxStateVisitor = Neo4Net.Kernel.Api.StorageEngine.TxState.TxStateVisitor;
	using VisibleForTesting = Neo4Net.Utils.VisibleForTesting;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueTuple = Neo4Net.Values.Storable.ValueTuple;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterables.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.NO_VALUE;

	/// <summary>
	/// This class contains transaction-local changes to the graph. These changes can then be used to augment reads from the
	/// committed state of the database (to make the local changes appear in local transaction read operations). At commit
	/// time a visitor is sent into this class to convert the end result of the tx changes into a physical changeset.
	/// <para>
	/// See <seealso cref="Neo4Net.kernel.impl.api.KernelTransactionImplementation"/> for how this happens.
	/// </para>
	/// <para>
	/// This class is very large, as it has been used as a gathering point to consolidate all transaction state knowledge
	/// into one component. Now that that work is done, this class should be refactored to increase transparency in how it
	/// works.
	/// </para>
	/// </summary>
	public class TxState : TransactionState, Neo4Net.Kernel.Api.StorageEngine.RelationshipVisitor_Home
	{
		 /// <summary>
		 /// This factory must be used only for creating collections representing internal state that doesn't leak outside this class.
		 /// </summary>
		 private readonly CollectionsFactory _collectionsFactory;

		 private MutableLongObjectMap<MutableLongDiffSets> _labelStatesMap;
		 private MutableLongObjectMap<NodeStateImpl> _nodeStatesMap;
		 private MutableLongObjectMap<RelationshipStateImpl> _relationshipStatesMap;

		 private MutableLongObjectMap<string> _createdLabelTokens;
		 private MutableLongObjectMap<string> _createdPropertyKeyTokens;
		 private MutableLongObjectMap<string> _createdRelationshipTypeTokens;

		 private GraphStateImpl _graphState;
		 private MutableDiffSets<IndexDescriptor> _indexChanges;
		 private MutableDiffSets<ConstraintDescriptor> _constraintsChanges;

		 private RemovalsCountingDiffSets _nodes;
		 private RemovalsCountingDiffSets _relationships;

		 private MutableObjectLongMap<IndexBackedConstraintDescriptor> _createdConstraintIndexesByConstraint;

		 private IDictionary<SchemaDescriptor, IDictionary<ValueTuple, MutableLongDiffSets>> _indexUpdates;

		 private long _revision;
		 private long _dataRevision;

		 public TxState() : this(OnHeapCollectionsFactory.INSTANCE)
		 {
		 }

		 public TxState( CollectionsFactory collectionsFactory )
		 {
			  this._collectionsFactory = collectionsFactory;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void accept(final Neo4Net.Kernel.Api.StorageEngine.TxState.TxStateVisitor visitor) throws Neo4Net.Kernel.Api.Internal.Exceptions.Schema.ConstraintValidationException, Neo4Net.Kernel.Api.Internal.Exceptions.Schema.CreateConstraintFailureException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public override void Accept( TxStateVisitor visitor )
		 {
			  if ( _nodes != null )
			  {
					_nodes.Added.each( visitor.visitCreatedNode );
			  }

			  if ( _relationships != null )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.iterator.LongIterator added = relationships.getAdded().longIterator();
					LongIterator added = _relationships.Added.longIterator();
					while ( added.hasNext() )
					{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long relId = added.next();
						 long relId = added.next();
						 if ( !RelationshipVisit( relId, visitor.visitCreatedRelationship ) )
						 {
							  throw new System.InvalidOperationException( "No RelationshipState for added relationship!" );
						 }
					}
					_relationships.Removed.forEach( visitor.visitDeletedRelationship );
			  }

			  if ( _nodes != null )
			  {
					_nodes.Removed.each( visitor.visitDeletedNode );
			  }

			  foreach ( NodeState node in ModifiedNodes() )
			  {
					if ( node.HasPropertyChanges() )
					{
						 visitor.VisitNodePropertyChanges( node.Id, node.AddedProperties(), node.ChangedProperties(), node.RemovedProperties() );
					}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.Kernel.Api.StorageEngine.TxState.LongDiffSets labelDiffSets = node.labelDiffSets();
					LongDiffSets labelDiffSets = node.LabelDiffSets();
					if ( !labelDiffSets.Empty )
					{
						 visitor.VisitNodeLabelChanges( node.Id, labelDiffSets.Added, labelDiffSets.Removed );
					}
			  }

			  foreach ( RelationshipState rel in ModifiedRelationships() )
			  {
					visitor.VisitRelPropertyChanges( rel.Id, rel.AddedProperties(), rel.ChangedProperties(), rel.RemovedProperties() );
			  }

			  if ( _graphState != null )
			  {
					visitor.VisitGraphPropertyChanges( _graphState.addedProperties(), _graphState.changedProperties(), _graphState.removedProperties() );
			  }

			  if ( _indexChanges != null )
			  {
					_indexChanges.Added.forEach( visitor.visitAddedIndex );
					_indexChanges.Removed.forEach( visitor.visitRemovedIndex );
			  }

			  if ( _constraintsChanges != null )
			  {
					foreach ( ConstraintDescriptor added in _constraintsChanges.Added )
					{
						 visitor.VisitAddedConstraint( added );
					}
					_constraintsChanges.Removed.forEach( visitor.visitRemovedConstraint );
			  }

			  if ( _createdLabelTokens != null )
			  {
					_createdLabelTokens.forEachKeyValue( visitor.visitCreatedLabelToken );
			  }

			  if ( _createdPropertyKeyTokens != null )
			  {
					_createdPropertyKeyTokens.forEachKeyValue( visitor.visitCreatedPropertyKeyToken );
			  }

			  if ( _createdRelationshipTypeTokens != null )
			  {
					_createdRelationshipTypeTokens.forEachKeyValue( visitor.visitCreatedRelationshipTypeToken );
			  }
		 }

		 public override bool HasChanges()
		 {
			  return _revision != 0;
		 }

		 public override IEnumerable<NodeState> ModifiedNodes()
		 {
			  return _nodeStatesMap == null ? Iterables.empty() : Iterables.cast(_nodeStatesMap.values());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting MutableLongDiffSets getOrCreateLabelStateNodeDiffSets(long labelId)
		 internal virtual MutableLongDiffSets GetOrCreateLabelStateNodeDiffSets( long labelId )
		 {
			  if ( _labelStatesMap == null )
			  {
					_labelStatesMap = new LongObjectHashMap<MutableLongDiffSets>();
			  }
			  return _labelStatesMap.getIfAbsentPut( labelId, () => new MutableLongDiffSetsImpl(_collectionsFactory) );
		 }

		 private LongDiffSets GetLabelStateNodeDiffSets( long labelId )
		 {
			  if ( _labelStatesMap == null )
			  {
					return LongDiffSets.EMPTY;
			  }
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.Kernel.Api.StorageEngine.TxState.LongDiffSets nodeDiffSets = labelStatesMap.get(labelId);
			  LongDiffSets nodeDiffSets = _labelStatesMap.get( labelId );
			  return nodeDiffSets == null ? LongDiffSets.EMPTY : nodeDiffSets;
		 }

		 public override LongDiffSets NodeStateLabelDiffSets( long nodeId )
		 {
			  return GetNodeState( nodeId ).labelDiffSets();
		 }

		 private MutableLongDiffSets GetOrCreateNodeStateLabelDiffSets( long nodeId )
		 {
			  return GetOrCreateNodeState( nodeId ).OrCreateLabelDiffSets;
		 }

		 public override bool NodeIsAddedInThisTx( long nodeId )
		 {
			  return _nodes != null && _nodes.isAdded( nodeId );
		 }

		 public override bool RelationshipIsAddedInThisTx( long relationshipId )
		 {
			  return _relationships != null && _relationships.isAdded( relationshipId );
		 }

		 private void Changed()
		 {
			  _revision++;
		 }

		 private void DataChanged()
		 {
			  Changed();
			  _dataRevision = _revision;
		 }

		 public override void NodeDoCreate( long id )
		 {
			  Nodes().add(id);
			  DataChanged();
		 }

		 public override void NodeDoDelete( long nodeId )
		 {
			  Nodes().remove(nodeId);

			  if ( _nodeStatesMap != null )
			  {
					NodeStateImpl nodeState = _nodeStatesMap.remove( nodeId );
					if ( nodeState != null )
					{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.Kernel.Api.StorageEngine.TxState.LongDiffSets diff = nodeState.labelDiffSets();
						 LongDiffSets diff = nodeState.LabelDiffSets();
						 diff.Added.each( label => GetOrCreateLabelStateNodeDiffSets( label ).remove( nodeId ) );
						 nodeState.ClearIndexDiffs( nodeId );
						 nodeState.Clear();
					}
			  }
			  DataChanged();
		 }

		 public override void RelationshipDoCreate( long id, int relationshipTypeId, long startNodeId, long endNodeId )
		 {
			  Relationships().add(id);

			  if ( startNodeId == endNodeId )
			  {
					GetOrCreateNodeState( startNodeId ).addRelationship( id, relationshipTypeId, RelationshipDirection.LOOP );
			  }
			  else
			  {
					GetOrCreateNodeState( startNodeId ).addRelationship( id, relationshipTypeId, RelationshipDirection.OUTGOING );
					GetOrCreateNodeState( endNodeId ).addRelationship( id, relationshipTypeId, RelationshipDirection.INCOMING );
			  }

			  GetOrCreateRelationshipState( id ).setMetaData( startNodeId, endNodeId, relationshipTypeId );

			  DataChanged();
		 }

		 public override bool NodeIsDeletedInThisTx( long nodeId )
		 {
			  return _nodes != null && _nodes.wasRemoved( nodeId );
		 }

		 public override void RelationshipDoDelete( long id, int type, long startNodeId, long endNodeId )
		 {
			  Relationships().remove(id);

			  if ( startNodeId == endNodeId )
			  {
					GetOrCreateNodeState( startNodeId ).removeRelationship( id, type, RelationshipDirection.LOOP );
			  }
			  else
			  {
					GetOrCreateNodeState( startNodeId ).removeRelationship( id, type, RelationshipDirection.OUTGOING );
					GetOrCreateNodeState( endNodeId ).removeRelationship( id, type, RelationshipDirection.INCOMING );
			  }

			  if ( _relationshipStatesMap != null )
			  {
					RelationshipStateImpl removed = _relationshipStatesMap.remove( id );
					if ( removed != null )
					{
						 removed.Clear();
					}
			  }

			  DataChanged();
		 }

		 public override void RelationshipDoDeleteAddedInThisTx( long relationshipId )
		 {
			  GetRelationshipState( relationshipId ).accept( this.relationshipDoDelete );
		 }

		 public override bool RelationshipIsDeletedInThisTx( long relationshipId )
		 {
			  return _relationships != null && _relationships.wasRemoved( relationshipId );
		 }

		 public override void NodeDoAddProperty( long nodeId, int newPropertyKeyId, Value value )
		 {
			  NodeStateImpl nodeState = GetOrCreateNodeState( nodeId );
			  nodeState.AddProperty( newPropertyKeyId, value );
			  DataChanged();
		 }

		 public override void NodeDoChangeProperty( long nodeId, int propertyKeyId, Value newValue )
		 {
			  GetOrCreateNodeState( nodeId ).changeProperty( propertyKeyId, newValue );
			  DataChanged();
		 }

		 public override void RelationshipDoReplaceProperty( long relationshipId, int propertyKeyId, Value replacedValue, Value newValue )
		 {
			  if ( replacedValue != NO_VALUE )
			  {
					GetOrCreateRelationshipState( relationshipId ).changeProperty( propertyKeyId, newValue );
			  }
			  else
			  {
					GetOrCreateRelationshipState( relationshipId ).addProperty( propertyKeyId, newValue );
			  }
			  DataChanged();
		 }

		 public override void GraphDoReplaceProperty( int propertyKeyId, Value replacedValue, Value newValue )
		 {
			  if ( replacedValue != NO_VALUE )
			  {
					OrCreateGraphState.changeProperty( propertyKeyId, newValue );
			  }
			  else
			  {
					OrCreateGraphState.addProperty( propertyKeyId, newValue );
			  }
			  DataChanged();
		 }

		 public override void NodeDoRemoveProperty( long nodeId, int propertyKeyId )
		 {
			  GetOrCreateNodeState( nodeId ).removeProperty( propertyKeyId );
			  DataChanged();
		 }

		 public override void RelationshipDoRemoveProperty( long relationshipId, int propertyKeyId )
		 {
			  GetOrCreateRelationshipState( relationshipId ).removeProperty( propertyKeyId );
			  DataChanged();
		 }

		 public override void GraphDoRemoveProperty( int propertyKeyId )
		 {
			  OrCreateGraphState.removeProperty( propertyKeyId );
			  DataChanged();
		 }

		 public override void NodeDoAddLabel( long labelId, long nodeId )
		 {
			  GetOrCreateLabelStateNodeDiffSets( labelId ).add( nodeId );
			  GetOrCreateNodeStateLabelDiffSets( nodeId ).add( labelId );
			  DataChanged();
		 }

		 public override void NodeDoRemoveLabel( long labelId, long nodeId )
		 {
			  GetOrCreateLabelStateNodeDiffSets( labelId ).remove( nodeId );
			  GetOrCreateNodeStateLabelDiffSets( nodeId ).remove( labelId );
			  DataChanged();
		 }

		 public override void LabelDoCreateForName( string labelName, long id )
		 {
			  if ( _createdLabelTokens == null )
			  {
					_createdLabelTokens = new LongObjectHashMap<string>();
			  }
			  _createdLabelTokens.put( id, labelName );
			  Changed();
		 }

		 public override void PropertyKeyDoCreateForName( string propertyKeyName, int id )
		 {
			  if ( _createdPropertyKeyTokens == null )
			  {
					_createdPropertyKeyTokens = new LongObjectHashMap<string>();
			  }
			  _createdPropertyKeyTokens.put( id, propertyKeyName );
			  Changed();
		 }

		 public override void RelationshipTypeDoCreateForName( string labelName, int id )
		 {
			  if ( _createdRelationshipTypeTokens == null )
			  {
					_createdRelationshipTypeTokens = new LongObjectHashMap<string>();
			  }
			  _createdRelationshipTypeTokens.put( id, labelName );
			  Changed();
		 }

		 public override NodeState GetNodeState( long id )
		 {
			  if ( _nodeStatesMap == null )
			  {
					return NodeStateImpl.EMPTY;
			  }
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.Kernel.Api.StorageEngine.TxState.NodeState nodeState = nodeStatesMap.get(id);
			  NodeState nodeState = _nodeStatesMap.get( id );
			  return nodeState == null ? NodeStateImpl.EMPTY : nodeState;
		 }

		 public override RelationshipState GetRelationshipState( long id )
		 {
			  if ( _relationshipStatesMap == null )
			  {
					return RelationshipStateImpl.EMPTY;
			  }
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final RelationshipStateImpl relationshipState = relationshipStatesMap.get(id);
			  RelationshipStateImpl relationshipState = _relationshipStatesMap.get( id );
			  return relationshipState == null ? RelationshipStateImpl.EMPTY : relationshipState;
		 }

		 public virtual GraphState GraphState
		 {
			 get
			 {
				  return _graphState;
			 }
		 }

		 public override MutableLongSet AugmentLabels( MutableLongSet labels, NodeState nodeState )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.Kernel.Api.StorageEngine.TxState.LongDiffSets labelDiffSets = nodeState.labelDiffSets();
			  LongDiffSets labelDiffSets = nodeState.LabelDiffSets();
			  if ( !labelDiffSets.Empty )
			  {
					labelDiffSets.Removed.forEach( labels.remove );
					labelDiffSets.Added.forEach( labels.add );
			  }
			  return labels;
		 }

		 public override LongDiffSets NodesWithLabelChanged( long label )
		 {
			  return GetLabelStateNodeDiffSets( label );
		 }

		 public override void IndexDoAdd( IndexDescriptor descriptor )
		 {
			  MutableDiffSets<IndexDescriptor> diff = IndexChangesDiffSets();
			  if ( !diff.UnRemove( descriptor ) )
			  {
					diff.Add( descriptor );
			  }
			  Changed();
		 }

		 public override void IndexDoDrop( IndexDescriptor descriptor )
		 {
			  IndexChangesDiffSets().remove(descriptor);
			  Changed();
		 }

		 public override bool IndexDoUnRemove( IndexDescriptor descriptor )
		 {
			  return IndexChangesDiffSets().unRemove(descriptor);
		 }

		 public override DiffSets<IndexDescriptor> IndexDiffSetsByLabel( int labelId )
		 {
			  return IndexChangesDiffSets().filterAdded(SchemaDescriptorPredicates.hasLabel(labelId));
		 }

		 public override DiffSets<IndexDescriptor> IndexDiffSetsByRelationshipType( int relationshipType )
		 {
			  return IndexChangesDiffSets().filterAdded(SchemaDescriptorPredicates.hasRelType(relationshipType));
		 }

		 public override DiffSets<IndexDescriptor> IndexDiffSetsBySchema( SchemaDescriptor schema )
		 {
			  return IndexChangesDiffSets().filterAdded(indexDescriptor => indexDescriptor.schema().Equals(schema));
		 }

		 public override DiffSets<IndexDescriptor> IndexChanges()
		 {
			  return Neo4Net.Kernel.Api.StorageEngine.TxState.DiffSets_Empty.IfNull( _indexChanges );
		 }

		 private MutableDiffSets<IndexDescriptor> IndexChangesDiffSets()
		 {
			  if ( _indexChanges == null )
			  {
					_indexChanges = new MutableDiffSetsImpl<IndexDescriptor>();
			  }
			  return _indexChanges;
		 }

		 public override LongDiffSets AddedAndRemovedNodes()
		 {
			  return _nodes == null ? LongDiffSets.EMPTY : _nodes;
		 }

		 private RemovalsCountingDiffSets Nodes()
		 {
			  if ( _nodes == null )
			  {
					_nodes = new RemovalsCountingDiffSets( this );
			  }
			  return _nodes;
		 }

		 public override LongDiffSets AddedAndRemovedRelationships()
		 {
			  return _relationships == null ? LongDiffSets.EMPTY : _relationships;
		 }

		 private RemovalsCountingDiffSets Relationships()
		 {
			  if ( _relationships == null )
			  {
					_relationships = new RemovalsCountingDiffSets( this );
			  }
			  return _relationships;
		 }

		 public override IEnumerable<RelationshipState> ModifiedRelationships()
		 {
			  return _relationshipStatesMap == null ? Iterables.empty() : Iterables.cast(_relationshipStatesMap.values());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting NodeStateImpl getOrCreateNodeState(long nodeId)
		 internal virtual NodeStateImpl GetOrCreateNodeState( long nodeId )
		 {
			  if ( _nodeStatesMap == null )
			  {
					_nodeStatesMap = new LongObjectHashMap<NodeStateImpl>();
			  }
			  return _nodeStatesMap.getIfAbsentPut( nodeId, () => new NodeStateImpl(nodeId, _collectionsFactory) );
		 }

		 private RelationshipStateImpl GetOrCreateRelationshipState( long relationshipId )
		 {
			  if ( _relationshipStatesMap == null )
			  {
					_relationshipStatesMap = new LongObjectHashMap<RelationshipStateImpl>();
			  }
			  return _relationshipStatesMap.getIfAbsentPut( relationshipId, () => new RelationshipStateImpl(relationshipId, _collectionsFactory) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting GraphStateImpl getOrCreateGraphState()
		 internal virtual GraphStateImpl OrCreateGraphState
		 {
			 get
			 {
				  if ( _graphState == null )
				  {
						_graphState = new GraphStateImpl( _collectionsFactory );
				  }
				  return _graphState;
			 }
		 }

		 public override void ConstraintDoAdd( IndexBackedConstraintDescriptor constraint, long indexId )
		 {
			  ConstraintsChangesDiffSets().add(constraint);
			  CreatedConstraintIndexesByConstraint().put(constraint, indexId);
			  Changed();
		 }

		 public override void ConstraintDoAdd( ConstraintDescriptor constraint )
		 {
			  ConstraintsChangesDiffSets().add(constraint);
			  Changed();
		 }

		 public override DiffSets<ConstraintDescriptor> ConstraintsChangesForLabel( int labelId )
		 {
			  return ConstraintsChangesDiffSets().filterAdded(SchemaDescriptorPredicates.hasLabel(labelId));
		 }

		 public override DiffSets<ConstraintDescriptor> ConstraintsChangesForSchema( SchemaDescriptor descriptor )
		 {
			  return ConstraintsChangesDiffSets().filterAdded(SchemaDescriptor.equalTo(descriptor));
		 }

		 public override DiffSets<ConstraintDescriptor> ConstraintsChangesForRelationshipType( int relTypeId )
		 {
			  return ConstraintsChangesDiffSets().filterAdded(SchemaDescriptorPredicates.hasRelType(relTypeId));
		 }

		 public override DiffSets<ConstraintDescriptor> ConstraintsChanges()
		 {
			  return Neo4Net.Kernel.Api.StorageEngine.TxState.DiffSets_Empty.IfNull( _constraintsChanges );
		 }

		 private MutableDiffSets<ConstraintDescriptor> ConstraintsChangesDiffSets()
		 {
			  if ( _constraintsChanges == null )
			  {
					_constraintsChanges = new MutableDiffSetsImpl<ConstraintDescriptor>();
			  }
			  return _constraintsChanges;
		 }

		 public override void ConstraintDoDrop( ConstraintDescriptor constraint )
		 {
			  ConstraintsChangesDiffSets().remove(constraint);
			  if ( constraint.EnforcesUniqueness() )
			  {
					IndexDoDrop( GetIndexForIndexBackedConstraint( ( IndexBackedConstraintDescriptor ) constraint ) );
			  }
			  Changed();
		 }

		 public override bool ConstraintDoUnRemove( ConstraintDescriptor constraint )
		 {
			  return ConstraintsChangesDiffSets().unRemove(constraint);
		 }

		 public override IEnumerable<IndexDescriptor> ConstraintIndexesCreatedInTx()
		 {
			  if ( _createdConstraintIndexesByConstraint != null && !_createdConstraintIndexesByConstraint.Empty )
			  {
					return map( TxState.getIndexForIndexBackedConstraint, _createdConstraintIndexesByConstraint.Keys );
			  }
			  return Iterables.empty();
		 }

		 public override long? IndexCreatedForConstraint( ConstraintDescriptor constraint )
		 {
			  return _createdConstraintIndexesByConstraint == null ? null : _createdConstraintIndexesByConstraint.get( constraint );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @Nullable public org.eclipse.collections.impl.UnmodifiableMap<Neo4Net.values.storable.ValueTuple, ? extends Neo4Net.Kernel.Api.StorageEngine.TxState.LongDiffSets> getIndexUpdates(Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor schema)
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public override UnmodifiableMap<ValueTuple, ? extends LongDiffSets> GetIndexUpdates( SchemaDescriptor schema )
		 {
			  if ( _indexUpdates == null )
			  {
					return null;
			  }
			  IDictionary<ValueTuple, MutableLongDiffSets> updates = _indexUpdates[schema];
			  if ( updates == null )
			  {
					return null;
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: return new org.eclipse.collections.impl.UnmodifiableMap<>(updates);
			  return new UnmodifiableMap<ValueTuple, ? extends LongDiffSets>( updates );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @Nullable public java.util.NavigableMap<Neo4Net.values.storable.ValueTuple, ? extends Neo4Net.Kernel.Api.StorageEngine.TxState.LongDiffSets> getSortedIndexUpdates(Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor descriptor)
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public override NavigableMap<ValueTuple, ? extends LongDiffSets> GetSortedIndexUpdates( SchemaDescriptor descriptor )
		 {
			  if ( _indexUpdates == null )
			  {
					return null;
			  }
			  IDictionary<ValueTuple, MutableLongDiffSets> updates = _indexUpdates[descriptor];
			  if ( updates == null )
			  {
					return null;
			  }
			  SortedDictionary<ValueTuple, MutableLongDiffSets> sortedUpdates;
			  if ( updates is SortedDictionary )
			  {
					sortedUpdates = ( SortedDictionary<ValueTuple, MutableLongDiffSets> ) updates;
			  }
			  else
			  {
					sortedUpdates = new SortedDictionary<ValueTuple, MutableLongDiffSets>( ValueTuple.COMPARATOR );
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
					sortedUpdates.putAll( updates );
					_indexUpdates[descriptor] = sortedUpdates;
			  }
			  return Collections.unmodifiableNavigableMap( sortedUpdates );
		 }

		 public override void IndexDoUpdateEntry( SchemaDescriptor descriptor, long nodeId, ValueTuple propertiesBefore, ValueTuple propertiesAfter )
		 {
			  NodeStateImpl nodeState = GetOrCreateNodeState( nodeId );
			  IDictionary<ValueTuple, MutableLongDiffSets> updates = GetOrCreateIndexUpdatesByDescriptor( descriptor );
			  if ( propertiesBefore != null )
			  {
					MutableLongDiffSets before = GetOrCreateIndexUpdatesForSeek( updates, propertiesBefore );
					//noinspection ConstantConditions
					before.Remove( nodeId );
					if ( before.Removed.contains( nodeId ) )
					{
						 nodeState.AddIndexDiff( before );
					}
					else
					{
						 nodeState.RemoveIndexDiff( before );
					}
			  }
			  if ( propertiesAfter != null )
			  {
					MutableLongDiffSets after = GetOrCreateIndexUpdatesForSeek( updates, propertiesAfter );
					//noinspection ConstantConditions
					after.Add( nodeId );
					if ( after.Added.contains( nodeId ) )
					{
						 nodeState.AddIndexDiff( after );
					}
					else
					{
						 nodeState.RemoveIndexDiff( after );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting MutableLongDiffSets getOrCreateIndexUpdatesForSeek(java.util.Map<Neo4Net.values.storable.ValueTuple, Neo4Net.kernel.impl.util.diffsets.MutableLongDiffSets> updates, Neo4Net.values.storable.ValueTuple values)
		 internal virtual MutableLongDiffSets GetOrCreateIndexUpdatesForSeek( IDictionary<ValueTuple, MutableLongDiffSets> updates, ValueTuple values )
		 {
			  return updates.computeIfAbsent( values, value => new MutableLongDiffSetsImpl( _collectionsFactory ) );
		 }

		 private IDictionary<ValueTuple, MutableLongDiffSets> GetOrCreateIndexUpdatesByDescriptor( SchemaDescriptor schema )
		 {
			  if ( _indexUpdates == null )
			  {
					_indexUpdates = new Dictionary<SchemaDescriptor, IDictionary<ValueTuple, MutableLongDiffSets>>();
			  }
			  return _indexUpdates.computeIfAbsent( schema, k => new Dictionary<>() );
		 }

		 private MutableObjectLongMap<IndexBackedConstraintDescriptor> CreatedConstraintIndexesByConstraint()
		 {
			  if ( _createdConstraintIndexesByConstraint == null )
			  {
					_createdConstraintIndexesByConstraint = new ObjectLongHashMap<IndexBackedConstraintDescriptor>();
			  }
			  return _createdConstraintIndexesByConstraint;
		 }

		 private static IndexDescriptor GetIndexForIndexBackedConstraint( IndexBackedConstraintDescriptor constraint )
		 {
			  return constraint.OwnedIndexDescriptor();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <EX extends Exception> boolean relationshipVisit(long relId, Neo4Net.Kernel.Api.StorageEngine.RelationshipVisitor<EX> visitor) throws EX
		 public override bool RelationshipVisit<EX>( long relId, RelationshipVisitor<EX> visitor ) where EX : Exception
		 {
			  return GetRelationshipState( relId ).accept( visitor );
		 }

		 public override bool HasDataChanges()
		 {
			  return _dataRevision != 0;
		 }

		 public virtual long DataRevision
		 {
			 get
			 {
				  return _dataRevision;
			 }
		 }

		 /// <summary>
		 /// This class works around the fact that create-delete in the same transaction is a no-op in <seealso cref="MutableDiffSetsImpl"/>,
		 /// whereas we need to know total number of explicit removals.
		 /// </summary>
		 private class RemovalsCountingDiffSets : MutableLongDiffSetsImpl
		 {
			 private readonly TxState _outerInstance;

			  internal MutableLongSet RemovedFromAdded;

			  internal RemovalsCountingDiffSets( TxState outerInstance ) : base( outerInstance.collectionsFactory )
			  {
				  this._outerInstance = outerInstance;
			  }

			  public override bool Remove( long elem )
			  {
					if ( IsAdded( elem ) && base.Remove( elem ) )
					{
						 if ( RemovedFromAdded == null )
						 {
							  RemovedFromAdded = outerInstance.collectionsFactory.NewLongSet();
						 }
						 RemovedFromAdded.add( elem );
						 return true;
					}
					return base.Remove( elem );
			  }

			  internal virtual bool WasRemoved( long id )
			  {
					return ( RemovedFromAdded != null && RemovedFromAdded.contains( id ) ) || base.IsRemoved( id );
			  }
		 }
	}

}