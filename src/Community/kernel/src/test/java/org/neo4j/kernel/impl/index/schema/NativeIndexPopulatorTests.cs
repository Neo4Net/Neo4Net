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
namespace Neo4Net.Kernel.Impl.Index.Schema
{
	using RandomStringUtils = org.apache.commons.lang3.RandomStringUtils;
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using Exceptions = Neo4Net.Helpers.Exceptions;
	using Neo4Net.Index.@internal.gbptree;
	using Neo4Net.Index.@internal.gbptree;
	using InternalIndexState = Neo4Net.@internal.Kernel.Api.InternalIndexState;
	using OpenMode = Neo4Net.Io.fs.OpenMode;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using PagedFile = Neo4Net.Io.pagecache.PagedFile;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using Neo4Net.Kernel.Api.Index;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using NodePropertyAccessor = Neo4Net.Storageengine.Api.NodePropertyAccessor;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using IndexSample = Neo4Net.Storageengine.Api.schema.IndexSample;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.GBPTree.NO_HEADER_READER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.InternalIndexState.FAILED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.InternalIndexState.ONLINE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.InternalIndexState.POPULATING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.api.index.PhaseTracker_Fields.nullInstance;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.NativeIndexPopulator.BYTE_FAILED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.NativeIndexPopulator.BYTE_ONLINE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.NativeIndexPopulator.BYTE_POPULATING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.ValueCreatorUtil.countUniqueValues;

	public abstract class NativeIndexPopulatorTests<KEY, VALUE> : NativeIndexTestUtil<KEY, VALUE> where KEY : NativeIndexKey<KEY> where VALUE : NativeIndexValue
	{
		 private const int LARGE_AMOUNT_OF_UPDATES = 1_000;
		 internal static readonly NodePropertyAccessor NullPropertyAccessor = ( nodeId, propKeyId ) =>
		 {
		  throw new Exception( "Did not expect an attempt to go to store" );
		 };

		 internal NativeIndexPopulator<KEY, VALUE> Populator;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setupPopulator() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetupPopulator()
		 {
			  Populator = CreatePopulator();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract NativeIndexPopulator<KEY,VALUE> createPopulator() throws java.io.IOException;
		 internal abstract NativeIndexPopulator<KEY, VALUE> CreatePopulator();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createShouldCreateFile()
		 public virtual void CreateShouldCreateFile()
		 {
			  // given
			  assertFileNotPresent();

			  // when
			  Populator.create();

			  // then
			  assertFilePresent();
			  Populator.close( true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createShouldClearExistingFile() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CreateShouldClearExistingFile()
		 {
			  // given
			  sbyte[] someBytes = FileWithContent();

			  // when
			  Populator.create();

			  // then
			  using ( StoreChannel r = fs.open( IndexFile, OpenMode.READ ) )
			  {
					sbyte[] firstBytes = new sbyte[someBytes.Length];
					r.ReadAll( ByteBuffer.wrap( firstBytes ) );
					assertNotEquals( "Expected previous file content to have been cleared but was still there", someBytes, firstBytes );
			  }
			  Populator.close( true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void dropShouldDeleteExistingFile()
		 public virtual void DropShouldDeleteExistingFile()
		 {
			  // given
			  Populator.create();

			  // when
			  Populator.drop();

			  // then
			  assertFileNotPresent();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void dropShouldSucceedOnNonExistentFile()
		 public virtual void DropShouldSucceedOnNonExistentFile()
		 {
			  // given
			  assertFileNotPresent();

			  // when
			  Populator.drop();

			  // then
			  assertFileNotPresent();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addShouldHandleEmptyCollection() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AddShouldHandleEmptyCollection()
		 {
			  // given
			  Populator.create();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<org.neo4j.kernel.api.index.IndexEntryUpdate<?>> updates = java.util.Collections.emptyList();
			  IList<IndexEntryUpdate<object>> updates = Collections.emptyList();

			  // when
			  Populator.add( updates );
			  Populator.scanCompleted( nullInstance );

			  // then
			  Populator.close( true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addShouldApplyAllUpdatesOnce() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AddShouldApplyAllUpdatesOnce()
		 {
			  // given
			  Populator.create();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") org.neo4j.kernel.api.index.IndexEntryUpdate<org.neo4j.storageengine.api.schema.IndexDescriptor>[] updates = valueCreatorUtil.someUpdates(random);
			  IndexEntryUpdate<IndexDescriptor>[] updates = valueCreatorUtil.someUpdates( random );

			  // when
			  Populator.add( Arrays.asList( updates ) );
			  Populator.scanCompleted( nullInstance );

			  // then
			  Populator.close( true );
			  verifyUpdates( updates );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void updaterShouldApplyUpdates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UpdaterShouldApplyUpdates()
		 {
			  // given
			  Populator.create();
			  IndexEntryUpdate<IndexDescriptor>[] updates = valueCreatorUtil.someUpdates( random );
			  using ( IndexUpdater updater = Populator.newPopulatingUpdater( NullPropertyAccessor ) )
			  {
					// when
					foreach ( IndexEntryUpdate<IndexDescriptor> update in updates )
					{
						 updater.Process( update );
					}
			  }

			  // then
			  Populator.scanCompleted( nullInstance );
			  Populator.close( true );
			  verifyUpdates( updates );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void updaterMustThrowIfProcessAfterClose() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UpdaterMustThrowIfProcessAfterClose()
		 {
			  // given
			  Populator.create();
			  IndexUpdater updater = Populator.newPopulatingUpdater( NullPropertyAccessor );

			  // when
			  updater.Close();

			  // then
			  try
			  {
					updater.Process( valueCreatorUtil.add( 1, Values.of( long.MaxValue ) ) );
					fail( "Expected process to throw on closed updater" );
			  }
			  catch ( System.InvalidOperationException )
			  {
					// good
			  }
			  Populator.close( true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyInterleavedUpdatesFromAddAndUpdater() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldApplyInterleavedUpdatesFromAddAndUpdater()
		 {
			  // given
			  Populator.create();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") org.neo4j.kernel.api.index.IndexEntryUpdate<org.neo4j.storageengine.api.schema.IndexDescriptor>[] updates = valueCreatorUtil.someUpdates(random);
			  IndexEntryUpdate<IndexDescriptor>[] updates = valueCreatorUtil.someUpdates( random );

			  // when
			  ApplyInterleaved( updates, Populator );

			  // then
			  Populator.scanCompleted( nullInstance );
			  Populator.close( true );
			  verifyUpdates( updates );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void successfulCloseMustCloseGBPTree() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SuccessfulCloseMustCloseGBPTree()
		 {
			  // given
			  Populator.create();
			  Optional<PagedFile> existingMapping = pageCache.getExistingMapping( IndexFile );
			  if ( existingMapping.Present )
			  {
					existingMapping.get().close();
			  }
			  else
			  {
					fail( "Expected underlying GBPTree to have a mapping for this file" );
			  }

			  // when
			  Populator.close( true );

			  // then
			  existingMapping = pageCache.getExistingMapping( IndexFile );
			  assertFalse( existingMapping.Present );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void successfulCloseMustMarkIndexAsOnline() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SuccessfulCloseMustMarkIndexAsOnline()
		 {
			  // given
			  Populator.create();

			  // when
			  Populator.close( true );

			  // then
			  AssertHeader( ONLINE, null, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unsuccessfulCloseMustSucceedWithoutMarkAsFailed()
		 public virtual void UnsuccessfulCloseMustSucceedWithoutMarkAsFailed()
		 {
			  // given
			  Populator.create();

			  // then
			  Populator.close( false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unsuccessfulCloseMustCloseGBPTree() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UnsuccessfulCloseMustCloseGBPTree()
		 {
			  // given
			  Populator.create();
			  Optional<PagedFile> existingMapping = pageCache.getExistingMapping( IndexFile );
			  if ( existingMapping.Present )
			  {
					existingMapping.get().close();
			  }
			  else
			  {
					fail( "Expected underlying GBPTree to have a mapping for this file" );
			  }

			  // when
			  Populator.close( false );

			  // then
			  existingMapping = pageCache.getExistingMapping( IndexFile );
			  assertFalse( existingMapping.Present );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unsuccessfulCloseMustNotMarkIndexAsOnline() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UnsuccessfulCloseMustNotMarkIndexAsOnline()
		 {
			  // given
			  Populator.create();

			  // when
			  Populator.close( false );

			  // then
			  AssertHeader( POPULATING, null, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void closeMustWriteFailureMessageAfterMarkedAsFailed() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CloseMustWriteFailureMessageAfterMarkedAsFailed()
		 {
			  // given
			  Populator.create();

			  // when
			  string failureMessage = "Fly, you fools!";
			  Populator.markAsFailed( failureMessage );
			  Populator.close( false );

			  // then
			  AssertHeader( FAILED, failureMessage, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void closeMustWriteFailureMessageAfterMarkedAsFailedWithLongMessage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CloseMustWriteFailureMessageAfterMarkedAsFailedWithLongMessage()
		 {
			  // given
			  Populator.create();

			  // when
			  string failureMessage = LongString( pageCache.pageSize() );
			  Populator.markAsFailed( failureMessage );
			  Populator.close( false );

			  // then
			  AssertHeader( FAILED, failureMessage, true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void successfulCloseMustThrowIfMarkedAsFailed()
		 public virtual void SuccessfulCloseMustThrowIfMarkedAsFailed()
		 {
			  // given
			  Populator.create();

			  // when
			  Populator.markAsFailed( "" );

			  // then
			  try
			  {
					Populator.close( true );
					fail( "Expected successful close to fail after markedAsFailed" );
			  }
			  catch ( Exception e )
			  {
					// then good
					assertTrue( "Expected cause to contain " + typeof( System.InvalidOperationException ), Exceptions.contains( e, typeof( System.InvalidOperationException ) ) );
			  }
			  Populator.close( false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyLargeAmountOfInterleavedRandomUpdates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldApplyLargeAmountOfInterleavedRandomUpdates()
		 {
			  // given
			  Populator.create();
			  random.reset();
			  Random updaterRandom = new Random( random.seed() );
			  IEnumerator<IndexEntryUpdate<IndexDescriptor>> updates = valueCreatorUtil.randomUpdateGenerator( random );

			  // when
			  int count = InterleaveLargeAmountOfUpdates( updaterRandom, updates );

			  // then
			  Populator.scanCompleted( nullInstance );
			  Populator.close( true );
			  random.reset();
			  VerifyUpdates( valueCreatorUtil.randomUpdateGenerator( random ), count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void dropMustSucceedAfterSuccessfulClose()
		 public virtual void DropMustSucceedAfterSuccessfulClose()
		 {
			  // given
			  Populator.create();
			  Populator.close( true );

			  // when
			  Populator.drop();

			  // then
			  assertFileNotPresent();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void dropMustSucceedAfterUnsuccessfulClose()
		 public virtual void DropMustSucceedAfterUnsuccessfulClose()
		 {
			  // given
			  Populator.create();
			  Populator.close( false );

			  // when
			  Populator.drop();

			  // then
			  assertFileNotPresent();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void successfulCloseMustThrowWithoutPriorSuccessfulCreate()
		 public virtual void SuccessfulCloseMustThrowWithoutPriorSuccessfulCreate()
		 {
			  // given
			  assertFileNotPresent();

			  // when
			  try
			  {
					Populator.close( true );
					fail( "Should have failed" );
			  }
			  catch ( Exception e )
			  {
					// then good
					assertTrue( "Expected cause to contain " + typeof( System.InvalidOperationException ), Exceptions.contains( e, typeof( System.InvalidOperationException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unsuccessfulCloseMustSucceedWithoutSuccessfulPriorCreate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UnsuccessfulCloseMustSucceedWithoutSuccessfulPriorCreate()
		 {
			  // given
			  assertFileNotPresent();
			  string failureMessage = "There is no spoon";
			  Populator.markAsFailed( failureMessage );

			  // when
			  Populator.close( false );

			  // then
			  AssertHeader( FAILED, failureMessage, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void successfulCloseMustThrowAfterDrop()
		 public virtual void SuccessfulCloseMustThrowAfterDrop()
		 {
			  // given
			  Populator.create();

			  // when
			  Populator.drop();

			  // then
			  try
			  {
					Populator.close( true );
					fail( "Should have failed" );
			  }
			  catch ( Exception e )
			  {
					// then good
					assertTrue( "Expected cause to contain " + typeof( System.InvalidOperationException ), Exceptions.contains( e, typeof( System.InvalidOperationException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unsuccessfulCloseMustThrowAfterDrop()
		 public virtual void UnsuccessfulCloseMustThrowAfterDrop()
		 {
			  // given
			  Populator.create();

			  // when
			  Populator.drop();

			  // then
			  try
			  {
					Populator.close( false );
					fail( "Should have failed" );
			  }
			  catch ( Exception e )
			  {
					// then good
					assertTrue( "Expected cause to contain " + typeof( System.InvalidOperationException ), Exceptions.contains( e, typeof( System.InvalidOperationException ) ) );
			  }
		 }

		 public abstract class Unique<K, V> : NativeIndexPopulatorTests<K, V> where K : NativeIndexKey<K> where V : NativeIndexValue
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addShouldThrowOnDuplicateValues()
			  public virtual void AddShouldThrowOnDuplicateValues()
			  {
					// given
					outerInstance.populator.create();
					IndexEntryUpdate<IndexDescriptor>[] updates = valueCreatorUtil.someUpdatesWithDuplicateValues( random );

					// when
					try
					{
						 outerInstance.populator.add( Arrays.asList( updates ) );
						 outerInstance.populator.scanCompleted( nullInstance );
						 fail( "Updates should have conflicted" );
					}
					catch ( IndexEntryConflictException )
					{
						 // then good
					}
					finally
					{
						 outerInstance.populator.close( true );
					}
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void updaterShouldThrowOnDuplicateValues() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void UpdaterShouldThrowOnDuplicateValues()
			  {
					// given
					outerInstance.populator.create();
					IndexEntryUpdate<IndexDescriptor>[] updates = valueCreatorUtil.someUpdatesWithDuplicateValues( random );
					IndexUpdater updater = outerInstance.populator.newPopulatingUpdater( NullPropertyAccessor );

					// when
					foreach ( IndexEntryUpdate<IndexDescriptor> update in updates )
					{
						 updater.Process( update );
					}
					try
					{
						 updater.Close();
						 outerInstance.populator.scanCompleted( nullInstance );
						 fail( "Updates should have conflicted" );
					}
					catch ( Exception e )
					{
						 // then
						 assertTrue( e.Message, Exceptions.contains( e, typeof( IndexEntryConflictException ) ) );
					}
					finally
					{
						 outerInstance.populator.close( true );
					}
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSampleUpdates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void ShouldSampleUpdates()
			  {
					// GIVEN
					outerInstance.populator.create();
					IndexEntryUpdate<IndexDescriptor>[] updates = valueCreatorUtil.someUpdates( random );

					// WHEN
					outerInstance.populator.add( asList( updates ) );
					foreach ( IndexEntryUpdate<IndexDescriptor> update in updates )
					{
						 outerInstance.populator.includeSample( update );
					}
					outerInstance.populator.scanCompleted( nullInstance );
					IndexSample sample = outerInstance.populator.sampleResult();

					// THEN
					assertEquals( updates.Length, sample.SampleSize() );
					assertEquals( updates.Length, sample.UniqueValues() );
					assertEquals( updates.Length, sample.IndexSize() );
					outerInstance.populator.close( true );
			  }
		 }

		 public abstract class NonUnique<K, V> : NativeIndexPopulatorTests<K, V> where K : NativeIndexKey<K> where V : NativeIndexValue
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addShouldApplyDuplicateValues() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void AddShouldApplyDuplicateValues()
			  {
					// given
					outerInstance.populator.create();
					IndexEntryUpdate<IndexDescriptor>[] updates = valueCreatorUtil.someUpdatesWithDuplicateValues( random );

					// when
					outerInstance.populator.add( Arrays.asList( updates ) );

					// then
					outerInstance.populator.scanCompleted( nullInstance );
					outerInstance.populator.close( true );
					verifyUpdates( updates );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void updaterShouldApplyDuplicateValues() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void UpdaterShouldApplyDuplicateValues()
			  {
					// given
					outerInstance.populator.create();
					IndexEntryUpdate<IndexDescriptor>[] updates = valueCreatorUtil.someUpdatesWithDuplicateValues( random );
					using ( IndexUpdater updater = outerInstance.populator.newPopulatingUpdater( NullPropertyAccessor ) )
					{
						 // when
						 foreach ( IndexEntryUpdate<IndexDescriptor> update in updates )
						 {
							  updater.Process( update );
						 }
					}

					// then
					outerInstance.populator.scanCompleted( nullInstance );
					outerInstance.populator.close( true );
					verifyUpdates( updates );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSampleUpdatesIfConfiguredForOnlineSampling() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void ShouldSampleUpdatesIfConfiguredForOnlineSampling()
			  {
					// GIVEN
					try
					{
						 outerInstance.populator.create();
						 IndexEntryUpdate<IndexDescriptor>[] scanUpdates = valueCreatorUtil.someUpdates( random );
						 outerInstance.populator.add( Arrays.asList( scanUpdates ) );
						 IEnumerator<IndexEntryUpdate<IndexDescriptor>> generator = valueCreatorUtil.randomUpdateGenerator( random );
						 Value[] updates = new Value[5];
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 updates[0] = generator.next().values()[0];
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 updates[1] = generator.next().values()[0];
						 updates[2] = updates[1];
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 updates[3] = generator.next().values()[0];
						 updates[4] = updates[3];
						 using ( IndexUpdater updater = outerInstance.populator.newPopulatingUpdater( NullPropertyAccessor ) )
						 {
							  long nodeId = 1000;
							  foreach ( Value value in updates )
							  {
									IndexEntryUpdate<IndexDescriptor> update = valueCreatorUtil.add( nodeId++, value );
									updater.Process( update );
							  }
						 }

						 // WHEN
						 outerInstance.populator.scanCompleted( nullInstance );
						 IndexSample sample = outerInstance.populator.sampleResult();

						 // THEN
						 Value[] allValues = Arrays.copyOf( updates, updates.Length + scanUpdates.Length );
						 Array.Copy( AsValues( scanUpdates ), 0, allValues, updates.Length, scanUpdates.Length );
						 assertEquals( updates.Length + scanUpdates.Length, sample.SampleSize() );
						 assertEquals( countUniqueValues( allValues ), sample.UniqueValues() );
						 assertEquals( updates.Length + scanUpdates.Length, sample.IndexSize() );
					}
					finally
					{
						 outerInstance.populator.close( true );
					}
			  }

			  internal virtual Value[] AsValues( IndexEntryUpdate<IndexDescriptor>[] updates )
			  {
					Value[] values = new Value[updates.Length];
					for ( int i = 0; i < updates.Length; i++ )
					{
						 values[i] = updates[i].Values()[0];
					}
					return values;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int interleaveLargeAmountOfUpdates(java.util.Random updaterRandom, java.util.Iterator<org.neo4j.kernel.api.index.IndexEntryUpdate<org.neo4j.storageengine.api.schema.IndexDescriptor>> updates) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 private int InterleaveLargeAmountOfUpdates( Random updaterRandom, IEnumerator<IndexEntryUpdate<IndexDescriptor>> updates )
		 {
			  int count = 0;
			  for ( int i = 0; i < LARGE_AMOUNT_OF_UPDATES; i++ )
			  {
					if ( updaterRandom.nextFloat() < 0.1 )
					{
						 using ( IndexUpdater indexUpdater = Populator.newPopulatingUpdater( NullPropertyAccessor ) )
						 {
							  int numberOfUpdaterUpdates = updaterRandom.Next( 100 );
							  for ( int j = 0; j < numberOfUpdaterUpdates; j++ )
							  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
									indexUpdater.Process( updates.next() );
									count++;
							  }
						 }
					}
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					Populator.add( Collections.singletonList( updates.next() ) );
					count++;
			  }
			  return count;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertHeader(org.neo4j.internal.kernel.api.InternalIndexState expectedState, String failureMessage, boolean messageTruncated) throws java.io.IOException
		 private void AssertHeader( InternalIndexState expectedState, string failureMessage, bool messageTruncated )
		 {
			  NativeIndexHeaderReader headerReader = new NativeIndexHeaderReader( NO_HEADER_READER );
			  using ( GBPTree<KEY, VALUE> ignored = ( new GBPTreeBuilder<KEY, VALUE>( pageCache, IndexFile, layout ) ).with( headerReader ).build() )
			  {
					switch ( expectedState )
					{
					case InternalIndexState.ONLINE:
						 assertEquals( "Index was not marked as online when expected not to be.", BYTE_ONLINE, headerReader.State );
						 assertNull( "Expected failure message to be null when marked as online.", headerReader.FailureMessage );
						 break;
					case InternalIndexState.FAILED:
						 assertEquals( "Index was marked as online when expected not to be.", BYTE_FAILED, headerReader.State );
						 if ( messageTruncated )
						 {
							  assertTrue( headerReader.FailureMessage.Length < failureMessage.Length );
							  assertTrue( failureMessage.StartsWith( headerReader.FailureMessage, StringComparison.Ordinal ) );
						 }
						 else
						 {
							  assertEquals( failureMessage, headerReader.FailureMessage );
						 }
						 break;
					case InternalIndexState.POPULATING:
						 assertEquals( "Index was not left as populating when expected to be.", BYTE_POPULATING, headerReader.State );
						 assertNull( "Expected failure message to be null when marked as populating.", headerReader.FailureMessage );
						 break;
					default:
						 throw new System.NotSupportedException( "Unexpected index state " + expectedState );
					}
			  }
		 }

		 private string LongString( int length )
		 {
			  return RandomStringUtils.random( length, true, true );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void applyInterleaved(org.neo4j.kernel.api.index.IndexEntryUpdate<org.neo4j.storageengine.api.schema.IndexDescriptor>[] updates, NativeIndexPopulator<KEY,VALUE> populator) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 private void ApplyInterleaved( IndexEntryUpdate<IndexDescriptor>[] updates, NativeIndexPopulator<KEY, VALUE> populator )
		 {
			  bool useUpdater = true;
			  ICollection<IndexEntryUpdate<IndexDescriptor>> populatorBatch = new List<IndexEntryUpdate<IndexDescriptor>>();
			  IndexUpdater updater = populator.NewPopulatingUpdater( NullPropertyAccessor );
			  foreach ( IndexEntryUpdate<IndexDescriptor> update in updates )
			  {
					if ( random.Next( 100 ) < 20 )
					{
						 if ( useUpdater )
						 {
							  updater.Close();
							  populatorBatch = new List<IndexEntryUpdate<IndexDescriptor>>();
						 }
						 else
						 {
							  populator.Add( populatorBatch );
							  updater = populator.NewPopulatingUpdater( NullPropertyAccessor );
						 }
						 useUpdater = !useUpdater;
					}
					if ( useUpdater )
					{
						 updater.Process( update );
					}
					else
					{
						 populatorBatch.Add( update );
					}
			  }
			  if ( useUpdater )
			  {
					updater.Close();
			  }
			  else
			  {
					populator.Add( populatorBatch );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyUpdates(java.util.Iterator<org.neo4j.kernel.api.index.IndexEntryUpdate<org.neo4j.storageengine.api.schema.IndexDescriptor>> indexEntryUpdateIterator, int count) throws java.io.IOException
		 private void VerifyUpdates( IEnumerator<IndexEntryUpdate<IndexDescriptor>> indexEntryUpdateIterator, int count )
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") org.neo4j.kernel.api.index.IndexEntryUpdate<org.neo4j.storageengine.api.schema.IndexDescriptor>[] updates = new org.neo4j.kernel.api.index.IndexEntryUpdate[count];
			  IndexEntryUpdate<IndexDescriptor>[] updates = new IndexEntryUpdate[count];
			  for ( int i = 0; i < count; i++ )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					updates[i] = indexEntryUpdateIterator.next();
			  }
			  verifyUpdates( updates );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private byte[] fileWithContent() throws java.io.IOException
		 private sbyte[] FileWithContent()
		 {
			  int size = 1000;
			  fs.mkdirs( IndexFile.ParentFile );
			  using ( StoreChannel storeChannel = fs.create( IndexFile ) )
			  {
					sbyte[] someBytes = new sbyte[size];
					random.NextBytes( someBytes );
					storeChannel.WriteAll( ByteBuffer.wrap( someBytes ) );
					return someBytes;
			  }
		 }
	}

}