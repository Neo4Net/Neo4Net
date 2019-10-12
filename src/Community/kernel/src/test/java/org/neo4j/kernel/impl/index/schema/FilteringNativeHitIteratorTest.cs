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
namespace Neo4Net.Kernel.Impl.Index.Schema
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Neo4Net.Cursors;
	using Neo4Net.Index.@internal.gbptree;
	using IndexQuery = Neo4Net.@internal.Kernel.Api.IndexQuery;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using TextValue = Neo4Net.Values.Storable.TextValue;
	using Value = Neo4Net.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class FilteringNativeHitIteratorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.RandomRule random = new org.neo4j.test.rule.RandomRule();
		 public readonly RandomRule Random = new RandomRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFilterResults()
		 public virtual void ShouldFilterResults()
		 {
			  // given
			  IList<string> keys = new List<string>();
			  for ( int i = 0; i < 100; i++ )
			  {
					// duplicates are fine
					keys.Add( Random.nextAlphaNumericString() );
			  }

			  RawCursor<Hit<StringIndexKey, NativeIndexValue>, IOException> cursor = new ResultCursor( keys.GetEnumerator() );
			  IndexQuery[] predicates = new IndexQuery[]{ mock( typeof( IndexQuery ) ) };
			  System.Predicate<string> filter = @string => @string.contains( "a" );
			  when( predicates[0].AcceptsValue( any( typeof( Value ) ) ) ).then( invocation => filter( ( ( TextValue )invocation.getArgument( 0 ) ).stringValue() ) );
			  FilteringNativeHitIterator<StringIndexKey, NativeIndexValue> iterator = new FilteringNativeHitIterator<StringIndexKey, NativeIndexValue>( cursor, new List<RawCursor<Hit<KEY, VALUE>, IOException>>(), predicates );
			  IList<long> result = new List<long>();

			  // when
			  while ( iterator.hasNext() )
			  {
					result.Add( iterator.next() );
			  }

			  // then
			  for ( int i = 0; i < keys.Count; i++ )
			  {
					if ( filter( keys[i] ) )
					{
						 assertTrue( result.RemoveAt( ( long ) i ) );
					}
			  }
			  assertTrue( result.Count == 0 );
		 }
	}

}