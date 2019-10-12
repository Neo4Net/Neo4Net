using System;
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
namespace Neo4Net.Kernel.impl.transaction.log.checkpoint
{

	using Neo4Net.Function;
	using Resource = Neo4Net.Graphdb.Resource;

	/// <summary>
	/// Mutex between <seealso cref="storeCopy(ThrowingAction) store-copy"/> and <seealso cref="checkPoint() check-point"/>.
	/// This to prevent those two running concurrently.
	/// <para>
	/// Normally a store-copy implies first doing a check-point and so this relationships is somewhat intricate.
	/// In addition to having <seealso cref="storeCopy(ThrowingAction)"/> as the "read lock" and <seealso cref="checkPoint()"/> as the
	/// "write lock", <seealso cref="storeCopy(ThrowingAction)"/> also accepts a code snippet to run before first concurrent
	/// store-copy grabs the lock, a snippet which can include a check-point.
	/// 
	/// <pre>
	///                                  <-WAIT--------------------|-CHECKPOINT--------->
	///                                  |                                              |
	/// |----------|-----|---------------|---|-----------|---------|-------------|------|--------------------------|-> TIME
	///            |     |                   |           |         |             |                                 |
	///            |     |                   <-----STORE-|-COPY---->             <-WAIT-|-CHECKPOINT-|-STORE-COPY-->
	///            |     |                               |
	///            |     <-WAIT--|-STORE-COPY------------>
	///            |                                   |
	///            <-CHECKPOINT--|-STORE-COPY---------->
	/// </pre>
	/// 
	/// In the image above there are three "events":
	/// <ol>
	/// <li>Store-copy 1, where there are three concurrent store-copies going on.
	/// Only the first one performs check-point</li>
	/// <li>External check-point, which waits for the ongoing store-copies to complete and then performs it</li>
	/// <li>Store-copy 2, which waits for the external check-point to complete and then starts its own
	/// check-point, which is part of the store-copy algorithm to then perform the store-copy.</li>
	/// </ol>
	/// 
	/// Status changes are made in synchronized as opposed to atomic CAS operations, since this results
	/// in simpler code and since this mutex is normally called a couple of times per hour it's not an issue.
	/// </para>
	/// </summary>
	public class StoreCopyCheckPointMutex
	{
		 /// <summary>
		 /// Main lock. Read-lock is for <seealso cref="storeCopy(ThrowingAction)"/> and write-lock is for <seealso cref="checkPoint()"/>.
		 /// </summary>
		 private readonly ReadWriteLock @lock;

		 /// <summary>
		 /// Number of currently ongoing store-copy requests.
		 /// </summary>
		 private int _storeCopyCount;

		 /// <summary>
		 /// Whether or not the first (of the concurrently ongoing store-copy requests) has had its "before"
		 /// action completed. The other store-copy requests will wait for this flag to be {@code true}.
		 /// </summary>
		 private volatile bool _storeCopyActionCompleted;

		 /// <summary>
		 /// Error which may have happened during first concurrent store-copy request. Made available to
		 /// the other concurrent store-copy requests so that they can fail instead of waiting forever.
		 /// </summary>
		 private volatile Exception _storeCopyActionError;

		 public StoreCopyCheckPointMutex() : this(new ReentrantReadWriteLock(true))
		 {
		 }

		 public StoreCopyCheckPointMutex( ReadWriteLock @lock )
		 {
			  this.@lock = @lock;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.graphdb.Resource storeCopy(org.neo4j.function.ThrowingAction<java.io.IOException> beforeFirstConcurrentStoreCopy) throws java.io.IOException
		 public virtual Resource StoreCopy( ThrowingAction<IOException> beforeFirstConcurrentStoreCopy )
		 {
			  Lock readLock = @lock.readLock();
			  bool firstConcurrentRead = IncrementCount() == 0;
			  bool success = false;
			  try
			  {
					if ( firstConcurrentRead )
					{
						 try
						 {
							  beforeFirstConcurrentStoreCopy.Apply();
						 }
						 catch ( IOException e )
						 {
							  _storeCopyActionError = e;
							  throw e;
						 }
						 catch ( Exception e )
						 {
							  _storeCopyActionError = e;
							  throw new IOException( e );
						 }
						 _storeCopyActionCompleted = true;
					}
					else
					{
						 // Wait for the "before" first store copy to complete
						 WaitForFirstStoreCopyActionToComplete();
					}
					success = true;
			  }
			  finally
			  {
					if ( success )
					{
						 readLock.@lock();
					}
					else
					{
						 DecrementCount();
					}
			  }

			  return () =>
			  {
				// Decrement concurrent store-copy count
				DecrementCount();
				readLock.unlock();
			  };
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void waitForFirstStoreCopyActionToComplete() throws java.io.IOException
		 private void WaitForFirstStoreCopyActionToComplete()
		 {
			  while ( !_storeCopyActionCompleted )
			  {
					if ( _storeCopyActionError != null )
					{
						 throw new IOException( "Co-operative action before store-copy failed", _storeCopyActionError );
					}
					ParkAWhile();
			  }
		 }

		 private void DecrementCount()
		 {
			 lock ( this )
			 {
				  _storeCopyCount--;
				  if ( _storeCopyCount == 0 )
				  {
						// If I'm the last one then also clear the other status fields so that a clean new session
						// can begin on the next store-copy request
						Clear();
				  }
			 }
		 }

		 private void Clear()
		 {
			  _storeCopyActionCompleted = false;
			  _storeCopyActionError = null;
		 }

		 private int IncrementCount()
		 {
			 lock ( this )
			 {
				  return _storeCopyCount++;
			 }
		 }

		 private static void ParkAWhile()
		 {
			  LockSupport.parkNanos( MILLISECONDS.toNanos( 100 ) );
		 }

		 public virtual Resource TryCheckPoint()
		 {
			  Lock writeLock = @lock.writeLock();
			  return writeLock.tryLock() ? writeLock.unlock : null;
		 }

		 public virtual Resource TryCheckPoint( System.Func<bool> timeoutPredicate )
		 {
			  Lock writeLock = @lock.writeLock();

			  try
			  {
					while ( !writeLock.tryLock( 100, MILLISECONDS ) )
					{
						 if ( timeoutPredicate() )
						 {
							  return null;
						 }
					}
			  }
			  catch ( InterruptedException )
			  {
					Thread.CurrentThread.Interrupt();
					return null;
			  }
			  return writeLock.unlock;
		 }

		 public virtual Resource CheckPoint()
		 {
			  Lock writeLock = @lock.writeLock();
			  writeLock.@lock();
			  return writeLock.unlock;
		 }
	}

}