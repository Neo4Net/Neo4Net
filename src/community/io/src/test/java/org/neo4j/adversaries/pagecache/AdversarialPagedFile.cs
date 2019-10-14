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
namespace Neo4Net.Adversaries.pagecache
{

	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using PagedFile = Neo4Net.Io.pagecache.PagedFile;

	/// <summary>
	/// A <seealso cref="PagedFile paged file"/> that wraps another paged file and an <seealso cref="Adversary adversary"/> to provide
	/// a misbehaving paged file implementation for testing.
	/// <para>
	/// Depending on the adversary each operation can throw either <seealso cref="System.Exception"/> like <seealso cref="SecurityException"/>
	/// or <seealso cref="IOException"/> like <seealso cref="FileNotFoundException"/>.
	/// </para>
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public class AdversarialPagedFile implements org.neo4j.io.pagecache.PagedFile
	public class AdversarialPagedFile : PagedFile
	{
		 private readonly PagedFile @delegate;
		 private readonly Adversary _adversary;

		 public AdversarialPagedFile( PagedFile @delegate, Adversary adversary )
		 {
			  this.@delegate = Objects.requireNonNull( @delegate );
			  this._adversary = Objects.requireNonNull( adversary );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.io.pagecache.PageCursor io(long pageId, int pf_flags) throws java.io.IOException
		 public override PageCursor Io( long pageId, int pfFlags )
		 {
			  _adversary.injectFailure( typeof( System.InvalidOperationException ) );
			  PageCursor pageCursor = @delegate.Io( pageId, pfFlags );
			  if ( ( pfFlags & Neo4Net.Io.pagecache.PagedFile_Fields.PF_SHARED_READ_LOCK ) == Neo4Net.Io.pagecache.PagedFile_Fields.PF_SHARED_READ_LOCK )
			  {
					return new AdversarialReadPageCursor( pageCursor, _adversary );
			  }
			  return new AdversarialWritePageCursor( pageCursor, _adversary );
		 }

		 public override int PageSize()
		 {
			  return @delegate.PageSize();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long fileSize() throws java.io.IOException
		 public override long FileSize()
		 {
			  _adversary.injectFailure( typeof( System.InvalidOperationException ) );
			  return @delegate.FileSize();
		 }

		 public override File File()
		 {
			  return @delegate.File();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void flushAndForce() throws java.io.IOException
		 public override void FlushAndForce()
		 {
			  _adversary.injectFailure( typeof( FileNotFoundException ), typeof( IOException ), typeof( SecurityException ) );
			  @delegate.FlushAndForce();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void flushAndForce(org.neo4j.io.pagecache.IOLimiter limiter) throws java.io.IOException
		 public override void FlushAndForce( IOLimiter limiter )
		 {
			  _adversary.injectFailure( typeof( FileNotFoundException ), typeof( IOException ), typeof( SecurityException ) );
			  @delegate.FlushAndForce( limiter );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long getLastPageId() throws java.io.IOException
		 public virtual long LastPageId
		 {
			 get
			 {
				  _adversary.injectFailure( typeof( System.InvalidOperationException ) );
				  return @delegate.LastPageId;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  _adversary.injectFailure( typeof( FileNotFoundException ), typeof( IOException ), typeof( SecurityException ) );
			  @delegate.Close();
		 }
	}

}