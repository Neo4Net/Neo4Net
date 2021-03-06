﻿using System.Collections.Concurrent;

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
namespace Org.Neo4j.Io.pagecache
{

	using BinaryLatch = Org.Neo4j.Util.concurrent.BinaryLatch;

	/// <summary>
	/// A tiny dumb lock manager built specifically for the page cache stress test, because it needs something to represent
	/// the entity locks since page write locks are not exclusive. Also, for the stress test, a simple array of
	/// ReentrantLocks would take up too much memory.
	/// </summary>
	public class TinyLockManager
	{
		 private readonly ConcurrentDictionary<int, BinaryLatch> _map = new ConcurrentDictionary<int, BinaryLatch>( 64, 0.75f, 64 );

		 public virtual void Lock( int recordId )
		 {
			  int? record = recordId;
			  BinaryLatch myLatch = new BinaryLatch();
			  for ( ;; )
			  {
					BinaryLatch existingLatch = _map.GetOrAdd( record, myLatch );
					if ( existingLatch == null )
					{
						 break;
					}
					else
					{
						 existingLatch.Await();
					}
			  }
		 }

		 public virtual bool TryLock( int recordId )
		 {
			  int? record = recordId;
			  BinaryLatch myLatch = new BinaryLatch();
			  BinaryLatch existingLatch = _map.GetOrAdd( record, myLatch );
			  return existingLatch == null;
		 }

		 public virtual void Unlock( int recordId )
		 {
			  _map.Remove( recordId ).release();
		 }
	}

}