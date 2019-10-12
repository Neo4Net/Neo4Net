using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.causalclustering.routing.load_balancing.filters
{
	using Test = org.junit.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;

	public class FilterChainTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFilterThroughAll()
		 public virtual void ShouldFilterThroughAll()
		 {
			  // given
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  Filter<int> removeValuesOfFive = data => data.Where( value => value != 5 ).collect( Collectors.toSet() );
			  Filter<int> mustHaveThreeValues = data => data.size() == 3 ? data : emptySet();
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  Filter<int> keepValuesBelowTen = data => data.Where( value => value < 10 ).collect( Collectors.toSet() );

			  FilterChain<int> filterChain = new FilterChain<int>( new IList<Filter<T>> { removeValuesOfFive, mustHaveThreeValues, keepValuesBelowTen } );
			  ISet<int> data = asSet( 5, 5, 5, 3, 5, 10, 9 ); // carefully crafted to check order as well

			  // when
			  data = filterChain.Apply( data );

			  // then
			  assertEquals( asSet( 3, 9 ), data );
		 }
	}

}