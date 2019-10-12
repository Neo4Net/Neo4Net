using System.Collections.Concurrent;
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
namespace Neo4Net.Kernel.impl.scheduler
{

	using Exceptions = Neo4Net.Helpers.Exceptions;
	using Group = Neo4Net.Scheduler.Group;
	using JobHandle = Neo4Net.Scheduler.JobHandle;

	internal sealed class ThreadPoolManager
	{
		 private readonly ConcurrentDictionary<Group, ThreadPool> _pools;
		 private readonly System.Func<Group, ThreadPool> _poolBuilder;

		 internal ThreadPoolManager( ThreadGroup topLevelGroup )
		 {
			  _pools = new ConcurrentDictionary<Group, ThreadPool>();
			  _poolBuilder = group => new ThreadPool( group, topLevelGroup );
		 }

		 internal ThreadPool GetThreadPool( Group group )
		 {
			  return _pools.computeIfAbsent( group, _poolBuilder );
		 }

		 internal JobHandle Submit( Group group, ThreadStart job )
		 {
			  ThreadPool threadPool = GetThreadPool( group );
			  return threadPool.Submit( job );
		 }

		 public InterruptedException ShutDownAll()
		 {
			  _pools.forEach( ( group, pool ) => pool.cancelAllJobs() );
			  _pools.forEach( ( group, pool ) => pool.shutDown() );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return _pools.Values.Select( ThreadPool::getShutdownException ).Aggregate( null, Exceptions.chain );
		 }
	}

}