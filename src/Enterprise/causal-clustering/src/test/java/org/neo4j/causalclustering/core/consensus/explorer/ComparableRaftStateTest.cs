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
namespace Neo4Net.causalclustering.core.consensus.explorer
{
	using Test = org.junit.Test;

	using ConsecutiveInFlightCache = Neo4Net.causalclustering.core.consensus.log.cache.ConsecutiveInFlightCache;
	using InMemoryRaftLog = Neo4Net.causalclustering.core.consensus.log.InMemoryRaftLog;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.identity.RaftTestMember.member;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;

	public class ComparableRaftStateTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void twoIdenticalStatesShouldBeEqual()
		 public virtual void TwoIdenticalStatesShouldBeEqual()
		 {
			  // given
			  NullLogProvider logProvider = NullLogProvider.Instance;
			  ComparableRaftState state1 = new ComparableRaftState( member( 0 ), asSet( member( 0 ), member( 1 ), member( 2 ) ), asSet( member( 0 ), member( 1 ), member( 2 ) ), false, new InMemoryRaftLog(), new ConsecutiveInFlightCache(), logProvider );

			  ComparableRaftState state2 = new ComparableRaftState( member( 0 ), asSet( member( 0 ), member( 1 ), member( 2 ) ), asSet( member( 0 ), member( 1 ), member( 2 ) ), false, new InMemoryRaftLog(), new ConsecutiveInFlightCache(), logProvider );

			  // then
			  assertEquals( state1, state2 );
		 }
	}

}