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
namespace Neo4Net.GraphDb
{
	using Test = org.junit.jupiter.api.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.iterator;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.ResourceClosingIterator.newResourceIterator;

	internal class ResourceIterableTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void streamShouldCloseSingleOnCompleted()
		 internal virtual void StreamShouldCloseSingleOnCompleted()
		 {
			  // Given
			  AtomicBoolean closed = new AtomicBoolean( false );
			  ResourceIterator<int> resourceIterator = newResourceIterator( iterator( new int?[]{ 1, 2, 3 } ), () => closed.set(true) );

			  ResourceIterable<int> iterable = () => resourceIterator;

			  // When
			  IList<int> result = iterable.ToList();

			  // Then
			  assertEquals( asList( 1,2,3 ), result );
			  assertTrue( closed.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void streamShouldCloseMultipleOnCompleted()
		 internal virtual void StreamShouldCloseMultipleOnCompleted()
		 {
			  // Given
			  AtomicInteger closed = new AtomicInteger();
			  Resource resource = closed.incrementAndGet;
			  ResourceIterator<int> resourceIterator = newResourceIterator( iterator( new int?[]{ 1, 2, 3 } ), resource, resource );

			  ResourceIterable<int> iterable = () => resourceIterator;

			  // When
			  IList<int> result = iterable.ToList();

			  // Then
			  assertEquals( asList( 1,2,3 ), result );
			  assertEquals( 2, closed.get(), "two calls to close" );
		 }
	}

}