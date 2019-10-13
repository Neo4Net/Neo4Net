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
namespace Neo4Net.Index.impl.lucene.@explicit
{

	using PrimitiveLongCollections = Neo4Net.Collections.PrimitiveLongCollections;
	using ExplicitIndexHits = Neo4Net.Kernel.api.ExplicitIndexHits;

	public class CombinedIndexHits : PrimitiveLongCollections.PrimitiveLongConcatingIterator, ExplicitIndexHits
	{
		 private readonly ICollection<ExplicitIndexHits> _allIndexHits;
		 private readonly int _size;

		 public CombinedIndexHits( ICollection<ExplicitIndexHits> iterators ) : base( iterators.GetEnumerator() )
		 {
			  this._allIndexHits = iterators;
			  _size = AccumulatedSize( iterators );
		 }

		 private int AccumulatedSize( ICollection<ExplicitIndexHits> iterators )
		 {
			  int result = 0;
			  foreach ( ExplicitIndexHits hits in iterators )
			  {
					result += hits.Size();
			  }
			  return result;
		 }

		 public override int Size()
		 {
			  return _size;
		 }

		 public override void Close()
		 {
			  foreach ( ExplicitIndexHits hits in _allIndexHits )
			  {
					hits.Close();
			  }
		 }

		 public override float CurrentScore()
		 {
			  return ( ( ExplicitIndexHits )CurrentIterator() ).currentScore();
		 }
	}

}