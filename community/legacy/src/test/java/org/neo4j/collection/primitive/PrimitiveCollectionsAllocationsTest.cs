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
namespace Org.Neo4j.Collection.primitive
{
	using Test = org.junit.jupiter.api.Test;

	using LocalMemoryTracker = Org.Neo4j.Memory.LocalMemoryTracker;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;

	internal class PrimitiveCollectionsAllocationsTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void trackPrimitiveMemoryAllocations()
		 internal virtual void TrackPrimitiveMemoryAllocations()
		 {
			  LocalMemoryTracker memoryTracker = new LocalMemoryTracker();
			  PrimitiveIntSet offHeapIntSet = Primitive.OffHeapIntSet( memoryTracker );
			  assertTrue( memoryTracker.UsedDirectMemory() > 0 );

			  offHeapIntSet.Close();
			  assertEquals( 0, memoryTracker.UsedDirectMemory() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void trackPrimitiveMemoryOnResize()
		 internal virtual void TrackPrimitiveMemoryOnResize()
		 {
			  LocalMemoryTracker memoryTracker = new LocalMemoryTracker();
			  PrimitiveIntSet offHeapIntSet = Primitive.OffHeapIntSet( memoryTracker );
			  long originalSetMemory = memoryTracker.UsedDirectMemory();

			  for ( int i = 0; i < Primitive.DefaultOffheapCapacity + 1; i++ )
			  {
					offHeapIntSet.Add( i );
			  }

			  assertTrue( memoryTracker.UsedDirectMemory() > originalSetMemory );

			  offHeapIntSet.Close();
			  assertEquals( 0, memoryTracker.UsedDirectMemory() );
		 }
	}

}