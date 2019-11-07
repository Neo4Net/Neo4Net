using System;
using System.Collections.Generic;
using System.Text;

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
namespace Neo4Net.Dbms.CommandLine
{
	using MutableLong = org.apache.commons.lang3.mutable.MutableLong;
	using Matcher = org.hamcrest.Matcher;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using OutsideWorld = Neo4Net.CommandLine.Admin.OutsideWorld;
	using RealOutsideWorld = Neo4Net.CommandLine.Admin.RealOutsideWorld;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Label = Neo4Net.GraphDb.Label;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Neo4Net.GraphDb.config;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using FailureStorage = Neo4Net.Kernel.Api.Impl.Index.storage.FailureStorage;
	using IndexDirectoryStructure = Neo4Net.Kernel.Api.Index.IndexDirectoryStructure;
	using StoreType = Neo4Net.Kernel.impl.store.StoreType;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using RandomValues = Neo4Net.Values.Storable.RandomValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.both;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.commandline.dbms.MemoryRecommendationsCommand.bytesToString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.commandline.dbms.MemoryRecommendationsCommand.recommendHeapMemory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.commandline.dbms.MemoryRecommendationsCommand.recommendOsMemory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.commandline.dbms.MemoryRecommendationsCommand.recommendPageCacheMemory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.configuration.ExternalSettings.initialHeapSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.configuration.ExternalSettings.maxHeapSize;
	using static Neo4Net.GraphDb.factory.GraphDatabaseSettings.SchemaIndex;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.factory.GraphDatabaseSettings.active_database;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.factory.GraphDatabaseSettings.data_directory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.factory.GraphDatabaseSettings.database_path;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.factory.GraphDatabaseSettings.default_schema_provider;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.factory.GraphDatabaseSettings.pagecache_memory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.ArrayUtil.array;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.MapUtil.load;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.MapUtil.store;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.io.ByteUnit.exbiBytes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.io.ByteUnit.gibiBytes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.io.ByteUnit.mebiBytes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.io.ByteUnit.tebiBytes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.configuration.Config.DEFAULT_CONFIG_FILE_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.configuration.Config.fromFile;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.configuration.Settings.BYTES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.configuration.Settings.buildSetting;

	public class MemoryRecommendationsCommandTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.TestDirectory directory = Neo4Net.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory Directory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustRecommendOSMemory()
		 public virtual void MustRecommendOSMemory()
		 {
			  assertThat( recommendOsMemory( mebiBytes( 100 ) ), Between( mebiBytes( 65 ), mebiBytes( 75 ) ) );
			  assertThat( recommendOsMemory( gibiBytes( 1 ) ), Between( mebiBytes( 650 ), mebiBytes( 750 ) ) );
			  assertThat( recommendOsMemory( gibiBytes( 3 ) ), Between( mebiBytes( 1256 ), mebiBytes( 1356 ) ) );
			  assertThat( recommendOsMemory( gibiBytes( 192 ) ), Between( gibiBytes( 17 ), gibiBytes( 19 ) ) );
			  assertThat( recommendOsMemory( gibiBytes( 1920 ) ), greaterThan( gibiBytes( 29 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustRecommendHeapMemory()
		 public virtual void MustRecommendHeapMemory()
		 {
			  assertThat( recommendHeapMemory( mebiBytes( 100 ) ), Between( mebiBytes( 25 ), mebiBytes( 35 ) ) );
			  assertThat( recommendHeapMemory( gibiBytes( 1 ) ), Between( mebiBytes( 300 ), mebiBytes( 350 ) ) );
			  assertThat( recommendHeapMemory( gibiBytes( 3 ) ), Between( mebiBytes( 1256 ), mebiBytes( 1356 ) ) );
			  assertThat( recommendHeapMemory( gibiBytes( 6 ) ), Between( mebiBytes( 3000 ), mebiBytes( 3200 ) ) );
			  assertThat( recommendHeapMemory( gibiBytes( 192 ) ), Between( gibiBytes( 30 ), gibiBytes( 32 ) ) );
			  assertThat( recommendHeapMemory( gibiBytes( 1920 ) ), Between( gibiBytes( 30 ), gibiBytes( 32 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustRecommendPageCacheMemory()
		 public virtual void MustRecommendPageCacheMemory()
		 {
			  assertThat( recommendPageCacheMemory( mebiBytes( 100 ) ), Between( mebiBytes( 7 ), mebiBytes( 12 ) ) );
			  assertThat( recommendPageCacheMemory( gibiBytes( 1 ) ), Between( mebiBytes( 50 ), mebiBytes( 60 ) ) );
			  assertThat( recommendPageCacheMemory( gibiBytes( 3 ) ), Between( mebiBytes( 470 ), mebiBytes( 530 ) ) );
			  assertThat( recommendPageCacheMemory( gibiBytes( 6 ) ), Between( mebiBytes( 980 ), mebiBytes( 1048 ) ) );
			  assertThat( recommendPageCacheMemory( gibiBytes( 192 ) ), Between( gibiBytes( 140 ), gibiBytes( 150 ) ) );
			  assertThat( recommendPageCacheMemory( gibiBytes( 1920 ) ), Between( gibiBytes( 1850 ), gibiBytes( 1900 ) ) );

			  // Also never recommend more than 16 TiB of page cache memory, regardless of how much is available.
			  assertThat( recommendPageCacheMemory( exbiBytes( 1 ) ), lessThan( tebiBytes( 17 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void bytesToStringMustBeParseableBySettings()
		 public virtual void BytesToStringMustBeParseableBySettings()
		 {
			  Setting<long> setting = buildSetting( "arg", BYTES ).build();
			  for ( int i = 1; i < 10_000; i++ )
			  {
					int mebibytes = 75 * i;
					long expectedBytes = mebiBytes( mebibytes );
					string bytesToString = bytesToString( expectedBytes );
					long actualBytes = setting.apply( s => bytesToString );
					long tenPercent = ( long )( expectedBytes * 0.1 );
					assertThat( mebibytes + "m", actualBytes, Between( expectedBytes - tenPercent, expectedBytes + tenPercent ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustPrintRecommendationsAsConfigReadableOutput() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustPrintRecommendationsAsConfigReadableOutput()
		 {
			  StringBuilder output = new StringBuilder();
			  Path homeDir = Paths.get( "home" );
			  Path configDir = Paths.get( "home", "config" );
			  OutsideWorld outsideWorld = new RealOutsideWorldAnonymousInnerClass( this, output );
			  MemoryRecommendationsCommand command = new MemoryRecommendationsCommand( homeDir, configDir, outsideWorld );
			  string heap = bytesToString( recommendHeapMemory( gibiBytes( 8 ) ) );
			  string pagecache = bytesToString( recommendPageCacheMemory( gibiBytes( 8 ) ) );

			  command.Execute( new string[]{ "--memory=8g" } );

			  IDictionary<string, string> stringMap = load( new StringReader( output.ToString() ) );
			  assertThat( stringMap[initialHeapSize.name()], @is(heap) );
			  assertThat( stringMap[maxHeapSize.name()], @is(heap) );
			  assertThat( stringMap[pagecache_memory.name()], @is(pagecache) );
		 }

		 private class RealOutsideWorldAnonymousInnerClass : RealOutsideWorld
		 {
			 private readonly MemoryRecommendationsCommandTest _outerInstance;

			 private StringBuilder _output;

			 public RealOutsideWorldAnonymousInnerClass( MemoryRecommendationsCommandTest outerInstance, StringBuilder output )
			 {
				 this.outerInstance = outerInstance;
				 this._output = output;
			 }

			 public override void stdOutLine( string text )
			 {
				  _output.Append( text ).Append( Environment.NewLine );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPrintKilobytesEvenForByteSizeBelowAKiloByte()
		 public virtual void ShouldPrintKilobytesEvenForByteSizeBelowAKiloByte()
		 {
			  // given
			  long bytesBelowK = 176;
			  long bytesBelow10K = 1762;
			  long bytesBelow100K = 17625;

			  // when
			  string stringBelowK = MemoryRecommendationsCommand.BytesToString( bytesBelowK );
			  string stringBelow10K = MemoryRecommendationsCommand.BytesToString( bytesBelow10K );
			  string stringBelow100K = MemoryRecommendationsCommand.BytesToString( bytesBelow100K );

			  // then
			  assertThat( stringBelowK, @is( "1k" ) );
			  assertThat( stringBelow10K, @is( "2k" ) );
			  assertThat( stringBelow100K, @is( "18k" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustPrintMinimalPageCacheMemorySettingForConfiguredDb() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustPrintMinimalPageCacheMemorySettingForConfiguredDb()
		 {
			  // given
			  StringBuilder output = new StringBuilder();
			  Path homeDir = Directory.directory().toPath();
			  Path configDir = homeDir.resolve( "conf" );
			  configDir.toFile().mkdirs();
			  Path configFile = configDir.resolve( DEFAULT_CONFIG_FILE_NAME );
			  string databaseName = "mydb";
			  store( stringMap( data_directory.name(), homeDir.ToString() ), configFile.toFile() );
			  File databaseDirectory = fromFile( configFile ).withHome( homeDir ).withSetting( active_database, databaseName ).build().get(database_path);
			  CreateDatabaseWithNativeIndexes( databaseDirectory );
			  OutsideWorld outsideWorld = new OutputCaptureOutsideWorld( output );
			  MemoryRecommendationsCommand command = new MemoryRecommendationsCommand( homeDir, configDir, outsideWorld );
			  string heap = bytesToString( recommendHeapMemory( gibiBytes( 8 ) ) );
			  string pagecache = bytesToString( recommendPageCacheMemory( gibiBytes( 8 ) ) );

			  // when
			  command.Execute( array( "--database", databaseName, "--memory", "8g" ) );

			  // then
			  string memrecString = output.ToString();
			  IDictionary<string, string> stringMap = load( new StringReader( memrecString ) );
			  assertThat( stringMap[initialHeapSize.name()], @is(heap) );
			  assertThat( stringMap[maxHeapSize.name()], @is(heap) );
			  assertThat( stringMap[pagecache_memory.name()], @is(pagecache) );

			  long[] expectedSizes = CalculatePageCacheFileSize( DatabaseLayout.of( databaseDirectory ) );
			  long expectedPageCacheSize = expectedSizes[0];
			  long expectedLuceneSize = expectedSizes[1];
			  assertThat( memrecString, containsString( "Lucene indexes: " + bytesToString( expectedLuceneSize ) ) );
			  assertThat( memrecString, containsString( "Data volume and native indexes: " + bytesToString( expectedPageCacheSize ) ) );
		 }

		 private static Matcher<long> Between( long lowerBound, long upperBound )
		 {
			  return both( greaterThanOrEqualTo( lowerBound ) ).and( lessThanOrEqualTo( upperBound ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static long[] calculatePageCacheFileSize(Neo4Net.io.layout.DatabaseLayout databaseLayout) throws java.io.IOException
		 private static long[] CalculatePageCacheFileSize( DatabaseLayout databaseLayout )
		 {
			  MutableLong pageCacheTotal = new MutableLong();
			  MutableLong luceneTotal = new MutableLong();
			  foreach ( StoreType storeType in StoreType.values() )
			  {
					if ( storeType.RecordStore )
					{
						 long length = databaseLayout.file( storeType.DatabaseFile ).mapToLong( File.length ).sum();
						 pageCacheTotal.add( length );
					}
			  }

			  Files.walkFileTree( IndexDirectoryStructure.baseSchemaIndexFolder( databaseLayout.DatabaseDirectory() ).toPath(), new SimpleFileVisitorAnonymousInnerClass(pageCacheTotal, luceneTotal) );
			  pageCacheTotal.add( databaseLayout.LabelScanStore().length() );
			  return new long[]{ pageCacheTotal.longValue(), luceneTotal.longValue() };
		 }

		 private class SimpleFileVisitorAnonymousInnerClass : SimpleFileVisitor<Path>
		 {
			 private MutableLong _pageCacheTotal;
			 private MutableLong _luceneTotal;

			 public SimpleFileVisitorAnonymousInnerClass( MutableLong pageCacheTotal, MutableLong luceneTotal )
			 {
				 this._pageCacheTotal = pageCacheTotal;
				 this._luceneTotal = luceneTotal;
			 }

			 public override FileVisitResult visitFile( Path path, BasicFileAttributes attrs )
			 {
				  File file = path.toFile();
				  Path name = path.getName( path.NameCount - 3 );
				  bool isLuceneFile = ( path.NameCount >= 3 && name.ToString().StartsWith("lucene-", StringComparison.Ordinal) ) || (path.NameCount >= 4 && path.getName(path.NameCount - 4).ToString().Equals("lucene"));
				  if ( !FailureStorage.DEFAULT_FAILURE_FILE_NAME.Equals( file.Name ) )
				  {
						( isLuceneFile ? _luceneTotal : _pageCacheTotal ).add( file.length() );
				  }
				  return FileVisitResult.CONTINUE;
			 }
		 }

		 private static void CreateDatabaseWithNativeIndexes( File databaseDirectory )
		 {
			  // Create one index for every provider that we have
			  foreach ( SchemaIndex schemaIndex in SchemaIndex.values() )
			  {
					GraphDatabaseService db = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(databaseDirectory).setConfig(default_schema_provider, schemaIndex.providerName()).newGraphDatabase();
					string key = "key-" + schemaIndex.name();
					try
					{
						 Label labelOne = Label.label( "one" );
						 using ( Transaction tx = Db.beginTx() )
						 {
							  Db.schema().indexFor(labelOne).on(key).create();
							  tx.Success();
						 }

						 using ( Transaction tx = Db.beginTx() )
						 {
							  RandomValues randomValues = RandomValues.create();
							  for ( int i = 0; i < 10_000; i++ )
							  {
									Db.createNode( labelOne ).setProperty( key, randomValues.NextValue().asObject() );
							  }
							  tx.Success();
						 }
					}
					finally
					{
						 Db.shutdown();
					}
			  }
		 }

		 private class OutputCaptureOutsideWorld : RealOutsideWorld
		 {
			  internal readonly StringBuilder Output;

			  internal OutputCaptureOutsideWorld( StringBuilder output )
			  {
					this.Output = output;
			  }

			  public override void StdOutLine( string text )
			  {
					Output.Append( text ).Append( Environment.NewLine );
			  }
		 }
	}

}