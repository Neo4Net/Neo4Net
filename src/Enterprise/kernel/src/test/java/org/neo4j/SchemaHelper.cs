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
namespace Neo4Net
{

	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Label = Neo4Net.Graphdb.Label;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Transaction = Neo4Net.Graphdb.Transaction;


	public sealed class SchemaHelper
	{
		 private SchemaHelper()
		 {
			  throw new AssertionError( "Not for instantiation!" );
		 }

		 public static void CreateIndex( GraphDatabaseService db, Label label, string property )
		 {
			  createIndex( db, label.Name(), property );
		 }

		 public static void CreateIndex( GraphDatabaseService db, string label, string property )
		 {
			  Db.execute( format( "CREATE INDEX ON :`%s`(`%s`)", label, property ) );
		 }

		 public static void CreateUniquenessConstraint( GraphDatabaseService db, Label label, string property )
		 {
			  createUniquenessConstraint( db, label.Name(), property );
		 }

		 public static void CreateUniquenessConstraint( GraphDatabaseService db, string label, string property )
		 {
			  Db.execute( format( "CREATE CONSTRAINT ON (n:`%s`) ASSERT n.`%s` IS UNIQUE", label, property ) );
		 }

		 public static void CreateNodeKeyConstraint( GraphDatabaseService db, Label label, params string[] properties )
		 {
			  createNodeKeyConstraint( db, label.Name(), properties );
		 }

		 public static void CreateNodeKeyConstraint( GraphDatabaseService db, string label, params string[] properties )
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  string keyProperties = java.util.properties.Select( property => format( "n.`%s`", property ) ).collect( joining( "," ) );
			  Db.execute( format( "CREATE CONSTRAINT ON (n:`%s`) ASSERT (%s) IS NODE KEY", label, keyProperties ) );
		 }

		 public static void CreateNodePropertyExistenceConstraint( GraphDatabaseService db, Label label, string property )
		 {
			  createNodePropertyExistenceConstraint( db, label.Name(), property );
		 }

		 public static void CreateNodePropertyExistenceConstraint( GraphDatabaseService db, string label, string property )
		 {
			  Db.execute( format( "CREATE CONSTRAINT ON (n:`%s`) ASSERT exists(n.`%s`)", label, property ) );
		 }

		 public static void CreateRelPropertyExistenceConstraint( GraphDatabaseService db, RelationshipType type, string property )
		 {
			  createRelPropertyExistenceConstraint( db, type.Name(), property );
		 }

		 public static void CreateRelPropertyExistenceConstraint( GraphDatabaseService db, string type, string property )
		 {
			  Db.execute( format( "CREATE CONSTRAINT ON ()-[r:`%s`]-() ASSERT exists(r.`%s`)", type, property ) );
		 }

		 public static void AwaitIndexes( GraphDatabaseService db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(10, TimeUnit.SECONDS);
					tx.Success();
			  }
		 }
	}

}