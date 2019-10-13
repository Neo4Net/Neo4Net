﻿using System;
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
namespace Neo4Net.Collections.primitive.hopscotch
{
	using Test = org.junit.jupiter.api.Test;


	using Neo4Net.Collections.primitive;
	using Neo4Net.Collections.primitive;
	using Neo4Net.Collections.primitive;
	using Neo4Net.Collections.primitive;
	using Neo4Net.Collections.primitive;
	using Neo4Net.Collections.primitive;
	using Neo4Net.Collections.primitive;
	using Neo4Net.Collections.primitive;
	using Neo4Net.Collections.primitive;
	using GlobalMemoryTracker = Neo4Net.Memory.GlobalMemoryTracker;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;

	internal class PrimitiveLongMapTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldContainAddedValues()
		 internal virtual void ShouldContainAddedValues()
		 {
			  // GIVEN
			  IDictionary<long, int> expectedEntries = new Dictionary<long, int>();
			  expectedEntries[1994703545L] = 59;
			  expectedEntries[1583732120L] = 98;
			  expectedEntries[756530774L] = 56;
			  expectedEntries[1433091763L] = 22;

			  PrimitiveLongObjectMap<int> map = Primitive.longObjectMap();
			  foreach ( KeyValuePair<long, int> entry in expectedEntries.SetOfKeyValuePairs() )
			  {
					map.Put( entry.Key, entry.Value );
			  }

			  // WHEN/THEN
			  bool existedBefore = map.ContainsKey( 1433091763 );
			  int? valueBefore = map.Get( 1433091763 );
			  int? previous = map.Put( 1433091763, 35 );
			  bool existsAfter = map.ContainsKey( 1433091763 );
			  int? valueAfter = map.Get( 1433091763 );
			  assertTrue( existedBefore, "1433091763 should exist before putting here" );
			  assertEquals( ( int? ) 22, valueBefore );
			  assertEquals( ( int? ) 22, previous );
			  assertTrue( existsAfter, "(1433091763, 35) should exist" );
			  assertEquals( ( int? ) 35, valueAfter );
			  expectedEntries[1433091763L] = 35;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Map<long, int> visitedEntries = new java.util.HashMap<>();
			  IDictionary<long, int> visitedEntries = new Dictionary<long, int>();
			  map.VisitEntries((key, value) =>
			  {
				visitedEntries[key] = value;
				return false;
			  });
			  assertEquals( expectedEntries, visitedEntries );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldContainAddedValues_2()
		 internal virtual void ShouldContainAddedValues_2()
		 {
			  // GIVEN
			  PrimitiveLongObjectMap<int> map = Primitive.longObjectMap();
			  map.Put( 1950736976, 4 );
			  map.Put( 1054824202, 58 );
			  map.Put( 348690619, 54 );
			  map.Put( 1224909480, 79 );
			  map.Put( 1508493474, 82 );
			  // WHEN/THEN
			  bool existedBefore = map.ContainsKey( 1508493474 );
			  int? valueBefore = map.Get( 1508493474 );
			  int? previous = map.Put( 1508493474, 62 );
			  bool existsAfter = map.ContainsKey( 1508493474 );
			  int? valueAfter = map.Get( 1508493474 );
			  assertTrue( existedBefore, "1508493474 should exist before putting here" );
			  assertEquals( ( int? ) 82, valueBefore, "value before should be 82" );
			  assertEquals( ( int? ) 82, previous, "value returned from put should be 82" );
			  assertTrue( existsAfter, "1508493474 should exist" );
			  assertEquals( ( int? ) 62, valueAfter, "value after putting should be 62" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldContainAddedValues_3()
		 internal virtual void ShouldContainAddedValues_3()
		 {
			  // GIVEN
			  PrimitiveLongObjectMap<int> map = Primitive.longObjectMap();
			  map.Remove( 1338037218 );
			  map.Put( 680125236, 83 );
			  map.Put( 680125236, 76 );
			  map.Put( 680125236, 47 );
			  map.Put( 680125236, 30 );
			  map.Put( 2080483597, 52 );
			  map.Put( 867107519, 80 );
			  map.Remove( 710100384 );
			  map.Put( 671477921, 88 );
			  map.Put( 1163609643, 17 );
			  map.Put( 680125236, 57 );
			  map.Put( 1163609643, 70 );
			  map.Put( 2080483597, 89 );
			  map.Put( 1472451898, 62 );
			  map.Put( 1379499183, 93 );
			  map.Put( 680125236, 17 );
			  map.Put( 567842571, 43 );
			  map.Put( 2045599221, 60 );
			  map.Remove( 641295711 );
			  map.Remove( 867107519 );
			  map.Put( 2045599221, 30 );
			  map.Remove( 2094689486 );
			  map.Put( 1572965945, 79 );
			  map.Remove( 1329473388 );
			  map.Put( 1572965945, 39 );
			  map.Put( 264067586, 60 );
			  map.Put( 1751846500, 5 );
			  map.Put( 1163609643, 25 );
			  map.Put( 1379499183, 54 );
			  map.Remove( 671477921 );
			  map.Put( 1572965945, 59 );
			  map.Put( 880140639, 87 );
			  // WHEN/THEN
			  bool existedBefore = map.ContainsKey( 468007595 );
			  int? valueBefore = map.Get( 468007595 );
			  int? previous = map.Put( 468007595, 67 );
			  bool existsAfter = map.ContainsKey( 468007595 );
			  int? valueAfter = map.Get( 468007595 );
			  assertFalse( existedBefore, "468007595 should not exist before putting here" );
			  assertNull( valueBefore, "value before putting should be null" );
			  assertNull( previous, "value returned from putting should be null" );
			  assertTrue( existsAfter, "468007595 should exist" );
			  assertEquals( ( int? ) 67, valueAfter, "value after putting should be 67" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHaveCorrectSize()
		 internal virtual void ShouldHaveCorrectSize()
		 {
			  // GIVEN
			  PrimitiveLongObjectMap<int> map = Primitive.longObjectMap();
			  map.Put( 152407843, 17 );
			  map.Put( 435803197, 29 );
			  map.Put( 2063473573, 75 );
			  map.Put( 162922679, 36 );
			  map.Put( 923042422, 47 );
			  map.Put( 204556993, 28 );
			  map.Put( 109670524, 80 );
			  map.Put( 214127443, 88 );
			  map.Put( 297958695, 97 );
			  map.Put( 873122371, 73 );
			  map.Put( 398704786, 25 );
			  map.Put( 376378917, 62 );
			  map.Put( 1948985185, 3 );
			  map.Put( 918339266, 4 );
			  map.Put( 1126937431, 48 );
			  map.Put( 568627750, 6 );
			  map.Put( 887668742, 1 );
			  map.Put( 888089153, 88 );
			  map.Put( 1671871078, 26 );
			  map.Put( 479217936, 11 );
			  map.Put( 1874408328, 56 );
			  map.Put( 1517450283, 83 );
			  map.Put( 1352952211, 57 );
			  map.Put( 686066722, 92 );
			  map.Put( 1593196310, 71 );
			  map.Put( 1535351391, 62 );
			  map.Put( 296560052, 59 );
			  map.Put( 1513542622, 49 );
			  map.Put( 1899330306, 57 );
			  map.Put( 746190595, 31 );
			  map.Put( 1216091366, 90 );
			  map.Put( 353922939, 16 );
			  map.Put( 680935464, 16 );
			  map.Put( 235368309, 8 );
			  map.Put( 1988133681, 32 );
			  map.Put( 330747855, 81 );
			  map.Put( 492627887, 74 );
			  map.Put( 1005495348, 8 );
			  map.Put( 2107419277, 82 );
			  map.Put( 1421265494, 15 );
			  map.Put( 1669915469, 92 );
			  map.Put( 2008247215, 9 );
			  map.Put( 2010142383, 77 );
			  map.Put( 829081830, 25 );
			  map.Put( 1349259272, 38 );
			  map.Put( 1987482877, 8 );
			  map.Put( 974334859, 83 );
			  map.Put( 1376908873, 10 );
			  map.Put( 2120105656, 22 );
			  map.Put( 1634193445, 8 );
			  map.Put( 1160987255, 34 );
			  map.Put( 2030156381, 16 );
			  map.Put( 2012943328, 22 );
			  map.Put( 75749275, 54 );
			  map.Put( 1415817090, 35 );
			  map.Put( 562352348, 43 );
			  map.Put( 658501173, 96 );
			  map.Put( 441278652, 24 );
			  map.Put( 633855945, 82 );
			  map.Put( 579807215, 31 );
			  map.Put( 1125922962, 33 );
			  map.Put( 1995076951, 91 );
			  map.Put( 322776761, 4 );
			  map.Put( 1011369342, 36 );
			  // WHEN/THEN
			  int sizeBefore = map.Size();
			  bool existedBefore = map.ContainsKey( 679686325 );
			  int? valueBefore = map.Get( 679686325 );
			  int? previous = map.Put( 679686325, 63 );
			  bool existsAfter = map.ContainsKey( 679686325 );
			  int? valueAfter = map.Get( 679686325 );
			  int sizeAfter = map.Size();
			  assertEquals( 64, sizeBefore, "Size before put should have been 64" );
			  assertFalse( existedBefore, "679686325 should not exist before putting here" );
			  assertNull( valueBefore, "value before putting should be null" );
			  assertNull( previous, "value returned from putting should be null" );
			  assertTrue( existsAfter, "679686325 should exist" );
			  assertEquals( ( int? ) 63, valueAfter, "value after putting should be 63" );
			  assertEquals( 65, sizeAfter, "Size after put should have been 65" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldMoveValuesWhenMovingEntriesAround()
		 internal virtual void ShouldMoveValuesWhenMovingEntriesAround()
		 {
			  // GIVEN
			  PrimitiveLongObjectMap<int> map = Primitive.longObjectMap();
			  map.Put( 206243105, 47 );
			  map.Put( 2083304695, 63 );
			  map.Put( 689837337, 7 );
			  map.Remove( 206243105 );
			  // WHEN/THEN
			  int sizeBefore = map.Size();
			  bool existedBefore = map.ContainsKey( 689837337 );
			  int? valueBefore = map.Get( 689837337 );
			  int? previous = map.Put( 689837337, 20 );
			  bool existsAfter = map.ContainsKey( 689837337 );
			  int? valueAfter = map.Get( 689837337 );
			  int sizeAfter = map.Size();
			  assertEquals( 2, sizeBefore, "Size before put should have been 2" );
			  assertTrue( existedBefore, "689837337 should exist before putting here" );
			  assertEquals( ( int? ) 7, valueBefore, "value before should be 7" );
			  assertEquals( ( int? ) 7, previous, "value returned from put should be 7" );
			  assertTrue( existsAfter, "689837337 should exist" );
			  assertEquals( ( int? ) 20, valueAfter, "value after putting should be 20" );
			  assertEquals( 2, sizeAfter, "Size after put should have been 2" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReturnCorrectPreviousValue()
		 internal virtual void ShouldReturnCorrectPreviousValue()
		 {
			  // GIVEN
			  PrimitiveLongIntMap map = Primitive.longIntMap();
			  map.Remove( 2050585513 );
			  map.Put( 429170228, 99 );
			  map.Put( 1356282827, 24 );
			  map.Remove( 1341095873 );
			  // WHEN/THEN
			  int sizeBefore = map.Size();
			  bool existedBefore = map.ContainsKey( 429170228 );
			  int valueBefore = map.Get( 429170228 );
			  int previous = map.Put( 429170228, 1 );
			  bool existsAfter = map.ContainsKey( 429170228 );
			  int valueAfter = map.Get( 429170228 );
			  int sizeAfter = map.Size();
			  assertEquals( 2, sizeBefore, "Size before put should have been 2" );
			  assertTrue( existedBefore, "429170228 should exist before putting here" );
			  assertEquals( 99, valueBefore, "value before should be 99" );
			  assertEquals( 99, previous, "value returned from put should be 99" );
			  assertTrue( existsAfter, "429170228 should exist" );
			  assertEquals( 1, valueAfter, "value after putting should be 1" );
			  assertEquals( 2, sizeAfter, "Size after put should have been 2" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldOnlyContainAddedValues()
		 internal virtual void ShouldOnlyContainAddedValues()
		 {
			  // GIVEN
			  PrimitiveLongIntMap map = Primitive.longIntMap();
			  map.Put( 1179059774, 54 );
			  map.Put( 612612792, 91 );
			  map.Put( 853030395, 81 );
			  map.Put( 1821941016, 69 );
			  map.Put( 815540261, 54 );
			  map.Put( 2120470777, 63 );
			  map.Put( 866144206, 41 );
			  map.Put( 905659306, 86 );
			  map.Put( 602586792, 24 );
			  map.Put( 1033857549, 61 );
			  map.Put( 1570231638, 69 );
			  map.Put( 30675820, 53 );
			  map.Put( 433666923, 14 );
			  map.Put( 1668952952, 52 );
			  map.Put( 1733960171, 14 );
			  map.Put( 1240027317, 64 );
			  map.Put( 250830995, 71 );
			  map.Put( 1446519846, 17 );
			  map.Put( 1857052106, 78 );
			  map.Put( 37351838, 26 );
			  map.Put( 1523695604, 78 );
			  map.Put( 1024540180, 12 );
			  map.Put( 603632507, 81 );
			  map.Put( 483087335, 37 );
			  map.Put( 216300592, 55 );
			  map.Put( 1729046213, 72 );
			  map.Put( 1397559084, 78 );
			  map.Put( 802042428, 34 );
			  map.Put( 1127990805, 6 );
			  map.Put( 2081866795, 53 );
			  map.Put( 1528122026, 39 );
			  map.Put( 642547543, 78 );
			  map.Put( 1909701557, 35 );
			  map.Put( 2070740876, 40 );
			  map.Put( 316027755, 18 );
			  map.Put( 824089651, 63 );
			  map.Put( 1082682044, 85 );
			  map.Put( 154864377, 44 );
			  map.Put( 26918244, 73 );
			  map.Put( 808069768, 20 );
			  map.Put( 38089155, 17 );
			  map.Put( 1772700678, 35 );
			  map.Put( 1790535392, 82 );
			  map.Put( 159186757, 10 );
			  map.Put( 73305650, 52 );
			  map.Put( 2025019209, 38 );
			  map.Put( 922996536, 53 );
			  map.Put( 1852424925, 34 );
			  map.Put( 1181179273, 9 );
			  map.Put( 107520967, 11 );
			  map.Put( 1702904247, 55 );
			  map.Put( 1819417390, 50 );
			  map.Put( 1163114165, 57 );
			  map.Put( 2036796587, 40 );
			  map.Put( 2130510197, 26 );
			  map.Put( 1710533919, 70 );
			  map.Put( 497498438, 48 );
			  map.Put( 147722732, 8 );
			  map.Remove( 802042428 );
			  map.Put( 1355114893, 90 );
			  map.Put( 419675404, 62 );
			  map.Put( 1722846265, 41 );
			  map.Put( 1287254514, 61 );
			  map.Put( 1925017947, 8 );
			  map.Put( 1290391303, 59 );
			  map.Put( 1938779966, 27 );
			  // WHEN/THEN
			  int sizeBefore = map.Size();
			  bool existedBefore = map.ContainsKey( 1452811669 );
			  int valueBefore = map.Get( 1452811669 );
			  int previous = map.Put( 1452811669, 16 );
			  bool existsAfter = map.ContainsKey( 1452811669 );
			  int valueAfter = map.Get( 1452811669 );
			  int sizeAfter = map.Size();
			  assertEquals( 64, sizeBefore, "Size before put should have been 64" );
			  assertFalse( existedBefore, "1452811669 should not exist before putting here" );
			  assertEquals( -1, valueBefore, "value before should be -1" );
			  assertEquals( -1, previous, "value returned from put should be -1" );
			  assertTrue( existsAfter, "1452811669 should exist" );
			  assertEquals( 16, valueAfter, "value after putting should be 16" );
			  assertEquals( 65, sizeAfter, "Size after put should have been 65" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldOnlyContainAddedValues_2()
		 internal virtual void ShouldOnlyContainAddedValues_2()
		 {
			  // GIVEN
			  PrimitiveLongIntMap map = Primitive.longIntMap();
			  map.Put( 913910231, 25 );
			  map.Put( 102310782, 40 );
			  map.Put( 634960377, 32 );
			  map.Put( 947168147, 96 );
			  map.Put( 947430652, 26 );
			  map.Put( 1391472521, 72 );
			  map.Put( 7905512, 10 );
			  map.Put( 7905512, 2 );
			  map.Put( 1391472521, 66 );
			  map.Put( 824376092, 79 );
			  map.Remove( 750639810 );
			  map.Put( 947168147, 61 );
			  map.Put( 831409018, 57 );
			  map.Put( 241941283, 76 );
			  map.Put( 824376092, 45 );
			  map.Remove( 2125994926 );
			  map.Put( 824376092, 47 );
			  map.Put( 1477982280, 1 );
			  map.Remove( 2129508263 );
			  map.Put( 1477982280, 41 );
			  map.Put( 642178985, 69 );
			  map.Put( 1447441709, 85 );
			  map.Put( 642178985, 27 );
			  map.Put( 875840384, 72 );
			  map.Put( 1967716733, 55 );
			  map.Put( 1965379174, 5 );
			  map.Put( 913910231, 40 );
			  // WHEN/THEN
			  bool existedBefore = map.ContainsKey( 947430652 );
			  int valueBefore = map.Get( 947430652 );
			  int removed = map.Remove( 947430652 );
			  bool existsAfter = map.ContainsKey( 947430652 );
			  int valueAfter = map.Get( 947430652 );
			  assertTrue( existedBefore, "947430652 should exist before removing here" );
			  assertEquals( 26, valueBefore, "value before should be 26" );
			  assertEquals( 26, removed, "value returned from remove should be 26" );
			  assertFalse( existsAfter, "947430652 should not exist" );
			  assertEquals( -1, valueAfter, "value after removing should be -1" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldOnlyContainAddedValues_3()
		 internal virtual void ShouldOnlyContainAddedValues_3()
		 {
			  // GIVEN
			  PrimitiveLongObjectMap<int> map = Primitive.longObjectMap();
			  map.Put( 2083704227957337692L, 50 );
			  map.Put( 1039748383662879297L, 12 );
			  map.Put( 6296247210943123044L, 45 );
			  map.Put( 8004677065031068097L, 5 );
			  map.Put( 1039748383662879297L, 70 );
			  map.Put( 5386804704064477958L, 97 );
			  map.Remove( 1506507783133586973L );
			  map.Put( 4287434858289406631L, 29 );
			  map.Put( 8004677065031068097L, 17 );
			  map.Put( 986286772325632801L, 14 );
			  map.Put( 7880139640446289959L, 68 );
			  map.Put( 8004677065031068097L, 23 );
			  map.Put( 5386804704064477958L, 72 );
			  map.Put( 5386804704064477958L, 71 );
			  map.Put( 2300381985575721987L, 0 );
			  map.Put( 6144230340727188436L, 31 );
			  map.Put( 425423457410117293L, 88 );
			  map.Put( 2083704227957337692L, 65 );
			  map.Put( 7805027477403310582L, 72 );
			  map.Put( 2254081933055750443L, 66 );
			  map.Put( 5386804704064477958L, 46 );
			  map.Put( 5787098127909281443L, 45 );
			  map.Put( 5508645210651400664L, 45 );
			  map.Put( 6092264867460428040L, 65 );
			  map.Put( 4551026293109220157L, 52 );
			  map.Put( 4669163071261559807L, 33 );
			  map.Put( 5790325306669462860L, 96 );
			  map.Put( 4337317298737908324L, 78 );
			  map.Put( 986286772325632801L, 71 );
			  map.Put( 4287434858289406631L, 47 );
			  map.Put( 1827085004206892313L, 30 );
			  map.Put( 6070945099342863711L, 88 );
			  map.Remove( 6300957726732252611L );
			  map.Put( 2300381985575721987L, 22 );
			  map.Put( 2083704227957337692L, 2 );
			  map.Put( 2885272279767063039L, 71 );
			  map.Put( 3627867780921264529L, 5 );
			  map.Remove( 5330274310754559602L );
			  map.Put( 8902857048431919030L, 23 );
			  map.Remove( 4287434858289406631L );
			  map.Put( 5459968256561120197L, 8 );
			  map.Put( 5790325306669462860L, 17 );
			  map.Put( 9003964541346458616L, 45 );
			  map.Put( 3832091967762842783L, 79 );
			  map.Put( 1332274446340546922L, 62 );
			  map.Put( 6610784890222945257L, 20 );
			  map.Put( 3627867780921264529L, 65 );
			  map.Put( 7988336790991560848L, 89 );
			  map.Put( 5386804704064477958L, 15 );
			  map.Put( 6296247210943123044L, 19 );
			  map.Put( 7776019112299874624L, 67 );
			  map.Put( 5827611175622537127L, 18 );
			  map.Remove( 8004677065031068097L );
			  map.Put( 2451971987846333787L, 48 );
			  map.Put( 3627867780921264529L, 16 );
			  map.Put( 2506727685914893570L, 61 );
			  map.Put( 6629089416451699826L, 89 );
			  map.Put( 875078333857781813L, 38 );
			  map.Put( 439984342972777679L, 51 );
			  map.Put( 9077428346047966819L, 19 );
			  map.Put( 7045299269724516542L, 73 );
			  map.Put( 8055487013098459354L, 24 );
			  map.Put( 6610784890222945257L, 65 );
			  map.Put( 986286772325632801L, 29 );
			  map.Put( 133928815519522465L, 81 );
			  map.Put( 5780114596098993316L, 15 );
			  map.Put( 3790785290324207363L, 91 );
			  map.Put( 2795040354588080479L, 48 );
			  map.Put( 4218658174275197144L, 59 );
			  map.Put( 6610784890222945257L, 70 );
			  map.Remove( 3722940212039795685L );
			  map.Put( 1817899559164238906L, 30 );
			  map.Put( 4551026293109220157L, 35 );
			  map.Put( 986286772325632801L, 57 );
			  map.Put( 3811462607668925015L, 57 );
			  map.Put( 2795040354588080479L, 85 );
			  map.Put( 8460476221939231932L, 86 );
			  map.Remove( 8957537157979159052L );
			  map.Put( 2032224502814063026L, 57 );
			  map.Remove( 8924941903092284834L );
			  map.Put( 5386804704064477958L, 2 );
			  map.Put( 6629089416451699826L, 18 );
			  map.Put( 425423457410117293L, 31 );
			  map.Put( 4337317298737908324L, 35 );
			  map.Remove( 5337770067730257989L );
			  map.Put( 6150561851033498431L, 49 );
			  map.Put( 5067121328094576685L, 46 );
			  map.Remove( 3742103310924563011L );
			  map.Put( 1327614778938791146L, 49 );
			  map.Put( 255729841510922319L, 16 );
			  map.Put( 8785988080128503533L, 69 );
			  map.Put( 4218658174275197144L, 20 );
			  map.Put( 1265271287408386915L, 43 );
			  map.Put( 255729841510922319L, 5 );
			  map.Put( 8651736753344997668L, 41 );
			  map.Put( 4363375305508283265L, 4 );
			  map.Put( 4185381066643227500L, 29 );
			  map.Put( 3790785290324207363L, 58 );
			  map.Put( 3058911485922749695L, 1 );
			  map.Put( 8629268898854377850L, 66 );
			  map.Put( 1762013345156514959L, 5 );
			  map.Remove( 4354754593499656793L );
			  map.Put( 1332274446340546922L, 16 );
			  map.Put( 4953501292937412915L, 87 );
			  map.Put( 2330841365833073849L, 83 );
			  map.Put( 8096564328797694553L, 44 );
			  map.Put( 8935185623148330821L, 7 );
			  map.Put( 6150561851033498431L, 48 );
			  map.Remove( 5827611175622537127L );
			  map.Put( 8048363335369773749L, 25 );
			  map.Put( 3627867780921264529L, 48 );
			  map.Put( 4806848030248674690L, 14 );
			  map.Put( 5430628648110105698L, 30 );
			  map.Remove( 7261476188677343032L );
			  map.Put( 1265271287408386915L, 61 );
			  map.Put( 9077428346047966819L, 32 );
			  map.Put( 1827085004206892313L, 95 );
			  map.Put( 6377023652046870199L, 8 );
			  map.Remove( 8096564328797694553L );
			  map.Put( 458594253548258561L, 37 );
			  map.Put( 4418108647578170347L, 60 );
			  map.Put( 4363375305508283265L, 50 );
			  map.Remove( 3220719966247388754L );
			  map.Put( 5067121328094576685L, 86 );
			  map.Put( 8030171618634928529L, 9 );
			  map.Remove( 5790325306669462860L );
			  map.Remove( 1693435088303118108L );
			  map.Put( 1817899559164238906L, 48 );
			  map.Put( 2823063986711596775L, 58 );
			  map.Put( 5065867711051034527L, 1 );
			  map.Put( 6144553725832876585L, 16 );
			  map.Put( 6066303112518690730L, 96 );
			  map.Put( 1627429134135319103L, 64 );
			  map.Put( 2083704227957337692L, 48 );
			  map.Put( 5074984076240598083L, 46 );
			  map.Put( 273737562207470342L, 60 );
			  map.Put( 5065867711051034527L, 7 );
			  map.Put( 1425720210238734727L, 23 );
			  map.Put( 8840483239403421070L, 42 );
			  map.Put( 622393419539870960L, 66 );
			  map.Put( 4649317581471627693L, 84 );
			  map.Put( 6344284253098418581L, 10 );
			  map.Put( 6066303112518690730L, 14 );
			  map.Put( 2032224502814063026L, 72 );
			  map.Put( 3860451022347437817L, 26 );
			  map.Put( 1931469116507191845L, 30 );
			  map.Put( 7264376865632246862L, 81 );
			  map.Put( 875078333857781813L, 41 );
			  map.Put( 6066303112518690730L, 65 );
			  map.Put( 357446231240164192L, 80 );
			  map.Put( 90138258774469874L, 73 );
			  map.Put( 2550828149718879762L, 72 );
			  map.Put( 357446231240164192L, 17 );
			  map.Put( 4233359298058523722L, 83 );
			  map.Put( 7879882017779927485L, 33 );
			  map.Put( 4554977248866184403L, 64 );
			  map.Put( 2032224502814063026L, 11 );
			  map.Put( 8460476221939231932L, 65 );
			  map.Put( 4404294840535520232L, 58 );
			  map.Put( 439984342972777679L, 83 );
			  map.Put( 143440583901416159L, 59 );
			  map.Put( 6980461179076170770L, 9 );
			  map.Put( 4253079906814783119L, 93 );
			  map.Put( 6377023652046870199L, 20 );
			  map.Put( 2885272279767063039L, 5 );
			  map.Put( 1115850061381524772L, 37 );
			  map.Put( 4288489609244987651L, 22 );
			  map.Put( 1869499448099043543L, 73 );
			  map.Put( 2233583342469238733L, 84 );
			  map.Put( 8785988080128503533L, 61 );
			  map.Put( 7396264003126204068L, 81 );
			  map.Put( 6553509363155186775L, 96 );
			  map.Put( 1265663249510580286L, 89 );
			  map.Put( 8824139147632000339L, 49 );
			  map.Put( 8629268898854377850L, 10 );
			  map.Put( 6463027127151126151L, 57 );
			  map.Put( 2577561266405706623L, 46 );
			  map.Put( 2942302849662258387L, 40 );
			  map.Put( 2233583342469238733L, 56 );
			  map.Put( 7971826071187872579L, 53 );
			  map.Put( 1425720210238734727L, 27 );
			  map.Remove( 7194434791627009043L );
			  map.Put( 1429250394105883546L, 82 );
			  map.Put( 8048363335369773749L, 19 );
			  map.Put( 425423457410117293L, 51 );
			  map.Remove( 3570674569632664356L );
			  map.Remove( 5925614419318569326L );
			  map.Put( 245367449754197583L, 27 );
			  map.Put( 8724491045048677021L, 55 );
			  map.Put( 1037934857236019066L, 66 );
			  map.Put( 8902857048431919030L, 61 );
			  map.Put( 4806848030248674690L, 17 );
			  map.Put( 8840483239403421070L, 95 );
			  map.Put( 2931578375554111170L, 54 );
			  map.Put( 5352224688502007093L, 36 );
			  map.Put( 6675404627060358866L, 64 );
			  map.Put( 5011448804620449550L, 48 );
			  map.Put( 9003964541346458616L, 44 );
			  map.Put( 8614830761978541860L, 70 );
			  map.Put( 3790785290324207363L, 95 );
			  map.Put( 3524676886726253569L, 54 );
			  map.Put( 6858076293577130289L, 60 );
			  map.Put( 6721253107702965701L, 41 );
			  map.Put( 655525227420977141L, 94 );
			  map.Put( 2344362186561469072L, 29 );
			  map.Put( 6144230340727188436L, 76 );
			  map.Put( 6751209943070153529L, 22 );
			  map.Put( 5528119873376392874L, 44 );
			  map.Put( 6675404627060358866L, 20 );
			  map.Put( 6167523814676644161L, 50 );
			  map.Put( 4288489609244987651L, 82 );
			  map.Remove( 3362704467864439992L );
			  map.Put( 8629268898854377850L, 50 );
			  map.Remove( 8824139147632000339L );
			  map.Remove( 8563575034946766108L );
			  map.Put( 4391871381220263726L, 20 );
			  map.Remove( 6143313773038364355L );
			  map.Remove( 3225044803974988142L );
			  map.Remove( 8048363335369773749L );
			  map.Remove( 439984342972777679L );
			  map.Put( 7776019112299874624L, 8 );
			  map.Put( 5414055783993307402L, 13 );
			  map.Put( 425423457410117293L, 91 );
			  map.Put( 8407567928758710341L, 30 );
			  map.Put( 6070945099342863711L, 14 );
			  map.Put( 5644323748441073606L, 91 );
			  map.Put( 5297141920581728538L, 61 );
			  map.Put( 7880139640446289959L, 1 );
			  map.Put( 2300381985575721987L, 92 );
			  map.Put( 8253246663621301435L, 26 );
			  map.Remove( 2074764355175726009L );
			  map.Remove( 3823843425563676964L );
			  map.Put( 8314906688468605292L, 91 );
			  map.Put( 6864119235983684905L, 56 );
			  map.Put( 6610784890222945257L, 85 );
			  map.Put( 3790785290324207363L, 7 );
			  map.Put( 9077428346047966819L, 20 );
			  map.Put( 5594781060356781714L, 76 );
			  map.Put( 4288489609244987651L, 24 );
			  map.Put( 5427718399315377322L, 93 );
			  map.Put( 6858076293577130289L, 41 );
			  map.Put( 4233359298058523722L, 43 );
			  map.Put( 3058911485922749695L, 88 );
			  map.Remove( 1327614778938791146L );
			  map.Put( 4665341449948530032L, 26 );
			  map.Remove( 2860868006143077426L );
			  map.Put( 6167523814676644161L, 70 );
			  map.Remove( 8314906688468605292L );
			  map.Put( 6396314739926743170L, 25 );
			  map.Put( 8924527320597926970L, 40 );
			  map.Put( 1817899559164238906L, 84 );
			  map.Remove( 4391871381220263726L );
			  map.Put( 8850817829384121639L, 50 );
			  map.Put( 6513548978704592547L, 52 );
			  map.Remove( 6066303112518690730L );
			  map.Remove( 3946964103425920940L );
			  map.Put( 7971826071187872579L, 71 );
			  map.Put( 90138258774469874L, 78 );
			  map.Put( 8309039683334256753L, 44 );
			  map.Put( 327300646665050265L, 52 );
			  map.Put( 4239841777571533415L, 22 );
			  map.Put( 7391753878925882699L, 46 );
			  map.Put( 5987501380005333533L, 31 );
			  map.Put( 6734545541042861356L, 45 );
			  map.Remove( 6566682167801344029L );
			  map.Put( 4218658174275197144L, 16 );
			  map.Put( 4363586488886891680L, 88 );
			  map.Put( 8030171618634928529L, 19 );
			  map.Put( 6513548978704592547L, 95 );
			  map.Put( 6721253107702965701L, 55 );
			  map.Put( 2153470608693815785L, 9 );
			  map.Put( 5807454155419905847L, 7 );
			  map.Remove( 4528425347504500078L );
			  map.Put( 339083533777732657L, 72 );
			  map.Put( 5162811261582626928L, 68 );
			  map.Put( 5459968256561120197L, 89 );
			  map.Put( 946125626260258615L, 97 );
			  map.Put( 986286772325632801L, 26 );
			  map.Put( 8309039683334256753L, 74 );
			  map.Put( 1609193622622537433L, 84 );
			  map.Put( 2506727685914893570L, 9 );
			  map.Put( 143440583901416159L, 33 );
			  map.Put( 7716482408003289208L, 30 );
			  map.Put( 7880139640446289959L, 74 );
			  map.Put( 5472992709007694577L, 27 );
			  map.Put( 3367972495572249232L, 8 );
			  map.Put( 6002824320296423294L, 71 );
			  map.Put( 5162811261582626928L, 10 );
			  map.Remove( 8309039683334256753L );
			  map.Put( 3103455156394998975L, 1 );
			  map.Put( 4943074037151902792L, 38 );
			  map.Put( 1455801901314190156L, 98 );
			  map.Put( 3502583509759951230L, 22 );
			  map.Remove( 8464127935014315372L );
			  map.Put( 6858076293577130289L, 35 );
			  map.Put( 8487179770790306175L, 5 );
			  map.Put( 946125626260258615L, 85 );
			  map.Put( 722144778357869055L, 1 );
			  map.Remove( 6832604792388788147L );
			  map.Remove( 7879882017779927485L );
			  map.Put( 4636443662717865247L, 98 );
			  map.Put( 6950926592851406543L, 12 );
			  map.Put( 8536120340569832116L, 73 );
			  map.Put( 86730768989854734L, 66 );
			  map.Put( 4558683789229895837L, 26 );
			  map.Put( 4806848030248674690L, 11 );
			  map.Put( 425423457410117293L, 38 );
			  map.Put( 8713875164075871710L, 97 );
			  map.Put( 3790785290324207363L, 77 );
			  map.Put( 4632006356221328093L, 21 );
			  map.Put( 7628512490650429100L, 28 );
			  map.Remove( 4651124484202085669L );
			  map.Put( 4320012891688937760L, 22 );
			  map.Put( 6092264867460428040L, 86 );
			  map.Put( 6610784890222945257L, 71 );
			  map.Remove( 3515175120945606156L );
			  map.Put( 5787098127909281443L, 10 );
			  map.Put( 5057609667342409825L, 50 );
			  map.Put( 5903362554916539560L, 75 );
			  map.Remove( 5339209082212961633L );
			  map.Put( 3502583509759951230L, 36 );
			  map.Put( 4198420341072443663L, 75 );
			  map.Put( 5037754181090593008L, 34 );
			  map.Put( 39606137866137388L, 19 );
			  map.Remove( 622393419539870960L );
			  map.Put( 2783004740411041924L, 79 );
			  map.Put( 6232331175163415825L, 72 );
			  map.Put( 4367206208262757151L, 33 );
			  map.Remove( 5879159150292946046L );
			  map.Put( 722144778357869055L, 80 );
			  map.Put( 9006426844471489361L, 92 );
			  map.Put( 550025535839604778L, 32 );
			  map.Remove( 5855895659233120621L );
			  map.Put( 1455801901314190156L, 24 );
			  map.Put( 3860451022347437817L, 81 );
			  map.Put( 2672104991948169160L, 57 );
			  map.Remove( 3860451022347437817L );
			  map.Remove( 655525227420977141L );
			  map.Put( 2413633498546493443L, 68 );
			  map.Put( 4185381066643227500L, 54 );
			  map.Put( 1280345971255663584L, 39 );
			  map.Put( 5796123963544961504L, 76 );
			  map.Put( 1892786158672061630L, 55 );
			  map.Remove( 5352224688502007093L );
			  map.Put( 3711105805930144213L, 47 );
			  map.Put( 4608237982157900285L, 41 );
			  map.Put( 4175794211341763944L, 31 );
			  map.Put( 2315250912582233395L, 81 );
			  map.Put( 357446231240164192L, 87 );
			  map.Put( 4110861648946406824L, 75 );
			  map.Put( 6912381889380280106L, 22 );
			  map.Put( 6721253107702965701L, 43 );
			  map.Put( 8536120340569832116L, 87 );
			  map.Put( 9134483648483594929L, 77 );
			  map.Put( 9132976039160654816L, 69 );
			  map.Remove( 7698175804504341415L );
			  map.Remove( 9134483648483594929L );
			  map.Put( 215721718639621876L, 11 );
			  map.Put( 8367455298026304238L, 78 );
			  map.Put( 215721718639621876L, 13 );
			  map.Put( 1398628381776162625L, 12 );
			  map.Put( 3818698536247649025L, 91 );
			  map.Put( 146020861698406718L, 41 );
			  map.Put( 39606137866137388L, 93 );
			  map.Put( 2032224502814063026L, 29 );
			  map.Remove( 6363504799104250810L );
			  map.Put( 7198198302699040275L, 75 );
			  map.Put( 1659665859871881503L, 35 );
			  map.Put( 2032224502814063026L, 25 );
			  map.Put( 7006780191094382053L, 2 );
			  map.Put( 2626850727701928459L, 97 );
			  map.Put( 5371963064889126677L, 49 );
			  map.Put( 2777831232791546183L, 35 );
			  map.Remove( 1265271287408386915L );
			  map.Remove( 1078791602714388223L );
			  map.Put( 7355915493826998767L, 39 );
			  map.Remove( 1557741259882614531L );
			  map.Put( 318456745029053198L, 18 );
			  map.Put( 5731549637584761783L, 77 );
			  map.Put( 875078333857781813L, 80 );
			  map.Remove( 4288489609244987651L );
			  map.Put( 6296247210943123044L, 67 );
			  map.Put( 6513548978704592547L, 60 );
			  map.Put( 7484688824700837146L, 79 );
			  map.Put( 4551026293109220157L, 77 );
			  map.Put( 2961669147182343860L, 80 );
			  map.Put( 4481942776688563562L, 28 );
			  map.Put( 5879809531485088687L, 63 );
			  map.Put( 5799223884087101214L, 94 );
			  map.Put( 8394473765965282856L, 59 );
			  map.Remove( 7273585073251585620L );
			  map.Remove( 5518575735665118270L );
			  map.Put( 1946691597339845823L, 64 );
			  map.Put( 1191724556568067952L, 33 );
			  map.Remove( 1803989601564179749L );
			  map.Put( 7909563548070411816L, 98 );
			  // WHEN/THEN
			  int sizeBefore = map.Size();
			  bool existedBefore = map.ContainsKey( 5826258075197365143L );
			  int? valueBefore = map.Get( 5826258075197365143L );
			  int? previous = map.Put( 5826258075197365143L, 6 );
			  bool existsAfter = map.ContainsKey( 5826258075197365143L );
			  int? valueAfter = map.Get( 5826258075197365143L );
			  int sizeAfter = map.Size();
			  assertEquals( 199, sizeBefore, "Size before put should have been 199" );
			  assertFalse( existedBefore, "5826258075197365143 should not exist before putting here" );
			  assertNull( valueBefore, "value before putting should be null" );
			  assertNull( previous, "value returned from putting should be null" );
			  assertTrue( existsAfter, "5826258075197365143 should exist" );
			  assertEquals( ( int? ) 6, valueAfter, "value after putting should be 6" );
			  assertEquals( 200, sizeAfter, "Size after put should have been 200" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Test void longIntEntryVisitorShouldSeeAllEntriesIfItDoesNotBreakOut()
		 internal virtual void LongIntEntryVisitorShouldSeeAllEntriesIfItDoesNotBreakOut()
		 {
			  // GIVEN
			  PrimitiveLongIntMap map = Primitive.longIntMap();
			  map.Put( 1, 100 );
			  map.Put( 2, 200 );
			  map.Put( 3, 300 );
			  PrimitiveLongIntVisitor<Exception> visitor = mock( typeof( PrimitiveLongIntVisitor ) );

			  // WHEN
			  map.VisitEntries( visitor );

			  // THEN
			  verify( visitor ).visited( 1, 100 );
			  verify( visitor ).visited( 2, 200 );
			  verify( visitor ).visited( 3, 300 );
			  verifyNoMoreInteractions( visitor );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void longIntEntryVisitorShouldNotSeeEntriesAfterRequestingBreakOut()
		 internal virtual void LongIntEntryVisitorShouldNotSeeEntriesAfterRequestingBreakOut()
		 {
			  // GIVEN
			  PrimitiveLongIntMap map = Primitive.longIntMap();
			  map.Put( 1, 100 );
			  map.Put( 2, 200 );
			  map.Put( 3, 300 );
			  map.Put( 4, 400 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicInteger counter = new java.util.concurrent.atomic.AtomicInteger();
			  AtomicInteger counter = new AtomicInteger();

			  // WHEN
			  map.VisitEntries( ( key, value ) => counter.incrementAndGet() > 2 );

			  // THEN
			  assertThat( counter.get(), @is(3) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Test void intLongEntryVisitorShouldSeeAllEntriesIfItDoesNotBreakOut()
		 internal virtual void IntLongEntryVisitorShouldSeeAllEntriesIfItDoesNotBreakOut()
		 {
			  // GIVEN
			  PrimitiveIntLongMap map = Primitive.intLongMap();
			  map.Put( 1, 100 );
			  map.Put( 2, 200 );
			  map.Put( 3, 300 );
			  PrimitiveIntLongVisitor<Exception> visitor = mock( typeof( PrimitiveIntLongVisitor ) );

			  // WHEN
			  map.VisitEntries( visitor );

			  // THEN
			  verify( visitor ).visited( 1, 100 );
			  verify( visitor ).visited( 2, 200 );
			  verify( visitor ).visited( 3, 300 );
			  verifyNoMoreInteractions( visitor );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void intLongEntryVisitorShouldNotSeeEntriesAfterRequestingBreakOut()
		 internal virtual void IntLongEntryVisitorShouldNotSeeEntriesAfterRequestingBreakOut()
		 {
			  // GIVEN
			  PrimitiveIntLongMap map = Primitive.intLongMap();
			  map.Put( 1, 100 );
			  map.Put( 2, 200 );
			  map.Put( 3, 300 );
			  map.Put( 4, 400 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicInteger counter = new java.util.concurrent.atomic.AtomicInteger();
			  AtomicInteger counter = new AtomicInteger();

			  // WHEN
			  map.VisitEntries( ( key, value ) => counter.incrementAndGet() > 2 );

			  // THEN
			  assertThat( counter.get(), @is(3) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Test void longLongEntryVisitorShouldSeeAllEntriesIfItDoesNotBreakOut()
		 internal virtual void LongLongEntryVisitorShouldSeeAllEntriesIfItDoesNotBreakOut()
		 {
			  // GIVEN
			  PrimitiveLongLongVisitor<Exception> visitor;
			  using ( PrimitiveLongLongMap map = Primitive.longLongMap() )
			  {
					map.Put( 1, 100 );
					map.Put( 2, 200 );
					map.Put( 3, 300 );
					visitor = mock( typeof( PrimitiveLongLongVisitor ) );

					// WHEN
					map.VisitEntries( visitor );
			  }

			  // THEN
			  verify( visitor ).visited( 1, 100 );
			  verify( visitor ).visited( 2, 200 );
			  verify( visitor ).visited( 3, 300 );
			  verifyNoMoreInteractions( visitor );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void longLongEntryVisitorShouldNotSeeEntriesAfterRequestingBreakOut()
		 internal virtual void LongLongEntryVisitorShouldNotSeeEntriesAfterRequestingBreakOut()
		 {
			  // GIVEN
			  AtomicInteger counter = new AtomicInteger();
			  using ( PrimitiveLongLongMap map = Primitive.longLongMap() )
			  {
					map.Put( 1, 100 );
					map.Put( 2, 200 );
					map.Put( 3, 300 );
					map.Put( 4, 400 );

					// WHEN
					map.VisitEntries( ( key, value ) => counter.incrementAndGet() > 2 );
			  }

			  // THEN
			  assertThat( counter.get(), @is(3) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Test void longLongOffHeapEntryVisitorShouldSeeAllEntriesIfItDoesNotBreakOut()
		 internal virtual void LongLongOffHeapEntryVisitorShouldSeeAllEntriesIfItDoesNotBreakOut()
		 {
			  // GIVEN
			  PrimitiveLongLongVisitor<Exception> visitor;
			  using ( PrimitiveLongLongMap map = Primitive.offHeapLongLongMap( GlobalMemoryTracker.INSTANCE ) )
			  {
					map.Put( 1, 100 );
					map.Put( 2, 200 );
					map.Put( 3, 300 );
					visitor = mock( typeof( PrimitiveLongLongVisitor ) );

					// WHEN
					map.VisitEntries( visitor );
			  }

			  // THEN
			  verify( visitor ).visited( 1, 100 );
			  verify( visitor ).visited( 2, 200 );
			  verify( visitor ).visited( 3, 300 );
			  verifyNoMoreInteractions( visitor );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void longLongOffHeapEntryVisitorShouldNotSeeEntriesAfterRequestingBreakOut()
		 internal virtual void LongLongOffHeapEntryVisitorShouldNotSeeEntriesAfterRequestingBreakOut()
		 {
			  // GIVEN
			  AtomicInteger counter = new AtomicInteger();
			  using ( PrimitiveLongLongMap map = Primitive.offHeapLongLongMap( GlobalMemoryTracker.INSTANCE ) )
			  {
					map.Put( 1, 100 );
					map.Put( 2, 200 );
					map.Put( 3, 300 );
					map.Put( 4, 400 );

					// WHEN
					map.VisitEntries( ( key, value ) => counter.incrementAndGet() > 2 );
			  }

			  // THEN
			  assertThat( counter.get(), @is(3) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Test void longObjectEntryVisitorShouldSeeAllEntriesIfItDoesNotBreakOut()
		 internal virtual void LongObjectEntryVisitorShouldSeeAllEntriesIfItDoesNotBreakOut()
		 {
			  // GIVEN
			  PrimitiveLongObjectMap<int> map = Primitive.longObjectMap();
			  map.Put( 1, 100 );
			  map.Put( 2, 200 );
			  map.Put( 3, 300 );
			  PrimitiveLongObjectVisitor<int, Exception> visitor = mock( typeof( PrimitiveLongObjectVisitor ) );

			  // WHEN
			  map.VisitEntries( visitor );

			  // THEN
			  verify( visitor ).visited( 1, 100 );
			  verify( visitor ).visited( 2, 200 );
			  verify( visitor ).visited( 3, 300 );
			  verifyNoMoreInteractions( visitor );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void longObjectEntryVisitorShouldNotSeeEntriesAfterRequestingBreakOut()
		 internal virtual void LongObjectEntryVisitorShouldNotSeeEntriesAfterRequestingBreakOut()
		 {
			  // GIVEN
			  PrimitiveLongObjectMap<int> map = Primitive.longObjectMap();
			  map.Put( 1, 100 );
			  map.Put( 2, 200 );
			  map.Put( 3, 300 );
			  map.Put( 4, 400 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicInteger counter = new java.util.concurrent.atomic.AtomicInteger();
			  AtomicInteger counter = new AtomicInteger();

			  // WHEN
			  map.VisitEntries( ( key, value ) => counter.incrementAndGet() > 2 );

			  // THEN
			  assertThat( counter.get(), @is(3) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Test void intObjectEntryVisitorShouldSeeAllEntriesIfItDoesNotBreakOut()
		 internal virtual void IntObjectEntryVisitorShouldSeeAllEntriesIfItDoesNotBreakOut()
		 {
			  // GIVEN
			  PrimitiveIntObjectMap<int> map = Primitive.intObjectMap();
			  map.Put( 1, 100 );
			  map.Put( 2, 200 );
			  map.Put( 3, 300 );
			  PrimitiveIntObjectVisitor<int, Exception> visitor = mock( typeof( PrimitiveIntObjectVisitor ) );

			  // WHEN
			  map.VisitEntries( visitor );

			  // THEN
			  verify( visitor ).visited( 1, 100 );
			  verify( visitor ).visited( 2, 200 );
			  verify( visitor ).visited( 3, 300 );
			  verifyNoMoreInteractions( visitor );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void intObjectEntryVisitorShouldNotSeeEntriesAfterRequestingBreakOut()
		 internal virtual void IntObjectEntryVisitorShouldNotSeeEntriesAfterRequestingBreakOut()
		 {
			  // GIVEN
			  PrimitiveIntObjectMap<int> map = Primitive.intObjectMap();
			  map.Put( 1, 100 );
			  map.Put( 2, 200 );
			  map.Put( 3, 300 );
			  map.Put( 4, 400 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicInteger counter = new java.util.concurrent.atomic.AtomicInteger();
			  AtomicInteger counter = new AtomicInteger();

			  // WHEN
			  map.VisitEntries( ( key, value ) => counter.incrementAndGet() > 2 );

			  // THEN
			  assertThat( counter.get(), @is(3) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Test void longIntKeyVisitorShouldSeeAllEntriesIfItDoesNotBreakOut()
		 internal virtual void LongIntKeyVisitorShouldSeeAllEntriesIfItDoesNotBreakOut()
		 {
			  // GIVEN
			  PrimitiveLongIntMap map = Primitive.longIntMap();
			  map.Put( 1, 100 );
			  map.Put( 2, 200 );
			  map.Put( 3, 300 );
			  PrimitiveLongVisitor<Exception> visitor = mock( typeof( PrimitiveLongVisitor ) );

			  // WHEN
			  map.VisitKeys( visitor );

			  // THEN
			  verify( visitor ).visited( 1 );
			  verify( visitor ).visited( 2 );
			  verify( visitor ).visited( 3 );
			  verifyNoMoreInteractions( visitor );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void longIntKeyVisitorShouldNotSeeEntriesAfterRequestingBreakOut()
		 internal virtual void LongIntKeyVisitorShouldNotSeeEntriesAfterRequestingBreakOut()
		 {
			  // GIVEN
			  PrimitiveLongIntMap map = Primitive.longIntMap();
			  map.Put( 1, 100 );
			  map.Put( 2, 200 );
			  map.Put( 3, 300 );
			  map.Put( 4, 400 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicInteger counter = new java.util.concurrent.atomic.AtomicInteger();
			  AtomicInteger counter = new AtomicInteger();

			  // WHEN
			  map.VisitKeys( value => counter.incrementAndGet() > 2 );

			  // THEN
			  assertThat( counter.get(), @is(3) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Test void longLongKeyVisitorShouldSeeAllEntriesIfItDoesNotBreakOut()
		 internal virtual void LongLongKeyVisitorShouldSeeAllEntriesIfItDoesNotBreakOut()
		 {
			  // GIVEN
			  PrimitiveLongVisitor<Exception> visitor = mock( typeof( PrimitiveLongVisitor ) );
			  using ( PrimitiveLongLongMap map = Primitive.longLongMap() )
			  {
					map.Put( 1, 100 );
					map.Put( 2, 200 );
					map.Put( 3, 300 );

					// WHEN
					map.VisitKeys( visitor );
			  }

			  // THEN
			  verify( visitor ).visited( 1 );
			  verify( visitor ).visited( 2 );
			  verify( visitor ).visited( 3 );
			  verifyNoMoreInteractions( visitor );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void longLongKeyVisitorShouldNotSeeEntriesAfterRequestingBreakOut()
		 internal virtual void LongLongKeyVisitorShouldNotSeeEntriesAfterRequestingBreakOut()
		 {
			  // GIVEN
			  AtomicInteger counter = new AtomicInteger();
			  using ( PrimitiveLongLongMap map = Primitive.longLongMap() )
			  {
					map.Put( 1, 100 );
					map.Put( 2, 200 );
					map.Put( 3, 300 );
					map.Put( 4, 400 );

					// WHEN
					map.VisitKeys( value => counter.incrementAndGet() > 2 );
			  }

			  // THEN
			  assertThat( counter.get(), @is(3) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Test void longLongOffHeapKeyVisitorShouldSeeAllEntriesIfItDoesNotBreakOut()
		 internal virtual void LongLongOffHeapKeyVisitorShouldSeeAllEntriesIfItDoesNotBreakOut()
		 {
			  // GIVEN
			  PrimitiveLongVisitor<Exception> visitor = mock( typeof( PrimitiveLongVisitor ) );
			  using ( PrimitiveLongLongMap map = Primitive.offHeapLongLongMap( GlobalMemoryTracker.INSTANCE ) )
			  {
					map.Put( 1, 100 );
					map.Put( 2, 200 );
					map.Put( 3, 300 );

					// WHEN
					map.VisitKeys( visitor );
			  }

			  // THEN
			  verify( visitor ).visited( 1 );
			  verify( visitor ).visited( 2 );
			  verify( visitor ).visited( 3 );
			  verifyNoMoreInteractions( visitor );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void longLongOffHeapKeyVisitorShouldNotSeeEntriesAfterRequestingBreakOut()
		 internal virtual void LongLongOffHeapKeyVisitorShouldNotSeeEntriesAfterRequestingBreakOut()
		 {
			  // GIVEN
			  AtomicInteger counter = new AtomicInteger();
			  using ( PrimitiveLongLongMap map = Primitive.offHeapLongLongMap( GlobalMemoryTracker.INSTANCE ) )
			  {
					map.Put( 1, 100 );
					map.Put( 2, 200 );
					map.Put( 3, 300 );
					map.Put( 4, 400 );

					// WHEN
					map.VisitKeys( value => counter.incrementAndGet() > 2 );
			  }

			  // THEN
			  assertThat( counter.get(), @is(3) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Test void longObjectKeyVisitorShouldSeeAllEntriesIfItDoesNotBreakOut()
		 internal virtual void LongObjectKeyVisitorShouldSeeAllEntriesIfItDoesNotBreakOut()
		 {
			  // GIVEN
			  PrimitiveLongObjectMap<int> map = Primitive.longObjectMap();
			  map.Put( 1, 100 );
			  map.Put( 2, 200 );
			  map.Put( 3, 300 );
			  PrimitiveLongVisitor<Exception> visitor = mock( typeof( PrimitiveLongVisitor ) );

			  // WHEN
			  map.VisitKeys( visitor );

			  // THEN
			  verify( visitor ).visited( 1 );
			  verify( visitor ).visited( 2 );
			  verify( visitor ).visited( 3 );
			  verifyNoMoreInteractions( visitor );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void longObjectKeyVisitorShouldNotSeeEntriesAfterRequestingBreakOut()
		 internal virtual void LongObjectKeyVisitorShouldNotSeeEntriesAfterRequestingBreakOut()
		 {
			  // GIVEN
			  PrimitiveLongObjectMap<int> map = Primitive.longObjectMap();
			  map.Put( 1, 100 );
			  map.Put( 2, 200 );
			  map.Put( 3, 300 );
			  map.Put( 4, 400 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicInteger counter = new java.util.concurrent.atomic.AtomicInteger();
			  AtomicInteger counter = new AtomicInteger();

			  // WHEN
			  map.VisitKeys( value => counter.incrementAndGet() > 2 );

			  // THEN
			  assertThat( counter.get(), @is(3) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Test void intObjectKeyVisitorShouldSeeAllEntriesIfItDoesNotBreakOut()
		 internal virtual void IntObjectKeyVisitorShouldSeeAllEntriesIfItDoesNotBreakOut()
		 {
			  // GIVEN
			  PrimitiveIntObjectMap<int> map = Primitive.intObjectMap();
			  map.Put( 1, 100 );
			  map.Put( 2, 200 );
			  map.Put( 3, 300 );
			  PrimitiveIntVisitor<Exception> visitor = mock( typeof( PrimitiveIntVisitor ) );

			  // WHEN
			  map.VisitKeys( visitor );

			  // THEN
			  verify( visitor ).visited( 1 );
			  verify( visitor ).visited( 2 );
			  verify( visitor ).visited( 3 );
			  verifyNoMoreInteractions( visitor );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void intObjectKeyVisitorShouldNotSeeEntriesAfterRequestingBreakOut()
		 internal virtual void IntObjectKeyVisitorShouldNotSeeEntriesAfterRequestingBreakOut()
		 {
			  // GIVEN
			  PrimitiveIntObjectMap<int> map = Primitive.intObjectMap();
			  map.Put( 1, 100 );
			  map.Put( 2, 200 );
			  map.Put( 3, 300 );
			  map.Put( 4, 400 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicInteger counter = new java.util.concurrent.atomic.AtomicInteger();
			  AtomicInteger counter = new AtomicInteger();

			  // WHEN
			  map.VisitKeys( value => counter.incrementAndGet() > 2 );

			  // THEN
			  assertThat( counter.get(), @is(3) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void longObjectMapValuesContainsAllValues()
		 internal virtual void LongObjectMapValuesContainsAllValues()
		 {
			  PrimitiveLongObjectMap<string> map = Primitive.longObjectMap();
			  map.Put( 1, "a" );
			  map.Put( 2, "b" );
			  map.Put( 3, "c" );

			  assertThat( map.Values(), containsInAnyOrder("a", "b", "c") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void recursivePutGrowInterleavingShouldNotDropOriginalValues()
		 internal virtual void RecursivePutGrowInterleavingShouldNotDropOriginalValues()
		 {
			  // List of values which causes calls to put() call grow(), which will call put() which calls grow() again
			  IList<long> lst = Arrays.asList( 44988L, 44868L, 44271L, 44399L, 44502L, 44655L, 44348L, 44843L, 44254L, 44774L, 44476L, 44664L, 44485L, 44237L, 44953L, 44468L, 44970L, 44808L, 44527L, 44987L, 44672L, 44647L, 44467L, 44825L, 44740L, 44220L, 44851L, 44902L, 44791L, 44416L, 44365L, 44382L, 44885L, 44510L, 44553L, 44894L, 44288L, 44306L, 44450L, 44689L, 44305L, 44374L, 44323L, 44493L, 44706L, 44681L, 44578L, 44723L, 44331L, 44936L, 44289L, 44919L, 44433L, 44826L, 44757L, 44561L, 44595L, 44612L, 44996L, 44646L, 44834L, 44314L, 44544L, 44629L, 44357L );

			  VerifyMapRetainsAllEntries( lst );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void recursivePutGrowInterleavingShouldNotDropOriginalValuesEvenWhenFirstGrowAddsMoreValuesAfterSecondGrow()
		 internal virtual void RecursivePutGrowInterleavingShouldNotDropOriginalValuesEvenWhenFirstGrowAddsMoreValuesAfterSecondGrow()
		 {
			  // List of values that cause recursive growth like above, but this time the first grow wants to add more values
			  // to the table *after* the second grow has occurred.
			  IList<long> lst = Arrays.asList( 85380L, 85124L, 85252L, 85259L, 85005L, 85260L, 85132L, 85141L, 85397L, 85013L, 85269L, 85277L, 85149L, 85404L, 85022L, 85150L, 85029L, 85414L, 85158L, 85286L, 85421L, 85039L, 85167L, 85294L, 85166L, 85431L, 85303L, 85046L, 85311L, 85439L, 85438L, 85184L, 85056L, 85063L, 85320L, 85448L, 85201L, 85073L, 85329L, 85456L, 85328L, 85337L, 85081L, 85465L, 85080L, 85208L, 85473L, 85218L, 85346L, 85090L, 85097L, 85225L, 85354L, 85098L, 85482L, 85235L, 85363L, 85107L, 85490L, 85115L, 85499L, 85242L, 85175L, 85371L, 85192L );

			  VerifyMapRetainsAllEntries( lst );
		 }

		 private static void VerifyMapRetainsAllEntries( IList<long> lst )
		 {
			  PrimitiveLongIntMap map = Primitive.longIntMap();
			  ISet<long> set = new HashSet<long>();
			  foreach ( long? value in lst )
			  {
					assertThat( map.Put( value.Value, 1 ), @is( -1 ) );
					assertTrue( set.Add( value ) );
			  }

			  assertThat( map.Size(), @is(set.Count) );
			  foreach ( long? aLong in set )
			  {
					assertThat( map.Get( aLong.Value ), @is( 1 ) );
			  }
		 }
	}

}