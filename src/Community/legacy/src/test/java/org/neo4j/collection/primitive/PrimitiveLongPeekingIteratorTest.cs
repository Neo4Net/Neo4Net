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
namespace Neo4Net.Collection.primitive
{
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;

	internal class PrimitiveLongPeekingIteratorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDetectMultipleValues()
		 internal virtual void ShouldDetectMultipleValues()
		 {
			  // GIVEN
			  long[] values = new long[]{ 1, 2, 3 };
			  PrimitiveLongIterator actual = PrimitiveLongCollections.Iterator( values );
			  PrimitiveLongPeekingIterator peekingIterator = new PrimitiveLongPeekingIterator( actual );

			  // THEN
			  assertTrue( peekingIterator.HasMultipleValues() );
			  foreach ( long value in values )
			  {
					assertEquals( value, peekingIterator.Next() );
			  }
			  assertFalse( peekingIterator.HasNext() );
			  assertTrue( peekingIterator.HasMultipleValues() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDetectSingleValue()
		 internal virtual void ShouldDetectSingleValue()
		 {
			  // GIVEN
			  long[] values = new long[]{ 1 };
			  PrimitiveLongIterator actual = PrimitiveLongCollections.Iterator( values );
			  PrimitiveLongPeekingIterator peekingIterator = new PrimitiveLongPeekingIterator( actual );
			  // THEN
			  assertFalse( peekingIterator.HasMultipleValues() );
			  foreach ( long value in values )
			  {
					assertEquals( value, peekingIterator.Next() );
			  }
			  assertFalse( peekingIterator.HasNext() );
			  assertFalse( peekingIterator.HasMultipleValues() );
		 }
	}

}