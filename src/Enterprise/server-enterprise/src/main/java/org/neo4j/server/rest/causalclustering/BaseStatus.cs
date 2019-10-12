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
namespace Neo4Net.Server.rest.causalclustering
{
	using ObjectMapper = org.codehaus.jackson.map.ObjectMapper;
	using JsonSerialize = org.codehaus.jackson.map.annotate.JsonSerialize;


	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using OutputFormat = Neo4Net.Server.rest.repr.OutputFormat;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.causalclustering.CausalClusteringService.BASE_PATH;

	internal abstract class BaseStatus : CausalClusteringStatus
	{
		public abstract Response Description();
		public abstract Response Writable();
		public abstract Response Readonly();
		public abstract Response Available();
		 private readonly OutputFormat _output;

		 internal BaseStatus( OutputFormat output )
		 {
			  this._output = output;
		 }

		 public override Response Discover()
		 {
			  return _output.ok( new CausalClusteringDiscovery( BASE_PATH ) );
		 }

		 internal virtual Response StatusResponse( long lastAppliedRaftIndex, bool isParticipatingInRaftGroup, ICollection<MemberId> votingMembers, bool isHealthy, MemberId memberId, MemberId leader, Duration millisSinceLastLeaderMessage, bool isCore )
		 {
			  string jsonObject;
			  ObjectMapper objectMapper = new ObjectMapper();
			  try
			  {
					jsonObject = objectMapper.writeValueAsString( new ClusterStatusResponse( lastAppliedRaftIndex, isParticipatingInRaftGroup, votingMembers, isHealthy, memberId, leader, millisSinceLastLeaderMessage, isCore ) );
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
			  return status( OK ).type( MediaType.APPLICATION_JSON ).entity( jsonObject ).build();
		 }

		 internal virtual Response PositiveResponse()
		 {
			  return PlainTextResponse( OK, "true" );
		 }

		 internal virtual Response NegativeResponse()
		 {
			  return PlainTextResponse( NOT_FOUND, "false" );
		 }

		 private Response PlainTextResponse( Response.Status status, string entityBody )
		 {
			  return status( status ).type( TEXT_PLAIN_TYPE ).entity( entityBody ).build();
		 }
	}

}