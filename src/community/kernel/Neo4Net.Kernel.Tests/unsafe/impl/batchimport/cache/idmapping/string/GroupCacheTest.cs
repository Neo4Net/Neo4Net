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
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.cache.NumberArrayFactory.HEAP;

	public class GroupCacheTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleSingleByteCount()
		 public virtual void ShouldHandleSingleByteCount()
		 {
			  // given
			  int max = 256;
			  GroupCache cache = GroupCache.select( HEAP, 100, max );

			  // when
			  AssertSetAndGet( cache, 10, 45 );
			  AssertSetAndGet( cache, 100, 145 );
			  AssertSetAndGet( cache, 1000, 245 );

			  // then
			  try
			  {
					cache.Set( 10000, 345 );
					fail( "Shouldn't handle that" );
			  }
			  catch ( ArithmeticException )
			  {
					// OK
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSwitchToTwoByteVersionBeyondSingleByteGroupIds()
		 public virtual void ShouldSwitchToTwoByteVersionBeyondSingleByteGroupIds()
		 {
			  // given
			  int max = 257;
			  GroupCache cache = GroupCache.select( HEAP, 100, max );

			  // when
			  AssertSetAndGet( cache, 10, 123 );
			  AssertSetAndGet( cache, 100, 1234 );
			  AssertSetAndGet( cache, 1000, 12345 );
			  AssertSetAndGet( cache, 10000, 0xFFFF );

			  // then
			  try
			  {
					cache.Set( 100000, 123456 );
					fail( "Shouldn't handle that" );
			  }
			  catch ( ArithmeticException )
			  {
					// OK
			  }
		 }

		 private void AssertSetAndGet( GroupCache cache, long nodeId, int groupId )
		 {
			  cache.Set( nodeId, groupId );
		 }
	}

}