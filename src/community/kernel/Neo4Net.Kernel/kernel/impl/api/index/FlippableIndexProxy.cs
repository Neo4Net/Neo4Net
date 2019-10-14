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
namespace Neo4Net.Kernel.Impl.Api.index
{

	using Neo4Net.Graphdb;
	using InternalIndexState = Neo4Net.Internal.Kernel.Api.InternalIndexState;
	using IndexNotFoundKernelException = Neo4Net.Internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using ExceptionDuringFlipKernelException = Neo4Net.Kernel.Api.Exceptions.index.ExceptionDuringFlipKernelException;
	using FlipFailedKernelException = Neo4Net.Kernel.Api.Exceptions.index.FlipFailedKernelException;
	using IndexActivationFailedKernelException = Neo4Net.Kernel.Api.Exceptions.index.IndexActivationFailedKernelException;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using IndexPopulationFailedKernelException = Neo4Net.Kernel.Api.Exceptions.index.IndexPopulationFailedKernelException;
	using IndexProxyAlreadyClosedKernelException = Neo4Net.Kernel.Api.Exceptions.index.IndexProxyAlreadyClosedKernelException;
	using UniquePropertyValueValidationException = Neo4Net.Kernel.Api.Exceptions.schema.UniquePropertyValueValidationException;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using DelegatingIndexUpdater = Neo4Net.Kernel.Impl.Api.index.updater.DelegatingIndexUpdater;
	using NodePropertyAccessor = Neo4Net.Storageengine.Api.NodePropertyAccessor;
	using CapableIndexDescriptor = Neo4Net.Storageengine.Api.schema.CapableIndexDescriptor;
	using IndexReader = Neo4Net.Storageengine.Api.schema.IndexReader;
	using PopulationProgress = Neo4Net.Storageengine.Api.schema.PopulationProgress;
	using Value = Neo4Net.Values.Storable.Value;

	public class FlippableIndexProxy : IndexProxy
	{
		 private volatile bool _closed;
		 private readonly ReentrantReadWriteLock @lock = new ReentrantReadWriteLock( true );
		 private volatile IndexProxyFactory _flipTarget;
		 // This variable below is volatile because it can be changed in flip or flipTo
		 // and even though it may look like acquiring the read lock, when using this variable
		 // for various things, execution flow would go through a memory barrier of some sort.
		 // But it turns out that that may not be the case. F.ex. ReentrantReadWriteLock
		 // code uses unsafe compareAndSwap that sort of circumvents an equivalent of a volatile read.
		 private volatile IndexProxy @delegate;
		 private bool _started;

		 public FlippableIndexProxy() : this(null)
		 {
		 }

		 internal FlippableIndexProxy( IndexProxy originalDelegate )
		 {
			  this.@delegate = originalDelegate;
		 }

		 public override void Start()
		 {
			  @lock.readLock().@lock();
			  try
			  {
					@delegate.Start();
					_started = true;
			  }
			  finally
			  {
					@lock.readLock().unlock();
			  }
		 }

		 public override IndexUpdater NewUpdater( IndexUpdateMode mode )
		 {
			  // Making use of reentrant locks to ensure that the delegate's constructor is called under lock protection
			  // while still retaining the lock until a call to close on the returned IndexUpdater
			  @lock.readLock().@lock();
			  try
			  {
					return new LockingIndexUpdater( this, @delegate.NewUpdater( mode ) );
			  }
			  finally
			  {
					@lock.readLock().unlock();
			  }
		 }

		 public override void Drop()
		 {
			  @lock.readLock().@lock();
			  try
			  {
					_closed = true;
					@delegate.Drop();
			  }
			  finally
			  {
					@lock.readLock().unlock();
			  }
		 }

		 /// <summary>
		 /// The {@code force()}-method is called during log rotation. At this time we do not want to wait for locks held by
		 /// <seealso cref="LockingIndexUpdater"/>. Waiting on such locks would cause a serious risk of deadlocks, since very likely
		 /// the reader we would be waiting on would be waiting on the log rotation lock held by the thread calling this
		 /// method. The reason we would wait for a read lock while trying to acquire a read lock is if there is a third
		 /// thread waiting on the write lock, probably an index populator wanting to
		 /// <seealso cref="flip(Callable, FailedIndexProxyFactory) flip the index into active state"/>.
		 /// <p/>
		 /// We avoid this deadlock situation by "barging" on the read lock, i.e. acquire it in an <i>unfair</i> way, where
		 /// we don't care about waiting threads, only about whether the exclusive lock is held or not.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void force(org.neo4j.io.pagecache.IOLimiter ioLimiter) throws java.io.IOException
		 public override void Force( IOLimiter ioLimiter )
		 {
			  Barge( @lock.readLock() ); // see javadoc of this method (above) for rationale on why we use barge(...) here
			  try
			  {
					@delegate.Force( ioLimiter );
			  }
			  finally
			  {
					@lock.readLock().unlock();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void refresh() throws java.io.IOException
		 public override void Refresh()
		 {
			  @lock.readLock();
			  try
			  {
					@delegate.Refresh();
			  }
			  finally
			  {
					@lock.readLock().unlock();
			  }
		 }

		 /// <summary>
		 /// Acquire the {@code ReadLock} in an <i>unfair</i> way, without waiting for queued up writers.
		 /// <p/>
		 /// The <seealso cref="ReentrantReadWriteLock.ReadLock.tryLock() tryLock"/>-method of the {@code ReadLock} implementation of
		 /// {@code ReentrantReadWriteLock} implements a <i>barging</i> behaviour, where if an exclusive lock is not held,
		 /// the shared lock will be acquired, even if there are other threads waiting for the lock. This behaviour is
		 /// regardless of whether the lock is fair or not.
		 /// <p/>
		 /// This allows us to avoid deadlocks where readers would wait for writers that wait for readers in critical
		 /// methods.
		 /// <p/>
		 /// The naive way to implement this method would be:
		 /// <pre><code>
		 ///     if ( !lock.tryLock() ) // try to barge
		 ///         lock.lock(); // fall back to normal blocking lock call
		 /// </code></pre>
		 /// This would however not implement the appropriate barging behaviour in a scenario like the following: Say the
		 /// exclusive lock is held, and there is a queue waiting containing first a reader and then a writer, in this case
		 /// the {@code tryLock()} method will return false. If the writer then finishes between the naive implementation
		 /// exiting {@code tryLock()} and before entering {@code lock()} the {@code barge(...)} method would now block in
		 /// the exact way we don't want it to block, with a read lock held and a writer waiting.<br/>
		 /// In order to get around this situation, the implementation of this method uses a
		 /// <seealso cref="Lock.tryLock(long, TimeUnit) timed wait"/> in a retry-loop in order to ensure that we make another
		 /// attempt to barge the lock at a later point.
		 /// <p/>
		 /// This method is written to be compatible with the signature of <seealso cref="Lock.lock()"/>, which is not interruptible,
		 /// but implemented based on the interruptible <seealso cref="Lock.tryLock(long, TimeUnit)"/>, so the implementation needs to
		 /// remember being interrupted, and reset the flag before exiting, so that later invocations of interruptible
		 /// methods detect the interruption.
		 /// </summary>
		 /// <param name="lock"> a <seealso cref="java.util.concurrent.locks.ReentrantReadWriteLock.ReadLock"/> </param>
		 private static void Barge( ReentrantReadWriteLock.ReadLock @lock )
		 {
			  bool interrupted = false;
			  // exponential retry back-off, no more than 1 second
			  for ( long timeout = 10; !@lock.tryLock(); timeout = Math.Min(1000, timeout * 2) )
			  {
					try
					{
						 if ( @lock.tryLock( timeout, TimeUnit.MILLISECONDS ) )
						 {
							  return;
						 }
					}
					// the barge()-method is uninterruptable, but implemented based on the interruptible tryLock()-method
					catch ( InterruptedException )
					{
						 Thread.interrupted(); // ensure the interrupt flag is cleared
						 interrupted = true; // remember to set interrupt flag before we exit
					}
			  }
			  if ( interrupted )
			  {
					Thread.CurrentThread.Interrupt(); // reset the interrupt flag
			  }
		 }

		 public virtual CapableIndexDescriptor Descriptor
		 {
			 get
			 {
				  @lock.readLock().@lock();
				  try
				  {
						return @delegate.Descriptor;
				  }
				  finally
				  {
						@lock.readLock().unlock();
				  }
			 }
		 }

		 public virtual InternalIndexState State
		 {
			 get
			 {
				  @lock.readLock().@lock();
				  try
				  {
						return @delegate.State;
				  }
				  finally
				  {
						@lock.readLock().unlock();
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  @lock.readLock().@lock();
			  try
			  {
					_closed = true;
					@delegate.Close();
			  }
			  finally
			  {
					@lock.readLock().unlock();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.storageengine.api.schema.IndexReader newReader() throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 public override IndexReader NewReader()
		 {
			  @lock.readLock().@lock();
			  try
			  {
					return @delegate.NewReader();
			  }
			  finally
			  {
					@lock.readLock().unlock();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean awaitStoreScanCompleted(long time, java.util.concurrent.TimeUnit unit) throws org.neo4j.kernel.api.exceptions.index.IndexPopulationFailedKernelException, InterruptedException
		 public override bool AwaitStoreScanCompleted( long time, TimeUnit unit )
		 {
			  IndexProxy proxy;
			  @lock.readLock().@lock();
			  proxy = @delegate;
			  @lock.readLock().unlock();
			  if ( _closed )
			  {
					return false;
			  }
			  bool stillGoing = proxy.AwaitStoreScanCompleted( time, unit );
			  if ( !stillGoing )
			  {
					// The waiting has ended. However we're not done because say that the delegate typically is a populating proxy, when the wait is over
					// the populating proxy flips into something else, and if that is a failed proxy then that failure should propagate out from this call.
					@lock.readLock().@lock();
					proxy = @delegate;
					@lock.readLock().unlock();
					proxy.AwaitStoreScanCompleted( time, unit );
			  }
			  return stillGoing;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void activate() throws org.neo4j.kernel.api.exceptions.index.IndexActivationFailedKernelException
		 public override void Activate()
		 {
			  // use write lock, since activate() might call flip*() which acquires a write lock itself.
			  @lock.writeLock().@lock();
			  try
			  {
					@delegate.Activate();
			  }
			  finally
			  {
					@lock.writeLock().unlock();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void validate() throws org.neo4j.kernel.api.exceptions.index.IndexPopulationFailedKernelException, org.neo4j.kernel.api.exceptions.schema.UniquePropertyValueValidationException
		 public override void Validate()
		 {
			  @lock.readLock().@lock();
			  try
			  {
					@delegate.Validate();
			  }
			  finally
			  {
					@lock.readLock().unlock();
			  }
		 }

		 public override void ValidateBeforeCommit( Value[] tuple )
		 {
			  @lock.readLock().@lock();
			  try
			  {
					@delegate.ValidateBeforeCommit( tuple );
			  }
			  finally
			  {
					@lock.readLock().unlock();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.graphdb.ResourceIterator<java.io.File> snapshotFiles() throws java.io.IOException
		 public override ResourceIterator<File> SnapshotFiles()
		 {
			  @lock.readLock().@lock();
			  try
			  {
					return @delegate.SnapshotFiles();
			  }
			  finally
			  {
					@lock.readLock().unlock();
			  }
		 }

		 public override IDictionary<string, Value> IndexConfig()
		 {
			  @lock.readLock().@lock();
			  try
			  {
					return @delegate.IndexConfig();
			  }
			  finally
			  {
					@lock.readLock().unlock();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public IndexPopulationFailure getPopulationFailure() throws IllegalStateException
		 public virtual IndexPopulationFailure PopulationFailure
		 {
			 get
			 {
				  @lock.readLock().@lock();
				  try
				  {
						return @delegate.PopulationFailure;
				  }
				  finally
				  {
						@lock.readLock().unlock();
				  }
			 }
		 }

		 public virtual PopulationProgress IndexPopulationProgress
		 {
			 get
			 {
				  @lock.readLock().@lock();
				  try
				  {
						return @delegate.IndexPopulationProgress;
				  }
				  finally
				  {
						@lock.readLock().unlock();
				  }
			 }
		 }

		 internal virtual IndexProxyFactory FlipTarget
		 {
			 set
			 {
				  @lock.writeLock().@lock();
				  try
				  {
						this._flipTarget = value;
				  }
				  finally
				  {
						@lock.writeLock().unlock();
				  }
			 }
		 }

		 internal virtual void FlipTo( IndexProxy targetDelegate )
		 {
			  @lock.writeLock().@lock();
			  try
			  {
					this.@delegate = targetDelegate;
			  }
			  finally
			  {
					@lock.writeLock().unlock();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void flip(java.util.concurrent.Callable<bool> actionDuringFlip, FailedIndexProxyFactory failureDelegate) throws org.neo4j.kernel.api.exceptions.index.FlipFailedKernelException
		 public virtual void Flip( Callable<bool> actionDuringFlip, FailedIndexProxyFactory failureDelegate )
		 {
			  @lock.writeLock().@lock();
			  try
			  {
					AssertOpen();
					try
					{
						 if ( actionDuringFlip.call() )
						 {
							  this.@delegate = _flipTarget.create();
							  if ( _started )
							  {
									this.@delegate.Start();
							  }
						 }
					}
					catch ( Exception e )
					{
						 this.@delegate = failureDelegate.Create( e );
						 throw new ExceptionDuringFlipKernelException( e );
					}
			  }
			  finally
			  {
					@lock.writeLock().unlock();
			  }
		 }

		 public override string ToString()
		 {
			  return this.GetType().Name + " -> " + @delegate + "[target:" + _flipTarget + "]";
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertOpen() throws org.neo4j.kernel.api.exceptions.index.IndexProxyAlreadyClosedKernelException
		 private void AssertOpen()
		 {
			  if ( _closed )
			  {
					throw new IndexProxyAlreadyClosedKernelException( this.GetType() );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void verifyDeferredConstraints(org.neo4j.storageengine.api.NodePropertyAccessor accessor) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException, java.io.IOException
		 public override void VerifyDeferredConstraints( NodePropertyAccessor accessor )
		 {
			  @lock.readLock().@lock();
			  try
			  {
					@delegate.VerifyDeferredConstraints( accessor );
			  }
			  finally
			  {
					@lock.readLock().unlock();
			  }
		 }

		 private class LockingIndexUpdater : DelegatingIndexUpdater
		 {
			 private readonly FlippableIndexProxy _outerInstance;

			  internal LockingIndexUpdater( FlippableIndexProxy outerInstance, IndexUpdater @delegate ) : base( @delegate )
			  {
				  this._outerInstance = outerInstance;
					outerInstance.@lock.readLock().@lock();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
			  public override void Close()
			  {
					try
					{
						 Delegate.close();
					}
					finally
					{
						 outerInstance.@lock.readLock().unlock();
					}
			  }
		 }
	}

}