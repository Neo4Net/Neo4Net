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
namespace Neo4Net.Kernel.impl.locking
{
	using After = org.junit.After;
	using Ignore = org.junit.Ignore;
	using Test = org.junit.Test;


	using Neo4Net.Helpers.Collections;
	using LockTracer = Neo4Net.Storageengine.Api.@lock.LockTracer;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
	using static Neo4Net.Kernel.impl.locking.Locks_Client;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.locking.ResourceTypes.NODE;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Ignore("Not a test. This is a compatibility suite, run from LockingCompatibilityTestSuite.") public class DeadlockCompatibility extends LockingCompatibilityTestSuite.Compatibility
	public class DeadlockCompatibility : LockingCompatibilityTestSuite.Compatibility
	{
		 public DeadlockCompatibility( LockingCompatibilityTestSuite suite ) : base( suite )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void shutdown()
		 public virtual void Shutdown()
		 {
			  ThreadA.interrupt();
			  ThreadB.interrupt();
			  ThreadC.interrupt();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectTwoClientExclusiveDeadlock()
		 public virtual void ShouldDetectTwoClientExclusiveDeadlock()
		 {
			  AssertDetectsDeadlock( AcquireExclusive( ClientA, LockTracer.NONE, NODE, 1L ), AcquireExclusive( ClientB, LockTracer.NONE, NODE, 2L ), AcquireExclusive( ClientB, LockTracer.NONE, NODE, 1L ), AcquireExclusive( ClientA, LockTracer.NONE, NODE, 2L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectThreeClientExclusiveDeadlock()
		 public virtual void ShouldDetectThreeClientExclusiveDeadlock()
		 {
			  AssertDetectsDeadlock( AcquireExclusive( ClientA, LockTracer.NONE, NODE, 1L ), AcquireExclusive( ClientB, LockTracer.NONE, NODE, 2L ), AcquireExclusive( ClientC, LockTracer.NONE, NODE, 3L ), AcquireExclusive( ClientB, LockTracer.NONE, NODE, 1L ), AcquireExclusive( ClientC, LockTracer.NONE, NODE, 2L ), AcquireExclusive( ClientA, LockTracer.NONE, NODE, 3L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectMixedExclusiveAndSharedDeadlock()
		 public virtual void ShouldDetectMixedExclusiveAndSharedDeadlock()
		 {
			  AssertDetectsDeadlock( AcquireShared( ClientA, LockTracer.NONE, NODE, 1L ), AcquireExclusive( ClientB, LockTracer.NONE, NODE, 2L ), AcquireExclusive( ClientB, LockTracer.NONE, NODE, 1L ), AcquireShared( ClientA, LockTracer.NONE, NODE, 2L ) );
		 }

		 private void AssertDetectsDeadlock( params LockCommand[] commands )
		 {
			  IList<Pair<Client, Future<object>>> calls = new List<Pair<Client, Future<object>>>();
			  foreach ( LockCommand command in commands )
			  {
					calls.Add( Pair.of( command.Client(), command.Call() ) );
			  }

			  long timeout = DateTimeHelper.CurrentUnixTimeMillis() + (1000 * 10);
			  while ( DateTimeHelper.CurrentUnixTimeMillis() < timeout )
			  {
					foreach ( Pair<Client, Future<object>> call in calls )
					{
						 try
						 {
							  call.Other().get(1, TimeUnit.MILLISECONDS);
						 }
						 catch ( ExecutionException e )
						 {
							  if ( e.InnerException is DeadlockDetectedException )
							  {
									return;
							  }
							  else
							  {
									throw new Exception( e );
							  }
						 }
						 catch ( Exception e ) when ( e is InterruptedException || e is TimeoutException )
						 {
							  // Fine, we're just looking for deadlocks, clients may still be waiting for things
						 }
					}
			  }

			  throw new AssertionError( "Failed to detect deadlock. Expected lock manager to detect deadlock, " + "but none of the clients reported any deadlocks." );
		 }
	}

}