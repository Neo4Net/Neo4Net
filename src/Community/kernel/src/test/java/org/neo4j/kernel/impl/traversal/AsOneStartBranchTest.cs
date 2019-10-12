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
namespace Neo4Net.Kernel.impl.traversal
{
	using Test = org.junit.Test;

	using Node = Neo4Net.Graphdb.Node;
	using Neo4Net.Graphdb.traversal;
	using TraversalContext = Neo4Net.Graphdb.traversal.TraversalContext;
	using Uniqueness = Neo4Net.Graphdb.traversal.Uniqueness;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class AsOneStartBranchTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void donNotExhaustIteratorWhenUsingRelationshipPath()
		 public virtual void DonNotExhaustIteratorWhenUsingRelationshipPath()
		 {
			  // Given
			  IEnumerable<Node> nodeIterable = mock( typeof( System.Collections.IEnumerable ) );
			  IEnumerator<Node> nodeIterator = mock( typeof( System.Collections.IEnumerator ) );
			  when( nodeIterable.GetEnumerator() ).thenReturn(nodeIterator);
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  when( nodeIterator.hasNext() ).thenReturn(true);

			  // When
			  new AsOneStartBranch( mock( typeof( TraversalContext ) ), nodeIterable, mock( typeof( InitialBranchState ) ), Uniqueness.RELATIONSHIP_PATH );

			  // Then
			  verify( nodeIterator, never() ).next();
		 }

	}

}