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

	public class StubPagedFile : PagedFile
	{
		 private readonly int _pageSize;
		 public readonly int ExposedPageSize;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 public long LastPageIdConflict = 1;

		 public StubPagedFile( int pageSize )
		 {
			  this._pageSize = pageSize;
			  this.ExposedPageSize = pageSize;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public PageCursor io(long pageId, int pf_flags) throws java.io.IOException
		 public override PageCursor Io( long pageId, int pfFlags )
		 {
			  StubPageCursor cursor = new StubPageCursor( pageId, _pageSize );
			  PrepareCursor( cursor );
			  return cursor;
		 }

		 protected internal virtual void PrepareCursor( StubPageCursor cursor )
		 {
		 }

		 public override int PageSize()
		 {
			  return ExposedPageSize;
		 }

		 public override long FileSize()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long lastPageId = getLastPageId();
			  long lastPageId = LastPageId;
			  if ( lastPageId < 0 )
			  {
					return 0L;
			  }
			  return ( lastPageId + 1 ) * PageSize();
		 }

		 public override File File()
		 {
			  return new File( "stub" );
		 }

		 public override void FlushAndForce()
		 {
		 }

		 public override void FlushAndForce( IOLimiter limiter )
		 {
		 }

		 public virtual long LastPageId
		 {
			 get
			 {
				  return LastPageIdConflict;
			 }
		 }

		 public override void Close()
		 {
		 }
	}

}