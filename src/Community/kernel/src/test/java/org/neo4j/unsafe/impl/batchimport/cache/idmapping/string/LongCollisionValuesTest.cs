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
namespace Neo4Net.@unsafe.Impl.Batchimport.cache.idmapping.@string
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;
	using Parameter = org.junit.runners.Parameterized.Parameter;
	using Parameters = org.junit.runners.Parameterized.Parameters;


	using RandomRule = Neo4Net.Test.rule.RandomRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.cache.NumberArrayFactory_Fields.AUTO_WITHOUT_PAGECACHE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.cache.NumberArrayFactory_Fields.CHUNKED_FIXED_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.cache.NumberArrayFactory.HEAP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.cache.NumberArrayFactory.OFF_HEAP;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class LongCollisionValuesTest
	public class LongCollisionValuesTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.RandomRule random = new org.neo4j.test.rule.RandomRule();
		 public readonly RandomRule Random = new RandomRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameters public static java.util.Collection<org.neo4j.unsafe.impl.batchimport.cache.NumberArrayFactory> data()
		 public static ICollection<NumberArrayFactory> Data()
		 {
			  return Arrays.asList( HEAP, OFF_HEAP, AUTO_WITHOUT_PAGECACHE, CHUNKED_FIXED_SIZE );
		 }

		 [Parameter(0)]
		 public NumberArrayFactory Factory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStoreAndLoadLongs()
		 public virtual void ShouldStoreAndLoadLongs()
		 {
			  // given
			  using ( LongCollisionValues values = new LongCollisionValues( Factory, 100 ) )
			  {
					// when
					long[] offsets = new long[100];
					long[] longs = new long[offsets.Length];
					for ( int i = 0; i < offsets.Length; i++ )
					{
						 long value = Random.nextLong( long.MaxValue );
						 offsets[i] = values.Add( value );
						 longs[i] = value;
					}

					// then
					for ( int i = 0; i < offsets.Length; i++ )
					{
						 assertEquals( longs[i], ( long ) values.Get( offsets[i] ) );
					}
			  }
		 }
	}

}