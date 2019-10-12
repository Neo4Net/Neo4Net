using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Kernel.Lifecycle
{
	using Test = org.junit.jupiter.api.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;

	internal class LifeSupportTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testOkLifecycle() throws LifecycleException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestOkLifecycle()
		 {
			  LifeSupport lifeSupport = NewLifeSupport();

			  LifecycleMock instance1 = new LifecycleMock( null, null, null, null );
			  lifeSupport.Add( instance1 );
			  LifecycleMock instance2 = new LifecycleMock( null, null, null, null );
			  lifeSupport.Add( instance2 );
			  LifecycleMock instance3 = new LifecycleMock( null, null, null, null );
			  lifeSupport.Add( instance3 );

			  lifeSupport.Init();
			  assertEquals( LifecycleStatus.Stopped, lifeSupport.Status );
			  assertEquals( LifecycleStatus.Stopped, instance1.Status );
			  assertEquals( LifecycleStatus.Stopped, instance2.Status );
			  assertEquals( LifecycleStatus.Stopped, instance3.Status );

			  lifeSupport.Start();
			  assertEquals( LifecycleStatus.Started, lifeSupport.Status );
			  assertEquals( LifecycleStatus.Started, instance1.Status );
			  assertEquals( LifecycleStatus.Started, instance2.Status );
			  assertEquals( LifecycleStatus.Started, instance3.Status );

			  lifeSupport.Stop();
			  assertEquals( LifecycleStatus.Stopped, lifeSupport.Status );
			  assertEquals( LifecycleStatus.Stopped, instance1.Status );
			  assertEquals( LifecycleStatus.Stopped, instance2.Status );
			  assertEquals( LifecycleStatus.Stopped, instance3.Status );

			  lifeSupport.Start();
			  assertEquals( LifecycleStatus.Started, lifeSupport.Status );
			  assertEquals( LifecycleStatus.Started, instance1.Status );
			  assertEquals( LifecycleStatus.Started, instance2.Status );
			  assertEquals( LifecycleStatus.Started, instance3.Status );

			  lifeSupport.Shutdown();
			  assertEquals( LifecycleStatus.Shutdown, lifeSupport.Status );
			  assertEquals( LifecycleStatus.Shutdown, instance1.Status );
			  assertEquals( LifecycleStatus.Shutdown, instance2.Status );
			  assertEquals( LifecycleStatus.Shutdown, instance3.Status );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testFailingInit()
		 internal virtual void TestFailingInit()
		 {
			  LifeSupport lifeSupport = NewLifeSupport();

			  LifecycleMock instance1 = new LifecycleMock( null, null, null, null );
			  lifeSupport.Add( instance1 );
			  Exception initThrowable = new Exception();
			  LifecycleMock instance2 = new LifecycleMock( initThrowable, null, null, null );
			  lifeSupport.Add( instance2 );
			  LifecycleMock instance3 = new LifecycleMock( null, null, null, null );
			  lifeSupport.Add( instance3 );

			  try
			  {
					lifeSupport.Init();
					fail( "Failure was expected" );
			  }
			  catch ( LifecycleException throwable )
			  {
					assertEquals( initThrowable, throwable.InnerException );
			  }
			  assertEquals( LifecycleStatus.Shutdown, lifeSupport.Status );
			  assertEquals( LifecycleStatus.Shutdown, instance1.Status );
			  assertEquals( LifecycleStatus.Shutdown, instance2.Status );
			  assertEquals( LifecycleStatus.None, instance3.Status );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testFailingStart()
		 internal virtual void TestFailingStart()
		 {
			  LifeSupport lifeSupport = NewLifeSupport();

			  LifecycleMock instance1 = new LifecycleMock( null, null, null, null );
			  lifeSupport.Add( instance1 );
			  Exception startThrowable = new Exception();
			  LifecycleMock instance2 = new LifecycleMock( null, startThrowable, null, null );
			  lifeSupport.Add( instance2 );
			  LifecycleMock instance3 = new LifecycleMock( null, null, null, null );
			  lifeSupport.Add( instance3 );

			  try
			  {
					lifeSupport.Start();
					fail( "Failure was expected" );
			  }
			  catch ( LifecycleException throwable )
			  {
					assertEquals( startThrowable, throwable.InnerException );
			  }
			  assertEquals( LifecycleStatus.Stopped, lifeSupport.Status );
			  assertEquals( LifecycleStatus.Stopped, instance1.Status );
			  assertEquals( LifecycleStatus.Stopped, instance2.Status );
			  assertEquals( LifecycleStatus.Stopped, instance3.Status );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testFailingStartAndFailingStop()
		 internal virtual void TestFailingStartAndFailingStop()
		 {
			  LifeSupport lifeSupport = NewLifeSupport();

			  Exception stopThrowable = new Exception();
			  LifecycleMock instance1 = new LifecycleMock( null, null, stopThrowable, null );
			  lifeSupport.Add( instance1 );
			  Exception startThrowable = new Exception();
			  LifecycleMock instance2 = new LifecycleMock( null, startThrowable, null, null );
			  lifeSupport.Add( instance2 );
			  LifecycleMock instance3 = new LifecycleMock( null, null, null, null );
			  lifeSupport.Add( instance3 );

			  try
			  {
					lifeSupport.Start();
					fail( "Failure was expected" );
			  }
			  catch ( LifecycleException throwable )
			  {
					assertEquals( startThrowable, throwable.InnerException );
					assertEquals( 1, throwable.Suppressed.length );
					assertThat( throwable.Suppressed[0], instanceOf( typeof( LifecycleException ) ) );
					assertEquals( stopThrowable, throwable.Suppressed[0].Cause );
			  }

			  assertEquals( LifecycleStatus.Stopped, lifeSupport.Status );
			  assertEquals( LifecycleStatus.Stopped, instance1.Status );
			  assertEquals( LifecycleStatus.Stopped, instance2.Status );
			  assertEquals( LifecycleStatus.Stopped, instance3.Status );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testFailingStop() throws LifecycleException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestFailingStop()
		 {
			  LifeSupport lifeSupport = NewLifeSupport();

			  LifecycleMock instance1 = new LifecycleMock( null, null, null, null );
			  lifeSupport.Add( instance1 );
			  Exception stopThrowable = new Exception();
			  LifecycleMock instance2 = new LifecycleMock( null, null, stopThrowable, null );
			  lifeSupport.Add( instance2 );
			  LifecycleMock instance3 = new LifecycleMock( null, null, null, null );
			  lifeSupport.Add( instance3 );

			  lifeSupport.Start();

			  try
			  {
					lifeSupport.Stop();
					fail( "Failure was expected" );
			  }
			  catch ( LifecycleException throwable )
			  {
					assertEquals( stopThrowable, throwable.InnerException );
			  }
			  assertEquals( LifecycleStatus.Stopped, lifeSupport.Status );
			  assertEquals( LifecycleStatus.Stopped, instance1.Status );
			  assertEquals( LifecycleStatus.Stopped, instance2.Status );
			  assertEquals( LifecycleStatus.Stopped, instance3.Status );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testFailingShutdown() throws LifecycleException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestFailingShutdown()
		 {
			  LifeSupport lifeSupport = NewLifeSupport();

			  LifecycleMock instance1 = new LifecycleMock( null, null, null, null );
			  lifeSupport.Add( instance1 );
			  Exception shutdownThrowable = new Exception();
			  LifecycleMock instance2 = new LifecycleMock( null, null, null, shutdownThrowable );
			  lifeSupport.Add( instance2 );
			  LifecycleMock instance3 = new LifecycleMock( null, null, null, null );
			  lifeSupport.Add( instance3 );

			  lifeSupport.Start();

			  try
			  {
					lifeSupport.Shutdown();
					fail( "Failure was expected" );
			  }
			  catch ( LifecycleException throwable )
			  {
					assertEquals( shutdownThrowable, throwable.InnerException );
			  }
			  assertEquals( LifecycleStatus.Shutdown, lifeSupport.Status );
			  assertEquals( LifecycleStatus.Shutdown, instance1.Status );
			  assertEquals( LifecycleStatus.Shutdown, instance2.Status );
			  assertEquals( LifecycleStatus.Shutdown, instance3.Status );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testAddInstanceWhenInitInitsInstance() throws LifecycleException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestAddInstanceWhenInitInitsInstance()
		 {
			  LifeSupport support = NewLifeSupport();

			  support.Init();

			  LifecycleMock instance1 = new LifecycleMock( null, null, null, null );

			  support.Add( instance1 );

			  assertEquals( LifecycleStatus.Stopped, instance1.Status );

			  assertEquals( LifecycleStatus.None, instance1.Transitions[0] );
			  assertEquals( 2, instance1.Transitions.Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testAddInstanceWhenStartedStartsInstance() throws LifecycleException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestAddInstanceWhenStartedStartsInstance()
		 {
			  LifeSupport support = NewLifeSupport();

			  support.Init();
			  support.Start();

			  LifecycleMock instance1 = new LifecycleMock( null, null, null, null );

			  support.Add( instance1 );

			  assertEquals( LifecycleStatus.Started, instance1.Status );

			  assertEquals( LifecycleStatus.None, instance1.Transitions[0] );
			  assertEquals( LifecycleStatus.Stopped, instance1.Transitions[1] );

			  assertEquals( 3, instance1.Transitions.Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testAddInstanceWhenStoppedInitsInstance() throws LifecycleException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestAddInstanceWhenStoppedInitsInstance()
		 {
			  LifeSupport support = NewLifeSupport();

			  support.Init();
			  support.Start();
			  support.Stop();

			  LifecycleMock instance1 = new LifecycleMock( null, null, null, null );

			  support.Add( instance1 );

			  assertEquals( LifecycleStatus.Stopped, instance1.Status );

			  assertEquals( LifecycleStatus.None, instance1.Transitions[0] );
			  assertEquals( LifecycleStatus.Stopped, instance1.Transitions[1] );

			  assertEquals( 2, instance1.Transitions.Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testAddInstanceWhenShutdownDoesNotAffectInstance() throws LifecycleException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestAddInstanceWhenShutdownDoesNotAffectInstance()
		 {
			  LifeSupport support = NewLifeSupport();

			  support.Init();
			  support.Start();
			  support.Stop();
			  support.Shutdown();

			  LifecycleMock instance1 = new LifecycleMock( null, null, null, null );

			  support.Add( instance1 );

			  assertEquals( LifecycleStatus.None, instance1.Status );

			  assertEquals( 1, instance1.Transitions.Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testInitFailsShutdownWorks() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestInitFailsShutdownWorks()
		 {
			  //given
			  LifeSupport lifeSupport = NewLifeSupport();
			  Lifecycle lifecycle = mock( typeof( Lifecycle ) );
			  Exception runtimeException = new Exception();

			  //when
			  doThrow( runtimeException ).when( lifecycle ).init();
			  lifeSupport.Add( lifecycle );
			  try
			  {
					lifeSupport.Init();
					fail( "Expected exception" );
			  }
			  catch ( LifecycleException e )
			  {
					assertEquals( runtimeException, e.InnerException );
					assertEquals( 0, e.Suppressed.length );
			  }

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testInitFailsShutdownFails() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestInitFailsShutdownFails()
		 {
			  LifeSupport lifeSupport = NewLifeSupport();
			  Lifecycle lifecycle1 = mock( typeof( Lifecycle ) );
			  Lifecycle lifecycle2 = mock( typeof( Lifecycle ) );
			  Exception initRuntimeException = new Exception();
			  Exception shutdownRuntimeException = new Exception();
			  doThrow( initRuntimeException ).when( lifecycle2 ).init();
			  doThrow( shutdownRuntimeException ).when( lifecycle1 ).shutdown();
			  lifeSupport.Add( lifecycle1 );
			  lifeSupport.Add( lifecycle2 );
			  try
			  {
					lifeSupport.Init();
					fail( "Expected exception" );
			  }
			  catch ( LifecycleException e )
			  {
					assertEquals( initRuntimeException, e.InnerException );
					assertEquals( 1, e.Suppressed.length );
					assertEquals( shutdownRuntimeException, e.Suppressed[0].Cause );
					assertThat( e.Suppressed[0], instanceOf( typeof( LifecycleException ) ) );
			  }

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testStartFailsStopWorks() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestStartFailsStopWorks()
		 {
			  LifeSupport lifeSupport = NewLifeSupport();
			  Lifecycle lifecycle = mock( typeof( Lifecycle ) );
			  Exception runtimeException = new Exception();
			  doThrow( runtimeException ).when( lifecycle ).start();
			  lifeSupport.Add( lifecycle );
			  try
			  {
					lifeSupport.Start();
					fail( "Expected exception" );
			  }
			  catch ( LifecycleException e )
			  {
					assertEquals( runtimeException, e.InnerException );
					assertEquals( 0, e.Suppressed.length );
			  }

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testStartFailsStopFails() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestStartFailsStopFails()
		 {
			  LifeSupport lifeSupport = NewLifeSupport();
			  Lifecycle lifecycle1 = mock( typeof( Lifecycle ) );
			  Lifecycle lifecycle2 = mock( typeof( Lifecycle ) );
			  Exception startRuntimeException = new Exception();
			  Exception stopRuntimeException = new Exception();
			  doThrow( startRuntimeException ).when( lifecycle2 ).start();
			  doThrow( stopRuntimeException ).when( lifecycle1 ).stop();
			  lifeSupport.Add( lifecycle1 );
			  lifeSupport.Add( lifecycle2 );
			  try
			  {
					lifeSupport.Start();
					fail( "Expected exception" );
			  }
			  catch ( LifecycleException e )
			  {
					assertEquals( startRuntimeException, e.InnerException );
					assertEquals( 1, e.Suppressed.length );
					assertEquals( stopRuntimeException, e.Suppressed[0].Cause );
					assertThat( e.Suppressed[0], instanceOf( typeof( LifecycleException ) ) );
			  }

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testStopFailsShutdownWorks() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestStopFailsShutdownWorks()
		 {
			  LifeSupport lifeSupport = NewLifeSupport();
			  Lifecycle lifecycle = mock( typeof( Lifecycle ) );
			  Exception runtimeException = new Exception();
			  doThrow( runtimeException ).when( lifecycle ).stop();
			  lifeSupport.Add( lifecycle );
			  lifeSupport.Start();
			  try
			  {
					lifeSupport.Stop();
					fail( "Expected exception" );
			  }
			  catch ( LifecycleException e )
			  {
					assertEquals( runtimeException, e.InnerException );
					assertEquals( 0, e.Suppressed.length );
			  }

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testStopFailsShutdownFails() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestStopFailsShutdownFails()
		 {
			  LifeSupport lifeSupport = NewLifeSupport();
			  Lifecycle lifecycle1 = mock( typeof( Lifecycle ) );
			  Lifecycle lifecycle2 = mock( typeof( Lifecycle ) );
			  Exception stopRuntimeException = new Exception();
			  Exception shutdownRuntimeException = new Exception();
			  doThrow( stopRuntimeException ).when( lifecycle2 ).stop();
			  doThrow( shutdownRuntimeException ).when( lifecycle1 ).shutdown();
			  lifeSupport.Add( lifecycle1 );
			  lifeSupport.Add( lifecycle2 );
			  lifeSupport.Start();
			  try
			  {
					lifeSupport.Shutdown();
					fail( "Expected exception" );
			  }
			  catch ( LifecycleException e )
			  {
					assertEquals( stopRuntimeException, e.InnerException );
					assertEquals( 1, e.Suppressed.length );
					assertEquals( shutdownRuntimeException, e.Suppressed[0].Cause );
					assertThat( e.Suppressed[0], instanceOf( typeof( LifecycleException ) ) );
			  }

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void tryToStopComponentOnStartFailure() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TryToStopComponentOnStartFailure()
		 {
			  LifeSupport lifeSupport = NewLifeSupport();
			  Lifecycle component = mock( typeof( Lifecycle ) );
			  doThrow( new Exception( "Start exceptions" ) ).when( component ).start();
			  doThrow( new Exception( "Stop exceptions" ) ).when( component ).stop();
			  lifeSupport.Add( component );

			  try
			  {
					lifeSupport.Start();
					fail( "Failure was expected" );
			  }
			  catch ( Exception e )
			  {
					string message = GetExceptionStackTrace( e );
					assertThat( message, containsString( "Exception during graceful attempt to stop partially started component. " + "Please use non suppressed exception to see original component failure." ) );
			  }

			  assertEquals( LifecycleStatus.Stopped, lifeSupport.Status );
			  verify( component ).stop();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void tryToShutdownComponentOnInitFailure() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TryToShutdownComponentOnInitFailure()
		 {
			  LifeSupport lifeSupport = NewLifeSupport();
			  Lifecycle component = mock( typeof( Lifecycle ) );
			  doThrow( new Exception( "Init exceptions" ) ).when( component ).init();
			  doThrow( new Exception( "Shutdown exceptions" ) ).when( component ).shutdown();
			  lifeSupport.Add( component );

			  try
			  {
					lifeSupport.Init();
					fail( "Failure was expected" );
			  }
			  catch ( Exception e )
			  {
					string message = GetExceptionStackTrace( e );
					assertThat( message, containsString( "Exception during graceful attempt to shutdown partially initialized component. " + "Please use non suppressed exception to see original component failure." ) );
			  }

			  assertEquals( LifecycleStatus.Shutdown, lifeSupport.Status );
			  verify( component ).shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void addLastComponentBeforeChain()
		 internal virtual void AddLastComponentBeforeChain()
		 {
			  LifeSupport lifeSupport = NewLifeSupport();
			  Lifecycle lastComponent = mock( typeof( Lifecycle ) );
			  Lifecycle notLastComponent1 = mock( typeof( Lifecycle ) );
			  Lifecycle notLastComponent2 = mock( typeof( Lifecycle ) );
			  Lifecycle notLastComponent3 = mock( typeof( Lifecycle ) );
			  Lifecycle notLastComponent4 = mock( typeof( Lifecycle ) );
			  lifeSupport.Last = lastComponent;
			  lifeSupport.Add( notLastComponent1 );
			  lifeSupport.Add( notLastComponent2 );
			  lifeSupport.Add( notLastComponent3 );
			  lifeSupport.Add( notLastComponent4 );

			  lifeSupport.Start();

			  IList<Lifecycle> lifecycleInstances = lifeSupport.LifecycleInstances;
			  assertSame( notLastComponent1, lifecycleInstances[0] );
			  assertSame( notLastComponent2, lifecycleInstances[1] );
			  assertSame( notLastComponent3, lifecycleInstances[2] );
			  assertSame( notLastComponent4, lifecycleInstances[3] );
			  assertSame( lastComponent, lifecycleInstances[4] );
			  assertThat( lifecycleInstances, hasSize( 5 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void addLastComponentSomewhereInAChain()
		 internal virtual void AddLastComponentSomewhereInAChain()
		 {
			  LifeSupport lifeSupport = NewLifeSupport();
			  Lifecycle notLastComponent1 = mock( typeof( Lifecycle ) );
			  Lifecycle notLastComponent2 = mock( typeof( Lifecycle ) );
			  Lifecycle lastComponent = mock( typeof( Lifecycle ) );
			  Lifecycle notLastComponent3 = mock( typeof( Lifecycle ) );
			  Lifecycle notLastComponent4 = mock( typeof( Lifecycle ) );
			  lifeSupport.Add( notLastComponent1 );
			  lifeSupport.Add( notLastComponent2 );
			  lifeSupport.Last = lastComponent;
			  lifeSupport.Add( notLastComponent3 );
			  lifeSupport.Add( notLastComponent4 );

			  lifeSupport.Start();

			  IList<Lifecycle> lifecycleInstances = lifeSupport.LifecycleInstances;
			  assertSame( notLastComponent1, lifecycleInstances[0] );
			  assertSame( notLastComponent2, lifecycleInstances[1] );
			  assertSame( notLastComponent3, lifecycleInstances[2] );
			  assertSame( notLastComponent4, lifecycleInstances[3] );
			  assertSame( lastComponent, lifecycleInstances[4] );
			  assertThat( lifecycleInstances, hasSize( 5 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void addOnlyLastComponent()
		 internal virtual void AddOnlyLastComponent()
		 {
			  LifeSupport lifeSupport = NewLifeSupport();
			  Lifecycle lastComponent = mock( typeof( Lifecycle ) );
			  lifeSupport.Last = lastComponent;
			  lifeSupport.Start();
			  IList<Lifecycle> lifecycleInstances = lifeSupport.LifecycleInstances;

			  assertSame( lastComponent, lifecycleInstances[0] );
			  assertThat( lifecycleInstances, hasSize( 1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void failToAddSeveralLastComponents()
		 internal virtual void FailToAddSeveralLastComponents()
		 {
			  LifeSupport lifeSupport = NewLifeSupport();
			  Lifecycle lastComponent = mock( typeof( Lifecycle ) );
			  Lifecycle anotherLastComponent = mock( typeof( Lifecycle ) );
			  lifeSupport.Last = lastComponent;
			  assertThrows( typeof( System.InvalidOperationException ), () => lifeSupport.setLast(anotherLastComponent) );
		 }

		 internal class LifecycleMock : Lifecycle
		 {

			  internal Exception InitThrowable;
			  internal Exception StartThrowable;
			  internal Exception StopThrowable;
			  internal Exception ShutdownThrowable;

			  internal IList<LifecycleStatus> Transitions;

			  internal LifecycleMock( Exception initThrowable, Exception startThrowable, Exception stopThrowable, Exception shutdownThrowable )
			  {
					this.InitThrowable = initThrowable;
					this.StartThrowable = startThrowable;
					this.StopThrowable = stopThrowable;
					this.ShutdownThrowable = shutdownThrowable;

					Transitions = new List<LifecycleStatus>();
					Transitions.Add( LifecycleStatus.None );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void init() throws Throwable
			  public override void Init()
			  {
					if ( InitThrowable != null )
					{
						 throw InitThrowable;
					}

					Transitions.Add( LifecycleStatus.Stopped );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start() throws Throwable
			  public override void Start()
			  {
					if ( StartThrowable != null )
					{
						 throw StartThrowable;
					}

					Transitions.Add( LifecycleStatus.Started );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void stop() throws Throwable
			  public override void Stop()
			  {
					Transitions.Add( LifecycleStatus.Stopped );

					if ( StopThrowable != null )
					{
						 throw StopThrowable;
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void shutdown() throws Throwable
			  public override void Shutdown()
			  {
					Transitions.Add( LifecycleStatus.Shutdown );

					if ( ShutdownThrowable != null )
					{
						 throw ShutdownThrowable;
					}
			  }

			  internal virtual LifecycleStatus Status
			  {
				  get
				  {
						return Transitions[Transitions.Count - 1];
				  }
			  }
		 }

		 private LifeSupport NewLifeSupport()
		 {
			  return new LifeSupport();
		 }

		 private string GetExceptionStackTrace( Exception e )
		 {
			  StringWriter stringWriter = new StringWriter();
			  e.printStackTrace( new PrintWriter( stringWriter ) );
			  return stringWriter.ToString();
		 }
	}

}