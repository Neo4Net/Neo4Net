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
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using SpaceFillingCurve = Neo4Net.Gis.Spatial.Index.curves.SpaceFillingCurve;
	using StandardConfiguration = Neo4Net.Gis.Spatial.Index.curves.StandardConfiguration;
	using RecoveryCleanupWorkCollector = Neo4Net.Index.Internal.gbptree.RecoveryCleanupWorkCollector;
	using IndexOrder = Neo4Net.Kernel.Api.Internal.IndexOrder;
	using IndexQuery = Neo4Net.Kernel.Api.Internal.IndexQuery;
	using IndexNotApplicableKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotApplicableKernelException;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using IndexDirectoryStructure = Neo4Net.Kernel.Api.Index.IndexDirectoryStructure;
	using Neo4Net.Kernel.Api.Index;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using Config = Neo4Net.Kernel.configuration.Config;
	using IndexUpdateMode = Neo4Net.Kernel.Impl.Api.index.IndexUpdateMode;
	using ConfiguredSpaceFillingCurveSettingsCache = Neo4Net.Kernel.Impl.Index.Schema.config.ConfiguredSpaceFillingCurveSettingsCache;
	using IndexSpecificSpaceFillingCurveSettingsCache = Neo4Net.Kernel.Impl.Index.Schema.config.IndexSpecificSpaceFillingCurveSettingsCache;
	using IndexReader = Neo4Net.Kernel.Api.StorageEngine.schema.IndexReader;
	using SimpleNodeValueClient = Neo4Net.Kernel.Api.StorageEngine.schema.SimpleNodeValueClient;
	using StoreIndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.StoreIndexDescriptor;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using PointArray = Neo4Net.Values.Storable.PointArray;
	using PointValue = Neo4Net.Values.Storable.PointValue;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.rules.RuleChain.outerRule;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.index.IndexProvider.Monitor_Fields.EMPTY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.rule.PageCacheRule.config;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.CoordinateReferenceSystem.WGS84;

	public class GenericAccessorPointsTest
	{
		private bool InstanceFieldsInitialized = false;

		public GenericAccessorPointsTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_directory = TestDirectory.testDirectory( this.GetType(), _fs.get() );
			Rules = outerRule( _random ).around( _fs ).around( _directory ).around( _pageCacheRule );
		}

		 private static readonly CoordinateReferenceSystem _crs = CoordinateReferenceSystem.WGS84;
		 private static readonly ConfiguredSpaceFillingCurveSettingsCache _configuredSettings = new ConfiguredSpaceFillingCurveSettingsCache( Config.defaults() );
		 private static readonly IndexSpecificSpaceFillingCurveSettingsCache _indexSettings = new IndexSpecificSpaceFillingCurveSettingsCache( _configuredSettings, new Dictionary<CoordinateReferenceSystem, SpaceFillingCurveSettings>() );
		 private static readonly SpaceFillingCurve _curve = _indexSettings.forCrs( _crs, true );

		 private readonly DefaultFileSystemRule _fs = new DefaultFileSystemRule();
		 private TestDirectory _directory;
		 private readonly PageCacheRule _pageCacheRule = new PageCacheRule( config().withAccessChecks(true) );
		 private readonly RandomRule _random = new RandomRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain rules = outerRule(random).around(fs).around(directory).around(pageCacheRule);
		 public RuleChain Rules;

		 private NativeIndexAccessor _accessor;
		 private StoreIndexDescriptor _descriptor;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  DefaultFileSystemAbstraction fs = this._fs.get();
			  PageCache pc = _pageCacheRule.getPageCache( fs );
			  File file = _directory.file( "index" );
			  GenericLayout layout = new GenericLayout( 1, _indexSettings );
			  RecoveryCleanupWorkCollector collector = RecoveryCleanupWorkCollector.ignore();
			  _descriptor = TestIndexDescriptorFactory.forLabel( 1, 1 ).withId( 1 );
			  IndexDirectoryStructure.Factory factory = IndexDirectoryStructure.directoriesByProvider( _directory.storeDir() );
			  IndexDirectoryStructure structure = factory.ForProvider( GenericNativeIndexProvider.Descriptor );
			  IndexDropAction dropAction = new FileSystemIndexDropAction( fs, structure );
			  _accessor = new GenericNativeIndexAccessor( pc, fs, file, layout, collector, EMPTY, _descriptor, _indexSettings, new StandardConfiguration(), dropAction, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  _accessor.close();
		 }

		 /// <summary>
		 /// This test verify that we correctly handle unique points that all belong to the same tile on the space filling curve.
		 /// All points share at least one dimension coordinate with another point to exercise minimal splitter.
		 /// We verify this by asserting that we always get exactly one hit on an exact match and that the value is what we expect.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustHandlePointsWithinSameTile() throws org.Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException, org.Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotApplicableKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustHandlePointsWithinSameTile()
		 {
			  // given
			  // Many random points that all are close enough to each other to belong to the same tile on the space filling curve.
			  int nbrOfValues = 10000;
			  PointValue origin = Values.pointValue( WGS84, 0.0, 0.0 );
			  long? derivedValueForCenterPoint = _curve.derivedValueFor( origin.Coordinate() );
			  double[] centerPoint = _curve.centerPointFor( derivedValueForCenterPoint.Value );
			  double xWidthMultiplier = _curve.getTileWidth( 0, _curve.MaxLevel ) / 2;
			  double yWidthMultiplier = _curve.getTileWidth( 1, _curve.MaxLevel ) / 2;

			  IList<Value> pointValues = new List<Value>();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<org.Neo4Net.kernel.api.index.IndexEntryUpdate<?>> updates = new java.util.ArrayList<>();
			  IList<IndexEntryUpdate<object>> updates = new List<IndexEntryUpdate<object>>();
			  long nodeId = 1;
			  for ( int i = 0; i < nbrOfValues / 4; i++ )
			  {
					double x1 = ( _random.NextDouble() * 2 - 1 ) * xWidthMultiplier;
					double x2 = ( _random.NextDouble() * 2 - 1 ) * xWidthMultiplier;
					double y1 = ( _random.NextDouble() * 2 - 1 ) * yWidthMultiplier;
					double y2 = ( _random.NextDouble() * 2 - 1 ) * yWidthMultiplier;
					PointValue value11 = Values.pointValue( WGS84, centerPoint[0] + x1, centerPoint[1] + y1 );
					PointValue value12 = Values.pointValue( WGS84, centerPoint[0] + x1, centerPoint[1] + y2 );
					PointValue value21 = Values.pointValue( WGS84, centerPoint[0] + x2, centerPoint[1] + y1 );
					PointValue value22 = Values.pointValue( WGS84, centerPoint[0] + x2, centerPoint[1] + y2 );
					AssertDerivedValue( derivedValueForCenterPoint, value11, value12, value21, value22 );

					nodeId = AddPointsToLists( pointValues, updates, nodeId, value11, value12, value21, value22 );
			  }

			  ProcessAll( updates );

			  // then
			  ExactMatchOnAllValues( pointValues );
		 }

		 /// <summary>
		 /// This test verify that we correctly handle unique point arrays where every point in every array belong to the same tile on the space filling curve.
		 /// We verify this by asserting that we always get exactly one hit on an exact match and that the value is what we expect.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustHandlePointArraysWithinSameTile() throws org.Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException, org.Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotApplicableKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustHandlePointArraysWithinSameTile()
		 {
			  // given
			  // Many random points that all are close enough to each other to belong to the same tile on the space filling curve.
			  int nbrOfValues = 10000;
			  PointValue origin = Values.pointValue( WGS84, 0.0, 0.0 );
			  long? derivedValueForCenterPoint = _curve.derivedValueFor( origin.Coordinate() );
			  double[] centerPoint = _curve.centerPointFor( derivedValueForCenterPoint.Value );
			  double xWidthMultiplier = _curve.getTileWidth( 0, _curve.MaxLevel ) / 2;
			  double yWidthMultiplier = _curve.getTileWidth( 1, _curve.MaxLevel ) / 2;

			  IList<Value> pointArrays = new List<Value>();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<org.Neo4Net.kernel.api.index.IndexEntryUpdate<?>> updates = new java.util.ArrayList<>();
			  IList<IndexEntryUpdate<object>> updates = new List<IndexEntryUpdate<object>>();
			  for ( int i = 0; i < nbrOfValues; i++ )
			  {
					int arrayLength = _random.Next( 5 ) + 1;
					PointValue[] pointValues = new PointValue[arrayLength];
					for ( int j = 0; j < arrayLength; j++ )
					{
						 double x = ( _random.NextDouble() * 2 - 1 ) * xWidthMultiplier;
						 double y = ( _random.NextDouble() * 2 - 1 ) * yWidthMultiplier;
						 PointValue value = Values.pointValue( WGS84, centerPoint[0] + x, centerPoint[1] + y );

						 AssertDerivedValue( derivedValueForCenterPoint, value );
						 pointValues[j] = value;
					}
					PointArray array = Values.pointArray( pointValues );
					pointArrays.Add( array );
					updates.Add( IndexEntryUpdate.add( i, _descriptor, array ) );
			  }

			  ProcessAll( updates );

			  // then
			  ExactMatchOnAllValues( pointArrays );
		 }

		 /// <summary>
		 /// The test mustHandlePointArraysWithinSameTile was flaky on random numbers that placed points just
		 /// within the tile upper bound, and allocated points to adjacent tiles due to rounding errors.
		 /// This test uses a specific point that triggers that exact failure in a non-flaky way.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotGetRoundingErrorsWithPointsJustWithinTheTileUpperBound()
		 public virtual void ShouldNotGetRoundingErrorsWithPointsJustWithinTheTileUpperBound()
		 {
			  PointValue origin = Values.pointValue( WGS84, 0.0, 0.0 );
			  long derivedValueForCenterPoint = _curve.derivedValueFor( origin.Coordinate() ).Value;
			  double[] centerPoint = _curve.centerPointFor( derivedValueForCenterPoint ); // [1.6763806343078613E-7, 8.381903171539307E-8]

			  double xWidthMultiplier = _curve.getTileWidth( 0, _curve.MaxLevel ) / 2; // 1.6763806343078613E-7
			  double yWidthMultiplier = _curve.getTileWidth( 1, _curve.MaxLevel ) / 2; // 8.381903171539307E-8

			  double[] faultyCoords = new double[]{ 1.874410632171803E-8, 1.6763806281859016E-7 };

			  assertTrue( "inside upper x limit", centerPoint[0] + xWidthMultiplier > faultyCoords[0] );
			  assertTrue( "inside lower x limit", centerPoint[0] - xWidthMultiplier < faultyCoords[0] );

			  assertTrue( "inside upper y limit", centerPoint[1] + yWidthMultiplier > faultyCoords[1] );
			  assertTrue( "inside lower y limit", centerPoint[1] - yWidthMultiplier < faultyCoords[1] );

			  long derivedValueForFaultyCoords = _curve.derivedValueFor( faultyCoords ).Value;
			  assertEquals( derivedValueForCenterPoint, derivedValueForFaultyCoords, "expected same derived value" );
		 }

		 private long AddPointsToLists<T1>( IList<Value> pointValues, IList<T1> updates, long nodeId, params PointValue[] values )
		 {
			  foreach ( PointValue value in values )
			  {
					pointValues.Add( value );
					updates.Add( IndexEntryUpdate.add( nodeId++, _descriptor, value ) );
			  }
			  return nodeId;
		 }

		 private void AssertDerivedValue( long? targetDerivedValue, params PointValue[] values )
		 {
			  foreach ( PointValue value in values )
			  {
					long? derivedValueForValue = _curve.derivedValueFor( value.Coordinate() );
					assertEquals( targetDerivedValue, derivedValueForValue, "expected random value to belong to same tile as center point" );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void processAll(java.util.List<org.Neo4Net.kernel.api.index.IndexEntryUpdate<?>> updates) throws org.Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException
		 private void ProcessAll<T1>( IList<T1> updates )
		 {
			  using ( NativeIndexUpdater updater = _accessor.newUpdater( IndexUpdateMode.ONLINE ) )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (org.Neo4Net.kernel.api.index.IndexEntryUpdate<?> update : updates)
					foreach ( IndexEntryUpdate<object> update in updates )
					{
						 //noinspection unchecked
						 updater.process( update );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void exactMatchOnAllValues(java.util.List<org.Neo4Net.values.storable.Value> values) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotApplicableKernelException
		 private void ExactMatchOnAllValues( IList<Value> values )
		 {
			  using ( IndexReader indexReader = _accessor.newReader() )
			  {
					SimpleNodeValueClient client = new SimpleNodeValueClient();
					foreach ( Value value in values )
					{
						 IndexQuery.ExactPredicate exact = IndexQuery.exact( _descriptor.schema().PropertyId, value );
						 indexReader.Query( client, IndexOrder.NONE, true, exact );

						 // then
						 assertTrue( client.Next() );
						 assertEquals( value, client.Values[0] );
						 assertFalse( client.Next() );
					}
			  }
		 }
	}

}