﻿/*
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
namespace Neo4Net.Kernel.impl.storageengine.impl.recordstorage.id
{

	using KernelTransactionsSnapshot = Neo4Net.Kernel.Impl.Api.KernelTransactionsSnapshot;
	using BufferingIdGeneratorFactory = Neo4Net.Kernel.impl.store.id.BufferingIdGeneratorFactory;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using Group = Neo4Net.Scheduler.Group;
	using JobHandle = Neo4Net.Scheduler.JobHandle;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;

	/// <summary>
	/// Storage id controller that provide buffering possibilities to be able so safely free and reuse ids.
	/// Allows perform clear and maintenance operations over currently buffered set of ids. </summary>
	/// <seealso cref= BufferingIdGeneratorFactory </seealso>
	public class BufferedIdController : LifecycleAdapter, IdController
	{
		 private readonly BufferingIdGeneratorFactory _bufferingIdGeneratorFactory;
		 private readonly JobScheduler _scheduler;
		 private JobHandle _jobHandle;

		 public BufferedIdController( BufferingIdGeneratorFactory bufferingIdGeneratorFactory, JobScheduler scheduler )
		 {
			  this._bufferingIdGeneratorFactory = bufferingIdGeneratorFactory;
			  this._scheduler = scheduler;
		 }

		 public override void Start()
		 {
			  _jobHandle = _scheduler.scheduleRecurring( Group.STORAGE_MAINTENANCE, this.maintenance, 1, TimeUnit.SECONDS );
		 }

		 public override void Stop()
		 {
			  _jobHandle.cancel( false );
		 }

		 public override void Clear()
		 {
			  _bufferingIdGeneratorFactory.clear();
		 }

		 public override void Maintenance()
		 {
			  _bufferingIdGeneratorFactory.maintenance();
		 }

		 public override void Initialize( System.Func<KernelTransactionsSnapshot> transactionsSnapshotSupplier )
		 {
			  _bufferingIdGeneratorFactory.initialize( transactionsSnapshotSupplier );
		 }
	}

}