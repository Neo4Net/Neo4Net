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
namespace Schema
{
	using StringUtils = org.apache.commons.lang3.StringUtils;
	using IndexWriter = Org.Apache.Lucene.Index.IndexWriter;
	using AfterEach = org.junit.jupiter.api.AfterEach;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;

	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using GraphDatabaseFactory = Neo4Net.Graphdb.factory.GraphDatabaseFactory;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using IndexDefinition = Neo4Net.Graphdb.schema.IndexDefinition;
	using Inject = Neo4Net.Test.extension.Inject;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.allOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.default_schema_provider;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.TreeNodeDynamicSize.keyValueSizeCapFromPageSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.PageCache_Fields.PAGE_SIZE;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(TestDirectoryExtension.class) class IndexValuesValidationTest
	internal class IndexValuesValidationTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.TestDirectory directory;
		 private TestDirectory _directory;

		 private GraphDatabaseService _database;

		 internal virtual params string[] Up
		 {
			 set
			 {
				  _database = ( new GraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(_directory.storeDir()).setConfig(stringMap(value)).newGraphDatabase();
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterEach void tearDown()
		 internal virtual void TearDown()
		 {
			  _database.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void validateIndexedNodePropertiesInLucene()
		 internal virtual void ValidateIndexedNodePropertiesInLucene()
		 {
			  setUp( default_schema_provider.name(), GraphDatabaseSettings.SchemaIndex.NATIVE10.providerName() );
			  Label label = Label.label( "indexedNodePropertiesTestLabel" );
			  string propertyName = "indexedNodePropertyName";

			  CreateIndex( label, propertyName );

			  using ( Transaction ignored = _database.beginTx() )
			  {
					_database.schema().awaitIndexesOnline(5, TimeUnit.MINUTES);
			  }

			  System.ArgumentException argumentException = assertThrows(typeof(System.ArgumentException), () =>
			  {
				using ( Transaction transaction = _database.beginTx() )
				{
					 Node node = _database.createNode( label );
					 node.setProperty( propertyName, StringUtils.repeat( "a", IndexWriter.MAX_TERM_LENGTH + 1 ) );
					 transaction.success();
				}
			  });
			  assertThat( argumentException.Message, equalTo( "Property value size is too large for index. Please see index documentation for limitations." ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void validateIndexedNodePropertiesInNativeBtree()
		 internal virtual void ValidateIndexedNodePropertiesInNativeBtree()
		 {
			  setUp();
			  Label label = Label.label( "indexedNodePropertiesTestLabel" );
			  string propertyName = "indexedNodePropertyName";

			  CreateIndex( label, propertyName );

			  using ( Transaction ignored = _database.beginTx() )
			  {
					_database.schema().awaitIndexesOnline(5, TimeUnit.MINUTES);
			  }

			  System.ArgumentException argumentException = assertThrows(typeof(System.ArgumentException), () =>
			  {
				using ( Transaction transaction = _database.beginTx() )
				{
					 Node node = _database.createNode( label );
					 node.setProperty( propertyName, StringUtils.repeat( "a", keyValueSizeCapFromPageSize( PAGE_SIZE ) + 1 ) );
					 transaction.success();
				}
			  });
			  assertThat( argumentException.Message, containsString( "is too large to index into this particular index. Please see index documentation for limitations." ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void validateNodePropertiesOnPopulation()
		 internal virtual void ValidateNodePropertiesOnPopulation()
		 {
			  setUp();
			  Label label = Label.label( "populationTestNodeLabel" );
			  string propertyName = "populationTestPropertyName";

			  using ( Transaction transaction = _database.beginTx() )
			  {
					Node node = _database.createNode( label );
					node.SetProperty( propertyName, StringUtils.repeat( "a", IndexWriter.MAX_TERM_LENGTH + 1 ) );
					transaction.Success();
			  }

			  IndexDefinition indexDefinition = CreateIndex( label, propertyName );
			  try
			  {
					using ( Transaction ignored = _database.beginTx() )
					{
						 _database.schema().awaitIndexesOnline(5, TimeUnit.MINUTES);
					}
			  }
			  catch ( System.InvalidOperationException )
			  {
					using ( Transaction ignored = _database.beginTx() )
					{
						 string indexFailure = _database.schema().getIndexFailure(indexDefinition);
						 assertThat( indexFailure, allOf( containsString( "java.lang.IllegalArgumentException:" ), containsString( "Please see index documentation for limitations." ) ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void validateExplicitIndexedNodeProperties()
		 internal virtual void ValidateExplicitIndexedNodeProperties()
		 {
			  setUp();
			  Label label = Label.label( "explicitIndexedNodePropertiesTestLabel" );
			  string propertyName = "explicitIndexedNodeProperties";
			  string explicitIndexedNodeIndex = "explicitIndexedNodeIndex";

			  using ( Transaction transaction = _database.beginTx() )
			  {
					Node node = _database.createNode( label );
					_database.index().forNodes(explicitIndexedNodeIndex).add(node, propertyName, "shortString");
					transaction.Success();
			  }

			  System.ArgumentException argumentException = assertThrows(typeof(System.ArgumentException), () =>
			  {
				using ( Transaction transaction = _database.beginTx() )
				{
					 Node node = _database.createNode( label );
					 string longValue = StringUtils.repeat( "a", IndexWriter.MAX_TERM_LENGTH + 1 );
					 _database.index().forNodes(explicitIndexedNodeIndex).add(node, propertyName, longValue);
					 transaction.Success();
				}
			  });
			  assertEquals( "Property value size is too large for index. Please see index documentation for limitations.", argumentException.Message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void validateExplicitIndexedRelationshipProperties()
		 internal virtual void ValidateExplicitIndexedRelationshipProperties()
		 {
			  setUp();
			  Label label = Label.label( "explicitIndexedRelationshipPropertiesTestLabel" );
			  string propertyName = "explicitIndexedRelationshipProperties";
			  string explicitIndexedRelationshipIndex = "explicitIndexedRelationshipIndex";
			  RelationshipType indexType = RelationshipType.withName( "explicitIndexType" );

			  using ( Transaction transaction = _database.beginTx() )
			  {
					Node source = _database.createNode( label );
					Node destination = _database.createNode( label );
					Relationship relationship = source.CreateRelationshipTo( destination, indexType );
					_database.index().forRelationships(explicitIndexedRelationshipIndex).add(relationship, propertyName, "shortString");
					transaction.Success();
			  }

			  System.ArgumentException argumentException = assertThrows(typeof(System.ArgumentException), () =>
			  {
				using ( Transaction transaction = _database.beginTx() )
				{
					 Node source = _database.createNode( label );
					 Node destination = _database.createNode( label );
					 Relationship relationship = source.createRelationshipTo( destination, indexType );
					 string longValue = StringUtils.repeat( "a", IndexWriter.MAX_TERM_LENGTH + 1 );
					 _database.index().forRelationships(explicitIndexedRelationshipIndex).add(relationship, propertyName, longValue);
					 transaction.Success();
				}
			  });
			  assertEquals( "Property value size is too large for index. Please see index documentation for limitations.", argumentException.Message );
		 }

		 private IndexDefinition CreateIndex( Label label, string propertyName )
		 {
			  using ( Transaction transaction = _database.beginTx() )
			  {
					IndexDefinition indexDefinition = _database.schema().indexFor(label).on(propertyName).create();
					transaction.Success();
					return indexDefinition;
			  }
		 }
	}

}