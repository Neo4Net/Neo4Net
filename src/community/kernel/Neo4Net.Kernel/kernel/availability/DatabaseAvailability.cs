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
namespace Neo4Net.Kernel.availability
{

	using TransactionCounters = Neo4Net.Kernel.impl.transaction.stats.TransactionCounters;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;


	/// <summary>
	/// This class handles whether the database as a whole is available to use at all.
	/// As it runs as the last service in the lifecycle list, the stop() is called first
	/// on stop, shutdown or restart, and thus blocks access to everything else for outsiders.
	/// </summary>
	public class DatabaseAvailability : LifecycleAdapter
	{
		 private static readonly AvailabilityRequirement _availabilityRequirement = new DescriptiveAvailabilityRequirement( "Database available" );
		 private readonly AvailabilityGuard _databaseAvailabilityGuard;
		 private readonly TransactionCounters _transactionCounters;
		 private readonly Clock _clock;
		 private readonly long _awaitActiveTransactionDeadlineMillis;
		 private volatile bool _started;

		 public DatabaseAvailability( AvailabilityGuard databaseAvailabilityGuard, TransactionCounters transactionCounters, Clock clock, long awaitActiveTransactionDeadlineMillis )
		 {
			  this._databaseAvailabilityGuard = databaseAvailabilityGuard;
			  this._transactionCounters = transactionCounters;
			  this._awaitActiveTransactionDeadlineMillis = awaitActiveTransactionDeadlineMillis;
			  this._clock = clock;

			  // On initial setup, deny availability
			  databaseAvailabilityGuard.Require( _availabilityRequirement );
		 }

		 public override void Start()
		 {
			  _databaseAvailabilityGuard.fulfill( _availabilityRequirement );
			  _started = true;
		 }

		 public override void Stop()
		 {
			  _started = false;
			  // Database is no longer available for use
			  // Deny beginning new transactions
			  _databaseAvailabilityGuard.require( _availabilityRequirement );

			  // Await transactions stopped
			  AwaitTransactionsClosedWithinTimeout();
		 }

		 public virtual bool Started
		 {
			 get
			 {
				  return _started;
			 }
		 }

		 private void AwaitTransactionsClosedWithinTimeout()
		 {
			  long deadline = _clock.millis() + _awaitActiveTransactionDeadlineMillis;
			  while ( _transactionCounters.NumberOfActiveTransactions > 0 && _clock.millis() < deadline )
			  {
					parkNanos( MILLISECONDS.toNanos( 10 ) );
			  }
		 }
	}

}