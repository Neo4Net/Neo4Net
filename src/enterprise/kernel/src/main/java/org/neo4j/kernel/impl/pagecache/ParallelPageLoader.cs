using System.Threading;

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

	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using PagedFile = Neo4Net.Io.pagecache.PagedFile;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.pagecache.PagedFile_Fields.PF_SHARED_READ_LOCK;

	internal class ParallelPageLoader : PageLoader
	{
		 private readonly PagedFile _file;
		 private readonly Executor _executor;
		 private readonly PageCache _pageCache;
		 private readonly AtomicLong _received;
		 private readonly AtomicLong _processed;

		 internal ParallelPageLoader( PagedFile file, Executor executor, PageCache pageCache )
		 {
			  this._file = file;
			  this._executor = executor;
			  this._pageCache = pageCache;
			  _received = new AtomicLong();
			  _processed = new AtomicLong();
		 }

		 public override void Load( long pageId )
		 {
			  _received.AndIncrement;
			  _executor.execute(() =>
			  {
				try
				{
					 try
					 {
						 using ( PageCursor cursor = _file.io( pageId, PF_SHARED_READ_LOCK ) )
						 {
							  cursor.next();
						 }
					 }
					 catch ( IOException )
					 {
					 }
				}
				finally
				{
					 _pageCache.reportEvents();
					 _processed.AndIncrement;
				}
			  });
		 }

		 public override void Close()
		 {
			  while ( _processed.get() < _received.get() )
			  {
					Thread.yield();
			  }
		 }
	}

}