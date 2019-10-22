/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Kernel.impl.pagecache
{

	using FileUtils = Neo4Net.Io.fs.FileUtils;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using PagedFile = Neo4Net.Io.pagecache.PagedFile;

	internal class PageLoaderFactory
	{
		 private readonly ExecutorService _executor;
		 private readonly PageCache _pageCache;

		 internal PageLoaderFactory( ExecutorService executor, PageCache pageCache )
		 {
			  this._executor = executor;
			  this._pageCache = pageCache;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: PageLoader getLoader(org.Neo4Net.io.pagecache.PagedFile file) throws java.io.IOException
		 internal virtual PageLoader GetLoader( PagedFile file )
		 {
			  if ( FileUtils.highIODevice( file.File().toPath(), false ) )
			  {
					return new ParallelPageLoader( file, _executor, _pageCache );
			  }
			  return new SingleCursorPageLoader( file );
		 }
	}

}