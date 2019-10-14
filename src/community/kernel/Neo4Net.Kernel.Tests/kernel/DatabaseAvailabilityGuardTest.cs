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
namespace Neo4Net.Kernel
{
	using Test = org.junit.Test;
	using Mockito = org.mockito.Mockito;
	using VerificationMode = org.mockito.verification.VerificationMode;


	using AvailabilityGuard = Neo4Net.Kernel.availability.AvailabilityGuard;
	using AvailabilityListener = Neo4Net.Kernel.availability.AvailabilityListener;
	using AvailabilityRequirement = Neo4Net.Kernel.availability.AvailabilityRequirement;
	using DatabaseAvailabilityGuard = Neo4Net.Kernel.availability.DatabaseAvailabilityGuard;
	using DescriptiveAvailabilityRequirement = Neo4Net.Kernel.availability.DescriptiveAvailabilityRequirement;
	using UnavailableException = Neo4Net.Kernel.availability.UnavailableException;
	using Log = Neo4Net.Logging.Log;
	using NullLog = Neo4Net.Logging.NullLog;
	using Clocks = Neo4Net.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.atLeastOnce;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyZeroInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.DEFAULT_DATABASE_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.NullLog.getInstance;

	public class DatabaseAvailabilityGuardTest
	{
		 private static readonly AvailabilityRequirement _requirement_1 = new DescriptiveAvailabilityRequirement( "Requirement 1" );
		 private static readonly AvailabilityRequirement _requirement_2 = new DescriptiveAvailabilityRequirement( "Requirement 2" );

		 private readonly Clock _clock = Clocks.systemClock();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void logOnAvailabilityChange()
		 public virtual void LogOnAvailabilityChange()
		 {
			  // Given
			  Log log = mock( typeof( Log ) );
			  AvailabilityGuard databaseAvailabilityGuard = GetDatabaseAvailabilityGuard( _clock, log );

			  // When starting out
			  verifyZeroInteractions( log );

			  // When requirement is added
			  databaseAvailabilityGuard.Require( _requirement_1 );

			  // Then log should have been called
			  VerifyLogging( log, atLeastOnce() );

			  // When requirement fulfilled
			  databaseAvailabilityGuard.Fulfill( _requirement_1 );

			  // Then log should have been called
			  VerifyLogging( log, times( 4 ) );

			  // When requirement is added
			  databaseAvailabilityGuard.Require( _requirement_1 );
			  databaseAvailabilityGuard.Require( _requirement_2 );

			  // Then log should have been called
			  VerifyLogging( log, times( 6 ) );

			  // When requirement fulfilled
			  databaseAvailabilityGuard.Fulfill( _requirement_1 );

			  // Then log should not have been called
			  VerifyLogging( log, times( 6 ) );

			  // When requirement fulfilled
			  databaseAvailabilityGuard.Fulfill( _requirement_2 );

			  // Then log should have been called
			  VerifyLogging( log, times( 8 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void givenAccessGuardWith2ConditionsWhenAwaitThenTimeoutAndReturnFalse()
		 public virtual void GivenAccessGuardWith2ConditionsWhenAwaitThenTimeoutAndReturnFalse()
		 {
			  // Given
			  Log log = mock( typeof( Log ) );
			  DatabaseAvailabilityGuard databaseAvailabilityGuard = GetDatabaseAvailabilityGuard( _clock, log );
			  databaseAvailabilityGuard.Require( _requirement_1 );
			  databaseAvailabilityGuard.Require( _requirement_2 );

			  // When
			  bool result = databaseAvailabilityGuard.IsAvailable( 1000 );

			  // Then
			  assertThat( result, equalTo( false ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void givenAccessGuardWith2ConditionsWhenAwaitThenActuallyWaitGivenTimeout()
		 public virtual void GivenAccessGuardWith2ConditionsWhenAwaitThenActuallyWaitGivenTimeout()
		 {
			  // Given
			  Log log = mock( typeof( Log ) );
			  DatabaseAvailabilityGuard databaseAvailabilityGuard = GetDatabaseAvailabilityGuard( _clock, log );
			  databaseAvailabilityGuard.Require( _requirement_1 );
			  databaseAvailabilityGuard.Require( _requirement_2 );

			  // When
			  long timeout = 1000;
			  long start = _clock.millis();
			  bool result = databaseAvailabilityGuard.IsAvailable( timeout );
			  long end = _clock.millis();

			  // Then
			  long waitTime = end - start;
			  assertThat( result, equalTo( false ) );
			  assertThat( waitTime, greaterThanOrEqualTo( timeout ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void givenAccessGuardWith2ConditionsWhenGrantOnceAndAwaitThenTimeoutAndReturnFalse()
		 public virtual void GivenAccessGuardWith2ConditionsWhenGrantOnceAndAwaitThenTimeoutAndReturnFalse()
		 {
			  // Given
			  Log log = mock( typeof( Log ) );
			  DatabaseAvailabilityGuard databaseAvailabilityGuard = GetDatabaseAvailabilityGuard( _clock, log );
			  databaseAvailabilityGuard.Require( _requirement_1 );
			  databaseAvailabilityGuard.Require( _requirement_2 );

			  // When
			  long start = _clock.millis();
			  long timeout = 1000;
			  databaseAvailabilityGuard.Fulfill( _requirement_1 );
			  bool result = databaseAvailabilityGuard.IsAvailable( timeout );
			  long end = _clock.millis();

			  // Then
			  long waitTime = end - start;
			  assertFalse( result );
			  assertThat( waitTime, greaterThanOrEqualTo( timeout ) );

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void givenAccessGuardWith2ConditionsWhenGrantEachAndAwaitThenTrue()
		 public virtual void GivenAccessGuardWith2ConditionsWhenGrantEachAndAwaitThenTrue()
		 {
			  // Given
			  Log log = mock( typeof( Log ) );
			  DatabaseAvailabilityGuard databaseAvailabilityGuard = GetDatabaseAvailabilityGuard( _clock, log );
			  databaseAvailabilityGuard.Require( _requirement_1 );
			  databaseAvailabilityGuard.Require( _requirement_2 );

			  // When
			  databaseAvailabilityGuard.Fulfill( _requirement_1 );
			  databaseAvailabilityGuard.Fulfill( _requirement_2 );

			  assertTrue( databaseAvailabilityGuard.IsAvailable( 1000 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void givenAccessGuardWith2ConditionsWhenGrantTwiceAndDenyOnceAndAwaitThenTimeoutAndReturnFalse()
		 public virtual void GivenAccessGuardWith2ConditionsWhenGrantTwiceAndDenyOnceAndAwaitThenTimeoutAndReturnFalse()
		 {
			  // Given
			  Log log = mock( typeof( Log ) );
			  DatabaseAvailabilityGuard databaseAvailabilityGuard = GetDatabaseAvailabilityGuard( _clock, log );
			  databaseAvailabilityGuard.Require( _requirement_1 );
			  databaseAvailabilityGuard.Require( _requirement_2 );

			  // When
			  databaseAvailabilityGuard.Fulfill( _requirement_1 );
			  databaseAvailabilityGuard.Fulfill( _requirement_1 );
			  databaseAvailabilityGuard.Require( _requirement_2 );

			  long start = _clock.millis();
			  long timeout = 1000;
			  bool result = databaseAvailabilityGuard.IsAvailable( timeout );
			  long end = _clock.millis();

			  // Then
			  long waitTime = end - start;
			  assertFalse( result );
			  assertThat( waitTime, greaterThanOrEqualTo( timeout ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void givenAccessGuardWith2ConditionsWhenGrantOnceAndAwaitAndGrantAgainThenReturnTrue()
		 public virtual void GivenAccessGuardWith2ConditionsWhenGrantOnceAndAwaitAndGrantAgainThenReturnTrue()
		 {
			  // Given
			  Log log = mock( typeof( Log ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.availability.DatabaseAvailabilityGuard databaseAvailabilityGuard = getDatabaseAvailabilityGuard(clock, log);
			  DatabaseAvailabilityGuard databaseAvailabilityGuard = GetDatabaseAvailabilityGuard( _clock, log );
			  databaseAvailabilityGuard.Require( _requirement_1 );
			  databaseAvailabilityGuard.Require( _requirement_2 );

			  databaseAvailabilityGuard.Fulfill( _requirement_2 );
			  assertFalse( databaseAvailabilityGuard.IsAvailable( 100 ) );

			  databaseAvailabilityGuard.Fulfill( _requirement_1 );
			  assertTrue( databaseAvailabilityGuard.IsAvailable( 100 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void givenAccessGuardWithConditionWhenGrantThenNotifyListeners()
		 public virtual void GivenAccessGuardWithConditionWhenGrantThenNotifyListeners()
		 {
			  // Given
			  Log log = mock( typeof( Log ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.availability.DatabaseAvailabilityGuard databaseAvailabilityGuard = getDatabaseAvailabilityGuard(clock, log);
			  DatabaseAvailabilityGuard databaseAvailabilityGuard = GetDatabaseAvailabilityGuard( _clock, log );
			  databaseAvailabilityGuard.Require( _requirement_1 );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicBoolean notified = new java.util.concurrent.atomic.AtomicBoolean();
			  AtomicBoolean notified = new AtomicBoolean();
			  AvailabilityListener availabilityListener = new AvailabilityListenerAnonymousInnerClass( this, notified );

			  databaseAvailabilityGuard.AddListener( availabilityListener );

			  // When
			  databaseAvailabilityGuard.Fulfill( _requirement_1 );

			  // Then
			  assertThat( notified.get(), equalTo(true) );
		 }

		 private class AvailabilityListenerAnonymousInnerClass : AvailabilityListener
		 {
			 private readonly DatabaseAvailabilityGuardTest _outerInstance;

			 private AtomicBoolean _notified;

			 public AvailabilityListenerAnonymousInnerClass( DatabaseAvailabilityGuardTest outerInstance, AtomicBoolean notified )
			 {
				 this.outerInstance = outerInstance;
				 this._notified = notified;
			 }

			 public void available()
			 {
				  _notified.set( true );
			 }

			 public void unavailable()
			 {
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void givenAccessGuardWithConditionWhenGrantAndDenyThenNotifyListeners()
		 public virtual void GivenAccessGuardWithConditionWhenGrantAndDenyThenNotifyListeners()
		 {
			  // Given
			  Log log = mock( typeof( Log ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.availability.DatabaseAvailabilityGuard databaseAvailabilityGuard = getDatabaseAvailabilityGuard(clock, log);
			  DatabaseAvailabilityGuard databaseAvailabilityGuard = GetDatabaseAvailabilityGuard( _clock, log );
			  databaseAvailabilityGuard.Require( _requirement_1 );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicBoolean notified = new java.util.concurrent.atomic.AtomicBoolean();
			  AtomicBoolean notified = new AtomicBoolean();
			  AvailabilityListener availabilityListener = new AvailabilityListenerAnonymousInnerClass2( this, notified );

			  databaseAvailabilityGuard.AddListener( availabilityListener );

			  // When
			  databaseAvailabilityGuard.Fulfill( _requirement_1 );
			  databaseAvailabilityGuard.Require( _requirement_1 );

			  // Then
			  assertThat( notified.get(), equalTo(true) );
		 }

		 private class AvailabilityListenerAnonymousInnerClass2 : AvailabilityListener
		 {
			 private readonly DatabaseAvailabilityGuardTest _outerInstance;

			 private AtomicBoolean _notified;

			 public AvailabilityListenerAnonymousInnerClass2( DatabaseAvailabilityGuardTest outerInstance, AtomicBoolean notified )
			 {
				 this.outerInstance = outerInstance;
				 this._notified = notified;
			 }

			 public void available()
			 {
			 }

			 public void unavailable()
			 {
				  _notified.set( true );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void givenAccessGuardWithConditionWhenShutdownThenInstantlyDenyAccess()
		 public virtual void GivenAccessGuardWithConditionWhenShutdownThenInstantlyDenyAccess()
		 {
			  // Given
			  Clock clock = Mockito.mock( typeof( Clock ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.availability.DatabaseAvailabilityGuard databaseAvailabilityGuard = getDatabaseAvailabilityGuard(clock, org.neo4j.logging.NullLog.getInstance());
			  DatabaseAvailabilityGuard databaseAvailabilityGuard = GetDatabaseAvailabilityGuard( clock, NullLog.Instance );
			  databaseAvailabilityGuard.Require( _requirement_1 );

			  // When
			  databaseAvailabilityGuard.Shutdown();

			  // Then
			  assertFalse( databaseAvailabilityGuard.IsAvailable( 1000 ) );
			  verifyZeroInteractions( clock );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExplainWhoIsBlockingAccess()
		 public virtual void ShouldExplainWhoIsBlockingAccess()
		 {
			  // Given
			  Log log = mock( typeof( Log ) );
			  DatabaseAvailabilityGuard databaseAvailabilityGuard = GetDatabaseAvailabilityGuard( _clock, log );

			  // When
			  databaseAvailabilityGuard.Require( _requirement_1 );
			  databaseAvailabilityGuard.Require( _requirement_2 );

			  // Then
			  assertThat( databaseAvailabilityGuard.DescribeWhoIsBlocking(), equalTo("2 reasons for blocking: Requirement 1, Requirement 2.") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExplainBlockersOnCheckAvailable() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldExplainBlockersOnCheckAvailable()
		 {
			  // GIVEN
			  DatabaseAvailabilityGuard databaseAvailabilityGuard = GetDatabaseAvailabilityGuard( Clocks.systemClock(), Instance );
			  // At this point it should be available
			  databaseAvailabilityGuard.CheckAvailable();

			  // WHEN
			  databaseAvailabilityGuard.Require( _requirement_1 );

			  // THEN
			  try
			  {
					databaseAvailabilityGuard.CheckAvailable();
					fail( "Should not be available" );
			  }
			  catch ( UnavailableException e )
			  {
					assertThat( e.Message, containsString( _requirement_1() ) );
			  }
		 }

		 private static void VerifyLogging( Log log, VerificationMode mode )
		 {
			  verify( log, mode ).info( anyString(), Mockito.anyVararg<object[]>() );
		 }

		 private static DatabaseAvailabilityGuard GetDatabaseAvailabilityGuard( Clock clock, Log log )
		 {
			  return new DatabaseAvailabilityGuard( DEFAULT_DATABASE_NAME, clock, log );
		 }
	}

}