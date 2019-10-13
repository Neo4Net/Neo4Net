using System.Collections.Generic;

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
namespace Neo4Net.Io.pagecache
{

	public class DelegatingPageCache : PageCache
	{
		 private readonly PageCache @delegate;

		 public DelegatingPageCache( PageCache @delegate )
		 {
			  this.@delegate = @delegate;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public PagedFile map(java.io.File file, int pageSize, java.nio.file.OpenOption... openOptions) throws java.io.IOException
		 public override PagedFile Map( File file, int pageSize, params OpenOption[] openOptions )
		 {
			  return @delegate.Map( file, pageSize, openOptions );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.Optional<PagedFile> getExistingMapping(java.io.File file) throws java.io.IOException
		 public override Optional<PagedFile> GetExistingMapping( File file )
		 {
			  return @delegate.GetExistingMapping( file );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.List<PagedFile> listExistingMappings() throws java.io.IOException
		 public override IList<PagedFile> ListExistingMappings()
		 {
			  return @delegate.ListExistingMappings();
		 }

		 public override int PageSize()
		 {
			  return @delegate.PageSize();
		 }

		 public override void Close()
		 {
			  @delegate.Close();
		 }

		 public override long MaxCachedPages()
		 {
			  return @delegate.MaxCachedPages();
		 }

		 public override void ReportEvents()
		 {
			  @delegate.ReportEvents();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void flushAndForce(IOLimiter limiter) throws java.io.IOException
		 public override void FlushAndForce( IOLimiter limiter )
		 {
			  @delegate.FlushAndForce( limiter );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void flushAndForce() throws java.io.IOException
		 public override void FlushAndForce()
		 {
			  @delegate.FlushAndForce();
		 }

	}

}