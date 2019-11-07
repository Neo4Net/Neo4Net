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
namespace Neo4Net.@unsafe.Impl.Batchimport.cache.idmapping.@string
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using RandomRule = Neo4Net.Test.rule.RandomRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.cache.idmapping.@string.BigIdTracker.MAX_ID;

	public class BigIdTrackerTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.RandomRule random = new Neo4Net.test.rule.RandomRule();
		 public readonly RandomRule Random = new RandomRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldKeepIdsAndMarkDuplicates()
		 public virtual void ShouldKeepIdsAndMarkDuplicates()
		 {
			  // given
			  int length = 10_000;
			  using ( BigIdTracker tracker = new BigIdTracker( NumberArrayFactory.HEAP.newByteArray( length, BigIdTracker.DefaultValue ) ) )
			  {
					// when
					long[] values = new long[length];
					bool[] marks = new bool[length];
					for ( int i = 0; i < length; i++ )
					{
						 tracker.Set( i, values[i] = Random.nextLong( MAX_ID ) );
						 if ( Random.nextBoolean() )
						 {
							  tracker.MarkAsDuplicate( i );
							  marks[i] = true;
						 }
					}

					// then
					for ( int i = 0; i < length; i++ )
					{
						 assertEquals( values[i], tracker.Get( i ) );
						 assertEquals( marks[i], tracker.IsMarkedAsDuplicate( i ) );
					}
			  }
		 }
	}

}