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
namespace Org.Neo4j.Index.@internal.gbptree
{

	using PageCursor = Org.Neo4j.Io.pagecache.PageCursor;
	using PagedFile = Org.Neo4j.Io.pagecache.PagedFile;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.PageCursorUtil.checkOutOfBounds;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.PageCursorUtil.goTo;

	internal class FreeListIdProvider : IdProvider
	{
		 internal interface Monitor
		 {
			  /// <summary>
			  /// Called when a page id was acquired for storing released ids into.
			  /// </summary>
			  /// <param name="freelistPageId"> page id of the acquired page. </param>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//			  default void acquiredFreelistPageId(long freelistPageId)
	//		  { // Empty by default
	//		  }

			  /// <summary>
			  /// Called when a free-list page was released due to all its ids being acquired.
			  /// A released free-list page ends up in the free-list itself.
			  /// </summary>
			  /// <param name="freelistPageId"> page if of the released page. </param>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//			  default void releasedFreelistPageId(long freelistPageId)
	//		  { // Empty by default
	//		  }
		 }

		 internal static readonly Monitor NO_MONITOR = new MonitorAnonymousInnerClass();

		 private class MonitorAnonymousInnerClass : Monitor
		 {
		 }

		 private readonly PagedFile _pagedFile;

		 /// <summary>
		 /// <seealso cref="FreelistNode"/> governs physical layout of a free-list.
		 /// </summary>
		 private readonly FreelistNode _freelistNode;

		 /// <summary>
		 /// There's one free-list which both stable and unstable state (the state pages A/B) shares.
		 /// Each free list page links to a potential next free-list page, by using the last entry containing
		 /// page id to the next.
		 /// <para>
		 /// Each entry in the the free list consist of a page id and the generation in which it was freed.
		 /// </para>
		 /// <para>
		 /// Read pointer cannot go beyond entries belonging to stable generation.
		 /// About the free-list id/offset variables below:
		 /// <pre>
		 /// Every cell in picture contains generation, page id is omitted for briefness.
		 /// StableGeneration   = 1
		 /// UnstableGeneration = 2
		 /// 
		 ///        <seealso cref="readPos"/>                         <seealso cref="writePos"/>
		 ///        v                               v
		 ///  ┌───┬───┬───┬───┬───┬───┐   ┌───┬───┬───┬───┬───┬───┐
		 ///  │ 1 │ 1 │ 1 │ 2 │ 2 │ 2 │-->│ 2 │ 2 │   │   │   │   │
		 ///  └───┴───┴───┴───┴───┴───┘   └───┴───┴───┴───┴───┴───┘
		 ///  ^                           ^
		 ///  <seealso cref="readPageId"/>                  <seealso cref="writePageId"/>
		 /// </pre>
		 /// </para>
		 /// </summary>
		 private volatile long _writePageId;
		 private volatile long _readPageId;
		 private volatile int _writePos;
		 private volatile int _readPos;

		 /// <summary>
		 /// Last allocated page id, used for allocating new ids as more data gets inserted into the tree.
		 /// </summary>
		 private volatile long _lastId;

		 /// <summary>
		 /// For monitoring internal free-list activity.
		 /// </summary>
		 private readonly Monitor _monitor;

		 internal FreeListIdProvider( PagedFile pagedFile, int pageSize, long lastId, Monitor monitor )
		 {
			  this._pagedFile = pagedFile;
			  this._monitor = monitor;
			  this._freelistNode = new FreelistNode( pageSize );
			  this._lastId = lastId;
		 }

		 internal virtual void Initialize( long lastId, long writePageId, long readPageId, int writePos, int readPos )
		 {
			  this._lastId = lastId;
			  this._writePageId = writePageId;
			  this._readPageId = readPageId;
			  this._writePos = writePos;
			  this._readPos = readPos;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void initializeAfterCreation() throws java.io.IOException
		 internal virtual void InitializeAfterCreation()
		 {
			  // Allocate a new free-list page id and set both write/read free-list page id to it.
			  _writePageId = NextLastId();
			  _readPageId = _writePageId;

			  using ( PageCursor cursor = _pagedFile.io( _writePageId, Org.Neo4j.Io.pagecache.PagedFile_Fields.PfSharedWriteLock ) )
			  {
					goTo( cursor, "free-list", _writePageId );
					FreelistNode.Initialize( cursor );
					checkOutOfBounds( cursor );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long acquireNewId(long stableGeneration, long unstableGeneration) throws java.io.IOException
		 public override long AcquireNewId( long stableGeneration, long unstableGeneration )
		 {
			  return AcquireNewId( stableGeneration, unstableGeneration, true );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long acquireNewId(long stableGeneration, long unstableGeneration, boolean allowTakeLastFromPage) throws java.io.IOException
		 private long AcquireNewId( long stableGeneration, long unstableGeneration, bool allowTakeLastFromPage )
		 {
			  // Acquire id from free-list or end of store file
			  long acquiredId = AcquireNewIdFromFreelistOrEnd( stableGeneration, unstableGeneration, allowTakeLastFromPage );

			  // Zap the page, i.e. set all bytes to zero
			  using ( PageCursor cursor = _pagedFile.io( acquiredId, Org.Neo4j.Io.pagecache.PagedFile_Fields.PfSharedWriteLock ) )
			  {
					PageCursorUtil.GoTo( cursor, "newly allocated free-list page", acquiredId );
					cursor.ZapPage();
					// don't initialize node here since this acquisition can be used both for tree nodes
					// as well as free-list nodes.
			  }
			  return acquiredId;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long acquireNewIdFromFreelistOrEnd(long stableGeneration, long unstableGeneration, boolean allowTakeLastFromPage) throws java.io.IOException
		 private long AcquireNewIdFromFreelistOrEnd( long stableGeneration, long unstableGeneration, bool allowTakeLastFromPage )
		 {
			  if ( ( _readPageId != _writePageId || _readPos < _writePos ) && ( allowTakeLastFromPage || _readPos < _freelistNode.maxEntries() - 1 ) )
			  {
					// It looks like reader isn't even caught up to the writer page-wise,
					// or the read pos is < write pos so check if we can grab the next id (generation could still mismatch).
					using ( PageCursor cursor = _pagedFile.io( _readPageId, Org.Neo4j.Io.pagecache.PagedFile_Fields.PF_SHARED_READ_LOCK ) )
					{
						 if ( !cursor.Next() )
						 {
							  throw new IOException( "Couldn't go to free-list read page " + _readPageId );
						 }

						 long resultPageId;
						 do
						 {
							  resultPageId = _freelistNode.read( cursor, stableGeneration, _readPos );
						 } while ( cursor.ShouldRetry() );

						 if ( resultPageId != FreelistNode.NoPageId )
						 {
							  // FreelistNode compares generation and so this means that we have an available
							  // id in the free list which we can acquire from a stable generation. Increment readPos
							  _readPos++;
							  if ( _readPos >= _freelistNode.maxEntries() )
							  {
									// The current reader page is exhausted, go to the next free-list page.
									_readPos = 0;
									do
									{
										 _readPageId = FreelistNode.Next( cursor );
									} while ( cursor.ShouldRetry() );

									// Put the exhausted free-list page id itself on the free-list
									long exhaustedFreelistPageId = cursor.CurrentPageId;
									ReleaseId( stableGeneration, unstableGeneration, exhaustedFreelistPageId );
									_monitor.releasedFreelistPageId( exhaustedFreelistPageId );
							  }
							  return resultPageId;
						 }
					}
			  }

			  // Fall-back to acquiring at the end of the file
			  return NextLastId();
		 }

		 private long NextLastId()
		 {
			  return ++_lastId;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void releaseId(long stableGeneration, long unstableGeneration, long id) throws java.io.IOException
		 public override void ReleaseId( long stableGeneration, long unstableGeneration, long id )
		 {
			  using ( PageCursor cursor = _pagedFile.io( _writePageId, Org.Neo4j.Io.pagecache.PagedFile_Fields.PfSharedWriteLock ) )
			  {
					PageCursorUtil.GoTo( cursor, "free-list write page", _writePageId );
					_freelistNode.write( cursor, unstableGeneration, id, _writePos );
					_writePos++;
			  }

			  if ( _writePos >= _freelistNode.maxEntries() )
			  {
					// Current free-list write page is full, allocate a new one.
					long nextFreelistPage = AcquireNewId( stableGeneration, unstableGeneration, false );
					using ( PageCursor cursor = _pagedFile.io( _writePageId, Org.Neo4j.Io.pagecache.PagedFile_Fields.PfSharedWriteLock ) )
					{
						 PageCursorUtil.GoTo( cursor, "free-list write page", _writePageId );
						 FreelistNode.Initialize( cursor );
						 // Link previous --> new writer page
						 FreelistNode.SetNext( cursor, nextFreelistPage );
					}
					_writePageId = nextFreelistPage;
					_writePos = 0;
					_monitor.acquiredFreelistPageId( nextFreelistPage );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void visitFreelist(IdProvider_IdProviderVisitor visitor) throws java.io.IOException
		 public override void VisitFreelist( IdProvider_IdProviderVisitor visitor )
		 {
			  if ( _readPageId == FreelistNode.NoPageId )
			  {
					return;
			  }

			  using ( PageCursor cursor = _pagedFile.io( 0, Org.Neo4j.Io.pagecache.PagedFile_Fields.PF_SHARED_READ_LOCK ) )
			  {
					GenerationKeeper generation = new GenerationKeeper();
					long prevPage;
					long pageId = _readPageId;
					int pos = _readPos;
					do
					{
						 PageCursorUtil.GoTo( cursor, "free-list", pageId );
						 visitor.BeginFreelistPage( pageId );
						 int targetPos = pageId == _writePageId ? _writePos : _freelistNode.maxEntries();
						 while ( pos < targetPos )
						 {
							  // Read next un-acquired id
							  long unacquiredId;
							  do
							  {
									unacquiredId = _freelistNode.read( cursor, long.MaxValue, pos, generation );
							  } while ( cursor.ShouldRetry() );
							  visitor.FreelistEntry( unacquiredId, generation.Generation, pos );
							  pos++;
						 }
						 visitor.EndFreelistPage( pageId );

						 prevPage = pageId;
						 pos = 0;
						 do
						 {
							  pageId = FreelistNode.Next( cursor );
						 } while ( cursor.ShouldRetry() );
					} while ( prevPage != _writePageId );
			  }
		 }

		 public override long LastId()
		 {
			  return _lastId;
		 }

		 internal virtual long WritePageId()
		 {
			  return _writePageId;
		 }

		 internal virtual long ReadPageId()
		 {
			  return _readPageId;
		 }

		 internal virtual int WritePos()
		 {
			  return _writePos;
		 }

		 internal virtual int ReadPos()
		 {
			  return _readPos;
		 }

		 // test-access method
		 internal virtual int EntriesPerPage()
		 {
			  return _freelistNode.maxEntries();
		 }
	}

}