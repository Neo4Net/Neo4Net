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
namespace Neo4Net.Kernel.Impl.Newapi
{
	using LongSet = org.eclipse.collections.api.set.primitive.LongSet;

	using LabelSet = Neo4Net.@internal.Kernel.Api.LabelSet;

	public class Labels : LabelSet
	{
		 /// <summary>
		 /// This really only needs to be {@code int[]}, but the underlying implementation uses {@code long[]} for some
		 /// reason.
		 /// </summary>
		 private readonly long[] _labels;

		 private Labels( long[] labels )
		 {
			  this._labels = labels;
		 }

		 public static Labels From( long[] labels )
		 {
			  return new Labels( labels );
		 }

		 internal static Labels From( LongSet set )
		 {
			  return new Labels( set.toArray() );
		 }

		 public override int NumberOfLabels()
		 {
			  return _labels.Length;
		 }

		 public override int Label( int offset )
		 {
			  return ( int ) _labels[offset];
		 }

		 public override bool Contains( int labelToken )
		 {
			  //It may look tempting to use binary search
			  //however doing a linear search is actually faster for reasonable
			  //label sizes (≤100 labels)
			  foreach ( long label in _labels )
			  {
					assert( int ) label == label : "value too big to be represented as and int";
					if ( label == labelToken )
					{
						 return true;
					}
			  }
			  return false;
		 }

		 public override string ToString()
		 {
			  return "Labels" + Arrays.ToString( _labels );
		 }

		 public override long[] All()
		 {
			  return _labels;
		 }
	}

}