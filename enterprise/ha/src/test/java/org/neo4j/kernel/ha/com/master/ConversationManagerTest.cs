using System.Threading;

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
namespace Org.Neo4j.Kernel.ha.com.master
{
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using InOrder = org.mockito.InOrder;
	using Mock = org.mockito.Mock;
	using MockitoJUnitRunner = org.mockito.junit.MockitoJUnitRunner;

	using RequestContext = Org.Neo4j.com.RequestContext;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using ConversationSPI = Org.Neo4j.Kernel.ha.cluster.ConversationSPI;
	using Org.Neo4j.Kernel.impl.util.collection;
	using Group = Org.Neo4j.Scheduler.Group;
	using JobHandle = Org.Neo4j.Scheduler.JobHandle;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.inOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(MockitoJUnitRunner.class) public class ConversationManagerTest
	public class ConversationManagerTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Mock private org.neo4j.kernel.ha.cluster.ConversationSPI conversationSPI;
		 private ConversationSPI _conversationSPI;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Mock private org.neo4j.kernel.configuration.Config config;
		 private Config _config;
		 private ConversationManager _conversationManager;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testStart()
		 public virtual void TestStart()
		 {
			  JobHandle reaperJobHandle = mock( typeof( JobHandle ) );
			  when( _config.get( HaSettings.lock_read_timeout ) ).thenReturn( Duration.ofMillis( 1 ) );
			  when( _conversationSPI.scheduleRecurringJob( any( typeof( Group ) ), any( typeof( Long ) ), any( typeof( ThreadStart ) ) ) ).thenReturn( reaperJobHandle );
			  _conversationManager = ConversationManager;

			  _conversationManager.start();

			  assertNotNull( _conversationManager.conversations );
			  verify( _conversationSPI ).scheduleRecurringJob( any( typeof( Group ) ), any( typeof( Long ) ), any( typeof( ThreadStart ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testStop()
		 public virtual void TestStop()
		 {
			  JobHandle reaperJobHandle = mock( typeof( JobHandle ) );
			  when( _config.get( HaSettings.lock_read_timeout ) ).thenReturn( Duration.ofMillis( 1 ) );
			  when( _conversationSPI.scheduleRecurringJob( any( typeof( Group ) ), any( typeof( Long ) ), any( typeof( ThreadStart ) ) ) ).thenReturn( reaperJobHandle );
			  _conversationManager = ConversationManager;

			  _conversationManager.start();
			  _conversationManager.stop();

			  assertNull( _conversationManager.conversations );
			  verify( reaperJobHandle ).cancel( false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testConversationWorkflow() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestConversationWorkflow()
		 {
			  RequestContext requestContext = RequestContext;
			  _conversationManager = ConversationManager;
			  TimedRepository conversationStorage = mock( typeof( TimedRepository ) );
			  _conversationManager.conversations = conversationStorage;

			  _conversationManager.begin( requestContext );
			  _conversationManager.acquire( requestContext );
			  _conversationManager.release( requestContext );
			  _conversationManager.end( requestContext );

			  InOrder conversationOrder = inOrder( conversationStorage );
			  conversationOrder.verify( conversationStorage ).begin( requestContext );
			  conversationOrder.verify( conversationStorage ).acquire( requestContext );
			  conversationOrder.verify( conversationStorage ).release( requestContext );
			  conversationOrder.verify( conversationStorage ).end( requestContext );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testConversationStop()
		 public virtual void TestConversationStop()
		 {
			  RequestContext requestContext = RequestContext;
			  _conversationManager = ConversationManager;

			  Conversation conversation = mock( typeof( Conversation ) );
			  when( conversation.Active ).thenReturn( true );

			  TimedRepository conversationStorage = mock( typeof( TimedRepository ) );
			  when( conversationStorage.end( requestContext ) ).thenReturn( conversation );
			  _conversationManager.conversations = conversationStorage;

			  _conversationManager.stop( requestContext );

			  verify( conversationStorage ).end( requestContext );
			  verify( conversation ).stop();
		 }

		 private RequestContext RequestContext
		 {
			 get
			 {
				  return new RequestContext( 1L, 1, 1, 1L, 1L );
			 }
		 }

		 private ConversationManager ConversationManager
		 {
			 get
			 {
				  return new ConversationManager( _conversationSPI, _config, 1000, 5000 );
			 }
		 }

	}

}