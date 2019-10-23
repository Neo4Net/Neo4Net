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
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using CommandsToApply = Neo4Net.Kernel.Api.StorageEngine.CommandsToApply;

	public class CacheInvalidationBatchTransactionApplier : Neo4Net.Kernel.Impl.Api.BatchTransactionApplier_Adapter
	{
		 private readonly NeoStores _neoStores;
		 private readonly CacheAccessBackDoor _cacheAccess;

		 public CacheInvalidationBatchTransactionApplier( NeoStores neoStores, CacheAccessBackDoor cacheAccess )
		 {
			  this._neoStores = neoStores;
			  this._cacheAccess = cacheAccess;
		 }

		 public override TransactionApplier StartTx( CommandsToApply transaction )
		 {
			  return new CacheInvalidationTransactionApplier( _neoStores, _cacheAccess );
		 }
	}

}