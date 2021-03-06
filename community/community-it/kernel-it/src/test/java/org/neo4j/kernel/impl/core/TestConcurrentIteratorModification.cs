﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.impl.core
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Label = Org.Neo4j.Graphdb.Label;
	using Node = Org.Neo4j.Graphdb.Node;
	using Org.Neo4j.Graphdb;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using EmbeddedDatabaseRule = Org.Neo4j.Test.rule.EmbeddedDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;

	public class TestConcurrentIteratorModification
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.EmbeddedDatabaseRule dbRule = new org.neo4j.test.rule.EmbeddedDatabaseRule();
		 public EmbeddedDatabaseRule DbRule = new EmbeddedDatabaseRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotThrowConcurrentModificationExceptionWhenUpdatingWhileIterating()
		 public virtual void ShouldNotThrowConcurrentModificationExceptionWhenUpdatingWhileIterating()
		 {
			  // given
			  GraphDatabaseService graph = DbRule.GraphDatabaseAPI;
			  Label label = Label.label( "Bird" );

			  Node node1;
			  Node node2;
			  Node node3;
			  using ( Transaction tx = graph.BeginTx() )
			  {
					node1 = graph.CreateNode( label );
					node2 = graph.CreateNode( label );
					tx.Success();
			  }

			  // when
			  ISet<Node> result = new HashSet<Node>();
			  using ( Transaction tx = graph.BeginTx() )
			  {
					node3 = graph.CreateNode( label );
					ResourceIterator<Node> iterator = graph.FindNodes( label );
					node3.RemoveLabel( label );
					graph.CreateNode( label );
					while ( iterator.MoveNext() )
					{
						 result.Add( iterator.Current );
					}
					tx.Success();
			  }

			  // then does not throw and retains view from iterator creation time
			  assertEquals( asSet( node1, node2, node3 ), result );
		 }
	}

}