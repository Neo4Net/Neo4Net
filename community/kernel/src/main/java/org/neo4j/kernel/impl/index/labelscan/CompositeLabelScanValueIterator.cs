using System;
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
namespace Org.Neo4j.Kernel.impl.index.labelscan
{
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;


	using PrimitiveLongBaseIterator = Org.Neo4j.Collection.PrimitiveLongCollections.PrimitiveLongBaseIterator;
	using PrimitiveLongResourceIterator = Org.Neo4j.Collection.PrimitiveLongResourceIterator;
	using ResourceUtils = Org.Neo4j.Graphdb.ResourceUtils;

	/// <summary>
	/// <seealso cref="LongIterator"/> acting as a combining of multiple <seealso cref="LongIterator"/>
	/// for merging their results lazily as iterating commences. Both {@code AND} and {@code OR} merging is supported.
	/// <para>
	/// Source iterators must be sorted in ascending order.
	/// </para>
	/// </summary>
	internal class CompositeLabelScanValueIterator : PrimitiveLongBaseIterator, PrimitiveLongResourceIterator
	{
		 private readonly PriorityQueue<IdAndSource> _sortedIterators = new PriorityQueue<IdAndSource>();
		 private readonly int _atLeastNumberOfLabels;
		 private readonly IList<PrimitiveLongResourceIterator> _toClose;
		 private long _last = -1;

		 /// <summary>
		 /// Constructs a <seealso cref="CompositeLabelScanValueIterator"/>.
		 /// </summary>
		 /// <param name="iterators"> <seealso cref="LongIterator iterators"/> to merge. </param>
		 /// <param name="trueForAll"> if {@code true} using {@code AND} merging, otherwise {@code OR} merging. </param>
		 internal CompositeLabelScanValueIterator( IList<PrimitiveLongResourceIterator> iterators, bool trueForAll )
		 {
			  this._toClose = iterators;
			  this._atLeastNumberOfLabels = trueForAll ? iterators.Count : 1;
			  foreach ( LongIterator iterator in iterators )
			  {
					if ( iterator.hasNext() )
					{
						 _sortedIterators.add( new IdAndSource( iterator.next(), iterator ) );
					}
			  }
		 }

		 protected internal override bool FetchNext()
		 {
			  int numberOfLabels = 0;
			  long next = _last;
			  while ( next == _last || numberOfLabels < _atLeastNumberOfLabels )
			  {
					IdAndSource idAndSource = _sortedIterators.poll();
					if ( idAndSource == null )
					{
						 return false;
					}

					if ( idAndSource.LatestReturned == next )
					{
						 numberOfLabels++;
					}
					else
					{
						 next = idAndSource.LatestReturned;
						 numberOfLabels = 1;
					}

					if ( idAndSource.Source.hasNext() )
					{
						 idAndSource.LatestReturned = idAndSource.Source.next();
						 _sortedIterators.offer( idAndSource );
					}
			  }
			  _last = next;
			  next( _last );
			  return true;
		 }

		 public override void Close()
		 {
			  ResourceUtils.closeAll( _toClose );
			  _sortedIterators.clear();
			  _toClose.Clear();
		 }

		 private class IdAndSource : IComparable<IdAndSource>
		 {
			  internal long LatestReturned;
			  internal readonly LongIterator Source;

			  internal IdAndSource( long latestReturned, LongIterator source )
			  {
					this.LatestReturned = latestReturned;
					this.Source = source;
			  }

			  public override int CompareTo( IdAndSource o )
			  {
					int keyComparison = Long.compare( LatestReturned, o.LatestReturned );
					if ( keyComparison == 0 )
					{
						 return Integer.compare( Source.GetHashCode(), o.Source.GetHashCode() );
					}
					return keyComparison;
			  }

			  public override bool Equals( object o )
			  {
					if ( this == o )
					{
						 return true;
					}
					if ( o == null || this.GetType() != o.GetType() )
					{
						 return false;
					}
					IdAndSource that = ( IdAndSource ) o;
					return CompareTo( that ) == 0;
			  }

			  public override int GetHashCode()
			  {
					return Objects.hash( LatestReturned, Source );
			  }
		 }
	}

}