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
namespace Neo4Net.@unsafe.Impl.Batchimport.cache
{
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class DynamicLongArrayTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWorkOnSingleChunk()
		 public virtual void ShouldWorkOnSingleChunk()
		 {
			  // GIVEN
			  long defaultValue = 0;
			  LongArray array = NumberArrayFactory_Fields.AutoWithoutPagecache.newDynamicLongArray( 10, defaultValue );
			  array.Set( 4, 5 );

			  // WHEN
			  assertEquals( 5L, array.Get( 4 ) );
			  assertEquals( defaultValue, array.Get( 12 ) );
			  array.Set( 7, 1324 );
			  assertEquals( 1324L, array.Get( 7 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldChunksAsNeeded()
		 public virtual void ShouldChunksAsNeeded()
		 {
			  // GIVEN
			  LongArray array = NumberArrayFactory_Fields.AutoWithoutPagecache.newDynamicLongArray( 10, 0 );

			  // WHEN
			  long index = 243;
			  long value = 5485748;
			  array.Set( index, value );

			  // THEN
			  assertEquals( value, array.Get( index ) );
		 }
	}

}