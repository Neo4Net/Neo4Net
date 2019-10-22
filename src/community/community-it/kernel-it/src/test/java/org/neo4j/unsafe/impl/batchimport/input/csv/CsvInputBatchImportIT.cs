using System;
using System.Collections;
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
namespace Neo4Net.@unsafe.Impl.Batchimport.input.csv
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Neo4Net.Helpers.Collections;
	using NamedToken = Neo4Net.Internal.Kernel.Api.NamedToken;
	using StatementConstants = Neo4Net.Kernel.api.StatementConstants;
	using Config = Neo4Net.Kernel.configuration.Config;
	using RecordStorageEngine = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordStorageEngine;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using Neo4Net.Kernel.impl.store;
	using RecordFormatSelector = Neo4Net.Kernel.impl.store.format.RecordFormatSelector;
	using Neo4Net.Kernel.impl.util;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using LogTimeZone = Neo4Net.Logging.LogTimeZone;
	using NullLogService = Neo4Net.Logging.Internal.NullLogService;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using ThreadPoolJobScheduler = Neo4Net.Scheduler.ThreadPoolJobScheduler;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using PointValue = Neo4Net.Values.Storable.PointValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.factory.GraphDatabaseSettings.db_timezone;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.util.AutoCreatingHashMap.nested;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.util.AutoCreatingHashMap.values;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.register.Registers.newDoubleLongRegister;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.ImportLogic.NO_MONITOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.input.Collectors.silentBadCollector;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.input.InputEntityDecorators.NO_DECORATOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.input.csv.Configuration.COMMAS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.input.csv.DataFactories.data;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.input.csv.DataFactories.datas;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.input.csv.DataFactories.defaultFormatNodeFileHeader;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.input.csv.DataFactories.defaultFormatRelationshipFileHeader;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.staging.ExecutionMonitors.invisible;

	public class CsvInputBatchImportIT
	{
		 /// <summary>
		 /// Don't support these counts at the moment so don't compute them </summary>
		 private const bool COMPUTE_DOUBLE_SIDED_RELATIONSHIP_COUNTS = false;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.TestDirectory directory = org.Neo4Net.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory Directory = TestDirectory.testDirectory();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.fs.DefaultFileSystemRule fileSystemRule = new org.Neo4Net.test.rule.fs.DefaultFileSystemRule();
		 public readonly DefaultFileSystemRule FileSystemRule = new DefaultFileSystemRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.RandomRule random = new org.Neo4Net.test.rule.RandomRule();
		 public readonly RandomRule Random = new RandomRule();

		 private static readonly System.Func<ZoneId> _testDefaultTimeZone = () => ZoneId.of("Asia/Shanghai");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldImportDataComingFromCsvFiles() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldImportDataComingFromCsvFiles()
		 {
			  // GIVEN
			  Config dbConfig = Config.builder().withSetting(db_timezone, LogTimeZone.SYSTEM.name()).build();
			  using ( IJobScheduler scheduler = new ThreadPoolJobScheduler() )
			  {
					BatchImporter importer = new ParallelBatchImporter( Directory.databaseLayout(), FileSystemRule.get(), null, SmallBatchSizeConfig(), NullLogService.Instance, invisible(), AdditionalInitialIds.EMPTY, dbConfig, RecordFormatSelector.defaultFormat(), NO_MONITOR, scheduler );
					IList<InputEntity> nodeData = RandomNodeData();
					IList<InputEntity> relationshipData = RandomRelationshipData( nodeData );

					// WHEN
					importer.DoImport( Csv( NodeDataAsFile( nodeData ), RelationshipDataAsFile( relationshipData ), IdType.String, LowBufferSize( COMMAS ), silentBadCollector( 0 ) ) );
					// THEN
					VerifyImportedData( nodeData, relationshipData );
			  }
		 }

		 public static Input Csv( File nodes, File relationships, IdType idType, Neo4Net.@unsafe.Impl.Batchimport.input.csv.Configuration configuration, Collector badCollector )
		 {
			  return new CsvInput( datas( data( NO_DECORATOR, defaultCharset(), nodes ) ), defaultFormatNodeFileHeader(_testDefaultTimeZone), datas(data(NO_DECORATOR, defaultCharset(), relationships)), defaultFormatRelationshipFileHeader(_testDefaultTimeZone), idType, configuration, badCollector, CsvInput.NoMonitor );
		 }

		 private static Neo4Net.@unsafe.Impl.Batchimport.input.csv.Configuration LowBufferSize( Neo4Net.@unsafe.Impl.Batchimport.input.csv.Configuration actual )
		 {
			  return new Configuration_OverriddenAnonymousInnerClass( actual );
		 }

		 private class Configuration_OverriddenAnonymousInnerClass : Neo4Net.@unsafe.Impl.Batchimport.input.csv.Configuration_Overridden
		 {
			 public Configuration_OverriddenAnonymousInnerClass( Neo4Net.@unsafe.Impl.Batchimport.input.csv.Configuration actual ) : base( actual )
			 {
			 }

			 public override int bufferSize()
			 {
				  return 10_000;
			 }
		 }

		 // ======================================================
		 // Below is code for generating import data
		 // ======================================================

		 private IList<InputEntity> RandomNodeData()
		 {
			  IList<InputEntity> nodes = new List<InputEntity>();
			  for ( int i = 0; i < 300; i++ )
			  {
					InputEntity node = new InputEntity();
					node.Id( System.Guid.randomUUID().ToString(), Neo4Net.@unsafe.Impl.Batchimport.input.Group_Fields.Global );
					node.property( "name", "Node " + i );
					node.property( "pointA", "\"   { x : -4.2, y : " + i + ", crs: WGS-84 } \"" );
					node.property( "pointB", "\" { x : -8, y : " + i + " } \"" );
					node.property( "date", LocalDate.of( 2018, i % 12 + 1, i % 28 + 1 ) );
					node.property( "time", OffsetTime.of( 1, i % 60, 0, 0, ZoneOffset.ofHours( 9 ) ) );
					node.property( "dateTime", ZonedDateTime.of( 2011, 9, 11, 8, i % 60, 0, 0, ZoneId.of( "Europe/Stockholm" ) ) );
					node.property( "dateTime2", new DateTime( 2011, 9, 11, 8, i % 60, 0, 0 ) ); // No zone specified
					node.property( "localTime", LocalTime.of( 1, i % 60, 0 ) );
					node.property( "localDateTime", new DateTime( 2011, 9, 11, 8, i % 60 ) );
					node.property( "duration", Period.of( 2, -3, i % 30 ) );
					node.Labels( RandomLabels( Random.random() ) );
					nodes.Add( node );
			  }
			  return nodes;
		 }

		 private string[] RandomLabels( Random random )
		 {
			  string[] labels = new string[random.Next( 3 )];
			  for ( int i = 0; i < labels.Length; i++ )
			  {
					labels[i] = "Label" + random.Next( 4 );
			  }
			  return labels;
		 }

		 private Configuration SmallBatchSizeConfig()
		 {
			  return new ConfigurationAnonymousInnerClass( this );
		 }

		 private class ConfigurationAnonymousInnerClass : Configuration
		 {
			 private readonly CsvInputBatchImportIT _outerInstance;

			 public ConfigurationAnonymousInnerClass( CsvInputBatchImportIT outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public int batchSize()
			 {
				  return 100;
			 }

			 public int denseNodeThreshold()
			 {
				  return 5;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File relationshipDataAsFile(java.util.List<org.Neo4Net.unsafe.impl.batchimport.input.InputEntity> relationshipData) throws java.io.IOException
		 private File RelationshipDataAsFile( IList<InputEntity> relationshipData )
		 {
			  File file = Directory.file( "relationships.csv" );
			  using ( Writer writer = FileSystemRule.get().openAsWriter(file, StandardCharsets.UTF_8, false) )
			  {
					// Header
					Println( writer, ":start_id,:end_id,:type" );

					// Data
					foreach ( InputEntity relationship in relationshipData )
					{
						 Println( writer, relationship.StartId() + "," + relationship.EndId() + "," + relationship.StringType );
					}
			  }
			  return file;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File nodeDataAsFile(java.util.List<org.Neo4Net.unsafe.impl.batchimport.input.InputEntity> nodeData) throws java.io.IOException
		 private File NodeDataAsFile( IList<InputEntity> nodeData )
		 {
			  File file = Directory.file( "nodes.csv" );
			  using ( Writer writer = FileSystemRule.get().openAsWriter(file, StandardCharsets.UTF_8, false) )
			  {
					// Header
					Println( writer, "id:ID,name,pointA:Point{crs:WGS-84},pointB:Point,date:Date,time:Time,dateTime:DateTime,dateTime2:DateTime,localTime:LocalTime," + "localDateTime:LocalDateTime,duration:Duration,some-labels:LABEL" );

					// Data
					foreach ( InputEntity node in nodeData )
					{
						 string csvLabels = csvLabels( node.Labels() );
						 StringBuilder sb = new StringBuilder( node.Id() + "," );
						 for ( int i = 0; i < node.PropertyCount(); i++ )
						 {
							  sb.Append( node.PropertyValue( i ) + "," );
						 }
						 sb.Append( !string.ReferenceEquals( csvLabels, null ) && csvLabels.Length > 0 ? csvLabels : "" );
						 Println( writer, sb.ToString() );
					}
			  }
			  return file;
		 }

		 private string CsvLabels( string[] labels )
		 {
			  if ( labels == null || labels.Length == 0 )
			  {
					return null;
			  }
			  StringBuilder builder = new StringBuilder();
			  foreach ( string label in labels )
			  {
					builder.Append( builder.Length > 0 ? ";" : "" ).Append( label );
			  }
			  return builder.ToString();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void println(java.io.Writer writer, String string) throws java.io.IOException
		 private void Println( Writer writer, string @string )
		 {
			  writer.write( @string + "\n" );
		 }

		 private IList<InputEntity> RandomRelationshipData( IList<InputEntity> nodeData )
		 {
			  IList<InputEntity> relationships = new List<InputEntity>();
			  for ( int i = 0; i < 1000; i++ )
			  {
					InputEntity relationship = new InputEntity();
					relationship.StartId( nodeData[Random.Next( nodeData.Count )].id(), Neo4Net.@unsafe.Impl.Batchimport.input.Group_Fields.Global );
					relationship.EndId( nodeData[Random.Next( nodeData.Count )].id(), Neo4Net.@unsafe.Impl.Batchimport.input.Group_Fields.Global );
					relationship.Type( "TYPE_" + Random.Next( 3 ) );
					relationships.Add( relationship );
			  }
			  return relationships;
		 }

		 // ======================================================
		 // Below is code for verifying the imported data
		 // ======================================================

		 private void VerifyImportedData( IList<InputEntity> nodeData, IList<InputEntity> relationshipData )
		 {
			  // Build up expected data for the verification below
			  IDictionary<string, InputEntity> expectedNodes = new Dictionary<string, InputEntity>();
			  IDictionary<string, string[]> expectedNodeNames = new Dictionary<string, string[]>();
			  IDictionary<string, IDictionary<string, System.Action<object>>> expectedNodePropertyVerifiers = new Dictionary<string, IDictionary<string, System.Action<object>>>();
			  IDictionary<string, IDictionary<string, IDictionary<string, AtomicInteger>>> expectedRelationships = new AutoCreatingHashMap<string, IDictionary<string, IDictionary<string, AtomicInteger>>>( nested( typeof( string ), nested( typeof( string ), values( typeof( AtomicInteger ) ) ) ) );
			  IDictionary<string, AtomicLong> expectedNodeCounts = new AutoCreatingHashMap<string, AtomicLong>( values( typeof( AtomicLong ) ) );
			  IDictionary<string, IDictionary<string, IDictionary<string, AtomicLong>>> expectedRelationshipCounts = new AutoCreatingHashMap<string, IDictionary<string, IDictionary<string, AtomicLong>>>( nested( typeof( string ), nested( typeof( string ), values( typeof( AtomicLong ) ) ) ) );
			  BuildUpExpectedData( nodeData, relationshipData, expectedNodes, expectedNodeNames, expectedNodePropertyVerifiers, expectedRelationships, expectedNodeCounts, expectedRelationshipCounts );

			  // Do the verification
			  IGraphDatabaseService db = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabase(Directory.databaseDir());
			  try
			  {
					  using ( Transaction tx = Db.beginTx() )
					  {
						// Verify nodes
						foreach ( Node node in Db.AllNodes )
						{
							 string name = ( string ) node.GetProperty( "name" );
							 string[] labels = expectedNodeNames.Remove( name );
							 assertEquals( asSet( labels ), Names( node.Labels ) );
      
							 // Verify node properties
							 IDictionary<string, System.Action<object>> expectedPropertyVerifiers = expectedNodePropertyVerifiers.Remove( name );
							 IDictionary<string, object> actualProperties = node.AllProperties;
							 actualProperties.Remove( "id" ); // The id does not exist in expected properties
							 foreach ( DictionaryEntry actualProperty in actualProperties.SetOfKeyValuePairs() )
							 {
								  System.Action v = expectedPropertyVerifiers[actualProperty.Key];
								  if ( v != null )
								  {
										v( actualProperty.Value );
								  }
							 }
						}
						assertEquals( 0, expectedNodeNames.Count );
      
						// Verify relationships
						foreach ( Relationship relationship in Db.AllRelationships )
						{
							 string startNodeName = ( string ) relationship.StartNode.getProperty( "name" );
							 IDictionary<string, IDictionary<string, AtomicInteger>> inner = expectedRelationships[startNodeName];
							 string endNodeName = ( string ) relationship.EndNode.getProperty( "name" );
							 IDictionary<string, AtomicInteger> innerInner = inner[endNodeName];
							 string type = relationship.Type.name();
							 int countAfterwards = innerInner[type].decrementAndGet();
							 assertThat( countAfterwards, greaterThanOrEqualTo( 0 ) );
							 if ( countAfterwards == 0 )
							 {
								  innerInner.Remove( type );
								  if ( innerInner.Count == 0 )
								  {
										inner.Remove( endNodeName );
										if ( inner.Count == 0 )
										{
											 expectedRelationships.Remove( startNodeName );
										}
								  }
							 }
						}
						assertEquals( 0, expectedRelationships.Count );
      
						// Verify counts, TODO how to get counts store other than this way?
						NeoStores neoStores = ( ( GraphDatabaseAPI )db ).DependencyResolver.resolveDependency( typeof( RecordStorageEngine ) ).testAccessNeoStores();
						System.Func<string, int> labelTranslationTable = TranslationTable( neoStores.LabelTokenStore, StatementConstants.ANY_LABEL );
						foreach ( Pair<int, long> count in AllNodeCounts( labelTranslationTable, expectedNodeCounts ) )
						{
							 assertEquals( "Label count mismatch for label " + count.First(), count.Other(), neoStores.Counts.nodeCount(count.First(), newDoubleLongRegister()).readSecond() );
						}
      
						System.Func<string, int> relationshipTypeTranslationTable = TranslationTable( neoStores.RelationshipTypeTokenStore, StatementConstants.ANY_RELATIONSHIP_TYPE );
						foreach ( Pair<RelationshipCountKey, long> count in AllRelationshipCounts( labelTranslationTable, relationshipTypeTranslationTable, expectedRelationshipCounts ) )
						{
							 RelationshipCountKey key = count.First();
							 assertEquals( "Label count mismatch for label " + key, count.Other(), neoStores.Counts.relationshipCount(key.StartLabel, key.Type, key.EndLabel, newDoubleLongRegister()).readSecond() );
						}
      
						tx.Success();
					  }
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

		 private class RelationshipCountKey
		 {
			  internal readonly int StartLabel;
			  internal readonly int Type;
			  internal readonly int EndLabel;

			  internal RelationshipCountKey( int startLabel, int type, int endLabel )
			  {
					this.StartLabel = startLabel;
					this.Type = type;
					this.EndLabel = endLabel;
			  }

			  public override string ToString()
			  {
					return format( "[start:%d, type:%d, end:%d]", StartLabel, Type, EndLabel );
			  }
		 }

		 private IEnumerable<Pair<RelationshipCountKey, long>> AllRelationshipCounts( System.Func<string, int> labelTranslationTable, System.Func<string, int> relationshipTypeTranslationTable, IDictionary<string, IDictionary<string, IDictionary<string, AtomicLong>>> counts )
		 {
			  ICollection<Pair<RelationshipCountKey, long>> result = new List<Pair<RelationshipCountKey, long>>();
			  foreach ( KeyValuePair<string, IDictionary<string, IDictionary<string, AtomicLong>>> startLabel in Counts.SetOfKeyValuePairs() )
			  {
					foreach ( KeyValuePair<string, IDictionary<string, AtomicLong>> type in startLabel.Value.entrySet() )
					{
						 foreach ( KeyValuePair<string, AtomicLong> endLabel in type.Value.entrySet() )
						 {
							  RelationshipCountKey key = new RelationshipCountKey( labelTranslationTable( startLabel.Key ), relationshipTypeTranslationTable( type.Key ), labelTranslationTable( endLabel.Key ) );
							  result.Add( Pair.of( key, endLabel.Value.longValue() ) );
						 }
					}
			  }
			  return result;
		 }

		 private IEnumerable<Pair<int, long>> AllNodeCounts( System.Func<string, int> labelTranslationTable, IDictionary<string, AtomicLong> counts )
		 {
			  ICollection<Pair<int, long>> result = new List<Pair<int, long>>();
			  foreach ( KeyValuePair<string, AtomicLong> count in Counts.SetOfKeyValuePairs() )
			  {
					result.Add( Pair.of( labelTranslationTable( count.Key ), count.Value.get() ) );
			  }
			  counts[null] = new AtomicLong( Counts.Count );
			  return result;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private System.Func<String, int> translationTable(org.Neo4Net.kernel.impl.store.TokenStore<?> tokenStore, final int anyValue)
		 private System.Func<string, int> TranslationTable<T1>( TokenStore<T1> tokenStore, int anyValue )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Map<String, int> translationTable = new java.util.HashMap<>();
			  IDictionary<string, int> translationTable = new Dictionary<string, int>();
			  foreach ( NamedToken token in tokenStore.Tokens )
			  {
					translationTable[token.Name()] = token.Id();
			  }
			  return from => from == null ? anyValue : translationTable[from];
		 }

		 private static ISet<string> Names( IEnumerable<Label> labels )
		 {
			  ISet<string> names = new HashSet<string>();
			  foreach ( Label label in labels )
			  {
					names.Add( label.Name() );
			  }
			  return names;
		 }

		 private static void BuildUpExpectedData( IList<InputEntity> nodeData, IList<InputEntity> relationshipData, IDictionary<string, InputEntity> expectedNodes, IDictionary<string, string[]> expectedNodeNames, IDictionary<string, IDictionary<string, System.Action<object>>> expectedNodePropertyVerifiers, IDictionary<string, IDictionary<string, IDictionary<string, AtomicInteger>>> expectedRelationships, IDictionary<string, AtomicLong> nodeCounts, IDictionary<string, IDictionary<string, IDictionary<string, AtomicLong>>> relationshipCounts )
		 {
			  foreach ( InputEntity node in nodeData )
			  {
					expectedNodes[( string ) node.Id()] = node;
					expectedNodeNames[NameOf( node )] = node.Labels();

					// Build default verifiers for all the properties that compares the property value using equals
					assertTrue( !node.HasIntPropertyKeyIds );
					IDictionary<string, System.Action<object>> propertyVerifiers = new SortedDictionary<string, System.Action<object>>();
					for ( int i = 0; i < node.PropertyCount(); i++ )
					{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Object expectedValue = node.propertyValue(i);
						 object expectedValue = node.PropertyValue( i );
						 System.Action verify;
						 if ( expectedValue is TemporalAmount )
						 {
							  // Since there is no straightforward comparison for TemporalAmount we add it to a reference
							  // point in time and compare the result
							  verify = actualValue =>
							  {
								DateTime referenceTemporal = new DateTime( 0, 1, 1, 0, 0 );
								DateTime expected = referenceTemporal.plus( ( TemporalAmount ) expectedValue );
								DateTime actual = referenceTemporal.plus( ( TemporalAmount ) actualValue );
								assertEquals( expected, actual );
							  };
						 }
						 else if ( expectedValue is Temporal )
						 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.time.LocalDate expectedDate = ((java.time.temporal.Temporal) expectedValue).query(java.time.temporal.TemporalQueries.localDate());
							  LocalDate expectedDate = ( ( Temporal ) expectedValue ).query( TemporalQueries.localDate() );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.time.LocalTime expectedTime = ((java.time.temporal.Temporal) expectedValue).query(java.time.temporal.TemporalQueries.localTime());
							  LocalTime expectedTime = ( ( Temporal ) expectedValue ).query( TemporalQueries.localTime() );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.time.ZoneId expectedZoneId = ((java.time.temporal.Temporal) expectedValue).query(java.time.temporal.TemporalQueries.zone());
							  ZoneId expectedZoneId = ( ( Temporal ) expectedValue ).query( TemporalQueries.zone() );

							  verify = actualValue =>
							  {
								LocalDate actualDate = ( ( Temporal ) actualValue ).query( TemporalQueries.localDate() );
								LocalTime actualTime = ( ( Temporal ) actualValue ).query( TemporalQueries.localTime() );
								ZoneId actualZoneId = ( ( Temporal ) actualValue ).query( TemporalQueries.zone() );

								assertEquals( expectedDate, actualDate );
								assertEquals( expectedTime, actualTime );
								if ( expectedZoneId == null )
								{
									 if ( actualZoneId != null )
									 {
										  // If the actual value is zoned it should have the default zone
										  assertEquals( _testDefaultTimeZone.get(), actualZoneId );
									 }
								}
								else
								{
									 assertEquals( expectedZoneId, actualZoneId );
								}
							  };
						 }
						 else
						 {
							  verify = actualValue => assertEquals( expectedValue, actualValue );
						 }
						 propertyVerifiers[( string ) node.PropertyKey( i )] = verify;
					}

					// Special verifier for pointA property
					System.Action verifyPointA = actualValue =>
					{
					 // The y-coordinate should match the node number
					 PointValue v = ( PointValue ) actualValue;
					 double actualY = v.Coordinates.get( 0 ).Coordinate.get( 1 );
					 double expectedY = IndexOf( node );
					 string message = actualValue.ToString() + " does not have y=" + expectedY;
					 assertEquals( message, expectedY, actualY, 0.1 );
					 message = actualValue.ToString() + " does not have crs=wgs-84";
					 assertEquals( message, CoordinateReferenceSystem.WGS84.Name, v.CoordinateReferenceSystem.Name );
					};
					propertyVerifiers["pointA"] = verifyPointA;

					// Special verifier for pointB property
					System.Action verifyPointB = actualValue =>
					{
					 // The y-coordinate should match the node number
					 PointValue v = ( PointValue ) actualValue;
					 double actualY = v.Coordinates.get( 0 ).Coordinate.get( 1 );
					 double expectedY = IndexOf( node );
					 string message = actualValue.ToString() + " does not have y=" + expectedY;
					 assertEquals( message, expectedY, actualY, 0.1 );
					 message = actualValue.ToString() + " does not have crs=cartesian";
					 assertEquals( message, CoordinateReferenceSystem.Cartesian.Name, v.CoordinateReferenceSystem.Name );
					};
					propertyVerifiers["pointB"] = verifyPointB;

					expectedNodePropertyVerifiers[NameOf( node )] = propertyVerifiers;

					CountNodeLabels( nodeCounts, node.Labels() );
			  }
			  foreach ( InputEntity relationship in relationshipData )
			  {
					// Expected relationship counts per node, type and direction
					InputEntity startNode = expectedNodes[relationship.StartId()];
					InputEntity endNode = expectedNodes[relationship.EndId()];
					{
						 expectedRelationships[NameOf( startNode )][NameOf( endNode )][relationship.StringType].incrementAndGet();
					}

					// Expected counts per start/end node label ids
					// Let's do what CountsState#addRelationship does, roughly
					relationshipCounts[null][null][null].incrementAndGet();
					relationshipCounts[null][relationship.StringType][null].incrementAndGet();
					foreach ( string startNodeLabelName in asSet( startNode.Labels() ) )
					{
						 IDictionary<string, IDictionary<string, AtomicLong>> startLabelCounts = relationshipCounts[startNodeLabelName];
						 startLabelCounts[null][null].incrementAndGet();
						 IDictionary<string, AtomicLong> typeCounts = startLabelCounts[relationship.StringType];
						 typeCounts[null].incrementAndGet();
						 if ( COMPUTE_DOUBLE_SIDED_RELATIONSHIP_COUNTS )
						 {
							  foreach ( string endNodeLabelName in asSet( endNode.Labels() ) )
							  {
									startLabelCounts[null][endNodeLabelName].incrementAndGet();
									typeCounts[endNodeLabelName].incrementAndGet();
							  }
						 }
					}
					foreach ( string endNodeLabelName in asSet( endNode.Labels() ) )
					{
						 relationshipCounts[null][null][endNodeLabelName].incrementAndGet();
						 relationshipCounts[null][relationship.StringType][endNodeLabelName].incrementAndGet();
					}
			  }
		 }

		 private static void CountNodeLabels( IDictionary<string, AtomicLong> nodeCounts, string[] labels )
		 {
			  ISet<string> seen = new HashSet<string>();
			  foreach ( string labelName in labels )
			  {
					if ( seen.Add( labelName ) )
					{
						 nodeCounts[labelName].incrementAndGet();
					}
			  }
		 }

		 private static string NameOf( InputEntity node )
		 {
			  return ( string ) node.Properties()[1];
		 }

		 private static int IndexOf( InputEntity node )
		 {
			  return int.Parse( ( ( string ) node.Properties()[1] ).Split("\\s", true)[1] );
		 }

	}

}