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
namespace Neo4Net.causalclustering.core.consensus.explorer.action
{
	using Test = org.junit.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.identity.RaftTestMember.member;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;

	public class OutOfOrderDeliveryTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReOrder() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReOrder()
		 {
			  // given
			  ClusterState clusterState = new ClusterState( asSet( member( 0 ) ) );
			  clusterState.Queues[member( 0 )].AddLast( new RaftMessages_Timeout_Election( member( 0 ) ) );
			  clusterState.Queues[member( 0 )].AddLast( new RaftMessages_Timeout_Heartbeat( member( 0 ) ) );

			  // when
			  ClusterState reOrdered = ( new OutOfOrderDelivery( member( 0 ) ) ).Advance( clusterState );

			  // then
			  assertEquals( new RaftMessages_Timeout_Heartbeat( member( 0 ) ), reOrdered.Queues[member( 0 )].RemoveFirst() );
			  assertEquals( new RaftMessages_Timeout_Election( member( 0 ) ), reOrdered.Queues[member( 0 )].RemoveFirst() );
		 }
	}

}