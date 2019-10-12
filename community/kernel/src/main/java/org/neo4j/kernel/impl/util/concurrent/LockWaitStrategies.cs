using System.Threading;

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
namespace Org.Neo4j.Kernel.impl.util.concurrent
{
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;
	using AcquireLockTimeoutException = Org.Neo4j.Storageengine.Api.@lock.AcquireLockTimeoutException;
	using Org.Neo4j.Storageengine.Api.@lock;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.exceptions.Status_Transaction.Interrupted;

	public enum LockWaitStrategies
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: SPIN { @Override public void apply(long iteration) throws org.neo4j.storageengine.api.lock.AcquireLockTimeoutException { } },
		 SPIN { public void apply( long iteration ) throws AcquireLockTimeoutException { } },
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: YIELD { @Override public void apply(long iteration) throws org.neo4j.storageengine.api.lock.AcquireLockTimeoutException { Thread.yield(); } },
		 YIELD
		 {
			 public void apply( long iteration ) throws AcquireLockTimeoutException { Thread.yield(); }
		 },
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: INCREMENTAL_BACKOFF { private static final int spinIterations = 1000; private static final long multiplyUntilIteration = spinIterations + 2; @Override public void apply(long iteration) throws org.neo4j.storageengine.api.lock.AcquireLockTimeoutException { if(iteration < spinIterations) { SPIN.apply(iteration); return; } try { if(iteration < multiplyUntilIteration) { Thread.sleep(0, 1 << (iteration - spinIterations)); } else { Thread.sleep(0, 500); } } catch(InterruptedException e) { Thread.interrupted(); throw new org.neo4j.storageengine.api.lock.AcquireLockTimeoutException(e, "Interrupted while waiting.", Interrupted); } } },
		 INCREMENTAL_BACKOFF
		 {
			 private static final int spinIterations = 1000; private static final long multiplyUntilIteration = spinIterations + 2; public void apply( long iteration ) throws AcquireLockTimeoutException
			 {
				 if ( iteration < spinIterations ) { SPIN.apply( iteration ); return; } try
				 {
					 if ( iteration < multiplyUntilIteration ) { Thread.Sleep( 0, 1 << ( iteration - spinIterations ) ); } else { Thread.Sleep( 0, 500 ); }
				 }
				 catch ( InterruptedException e ) { Thread.interrupted(); throw new AcquireLockTimeoutException(e, "Interrupted while waiting.", Interrupted); }
			 }
		 },
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: NO_WAIT { @Override public void apply(long iteration) throws org.neo4j.storageengine.api.lock.AcquireLockTimeoutException { throw new org.neo4j.storageengine.api.lock.AcquireLockTimeoutException("Cannot acquire lock, and refusing to wait.", org.neo4j.kernel.api.exceptions.Status_Transaction.DeadlockDetected); } }
		 NO_WAIT
		 {
			 public void apply( long iteration ) throws AcquireLockTimeoutException { throw new AcquireLockTimeoutException( "Cannot acquire lock, and refusing to wait.", Org.Neo4j.Kernel.Api.Exceptions.Status_Transaction.DeadlockDetected ); }
		 }
	}

}