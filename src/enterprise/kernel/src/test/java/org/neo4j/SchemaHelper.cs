/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net
{

	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Label = Neo4Net.GraphDb.Label;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Transaction = Neo4Net.GraphDb.Transaction;


	public sealed class SchemaHelper
	{
		 private SchemaHelper()
		 {
			  throw new AssertionError( "Not for instantiation!" );
		 }

		 public static void CreateIndex( IGraphDatabaseService db, Label label, string property )
		 {
			  createIndex( db, label.Name(), property );
		 }

		 public static void CreateIndex( IGraphDatabaseService db, string label, string property )
		 {
			  Db.execute( format( "CREATE INDEX ON :`%s`(`%s`)", label, property ) );
		 }

		 public static void CreateUniquenessConstraint( IGraphDatabaseService db, Label label, string property )
		 {
			  createUniquenessConstraint( db, label.Name(), property );
		 }

		 public static void CreateUniquenessConstraint( IGraphDatabaseService db, string label, string property )
		 {
			  Db.execute( format( "CREATE CONSTRAINT ON (n:`%s`) ASSERT n.`%s` IS UNIQUE", label, property ) );
		 }

		 public static void CreateNodeKeyConstraint( IGraphDatabaseService db, Label label, params string[] properties )
		 {
			  createNodeKeyConstraint( db, label.Name(), properties );
		 }

		 public static void CreateNodeKeyConstraint( IGraphDatabaseService db, string label, params string[] properties )
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  string keyProperties = java.util.properties.Select( property => format( "n.`%s`", property ) ).collect( joining( "," ) );
			  Db.execute( format( "CREATE CONSTRAINT ON (n:`%s`) ASSERT (%s) IS NODE KEY", label, keyProperties ) );
		 }

		 public static void CreateNodePropertyExistenceConstraint( IGraphDatabaseService db, Label label, string property )
		 {
			  createNodePropertyExistenceConstraint( db, label.Name(), property );
		 }

		 public static void CreateNodePropertyExistenceConstraint( IGraphDatabaseService db, string label, string property )
		 {
			  Db.execute( format( "CREATE CONSTRAINT ON (n:`%s`) ASSERT exists(n.`%s`)", label, property ) );
		 }

		 public static void CreateRelPropertyExistenceConstraint( IGraphDatabaseService db, RelationshipType type, string property )
		 {
			  createRelPropertyExistenceConstraint( db, type.Name(), property );
		 }

		 public static void CreateRelPropertyExistenceConstraint( IGraphDatabaseService db, string type, string property )
		 {
			  Db.execute( format( "CREATE CONSTRAINT ON ()-[r:`%s`]-() ASSERT exists(r.`%s`)", type, property ) );
		 }

		 public static void AwaitIndexes( IGraphDatabaseService db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(10, TimeUnit.SECONDS);
					tx.Success();
			  }
		 }
	}

}