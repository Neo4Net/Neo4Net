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
namespace Neo4Net.Kernel.impl.transaction.command
{
	using TransactionApplier = Neo4Net.Kernel.Impl.Api.TransactionApplier;
	using CacheAccessBackDoor = Neo4Net.Kernel.impl.core.CacheAccessBackDoor;
	using LockGroup = Neo4Net.Kernel.impl.locking.LockGroup;
	using LockService = Neo4Net.Kernel.impl.locking.LockService;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using Neo4Net.Kernel.impl.store;
	using SchemaStore = Neo4Net.Kernel.impl.store.SchemaStore;
	using AbstractBaseRecord = Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord;
	using ConstraintRule = Neo4Net.Kernel.Impl.Store.Records.ConstraintRule;
	using DynamicRecord = Neo4Net.Kernel.Impl.Store.Records.DynamicRecord;
	using Neo4Net.Kernel.impl.transaction.command.Command;
	using CommandVersion = Neo4Net.Kernel.Api.StorageEngine.CommandVersion;
	using SchemaRule = Neo4Net.Kernel.Api.StorageEngine.schema.SchemaRule;

	/// <summary>
	/// Visits commands targeted towards the <seealso cref="NeoStores"/> and update corresponding stores.
	/// What happens in here is what will happen in a "internal" transaction, i.e. a transaction that has been
	/// forged in this database, with transaction state, a KernelTransaction and all that and is now committing.
	/// <para>
	/// For other modes of application, like recovery or external there are other, added functionality, decorated
	/// outside this applier.
	/// </para>
	/// </summary>
	public class NeoStoreTransactionApplier : Neo4Net.Kernel.Impl.Api.TransactionApplier_Adapter
	{
		 private readonly CommandVersion _version;
		 private readonly LockGroup _lockGroup;
		 private readonly long _transactionId;
		 private readonly NeoStores _neoStores;
		 private readonly CacheAccessBackDoor _cacheAccess;
		 private readonly LockService _lockService;

		 public NeoStoreTransactionApplier( CommandVersion version, NeoStores neoStores, CacheAccessBackDoor cacheAccess, LockService lockService, long transactionId, LockGroup lockGroup )
		 {
			  this._version = version;
			  this._lockGroup = lockGroup;
			  this._transactionId = transactionId;
			  this._lockService = lockService;
			  this._neoStores = neoStores;
			  this._cacheAccess = cacheAccess;
		 }

		 public override bool VisitNodeCommand( Command.NodeCommand command )
		 {
			  // acquire lock
			  _lockGroup.add( _lockService.acquireNodeLock( command.Key, Neo4Net.Kernel.impl.locking.LockService_LockType.WriteLock ) );

			  // update store
			  UpdateStore( _neoStores.NodeStore, command );
			  return false;
		 }

		 public override bool VisitRelationshipCommand( Command.RelationshipCommand command )
		 {
			  _lockGroup.add( _lockService.acquireRelationshipLock( command.Key, Neo4Net.Kernel.impl.locking.LockService_LockType.WriteLock ) );

			  UpdateStore( _neoStores.RelationshipStore, command );
			  return false;
		 }

		 public override bool VisitPropertyCommand( Command.PropertyCommand command )
		 {
			  // acquire lock
			  if ( command.NodeId != -1 )
			  {
					_lockGroup.add( _lockService.acquireNodeLock( command.NodeId, Neo4Net.Kernel.impl.locking.LockService_LockType.WriteLock ) );
			  }
			  else if ( command.RelId != -1 )
			  {
					_lockGroup.add( _lockService.acquireRelationshipLock( command.RelId, Neo4Net.Kernel.impl.locking.LockService_LockType.WriteLock ) );
			  }

			  UpdateStore( _neoStores.PropertyStore, command );
			  return false;
		 }

		 public override bool VisitRelationshipGroupCommand( Command.RelationshipGroupCommand command )
		 {
			  UpdateStore( _neoStores.RelationshipGroupStore, command );
			  return false;
		 }

		 public override bool VisitRelationshipTypeTokenCommand( Command.RelationshipTypeTokenCommand command )
		 {
			  UpdateStore( _neoStores.RelationshipTypeTokenStore, command );
			  return false;
		 }

		 public override bool VisitLabelTokenCommand( Command.LabelTokenCommand command )
		 {
			  UpdateStore( _neoStores.LabelTokenStore, command );
			  return false;
		 }

		 public override bool VisitPropertyKeyTokenCommand( Command.PropertyKeyTokenCommand command )
		 {
			  UpdateStore( _neoStores.PropertyKeyTokenStore, command );
			  return false;
		 }

		 public override bool VisitSchemaRuleCommand( Command.SchemaRuleCommand command )
		 {
			  SchemaStore schemaStore = _neoStores.SchemaStore;
			  if ( _version == CommandVersion.BEFORE )
			  {
					// We are doing reverse-recovery. There is no need for updating the cache, since the indexing service be told what it needs to know when we do
					// forward-recovery later.
					bool create = command.Mode == Command.Mode.Create;
					foreach ( DynamicRecord record in command.RecordsBefore )
					{
						 if ( create )
						 {
							  // Schema create commands do not properly store their before images, so we need to correct them.
							  // That is, if the schema was created by this command, then obviously the before image of those records were not in use.
							  record.InUse = false;
						 }
						 schemaStore.UpdateRecord( record );
					}
					return false;
			  }

			  // schema rules. Execute these after generating the property updates so. If executed
			  // before and we've got a transaction that sets properties/labels as well as creating an index
			  // we might end up with this corner-case:
			  // 1) index rule created and index population job started
			  // 2) index population job processes some nodes, but doesn't complete
			  // 3) we gather up property updates and send those to the indexes. The newly created population
			  //    job might get those as updates
			  // 4) the population job will apply those updates as added properties, and might end up with duplicate
			  //    entries for the same property
			  foreach ( DynamicRecord record in command.RecordsAfter )
			  {
					schemaStore.UpdateRecord( record );
			  }

			  if ( command.SchemaRule is ConstraintRule )
			  {
					switch ( command.Mode )
					{
					case UPDATE:
					case CREATE:
						 _neoStores.MetaDataStore.LatestConstraintIntroducingTx = _transactionId;
						 break;
					case DELETE:
						 break;
					default:
						 throw new System.InvalidOperationException( command.Mode.name() );
					}
			  }

			  switch ( command.Mode )
			  {
			  case DELETE:
					_cacheAccess.removeSchemaRuleFromCache( command.Key );
					break;
			  default:
					_cacheAccess.addSchemaRule( command.SchemaRule );
				break;
			  }
			  return false;
		 }

		 public override bool VisitNeoStoreCommand( Command.NeoStoreCommand command )
		 {
			  _neoStores.MetaDataStore.GraphNextProp = SelectRecordByCommandVersion( command ).NextProp;
			  return false;
		 }

		 private void UpdateStore<RECORD>( RecordStore<RECORD> store, BaseCommand<RECORD> command ) where RECORD : Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord
		 {
			  store.UpdateRecord( SelectRecordByCommandVersion( command ) );
		 }

		 private RECORD SelectRecordByCommandVersion<RECORD>( BaseCommand<RECORD> command ) where RECORD : Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord
		 {
			  switch ( _version )
			  {
			  case CommandVersion.BEFORE:
					return command.Before;
			  case CommandVersion.AFTER:
					return command.After;
			  default:
					throw new System.ArgumentException( "Unexpected command version " + _version );
			  }
		 }
	}

}