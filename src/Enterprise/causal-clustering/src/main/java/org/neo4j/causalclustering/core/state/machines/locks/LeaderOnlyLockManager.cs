using System.Threading;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.causalclustering.core.state.machines.locks
{

	using LeaderLocator = Neo4Net.causalclustering.core.consensus.LeaderLocator;
	using NoLeaderFoundException = Neo4Net.causalclustering.core.consensus.NoLeaderFoundException;
	using ReplicationFailureException = Neo4Net.causalclustering.core.replication.ReplicationFailureException;
	using Replicator = Neo4Net.causalclustering.core.replication.Replicator;
	using ReplicatedTransactionStateMachine = Neo4Net.causalclustering.core.state.machines.tx.ReplicatedTransactionStateMachine;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using ActiveLock = Neo4Net.Kernel.impl.locking.ActiveLock;
	using Locks = Neo4Net.Kernel.impl.locking.Locks;
	using AcquireLockTimeoutException = Neo4Net.Storageengine.Api.@lock.AcquireLockTimeoutException;
	using LockTracer = Neo4Net.Storageengine.Api.@lock.LockTracer;
	using ResourceType = Neo4Net.Storageengine.Api.@lock.ResourceType;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.exceptions.Status_Cluster.NoLeaderAvailable;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.exceptions.Status_Cluster.NotALeader;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.exceptions.Status_Cluster.ReplicationFailure;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.exceptions.Status_Transaction.Interrupted;

	/// <summary>
	/// Each member of the cluster uses its own <seealso cref="LeaderOnlyLockManager"/> which wraps a local <seealso cref="Locks"/> manager.
	/// The validity of local lock managers is synchronized by using a token which gets requested by each server as necessary
	/// and if the request is granted then the associated id can be used to identify a unique lock session in the cluster.
	/// <p/>
	/// The fundamental strategy is to only allow locks on the leader. This has the benefit of minimizing the synchronization
	/// to only concern the single token but it also means that non-leaders should not even attempt to request the token or
	/// significant churn of this single resource will lead to a high level of aborted transactions.
	/// <p/>
	/// The token requests carry a candidate id and they get ordered with respect to the transactions in the consensus
	/// machinery.
	/// The latest request which gets accepted (see <seealso cref="ReplicatedTransactionStateMachine"/>) defines the currently valid
	/// lock session id in this ordering. Each transaction that uses locking gets marked with a lock session id that was
	/// valid
	/// at the time of acquiring it, but by the time a transaction commits it might no longer be valid, which in such case
	/// would lead to the transaction being rejected and failed.
	/// <p/>
	/// The <seealso cref="ReplicatedLockTokenStateMachine"/> handles the token requests and considers only one to be valid at a time.
	/// Meanwhile, <seealso cref="ReplicatedTransactionStateMachine"/> rejects any transactions that get committed under an
	/// invalid token.
	/// </summary>

	// TODO: Fix lock exception usage when lock exception hierarchy has been fixed.
	public class LeaderOnlyLockManager : Locks
	{
		 public const string LOCK_NOT_ON_LEADER_ERROR_MESSAGE = "Should only attempt to take locks when leader.";

		 private readonly MemberId _myself;

		 private readonly Replicator _replicator;
		 private readonly LeaderLocator _leaderLocator;
		 private readonly Locks _localLocks;
		 private readonly ReplicatedLockTokenStateMachine _lockTokenStateMachine;

		 public LeaderOnlyLockManager( MemberId myself, Replicator replicator, LeaderLocator leaderLocator, Locks localLocks, ReplicatedLockTokenStateMachine lockTokenStateMachine )
		 {
			  this._myself = myself;
			  this._replicator = replicator;
			  this._leaderLocator = leaderLocator;
			  this._localLocks = localLocks;
			  this._lockTokenStateMachine = lockTokenStateMachine;
		 }

		 public override Neo4Net.Kernel.impl.locking.Locks_Client NewClient()
		 {
			  return new LeaderOnlyLockClient( this, _localLocks.newClient() );
		 }

		 /// <summary>
		 /// Acquires a valid token id owned by us or throws.
		 /// </summary>
		 private int AcquireTokenOrThrow()
		 {
			 lock ( this )
			 {
				  LockToken currentToken = _lockTokenStateMachine.currentToken();
				  if ( _myself.Equals( currentToken.Owner() ) )
				  {
						return currentToken.Id();
				  }
      
				  /* If we are not the leader then we will not even attempt to get the token,
				     since only the leader should take locks. */
				  EnsureLeader();
      
				  ReplicatedLockTokenRequest lockTokenRequest = new ReplicatedLockTokenRequest( _myself, LockToken.nextCandidateId( currentToken.Id() ) );
      
				  Future<object> future;
				  try
				  {
						future = _replicator.replicate( lockTokenRequest, true );
				  }
				  catch ( ReplicationFailureException e )
				  {
						throw new AcquireLockTimeoutException( e, "Replication failure acquiring lock token.", ReplicationFailure );
				  }
      
				  try
				  {
						bool success = ( bool ) future.get();
						if ( success )
						{
							 return lockTokenRequest.Id();
						}
						else
						{
							 throw new AcquireLockTimeoutException( "Failed to acquire lock token. Was taken by another candidate.", NotALeader );
						}
				  }
				  catch ( ExecutionException e )
				  {
						throw new AcquireLockTimeoutException( e, "Failed to acquire lock token.", NotALeader );
				  }
				  catch ( InterruptedException e )
				  {
						Thread.CurrentThread.Interrupt();
						throw new AcquireLockTimeoutException( e, "Failed to acquire lock token.", Interrupted );
				  }
			 }
		 }

		 private void EnsureLeader()
		 {
			  MemberId leader;

			  try
			  {
					leader = _leaderLocator.Leader;
			  }
			  catch ( NoLeaderFoundException e )
			  {
					throw new AcquireLockTimeoutException( e, "Could not acquire lock token.", NoLeaderAvailable );
			  }

			  if ( !leader.Equals( _myself ) )
			  {
					throw new AcquireLockTimeoutException( LOCK_NOT_ON_LEADER_ERROR_MESSAGE, NotALeader );
			  }
		 }

		 public override void Accept( Neo4Net.Kernel.impl.locking.Locks_Visitor visitor )
		 {
			  _localLocks.accept( visitor );
		 }

		 public override void Close()
		 {
			  _localLocks.close();
		 }

		 /// <summary>
		 /// The LeaderOnlyLockClient delegates to a local lock client for taking locks, but makes
		 /// sure that it holds the cluster locking token before actually taking locks. If the token
		 /// is lost during a locking session then a transaction will either fail on a subsequent
		 /// local locking operation or during commit time.
		 /// </summary>
		 private class LeaderOnlyLockClient : Neo4Net.Kernel.impl.locking.Locks_Client
		 {
			 private readonly LeaderOnlyLockManager _outerInstance;

			  internal readonly Neo4Net.Kernel.impl.locking.Locks_Client LocalClient;
			  internal int LockTokenId = LockToken_Fields.INVALID_LOCK_TOKEN_ID;

			  internal LeaderOnlyLockClient( LeaderOnlyLockManager outerInstance, Neo4Net.Kernel.impl.locking.Locks_Client localClient )
			  {
				  this._outerInstance = outerInstance;
					this.LocalClient = localClient;
			  }

			  /// <summary>
			  /// This ensures that a valid token was held at some point in time. It throws an
			  /// exception if it was held but was later lost or never could be taken to
			  /// begin with.
			  /// </summary>
			  internal virtual void EnsureHoldingToken()
			  {
					if ( LockTokenId == LockToken_Fields.INVALID_LOCK_TOKEN_ID )
					{
						 LockTokenId = outerInstance.acquireTokenOrThrow();
					}
					else if ( LockTokenId != outerInstance.lockTokenStateMachine.CurrentToken().id() )
					{
						 throw new AcquireLockTimeoutException( "Local instance lost lock token.", NotALeader );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void acquireShared(org.neo4j.storageengine.api.lock.LockTracer tracer, org.neo4j.storageengine.api.lock.ResourceType resourceType, long... resourceId) throws org.neo4j.storageengine.api.lock.AcquireLockTimeoutException
			  public override void AcquireShared( LockTracer tracer, ResourceType resourceType, params long[] resourceId )
			  {
					LocalClient.acquireShared( tracer, resourceType, resourceId );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void acquireExclusive(org.neo4j.storageengine.api.lock.LockTracer tracer, org.neo4j.storageengine.api.lock.ResourceType resourceType, long... resourceId) throws org.neo4j.storageengine.api.lock.AcquireLockTimeoutException
			  public override void AcquireExclusive( LockTracer tracer, ResourceType resourceType, params long[] resourceId )
			  {
					EnsureHoldingToken();
					LocalClient.acquireExclusive( tracer, resourceType, resourceId );
			  }

			  public override bool TryExclusiveLock( ResourceType resourceType, long resourceId )
			  {
					EnsureHoldingToken();
					return LocalClient.tryExclusiveLock( resourceType, resourceId );
			  }

			  public override bool TrySharedLock( ResourceType resourceType, long resourceId )
			  {
					return LocalClient.trySharedLock( resourceType, resourceId );
			  }

			  public override bool ReEnterShared( ResourceType resourceType, long resourceId )
			  {
					return LocalClient.reEnterShared( resourceType, resourceId );
			  }

			  public override bool ReEnterExclusive( ResourceType resourceType, long resourceId )
			  {
					EnsureHoldingToken();
					return LocalClient.reEnterExclusive( resourceType, resourceId );
			  }

			  public override void ReleaseShared( ResourceType resourceType, params long[] resourceIds )
			  {
					LocalClient.releaseShared( resourceType, resourceIds );
			  }

			  public override void ReleaseExclusive( ResourceType resourceType, params long[] resourceIds )
			  {
					LocalClient.releaseExclusive( resourceType, resourceIds );
			  }

			  public override void Prepare()
			  {
					LocalClient.prepare();
			  }

			  public override void Stop()
			  {
					LocalClient.stop();
			  }

			  public override void Close()
			  {
					LocalClient.close();
			  }

			  public virtual int LockSessionId
			  {
				  get
				  {
						return LockTokenId;
				  }
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public java.util.stream.Stream<? extends org.neo4j.kernel.impl.locking.ActiveLock> activeLocks()
			  public override Stream<ActiveLock> ActiveLocks()
			  {
					return LocalClient.activeLocks();
			  }

			  public override long ActiveLockCount()
			  {
					return LocalClient.activeLockCount();
			  }
		 }
	}

}