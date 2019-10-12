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
namespace Neo4Net.Graphdb.index
{
	/// <summary>
	/// This class is simply a progress counter of indexing population progress. It has the constraint that
	/// {@code 0 <= completed <= total}
	/// <para>
	/// Use IndexPopulationProgress.NONE if you need an object without any particular progress.
	/// </para>
	/// </summary>
	public class IndexPopulationProgress
	{
		 public static readonly IndexPopulationProgress None = new IndexPopulationProgress( 0, 0 );
		 public static readonly IndexPopulationProgress Done = new IndexPopulationProgress( 1, 1 );

		 private readonly long _completedCount;
		 private readonly long _totalCount;

		 public IndexPopulationProgress( long completed, long total )
		 {
			  if ( total < 0 || completed < 0 || completed > total )
			  {
					throw new System.ArgumentException( "Invalid progress specified: " + completed + "/" + total );
			  }
			  this._completedCount = completed;
			  this._totalCount = total;
		 }

		 /// <returns> percentage (from 0 to 100) of totalCount items which have been indexed. If totalCount is 0, returns 0. </returns>
		 public virtual float CompletedPercentage
		 {
			 get
			 {
				  return _totalCount > 0 ? ( ( float )( _completedCount * 100 ) / _totalCount ) : 0.0f;
			 }
		 }

		 /// <returns> number of completed items </returns>
		 /// @deprecated since this number won't be reliable throughout a population and should therefore not be used. 
		 public virtual long CompletedCount
		 {
			 get
			 {
				  return _completedCount;
			 }
		 }

		 /// <returns> total number of items to index </returns>
		 /// @deprecated since this number won't be reliable throughout a population and should therefore not be used. 
		 public virtual long TotalCount
		 {
			 get
			 {
				  return _totalCount;
			 }
		 }

		 public override string ToString()
		 {
			  return string.Format( "{0,1:F1}%", CompletedPercentage );
		 }
	}

}