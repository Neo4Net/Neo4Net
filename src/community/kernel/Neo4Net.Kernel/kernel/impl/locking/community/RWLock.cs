using System;
using System.Collections.Generic;
using System.Text;
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
namespace Neo4Net.Kernel.impl.locking.community
{

	using MathUtil = Neo4Net.Helpers.MathUtil;
	using Logger = Neo4Net.Logging.Logger;
	using LockTracer = Neo4Net.Kernel.Api.StorageEngine.@lock.LockTracer;
	using LockWaitEvent = Neo4Net.Kernel.Api.StorageEngine.@lock.LockWaitEvent;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Thread.currentThread;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Thread.interrupted;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.locking.LockType.READ;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.locking.LockType.WRITE;

	/// <summary>
	/// A read/write lock is a lock that will allow many transactions to acquire read
	/// locks as long as there is no transaction holding the write lock.
	/// <p/>
	/// When a transaction has write lock no other tx is allowed to acquire read or
	/// write lock on that resource but the tx holding the write lock. If one tx has
	/// acquired write lock and another tx needs a lock on the same resource that tx
	/// must wait. When the lock is released the other tx is notified and wakes up so
	/// it can acquire the lock.
	/// <p/>
	/// Waiting for locks may lead to a deadlock. Consider the following scenario. Tx
	/// T1 acquires write lock on resource R1. T2 acquires write lock on R2. Now T1
	/// tries to acquire read lock on R2 but has to wait since R2 is locked by T2. If
	/// T2 now tries to acquire a lock on R1 it also has to wait because R1 is locked
	/// by T1. T2 cannot wait on R1 because that would lead to a deadlock where T1
	/// and T2 waits forever.
	/// <p/>
	/// Avoiding deadlocks can be done by keeping a resource allocation graph. This
	/// class works together with the <seealso cref="RagManager"/> to make sure no deadlocks
	/// occur.
	/// <p/>
	/// Waiting transactions are put into a queue and when some tx releases the lock
	/// the queue is checked for waiting txs. This implementation tries to avoid lock
	/// starvation and increase performance since only waiting txs that can acquire
	/// the lock are notified.
	/// </summary>
	internal class RWLock
	{
		 private readonly LockResource _resource; // the resource this RWLock locks
		 private readonly LinkedList<LockRequest> _waitingThreadList = new LinkedList<LockRequest>();
		 private readonly IDictionary<object, TxLockElement> _txLockElementMap = new Dictionary<object, TxLockElement>();
		 private readonly RagManager _ragManager;
		 private readonly Clock _clock;
		 private readonly long _lockAcquisitionTimeoutMillis;

		 // access to these is guarded by synchronized blocks
		 private int _totalReadCount;
		 private int _totalWriteCount;
		 private int _marked; // synch helper in LockManager

		 internal RWLock( LockResource resource, RagManager ragManager, Clock clock, long lockAcquisitionTimeoutMillis )
		 {
			  this._resource = resource;
			  this._ragManager = ragManager;
			  this._clock = clock;
			  this._lockAcquisitionTimeoutMillis = lockAcquisitionTimeoutMillis;
		 }

		 // keeps track of a transactions read and write lock count on this RWLock
		 private class TxLockElement
		 {
			  internal readonly object Tx;

			  // access to these is guarded by synchronized blocks
			  internal int ReadCount;
			  internal int WriteCount;
			  // represent number of active request that where current TxLockElement participate in
			  // as soon as hasNoRequests return true - txLockElement can be cleaned up
			  internal int Requests;
			  // flag indicate that current TxLockElement is terminated because owning client closed
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool TerminatedConflict;

			  internal TxLockElement( object tx )
			  {
					this.Tx = tx;
			  }

			  internal virtual void IncrementRequests()
			  {
					Requests = Math.incrementExact( Requests );
			  }

			  internal virtual void DecrementRequests()
			  {
					Requests = MathUtil.decrementExactNotPastZero( Requests );
			  }

			  internal virtual bool HasNoRequests()
			  {
					return Requests == 0;
			  }

			  internal virtual bool Free
			  {
				  get
				  {
						return ReadCount == 0 && WriteCount == 0;
				  }
			  }

			  public virtual bool Terminated
			  {
				  get
				  {
						return TerminatedConflict;
				  }
				  set
				  {
						this.TerminatedConflict = value;
				  }
			  }

		 }

		 // keeps track of what type of lock a thread is waiting for
		 private class LockRequest
		 {
			  internal readonly TxLockElement Element;
			  internal readonly LockType LockType;
			  internal readonly Thread WaitingThread;
			  internal readonly long Since = DateTimeHelper.CurrentUnixTimeMillis();

			  internal LockRequest( TxLockElement element, LockType lockType, Thread thread )
			  {
					this.Element = element;
					this.LockType = lockType;
					this.WaitingThread = thread;
			  }
		 }

		 public virtual object Resource()
		 {
			  return _resource;
		 }

		 internal virtual void Mark()
		 {
			 lock ( this )
			 {
				  _marked = Math.incrementExact( _marked );
			 }
		 }

		 /// <summary>
		 /// synchronized by all caller methods </summary>
		 private void Unmark()
		 {
			  _marked = MathUtil.decrementExactNotPastZero( _marked );
		 }

		 internal virtual bool Marked
		 {
			 get
			 {
				 lock ( this )
				 {
					  return _marked > 0;
				 }
			 }
		 }

		 /// <summary>
		 /// Tries to acquire read lock for a given transaction. If
		 /// <CODE>this.writeCount</CODE> is greater than the currents tx's write
		 /// count the transaction has to wait and the <seealso cref="RagManager.checkWaitOn"/>
		 /// method is invoked for deadlock detection.
		 /// <p/>
		 /// If the lock can be acquired the lock count is updated on <CODE>this</CODE>
		 /// and the transaction lock element (tle).
		 /// Waiting for a lock can also be terminated. In that case waiting thread will be interrupted and corresponding
		 /// <seealso cref="org.Neo4Net.kernel.impl.locking.community.RWLock.TxLockElement"/> will be marked as terminated.
		 /// In that case lock will not be acquired and false will be return as result of acquisition
		 /// </summary>
		 /// <returns> true is lock was acquired, false otherwise </returns>
		 /// <exception cref="DeadlockDetectedException"> if a deadlock is detected </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: synchronized boolean acquireReadLock(org.Neo4Net.Kernel.Api.StorageEngine.lock.LockTracer tracer, Object tx) throws org.Neo4Net.kernel.DeadlockDetectedException
		 internal virtual bool AcquireReadLock( LockTracer tracer, object tx )
		 {
			 lock ( this )
			 {
				  TxLockElement tle = GetOrCreateLockElement( tx );
      
				  LockRequest lockRequest = null;
				  LockWaitEvent waitEvent = null;
				  // used to track do we need to add lock request to a waiting queue or we still have it there
				  bool addLockRequest = true;
				  try
				  {
						tle.IncrementRequests();
						Thread currentThread = currentThread();
      
						long lockAcquisitionTimeBoundary = _clock.millis() + _lockAcquisitionTimeoutMillis;
						while ( !tle.Terminated && ( _totalWriteCount > tle.WriteCount ) )
						{
							 AssertNotExpired( lockAcquisitionTimeBoundary );
							 _ragManager.checkWaitOn( this, tx );
      
							 if ( addLockRequest )
							 {
								  lockRequest = new LockRequest( tle, READ, currentThread );
								  _waitingThreadList.AddFirst( lockRequest );
							 }
      
							 if ( waitEvent == null )
							 {
								  waitEvent = tracer.WaitForLock( false, _resource.type(), _resource.resourceId() );
							 }
							 addLockRequest = WaitUninterruptedly( lockAcquisitionTimeBoundary );
							 _ragManager.stopWaitOn( this, tx );
						}
      
						if ( !tle.Terminated )
						{
							 RegisterReadLockAcquired( tx, tle );
							 return true;
						}
						else
						{
							 // in case if lock element was interrupted and it was never register before
							 // we need to clean it from lock element map
							 // if it was register before it will be cleaned up during standard lock release call
							 if ( tle.Requests == 1 && tle.Free )
							 {
								  _txLockElementMap.Remove( tx );
							 }
							 return false;
						}
				  }
				  finally
				  {
						if ( waitEvent != null )
						{
							 waitEvent.Close();
						}
						CleanupWaitingListRequests( lockRequest, tle, addLockRequest );
						// for cases when spurious wake up was the reason why we waked up, but also there
						// was an interruption as described at 17.2 just clearing interruption flag
						interrupted();
						// if deadlocked, remove marking so lock is removed when empty
						tle.DecrementRequests();
						Unmark();
				  }
			 }
		 }

		 internal virtual bool TryAcquireReadLock( object tx )
		 {
			 lock ( this )
			 {
				  TxLockElement tle = GetOrCreateLockElement( tx );
      
				  try
				  {
						if ( tle.Terminated || ( _totalWriteCount > tle.WriteCount ) )
						{
							 return false;
						}
      
						RegisterReadLockAcquired( tx, tle );
						return true;
				  }
				  finally
				  {
						// if deadlocked, remove marking so lock is removed when empty
						Unmark();
				  }
			 }
		 }

		 /// <summary>
		 /// Releases the read lock held by the provided transaction. If it is null then
		 /// an attempt to acquire the current transaction will be made. This is to
		 /// make safe calling the method from the context of an
		 /// <code>afterCompletion()</code> hook where the tx is locally stored and
		 /// not necessarily available through the tm. If there are waiting
		 /// transactions in the queue they will be interrupted if they can acquire
		 /// the lock.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: synchronized void releaseReadLock(Object tx) throws LockNotFoundException
		 internal virtual void ReleaseReadLock( object tx )
		 {
			 lock ( this )
			 {
				  TxLockElement tle = GetLockElement( tx );
      
				  if ( tle.ReadCount == 0 )
				  {
						throw new LockNotFoundException( "" + tx + " don't have readLock" );
				  }
      
				  _totalReadCount = MathUtil.decrementExactNotPastZero( _totalReadCount );
				  tle.ReadCount = MathUtil.decrementExactNotPastZero( tle.ReadCount );
				  if ( tle.Free )
				  {
						_ragManager.lockReleased( this, tx );
						if ( tle.HasNoRequests() )
						{
							 _txLockElementMap.Remove( tx );
						}
				  }
				  if ( _waitingThreadList.Count > 0 )
				  {
						LockRequest lockRequest = _waitingThreadList.Last.Value;
      
						if ( lockRequest.LockType == LockType.WRITE )
						{
							 // this one is tricky...
							 // if readCount > 0 lockRequest either have to find a waiting read lock
							 // in the queue or a waiting write lock that has all read
							 // locks, if none of these are found it means that there
							 // is a (are) thread(s) that will release read lock(s) in the
							 // near future...
							 if ( _totalReadCount == lockRequest.Element.readCount )
							 {
								  // found a write lock with all read locks
								  _waitingThreadList.RemoveLast();
								  lockRequest.WaitingThread.Interrupt();
							 }
							 else
							 {
								  IEnumerator<LockRequest> listItr = _waitingThreadList.listIterator( _waitingThreadList.LastIndexOf( lockRequest ) );
								  // hm am I doing the first all over again?
								  // think I am if cursor is at lastIndex + 0.5 oh well...
								  while ( listItr.hasPrevious() )
								  {
										lockRequest = listItr.previous();
										if ( lockRequest.LockType == LockType.WRITE && _totalReadCount == lockRequest.Element.readCount )
										{
											 // found a write lock with all read locks
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
											 listItr.remove();
											 lockRequest.WaitingThread.Interrupt();
											 break;
										}
										else if ( lockRequest.LockType == LockType.READ )
										{
											 // found a read lock, let it do the job...
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
											 listItr.remove();
											 lockRequest.WaitingThread.Interrupt();
										}
								  }
							 }
						}
						else
						{
							 // some thread may have the write lock and released a read lock
							 // if writeCount is down to zero lockRequest can interrupt the waiting
							 // read lock
							 if ( _totalWriteCount == 0 )
							 {
								  _waitingThreadList.RemoveLast();
								  lockRequest.WaitingThread.Interrupt();
							 }
						}
				  }
			 }
		 }

		 /// <summary>
		 /// Tries to acquire write lock for a given transaction. If
		 /// <CODE>this.writeCount</CODE> is greater than the currents tx's write
		 /// count or the read count is greater than the currents tx's read count the
		 /// transaction has to wait and the <seealso cref="RagManager.checkWaitOn"/> method is
		 /// invoked for deadlock detection.
		 /// <p/>
		 /// If the lock can be acquires the lock count is updated on <CODE>this</CODE>
		 /// and the transaction lock element (tle).
		 /// Waiting for a lock can also be terminated. In that case waiting thread will be interrupted and corresponding
		 /// <seealso cref="org.Neo4Net.kernel.impl.locking.community.RWLock.TxLockElement"/> will be marked as terminated.
		 /// In that case lock will not be acquired and false will be return as result of acquisition
		 /// </summary>
		 /// <returns> true is lock was acquired, false otherwise </returns>
		 /// <exception cref="DeadlockDetectedException"> if a deadlock is detected </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: synchronized boolean acquireWriteLock(org.Neo4Net.Kernel.Api.StorageEngine.lock.LockTracer tracer, Object tx) throws org.Neo4Net.kernel.DeadlockDetectedException
		 internal virtual bool AcquireWriteLock( LockTracer tracer, object tx )
		 {
			 lock ( this )
			 {
				  TxLockElement tle = GetOrCreateLockElement( tx );
      
				  LockRequest lockRequest = null;
				  LockWaitEvent waitEvent = null;
				  // used to track do we need to add lock request to a waiting queue or we still have it there
				  bool addLockRequest = true;
				  try
				  {
						tle.IncrementRequests();
						Thread currentThread = currentThread();
      
						long lockAcquisitionTimeBoundary = _clock.millis() + _lockAcquisitionTimeoutMillis;
						while ( !tle.Terminated && ( _totalWriteCount > tle.WriteCount || _totalReadCount > tle.ReadCount ) )
						{
							 AssertNotExpired( lockAcquisitionTimeBoundary );
							 _ragManager.checkWaitOn( this, tx );
      
							 if ( addLockRequest )
							 {
								  lockRequest = new LockRequest( tle, WRITE, currentThread );
								  _waitingThreadList.AddFirst( lockRequest );
							 }
      
							 if ( waitEvent == null )
							 {
								  waitEvent = tracer.WaitForLock( true, _resource.type(), _resource.resourceId() );
							 }
							 addLockRequest = WaitUninterruptedly( lockAcquisitionTimeBoundary );
							 _ragManager.stopWaitOn( this, tx );
						}
      
						if ( !tle.Terminated )
						{
							 RegisterWriteLockAcquired( tx, tle );
							 return true;
						}
						else
						{
							 // in case if lock element was interrupted and it was never register before
							 // we need to clean it from lock element map
							 // if it was register before it will be cleaned up during standard lock release call
							 if ( tle.Requests == 1 && tle.Free )
							 {
								  _txLockElementMap.Remove( tx );
							 }
							 return false;
						}
				  }
				  finally
				  {
						if ( waitEvent != null )
						{
							 waitEvent.Close();
						}
						CleanupWaitingListRequests( lockRequest, tle, addLockRequest );
						// for cases when spurious wake up was the reason why we waked up, but also there
						// was an interruption as described at 17.2 just clearing interruption flag
						interrupted();
						// if deadlocked, remove marking so lock is removed when empty
						tle.DecrementRequests();
						Unmark();
				  }
			 }
		 }

		 private bool WaitUninterruptedly( long lockAcquisitionTimeBoundary )
		 {
			  bool addLockRequest;
			  try
			  {
					if ( _lockAcquisitionTimeoutMillis > 0 )
					{
						 AssertNotExpired( lockAcquisitionTimeBoundary );
						 Monitor.Wait( this, TimeSpan.FromMilliseconds( Math.Abs( lockAcquisitionTimeBoundary - _clock.millis() ) ) );
					}
					else
					{
						 Monitor.Wait( this );
					}
					addLockRequest = false;
			  }
			  catch ( InterruptedException )
			  {
					interrupted();
					addLockRequest = true;
			  }
			  return addLockRequest;
		 }

		 // in case of spurious wake up, deadlock during spurious wake up, termination
		 // when we already have request in a queue we need to clean it up
		 private void CleanupWaitingListRequests( LockRequest lockRequest, TxLockElement lockElement, bool addLockRequest )
		 {
			  if ( lockRequest != null && ( lockElement.Terminated || !addLockRequest ) )
			  {
//JAVA TO C# CONVERTER TODO TASK: There is no .NET LinkedList equivalent to the Java 'remove' method:
					_waitingThreadList.remove( lockRequest );
			  }
		 }

		 internal virtual bool TryAcquireWriteLock( object tx )
		 {
			 lock ( this )
			 {
				  TxLockElement tle = GetOrCreateLockElement( tx );
      
				  try
				  {
						if ( tle.Terminated || ( _totalWriteCount > tle.WriteCount ) || ( _totalReadCount > tle.ReadCount ) )
						{
							 return false;
						}
      
						RegisterWriteLockAcquired( tx, tle );
						return true;
				  }
				  finally
				  {
						// if deadlocked, remove marking so lock is removed when empty
						Unmark();
				  }
			 }
		 }

		 /// <summary>
		 /// Releases the write lock held by the provided tx. If it is null then an
		 /// attempt to acquire the current transaction from the transaction manager
		 /// will be made. This is to make safe calling this method as an
		 /// <code>afterCompletion()</code> hook where the transaction context is not
		 /// necessarily available. If write count is zero and there are waiting
		 /// transactions in the queue they will be interrupted if they can acquire
		 /// the lock.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: synchronized void releaseWriteLock(Object tx) throws LockNotFoundException
		 internal virtual void ReleaseWriteLock( object tx )
		 {
			 lock ( this )
			 {
				  TxLockElement tle = GetLockElement( tx );
      
				  if ( tle.WriteCount == 0 )
				  {
						throw new LockNotFoundException( "" + tx + " don't have writeLock" );
				  }
      
				  _totalWriteCount = MathUtil.decrementExactNotPastZero( _totalWriteCount );
				  tle.WriteCount = MathUtil.decrementExactNotPastZero( tle.WriteCount );
				  if ( tle.Free )
				  {
						_ragManager.lockReleased( this, tx );
						if ( tle.HasNoRequests() )
						{
							 _txLockElementMap.Remove( tx );
						}
				  }
      
				  // the threads in the waitingList cannot be currentThread
				  // so we only have to wake other elements if writeCount is down to zero
				  // (that is: If writeCount > 0 a waiting thread in the queue cannot be
				  // the thread that holds the write locks because then it would never
				  // have been put into wait mode)
				  if ( _totalWriteCount == 0 && _waitingThreadList.Count > 0 )
				  {
						// wake elements in queue until a write lock is found or queue is
						// empty
						do
						{
							 LockRequest lockRequest = _waitingThreadList.RemoveLast();
							 lockRequest.WaitingThread.Interrupt();
							 if ( lockRequest.LockType == LockType.WRITE )
							 {
								  break;
							 }
						} while ( _waitingThreadList.Count > 0 );
				  }
			 }
		 }

		 internal virtual int WriteCount
		 {
			 get
			 {
				 lock ( this )
				 {
					  return _totalWriteCount;
				 }
			 }
		 }

		 internal virtual int ReadCount
		 {
			 get
			 {
				 lock ( this )
				 {
					  return _totalReadCount;
				 }
			 }
		 }

		 internal virtual int WaitingThreadsCount
		 {
			 get
			 {
				 lock ( this )
				 {
					  return _waitingThreadList.Count;
				 }
			 }
		 }

		 public virtual bool LogTo( Logger logger )
		 {
			 lock ( this )
			 {
				  logger.Log( "Total lock count: readCount=" + _totalReadCount + " writeCount=" + _totalWriteCount + " for " + _resource );
      
				  logger.Log( "Waiting list:" );
				  IEnumerator<LockRequest> wElements = _waitingThreadList.GetEnumerator();
				  while ( wElements.MoveNext() )
				  {
						LockRequest lockRequest = wElements.Current;
						logger.Log( "[" + lockRequest.WaitingThread + "(" + lockRequest.Element.readCount + "r," + lockRequest.Element.writeCount + "w)," + lockRequest.LockType + "]" );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						if ( wElements.hasNext() )
						{
							 logger.Log( "," );
						}
						else
						{
							 logger.Log( "" );
						}
				  }
      
				  logger.Log( "Locking transactions:" );
				  foreach ( TxLockElement tle in _txLockElementMap.Values )
				  {
						logger.Log( "" + tle.Tx + "(" + tle.ReadCount + "r," + tle.WriteCount + "w)" );
				  }
				  return true;
			 }
		 }

		 public virtual string Describe()
		 {
			 lock ( this )
			 {
				  StringBuilder sb = new StringBuilder( this.ToString() );
				  sb.Append( " Total lock count: readCount=" ).Append( _totalReadCount ).Append( " writeCount=" ).Append( _totalWriteCount ).Append( " for " ).Append( _resource ).Append( "\n" ).Append( "Waiting list:" + "\n" );
				  IEnumerator<LockRequest> wElements = _waitingThreadList.GetEnumerator();
				  while ( wElements.MoveNext() )
				  {
						LockRequest lockRequest = wElements.Current;
						sb.Append( "[" ).Append( lockRequest.WaitingThread ).Append( "(" ).Append( lockRequest.Element.readCount ).Append( "r," ).Append( lockRequest.Element.writeCount ).Append( "w)," ).Append( lockRequest.LockType ).Append( "]\n" );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						if ( wElements.hasNext() )
						{
							 sb.Append( "," );
						}
				  }
      
				  sb.Append( "Locking transactions:\n" );
				  foreach ( TxLockElement tle in _txLockElementMap.Values )
				  {
						sb.Append( tle.Tx ).Append( "(" ).Append( tle.ReadCount ).Append( "r," ).Append( tle.WriteCount ).Append( "w)\n" );
				  }
				  return sb.ToString();
			 }
		 }

		 public virtual long MaxWaitTime()
		 {
			 lock ( this )
			 {
				  long max = 0L;
				  foreach ( LockRequest thread in _waitingThreadList )
				  {
						if ( thread.Since < max )
						{
							 max = thread.Since;
						}
				  }
				  return DateTimeHelper.CurrentUnixTimeMillis() - max;
			 }
		 }

		 // for specified transaction object mark all lock elements as terminated
		 // and interrupt all waiters
		 internal virtual void TerminateLockRequestsForLockTransaction( object lockTransaction )
		 {
			 lock ( this )
			 {
				  TxLockElement lockElement = _txLockElementMap[lockTransaction];
				  if ( lockElement != null && !lockElement.Terminated )
				  {
						lockElement.Terminated = true;
						foreach ( LockRequest lockRequest in _waitingThreadList )
						{
							 if ( lockRequest.Element.tx.Equals( lockTransaction ) )
							 {
								  lockRequest.WaitingThread.Interrupt();
							 }
						}
				  }
			 }
		 }

		 public override string ToString()
		 {
			  return "RWLock[" + _resource + ", hash=" + GetHashCode() + "]";
		 }

		 private void RegisterReadLockAcquired( object tx, TxLockElement tle )
		 {
			  RegisterLockAcquired( tx, tle );
			  _totalReadCount = Math.incrementExact( _totalReadCount );
			  tle.ReadCount = Math.incrementExact( tle.ReadCount );
		 }

		 private void RegisterWriteLockAcquired( object tx, TxLockElement tle )
		 {
			  RegisterLockAcquired( tx, tle );
			  _totalWriteCount = Math.incrementExact( _totalWriteCount );
			  tle.WriteCount = Math.incrementExact( tle.WriteCount );
		 }

		 private void RegisterLockAcquired( object tx, TxLockElement tle )
		 {
			  if ( tle.Free )
			  {
					_ragManager.lockAcquired( this, tx );
			  }
		 }

		 private TxLockElement GetLockElement( object tx )
		 {
			  TxLockElement tle = _txLockElementMap[tx];
			  if ( tle == null )
			  {
					throw new LockNotFoundException( "No transaction lock element found for " + tx );
			  }
			  return tle;
		 }

		 private void AssertTransaction( object tx )
		 {
			  if ( tx == null )
			  {
					throw new System.ArgumentException();
			  }
		 }

		 private TxLockElement GetOrCreateLockElement( object tx )
		 {
			  AssertTransaction( tx );
			  TxLockElement tle = _txLockElementMap[tx];
			  if ( tle == null )
			  {
					_txLockElementMap[tx] = tle = new TxLockElement( tx );
			  }
			  return tle;
		 }

		 private void AssertNotExpired( long timeBoundary )
		 {
			  if ( _lockAcquisitionTimeoutMillis > 0 )
			  {
					if ( timeBoundary < _clock.millis() )
					{
						 throw new LockAcquisitionTimeoutException( _resource.type(), _resource.resourceId(), _lockAcquisitionTimeoutMillis );
					}
			  }
		 }

		 internal virtual object TxLockElementCount
		 {
			 get
			 {
				 lock ( this )
				 {
					  return _txLockElementMap.Count;
				 }
			 }
		 }
	}

}