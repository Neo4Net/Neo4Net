﻿using System;
using System.Threading;

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
namespace Org.Neo4j.Scheduler
{

	using LifecycleAdapter = Org.Neo4j.Kernel.Lifecycle.LifecycleAdapter;

	public class JobSchedulerAdapter : LifecycleAdapter, JobScheduler
	{
		 public virtual string TopLevelGroupName
		 {
			 set
			 {
			 }
		 }

		 public override Executor Executor( Group group )
		 {
			  return null;
		 }

		 public override ThreadFactory ThreadFactory( Group group )
		 {
			  return null;
		 }

		 public override JobHandle Schedule( Group group, ThreadStart job )
		 {
			  return null;
		 }

		 public override JobHandle Schedule( Group group, ThreadStart runnable, long initialDelay, TimeUnit timeUnit )
		 {
			  return null;
		 }

		 public override JobHandle ScheduleRecurring( Group group, ThreadStart runnable, long period, TimeUnit timeUnit )
		 {
			  return null;
		 }

		 public override JobHandle ScheduleRecurring( Group group, ThreadStart runnable, long initialDelay, long period, TimeUnit timeUnit )
		 {
			  return null;
		 }

		 public override ExecutorService WorkStealingExecutor( Group group, int parallelism )
		 {
			  return null;
		 }

		 public override ExecutorService WorkStealingExecutorAsyncMode( Group group, int parallelism )
		 {
			  return null;
		 }

		 public override void Close()
		 {
			  try
			  {
					Shutdown();
			  }
			  catch ( Exception throwable )
			  {
					throw new Exception( throwable );
			  }
		 }
	}

}