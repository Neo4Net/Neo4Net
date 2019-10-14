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
	using Test = org.junit.jupiter.api.Test;

	using KernelTransactionMonitor = Neo4Net.Kernel.Impl.Api.transaciton.monitor.KernelTransactionMonitor;
	using KernelTransactionMonitorScheduler = Neo4Net.Kernel.Impl.Api.transaciton.monitor.KernelTransactionMonitorScheduler;
	using Group = Neo4Net.Scheduler.Group;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyZeroInteractions;

	internal class KernelTransactionMonitorSchedulerTest
	{
		 private readonly JobScheduler _scheduler = mock( typeof( JobScheduler ) );
		 private readonly KernelTransactionMonitor _transactionTimeoutMonitor = mock( typeof( KernelTransactionMonitor ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void scheduleRecurringMonitorJobIfConfigured()
		 internal virtual void ScheduleRecurringMonitorJobIfConfigured()
		 {
			  KernelTransactionMonitorScheduler transactionMonitorScheduler = CreateMonitorScheduler( 1 );
			  transactionMonitorScheduler.Start();

			  verify( _scheduler ).scheduleRecurring( Group.TRANSACTION_TIMEOUT_MONITOR, _transactionTimeoutMonitor, 1, TimeUnit.MILLISECONDS );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void doNotScheduleMonitorJobIfDisabled()
		 internal virtual void DoNotScheduleMonitorJobIfDisabled()
		 {
			  KernelTransactionMonitorScheduler transactionMonitorScheduler = CreateMonitorScheduler( 0 );
			  transactionMonitorScheduler.Start();

			  verifyZeroInteractions( _scheduler );
		 }

		 private KernelTransactionMonitorScheduler CreateMonitorScheduler( long checkInterval )
		 {
			  return new KernelTransactionMonitorScheduler( _transactionTimeoutMonitor, _scheduler, checkInterval );
		 }
	}

}