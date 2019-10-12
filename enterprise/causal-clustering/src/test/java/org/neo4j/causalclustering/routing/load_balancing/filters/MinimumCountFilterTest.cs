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
namespace Org.Neo4j.causalclustering.routing.load_balancing.filters
{
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;

	public class MinimumCountFilterTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFilterBelowCount()
		 public virtual void ShouldFilterBelowCount()
		 {
			  // given
			  MinimumCountFilter<int> minFilter = new MinimumCountFilter<int>( 3 );

			  ISet<int> input = asSet( 1, 2 );

			  // when
			  ISet<int> output = minFilter.Apply( input );

			  // then
			  assertEquals( emptySet(), output );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPassAtCount()
		 public virtual void ShouldPassAtCount()
		 {
			  // given
			  MinimumCountFilter<int> minFilter = new MinimumCountFilter<int>( 3 );

			  ISet<int> input = asSet( 1, 2, 3 );

			  // when
			  ISet<int> output = minFilter.Apply( input );

			  // then
			  assertEquals( input, output );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPassAboveCount()
		 public virtual void ShouldPassAboveCount()
		 {
			  // given
			  MinimumCountFilter<int> minFilter = new MinimumCountFilter<int>( 3 );

			  ISet<int> input = asSet( 1, 2, 3, 4 );

			  // when
			  ISet<int> output = minFilter.Apply( input );

			  // then
			  assertEquals( input, output );
		 }
	}

}