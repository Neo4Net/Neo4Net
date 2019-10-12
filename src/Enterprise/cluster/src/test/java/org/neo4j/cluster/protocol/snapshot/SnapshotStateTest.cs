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
namespace Neo4Net.cluster.protocol.snapshot
{
	using Test = org.junit.Test;
	using Mockito = org.mockito.Mockito;


	using Neo4Net.cluster.com.message;
	using MessageHolder = Neo4Net.cluster.com.message.MessageHolder;
	using ClusterConfiguration = Neo4Net.cluster.protocol.cluster.ClusterConfiguration;
	using ClusterContext = Neo4Net.cluster.protocol.cluster.ClusterContext;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class SnapshotStateTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNoSnapshotRequestIfCoordinatorInExistingCluster() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestNoSnapshotRequestIfCoordinatorInExistingCluster()
		 {
			  IDictionary<InstanceId, URI> extraMember = new Dictionary<InstanceId, URI>();
			  URI other = URI.create( "cluster://other" );
			  extraMember[new InstanceId( 2 )] = other;
			  BaseNoSendTest( extraMember );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNoSnapshotRequestIfOnlyMember() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestNoSnapshotRequestIfOnlyMember()
		 {
			  IDictionary<InstanceId, URI> extraMember = new Dictionary<InstanceId, URI>();
			  BaseNoSendTest( extraMember );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void baseNoSendTest(java.util.Map<org.neo4j.cluster.InstanceId,java.net.URI> extraMembers) throws Throwable
		 private void BaseNoSendTest( IDictionary<InstanceId, URI> extraMembers )
		 {
			  URI me = URI.create( "cluster://me" );

			  IDictionary<InstanceId, URI> members = new Dictionary<InstanceId, URI>();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.cluster.InstanceId myId = new org.neo4j.cluster.InstanceId(1);
			  InstanceId myId = new InstanceId( 1 );
			  members[myId] = me;
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
			  members.putAll( extraMembers );

			  ClusterConfiguration clusterConfiguration = mock( typeof( ClusterConfiguration ) );
			  when( clusterConfiguration.Members ).thenReturn( members );
			  when( clusterConfiguration.GetElected( ClusterConfiguration.COORDINATOR ) ).thenReturn( myId );
			  when( clusterConfiguration.GetUriForId( myId ) ).thenReturn( me );

			  ClusterContext clusterContext = mock( typeof( ClusterContext ) );
			  when( clusterContext.Configuration ).thenReturn( clusterConfiguration );
			  when( clusterContext.MyId ).thenReturn( myId );

			  SnapshotContext context = mock( typeof( SnapshotContext ) );
			  when( context.ClusterContext ).thenReturn( clusterContext );
			  SnapshotProvider snapshotProvider = mock( typeof( SnapshotProvider ) );
			  when( context.SnapshotProvider ).thenReturn( snapshotProvider );

			  Message<SnapshotMessage> message = Message.to( SnapshotMessage.RefreshSnapshot, me );

			  MessageHolder outgoing = mock( typeof( MessageHolder ) );

			  SnapshotState newState = ( SnapshotState ) SnapshotState.Ready.handle( context, message, outgoing );
			  assertThat( newState, equalTo( SnapshotState.Ready ) );
			  Mockito.verifyZeroInteractions( outgoing );
		 }
	}

}