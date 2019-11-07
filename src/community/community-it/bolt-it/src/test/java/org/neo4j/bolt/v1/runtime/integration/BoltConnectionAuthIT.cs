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
namespace Neo4Net.Bolt.v1.runtime.integration
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using BoltStateMachine = Neo4Net.Bolt.runtime.BoltStateMachine;
	using BoltResponseRecorder = Neo4Net.Bolt.testing.BoltResponseRecorder;
	using BoltTestUtil = Neo4Net.Bolt.testing.BoltTestUtil;
	using InitMessage = Neo4Net.Bolt.v1.messaging.request.InitMessage;
	using RunMessage = Neo4Net.Bolt.v1.messaging.request.RunMessage;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using Version = Neo4Net.Kernel.Internal.Version;
	using UTF8 = Neo4Net.Strings.UTF8;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.testing.BoltMatchers.failedWithStatus;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.testing.BoltMatchers.succeeded;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.testing.BoltMatchers.succeededWithMetadata;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.testing.BoltMatchers.verifyKillsConnection;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.api.security.AuthToken.newBasicAuthToken;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.TRUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.stringValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.@virtual.VirtualValues.EMPTY_MAP;

	public class BoltConnectionAuthIT
	{
		 private const string USER_AGENT = "BoltConnectionAuthIT/0.0";
		 private static readonly BoltChannel _boltChannel = BoltTestUtil.newTestBoltChannel();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public SessionRule env = new SessionRule().withAuthEnabled(true);
		 public SessionRule Env = new SessionRule().withAuthEnabled(true);

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveCredentialsExpiredStatusOnExpiredCredentials() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGiveCredentialsExpiredStatusOnExpiredCredentials()
		 {
			  // Given it is important for client applications to programmatically
			  // identify expired credentials as the cause of not being authenticated
			  BoltStateMachine machine = Env.newMachine( _boltChannel );
			  BoltResponseRecorder recorder = new BoltResponseRecorder();

			  // When
			  InitMessage init = new InitMessage( USER_AGENT, newBasicAuthToken( "Neo4Net", "Neo4Net" ) );

			  machine.Process( init, recorder );
			  machine.Process( new RunMessage( "CREATE ()", EMPTY_MAP ), recorder );

			  // Then
			  assertThat( recorder.NextResponse(), succeededWithMetadata("credentials_expired", TRUE) );
			  assertThat( recorder.NextResponse(), failedWithStatus(Neo4Net.Kernel.Api.Exceptions.Status_Security.CredentialsExpired) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveKernelVersionOnInit() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGiveKernelVersionOnInit()
		 {
			  // Given it is important for client applications to programmatically
			  // identify expired credentials as the cause of not being authenticated
			  BoltStateMachine machine = Env.newMachine( _boltChannel );
			  BoltResponseRecorder recorder = new BoltResponseRecorder();
			  string version = "Neo4Net/" + Version.Neo4NetVersion;

			  // When
			  InitMessage init = new InitMessage( USER_AGENT, newBasicAuthToken( "Neo4Net", "Neo4Net" ) );

			  machine.Process( init, recorder );
			  machine.Process( new RunMessage( "CREATE ()", EMPTY_MAP ), recorder );

			  // Then
			  assertThat( recorder.NextResponse(), succeededWithMetadata("server", stringValue(version)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseConnectionAfterAuthenticationFailure() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCloseConnectionAfterAuthenticationFailure()
		 {
			  // Given
			  BoltStateMachine machine = Env.newMachine( _boltChannel );

			  // When... then
			  InitMessage init = new InitMessage( USER_AGENT, newBasicAuthToken( "Neo4Net", "j4oen" ) );
			  BoltResponseRecorder recorder = new BoltResponseRecorder();
			  verifyKillsConnection( () => machine.process(init, recorder) );

			  // ...and
			  assertThat( recorder.NextResponse(), failedWithStatus(Neo4Net.Kernel.Api.Exceptions.Status_Security.Unauthorized) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToActOnSessionWhenUpdatingCredentials() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToActOnSessionWhenUpdatingCredentials()
		 {
			  BoltStateMachine machine = Env.newMachine( _boltChannel );
			  BoltResponseRecorder recorder = new BoltResponseRecorder();

			  // when
			  InitMessage message = new InitMessage( USER_AGENT, map( "scheme", "basic", "principal", "Neo4Net", "credentials", UTF8.encode( "Neo4Net" ), "new_credentials", UTF8.encode( "secret" ) ) );
			  machine.Process( message, recorder );
			  machine.Process( new RunMessage( "CREATE ()", EMPTY_MAP ), recorder );

			  // then
			  assertThat( recorder.NextResponse(), succeeded() );
			  assertThat( recorder.NextResponse(), succeeded() );
		 }
	}

}