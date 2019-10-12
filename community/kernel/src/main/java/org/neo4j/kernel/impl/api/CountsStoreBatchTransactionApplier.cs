using System.Diagnostics;

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
namespace Org.Neo4j.Kernel.Impl.Api
{

	using CountsTracker = Org.Neo4j.Kernel.impl.store.counts.CountsTracker;
	using CommandsToApply = Org.Neo4j.Storageengine.Api.CommandsToApply;
	using TransactionApplicationMode = Org.Neo4j.Storageengine.Api.TransactionApplicationMode;

	public class CountsStoreBatchTransactionApplier : BatchTransactionApplier_Adapter
	{
		 private readonly CountsTracker _countsTracker;
		 private CountsTracker.Updater _countsUpdater;
		 private readonly TransactionApplicationMode _mode;

		 public CountsStoreBatchTransactionApplier( CountsTracker countsTracker, TransactionApplicationMode mode )
		 {
			  this._countsTracker = countsTracker;
			  this._mode = mode;
		 }

		 public override TransactionApplier StartTx( CommandsToApply transaction )
		 {
			  Optional<CountsAccessor_Updater> result = _countsTracker.apply( transaction.TransactionId() );
			  result.ifPresent( updater => this._countsUpdater = updater );
			  Debug.Assert( this._countsUpdater != null || _mode == TransactionApplicationMode.RECOVERY );

			  return new CountsStoreTransactionApplier( _mode, _countsUpdater );
		 }
	}

}