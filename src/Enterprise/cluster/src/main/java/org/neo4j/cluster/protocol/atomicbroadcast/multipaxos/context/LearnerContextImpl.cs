using System;
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
namespace Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.context
{
	using HeartbeatContext = Neo4Net.cluster.protocol.heartbeat.HeartbeatContext;
	using Timeouts = Neo4Net.cluster.timeout.Timeouts;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using CappedLogger = Neo4Net.Logging.@internal.CappedLogger;

	internal class LearnerContextImpl : AbstractContextImpl, LearnerContext
	{
		 // LearnerContext
		 private long _lastDeliveredInstanceId = -1;
		 private long _lastLearnedInstanceId = -1;

		 /// <summary>
		 /// To minimize logging, keep track of the latest learn miss, only log when it changes. </summary>
		 private readonly CappedLogger _learnMissLogger;

		 private readonly HeartbeatContext _heartbeatContext;
		 private readonly AcceptorInstanceStore _instanceStore;
		 private readonly ObjectInputStreamFactory _objectInputStreamFactory;
		 private readonly ObjectOutputStreamFactory _objectOutputStreamFactory;
		 private readonly PaxosInstanceStore _paxosInstances;

		 internal LearnerContextImpl( Neo4Net.cluster.InstanceId me, CommonContextState commonState, LogProvider logging, Timeouts timeouts, PaxosInstanceStore paxosInstances, AcceptorInstanceStore instanceStore, ObjectInputStreamFactory objectInputStreamFactory, ObjectOutputStreamFactory objectOutputStreamFactory, HeartbeatContext heartbeatContext ) : base( me, commonState, logging, timeouts )
		 {
			  this._heartbeatContext = heartbeatContext;
			  this._instanceStore = instanceStore;
			  this._objectInputStreamFactory = objectInputStreamFactory;
			  this._objectOutputStreamFactory = objectOutputStreamFactory;
			  this._paxosInstances = paxosInstances;
			  this._learnMissLogger = ( new CappedLogger( logging.GetLog( typeof( LearnerState ) ) ) ).setDuplicateFilterEnabled( true );
		 }

		 private LearnerContextImpl( Neo4Net.cluster.InstanceId me, CommonContextState commonState, LogProvider logging, Timeouts timeouts, long lastDeliveredInstanceId, long lastLearnedInstanceId, HeartbeatContext heartbeatContext, AcceptorInstanceStore instanceStore, ObjectInputStreamFactory objectInputStreamFactory, ObjectOutputStreamFactory objectOutputStreamFactory, PaxosInstanceStore paxosInstances ) : base( me, commonState, logging, timeouts )
		 {
			  this._lastDeliveredInstanceId = lastDeliveredInstanceId;
			  this._lastLearnedInstanceId = lastLearnedInstanceId;
			  this._heartbeatContext = heartbeatContext;
			  this._instanceStore = instanceStore;
			  this._objectInputStreamFactory = objectInputStreamFactory;
			  this._objectOutputStreamFactory = objectOutputStreamFactory;
			  this._paxosInstances = paxosInstances;
			  this._learnMissLogger = ( new CappedLogger( logging.GetLog( typeof( LearnerState ) ) ) ).setDuplicateFilterEnabled( true );
		 }

		 public virtual long LastDeliveredInstanceId
		 {
			 get
			 {
				  return _lastDeliveredInstanceId;
			 }
			 set
			 {
				  this._lastDeliveredInstanceId = value;
				  _instanceStore.lastDelivered( new InstanceId( value ) );
			 }
		 }


		 public virtual long LastLearnedInstanceId
		 {
			 get
			 {
				  return _lastLearnedInstanceId;
			 }
		 }

		 public virtual long LastKnownLearnedInstanceInCluster
		 {
			 get
			 {
				  return CommonState.lastKnownLearnedInstanceInCluster();
			 }
		 }

		 public override void SetLastKnownLearnedInstanceInCluster( long lastKnownLearnedInstanceInCluster, Neo4Net.cluster.InstanceId instanceId )
		 {
			  CommonState.setLastKnownLearnedInstanceInCluster( lastKnownLearnedInstanceInCluster, instanceId );
		 }

		 public virtual Neo4Net.cluster.InstanceId LastKnownAliveUpToDateInstance
		 {
			 get
			 {
				  return CommonState.LastKnownAliveUpToDateInstance;
			 }
		 }

		 public override void LearnedInstanceId( long instanceId )
		 {
			  this._lastLearnedInstanceId = Math.Max( _lastLearnedInstanceId, instanceId );
			  if ( _lastLearnedInstanceId > CommonState.lastKnownLearnedInstanceInCluster() )
			  {
					CommonState.setLastKnownLearnedInstanceInCluster( _lastLearnedInstanceId, null );
			  }
		 }

		 public override bool HasDeliveredAllKnownInstances()
		 {
			  return _lastDeliveredInstanceId == CommonState.lastKnownLearnedInstanceInCluster();
		 }

		 public override void Leave()
		 {
			  _lastDeliveredInstanceId = -1;
			  _lastLearnedInstanceId = -1;
			  CommonState.setLastKnownLearnedInstanceInCluster( -1, null );
		 }

		 public override PaxosInstance GetPaxosInstance( InstanceId instanceId )
		 {
			  return _paxosInstances.getPaxosInstance( instanceId );
		 }

		 public override AtomicBroadcastSerializer NewSerializer()
		 {
			  return new AtomicBroadcastSerializer( _objectInputStreamFactory, _objectOutputStreamFactory );
		 }

		 public virtual IEnumerable<Neo4Net.cluster.InstanceId> Alive
		 {
			 get
			 {
				  return _heartbeatContext.Alive;
			 }
		 }

		 public virtual long NextInstanceId
		 {
			 set
			 {
				  CommonState.NextInstanceId = value;
			 }
		 }

		 public override void NotifyLearnMiss( InstanceId instanceId )
		 {
			  _learnMissLogger.warn( "Did not have learned value for Paxos instance " + instanceId + ". " + "This generally indicates that this instance has missed too many cluster events and is " + "failing to catch up. If this error does not resolve soon it may become necessary to " + "restart this cluster member so normal operation can resume." );
		 }

		 public virtual LearnerContextImpl Snapshot( CommonContextState commonStateSnapshot, LogProvider logging, Timeouts timeouts, PaxosInstanceStore paxosInstancesSnapshot, AcceptorInstanceStore instanceStore, ObjectInputStreamFactory objectInputStreamFactory, ObjectOutputStreamFactory objectOutputStreamFactory, HeartbeatContextImpl snapshotHeartbeatContext )
		 {
			  return new LearnerContextImpl( Me, commonStateSnapshot, logging, timeouts, _lastDeliveredInstanceId, _lastLearnedInstanceId, snapshotHeartbeatContext, instanceStore, objectInputStreamFactory, objectOutputStreamFactory, paxosInstancesSnapshot );
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

			  LearnerContextImpl that = ( LearnerContextImpl ) o;

			  if ( _lastDeliveredInstanceId != that._lastDeliveredInstanceId )
			  {
					return false;
			  }
			  if ( _lastLearnedInstanceId != that._lastLearnedInstanceId )
			  {
					return false;
			  }
			  if ( _heartbeatContext != null ?!_heartbeatContext.Equals( that._heartbeatContext ) : that._heartbeatContext != null )
			  {
					return false;
			  }
			  if ( _instanceStore != null ?!_instanceStore.Equals( that._instanceStore ) : that._instanceStore != null )
			  {
					return false;
			  }
			  return _paxosInstances != null ? _paxosInstances.Equals( that._paxosInstances ) : that._paxosInstances == null;
		 }

		 public override int GetHashCode()
		 {
			  int result = ( int )( _lastDeliveredInstanceId ^ ( ( long )( ( ulong )_lastDeliveredInstanceId >> 32 ) ) );
			  result = 31 * result + ( int )( _lastLearnedInstanceId ^ ( ( long )( ( ulong )_lastLearnedInstanceId >> 32 ) ) );
			  result = 31 * result + ( _heartbeatContext != null ? _heartbeatContext.GetHashCode() : 0 );
			  result = 31 * result + ( _instanceStore != null ? _instanceStore.GetHashCode() : 0 );
			  result = 31 * result + ( _paxosInstances != null ? _paxosInstances.GetHashCode() : 0 );
			  return result;
		 }
	}

}