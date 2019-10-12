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
namespace Org.Neo4j.Kernel.Impl.Index.Schema
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using EphemeralFileSystemAbstraction = Org.Neo4j.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using IndexProviderDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.IndexProviderDescriptor;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using IndexEntryConflictException = Org.Neo4j.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using IndexDirectoryStructure = Org.Neo4j.Kernel.Api.Index.IndexDirectoryStructure;
	using Org.Neo4j.Kernel.Api.Index;
	using IndexUpdater = Org.Neo4j.Kernel.Api.Index.IndexUpdater;
	using SchemaDescriptorFactory = Org.Neo4j.Kernel.api.schema.SchemaDescriptorFactory;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using ConfiguredSpaceFillingCurveSettingsCache = Org.Neo4j.Kernel.Impl.Index.Schema.config.ConfiguredSpaceFillingCurveSettingsCache;
	using IndexSpecificSpaceFillingCurveSettingsCache = Org.Neo4j.Kernel.Impl.Index.Schema.config.IndexSpecificSpaceFillingCurveSettingsCache;
	using LocalMemoryTracker = Org.Neo4j.Memory.LocalMemoryTracker;
	using ThreadSafePeakMemoryAllocationTracker = Org.Neo4j.Memory.ThreadSafePeakMemoryAllocationTracker;
	using IndexDescriptorFactory = Org.Neo4j.Storageengine.Api.schema.IndexDescriptorFactory;
	using PopulationProgress = Org.Neo4j.Storageengine.Api.schema.PopulationProgress;
	using StoreIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.StoreIndexDescriptor;
	using Barrier = Org.Neo4j.Test.Barrier;
	using Race = Org.Neo4j.Test.Race;
	using PageCacheAndDependenciesRule = Org.Neo4j.Test.rule.PageCacheAndDependenciesRule;
	using Org.Neo4j.Test.rule.concurrent;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.index.IndexDirectoryStructure.directoriesByProvider;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.index.IndexProvider.Monitor_Fields.EMPTY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.api.index.PhaseTracker_Fields.nullInstance;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.BlockStorage.Monitor_Fields.NO_MONITOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.ByteBufferFactory.heapBufferFactory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.OtherThreadExecutor.command;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.Race.throwing;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringValue;

	public class BlockBasedIndexPopulatorTest
	{
		 private static readonly StoreIndexDescriptor _indexDescriptor = IndexDescriptorFactory.forSchema( SchemaDescriptorFactory.forLabel( 1, 1 ) ).withId( 1 );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.PageCacheAndDependenciesRule storage = new org.neo4j.test.rule.PageCacheAndDependenciesRule();
		 public readonly PageCacheAndDependenciesRule Storage = new PageCacheAndDependenciesRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.concurrent.OtherThreadRule<Void> t2 = new org.neo4j.test.rule.concurrent.OtherThreadRule<>("MERGER");
		 public readonly OtherThreadRule<Void> T2 = new OtherThreadRule<Void>( "MERGER" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.concurrent.OtherThreadRule<Void> t3 = new org.neo4j.test.rule.concurrent.OtherThreadRule<>("CLOSER");
		 public readonly OtherThreadRule<Void> T3 = new OtherThreadRule<Void>( "CLOSER" );

		 private IndexDirectoryStructure _directoryStructure;
		 private File _indexDir;
		 private File _indexFile;
		 private FileSystemAbstraction _fs;
		 private IndexDropAction _dropAction;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  IndexProviderDescriptor providerDescriptor = new IndexProviderDescriptor( "test", "v1" );
			  _directoryStructure = directoriesByProvider( Storage.directory().databaseDir() ).forProvider(providerDescriptor);
			  _indexDir = _directoryStructure.directoryForIndex( _indexDescriptor.Id );
			  _indexFile = new File( _indexDir, "index" );
			  _fs = Storage.fileSystem();
			  _dropAction = new FileSystemIndexDropAction( _fs, _directoryStructure );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAwaitMergeToBeFullyAbortedBeforeLeavingCloseMethod() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAwaitMergeToBeFullyAbortedBeforeLeavingCloseMethod()
		 {
			  // given
			  TrappingMonitor monitor = new TrappingMonitor( ignore => false );
			  BlockBasedIndexPopulator<GenericKey, NativeIndexValue> populator = InstantiatePopulator( monitor );
			  bool closed = false;
			  try
			  {
					populator.Add( BatchOfUpdates() );

					// when starting to merge (in a separate thread)
					Future<object> mergeFuture = T2.execute( command( () => populator.scanCompleted(nullInstance) ) );
					// and waiting for merge to get going
					monitor.Barrier.awaitUninterruptibly();
					// calling close here should wait for the merge future, so that checking the merge future for "done" immediately afterwards must say true
					Future<object> closeFuture = T3.execute( command( () => populator.close(false) ) );
					T3.get().waitUntilWaiting();
					monitor.Barrier.release();
					closeFuture.get();
					closed = true;

					// then
					assertTrue( mergeFuture.Done );
			  }
			  finally
			  {
					if ( !closed )
					{
						 populator.Close( true );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleBeingAbortedWhileMerging() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleBeingAbortedWhileMerging()
		 {
			  // given
			  TrappingMonitor monitor = new TrappingMonitor( numberOfBlocks => numberOfBlocks == 2 );
			  BlockBasedIndexPopulator<GenericKey, NativeIndexValue> populator = InstantiatePopulator( monitor );
			  bool closed = false;
			  try
			  {
					populator.Add( BatchOfUpdates() );

					// when starting to merge (in a separate thread)
					Future<object> mergeFuture = T2.execute( command( () => populator.scanCompleted(nullInstance) ) );
					// and waiting for merge to get going
					monitor.Barrier.await();
					monitor.Barrier.release();
					monitor.MergeFinishedBarrier.awaitUninterruptibly();
					// calling close here should wait for the merge future, so that checking the merge future for "done" immediately afterwards must say true
					Future<object> closeFuture = T3.execute( command( () => populator.close(false) ) );
					T3.get().waitUntilWaiting();
					monitor.MergeFinishedBarrier.release();
					closeFuture.get();
					closed = true;

					// then let's make sure scanComplete was cancelled, not throwing exception or anything.
					mergeFuture.get();
			  }
			  finally
			  {
					if ( !closed )
					{
						 populator.Close( false );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportAccurateProgressThroughoutThePhases() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportAccurateProgressThroughoutThePhases()
		 {
			  // given
			  TrappingMonitor monitor = new TrappingMonitor( numberOfBlocks => numberOfBlocks == 1 );
			  BlockBasedIndexPopulator<GenericKey, NativeIndexValue> populator = InstantiatePopulator( monitor );
			  try
			  {
					populator.Add( BatchOfUpdates() );

					// when starting to merge (in a separate thread)
					Future<object> mergeFuture = T2.execute( command( () => populator.scanCompleted(nullInstance) ) );
					// and waiting for merge to get going
					monitor.Barrier.awaitUninterruptibly();
					// this is a bit fuzzy, but what we want is to assert that the scan doesn't represent 100% of the work
					assertEquals( 0.5f, populator.Progress( Org.Neo4j.Storageengine.Api.schema.PopulationProgress_Fields.Done ).Progress, 0.1f );
					monitor.Barrier.release();
					monitor.MergeFinishedBarrier.awaitUninterruptibly();
					assertEquals( 0.7f, populator.Progress( Org.Neo4j.Storageengine.Api.schema.PopulationProgress_Fields.Done ).Progress, 0.1f );
					monitor.MergeFinishedBarrier.release();
					mergeFuture.get();
					assertEquals( 1f, populator.Progress( Org.Neo4j.Storageengine.Api.schema.PopulationProgress_Fields.Done ).Progress, 0f );
			  }
			  finally
			  {
					populator.Close( true );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCorrectlyDecideToAwaitMergeDependingOnProgress() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCorrectlyDecideToAwaitMergeDependingOnProgress()
		 {
			  // given
			  BlockBasedIndexPopulator<GenericKey, NativeIndexValue> populator = InstantiatePopulator( NO_MONITOR );
			  bool closed = false;
			  try
			  {
					populator.Add( BatchOfUpdates() );

					// when
					Race race = new Race();
					race.AddContestant( throwing( () => populator.scanCompleted(nullInstance) ) );
					race.AddContestant( throwing( () => populator.close(false) ) );
					race.Go();
					closed = true;

					// then regardless of who wins (close/merge) after close call returns no files should still be mapped
					EphemeralFileSystemAbstraction ephemeralFileSystem = ( EphemeralFileSystemAbstraction ) _fs;
					ephemeralFileSystem.AssertNoOpenFiles();
			  }
			  finally
			  {
					if ( !closed )
					{
						 populator.Close( true );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDeleteDirectoryOnDrop() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDeleteDirectoryOnDrop()
		 {
			  // given
			  TrappingMonitor monitor = new TrappingMonitor( ignore => false );
			  BlockBasedIndexPopulator<GenericKey, NativeIndexValue> populator = InstantiatePopulator( monitor );
			  bool closed = false;
			  try
			  {
					populator.Add( BatchOfUpdates() );

					// when starting to merge (in a separate thread)
					Future<object> mergeFuture = T2.execute( command( () => populator.scanCompleted(nullInstance) ) );
					// and waiting for merge to get going
					monitor.Barrier.awaitUninterruptibly();
					// calling drop here should wait for the merge future and then delete index directory
					assertTrue( _fs.fileExists( _indexDir ) );
					assertTrue( _fs.isDirectory( _indexDir ) );
					assertTrue( _fs.listFiles( _indexDir ).Length > 0 );

					Future<object> dropFuture = T3.execute( command( populator.drop ) );
					T3.get().waitUntilWaiting();
					monitor.Barrier.release();
					dropFuture.get();
					closed = true;

					// then
					assertTrue( mergeFuture.Done );
					assertFalse( _fs.fileExists( _indexDir ) );
			  }
			  finally
			  {
					if ( !closed )
					{
						 populator.Close( true );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDeallocateAllAllocatedMemoryOnClose() throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDeallocateAllAllocatedMemoryOnClose()
		 {
			  // given
			  ThreadSafePeakMemoryAllocationTracker memoryTracker = new ThreadSafePeakMemoryAllocationTracker( new LocalMemoryTracker() );
			  ByteBufferFactory bufferFactory = new ByteBufferFactory( () => new UnsafeDirectByteBufferAllocator(memoryTracker), 100 );
			  BlockBasedIndexPopulator<GenericKey, NativeIndexValue> populator = InstantiatePopulator( NO_MONITOR, bufferFactory );
			  bool closed = false;
			  try
			  {
					// when
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Collection<org.neo4j.kernel.api.index.IndexEntryUpdate<?>> updates = batchOfUpdates();
					ICollection<IndexEntryUpdate<object>> updates = BatchOfUpdates();
					populator.Add( updates );
					int nextId = updates.Count;
					ExternalUpdates( populator, nextId, nextId + 10 );
					nextId = nextId + 10;
					long memoryBeforeScanCompleted = memoryTracker.UsedDirectMemory();
					populator.ScanCompleted( nullInstance );
					ExternalUpdates( populator, nextId, nextId + 10 );

					// then
					assertTrue( "expected some memory to have been temporarily allocated in scanCompleted", memoryTracker.PeakMemoryUsage() > memoryBeforeScanCompleted );
					populator.Close( true );
					assertEquals( "expected all allocated memory to have been freed on close", memoryBeforeScanCompleted, memoryTracker.UsedDirectMemory() );
					closed = true;

					bufferFactory.Close();
					assertEquals( 0, memoryTracker.UsedDirectMemory() );
			  }
			  finally
			  {
					if ( !closed )
					{
						 populator.Close( true );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDeallocateAllAllocatedMemoryOnDrop() throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDeallocateAllAllocatedMemoryOnDrop()
		 {
			  // given
			  ThreadSafePeakMemoryAllocationTracker memoryTracker = new ThreadSafePeakMemoryAllocationTracker( new LocalMemoryTracker() );
			  ByteBufferFactory bufferFactory = new ByteBufferFactory( () => new UnsafeDirectByteBufferAllocator(memoryTracker), 100 );
			  BlockBasedIndexPopulator<GenericKey, NativeIndexValue> populator = InstantiatePopulator( NO_MONITOR, bufferFactory );
			  bool closed = false;
			  try
			  {
					// when
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Collection<org.neo4j.kernel.api.index.IndexEntryUpdate<?>> updates = batchOfUpdates();
					ICollection<IndexEntryUpdate<object>> updates = BatchOfUpdates();
					populator.Add( updates );
					int nextId = updates.Count;
					ExternalUpdates( populator, nextId, nextId + 10 );
					nextId = nextId + 10;
					long memoryBeforeScanCompleted = memoryTracker.UsedDirectMemory();
					populator.ScanCompleted( nullInstance );
					ExternalUpdates( populator, nextId, nextId + 10 );

					// then
					assertTrue( "expected some memory to have been temporarily allocated in scanCompleted", memoryTracker.PeakMemoryUsage() > memoryBeforeScanCompleted );
					populator.Drop();
					closed = true;
					assertEquals( "expected all allocated memory to have been freed on drop", memoryBeforeScanCompleted, memoryTracker.UsedDirectMemory() );

					bufferFactory.Close();
					assertEquals( 0, memoryTracker.UsedDirectMemory() );
			  }
			  finally
			  {
					if ( !closed )
					{
						 populator.Close( true );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void externalUpdates(BlockBasedIndexPopulator<GenericKey,NativeIndexValue> populator, int firstId, int lastId) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 private void ExternalUpdates( BlockBasedIndexPopulator<GenericKey, NativeIndexValue> populator, int firstId, int lastId )
		 {
			  using ( IndexUpdater updater = populator.NewPopulatingUpdater() )
			  {
					for ( int i = firstId; i < lastId; i++ )
					{
						 updater.Process( Add( i ) );
					}
			  }
		 }

		 private BlockBasedIndexPopulator<GenericKey, NativeIndexValue> InstantiatePopulator( BlockStorage.Monitor monitor )
		 {
			  return InstantiatePopulator( monitor, heapBufferFactory( 100 ) );
		 }

		 private BlockBasedIndexPopulator<GenericKey, NativeIndexValue> InstantiatePopulator( BlockStorage.Monitor monitor, ByteBufferFactory bufferFactory )
		 {
			  Config config = Config.defaults();
			  ConfiguredSpaceFillingCurveSettingsCache settingsCache = new ConfiguredSpaceFillingCurveSettingsCache( config );
			  IndexSpecificSpaceFillingCurveSettingsCache spatialSettings = new IndexSpecificSpaceFillingCurveSettingsCache( settingsCache, new Dictionary<Org.Neo4j.Values.Storable.CoordinateReferenceSystem, SpaceFillingCurveSettings>() );
			  GenericLayout layout = new GenericLayout( 1, spatialSettings );
			  BlockBasedIndexPopulator<GenericKey, NativeIndexValue> populator = new BlockBasedIndexPopulatorAnonymousInnerClass( this, Storage.pageCache(), _fs, _indexFile, layout, EMPTY, _indexDescriptor, spatialSettings, _directoryStructure, _dropAction, bufferFactory, monitor );
			  populator.Create();
			  return populator;
		 }

		 private class BlockBasedIndexPopulatorAnonymousInnerClass : BlockBasedIndexPopulator<GenericKey, NativeIndexValue>
		 {
			 private readonly BlockBasedIndexPopulatorTest _outerInstance;

			 public BlockBasedIndexPopulatorAnonymousInnerClass( BlockBasedIndexPopulatorTest outerInstance, Org.Neo4j.Io.pagecache.PageCache pageCache, FileSystemAbstraction fs, File indexFile, Org.Neo4j.Kernel.Impl.Index.Schema.GenericLayout layout, UnknownType empty, StoreIndexDescriptor indexDescriptor, IndexSpecificSpaceFillingCurveSettingsCache spatialSettings, IndexDirectoryStructure directoryStructure, Org.Neo4j.Kernel.Impl.Index.Schema.IndexDropAction dropAction, Org.Neo4j.Kernel.Impl.Index.Schema.ByteBufferFactory bufferFactory, Org.Neo4j.Kernel.Impl.Index.Schema.BlockStorage.Monitor monitor ) : base( pageCache, fs, indexFile, layout, empty, indexDescriptor, spatialSettings, directoryStructure, dropAction, false, bufferFactory, 2, monitor )
			 {
				 this.outerInstance = outerInstance;
			 }

			 internal override NativeIndexReader<GenericKey, NativeIndexValue> newReader()
			 {
				  throw new System.NotSupportedException( "Not needed in this test" );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private static java.util.Collection<org.neo4j.kernel.api.index.IndexEntryUpdate<?>> batchOfUpdates()
		 private static ICollection<IndexEntryUpdate<object>> BatchOfUpdates()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<org.neo4j.kernel.api.index.IndexEntryUpdate<?>> updates = new java.util.ArrayList<>();
			  IList<IndexEntryUpdate<object>> updates = new List<IndexEntryUpdate<object>>();
			  for ( int i = 0; i < 50; i++ )
			  {
					updates.Add( Add( i ) );
			  }
			  return updates;
		 }

		 private static IndexEntryUpdate<StoreIndexDescriptor> Add( int i )
		 {
			  return IndexEntryUpdate.add( i, _indexDescriptor, stringValue( "Value" + i ) );
		 }

		 private class TrappingMonitor : BlockStorage.Monitor_Adapter
		 {
			  internal readonly Org.Neo4j.Test.Barrier_Control Barrier = new Org.Neo4j.Test.Barrier_Control();
			  internal readonly Org.Neo4j.Test.Barrier_Control MergeFinishedBarrier = new Org.Neo4j.Test.Barrier_Control();
			  internal readonly System.Func<long, bool> TrapForMergeIterationFinished;

			  internal TrappingMonitor( System.Func<long, bool> trapForMergeIterationFinished )
			  {
					this.TrapForMergeIterationFinished = trapForMergeIterationFinished;
			  }

			  public override void MergedBlocks( long resultingBlockSize, long resultingEntryCount, long numberOfBlocks )
			  {
					Barrier.reached();
			  }

			  public override void MergeIterationFinished( long numberOfBlocksBefore, long numberOfBlocksAfter )
			  {
					if ( TrapForMergeIterationFinished.test( numberOfBlocksAfter ) )
					{
						 MergeFinishedBarrier.reached();
					}
			  }
		 }
	}

}