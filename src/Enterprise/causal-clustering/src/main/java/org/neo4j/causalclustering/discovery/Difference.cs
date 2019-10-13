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
namespace Neo4Net.causalclustering.discovery
{
	using MemberId = Neo4Net.causalclustering.identity.MemberId;

	internal class Difference
	{
		 private MemberId _memberId;
		 private CatchupServerAddress _server;

		 private Difference( MemberId memberId, CatchupServerAddress server )
		 {
			  this._memberId = memberId;
			  this._server = server;
		 }

		 internal static Difference AsDifference<T>( Topology<T> topology, MemberId memberId ) where T : DiscoveryServerInfo
		 {
			  return new Difference( memberId, topology.Find( memberId ).orElse( null ) );
		 }

		 public override string ToString()
		 {
			  return string.Format( "{{memberId={0}, info={1}}}", _memberId, _server );
		 }
	}

}