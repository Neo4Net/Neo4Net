using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.Kernel.Impl.Api.transaciton.monitor
{

	using KernelTransactionHandle = Neo4Net.Kernel.Api.KernelTransactionHandle;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using Log = Neo4Net.Logging.Log;
	using LogService = Neo4Net.Logging.Internal.LogService;
	using SystemNanoClock = Neo4Net.Time.SystemNanoClock;

	/// <summary>
	/// Transaction monitor that check transactions with a configured timeout for expiration.
	/// In case if transaction timed out it will be terminated.
	/// </summary>
	public class KernelTransactionMonitor : ThreadStart
	{
		 private readonly KernelTransactions _kernelTransactions;
		 private readonly SystemNanoClock _clock;
		 private readonly Log _log;

		 public KernelTransactionMonitor( KernelTransactions kernelTransactions, SystemNanoClock clock, LogService logService )
		 {
			  this._kernelTransactions = kernelTransactions;
			  this._clock = clock;
			  this._log = logService.GetInternalLog( typeof( KernelTransactionMonitor ) );
		 }

		 public override void Run()
		 {
			 lock ( this )
			 {
				  long nowNanos = _clock.nanos();
				  ISet<KernelTransactionHandle> activeTransactions = _kernelTransactions.activeTransactions();
				  CheckExpiredTransactions( activeTransactions, nowNanos );
			 }
		 }

		 private void CheckExpiredTransactions( ISet<KernelTransactionHandle> activeTransactions, long nowNanos )
		 {
			  foreach ( KernelTransactionHandle activeTransaction in activeTransactions )
			  {
					long transactionTimeoutMillis = activeTransaction.TimeoutMillis();
					if ( transactionTimeoutMillis > 0 )
					{
						 if ( IsTransactionExpired( activeTransaction, nowNanos, transactionTimeoutMillis ) && !activeTransaction.SchemaTransaction )
						 {
							  if ( activeTransaction.MarkForTermination( Neo4Net.Kernel.Api.Exceptions.Status_Transaction.TransactionTimedOut ) )
							  {
									_log.warn( "Transaction %s timeout.", activeTransaction );
							  }
						 }
					}
			  }
		 }

		 private static bool IsTransactionExpired( KernelTransactionHandle activeTransaction, long nowNanos, long transactionTimeoutMillis )
		 {
			  return nowNanos - activeTransaction.StartTimeNanos() > TimeUnit.MILLISECONDS.toNanos(transactionTimeoutMillis);
		 }
	}

}