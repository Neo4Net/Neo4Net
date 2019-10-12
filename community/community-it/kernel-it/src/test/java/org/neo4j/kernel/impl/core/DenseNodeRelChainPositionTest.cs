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
namespace Org.Neo4j.Kernel.impl.core
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Direction = Org.Neo4j.Graphdb.Direction;
	using Node = Org.Neo4j.Graphdb.Node;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using DatabaseRule = Org.Neo4j.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Org.Neo4j.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class DenseNodeRelChainPositionTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.DatabaseRule db = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public readonly DatabaseRule Db = new ImpermanentDatabaseRule();

		 /*
		  * Tests for a particular bug with dense nodes. It used to be that if a dense node had relationships
		  * for only one direction, if a request for relationships of the other direction was made, no relationships
		  * would be returned, ever.
		  */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void givenDenseNodeWhenAskForWrongDirectionThenIncorrectNrOfRelsReturned()
		 public virtual void GivenDenseNodeWhenAskForWrongDirectionThenIncorrectNrOfRelsReturned()
		 {
			  // Given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int denseNodeThreshold = int.Parse(org.neo4j.graphdb.factory.GraphDatabaseSettings.dense_node_threshold.getDefaultValue()) + 1;
			  int denseNodeThreshold = int.Parse( GraphDatabaseSettings.dense_node_threshold.DefaultValue ) + 1;

			  Node node1;
			  using ( Transaction tx = Db.beginTx() )
			  {
					node1 = Db.createNode();
					Node node2 = Db.createNode();

					for ( int i = 0; i < denseNodeThreshold; i++ )
					{
						 node1.CreateRelationshipTo( node2, RelationshipType.withName( "FOO" ) );
					}
					tx.Success();
			  }

			  // When/Then
			  using ( Transaction ignored = Db.beginTx() )
			  {
					Node node1b = Db.getNodeById( node1.Id );

					IEnumerable<Relationship> rels = node1b.GetRelationships( Direction.INCOMING );
					assertEquals( 0, Iterables.count( rels ) );

					IEnumerable<Relationship> rels2 = node1b.GetRelationships( Direction.OUTGOING );
					assertEquals( denseNodeThreshold, Iterables.count( rels2 ) );
			  }
		 }
	}

}