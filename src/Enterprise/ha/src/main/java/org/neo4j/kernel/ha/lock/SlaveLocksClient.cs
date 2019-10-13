using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.Kernel.ha.@lock
{
	using LongList = org.eclipse.collections.api.list.primitive.LongList;
	using MutableLongList = org.eclipse.collections.api.list.primitive.MutableLongList;
	using LongArrayList = org.eclipse.collections.impl.list.mutable.primitive.LongArrayList;


	using PrimitiveLongCollections = Neo4Net.Collections.PrimitiveLongCollections;
	using ComException = Neo4Net.com.ComException;
	using RequestContext = Neo4Net.com.RequestContext;
	using Neo4Net.com;
	using TransientDatabaseFailureException = Neo4Net.Graphdb.TransientDatabaseFailureException;
	using AvailabilityGuard = Neo4Net.Kernel.availability.AvailabilityGuard;
	using UnavailableException = Neo4Net.Kernel.availability.UnavailableException;
	using RequestContextFactory = Neo4Net.Kernel.ha.com.RequestContextFactory;
	using Master = Neo4Net.Kernel.ha.com.master.Master;
	using ActiveLock = Neo4Net.Kernel.impl.locking.ActiveLock;
	using LockClientStoppedException = Neo4Net.Kernel.impl.locking.LockClientStoppedException;
	using Locks = Neo4Net.Kernel.impl.locking.Locks;
	using ResourceTypes = Neo4Net.Kernel.impl.locking.ResourceTypes;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using AcquireLockTimeoutException = Neo4Net.Storageengine.Api.@lock.AcquireLockTimeoutException;
	using LockTracer = Neo4Net.Storageengine.Api.@lock.LockTracer;
	using LockWaitEvent = Neo4Net.Storageengine.Api.@lock.LockWaitEvent;
	using ResourceType = Neo4Net.Storageengine.Api.@lock.ResourceType;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.locking.LockType.READ;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.locking.LockType.WRITE;

	/// <summary>
	/// The slave locks client is responsible for managing locks on behalf of some actor on a slave machine. An actor
	/// could be a transaction or some other job that runs in the database.
	/// <p/>
	/// The client maintains a local "real" lock client, backed by some regular Locks implementation, but it also coordinates
	/// with the master for certain types of locks. If you grab a lock on a node, for instance, this class will grab a
	/// cluster-global lock by talking to the master machine, and then grab that same lock locally before returning.
	/// </summary>
	internal class SlaveLocksClient : Neo4Net.Kernel.impl.locking.Locks_Client
	{
		 private readonly Master _master;
		 private readonly Neo4Net.Kernel.impl.locking.Locks_Client _client;
		 private readonly Locks _localLockManager;
		 private readonly RequestContextFactory _requestContextFactory;
		 private readonly AvailabilityGuard _availabilityGuard;

		 // Using atomic ints to avoid creating garbage through boxing.
		 private readonly Log _log;
		 private bool _initialized;
		 private volatile bool _stopped;

		 internal SlaveLocksClient( Master master, Neo4Net.Kernel.impl.locking.Locks_Client local, Locks localLockManager, RequestContextFactory requestContextFactory, AvailabilityGuard availabilityGuard, LogProvider logProvider )
		 {
			  this._master = master;
			  this._client = local;
			  this._localLockManager = localLockManager;
			  this._requestContextFactory = requestContextFactory;
			  this._availabilityGuard = availabilityGuard;
			  this._log = logProvider.getLog( this.GetType() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void acquireShared(org.neo4j.storageengine.api.lock.LockTracer tracer, org.neo4j.storageengine.api.lock.ResourceType resourceType, long... resourceIds) throws org.neo4j.storageengine.api.lock.AcquireLockTimeoutException
		 public override void AcquireShared( LockTracer tracer, ResourceType resourceType, params long[] resourceIds )
		 {
			  AssertNotStopped();

			  long[] newResourceIds = FirstTimeSharedLocks( resourceType, resourceIds );
			  if ( newResourceIds.Length > 0 )
			  {
					try
					{
						 AcquireSharedOnMasterFiltered( tracer, resourceType, newResourceIds );
					}
					catch ( Exception failure )
					{
						 if ( resourceIds != newResourceIds )
						 {
							  ReleaseShared( resourceType, resourceIds, newResourceIds );
						 }
						 throw failure;
					}
					foreach ( long resourceId in newResourceIds )
					{
						 if ( !_client.trySharedLock( resourceType, resourceId ) )
						 {
							  throw new LocalDeadlockDetectedException( _client, _localLockManager, resourceType, resourceId, READ );
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void acquireExclusive(org.neo4j.storageengine.api.lock.LockTracer tracer, org.neo4j.storageengine.api.lock.ResourceType resourceType, long... resourceIds) throws org.neo4j.storageengine.api.lock.AcquireLockTimeoutException
		 public override void AcquireExclusive( LockTracer tracer, ResourceType resourceType, params long[] resourceIds )
		 {
			  AssertNotStopped();

			  long[] newResourceIds = FirstTimeExclusiveLocks( resourceType, resourceIds );
			  if ( newResourceIds.Length > 0 )
			  {
					try
					{
							using ( LockWaitEvent @event = tracer.WaitForLock( true, resourceType, newResourceIds ) )
							{
							 AcquireExclusiveOnMaster( resourceType, newResourceIds );
							}
					}
					catch ( Exception failure )
					{
						 if ( resourceIds != newResourceIds )
						 {
							  ReleaseExclusive( resourceType, resourceIds, newResourceIds );
						 }
						 throw failure;
					}
					foreach ( long resourceId in newResourceIds )
					{
						 if ( !_client.tryExclusiveLock( resourceType, resourceId ) )
						 {
							  throw new LocalDeadlockDetectedException( _client, _localLockManager, resourceType, resourceId, WRITE );
						 }
					}
			  }
		 }

		 public override bool TryExclusiveLock( ResourceType resourceType, long resourceId )
		 {
			  throw NewUnsupportedDirectTryLockUsageException();
		 }

		 public override bool TrySharedLock( ResourceType resourceType, long resourceId )
		 {
			  throw NewUnsupportedDirectTryLockUsageException();
		 }

		 public override bool ReEnterShared( ResourceType resourceType, long resourceId )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override bool ReEnterExclusive( ResourceType resourceType, long resourceId )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void ReleaseShared( ResourceType resourceType, params long[] resourceIds )
		 {
			  AssertNotStopped();

			  _client.releaseShared( resourceType, resourceIds );
		 }

		 public override void ReleaseExclusive( ResourceType resourceType, params long[] resourceIds )
		 {
			  AssertNotStopped();

			  _client.releaseExclusive( resourceType, resourceIds );
		 }

		 public override void Prepare()
		 {
			  _client.prepare();
		 }

		 public override void Stop()
		 {
			  _client.stop();
			  StopLockSessionOnMaster();
			  _stopped = true;
		 }

		 public override void Close()
		 {
			  _client.close();
			  if ( _initialized )
			  {
					if ( !_stopped )
					{
						 CloseLockSessionOnMaster();
						 _stopped = true;
					}
					_initialized = false;
			  }
		 }

		 public virtual int LockSessionId
		 {
			 get
			 {
				  AssertNotStopped();
				  return _initialized ? _client.LockSessionId : -1;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public java.util.stream.Stream<? extends org.neo4j.kernel.impl.locking.ActiveLock> activeLocks()
		 public override Stream<ActiveLock> ActiveLocks()
		 {
			  return _client.activeLocks();
		 }

		 public override long ActiveLockCount()
		 {
			  return _client.activeLockCount();
		 }

		 /// <summary>
		 /// In order to prevent various indexes collisions on master during transaction commit that originate on one of the
		 /// slaves we need to grab same locks on <seealso cref="ResourceTypes.LABEL"/> and <seealso cref="ResourceTypes.RELATIONSHIP_TYPE"/>
		 /// that
		 /// where obtained on origin. To be able to do that and also prevent shared locks to be propagates to master in cases
		 /// of
		 /// read only transactions we need to postpone obtaining them till we know that we participating in a
		 /// transaction that performs modifications.
		 /// </summary>
		 /// <param name="tracer"> lock tracer </param>
		 internal virtual void AcquireDeferredSharedLocks( LockTracer tracer )
		 {
			  AssertNotStopped();
			  IDictionary<ResourceType, MutableLongList> deferredLocksMap = new Dictionary<ResourceType, MutableLongList>();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<? extends org.neo4j.kernel.impl.locking.ActiveLock> activeLocks = client.activeLocks().filter(activeLock -> org.neo4j.kernel.impl.locking.ActiveLock_Fields.SHARED_MODE.equals(activeLock.mode())).filter(this::isLabelOrRelationshipType).collect(java.util.stream.Collectors.toList());
			  IList<ActiveLock> activeLocks = _client.activeLocks().filter(activeLock => Neo4Net.Kernel.impl.locking.ActiveLock_Fields.SHARED_MODE.Equals(activeLock.mode())).filter(this.isLabelOrRelationshipType).collect(Collectors.toList());
			  foreach ( ActiveLock activeLock in activeLocks )
			  {
					System.Func<ResourceType, MutableLongList> listCreator = resourceType => new LongArrayList();
					deferredLocksMap.computeIfAbsent( activeLock.ResourceType(), listCreator ).add(activeLock.ResourceId());
			  }
			  deferredLocksMap.forEach( ( type, ids ) => lockResourcesOnMaster( tracer, type, ids ) );
		 }

		 private void LockResourcesOnMaster( LockTracer tracer, ResourceType type, LongList ids )
		 {
			  long[] resourceIds = ids.toArray();
			  using ( LockWaitEvent @event = tracer.WaitForLock( false, type, resourceIds ) )
			  {
					AcquireSharedOnMaster( type, resourceIds );
			  }
		 }

		 private bool IsLabelOrRelationshipType( ActiveLock activeLock )
		 {
			  return ( activeLock.ResourceType() == ResourceTypes.LABEL ) || (activeLock.ResourceType() == ResourceTypes.RELATIONSHIP_TYPE);
		 }

		 private void StopLockSessionOnMaster()
		 {
			  try
			  {
					EndLockSessionOnMaster( false );
			  }
			  catch ( Exception t )
			  {
					_log.warn( "Unable to stop lock session on master", t );
			  }
		 }

		 private void CloseLockSessionOnMaster()
		 {
			  EndLockSessionOnMaster( true );
		 }

		 private void EndLockSessionOnMaster( bool success )
		 {
			  try
			  {
					  using ( Response<Void> ignored = _master.endLockSession( NewRequestContextFor( _client ), success ) )
					  {
						// Lock session is closed on master at this point
					  }
			  }
			  catch ( ComException e )
			  {
					throw new DistributedLockFailureException( "Failed to end the lock session on the master (which implies releasing all held locks)", _master, e );
			  }
		 }

		 private long[] FirstTimeSharedLocks( ResourceType resourceType, long[] resourceIds )
		 {
			  int cursor = 0;
			  for ( int i = 0; i < resourceIds.Length; i++ )
			  {
					if ( !_client.reEnterShared( resourceType, resourceIds[i] ) )
					{
						 resourceIds[cursor++] = resourceIds[i];
					}
			  }
			  if ( cursor == 0 )
			  {
					return PrimitiveLongCollections.EMPTY_LONG_ARRAY;
			  }
			  return cursor == resourceIds.Length ? resourceIds : Arrays.copyOf( resourceIds, cursor );
		 }

		 private long[] FirstTimeExclusiveLocks( ResourceType resourceType, long[] resourceIds )
		 {
			  int cursor = 0;
			  for ( int i = 0; i < resourceIds.Length; i++ )
			  {
					if ( !_client.reEnterExclusive( resourceType, resourceIds[i] ) )
					{
						 resourceIds[cursor++] = resourceIds[i];
					}
			  }
			  if ( cursor == 0 )
			  {
					return PrimitiveLongCollections.EMPTY_LONG_ARRAY;
			  }
			  return cursor == resourceIds.Length ? resourceIds : Arrays.copyOf( resourceIds, cursor );
		 }

		 private void ReleaseShared( ResourceType resourceType, long[] resourceIds, long[] excludedIds )
		 {
			  for ( int i = 0, j = 0; i < resourceIds.Length; i++ )
			  {
					if ( resourceIds[i] == excludedIds[j] )
					{
						 j++;
					}
					else
					{
						 _client.releaseShared( resourceType, resourceIds[i] );
					}
			  }
		 }

		 private void ReleaseExclusive( ResourceType resourceType, long[] resourceIds, long[] excludedIds )
		 {
			  for ( int i = 0, j = 0; i < resourceIds.Length; i++ )
			  {
					if ( resourceIds[i] == excludedIds[j] )
					{
						 j++;
					}
					else
					{
						 _client.releaseShared( resourceType, resourceIds[i] );
					}
			  }
		 }

		 private void AcquireSharedOnMasterFiltered( LockTracer lockTracer, ResourceType resourceType, params long[] resourceIds )
		 {
			  if ( ( resourceType == ResourceTypes.INDEX_ENTRY ) || ( resourceType == ResourceTypes.LABEL ) || ( resourceType == ResourceTypes.RELATIONSHIP_TYPE ) )
			  {
					return;
			  }
			  using ( LockWaitEvent @event = lockTracer.WaitForLock( false, resourceType, resourceIds ) )
			  {
					AcquireSharedOnMaster( resourceType, resourceIds );
			  }
		 }

		 private void AcquireSharedOnMaster( ResourceType resourceType, long[] resourceIds )
		 {
			  MakeSureTxHasBeenInitialized();
			  RequestContext requestContext = NewRequestContextFor( this );
			  try
			  {
					  using ( Response<LockResult> response = _master.acquireSharedLock( requestContext, resourceType, resourceIds ) )
					  {
						ReceiveLockResponse( response );
					  }
			  }
			  catch ( ComException e )
			  {
					throw new DistributedLockFailureException( "Cannot get shared lock(s) on master", _master, e );
			  }
		 }

		 private void AcquireExclusiveOnMaster( ResourceType resourceType, params long[] resourceIds )
		 {
			  MakeSureTxHasBeenInitialized();
			  RequestContext requestContext = NewRequestContextFor( this );
			  try
			  {
					  using ( Response<LockResult> response = _master.acquireExclusiveLock( requestContext, resourceType, resourceIds ) )
					  {
						ReceiveLockResponse( response );
					  }
			  }
			  catch ( ComException e )
			  {
					throw new DistributedLockFailureException( "Cannot get exclusive lock(s) on master", _master, e );
			  }
		 }

		 private void ReceiveLockResponse( Response<LockResult> response )
		 {
			  LockResult result = response.ResponseConflict();

			  switch ( result.Status )
			  {
					case DEAD_LOCKED:
						 throw new DeadlockDetectedException( result.Message );
					case NOT_LOCKED:
						 throw new System.NotSupportedException( result.ToString() );
					case OK_LOCKED:
						 break;
					default:
						 throw new System.NotSupportedException( result.ToString() );
			  }
		 }

		 private void MakeSureTxHasBeenInitialized()
		 {
			  try
			  {
					_availabilityGuard.checkAvailable();
			  }
			  catch ( UnavailableException e )
			  {
					throw new TransientDatabaseFailureException( "Database not available", e );
			  }

			  if ( !_initialized )
			  {
					try
					{
							using ( Response<Void> ignored = _master.newLockSession( NewRequestContextFor( _client ) ) )
							{
							 // Lock session is initialized on master at this point
							}
					}
					catch ( Exception exception )
					{
						 // Temporary wrapping, we should review the exception structure of the Locks API to allow this to
						 // not use runtime exceptions here.
						 ComException e;
						 if ( exception is ComException )
						 {
							  e = ( ComException ) exception;
						 }
						 else
						 {
							  e = new ComException( exception );
						 }
						 throw new DistributedLockFailureException( "Failed to start a new lock session on master", _master, e );
					}
					_initialized = true;
			  }
		 }

		 private RequestContext NewRequestContextFor( Neo4Net.Kernel.impl.locking.Locks_Client client )
		 {
			  return _requestContextFactory.newRequestContext( client.LockSessionId );
		 }

		 private void AssertNotStopped()
		 {
			  if ( _stopped )
			  {
					throw new LockClientStoppedException( this );
			  }
		 }

		 private System.NotSupportedException NewUnsupportedDirectTryLockUsageException()
		 {
			  return new System.NotSupportedException( "Distributed tryLocks are not supported. They only work with local lock managers." );
		 }
	}

}