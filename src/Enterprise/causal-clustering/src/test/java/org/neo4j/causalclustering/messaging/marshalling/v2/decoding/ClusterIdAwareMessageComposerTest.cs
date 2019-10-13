using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.messaging.marshalling.v2.decoding
{
	using Test = org.junit.Test;


	using RaftMessages = Neo4Net.causalclustering.core.consensus.RaftMessages;
	using ReplicatedTransaction = Neo4Net.causalclustering.core.state.machines.tx.ReplicatedTransaction;
	using ClusterId = Neo4Net.causalclustering.identity.ClusterId;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.fail;

	public class ClusterIdAwareMessageComposerTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowExceptionOnConflictingMessageHeaders()
		 public virtual void ShouldThrowExceptionOnConflictingMessageHeaders()
		 {
			  try
			  {
					RaftMessageComposer raftMessageComposer = new RaftMessageComposer( Clock.systemUTC() );

					raftMessageComposer.Decode( null, MessageCreator( ( a, b ) => null ), null );
					raftMessageComposer.Decode( null, MessageCreator( ( a, b ) => null ), null );
			  }
			  catch ( System.InvalidOperationException e )
			  {
					assertThat( e.Message, containsString( "Pipeline already contains message header waiting to build." ) );
					return;
			  }
			  fail();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowExceptionIfNotAllResourcesAreUsed()
		 public virtual void ShouldThrowExceptionIfNotAllResourcesAreUsed()
		 {
			  try
			  {
					RaftMessageComposer raftMessageComposer = new RaftMessageComposer( Clock.systemUTC() );

					ReplicatedTransaction replicatedTransaction = ReplicatedTransaction.from( new sbyte[0] );
					raftMessageComposer.Decode( null, replicatedTransaction, null );
					IList<object> @out = new List<object>();
					raftMessageComposer.Decode( null, MessageCreator( ( a, b ) => DummyRequest() ), @out );
			  }
			  catch ( System.InvalidOperationException e )
			  {
					assertThat( e.Message, containsString( "was composed without using all resources in the pipeline. Pipeline still contains Replicated contents" ) );
					return;
			  }
			  fail();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowExceptionIfUnrecognizedObjectIsFound()
		 public virtual void ShouldThrowExceptionIfUnrecognizedObjectIsFound()
		 {
			  try
			  {
					RaftMessageComposer raftMessageComposer = new RaftMessageComposer( Clock.systemUTC() );

					raftMessageComposer.Decode( null, "a string", null );
			  }
			  catch ( System.InvalidOperationException e )
			  {
					assertThat( e.Message, equalTo( "Unexpected object in the pipeline: a string" ) );
					return;
			  }
			  fail();
		 }

		 private Neo4Net.causalclustering.core.consensus.RaftMessages_PruneRequest DummyRequest()
		 {
			  return new Neo4Net.causalclustering.core.consensus.RaftMessages_PruneRequest( 1 );
		 }

		 private RaftMessageDecoder.ClusterIdAwareMessageComposer MessageCreator( RaftMessageDecoder.LazyComposer composer )
		 {
			  return new RaftMessageDecoder.ClusterIdAwareMessageComposer( composer, new ClusterId( System.Guid.randomUUID() ) );
		 }
	}

}