using System;

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
	using BatchTransactionApplier = Neo4Net.Kernel.Impl.Api.BatchTransactionApplier;
	using TransactionApplier = Neo4Net.Kernel.Impl.Api.TransactionApplier;
	using CacheAccessBackDoor = Neo4Net.Kernel.impl.core.CacheAccessBackDoor;
	using LockGroup = Neo4Net.Kernel.impl.locking.LockGroup;
	using LockService = Neo4Net.Kernel.impl.locking.LockService;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using CommandVersion = Neo4Net.Kernel.Api.StorageEngine.CommandVersion;
	using CommandsToApply = Neo4Net.Kernel.Api.StorageEngine.CommandsToApply;

	/// <summary>
	/// Visits commands targeted towards the <seealso cref="NeoStores"/> and update corresponding stores. What happens in here is what
	/// will happen in a "internal" transaction, i.e. a transaction that has been forged in this database, with transaction
	/// state, a KernelTransaction and all that and is now committing. <para> For other modes of application, like recovery or
	/// external there are other, added functionality, decorated outside this applier.
	/// </para>
	/// </summary>
	public class NeoStoreBatchTransactionApplier : Neo4Net.Kernel.Impl.Api.BatchTransactionApplier_Adapter
	{
		 private readonly CommandVersion _version;
		 private readonly NeoStores _neoStores;
		 // Ideally we don't want any cache access in here, but it is how it is. At least we try to minimize use of it
		 private readonly CacheAccessBackDoor _cacheAccess;
		 private readonly LockService _lockService;

		 public NeoStoreBatchTransactionApplier( NeoStores store, CacheAccessBackDoor cacheAccess, LockService lockService ) : this( CommandVersion.AFTER, store, cacheAccess, lockService )
		 {
		 }

		 public NeoStoreBatchTransactionApplier( CommandVersion version, NeoStores store, CacheAccessBackDoor cacheAccess, LockService lockService )
		 {
			  this._version = version;
			  this._neoStores = store;
			  this._cacheAccess = cacheAccess;
			  this._lockService = lockService;
		 }

		 public override TransactionApplier StartTx( CommandsToApply transaction )
		 {
			  throw new Exception( "NeoStoreTransactionApplier requires a LockGroup" );
		 }

		 public override TransactionApplier StartTx( CommandsToApply transaction, LockGroup lockGroup )
		 {
			  return new NeoStoreTransactionApplier( _version, _neoStores, _cacheAccess, _lockService, transaction.TransactionId(), lockGroup );
		 }
	}

}