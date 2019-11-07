using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.routing.load_balancing.filters
{
	using Test = org.junit.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterators.asSet;

	public class FirstValidRuleTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUseResultOfFirstNonEmpty()
		 public virtual void ShouldUseResultOfFirstNonEmpty()
		 {
			  // given
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  Filter<int> removeValuesOfFive = data => data.Where( value => value != 5 ).collect( Collectors.toSet() );
			  Filter<int> countMoreThanFour = data => data.size() > 4 ? data : Collections.emptySet();
			  Filter<int> countMoreThanThree = data => data.size() > 3 ? data : Collections.emptySet();

			  FilterChain<int> ruleA = new FilterChain<int>( new IList<Filter<T>> { removeValuesOfFive, countMoreThanFour } ); // should not succeed
			  FilterChain<int> ruleB = new FilterChain<int>( new IList<Filter<T>> { removeValuesOfFive, countMoreThanThree } ); // should succeed
			  FilterChain<int> ruleC = new FilterChain<int>( singletonList( countMoreThanFour ) ); // never reached

			  FirstValidRule<int> firstValidRule = new FirstValidRule<int>( new IList<FilterChain<T>> { ruleA, ruleB, ruleC } );

			  ISet<int> data = asSet( 5, 1, 5, 2, 5, 3, 5, 4 );

			  // when
			  data = firstValidRule.Apply( data );

			  // then
			  assertEquals( asSet( 1, 2, 3, 4 ), data );
		 }
	}

}