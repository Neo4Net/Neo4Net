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
namespace Neo4Net.Io.pagecache
{

	public class DelegatingPagedFile : PagedFile
	{
		 private readonly PagedFile @delegate;

		 public DelegatingPagedFile( PagedFile @delegate )
		 {
			  this.@delegate = @delegate;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public PageCursor io(long pageId, int pf_flags) throws java.io.IOException
		 public override PageCursor Io( long pageId, int pfFlags )
		 {
			  return @delegate.Io( pageId, pfFlags );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void flushAndForce() throws java.io.IOException
		 public override void FlushAndForce()
		 {
			  @delegate.FlushAndForce();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long getLastPageId() throws java.io.IOException
		 public virtual long LastPageId
		 {
			 get
			 {
				  return @delegate.LastPageId;
			 }
		 }

		 public override int PageSize()
		 {
			  return @delegate.PageSize();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long fileSize() throws java.io.IOException
		 public override long FileSize()
		 {
			  return @delegate.FileSize();
		 }

		 public override File File()
		 {
			  return @delegate.File();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  @delegate.Close();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void flushAndForce(IOLimiter limiter) throws java.io.IOException
		 public override void FlushAndForce( IOLimiter limiter )
		 {
			  @delegate.FlushAndForce( limiter );
		 }
	}

}