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
namespace Org.Neo4j.Consistency.checking.full
{

	using Org.Neo4j.Helpers.Collection;
	using AllEntriesLabelScanReader = Org.Neo4j.Kernel.api.labelscan.AllEntriesLabelScanReader;
	using NodeLabelRange = Org.Neo4j.Kernel.api.labelscan.NodeLabelRange;

	/// <summary>
	/// Inserts empty <seealso cref="NodeLabelRange"/> for those ranges missing from the source iterator.
	/// High node id is known up front such that ranges are returned up to that point.
	/// </summary>
	internal class GapFreeAllEntriesLabelScanReader : AllEntriesLabelScanReader
	{
		 private readonly AllEntriesLabelScanReader _nodeLabelRanges;
		 private readonly long _highId;

		 internal GapFreeAllEntriesLabelScanReader( AllEntriesLabelScanReader nodeLabelRanges, long highId )
		 {
			  this._nodeLabelRanges = nodeLabelRanges;
			  this._highId = highId;
		 }

		 public override long MaxCount()
		 {
			  return _nodeLabelRanges.maxCount();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws Exception
		 public override void Close()
		 {
			  _nodeLabelRanges.close();
		 }

		 public override int RangeSize()
		 {
			  return _nodeLabelRanges.rangeSize();
		 }

		 public override IEnumerator<NodeLabelRange> Iterator()
		 {
			  return new GapFillingIterator( _nodeLabelRanges.GetEnumerator(), (_highId - 1) / _nodeLabelRanges.rangeSize(), _nodeLabelRanges.rangeSize() );
		 }

		 private class GapFillingIterator : PrefetchingIterator<NodeLabelRange>
		 {
			  internal readonly long HighestRangeId;
			  internal readonly IEnumerator<NodeLabelRange> Source;
			  internal readonly long[][] EmptyRangeData;

			  internal NodeLabelRange NextFromSource;
			  internal long CurrentRangeId = -1;

			  internal GapFillingIterator( IEnumerator<NodeLabelRange> nodeLabelRangeIterator, long highestRangeId, int rangeSize )
			  {
					this.HighestRangeId = highestRangeId;
					this.Source = nodeLabelRangeIterator;
					this.EmptyRangeData = new long[rangeSize][];
			  }

			  protected internal override NodeLabelRange FetchNextOrNull()
			  {
					while ( true )
					{
						 // These conditions only come into play after we've gotten the first range from the source
						 if ( NextFromSource != null )
						 {
							  if ( CurrentRangeId + 1 == NextFromSource.id() )
							  {
									// Next to return is the one from source
									CurrentRangeId++;
									return NextFromSource;
							  }

							  if ( CurrentRangeId < NextFromSource.id() )
							  {
									// Source range iterator has a gap we need to fill
									return new NodeLabelRange( ++CurrentRangeId, EmptyRangeData );
							  }
						 }

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 if ( Source.hasNext() )
						 {
							  // The source iterator has more ranges, grab the next one
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
							  NextFromSource = Source.next();
							  // continue in the outer loop
						 }
						 else if ( CurrentRangeId < HighestRangeId )
						 {
							  NextFromSource = new NodeLabelRange( HighestRangeId, EmptyRangeData );
							  // continue in the outer loop
						 }
						 else
						 {
							  // End has been reached
							  return null;
						 }
					}
			  }
		 }
	}

}