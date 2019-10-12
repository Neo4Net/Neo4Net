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
namespace Org.Neo4j.Kernel.impl.transaction.command
{

	using BatchTransactionApplier = Org.Neo4j.Kernel.Impl.Api.BatchTransactionApplier;
	using TransactionApplier = Org.Neo4j.Kernel.Impl.Api.TransactionApplier;
	using CacheAccessBackDoor = Org.Neo4j.Kernel.impl.core.CacheAccessBackDoor;
	using NeoStores = Org.Neo4j.Kernel.impl.store.NeoStores;
	using CommandsToApply = Org.Neo4j.Storageengine.Api.CommandsToApply;

	public class CacheInvalidationBatchTransactionApplier : Org.Neo4j.Kernel.Impl.Api.BatchTransactionApplier_Adapter
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