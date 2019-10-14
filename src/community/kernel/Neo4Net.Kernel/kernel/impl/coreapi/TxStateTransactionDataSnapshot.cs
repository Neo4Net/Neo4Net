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
namespace Neo4Net.Kernel.impl.coreapi
{
	using LongIterable = org.eclipse.collections.api.LongIterable;
	using MutableLongObjectMap = org.eclipse.collections.api.map.primitive.MutableLongObjectMap;
	using LongSet = org.eclipse.collections.api.set.primitive.LongSet;
	using LongObjectHashMap = org.eclipse.collections.impl.map.mutable.primitive.LongObjectHashMap;


	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using LabelEntry = Neo4Net.Graphdb.@event.LabelEntry;
	using Neo4Net.Graphdb.@event;
	using TransactionData = Neo4Net.Graphdb.@event.TransactionData;
	using TokenRead = Neo4Net.Internal.Kernel.Api.TokenRead;
	using EntityNotFoundException = Neo4Net.Internal.Kernel.Api.exceptions.EntityNotFoundException;
	using LabelNotFoundKernelException = Neo4Net.Internal.Kernel.Api.exceptions.LabelNotFoundKernelException;
	using PropertyKeyIdNotFoundKernelException = Neo4Net.Internal.Kernel.Api.exceptions.PropertyKeyIdNotFoundKernelException;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using KernelTransactionImplementation = Neo4Net.Kernel.Impl.Api.KernelTransactionImplementation;
	using EmbeddedProxySPI = Neo4Net.Kernel.impl.core.EmbeddedProxySPI;
	using NodeProxy = Neo4Net.Kernel.impl.core.NodeProxy;
	using RelationshipProxy = Neo4Net.Kernel.impl.core.RelationshipProxy;
	using StorageNodeCursor = Neo4Net.Storageengine.Api.StorageNodeCursor;
	using StorageProperty = Neo4Net.Storageengine.Api.StorageProperty;
	using StoragePropertyCursor = Neo4Net.Storageengine.Api.StoragePropertyCursor;
	using StorageReader = Neo4Net.Storageengine.Api.StorageReader;
	using StorageRelationshipScanCursor = Neo4Net.Storageengine.Api.StorageRelationshipScanCursor;
	using LongDiffSets = Neo4Net.Storageengine.Api.txstate.LongDiffSets;
	using NodeState = Neo4Net.Storageengine.Api.txstate.NodeState;
	using ReadableTransactionState = Neo4Net.Storageengine.Api.txstate.ReadableTransactionState;
	using RelationshipState = Neo4Net.Storageengine.Api.txstate.RelationshipState;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.toIntExact;

	/// <summary>
	/// Transform for <seealso cref="org.neo4j.storageengine.api.txstate.ReadableTransactionState"/> to make it accessible as <seealso cref="TransactionData"/>.
	/// </summary>
	public class TxStateTransactionDataSnapshot : TransactionData
	{
		 private readonly ReadableTransactionState _state;
		 private readonly EmbeddedProxySPI _proxySpi;
		 private readonly StorageReader _store;
		 private readonly KernelTransaction _transaction;

		 private readonly ICollection<PropertyEntry<Node>> _assignedNodeProperties = new List<PropertyEntry<Node>>();
		 private readonly ICollection<PropertyEntry<Relationship>> _assignedRelationshipProperties = new List<PropertyEntry<Relationship>>();
		 private readonly ICollection<LabelEntry> _assignedLabels = new List<LabelEntry>();

		 private readonly ICollection<PropertyEntry<Node>> _removedNodeProperties = new List<PropertyEntry<Node>>();
		 private readonly ICollection<PropertyEntry<Relationship>> _removedRelationshipProperties = new List<PropertyEntry<Relationship>>();
		 private readonly ICollection<LabelEntry> _removedLabels = new List<LabelEntry>();
		 private readonly MutableLongObjectMap<RelationshipProxy> _relationshipsReadFromStore = new LongObjectHashMap<RelationshipProxy>( 16 );

		 public TxStateTransactionDataSnapshot( ReadableTransactionState state, EmbeddedProxySPI proxySpi, StorageReader storageReader, KernelTransaction transaction )
		 {
			  this._state = state;
			  this._proxySpi = proxySpi;
			  this._store = storageReader;
			  this._transaction = transaction;

			  // Load changes that require store access eagerly, because we won't have access to the after-state
			  // after the tx has been committed.
			  TakeSnapshot();
		 }

		 public override IEnumerable<Node> CreatedNodes()
		 {
			  return Map2Nodes( _state.addedAndRemovedNodes().Added );
		 }

		 public override IEnumerable<Node> DeletedNodes()
		 {
			  return Map2Nodes( _state.addedAndRemovedNodes().Removed );
		 }

		 public override IEnumerable<Relationship> CreatedRelationships()
		 {
			  return Map2Rels( _state.addedAndRemovedRelationships().Added );
		 }

		 public override IEnumerable<Relationship> DeletedRelationships()
		 {
			  return Map2Rels( _state.addedAndRemovedRelationships().Removed );
		 }

		 public override bool IsDeleted( Node node )
		 {
			  return _state.nodeIsDeletedInThisTx( node.Id );
		 }

		 public override bool IsDeleted( Relationship relationship )
		 {
			  return _state.relationshipIsDeletedInThisTx( relationship.Id );
		 }

		 public override IEnumerable<PropertyEntry<Node>> AssignedNodeProperties()
		 {
			  return _assignedNodeProperties;
		 }

		 public override IEnumerable<PropertyEntry<Node>> RemovedNodeProperties()
		 {
			  return _removedNodeProperties;
		 }

		 public override IEnumerable<PropertyEntry<Relationship>> AssignedRelationshipProperties()
		 {
			  return _assignedRelationshipProperties;
		 }

		 public override IEnumerable<PropertyEntry<Relationship>> RemovedRelationshipProperties()
		 {
			  return _removedRelationshipProperties;
		 }

		 public override string Username()
		 {
			  return _transaction.securityContext().subject().username();
		 }

		 public override IDictionary<string, object> MetaData()
		 {
			  if ( _transaction is KernelTransactionImplementation )
			  {
					return ( ( KernelTransactionImplementation ) _transaction ).MetaData;
			  }
			  else
			  {
					return Collections.emptyMap();
			  }
		 }

		 public override IEnumerable<LabelEntry> RemovedLabels()
		 {
			  return _removedLabels;
		 }

		 public override IEnumerable<LabelEntry> AssignedLabels()
		 {
			  return _assignedLabels;
		 }

		 public virtual long TransactionId
		 {
			 get
			 {
				  return _transaction.TransactionId;
			 }
		 }

		 public virtual long CommitTime
		 {
			 get
			 {
				  return _transaction.CommitTime;
			 }
		 }

		 private void TakeSnapshot()
		 {
			  try
			  {
					  using ( StorageNodeCursor node = _store.allocateNodeCursor(), StoragePropertyCursor properties = _store.allocatePropertyCursor(), StorageRelationshipScanCursor relationship = _store.allocateRelationshipScanCursor() )
					  {
						TokenRead tokenRead = _transaction.tokenRead();
						_state.addedAndRemovedNodes().Removed.each(nodeId =>
						{
						 node.Single( nodeId );
						 if ( node.Next() )
						 {
							  properties.Init( node.PropertiesReference() );
							  while ( properties.Next() )
							  {
									try
									{
										 _removedNodeProperties.Add( new NodePropertyEntryView( this, nodeId, tokenRead.PropertyKeyName( properties.PropertyKey() ), null, properties.PropertyValue() ) );
									}
									catch ( PropertyKeyIdNotFoundKernelException e )
									{
										 throw new System.InvalidOperationException( "Nonexisting properties was modified for node " + nodeId, e );
									}
							  }

							  foreach ( long labelId in node.Labels() )
							  {
									try
									{
										 _removedLabels.Add( new LabelEntryView( this, nodeId, tokenRead.NodeLabelName( toIntExact( labelId ) ) ) );
									}
									catch ( LabelNotFoundKernelException e )
									{
										 throw new System.InvalidOperationException( "Nonexisting label was modified for node " + nodeId, e );
									}
							  }
						 }
						});
						_state.addedAndRemovedRelationships().Removed.each(relId =>
						{
						 Relationship relationshipProxy = relationship( relId );
						 relationship.Single( relId );
						 if ( relationship.Next() )
						 {
							  properties.Init( relationship.PropertiesReference() );
							  while ( properties.Next() )
							  {
									try
									{
										 _removedRelationshipProperties.Add( new RelationshipPropertyEntryView( relationshipProxy, tokenRead.PropertyKeyName( properties.PropertyKey() ), null, properties.PropertyValue() ) );
									}
									catch ( PropertyKeyIdNotFoundKernelException e )
									{
										 throw new System.InvalidOperationException( "Nonexisting node properties was modified for relationship " + relId, e );
									}
							  }
						 }
						});
						foreach ( NodeState nodeState in _state.modifiedNodes() )
						{
							 IEnumerator<StorageProperty> added = nodeState.AddedAndChangedProperties();
							 long nodeId = nodeState.Id;
							 while ( added.MoveNext() )
							 {
								  StorageProperty property = added.Current;
								  _assignedNodeProperties.Add( new NodePropertyEntryView( this, nodeId, tokenRead.PropertyKeyName( property.PropertyKeyId() ), property.Value(), CommittedValue(nodeState, property.PropertyKeyId(), node, properties) ) );
							 }
							 nodeState.RemovedProperties().each(id =>
							 {
							  try
							  {
									NodePropertyEntryView entryView = new NodePropertyEntryView( this, nodeId, tokenRead.PropertyKeyName( id ), null, CommittedValue( nodeState, id, node, properties ) );
									_removedNodeProperties.Add( entryView );
							  }
							  catch ( PropertyKeyIdNotFoundKernelException e )
							  {
									throw new System.InvalidOperationException( "Nonexisting node properties was modified for node " + nodeId, e );
							  }
							 });
      
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.storageengine.api.txstate.LongDiffSets labels = nodeState.labelDiffSets();
							 LongDiffSets labels = nodeState.LabelDiffSets();
							 AddLabelEntriesTo( nodeId, labels.Added, _assignedLabels );
							 AddLabelEntriesTo( nodeId, labels.Removed, _removedLabels );
						}
						foreach ( RelationshipState relState in _state.modifiedRelationships() )
						{
							 Relationship relationshipProxy = relationship( relState.Id );
							 IEnumerator<StorageProperty> added = relState.AddedAndChangedProperties();
							 while ( added.MoveNext() )
							 {
								  StorageProperty property = added.Current;
								  _assignedRelationshipProperties.Add( new RelationshipPropertyEntryView( relationshipProxy, tokenRead.PropertyKeyName( property.PropertyKeyId() ), property.Value(), CommittedValue(relState, property.PropertyKeyId(), relationship, properties) ) );
							 }
							 relState.RemovedProperties().each(id =>
							 {
							  try
							  {
									RelationshipPropertyEntryView entryView = new RelationshipPropertyEntryView( relationshipProxy, tokenRead.PropertyKeyName( id ), null, CommittedValue( relState, id, relationship, properties ) );
									_removedRelationshipProperties.Add( entryView );
							  }
							  catch ( PropertyKeyIdNotFoundKernelException e )
							  {
									throw new System.InvalidOperationException( "Nonexisting properties was modified for relationship " + relState.Id, e );
							  }
							 });
						}
					  }
			  }
			  catch ( PropertyKeyIdNotFoundKernelException e )
			  {
					throw new System.InvalidOperationException( "An entity that does not exist was modified.", e );
			  }
		 }

		 private void AddLabelEntriesTo( long nodeId, LongSet labelIds, ICollection<LabelEntry> target )
		 {
			  labelIds.each(labelId =>
			  {
				try
				{
					 LabelEntry labelEntryView = new LabelEntryView( this, nodeId, _transaction.tokenRead().nodeLabelName(toIntExact(labelId)) );
					 target.Add( labelEntryView );
				}
				catch ( LabelNotFoundKernelException e )
				{
					 throw new System.InvalidOperationException( "Nonexisting label was modified for node " + nodeId, e );
				}
			  });
		 }

		 private Relationship Relationship( long relId )
		 {
			  RelationshipProxy relationship = _proxySpi.newRelationshipProxy( relId );
			  if ( !_state.relationshipVisit( relId, relationship ) )
			  { // This relationship has been created or changed in this transaction
					RelationshipProxy cached = _relationshipsReadFromStore.get( relId );
					if ( cached != null )
					{
						 return cached;
					}

					try
					{ // Get this relationship data from the store
						 _store.relationshipVisit( relId, relationship );
						 _relationshipsReadFromStore.put( relId, relationship );
					}
					catch ( EntityNotFoundException e )
					{
						 throw new System.InvalidOperationException( "Getting deleted relationship data should have been covered by the tx state", e );
					}
			  }
			  return relationship;
		 }

		 private IEnumerable<Node> Map2Nodes( LongIterable ids )
		 {
			  return ids.asLazy().collect(id => new NodeProxy(_proxySpi, id));
		 }

		 private IEnumerable<Relationship> Map2Rels( LongIterable ids )
		 {
			  return ids.asLazy().collect(this.relationship);
		 }

		 private Value CommittedValue( NodeState nodeState, int property, StorageNodeCursor node, StoragePropertyCursor properties )
		 {
			  if ( _state.nodeIsAddedInThisTx( nodeState.Id ) )
			  {
					return Values.NO_VALUE;
			  }

			  node.Single( nodeState.Id );
			  if ( !node.Next() )
			  {
					return Values.NO_VALUE;
			  }

			  return CommittedValue( properties, node.PropertiesReference(), property );
		 }

		 private Value CommittedValue( StoragePropertyCursor properties, long propertiesReference, int propertyKey )
		 {
			  properties.Init( propertiesReference );
			  while ( properties.Next() )
			  {
					if ( properties.PropertyKey() == propertyKey )
					{
						 return properties.PropertyValue();
					}
			  }

			  return Values.NO_VALUE;
		 }

		 private Value CommittedValue( RelationshipState relState, int property, StorageRelationshipScanCursor relationship, StoragePropertyCursor properties )
		 {
			  if ( _state.relationshipIsAddedInThisTx( relState.Id ) )
			  {
					return Values.NO_VALUE;
			  }

			  relationship.Single( relState.Id );
			  if ( !relationship.Next() )
			  {
					return Values.NO_VALUE;
			  }

			  return CommittedValue( properties, relationship.PropertiesReference(), property );
		 }

		 private class NodePropertyEntryView : PropertyEntry<Node>
		 {
			 private readonly TxStateTransactionDataSnapshot _outerInstance;

			  internal readonly long NodeId;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly string KeyConflict;
			  internal readonly Value NewValue;
			  internal readonly Value OldValue;

			  internal NodePropertyEntryView( TxStateTransactionDataSnapshot outerInstance, long nodeId, string key, Value newValue, Value oldValue )
			  {
				  this._outerInstance = outerInstance;
					this.NodeId = nodeId;
					this.KeyConflict = key;
					this.NewValue = newValue;
					this.OldValue = oldValue;
			  }

			  public override Node Entity()
			  {
					return new NodeProxy( outerInstance.proxySpi, NodeId );
			  }

			  public override string Key()
			  {
					return KeyConflict;
			  }

			  public override object PreviouslyCommitedValue()
			  {
					return OldValue.asObjectCopy();
			  }

			  public override object Value()
			  {
					if ( NewValue == null || NewValue == Values.NO_VALUE )
					{
						 throw new System.InvalidOperationException( "This property has been removed, it has no value anymore: " + this );
					}
					return NewValue.asObjectCopy();
			  }

			  public override string ToString()
			  {
					return "NodePropertyEntryView{" +
							  "nodeId=" + NodeId +
							  ", key='" + KeyConflict + '\'' +
							  ", newValue=" + NewValue +
							  ", oldValue=" + OldValue +
							  '}';
			  }
		 }

		 private class RelationshipPropertyEntryView : PropertyEntry<Relationship>
		 {
			  internal readonly Relationship Relationship;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly string KeyConflict;
			  internal readonly Value NewValue;
			  internal readonly Value OldValue;

			  internal RelationshipPropertyEntryView( Relationship relationship, string key, Value newValue, Value oldValue )
			  {
					this.Relationship = relationship;
					this.KeyConflict = key;
					this.NewValue = newValue;
					this.OldValue = oldValue;
			  }

			  public override Relationship Entity()
			  {
					return Relationship;
			  }

			  public override string Key()
			  {
					return KeyConflict;
			  }

			  public override object PreviouslyCommitedValue()
			  {
					return OldValue.asObjectCopy();
			  }

			  public override object Value()
			  {
					if ( NewValue == null || NewValue == Values.NO_VALUE )
					{
						 throw new System.InvalidOperationException( "This property has been removed, it has no value anymore: " + this );
					}
					return NewValue.asObjectCopy();
			  }

			  public override string ToString()
			  {
					return "RelationshipPropertyEntryView{" +
							  "relId=" + Relationship.Id +
							  ", key='" + KeyConflict + '\'' +
							  ", newValue=" + NewValue +
							  ", oldValue=" + OldValue +
							  '}';
			  }
		 }

		 private class LabelEntryView : LabelEntry
		 {
			 private readonly TxStateTransactionDataSnapshot _outerInstance;

			  internal readonly long NodeId;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly Label LabelConflict;

			  internal LabelEntryView( TxStateTransactionDataSnapshot outerInstance, long nodeId, string labelName )
			  {
				  this._outerInstance = outerInstance;
					this.NodeId = nodeId;
					this.LabelConflict = Label.label( labelName );
			  }

			  public override Label Label()
			  {
					return LabelConflict;
			  }

			  public override Node Node()
			  {
					return new NodeProxy( outerInstance.proxySpi, NodeId );
			  }

			  public override string ToString()
			  {
					return "LabelEntryView{" +
							  "nodeId=" + NodeId +
							  ", label=" + LabelConflict +
							  '}';
			  }
		 }
	}

}