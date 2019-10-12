using System;
using System.Collections.Generic;

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
namespace Neo4Net.Index.@internal.gbptree
{
	using Pair = org.apache.commons.lang3.tuple.Pair;


	using PageCursor = Neo4Net.Io.pagecache.PageCursor;

	internal class SimpleIdProvider : IdProvider
	{
		 private readonly LinkedList<Pair<long, long>> _releasedIds = new LinkedList<Pair<long, long>>();
		 private readonly System.Func<PageCursor> _cursorSupplier;
		 private long _lastId;

		 internal SimpleIdProvider( System.Func<PageCursor> cursorSupplier )
		 {
			  this._cursorSupplier = cursorSupplier;
			  Reset();
		 }

		 public override long AcquireNewId( long stableGeneration, long unstableGeneration )
		 {
			  if ( _releasedIds.Count > 0 )
			  {
					Pair<long, long> free = _releasedIds.First.Value;
					if ( free.Left <= stableGeneration )
					{
						 _releasedIds.RemoveFirst();
						 long? pageId = free.Right;
						 ZapPage( pageId );
						 return pageId.Value;
					}
			  }
			  _lastId++;
			  return _lastId;
		 }

		 public override void ReleaseId( long stableGeneration, long unstableGeneration, long id )
		 {
			  _releasedIds.AddLast( Pair.of( unstableGeneration, id ) );
		 }

		 public override void VisitFreelist( IdProvider_IdProviderVisitor visitor )
		 {
			  int pos = 0;
			  visitor.BeginFreelistPage( 0 );
			  foreach ( Pair<long, long> releasedId in _releasedIds )
			  {
					visitor.FreelistEntry( releasedId.Right, releasedId.Left, pos++ );
			  }
			  visitor.EndFreelistPage( 0 );
		 }

		 public override long LastId()
		 {
			  return _lastId;
		 }

		 internal virtual void Reset()
		 {
			  _releasedIds.Clear();
			  _lastId = IdSpace.MIN_TREE_NODE_ID - 1;
		 }

		 private void ZapPage( long? pageId )
		 {
			  try
			  {
					  using ( PageCursor cursor = _cursorSupplier.get() )
					  {
						cursor.Next( pageId.Value );
						cursor.ZapPage();
					  }
			  }
			  catch ( IOException )
			  {
					throw new Exception( "Could not go to page " + pageId );
			  }
		 }
	}

}