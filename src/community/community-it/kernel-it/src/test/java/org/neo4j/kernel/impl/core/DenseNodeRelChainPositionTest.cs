using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.core
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Direction = Neo4Net.GraphDb.Direction;
	using Node = Neo4Net.GraphDb.Node;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class DenseNodeRelChainPositionTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.DatabaseRule db = new org.Neo4Net.test.rule.ImpermanentDatabaseRule();
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
//ORIGINAL LINE: final int denseNodeThreshold = int.Parse(org.Neo4Net.graphdb.factory.GraphDatabaseSettings.dense_node_threshold.getDefaultValue()) + 1;
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