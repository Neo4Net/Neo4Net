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
	using Mockito = org.mockito.Mockito;

	using KernelTransactionMonitor = Neo4Net.Kernel.Impl.Api.transaciton.monitor.KernelTransactionMonitor;
	using KernelTransactionMonitorScheduler = Neo4Net.Kernel.Impl.Api.transaciton.monitor.KernelTransactionMonitorScheduler;
	using Group = Neo4Net.Scheduler.Group;
	using JobHandle = Neo4Net.Scheduler.JobHandle;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	internal class KernelTransactionTimeoutMonitorSchedulerTest
	{

		 private readonly KernelTransactionMonitor _transactionMonitor = mock( typeof( KernelTransactionMonitor ) );
		 private readonly IJobScheduler _jobScheduler = mock( typeof( IJobScheduler ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void startJobTransactionMonitor()
		 internal virtual void StartJobTransactionMonitor()
		 {
			  JobHandle jobHandle = Mockito.mock( typeof( JobHandle ) );
			  when( _jobScheduler.scheduleRecurring( eq( Group.TRANSACTION_TIMEOUT_MONITOR ), eq( _transactionMonitor ), anyLong(), any(typeof(TimeUnit)) ) ).thenReturn(jobHandle);

			  KernelTransactionMonitorScheduler monitorScheduler = new KernelTransactionMonitorScheduler( _transactionMonitor, _jobScheduler, 7 );

			  monitorScheduler.Start();
			  verify( _jobScheduler ).scheduleRecurring( Group.TRANSACTION_TIMEOUT_MONITOR, _transactionMonitor, 7, TimeUnit.MILLISECONDS );

			  monitorScheduler.Stop();
			  verify( jobHandle ).cancel( true );
		 }
	}

}