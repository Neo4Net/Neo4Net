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
namespace Neo4Net.Io.pagecache.impl.muninn
{
	using UnsafeUtil = Neo4Net.@unsafe.Impl.@internal.Dragons.UnsafeUtil;

	/// <summary>
	/// OffHeapPageLock is a sequence-based lock like StampedLock, but entirely non-blocking, and with special lock modes
	/// that are needed by the Muninn page cache.
	/// <para>
	/// The OffHeapPageLock supports non-blocking optimistic concurrent read locks, non-blocking concurrent write locks,
	/// and non-blocking pessimistic flush and exclusive locks.
	/// </para>
	/// <para>
	/// The optimistic read lock works through validation, so at the end of the critical section, the read lock has to be
	/// validated and, if the validation fails, the critical section has to be retried. The read-lock acquires a stamp
	/// at the start of the critical section, which is then validated at the end of the critical section. The stamp is
	/// invalidated if any write lock or exclusive lock was overlapping with the read lock.
	/// </para>
	/// <para>
	/// The concurrent write locks works by assuming that writes are always non-conflicting, so no validation is required.
	/// However, the write locks will check if a pessimistic exclusive lock is held at the start of the critical
	/// section, and if so, fail to be acquired. The write locks will invalidate all optimistic read locks. The write lock
	/// is try-lock only, and will never block. A successfully taken write lock will raise the <em>modified</em> bit, if it
	/// is not already raised. When the modified bit is raised, <seealso cref="isModified(long)"/> will return {@code true}.
	/// </para>
	/// <para>
	/// The flush lock is also non-blocking (try-lock only), but can only be held by one thread at a time for
	/// implementation reasons. The flush lock is meant for flushing the data in a page. This means that flush locks do not
	/// invalidate optimistic read locks, nor does it prevent overlapping write locks. However, it does prevent other
	/// overlapping flush and exclusive locks. Likewise, if another flush or exclusive lock is held, the attempt to take the
	/// flush lock will fail. The release of the flush lock will lower the modified bit if, and only if, it was not
	/// overlapping with any write lock <em>and</em> the flush lock release method was given a success flag. The success flag
	/// is necessary because the flush operation itself is an IO operation that may fail for any number of reasons.
	/// </para>
	/// <para>
	/// The exclusive lock will also invalidate the optimistic read locks. The exclusive lock is try-lock only, and will
	/// never block. If a write or flush lock is currently held, the attempt to take the exclusive lock will fail, and
	/// the exclusive lock will likewise prevent write and flush locks from being taken.
	/// </para>
	/// <para>
	/// Because all lock types are non-blocking, and because the lock-word itself is external to the implementation, this
	/// class does not need to maintain any state by itself. Thus, the class cannot be instantiated, and all methods are
	/// static.
	/// </para>
	/// <para>
	/// Note that the lock-word is assumed to be 8 bytes, and should ideally be aligned to 8-byte boundaries, and ideally
	/// run on platforms that support 8-byte-wide atomic memory operations.
	/// </para>
	/// </summary>
	public sealed class OffHeapPageLock
	{
		 /*
		  * Bits for counting concurrent write-locks. We use 17 bits because our pages are most likely 8192 bytes, and
		  * 2^17 = 131.072, which is far more than our page size, so makes it highly unlikely that we are going to overflow
		  * our concurrent write lock counter. Meanwhile, it's also small enough that we have a very large (2^44) number
		  * space for our sequence. This one value controls the layout of the lock bit-state. The rest of the layout is
		  * derived from this.
		  *
		  * With 17 writer count bits, the layout looks like this:
		  *
		  * ┏━ [FLS] Flush lock bit
		  * ┃┏━ [EXL] Exclusive lock bit
		  * ┃┃┏━ [MOD] Modified bit
		  * ┃┃┃    ┏━ [CNT] Count of currently concurrently held write locks, 17 bits.
		  * ┃┃┃    ┃                 ┏━ [SEQ] 44 bits for the read lock sequence, incremented on write & exclusive unlock.
		  * ┃┃┃┏━━━┻━━━━━━━━━━━━━┓┏━━┻━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓
		  * FEMWWWWW WWWWWWWW WWWWSSSS SSSSSSSS SSSSSSSS SSSSSSSS SSSSSSSS SSSSSSSS
		  * 1        2        3        4        5        6        7        8        byte
		  */
		 private const long CNT_BITS = 17;

		 private const long BITS_IN_LONG = 64;
		 private const long EXL_LOCK_BITS = 1; // Exclusive lock bits (only 1 is supported)
		 private const long FLS_LOCK_BITS = 1; // Flush lock bits (only 1 is supported)
		 private const long MOD_BITS = 1; // Modified state bits (only 1 is supported)
		 private static readonly long _seqBits = BITS_IN_LONG - FLS_LOCK_BITS - EXL_LOCK_BITS - MOD_BITS - CNT_BITS;

		 // Bit map reference:              = 0bFEMWWWWW WWWWWWWW WWWWSSSS SSSSSSSS SSSSSSSS SSSSSSSS SSSSSSSS SSSSSSSS
		 private static readonly long _flsMask = 0b10000000_00000000_00000000_00000000_00000000_00000000_00000000_00000000L;
		 private static readonly long _exlMask = 0b01000000_00000000_00000000_00000000_00000000_00000000_00000000_00000000L;
		 private static readonly long _modMask = 0b00100000_00000000_00000000_00000000_00000000_00000000_00000000_00000000L;
		 private static readonly long _cntMask = 0b00011111_11111111_11110000_00000000_00000000_00000000_00000000_00000000L;
		 private static readonly long _seqMask = 0b00000000_00000000_00001111_11111111_11111111_11111111_11111111_11111111L;
		 private static readonly long _cntUnit = 0b00000000_00000000_00010000_00000000_00000000_00000000_00000000_00000000L;
		 private static readonly long _seqImsk = 0b11111111_11111111_11110000_00000000_00000000_00000000_00000000_00000000L;
		 // Mask used to check optimistic read lock validity:
		 private static readonly long _chkMask = 0b01011111_11111111_11111111_11111111_11111111_11111111_11111111_11111111L;
		 // "Flush and/or exclusive" mask:
		 private static readonly long _faeMask = 0b11000000_00000000_00000000_00000000_00000000_00000000_00000000_00000000L;
		 // Unlocked mask:
		 private static readonly long _unlMask = 0b11011111_11111111_11110000_00000000_00000000_00000000_00000000_00000000L;

		 private OffHeapPageLock()
		 {
			  // The static version keeps all state externally.
		 }

		 private static long GetState( long address )
		 {
			  return UnsafeUtil.getLongVolatile( address );
		 }

		 private static bool CompareAndSetState( long address, long expect, long update )
		 {
			  return UnsafeUtil.compareAndSwapLong( null, address, expect, update );
		 }

		 private static void UnconditionallySetState( long address, long update )
		 {
			  UnsafeUtil.putLongVolatile( address, update );
		 }

		 /// <returns> A newly initialised lock word, for a lock that is exclusively locked. </returns>
		 public static long InitialLockWordWithExclusiveLock()
		 {
			  return _exlMask;
		 }

		 /// <summary>
		 /// Start an optimistic critical section, and return a stamp that can be used to validate if the read lock was
		 /// consistent. That is, if no write or exclusive lock was overlapping with the optimistic read lock.
		 /// </summary>
		 /// <returns> A stamp that must be passed to <seealso cref="validateReadLock(long, long)"/> to validate the critical section. </returns>
		 public static long TryOptimisticReadLock( long address )
		 {
			  return GetState( address ) & _seqMask;
		 }

		 /// <summary>
		 /// Validate a stamp from <seealso cref="tryOptimisticReadLock(long)"/> or <seealso cref="unlockExclusive(long)"/>, and return
		 /// {@code true} if no write or exclusive lock overlapped with the critical section of the optimistic read lock
		 /// represented by the stamp.
		 /// </summary>
		 /// <param name="stamp"> The stamp of the optimistic read lock. </param>
		 /// <returns> {@code true} if the optimistic read lock was valid, {@code false} otherwise. </returns>
		 public static bool ValidateReadLock( long address, long stamp )
		 {
			  UnsafeUtil.loadFence();
			  return ( GetState( address ) & _chkMask ) == stamp;
		 }

		 public static bool IsModified( long address )
		 {
			  return ( GetState( address ) & _modMask ) == _modMask;
		 }

		 public static bool IsExclusivelyLocked( long address )
		 {
			  return ( GetState( address ) & _exlMask ) == _exlMask;
		 }

		 /// <summary>
		 /// Try taking a concurrent write lock. Multiple write locks can be held at the same time. Write locks will
		 /// invalidate any optimistic read lock that overlaps with them, and write locks will make any attempt at grabbing
		 /// an exclusive lock fail. If an exclusive lock is currently held, then the attempt to take a write lock will fail.
		 /// <para>
		 /// Write locks must be paired with a corresponding <seealso cref="unlockWrite(long)"/>.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> {@code true} if the write lock was taken, {@code false} otherwise. </returns>
		 public static bool TryWriteLock( long address )
		 {
			  long s;
			  long n;
			  for ( ; ; )
			  {
					s = GetState( address );
					bool unwritablyLocked = ( s & _exlMask ) != 0;
					bool writeCountOverflow = ( s & _cntMask ) == _cntMask;

					// bitwise-OR to reduce branching and allow more ILP
					if ( unwritablyLocked | writeCountOverflow )
					{
						 return FailWriteLock( s, writeCountOverflow );
					}

					n = s + _cntUnit | _modMask;
					if ( CompareAndSetState( address, s, n ) )
					{
						 UnsafeUtil.storeFence();
						 return true;
					}
			  }
		 }

		 private static bool FailWriteLock( long s, bool writeCountOverflow )
		 {
			  if ( writeCountOverflow )
			  {
					ThrowWriteLockOverflow( s );
			  }
			  // Otherwise it was exclusively locked
			  return false;
		 }

		 private static void ThrowWriteLockOverflow( long s )
		 {
			  throw new IllegalMonitorStateException( "Write lock counter overflow: " + DescribeState( s ) );
		 }

		 /// <summary>
		 /// Release a write lock taking with <seealso cref="tryWriteLock(long)"/>.
		 /// </summary>
		 public static void UnlockWrite( long address )
		 {
			  long s;
			  long n;
			  do
			  {
					s = GetState( address );
					if ( ( s & _cntMask ) == 0 )
					{
						 ThrowUnmatchedUnlockWrite( s );
					}
					n = NextSeq( s ) - _cntUnit;
			  } while ( !CompareAndSetState( address, s, n ) );
		 }

		 private static void ThrowUnmatchedUnlockWrite( long s )
		 {
			  throw new IllegalMonitorStateException( "Unmatched unlockWrite: " + DescribeState( s ) );
		 }

		 private static long NextSeq( long s )
		 {
			  return ( s & _seqImsk ) + ( s + 1 & _seqMask );
		 }

		 public static long UnlockWriteAndTryTakeFlushLock( long address )
		 {
			  long s;
			  long n;
			  long r;
			  do
			  {
					r = 0;
					s = GetState( address );
					if ( ( s & _cntMask ) == 0 )
					{
						 ThrowUnmatchedUnlockWrite( s );
					}
					n = NextSeq( s ) - _cntUnit;
					if ( ( n & _faeMask ) == 0 )
					{
						 n += _flsMask;
						 r = n;
					}
			  } while ( !CompareAndSetState( address, s, n ) );
			  UnsafeUtil.storeFence();
			  return r;
		 }

		 /// <summary>
		 /// Grab the exclusive lock if it is immediately available. Exclusive locks will invalidate any overlapping
		 /// optimistic read lock, and fail write and flush locks. If any write or flush locks are currently taken, or if
		 /// the exclusive lock is already taken, then the attempt to grab an exclusive lock will fail.
		 /// <para>
		 /// Successfully grabbed exclusive locks must always be paired with a corresponding <seealso cref="unlockExclusive(long)"/>.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> {@code true} if we successfully got the exclusive lock, {@code false} otherwise. </returns>
		 public static bool TryExclusiveLock( long address )
		 {
			  long s = GetState( address );
			  bool res = ( ( s & _unlMask ) == 0 ) && CompareAndSetState( address, s, s + _exlMask );
			  UnsafeUtil.storeFence();
			  return res;
		 }

		 /// <summary>
		 /// Unlock the currently held exclusive lock, and atomically and implicitly take an optimistic read lock, as
		 /// represented by the returned stamp.
		 /// </summary>
		 /// <returns> A stamp that represents an optimistic read lock, in case you need it. </returns>
		 public static long UnlockExclusive( long address )
		 {
			  long s = InitiateExclusiveLockRelease( address );
			  long n = NextSeq( s ) - _exlMask;
			  // Exclusive locks prevent any state modifications from write locks
			  UnconditionallySetState( address, n );
			  return n;
		 }

		 /// <summary>
		 /// Atomically unlock the currently held exclusive lock, and take a write lock.
		 /// </summary>
		 public static void UnlockExclusiveAndTakeWriteLock( long address )
		 {
			  long s = InitiateExclusiveLockRelease( address );
			  long n = ( NextSeq( s ) - _exlMask + _cntUnit ) | _modMask;
			  UnconditionallySetState( address, n );
		 }

		 private static long InitiateExclusiveLockRelease( long address )
		 {
			  long s = GetState( address );
			  if ( ( s & _exlMask ) != _exlMask )
			  {
					ThrowUnmatchedUnlockExclusive( s );
			  }
			  return s;
		 }

		 private static void ThrowUnmatchedUnlockExclusive( long s )
		 {
			  throw new IllegalMonitorStateException( "Unmatched unlockExclusive: " + DescribeState( s ) );
		 }

		 /// <summary>
		 /// If the given lock is exclusively held, then the <em>modified</em> flag will be explicitly lowered (marked as
		 /// unmodified) if the <em>modified</em> is currently raised.
		 /// <para>
		 /// If the <em>modified</em> flag is currently not raised, then this method does nothing.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <exception cref="IllegalStateException"> if the lock at the given address is not in the exclusively locked state. </exception>
		 public static void ExplicitlyMarkPageUnmodifiedUnderExclusiveLock( long address )
		 {
			  long s = GetState( address );
			  if ( ( s & _exlMask ) != _exlMask )
			  {
					throw new System.InvalidOperationException( "Page must be exclusively locked to explicitly lower modified bit" );
			  }
			  s = s & ( ~_modMask );
			  UnconditionallySetState( address, s );
		 }

		 /// <summary>
		 /// Grab the flush lock if it is immediately available. Flush locks prevent overlapping exclusive locks,
		 /// but do not invalidate optimistic read locks, nor do they prevent overlapping write locks. Only one flush lock
		 /// can be held at a time. If any flush or exclusive lock is already held, the attempt to take the flush lock will
		 /// fail.
		 /// <para>
		 /// Successfully grabbed flush locks must always be paired with a corresponding
		 /// <seealso cref="unlockFlush(long, long, bool)"/>.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> If the lock is successfully grabbed, the method will return a stamp value that must be passed to the
		 /// <seealso cref="unlockFlush(long, long, bool)"/>, and which is used for detecting any overlapping write locks. If the
		 /// flush lock could not be taken, {@code 0} will be returned. </returns>
		 public static long TryFlushLock( long address )
		 {
			  long s = GetState( address );
			  if ( ( s & _faeMask ) == 0 )
			  {
					long n = s + _flsMask;
					bool res = CompareAndSetState( address, s, n );
					UnsafeUtil.storeFence();
					return res ? n : 0;
			  }
			  return 0;
		 }

		 /// <summary>
		 /// Unlock the currently held flush lock.
		 /// </summary>
		 public static void UnlockFlush( long address, long stamp, bool success )
		 {
			  long s;
			  long n;
			  do
			  {
					s = GetState( address );
					if ( ( s & _flsMask ) != _flsMask )
					{
						 ThrowUnmatchedUnlockFlush( s );
					}
					// We don't increment the sequence with nextSeq here, because flush locks don't invalidate readers
					n = s - _flsMask;
					if ( success && ( s & _chkMask ) == ( stamp & _seqMask ) )
					{
						 // The flush was successful and we had no overlapping writers, thus we can lower the modified flag
						 n = n & ( ~_modMask );
					}
			  } while ( !CompareAndSetState( address, s, n ) );
		 }

		 private static void ThrowUnmatchedUnlockFlush( long s )
		 {
			  throw new IllegalMonitorStateException( "Unmatched unlockFlush: " + DescribeState( s ) );
		 }

		 private static string DescribeState( long s )
		 {
			  long flush = ( long )( ( ulong )s >> EXL_LOCK_BITS + MOD_BITS + CNT_BITS + _seqBits );
			  long excl = ( long )( ( ulong )( s & _exlMask ) >> MOD_BITS + CNT_BITS + _seqBits );
			  long mod = ( long )( ( ulong )( s & _modMask ) >> CNT_BITS + _seqBits );
			  long cnt = ( s & _cntMask ) >> _seqBits;
			  long seq = s & _seqMask;
			  return "OffHeapPageLock[" +
						"Flush: " + flush + ", Excl: " + excl + ", Mod: " + mod + ", Ws: " + cnt + ", S: " + seq + "]";
		 }

		 internal static string ToString( long address )
		 {
			  return DescribeState( GetState( address ) );
		 }
	}

}