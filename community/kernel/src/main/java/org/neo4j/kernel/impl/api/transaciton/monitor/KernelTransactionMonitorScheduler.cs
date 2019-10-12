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
namespace Org.Neo4j.Kernel.Impl.Api.transaciton.monitor
{

	using LifecycleAdapter = Org.Neo4j.Kernel.Lifecycle.LifecycleAdapter;
	using Group = Org.Neo4j.Scheduler.Group;
	using JobHandle = Org.Neo4j.Scheduler.JobHandle;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;

	public class KernelTransactionMonitorScheduler : LifecycleAdapter
	{
		 private readonly KernelTransactionMonitor _kernelTransactionMonitor;
		 private readonly JobScheduler _scheduler;
		 private readonly long _checkIntervalMillis;
		 private JobHandle _monitorJobHandle;

		 public KernelTransactionMonitorScheduler( KernelTransactionMonitor kernelTransactionMonitor, JobScheduler scheduler, long checkIntervalMillis )
		 {
			  this._kernelTransactionMonitor = kernelTransactionMonitor;
			  this._scheduler = scheduler;
			  this._checkIntervalMillis = checkIntervalMillis;
		 }

		 public override void Start()
		 {
			  if ( _checkIntervalMillis > 0 )
			  {
					_monitorJobHandle = _scheduler.scheduleRecurring( Group.TRANSACTION_TIMEOUT_MONITOR, _kernelTransactionMonitor, _checkIntervalMillis, TimeUnit.MILLISECONDS );
			  }
		 }

		 public override void Stop()
		 {
			  if ( _monitorJobHandle != null )
			  {
					_monitorJobHandle.cancel( true );
			  }
		 }
	}

}