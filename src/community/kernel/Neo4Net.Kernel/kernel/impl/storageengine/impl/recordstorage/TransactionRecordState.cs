using System.Collections.Generic;
using System.Diagnostics;

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
namespace Neo4Net.Kernel.impl.storageengine.impl.recordstorage
{

	using TransactionFailureException = Neo4Net.Internal.Kernel.Api.exceptions.TransactionFailureException;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using MetaDataStore = Neo4Net.Kernel.impl.store.MetaDataStore;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using NodeStore = Neo4Net.Kernel.impl.store.NodeStore;
	using PropertyStore = Neo4Net.Kernel.impl.store.PropertyStore;
	using Neo4Net.Kernel.impl.store;
	using RelationshipStore = Neo4Net.Kernel.impl.store.RelationshipStore;
	using SchemaStore = Neo4Net.Kernel.impl.store.SchemaStore;
	using AbstractBaseRecord = Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord;
	using DynamicRecord = Neo4Net.Kernel.Impl.Store.Records.DynamicRecord;
	using LabelTokenRecord = Neo4Net.Kernel.Impl.Store.Records.LabelTokenRecord;
	using NeoStoreRecord = Neo4Net.Kernel.Impl.Store.Records.NeoStoreRecord;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using PrimitiveRecord = Neo4Net.Kernel.Impl.Store.Records.PrimitiveRecord;
	using PropertyKeyTokenRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyKeyTokenRecord;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;
	using Record = Neo4Net.Kernel.Impl.Store.Records.Record;
	using RelationshipGroupRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;
	using RelationshipTypeTokenRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipTypeTokenRecord;
	using SchemaRecord = Neo4Net.Kernel.Impl.Store.Records.SchemaRecord;
	using Command = Neo4Net.Kernel.impl.transaction.command.Command;
	using Mode = Neo4Net.Kernel.impl.transaction.command.Command.Mode;
	using IntegrityValidator = Neo4Net.Kernel.impl.transaction.state.IntegrityValidator;
	using Neo4Net.Kernel.impl.transaction.state;
	using RecordAccessSet = Neo4Net.Kernel.impl.transaction.state.RecordAccessSet;
	using RecordChangeSet = Neo4Net.Kernel.impl.transaction.state.RecordChangeSet;
	using Neo4Net.Kernel.impl.transaction.state;
	using Neo4Net.Kernel.impl.transaction.state;
	using IntCounter = Neo4Net.Kernel.impl.util.statistics.IntCounter;
	using StorageCommand = Neo4Net.Storageengine.Api.StorageCommand;
	using StorageProperty = Neo4Net.Storageengine.Api.StorageProperty;
	using ResourceLocker = Neo4Net.Storageengine.Api.@lock.ResourceLocker;
	using SchemaRule = Neo4Net.Storageengine.Api.schema.SchemaRule;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;
	using Value = Neo4Net.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.NodeLabelsField.parseLabelsField;

	/// <summary>
	/// Transaction containing <seealso cref="org.neo4j.kernel.impl.transaction.command.Command commands"/> reflecting the operations
	/// performed in the transaction.
	/// 
	/// This class currently has a symbiotic relationship with <seealso cref="KernelTransaction"/>, with which it always has a 1-1
	/// relationship.
	/// 
	/// The idea here is that KernelTransaction will eventually take on the responsibilities of WriteTransaction, such as
	/// keeping track of transaction state, serialization and deserialization to and from logical log, and applying things
	/// to store. It would most likely do this by keeping a component derived from the current WriteTransaction
	/// implementation as a sub-component, responsible for handling logical log commands.
	/// </summary>
	public class TransactionRecordState : RecordState
	{
		 private static readonly CommandComparator _commandComparator = new CommandComparator();
		 private static readonly Command[] _emptyCommands = new Command[0];

		 private readonly NeoStores _neoStores;
		 private readonly IntegrityValidator _integrityValidator;
		 private readonly NodeStore _nodeStore;
		 private readonly RelationshipStore _relationshipStore;
		 private readonly PropertyStore _propertyStore;
		 private readonly RecordStore<RelationshipGroupRecord> _relationshipGroupStore;
		 private readonly MetaDataStore _metaDataStore;
		 private readonly SchemaStore _schemaStore;
		 private readonly RecordAccessSet _recordChangeSet;
		 private readonly long _lastCommittedTxWhenTransactionStarted;
		 private readonly ResourceLocker _locks;
		 private readonly RelationshipCreator _relationshipCreator;
		 private readonly RelationshipDeleter _relationshipDeleter;
		 private readonly PropertyCreator _propertyCreator;
		 private readonly PropertyDeleter _propertyDeleter;

		 private RecordChanges<NeoStoreRecord, Void> _neoStoreRecord;
		 private bool _prepared;

		 internal TransactionRecordState( NeoStores neoStores, IntegrityValidator integrityValidator, RecordChangeSet recordChangeSet, long lastCommittedTxWhenTransactionStarted, ResourceLocker locks, RelationshipCreator relationshipCreator, RelationshipDeleter relationshipDeleter, PropertyCreator propertyCreator, PropertyDeleter propertyDeleter )
		 {
			  this._neoStores = neoStores;
			  this._nodeStore = neoStores.NodeStore;
			  this._relationshipStore = neoStores.RelationshipStore;
			  this._propertyStore = neoStores.PropertyStore;
			  this._relationshipGroupStore = neoStores.RelationshipGroupStore;
			  this._metaDataStore = neoStores.MetaDataStore;
			  this._schemaStore = neoStores.SchemaStore;
			  this._integrityValidator = integrityValidator;
			  this._recordChangeSet = recordChangeSet;
			  this._lastCommittedTxWhenTransactionStarted = lastCommittedTxWhenTransactionStarted;
			  this._locks = locks;
			  this._relationshipCreator = relationshipCreator;
			  this._relationshipDeleter = relationshipDeleter;
			  this._propertyCreator = propertyCreator;
			  this._propertyDeleter = propertyDeleter;
		 }

		 public override bool HasChanges()
		 {
			  return _recordChangeSet.hasChanges() || (_neoStoreRecord != null && _neoStoreRecord.changeSize() > 0);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void extractCommands(java.util.Collection<org.neo4j.storageengine.api.StorageCommand> commands) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 public override void ExtractCommands( ICollection<StorageCommand> commands )
		 {
			  Debug.Assert( !_prepared, "Transaction has already been prepared" );

			  _integrityValidator.validateTransactionStartKnowledge( _lastCommittedTxWhenTransactionStarted );

			  int noOfCommands = _recordChangeSet.changeSize() + (_neoStoreRecord != null ? _neoStoreRecord.changeSize() : 0);

			  foreach ( RecordAccess_RecordProxy<LabelTokenRecord, Void> record in _recordChangeSet.LabelTokenChanges.changes() )
			  {
					commands.Add( new Command.LabelTokenCommand( record.Before, record.ForReadingLinkage() ) );
			  }
			  foreach ( RecordAccess_RecordProxy<RelationshipTypeTokenRecord, Void> record in _recordChangeSet.RelationshipTypeTokenChanges.changes() )
			  {
					commands.Add( new Command.RelationshipTypeTokenCommand( record.Before, record.ForReadingLinkage() ) );
			  }
			  foreach ( RecordAccess_RecordProxy<PropertyKeyTokenRecord, Void> record in _recordChangeSet.PropertyKeyTokenChanges.changes() )
			  {
					commands.Add( new Command.PropertyKeyTokenCommand( record.Before, record.ForReadingLinkage() ) );
			  }

			  // Collect nodes, relationships, properties
			  Command[] nodeCommands = _emptyCommands;
			  int skippedCommands = 0;
			  if ( _recordChangeSet.NodeRecords.changeSize() > 0 )
			  {
					nodeCommands = new Command[_recordChangeSet.NodeRecords.changeSize()];
					int i = 0;
					foreach ( RecordAccess_RecordProxy<NodeRecord, Void> change in _recordChangeSet.NodeRecords.changes() )
					{
						 NodeRecord record = Prepared( change, _nodeStore );
						 _integrityValidator.validateNodeRecord( record );
						 nodeCommands[i++] = new Command.NodeCommand( change.Before, record );
					}
					Arrays.sort( nodeCommands, _commandComparator );
			  }

			  Command[] relCommands = _emptyCommands;
			  if ( _recordChangeSet.RelRecords.changeSize() > 0 )
			  {
					relCommands = new Command[_recordChangeSet.RelRecords.changeSize()];
					int i = 0;
					foreach ( RecordAccess_RecordProxy<RelationshipRecord, Void> change in _recordChangeSet.RelRecords.changes() )
					{
						 relCommands[i++] = new Command.RelationshipCommand( change.Before, Prepared( change, _relationshipStore ) );
					}
					Arrays.sort( relCommands, _commandComparator );
			  }

			  Command[] propCommands = _emptyCommands;
			  if ( _recordChangeSet.PropertyRecords.changeSize() > 0 )
			  {
					propCommands = new Command[_recordChangeSet.PropertyRecords.changeSize()];
					int i = 0;
					foreach ( RecordAccess_RecordProxy<PropertyRecord, PrimitiveRecord> change in _recordChangeSet.PropertyRecords.changes() )
					{
						 propCommands[i++] = new Command.PropertyCommand( change.Before, Prepared( change, _propertyStore ) );
					}
					Arrays.sort( propCommands, _commandComparator );
			  }

			  Command[] relGroupCommands = _emptyCommands;
			  if ( _recordChangeSet.RelGroupRecords.changeSize() > 0 )
			  {
					relGroupCommands = new Command[_recordChangeSet.RelGroupRecords.changeSize()];
					int i = 0;
					foreach ( RecordAccess_RecordProxy<RelationshipGroupRecord, int> change in _recordChangeSet.RelGroupRecords.changes() )
					{
						 if ( change.Created && !change.ForReadingLinkage().inUse() )
						 {
							  /*
							   * This is an edge case that may come up and which we must handle properly. Relationship groups are
							   * not managed by the tx state, since they are created as side effects rather than through
							   * direct calls. However, they differ from say, dynamic records, in that their management can happen
							   * through separate code paths. What we are interested in here is the following scenario.
							   * 0. A node has one less relationship that is required to transition to dense node. The relationships
							   *    it has belong to at least two different types
							   * 1. In the same tx, a relationship is added making the node dense and all the relationships of a type
							   *    are removed from that node. Regardless of the order these operations happen, the creation of the
							   *    relationship (and the transition of the node to dense) will happen first.
							   * 2. A relationship group will be created because of the transition to dense and then deleted because
							   *    all the relationships it would hold are no longer there. This results in a relationship group
							   *    command that appears in the tx as not in use. Depending on the final order of operations, this
							   *    can end up using an id that is higher than the highest id seen so far. This may not be a problem
							   *    for a single instance, but it can result in errors in cases where transactions are applied
							   *    externally, such as backup or HA.
							   *
							   * The way we deal with this issue here is by not issuing a command for that offending record. This is
							   * safe, since the record is not in use and never was, so the high id is not necessary to change and
							   * the store remains consistent.
							   */
							  skippedCommands++;
							  continue;
						 }
						 relGroupCommands[i++] = new Command.RelationshipGroupCommand( change.Before, Prepared( change, _relationshipGroupStore ) );
					}
					relGroupCommands = i < relGroupCommands.Length ? Arrays.copyOf( relGroupCommands, i ) : relGroupCommands;
					Arrays.sort( relGroupCommands, _commandComparator );
			  }

			  AddFiltered( commands, Command.Mode.CREATE, propCommands, relCommands, relGroupCommands, nodeCommands );
			  AddFiltered( commands, Command.Mode.UPDATE, propCommands, relCommands, relGroupCommands, nodeCommands );
			  AddFiltered( commands, Command.Mode.DELETE, propCommands, relCommands, relGroupCommands, nodeCommands );

			  if ( _neoStoreRecord != null )
			  {
					foreach ( RecordAccess_RecordProxy<NeoStoreRecord, Void> change in _neoStoreRecord.changes() )
					{
						 commands.Add( new Command.NeoStoreCommand( change.Before, change.ForReadingData() ) );
					}
			  }
			  //noinspection unchecked
			  IList<Command>[] schemaChangeByMode = new System.Collections.IList[Command.Mode.values().length];
			  for ( int i = 0; i < schemaChangeByMode.Length; i++ )
			  {
					schemaChangeByMode[i] = new List<Command>();
			  }
			  foreach ( RecordAccess_RecordProxy<SchemaRecord, SchemaRule> change in _recordChangeSet.SchemaRuleChanges.changes() )
			  {
					if ( change.ForReadingLinkage().inUse() )
					{
						 _integrityValidator.validateSchemaRule( change.AdditionalData );
					}
					Command.SchemaRuleCommand cmd = new Command.SchemaRuleCommand( change.Before, change.ForChangingData(), change.AdditionalData );
					schemaChangeByMode[cmd.Mode.ordinal()].Add(cmd);
			  }
			  commands.addAll( schemaChangeByMode[Command.Mode.DELETE.ordinal()] );
			  commands.addAll( schemaChangeByMode[Command.Mode.CREATE.ordinal()] );
			  commands.addAll( schemaChangeByMode[Command.Mode.UPDATE.ordinal()] );
			  Debug.Assert( commands.Count == noOfCommands - skippedCommands, format( "Expected %d final commands, got %d " + "instead, with %d skipped", noOfCommands, commands.Count, skippedCommands ) );

			  _prepared = true;
		 }

		 private RECORD Prepared<RECORD, T1>( RecordAccess_RecordProxy<T1> proxy, RecordStore<RECORD> store ) where RECORD : Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord
		 {
			  RECORD after = proxy.ForReadingLinkage();
			  store.PrepareForCommit( after );
			  return after;
		 }

		 internal virtual void RelCreate( long id, int typeId, long startNodeId, long endNodeId )
		 {
			  _relationshipCreator.relationshipCreate( id, typeId, startNodeId, endNodeId, _recordChangeSet, _locks );
		 }

		 internal virtual void RelDelete( long relId )
		 {
			  _relationshipDeleter.relDelete( relId, _recordChangeSet, _locks );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs private final void addFiltered(java.util.Collection<org.neo4j.storageengine.api.StorageCommand> target, org.neo4j.kernel.impl.transaction.command.Command.Mode mode, org.neo4j.kernel.impl.transaction.command.Command[]... commands)
		 private void AddFiltered( ICollection<StorageCommand> target, Command.Mode mode, params Command[][] commands )
		 {
			  foreach ( Command[] c in commands )
			  {
					foreach ( Command command in c )
					{
						 if ( command.GetMode() == mode )
						 {
							  target.Add( command );
						 }
					}
			  }
		 }

		 /// <summary>
		 /// Deletes a node by its id, returning its properties which are now removed.
		 /// </summary>
		 /// <param name="nodeId"> The id of the node to delete. </param>
		 public virtual void NodeDelete( long nodeId )
		 {
			  NodeRecord nodeRecord = _recordChangeSet.NodeRecords.getOrLoad( nodeId, null ).forChangingData();
			  if ( !nodeRecord.InUse() )
			  {
					throw new System.InvalidOperationException( "Unable to delete Node[" + nodeId + "] since it has already been deleted." );
			  }
			  nodeRecord.InUse = false;
			  nodeRecord.SetLabelField( Record.NO_LABELS_FIELD.intValue(), MarkNotInUse(nodeRecord.DynamicLabelRecords) );
			  GetAndDeletePropertyChain( nodeRecord );
		 }

		 private ICollection<DynamicRecord> MarkNotInUse( ICollection<DynamicRecord> dynamicLabelRecords )
		 {
			  foreach ( DynamicRecord record in dynamicLabelRecords )
			  {
					record.InUse = false;
			  }
			  return dynamicLabelRecords;
		 }

		 private void GetAndDeletePropertyChain( NodeRecord nodeRecord )
		 {
			  _propertyDeleter.deletePropertyChain( nodeRecord, _recordChangeSet.PropertyRecords );
		 }

		 /// <summary>
		 /// Removes the given property identified by its index from the relationship
		 /// with the given id.
		 /// </summary>
		 /// <param name="relId"> The id of the relationship that is to have the property
		 ///            removed. </param>
		 /// <param name="propertyKey"> The index key of the property. </param>
		 internal virtual void RelRemoveProperty( long relId, int propertyKey )
		 {
			  RecordAccess_RecordProxy<RelationshipRecord, Void> rel = _recordChangeSet.RelRecords.getOrLoad( relId, null );
			  _propertyDeleter.removeProperty( rel, propertyKey, _recordChangeSet.PropertyRecords );
		 }

		 /// <summary>
		 /// Removes the given property identified by indexKeyId of the node with the
		 /// given id.
		 /// </summary>
		 /// <param name="nodeId"> The id of the node that is to have the property removed. </param>
		 /// <param name="propertyKey"> The index key of the property. </param>
		 public virtual void NodeRemoveProperty( long nodeId, int propertyKey )
		 {
			  RecordAccess_RecordProxy<NodeRecord, Void> node = _recordChangeSet.NodeRecords.getOrLoad( nodeId, null );
			  _propertyDeleter.removeProperty( node, propertyKey, _recordChangeSet.PropertyRecords );
		 }

		 /// <summary>
		 /// Changes an existing property's value of the given relationship, with the
		 /// given index to the passed value </summary>
		 ///  <param name="relId"> The id of the relationship which holds the property to
		 ///            change. </param>
		 /// <param name="propertyKey"> The index of the key of the property to change. </param>
		 /// <param name="value"> The new value of the property. </param>
		 internal virtual void RelChangeProperty( long relId, int propertyKey, Value value )
		 {
			  RecordAccess_RecordProxy<RelationshipRecord, Void> rel = _recordChangeSet.RelRecords.getOrLoad( relId, null );
			  _propertyCreator.primitiveSetProperty( rel, propertyKey, value, _recordChangeSet.PropertyRecords );
		 }

		 /// <summary>
		 /// Changes an existing property of the given node, with the given index to
		 /// the passed value </summary>
		 ///  <param name="nodeId"> The id of the node which holds the property to change. </param>
		 /// <param name="propertyKey"> The index of the key of the property to change. </param>
		 /// <param name="value"> The new value of the property. </param>
		 internal virtual void NodeChangeProperty( long nodeId, int propertyKey, Value value )
		 {
			  RecordAccess_RecordProxy<NodeRecord, Void> node = _recordChangeSet.NodeRecords.getOrLoad( nodeId, null );
			  _propertyCreator.primitiveSetProperty( node, propertyKey, value, _recordChangeSet.PropertyRecords );
		 }

		 /// <summary>
		 /// Adds a property to the given relationship, with the given index and
		 /// value. </summary>
		 ///  <param name="relId"> The id of the relationship to which to add the property. </param>
		 /// <param name="propertyKey"> The index of the key of the property to add. </param>
		 /// <param name="value"> The value of the property. </param>
		 internal virtual void RelAddProperty( long relId, int propertyKey, Value value )
		 {
			  RecordAccess_RecordProxy<RelationshipRecord, Void> rel = _recordChangeSet.RelRecords.getOrLoad( relId, null );
			  _propertyCreator.primitiveSetProperty( rel, propertyKey, value, _recordChangeSet.PropertyRecords );
		 }

		 /// <summary>
		 /// Adds a property to the given node, with the given index and value. </summary>
		 ///  <param name="nodeId"> The id of the node to which to add the property. </param>
		 /// <param name="propertyKey"> The index of the key of the property to add. </param>
		 /// <param name="value"> The value of the property. </param>
		 internal virtual void NodeAddProperty( long nodeId, int propertyKey, Value value )
		 {
			  RecordAccess_RecordProxy<NodeRecord, Void> node = _recordChangeSet.NodeRecords.getOrLoad( nodeId, null );
			  _propertyCreator.primitiveSetProperty( node, propertyKey, value, _recordChangeSet.PropertyRecords );
		 }

		 /// <summary>
		 /// Creates a node for the given id
		 /// </summary>
		 /// <param name="nodeId"> The id of the node to create. </param>
		 public virtual void NodeCreate( long nodeId )
		 {
			  NodeRecord nodeRecord = _recordChangeSet.NodeRecords.create( nodeId, null ).forChangingData();
			  nodeRecord.InUse = true;
			  nodeRecord.SetCreated();
		 }

		 /// <summary>
		 /// Creates a property index entry out of the given id and string.
		 /// </summary>
		 /// <param name="key"> The key of the property index, as a string. </param>
		 /// <param name="id"> The property index record id. </param>
		 internal virtual void CreatePropertyKeyToken( string key, long id )
		 {
			  TokenCreator<PropertyKeyTokenRecord> creator = new TokenCreator<PropertyKeyTokenRecord>( _neoStores.PropertyKeyTokenStore );
			  creator.CreateToken( key, id, _recordChangeSet.PropertyKeyTokenChanges );
		 }

		 /// <summary>
		 /// Creates a property index entry out of the given id and string.
		 /// </summary>
		 /// <param name="name"> The key of the property index, as a string. </param>
		 /// <param name="id"> The property index record id. </param>
		 internal virtual void CreateLabelToken( string name, long id )
		 {
			  TokenCreator<LabelTokenRecord> creator = new TokenCreator<LabelTokenRecord>( _neoStores.LabelTokenStore );
			  creator.CreateToken( name, id, _recordChangeSet.LabelTokenChanges );
		 }

		 /// <summary>
		 /// Creates a new RelationshipType record with the given id that has the
		 /// given name.
		 /// </summary>
		 /// <param name="name"> The name of the relationship type. </param>
		 /// <param name="id"> The id of the new relationship type record. </param>
		 internal virtual void CreateRelationshipTypeToken( string name, long id )
		 {
			  TokenCreator<RelationshipTypeTokenRecord> creator = new TokenCreator<RelationshipTypeTokenRecord>( _neoStores.RelationshipTypeTokenStore );
			  creator.CreateToken( name, id, _recordChangeSet.RelationshipTypeTokenChanges );
		 }

		 private class CommandComparator : IComparer<Command>
		 {
			  public override int Compare( Command o1, Command o2 )
			  {
					long id1 = o1.Key;
					long id2 = o2.Key;
					return Long.compare( id1, id2 );
			  }
		 }

		 private RecordAccess_RecordProxy<NeoStoreRecord, Void> OrLoadNeoStoreRecord
		 {
			 get
			 {
				  // TODO Move this neo store record thingie into RecordAccessSet
				  if ( _neoStoreRecord == null )
				  {
						_neoStoreRecord = new RecordChanges<>(new LoaderAnonymousInnerClass(this)
					  , new IntCounter());
				  }
				  return _neoStoreRecord.getOrLoad( 0L, null );
			 }
		 }

		 private class LoaderAnonymousInnerClass : RecordChanges.Loader<NeoStoreRecord, Void>
		 {
			 private readonly TransactionRecordState _outerInstance;

			 public LoaderAnonymousInnerClass( TransactionRecordState outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override NeoStoreRecord newUnused( long key, Void additionalData )
			 {
				  throw new System.NotSupportedException();
			 }

			 public override NeoStoreRecord load( long key, Void additionalData )
			 {
				  return _outerInstance.metaDataStore.graphPropertyRecord();
			 }

			 public override void ensureHeavy( NeoStoreRecord record )
			 {
			 }

			 public override NeoStoreRecord clone( NeoStoreRecord neoStoreRecord )
			 {
				  return neoStoreRecord.Clone();
			 }
		 }

		 /// <summary>
		 /// Adds a property to the graph, with the given index and value. </summary>
		 ///  <param name="propertyKey"> The index of the key of the property to add. </param>
		 /// <param name="value"> The value of the property. </param>
		 internal virtual void GraphAddProperty( int propertyKey, Value value )
		 {
			  _propertyCreator.primitiveSetProperty( OrLoadNeoStoreRecord, propertyKey, value, _recordChangeSet.PropertyRecords );
		 }

		 /// <summary>
		 /// Changes an existing property of the graph, with the given index to
		 /// the passed value
		 /// </summary>
		 /// <param name="propertyKey"> The index of the key of the property to change. </param>
		 /// <param name="value"> The new value of the property. </param>
		 internal virtual void GraphChangeProperty( int propertyKey, Value value )
		 {
			  _propertyCreator.primitiveSetProperty( OrLoadNeoStoreRecord, propertyKey, value, _recordChangeSet.PropertyRecords );
		 }

		 /// <summary>
		 /// Removes the given property identified by indexKeyId of the graph with the
		 /// given id.
		 /// </summary>
		 /// <param name="propertyKey"> The index key of the property. </param>
		 internal virtual void GraphRemoveProperty( int propertyKey )
		 {
			  RecordAccess_RecordProxy<NeoStoreRecord, Void> recordChange = OrLoadNeoStoreRecord;
			  _propertyDeleter.removeProperty( recordChange, propertyKey, _recordChangeSet.PropertyRecords );
		 }

		 internal virtual void CreateSchemaRule( SchemaRule schemaRule )
		 {
			  foreach ( DynamicRecord change in _recordChangeSet.SchemaRuleChanges.create( schemaRule.Id, schemaRule ).forChangingData() )
			  {
					change.InUse = true;
					change.SetCreated();
			  }
		 }

		 internal virtual void DropSchemaRule( SchemaRule rule )
		 {
			  RecordAccess_RecordProxy<SchemaRecord, SchemaRule> change = _recordChangeSet.SchemaRuleChanges.getOrLoad( rule.Id, rule );
			  SchemaRecord records = change.ForChangingData();
			  foreach ( DynamicRecord record in records )
			  {
					record.InUse = false;
			  }
			  records.InUse = false;
		 }

		 private void ChangeSchemaRule( SchemaRule rule, SchemaRule updatedRule )
		 {
			  //Read the current record
			  RecordAccess_RecordProxy<SchemaRecord, SchemaRule> change = _recordChangeSet.SchemaRuleChanges.getOrLoad( rule.Id, rule );
			  SchemaRecord records = change.ForReadingData();

			  //Register the change of the record
			  RecordAccess_RecordProxy<SchemaRecord, SchemaRule> recordChange = _recordChangeSet.SchemaRuleChanges.setRecord( rule.Id, records, updatedRule );
			  SchemaRecord dynamicRecords = recordChange.ForChangingData();

			  //Update the record
			  dynamicRecords.DynamicRecords = _schemaStore.allocateFrom( updatedRule );
		 }

		 internal virtual void AddLabelToNode( long labelId, long nodeId )
		 {
			  NodeRecord nodeRecord = _recordChangeSet.NodeRecords.getOrLoad( nodeId, null ).forChangingData();
			  parseLabelsField( nodeRecord ).add( labelId, _nodeStore, _nodeStore.DynamicLabelStore );
		 }

		 internal virtual void RemoveLabelFromNode( long labelId, long nodeId )
		 {
			  NodeRecord nodeRecord = _recordChangeSet.NodeRecords.getOrLoad( nodeId, null ).forChangingData();
			  parseLabelsField( nodeRecord ).remove( labelId, _nodeStore );
		 }

		 internal virtual void SetConstraintIndexOwner( StoreIndexDescriptor storeIndex, long constraintId )
		 {
			  StoreIndexDescriptor updatedStoreIndex = storeIndex.WithOwningConstraint( constraintId );
			  ChangeSchemaRule( storeIndex, updatedStoreIndex );
		 }

		 public interface PropertyReceiver<P> where P : Neo4Net.Storageengine.Api.StorageProperty
		 {
			  void Receive( P property, long propertyRecordId );
		 }
	}

}