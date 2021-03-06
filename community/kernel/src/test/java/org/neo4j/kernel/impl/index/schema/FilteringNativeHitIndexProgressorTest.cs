﻿using System.Collections.Generic;

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
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Org.Neo4j.Cursor;
	using Org.Neo4j.Index.@internal.gbptree;
	using IndexOrder = Org.Neo4j.@internal.Kernel.Api.IndexOrder;
	using IndexQuery = Org.Neo4j.@internal.Kernel.Api.IndexQuery;
	using TestIndexDescriptorFactory = Org.Neo4j.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using RandomRule = Org.Neo4j.Test.rule.RandomRule;
	using TextValue = Org.Neo4j.Values.Storable.TextValue;
	using Value = Org.Neo4j.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class FilteringNativeHitIndexProgressorTest
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
					keys.Add( Random.nextString() );
			  }

			  RawCursor<Hit<StringIndexKey, NativeIndexValue>, IOException> cursor = new ResultCursor( keys.GetEnumerator() );
			  NodeValueIterator valueClient = new NodeValueIteratorAnonymousInnerClass( this );
			  IndexQuery[] predicates = new IndexQuery[]{ mock( typeof( IndexQuery ) ) };
			  System.Predicate<string> filter = @string => @string.contains( "a" );
			  when( predicates[0].AcceptsValue( any( typeof( Value ) ) ) ).then( invocation => filter( ( ( TextValue )invocation.getArgument( 0 ) ).stringValue() ) );
			  FilteringNativeHitIndexProgressor<StringIndexKey, NativeIndexValue> progressor = new FilteringNativeHitIndexProgressor<StringIndexKey, NativeIndexValue>( cursor, valueClient, new List<RawCursor<Hit<KEY, VALUE>, IOException>>(), predicates );
			  valueClient.Initialize( TestIndexDescriptorFactory.forLabel( 0, 0 ), progressor, predicates, IndexOrder.NONE, true );
			  IList<long> result = new List<long>();

			  // when
			  while ( valueClient.HasNext() )
			  {
					result.Add( valueClient.Next() );
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

		 private class NodeValueIteratorAnonymousInnerClass : NodeValueIterator
		 {
			 private readonly FilteringNativeHitIndexProgressorTest _outerInstance;

			 public NodeValueIteratorAnonymousInnerClass( FilteringNativeHitIndexProgressorTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override bool needsValues()
			 {
				  return true;
			 }
		 }
	}

}