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
namespace Neo4Net.Index.Internal.gbptree
{

	using Monitor = Neo4Net.Index.Internal.gbptree.GBPTree.Monitor;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.index.Internal.gbptree.GBPTree.NO_HEADER_READER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.index.Internal.gbptree.GBPTree.NO_HEADER_WRITER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.index.Internal.gbptree.GBPTree.NO_MONITOR;

	/// <summary>
	/// Convenient builder for a <seealso cref="GBPTree"/>. Either created using zero-argument constructor for maximum
	/// flexibility, or with constructor with arguments considered mandatory to be able to build a proper tree.
	/// </summary>
	/// @param <KEY> type of key in <seealso cref="GBPTree"/> </param>
	/// @param <VALUE> type of value in <seealso cref="GBPTree"/> </param>
	public class GBPTreeBuilder<KEY, VALUE>
	{
		 private PageCache _pageCache;
		 private File _file;
		 private int _tentativeIndexPageSize;
		 private Monitor _monitor = NO_MONITOR;
		 private Header.Reader _headerReader = NO_HEADER_READER;
		 private Layout<KEY, VALUE> _layout;
		 private System.Action<PageCursor> _headerWriter = NO_HEADER_WRITER;
		 private RecoveryCleanupWorkCollector _recoveryCleanupWorkCollector = RecoveryCleanupWorkCollector.Immediate();
		 private bool _readOnly;

		 public GBPTreeBuilder( PageCache pageCache, File file, Layout<KEY, VALUE> layout )
		 {
			  With( pageCache );
			  With( file );
			  With( layout );
		 }

		 public virtual GBPTreeBuilder<KEY, VALUE> With( Layout<KEY, VALUE> layout )
		 {
			  this._layout = layout;
			  return this;
		 }

		 public virtual GBPTreeBuilder<KEY, VALUE> With( File file )
		 {
			  this._file = file;
			  return this;
		 }

		 public virtual GBPTreeBuilder<KEY, VALUE> With( PageCache pageCache )
		 {
			  this._pageCache = pageCache;
			  return this;
		 }

		 public virtual GBPTreeBuilder<KEY, VALUE> WithIndexPageSize( int tentativeIndexPageSize )
		 {
			  this._tentativeIndexPageSize = tentativeIndexPageSize;
			  return this;
		 }

		 public virtual GBPTreeBuilder<KEY, VALUE> With( GBPTree.Monitor monitor )
		 {
			  this._monitor = monitor;
			  return this;
		 }

		 public virtual GBPTreeBuilder<KEY, VALUE> With( Header.Reader headerReader )
		 {
			  this._headerReader = headerReader;
			  return this;
		 }

		 public virtual GBPTreeBuilder<KEY, VALUE> With( System.Action<PageCursor> headerWriter )
		 {
			  this._headerWriter = headerWriter;
			  return this;
		 }

		 public virtual GBPTreeBuilder<KEY, VALUE> With( RecoveryCleanupWorkCollector recoveryCleanupWorkCollector )
		 {
			  this._recoveryCleanupWorkCollector = recoveryCleanupWorkCollector;
			  return this;
		 }

		 public virtual GBPTreeBuilder<KEY, VALUE> WithReadOnly( bool readOnly )
		 {
			  this._readOnly = readOnly;
			  return this;
		 }

		 public virtual GBPTree<KEY, VALUE> Build()
		 {
			  return new GBPTree<KEY, VALUE>( _pageCache, _file, _layout, _tentativeIndexPageSize, _monitor, _headerReader, _headerWriter, _recoveryCleanupWorkCollector, _readOnly );
		 }
	}

}