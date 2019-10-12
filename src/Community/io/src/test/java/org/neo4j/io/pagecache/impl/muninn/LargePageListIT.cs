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
namespace Neo4Net.Io.pagecache.impl.muninn
{
	using Test = org.junit.jupiter.api.Test;

	using MemoryAllocator = Neo4Net.Io.mem.MemoryAllocator;
	using GlobalMemoryTracker = Neo4Net.Memory.GlobalMemoryTracker;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;

	internal class LargePageListIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void veryLargePageListsMustBeFullyAccessible()
		 internal virtual void VeryLargePageListsMustBeFullyAccessible()
		 {
			  // We need roughly 2 GiBs of memory for the meta-data here, which is why this is an IT and not a Test.
			  // We add one extra page worth of data to the size here, to avoid ending up on a "convenient" boundary.
			  int pageSize = ( int ) ByteUnit.kibiBytes( 8 );
			  long pageCacheSize = ByteUnit.gibiBytes( 513 ) + pageSize;
			  int pages = Math.toIntExact( pageCacheSize / pageSize );

			  MemoryAllocator mman = MemoryAllocator.createAllocator( "2 GiB", GlobalMemoryTracker.INSTANCE );
			  SwapperSet swappers = new SwapperSet();
			  long victimPage = VictimPageReference.GetVictimPage( pageSize, GlobalMemoryTracker.INSTANCE );

			  PageList pageList = new PageList( pages, pageSize, mman, swappers, victimPage, Long.BYTES );

			  // Verify we end up with the correct number of pages.
			  assertThat( pageList.PageCount, @is( pages ) );

			  // Spot-check the accessibility in the bulk of the pages.
			  IntStream.range( 0, pages / 32 ).parallel().forEach(id => verifyPageMetaDataIsAccessible(pageList, id * 32));

			  // Thoroughly check the accessibility around the tail end of the page list.
			  IntStream.range( pages - 2000, pages ).parallel().forEach(id => verifyPageMetaDataIsAccessible(pageList, id));
		 }

		 private static void VerifyPageMetaDataIsAccessible( PageList pageList, int id )
		 {
			  long @ref = pageList.Deref( id );
			  pageList.IncrementUsage( @ref );
			  pageList.IncrementUsage( @ref );
			  assertFalse( pageList.DecrementUsage( @ref ) );
			  assertTrue( pageList.DecrementUsage( @ref ) );
			  assertEquals( id, pageList.ToId( @ref ) );
		 }
	}

}