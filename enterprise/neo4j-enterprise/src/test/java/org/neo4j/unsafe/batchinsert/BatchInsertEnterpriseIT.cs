﻿using System.Collections.Generic;

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
namespace Org.Neo4j.@unsafe.Batchinsert
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;
	using Parameter = org.junit.runners.Parameterized.Parameter;
	using Parameters = org.junit.runners.Parameterized.Parameters;


	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Label = Org.Neo4j.Graphdb.Label;
	using Node = Org.Neo4j.Graphdb.Node;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using EnterpriseGraphDatabaseFactory = Org.Neo4j.Graphdb.factory.EnterpriseGraphDatabaseFactory;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;
	using MyRelTypes = Org.Neo4j.Kernel.impl.MyRelTypes;
	using OnlineBackupSettings = Org.Neo4j.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using HighLimit = Org.Neo4j.Kernel.impl.store.format.highlimit.HighLimit;
	using Standard = Org.Neo4j.Kernel.impl.store.format.standard.Standard;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.single;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;

	/// <summary>
	/// Just testing the <seealso cref="BatchInserter"/> in an enterprise setting, i.e. with all packages and extensions
	/// that exist in enterprise edition.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class BatchInsertEnterpriseIT
	public class BatchInsertEnterpriseIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory directory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory Directory = TestDirectory.testDirectory();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.DefaultFileSystemRule fileSystemRule = new org.neo4j.test.rule.fs.DefaultFileSystemRule();
		 public readonly DefaultFileSystemRule FileSystemRule = new DefaultFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameter public String recordFormat;
		 public string RecordFormat;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameters(name = "{0}") public static java.util.List<String> recordFormats()
		 public static IList<string> RecordFormats()
		 {
			  return Arrays.asList( Standard.LATEST_NAME, HighLimit.NAME );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInsertDifferentTypesOfThings() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInsertDifferentTypesOfThings()
		 {
			  // GIVEN
			  BatchInserter inserter = BatchInserters.Inserter( Directory.databaseDir(), FileSystemRule.get(), stringMap(GraphDatabaseSettings.log_queries.name(), "true", GraphDatabaseSettings.record_format.name(), RecordFormat, GraphDatabaseSettings.log_queries_filename.name(), Directory.file("query.log").AbsolutePath) );
			  long node1Id;
			  long node2Id;
			  long relationshipId;
			  try
			  {
					// WHEN
					node1Id = inserter.createNode( SomeProperties( 1 ), Enum.GetValues( typeof( Labels ) ) );
					node2Id = node1Id + 10;
					inserter.createNode( node2Id, SomeProperties( 2 ), Enum.GetValues( typeof( Labels ) ) );
					relationshipId = inserter.CreateRelationship( node1Id, node2Id, MyRelTypes.TEST, SomeProperties( 3 ) );
					inserter.CreateDeferredSchemaIndex( Labels.One ).on( "key" ).create();
					inserter.CreateDeferredConstraint( Labels.Two ).assertPropertyIsUnique( "key" ).create();
			  }
			  finally
			  {
					inserter.Shutdown();
			  }

			  // THEN
			  GraphDatabaseService db = ( new EnterpriseGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(Directory.databaseDir()).setConfig(OnlineBackupSettings.online_backup_enabled, Settings.FALSE).newGraphDatabase();

			  try
			  {
					  using ( Transaction tx = Db.beginTx() )
					  {
						Node node1 = Db.getNodeById( node1Id );
						Node node2 = Db.getNodeById( node2Id );
						assertEquals( SomeProperties( 1 ), node1.AllProperties );
						assertEquals( SomeProperties( 2 ), node2.AllProperties );
						assertEquals( relationshipId, single( node1.Relationships ).Id );
						assertEquals( relationshipId, single( node2.Relationships ).Id );
						assertEquals( SomeProperties( 3 ), single( node1.Relationships ).AllProperties );
						tx.Success();
					  }
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void insertIntoExistingDatabase() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void InsertIntoExistingDatabase()
		 {
			  File storeDir = Directory.databaseDir();

			  GraphDatabaseService db = NewDb( storeDir, RecordFormat );
			  try
			  {
					CreateThreeNodes( db );
			  }
			  finally
			  {
					Db.shutdown();
			  }

			  BatchInserter inserter = BatchInserters.Inserter( Directory.databaseDir(), FileSystemRule.get() );
			  try
			  {
					long start = inserter.createNode( SomeProperties( 5 ), Labels.One );
					long end = inserter.createNode( SomeProperties( 5 ), Labels.One );
					inserter.CreateRelationship( start, end, MyRelTypes.TEST, SomeProperties( 5 ) );
			  }
			  finally
			  {
					inserter.Shutdown();
			  }

			  db = NewDb( storeDir, RecordFormat );
			  try
			  {
					VerifyNodeCount( db, 4 );
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

		 private static void VerifyNodeCount( GraphDatabaseService db, int expectedNodeCount )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					assertEquals( expectedNodeCount, Iterables.count( Db.AllNodes ) );
					tx.Success();
			  }
		 }

		 private static void CreateThreeNodes( GraphDatabaseService db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node start = Db.createNode( Labels.One );
					SomeProperties( 5 ).forEach( start.setProperty );

					Node end = Db.createNode( Labels.Two );
					SomeProperties( 5 ).forEach( end.setProperty );

					Relationship rel = start.CreateRelationshipTo( end, MyRelTypes.TEST );
					SomeProperties( 5 ).forEach( rel.setProperty );

					tx.Success();
			  }
		 }

		 private static IDictionary<string, object> SomeProperties( int id )
		 {
			  return map( "key", "value" + id, "number", 10 + id );
		 }

		 private GraphDatabaseService NewDb( File storeDir, string recordFormat )
		 {
			  return ( new EnterpriseGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(storeDir).setConfig(GraphDatabaseSettings.record_format, recordFormat).setConfig(OnlineBackupSettings.online_backup_enabled, Settings.FALSE).newGraphDatabase();
		 }

		 private enum Labels
		 {
			  One,
			  Two
		 }
	}

}