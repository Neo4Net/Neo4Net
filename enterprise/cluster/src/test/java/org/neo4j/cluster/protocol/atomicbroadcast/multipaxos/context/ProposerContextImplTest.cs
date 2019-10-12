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
namespace Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos.context
{
	using Test = org.junit.Test;


	using HeartbeatContext = Org.Neo4j.cluster.protocol.heartbeat.HeartbeatContext;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class ProposerContextImplTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOnlyReturnMembersAsAcceptorsIfTheyAreAlive()
		 public virtual void ShouldOnlyReturnMembersAsAcceptorsIfTheyAreAlive()
		 {
			  assertEquals( 5, LimitedAcceptors( 5, InstanceIds( 5 ) ) );
			  assertEquals( 3, LimitedAcceptors( 3, InstanceIds( 5 ) ) );
			  assertEquals( 3, LimitedAcceptors( 3, InstanceIds( 3 ) ) );
			  assertEquals( 2, LimitedAcceptors( 2, InstanceIds( 2 ) ) );
			  assertEquals( 1, LimitedAcceptors( 1, InstanceIds( 1 ) ) );
			  assertEquals( 0, LimitedAcceptors( 1, InstanceIds( 0 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCalculateMajorityOfAcceptors()
		 public virtual void ShouldCalculateMajorityOfAcceptors()
		 {
			  ProposerContextImpl proposerContext = new ProposerContextImpl( new InstanceId( 1 ), null, null, null, null, null );

			  assertEquals( 1, proposerContext.GetMinimumQuorumSize( AcceptorUris( 1 ) ) );
			  assertEquals( 2, proposerContext.GetMinimumQuorumSize( AcceptorUris( 2 ) ) );
			  assertEquals( 2, proposerContext.GetMinimumQuorumSize( AcceptorUris( 3 ) ) );
			  assertEquals( 3, proposerContext.GetMinimumQuorumSize( AcceptorUris( 4 ) ) );
			  assertEquals( 3, proposerContext.GetMinimumQuorumSize( AcceptorUris( 5 ) ) );
			  assertEquals( 4, proposerContext.GetMinimumQuorumSize( AcceptorUris( 6 ) ) );
			  assertEquals( 4, proposerContext.GetMinimumQuorumSize( AcceptorUris( 7 ) ) );
		 }

		 private IList<URI> AcceptorUris( int numberOfAcceptors )
		 {
			  IList<URI> items = new List<URI>();

			  for ( int i = 0; i < numberOfAcceptors; i++ )
			  {
					items.Add( URI.create( i.ToString() ) );
			  }

			  return items;
		 }

		 private IList<InstanceId> InstanceIds( int numberOfAcceptors )
		 {
			  IList<InstanceId> items = new List<InstanceId>();

			  for ( int i = 0; i < numberOfAcceptors; i++ )
			  {
					items.Add( new InstanceId( i ) );
			  }

			  return items;
		 }

		 private int LimitedAcceptors( int maxAcceptors, IList<InstanceId> alive )
		 {
			  CommonContextState commonContextState = new CommonContextState( null, maxAcceptors );

			  HeartbeatContext heartbeatContext = mock( typeof( HeartbeatContext ) );
			  when( heartbeatContext.Alive ).thenReturn( alive );
			  when( heartbeatContext.GetUriForId( any( typeof( InstanceId ) ) ) ).thenReturn( URI.create( "http://localhost:8080" ) );

			  // when
			  ProposerContextImpl proposerContext = new ProposerContextImpl( new InstanceId( 1 ), commonContextState, null, null, null, heartbeatContext );

			  return proposerContext.Acceptors.Count;
		 }
	}

}