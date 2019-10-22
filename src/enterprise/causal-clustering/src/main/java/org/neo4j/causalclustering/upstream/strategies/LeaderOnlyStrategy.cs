﻿using System.Collections.Generic;

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

	using RoleInfo = Neo4Net.causalclustering.discovery.RoleInfo;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Service = Neo4Net.Helpers.Service;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(UpstreamDatabaseSelectionStrategy.class) public class LeaderOnlyStrategy extends org.Neo4Net.causalclustering.upstream.UpstreamDatabaseSelectionStrategy
	public class LeaderOnlyStrategy : UpstreamDatabaseSelectionStrategy
	{
		 public const string IDENTITY = "leader-only";

		 public LeaderOnlyStrategy() : base(IDENTITY)
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.Optional<org.Neo4Net.causalclustering.identity.MemberId> upstreamDatabase() throws org.Neo4Net.causalclustering.upstream.UpstreamDatabaseSelectionException
		 public override Optional<MemberId> UpstreamDatabase()
		 {
			  IDictionary<MemberId, RoleInfo> memberRoles = TopologyService.allCoreRoles();

			  if ( memberRoles.Count == 0 )
			  {
					throw new UpstreamDatabaseSelectionException( "No core servers available" );
			  }

			  foreach ( KeyValuePair<MemberId, RoleInfo> entry in memberRoles.SetOfKeyValuePairs() )
			  {
					RoleInfo role = entry.Value;
					if ( role == RoleInfo.LEADER && !Objects.Equals( Myself, entry.Key ) )
					{
						 return entry.Key;
					}
			  }

			  return null;
		 }
	}

}