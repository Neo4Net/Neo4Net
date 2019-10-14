using System.Collections.Generic;
using System.IO;

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
namespace Neo4Net.@unsafe.Impl.Batchimport.input.csv
{
	using Charsets = org.apache.commons.io.Charsets;
	using MutableLong = org.apache.commons.lang3.mutable.MutableLong;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseFile = Neo4Net.Io.layout.DatabaseFile;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using PageCacheTracer = Neo4Net.Io.pagecache.tracing.PageCacheTracer;
	using PageCursorTracerSupplier = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using VersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ConfiguringPageCacheFactory = Neo4Net.Kernel.impl.pagecache.ConfiguringPageCacheFactory;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using PropertyValueRecordSizeCalculator = Neo4Net.Kernel.impl.store.PropertyValueRecordSizeCalculator;
	using StoreFactory = Neo4Net.Kernel.impl.store.StoreFactory;
	using RecordFormats = Neo4Net.Kernel.impl.store.format.RecordFormats;
	using DefaultIdGeneratorFactory = Neo4Net.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;
	using NullLog = Neo4Net.Logging.NullLog;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using NullLogService = Neo4Net.Logging.Internal.NullLogService;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;
	using ThreadPoolJobScheduler = Neo4Net.Scheduler.ThreadPoolJobScheduler;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using Neo4Net.@unsafe.Impl.Batchimport.input;
	using ExecutionMonitors = Neo4Net.@unsafe.Impl.Batchimport.staging.ExecutionMonitors;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Integer.parseInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.abs;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.toIntExact;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.csv.reader.CharSeekers.charSeeker;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.csv.reader.Readables.wrap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.count;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.NoStoreHeader.NO_STORE_HEADER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.format.standard.Standard.LATEST_RECORD_FORMATS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.RecordLoad.CHECK;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.ImportLogic.NO_MONITOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.input.RandomEntityDataGenerator.convert;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.input.csv.Configuration.COMMAS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.input.csv.DataFactories.defaultFormatNodeFileHeader;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.input.csv.DataFactories.defaultFormatRelationshipFileHeader;

	public class CsvInputEstimateCalculationIT
	{
		 private const long NODE_COUNT = 600_000;
		 private const long RELATIONSHIP_COUNT = 600_000;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.RandomRule random = new org.neo4j.test.rule.RandomRule();
		 public readonly RandomRule Random = new RandomRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory directory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory Directory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCalculateCorrectEstimates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCalculateCorrectEstimates()
		 {
			  // given a couple of input files of various layouts
			  Input input = GenerateData();
			  RecordFormats format = LATEST_RECORD_FORMATS;
			  Neo4Net.@unsafe.Impl.Batchimport.input.Input_Estimates estimates = input.CalculateEstimates( new PropertyValueRecordSizeCalculator( format.Property().getRecordSize(NO_STORE_HEADER), parseInt(GraphDatabaseSettings.string_block_size.DefaultValue), 0, parseInt(GraphDatabaseSettings.array_block_size.DefaultValue), 0 ) );

			  // when
			  DatabaseLayout databaseLayout = Directory.databaseLayout();
			  Config config = Config.defaults();
			  FileSystemAbstraction fs = new DefaultFileSystemAbstraction();
			  using ( JobScheduler jobScheduler = new ThreadPoolJobScheduler() )
			  {
					( new ParallelBatchImporter( databaseLayout, fs, null, Configuration.DEFAULT, NullLogService.Instance, ExecutionMonitors.invisible(), AdditionalInitialIds.EMPTY, config, format, NO_MONITOR, jobScheduler ) ).doImport(input);

					// then compare estimates with actual disk sizes
					VersionContextSupplier contextSupplier = EmptyVersionContextSupplier.EMPTY;
					using ( PageCache pageCache = ( new ConfiguringPageCacheFactory( fs, config, PageCacheTracer.NULL, Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null, NullLog.Instance, contextSupplier, jobScheduler ) ).OrCreatePageCache, NeoStores stores = ( new StoreFactory( databaseLayout, config, new DefaultIdGeneratorFactory( fs ), pageCache, fs, NullLogProvider.Instance, contextSupplier ) ).openAllNeoStores() )
					{
						 AssertRoughlyEqual( estimates.NumberOfNodes(), stores.NodeStore.NumberOfIdsInUse );
						 AssertRoughlyEqual( estimates.NumberOfRelationships(), stores.RelationshipStore.NumberOfIdsInUse );
						 AssertRoughlyEqual( estimates.NumberOfNodeProperties() + estimates.NumberOfRelationshipProperties(), CalculateNumberOfProperties(stores) );
					}
					AssertRoughlyEqual( PropertyStorageSize(), estimates.SizeOfNodeProperties() + estimates.SizeOfRelationshipProperties() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCalculateCorrectEstimatesOnEmptyData() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCalculateCorrectEstimatesOnEmptyData()
		 {
			  // given
			  Groups groups = new Groups();
			  ICollection<DataFactory> nodeData = asList( GenerateData( defaultFormatNodeFileHeader(), new MutableLong(), 0, 0, ":ID", "nodes-1.csv", groups ) );
			  ICollection<DataFactory> relationshipData = asList( GenerateData( defaultFormatRelationshipFileHeader(), new MutableLong(), 0, 0, ":START_ID,:TYPE,:END_ID", "rels-1.csv", groups ) );
			  Input input = new CsvInput( nodeData, defaultFormatNodeFileHeader(), relationshipData, defaultFormatRelationshipFileHeader(), IdType.Integer, COMMAS, Collector.EMPTY, CsvInput.NoMonitor, groups );

			  // when
			  Neo4Net.@unsafe.Impl.Batchimport.input.Input_Estimates estimates = input.CalculateEstimates( new PropertyValueRecordSizeCalculator( LATEST_RECORD_FORMATS.property().getRecordSize(NO_STORE_HEADER), parseInt(GraphDatabaseSettings.string_block_size.DefaultValue), 0, parseInt(GraphDatabaseSettings.array_block_size.DefaultValue), 0 ) );

			  // then
			  assertEquals( 0, estimates.NumberOfNodes() );
			  assertEquals( 0, estimates.NumberOfRelationships() );
			  assertEquals( 0, estimates.NumberOfRelationshipProperties() );
			  assertEquals( 0, estimates.NumberOfNodeProperties() );
			  assertEquals( 0, estimates.NumberOfNodeLabels() );
		 }

		 private long PropertyStorageSize()
		 {
			  return SizeOf( DatabaseFile.PROPERTY_STORE ) + SizeOf( DatabaseFile.PROPERTY_ARRAY_STORE ) + SizeOf( DatabaseFile.PROPERTY_STRING_STORE );
		 }

		 private long SizeOf( DatabaseFile file )
		 {
			  return Directory.databaseLayout().file(file).mapToLong(File.length).sum();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.unsafe.impl.batchimport.input.Input generateData() throws java.io.IOException
		 private Input GenerateData()
		 {
			  IList<DataFactory> nodeData = new List<DataFactory>();
			  MutableLong start = new MutableLong();
			  Groups groups = new Groups();
			  nodeData.Add( GenerateData( defaultFormatNodeFileHeader(), start, NODE_COUNT / 3, NODE_COUNT, ":ID", "nodes-1.csv", groups ) );
			  nodeData.Add( GenerateData( defaultFormatNodeFileHeader(), start, NODE_COUNT / 3, NODE_COUNT, ":ID,:LABEL,name:String,yearOfBirth:int", "nodes-2.csv", groups ) );
			  nodeData.Add( GenerateData( defaultFormatNodeFileHeader(), start, NODE_COUNT - start.longValue(), NODE_COUNT, ":ID,name:String,yearOfBirth:int,other", "nodes-3.csv", groups ) );
			  IList<DataFactory> relationshipData = new List<DataFactory>();
			  start.Value = 0;
			  relationshipData.Add( GenerateData( defaultFormatRelationshipFileHeader(), start, RELATIONSHIP_COUNT / 2, NODE_COUNT, ":START_ID,:TYPE,:END_ID", "relationships-1.csv", groups ) );
			  relationshipData.Add( GenerateData( defaultFormatRelationshipFileHeader(), start, RELATIONSHIP_COUNT - start.longValue(), NODE_COUNT, ":START_ID,:TYPE,:END_ID,prop1,prop2", "relationships-2.csv", groups ) );
			  return new CsvInput( nodeData, defaultFormatNodeFileHeader(), relationshipData, defaultFormatRelationshipFileHeader(), IdType.Integer, COMMAS, Collector.EMPTY, CsvInput.NoMonitor, groups );
		 }

		 private static long CalculateNumberOfProperties( NeoStores stores )
		 {
			  long count = 0;
			  PropertyRecord record = stores.PropertyStore.newRecord();
			  using ( PageCursor cursor = stores.PropertyStore.openPageCursorForReading( 0 ) )
			  {
					long highId = stores.PropertyStore.HighId;
					for ( long id = 0; id < highId; id++ )
					{
						 stores.PropertyStore.getRecordByCursor( id, record, CHECK, cursor );
						 if ( record.InUse() )
						 {
							  count += count( record );
						 }
					}
			  }
			  return count;
		 }

		 private static void AssertRoughlyEqual( long expected, long actual )
		 {
			  long diff = abs( expected - actual );
			  assertThat( expected / 10, greaterThan( diff ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private DataFactory generateData(Header.Factory factory, org.apache.commons.lang3.mutable.MutableLong start, long count, long nodeCount, String headerString, String fileName, org.neo4j.unsafe.impl.batchimport.input.Groups groups) throws java.io.IOException
		 private DataFactory GenerateData( Header.Factory factory, MutableLong start, long count, long nodeCount, string headerString, string fileName, Groups groups )
		 {
			  File file = Directory.file( fileName );
			  Header header = factory.Create( charSeeker( wrap( headerString ), COMMAS, false ), COMMAS, IdType.Integer, groups );
			  Distribution<string> distribution = new Distribution<string>( new string[] { "Token" } );
			  Deserialization<string> deserialization = new StringDeserialization( COMMAS );
			  using ( PrintWriter @out = new PrintWriter( new StreamWriter( file ) ), RandomEntityDataGenerator generator = new RandomEntityDataGenerator( nodeCount, count, toIntExact( count ), Random.seed(), start.longValue(), header, distribution, distribution, 0, 0 ), InputChunk chunk = generator.NewChunk(), InputEntity entity = new InputEntity() )
			  {
					@out.println( headerString );
					while ( generator.Next( chunk ) )
					{
						 while ( chunk.Next( entity ) )
						 {
							  @out.println( convert( entity, deserialization, header ) );
						 }
					}
			  }
			  start.add( count );
			  return DataFactories.Data( InputEntityDecorators.NO_DECORATOR, Charsets.UTF_8, file );
		 }
	}

}