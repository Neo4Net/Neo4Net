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
namespace Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.context
{

	using ClusterConfiguration = Neo4Net.cluster.protocol.cluster.ClusterConfiguration;
	using Log = Neo4Net.Logging.Log;

	internal class CommonContextState
	{
		 private URI _boundAt;
		 private long _lastKnownLearnedInstanceInCluster = -1;
		 private InstanceId _lastKnownAliveUpToDateInstance;
		 private long _nextInstanceId;
		 private ClusterConfiguration _configuration;
		 private readonly int _maxAcceptors;

		 internal CommonContextState( ClusterConfiguration configuration, int maxAcceptors )
		 {
			  this._configuration = configuration;
			  this._maxAcceptors = maxAcceptors;
		 }

		 private CommonContextState( URI boundAt, long lastKnownLearnedInstanceInCluster, long nextInstanceId, ClusterConfiguration configuration, int maxAcceptors )
		 {
			  this._boundAt = boundAt;
			  this._lastKnownLearnedInstanceInCluster = lastKnownLearnedInstanceInCluster;
			  this._nextInstanceId = nextInstanceId;
			  this._configuration = configuration;
			  this._maxAcceptors = maxAcceptors;
		 }

		 public virtual URI BoundAt()
		 {
			  return _boundAt;
		 }

		 public virtual void SetBoundAt( InstanceId me, URI boundAt )
		 {
			  this._boundAt = boundAt;
			  _configuration.Members[me] = boundAt;
		 }

		 public virtual long LastKnownLearnedInstanceInCluster()
		 {
			  return _lastKnownLearnedInstanceInCluster;
		 }

		 public virtual void SetLastKnownLearnedInstanceInCluster( long lastKnownLearnedInstanceInCluster, InstanceId instanceId )
		 {
			  if ( this._lastKnownLearnedInstanceInCluster <= lastKnownLearnedInstanceInCluster )
			  {
					this._lastKnownLearnedInstanceInCluster = lastKnownLearnedInstanceInCluster;
					if ( instanceId != null )
					{
						 this._lastKnownAliveUpToDateInstance = instanceId;
					}
			  }
			  else if ( lastKnownLearnedInstanceInCluster == -1 )
			  {
					// Special case for clearing the state
					this._lastKnownLearnedInstanceInCluster = -1;
			  }
		 }

		 public virtual InstanceId LastKnownAliveUpToDateInstance
		 {
			 get
			 {
				  return this._lastKnownAliveUpToDateInstance;
			 }
		 }

		 public virtual long NextInstanceId()
		 {
			  return _nextInstanceId;
		 }

		 public virtual long NextInstanceId
		 {
			 set
			 {
				  this._nextInstanceId = value;
			 }
		 }

		 public virtual long AndIncrementInstanceId
		 {
			 get
			 {
				  return _nextInstanceId++;
			 }
		 }

		 public virtual ClusterConfiguration Configuration()
		 {
			  return _configuration;
		 }

		 public virtual ClusterConfiguration Configuration
		 {
			 set
			 {
				  this._configuration = value;
			 }
		 }

		 public virtual int MaxAcceptors
		 {
			 get
			 {
				  return _maxAcceptors;
			 }
		 }

		 public virtual CommonContextState Snapshot( Log log )
		 {
			  return new CommonContextState( _boundAt, _lastKnownLearnedInstanceInCluster, _nextInstanceId, _configuration.snapshot( log ), _maxAcceptors );
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

			  CommonContextState that = ( CommonContextState ) o;

			  if ( _lastKnownLearnedInstanceInCluster != that._lastKnownLearnedInstanceInCluster )
			  {
					return false;
			  }
			  if ( _nextInstanceId != that._nextInstanceId )
			  {
					return false;
			  }
			  if ( _boundAt != null ?!_boundAt.Equals( that._boundAt ) : that._boundAt != null )
			  {
					return false;
			  }
			  return _configuration != null ? _configuration.Equals( that._configuration ) : that._configuration == null;
		 }

		 public override int GetHashCode()
		 {
			  int result = _boundAt != null ? _boundAt.GetHashCode() : 0;
			  result = 31 * result + ( int )( _lastKnownLearnedInstanceInCluster ^ ( ( long )( ( ulong )_lastKnownLearnedInstanceInCluster >> 32 ) ) );
			  result = 31 * result + ( int )( _nextInstanceId ^ ( ( long )( ( ulong )_nextInstanceId >> 32 ) ) );
			  result = 31 * result + ( _configuration != null ? _configuration.GetHashCode() : 0 );
			  return result;
		 }
	}

}