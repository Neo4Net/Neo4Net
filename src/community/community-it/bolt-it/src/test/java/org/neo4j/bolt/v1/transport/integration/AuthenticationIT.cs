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
namespace Neo4Net.Bolt.v1.transport.integration
{
	using Description = org.hamcrest.Description;
	using TypeSafeMatcher = org.hamcrest.TypeSafeMatcher;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using ResponseMessage = Neo4Net.Bolt.messaging.ResponseMessage;
	using AckFailureMessage = Neo4Net.Bolt.v1.messaging.request.AckFailureMessage;
	using InitMessage = Neo4Net.Bolt.v1.messaging.request.InitMessage;
	using PullAllMessage = Neo4Net.Bolt.v1.messaging.request.PullAllMessage;
	using ResetMessage = Neo4Net.Bolt.v1.messaging.request.ResetMessage;
	using RunMessage = Neo4Net.Bolt.v1.messaging.request.RunMessage;
	using FailureMessage = Neo4Net.Bolt.v1.messaging.response.FailureMessage;
	using TransportConnection = Neo4Net.Bolt.v1.transport.socket.client.TransportConnection;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using ValueUtils = Neo4Net.Kernel.impl.util.ValueUtils;
	using Version = Neo4Net.Kernel.Internal.Version;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;
	using AnyValue = Neo4Net.Values.AnyValue;
	using MapValue = Neo4Net.Values.@virtual.MapValue;
	using VirtualValues = Neo4Net.Values.@virtual.VirtualValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.util.MessageMatchers.msgFailure;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.util.MessageMatchers.msgIgnored;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.util.MessageMatchers.msgSuccess;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.transport.integration.TransportTestUtil.eventuallyDisconnects;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.AssertableLogProvider.inLog;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.assertion.Assert.assertEventually;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.EMPTY_MAP;

	public class AuthenticationIT : AbstractBoltTransportsTest
	{
		private bool InstanceFieldsInitialized = false;

		public AuthenticationIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			Server = new Neo4jWithSocket( this.GetType(), TestGraphDatabaseFactory, FsRule, SettingsFunction );
			RuleChain = RuleChain.outerRule( FsRule ).around( Server );
		}

		 protected internal EphemeralFileSystemRule FsRule = new EphemeralFileSystemRule();
		 protected internal readonly AssertableLogProvider LogProvider = new AssertableLogProvider();
		 protected internal Neo4jWithSocket Server;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(fsRule).around(server);
		 public RuleChain RuleChain;

		 protected internal virtual TestGraphDatabaseFactory TestGraphDatabaseFactory
		 {
			 get
			 {
				  return new TestGraphDatabaseFactory( LogProvider );
			 }
		 }

		 protected internal virtual System.Action<IDictionary<string, string>> SettingsFunction
		 {
			 get
			 {
				  return settings => settings.put( GraphDatabaseSettings.auth_enabled.name(), "true" );
			 }
		 }

		 private new HostnamePort _address;
		 private readonly string _version = "Neo4j/" + Version.Neo4jVersion;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _address = Server.lookupDefaultConnector();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWithCredentialsExpiredOnFirstUse() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondWithCredentialsExpiredOnFirstUse()
		 {
			  // When
			  Connection.connect( _address ).send( Util.defaultAcceptedVersions() ).send(Util.chunk(new InitMessage("TestClient/1.1", map("principal", "neo4j", "credentials", "neo4j", "scheme", "basic"))));

			  // Then
			  assertThat( Connection, Util.eventuallyReceivesSelectedProtocolVersion() );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess( map( "credentials_expired", true, "server", _version ) ) ) );

			  VerifyConnectionOpen();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyConnectionOpen() throws java.io.IOException
		 private void VerifyConnectionOpen()
		 {
			  Connection.send( Util.chunk( ResetMessage.INSTANCE ) );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailIfWrongCredentials() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailIfWrongCredentials()
		 {
			  // When
			  Connection.connect( _address ).send( Util.defaultAcceptedVersions() ).send(Util.chunk(new InitMessage("TestClient/1.1", map("principal", "neo4j", "credentials", "wrong", "scheme", "basic"))));

			  // Then
			  assertThat( Connection, Util.eventuallyReceivesSelectedProtocolVersion() );
			  assertThat( Connection, Util.eventuallyReceives( msgFailure( Neo4Net.Kernel.Api.Exceptions.Status_Security.Unauthorized, "The client is unauthorized due to authentication failure." ) ) );

			  assertThat( Connection, eventuallyDisconnects() );
			  assertEventually( ignore => "Matching log call not found in\n" + LogProvider.serialize(), this.authFailureLoggedToUserLog, @is(true), 30, SECONDS );
		 }

		 private bool AuthFailureLoggedToUserLog()
		 {
			  string boltPackageName = typeof( BoltServer ).Assembly.GetName().Name;
			  return LogProvider.containsMatchingLogCall( inLog( containsString( boltPackageName ) ).warn( containsString( "The client is unauthorized due to authentication failure." ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailIfWrongCredentialsFollowingSuccessfulLogin() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailIfWrongCredentialsFollowingSuccessfulLogin()
		 {
			  // When change password
			  Connection.connect( _address ).send( Util.defaultAcceptedVersions() ).send(Util.chunk(new InitMessage("TestClient/1.1", map("principal", "neo4j", "credentials", "neo4j", "new_credentials", "secret", "scheme", "basic"))));
			  // Then
			  assertThat( Connection, Util.eventuallyReceivesSelectedProtocolVersion() );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess() ) );

			  // When login again with the new password
			  Reconnect();
			  Connection.connect( _address ).send( Util.defaultAcceptedVersions() ).send(Util.chunk(new InitMessage("TestClient/1.1", map("principal", "neo4j", "credentials", "secret", "scheme", "basic"))));

			  // Then
			  assertThat( Connection, Util.eventuallyReceivesSelectedProtocolVersion() );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess() ) );

			  // When login again with the wrong password
			  Reconnect();
			  Connection.connect( _address ).send( Util.defaultAcceptedVersions() ).send(Util.chunk(new InitMessage("TestClient/1.1", map("principal", "neo4j", "credentials", "wrong", "scheme", "basic"))));

			  // Then
			  assertThat( Connection, Util.eventuallyReceivesSelectedProtocolVersion() );
			  assertThat( Connection, Util.eventuallyReceives( msgFailure( Neo4Net.Kernel.Api.Exceptions.Status_Security.Unauthorized, "The client is unauthorized due to authentication failure." ) ) );

			  assertThat( Connection, eventuallyDisconnects() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailIfMalformedAuthTokenWrongType() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailIfMalformedAuthTokenWrongType()
		 {
			  // When
			  Connection.connect( _address ).send( Util.defaultAcceptedVersions() ).send(Util.chunk(new InitMessage("TestClient/1.1", map("principal", singletonList("neo4j"), "credentials", "neo4j", "scheme", "basic"))));

			  // Then
			  assertThat( Connection, Util.eventuallyReceivesSelectedProtocolVersion() );
			  assertThat( Connection, Util.eventuallyReceives( msgFailure( Neo4Net.Kernel.Api.Exceptions.Status_Security.Unauthorized, "Unsupported authentication token, the value associated with the key `principal` " + "must be a String but was: ArrayList" ) ) );

			  assertThat( Connection, eventuallyDisconnects() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailIfMalformedAuthTokenMissingKey() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailIfMalformedAuthTokenMissingKey()
		 {
			  // When
			  Connection.connect( _address ).send( Util.defaultAcceptedVersions() ).send(Util.chunk(new InitMessage("TestClient/1.1", map("principal", "neo4j", "this-should-have-been-credentials", "neo4j", "scheme", "basic"))));

			  // Then
			  assertThat( Connection, Util.eventuallyReceivesSelectedProtocolVersion() );
			  assertThat( Connection, Util.eventuallyReceives( msgFailure( Neo4Net.Kernel.Api.Exceptions.Status_Security.Unauthorized, "Unsupported authentication token, missing key `credentials`" ) ) );

			  assertThat( Connection, eventuallyDisconnects() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailIfMalformedAuthTokenMissingScheme() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailIfMalformedAuthTokenMissingScheme()
		 {
			  // When
			  Connection.connect( _address ).send( Util.defaultAcceptedVersions() ).send(Util.chunk(new InitMessage("TestClient/1.1", map("principal", "neo4j", "credentials", "neo4j"))));

			  // Then
			  assertThat( Connection, Util.eventuallyReceivesSelectedProtocolVersion() );
			  assertThat( Connection, Util.eventuallyReceives( msgFailure( Neo4Net.Kernel.Api.Exceptions.Status_Security.Unauthorized, "Unsupported authentication token, missing key `scheme`" ) ) );

			  assertThat( Connection, eventuallyDisconnects() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailIfMalformedAuthTokenUnknownScheme() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailIfMalformedAuthTokenUnknownScheme()
		 {
			  // When
			  Connection.connect( _address ).send( Util.defaultAcceptedVersions() ).send(Util.chunk(new InitMessage("TestClient/1.1", map("principal", "neo4j", "credentials", "neo4j", "scheme", "unknown"))));

			  // Then
			  assertThat( Connection, Util.eventuallyReceivesSelectedProtocolVersion() );
			  assertThat( Connection, Util.eventuallyReceives( msgFailure( Neo4Net.Kernel.Api.Exceptions.Status_Security.Unauthorized, "Unsupported authentication token, scheme 'unknown' is not supported." ) ) );

			  assertThat( Connection, eventuallyDisconnects() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailDifferentlyIfTooManyFailedAuthAttempts() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailDifferentlyIfTooManyFailedAuthAttempts()
		 {
			  // Given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long timeout = System.currentTimeMillis() + 60_000;
			  long timeout = DateTimeHelper.CurrentUnixTimeMillis() + 60_000;
			  FailureMessage failureMessage = null;

			  // When
			  while ( failureMessage == null )
			  {
					if ( DateTimeHelper.CurrentUnixTimeMillis() > timeout )
					{
						 fail( "Timed out waiting for the authentication failure to occur." );
					}

					ExecutorService executor = Executors.newFixedThreadPool( 10 );

					// Fire up some parallel connections that all send wrong authentication tokens
					IList<CompletableFuture<FailureMessage>> futures = new List<CompletableFuture<FailureMessage>>();
					for ( int i = 0; i < 10; i++ )
					{
						 futures.Add( CompletableFuture.supplyAsync( this.collectAuthFailureOnFailedAuth, executor ) );
					}

					try
					{
						 // Wait for all tasks to complete
						 CompletableFuture.allOf( futures.ToArray() ).get(30, SECONDS);

						 // We want at least one of the futures to fail with our expected code
						 for ( int i = 0; i < futures.Count; i++ )
						 {
							  FailureMessage recordedMessage = futures[i].get();

							  if ( recordedMessage != null )
							  {
									failureMessage = recordedMessage;

									break;
							  }
						 }
					}
					catch ( TimeoutException )
					{
						 // if jobs did not complete, let's try again
						 // do nothing
					}
					finally
					{
						 executor.shutdown();
					}
			  }

			  assertThat( failureMessage.Status(), equalTo(Neo4Net.Kernel.Api.Exceptions.Status_Security.AuthenticationRateLimit) );
			  assertThat( failureMessage.Message(), containsString("The client has provided incorrect authentication details too many times in a row.") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToUpdateCredentials() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToUpdateCredentials()
		 {
			  // When
			  Connection.connect( _address ).send( Util.defaultAcceptedVersions() ).send(Util.chunk(new InitMessage("TestClient/1.1", map("principal", "neo4j", "credentials", "neo4j", "new_credentials", "secret", "scheme", "basic"))));

			  // Then
			  assertThat( Connection, Util.eventuallyReceivesSelectedProtocolVersion() );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess() ) );

			  // If I reconnect I cannot use the old password
			  Reconnect();
			  Connection.connect( _address ).send( Util.defaultAcceptedVersions() ).send(Util.chunk(new InitMessage("TestClient/1.1", map("principal", "neo4j", "credentials", "neo4j", "scheme", "basic"))));
			  assertThat( Connection, Util.eventuallyReceivesSelectedProtocolVersion() );
			  assertThat( Connection, Util.eventuallyReceives( msgFailure( Neo4Net.Kernel.Api.Exceptions.Status_Security.Unauthorized, "The client is unauthorized due to authentication failure." ) ) );

			  // But the new password works fine
			  Reconnect();
			  Connection.connect( _address ).send( Util.defaultAcceptedVersions() ).send(Util.chunk(new InitMessage("TestClient/1.1", map("principal", "neo4j", "credentials", "secret", "scheme", "basic"))));
			  assertThat( Connection, Util.eventuallyReceivesSelectedProtocolVersion() );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAuthenticatedAfterUpdatingCredentials() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAuthenticatedAfterUpdatingCredentials()
		 {
			  // When
			  Connection.connect( _address ).send( Util.defaultAcceptedVersions() ).send(Util.chunk(new InitMessage("TestClient/1.1", map("principal", "neo4j", "credentials", "neo4j", "new_credentials", "secret", "scheme", "basic"))));

			  // Then
			  assertThat( Connection, Util.eventuallyReceivesSelectedProtocolVersion() );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess() ) );

			  // When
			  Connection.send( Util.chunk( new RunMessage( "MATCH (n) RETURN n", EMPTY_MAP ), PullAllMessage.INSTANCE ) );

			  // Then
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess(), msgSuccess() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToChangePasswordUsingBuiltInProcedure() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToChangePasswordUsingBuiltInProcedure()
		 {
			  // When
			  Connection.connect( _address ).send( Util.defaultAcceptedVersions() ).send(Util.chunk(new InitMessage("TestClient/1.1", map("principal", "neo4j", "credentials", "neo4j", "scheme", "basic"))));

			  // Then
			  assertThat( Connection, Util.eventuallyReceivesSelectedProtocolVersion() );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess( map( "credentials_expired", true, "server", _version ) ) ) );

			  // When
			  Connection.send( Util.chunk( new RunMessage( "CALL dbms.security.changePassword", SingletonMap( "password", "secret" ) ), PullAllMessage.INSTANCE ) );

			  // Then
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess() ) );

			  // If I reconnect I cannot use the old password
			  Reconnect();
			  Connection.connect( _address ).send( Util.defaultAcceptedVersions() ).send(Util.chunk(new InitMessage("TestClient/1.1", map("principal", "neo4j", "credentials", "neo4j", "scheme", "basic"))));
			  assertThat( Connection, Util.eventuallyReceivesSelectedProtocolVersion() );
			  assertThat( Connection, Util.eventuallyReceives( msgFailure( Neo4Net.Kernel.Api.Exceptions.Status_Security.Unauthorized, "The client is unauthorized due to authentication failure." ) ) );

			  // But the new password works fine
			  Reconnect();
			  Connection.connect( _address ).send( Util.defaultAcceptedVersions() ).send(Util.chunk(new InitMessage("TestClient/1.1", map("principal", "neo4j", "credentials", "secret", "scheme", "basic"))));
			  assertThat( Connection, Util.eventuallyReceivesSelectedProtocolVersion() );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAuthenticatedAfterChangePasswordUsingBuiltInProcedure() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAuthenticatedAfterChangePasswordUsingBuiltInProcedure()
		 {
			  // When
			  Connection.connect( _address ).send( Util.defaultAcceptedVersions() ).send(Util.chunk(new InitMessage("TestClient/1.1", map("principal", "neo4j", "credentials", "neo4j", "scheme", "basic"))));

			  // Then
			  assertThat( Connection, Util.eventuallyReceivesSelectedProtocolVersion() );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess( map( "credentials_expired", true, "server", _version ) ) ) );

			  // When
			  Connection.send( Util.chunk( new RunMessage( "CALL dbms.security.changePassword", SingletonMap( "password", "secret" ) ), PullAllMessage.INSTANCE ) );

			  // Then
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess(), msgSuccess() ) );

			  // When
			  Connection.send( Util.chunk( new RunMessage( "MATCH (n) RETURN n", EMPTY_MAP ), PullAllMessage.INSTANCE ) );

			  // Then
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess(), msgSuccess() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailWhenReusingTheSamePassword() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailWhenReusingTheSamePassword()
		 {
			  // When
			  Connection.connect( _address ).send( Util.defaultAcceptedVersions() ).send(Util.chunk(new InitMessage("TestClient/1.1", map("principal", "neo4j", "credentials", "neo4j", "scheme", "basic"))));

			  // Then
			  assertThat( Connection, Util.eventuallyReceivesSelectedProtocolVersion() );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess( map( "credentials_expired", true, "server", _version ) ) ) );

			  // When
			  Connection.send( Util.chunk( new RunMessage( "CALL dbms.security.changePassword", SingletonMap( "password", "neo4j" ) ), PullAllMessage.INSTANCE ) );

			  // Then
			  assertThat( Connection, Util.eventuallyReceives( msgFailure( Neo4Net.Kernel.Api.Exceptions.Status_General.InvalidArguments, "Old password and new password cannot be the same." ) ) );

			  // However you should also be able to recover
			  Connection.send( Util.chunk( AckFailureMessage.INSTANCE, new RunMessage( "CALL dbms.security.changePassword", SingletonMap( "password", "abc" ) ), PullAllMessage.INSTANCE ) );
			  assertThat( Connection, Util.eventuallyReceives( msgIgnored(), msgSuccess(), msgSuccess(), msgSuccess() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailWhenSubmittingEmptyPassword() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailWhenSubmittingEmptyPassword()
		 {
			  // When
			  Connection.connect( _address ).send( Util.defaultAcceptedVersions() ).send(Util.chunk(new InitMessage("TestClient/1.1", map("principal", "neo4j", "credentials", "neo4j", "scheme", "basic"))));

			  // Then
			  assertThat( Connection, Util.eventuallyReceivesSelectedProtocolVersion() );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess( map( "credentials_expired", true, "server", _version ) ) ) );

			  // When
			  Connection.send( Util.chunk( new RunMessage( "CALL dbms.security.changePassword", SingletonMap( "password", "" ) ), PullAllMessage.INSTANCE ) );

			  // Then
			  assertThat( Connection, Util.eventuallyReceives( msgFailure( Neo4Net.Kernel.Api.Exceptions.Status_General.InvalidArguments, "A password cannot be empty." ) ) );

			  // However you should also be able to recover
			  Connection.send( Util.chunk( AckFailureMessage.INSTANCE, new RunMessage( "CALL dbms.security.changePassword", SingletonMap( "password", "abc" ) ), PullAllMessage.INSTANCE ) );
			  assertThat( Connection, Util.eventuallyReceives( msgIgnored(), msgSuccess(), msgSuccess(), msgSuccess() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToReadWhenPasswordChangeRequired() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotBeAbleToReadWhenPasswordChangeRequired()
		 {
			  // When
			  Connection.connect( _address ).send( Util.defaultAcceptedVersions() ).send(Util.chunk(new InitMessage("TestClient/1.1", map("principal", "neo4j", "credentials", "neo4j", "scheme", "basic"))));

			  // Then
			  assertThat( Connection, Util.eventuallyReceivesSelectedProtocolVersion() );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess( map( "credentials_expired", true, "server", _version ) ) ) );

			  // When
			  Connection.send( Util.chunk( new RunMessage( "MATCH (n) RETURN n", EMPTY_MAP ), PullAllMessage.INSTANCE ) );

			  // Then
			  assertThat( Connection, Util.eventuallyReceives( msgFailure( Neo4Net.Kernel.Api.Exceptions.Status_Security.CredentialsExpired, "The credentials you provided were valid, but must be changed before you can use this instance." ) ) );

			  assertThat( Connection, eventuallyDisconnects() );
		 }

		 internal class FailureMsgMatcher : TypeSafeMatcher<ResponseMessage>
		 {
			 private readonly AuthenticationIT _outerInstance;

			 public FailureMsgMatcher( AuthenticationIT outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal FailureMessage SpecialMessage;

			  public override void DescribeTo( Description description )
			  {
					description.appendText( "FAILURE" );
			  }

			  protected internal override bool MatchesSafely( ResponseMessage t )
			  {
					assertThat( t, instanceOf( typeof( FailureMessage ) ) );
					FailureMessage msg = ( FailureMessage ) t;
					if ( !msg.Status().Equals(Neo4Net.Kernel.Api.Exceptions.Status_Security.Unauthorized) || !msg.Message().Contains("The client is unauthorized due to authentication failure.") )
					{
						 SpecialMessage = msg;
					}
					return true;
			  }

			  public virtual bool GotSpecialMessage()
			  {
					return SpecialMessage != null;
			  }
		 }

		 private MapValue SingletonMap( string key, object value )
		 {
			  return VirtualValues.map( new string[]{ key }, new AnyValue[]{ ValueUtils.of( value ) } );
		 }

		 private FailureMessage CollectAuthFailureOnFailedAuth()
		 {
			  FailureMsgMatcher failureRecorder = new FailureMsgMatcher( this );

			  TransportConnection connection = null;
			  try
			  {
					connection = NewConnection();

					connection.Connect( _address ).send( Util.defaultAcceptedVersions() ).send(Util.chunk(new InitMessage("TestClient/1.1", map("principal", "neo4j", "credentials", "WHAT_WAS_THE_PASSWORD_AGAIN", "scheme", "basic"))));

					assertThat( connection, Util.eventuallyReceivesSelectedProtocolVersion() );
					assertThat( connection, Util.eventuallyReceives( failureRecorder ) );
					assertThat( connection, eventuallyDisconnects() );
			  }
			  catch ( Exception ex )
			  {
					throw new Exception( ex );
			  }
			  finally
			  {
					if ( connection != null )
					{
						 try
						 {
							  connection.Disconnect();
						 }
						 catch ( IOException ex )
						 {
							  throw new Exception( ex );
						 }
					}
			  }

			  return failureRecorder.SpecialMessage;
		 }
	}

}