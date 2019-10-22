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
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ConfiguredSpaceFillingCurveSettingsCache = Neo4Net.Kernel.Impl.Index.Schema.config.ConfiguredSpaceFillingCurveSettingsCache;
	using IndexSpecificSpaceFillingCurveSettingsCache = Neo4Net.Kernel.Impl.Index.Schema.config.IndexSpecificSpaceFillingCurveSettingsCache;
	using LocalMemoryTracker = Neo4Net.Memory.LocalMemoryTracker;
	using Inject = Neo4Net.Test.extension.Inject;
	using RandomExtension = Neo4Net.Test.extension.RandomExtension;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using Value = Neo4Net.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.schema.ByteBufferFactory.HEAP_ALLOCATOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.schema.NativeIndexKey.Inclusion.NEUTRAL;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith({TestDirectoryExtension.class, RandomExtension.class}) class IndexKeyStorageTest
	internal class IndexKeyStorageTest
	{
		 private const int BLOCK_SIZE = 2000;
		 private static readonly IndexSpecificSpaceFillingCurveSettingsCache _spatialSettings = new IndexSpecificSpaceFillingCurveSettingsCache( new ConfiguredSpaceFillingCurveSettingsCache( Config.defaults() ), new Dictionary<Neo4Net.Values.Storable.CoordinateReferenceSystem, SpaceFillingCurveSettings>() );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject protected org.Neo4Net.test.rule.TestDirectory directory;
		 protected internal TestDirectory Directory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject protected org.Neo4Net.test.rule.RandomRule random;
		 protected internal RandomRule Random;

		 private GenericLayout _layout;
		 private int _numberOfSlots;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void createLayout()
		 internal virtual void CreateLayout()
		 {
			  this._numberOfSlots = Random.Next( 1, 3 );
			  this._layout = new GenericLayout( _numberOfSlots, _spatialSettings );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldAddAndReadZeroKey() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldAddAndReadZeroKey()
		 {
			  using ( IndexKeyStorage<GenericKey> keyStorage = keyStorage() )
			  {
					keyStorage.doneAdding();
					using ( IndexKeyStorage.KeyEntryCursor<GenericKey> reader = keyStorage.reader() )
					{
						 assertFalse( reader.Next(), "Didn't expect reader to have any entries." );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldAddAndReadOneKey() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldAddAndReadOneKey()
		 {
			  using ( IndexKeyStorage<GenericKey> keyStorage = keyStorage() )
			  {
					GenericKey expected = RandomKey( 1 );
					keyStorage.add( expected );
					keyStorage.doneAdding();
					using ( IndexKeyStorage.KeyEntryCursor<GenericKey> reader = keyStorage.reader() )
					{
						 assertTrue( reader.Next(), "Expected reader to have one entry" );
						 GenericKey actual = reader.Key();
						 assertEquals( 0, _layout.compare( expected, actual ), "Expected stored key to be equal to original." );
						 assertFalse( reader.Next(), "Expected reader to have only one entry, second entry was " + reader.Key() );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldAddAndReadMultipleKeys() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldAddAndReadMultipleKeys()
		 {
			  IList<GenericKey> keys = new List<GenericKey>();
			  int numberOfKeys = 1000;
			  for ( int i = 0; i < numberOfKeys; i++ )
			  {
					keys.Add( RandomKey( i ) );
			  }
			  using ( IndexKeyStorage<GenericKey> keyStorage = keyStorage() )
			  {
					foreach ( GenericKey key in keys )
					{
						 keyStorage.add( key );
					}
					keyStorage.doneAdding();
					using ( IndexKeyStorage.KeyEntryCursor<GenericKey> reader = keyStorage.reader() )
					{
						 foreach ( GenericKey expected in keys )
						 {
							  assertTrue( reader.Next() );
							  GenericKey actual = reader.Key();
							  assertEquals( 0, _layout.compare( expected, actual ), "Expected stored key to be equal to original." );
						 }
						 assertFalse( reader.Next(), "Expected reader to have no more entries, but had at least one additional " + reader.Key() );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotCreateFileIfNoData() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldNotCreateFileIfNoData()
		 {
			  FileSystemAbstraction fs = Directory.FileSystem;
			  File makeSureImDeleted = Directory.file( "makeSureImDeleted" );
			  using ( IndexKeyStorage<GenericKey> keyStorage = keyStorage( makeSureImDeleted ) )
			  {
					assertFalse( fs.FileExists( makeSureImDeleted ), "Expected this file to exist now so that we can assert deletion later." );
					keyStorage.doneAdding();
					assertFalse( fs.FileExists( makeSureImDeleted ), "Expected this file to exist now so that we can assert deletion later." );
			  }
			  assertFalse( fs.FileExists( makeSureImDeleted ), "Expected this file to be deleted on close." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDeleteFileOnCloseWithData() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldDeleteFileOnCloseWithData()
		 {
			  FileSystemAbstraction fs = Directory.FileSystem;
			  File makeSureImDeleted = Directory.file( "makeSureImDeleted" );
			  using ( IndexKeyStorage<GenericKey> keyStorage = keyStorage( makeSureImDeleted ) )
			  {
					keyStorage.add( RandomKey( 1 ) );
					keyStorage.doneAdding();
					assertTrue( fs.FileExists( makeSureImDeleted ), "Expected this file to exist now so that we can assert deletion later." );
			  }
			  assertFalse( fs.FileExists( makeSureImDeleted ), "Expected this file to be deleted on close." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDeleteFileOnCloseWithDataBeforeDoneAdding() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldDeleteFileOnCloseWithDataBeforeDoneAdding()
		 {
			  FileSystemAbstraction fs = Directory.FileSystem;
			  File makeSureImDeleted = Directory.file( "makeSureImDeleted" );
			  using ( IndexKeyStorage<GenericKey> keyStorage = keyStorage( makeSureImDeleted ) )
			  {
					keyStorage.add( RandomKey( 1 ) );
					assertTrue( fs.FileExists( makeSureImDeleted ), "Expected this file to exist now so that we can assert deletion later." );
			  }
			  assertFalse( fs.FileExists( makeSureImDeleted ), "Expected this file to be deleted on close." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustAllocateResourcesLazilyAndCleanUpOnClose() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustAllocateResourcesLazilyAndCleanUpOnClose()
		 {
			  FileSystemAbstraction fs = Directory.FileSystem;
			  LocalMemoryTracker allocationTracker = new LocalMemoryTracker();
			  File file = Directory.file( "file" );
			  using ( UnsafeDirectByteBufferAllocator bufferFactory = new UnsafeDirectByteBufferAllocator( allocationTracker ), IndexKeyStorage<GenericKey> keyStorage = keyStorage( file, bufferFactory ) )
			  {
					assertEquals( 0, allocationTracker.UsedDirectMemory(), "Expected to not have any buffers allocated yet" );
					assertFalse( fs.FileExists( file ), "Expected file to be created lazily" );
					keyStorage.add( RandomKey( 1 ) );
					assertEquals( BLOCK_SIZE, allocationTracker.UsedDirectMemory(), "Expected to have exactly one buffer allocated by now" );
					assertTrue( fs.FileExists( file ), "Expected file to be created by now" );
			  }
			  assertFalse( fs.FileExists( file ), "Expected file to be deleted on close" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportCorrectCount() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldReportCorrectCount()
		 {
			  using ( IndexKeyStorage<GenericKey> keyStorage = keyStorage() )
			  {
					assertEquals( 0, keyStorage.count() );
					keyStorage.add( RandomKey( 1 ) );
					assertEquals( 1, keyStorage.count() );
					keyStorage.add( RandomKey( 2 ) );
					assertEquals( 2, keyStorage.count() );
					keyStorage.doneAdding();
					assertEquals( 2, keyStorage.count() );
			  }
		 }

		 private GenericKey RandomKey( int IEntityId )
		 {
			  GenericKey key = _layout.newKey();
			  key.Initialize( IEntityId );
			  for ( int i = 0; i < _numberOfSlots; i++ )
			  {
					Value value = Random.randomValues().nextValue();
					key.InitFromValue( i, value, NEUTRAL );
			  }
			  return key;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private IndexKeyStorage<GenericKey> keyStorage() throws java.io.IOException
		 private IndexKeyStorage<GenericKey> KeyStorage()
		 {
			  return KeyStorage( Directory.file( "file" ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private IndexKeyStorage<GenericKey> keyStorage(java.io.File file) throws java.io.IOException
		 private IndexKeyStorage<GenericKey> KeyStorage( File file )
		 {
			  return KeyStorage( file, HEAP_ALLOCATOR );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private IndexKeyStorage<GenericKey> keyStorage(java.io.File file, ByteBufferFactory.Allocator bufferFactory) throws java.io.IOException
		 private IndexKeyStorage<GenericKey> KeyStorage( File file, ByteBufferFactory.Allocator bufferFactory )
		 {
			  return new IndexKeyStorage<GenericKey>( Directory.FileSystem, file, bufferFactory, BLOCK_SIZE, _layout );
		 }
	}

}