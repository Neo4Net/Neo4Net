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
namespace Org.Neo4j.causalclustering.core.consensus
{
	using AppendEntriesRequestBuilder = Org.Neo4j.causalclustering.core.consensus.roles.AppendEntriesRequestBuilder;
	using AppendEntriesResponseBuilder = Org.Neo4j.causalclustering.core.consensus.roles.AppendEntriesResponseBuilder;
	using PreVoteRequestBuilder = Org.Neo4j.causalclustering.core.consensus.vote.PreVoteRequestBuilder;
	using PreVoteResponseBuilder = Org.Neo4j.causalclustering.core.consensus.vote.PreVoteResponseBuilder;
	using VoteRequestBuilder = Org.Neo4j.causalclustering.core.consensus.vote.VoteRequestBuilder;
	using VoteResponseBuilder = Org.Neo4j.causalclustering.core.consensus.vote.VoteResponseBuilder;

	public class TestMessageBuilders
	{
		 private TestMessageBuilders()
		 {
		 }

		 public static AppendEntriesRequestBuilder AppendEntriesRequest()
		 {
			  return new AppendEntriesRequestBuilder();
		 }

		 public static AppendEntriesResponseBuilder AppendEntriesResponse()
		 {
			  return new AppendEntriesResponseBuilder();
		 }

		 public static HeartbeatBuilder Heartbeat()
		 {
			  return new HeartbeatBuilder();
		 }

		 public static VoteRequestBuilder VoteRequest()
		 {
			  return new VoteRequestBuilder();
		 }

		 public static PreVoteRequestBuilder PreVoteRequest()
		 {
			  return new PreVoteRequestBuilder();
		 }

		 public static VoteResponseBuilder VoteResponse()
		 {
			  return new VoteResponseBuilder();
		 }

		 public static PreVoteResponseBuilder PreVoteResponse()
		 {
			  return new PreVoteResponseBuilder();
		 }
	}

}