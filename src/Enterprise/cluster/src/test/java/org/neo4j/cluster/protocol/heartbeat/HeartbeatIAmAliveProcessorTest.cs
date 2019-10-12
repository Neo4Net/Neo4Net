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
namespace Neo4Net.cluster.protocol.heartbeat
{
	using Test = org.junit.Test;
	using ArgumentMatchers = org.mockito.ArgumentMatchers;


	using Neo4Net.cluster.com.message;
	using MessageHolder = Neo4Net.cluster.com.message.MessageHolder;
	using MessageType = Neo4Net.cluster.com.message.MessageType;
	using ClusterConfiguration = Neo4Net.cluster.protocol.cluster.ClusterConfiguration;
	using ClusterContext = Neo4Net.cluster.protocol.cluster.ClusterContext;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doAnswer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyZeroInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class HeartbeatIAmAliveProcessorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCreateHeartbeatsForNonExistingInstances()
		 public virtual void ShouldNotCreateHeartbeatsForNonExistingInstances()
		 {
			  // GIVEN
			  MessageHolder outgoing = mock( typeof( MessageHolder ) );
			  ClusterContext mockContext = mock( typeof( ClusterContext ) );
			  ClusterConfiguration mockConfiguration = mock( typeof( ClusterConfiguration ) );
			  when( mockConfiguration.Members ).thenReturn( new HashMapAnonymousInnerClass( this ) );
			  when( mockContext.Configuration ).thenReturn( mockConfiguration );
			  HeartbeatIAmAliveProcessor processor = new HeartbeatIAmAliveProcessor( outgoing, mockContext );

			  Message incoming = Message.to( mock( typeof( MessageType ) ), URI.create( "ha://someAwesomeInstanceInJapan" ) ).setHeader( Message.HEADER_FROM, "some://value" ).setHeader( Message.HEADER_INSTANCE_ID, "5" );

			  // WHEN
			  processor.Process( incoming );

			  // THEN
			  verifyZeroInteractions( outgoing );
		 }

		 private class HashMapAnonymousInnerClass : Dictionary<InstanceId, URI>
		 {
			 private readonly HeartbeatIAmAliveProcessorTest _outerInstance;

			 public HashMapAnonymousInnerClass( HeartbeatIAmAliveProcessorTest outerInstance )
			 {
				 this.outerInstance = outerInstance;

				 this.put( new InstanceId( 1 ), URI.create( "ha://1" ) );
				 this.put( new InstanceId( 2 ), URI.create( "ha://2" ) );
			 }

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotProcessMessagesWithEqualFromAndToHeaders()
		 public virtual void ShouldNotProcessMessagesWithEqualFromAndToHeaders()
		 {
			  URI to = URI.create( "ha://someAwesomeInstanceInJapan" );

			  // GIVEN
			  MessageHolder outgoing = mock( typeof( MessageHolder ) );
			  ClusterContext mockContext = mock( typeof( ClusterContext ) );
			  ClusterConfiguration mockConfiguration = mock( typeof( ClusterConfiguration ) );
			  when( mockConfiguration.Members ).thenReturn( new HashMapAnonymousInnerClass2( this ) );
			  when( mockContext.Configuration ).thenReturn( mockConfiguration );

			  HeartbeatIAmAliveProcessor processor = new HeartbeatIAmAliveProcessor( outgoing, mockContext );
			  Message incoming = Message.to( mock( typeof( MessageType ) ), to ).setHeader( Message.HEADER_FROM, to.toASCIIString() ).setHeader(Message.HEADER_INSTANCE_ID, "1");

			  // WHEN
			  processor.Process( incoming );

			  // THEN
			  verifyZeroInteractions( outgoing );
		 }

		 private class HashMapAnonymousInnerClass2 : Dictionary<InstanceId, URI>
		 {
			 private readonly HeartbeatIAmAliveProcessorTest _outerInstance;

			 public HashMapAnonymousInnerClass2( HeartbeatIAmAliveProcessorTest outerInstance )
			 {
				 this.outerInstance = outerInstance;

				 this.put( new InstanceId( 1 ), URI.create( "ha://1" ) );
				 this.put( new InstanceId( 2 ), URI.create( "ha://2" ) );
			 }

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotGenerateHeartbeatsForSuspicions()
		 public virtual void ShouldNotGenerateHeartbeatsForSuspicions()
		 {
			  URI to = URI.create( "ha://1" );

			  // GIVEN
			  MessageHolder outgoing = mock( typeof( MessageHolder ) );
			  ClusterContext mockContext = mock( typeof( ClusterContext ) );
			  ClusterConfiguration mockConfiguration = mock( typeof( ClusterConfiguration ) );
			  when( mockConfiguration.Members ).thenReturn( new HashMapAnonymousInnerClass3( this ) );
			  when( mockContext.Configuration ).thenReturn( mockConfiguration );

			  HeartbeatIAmAliveProcessor processor = new HeartbeatIAmAliveProcessor( outgoing, mockContext );
			  Message incoming = Message.to( HeartbeatMessage.Suspicions, to ).setHeader( Message.HEADER_FROM, to.toASCIIString() ).setHeader(Message.HEADER_INSTANCE_ID, "1");
			  assertEquals( HeartbeatMessage.Suspicions, incoming.MessageType );

			  // WHEN
			  processor.Process( incoming );

			  // THEN
			  verifyZeroInteractions( outgoing );
		 }

		 private class HashMapAnonymousInnerClass3 : Dictionary<InstanceId, URI>
		 {
			 private readonly HeartbeatIAmAliveProcessorTest _outerInstance;

			 public HashMapAnonymousInnerClass3( HeartbeatIAmAliveProcessorTest outerInstance )
			 {
				 this.outerInstance = outerInstance;

				 this.put( new InstanceId( 1 ), URI.create( "ha://1" ) );
				 this.put( new InstanceId( 2 ), URI.create( "ha://2" ) );
			 }

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotGenerateHeartbeatsForHeartbeats()
		 public virtual void ShouldNotGenerateHeartbeatsForHeartbeats()
		 {
			  URI to = URI.create( "ha://1" );

			  // GIVEN
			  MessageHolder outgoing = mock( typeof( MessageHolder ) );
			  ClusterContext mockContext = mock( typeof( ClusterContext ) );
			  ClusterConfiguration mockConfiguration = mock( typeof( ClusterConfiguration ) );
			  when( mockConfiguration.Members ).thenReturn( new HashMapAnonymousInnerClass4( this ) );
			  when( mockContext.Configuration ).thenReturn( mockConfiguration );

			  HeartbeatIAmAliveProcessor processor = new HeartbeatIAmAliveProcessor( outgoing, mockContext );
			  Message incoming = Message.to( HeartbeatMessage.IAmAlive, to ).setHeader( Message.HEADER_FROM, to.toASCIIString() ).setHeader(Message.HEADER_INSTANCE_ID, "1");
			  assertEquals( HeartbeatMessage.IAmAlive, incoming.MessageType );

			  // WHEN
			  processor.Process( incoming );

			  // THEN
			  verifyZeroInteractions( outgoing );
		 }

		 private class HashMapAnonymousInnerClass4 : Dictionary<InstanceId, URI>
		 {
			 private readonly HeartbeatIAmAliveProcessorTest _outerInstance;

			 public HashMapAnonymousInnerClass4( HeartbeatIAmAliveProcessorTest outerInstance )
			 {
				 this.outerInstance = outerInstance;

				 this.put( new InstanceId( 1 ), URI.create( "ha://1" ) );
				 this.put( new InstanceId( 2 ), URI.create( "ha://2" ) );
			 }

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCorrectlySetTheInstanceIdHeaderInTheGeneratedHeartbeat()
		 public virtual void ShouldCorrectlySetTheInstanceIdHeaderInTheGeneratedHeartbeat()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<org.neo4j.cluster.com.message.Message> sentOut = new java.util.LinkedList<>();
			  IList<Message> sentOut = new LinkedList<Message>();

			  // Given
			  MessageHolder holder = mock( typeof( MessageHolder ) );
			  // The sender, which adds messages outgoing to the list above.
			  doAnswer(invocation =>
			  {
				sentOut.Add( invocation.getArgument( 0 ) );
				return null;
			  }).when( holder ).offer( ArgumentMatchers.any<Message<MessageType>>() );

			  ClusterContext mockContext = mock( typeof( ClusterContext ) );
			  ClusterConfiguration mockConfiguration = mock( typeof( ClusterConfiguration ) );
			  when( mockConfiguration.Members ).thenReturn( new HashMapAnonymousInnerClass5( this ) );
			  when( mockContext.Configuration ).thenReturn( mockConfiguration );

			  HeartbeatIAmAliveProcessor processor = new HeartbeatIAmAliveProcessor( holder, mockContext );

			  Message incoming = Message.to( mock( typeof( MessageType ) ), URI.create( "ha://someAwesomeInstanceInJapan" ) ).setHeader( Message.HEADER_INSTANCE_ID, "2" ).setHeader( Message.HEADER_FROM, "ha://2" );

			  // WHEN
			  processor.Process( incoming );

			  // THEN
			  assertEquals( 1, sentOut.Count );
			  assertEquals( HeartbeatMessage.IAmAlive, sentOut[0].MessageType );
			  assertEquals( new InstanceId( 2 ), ( ( HeartbeatMessage.IAmAliveState ) sentOut[0].Payload ).Server );
		 }

		 private class HashMapAnonymousInnerClass5 : Dictionary<InstanceId, URI>
		 {
			 private readonly HeartbeatIAmAliveProcessorTest _outerInstance;

			 public HashMapAnonymousInnerClass5( HeartbeatIAmAliveProcessorTest outerInstance )
			 {
				 this.outerInstance = outerInstance;

				 this.put( new InstanceId( 1 ), URI.create( "ha://1" ) );
				 this.put( new InstanceId( 2 ), URI.create( "ha://2" ) );
			 }

		 }

		 /*
		  * This test is required to ensure compatibility with the previous version. If we fail on non existing HEADER_INSTANCE_ID
		  * header then heartbeats may pause during rolling upgrades and cause timeouts, which we don't want.
		  */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRevertToInverseUriLookupIfNoInstanceIdHeader()
		 public virtual void ShouldRevertToInverseUriLookupIfNoInstanceIdHeader()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<org.neo4j.cluster.com.message.Message> sentOut = new java.util.LinkedList<>();
			  IList<Message> sentOut = new LinkedList<Message>();
			  string instance2UriString = "ha://2";

			  // Given
			  MessageHolder holder = mock( typeof( MessageHolder ) );
			  // The sender, which adds messages outgoing to the list above.
			  doAnswer(invocation =>
			  {
				sentOut.Add( invocation.getArgument( 0 ) );
				return null;
			  }).when( holder ).offer( ArgumentMatchers.any<Message<MessageType>>() );

			  ClusterContext mockContext = mock( typeof( ClusterContext ) );
			  ClusterConfiguration mockConfiguration = mock( typeof( ClusterConfiguration ) );
			  when( mockConfiguration.GetIdForUri( URI.create( instance2UriString ) ) ).thenReturn( new InstanceId( 2 ) );
			  when( mockConfiguration.Members ).thenReturn( new HashMapAnonymousInnerClass6( this ) );
			  when( mockContext.Configuration ).thenReturn( mockConfiguration );

			  HeartbeatIAmAliveProcessor processor = new HeartbeatIAmAliveProcessor( holder, mockContext );

			  Message incoming = Message.to( mock( typeof( MessageType ) ), URI.create( "ha://someAwesomeInstanceInJapan" ) ).setHeader( Message.HEADER_FROM, instance2UriString );

			  // WHEN
			  processor.Process( incoming );

			  // THEN
			  assertEquals( 1, sentOut.Count );
			  assertEquals( HeartbeatMessage.IAmAlive, sentOut[0].MessageType );
			  assertEquals( new InstanceId( 2 ), ( ( HeartbeatMessage.IAmAliveState ) sentOut[0].Payload ).Server );
		 }

		 private class HashMapAnonymousInnerClass6 : Dictionary<InstanceId, URI>
		 {
			 private readonly HeartbeatIAmAliveProcessorTest _outerInstance;

			 public HashMapAnonymousInnerClass6( HeartbeatIAmAliveProcessorTest outerInstance )
			 {
				 this.outerInstance = outerInstance;

				 this.put( new InstanceId( 1 ), URI.create( "ha://1" ) );
				 this.put( new InstanceId( 2 ), URI.create( "ha://2" ) );
			 }

		 }
	}

}