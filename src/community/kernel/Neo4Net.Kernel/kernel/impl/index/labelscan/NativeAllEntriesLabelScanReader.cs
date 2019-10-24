using System;
using System.Collections.Generic;
using System.Diagnostics;

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
	using MutableLongList = org.eclipse.collections.api.list.primitive.MutableLongList;


	using Neo4Net.Cursors;
	using Neo4Net.Collections.Helpers;
	using Neo4Net.Index.Internal.gbptree;
	using Neo4Net.Index.Internal.gbptree;
	using AllEntriesLabelScanReader = Neo4Net.Kernel.api.labelscan.AllEntriesLabelScanReader;
	using NodeLabelRange = Neo4Net.Kernel.api.labelscan.NodeLabelRange;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Long.min;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.fill;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.labelscan.NodeLabelRange.convertState;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.labelscan.NodeLabelRange.readBitmap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.labelscan.LabelScanValue.RANGE_SIZE;

	/// <summary>
	/// <seealso cref="AllEntriesLabelScanReader"/> for <seealso cref="NativeLabelScanStore"/>.
	/// <para>
	/// <seealso cref="NativeLabelScanStore"/> uses <seealso cref="GBPTree"/> for storage and it doesn't have means of aggregating
	/// results, so the approach this implementation is taking is to create one (lazy) seek cursor per label id
	/// and coordinate those simultaneously over the scan. Each <seealso cref="NodeLabelRange"/> returned is a view
	/// over all cursors at that same range, giving an aggregation of all labels in that node id range.
	/// </para>
	/// </summary>
	internal class NativeAllEntriesLabelScanReader : AllEntriesLabelScanReader
	{
		 private readonly System.Func<int, IRawCursor<Hit<LabelScanKey, LabelScanValue>, IOException>> _seekProvider;
		 private readonly IList<RawCursor<Hit<LabelScanKey, LabelScanValue>, IOException>> _cursors = new List<RawCursor<Hit<LabelScanKey, LabelScanValue>, IOException>>();
		 private readonly int _highestLabelId;

		 internal NativeAllEntriesLabelScanReader( System.Func<int, IRawCursor<Hit<LabelScanKey, LabelScanValue>, IOException>> seekProvider, int highestLabelId )
		 {
			  this._seekProvider = seekProvider;
			  this._highestLabelId = highestLabelId;
		 }

		 public override long MaxCount()
		 {
			  return Neo4Net.Collections.Helpers.BoundedIterable_Fields.UNKNOWN_MAX_COUNT;
		 }

		 public override int RangeSize()
		 {
			  return RANGE_SIZE;
		 }

		 public override IEnumerator<NodeLabelRange> Iterator()
		 {
			  try
			  {
					long lowestRange = long.MaxValue;
					CloseCursors();
					for ( int labelId = 0; labelId <= _highestLabelId; labelId++ )
					{
						 IRawCursor<Hit<LabelScanKey, LabelScanValue>, IOException> cursor = _seekProvider.apply( labelId );

						 // Bootstrap the cursor, which also provides a great opportunity to exclude if empty
						 if ( cursor.Next() )
						 {
							  lowestRange = min( lowestRange, cursor.get().key().idRange );
							  _cursors.Add( cursor );
						 }
					}
					return new NodeLabelRangeIterator( this, lowestRange );
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void closeCursors() throws java.io.IOException
		 private void CloseCursors()
		 {
			  foreach ( IRawCursor<Hit<LabelScanKey, LabelScanValue>, IOException> cursor in _cursors )
			  {
					cursor.Close();
			  }
			  _cursors.Clear();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws Exception
		 public override void Close()
		 {
			  CloseCursors();
		 }

		 /// <summary>
		 /// The main iterator over <seealso cref="NodeLabelRange ranges"/>, aggregating all the cursors as it goes.
		 /// </summary>
		 private class NodeLabelRangeIterator : PrefetchingIterator<NodeLabelRange>
		 {
			 private readonly NativeAllEntriesLabelScanReader _outerInstance;

			  internal long CurrentRange;

			  // nodeId (relative to lowestRange) --> labelId[]
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private final org.eclipse.collections.api.list.primitive.MutableLongList[] labelsForEachNode = new org.eclipse.collections.api.list.primitive.MutableLongList[RANGE_SIZE];
			  internal readonly MutableLongList[] LabelsForEachNode = new MutableLongList[RANGE_SIZE];

			  internal NodeLabelRangeIterator( NativeAllEntriesLabelScanReader outerInstance, long lowestRange )
			  {
				  this._outerInstance = outerInstance;
					this.CurrentRange = lowestRange;
			  }

			  protected internal override NodeLabelRange FetchNextOrNull()
			  {
					if ( CurrentRange == long.MaxValue )
					{
						 return null;
					}

					fill( LabelsForEachNode, null );
					long nextLowestRange = long.MaxValue;
					try
					{
						 // One "rangeSize" range at a time
						 foreach ( IRawCursor<Hit<LabelScanKey, LabelScanValue>, IOException> cursor in outerInstance.cursors )
						 {
							  long idRange = cursor.get().key().idRange;
							  if ( idRange < CurrentRange )
							  {
									// This should only happen if the cursor has been exhausted and the iterator have moved on
									// from the range it returned as its last hit.
									Debug.Assert( !cursor.Next() );
							  }
							  else if ( idRange == CurrentRange )
							  {
									long bits = cursor.get().value().bits;
									long labelId = cursor.get().key().labelId;
									readBitmap( bits, labelId, LabelsForEachNode );

									// Advance cursor and look ahead to the next range
									if ( cursor.Next() )
									{
										 nextLowestRange = min( nextLowestRange, cursor.get().key().idRange );
									}
							  }
							  else
							  {
									// Excluded from this range
									nextLowestRange = min( nextLowestRange, cursor.get().key().idRange );
							  }
						 }

						 NodeLabelRange range = new NodeLabelRange( CurrentRange, convertState( LabelsForEachNode ) );
						 CurrentRange = nextLowestRange;

						 return range;
					}
					catch ( IOException e )
					{
						 throw new Exception( e );
					}
			  }
		 }
	}

}