using System.Collections.Concurrent;
using System.Collections.Generic;

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

	using CancelListener = Neo4Net.Scheduler.CancelListener;
	using JobHandle = Neo4Net.Scheduler.JobHandle;

	internal sealed class PooledJobHandle : JobHandle
	{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.concurrent.Future<?> future;
		 private readonly Future<object> _future;
		 private readonly object _registryKey;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.concurrent.ConcurrentHashMap<Object,java.util.concurrent.Future<?>> registry;
		 private readonly ConcurrentDictionary<object, Future<object>> _registry;
		 private readonly IList<CancelListener> _cancelListeners = new CopyOnWriteArrayList<CancelListener>();

		 internal PooledJobHandle<T1, T2>( Future<T1> future, object registryKey, ConcurrentDictionary<T2> registry )
		 {
			  this._future = future;
			  this._registryKey = registryKey;
			  this._registry = registry;
		 }

		 public override void Cancel( bool mayInterruptIfRunning )
		 {
			  _future.cancel( mayInterruptIfRunning );
			  foreach ( CancelListener cancelListener in _cancelListeners )
			  {
					cancelListener.Cancelled( mayInterruptIfRunning );
			  }
			  _registry.Remove( _registryKey );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void waitTermination() throws InterruptedException, java.util.concurrent.ExecutionException
		 public override void WaitTermination()
		 {
			  _future.get();
		 }

		 public override void RegisterCancelListener( CancelListener listener )
		 {
			  _cancelListeners.Add( listener );
		 }
	}

}