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
namespace Org.Neo4j.Graphdb
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;

	using ImpermanentDatabaseRule = Org.Neo4j.Test.rule.ImpermanentDatabaseRule;

	public class DeleteNodeWithRelsIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.ImpermanentDatabaseRule db = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public ImpermanentDatabaseRule Db = new ImpermanentDatabaseRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException exception = org.junit.rules.ExpectedException.none();
		 public ExpectedException Exception = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveHelpfulExceptionWhenDeletingNodeWithRels()
		 public virtual void ShouldGiveHelpfulExceptionWhenDeletingNodeWithRels()
		 {
			  // Given
			  GraphDatabaseService db = this.Db.GraphDatabaseAPI;

			  Node node;
			  using ( Transaction tx = Db.beginTx() )
			  {
					node = Db.createNode();
					node.CreateRelationshipTo( Db.createNode(), RelationshipType.withName("MAYOR_OF") );
					tx.Success();
			  }

			  // And given a transaction deleting just the node
			  Transaction tx = Db.beginTx();
			  node.Delete();
			  tx.Success();

			  // Expect
			  Exception.expect( typeof( ConstraintViolationException ) );
			  Exception.expectMessage( "Cannot delete node<" + node.Id + ">, because it still has relationships. " + "To delete this node, you must first delete its relationships." );

			  // When I commit
			  tx.Close();
		 }

	}

}