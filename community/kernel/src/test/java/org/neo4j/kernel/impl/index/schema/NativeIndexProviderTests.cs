using System;

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
namespace Org.Neo4j.Kernel.Impl.Index.Schema
{
	using Matchers = org.hamcrest.Matchers;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using RecoveryCleanupWorkCollector = Org.Neo4j.Index.@internal.gbptree.RecoveryCleanupWorkCollector;
	using InternalIndexState = Org.Neo4j.@internal.Kernel.Api.InternalIndexState;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using IndexEntryConflictException = Org.Neo4j.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using IndexAccessor = Org.Neo4j.Kernel.Api.Index.IndexAccessor;
	using IndexDirectoryStructure = Org.Neo4j.Kernel.Api.Index.IndexDirectoryStructure;
	using Org.Neo4j.Kernel.Api.Index;
	using IndexPopulator = Org.Neo4j.Kernel.Api.Index.IndexPopulator;
	using IndexProvider = Org.Neo4j.Kernel.Api.Index.IndexProvider;
	using IndexUpdater = Org.Neo4j.Kernel.Api.Index.IndexUpdater;
	using LoggingMonitor = Org.Neo4j.Kernel.Api.Index.LoggingMonitor;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using IndexUpdateMode = Org.Neo4j.Kernel.Impl.Api.index.IndexUpdateMode;
	using IndexSamplingConfig = Org.Neo4j.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using AssertableLogProvider = Org.Neo4j.Logging.AssertableLogProvider;
	using IndexDescriptorFactory = Org.Neo4j.Storageengine.Api.schema.IndexDescriptorFactory;
	using StoreIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.StoreIndexDescriptor;
	using PageCacheAndDependenciesRule = Org.Neo4j.Test.rule.PageCacheAndDependenciesRule;
	using Value = Org.Neo4j.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.RecoveryCleanupWorkCollector.immediate;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.index.IndexDirectoryStructure.directoriesByProvider;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.schema.SchemaDescriptorFactory.forLabel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.api.index.TestIndexProviderDescriptor.PROVIDER_DESCRIPTOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.ByteBufferFactory.heapBufferFactory;

	public abstract class NativeIndexProviderTests
	{
		private bool InstanceFieldsInitialized = false;

		public NativeIndexProviderTests()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_monitor = new LoggingMonitor( _logging.getLog( "test" ) );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.PageCacheAndDependenciesRule rules = new org.neo4j.test.rule.PageCacheAndDependenciesRule();
		 public PageCacheAndDependenciesRule Rules = new PageCacheAndDependenciesRule();

		 private const int INDEX_ID = 1;
		 private const int LABEL_ID = 1;
		 private const int PROP_ID = 1;
		 private IndexProvider _provider;
		 private readonly AssertableLogProvider _logging = new AssertableLogProvider();
		 private IndexProvider.Monitor _monitor;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  File nativeSchemaIndexStoreDirectory = NewProvider().directoryStructure().rootDirectory();
			  Rules.fileSystem().mkdirs(nativeSchemaIndexStoreDirectory);
		 }

		 /* getPopulator */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getPopulatorMustThrowIfInReadOnlyMode()
		 public virtual void getPopulatorMustThrowIfInReadOnlyMode()
		 {
			  // given
			  _provider = NewReadOnlyProvider();

			  try
			  {
					// when
					_provider.getPopulator( Descriptor(), SamplingConfig(), heapBufferFactory(1024) );
					fail( "Should have failed" );
			  }
			  catch ( System.NotSupportedException )
			  {
					// then
					// good
			  }
		 }

		 /* getOnlineAccessor */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCheckConflictsWhenApplyingUpdatesInOnlineAccessor() throws java.io.IOException, org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotCheckConflictsWhenApplyingUpdatesInOnlineAccessor()
		 {
			  // given
			  _provider = NewProvider();

			  // when
			  StoreIndexDescriptor descriptor = DescriptorUnique();
			  using ( IndexAccessor accessor = _provider.getOnlineAccessor( descriptor, SamplingConfig() ), IndexUpdater indexUpdater = accessor.NewUpdater(IndexUpdateMode.ONLINE) )
			  {
					Value value = SomeValue();
					indexUpdater.Process( IndexEntryUpdate.add( 1, descriptor.Schema(), value ) );

					// then
					// ... expect no failure on duplicate value
					indexUpdater.Process( IndexEntryUpdate.add( 2, descriptor.Schema(), value ) );
			  }
		 }

		 /* getPopulationFailure */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getPopulationFailureMustThrowIfNoFailure()
		 public virtual void getPopulationFailureMustThrowIfNoFailure()
		 {
			  // given
			  _provider = NewProvider();
			  IndexPopulator populator = _provider.getPopulator( Descriptor(), SamplingConfig(), heapBufferFactory(1024) );
			  populator.Create();
			  populator.Close( true );

			  // when
			  // ... no failure on populator

			  // then
			  try
			  {
					_provider.getPopulationFailure( Descriptor() );
					fail( "Should have failed" );
			  }
			  catch ( System.InvalidOperationException e )
			  {
					// good
					assertThat( e.Message, Matchers.containsString( Convert.ToString( INDEX_ID ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getPopulationFailureMustThrowEvenIfFailureOnOtherIndex()
		 public virtual void getPopulationFailureMustThrowEvenIfFailureOnOtherIndex()
		 {
			  // given
			  _provider = NewProvider();

			  int nonFailedIndexId = NativeIndexProviderTests.INDEX_ID;
			  IndexPopulator nonFailedPopulator = _provider.getPopulator( Descriptor( nonFailedIndexId ), SamplingConfig(), heapBufferFactory(1024) );
			  nonFailedPopulator.Create();
			  nonFailedPopulator.Close( true );

			  int failedIndexId = 2;
			  IndexPopulator failedPopulator = _provider.getPopulator( Descriptor( failedIndexId ), SamplingConfig(), heapBufferFactory(1024) );
			  failedPopulator.Create();

			  // when
			  failedPopulator.MarkAsFailed( "failure" );
			  failedPopulator.Close( false );

			  // then
			  try
			  {
					_provider.getPopulationFailure( Descriptor( nonFailedIndexId ) );
					fail( "Should have failed" );
			  }
			  catch ( System.InvalidOperationException e )
			  {
					// good
					assertThat( e.Message, Matchers.containsString( Convert.ToString( nonFailedIndexId ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getPopulationFailureMustReturnReportedFailure()
		 public virtual void getPopulationFailureMustReturnReportedFailure()
		 {
			  // given
			  _provider = NewProvider();
			  IndexPopulator populator = _provider.getPopulator( Descriptor(), SamplingConfig(), heapBufferFactory(1024) );
			  populator.Create();

			  // when
			  string failureMessage = "fail";
			  populator.MarkAsFailed( failureMessage );
			  populator.Close( false );

			  // then
			  string populationFailure = _provider.getPopulationFailure( Descriptor() );
			  assertThat( populationFailure, @is( failureMessage ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getPopulationFailureMustReturnReportedFailuresForDifferentIndexIds()
		 public virtual void getPopulationFailureMustReturnReportedFailuresForDifferentIndexIds()
		 {
			  // given
			  _provider = NewProvider();
			  int first = 1;
			  int second = 2;
			  int third = 3;
			  IndexPopulator firstPopulator = _provider.getPopulator( Descriptor( first ), SamplingConfig(), heapBufferFactory(1024) );
			  firstPopulator.Create();
			  IndexPopulator secondPopulator = _provider.getPopulator( Descriptor( second ), SamplingConfig(), heapBufferFactory(1024) );
			  secondPopulator.Create();
			  IndexPopulator thirdPopulator = _provider.getPopulator( Descriptor( third ), SamplingConfig(), heapBufferFactory(1024) );
			  thirdPopulator.Create();

			  // when
			  string firstFailure = "first failure";
			  firstPopulator.MarkAsFailed( firstFailure );
			  firstPopulator.Close( false );
			  secondPopulator.Close( true );
			  string thirdFailure = "third failure";
			  thirdPopulator.MarkAsFailed( thirdFailure );
			  thirdPopulator.Close( false );

			  // then
			  assertThat( _provider.getPopulationFailure( Descriptor( first ) ), @is( firstFailure ) );
			  assertThat( _provider.getPopulationFailure( Descriptor( third ) ), @is( thirdFailure ) );
			  try
			  {
					_provider.getPopulationFailure( Descriptor( second ) );
					fail( "Should have failed" );
			  }
			  catch ( System.InvalidOperationException )
			  {
					// good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getPopulationFailureMustPersistReportedFailure()
		 public virtual void getPopulationFailureMustPersistReportedFailure()
		 {
			  // given
			  _provider = NewProvider();
			  IndexPopulator populator = _provider.getPopulator( Descriptor(), SamplingConfig(), heapBufferFactory(1024) );
			  populator.Create();

			  // when
			  string failureMessage = "fail";
			  populator.MarkAsFailed( failureMessage );
			  populator.Close( false );

			  // then
			  _provider = NewProvider();
			  string populationFailure = _provider.getPopulationFailure( Descriptor() );
			  assertThat( populationFailure, @is( failureMessage ) );
		 }

		 /* getInitialState */
		 // pattern: open populator, markAsFailed, close populator, getInitialState, getPopulationFailure

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportCorrectInitialStateIfIndexDoesntExist()
		 public virtual void ShouldReportCorrectInitialStateIfIndexDoesntExist()
		 {
			  // given
			  _provider = NewProvider();

			  // when
			  InternalIndexState state = _provider.getInitialState( Descriptor() );

			  // then
			  InternalIndexState expected = ExpectedStateOnNonExistingSubIndex();
			  assertEquals( expected, state );
			  if ( InternalIndexState.POPULATING == expected )
			  {
					_logging.rawMessageMatcher().assertContains("Failed to open index");
			  }
			  else
			  {
					_logging.rawMessageMatcher().assertNotContains("Failed to open index");
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportInitialStateAsPopulatingIfPopulationStartedButIncomplete()
		 public virtual void ShouldReportInitialStateAsPopulatingIfPopulationStartedButIncomplete()
		 {
			  // given
			  _provider = NewProvider();
			  IndexPopulator populator = _provider.getPopulator( Descriptor(), SamplingConfig(), heapBufferFactory(1024) );
			  populator.Create();

			  // when
			  InternalIndexState state = _provider.getInitialState( Descriptor() );

			  // then
			  assertEquals( InternalIndexState.POPULATING, state );
			  populator.Close( true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportInitialStateAsFailedIfMarkedAsFailed()
		 public virtual void ShouldReportInitialStateAsFailedIfMarkedAsFailed()
		 {
			  // given
			  _provider = NewProvider();
			  IndexPopulator populator = _provider.getPopulator( Descriptor(), SamplingConfig(), heapBufferFactory(1024) );
			  populator.Create();
			  populator.MarkAsFailed( "Just some failure" );
			  populator.Close( false );

			  // when
			  InternalIndexState state = _provider.getInitialState( Descriptor() );

			  // then
			  assertEquals( InternalIndexState.FAILED, state );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportInitialStateAsOnlineIfPopulationCompletedSuccessfully()
		 public virtual void ShouldReportInitialStateAsOnlineIfPopulationCompletedSuccessfully()
		 {
			  // given
			  _provider = NewProvider();
			  IndexPopulator populator = _provider.getPopulator( Descriptor(), SamplingConfig(), heapBufferFactory(1024) );
			  populator.Create();
			  populator.Close( true );

			  // when
			  InternalIndexState state = _provider.getInitialState( Descriptor() );

			  // then
			  assertEquals( InternalIndexState.ONLINE, state );
		 }

		 /* storeMigrationParticipant */

		 protected internal abstract InternalIndexState ExpectedStateOnNonExistingSubIndex();

		 protected internal abstract Value SomeValue();

		 internal abstract IndexProvider NewProvider( PageCache pageCache, FileSystemAbstraction fs, IndexDirectoryStructure.Factory dir, IndexProvider.Monitor monitor, RecoveryCleanupWorkCollector collector, bool readOnly );

		 private IndexProvider NewProvider()
		 {
			  return NewProvider( PageCache(), Fs(), directoriesByProvider(BaseDir()), _monitor, immediate(), false );
		 }

		 private IndexProvider NewReadOnlyProvider()
		 {
			  return NewProvider( PageCache(), Fs(), directoriesByProvider(BaseDir()), _monitor, immediate(), true );
		 }

		 private IndexSamplingConfig SamplingConfig()
		 {
			  return new IndexSamplingConfig( Config.defaults() );
		 }

		 private StoreIndexDescriptor Descriptor()
		 {
			  return IndexDescriptorFactory.forSchema( forLabel( LABEL_ID, PROP_ID ), PROVIDER_DESCRIPTOR ).withId( INDEX_ID );
		 }

		 private StoreIndexDescriptor Descriptor( long indexId )
		 {
			  return IndexDescriptorFactory.forSchema( forLabel( LABEL_ID, PROP_ID ), PROVIDER_DESCRIPTOR ).withId( indexId );
		 }

		 private StoreIndexDescriptor DescriptorUnique()
		 {
			  return IndexDescriptorFactory.uniqueForSchema( forLabel( LABEL_ID, PROP_ID ), PROVIDER_DESCRIPTOR ).withId( INDEX_ID );
		 }

		 private PageCache PageCache()
		 {
			  return Rules.pageCache();
		 }

		 private FileSystemAbstraction Fs()
		 {
			  return Rules.fileSystem();
		 }

		 private File BaseDir()
		 {
			  return Rules.directory().absolutePath();
		 }
	}

}