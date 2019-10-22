using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.upstream.strategies
{
	using Assert = org.junit.Assert;
	using Test = org.junit.Test;


	using RoleInfo = Neo4Net.causalclustering.discovery.RoleInfo;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Config = Neo4Net.Kernel.configuration.Config;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;


	public class LeaderOnlyStrategyTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void ignoresSelf() throws org.Neo4Net.causalclustering.upstream.UpstreamDatabaseSelectionException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void IgnoresSelf()
		 {
			  // given
			  MemberId myself = new MemberId( new System.Guid( 1234, 5678 ) );
			  string groupName = "groupName";

			  // and
			  LeaderOnlyStrategy leaderOnlyStrategy = new LeaderOnlyStrategy();
			  TopologyServiceThatPrioritisesItself topologyServiceNoRetriesStrategy = new TopologyServiceThatPrioritisesItselfAnonymousInnerClass( this, myself, groupName );
			  leaderOnlyStrategy.Inject( topologyServiceNoRetriesStrategy, Config.defaults(), NullLogProvider.Instance, myself );

			  // when
			  Optional<MemberId> resolved = leaderOnlyStrategy.UpstreamDatabase();

			  // then
			  Assert.assertTrue( resolved.Present );
			  Assert.assertNotEquals( myself, resolved.get() );
		 }

		 private class TopologyServiceThatPrioritisesItselfAnonymousInnerClass : TopologyServiceThatPrioritisesItself
		 {
			 private readonly LeaderOnlyStrategyTest _outerInstance;

			 private MemberId _myself;

			 public TopologyServiceThatPrioritisesItselfAnonymousInnerClass( LeaderOnlyStrategyTest outerInstance, MemberId myself, string groupName ) : base( myself, groupName )
			 {
				 this.outerInstance = outerInstance;
				 this._myself = myself;
			 }

			 public override IDictionary<MemberId, RoleInfo> allCoreRoles()
			 {
				  IDictionary<MemberId, RoleInfo> roles = new Dictionary<MemberId, RoleInfo>();
				  roles[_myself] = RoleInfo.LEADER;
				  roles[coreNotSelf] = RoleInfo.LEADER;
				  return roles;
			 }
		 }
	}

}