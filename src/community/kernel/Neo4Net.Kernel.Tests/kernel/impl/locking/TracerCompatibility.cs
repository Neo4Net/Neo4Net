using System;
using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.Kernel.impl.locking
{
	using Ignore = org.junit.Ignore;
	using Test = org.junit.Test;


	using LockTracer = Neo4Net.Kernel.Api.StorageEngine.@lock.LockTracer;
	using LockWaitEvent = Neo4Net.Kernel.Api.StorageEngine.@lock.LockWaitEvent;
	using ResourceType = Neo4Net.Kernel.Api.StorageEngine.@lock.ResourceType;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.locking.ResourceTypes.NODE;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Ignore("Not a test. This is a compatibility suite, run from LockingCompatibilityTestSuite.") public class TracerCompatibility extends LockingCompatibilityTestSuite.Compatibility
	public class TracerCompatibility : LockingCompatibilityTestSuite.Compatibility
	{
		 public TracerCompatibility( LockingCompatibilityTestSuite suite ) : base( suite )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTraceWaitTimeWhenTryingToAcquireExclusiveLockAndExclusiveIsHeld() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTraceWaitTimeWhenTryingToAcquireExclusiveLockAndExclusiveIsHeld()
		 {
			  // given
			  Tracer tracerA = new Tracer();
			  Tracer tracerB = new Tracer();
			  ClientA.acquireExclusive( tracerA, NODE, 17 );

			  // when
			  Future<object> future = AcquireExclusive( ClientB, tracerB, NODE, 17 ).callAndAssertWaiting();

			  // then
			  ClientA.releaseExclusive( NODE, 17 );
			  future.get();
			  tracerA.AssertCalls( 0 );
			  tracerB.AssertCalls( 1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTraceWaitTimeWhenTryingToAcquireSharedLockAndExclusiveIsHeld() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTraceWaitTimeWhenTryingToAcquireSharedLockAndExclusiveIsHeld()
		 {
			  // given
			  Tracer tracerA = new Tracer();
			  Tracer tracerB = new Tracer();
			  ClientA.acquireExclusive( tracerA, NODE, 17 );

			  // when
			  Future<object> future = AcquireShared( ClientB, tracerB, NODE, 17 ).callAndAssertWaiting();

			  // then
			  ClientA.releaseExclusive( NODE, 17 );
			  future.get();
			  tracerA.AssertCalls( 0 );
			  tracerB.AssertCalls( 1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTraceWaitTimeWhenTryingToAcquireExclusiveLockAndSharedIsHeld() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTraceWaitTimeWhenTryingToAcquireExclusiveLockAndSharedIsHeld()
		 {
			  // given
			  Tracer tracerA = new Tracer();
			  Tracer tracerB = new Tracer();
			  ClientA.acquireShared( tracerA, NODE, 17 );

			  // when
			  Future<object> future = AcquireExclusive( ClientB, tracerB, NODE, 17 ).callAndAssertWaiting();

			  // then
			  ClientA.releaseShared( NODE, 17 );
			  future.get();
			  tracerA.AssertCalls( 0 );
			  tracerB.AssertCalls( 1 );
		 }

		 internal class Tracer : LockTracer, LockWaitEvent
		 {
			  internal int Done;
			  internal readonly IList<StackTraceElement[]> WaitCalls = new List<StackTraceElement[]>();

			  public override LockWaitEvent WaitForLock( bool exclusive, ResourceType resourceType, params long[] resourceIds )
			  {
					WaitCalls.Add( Thread.CurrentThread.StackTrace );
					return this;
			  }

			  public override void Close()
			  {
					Done++;
			  }

			  internal virtual void AssertCalls( int expected )
			  {
					if ( WaitCalls.Count != Done )
					{
						 throw WithCallTraces( new AssertionError( "Should complete waiting as many times as started." ) );
					}
					if ( Done != expected )
					{
						 throw WithCallTraces( new AssertionError( format( "Expected %d calls, but got %d", expected, Done ) ) );
					}
			  }

			  internal virtual EX WithCallTraces<EX>( EX failure ) where EX : Exception
			  {
					foreach ( StackTraceElement[] waitCall in WaitCalls )
					{
						 Exception call = new Exception( "Wait called" );
						 call.StackTrace = waitCall;
						 failure.addSuppressed( call );
					}
					return failure;
			  }
		 }
	}

}