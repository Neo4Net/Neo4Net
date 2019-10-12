using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.Api.Impl.Schema.reader
{
	using MutableInt = org.apache.commons.lang3.mutable.MutableInt;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using LuceneIndexWriter = Org.Neo4j.Kernel.Api.Impl.Schema.writer.LuceneIndexWriter;
	using SchemaDescriptorFactory = Org.Neo4j.Kernel.api.schema.SchemaDescriptorFactory;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using GatheringNodeValueClient = Org.Neo4j.Kernel.Impl.Index.Schema.GatheringNodeValueClient;
	using NodePropertyAccessor = Org.Neo4j.Storageengine.Api.NodePropertyAccessor;
	using IndexDescriptorFactory = Org.Neo4j.Storageengine.Api.schema.IndexDescriptorFactory;
	using IndexReader = Org.Neo4j.Storageengine.Api.schema.IndexReader;
	using RandomRule = Org.Neo4j.Test.rule.RandomRule;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;
	using Value = Org.Neo4j.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.schema.LuceneDocumentStructure.documentRepresentingProperties;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringValue;

	public class SimpleIndexReaderDistinctValuesTest
	{
		private bool InstanceFieldsInitialized = false;

		public SimpleIndexReaderDistinctValuesTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			Directory = TestDirectory.testDirectory( Fs );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.RandomRule random = new org.neo4j.test.rule.RandomRule();
		 public readonly RandomRule Random = new RandomRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.DefaultFileSystemRule fs = new org.neo4j.test.rule.fs.DefaultFileSystemRule();
		 public readonly DefaultFileSystemRule Fs = new DefaultFileSystemRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory directory = org.neo4j.test.rule.TestDirectory.testDirectory(fs);
		 public TestDirectory Directory;
		 private SchemaIndex _index;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  _index = LuceneSchemaIndexBuilder.create( IndexDescriptorFactory.forSchema( SchemaDescriptorFactory.forLabel( 1, 1 ) ), Config.defaults() ).withFileSystem(Fs).withIndexRootFolder(Directory.directory()).build();
			  _index.create();
			  _index.open();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TearDown()
		 {
			  _index.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetDistinctStringValues() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGetDistinctStringValues()
		 {
			  // given
			  LuceneIndexWriter writer = _index.IndexWriter;
			  IDictionary<Value, MutableInt> expectedCounts = new Dictionary<Value, MutableInt>();
			  for ( int i = 0; i < 10_000; i++ )
			  {
					Value value = stringValue( Random.Next( 1_000 ).ToString() );
					writer.AddDocument( documentRepresentingProperties( i, value ) );
					expectedCounts.computeIfAbsent( value, v => new MutableInt( 0 ) ).increment();
			  }
			  _index.maybeRefreshBlocking();

			  // when/then
			  GatheringNodeValueClient client = new GatheringNodeValueClient();
			  NodePropertyAccessor propertyAccessor = mock( typeof( NodePropertyAccessor ) );
			  using ( IndexReader reader = _index.IndexReader )
			  {
					reader.DistinctValues( client, propertyAccessor, true );
					while ( client.Progressor.next() )
					{
						 Value value = client.Values[0];
						 MutableInt expectedCount = expectedCounts.Remove( value );
						 assertNotNull( expectedCount );
						 assertEquals( expectedCount.intValue(), client.Reference );
					}
					assertTrue( expectedCounts.Count == 0 );
			  }
			  verifyNoMoreInteractions( propertyAccessor );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCountDistinctValues() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCountDistinctValues()
		 {
			  // given
			  LuceneIndexWriter writer = _index.IndexWriter;
			  int expectedCount = 10_000;
			  for ( int i = 0; i < expectedCount; i++ )
			  {
					Value value = Random.nextValue();
					writer.AddDocument( documentRepresentingProperties( i, value ) );
			  }
			  _index.maybeRefreshBlocking();

			  // when/then
			  GatheringNodeValueClient client = new GatheringNodeValueClient();
			  NodePropertyAccessor propertyAccessor = mock( typeof( NodePropertyAccessor ) );
			  using ( IndexReader reader = _index.IndexReader )
			  {
					reader.DistinctValues( client, propertyAccessor, true );
					int actualCount = 0;
					while ( client.Progressor.next() )
					{
						 actualCount += ( int )client.Reference;
					}
					assertEquals( expectedCount, actualCount );
			  }
			  verifyNoMoreInteractions( propertyAccessor );
		 }
	}

}