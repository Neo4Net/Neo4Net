﻿/*
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
namespace Neo4Net.Collections.Helpers
{
	using Test = org.junit.jupiter.api.Test;

	using Neo4Net.GraphDb;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.spy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterators.asResourceIterator;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterators.iterator;

	internal class CombiningResourceIteratorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotCloseDuringIteration()
		 internal virtual void ShouldNotCloseDuringIteration()
		 {
			  // Given
			  IResourceIterator<long> it1 = spy( asResourceIterator( iterator( 1L, 2L, 3L ) ) );
			  IResourceIterator<long> it2 = spy( asResourceIterator( iterator( 5L, 6L, 7L ) ) );
			  CombiningResourceIterator<long> combingIterator = new CombiningResourceIterator<long>( iterator( it1, it2 ) );

			  // When I iterate through it, things come back in the right order
			  assertThat( Iterators.AsList( combingIterator ), equalTo( asList( 1L, 2L, 3L, 5L, 6L, 7L ) ) );

			  // Then
			  verify( it1, never() ).close();
			  verify( it2, never() ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void closesAllIteratorsOnShutdown()
		 internal virtual void ClosesAllIteratorsOnShutdown()
		 {
			  // Given
			  IResourceIterator<long> it1 = spy( asResourceIterator( iterator( 1L, 2L, 3L ) ) );
			  IResourceIterator<long> it2 = spy( asResourceIterator( iterator( 5L, 6L, 7L ) ) );
			  CombiningResourceIterator<long> combingIterator = new CombiningResourceIterator<long>( iterator( it1, it2 ) );

			  // Given I iterate through half of it
			  int iterations = 4;
			  while ( iterations-- > 0 )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					combingIterator.Next();
			  }

			  // When
			  combingIterator.Close();

			  // Then
			  verify( it1 ).close();
			  verify( it2 ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleSingleItemIterators()
		 internal virtual void ShouldHandleSingleItemIterators()
		 {
			  // Given
			  IResourceIterator<long> it1 = asResourceIterator( iterator( 1L ) );
			  IResourceIterator<long> it2 = asResourceIterator( iterator( 5L, 6L, 7L ) );
			  CombiningResourceIterator<long> combingIterator = new CombiningResourceIterator<long>( iterator( it1, it2 ) );

			  // When I iterate through it, things come back in the right order
			  assertThat( Iterators.AsList( combingIterator ), equalTo( asList( 1L, 5L, 6L, 7L ) ) );
		 }
	}

}