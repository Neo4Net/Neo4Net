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
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using SchemaDescriptorSupplier = Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptorSupplier;
	using Neo4Net.Kernel.Api.Index;
	using SchemaDescriptorFactory = Neo4Net.Kernel.api.schema.SchemaDescriptorFactory;
	using Config = Neo4Net.Kernel.configuration.Config;
	using UpdateMode = Neo4Net.Kernel.Impl.Api.index.UpdateMode;
	using ConfiguredSpaceFillingCurveSettingsCache = Neo4Net.Kernel.Impl.Index.Schema.config.ConfiguredSpaceFillingCurveSettingsCache;
	using IndexSpecificSpaceFillingCurveSettingsCache = Neo4Net.Kernel.Impl.Index.Schema.config.IndexSpecificSpaceFillingCurveSettingsCache;
	using Inject = Neo4Net.Test.extension.Inject;
	using RandomExtension = Neo4Net.Test.extension.RandomExtension;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.schema.ByteBufferFactory.HEAP_ALLOCATOR;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith({TestDirectoryExtension.class, RandomExtension.class}) class IndexUpdateStorageTest
	internal class IndexUpdateStorageTest
	{
		 private static readonly IndexSpecificSpaceFillingCurveSettingsCache _spatialSettings = new IndexSpecificSpaceFillingCurveSettingsCache( new ConfiguredSpaceFillingCurveSettingsCache( Config.defaults() ), new Dictionary<Neo4Net.Values.Storable.CoordinateReferenceSystem, SpaceFillingCurveSettings>() );
		 private static readonly SchemaDescriptorSupplier _descriptor = SchemaDescriptorFactory.forLabel( 1, 1 );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject protected org.Neo4Net.test.rule.TestDirectory directory;
		 protected internal TestDirectory Directory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject protected org.Neo4Net.test.rule.RandomRule random;
		 protected internal RandomRule Random;

		 private readonly GenericLayout _layout = new GenericLayout( 1, _spatialSettings );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldAddZeroEntries() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldAddZeroEntries()
		 {
			  // given
			  using ( IndexUpdateStorage<GenericKey, NativeIndexValue> storage = new IndexUpdateStorage<GenericKey, NativeIndexValue>( Directory.FileSystem, Directory.file( "file" ), HEAP_ALLOCATOR, 1000, _layout ) )
			  {
					// when
					IList<IndexEntryUpdate<SchemaDescriptorSupplier>> expected = GenerateSomeUpdates( 0 );
					StoreAll( storage, expected );

					// then
					Verify( expected, storage );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldAddFewEntries() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldAddFewEntries()
		 {
			  // given
			  using ( IndexUpdateStorage<GenericKey, NativeIndexValue> storage = new IndexUpdateStorage<GenericKey, NativeIndexValue>( Directory.FileSystem, Directory.file( "file" ), HEAP_ALLOCATOR, 1000, _layout ) )
			  {
					// when
					IList<IndexEntryUpdate<SchemaDescriptorSupplier>> expected = GenerateSomeUpdates( 5 );
					StoreAll( storage, expected );

					// then
					Verify( expected, storage );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldAddManyEntries() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldAddManyEntries()
		 {
			  // given
			  using ( IndexUpdateStorage<GenericKey, NativeIndexValue> storage = new IndexUpdateStorage<GenericKey, NativeIndexValue>( Directory.FileSystem, Directory.file( "file" ), HEAP_ALLOCATOR, 10_000, _layout ) )
			  {
					// when
					IList<IndexEntryUpdate<SchemaDescriptorSupplier>> expected = GenerateSomeUpdates( 1_000 );
					StoreAll( storage, expected );

					// then
					Verify( expected, storage );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void storeAll(IndexUpdateStorage<GenericKey,NativeIndexValue> storage, java.util.List<org.Neo4Net.kernel.api.index.IndexEntryUpdate<org.Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptorSupplier>> expected) throws java.io.IOException
		 private static void StoreAll( IndexUpdateStorage<GenericKey, NativeIndexValue> storage, IList<IndexEntryUpdate<SchemaDescriptorSupplier>> expected )
		 {
			  foreach ( IndexEntryUpdate<SchemaDescriptorSupplier> update in expected )
			  {
					storage.add( update );
			  }
			  storage.doneAdding();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void verify(java.util.List<org.Neo4Net.kernel.api.index.IndexEntryUpdate<org.Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptorSupplier>> expected, IndexUpdateStorage<GenericKey,NativeIndexValue> storage) throws java.io.IOException
		 private static void Verify( IList<IndexEntryUpdate<SchemaDescriptorSupplier>> expected, IndexUpdateStorage<GenericKey, NativeIndexValue> storage )
		 {
			  using ( IndexUpdateCursor<GenericKey, NativeIndexValue> reader = storage.reader() )
			  {
					foreach ( IndexEntryUpdate<SchemaDescriptorSupplier> expectedUpdate in expected )
					{
						 assertTrue( reader.Next() );
						 assertEquals( expectedUpdate, AsUpdate( reader ) );
					}
					assertFalse( reader.Next() );
			  }
		 }

		 private static IndexEntryUpdate<SchemaDescriptorSupplier> AsUpdate( IndexUpdateCursor<GenericKey, NativeIndexValue> reader )
		 {
			  switch ( reader.UpdateMode() )
			  {
			  case ADDED:
					return IndexEntryUpdate.add( reader.Key().EntityId, _descriptor, reader.Key().asValue() );
			  case CHANGED:
					return IndexEntryUpdate.change( reader.Key().EntityId, _descriptor, reader.Key().asValue(), reader.Key2().asValue() );
			  case REMOVED:
					return IndexEntryUpdate.remove( reader.Key().EntityId, _descriptor, reader.Key().asValue() );
			  default:
					throw new System.ArgumentException();
			  }
		 }

		 private IList<IndexEntryUpdate<SchemaDescriptorSupplier>> GenerateSomeUpdates( int count )
		 {
			  IList<IndexEntryUpdate<SchemaDescriptorSupplier>> updates = new List<IndexEntryUpdate<SchemaDescriptorSupplier>>();
			  for ( int i = 0; i < count; i++ )
			  {
					long IEntityId = Random.nextLong( 10_000_000 );
					switch ( Random.among( UpdateMode.MODES ) )
					{
					case ADDED:
						 updates.Add( IndexEntryUpdate.add( IEntityId, _descriptor, Random.nextValue() ) );
						 break;
					case REMOVED:
						 updates.Add( IndexEntryUpdate.remove( IEntityId, _descriptor, Random.nextValue() ) );
						 break;
					case CHANGED:
						 updates.Add( IndexEntryUpdate.change( IEntityId, _descriptor, Random.nextValue(), Random.nextValue() ) );
						 break;
					default:
						 throw new System.ArgumentException();
					}
			  }
			  return updates;
		 }
	}

}