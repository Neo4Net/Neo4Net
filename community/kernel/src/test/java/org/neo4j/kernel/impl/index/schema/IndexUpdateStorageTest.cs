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
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using SchemaDescriptorSupplier = Org.Neo4j.@internal.Kernel.Api.schema.SchemaDescriptorSupplier;
	using Org.Neo4j.Kernel.Api.Index;
	using SchemaDescriptorFactory = Org.Neo4j.Kernel.api.schema.SchemaDescriptorFactory;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using UpdateMode = Org.Neo4j.Kernel.Impl.Api.index.UpdateMode;
	using ConfiguredSpaceFillingCurveSettingsCache = Org.Neo4j.Kernel.Impl.Index.Schema.config.ConfiguredSpaceFillingCurveSettingsCache;
	using IndexSpecificSpaceFillingCurveSettingsCache = Org.Neo4j.Kernel.Impl.Index.Schema.config.IndexSpecificSpaceFillingCurveSettingsCache;
	using Inject = Org.Neo4j.Test.extension.Inject;
	using RandomExtension = Org.Neo4j.Test.extension.RandomExtension;
	using TestDirectoryExtension = Org.Neo4j.Test.extension.TestDirectoryExtension;
	using RandomRule = Org.Neo4j.Test.rule.RandomRule;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.ByteBufferFactory.HEAP_ALLOCATOR;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith({TestDirectoryExtension.class, RandomExtension.class}) class IndexUpdateStorageTest
	internal class IndexUpdateStorageTest
	{
		 private static readonly IndexSpecificSpaceFillingCurveSettingsCache _spatialSettings = new IndexSpecificSpaceFillingCurveSettingsCache( new ConfiguredSpaceFillingCurveSettingsCache( Config.defaults() ), new Dictionary<Org.Neo4j.Values.Storable.CoordinateReferenceSystem, SpaceFillingCurveSettings>() );
		 private static readonly SchemaDescriptorSupplier _descriptor = SchemaDescriptorFactory.forLabel( 1, 1 );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject protected org.neo4j.test.rule.TestDirectory directory;
		 protected internal TestDirectory Directory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject protected org.neo4j.test.rule.RandomRule random;
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
//ORIGINAL LINE: private static void storeAll(IndexUpdateStorage<GenericKey,NativeIndexValue> storage, java.util.List<org.neo4j.kernel.api.index.IndexEntryUpdate<org.neo4j.internal.kernel.api.schema.SchemaDescriptorSupplier>> expected) throws java.io.IOException
		 private static void StoreAll( IndexUpdateStorage<GenericKey, NativeIndexValue> storage, IList<IndexEntryUpdate<SchemaDescriptorSupplier>> expected )
		 {
			  foreach ( IndexEntryUpdate<SchemaDescriptorSupplier> update in expected )
			  {
					storage.add( update );
			  }
			  storage.doneAdding();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void verify(java.util.List<org.neo4j.kernel.api.index.IndexEntryUpdate<org.neo4j.internal.kernel.api.schema.SchemaDescriptorSupplier>> expected, IndexUpdateStorage<GenericKey,NativeIndexValue> storage) throws java.io.IOException
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
					long entityId = Random.nextLong( 10_000_000 );
					switch ( Random.among( UpdateMode.MODES ) )
					{
					case ADDED:
						 updates.Add( IndexEntryUpdate.add( entityId, _descriptor, Random.nextValue() ) );
						 break;
					case REMOVED:
						 updates.Add( IndexEntryUpdate.remove( entityId, _descriptor, Random.nextValue() ) );
						 break;
					case CHANGED:
						 updates.Add( IndexEntryUpdate.change( entityId, _descriptor, Random.nextValue(), Random.nextValue() ) );
						 break;
					default:
						 throw new System.ArgumentException();
					}
			  }
			  return updates;
		 }
	}

}