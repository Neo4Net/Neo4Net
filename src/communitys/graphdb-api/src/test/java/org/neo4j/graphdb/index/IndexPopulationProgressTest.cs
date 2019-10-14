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
namespace Neo4Net.Graphdb.index
{
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;

	internal class IndexPopulationProgressTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testNone()
		 internal virtual void TestNone()
		 {
			  assertEquals( 0, IndexPopulationProgress.None.CompletedPercentage, 0.01 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testDone()
		 internal virtual void TestDone()
		 {
			  assertEquals( 100, IndexPopulationProgress.Done.CompletedPercentage, 0.01 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testNegativeCompleted()
		 internal virtual void TestNegativeCompleted()
		 {
			  assertThrows( typeof( System.ArgumentException ), () => new IndexPopulationProgress(-1, 1) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testNegativeTotal()
		 internal virtual void TestNegativeTotal()
		 {
			  assertThrows( typeof( System.ArgumentException ), () => new IndexPopulationProgress(0, -1) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testAllZero()
		 internal virtual void TestAllZero()
		 {
			  IndexPopulationProgress progress = new IndexPopulationProgress( 0, 0 );
			  assertEquals( 0, progress.CompletedCount );
			  assertEquals( 0, progress.TotalCount );
			  assertEquals( 0, progress.CompletedPercentage, 0.01 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testCompletedZero()
		 internal virtual void TestCompletedZero()
		 {
			  IndexPopulationProgress progress = new IndexPopulationProgress( 0, 1 );
			  assertEquals( 1, progress.TotalCount );
			  assertEquals( 0, progress.CompletedCount );
			  assertEquals( 0, progress.CompletedPercentage, 0.01 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testCompletedGreaterThanTotal()
		 internal virtual void TestCompletedGreaterThanTotal()
		 {
			  assertThrows( typeof( System.ArgumentException ), () => new IndexPopulationProgress(2, 1) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testGetCompletedPercentage()
		 internal virtual void TestGetCompletedPercentage()
		 {
			  IndexPopulationProgress progress = new IndexPopulationProgress( 1, 2 );
			  assertEquals( 50.0f, progress.CompletedPercentage, 0.01f );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testGetCompleted()
		 internal virtual void TestGetCompleted()
		 {
			  IndexPopulationProgress progress = new IndexPopulationProgress( 1, 2 );
			  assertEquals( 1L, progress.CompletedCount );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testGetTotal()
		 internal virtual void TestGetTotal()
		 {
			  IndexPopulationProgress progress = new IndexPopulationProgress( 1, 2 );
			  assertEquals( 2L, progress.TotalCount );
		 }
	}

}