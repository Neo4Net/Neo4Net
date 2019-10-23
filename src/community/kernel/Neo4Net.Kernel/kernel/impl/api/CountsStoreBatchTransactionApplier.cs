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
namespace Neo4Net.Kernel.Impl.Api
{

	using CountsTracker = Neo4Net.Kernel.impl.store.counts.CountsTracker;
	using CommandsToApply = Neo4Net.Kernel.Api.StorageEngine.CommandsToApply;
	using TransactionApplicationMode = Neo4Net.Kernel.Api.StorageEngine.TransactionApplicationMode;

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