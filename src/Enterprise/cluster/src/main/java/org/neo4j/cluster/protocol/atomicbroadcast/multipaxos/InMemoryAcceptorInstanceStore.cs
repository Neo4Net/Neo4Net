using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.cluster.protocol.atomicbroadcast.multipaxos
{

	/// <summary>
	/// In memory version of an acceptor instance store.
	/// </summary>
	public class InMemoryAcceptorInstanceStore : AcceptorInstanceStore
	{
		 private readonly IDictionary<InstanceId, AcceptorInstance> _instances;
		 private readonly BlockingQueue<InstanceId> _currentInstances;

		 private long _lastDeliveredInstanceId;

		 public InMemoryAcceptorInstanceStore() : this(new Dictionary<InstanceId, AcceptorInstance>(), new ArrayBlockingQueue<InstanceId>(1000), -1)
		 {
		 }

		 private InMemoryAcceptorInstanceStore( IDictionary<InstanceId, AcceptorInstance> instances, BlockingQueue<InstanceId> currentInstances, long lastDeliveredInstanceId )
		 {
			  this._instances = instances;
			  this._lastDeliveredInstanceId = lastDeliveredInstanceId;
			  this._currentInstances = currentInstances;
		 }

		 public override AcceptorInstance GetAcceptorInstance( InstanceId instanceId )
		 {
			  AcceptorInstance instance = _instances[instanceId];
			  if ( instance == null )
			  {
					instance = new AcceptorInstance();
					_instances[instanceId] = instance;

					// Make sure we only keep a maximum number of instances, to not run out of memory
					if ( !_currentInstances.offer( instanceId ) )
					{
						 _instances.Remove( _currentInstances.poll() );
						 _currentInstances.offer( instanceId );
					}
			  }

			  return instance;
		 }

		 public override void Promise( AcceptorInstance instance, long ballot )
		 {
			  instance.Promise( ballot );
		 }

		 public override void Accept( AcceptorInstance instance, object value )
		 {
			  instance.Accept( value );
		 }

		 public override void LastDelivered( InstanceId instanceId )
		 {
			  _lastDeliveredInstanceId = instanceId.Id;
		 }

		 public override void Clear()
		 {
			  _instances.Clear();
		 }

		 public virtual InMemoryAcceptorInstanceStore Snapshot()
		 {
			  return new InMemoryAcceptorInstanceStore( new Dictionary<>( _instances ), new ArrayBlockingQueue<>( _currentInstances.size() + _currentInstances.remainingCapacity(), false, _currentInstances ), _lastDeliveredInstanceId );
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }

			  InMemoryAcceptorInstanceStore that = ( InMemoryAcceptorInstanceStore ) o;

			  if ( _lastDeliveredInstanceId != that._lastDeliveredInstanceId )
			  {
					return false;
			  }
			  return _instances.Equals( that._instances );
		 }

		 public override int GetHashCode()
		 {
			  int result = _instances.GetHashCode();
			  result = 31 * result + ( int )( _lastDeliveredInstanceId ^ ( ( long )( ( ulong )_lastDeliveredInstanceId >> 32 ) ) );
			  return result;
		 }
	}

}