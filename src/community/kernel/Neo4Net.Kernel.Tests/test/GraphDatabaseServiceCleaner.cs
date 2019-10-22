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
namespace Neo4Net.Test
{
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using ConstraintDefinition = Neo4Net.GraphDb.schema.ConstraintDefinition;
	using IndexDefinition = Neo4Net.GraphDb.schema.IndexDefinition;

	public class IGraphDatabaseServiceCleaner
	{
		 private IGraphDatabaseServiceCleaner()
		 {
			  throw new System.NotSupportedException();
		 }

		 public static void CleanDatabaseContent( IGraphDatabaseService db )
		 {
			  CleanupSchema( db );
			  CleanupAllRelationshipsAndNodes( db );
		 }

		 public static void CleanupSchema( IGraphDatabaseService db )
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

		 public static void CleanupAllRelationshipsAndNodes( IGraphDatabaseService db )
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