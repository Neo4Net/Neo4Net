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
namespace Org.Neo4j.@unsafe.Impl.Batchimport.cache
{
	/// <summary>
	/// Contains basic functionality of fixed size number arrays.
	/// </summary>
	internal abstract class BaseNumberArray<N> : NumberArray<N> where N : NumberArray<N>
	{
		public abstract void Close();
		public abstract void Clear();
		public abstract void Swap( long fromIndex, long toIndex );
		public abstract long Length();
		 protected internal readonly int ItemSize;
		 protected internal readonly long Base;

		 /// <param name="itemSize"> byte size of each item in this array. </param>
		 /// <param name="base"> base index to rebase all indexes in accessor methods off of. See <seealso cref="at(long)"/>. </param>
		 protected internal BaseNumberArray( int itemSize, long @base )
		 {
			  this.ItemSize = itemSize;
			  this.Base = @base;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Override public N at(long index)
		 public override N At( long index )
		 {
			  return ( N )this;
		 }

		 /// <summary>
		 /// Utility for rebasing an external index to internal index. </summary>
		 /// <param name="index"> external index. </param>
		 /// <returns> index into internal data structure. </returns>
		 protected internal virtual long Rebase( long index )
		 {
			  return index - Base;
		 }
	}

}