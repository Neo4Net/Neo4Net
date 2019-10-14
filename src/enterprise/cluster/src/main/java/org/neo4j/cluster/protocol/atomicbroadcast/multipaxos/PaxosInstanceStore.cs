using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
	/// Store of Paxos instances, from a proposer perspective
	/// </summary>
	public class PaxosInstanceStore
	{
		 /*
		  * This represents the number of delivered paxos instances to keep in memory - it is essentially the length
		  * of the delivered queue and therefore the size of the instances map.
		  * The use of this is to server Learn requests. While learning is the final phase of the Paxos algo, it is also
		  * executed when an instance needs to catch up with events that happened in the cluster but it missed because it
		  * was temporarily disconnected.
		  * MAX_STORED therefore represents the maximum number of paxos instances a lagging member may be lagging behind
		  * and be able to recover from. This number must be large enough to account for a few minutes of downtime (like
		  * an extra long GC pause) during which intense broadcasting happens.
		  * Assuming 2 paxos instances per second gives us 120 instances per minute or 1200 instances for 10 minutes of
		  * downtime. We about 5x that here since instances are relatively small in size and we can spare the memory.
		  */
		 // TODO (quite challenging and interesting) Prune this queue aggressively.
		 /*
		  * This queue, as it stands now, will always remain at full capacity. However, if we could figure out that
		  * all cluster members have learned a particular paxos instance then we can remove it since no one will ever
		  * request it. That way the MAX_STORED value should be reached only when an instance is know to be in the failed
		  * state.
		  */

		 private const int MAX_STORED = 5000;

		 private int _queued;
		 private LinkedList<InstanceId> _delivered = new LinkedList<InstanceId>();
		 private IDictionary<InstanceId, PaxosInstance> _instances = new Dictionary<InstanceId, PaxosInstance>();
		 private readonly int _maxInstancesToStore;

		 public PaxosInstanceStore() : this(MAX_STORED)
		 {
		 }

		 public PaxosInstanceStore( int maxInstancesToStore )
		 {
			  this._maxInstancesToStore = maxInstancesToStore;
		 }

		 public virtual PaxosInstance GetPaxosInstance( InstanceId instanceId )
		 {
			  if ( instanceId == null )
			  {
					throw new System.NullReferenceException( "InstanceId may not be null" );
			  }

			  return _instances.computeIfAbsent( instanceId, i => new PaxosInstance( this, i ) );
		 }

		 public virtual void Delivered( InstanceId instanceId )
		 {
			  _queued++;
			  _delivered.AddLast( instanceId );

			  if ( _queued > _maxInstancesToStore )
			  {
					InstanceId removeInstanceId = _delivered.RemoveFirst();
					_instances.Remove( removeInstanceId );
					_queued--;
			  }
		 }

		 public virtual void Leave()
		 {
			  _queued = 0;
			  _delivered.Clear();
			  _instances.Clear();
		 }

		 public virtual PaxosInstanceStore Snapshot()
		 {
			  PaxosInstanceStore snapshotStore = new PaxosInstanceStore();
			  snapshotStore._queued = _queued;
			  snapshotStore._delivered = new LinkedList<InstanceId>( _delivered );
			  foreach ( KeyValuePair<InstanceId, PaxosInstance> instance in _instances.SetOfKeyValuePairs() )
			  {
					snapshotStore._instances[instance.Key] = instance.Value.snapshot( snapshotStore );
			  }
			  return snapshotStore;
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

			  PaxosInstanceStore that = ( PaxosInstanceStore ) o;

			  if ( _queued != that._queued )
			  {
					return false;
			  }
//JAVA TO C# CONVERTER WARNING: LINQ 'SequenceEqual' is not always identical to Java AbstractList 'equals':
//ORIGINAL LINE: if (!delivered.equals(that.delivered))
			  if ( !_delivered.SequenceEqual( that._delivered ) )
			  {
					return false;
			  }
			  return _instances.Equals( that._instances );
		 }

		 public override int GetHashCode()
		 {
			  int result = _queued;
			  result = 31 * result + _delivered.GetHashCode();
			  result = 31 * result + _instances.GetHashCode();
			  return result;
		 }
	}

}