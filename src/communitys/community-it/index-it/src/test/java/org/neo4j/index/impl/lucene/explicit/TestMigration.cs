using System;
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
namespace Neo4Net.Index.impl.lucene.@explicit
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using PropertyContainer = Neo4Net.Graphdb.PropertyContainer;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using IndexManager = Neo4Net.Graphdb.index.IndexManager;
	using MapUtil = Neo4Net.Helpers.Collections.MapUtil;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using IndexConfigStore = Neo4Net.Kernel.impl.index.IndexConfigStore;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class TestMigration
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.DefaultFileSystemRule fileSystemRule = new org.neo4j.test.rule.fs.DefaultFileSystemRule();
		 public readonly DefaultFileSystemRule FileSystemRule = new DefaultFileSystemRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void providerGetsFilledInAutomatically()
		 public virtual void ProviderGetsFilledInAutomatically()
		 {
			  IDictionary<string, string> correctConfig = MapUtil.stringMap( "type", "exact", Neo4Net.Graphdb.index.IndexManager_Fields.PROVIDER, "lucene" );
			  File storeDir = TestDirectory.storeDir();
			  Neo4jTestCase.deleteFileOrDirectory( storeDir );
			  GraphDatabaseService graphDb = StartDatabase( storeDir );
			  using ( Transaction transaction = graphDb.BeginTx() )
			  {
					assertEquals( correctConfig, graphDb.Index().getConfiguration(graphDb.Index().forNodes("default")) );
					assertEquals( correctConfig, graphDb.Index().getConfiguration(graphDb.Index().forNodes("wo-provider", MapUtil.stringMap("type", "exact"))) );
					assertEquals( correctConfig, graphDb.Index().getConfiguration(graphDb.Index().forNodes("w-provider", MapUtil.stringMap("type", "exact", Neo4Net.Graphdb.index.IndexManager_Fields.PROVIDER, "lucene"))) );
					assertEquals( correctConfig, graphDb.Index().getConfiguration(graphDb.Index().forRelationships("default")) );
					assertEquals( correctConfig, graphDb.Index().getConfiguration(graphDb.Index().forRelationships("wo-provider", MapUtil.stringMap("type", "exact"))) );
					assertEquals( correctConfig, graphDb.Index().getConfiguration(graphDb.Index().forRelationships("w-provider", MapUtil.stringMap("type", "exact", Neo4Net.Graphdb.index.IndexManager_Fields.PROVIDER, "lucene"))) );
					transaction.Success();
			  }

			  graphDb.Shutdown();

			  RemoveProvidersFromIndexDbFile( TestDirectory.databaseLayout() );
			  graphDb = StartDatabase( storeDir );

			  using ( Transaction ignored = graphDb.BeginTx() )
			  {
					// Getting the index w/o exception means that the provider has been reinstated
					assertEquals( correctConfig, graphDb.Index().getConfiguration(graphDb.Index().forNodes("default")) );
					assertEquals( correctConfig, graphDb.Index().getConfiguration(graphDb.Index().forNodes("wo-provider", MapUtil.stringMap("type", "exact"))) );
					assertEquals( correctConfig, graphDb.Index().getConfiguration(graphDb.Index().forNodes("w-provider", MapUtil.stringMap("type", "exact", Neo4Net.Graphdb.index.IndexManager_Fields.PROVIDER, "lucene"))) );
					assertEquals( correctConfig, graphDb.Index().getConfiguration(graphDb.Index().forRelationships("default")) );
					assertEquals( correctConfig, graphDb.Index().getConfiguration(graphDb.Index().forRelationships("wo-provider", MapUtil.stringMap("type", "exact"))) );
					assertEquals( correctConfig, graphDb.Index().getConfiguration(graphDb.Index().forRelationships("w-provider", MapUtil.stringMap("type", "exact", Neo4Net.Graphdb.index.IndexManager_Fields.PROVIDER, "lucene"))) );
			  }

			  graphDb.Shutdown();

			  RemoveProvidersFromIndexDbFile( TestDirectory.databaseLayout() );
			  graphDb = StartDatabase( storeDir );

			  using ( Transaction ignored = graphDb.BeginTx() )
			  {
					// Getting the index w/o exception means that the provider has been reinstated
					assertEquals( correctConfig, graphDb.Index().getConfiguration(graphDb.Index().forNodes("default")) );
					assertEquals( correctConfig, graphDb.Index().getConfiguration(graphDb.Index().forNodes("wo-provider")) );
					assertEquals( correctConfig, graphDb.Index().getConfiguration(graphDb.Index().forNodes("w-provider")) );
					assertEquals( correctConfig, graphDb.Index().getConfiguration(graphDb.Index().forRelationships("default")) );
					assertEquals( correctConfig, graphDb.Index().getConfiguration(graphDb.Index().forRelationships("wo-provider")) );
					assertEquals( correctConfig, graphDb.Index().getConfiguration(graphDb.Index().forRelationships("w-provider")) );
			  }

			  graphDb.Shutdown();
		 }

		 private static GraphDatabaseService StartDatabase( File storeDir )
		 {
			  return ( new TestGraphDatabaseFactory() ).newEmbeddedDatabase(storeDir);
		 }

		 private void RemoveProvidersFromIndexDbFile( DatabaseLayout databaseLayout )
		 {
			  IndexConfigStore indexStore = new IndexConfigStore( databaseLayout, FileSystemRule.get() );
			  foreach ( Type cls in new Type[] { typeof( Node ), typeof( Relationship ) } )
			  {
					foreach ( string name in indexStore.GetNames( cls ) )
					{
						 IDictionary<string, string> config = indexStore.Get( cls, name );
						 config = new Dictionary<string, string>( config );
						 config.Remove( Neo4Net.Graphdb.index.IndexManager_Fields.PROVIDER );
						 indexStore.Set( typeof( Node ), name, config );
					}
			  }
		 }
	}

}