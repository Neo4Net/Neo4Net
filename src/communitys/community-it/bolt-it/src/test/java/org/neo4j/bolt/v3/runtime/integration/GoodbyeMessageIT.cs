using System;
using System.Collections.Generic;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Bolt.v3.runtime.integration
{
	using Matcher = org.hamcrest.Matcher;
	using TypeSafeMatcher = org.hamcrest.TypeSafeMatcher;
	using Test = org.junit.Test;


	using ResetMessage = Neo4Net.Bolt.v1.messaging.request.ResetMessage;
	using Neo4jWithSocket = Neo4Net.Bolt.v1.transport.integration.Neo4jWithSocket;
	using BeginMessage = Neo4Net.Bolt.v3.messaging.request.BeginMessage;
	using RunMessage = Neo4Net.Bolt.v3.messaging.request.RunMessage;
	using Predicates = Neo4Net.Functions.Predicates;
	using KernelTransactionHandle = Neo4Net.Kernel.api.KernelTransactionHandle;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using KernelTransactions = Neo4Net.Kernel.Impl.Api.KernelTransactions;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.allOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasEntry;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasKey;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.util.MessageMatchers.msgFailure;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.util.MessageMatchers.msgSuccess;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.transport.integration.TransportTestUtil.eventuallyReceives;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.transport.integration.TransportTestUtil.serverImmediatelyDisconnects;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v3.messaging.request.GoodbyeMessage.GOODBYE_MESSAGE;

	public class GoodbyeMessageIT : BoltV3TransportBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseConnectionInConnected() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCloseConnectionInConnected()
		 {
			  // Given
			  Connection.connect( Address ).send( Util.acceptedVersions( 3, 2, 1, 0 ) );
			  assertThat( Connection, eventuallyReceives( new sbyte[]{ 0, 0, 0, 3 } ) );

			  // When
			  Connection.send( Util.chunk( GOODBYE_MESSAGE ) );

			  // Then
			  assertThat( Connection, serverImmediatelyDisconnects() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseConnectionInReady() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCloseConnectionInReady()
		 {
			  // Given
			  NegotiateBoltV3();

			  // When
			  Connection.send( Util.chunk( GOODBYE_MESSAGE ) );

			  // Then
			  assertThat( Connection, serverImmediatelyDisconnects() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseConnectionInStreaming() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCloseConnectionInStreaming()
		 {
			  // Given
			  NegotiateBoltV3();

			  // When
			  Connection.send( Util.chunk( new RunMessage( "UNWIND [1,2,3] AS a RETURN a, a * a AS a_squared" ) ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.hamcrest.Matcher<java.util.Map<? extends String,?>> entryFieldMatcher = hasEntry(is("fields"), equalTo(asList("a", "a_squared")));
			  Matcher<IDictionary<string, ?>> entryFieldMatcher = hasEntry( @is( "fields" ), equalTo( asList( "a", "a_squared" ) ) );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess( allOf( entryFieldMatcher, hasKey( "t_first" ) ) ) ) );
			  // you shall be in the streaming state now
			  Connection.send( Util.chunk( GOODBYE_MESSAGE ) );

			  // Then
			  assertThat( Connection, serverImmediatelyDisconnects() );
			  assertThat( Server, EventuallyClosesTransaction() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseConnectionInFailed() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCloseConnectionInFailed()
		 {
			  // Given
			  NegotiateBoltV3();

			  // When
			  Connection.send( Util.chunk( new RunMessage( "I am sending you to failed state!" ) ) );
			  assertThat( Connection, Util.eventuallyReceives( msgFailure( Neo4Net.Kernel.Api.Exceptions.Status_Statement.SyntaxError, string.Format( "Invalid input 'I': expected <init> (line 1, column 1 (offset: 0))%n" + "\"I am sending you to failed state!\"%n" + " ^" ) ) ) );
			  // you shall be in the failed state now
			  Connection.send( Util.chunk( GOODBYE_MESSAGE ) );

			  // Then
			  assertThat( Connection, serverImmediatelyDisconnects() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseConnectionInTxReady() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCloseConnectionInTxReady()
		 {
			  // Given
			  NegotiateBoltV3();

			  // When
			  Connection.send( Util.chunk( new BeginMessage() ) );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess() ) );
			  // you shall be in tx_ready state now
			  Connection.send( Util.chunk( GOODBYE_MESSAGE ) );

			  // Then
			  assertThat( Connection, serverImmediatelyDisconnects() );
			  assertThat( Server, EventuallyClosesTransaction() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseConnectionInTxStreaming() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCloseConnectionInTxStreaming()
		 {
			  // Given
			  NegotiateBoltV3();

			  // When
			  Connection.send( Util.chunk( new BeginMessage(), new RunMessage("UNWIND [1,2,3] AS a RETURN a, a * a AS a_squared") ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.hamcrest.Matcher<java.util.Map<? extends String,?>> entryFieldMatcher = hasEntry(is("fields"), equalTo(asList("a", "a_squared")));
			  Matcher<IDictionary<string, ?>> entryFieldMatcher = hasEntry( @is( "fields" ), equalTo( asList( "a", "a_squared" ) ) );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess(), msgSuccess(allOf(entryFieldMatcher, hasKey("t_first"))) ) );

			  // you shall be in the tx_streaming state now
			  Connection.send( Util.chunk( GOODBYE_MESSAGE ) );
			  // Then
			  assertThat( Connection, serverImmediatelyDisconnects() );
			  assertThat( Server, EventuallyClosesTransaction() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDropConnectionImmediatelyAfterGoodbye() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDropConnectionImmediatelyAfterGoodbye()
		 {
			  // Given
			  NegotiateBoltV3();

			  // When
			  Connection.send( Util.chunk( GOODBYE_MESSAGE, ResetMessage.INSTANCE, new RunMessage( "RETURN 1" ) ) );

			  // Then
			  assertThat( Connection, serverImmediatelyDisconnects() );
		 }

		 private static Matcher<Neo4jWithSocket> EventuallyClosesTransaction()
		 {
			  return new TypeSafeMatcherAnonymousInnerClass();
		 }

		 private class TypeSafeMatcherAnonymousInnerClass : TypeSafeMatcher<Neo4jWithSocket>
		 {
			 public override void describeTo( org.hamcrest.Description description )
			 {
				  description.appendText( "Eventually close all transactions" );
			 }

			 protected internal override bool matchesSafely( Neo4jWithSocket server )
			 {
				  System.Func<bool> condition = () => getActiveTransactions(server).size() == 0;
				  try
				  {
						Predicates.await( condition, 2, TimeUnit.SECONDS );
						return true;
				  }
				  catch ( Exception )
				  {
						return false;
				  }
			 }

			 private ISet<KernelTransactionHandle> getActiveTransactions( Neo4jWithSocket server )
			 {
				  GraphDatabaseAPI gdb = ( GraphDatabaseAPI ) server.GraphDatabaseService();
				  return gdb.DependencyResolver.resolveDependency( typeof( KernelTransactions ) ).activeTransactions();
			 }
		 }
	}

}