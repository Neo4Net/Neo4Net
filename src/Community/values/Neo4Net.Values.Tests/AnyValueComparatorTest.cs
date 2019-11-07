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
namespace Neo4Net.Values
{
	using Test = org.junit.jupiter.api.Test;


	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using VirtualValueTestUtil = Neo4Net.Values.@virtual.VirtualValueTestUtil;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Integer.signum;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.DateTimeValue.datetime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.DateValue.date;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.DurationValue.duration;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.LocalDateTimeValue.localDateTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.LocalTimeValue.localTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.TimeValue.time;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.pointValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.stringArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.stringValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.@virtual.VirtualValueTestUtil.list;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.@virtual.VirtualValueTestUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.@virtual.VirtualValueTestUtil.nodes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.@virtual.VirtualValueTestUtil.relationships;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.@virtual.VirtualValues.emptyMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.@virtual.VirtualValues.node;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.@virtual.VirtualValues.nodeValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.@virtual.VirtualValues.path;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.@virtual.VirtualValues.relationship;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.@virtual.VirtualValues.relationshipValue;

	internal class AnyValueComparatorTest
	{
		 private static IComparer<AnyValue> _comparator = AnyValues.Comparator;

		 private object[] _objs = new object[]{ map(), map("1", 'a'), map("1", 'b'), map("2", 'a'), map("1", map("1", map("1", 'a')), "2", 'x'), map("1", map("1", map("1", 'b')), "2", 'x'), map("1", 'a', "2", 'b'), map("1", 'b', "2", map()), map("1", 'b', "2", map("10", 'a')), map("1", 'b', "2", map("10", 'b')), map("1", 'b', "2", map("20", 'a')), map("1", 'b', "2", 'a'), node(1L), nodeValue(2L, stringArray("L"), emptyMap()), node(3L), relationship(1L), relationshipValue(2L, nodeValue(1L, stringArray("L"), emptyMap()), nodeValue(2L, stringArray("L"), emptyMap()), stringValue("type"), emptyMap()), relationship(3L), list(), new string[]{ "a" }, new bool[]{ false }, list(1), list(1, 2), list(1, 3), list(2, 1), new short[]{ 2, 3 }, list(3), list(3, list(1)), list(3, list(1, 2)), list(3, list(2)), list(3, 1), new double[]{ 3.0, 2.0 }, list(4, list(1, list(1))), list(4, list(1, list(2))), new int[]{ 4, 1 }, path(nodes(1L), relationships()), path(nodes(1L, 2L), relationships(1L)), path(nodes(1L, 2L, 3L), relationships(1L, 2L)), path(nodes(1L, 2L, 3L), relationships(1L, 3L)), path(nodes(1L, 2L, 3L, 4L), relationships(1L, 3L, 4L)), path(nodes(1L, 2L, 3L, 4L), relationships(1L, 4L, 2L)), path(nodes(1L, 2L, 3L), relationships(2L, 3L)), path(nodes(1L, 2L), relationships(3L)), path(nodes(2L), relationships()), path(nodes(2L, 1L), relationships(1L)), path(nodes(3L), relationships()), path(nodes(4L, 5L), relationships(2L)), path(nodes(5L, 4L), relationships(2L)), pointValue(CoordinateReferenceSystem.Cartesian, 1.0, 1.0), datetime(2018, 2, 2, 0, 0, 0, 0, "+00:00"), localDateTime(2018, 2, 2, 0, 0, 0, 0), date(2018, 2, 1), time(12, 0, 0, 0, "+00:00"), localTime(0, 0, 0, 1), duration(0, 0, 0, 0), "hello", true, 1L, Math.PI, short.MaxValue, Double.NaN, null };

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldOrderValuesCorrectly()
		 internal virtual void ShouldOrderValuesCorrectly()
		 {
			  IList<AnyValue> values = java.util.objs.Select( VirtualValueTestUtil.toAnyValue ).ToList();

			  for ( int i = 0; i < values.Count; i++ )
			  {
					for ( int j = 0; j < values.Count; j++ )
					{
						 AnyValue left = values[i];
						 AnyValue right = values[j];

						 int cmpPos = signum( i - j );
						 int cmpVal = signum( Compare( _comparator, left, right ) );
						 assertEquals( cmpPos, cmpVal, format( "Comparing %s against %s does not agree with their positions in the sorted list (%d and %d)", left, right, i, j ) );
					}
			  }
		 }

		 private int Compare<T>( IComparer<T> comparator, T left, T right )
		 {
			  int cmp1 = comparator.Compare( left, right );
			  int cmp2 = comparator.Compare( right, left );
			  assertEquals( signum( cmp1 ), -signum( cmp2 ), format( "%s is not symmetric on %s and %s", comparator, left, right ) );
			  return cmp1;
		 }
	}

}