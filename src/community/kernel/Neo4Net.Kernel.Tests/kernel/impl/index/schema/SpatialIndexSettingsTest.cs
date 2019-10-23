using System;

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
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using StandardConfiguration = Neo4Net.Gis.Spatial.Index.curves.StandardConfiguration;
	using Neo4Net.GraphDb.config;
	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using IndexAccessor = Neo4Net.Kernel.Api.Index.IndexAccessor;
	using Neo4Net.Kernel.Api.Index;
	using IndexPopulator = Neo4Net.Kernel.Api.Index.IndexPopulator;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using Config = Neo4Net.Kernel.configuration.Config;
	using IndexSamplingConfig = Neo4Net.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using ConfiguredSpaceFillingCurveSettingsCache = Neo4Net.Kernel.Impl.Index.Schema.config.ConfiguredSpaceFillingCurveSettingsCache;
	using SpaceFillingCurveSettings = Neo4Net.Kernel.Impl.Index.Schema.config.SpaceFillingCurveSettings;
	using SpaceFillingCurveSettingsFactory = Neo4Net.Kernel.Impl.Index.Schema.config.SpaceFillingCurveSettingsFactory;
	using SpatialIndexSettings = Neo4Net.Kernel.Impl.Index.Schema.config.SpatialIndexSettings;
	using IndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor;
	using StoreIndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.StoreIndexDescriptor;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.rules.RuleChain.outerRule;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.index.Internal.gbptree.RecoveryCleanupWorkCollector.immediate;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.index.IndexDirectoryStructure.directoriesByProvider;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.api.index.IndexUpdateMode.ONLINE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.schema.ByteBufferFactory.heapBufferFactory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.rule.PageCacheRule.config;

	public class SpatialIndexSettingsTest
	{
		private bool InstanceFieldsInitialized = false;

		public SpatialIndexSettingsTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_directory = TestDirectory.testDirectory( this.GetType(), Fs.get() );
			Rules = outerRule( Fs ).around( _directory ).around( _pageCacheRule ).around( _randomRule );
		}

		 private static readonly CoordinateReferenceSystem _crs = CoordinateReferenceSystem.WGS84;
		 private static readonly Config _config1 = Config.defaults();
		 private static readonly Config _config2 = ConfigWithRange( 0, -90, 180, 90 );
		 private static readonly ConfiguredSpaceFillingCurveSettingsCache _configuredSettings1 = new ConfiguredSpaceFillingCurveSettingsCache( _config1 );
		 private static readonly ConfiguredSpaceFillingCurveSettingsCache _configuredSettings2 = new ConfiguredSpaceFillingCurveSettingsCache( _config2 );

		 private StoreIndexDescriptor _schemaIndexDescriptor1;
		 private StoreIndexDescriptor _schemaIndexDescriptor2;
		 private ValueCreatorUtil<SpatialIndexKey, NativeIndexValue> _layoutUtil1;
		 private ValueCreatorUtil<SpatialIndexKey, NativeIndexValue> _layoutUtil2;
		 private long _indexId1 = 1;
		 private long _indexId2 = 2;

		 internal readonly DefaultFileSystemRule Fs = new DefaultFileSystemRule();
		 private TestDirectory _directory;
		 private readonly PageCacheRule _pageCacheRule = new PageCacheRule( config().withAccessChecks(true) );
		 private RandomRule _randomRule = new RandomRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain rules = outerRule(fs).around(directory).around(pageCacheRule).around(randomRule);
		 public RuleChain Rules;

		 private PageCache _pageCache;
		 private IndexProvider.Monitor _monitor = IndexProvider.Monitor_Fields.EMPTY;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setupTwoIndexes() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetupTwoIndexes()
		 {
			  _pageCache = _pageCacheRule.getPageCache( Fs );

			  // Define two indexes based on different labels and different configuredSettings
			  _layoutUtil1 = CreateLayoutTestUtil( _indexId1, 42 );
			  _layoutUtil2 = CreateLayoutTestUtil( _indexId2, 43 );
			  _schemaIndexDescriptor1 = _layoutUtil1.indexDescriptor();
			  _schemaIndexDescriptor2 = _layoutUtil2.indexDescriptor();

			  // Create the two indexes as empty, based on differently configured configuredSettings above
			  CreateEmptyIndex( _schemaIndexDescriptor1, _configuredSettings1 );
			  CreateEmptyIndex( _schemaIndexDescriptor2, _configuredSettings2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAddToSpatialIndexWithDefaults() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAddToSpatialIndexWithDefaults()
		 {
			  // given
			  SpatialIndexProvider provider = NewSpatialIndexProvider( _config1 );
			  AddUpdates( provider, _schemaIndexDescriptor1, _layoutUtil1 );

			  // then
			  VerifySpatialSettings( IndexFile( _indexId1 ), _configuredSettings1.forCRS( _crs ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAddToSpatialIndexWithModifiedSettings() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAddToSpatialIndexWithModifiedSettings()
		 {
			  // given
			  SpatialIndexProvider provider = NewSpatialIndexProvider( _config2 );
			  AddUpdates( provider, _schemaIndexDescriptor2, _layoutUtil2 );

			  // then
			  VerifySpatialSettings( IndexFile( _indexId2 ), _configuredSettings2.forCRS( _crs ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAddToTwoDifferentIndexesOneDefaultAndOneModified() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAddToTwoDifferentIndexesOneDefaultAndOneModified()
		 {
			  // given
			  SpatialIndexProvider provider = NewSpatialIndexProvider( _config2 );
			  AddUpdates( provider, _schemaIndexDescriptor1, _layoutUtil1 );
			  AddUpdates( provider, _schemaIndexDescriptor2, _layoutUtil2 );

			  // then even though the provider was created with modified configuredSettings, only the second index should have them
			  VerifySpatialSettings( IndexFile( _indexId1 ), _configuredSettings1.forCRS( _crs ) );
			  VerifySpatialSettings( IndexFile( _indexId2 ), _configuredSettings2.forCRS( _crs ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotLeakSpaceFillingCurveSettingsBetweenExistingAndNewIndexes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotLeakSpaceFillingCurveSettingsBetweenExistingAndNewIndexes()
		 {
			  // given two indexes previously created with different configuredSettings
			  Config config = ConfigWithRange( -10, -10, 10, 10 );
			  SpatialIndexProvider provider = NewSpatialIndexProvider( config );
			  AddUpdates( provider, _schemaIndexDescriptor1, _layoutUtil1 );
			  AddUpdates( provider, _schemaIndexDescriptor2, _layoutUtil2 );

			  // and when creating and populating a third index with a third set of configuredSettings
			  long indexId3 = 3;
			  ConfiguredSpaceFillingCurveSettingsCache settings3 = new ConfiguredSpaceFillingCurveSettingsCache( config );
			  SpatialValueCreatorUtil layoutUtil3 = CreateLayoutTestUtil( indexId3, 44 );
			  StoreIndexDescriptor schemaIndexDescriptor3 = layoutUtil3.IndexDescriptor();
			  CreateEmptyIndex( schemaIndexDescriptor3, provider );
			  AddUpdates( provider, schemaIndexDescriptor3, layoutUtil3 );

			  // Then all indexes should still have their own correct and different configuredSettings
			  VerifySpatialSettings( IndexFile( _indexId1 ), _configuredSettings1.forCRS( _crs ) );
			  VerifySpatialSettings( IndexFile( _indexId2 ), _configuredSettings2.forCRS( _crs ) );
			  VerifySpatialSettings( IndexFile( indexId3 ), settings3.ForCRS( _crs ) );
		 }

		 private IndexSamplingConfig SamplingConfig()
		 {
			  return new IndexSamplingConfig( Config.defaults() );
		 }

		 private SpatialValueCreatorUtil CreateLayoutTestUtil( long indexId, int labelId )
		 {
			  StoreIndexDescriptor descriptor = TestIndexDescriptorFactory.forLabel( labelId, 666 ).withId( indexId );
			  return new SpatialValueCreatorUtil( descriptor, ValueCreatorUtil.FRACTION_DUPLICATE_NON_UNIQUE );
		 }

		 private SpatialIndexProvider NewSpatialIndexProvider( Config config )
		 {
			  return new SpatialIndexProvider( _pageCache, Fs, directoriesByProvider( _directory.databaseDir() ), _monitor, immediate(), false, config );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void addUpdates(SpatialIndexProvider provider, org.Neo4Net.Kernel.Api.StorageEngine.schema.StoreIndexDescriptor schemaIndexDescriptor, ValueCreatorUtil<SpatialIndexKey,NativeIndexValue> layoutUtil) throws java.io.IOException, org.Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException
		 private void AddUpdates( SpatialIndexProvider provider, StoreIndexDescriptor schemaIndexDescriptor, ValueCreatorUtil<SpatialIndexKey, NativeIndexValue> layoutUtil )
		 {
			  IndexAccessor accessor = provider.GetOnlineAccessor( schemaIndexDescriptor, SamplingConfig() );
			  using ( IndexUpdater updater = accessor.NewUpdater( ONLINE ) )
			  {
					// when
					foreach ( IndexEntryUpdate<IndexDescriptor> update in layoutUtil.SomeUpdates( _randomRule ) )
					{
						 updater.Process( update );
					}
			  }
			  accessor.Force( Neo4Net.Io.pagecache.IOLimiter_Fields.Unlimited );
			  accessor.Dispose();
		 }

		 private SpatialIndexFiles.SpatialFile MakeIndexFile( long indexId, ConfiguredSpaceFillingCurveSettingsCache configuredSettings )
		 {
			  return new SpatialIndexFiles.SpatialFile( CoordinateReferenceSystem.WGS84, configuredSettings, IndexDir( indexId ) );
		 }

		 private File IndexDir( long indexId )
		 {
			  return new File( IndexRoot(), Convert.ToString(indexId) );
		 }

		 private File IndexFile( long indexId )
		 {
			  // The indexFile location is independent of the configuredSettings, so we just use the defaults
			  return MakeIndexFile( indexId, new ConfiguredSpaceFillingCurveSettingsCache( Config.defaults() ) ).IndexFile;
		 }

		 private File IndexRoot()
		 {
			  return new File( new File( new File( _directory.databaseDir(), "schema" ), "index" ), "spatial-1.0" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void createEmptyIndex(org.Neo4Net.Kernel.Api.StorageEngine.schema.StoreIndexDescriptor schemaIndexDescriptor, org.Neo4Net.kernel.impl.index.schema.config.ConfiguredSpaceFillingCurveSettingsCache configuredSettings) throws java.io.IOException
		 private void CreateEmptyIndex( StoreIndexDescriptor schemaIndexDescriptor, ConfiguredSpaceFillingCurveSettingsCache configuredSettings )
		 {
			  SpatialIndexFiles.SpatialFileLayout fileLayout = MakeIndexFile( schemaIndexDescriptor.Id, configuredSettings ).LayoutForNewIndex;
			  SpatialIndexPopulator.PartPopulator populator = new SpatialIndexPopulator.PartPopulator( _pageCache, Fs, fileLayout, _monitor, schemaIndexDescriptor, new StandardConfiguration() );
			  populator.Create();
			  populator.Close( true );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void createEmptyIndex(org.Neo4Net.Kernel.Api.StorageEngine.schema.StoreIndexDescriptor schemaIndexDescriptor, SpatialIndexProvider provider) throws java.io.IOException
		 private void CreateEmptyIndex( StoreIndexDescriptor schemaIndexDescriptor, SpatialIndexProvider provider )
		 {
			  IndexPopulator populator = provider.GetPopulator( schemaIndexDescriptor, SamplingConfig(), heapBufferFactory(1024) );
			  populator.Create();
			  populator.Close( true );
		 }

		 private void VerifySpatialSettings( File indexFile, SpaceFillingCurveSettings expectedSettings )
		 {
			  try
			  {
					SpaceFillingCurveSettings settings = SpaceFillingCurveSettingsFactory.fromGBPTree( indexFile, _pageCache, NativeIndexHeaderReader.readFailureMessage );
					assertThat( "Should get correct results from header", settings, equalTo( expectedSettings ) );
			  }
			  catch ( IOException e )
			  {
					fail( "Failed to read GBPTree header: " + e.Message );
			  }
		 }

		 private static Config ConfigWithRange( double minX, double minY, double maxX, double maxY )
		 {
			  Setting<double> wgs84MinX = SpatialIndexSettings.makeCRSRangeSetting( CoordinateReferenceSystem.WGS84, 0, "min" );
			  Setting<double> wgs84MinY = SpatialIndexSettings.makeCRSRangeSetting( CoordinateReferenceSystem.WGS84, 1, "min" );
			  Setting<double> wgs84MaxX = SpatialIndexSettings.makeCRSRangeSetting( CoordinateReferenceSystem.WGS84, 0, "max" );
			  Setting<double> wgs84MaxY = SpatialIndexSettings.makeCRSRangeSetting( CoordinateReferenceSystem.WGS84, 1, "max" );
			  Config config = Config.defaults();
			  config.Augment( wgs84MinX, Convert.ToString( minX ) );
			  config.Augment( wgs84MinY, Convert.ToString( minY ) );
			  config.Augment( wgs84MaxX, Convert.ToString( maxX ) );
			  config.Augment( wgs84MaxY, Convert.ToString( maxY ) );
			  return config;
		 }
	}

}