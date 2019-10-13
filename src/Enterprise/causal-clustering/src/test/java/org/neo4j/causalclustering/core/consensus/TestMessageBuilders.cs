﻿/*
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
namespace Neo4Net.causalclustering.core.consensus
{
	using AppendEntriesRequestBuilder = Neo4Net.causalclustering.core.consensus.roles.AppendEntriesRequestBuilder;
	using AppendEntriesResponseBuilder = Neo4Net.causalclustering.core.consensus.roles.AppendEntriesResponseBuilder;
	using PreVoteRequestBuilder = Neo4Net.causalclustering.core.consensus.vote.PreVoteRequestBuilder;
	using PreVoteResponseBuilder = Neo4Net.causalclustering.core.consensus.vote.PreVoteResponseBuilder;
	using VoteRequestBuilder = Neo4Net.causalclustering.core.consensus.vote.VoteRequestBuilder;
	using VoteResponseBuilder = Neo4Net.causalclustering.core.consensus.vote.VoteResponseBuilder;

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