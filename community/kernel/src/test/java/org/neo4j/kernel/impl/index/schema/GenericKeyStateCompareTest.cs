using System;
using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.Impl.Index.Schema
{
	using Test = org.junit.jupiter.api.Test;


	using DateTimeValue = Org.Neo4j.Values.Storable.DateTimeValue;
	using DateValue = Org.Neo4j.Values.Storable.DateValue;
	using DurationValue = Org.Neo4j.Values.Storable.DurationValue;
	using LocalDateTimeValue = Org.Neo4j.Values.Storable.LocalDateTimeValue;
	using LocalTimeValue = Org.Neo4j.Values.Storable.LocalTimeValue;
	using TimeValue = Org.Neo4j.Values.Storable.TimeValue;
	using Value = Org.Neo4j.Values.Storable.Value;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;

	internal class GenericKeyStateCompareTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void compareGenericKeyState()
		 internal virtual void CompareGenericKeyState()
		 {
			  IList<Value> allValues = Arrays.asList( Values.of( "string1" ), Values.of( 42 ), Values.of( true ), Values.of( new char[]{ 'a', 'z' } ), Values.of( new string[]{ "arrayString1", "arraysString2" } ), Values.of( new sbyte[]{ ( sbyte ) 1, ( sbyte ) 12 } ), Values.of( new short[]{ 314, 1337 } ), Values.of( new int[]{ 3140, 13370 } ), Values.of( new long[]{ 31400, 133700 } ), Values.of( new bool[]{ false, true } ), DateValue.epochDate( 2 ), LocalTimeValue.localTime( 100000 ), TimeValue.time( 43_200_000_000_000L, ZoneOffset.UTC ), TimeValue.time( 43_201_000_000_000L, ZoneOffset.UTC ), TimeValue.time( 43_200_000_000_000L, ZoneOffset.of( "+01:00" ) ), TimeValue.time( 46_800_000_000_000L, ZoneOffset.UTC ), LocalDateTimeValue.localDateTime( 2018, 3, 1, 13, 50, 42, 1337 ), DateTimeValue.datetime( 2014, 3, 25, 12, 45, 13, 7474, "UTC" ), DateTimeValue.datetime( 2014, 3, 25, 12, 45, 13, 7474, "Europe/Stockholm" ), DateTimeValue.datetime( 2014, 3, 25, 12, 45, 13, 7474, "+05:00" ), DateTimeValue.datetime( 2015, 3, 25, 12, 45, 13, 7474, "+05:00" ), DateTimeValue.datetime( 2014, 4, 25, 12, 45, 13, 7474, "+05:00" ), DateTimeValue.datetime( 2014, 3, 26, 12, 45, 13, 7474, "+05:00" ), DateTimeValue.datetime( 2014, 3, 25, 13, 45, 13, 7474, "+05:00" ), DateTimeValue.datetime( 2014, 3, 25, 12, 46, 13, 7474, "+05:00" ), DateTimeValue.datetime( 2014, 3, 25, 12, 45, 14, 7474, "+05:00" ), DateTimeValue.datetime( 2014, 3, 25, 12, 45, 13, 7475, "+05:00" ), DateTimeValue.datetime( 2038, 1, 18, 9, 14, 7, 0, "-18:00" ), DateTimeValue.datetime( 10000, 100, ZoneOffset.ofTotalSeconds( 3 ) ), DateTimeValue.datetime( 10000, 101, ZoneOffset.ofTotalSeconds( -3 ) ), DurationValue.duration( 10, 20, 30, 40 ), DurationValue.duration( 11, 20, 30, 40 ), DurationValue.duration( 10, 21, 30, 40 ), DurationValue.duration( 10, 20, 31, 40 ), DurationValue.duration( 10, 20, 30, 41 ), Values.dateTimeArray( new ZonedDateTime[]{ ZonedDateTime.of( 2018, 10, 9, 8, 7, 6, 5, ZoneId.of( "UTC" ) ), ZonedDateTime.of( 2017, 9, 8, 7, 6, 5, 4, ZoneId.of( "UTC" ) ) } ), Values.localDateTimeArray( new DateTime[]{ new DateTime( 2018, 10, 9, 8, 7, 6, 5 ), new DateTime( 2018, 10, 9, 8, 7, 6, 5 ) } ), Values.timeArray( new OffsetTime[]{ OffsetTime.of( 20, 8, 7, 6, ZoneOffset.UTC ), OffsetTime.of( 20, 8, 7, 6, ZoneOffset.UTC ) } ), Values.dateArray( new LocalDate[]{ LocalDate.of( 2018, 12, 28 ), LocalDate.of( 2018, 12, 28 ) } ), Values.localTimeArray( new LocalTime[]{ LocalTime.of( 9, 28 ), LocalTime.of( 9, 28 ) } ), Values.durationArray( new DurationValue[]{ DurationValue.duration( 12, 10, 10, 10 ), DurationValue.duration( 12, 10, 10, 10 ) } ) );
			  allValues.sort( Values.COMPARATOR );

			  IList<GenericKey> states = new List<GenericKey>();
			  foreach ( Value value in allValues )
			  {
					GenericKey state = new GenericKey( null );
					state.WriteValue( value, NativeIndexKey.Inclusion.Neutral );
					states.Add( state );
			  }
			  Collections.shuffle( states );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  states.sort( GenericKey::compareValueTo );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  IList<Value> sortedStatesAsValues = states.Select( GenericKey::asValue ).ToList();
			  assertEquals( allValues, sortedStatesAsValues );
		 }
	}

}