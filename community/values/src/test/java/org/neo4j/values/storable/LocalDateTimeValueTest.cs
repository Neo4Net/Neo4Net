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
namespace Org.Neo4j.Values.Storable
{
	using Test = org.junit.jupiter.api.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.DateValue.date;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.LocalDateTimeValue.localDateTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.LocalDateTimeValue.parse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.LocalTimeValue.localTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.utils.AnyValueTestUtil.assertEqual;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.utils.AnyValueTestUtil.assertNotEqual;

	internal class LocalDateTimeValueTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldParseDate()
		 internal virtual void ShouldParseDate()
		 {
			  assertEquals( localDateTime( date( 2017, 12, 17 ), localTime( 17, 14, 35, 123456789 ) ), parse( "2017-12-17T17:14:35.123456789" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldWriteDateTime()
		 internal virtual void ShouldWriteDateTime()
		 {
			  // given
			  foreach ( LocalDateTimeValue value in new LocalDateTimeValue[] { localDateTime( date( 2017, 3, 26 ), localTime( 1, 0, 0, 0 ) ), localDateTime( date( 2017, 3, 26 ), localTime( 2, 0, 0, 0 ) ), localDateTime( date( 2017, 3, 26 ), localTime( 3, 0, 0, 0 ) ), localDateTime( date( 2017, 10, 29 ), localTime( 2, 0, 0, 0 ) ), localDateTime( date( 2017, 10, 29 ), localTime( 3, 0, 0, 0 ) ), localDateTime( date( 2017, 10, 29 ), localTime( 4, 0, 0, 0 ) ) } )
			  {
					IList<LocalDateTimeValue> values = new List<LocalDateTimeValue>( 1 );
					ValueWriter<Exception> writer = new AssertOnlyAnonymousInnerClass( this, values );

					// when
					value.WriteTo( writer );

					// then
					assertEquals( singletonList( value ), values );
			  }
		 }

		 private class AssertOnlyAnonymousInnerClass : ThrowingValueWriter.AssertOnly
		 {
			 private readonly LocalDateTimeValueTest _outerInstance;

			 private IList<LocalDateTimeValue> _values;

			 public AssertOnlyAnonymousInnerClass( LocalDateTimeValueTest outerInstance, IList<LocalDateTimeValue> values )
			 {
				 this.outerInstance = outerInstance;
				 this._values = values;
			 }

			 public override void writeLocalDateTime( DateTime localDateTime )
			 {
				  _values.Add( localDateTime( localDateTime ) );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldEqualItself()
		 internal virtual void ShouldEqualItself()
		 {
			  assertEqual( localDateTime( 2018, 1, 31, 10, 52, 5, 6 ), localDateTime( 2018, 1, 31, 10, 52, 5, 6 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotEqualOther()
		 internal virtual void ShouldNotEqualOther()
		 {
			  assertNotEqual( localDateTime( 2018, 1, 31, 10, 52, 5, 6 ), localDateTime( 2018, 1, 31, 10, 52, 5, 7 ) );
		 }
	}

}