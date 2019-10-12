using System;
using System.Collections.Generic;
using System.Text;

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
namespace Neo4Net.Kernel.impl.enterprise.@lock.forseti
{

	using SimpleBitSet = Neo4Net.Kernel.impl.util.collection.SimpleBitSet;

	/// <summary>
	/// A Forseti share lock. Can be upgraded to an update lock, which will block new attempts at acquiring shared lock,
	/// but will allow existing holders to complete.
	/// </summary>
	internal class SharedLock : ForsetiLockManager.Lock
	{
		 /// <summary>
		 /// The update lock flag is inlined into the ref count integer, in order to allow common CAS operations across
		 /// both the update flag and the refCount simultaneously. This avoids a nasty series of race conditions, but
		 /// makes the reference counting code much mode complicated. May be worth revisiting.
		 /// </summary>
		 private static readonly int _updateLockFlag = 1 << 31;

		 /// <summary>
		 /// No more holders than this allowed, don't change this without changing the sizing of
		 /// <seealso cref="clientsHoldingThisLock"/>.
		 /// </summary>
		 private const int MAX_HOLDERS = 4680;

		 // TODO Investigate inlining and padding the refCount.
		 // TODO My gut feeling tells me there's a high chance of false-sharing
		 // TODO on these unpadded AtomicIntegers.
		 private readonly AtomicInteger _refCount = new AtomicInteger( 1 );

		 /// <summary>
		 /// When reading this, keep in mind the main design goals here: Releasing and acquiring this lock should not require
		 /// synchronization, and the lock should have as low of a memory footprint as possible.
		 /// <p/>
		 /// An array of arrays containing references to clients holding this lock. Each client can only show up once.
		 /// When the lock is created only the first reference array is created (so the last three slots in the outer array
		 /// are empty). The outer array is populated when the reference arrays are filled up, with exponentially larger
		 /// reference arrays:
		 /// <p/>
		 /// clientsHoldingThisLock[0] = 8 slots
		 /// clientsHoldingThisLock[1] = 64 slots
		 /// clientsHoldingThisLock[2] = 512 slots
		 /// clientsHoldingThisLock[3] = 4096 slots
		 /// <p/>
		 /// Allowing a total of 4680 transactions holding the same shared lock simultaneously.
		 /// <p/>
		 /// This data structure was chosen over using regular resizing of the array, because we need to be able to increase
		 /// the size of the array without requiring synchronization between threads writing to the array and threads trying
		 /// to resize (since the threads writing to the array are on one of the hottest code paths in the database).
		 /// <p/>
		 /// This data structure is, however, not optimal, since it requires O(n) at worst to search for a slot and to remove
		 /// a client from the array. This should be revisited in the future.
		 /// </summary>
		 private readonly AtomicReferenceArray<ForsetiClient>[] _clientsHoldingThisLock = new AtomicReferenceArray[4];

		 /// <summary>
		 /// Client that holds the update lock, if any. </summary>
		 private ForsetiClient _updateHolder;

		 internal SharedLock( ForsetiClient client )
		 {
			  AddClientHoldingLock( client );
		 }

		 public virtual bool Acquire( ForsetiClient client )
		 {
			  // First, bump refcount to make sure no one drops this lock on the floor
			  if ( !AcquireReference() )
			  {
					return false;
			  }

			  // Then add our wait list to the pile of things waiting in case if we are not there yet
			  // if we already waiting we will release a reference to keep counter in sync
			  if ( !ClientHoldsThisLock( client ) )
			  {
					// try to add client to a clients that holding current lock.
					return AddClientHoldingLock( client );
			  }
			  else
			  {
					ReleaseReference();
					return false;
			  }
		 }

		 public virtual bool Release( ForsetiClient client )
		 {
			  RemoveClientHoldingLock( client );
			  return ReleaseReference();
		 }

		 public override void CopyHolderWaitListsInto( SimpleBitSet waitList )
		 {
			  foreach ( AtomicReferenceArray<ForsetiClient> holders in _clientsHoldingThisLock )
			  {
					for ( int j = 0; holders != null && j < holders.length(); j++ )
					{
						 ForsetiClient client = holders.get( j );
						 if ( client != null )
						 {
							  client.CopyWaitListTo( waitList );
						 }
					}
			  }
		 }

		 public override int DetectDeadlock( int clientId )
		 {
			  foreach ( AtomicReferenceArray<ForsetiClient> holders in _clientsHoldingThisLock )
			  {
					for ( int j = 0; holders != null && j < holders.length(); j++ )
					{
						 ForsetiClient client = holders.get( j );
						 if ( client != null && client.IsWaitingFor( clientId ) )
						 {
							  return client.Id();
						 }
					}
			  }
			  return -1;
		 }

		 public virtual bool TryAcquireUpdateLock( ForsetiClient client )
		 {
			  while ( true )
			  {
					int refs = _refCount.get();
					if ( refs > 0 )
					{
						 if ( _refCount.compareAndSet( refs, refs | _updateLockFlag ) )
						 {
							  _updateHolder = client;
							  return true;
						 }
					}
					else
					{
						 return false;
					}
			  }
		 }

		 public virtual void ReleaseUpdateLock()
		 {
			  while ( true )
			  {
					int refs = _refCount.get();
					CleanUpdateHolder();
					if ( _refCount.compareAndSet( refs, refs & ~_updateLockFlag ) )
					{
						 return;
					}
			  }
		 }

		 public virtual void CleanUpdateHolder()
		 {
			  _updateHolder = null;
		 }

		 public virtual int NumberOfHolders()
		 {
			  return _refCount.get() & ~_updateLockFlag;
		 }

		 public virtual bool UpdateLock
		 {
			 get
			 {
				  return ( _refCount.get() & _updateLockFlag ) == _updateLockFlag;
			 }
		 }

		 public override string DescribeWaitList()
		 {
			  StringBuilder sb = new StringBuilder( "SharedLock[" );
			  foreach ( AtomicReferenceArray<ForsetiClient> holders in _clientsHoldingThisLock )
			  {
					bool first = true;
					for ( int j = 0; holders != null && j < holders.length(); j++ )
					{
						 ForsetiClient current = holders.get( j );
						 if ( current != null )
						 {
							  sb.Append( first ? "" : ", " ).Append( current.DescribeWaitList() );
							  first = false;
						 }
					}
			  }
			  return sb.Append( "]" ).ToString();
		 }

		 public override void CollectOwners( ISet<ForsetiClient> owners )
		 {
			  foreach ( AtomicReferenceArray<ForsetiClient> ownerArray in _clientsHoldingThisLock )
			  {
					if ( ownerArray != null )
					{
						 int len = ownerArray.length();
						 for ( int i = 0; i < len; i++ )
						 {
							  ForsetiClient owner = ownerArray.get( i );
							  if ( owner != null )
							  {
									owners.Add( owner );
							  }
						 }
					}
			  }
		 }

		 public override string ToString()
		 {
			  // TODO we should only read out the refCount once, and build a deterministic string based on that
			  if ( UpdateLock )
			  {
					return "UpdateLock{" +
							 "objectId=" + System.identityHashCode( this ) +
							 ", refCount=" + ( _refCount.get() & ~_updateLockFlag ) +
							 ", holder=" + _updateHolder +
							 '}';
			  }
			  else
			  {
					return "SharedLock{" +
							 "objectId=" + System.identityHashCode( this ) +
							 ", refCount=" + _refCount +
							 '}';
			  }
		 }

		 private void RemoveClientHoldingLock( ForsetiClient client )
		 {
			  foreach ( AtomicReferenceArray<ForsetiClient> holders in _clientsHoldingThisLock )
			  {
					if ( holders == null )
					{
						 break;
					}

					for ( int j = 0; j < holders.length(); j++ )
					{
						 ForsetiClient current = holders.get( j );
						 if ( current != null && current.Equals( client ) )
						 {
							  holders.set( j, null );
							  return;
						 }
					}
			  }

			  throw new System.InvalidOperationException( client + " asked to be removed from holder list, but it does not hold " + this );
		 }

		 private bool AddClientHoldingLock( ForsetiClient client )
		 {
			  while ( true )
			  {
					for ( int i = 0; i < _clientsHoldingThisLock.Length; i++ )
					{
						 AtomicReferenceArray<ForsetiClient> holders = _clientsHoldingThisLock[i];
						 if ( holders == null )
						 {
							  holders = AddHolderArray( i );
						 }

						 for ( int j = 0; j < holders.length(); j++ )
						 {
							  ForsetiClient c = holders.get( j );
							  if ( c == null )
							  {
									// TODO This means we do CAS on each entry, very likely hitting a lot of failures until we
									// TODO find a slot. We should look into better strategies here.
									// TODO One such strategy could be binary searching for a free slot, and then linear scan
									// TODO after that if the CAS fails on the slot we found with binary search.
									if ( holders.compareAndSet( j, null, client ) )
									{
										 return true;
									}
							  }
						 }
					}
			  }
		 }

		 private bool AcquireReference()
		 {
			  while ( true )
			  {
					int refs = _refCount.get();
					// UPDATE_LOCK flips the sign bit, so refs will be < 0 if it is an update lock.
					if ( refs > 0 && refs < MAX_HOLDERS )
					{
						 if ( _refCount.compareAndSet( refs, refs + 1 ) )
						 {
							  return true;
						 }
					}
					else
					{
						 return false;
					}
			  }
		 }

		 private bool ReleaseReference()
		 {
			  while ( true )
			  {
					int refAndUpdateFlag = _refCount.get();
					int newRefCount = ( refAndUpdateFlag & ~_updateLockFlag ) - 1;
					if ( _refCount.compareAndSet( refAndUpdateFlag, newRefCount | ( refAndUpdateFlag & _updateLockFlag ) ) )
					{
						 return newRefCount == 0;
					}
			  }
		 }

		 private AtomicReferenceArray<ForsetiClient> AddHolderArray( int slot )
		 {
			 lock ( this )
			 {
				  if ( _clientsHoldingThisLock[slot] == null )
				  {
						_clientsHoldingThisLock[slot] = new AtomicReferenceArray<ForsetiClient>( ( int )( 8 * Math.Pow( 8, slot ) ) );
				  }
				  return _clientsHoldingThisLock[slot];
			 }
		 }

		 private bool ClientHoldsThisLock( ForsetiClient client )
		 {
			  foreach ( AtomicReferenceArray<ForsetiClient> holders in _clientsHoldingThisLock )
			  {
					for ( int j = 0; holders != null && j < holders.length(); j++ )
					{
						 ForsetiClient current = holders.get( j );
						 if ( current != null && current.Equals( client ) )
						 {
							  return true;
						 }
					}
			  }
			  return false;
		 }
	}

}