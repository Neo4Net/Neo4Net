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
namespace Org.Neo4j.Index.@internal.gbptree
{

	using UnsafeUtil = Org.Neo4j.@unsafe.Impl.@internal.Dragons.UnsafeUtil;

	internal class GBPTreeLock
	{
		 private static readonly long _stateOffset = UnsafeUtil.getFieldOffset( typeof( GBPTreeLock ), "state" );
		 private const long WRITER_LOCK_BIT = 0x00000000_00000001L;
		 private const long CLEANER_LOCK_BIT = 0x00000000_00000002L;
		 private volatile long _state;

		 // Used for testing
		 internal virtual GBPTreeLock Copy()
		 {
			  GBPTreeLock copy = new GBPTreeLock();
			  copy._state = _state;
			  return copy;
		 }

		 internal virtual void WriterLock()
		 {
			  DoLock( WRITER_LOCK_BIT );
		 }

		 internal virtual void WriterUnlock()
		 {
			  DoUnlock( WRITER_LOCK_BIT );
		 }

		 internal virtual void CleanerLock()
		 {
			  DoLock( CLEANER_LOCK_BIT );
		 }

		 internal virtual void CleanerUnlock()
		 {
			  DoUnlock( CLEANER_LOCK_BIT );
		 }

		 internal virtual void WriterAndCleanerLock()
		 {
			  DoLock( WRITER_LOCK_BIT | CLEANER_LOCK_BIT );
		 }

		 internal virtual void WriterAndCleanerUnlock()
		 {
			  DoUnlock( WRITER_LOCK_BIT | CLEANER_LOCK_BIT );
		 }

		 private void DoLock( long targetLockBit )
		 {
			  long currentState;
			  long newState;
			  do
			  {
					currentState = _state;
					while ( !CanLock( currentState, targetLockBit ) )
					{
						 // sleep
						 Sleep();
						 currentState = _state;
					}
					newState = currentState | targetLockBit;
			  } while ( !UnsafeUtil.compareAndSwapLong( this, _stateOffset, currentState, newState ) );
		 }

		 private void DoUnlock( long targetLockBit )
		 {
			  long currentState;
			  long newState;
			  do
			  {
					currentState = _state;
					if ( !CanUnlock( currentState, targetLockBit ) )
					{
						 throw new System.InvalidOperationException( "Can not unlock lock that is already locked" );
					}
					newState = currentState & ~targetLockBit;
			  } while ( !UnsafeUtil.compareAndSwapLong( this, _stateOffset, currentState, newState ) );
		 }

		 private bool CanLock( long state, long targetLockBit )
		 {
			  return ( state & targetLockBit ) == 0;
		 }

		 private bool CanUnlock( long state, long targetLockBit )
		 {
			  return ( state & targetLockBit ) == targetLockBit;
		 }

		 private void Sleep()
		 {
			  LockSupport.parkNanos( TimeUnit.MILLISECONDS.toNanos( 10 ) );
		 }
	}

}