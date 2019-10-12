using System.Collections.Concurrent;

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
namespace Neo4Net.Udc
{

	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using Group = Neo4Net.Scheduler.Group;
	using JobHandle = Neo4Net.Scheduler.JobHandle;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;

	/// <summary>
	/// An in-memory storage location for usage metadata.
	/// Any component is allowed to publish it's usage date here, and it can be any object,
	/// including mutable classes. It is up to the usage data publishing code to choose which items from this repository
	/// to publish.
	/// 
	/// This service is meant as a diagnostic and informational tool, notably used by UDC.
	/// </summary>
	public class UsageData : LifecycleAdapter
	{
		 private readonly ConcurrentDictionary<UsageDataKey, object> _store = new ConcurrentDictionary<UsageDataKey, object>();
		 private readonly JobScheduler _scheduler;
		 private JobHandle _featureDecayJob;

		 public UsageData( JobScheduler scheduler )
		 {
			  this._scheduler = scheduler;
		 }

		 public virtual void Set<T>( UsageDataKey<T> key, T value )
		 {
			  _store[key] = value;
		 }

		 public virtual T Get<T>( UsageDataKey<T> key )
		 {
			  object o = _store[key];
			  if ( o == null )
			  {
					// When items are missing, if there is a default value, we do a get-or-create style operation
					// This allows outside actors to get-or-create rich objects and know they will get the same object out
					// that other threads would use, which is helpful when we store mutable objects
					T value = key.GenerateDefaultValue();
					if ( value == default( T ) )
					{
						 return default( T );
					}

					_store.GetOrAdd( key, value );
					return Get( key );
			  }
			  return ( T ) o;
		 }

		 public override void Stop()
		 {
			  if ( _featureDecayJob != null )
			  {
					_featureDecayJob.cancel( false );
			  }
		 }

		 public override void Start()
		 {
			  _featureDecayJob = _scheduler.schedule( Group.UDC, Get( UsageDataKeys.Features ).sweep, 1, DAYS );
		 }
	}

}