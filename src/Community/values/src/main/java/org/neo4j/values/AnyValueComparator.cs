using System.Collections.Generic;
using System.Diagnostics;

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
namespace Neo4Net.Values
{

	using Value = Neo4Net.Values.Storable.Value;
	using ValueComparator = Neo4Net.Values.Storable.ValueComparator;
	using Values = Neo4Net.Values.Storable.Values;
	using VirtualValueGroup = Neo4Net.Values.@virtual.VirtualValueGroup;

	/// <summary>
	/// Comparator for any values.
	/// </summary>
	internal class AnyValueComparator : IComparer<AnyValue>, TernaryComparator<AnyValue>
	{
		 private readonly IComparer<VirtualValueGroup> _virtualValueGroupComparator;
		 private readonly ValueComparator _valueComparator;

		 internal AnyValueComparator( ValueComparator valueComparator, IComparer<VirtualValueGroup> virtualValueGroupComparator )
		 {
			  this._virtualValueGroupComparator = virtualValueGroupComparator;
			  this._valueComparator = valueComparator;
		 }

		 private Comparison Cmp( AnyValue v1, AnyValue v2, bool ternary )
		 {
			  Debug.Assert( v1 != null && v2 != null, "null values are not supported, use NoValue.NO_VALUE instead" );

			  // NO_VALUE is bigger than all other values, need to check for that up
			  // front
			  if ( v1 == v2 )
			  {
					return Comparison.Equal;
			  }
			  if ( v1 == Values.NO_VALUE )
			  {
					return Comparison.GreaterThan;
			  }
			  if ( v2 == Values.NO_VALUE )
			  {
					return Comparison.SmallerThan;
			  }

			  // We must handle sequences as a special case, as they can be both storable and virtual
			  bool isSequence1 = v1.SequenceValue;
			  bool isSequence2 = v2.SequenceValue;

			  if ( isSequence1 && isSequence2 )
			  {
					return Comparison.from( CompareSequences( ( SequenceValue ) v1, ( SequenceValue ) v2 ) );
			  }
			  else if ( isSequence1 )
			  {
					return Comparison.from( CompareSequenceAndNonSequence( ( SequenceValue ) v1, v2 ) );
			  }
			  else if ( isSequence2 )
			  {
					return Comparison.from( -CompareSequenceAndNonSequence( ( SequenceValue ) v2, v1 ) );
			  }

			  // Handle remaining AnyValues
			  bool isValue1 = v1 is Value;
			  bool isValue2 = v2 is Value;

			  int x = Boolean.compare( isValue1, isValue2 );

			  if ( x == 0 )
			  {
					//noinspection ConstantConditions
					// Do not turn this into ?-operator
					if ( isValue1 )
					{
						 if ( ternary )
						 {
							  return _valueComparator.ternaryCompare( ( Value ) v1, ( Value ) v2 );
						 }
						 else
						 {
							  return Comparison.from( _valueComparator.Compare( ( Value ) v1, ( Value ) v2 ) );
						 }
					}
					else
					{
						 // This returns int
						 return Comparison.from( CompareVirtualValues( ( VirtualValue ) v1, ( VirtualValue ) v2 ) );
					}

			  }
			  return Comparison.from( x );
		 }

		 public override int Compare( AnyValue v1, AnyValue v2 )
		 {
			  return Cmp( v1, v2, false ).value();
		 }

		 public override Comparison TernaryCompare( AnyValue v1, AnyValue v2 )
		 {
			  return Cmp( v1, v2, true );
		 }

		 public override bool Equals( object obj )
		 {
			  return obj is AnyValueComparator;
		 }

		 public override int GetHashCode()
		 {
			  return 1;
		 }

		 private int CompareVirtualValues( VirtualValue v1, VirtualValue v2 )
		 {
			  VirtualValueGroup id1 = v1.ValueGroup();
			  VirtualValueGroup id2 = v2.ValueGroup();

			  int x = _virtualValueGroupComparator.Compare( id1, id2 );

			  if ( x == 0 )
			  {
					return v1.CompareTo( v2, this );
			  }
			  return x;
		 }

		 private int CompareSequenceAndNonSequence( SequenceValue v1, AnyValue v2 )
		 {
			  bool isValue2 = v2 is Value;
			  if ( isValue2 )
			  {
					return -1;
			  }
			  else
			  {
					return _virtualValueGroupComparator.Compare( VirtualValueGroup.LIST, ( ( VirtualValue ) v2 ).ValueGroup() );
			  }
		 }

		 private int CompareSequences( SequenceValue v1, SequenceValue v2 )
		 {
			  return v1.CompareToSequence( v2, this );
		 }
	}

}