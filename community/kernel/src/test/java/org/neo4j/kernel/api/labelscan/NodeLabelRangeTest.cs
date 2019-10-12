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
namespace Org.Neo4j.Kernel.api.labelscan
{
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;

	public class NodeLabelRangeTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTransposeNodeIdsAndLabelIds()
		 public virtual void ShouldTransposeNodeIdsAndLabelIds()
		 {
			  // given
			  long[][] labelsPerNode = new long[][]
			  {
				  new long[] { 1 },
				  new long[] { 1, 3 },
				  new long[] { 3, 5, 7 },
				  new long[] {},
				  new long[] { 1, 5, 7 },
				  new long[] {},
				  new long[] {},
				  new long[] { 1, 2, 3, 4 }
			  };

			  // when
			  NodeLabelRange range = new NodeLabelRange( 0, labelsPerNode );

			  // then
			  assertArrayEquals( new long[] { 0, 1, 2, 3, 4, 5, 6, 7 }, range.Nodes() );
			  for ( int i = 0; i < labelsPerNode.Length; i++ )
			  {
					assertArrayEquals( labelsPerNode[i], range.Labels( i ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRebaseOnRangeId()
		 public virtual void ShouldRebaseOnRangeId()
		 {
			  // given
			  long[][] labelsPerNode = new long[][]
			  {
				  new long[] { 1 },
				  new long[] { 1, 3 },
				  new long[] { 3, 5, 7 },
				  new long[] {},
				  new long[] { 1, 5, 7 },
				  new long[] {},
				  new long[] {},
				  new long[] { 1, 2, 3, 4 }
			  };

			  // when
			  NodeLabelRange range = new NodeLabelRange( 10, labelsPerNode );

			  // then
			  long baseNodeId = range.Id() * labelsPerNode.Length;
			  long[] expectedNodeIds = new long[labelsPerNode.Length];
			  for ( int i = 0; i < expectedNodeIds.Length; i++ )
			  {
					expectedNodeIds[i] = baseNodeId + i;
			  }
			  assertArrayEquals( expectedNodeIds, range.Nodes() );
		 }
	}

}