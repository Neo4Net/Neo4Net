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
namespace Org.Neo4j.Kernel.Impl.Index.Schema
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using PrimitiveLongResourceIterator = Org.Neo4j.Collection.PrimitiveLongResourceIterator;
	using SpaceFillingCurveConfiguration = Org.Neo4j.Gis.Spatial.Index.curves.SpaceFillingCurveConfiguration;
	using IndexOrder = Org.Neo4j.@internal.Kernel.Api.IndexOrder;
	using IndexQuery = Org.Neo4j.@internal.Kernel.Api.IndexQuery;
	using IndexProviderDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.IndexProviderDescriptor;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using IndexEntryConflictException = Org.Neo4j.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using IndexDirectoryStructure = Org.Neo4j.Kernel.Api.Index.IndexDirectoryStructure;
	using Org.Neo4j.Kernel.Api.Index;
	using IndexUpdater = Org.Neo4j.Kernel.Api.Index.IndexUpdater;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using ConfiguredSpaceFillingCurveSettingsCache = Org.Neo4j.Kernel.Impl.Index.Schema.config.ConfiguredSpaceFillingCurveSettingsCache;
	using IndexSpecificSpaceFillingCurveSettingsCache = Org.Neo4j.Kernel.Impl.Index.Schema.config.IndexSpecificSpaceFillingCurveSettingsCache;
	using SpaceFillingCurveSettingsFactory = Org.Neo4j.Kernel.Impl.Index.Schema.config.SpaceFillingCurveSettingsFactory;
	using SimpleNodeValueClient = Org.Neo4j.Storageengine.Api.schema.SimpleNodeValueClient;
	using StoreIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.StoreIndexDescriptor;
	using PageCacheAndDependenciesRule = Org.Neo4j.Test.rule.PageCacheAndDependenciesRule;
	using TextValue = Org.Neo4j.Values.Storable.TextValue;
	using Value = Org.Neo4j.Values.Storable.Value;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.index.IndexDirectoryStructure.directoriesByProvider;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.index.IndexEntryUpdate.add;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.index.IndexProvider.Monitor_Fields.EMPTY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.schema.SchemaDescriptorFactory.forLabel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.api.index.PhaseTracker_Fields.nullInstance;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.ByteBufferFactory.heapBufferFactory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.schema.IndexDescriptorFactory.forSchema;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.schema.IndexDescriptorFactory.uniqueForSchema;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringValue;

	public class GenericBlockBasedIndexPopulatorTest
	{
		 private static readonly StoreIndexDescriptor _indexDescriptor = forSchema( forLabel( 1, 1 ) ).withId( 1 );
		 private static readonly StoreIndexDescriptor _uniqueIndexDescriptor = uniqueForSchema( forLabel( 1, 1 ) ).withId( 1 );

		 private IndexDirectoryStructure _directoryStructure;
		 private File _indexFile;
		 private FileSystemAbstraction _fs;
		 private IndexDropAction _dropAction;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  IndexProviderDescriptor providerDescriptor = new IndexProviderDescriptor( "test", "v1" );
			  _directoryStructure = directoriesByProvider( Storage.directory().databaseDir() ).forProvider(providerDescriptor);
			  File indexDir = _directoryStructure.directoryForIndex( _indexDescriptor.Id );
			  _indexFile = new File( indexDir, "index" );
			  _fs = Storage.fileSystem();
			  _dropAction = new FileSystemIndexDropAction( _fs, _directoryStructure );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.PageCacheAndDependenciesRule storage = new org.neo4j.test.rule.PageCacheAndDependenciesRule();
		 public readonly PageCacheAndDependenciesRule Storage = new PageCacheAndDependenciesRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeExternalUpdateBothBeforeAndAfterScanCompleted() throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeExternalUpdateBothBeforeAndAfterScanCompleted()
		 {
			  // given
			  BlockBasedIndexPopulator<GenericKey, NativeIndexValue> populator = InstantiatePopulator( _indexDescriptor );
			  try
			  {
					// when
					TextValue hakuna = stringValue( "hakuna" );
					TextValue matata = stringValue( "matata" );
					int hakunaId = 1;
					int matataId = 2;
					ExternalUpdate( populator, hakuna, hakunaId );
					populator.ScanCompleted( nullInstance );
					ExternalUpdate( populator, matata, matataId );

					// then
					AssertMatch( populator, hakuna, hakunaId );
					AssertMatch( populator, matata, matataId );
			  }
			  finally
			  {
					populator.Close( true );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowOnDuplicatedValuesFromScan()
		 public virtual void ShouldThrowOnDuplicatedValuesFromScan()
		 {
			  // given
			  BlockBasedIndexPopulator<GenericKey, NativeIndexValue> populator = InstantiatePopulator( _uniqueIndexDescriptor );
			  bool closed = false;
			  try
			  {
					// when
					Value duplicate = Values.of( "duplicate" );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.kernel.api.index.IndexEntryUpdate<?> firstScanUpdate = org.neo4j.kernel.api.index.IndexEntryUpdate.add(1, INDEX_DESCRIPTOR, duplicate);
					IndexEntryUpdate<object> firstScanUpdate = IndexEntryUpdate.add( 1, _indexDescriptor, duplicate );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.kernel.api.index.IndexEntryUpdate<?> secondScanUpdate = org.neo4j.kernel.api.index.IndexEntryUpdate.add(2, INDEX_DESCRIPTOR, duplicate);
					IndexEntryUpdate<object> secondScanUpdate = IndexEntryUpdate.add( 2, _indexDescriptor, duplicate );
					try
					{
						 populator.Add( singleton( firstScanUpdate ) );
						 populator.Add( singleton( secondScanUpdate ) );
						 populator.ScanCompleted( nullInstance );

						 fail( "Expected to throw" );
					}
					catch ( IndexEntryConflictException )
					{
						 // then
					}
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
//ORIGINAL LINE: @Test public void shouldThrowOnDuplicatedValuesFromExternalUpdates()
		 public virtual void ShouldThrowOnDuplicatedValuesFromExternalUpdates()
		 {
			  // given
			  BlockBasedIndexPopulator<GenericKey, NativeIndexValue> populator = InstantiatePopulator( _uniqueIndexDescriptor );
			  bool closed = false;
			  try
			  {
					// when
					Value duplicate = Values.of( "duplicate" );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.kernel.api.index.IndexEntryUpdate<?> firstExternalUpdate = org.neo4j.kernel.api.index.IndexEntryUpdate.add(1, INDEX_DESCRIPTOR, duplicate);
					IndexEntryUpdate<object> firstExternalUpdate = IndexEntryUpdate.add( 1, _indexDescriptor, duplicate );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.kernel.api.index.IndexEntryUpdate<?> secondExternalUpdate = org.neo4j.kernel.api.index.IndexEntryUpdate.add(2, INDEX_DESCRIPTOR, duplicate);
					IndexEntryUpdate<object> secondExternalUpdate = IndexEntryUpdate.add( 2, _indexDescriptor, duplicate );
					try
					{
						 using ( IndexUpdater updater = populator.NewPopulatingUpdater() )
						 {
							  updater.Process( firstExternalUpdate );
							  updater.Process( secondExternalUpdate );
						 }
						 populator.ScanCompleted( nullInstance );

						 fail( "Expected to throw" );
					}
					catch ( IndexEntryConflictException )
					{
						 // then
					}
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
//ORIGINAL LINE: @Test public void shouldThrowOnDuplicatedValuesFromScanAndExternalUpdates()
		 public virtual void ShouldThrowOnDuplicatedValuesFromScanAndExternalUpdates()
		 {
			  // given
			  BlockBasedIndexPopulator<GenericKey, NativeIndexValue> populator = InstantiatePopulator( _uniqueIndexDescriptor );
			  bool closed = false;
			  try
			  {
					// when
					Value duplicate = Values.of( "duplicate" );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.kernel.api.index.IndexEntryUpdate<?> externalUpdate = org.neo4j.kernel.api.index.IndexEntryUpdate.add(1, INDEX_DESCRIPTOR, duplicate);
					IndexEntryUpdate<object> externalUpdate = IndexEntryUpdate.add( 1, _indexDescriptor, duplicate );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.kernel.api.index.IndexEntryUpdate<?> scanUpdate = org.neo4j.kernel.api.index.IndexEntryUpdate.add(2, INDEX_DESCRIPTOR, duplicate);
					IndexEntryUpdate<object> scanUpdate = IndexEntryUpdate.add( 2, _indexDescriptor, duplicate );
					try
					{
						 using ( IndexUpdater updater = populator.NewPopulatingUpdater() )
						 {
							  updater.Process( externalUpdate );
						 }
						 populator.Add( singleton( scanUpdate ) );
						 populator.ScanCompleted( nullInstance );

						 fail( "Expected to throw" );
					}
					catch ( IndexEntryConflictException )
					{
						 // then
					}
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
//ORIGINAL LINE: @Test public void shouldNotThrowOnDuplicationsLaterFixedByExternalUpdates() throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotThrowOnDuplicationsLaterFixedByExternalUpdates()
		 {
			  // given
			  BlockBasedIndexPopulator<GenericKey, NativeIndexValue> populator = InstantiatePopulator( _uniqueIndexDescriptor );
			  bool closed = false;
			  try
			  {
					// when
					Value duplicate = Values.of( "duplicate" );
					Value unique = Values.of( "unique" );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.kernel.api.index.IndexEntryUpdate<?> firstScanUpdate = org.neo4j.kernel.api.index.IndexEntryUpdate.add(1, INDEX_DESCRIPTOR, duplicate);
					IndexEntryUpdate<object> firstScanUpdate = IndexEntryUpdate.add( 1, _indexDescriptor, duplicate );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.kernel.api.index.IndexEntryUpdate<?> secondScanUpdate = org.neo4j.kernel.api.index.IndexEntryUpdate.add(2, INDEX_DESCRIPTOR, duplicate);
					IndexEntryUpdate<object> secondScanUpdate = IndexEntryUpdate.add( 2, _indexDescriptor, duplicate );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.kernel.api.index.IndexEntryUpdate<?> externalUpdate = org.neo4j.kernel.api.index.IndexEntryUpdate.change(1, INDEX_DESCRIPTOR, duplicate, unique);
					IndexEntryUpdate<object> externalUpdate = IndexEntryUpdate.change( 1, _indexDescriptor, duplicate, unique );
					populator.Add( singleton( firstScanUpdate ) );
					using ( IndexUpdater updater = populator.NewPopulatingUpdater() )
					{
						 updater.Process( externalUpdate );
					}
					populator.Add( singleton( secondScanUpdate ) );
					populator.ScanCompleted( nullInstance );

					// then
					AssertHasEntry( populator, unique, 1 );
					AssertHasEntry( populator, duplicate, 2 );
			  }
			  finally
			  {
					if ( !closed )
					{
						 populator.Close( true );
					}
			  }
		 }

		 private void AssertHasEntry( BlockBasedIndexPopulator<GenericKey, NativeIndexValue> populator, Value duplicate, int expectedId )
		 {
			  using ( NativeIndexReader<GenericKey, NativeIndexValue> reader = populator.newReader() )
			  {
					PrimitiveLongResourceIterator query = reader.Query( IndexQuery.exact( _indexDescriptor.properties()[0], duplicate ) );
					assertTrue( query.hasNext() );
					long id = query.next();
					assertEquals( expectedId, id );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void externalUpdate(BlockBasedIndexPopulator<GenericKey,NativeIndexValue> populator, org.neo4j.values.storable.TextValue matata, int matataId) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 private void ExternalUpdate( BlockBasedIndexPopulator<GenericKey, NativeIndexValue> populator, TextValue matata, int matataId )
		 {
			  using ( IndexUpdater indexUpdater = populator.NewPopulatingUpdater() )
			  {
					// After scanCompleted
					indexUpdater.Process( add( matataId, _indexDescriptor, matata ) );
			  }
		 }

		 private void AssertMatch( BlockBasedIndexPopulator<GenericKey, NativeIndexValue> populator, Value value, long id )
		 {
			  using ( NativeIndexReader<GenericKey, NativeIndexValue> reader = populator.newReader() )
			  {
					SimpleNodeValueClient cursor = new SimpleNodeValueClient();
					reader.Query( cursor, IndexOrder.NONE, true, IndexQuery.exact( _indexDescriptor.properties()[0], value ) );
					assertTrue( cursor.Next() );
					assertEquals( id, cursor.Reference );
					assertEquals( value, cursor.Values[0] );
					assertFalse( cursor.Next() );
			  }
		 }

		 private GenericBlockBasedIndexPopulator InstantiatePopulator( StoreIndexDescriptor indexDescriptor )
		 {
			  Config config = Config.defaults();
			  ConfiguredSpaceFillingCurveSettingsCache settingsCache = new ConfiguredSpaceFillingCurveSettingsCache( config );
			  IndexSpecificSpaceFillingCurveSettingsCache spatialSettings = new IndexSpecificSpaceFillingCurveSettingsCache( settingsCache, new Dictionary<Org.Neo4j.Values.Storable.CoordinateReferenceSystem, SpaceFillingCurveSettings>() );
			  GenericLayout layout = new GenericLayout( 1, spatialSettings );
			  SpaceFillingCurveConfiguration configuration = SpaceFillingCurveSettingsFactory.getConfiguredSpaceFillingCurveConfiguration( config );
			  GenericBlockBasedIndexPopulator populator = new GenericBlockBasedIndexPopulator( Storage.pageCache(), _fs, _indexFile, layout, EMPTY, indexDescriptor, spatialSettings, _directoryStructure, configuration, _dropAction, false, heapBufferFactory(1024) );
			  populator.Create();
			  return populator;
		 }
	}

}