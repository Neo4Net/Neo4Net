using System;
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
	using Monitor = Neo4Net.Collections.primitive.hopscotch.HopScotchHashingAlgorithm.Monitor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.collection.primitive.Primitive.VALUE_MARKER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.collection.primitive.hopscotch.HopScotchHashingAlgorithm.NO_MONITOR;

	internal class PrimitiveLongSetTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldContainAddedValues_generated_1()
		 internal virtual void ShouldContainAddedValuesGenerated_1()
		 {
			  // GIVEN
			  PrimitiveLongSet set = NewSet( 15 );
			  ISet<long> expectedValues = new HashSet<long>();
			  long[] valuesToAdd = new long[]{ 1207043189, 380713862, 1902858197, 1996873101, 1357024628, 1044248801, 1558157493, 2040311008, 2017660098, 1332670047, 663662790, 2063747422, 1554358949, 1761477445, 1141526838, 1698679618, 1279767067, 508574, 2071755904 };
			  foreach ( long key in valuesToAdd )
			  {
					set.Add( key );
					expectedValues.Add( key );
			  }

			  // WHEN/THEN
			  bool existedBefore = set.Contains( 679990875 );
			  bool added = set.Add( 679990875 );
			  bool existsAfter = set.Contains( 679990875 );
			  assertFalse( existedBefore, "679990875 should not exist before adding here" );
			  assertTrue( added, "679990875 should be reported as added here" );
			  assertTrue( existsAfter, "679990875 should exist" );
			  expectedValues.Add( 679990875L );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Set<long> visitedKeys = new java.util.HashSet<>();
			  ISet<long> visitedKeys = new HashSet<long>();
			  set.VisitKeys(value =>
			  {
				assertTrue( visitedKeys.Add( value ) );
				return false;
			  });
			  assertEquals( expectedValues, visitedKeys );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldContainAddedValues_generated_6()
		 internal virtual void ShouldContainAddedValuesGenerated_6()
		 {
			  // GIVEN
			  PrimitiveLongSet set = NewSet( 11 );
			  set.Add( 492321488 );
			  set.Add( 877087251 );
			  set.Add( 1809668113 );
			  set.Add( 1766034186 );
			  set.Add( 1879253467 );
			  set.Add( 669295491 );
			  set.Add( 176011165 );
			  set.Add( 1638959981 );
			  set.Add( 1093132636 );
			  set.Add( 6133241 );
			  set.Add( 486112773 );
			  set.Add( 205218385 );
			  set.Add( 1756491867 );
			  set.Add( 90390732 );
			  set.Add( 937266036 );
			  set.Add( 1269020584 );
			  set.Add( 521469166 );
			  set.Add( 1314928747 );

			  // WHEN/THEN
			  bool existedBefore = set.Contains( 2095121629 );
			  bool added = set.Add( 2095121629 );
			  bool existsAfter = set.Contains( 2095121629 );
			  assertFalse( existedBefore, "2095121629 should not exist before adding here" );
			  assertTrue( added, "2095121629 should be reported as added here" );
			  assertTrue( existsAfter, "2095121629 should exist" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldContainAddedValues_generated_4()
		 internal virtual void ShouldContainAddedValuesGenerated_4()
		 {
			  // GIVEN
			  PrimitiveLongSet set = NewSet( 9 );
			  set.Add( 1934106304 );
			  set.Add( 783754072 );
			  set.Remove( 1934106304 );

			  // WHEN/THEN
			  bool existedBefore = set.Contains( 783754072 );
			  bool added = set.Add( 783754072 );
			  bool existsAfter = set.Contains( 783754072 );
			  assertTrue( existedBefore, "783754072 should exist before adding here" );
			  assertFalse( added, "783754072 should not be reported as added here" );
			  assertTrue( existsAfter, "783754072 should exist" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldOnlyContainAddedValues_generated_8()
		 internal virtual void ShouldOnlyContainAddedValuesGenerated_8()
		 {
			  // GIVEN
			  PrimitiveLongSet set = NewSet( 7 );
			  set.Add( 375712513 );
			  set.Remove( 1507941820 );
			  set.Add( 671750317 );
			  set.Remove( 1054641019 );
			  set.Add( 671750317 );
			  set.Add( 1768202223 );
			  set.Add( 1768202223 );
			  set.Add( 1927780165 );
			  set.Add( 2139399764 );
			  set.Remove( 1243370828 );
			  set.Add( 1768202223 );
			  set.Add( 1335041891 );
			  set.Remove( 1578984313 );
			  set.Add( 1227954408 );
			  set.Remove( 946917826 );
			  set.Add( 1768202223 );
			  set.Add( 375712513 );
			  set.Add( 1668515054 );
			  set.Add( 401047579 );
			  set.Add( 33226244 );
			  set.Add( 126791689 );
			  set.Add( 401047579 );
			  set.Add( 1963437853 );
			  set.Add( 1739617766 );
			  set.Add( 671750317 );
			  set.Add( 401047579 );
			  set.Add( 789094467 );
			  set.Add( 1291421506 );
			  set.Add( 1694968582 );
			  set.Add( 1508353936 );

			  // WHEN/THEN
			  bool existedBefore = set.Contains( 1739617766 );
			  bool added = set.Add( 1739617766 );
			  bool existsAfter = set.Contains( 1739617766 );
			  assertTrue( existedBefore, "1739617766 should exist before adding here" );
			  assertFalse( added, "1739617766 should not be reported as added here" );
			  assertTrue( existsAfter, "1739617766 should exist" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldContainReallyBigLongValue()
		 internal virtual void ShouldContainReallyBigLongValue()
		 {
			  // GIVEN
			  PrimitiveLongSet set = NewSet( 10 );
			  set.Add( 7416509207113022571L );

			  // WHEN/THEN
			  bool existedBefore = set.Contains( 7620037383187366331L );
			  bool added = set.Add( 7620037383187366331L );
			  bool existsAfter = set.Contains( 7620037383187366331L );
			  assertFalse( existedBefore, "7620037383187366331 should not exist before adding here" );
			  assertTrue( added, "7620037383187366331 should be reported as added here" );
			  assertTrue( existsAfter, "7620037383187366331 should exist" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldOnlyContainAddedValues()
		 internal virtual void ShouldOnlyContainAddedValues()
		 {
			  // GIVEN
			  PrimitiveLongSet set = NewSet( 13 );
			  set.Add( 52450040186687566L );
			  set.Add( 52450040186687566L );
			  set.Add( 5165002753277288833L );
			  set.Add( 4276883133717080762L );
			  set.Add( 5547940863757133161L );
			  set.Add( 8933830774911919116L );
			  set.Add( 3298254474623565974L );
			  set.Add( 3366017425691021883L );
			  set.Add( 8933830774911919116L );
			  set.Add( 2962608069916354604L );
			  set.Add( 3366017425691021883L );
			  set.Remove( 4008464697042048519L );
			  set.Add( 5547940863757133161L );
			  set.Add( 52450040186687566L );
			  set.Add( 4276883133717080762L );
			  set.Remove( 3298254474623565974L );
			  set.Remove( 180852386934131061L );
			  set.Add( 4835176885665539239L );
			  set.Add( 52450040186687566L );
			  set.Add( 4591251124405056753L );
			  set.Add( 5165002753277288833L );
			  set.Add( 8933830774911919116L );
			  set.Remove( 3458250832356869483L );
			  set.Add( 3038543946711308923L );
			  set.Add( 8743060827282266460L );
			  set.Add( 5771902951077476377L );
			  set.Add( 4591251124405056753L );
			  set.Add( 4835176885665539239L );
			  set.Remove( 4827343064671369647L );
			  set.Add( 1533535091190658734L );
			  set.Remove( 7125666881901305989L );
			  set.Add( 1533535091190658734L );
			  set.Add( 52450040186687566L );
			  set.Remove( 1333521853804287175L );
			  set.Add( 2962608069916354604L );
			  set.Add( 5914630622072544054L );
			  set.Add( 52450040186687566L );
			  set.Add( 8933830774911919116L );
			  set.Add( 6198968672674664718L );
			  set.Add( 6239021001199390909L );
			  set.Add( 6563452500080365738L );
			  set.Add( 6128819131542184648L );
			  set.Add( 5914630622072544054L );
			  set.Add( 7024933384543504364L );
			  set.Remove( 3949644814017615281L );
			  set.Add( 3459376060749741528L );
			  set.Add( 3201250389951283395L );
			  set.Add( 4463681497523421181L );
			  set.Add( 4304197328678536531L );
			  set.Remove( 4559066538220393098L );
			  set.Add( 2870119173652414003L );
			  set.Add( 4048902329274369372L );
			  set.Add( 3366017425691021883L );
			  set.Remove( 1092409052848583664L );
			  set.Add( 7024933384543504364L );
			  set.Add( 4276883133717080762L );
			  set.Add( 5914630622072544054L );
			  set.Add( 4048902329274369372L );
			  set.Add( 4304197328678536531L );
			  set.Add( 4151178923662618318L );
			  set.Remove( 51389524801735953L );
			  set.Add( 5371788772386487501L );
			  set.Remove( 8933830774911919116L );
			  set.Add( 4928410670964886834L );
			  set.Add( 8306393274966855450L );
			  set.Add( 2870119173652414003L );
			  set.Add( 8281622709908651825L );
			  set.Remove( 9194058056102544672L );
			  set.Remove( 5547940863757133161L );
			  set.Add( 9184590238993521817L );
			  set.Add( 5085293141623130492L );
			  set.Add( 5633993155928642090L );
			  set.Remove( 8794875254017117580L );
			  set.Add( 5894404415376700909L );
			  set.Add( 4835176885665539239L );
			  set.Remove( 8743060827282266460L );
			  set.Remove( 3460096065015553722L );
			  set.Remove( 3296380689310185627L );
			  set.Add( 337242488691685550L );
			  set.Add( 6239021001199390909L );
			  set.Add( 9104240733803011297L );
			  set.Add( 807326424150812437L );
			  set.Add( 3336115330297894183L );
			  set.Add( 1788796898879121715L );
			  set.Add( 5756965080438171769L );
			  set.Remove( 4366313798399763194L );
			  set.Add( 6198968672674664718L );
			  set.Add( 486897301084183614L );
			  set.Add( 2870119173652414003L );
			  set.Add( 5085293141623130492L );
			  set.Add( 5771902951077476377L );
			  set.Remove( 6563452500080365738L );
			  set.Add( 5347453991851285676L );
			  set.Add( 7437999035528158926L );
			  set.Add( 3223908005448803428L );
			  set.Add( 4300856565210203390L );
			  set.Remove( 4732570527126410147L );
			  set.Add( 2180591071166584277L );
			  set.Add( 5160374384234262648L );
			  set.Remove( 5165002753277288833L );
			  set.Add( 4463681497523421181L );
			  set.Add( 7360196143740041480L );
			  set.Add( 4928410670964886834L );
			  set.Add( 807326424150812437L );
			  set.Remove( 4069279832998820447L );
			  set.Remove( 337242488691685550L );
			  set.Add( 3201250389951283395L );
			  set.Add( 4012293068834101219L );
			  set.Add( 2333643358471038273L );
			  set.Add( 1158824602601458449L );
			  set.Remove( 3906518453155830597L );
			  set.Add( 7402912598585277900L );
			  set.Add( 6556025329057634951L );
			  set.Add( 6684709657047103197L );
			  set.Remove( 3448774195820272496L );
			  set.Add( 715736913341007544L );
			  set.Add( 9104240733803011297L );

			  // WHEN/THEN
			  bool existedBefore = set.Contains( 1103190229303827372L );
			  bool added = set.Add( 1103190229303827372L );
			  bool existsAfter = set.Contains( 1103190229303827372L );
			  assertFalse( existedBefore, "1103190229303827372 should not exist before adding here" );
			  assertTrue( added, "1103190229303827372 should be reported as added here" );
			  assertTrue( existsAfter, "1103190229303827372 should exist" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Test void longVisitorShouldSeeAllEntriesIfItDoesNotBreakOut()
		 internal virtual void LongVisitorShouldSeeAllEntriesIfItDoesNotBreakOut()
		 {
			  // GIVEN
			  PrimitiveLongSet set = Primitive.longSet();
			  set.Add( 1 );
			  set.Add( 2 );
			  set.Add( 3 );
			  PrimitiveLongVisitor<Exception> visitor = mock( typeof( PrimitiveLongVisitor ) );

			  // WHEN
			  set.VisitKeys( visitor );

			  // THEN
			  verify( visitor ).visited( 1 );
			  verify( visitor ).visited( 2 );
			  verify( visitor ).visited( 3 );
			  verifyNoMoreInteractions( visitor );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void longVisitorShouldNotSeeEntriesAfterRequestingBreakOut()
		 internal virtual void LongVisitorShouldNotSeeEntriesAfterRequestingBreakOut()
		 {
			  // GIVEN
			  PrimitiveIntSet map = Primitive.intSet();
			  map.Add( 1 );
			  map.Add( 2 );
			  map.Add( 3 );
			  map.Add( 4 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicInteger counter = new java.util.concurrent.atomic.AtomicInteger();
			  AtomicInteger counter = new AtomicInteger();

			  // WHEN
			  map.VisitKeys( value => counter.incrementAndGet() > 2 );

			  // THEN
			  assertThat( counter.get(), @is(3) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Test void intVisitorShouldSeeAllEntriesIfItDoesNotBreakOut()
		 internal virtual void IntVisitorShouldSeeAllEntriesIfItDoesNotBreakOut()
		 {
			  // GIVEN
			  PrimitiveIntSet set = Primitive.intSet();
			  set.Add( 1 );
			  set.Add( 2 );
			  set.Add( 3 );
			  PrimitiveIntVisitor<Exception> visitor = mock( typeof( PrimitiveIntVisitor ) );

			  // WHEN
			  set.VisitKeys( visitor );

			  // THEN
			  verify( visitor ).visited( 1 );
			  verify( visitor ).visited( 2 );
			  verify( visitor ).visited( 3 );
			  verifyNoMoreInteractions( visitor );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void intVisitorShouldNotSeeEntriesAfterRequestingBreakOut()
		 internal virtual void IntVisitorShouldNotSeeEntriesAfterRequestingBreakOut()
		 {
			  // GIVEN
			  PrimitiveIntSet map = Primitive.intSet();
			  map.Add( 1 );
			  map.Add( 2 );
			  map.Add( 3 );
			  map.Add( 4 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicInteger counter = new java.util.concurrent.atomic.AtomicInteger();
			  AtomicInteger counter = new AtomicInteger();

			  // WHEN
			  map.VisitKeys( value => counter.incrementAndGet() > 2 );

			  // THEN
			  assertThat( counter.get(), @is(3) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleEmptySet()
		 internal virtual void ShouldHandleEmptySet()
		 {
			  // GIVEN
			  PrimitiveLongSet set = Primitive.longSet( 0 );

			  // THEN
			  assertFalse( set.Contains( 564 ) );
		 }

		 private static PrimitiveLongHashSet NewSet( int h )
		 {
			  return NewSet( h, NO_MONITOR );
		 }

		 private static PrimitiveLongHashSet NewSet( int h, Monitor monitor )
		 {
			  return new PrimitiveLongHashSet( new LongKeyTable<object>( h, VALUE_MARKER ), VALUE_MARKER, monitor );
		 }
	}

}