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

	using Outcome = Org.Neo4j.causalclustering.core.consensus.outcome.Outcome;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using Org.Neo4j.Helpers.Collection;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;

	public class MessageUtils
	{
		 private MessageUtils()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static RaftMessages_RaftMessage messageFor(org.neo4j.causalclustering.core.consensus.outcome.Outcome outcome, final org.neo4j.causalclustering.identity.MemberId member)
		 public static RaftMessages_RaftMessage MessageFor( Outcome outcome, MemberId member )
		 {
			  System.Predicate<RaftMessages_Directed> selectMember = message => message.to() == member;
			  try
			  {
					return Iterables.single( new FilteringIterable<>( outcome.OutgoingMessages, selectMember ) ).message();
			  }
			  catch ( NoSuchElementException )
			  {
					throw new AssertionError( format( "Expected message for %s, but outcome only contains %s.", member, outcome.OutgoingMessages ) );
			  }
		 }
	}

}