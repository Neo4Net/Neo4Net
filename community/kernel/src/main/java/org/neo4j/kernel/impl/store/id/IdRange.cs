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
namespace Org.Neo4j.Kernel.impl.store.id
{

	public class IdRange
	{
		 private readonly long[] _defragIds;
		 private readonly long _rangeStart;
		 private readonly int _rangeLength;

		 public IdRange( long[] defragIds, long rangeStart, int rangeLength )
		 {
			  this._defragIds = defragIds;
			  this._rangeStart = rangeStart;
			  this._rangeLength = rangeLength;
		 }

		 public virtual long[] DefragIds
		 {
			 get
			 {
				  return _defragIds;
			 }
		 }

		 public virtual long RangeStart
		 {
			 get
			 {
				  return _rangeStart;
			 }
		 }

		 public virtual int RangeLength
		 {
			 get
			 {
				  return _rangeLength;
			 }
		 }

		 public override string ToString()
		 {
			  return "IdRange[" + _rangeStart + "-" + ( _rangeStart + _rangeLength - 1 ) + ", defrag " + Arrays.ToString( _defragIds ) + "]";
		 }

		 public virtual int TotalSize()
		 {
			  return _defragIds.Length + _rangeLength;
		 }

		 public virtual IdRangeIterator Iterator()
		 {
			  return new IdRangeIterator( this );
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
			  IdRange idRange = ( IdRange ) o;
			  return _rangeStart == idRange._rangeStart && _rangeLength == idRange._rangeLength && Arrays.Equals( _defragIds, idRange._defragIds );
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( Arrays.GetHashCode( _defragIds ), _rangeStart, _rangeLength );
		 }

		 public virtual long HighId
		 {
			 get
			 {
				  return _rangeStart + _rangeLength;
			 }
		 }
	}

}