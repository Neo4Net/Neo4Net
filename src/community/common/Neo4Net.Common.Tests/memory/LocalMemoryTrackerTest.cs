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
namespace Neo4Net.Memory
{
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;

	internal class LocalMemoryTrackerTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void trackMemoryAllocations()
		 internal virtual void TrackMemoryAllocations()
		 {
			  LocalMemoryTracker memoryTracker = new LocalMemoryTracker();
			  memoryTracker.Allocated( 10 );
			  memoryTracker.Allocated( 20 );
			  memoryTracker.Allocated( 40 );
			  assertEquals( 70, memoryTracker.UsedDirectMemory() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void trackMemoryDeallocations()
		 internal virtual void TrackMemoryDeallocations()
		 {
			  LocalMemoryTracker memoryTracker = new LocalMemoryTracker();
			  memoryTracker.Allocated( 100 );
			  assertEquals( 100, memoryTracker.UsedDirectMemory() );

			  memoryTracker.Deallocated( 20 );
			  assertEquals( 80, memoryTracker.UsedDirectMemory() );

			  memoryTracker.Deallocated( 40 );
			  assertEquals( 40, memoryTracker.UsedDirectMemory() );
		 }
	}

}