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
namespace Neo4Net.causalclustering.discovery
{

	using LeaderInfo = Neo4Net.causalclustering.core.consensus.LeaderInfo;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Config = Neo4Net.Kernel.configuration.Config;
	using SafeLifecycle = Neo4Net.Kernel.Lifecycle.SafeLifecycle;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

	public abstract class AbstractCoreTopologyService : SafeLifecycle, CoreTopologyService
	{
		public abstract IDictionary<MemberId, RoleInfo> AllCoreRoles();
		public abstract Optional<Neo4Net.Helpers.AdvertisedSocketAddress> FindCatchupAddress( MemberId upstream );
		public abstract ReadReplicaTopology LocalReadReplicas();
		public abstract ReadReplicaTopology AllReadReplicas();
		public abstract CoreTopology LocalCoreServers();
		public abstract CoreTopology AllCoreServers();
		public abstract string LocalDBName();
		public abstract LeaderInfo Leader { get; }
		public abstract bool SetClusterId( Neo4Net.causalclustering.identity.ClusterId clusterId, string dbName );
		 protected internal readonly CoreTopologyListenerService ListenerService = new CoreTopologyListenerService();
		 protected internal readonly Config Config;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal readonly MemberId MyselfConflict;
		 protected internal readonly Log Log;
		 protected internal readonly Log UserLog;

		 protected internal AbstractCoreTopologyService( Config config, MemberId myself, LogProvider logProvider, LogProvider userLogProvider )
		 {
			  this.Config = config;
			  this.MyselfConflict = myself;
			  this.Log = logProvider.getLog( this.GetType() );
			  this.UserLog = userLogProvider.getLog( this.GetType() );
		 }

		 public override void AddLocalCoreTopologyListener( CoreTopologyService_Listener listener )
		 {
			 lock ( this )
			 {
				  this.ListenerService.addCoreTopologyListener( listener );
				  listener.OnCoreTopologyChange( LocalCoreServers() );
			 }
		 }

		 public override void RemoveLocalCoreTopologyListener( CoreTopologyService_Listener listener )
		 {
			  ListenerService.removeCoreTopologyListener( listener );
		 }

		 public override void SetLeader( LeaderInfo newLeader, string dbName )
		 {
			  LeaderInfo currentLeaderInfo = Leader;

			  if ( currentLeaderInfo.Term() < newLeader.Term() && LocalDBName().Equals(dbName) )
			  {
					Log.info( "Leader %s updating leader info for database %s and term %s", MyselfConflict, dbName, newLeader.Term() );
					Leader0 = newLeader;
			  }
		 }

		 protected internal abstract LeaderInfo Leader0 { set; }

		 public override void HandleStepDown( long term, string dbName )
		 {
			  LeaderInfo localLeaderInfo = Leader;

			  bool wasLeaderForDbAndTerm = Objects.Equals( MyselfConflict, localLeaderInfo.MemberId() ) && LocalDBName().Equals(dbName) && term == localLeaderInfo.Term();

			  if ( wasLeaderForDbAndTerm )
			  {
					Log.info( "Step down event detected. This topology member, with MemberId %s, was leader in term %s, now moving " + "to follower.", MyselfConflict, localLeaderInfo.Term() );
					HandleStepDown0( localLeaderInfo.StepDown() );
			  }
		 }

		 protected internal abstract void HandleStepDown0( LeaderInfo steppingDown );

		 public override MemberId Myself()
		 {
			  return MyselfConflict;
		 }
	}

}