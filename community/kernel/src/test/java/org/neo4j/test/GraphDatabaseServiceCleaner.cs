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
namespace Org.Neo4j.Test
{
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Node = Org.Neo4j.Graphdb.Node;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using ConstraintDefinition = Org.Neo4j.Graphdb.schema.ConstraintDefinition;
	using IndexDefinition = Org.Neo4j.Graphdb.schema.IndexDefinition;

	public class GraphDatabaseServiceCleaner
	{
		 private GraphDatabaseServiceCleaner()
		 {
			  throw new System.NotSupportedException();
		 }

		 public static void CleanDatabaseContent( GraphDatabaseService db )
		 {
			  CleanupSchema( db );
			  CleanupAllRelationshipsAndNodes( db );
		 }

		 public static void CleanupSchema( GraphDatabaseService db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					foreach ( ConstraintDefinition constraint in Db.schema().Constraints )
					{
						 constraint.Drop();
					}

					foreach ( IndexDefinition index in Db.schema().Indexes )
					{
						 index.Drop();
					}
					tx.Success();
			  }
		 }

		 public static void CleanupAllRelationshipsAndNodes( GraphDatabaseService db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					foreach ( Relationship relationship in Db.AllRelationships )
					{
						 relationship.Delete();
					}

					foreach ( Node node in Db.AllNodes )
					{
						 node.Delete();
					}
					tx.Success();
			  }
		 }
	}

}