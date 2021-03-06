﻿/*
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
namespace Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.context
{

	using HeartbeatContext = Org.Neo4j.cluster.protocol.heartbeat.HeartbeatContext;
	using Timeouts = Org.Neo4j.cluster.timeout.Timeouts;
	using Quorums = Org.Neo4j.cluster.util.Quorums;
	using Org.Neo4j.Helpers;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

	/// <summary>
	/// Context for <seealso cref="AtomicBroadcastState"/> state machine.
	/// <p/>
	/// This holds the set of listeners for atomic broadcasts, and allows distribution of received values to those listeners.
	/// </summary>
	internal class AtomicBroadcastContextImpl : AbstractContextImpl, AtomicBroadcastContext
	{
		 private readonly Listeners<AtomicBroadcastListener> _listeners = new Listeners<AtomicBroadcastListener>();
		 private readonly Executor _executor;
		 private readonly HeartbeatContext _heartbeatContext;

		 internal AtomicBroadcastContextImpl( Org.Neo4j.cluster.InstanceId me, CommonContextState commonState, LogProvider logging, Timeouts timeouts, Executor executor, HeartbeatContext heartbeatContext ) : base( me, commonState, logging, timeouts )
		 {
			  this._executor = executor;
			  this._heartbeatContext = heartbeatContext;
		 }

		 public override void AddAtomicBroadcastListener( AtomicBroadcastListener listener )
		 {
			  _listeners.add( listener );
		 }

		 public override void RemoveAtomicBroadcastListener( AtomicBroadcastListener listener )
		 {
			  _listeners.remove( listener );
		 }

		 public override void Receive( Payload value )
		 {
			  _listeners.notify( _executor, listener => listener.receive( value ) );
		 }

		 public virtual AtomicBroadcastContextImpl Snapshot( CommonContextState commonStateSnapshot, LogProvider logging, Timeouts timeouts, Executor executor, HeartbeatContext heartbeatContext )
		 {
			  return new AtomicBroadcastContextImpl( Me, commonStateSnapshot, logging, timeouts, executor, heartbeatContext );
		 }

		 public override bool Equals( object o )
		 {
			  return this == o || !( o == null || this.GetType() != o.GetType() );
		 }

		 public override int GetHashCode()
		 {
			  return 0;
		 }

		 public override bool HasQuorum()
		 {
			  int availableMembers = ( int ) Iterables.count( _heartbeatContext.Alive );
			  int totalMembers = CommonState.configuration().Members.Count;
			  return Quorums.isQuorum( availableMembers, totalMembers );
		 }
	}

}