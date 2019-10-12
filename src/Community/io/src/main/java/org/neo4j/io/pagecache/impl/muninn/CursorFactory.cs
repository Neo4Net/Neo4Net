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
namespace Neo4Net.Io.pagecache.impl.muninn
{
	using PageCacheTracer = Neo4Net.Io.pagecache.tracing.PageCacheTracer;
	using PageCursorTracer = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracer;
	using PageCursorTracerSupplier = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using VersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.VersionContextSupplier;

	internal sealed class CursorFactory
	{
		 private readonly MuninnPagedFile _pagedFile;
		 private readonly long _victimPage;
		 private readonly PageCursorTracerSupplier _pageCursorTracerSupplier;
		 private readonly PageCacheTracer _pageCacheTracer;
		 private readonly VersionContextSupplier _versionContextSupplier;

		 /// <summary>
		 /// Cursor factory construction </summary>
		 /// <param name="pagedFile"> paged file for which cursor is created </param>
		 /// <param name="pageCursorTracerSupplier"> supplier of thread local (transaction local) page cursor tracers that will
		 /// provide thread local page cache statistics </param>
		 /// <param name="pageCacheTracer"> global page cache tracer </param>
		 /// <param name="versionContextSupplier"> version context supplier </param>
		 internal CursorFactory( MuninnPagedFile pagedFile, PageCursorTracerSupplier pageCursorTracerSupplier, PageCacheTracer pageCacheTracer, VersionContextSupplier versionContextSupplier )
		 {
			  this._pagedFile = pagedFile;
			  this._victimPage = pagedFile.PageCache.victimPage;
			  this._pageCursorTracerSupplier = pageCursorTracerSupplier;
			  this._pageCacheTracer = pageCacheTracer;
			  this._versionContextSupplier = versionContextSupplier;
		 }

		 internal MuninnReadPageCursor TakeReadCursor( long pageId, int pfFlags )
		 {
			  MuninnReadPageCursor cursor = new MuninnReadPageCursor( _victimPage, PageCursorTracer, _versionContextSupplier );
			  cursor.Initialise( _pagedFile, pageId, pfFlags );
			  return cursor;
		 }

		 internal MuninnWritePageCursor TakeWriteCursor( long pageId, int pfFlags )
		 {
			  MuninnWritePageCursor cursor = new MuninnWritePageCursor( _victimPage, PageCursorTracer, _versionContextSupplier );
			  cursor.Initialise( _pagedFile, pageId, pfFlags );
			  return cursor;
		 }

		 private PageCursorTracer PageCursorTracer
		 {
			 get
			 {
				  PageCursorTracer pageCursorTracer = _pageCursorTracerSupplier.get();
				  pageCursorTracer.Init( _pageCacheTracer );
				  return pageCursorTracer;
			 }
		 }
	}

}