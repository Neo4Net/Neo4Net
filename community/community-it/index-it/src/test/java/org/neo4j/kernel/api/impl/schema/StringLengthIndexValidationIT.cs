﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.Api.Impl.Schema
{
	using RandomStringUtils = org.apache.commons.lang3.RandomStringUtils;
	using Matchers = org.hamcrest.Matchers;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Node = Org.Neo4j.Graphdb.Node;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using IndexDefinition = Org.Neo4j.Graphdb.schema.IndexDefinition;
	using Schema = Org.Neo4j.Graphdb.schema.Schema;
	using Org.Neo4j.Index.@internal.gbptree;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using DatabaseRule = Org.Neo4j.Test.rule.DatabaseRule;
	using EmbeddedDatabaseRule = Org.Neo4j.Test.rule.EmbeddedDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.TestLabels.LABEL_ONE;

	public class StringLengthIndexValidationIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.DatabaseRule db = new org.neo4j.test.rule.EmbeddedDatabaseRule().withSetting(org.neo4j.graphdb.factory.GraphDatabaseSettings.default_schema_provider, org.neo4j.graphdb.factory.GraphDatabaseSettings.SchemaIndex.NATIVE20.providerName());
		 public DatabaseRule Db = new EmbeddedDatabaseRule().withSetting(GraphDatabaseSettings.default_schema_provider, GraphDatabaseSettings.SchemaIndex.NATIVE20.providerName());

		 private const string PROP_KEY = "largeString";
		 private static readonly int _keySizeLimit = TreeNodeDynamicSize.keyValueSizeCapFromPageSize( Org.Neo4j.Io.pagecache.PageCache_Fields.PAGE_SIZE ) - Long.BYTES;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSuccessfullyWriteAndReadWithinIndexKeySizeLimit()
		 public virtual void ShouldSuccessfullyWriteAndReadWithinIndexKeySizeLimit()
		 {
			  CreateIndex();
			  string propValue = GetString( _keySizeLimit );
			  long expectedNodeId;

			  // Write
			  expectedNodeId = CreateNode( propValue );

			  // Read
			  AssertReadNode( propValue, expectedNodeId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSuccessfullyPopulateIndexWithinIndexKeySizeLimit()
		 public virtual void ShouldSuccessfullyPopulateIndexWithinIndexKeySizeLimit()
		 {
			  string propValue = GetString( _keySizeLimit );
			  long expectedNodeId;

			  // Write
			  expectedNodeId = CreateNode( propValue );

			  // Populate
			  CreateIndex();

			  // Read
			  AssertReadNode( propValue, expectedNodeId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void txMustFailIfExceedingIndexKeySizeLimit()
		 public virtual void TxMustFailIfExceedingIndexKeySizeLimit()
		 {
			  CreateIndex();

			  // Write
			  try
			  {
					  using ( Transaction tx = Db.beginTx() )
					  {
						string propValue = GetString( _keySizeLimit + 1 );
						Db.createNode( LABEL_ONE ).setProperty( PROP_KEY, propValue );
						tx.Success();
					  }
			  }
			  catch ( System.ArgumentException e )
			  {
					assertThat( e.Message, Matchers.containsString( "Property value size is too large for index. Please see index documentation for limitations." ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void indexPopulationMustFailIfExceedingIndexKeySizeLimit()
		 public virtual void IndexPopulationMustFailIfExceedingIndexKeySizeLimit()
		 {
			  // Write
			  string propValue = GetString( _keySizeLimit + 1 );
			  CreateNode( propValue );

			  // Create index should be fine
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().indexFor(LABEL_ONE).on(PROP_KEY).create();
					tx.Success();
			  }

			  // Waiting for it to come online should fail
			  try
			  {
					  using ( Transaction tx = Db.beginTx() )
					  {
						Db.schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
						tx.Success();
					  }
			  }
			  catch ( System.InvalidOperationException e )
			  {
					assertThat( e.Message, Matchers.containsString( "Index IndexDefinition[label:LABEL_ONE on:largeString] (IndexRule[id=1, descriptor=Index( GENERAL, :label[0](property[0]) ), " + "provider={key=lucene+native, version=2.0}]) entered a FAILED state." ) );
			  }

			  // Index should be in failed state
			  using ( Transaction tx = Db.beginTx() )
			  {
					IEnumerator<IndexDefinition> iterator = Db.schema().getIndexes(LABEL_ONE).GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( iterator.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					IndexDefinition next = iterator.next();
					assertEquals( "state is FAILED", Org.Neo4j.Graphdb.schema.Schema_IndexState.Failed, Db.schema().getIndexState(next) );
					assertThat( Db.schema().getIndexFailure(next), Matchers.containsString("Index key-value size it to large. Please see index documentation for limitations.") );
					tx.Success();
			  }
		 }

		 // Each char in string need to fit in one byte
		 private string GetString( int byteArraySize )
		 {
			  return RandomStringUtils.randomAlphabetic( byteArraySize );
		 }

		 private void CreateIndex()
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().indexFor(LABEL_ONE).on(PROP_KEY).create();
					tx.Success();
			  }
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
					tx.Success();
			  }
		 }

		 private long CreateNode( string propValue )
		 {
			  long expectedNodeId;
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode( LABEL_ONE );
					node.SetProperty( PROP_KEY, propValue );
					expectedNodeId = node.Id;
					tx.Success();
			  }
			  return expectedNodeId;
		 }

		 private void AssertReadNode( string propValue, long expectedNodeId )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.findNode( LABEL_ONE, PROP_KEY, propValue );
					assertNotNull( node );
					assertEquals( "node id", expectedNodeId, node.Id );
					tx.Success();
			  }
		 }
	}

}