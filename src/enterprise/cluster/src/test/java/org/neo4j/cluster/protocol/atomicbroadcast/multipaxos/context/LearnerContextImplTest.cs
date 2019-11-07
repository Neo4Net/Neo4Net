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
namespace Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.context
{
	using Test = org.junit.Test;

	using Timeouts = Neo4Net.cluster.timeout.Timeouts;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.StringContains.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.logging.AssertableLogProvider.inLog;

	public class LearnerContextImplTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOnlyLogLearnMissOnce()
		 public virtual void ShouldOnlyLogLearnMissOnce()
		 {
			  // Given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.logging.AssertableLogProvider logProvider = new Neo4Net.logging.AssertableLogProvider();
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  LearnerContextImpl ctx = new LearnerContextImpl( new InstanceId( 1 ), mock( typeof( CommonContextState ) ), logProvider, mock( typeof( Timeouts ) ), mock( typeof( PaxosInstanceStore ) ), mock( typeof( AcceptorInstanceStore ) ), mock( typeof( ObjectInputStreamFactory ) ), mock( typeof( ObjectOutputStreamFactory ) ), mock( typeof( HeartbeatContextImpl ) ) );

			  // When
			  ctx.NotifyLearnMiss( new Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.InstanceId( 1L ) );
			  ctx.NotifyLearnMiss( new Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.InstanceId( 1L ) );
			  ctx.NotifyLearnMiss( new Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.InstanceId( 2L ) );
			  ctx.NotifyLearnMiss( new Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.InstanceId( 2L ) );
			  ctx.NotifyLearnMiss( new Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.InstanceId( 1L ) );

			  // Then
			  logProvider.AssertExactly( inLog( typeof( LearnerState ) ).warn( containsString( "Did not have learned value for Paxos instance 1." ) ), inLog( typeof( LearnerState ) ).warn( containsString( "Did not have learned value for Paxos instance 2." ) ), inLog( typeof( LearnerState ) ).warn( containsString( "Did not have learned value for Paxos instance 1." ) ) );
		 }

	}

}