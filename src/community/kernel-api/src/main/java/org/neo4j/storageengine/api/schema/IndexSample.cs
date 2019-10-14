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
namespace Neo4Net.Storageengine.Api.schema
{
	public sealed class IndexSample
	{
		 private readonly long _indexSize;
		 private readonly long _uniqueValues;
		 private readonly long _sampleSize;

		 public IndexSample() : this(0, 0, 0)
		 {
		 }

		 public IndexSample( long indexSize, long uniqueValues, long sampleSize )
		 {
			  this._indexSize = indexSize;
			  this._uniqueValues = uniqueValues;
			  this._sampleSize = sampleSize;
		 }

		 public long IndexSize()
		 {
			  return _indexSize;
		 }

		 public long UniqueValues()
		 {
			  return _uniqueValues;
		 }

		 public long SampleSize()
		 {
			  return _sampleSize;
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
			  IndexSample that = ( IndexSample ) o;
			  return _indexSize == that._indexSize && _uniqueValues == that._uniqueValues && _sampleSize == that._sampleSize;
		 }

		 public override int GetHashCode()
		 {
			  int result = ( int )( _indexSize ^ ( ( long )( ( ulong )_indexSize >> 32 ) ) );
			  result = 31 * result + ( int )( _uniqueValues ^ ( ( long )( ( ulong )_uniqueValues >> 32 ) ) );
			  result = 31 * result + ( int )( _sampleSize ^ ( ( long )( ( ulong )_sampleSize >> 32 ) ) );
			  return result;
		 }

		 public override string ToString()
		 {
			  return "IndexSample{" +
						"indexSize=" + _indexSize +
						", uniqueValues=" + _uniqueValues +
						", sampleSize=" + _sampleSize +
						'}';
		 }
	}

}