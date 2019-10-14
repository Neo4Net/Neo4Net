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
namespace Neo4Net.Io.pagecache.impl.muninn
{
	using UnsafeUtil = Neo4Net.@unsafe.Impl.@internal.Dragons.UnsafeUtil;
	using FeatureToggles = Neo4Net.Utils.FeatureToggles;
	using BinaryLatch = Neo4Net.Utils.Concurrent.BinaryLatch;

	/// <summary>
	/// The LatchMap is used by the <seealso cref="MuninnPagedFile"/> to coordinate concurrent page faults, and ensure that no two
	/// threads try to fault in the same page at the same time. If there is high demand for a particular page, then the
	/// LatchMap will ensure that only one thread actually does the faulting, and that any other interested threads will
	/// wait for the faulting thread to complete the fault before they proceed.
	/// </summary>
	internal sealed class LatchMap
	{
		 internal sealed class Latch : BinaryLatch
		 {
			  internal LatchMap LatchMap;
			  internal int Index;

			  public override void Release()
			  {
					LatchMap.setLatch( Index, null );
					base.Release();
			  }
		 }

		 private static readonly int _faultLockStriping = FeatureToggles.getInteger( typeof( LatchMap ), "faultLockStriping", 128 );
		 private static readonly long _faultLockMask = _faultLockStriping - 1;
		 private static readonly int _latchesArrayBase = UnsafeUtil.arrayBaseOffset( typeof( Latch[] ) );
		 private static readonly int _latchesArrayScale = UnsafeUtil.arrayIndexScale( typeof( Latch[] ) );

		 private readonly Latch[] _latches;

		 internal LatchMap()
		 {
			  _latches = new Latch[_faultLockStriping];
		 }

		 private long Offset( int index )
		 {
			  return UnsafeUtil.arrayOffset( index, _latchesArrayBase, _latchesArrayScale );
		 }

		 private void SetLatch( int index, BinaryLatch newValue )
		 {
			  UnsafeUtil.putObjectVolatile( _latches, Offset( index ), newValue );
		 }

		 private bool CompareAndSetLatch( int index, Latch expected, Latch update )
		 {
			  return UnsafeUtil.compareAndSwapObject( _latches, Offset( index ), expected, update );
		 }

		 private Latch GetLatch( int index )
		 {
			  return ( Latch ) UnsafeUtil.getObjectVolatile( _latches, Offset( index ) );
		 }

		 /// <summary>
		 /// If a latch is currently installed for the given (or any colliding) identifier, then it will be waited upon and
		 /// {@code null} will be returned.
		 /// 
		 /// Otherwise, if there is currently no latch installed for the given identifier, then one will be created and
		 /// installed, and that latch will be returned. Once the page fault has been completed, the returned latch must be
		 /// released. Releasing the latch will unblock all threads that are waiting upon it, and the latch will be
		 /// atomically uninstalled.
		 /// </summary>
		 internal Latch TakeOrAwaitLatch( long identifier )
		 {
			  int index = index( identifier );
			  Latch latch = GetLatch( index );
			  while ( latch == null )
			  {
					latch = new Latch();
					if ( CompareAndSetLatch( index, null, latch ) )
					{
						 latch.LatchMap = this;
						 latch.Index = index;
						 return latch;
					}
					latch = GetLatch( index );
			  }
			  latch.Await();
			  return null;
		 }

		 private int Index( long identifier )
		 {
			  return ( int )( Mix( identifier ) & _faultLockMask );
		 }

		 private long Mix( long identifier )
		 {
			  identifier ^= identifier << 21;
			  identifier ^= ( long )( ( ulong )identifier >> 35 );
			  identifier ^= identifier << 4;
			  return identifier;
		 }
	}

}