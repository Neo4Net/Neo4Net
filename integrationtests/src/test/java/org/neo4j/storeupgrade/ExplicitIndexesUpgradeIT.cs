using System;
using System.Collections.Generic;

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
namespace Org.Neo4j.storeupgrade
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;


	using OnlineBackupSettings = Org.Neo4j.backup.OnlineBackupSettings;
	using Org.Neo4j.Function;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Node = Org.Neo4j.Graphdb.Node;
	using PropertyContainer = Org.Neo4j.Graphdb.PropertyContainer;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using GraphDatabaseBuilder = Org.Neo4j.Graphdb.factory.GraphDatabaseBuilder;
	using GraphDatabaseFactory = Org.Neo4j.Graphdb.factory.GraphDatabaseFactory;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Org.Neo4j.Graphdb.index;
	using RelationshipIndex = Org.Neo4j.Graphdb.index.RelationshipIndex;
	using ValueContext = Org.Neo4j.Index.lucene.ValueContext;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;
	using UpgradeNotAllowedByConfigurationException = Org.Neo4j.Kernel.impl.storemigration.UpgradeNotAllowedByConfigurationException;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using Unzip = Org.Neo4j.Test.Unzip;
	using NestedThrowableMatcher = Org.Neo4j.Test.matchers.NestedThrowableMatcher;
	using SuppressOutput = Org.Neo4j.Test.rule.SuppressOutput;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.single;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.impl.lucene.@explicit.LuceneIndexImplementation.EXACT_CONFIG;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.impl.lucene.@explicit.LuceneIndexImplementation.FULLTEXT_CONFIG;

	public class ExplicitIndexesUpgradeIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDir = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDir = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.ExpectedException expectedException = org.junit.rules.ExpectedException.none();
		 public readonly ExpectedException ExpectedException = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.SuppressOutput suppressOutput = org.neo4j.test.rule.SuppressOutput.suppressAll();
		 public readonly SuppressOutput SuppressOutput = SuppressOutput.suppressAll();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void successfulMigrationWithoutExplicitIndexes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SuccessfulMigrationWithoutExplicitIndexes()
		 {
			  PrepareStore( "empty-explicit-index-db.zip" );
			  GraphDatabaseService db = StartDatabase( true );
			  try
			  {
					CheckDbAccessible( db );
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void successfulMigrationExplicitIndexes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SuccessfulMigrationExplicitIndexes()
		 {
			  PrepareStore( "explicit-index-db.zip" );

			  GraphDatabaseService db = StartDatabase( true );
			  try
			  {
					CheckDbAccessible( db );
					CheckIndexData( db );
			  }
			  finally
			  {
					Db.shutdown();
			  }

			  CheckMigrationProgressFeedback();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void migrationShouldFailIfUpgradeNotAllowed() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MigrationShouldFailIfUpgradeNotAllowed()
		 {
			  PrepareStore( "explicit-index-db.zip" );
			  ExpectedException.expect( new NestedThrowableMatcher( typeof( UpgradeNotAllowedByConfigurationException ) ) );

			  StartDatabase( false );
		 }

		 private static void CheckDbAccessible( GraphDatabaseService db )
		 {
			  using ( Transaction transaction = Db.beginTx() )
			  {
					assertNotNull( Db.getNodeById( 1 ) );
					transaction.Success();
			  }
		 }

		 private GraphDatabaseService StartDatabase( bool allowUpgrade )
		 {
			  GraphDatabaseFactory factory = new TestGraphDatabaseFactory();
			  GraphDatabaseBuilder builder = factory.NewEmbeddedDatabaseBuilder( TestDir.databaseDir() );
			  builder.SetConfig( GraphDatabaseSettings.allow_upgrade, Convert.ToString( allowUpgrade ) );
			  builder.SetConfig( GraphDatabaseSettings.pagecache_memory, "8m" );
			  builder.SetConfig( OnlineBackupSettings.online_backup_enabled, Settings.FALSE );
			  return builder.NewGraphDatabase();
		 }

		 private static void CheckIndexData( GraphDatabaseService db )
		 {
			  System.Func<int, string> keyFactory = BasicKeyFactory();
			  Factory<Node> readNodes = readNodes( db );
			  ReadIndex( db, NodeIndex( db, "node-1", EXACT_CONFIG ), readNodes, keyFactory, StringValues() );
			  ReadIndex( db, NodeIndex( db, "node-2", EXACT_CONFIG ), readNodes, keyFactory, IntValues() );
			  ReadIndex( db, NodeIndex( db, "node-3", FULLTEXT_CONFIG ), readNodes, keyFactory, StringValues() );
			  ReadIndex( db, NodeIndex( db, "node-4", FULLTEXT_CONFIG ), readNodes, keyFactory, LongValues() );
			  Factory<Relationship> relationships = ReadRelationships( db );
			  ReadIndex( db, RelationshipIndex( db, "rel-1", EXACT_CONFIG ), relationships, keyFactory, StringValues() );
			  ReadIndex( db, RelationshipIndex( db, "rel-2", EXACT_CONFIG ), relationships, keyFactory, FloatValues() );
			  ReadIndex( db, RelationshipIndex( db, "rel-3", FULLTEXT_CONFIG ), relationships, keyFactory, StringValues() );
			  ReadIndex( db, RelationshipIndex( db, "rel-4", FULLTEXT_CONFIG ), relationships, keyFactory, DoubleValues() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void prepareStore(String store) throws java.io.IOException
		 private void PrepareStore( string store )
		 {
			  Unzip.unzip( this.GetType(), store, TestDir.databaseDir() );
		 }

		 private static System.Func<int, object> IntValues()
		 {
			  return ValueContext.numeric;
		 }

		 private static System.Func<int, object> LongValues()
		 {
			  return value => ValueContext.numeric( ( long ) value );
		 }

		 private static System.Func<int, object> FloatValues()
		 {
			  return value => ValueContext.numeric( ( float ) value );
		 }

		 private static System.Func<int, object> DoubleValues()
		 {
			  return value => ValueContext.numeric( ( double ) value );
		 }

		 private static System.Func<int, object> StringValues()
		 {
			  return value => "value balue " + value;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static org.neo4j.function.Factory<org.neo4j.graphdb.Node> readNodes(final org.neo4j.graphdb.GraphDatabaseService db)
		 private static Factory<Node> ReadNodes( GraphDatabaseService db )
		 {
			  return new FactoryAnonymousInnerClass( db );
		 }

		 private class FactoryAnonymousInnerClass : Factory<Node>
		 {
			 private GraphDatabaseService _db;

			 public FactoryAnonymousInnerClass( GraphDatabaseService db )
			 {
				 this._db = db;
			 }

			 private long id;

			 public Node newInstance()
			 {
				  return _db.getNodeById( id++ );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static org.neo4j.function.Factory<org.neo4j.graphdb.Relationship> readRelationships(final org.neo4j.graphdb.GraphDatabaseService db)
		 private static Factory<Relationship> ReadRelationships( GraphDatabaseService db )
		 {
			  return new FactoryAnonymousInnerClass2( db );
		 }

		 private class FactoryAnonymousInnerClass2 : Factory<Relationship>
		 {
			 private GraphDatabaseService _db;

			 public FactoryAnonymousInnerClass2( GraphDatabaseService db )
			 {
				 this._db = db;
			 }

			 private long id;

			 public Relationship newInstance()
			 {
				  return _db.getRelationshipById( id++ );
			 }
		 }

		 private static Index<Node> NodeIndex( GraphDatabaseService db, string name, IDictionary<string, string> config )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Index<Node> index = Db.index().forNodes(name, config);
					tx.Success();
					return index;
			  }
		 }

		 private static RelationshipIndex RelationshipIndex( GraphDatabaseService db, string name, IDictionary<string, string> config )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					RelationshipIndex index = Db.index().forRelationships(name, config);
					tx.Success();
					return index;
			  }
		 }

		 private static void ReadIndex<ENTITY>( GraphDatabaseService db, Index<ENTITY> index, Factory<ENTITY> entityFactory, System.Func<int, string> keyFactory, System.Func<int, object> valueFactory ) where ENTITY : Org.Neo4j.Graphdb.PropertyContainer
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					for ( int i = 0; i < 10; i++ )
					{
						 ENTITY entity = entityFactory.NewInstance();
						 string key = keyFactory( i );
						 object value = valueFactory( i );
						 assertEquals( entity, single( index.get( key, value ) ) );
					}
					tx.Success();
			  }
		 }

		 private static System.Func<int, string> BasicKeyFactory()
		 {
			  return value => "key-" + ( value % 3 );
		 }

		 private void CheckMigrationProgressFeedback()
		 {
			  SuppressOutput.OutputVoice.containsMessage( "Starting upgrade of database" );
			  SuppressOutput.OutputVoice.containsMessage( "Successfully finished upgrade of database" );
			  SuppressOutput.OutputVoice.containsMessage( "10%" );
			  SuppressOutput.OutputVoice.containsMessage( "100%" );
		 }
	}

}