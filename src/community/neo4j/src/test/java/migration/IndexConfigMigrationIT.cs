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
namespace Migration
{
	using Disabled = org.junit.jupiter.api.Disabled;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using Neo4Net.Graphdb.config;
	using GraphDatabaseBuilder = Neo4Net.Graphdb.factory.GraphDatabaseBuilder;
	using GraphDatabaseFactory = Neo4Net.Graphdb.factory.GraphDatabaseFactory;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using IndexDefinition = Neo4Net.Graphdb.schema.IndexDefinition;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using IndexReference = Neo4Net.Internal.Kernel.Api.IndexReference;
	using SchemaRead = Neo4Net.Internal.Kernel.Api.SchemaRead;
	using TokenRead = Neo4Net.Internal.Kernel.Api.TokenRead;
	using IndexNotFoundKernelException = Neo4Net.Internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using ZipUtils = Neo4Net.Io.compress.ZipUtils;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileUtils = Neo4Net.Io.fs.FileUtils;
	using SchemaDescriptorFactory = Neo4Net.Kernel.api.schema.SchemaDescriptorFactory;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using IndexProxy = Neo4Net.Kernel.Impl.Api.index.IndexProxy;
	using IndexingService = Neo4Net.Kernel.Impl.Api.index.IndexingService;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using Inject = Neo4Net.Test.extension.Inject;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using PointValue = Neo4Net.Values.Storable.PointValue;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.SchemaIndex.LUCENE10;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.SchemaIndex.NATIVE10;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.SchemaIndex.NATIVE20;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.SchemaIndex.NATIVE_BTREE10;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.fulltext.FulltextIndexSettings.INDEX_CONFIG_ANALYZER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.fulltext.FulltextIndexSettings.INDEX_CONFIG_EVENTUALLY_CONSISTENT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.config.SpatialIndexSettings.makeCRSRangeSetting;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.config.SpatialIndexSettings.space_filling_curve_max_bits;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.Unzip.unzip;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.CoordinateReferenceSystem.Cartesian;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.CoordinateReferenceSystem.Cartesian_3D;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.CoordinateReferenceSystem.WGS84;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.CoordinateReferenceSystem.WGS84_3D;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.CoordinateReferenceSystem.all;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.COMPARATOR;

	/// <summary>
	/// This test should verify that index configurations from a 3.5 store stay intact when opened again, with migration if needed.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(TestDirectoryExtension.class) class IndexConfigMigrationIT
	internal class IndexConfigMigrationIT
	{
		 private sealed class MinMaxSetting
		 {
			  public static readonly MinMaxSetting Wgs84MinX = new MinMaxSetting( "Wgs84MinX", InnerEnum.Wgs84MinX, makeCRSRangeSetting( WGS84, 0, "min" ), "-1" );
			  public static readonly MinMaxSetting Wgs84MinY = new MinMaxSetting( "Wgs84MinY", InnerEnum.Wgs84MinY, makeCRSRangeSetting( WGS84, 1, "min" ), "-2" );
			  public static readonly MinMaxSetting Wgs84MaxX = new MinMaxSetting( "Wgs84MaxX", InnerEnum.Wgs84MaxX, makeCRSRangeSetting( WGS84, 0, "max" ), "3" );
			  public static readonly MinMaxSetting Wgs84MaxY = new MinMaxSetting( "Wgs84MaxY", InnerEnum.Wgs84MaxY, makeCRSRangeSetting( WGS84, 1, "max" ), "4" );
			  public static readonly MinMaxSetting Wgs84_3DMinX = new MinMaxSetting( "Wgs84_3DMinX", InnerEnum.Wgs84_3DMinX, makeCRSRangeSetting( WGS84_3D, 0, "min" ), "-5" );
			  public static readonly MinMaxSetting Wgs84_3DMinY = new MinMaxSetting( "Wgs84_3DMinY", InnerEnum.Wgs84_3DMinY, makeCRSRangeSetting( WGS84_3D, 1, "min" ), "-6" );
			  public static readonly MinMaxSetting Wgs84_3DMinZ = new MinMaxSetting( "Wgs84_3DMinZ", InnerEnum.Wgs84_3DMinZ, makeCRSRangeSetting( WGS84_3D, 2, "min" ), "-7" );
			  public static readonly MinMaxSetting Wgs84_3DMaxX = new MinMaxSetting( "Wgs84_3DMaxX", InnerEnum.Wgs84_3DMaxX, makeCRSRangeSetting( WGS84_3D, 0, "max" ), "8" );
			  public static readonly MinMaxSetting Wgs84_3DMaxY = new MinMaxSetting( "Wgs84_3DMaxY", InnerEnum.Wgs84_3DMaxY, makeCRSRangeSetting( WGS84_3D, 1, "max" ), "9" );
			  public static readonly MinMaxSetting Wgs84_3DMaxZ = new MinMaxSetting( "Wgs84_3DMaxZ", InnerEnum.Wgs84_3DMaxZ, makeCRSRangeSetting( WGS84_3D, 2, "max" ), "10" );
			  public static readonly MinMaxSetting CartesianMinX = new MinMaxSetting( "CartesianMinX", InnerEnum.CartesianMinX, makeCRSRangeSetting( Cartesian, 0, "min" ), "-11" );
			  public static readonly MinMaxSetting CartesianMinY = new MinMaxSetting( "CartesianMinY", InnerEnum.CartesianMinY, makeCRSRangeSetting( Cartesian, 1, "min" ), "-12" );
			  public static readonly MinMaxSetting CartesianMaxX = new MinMaxSetting( "CartesianMaxX", InnerEnum.CartesianMaxX, makeCRSRangeSetting( Cartesian, 0, "max" ), "13" );
			  public static readonly MinMaxSetting CartesianMaxY = new MinMaxSetting( "CartesianMaxY", InnerEnum.CartesianMaxY, makeCRSRangeSetting( Cartesian, 1, "max" ), "14" );
			  public static readonly MinMaxSetting Cartesian_3DMinX = new MinMaxSetting( "Cartesian_3DMinX", InnerEnum.Cartesian_3DMinX, makeCRSRangeSetting( Cartesian_3D, 0, "min" ), "-15" );
			  public static readonly MinMaxSetting Cartesian_3DMinY = new MinMaxSetting( "Cartesian_3DMinY", InnerEnum.Cartesian_3DMinY, makeCRSRangeSetting( Cartesian_3D, 1, "min" ), "-16" );
			  public static readonly MinMaxSetting Cartesian_3DMinZ = new MinMaxSetting( "Cartesian_3DMinZ", InnerEnum.Cartesian_3DMinZ, makeCRSRangeSetting( Cartesian_3D, 2, "min" ), "-17" );
			  public static readonly MinMaxSetting Cartesian_3DMaxX = new MinMaxSetting( "Cartesian_3DMaxX", InnerEnum.Cartesian_3DMaxX, makeCRSRangeSetting( Cartesian_3D, 0, "max" ), "18" );
			  public static readonly MinMaxSetting Cartesian_3DMaxY = new MinMaxSetting( "Cartesian_3DMaxY", InnerEnum.Cartesian_3DMaxY, makeCRSRangeSetting( Cartesian_3D, 1, "max" ), "19" );
			  public static readonly MinMaxSetting Cartesian_3DMaxZ = new MinMaxSetting( "Cartesian_3DMaxZ", InnerEnum.Cartesian_3DMaxZ, makeCRSRangeSetting( Cartesian_3D, 2, "max" ), "20" );

			  private static readonly IList<MinMaxSetting> valueList = new List<MinMaxSetting>();

			  static MinMaxSetting()
			  {
				  valueList.Add( Wgs84MinX );
				  valueList.Add( Wgs84MinY );
				  valueList.Add( Wgs84MaxX );
				  valueList.Add( Wgs84MaxY );
				  valueList.Add( Wgs84_3DMinX );
				  valueList.Add( Wgs84_3DMinY );
				  valueList.Add( Wgs84_3DMinZ );
				  valueList.Add( Wgs84_3DMaxX );
				  valueList.Add( Wgs84_3DMaxY );
				  valueList.Add( Wgs84_3DMaxZ );
				  valueList.Add( CartesianMinX );
				  valueList.Add( CartesianMinY );
				  valueList.Add( CartesianMaxX );
				  valueList.Add( CartesianMaxY );
				  valueList.Add( Cartesian_3DMinX );
				  valueList.Add( Cartesian_3DMinY );
				  valueList.Add( Cartesian_3DMinZ );
				  valueList.Add( Cartesian_3DMaxX );
				  valueList.Add( Cartesian_3DMaxY );
				  valueList.Add( Cartesian_3DMaxZ );
			  }

			  public enum InnerEnum
			  {
				  Wgs84MinX,
				  Wgs84MinY,
				  Wgs84MaxX,
				  Wgs84MaxY,
				  Wgs84_3DMinX,
				  Wgs84_3DMinY,
				  Wgs84_3DMinZ,
				  Wgs84_3DMaxX,
				  Wgs84_3DMaxY,
				  Wgs84_3DMaxZ,
				  CartesianMinX,
				  CartesianMinY,
				  CartesianMaxX,
				  CartesianMaxY,
				  Cartesian_3DMinX,
				  Cartesian_3DMinY,
				  Cartesian_3DMinZ,
				  Cartesian_3DMaxX,
				  Cartesian_3DMaxY,
				  Cartesian_3DMaxZ
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  internal Private readonly;
			  internal Private readonly;

			  internal MinMaxSetting( string name, InnerEnum innerEnum, Neo4Net.Graphdb.config.Setting<double> setting, string settingValue )
			  {
					this._setting = setting;
					this._settingValue = settingValue;

				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			 public static IList<MinMaxSetting> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static MinMaxSetting valueOf( string name )
			 {
				 foreach ( MinMaxSetting enumInstance in MinMaxSetting.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

		 private const string SPACE_FILLING_CURVE_MAX_BITS_VALUE = "30";
		 private static readonly IDictionary<string, Value> _staticExpectedIndexConfig = new Dictionary<string, Value>();

		 static IndexConfigMigrationIT()
		 {
			  _staticExpectedIndexConfig["spatial.wgs-84.tableId"] = Values.intValue( 1 );
			  _staticExpectedIndexConfig["spatial.wgs-84.code"] = Values.intValue( 4326 );
			  _staticExpectedIndexConfig["spatial.wgs-84.dimensions"] = Values.intValue( 2 );
			  _staticExpectedIndexConfig["spatial.wgs-84.maxLevels"] = Values.intValue( 15 );
			  _staticExpectedIndexConfig["spatial.wgs-84.min"] = Values.doubleArray( new double[]{ -1.0, -2.0 } );
			  _staticExpectedIndexConfig["spatial.wgs-84.max"] = Values.doubleArray( new double[]{ 3.0, 4.0 } );

			  _staticExpectedIndexConfig["spatial.wgs-84-3d.tableId"] = Values.intValue( 1 );
			  _staticExpectedIndexConfig["spatial.wgs-84-3d.code"] = Values.intValue( 4979 );
			  _staticExpectedIndexConfig["spatial.wgs-84-3d.dimensions"] = Values.intValue( 3 );
			  _staticExpectedIndexConfig["spatial.wgs-84-3d.maxLevels"] = Values.intValue( 10 );
			  _staticExpectedIndexConfig["spatial.wgs-84-3d.min"] = Values.doubleArray( new double[]{ -5.0, -6.0, -7.0 } );
			  _staticExpectedIndexConfig["spatial.wgs-84-3d.max"] = Values.doubleArray( new double[]{ 8.0, 9.0, 10.0 } );

			  _staticExpectedIndexConfig["spatial.cartesian.tableId"] = Values.intValue( 2 );
			  _staticExpectedIndexConfig["spatial.cartesian.code"] = Values.intValue( 7203 );
			  _staticExpectedIndexConfig["spatial.cartesian.dimensions"] = Values.intValue( 2 );
			  _staticExpectedIndexConfig["spatial.cartesian.maxLevels"] = Values.intValue( 15 );
			  _staticExpectedIndexConfig["spatial.cartesian.min"] = Values.doubleArray( new double[]{ -11.0, -12.0 } );
			  _staticExpectedIndexConfig["spatial.cartesian.max"] = Values.doubleArray( new double[]{ 13.0, 14.0 } );

			  _staticExpectedIndexConfig["spatial.cartesian-3d.tableId"] = Values.intValue( 2 );
			  _staticExpectedIndexConfig["spatial.cartesian-3d.code"] = Values.intValue( 9157 );
			  _staticExpectedIndexConfig["spatial.cartesian-3d.dimensions"] = Values.intValue( 3 );
			  _staticExpectedIndexConfig["spatial.cartesian-3d.maxLevels"] = Values.intValue( 10 );
			  _staticExpectedIndexConfig["spatial.cartesian-3d.min"] = Values.doubleArray( new double[]{ -15.0, -16.0, -17.0 } );
			  _staticExpectedIndexConfig["spatial.cartesian-3d.max"] = Values.doubleArray( new double[]{ 18.0, 19.0, 20.0 } );
		 }

		 private const string ZIP_FILE_3_5 = "IndexConfigMigrationIT-3_5-db.zip";

		 // Schema index
		 private const string PROP_KEY = "key";
		 private static readonly Label _label1 = Label.label( "label1" );
		 private static readonly Label _label2 = Label.label( "label2" );
		 private static readonly Label _label3 = Label.label( "label3" );
		 private static readonly Label _label4 = Label.label( "label4" );
		 private static readonly Label[] _labels = new Label[] { _label1, _label2, _label3, _label4 };

		 // Fulltext index
		 private sealed class FulltextIndexDescription
		 {
			  public static readonly FulltextIndexDescription Both = new FulltextIndexDescription( "Both", InnerEnum.Both, "fulltextBoth", true, "fulltextToken1", AsConfigMap( "simple", true ) );
			  public static readonly FulltextIndexDescription AnalyzerOnly = new FulltextIndexDescription( "AnalyzerOnly", InnerEnum.AnalyzerOnly, "fulltextAnalyzer", false, "fulltextToken2", AsConfigMap( "russian" ) );
			  public static readonly FulltextIndexDescription EventuallyConsistenyOnly = new FulltextIndexDescription( "EventuallyConsistenyOnly", InnerEnum.EventuallyConsistenyOnly, "fulltextEC", true, "fulltextToken3", AsConfigMap( true ) );

			  private static readonly IList<FulltextIndexDescription> valueList = new List<FulltextIndexDescription>();

			  static FulltextIndexDescription()
			  {
				  valueList.Add( Both );
				  valueList.Add( AnalyzerOnly );
				  valueList.Add( EventuallyConsistenyOnly );
			  }

			  public enum InnerEnum
			  {
				  Both,
				  AnalyzerOnly,
				  EventuallyConsistenyOnly
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  internal Private readonly;
			  internal Private readonly;
			  internal Private readonly;
			  internal Private readonly;

			  internal FulltextIndexDescription( string name, InnerEnum innerEnum, string indexName, bool nodeIndex, string tokenName, IDictionary<string, Neo4Net.Values.Storable.Value> configMap )
			  {
					this._indexName = indexName;
					this._tokenName = tokenName;
					this._configMap = configMap;
					this._indexProcedure = nodeIndex ? "createNodeIndex" : "createRelationshipIndex";

				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			 public static IList<FulltextIndexDescription> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static FulltextIndexDescription valueOf( string name )
			 {
				 foreach ( FulltextIndexDescription enumInstance in FulltextIndexDescription.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.TestDirectory directory;
		 private TestDirectory _directory;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static java.io.File tempStoreDirectory() throws java.io.IOException
		 private static File TempStoreDirectory()
		 {
			  File file = File.createTempFile( "create-db", "neo4j" );
			  File storeDir = new File( file.AbsoluteFile.ParentFile, file.Name );
			  FileUtils.deleteFile( file );
			  return storeDir;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Disabled("Here as reference for how 3.5 db was created") @Test void create3_5Database() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void Create3_5Database()
		 {
			  File storeDir = TempStoreDirectory();
			  GraphDatabaseBuilder builder = ( new GraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(storeDir);
			  SpatialConfig = builder;

			  GraphDatabaseService db = builder.NewGraphDatabase();
			  CreateIndex( db, NATIVE_BTREE10.providerName(), _label1 );
			  CreateIndex( db, NATIVE20.providerName(), _label2 );
			  CreateIndex( db, NATIVE10.providerName(), _label3 );
			  CreateIndex( db, LUCENE10.providerName(), _label4 );
			  CreateSpatialData( db, _label1, _label2, _label3, _label4 );
			  foreach ( FulltextIndexDescription fulltextIndex in FulltextIndexDescription.values() )
			  {
					CreateFulltextIndex( db, fulltextIndex.indexProcedure, fulltextIndex.indexName, fulltextIndex.tokenName, PROP_KEY, fulltextIndex.configMap );
			  }
			  Db.shutdown();

			  File zipFile = new File( storeDir.ParentFile, storeDir.Name + ".zip" );
			  ZipUtils.zip( new DefaultFileSystemAbstraction(), storeDir, zipFile );
			  Console.WriteLine( "Db created in " + zipFile.AbsolutePath );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHaveCorrectDataAndIndexConfiguration() throws java.io.IOException, org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldHaveCorrectDataAndIndexConfiguration()
		 {
			  File storeDir = _directory.databaseDir();
			  unzip( this.GetType(), ZIP_FILE_3_5, storeDir );
			  // when
			  GraphDatabaseAPI db = ( GraphDatabaseAPI ) ( new GraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(storeDir).setConfig(GraphDatabaseSettings.allow_upgrade, Settings.TRUE).newGraphDatabase();
			  try
			  {
					  using ( Transaction tx = Db.beginTx() )
					  {
						ISet<CoordinateReferenceSystem> allCRS = Iterables.asSet( all() );
						HasIndexCount( db, 7 );
						foreach ( Node node in Db.AllNodes )
						{
							 HasLabels( node, _label1, _label2, _label3, _label4 );
							 object property = node.GetProperty( PROP_KEY );
							 if ( property is PointValue )
							 {
								  allCRS.remove( ( ( PointValue ) property ).CoordinateReferenceSystem );
							 }
						}
						assertTrue( allCRS.Count == 0, "Expected all CRS to be represented in store, but missing " + allCRS );
						AssertIndexConfiguration( db );
						AssertFulltextIndexConfiguration( db );
						tx.Success();
					  }
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void assertIndexConfiguration(org.neo4j.kernel.internal.GraphDatabaseAPI db) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 private static void AssertIndexConfiguration( GraphDatabaseAPI db )
		 {
			  foreach ( Label label in _labels )
			  {
					IDictionary<string, Value> actualIndexConfig = GetIndexConfig( db, label, PROP_KEY );
					IDictionary<string, Value> expectedIndexConfig = new Dictionary<string, Value>( _staticExpectedIndexConfig );
					foreach ( KeyValuePair<string, Value> entry in actualIndexConfig.SetOfKeyValuePairs() )
					{
						 string actualKey = entry.Key;
						 Value actualValue = entry.Value;
						 Value expectedValue = expectedIndexConfig.Remove( actualKey );
						 assertNotNull( expectedValue, "Actual index config had map entry that was not among expected " + entry );
						 assertEquals( 0, COMPARATOR.compare( expectedValue, actualValue ), format( "Expected and actual index config value differed for %s, expected %s but was %s.", actualKey, expectedValue, actualValue ) );
					}
					assertTrue( expectedIndexConfig.Count == 0, "Actual index config was missing some values: " + expectedIndexConfig );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void assertFulltextIndexConfiguration(org.neo4j.kernel.internal.GraphDatabaseAPI db) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 private static void AssertFulltextIndexConfiguration( GraphDatabaseAPI db )
		 {
			  foreach ( FulltextIndexDescription fulltextIndex in FulltextIndexDescription.values() )
			  {
					IDictionary<string, Value> actualIndexConfig = GetFulltextIndexConfig( db, fulltextIndex.indexName );
					foreach ( KeyValuePair<string, Value> expectedEntry in fulltextIndex.configMap.entrySet() )
					{
						 Value actualValue = actualIndexConfig[expectedEntry.Key];
						 assertEquals( expectedEntry.Value, actualValue, format( "Index did not have expected config, %s.%nExpected: %s%nActual: %s ", fulltextIndex.indexName, fulltextIndex.configMap, actualIndexConfig ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static java.util.Map<String,org.neo4j.values.storable.Value> getFulltextIndexConfig(org.neo4j.kernel.internal.GraphDatabaseAPI db, String indexName) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 private static IDictionary<string, Value> GetFulltextIndexConfig( GraphDatabaseAPI db, string indexName )
		 {
			  IndexingService indexingService = GetIndexingService( db );
			  IndexReference indexReference = SchemaRead( db ).indexGetForName( indexName );
			  IndexProxy indexProxy = indexingService.GetIndexProxy( indexReference.Schema() );
			  return indexProxy.IndexConfig();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("SameParameterValue") private static java.util.Map<String,org.neo4j.values.storable.Value> getIndexConfig(org.neo4j.kernel.internal.GraphDatabaseAPI db, org.neo4j.graphdb.Label label, String propKey) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 private static IDictionary<string, Value> GetIndexConfig( GraphDatabaseAPI db, Label label, string propKey )
		 {
			  TokenRead tokenRead = tokenRead( db );
			  IndexingService indexingService = GetIndexingService( db );
			  int labelId = tokenRead.NodeLabel( label.Name() );
			  int propKeyId = tokenRead.PropertyKey( propKey );
			  IndexProxy indexProxy = indexingService.getIndexProxy( SchemaDescriptorFactory.forLabel( labelId, propKeyId ) );
			  return indexProxy.IndexConfig();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("SameParameterValue") private static void hasIndexCount(org.neo4j.kernel.internal.GraphDatabaseAPI db, int expectedIndexCount)
		 private static void HasIndexCount( GraphDatabaseAPI db, int expectedIndexCount )
		 {
			  IEnumerable<IndexDefinition> indexes = Db.schema().Indexes;
			  long actualIndexCount = Iterables.count( indexes );
			  assertEquals( expectedIndexCount, actualIndexCount, "Expected there to be " + expectedIndexCount + " indexes but was " + actualIndexCount );
		 }

		 private static void HasLabels( Node node, params Label[] labels )
		 {
			  foreach ( Label label in labels )
			  {
					assertTrue( node.HasLabel( label ), "Did not have label " + label );
			  }
		 }

		 private static void CreateSpatialData( GraphDatabaseService db, params Label[] labels )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					foreach ( CoordinateReferenceSystem crs in all() )
					{
						 Node node = Db.createNode( labels );
						 int dim = crs.Dimension;
						 double[] coords = new double[dim];
						 node.SetProperty( PROP_KEY, Values.pointValue( crs, coords ) );
					}
					tx.Success();
			  }
		 }

		 private static void CreateIndex( GraphDatabaseService db, string providerName, Label label )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					string indexPattern = format( "\":%s(%s)\"", label.Name(), PROP_KEY );
					string indexProvider = "\"" + providerName + "\"";
					Db.execute( format( "CALL db.createIndex( %s, %s )", indexPattern, indexProvider ) ).close();
					tx.Success();
			  }
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
					tx.Success();
			  }
		 }

		 private static void CreateFulltextIndex( GraphDatabaseService db, string indexProcedure, string fulltextName, string token, string propKey, IDictionary<string, Value> configMap )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					string labelArray = Array( token );
					string propArray = Array( propKey );
					string configString = AsConfigString( configMap );
					Console.WriteLine( fulltextName + " created with config: " + configString );
					string query = format( "CALL db.index.fulltext." + indexProcedure + "(\"%s\", %s, %s, %s )", fulltextName, labelArray, propArray, configString );
					Db.execute( query ).close();
					tx.Success();
			  }
		 }

		 private static IDictionary<string, Value> AsConfigMap( string analyzer, bool eventuallyConsistent )
		 {
			  IDictionary<string, Value> map = new Dictionary<string, Value>();
			  map[INDEX_CONFIG_ANALYZER] = Values.stringValue( analyzer );
			  map[INDEX_CONFIG_EVENTUALLY_CONSISTENT] = Values.booleanValue( eventuallyConsistent );
			  return map;
		 }

		 private static IDictionary<string, Value> AsConfigMap( string analyzer )
		 {
			  IDictionary<string, Value> map = new Dictionary<string, Value>();
			  map[INDEX_CONFIG_ANALYZER] = Values.stringValue( analyzer );
			  return map;
		 }

		 private static IDictionary<string, Value> AsConfigMap( bool eventuallyConsistent )
		 {
			  IDictionary<string, Value> map = new Dictionary<string, Value>();
			  map[INDEX_CONFIG_EVENTUALLY_CONSISTENT] = Values.booleanValue( eventuallyConsistent );
			  return map;
		 }

		 private static string AsConfigString( IDictionary<string, Value> configMap )
		 {
			  StringJoiner joiner = new StringJoiner( ", ", "{", "}" );
			  configMap.forEach( ( k, v ) => joiner.add( k + ": \"" + v.asObject() + "\"" ) );
			  return joiner.ToString();
		 }

		 private static string Array( params string[] args )
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  return java.util.args.collect( Collectors.joining( "\", \"", "[\"", "\"]" ) );
		 }

		 private static GraphDatabaseBuilder SpatialConfig
		 {
			 set
			 {
				  value.setConfig( space_filling_curve_max_bits, SPACE_FILLING_CURVE_MAX_BITS_VALUE );
				  foreach ( MinMaxSetting minMaxSetting in MinMaxSetting.values() )
				  {
						value.setConfig( minMaxSetting.setting, minMaxSetting.settingValue );
				  }
			 }
		 }

		 private static IndexingService GetIndexingService( GraphDatabaseAPI db )
		 {
			  return Db.DependencyResolver.resolveDependency( typeof( IndexingService ) );
		 }

		 private static TokenRead TokenRead( GraphDatabaseAPI db )
		 {
			  return Db.DependencyResolver.resolveDependency( typeof( ThreadToStatementContextBridge ) ).getKernelTransactionBoundToThisThread( false ).tokenRead();
		 }

		 private static SchemaRead SchemaRead( GraphDatabaseAPI db )
		 {
			  return Db.DependencyResolver.resolveDependency( typeof( ThreadToStatementContextBridge ) ).getKernelTransactionBoundToThisThread( false ).schemaRead();
		 }
	}

}