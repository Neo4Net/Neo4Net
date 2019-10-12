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
namespace Org.Neo4j.Kernel.impl.locking
{

	using AcquireLockTimeoutException = Org.Neo4j.Storageengine.Api.@lock.AcquireLockTimeoutException;
	using LockTracer = Org.Neo4j.Storageengine.Api.@lock.LockTracer;
	using ResourceLocker = Org.Neo4j.Storageengine.Api.@lock.ResourceLocker;
	using ResourceType = Org.Neo4j.Storageengine.Api.@lock.ResourceType;
	using Org.Neo4j.Storageengine.Api.@lock;

	/// <summary>
	/// API for managing locks.
	/// 
	/// Locks are grabbed by clients (which generally map to a transaction, but can be any actor in the system).
	/// 
	/// ## Upgrading and downgrading
	/// 
	/// Shared locks allow upgrading, and exclusive locks allow downgrading. To upgrade a held shared lock to an exclusive
	/// lock, simply acquire an exclusive lock and then release the shared lock. The acquire call will block other clients
	/// from acquiring shared or exclusive locks, and then wait until all other holders of the shared locks have released
	/// before returning.
	/// 
	/// Downgrading a held exclusive lock is done by acquiring a shared lock, and then releasing the exclusive lock.
	/// 
	/// ## Lock stacking
	/// 
	/// Each call to acquire a lock must be accompanied by a call to release that same lock. A user can call acquire on the
	/// same lock multiple times, thus requiring an equal number of calls to release those locks.
	/// </summary>
	public interface Locks
	{

		 /// <summary>
		 /// For introspection and debugging. </summary>

		 /// <summary>
		 /// A client is able to grab and release locks, and compete with other clients for them. This can be re-used until
		 /// you call <seealso cref="Locks.Client.close()"/>.
		 /// </summary>
		 /// <exception cref="IllegalStateException"> if this instance has been closed, i.e has had <seealso cref="close()"/> called. </exception>
		 Locks_Client NewClient();

		 /// <summary>
		 /// Visit all held locks. </summary>
		 void Accept( Locks_Visitor visitor );

		 void Close();
	}

	 public interface Locks_Visitor
	 {
		  /// <summary>
		  /// Visit the description of a lock held by at least one client. </summary>
		  void Visit( ResourceType resourceType, long resourceId, string description, long estimatedWaitTime, long lockIdentityHashCode );
	 }

	 public interface Locks_Client : ResourceLocker, AutoCloseable
	 {
		  /// <summary>
		  /// Represents the fact that no lock session is used because no locks are taken.
		  /// </summary>

		  /// <summary>
		  /// Can be grabbed when there are no locks or only share locks on a resource. If the lock cannot be acquired,
		  /// behavior is specified by the <seealso cref="WaitStrategy"/> for the given <seealso cref="ResourceType"/>.
		  /// </summary>
		  /// <param name="tracer"> a tracer for listening on lock events. </param>
		  /// <param name="resourceType"> type or resource(s) to lock. </param>
		  /// <param name="resourceIds"> id(s) of resources to lock. Multiple ids should be ordered consistently by all callers </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void acquireShared(org.neo4j.storageengine.api.lock.LockTracer tracer, org.neo4j.storageengine.api.lock.ResourceType resourceType, long... resourceIds) throws org.neo4j.storageengine.api.lock.AcquireLockTimeoutException;
		  void AcquireShared( LockTracer tracer, ResourceType resourceType, params long[] resourceIds );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void acquireExclusive(org.neo4j.storageengine.api.lock.LockTracer tracer, org.neo4j.storageengine.api.lock.ResourceType resourceType, long... resourceIds) throws org.neo4j.storageengine.api.lock.AcquireLockTimeoutException;
		  void AcquireExclusive( LockTracer tracer, ResourceType resourceType, params long[] resourceIds );

		  /// <summary>
		  /// Try grabbing exclusive lock, not waiting and returning a boolean indicating if we got the lock. </summary>
		  bool TryExclusiveLock( ResourceType resourceType, long resourceId );

		  /// <summary>
		  /// Try grabbing shared lock, not waiting and returning a boolean indicating if we got the lock. </summary>
		  bool TrySharedLock( ResourceType resourceType, long resourceId );

		  bool ReEnterShared( ResourceType resourceType, long resourceId );

		  bool ReEnterExclusive( ResourceType resourceType, long resourceId );

		  /// <summary>
		  /// Release a set of shared locks </summary>
		  void ReleaseShared( ResourceType resourceType, params long[] resourceIds );

		  /// <summary>
		  /// Release a set of exclusive locks </summary>
		  void ReleaseExclusive( ResourceType resourceType, params long[] resourceIds );

		  /// <summary>
		  /// Start preparing this transaction for committing. In two-phase locking palace, we will in principle no longer
		  /// be acquiring any new locks - though we still allow it because it is useful in certain technical situations -
		  /// but when we are ready, we will start releasing them. This also means that we will no longer accept being
		  /// <seealso cref="stop() asynchronously stopped"/>. From this point on, only the commit process can decide if the
		  /// transaction lives or dies, and in either case, the lock client will end up releasing all locks via the
		  /// <seealso cref="close()"/> method.
		  /// </summary>
		  void Prepare();

		  /// <summary>
		  /// Stop all active lock waiters and release them.
		  /// All new attempts to acquire any locks will cause exceptions.
		  /// This client can and should only be <seealso cref="close() closed"/> afterwards.
		  /// If this client has been <seealso cref="prepare() prepared"/>, then all currently acquired locks will remain held,
		  /// otherwise they will be released immediately.
		  /// </summary>
		  void Stop();

		  /// <summary>
		  /// Releases all locks, using the client after calling this is undefined. </summary>
		  void Close();

		  /// <summary>
		  /// For slave transactions, this tracks an identifier for the lock session running on the master </summary>
		  int LockSessionId { get; }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.stream.Stream<? extends ActiveLock> activeLocks();
		  Stream<ActiveLock> ActiveLocks();

		  long ActiveLockCount();
	 }

	 public static class Locks_Client_Fields
	 {
		  public const int NO_LOCK_SESSION_ID = -1;
	 }

}