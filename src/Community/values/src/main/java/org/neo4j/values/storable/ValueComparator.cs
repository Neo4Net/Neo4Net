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
namespace Neo4Net.Values.Storable
{

	using Neo4Net.Values;

	/// <summary>
	/// Comparator for values. Usable for sorting values, for example during index range scans.
	/// </summary>
	public sealed class ValueComparator : IComparer<Value>, TernaryComparator<Value>
	{
		 private readonly IComparer<ValueGroup> _valueGroupComparator;

		 internal ValueComparator( IComparer<ValueGroup> valueGroupComparator )
		 {
			  this._valueGroupComparator = valueGroupComparator;
		 }

		 public override int Compare( Value v1, Value v2 )
		 {
			  Debug.Assert( v1 != null && v2 != null, "null values are not supported, use NoValue.NO_VALUE instead" );

			  ValueGroup id1 = v1.ValueGroup();
			  ValueGroup id2 = v2.ValueGroup();

			  int x = _valueGroupComparator.Compare( id1, id2 );

			  if ( x == 0 )
			  {
					return v1.UnsafeCompareTo( v2 );
			  }
			  return x;
		 }

		 public override Comparison TernaryCompare( Value v1, Value v2 )
		 {
			  Debug.Assert( v1 != null && v2 != null, "null values are not supported, use NoValue.NO_VALUE instead" );

			  ValueGroup id1 = v1.ValueGroup();
			  ValueGroup id2 = v2.ValueGroup();

			  int x = _valueGroupComparator.Compare( id1, id2 );

			  if ( x == 0 )
			  {
					return v1.UnsafeTernaryCompareTo( v2 );
			  }
			  return Comparison.from( x );
		 }

		 public override bool Equals( object obj )
		 {
			  return obj is ValueComparator;
		 }

		 public override int GetHashCode()
		 {
			  return 1;
		 }
	}

}