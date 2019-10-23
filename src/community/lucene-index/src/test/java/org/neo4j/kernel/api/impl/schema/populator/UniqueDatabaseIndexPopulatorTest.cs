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
namespace Neo4Net.Kernel.Api.Impl.Schema.populator
{
	using Directory = org.apache.lucene.store.Directory;
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using PrimitiveLongCollections = Neo4Net.Collections.PrimitiveLongCollections;
	using IndexQuery = Neo4Net.Kernel.Api.Internal.IndexQuery;
	using LabelSchemaDescriptor = Neo4Net.Kernel.Api.Internal.schema.LabelSchemaDescriptor;
	using SchemaDescriptor = Neo4Net.Kernel.Api.Internal.schema.SchemaDescriptor;
	using IOUtils = Neo4Net.Io.IOUtils;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using DirectoryFactory = Neo4Net.Kernel.Api.Impl.Index.storage.DirectoryFactory;
	using PartitionedIndexStorage = Neo4Net.Kernel.Api.Impl.Index.storage.PartitionedIndexStorage;
	using Neo4Net.Kernel.Api.Index;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using NodePropertyAccessor = Neo4Net.Kernel.Api.StorageEngine.NodePropertyAccessor;
	using SchemaDescriptorFactory = Neo4Net.Kernel.api.schema.SchemaDescriptorFactory;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using Config = Neo4Net.Kernel.configuration.Config;
	using IndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor;
	using IndexReader = Neo4Net.Kernel.Api.StorageEngine.schema.IndexReader;
	using IndexSample = Neo4Net.Kernel.Api.StorageEngine.schema.IndexSample;
	using Neo4Net.Test;
	using Neo4Net.Test.OtherThreadExecutor;
	using CleanupRule = Neo4Net.Test.rule.CleanupRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.index.IndexQueryHelper.add;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.index.IndexQueryHelper.change;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.index.IndexQueryHelper.remove;

	public class UniqueDatabaseIndexPopulatorTest
	{
		private bool InstanceFieldsInitialized = false;

		public UniqueDatabaseIndexPopulatorTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _testDir ).around( _cleanup ).around( _fileSystemRule );
		}

		 private readonly CleanupRule _cleanup = new CleanupRule();
		 private readonly TestDirectory _testDir = TestDirectory.testDirectory();
		 private readonly DefaultFileSystemRule _fileSystemRule = new DefaultFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(testDir).around(cleanup).around(fileSystemRule);
		 public RuleChain RuleChain;

		 private const int LABEL_ID = 1;
		 private const int PROPERTY_KEY_ID = 2;

		 private readonly DirectoryFactory _directoryFactory = new Neo4Net.Kernel.Api.Impl.Index.storage.DirectoryFactory_InMemoryDirectoryFactory();
		 private static readonly IndexDescriptor _descriptor = TestIndexDescriptorFactory.forLabel( LABEL_ID, PROPERTY_KEY_ID );

		 private readonly NodePropertyAccessor _nodePropertyAccessor = mock( typeof( NodePropertyAccessor ) );

		 private PartitionedIndexStorage _indexStorage;
		 private SchemaIndex _index;
		 private UniqueLuceneIndexPopulator _populator;
		 private SchemaDescriptor _schemaDescriptor;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  File folder = _testDir.directory( "folder" );
			  _indexStorage = new PartitionedIndexStorage( _directoryFactory, _fileSystemRule.get(), folder );
			  _index = LuceneSchemaIndexBuilder.create( _descriptor, Config.defaults() ).withIndexStorage(_indexStorage).build();
			  _schemaDescriptor = _descriptor.schema();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TearDown()
		 {
			  if ( _populator != null )
			  {
					_populator.close( false );
			  }
			  IOUtils.closeAll( _index, _directoryFactory );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldVerifyThatThereAreNoDuplicates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldVerifyThatThereAreNoDuplicates()
		 {
			  // given
			  _populator = NewPopulator();

			  AddUpdate( _populator, 1, "value1" );
			  AddUpdate( _populator, 2, "value2" );
			  AddUpdate( _populator, 3, "value3" );

			  // when
			  _populator.verifyDeferredConstraints( _nodePropertyAccessor );
			  _populator.close( true );

			  // then
			  assertEquals( asList( 1L ), GetAllNodes( Directory, "value1" ) );
			  assertEquals( asList( 2L ), GetAllNodes( Directory, "value2" ) );
			  assertEquals( asList( 3L ), GetAllNodes( Directory, "value3" ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.apache.lucene.store.Directory getDirectory() throws java.io.IOException
		 private Directory Directory
		 {
			 get
			 {
				  File partitionFolder = _indexStorage.getPartitionFolder( 1 );
				  return _indexStorage.openDirectory( partitionFolder );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpdateEntryForNodeThatHasAlreadyBeenIndexed() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUpdateEntryForNodeThatHasAlreadyBeenIndexed()
		 {
			  // given
			  _populator = NewPopulator();

			  AddUpdate( _populator, 1, "value1" );

			  // when
			  IndexUpdater updater = _populator.newPopulatingUpdater( _nodePropertyAccessor );

			  updater.Process( change( 1, _schemaDescriptor, "value1", "value2" ) );

			  _populator.close( true );

			  // then
			  assertEquals( Collections.EMPTY_LIST, GetAllNodes( Directory, "value1" ) );
			  assertEquals( asList( 1L ), GetAllNodes( Directory, "value2" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpdateEntryForNodeThatHasPropertyRemovedAndThenAddedAgain() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUpdateEntryForNodeThatHasPropertyRemovedAndThenAddedAgain()
		 {
			  // given
			  _populator = NewPopulator();

			  AddUpdate( _populator, 1, "value1" );

			  // when
			  IndexUpdater updater = _populator.newPopulatingUpdater( _nodePropertyAccessor );

			  updater.Process( remove( 1, _schemaDescriptor, "value1" ) );
			  updater.Process( add( 1, _schemaDescriptor, "value1" ) );

			  _populator.close( true );

			  // then
			  assertEquals( asList( 1L ), GetAllNodes( Directory, "value1" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveEntryForNodeThatHasAlreadyBeenIndexed() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRemoveEntryForNodeThatHasAlreadyBeenIndexed()
		 {
			  // given
			  _populator = NewPopulator();

			  AddUpdate( _populator, 1, "value1" );

			  // when
			  IndexUpdater updater = _populator.newPopulatingUpdater( _nodePropertyAccessor );

			  updater.Process( remove( 1, _schemaDescriptor, "value1" ) );

			  _populator.close( true );

			  // then
			  assertEquals( Collections.EMPTY_LIST, GetAllNodes( Directory, "value1" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToHandleSwappingOfIndexValues() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToHandleSwappingOfIndexValues()
		 {
			  // given
			  _populator = NewPopulator();

			  AddUpdate( _populator, 1, "value1" );
			  AddUpdate( _populator, 2, "value2" );

			  // when
			  IndexUpdater updater = _populator.newPopulatingUpdater( _nodePropertyAccessor );

			  updater.Process( change( 1, _schemaDescriptor, "value1", "value2" ) );
			  updater.Process( change( 2, _schemaDescriptor, "value2", "value1" ) );

			  _populator.close( true );

			  // then
			  assertEquals( asList( 2L ), GetAllNodes( Directory, "value1" ) );
			  assertEquals( asList( 1L ), GetAllNodes( Directory, "value2" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailAtVerificationStageWithAlreadyIndexedStringValue() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailAtVerificationStageWithAlreadyIndexedStringValue()
		 {
			  // given
			  _populator = NewPopulator();

			  string value = "value1";
			  AddUpdate( _populator, 1, value );
			  AddUpdate( _populator, 2, "value2" );
			  AddUpdate( _populator, 3, value );

			  when( _nodePropertyAccessor.getNodePropertyValue( 1, PROPERTY_KEY_ID ) ).thenReturn( Values.of( value ) );
			  when( _nodePropertyAccessor.getNodePropertyValue( 3, PROPERTY_KEY_ID ) ).thenReturn( Values.of( value ) );

			  // when
			  try
			  {
					_populator.verifyDeferredConstraints( _nodePropertyAccessor );

					fail( "should have thrown exception" );
			  }
			  // then
			  catch ( IndexEntryConflictException conflict )
			  {
					assertEquals( 1, conflict.ExistingNodeId );
					assertEquals( Values.of( value ), conflict.SinglePropertyValue );
					assertEquals( 3, conflict.AddedNodeId );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailAtVerificationStageWithAlreadyIndexedNumberValue() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailAtVerificationStageWithAlreadyIndexedNumberValue()
		 {
			  // given
			  _populator = NewPopulator();

			  AddUpdate( _populator, 1, 1 );
			  AddUpdate( _populator, 2, 2 );
			  AddUpdate( _populator, 3, 1 );

			  when( _nodePropertyAccessor.getNodePropertyValue( 1, PROPERTY_KEY_ID ) ).thenReturn( Values.of( 1 ) );
			  when( _nodePropertyAccessor.getNodePropertyValue( 3, PROPERTY_KEY_ID ) ).thenReturn( Values.of( 1 ) );

			  // when
			  try
			  {
					_populator.verifyDeferredConstraints( _nodePropertyAccessor );

					fail( "should have thrown exception" );
			  }
			  // then
			  catch ( IndexEntryConflictException conflict )
			  {
					assertEquals( 1, conflict.ExistingNodeId );
					assertEquals( Values.of( 1 ), conflict.SinglePropertyValue );
					assertEquals( 3, conflict.AddedNodeId );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRejectDuplicateEntryWhenUsingPopulatingUpdater() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRejectDuplicateEntryWhenUsingPopulatingUpdater()
		 {
			  // given
			  _populator = NewPopulator();

			  AddUpdate( _populator, 1, "value1" );
			  AddUpdate( _populator, 2, "value2" );

			  Value value = Values.of( "value1" );
			  when( _nodePropertyAccessor.getNodePropertyValue( 1, PROPERTY_KEY_ID ) ).thenReturn( value );
			  when( _nodePropertyAccessor.getNodePropertyValue( 3, PROPERTY_KEY_ID ) ).thenReturn( value );

			  // when
			  try
			  {
					IndexUpdater updater = _populator.newPopulatingUpdater( _nodePropertyAccessor );
					updater.Process( add( 3, _schemaDescriptor, "value1" ) );
					updater.Close();

					fail( "should have thrown exception" );
			  }
			  // then
			  catch ( IndexEntryConflictException conflict )
			  {
					assertEquals( 1, conflict.ExistingNodeId );
					assertEquals( value, conflict.SinglePropertyValue );
					assertEquals( 3, conflict.AddedNodeId );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRejectDuplicateEntryAfterUsingPopulatingUpdater() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRejectDuplicateEntryAfterUsingPopulatingUpdater()
		 {
			  // given
			  _populator = NewPopulator();

			  string valueString = "value1";
			  IndexUpdater updater = _populator.newPopulatingUpdater( _nodePropertyAccessor );
			  updater.Process( add( 1, _schemaDescriptor, valueString ) );
			  AddUpdate( _populator, 2, valueString );

			  Value value = Values.of( valueString );
			  when( _nodePropertyAccessor.getNodePropertyValue( 1, PROPERTY_KEY_ID ) ).thenReturn( value );
			  when( _nodePropertyAccessor.getNodePropertyValue( 2, PROPERTY_KEY_ID ) ).thenReturn( value );

			  // when
			  try
			  {
					_populator.verifyDeferredConstraints( _nodePropertyAccessor );

					fail( "should have thrown exception" );
			  }
			  // then
			  catch ( IndexEntryConflictException conflict )
			  {
					assertEquals( 1, conflict.ExistingNodeId );
					assertEquals( value, conflict.SinglePropertyValue );
					assertEquals( 2, conflict.AddedNodeId );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotRejectDuplicateEntryOnSameNodeIdAfterUsingPopulatingUpdater() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotRejectDuplicateEntryOnSameNodeIdAfterUsingPopulatingUpdater()
		 {
			  // given
			  _populator = NewPopulator();

			  when( _nodePropertyAccessor.getNodePropertyValue( 1, PROPERTY_KEY_ID ) ).thenReturn( Values.of( "value1" ) );

			  IndexUpdater updater = _populator.newPopulatingUpdater( _nodePropertyAccessor );
			  updater.Process( add( 1, _schemaDescriptor, "value1" ) );
			  updater.Process( change( 1, _schemaDescriptor, "value1", "value1" ) );
			  updater.Close();
			  AddUpdate( _populator, 2, "value2" );
			  AddUpdate( _populator, 3, "value3" );

			  // when
			  _populator.verifyDeferredConstraints( _nodePropertyAccessor );
			  _populator.close( true );

			  // then
			  assertEquals( asList( 1L ), GetAllNodes( Directory, "value1" ) );
			  assertEquals( asList( 2L ), GetAllNodes( Directory, "value2" ) );
			  assertEquals( asList( 3L ), GetAllNodes( Directory, "value3" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotRejectIndexCollisionsCausedByPrecisionLossAsDuplicates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotRejectIndexCollisionsCausedByPrecisionLossAsDuplicates()
		 {
			  // given
			  _populator = NewPopulator();

			  // Given we have a collision in our index...
			  AddUpdate( _populator, 1, 1000000000000000001L );
			  AddUpdate( _populator, 2, 2 );
			  AddUpdate( _populator, 3, 1000000000000000001L );

			  // ... but the actual data in the store does not collide
			  when( _nodePropertyAccessor.getNodePropertyValue( 1, PROPERTY_KEY_ID ) ).thenReturn( Values.of( 1000000000000000001L ) );
			  when( _nodePropertyAccessor.getNodePropertyValue( 3, PROPERTY_KEY_ID ) ).thenReturn( Values.of( 1000000000000000002L ) );

			  // Then our verification should NOT fail:
			  _populator.verifyDeferredConstraints( _nodePropertyAccessor );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCheckAllCollisionsFromPopulatorAdd() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCheckAllCollisionsFromPopulatorAdd()
		 {
			  // given
			  _populator = NewPopulator();

			  int iterations = 228; // This value has to be high enough to stress the EntrySet implementation
			  IndexUpdater updater = _populator.newPopulatingUpdater( _nodePropertyAccessor );

			  for ( int nodeId = 0; nodeId < iterations; nodeId++ )
			  {
					updater.Process( add( nodeId, _schemaDescriptor, 1 ) );
					when( _nodePropertyAccessor.getNodePropertyValue( nodeId, PROPERTY_KEY_ID ) ).thenReturn( Values.of( nodeId ) );
			  }

			  // ... and the actual conflicting property:
			  updater.Process( add( iterations, _schemaDescriptor, 1 ) );
			  when( _nodePropertyAccessor.getNodePropertyValue( iterations, PROPERTY_KEY_ID ) ).thenReturn( Values.of( 1 ) ); // This collision is real!!!

			  // when
			  try
			  {
					updater.Close();
					fail( "should have thrown exception" );
			  }
			  // then
			  catch ( IndexEntryConflictException conflict )
			  {
					assertEquals( 1, conflict.ExistingNodeId );
					assertEquals( Values.of( 1 ), conflict.SinglePropertyValue );
					assertEquals( iterations, conflict.AddedNodeId );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCheckAllCollisionsFromUpdaterClose() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCheckAllCollisionsFromUpdaterClose()
		 {
			  // given
			  _populator = NewPopulator();

			  int iterations = 228; // This value has to be high enough to stress the EntrySet implementation

			  for ( int nodeId = 0; nodeId < iterations; nodeId++ )
			  {
					AddUpdate( _populator, nodeId, 1 );
					when( _nodePropertyAccessor.getNodePropertyValue( nodeId, PROPERTY_KEY_ID ) ).thenReturn( Values.of( nodeId ) );
			  }

			  // ... and the actual conflicting property:
			  AddUpdate( _populator, iterations, 1 );
			  when( _nodePropertyAccessor.getNodePropertyValue( iterations, PROPERTY_KEY_ID ) ).thenReturn( Values.of( 1 ) ); // This collision is real!!!

			  // when
			  try
			  {
					_populator.verifyDeferredConstraints( _nodePropertyAccessor );
					fail( "should have thrown exception" );
			  }
			  // then
			  catch ( IndexEntryConflictException conflict )
			  {
					assertEquals( 1, conflict.ExistingNodeId );
					assertEquals( Values.of( 1 ), conflict.SinglePropertyValue );
					assertEquals( iterations, conflict.AddedNodeId );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReleaseSearcherProperlyAfterVerifyingDeferredConstraints() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReleaseSearcherProperlyAfterVerifyingDeferredConstraints()
		 {
			  // given
			  _populator = NewPopulator();

			  /*
			   * This test was created due to a problem in closing an index updater after deferred constraints
			   * had been verified, where it got stuck in a busy loop in ReferenceManager#acquire.
			   */

			  // GIVEN an index updater that we close
			  OtherThreadExecutor<Void> executor = _cleanup.add( new OtherThreadExecutor<Void>( "Deferred", null ) );
			  executor.Execute((OtherThreadExecutor.WorkerCommand<Void, Void>) state =>
			  {
				using ( IndexUpdater updater = _populator.newPopulatingUpdater( _nodePropertyAccessor ) )
				{ // Just open it and let it be closed
				}
				return null;
			  });
			  // ... and where we verify deferred constraints after
			  executor.Execute((OtherThreadExecutor.WorkerCommand<Void, Void>) state =>
			  {
				_populator.verifyDeferredConstraints( _nodePropertyAccessor );
				return null;
			  });

			  // WHEN doing more index updating after that
			  // THEN it should be able to complete within a very reasonable time
			  executor.Execute((OtherThreadExecutor.WorkerCommand<Void, Void>) state =>
			  {
				using ( IndexUpdater secondUpdater = _populator.newPopulatingUpdater( _nodePropertyAccessor ) )
				{ // Just open it and let it be closed
				}
				return null;
			  }, 5, SECONDS);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void sampleEmptyIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SampleEmptyIndex()
		 {
			  _populator = NewPopulator();

			  IndexSample sample = _populator.sampleResult();

			  assertEquals( new IndexSample(), sample );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void sampleIncludedUpdates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SampleIncludedUpdates()
		 {
			  LabelSchemaDescriptor schemaDescriptor = SchemaDescriptorFactory.forLabel( 1, 1 );
			  _populator = NewPopulator();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<org.Neo4Net.kernel.api.index.IndexEntryUpdate<?>> updates = java.util.Arrays.asList(add(1, schemaDescriptor, "foo"), add(2, schemaDescriptor, "bar"), add(3, schemaDescriptor, "baz"), add(4, schemaDescriptor, "qux"));
			  IList<IndexEntryUpdate<object>> updates = Arrays.asList( add( 1, schemaDescriptor, "foo" ), add( 2, schemaDescriptor, "bar" ), add( 3, schemaDescriptor, "baz" ), add( 4, schemaDescriptor, "qux" ) );

			  updates.ForEach( _populator.includeSample );

			  IndexSample sample = _populator.sampleResult();

			  assertEquals( new IndexSample( 4, 4, 4 ), sample );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addUpdates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AddUpdates()
		 {
			  _populator = NewPopulator();

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<org.Neo4Net.kernel.api.index.IndexEntryUpdate<?>> updates = java.util.Arrays.asList(add(1, schemaDescriptor, "aaa"), add(2, schemaDescriptor, "bbb"), add(3, schemaDescriptor, "ccc"));
			  IList<IndexEntryUpdate<object>> updates = Arrays.asList( add( 1, _schemaDescriptor, "aaa" ), add( 2, _schemaDescriptor, "bbb" ), add( 3, _schemaDescriptor, "ccc" ) );

			  _populator.add( updates );

			  _index.maybeRefreshBlocking();
			  using ( IndexReader reader = _index.IndexReader )
			  {
					LongIterator allEntities = reader.Query( IndexQuery.exists( 1 ) );
					assertArrayEquals( new long[]{ 1, 2, 3 }, PrimitiveLongCollections.asArray( allEntities ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private UniqueLuceneIndexPopulator newPopulator() throws java.io.IOException
		 private UniqueLuceneIndexPopulator NewPopulator()
		 {
			  UniqueLuceneIndexPopulator populator = new UniqueLuceneIndexPopulator( _index, _descriptor );
			  populator.Create();
			  return populator;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void addUpdate(UniqueLuceneIndexPopulator populator, long nodeId, Object value) throws java.io.IOException
		 private static void AddUpdate( UniqueLuceneIndexPopulator populator, long nodeId, object value )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.Neo4Net.kernel.api.index.IndexEntryUpdate<?> update = add(nodeId, descriptor.schema(), value);
			  IndexEntryUpdate<object> update = add( nodeId, _descriptor.schema(), value );
			  populator.Add( asList( update ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.List<long> getAllNodes(org.apache.lucene.store.Directory directory, Object value) throws java.io.IOException
		 private IList<long> GetAllNodes( Directory directory, object value )
		 {
			  return AllNodesCollector.getAllNodes( directory, Values.of( value ) );
		 }
	}

}