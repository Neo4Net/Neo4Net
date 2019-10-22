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
namespace Neo4Net.Server.Security.Auth
{
	using Test = org.junit.Test;


	using AuthenticationResult = Neo4Net.Internal.Kernel.Api.security.AuthenticationResult;
	using Config = Neo4Net.Kernel.configuration.Config;
	using User = Neo4Net.Kernel.impl.security.User;
	using Clocks = Neo4Net.Time.Clocks;
	using FakeClock = Neo4Net.Time.FakeClock;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.factory.GraphDatabaseSettings.auth_lock_time;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.security.auth.BasicAuthManagerTest.password;

	public class RateLimitedAuthenticationStrategyTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnSuccessForValidAttempt()
		 public virtual void ShouldReturnSuccessForValidAttempt()
		 {
			  // Given
			  FakeClock clock = FakeClock;
			  AuthenticationStrategy authStrategy = NewAuthStrategy( clock, 3 );
			  User user = ( new User.Builder( "user", LegacyCredential.ForPassword( "right" ) ) ).build();

			  // Then
			  assertThat( authStrategy.Authenticate( user, password( "right" ) ), equalTo( AuthenticationResult.SUCCESS ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnFailureForInvalidAttempt()
		 public virtual void ShouldReturnFailureForInvalidAttempt()
		 {
			  // Given
			  FakeClock clock = FakeClock;
			  AuthenticationStrategy authStrategy = NewAuthStrategy( clock, 3 );
			  User user = ( new User.Builder( "user", LegacyCredential.ForPassword( "right" ) ) ).build();

			  // Then
			  assertThat( authStrategy.Authenticate( user, password( "wrong" ) ), equalTo( AuthenticationResult.FAILURE ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotSlowRequestRateOnLessThanMaxFailedAttempts()
		 public virtual void ShouldNotSlowRequestRateOnLessThanMaxFailedAttempts()
		 {
			  // Given
			  FakeClock clock = FakeClock;
			  AuthenticationStrategy authStrategy = NewAuthStrategy( clock, 3 );
			  User user = ( new User.Builder( "user", LegacyCredential.ForPassword( "right" ) ) ).build();

			  // When we've failed two times
			  assertThat( authStrategy.Authenticate( user, password( "wrong" ) ), equalTo( AuthenticationResult.FAILURE ) );
			  assertThat( authStrategy.Authenticate( user, password( "wrong" ) ), equalTo( AuthenticationResult.FAILURE ) );

			  // Then
			  assertThat( authStrategy.Authenticate( user, password( "right" ) ), equalTo( AuthenticationResult.SUCCESS ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSlowRequestRateOnMultipleFailedAttempts()
		 public virtual void ShouldSlowRequestRateOnMultipleFailedAttempts()
		 {
			  TestSlowRequestRateOnMultipleFailedAttempts( 3, Duration.ofSeconds( 5 ) );
			  TestSlowRequestRateOnMultipleFailedAttempts( 1, Duration.ofSeconds( 10 ) );
			  TestSlowRequestRateOnMultipleFailedAttempts( 6, Duration.ofMinutes( 1 ) );
			  TestSlowRequestRateOnMultipleFailedAttempts( 42, Duration.ofMinutes( 2 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSlowRequestRateOnMultipleFailedAttemptsWhereAttemptIsValid()
		 public virtual void ShouldSlowRequestRateOnMultipleFailedAttemptsWhereAttemptIsValid()
		 {
			  TestSlowRequestRateOnMultipleFailedAttemptsWhereAttemptIsValid( 3, Duration.ofSeconds( 5 ) );
			  TestSlowRequestRateOnMultipleFailedAttemptsWhereAttemptIsValid( 1, Duration.ofSeconds( 11 ) );
			  TestSlowRequestRateOnMultipleFailedAttemptsWhereAttemptIsValid( 22, Duration.ofMinutes( 2 ) );
			  TestSlowRequestRateOnMultipleFailedAttemptsWhereAttemptIsValid( 42, Duration.ofDays( 4 ) );
		 }

		 private void TestSlowRequestRateOnMultipleFailedAttempts( int maxFailedAttempts, Duration lockDuration )
		 {
			  // Given
			  FakeClock clock = FakeClock;
			  AuthenticationStrategy authStrategy = NewAuthStrategy( clock, maxFailedAttempts, lockDuration );
			  User user = ( new User.Builder( "user", LegacyCredential.ForPassword( "right" ) ) ).build();

			  // When we've failed max number of times
			  for ( int i = 0; i < maxFailedAttempts; i++ )
			  {
					assertThat( authStrategy.Authenticate( user, password( "wrong" ) ), equalTo( AuthenticationResult.FAILURE ) );
			  }

			  // Then
			  assertThat( authStrategy.Authenticate( user, password( "wrong" ) ), equalTo( AuthenticationResult.TOO_MANY_ATTEMPTS ) );

			  // But when time heals all wounds
			  clock.Forward( lockDuration.plus( 1, SECONDS ) );

			  // Then things should be alright
			  assertThat( authStrategy.Authenticate( user, password( "wrong" ) ), equalTo( AuthenticationResult.FAILURE ) );
		 }

		 private void TestSlowRequestRateOnMultipleFailedAttemptsWhereAttemptIsValid( int maxFailedAttempts, Duration lockDuration )
		 {
			  // Given
			  FakeClock clock = FakeClock;
			  AuthenticationStrategy authStrategy = NewAuthStrategy( clock, maxFailedAttempts, lockDuration );
			  User user = ( new User.Builder( "user", LegacyCredential.ForPassword( "right" ) ) ).build();

			  // When we've failed max number of times
			  for ( int i = 0; i < maxFailedAttempts; i++ )
			  {
					assertThat( authStrategy.Authenticate( user, password( "wrong" ) ), equalTo( AuthenticationResult.FAILURE ) );
			  }

			  // Then
			  assertThat( authStrategy.Authenticate( user, password( "right" ) ), equalTo( AuthenticationResult.TOO_MANY_ATTEMPTS ) );

			  // But when time heals all wounds
			  clock.Forward( lockDuration.plus( 1, SECONDS ) );

			  // Then things should be alright
			  assertThat( authStrategy.Authenticate( user, password( "right" ) ), equalTo( AuthenticationResult.SUCCESS ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowUnlimitedFailedAttemptsWhenMaxFailedAttemptsIsZero()
		 public virtual void ShouldAllowUnlimitedFailedAttemptsWhenMaxFailedAttemptsIsZero()
		 {
			  TestUnlimitedFailedAuthAttempts( 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowUnlimitedFailedAttemptsWhenMaxFailedAttemptsIsNegative()
		 public virtual void ShouldAllowUnlimitedFailedAttemptsWhenMaxFailedAttemptsIsNegative()
		 {
			  TestUnlimitedFailedAuthAttempts( -42 );
		 }

		 private void TestUnlimitedFailedAuthAttempts( int maxFailedAttempts )
		 {
			  FakeClock clock = FakeClock;
			  AuthenticationStrategy authStrategy = NewAuthStrategy( clock, maxFailedAttempts );
			  User user = ( new User.Builder( "user", LegacyCredential.ForPassword( "right" ) ) ).build();

			  int attempts = ThreadLocalRandom.current().Next(5, 100);
			  for ( int i = 0; i < attempts; i++ )
			  {
					assertEquals( AuthenticationResult.FAILURE, authStrategy.Authenticate( user, password( "wrong" ) ) );
			  }
		 }

		 private FakeClock FakeClock
		 {
			 get
			 {
				  return Clocks.fakeClock();
			 }
		 }

		 private static RateLimitedAuthenticationStrategy NewAuthStrategy( Clock clock, int maxFailedAttempts )
		 {
			  Duration defaultLockDuration = Config.defaults().get(auth_lock_time);
			  return NewAuthStrategy( clock, maxFailedAttempts, defaultLockDuration );
		 }

		 private static RateLimitedAuthenticationStrategy NewAuthStrategy( Clock clock, int maxFailedAttempts, Duration lockDuration )
		 {
			  return new RateLimitedAuthenticationStrategy( clock, lockDuration, maxFailedAttempts );
		 }
	}

}