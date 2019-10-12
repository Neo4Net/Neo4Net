/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.causalclustering
{
	using CoreGraphDatabase = Org.Neo4j.causalclustering.core.CoreGraphDatabase;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Label = Org.Neo4j.Graphdb.Label;
	using Node = Org.Neo4j.Graphdb.Node;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using DbRepresentation = Org.Neo4j.Test.DbRepresentation;

	public class ClusterHelper
	{
		 public static readonly Label Label = Label.label( "any_label" );
		 public const string PROP_NAME = "name";
		 public const string PROP_RANDOM = "random";

		 /// <summary>
		 /// Used by non-cc </summary>
		 /// <param name="db">
		 /// @return </param>
		 public static DbRepresentation CreateSomeData( GraphDatabaseService db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					CreateSomeData( db, tx );
			  }
			  return DbRepresentation.of( db );
		 }

		 /// <summary>
		 /// Used by cc </summary>
		 /// <param name="db"> </param>
		 /// <param name="tx"> </param>
		 public static void CreateSomeData( GraphDatabaseService db, Transaction tx )
		 {
			  Node node = Db.createNode();
			  node.SetProperty( PROP_NAME, "Neo" );
			  node.SetProperty( PROP_RANDOM, GlobalRandom.NextDouble * 10000 );
			  Db.createNode().createRelationshipTo(node, RelationshipType.withName("KNOWS"));
			  tx.Success();
		 }

		 public static void CreateIndexes( CoreGraphDatabase db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().indexFor(Label).on(PROP_NAME).on(PROP_RANDOM).create();
					tx.Success();
			  }
		 }

	}

}