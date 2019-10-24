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
namespace Neo4Net.Kernel.impl.index.labelscan
{
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;


	using PrimitiveLongResourceIterator = Neo4Net.Collections.PrimitiveLongResourceIterator;
	using Neo4Net.Cursors;
	using Neo4Net.GraphDb.Index;
	using Neo4Net.Index.Internal.gbptree;
	using Neo4Net.Index.Internal.gbptree;
	using IndexProgressor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexProgressor;
	using LabelScanReader = Neo4Net.Kernel.Api.StorageEngine.schema.LabelScanReader;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.labelscan.NativeLabelScanWriter.rangeOf;

	/// <summary>
	/// <seealso cref="LabelScanReader"/> for reading data from <seealso cref="NativeLabelScanStore"/>.
	/// Each <seealso cref="LongIterator"/> returned from each of the methods is backed by <seealso cref="RawCursor"/>
	/// directly from <seealso cref="GBPTree.seek(object, object)"/>.
	/// <para>
	/// The returned <seealso cref="LongIterator"/> aren't closable so the cursors retrieved are managed
	/// inside of this reader and closed between each new query and on <seealso cref="close()"/>.
	/// </para>
	/// </summary>
	internal class NativeLabelScanReader : LabelScanReader
	{
		 /// <summary>
		 /// <seealso cref="Index"/> which is queried when calling the methods below.
		 /// </summary>
		 private readonly GBPTree<LabelScanKey, LabelScanValue> _index;

		 /// <summary>
		 /// Currently open <seealso cref="RawCursor"/> from query methods below. Open cursors are closed when calling
		 /// new query methods or when <seealso cref="close() closing"/> this reader.
		 /// </summary>
		 private readonly ISet<RawCursor<Hit<LabelScanKey, LabelScanValue>, IOException>> _openCursors;

		 internal NativeLabelScanReader( GBPTree<LabelScanKey, LabelScanValue> index )
		 {
			  this._index = index;
			  this._openCursors = new HashSet<RawCursor<Hit<LabelScanKey, LabelScanValue>, IOException>>();
		 }

		 /// <summary>
		 /// Closes all currently open <seealso cref="RawCursor cursors"/> from last query method call.
		 /// </summary>
		 public override void Close()
		 {
			  try
			  {
					EnsureOpenCursorsClosed();
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 public override PrimitiveLongResourceIterator NodesWithLabel( int labelId )
		 {
			  IRawCursor<Hit<LabelScanKey, LabelScanValue>, IOException> cursor;
			  try
			  {
					cursor = SeekerForLabel( 0, labelId );
					_openCursors.Add( cursor );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }

			  return new LabelScanValueIterator( cursor, _openCursors, Neo4Net.Kernel.Api.StorageEngine.schema.LabelScanReader_Fields.NO_ID );
		 }

		 public override PrimitiveLongResourceIterator NodesWithAnyOfLabels( long fromId, params int[] labelIds )
		 {
			  IList<PrimitiveLongResourceIterator> iterators = IteratorsForLabels( fromId, labelIds );
			  return new CompositeLabelScanValueIterator( iterators, false );
		 }

		 public override PrimitiveLongResourceIterator NodesWithAllLabels( params int[] labelIds )
		 {
			  IList<PrimitiveLongResourceIterator> iterators = IteratorsForLabels( Neo4Net.Kernel.Api.StorageEngine.schema.LabelScanReader_Fields.NO_ID, labelIds );
			  return new CompositeLabelScanValueIterator( iterators, true );
		 }

		 public override void NodesWithLabel( Neo4Net.Kernel.Api.StorageEngine.schema.IndexProgressor_NodeLabelClient client, int labelId )
		 {
			  IRawCursor<Hit<LabelScanKey, LabelScanValue>, IOException> cursor;
			  try
			  {
					cursor = SeekerForLabel( 0, labelId );
					_openCursors.Add( cursor );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }

			  client.Scan( new LabelScanValueIndexProgressor( cursor, _openCursors, client ), false, labelId );
		 }

		 private IList<PrimitiveLongResourceIterator> IteratorsForLabels( long fromId, int[] labelIds )
		 {
			  IList<PrimitiveLongResourceIterator> iterators = new List<PrimitiveLongResourceIterator>();
			  try
			  {
					foreach ( int labelId in labelIds )
					{
						 IRawCursor<Hit<LabelScanKey, LabelScanValue>, IOException> cursor = SeekerForLabel( fromId, labelId );
						 _openCursors.Add( cursor );
						 iterators.Add( new LabelScanValueIterator( cursor, _openCursors, fromId ) );
					}
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
			  return iterators;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.cursor.RawCursor<org.Neo4Net.index.internal.gbptree.Hit<LabelScanKey,LabelScanValue>,java.io.IOException> seekerForLabel(long startId, int labelId) throws java.io.IOException
		 private IRawCursor<Hit<LabelScanKey, LabelScanValue>, IOException> SeekerForLabel( long startId, int labelId )
		 {
			  LabelScanKey from = new LabelScanKey( labelId, rangeOf( startId ) );
			  LabelScanKey to = new LabelScanKey( labelId, long.MaxValue );
			  return _index.seek( from, to );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void ensureOpenCursorsClosed() throws java.io.IOException
		 private void EnsureOpenCursorsClosed()
		 {
			  foreach ( IRawCursor<Hit<LabelScanKey, LabelScanValue>, IOException> cursor in _openCursors )
			  {
					cursor.Close();
			  }
			  _openCursors.Clear();
		 }
	}

}