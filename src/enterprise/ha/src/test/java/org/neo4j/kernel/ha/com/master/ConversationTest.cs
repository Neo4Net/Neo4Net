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
namespace Neo4Net.Kernel.ha.com.master
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using InjectMocks = org.mockito.InjectMocks;
	using Mock = org.mockito.Mock;
	using MockitoJUnitRunner = org.mockito.junit.MockitoJUnitRunner;


	using Locks = Neo4Net.Kernel.impl.locking.Locks;
	using ThreadingRule = Neo4Net.Test.rule.concurrent.ThreadingRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doAnswer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(MockitoJUnitRunner.class) public class ConversationTest
	public class ConversationTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Mock private org.Neo4Net.kernel.impl.locking.Locks_Client client;
		 private Neo4Net.Kernel.impl.locking.Locks_Client _client;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @InjectMocks private Conversation conversation;
		 private Conversation _conversation;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.concurrent.ThreadingRule threadingRule = new org.Neo4Net.test.rule.concurrent.ThreadingRule();
		 public ThreadingRule ThreadingRule = new ThreadingRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void stopAlreadyClosedConversationDoNotTouchLocks()
		 public virtual void StopAlreadyClosedConversationDoNotTouchLocks()
		 {
			  _conversation.close();
			  _conversation.stop();
			  _conversation.stop();
			  _conversation.stop();

			  verify( _client ).close();
			  assertFalse( _conversation.Active );
			  verifyNoMoreInteractions( _client );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void stopCloseConversation()
		 public virtual void StopCloseConversation()
		 {
			  _conversation.stop();
			  _conversation.close();

			  verify( _client ).stop();
			  verify( _client ).close();
			  assertFalse( _conversation.Active );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 3000) public void conversationCanNotBeStoppedAndClosedConcurrently() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ConversationCanNotBeStoppedAndClosedConcurrently()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch answerLatch = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent answerLatch = new System.Threading.CountdownEvent( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch stopLatch = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent stopLatch = new System.Threading.CountdownEvent( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch stopReadyLatch = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent stopReadyLatch = new System.Threading.CountdownEvent( 1 );
			  const int sleepTime = 1000;
			  doAnswer(invocation =>
			  {
				stopReadyLatch.Signal();
				stopLatch.await();
				TimeUnit.MILLISECONDS.sleep( sleepTime );
				return null;
			  }).when( _client ).stop();
			  doAnswer(invocation =>
			  {
				answerLatch.Signal();
				return null;
			  }).when( _client ).close();

			  ThreadingRule.execute(_conversation =>
			  {
				_conversation.stop();
				return null;
			  }, _conversation);

			  stopReadyLatch.await();
			  ThreadingRule.execute(_conversation =>
			  {
				_conversation.close();
				return null;
			  }, _conversation);

			  long raceStartTime = DateTimeHelper.CurrentUnixTimeMillis();
			  stopLatch.Signal();
			  answerLatch.await();
			  // execution time should be at least 1000 millis
			  long executionTime = DateTimeHelper.CurrentUnixTimeMillis() - raceStartTime;
			  assertTrue( string.Format( "Execution time should be at least equal to {0:D}, but was {1:D}.", sleepTime, executionTime ), executionTime >= sleepTime );
		 }
	}

}