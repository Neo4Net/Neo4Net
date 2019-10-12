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
namespace Org.Neo4j.Adversaries.pagecache
{
	using ArrayUtils = org.apache.commons.lang3.ArrayUtils;


	using IOLimiter = Org.Neo4j.Io.pagecache.IOLimiter;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using PagedFile = Org.Neo4j.Io.pagecache.PagedFile;

	/// <summary>
	/// A <seealso cref="PageCache page cache"/> that wraps another page cache and an <seealso cref="Adversary adversary"/> to provide
	/// a misbehaving page cache implementation for testing.
	/// <para>
	/// Depending on the adversary each operation can throw either <seealso cref="System.Exception"/> like <seealso cref="SecurityException"/>
	/// or <seealso cref="IOException"/> like <seealso cref="FileNotFoundException"/>.
	/// </para>
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public class AdversarialPageCache implements org.neo4j.io.pagecache.PageCache
	public class AdversarialPageCache : PageCache
	{
		 private readonly PageCache @delegate;
		 private readonly Adversary _adversary;

		 public AdversarialPageCache( PageCache @delegate, Adversary adversary )
		 {
			  this.@delegate = Objects.requireNonNull( @delegate );
			  this._adversary = Objects.requireNonNull( adversary );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.io.pagecache.PagedFile map(java.io.File file, int pageSize, java.nio.file.OpenOption... openOptions) throws java.io.IOException
		 public override PagedFile Map( File file, int pageSize, params OpenOption[] openOptions )
		 {
			  if ( ArrayUtils.contains( openOptions, StandardOpenOption.CREATE ) )
			  {
					_adversary.injectFailure( typeof( IOException ), typeof( SecurityException ) );
			  }
			  else
			  {
					_adversary.injectFailure( typeof( FileNotFoundException ), typeof( IOException ), typeof( SecurityException ) );
			  }
			  PagedFile pagedFile = @delegate.Map( file, pageSize, openOptions );
			  return new AdversarialPagedFile( pagedFile, _adversary );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.Optional<org.neo4j.io.pagecache.PagedFile> getExistingMapping(java.io.File file) throws java.io.IOException
		 public override Optional<PagedFile> GetExistingMapping( File file )
		 {
			  _adversary.injectFailure( typeof( IOException ), typeof( SecurityException ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Optional<org.neo4j.io.pagecache.PagedFile> optional = delegate.getExistingMapping(file);
			  Optional<PagedFile> optional = @delegate.GetExistingMapping( file );
			  return optional.map( pagedFile => new AdversarialPagedFile( pagedFile, _adversary ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.List<org.neo4j.io.pagecache.PagedFile> listExistingMappings() throws java.io.IOException
		 public override IList<PagedFile> ListExistingMappings()
		 {
			  _adversary.injectFailure( typeof( IOException ), typeof( SecurityException ) );
			  IList<PagedFile> list = @delegate.ListExistingMappings();
			  for ( int i = 0; i < list.Count; i++ )
			  {
					list[i] = new AdversarialPagedFile( list[i], _adversary );
			  }
			  return list;
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

		 public override void Close()
		 {
			  _adversary.injectFailure( typeof( System.InvalidOperationException ) );
			  @delegate.Close();
		 }

		 public override int PageSize()
		 {
			  return @delegate.PageSize();
		 }

		 public override long MaxCachedPages()
		 {
			  return @delegate.MaxCachedPages();
		 }

		 public override void ReportEvents()
		 {
			  @delegate.ReportEvents();
		 }
	}

}