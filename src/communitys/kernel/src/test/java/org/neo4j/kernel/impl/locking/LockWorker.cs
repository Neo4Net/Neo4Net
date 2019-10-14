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

	using AcquireLockTimeoutException = Neo4Net.Storageengine.Api.@lock.AcquireLockTimeoutException;
	using LockTracer = Neo4Net.Storageengine.Api.@lock.LockTracer;
	using Neo4Net.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.locking.ResourceTypes.NODE;

	public class LockWorker : OtherThreadExecutor<LockWorkerState>
	{
		 public LockWorker( string name, Locks locks ) : base( name, new LockWorkerState( locks ) )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.concurrent.Future<Void> perform(AcquireLockCommand acquireLockCommand, boolean wait) throws Exception
		 private Future<Void> Perform( AcquireLockCommand acquireLockCommand, bool wait )
		 {
			  Future<Void> future = ExecuteDontWait( acquireLockCommand );
			  if ( wait )
			  {
					AwaitFuture( future );
			  }
			  else
			  {
					WaitUntilWaiting();
			  }
			  return future;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.concurrent.Future<Void> getReadLock(final long resource, final boolean wait) throws Exception
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public virtual Future<Void> GetReadLock( long resource, bool wait )
		 {
			  return perform(new AcquireLockCommandAnonymousInnerClass(this, resource, wait)
			 , wait);
		 }

		 private class AcquireLockCommandAnonymousInnerClass : AcquireLockCommand
		 {
			 private readonly LockWorker _outerInstance;

			 private long _resource;
			 private bool _wait;

			 public AcquireLockCommandAnonymousInnerClass( LockWorker outerInstance, long resource, bool wait )
			 {
				 this.outerInstance = outerInstance;
				 this._resource = resource;
				 this._wait = wait;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void acquireLock(LockWorkerState state) throws org.neo4j.storageengine.api.lock.AcquireLockTimeoutException
			 protected internal override void acquireLock( LockWorkerState state )
			 {
				  state.Doing( "+R " + _resource + ", wait:" + _wait );
				  state.Client.acquireShared( LockTracer.NONE, NODE, _resource );
				  state.Done();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.concurrent.Future<Void> getWriteLock(final long resource, final boolean wait) throws Exception
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public virtual Future<Void> GetWriteLock( long resource, bool wait )
		 {
			  return perform(new AcquireLockCommandAnonymousInnerClass2(this, resource, wait)
			 , wait);
		 }

		 private class AcquireLockCommandAnonymousInnerClass2 : AcquireLockCommand
		 {
			 private readonly LockWorker _outerInstance;

			 private long _resource;
			 private bool _wait;

			 public AcquireLockCommandAnonymousInnerClass2( LockWorker outerInstance, long resource, bool wait )
			 {
				 this.outerInstance = outerInstance;
				 this._resource = resource;
				 this._wait = wait;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void acquireLock(LockWorkerState state) throws org.neo4j.storageengine.api.lock.AcquireLockTimeoutException
			 protected internal override void acquireLock( LockWorkerState state )
			 {
				  state.Doing( "+W " + _resource + ", wait:" + _wait );
				  state.Client.acquireExclusive( LockTracer.NONE, NODE, _resource );
				  state.Done();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void releaseReadLock(final long resource) throws Exception
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public virtual void ReleaseReadLock( long resource )
		 {
			  perform(new AcquireLockCommandAnonymousInnerClass3(this, resource)
			 , true);
		 }

		 private class AcquireLockCommandAnonymousInnerClass3 : AcquireLockCommand
		 {
			 private readonly LockWorker _outerInstance;

			 private long _resource;

			 public AcquireLockCommandAnonymousInnerClass3( LockWorker outerInstance, long resource )
			 {
				 this.outerInstance = outerInstance;
				 this._resource = resource;
			 }

			 protected internal override void acquireLock( LockWorkerState state )
			 {
				  state.Doing( "-R " + _resource );
				  state.Client.releaseShared( NODE, _resource );
				  state.Done();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void releaseWriteLock(final long resource) throws Exception
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public virtual void ReleaseWriteLock( long resource )
		 {
			  perform(new AcquireLockCommandAnonymousInnerClass4(this, resource)
			 , true);
		 }

		 private class AcquireLockCommandAnonymousInnerClass4 : AcquireLockCommand
		 {
			 private readonly LockWorker _outerInstance;

			 private long _resource;

			 public AcquireLockCommandAnonymousInnerClass4( LockWorker outerInstance, long resource )
			 {
				 this.outerInstance = outerInstance;
				 this._resource = resource;
			 }

			 protected internal override void acquireLock( LockWorkerState state )
			 {
				  state.Doing( "-W " + _resource );
				  state.Client.releaseExclusive( NODE, _resource );
				  state.Done();
			 }
		 }

		 public virtual bool LastGetLockDeadLock
		 {
			 get
			 {
				  return StateConflict.deadlockOnLastWait;
			 }
		 }

		 private abstract class AcquireLockCommand : WorkerCommand<LockWorkerState, Void>
		 {
			 public abstract R DoWork( T state );
			  public override Void DoWork( LockWorkerState state )
			  {
					try
					{
						 AcquireLock( state );
						 state.DeadlockOnLastWait = false;
					}
					catch ( DeadlockDetectedException )
					{
						 state.DeadlockOnLastWait = true;
					}
					return null;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract void acquireLock(LockWorkerState state) throws org.neo4j.storageengine.api.lock.AcquireLockTimeoutException;
			  protected internal abstract void AcquireLock( LockWorkerState state );
		 }
	}

}