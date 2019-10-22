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
namespace Neo4Net.Values.Storable
{
	using Test = org.junit.jupiter.api.Test;


	using TemporalParseException = Neo4Net.Values.utils.TemporalParseException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.LocalTimeValue.localTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.LocalTimeValue.parse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.utils.AnyValueTestUtil.assertEqual;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.utils.AnyValueTestUtil.assertNotEqual;

	internal class LocalTimeValueTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldParseTimeWithOnlyHour()
		 internal virtual void ShouldParseTimeWithOnlyHour()
		 {
			  assertEquals( localTime( 14, 0, 0, 0 ), parse( "14" ) );
			  assertEquals( localTime( 4, 0, 0, 0 ), parse( "4" ) );
			  assertEquals( localTime( 4, 0, 0, 0 ), parse( "04" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldParseTimeWithHourAndMinute()
		 internal virtual void ShouldParseTimeWithHourAndMinute()
		 {
			  assertEquals( localTime( 14, 5, 0, 0 ), parse( "1405" ) );
			  assertEquals( localTime( 14, 5, 0, 0 ), parse( "14:5" ) );
			  assertEquals( localTime( 4, 15, 0, 0 ), parse( "4:15" ) );
			  assertEquals( localTime( 9, 7, 0, 0 ), parse( "9:7" ) );
			  assertEquals( localTime( 3, 4, 0, 0 ), parse( "03:04" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldParseTimeWithHourMinuteAndSecond()
		 internal virtual void ShouldParseTimeWithHourMinuteAndSecond()
		 {
			  assertEquals( localTime( 14, 5, 17, 0 ), parse( "140517" ) );
			  assertEquals( localTime( 14, 5, 17, 0 ), parse( "14:5:17" ) );
			  assertEquals( localTime( 4, 15, 4, 0 ), parse( "4:15:4" ) );
			  assertEquals( localTime( 9, 7, 19, 0 ), parse( "9:7:19" ) );
			  assertEquals( localTime( 3, 4, 1, 0 ), parse( "03:04:01" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldParseTimeWithHourMinuteSecondAndFractions()
		 internal virtual void ShouldParseTimeWithHourMinuteSecondAndFractions()
		 {
			  assertEquals( localTime( 14, 5, 17, 123000000 ), parse( "140517.123" ) );
			  assertEquals( localTime( 14, 5, 17, 1 ), parse( "14:5:17.000000001" ) );
			  assertEquals( localTime( 4, 15, 4, 0 ), parse( "4:15:4.000" ) );
			  assertEquals( localTime( 9, 7, 19, 999999999 ), parse( "9:7:19.999999999" ) );
			  assertEquals( localTime( 3, 4, 1, 123456789 ), parse( "03:04:01.123456789" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFailToParseTimeOutOfRange()
		 internal virtual void ShouldFailToParseTimeOutOfRange()
		 {
			  assertThrows( typeof( TemporalParseException ), () => parse("24") );
			  assertThrows( typeof( TemporalParseException ), () => parse("1760") );
			  assertThrows( typeof( TemporalParseException ), () => parse("173260") );
			  assertThrows( typeof( TemporalParseException ), () => parse("173250.0000000001") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldWriteLocalTime()
		 internal virtual void ShouldWriteLocalTime()
		 {
			  // given
			  foreach ( LocalTimeValue value in new LocalTimeValue[] { localTime( 0, 0, 0, 0 ), localTime( 11, 22, 33, 123456789 ), localTime( 2, 3, 4, 5 ), localTime( 23, 59, 59, 999999999 ) } )
			  {
					IList<LocalTimeValue> values = new List<LocalTimeValue>( 1 );
					ValueWriter<Exception> writer = new AssertOnlyAnonymousInnerClass( this, values );

					// when
					value.WriteTo( writer );

					// then
					assertEquals( singletonList( value ), values );
			  }
		 }

		 private class AssertOnlyAnonymousInnerClass : ThrowingValueWriter.AssertOnly
		 {
			 private readonly LocalTimeValueTest _outerInstance;

			 private IList<LocalTimeValue> _values;

			 public AssertOnlyAnonymousInnerClass( LocalTimeValueTest outerInstance, IList<LocalTimeValue> values )
			 {
				 this.outerInstance = outerInstance;
				 this._values = values;
			 }

			 public override void writeLocalTime( LocalTime localTime )
			 {
				  _values.Add( localTime( localTime ) );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldEqualItself()
		 internal virtual void ShouldEqualItself()
		 {
			  assertEqual( localTime( 10, 52, 5, 6 ), localTime( 10, 52, 5, 6 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotEqualOther()
		 internal virtual void ShouldNotEqualOther()
		 {
			  assertNotEqual( localTime( 10, 52, 5, 6 ), localTime( 10, 52, 5, 7 ) );
		 }
	}

}