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
namespace Neo4Net.Tooling
{
	using MutableInt = org.apache.commons.lang3.mutable.MutableInt;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using IllegalMultilineFieldException = Neo4Net.Csv.Reader.IllegalMultilineFieldException;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Neo4Net.GraphDb;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using Iterators = Neo4Net.Helpers.Collections.Iterators;
	using Neo4Net.Helpers.Collections;
	using FileUtils = Neo4Net.Io.fs.FileUtils;
	using Config = Neo4Net.Kernel.configuration.Config;
	using RecordStorageEngine = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordStorageEngine;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using StoreType = Neo4Net.Kernel.impl.store.StoreType;
	using Standard = Neo4Net.Kernel.impl.store.format.standard.Standard;
	using Neo4Net.Kernel.impl.util;
	using Validators = Neo4Net.Kernel.impl.util.Validators;
	using Version = Neo4Net.Kernel.Internal.Version;
	using EmbeddedDatabaseRule = Neo4Net.Test.rule.EmbeddedDatabaseRule;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;
	using DuplicateInputIdException = Neo4Net.@unsafe.Impl.Batchimport.cache.idmapping.@string.DuplicateInputIdException;
	using InputException = Neo4Net.@unsafe.Impl.Batchimport.input.InputException;
	using Configuration = Neo4Net.@unsafe.Impl.Batchimport.input.csv.Configuration;
	using Type = Neo4Net.@unsafe.Impl.Batchimport.input.csv.Type;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.commons.lang3.StringUtils.repeat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.RelationshipType.withName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.factory.GraphDatabaseSettings.logs_directory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.factory.GraphDatabaseSettings.store_internal_log_path;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.ArrayUtil.join;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.Exceptions.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.Exceptions.withMessage;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterables.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.count;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.MapUtil.store;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.fs.FileUtils.writeToFile;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.tooling.ImportTool.MULTI_FILE_DELIMITER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.Configuration_Fields.BAD_FILE_NAME;

	public class ImportToolTest
	{
		 private const int MAX_LABEL_ID = 4;
		 private const int RELATIONSHIP_COUNT = 10_000;
		 private const int NODE_COUNT = 100;
		 private static readonly System.Func<int, bool> @true = i => true;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.EmbeddedDatabaseRule dbRule = new org.Neo4Net.test.rule.EmbeddedDatabaseRule().startLazily();
		 public readonly EmbeddedDatabaseRule DbRule = new EmbeddedDatabaseRule().startLazily();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.RandomRule random = new org.Neo4Net.test.rule.RandomRule();
		 public readonly RandomRule Random = new RandomRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.SuppressOutput suppressOutput = org.Neo4Net.test.rule.SuppressOutput.suppressAll();
		 public readonly SuppressOutput SuppressOutput = SuppressOutput.suppressAll();
		 private int _dataIndex;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void usageMessageIncludeExample() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UsageMessageIncludeExample()
		 {
			  SuppressOutput.Voice outputVoice = SuppressOutput.OutputVoice;
			  ImportTool( "?" );
			  assertTrue( "Usage message should include example section, but was:" + outputVoice, outputVoice.ContainsMessage( "Example:" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void usageMessagePrintedOnEmptyInputParameters() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UsageMessagePrintedOnEmptyInputParameters()
		 {
			  SuppressOutput.Voice outputVoice = SuppressOutput.OutputVoice;
			  ImportTool();
			  assertTrue( "Output should include usage section, but was:" + outputVoice, outputVoice.ContainsMessage( "Example:" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldImportWithAsManyDefaultsAsAvailable() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldImportWithAsManyDefaultsAsAvailable()
		 {
			  // GIVEN
			  IList<string> nodeIds = nodeIds();
			  Configuration config = Configuration.COMMAS;

			  // WHEN
			  ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--nodes", NodeData( true, config, nodeIds, @true ).AbsolutePath, "--relationships", RelationshipData( true, config, nodeIds, @true, true ).AbsolutePath );

			  // THEN
			  assertTrue( SuppressOutput.OutputVoice.containsMessage( "IMPORT DONE" ) );
			  VerifyData();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldImportWithHeadersBeingInSeparateFiles() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldImportWithHeadersBeingInSeparateFiles()
		 {
			  // GIVEN
			  IList<string> nodeIds = nodeIds();
			  Configuration config = Configuration.TABS;

			  // WHEN
			  ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--delimiter", "TAB", "--array-delimiter", config.ArrayDelimiter().ToString(), "--nodes", NodeHeader(config).AbsolutePath + MULTI_FILE_DELIMITER + NodeData(false, config, nodeIds, @true).AbsolutePath, "--relationships", RelationshipHeader(config).AbsolutePath + MULTI_FILE_DELIMITER + RelationshipData(false, config, nodeIds, @true, true).AbsolutePath );

			  // THEN
			  VerifyData();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void import4097Labels() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Import4097Labels()
		 {
			  // GIVEN
			  File header = File( FileName( "4097labels-header.csv" ) );
			  using ( PrintStream writer = new PrintStream( header ) )
			  {
					writer.println( ":LABEL" );
			  }
			  File data = File( FileName( "4097labels.csv" ) );
			  using ( PrintStream writer = new PrintStream( data ) )
			  {
					// Need to have unique names in order to get unique ids for labels. Want 4096 unique label ids present.
					for ( int i = 0; i < 4096; i++ )
					{
						 writer.println( "SIMPLE" + i );
					}
					// Then insert one with 3 array entries which will get ids greater than 4096. These cannot be inlined
					// due 36 bits being divided into 3 parts of 12 bits each and 4097 > 2^12, thus these labels will be
					// need to be dynamic records.
					writer.println( "FIRST 4096|SECOND 4096|THIRD 4096" );
			  }

			  // WHEN
			  ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--delimiter", "TAB", "--array-delimiter", "|", "--nodes", header.AbsolutePath + MULTI_FILE_DELIMITER + data.AbsolutePath );

			  // THEN
			  using ( Transaction tx = DbRule.beginTx() )
			  {
					long nodeCount = Iterables.count( DbRule.AllNodes );
					assertEquals( 4097, nodeCount );

					tx.Success();
					ResourceIterator<Node> nodes = DbRule.findNodes( label( "FIRST 4096" ) );
					assertEquals( 1, Iterators.asList( nodes ).Count );
					nodes = DbRule.findNodes( label( "SECOND 4096" ) );
					assertEquals( 1, Iterators.asList( nodes ).Count );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreWhitespaceAroundIntegers() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIgnoreWhitespaceAroundIntegers()
		 {
			  // GIVEN
			  // Faster to do all successful in one import than in N separate tests
			  IList<string> values = Arrays.asList( "17", "    21", "99   ", "  34  ", "-34", "        -12", "-92 " );

			  File data = File( FileName( "whitespace.csv" ) );
			  using ( PrintStream writer = new PrintStream( data ) )
			  {
					writer.println( ":LABEL,name,s:short,b:byte,i:int,l:long,f:float,d:double" );

					// For each test value
					foreach ( string value in values )
					{
						 // Save value as a String in name
						 writer.print( "PERSON,'" + value + "'" );
						 // For each numerical type
						 for ( int j = 0; j < 6; j++ )
						 {
							  writer.print( "," + value );
						 }
						 // End line
						 writer.println();
					}
			  }

			  // WHEN
			  ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--quote", "'", "--nodes", data.AbsolutePath );

			  // THEN
			  int nodeCount = 0;
			  using ( Transaction tx = DbRule.beginTx() )
			  {
					foreach ( Node node in DbRule.AllNodes )
					{
						 nodeCount++;
						 string name = ( string ) node.GetProperty( "name" );

						 string expected = name.Trim();

						 assertEquals( 7, node.AllProperties.Count );
						 foreach ( string key in node.PropertyKeys )
						 {
							  if ( key.Equals( "name" ) )
							  {
									continue;
							  }
							  else if ( key.Equals( "f" ) || key.Equals( "d" ) )
							  {
									// Floating points have decimals
									expected = double.Parse( expected ).ToString();
							  }

							  assertEquals( "Wrong value for " + key, expected, node.GetProperty( key ).ToString() );
						 }
					}

					tx.Success();
			  }

			  assertEquals( values.Count, nodeCount );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreWhitespaceAroundDecimalNumbers() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIgnoreWhitespaceAroundDecimalNumbers()
		 {
			  // GIVEN
			  // Faster to do all successful in one import than in N separate tests
			  IList<string> values = Arrays.asList( "1.0", "   3.5", "45.153    ", "   925.12   ", "-2.121", "   -3.745", "-412.153    ", "   -5.12   " );

			  File data = File( FileName( "whitespace.csv" ) );
			  using ( PrintStream writer = new PrintStream( data ) )
			  {
					writer.println( ":LABEL,name,f:float,d:double" );

					// For each test value
					foreach ( string value in values )
					{
						 // Save value as a String in name
						 writer.print( "PERSON,'" + value + "'" );
						 // For each numerical type
						 for ( int j = 0; j < 2; j++ )
						 {
							  writer.print( "," + value );
						 }
						 // End line
						 writer.println();
					}
			  }

			  // WHEN
			  ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--quote", "'", "--nodes", data.AbsolutePath );

			  // THEN
			  int nodeCount = 0;
			  using ( Transaction tx = DbRule.beginTx() )
			  {
					foreach ( Node node in DbRule.AllNodes )
					{
						 nodeCount++;
						 string name = ( string ) node.GetProperty( "name" );

						 double expected = double.Parse( name.Trim() );

						 assertEquals( 3, node.AllProperties.Count );
						 foreach ( string key in node.PropertyKeys )
						 {
							  if ( key.Equals( "name" ) )
							  {
									continue;
							  }

							  assertEquals( "Wrong value for " + key, expected, Convert.ToDouble( node.GetProperty( key ).ToString() ), 0.0 );
						 }
					}

					tx.Success();
			  }

			  assertEquals( values.Count, nodeCount );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreWhitespaceAroundBooleans() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIgnoreWhitespaceAroundBooleans()
		 {
			  // GIVEN
			  File data = File( FileName( "whitespace.csv" ) );
			  using ( PrintStream writer = new PrintStream( data ) )
			  {
					writer.println( ":LABEL,name,adult:boolean" );

					writer.println( "PERSON,'t1',true" );
					writer.println( "PERSON,'t2',  true" );
					writer.println( "PERSON,'t3',true  " );
					writer.println( "PERSON,'t4',  true  " );

					writer.println( "PERSON,'f1',false" );
					writer.println( "PERSON,'f2',  false" );
					writer.println( "PERSON,'f3',false  " );
					writer.println( "PERSON,'f4',  false  " );
					writer.println( "PERSON,'f5',  truebutactuallyfalse  " );

					writer.println( "PERSON,'f6',  non true things are interpreted as false  " );
			  }

			  // WHEN
			  ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--quote", "'", "--nodes", data.AbsolutePath );

			  // THEN
			  using ( Transaction tx = DbRule.beginTx() )
			  {
					foreach ( Node node in DbRule.AllNodes )
					{
						 string name = ( string ) node.GetProperty( "name" );
						 if ( name.StartsWith( "t", StringComparison.Ordinal ) )
						 {
							  assertTrue( "Wrong value on " + name, ( bool ) node.GetProperty( "adult" ) );
						 }
						 else
						 {
							  assertFalse( "Wrong value on " + name, ( bool ) node.GetProperty( "adult" ) );
						 }
					}

					long nodeCount = Iterables.count( DbRule.AllNodes );
					assertEquals( 10, nodeCount );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreWhitespaceInAndAroundIntegerArrays() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIgnoreWhitespaceInAndAroundIntegerArrays()
		 {
			  // GIVEN
			  // Faster to do all successful in one import than in N separate tests
			  string[] values = new string[]{ "   17", "21", "99   ", "  34  ", "-34", "        -12", "-92 " };

			  File data = WriteArrayCsv( new string[]{ "s:short[]", "b:byte[]", "i:int[]", "l:long[]", "f:float[]", "d:double[]" }, values );

			  // WHEN
			  ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--quote", "'", "--nodes", data.AbsolutePath );

			  // THEN
			  // Expected value for integer types
			  string iExpected = JoinStringArray( values );

			  // Expected value for floating point types
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  string fExpected = java.util.values.Select( string.Trim ).Select( double?.ValueOf ).Select( string.ValueOf ).collect( joining( ", ", "[", "]" ) );

			  int nodeCount = 0;
			  using ( Transaction tx = DbRule.beginTx() )
			  {
					foreach ( Node node in DbRule.AllNodes )
					{
						 nodeCount++;

						 assertEquals( 6, node.AllProperties.Count );
						 foreach ( string key in node.PropertyKeys )
						 {
							  object things = node.GetProperty( key );
							  string result = "";
							  string expected = iExpected;
							  switch ( key )
							  {
							  case "s":
									result = Arrays.ToString( ( short[] ) things );
									break;
							  case "b":
									result = Arrays.ToString( ( sbyte[] ) things );
									break;
							  case "i":
									result = Arrays.ToString( ( int[] ) things );
									break;
							  case "l":
									result = Arrays.ToString( ( long[] ) things );
									break;
							  case "f":
									result = Arrays.ToString( ( float[] ) things );
									expected = fExpected;
									break;
							  case "d":
									result = Arrays.ToString( ( double[] ) things );
									expected = fExpected;
									break;
							  default:
									break;
							  }

							  assertEquals( expected, result );
						 }
					}

					tx.Success();
			  }

			  assertEquals( 1, nodeCount );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreWhitespaceInAndAroundDecimalArrays() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIgnoreWhitespaceInAndAroundDecimalArrays()
		 {
			  // GIVEN
			  // Faster to do all successful in one import than in N separate tests
			  string[] values = new string[]{ "1.0", "   3.5", "45.153    ", "   925.12   ", "-2.121", "   -3.745", "-412.153    ", "   -5.12   " };

			  File data = WriteArrayCsv( new string[]{ "f:float[]", "d:double[]" }, values );

			  // WHEN
			  ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--quote", "'", "--nodes", data.AbsolutePath );

			  // THEN
			  string expected = JoinStringArray( values );

			  int nodeCount = 0;
			  using ( Transaction tx = DbRule.beginTx() )
			  {
					foreach ( Node node in DbRule.AllNodes )
					{
						 nodeCount++;

						 assertEquals( 2, node.AllProperties.Count );
						 foreach ( string key in node.PropertyKeys )
						 {
							  object things = node.GetProperty( key );
							  string result = "";
							  switch ( key )
							  {
							  case "f":
									result = Arrays.ToString( ( float[] ) things );
									break;
							  case "d":
									result = Arrays.ToString( ( double[] ) things );
									break;
							  default:
									break;
							  }

							  assertEquals( expected, result );
						 }
					}

					tx.Success();
			  }

			  assertEquals( 1, nodeCount );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreWhitespaceInAndAroundBooleanArrays() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIgnoreWhitespaceInAndAroundBooleanArrays()
		 {
			  // GIVEN
			  // Faster to do all successful in one import than in N separate tests
			  string[] values = new string[]{ "true", "  true", "true   ", "  true  ", " false ", "false ", " false", "false ", " false" };
			  string expected = JoinStringArray( values );

			  File data = WriteArrayCsv( new string[]{ "b:boolean[]" }, values );

			  // WHEN
			  ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--quote", "'", "--nodes", data.AbsolutePath );

			  // THEN
			  int nodeCount = 0;
			  using ( Transaction tx = DbRule.beginTx() )
			  {
					foreach ( Node node in DbRule.AllNodes )
					{
						 nodeCount++;

						 assertEquals( 1, node.AllProperties.Count );
						 foreach ( string key in node.PropertyKeys )
						 {
							  object things = node.GetProperty( key );
							  string result = Arrays.ToString( ( bool[] ) things );

							  assertEquals( expected, result );
						 }
					}

					tx.Success();
			  }

			  assertEquals( 1, nodeCount );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailIfHeaderHasLessColumnsThanData() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailIfHeaderHasLessColumnsThanData()
		 {
			  // GIVEN
			  IList<string> nodeIds = nodeIds();
			  Configuration config = Configuration.TABS;

			  // WHEN data file contains more columns than header file
			  int extraColumns = 3;
			  try
			  {
					ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--delimiter", "TAB", "--array-delimiter", config.ArrayDelimiter().ToString(), "--nodes", NodeHeader(config).AbsolutePath + MULTI_FILE_DELIMITER + NodeData(false, config, nodeIds, @true, Charset.defaultCharset(), extraColumns).AbsolutePath, "--relationships", RelationshipHeader(config).AbsolutePath + MULTI_FILE_DELIMITER + RelationshipData(false, config, nodeIds, @true, true).AbsolutePath );

					fail( "Should have thrown exception" );
			  }
			  catch ( InputException e )
			  {
					// THEN
					assertTrue( SuppressOutput.OutputVoice.containsMessage( "IMPORT FAILED" ) );
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					assertFalse( SuppressOutput.ErrorVoice.containsMessage( e.GetType().FullName ) );
					assertTrue( e.Message.contains( "Extra column not present in header on line" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWarnIfHeaderHasLessColumnsThanDataWhenToldTo() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWarnIfHeaderHasLessColumnsThanDataWhenToldTo()
		 {
			  // GIVEN
			  IList<string> nodeIds = nodeIds();
			  Configuration config = Configuration.TABS;
			  File bad = BadFile();

			  // WHEN data file contains more columns than header file
			  int extraColumns = 3;
			  ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--bad", bad.AbsolutePath, "--bad-tolerance", Convert.ToString( nodeIds.Count * extraColumns ), "--ignore-extra-columns", "--delimiter", "TAB", "--array-delimiter", config.ArrayDelimiter().ToString(), "--nodes", NodeHeader(config).AbsolutePath + MULTI_FILE_DELIMITER + NodeData(false, config, nodeIds, @true, Charset.defaultCharset(), extraColumns).AbsolutePath, "--relationships", RelationshipHeader(config).AbsolutePath + MULTI_FILE_DELIMITER + RelationshipData(false, config, nodeIds, @true, true).AbsolutePath );

			  // THEN
			  string badContents = FileUtils.readTextFile( bad, Charset.defaultCharset() );
			  assertTrue( badContents.Contains( "Extra column not present in header on line" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldImportSplitInputFiles() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldImportSplitInputFiles()
		 {
			  // GIVEN
			  IList<string> nodeIds = nodeIds();
			  Configuration config = Configuration.COMMAS;

			  // WHEN
			  ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--nodes", NodeHeader( config ).AbsolutePath + MULTI_FILE_DELIMITER + NodeData( false, config, nodeIds, Lines( 0, NODE_COUNT / 2 ) ).AbsolutePath, "--nodes", NodeData( true, config, nodeIds, Lines( NODE_COUNT / 2, NODE_COUNT * 3 / 4 ) ).AbsolutePath + MULTI_FILE_DELIMITER + NodeData( false, config, nodeIds, Lines( NODE_COUNT * 3 / 4, NODE_COUNT ) ).AbsolutePath, "--relationships", RelationshipHeader( config ).AbsolutePath + MULTI_FILE_DELIMITER + RelationshipData( false, config, nodeIds, @true, true ).AbsolutePath );

			  // THEN
			  VerifyData();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldImportMultipleInputsWithAddedLabelsAndDefaultRelationshipType() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldImportMultipleInputsWithAddedLabelsAndDefaultRelationshipType()
		 {
			  // GIVEN
			  IList<string> nodeIds = nodeIds();
			  Configuration config = Configuration.COMMAS;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String[] firstLabels = {"AddedOne", "AddedTwo"};
			  string[] firstLabels = new string[] { "AddedOne", "AddedTwo" };
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String[] secondLabels = {"AddedThree"};
			  string[] secondLabels = new string[] { "AddedThree" };
			  const string firstType = "TYPE_1";
			  const string secondType = "TYPE_2";

			  // WHEN
			  ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--nodes:" + join( firstLabels, ":" ), NodeData( true, config, nodeIds, Lines( 0, NODE_COUNT / 2 ) ).AbsolutePath, "--nodes:" + join( secondLabels, ":" ), NodeData( true, config, nodeIds, Lines( NODE_COUNT / 2, NODE_COUNT ) ).AbsolutePath, "--relationships:" + firstType, RelationshipData( true, config, nodeIds, Lines( 0, RELATIONSHIP_COUNT / 2 ), false ).AbsolutePath, "--relationships:" + secondType, RelationshipData( true, config, nodeIds, Lines( RELATIONSHIP_COUNT / 2, RELATIONSHIP_COUNT ), false ).AbsolutePath );

			  // THEN
			  MutableInt numberOfNodesWithFirstSetOfLabels = new MutableInt();
			  MutableInt numberOfNodesWithSecondSetOfLabels = new MutableInt();
			  MutableInt numberOfRelationshipsWithFirstType = new MutableInt();
			  MutableInt numberOfRelationshipsWithSecondType = new MutableInt();
			  VerifyData(node =>
			  {
						  if ( NodeHasLabels( node, firstLabels ) )
						  {
								numberOfNodesWithFirstSetOfLabels.increment();
						  }
						  else if ( NodeHasLabels( node, secondLabels ) )
						  {
								numberOfNodesWithSecondSetOfLabels.increment();
						  }
						  else
						  {
								fail( node + " has neither set of labels, it has " + LabelsOf( node ) );
						  }
			  }, relationship =>
			  {
						  if ( relationship.isType( RelationshipType.withName( firstType ) ) )
						  {
								numberOfRelationshipsWithFirstType.increment();
						  }
						  else if ( relationship.isType( RelationshipType.withName( secondType ) ) )
						  {
								numberOfRelationshipsWithSecondType.increment();
						  }
						  else
						  {
								fail( relationship + " didn't have either type, it has " + relationship.Type.name() );
						  }
					 });
			  assertEquals( NODE_COUNT / 2, numberOfNodesWithFirstSetOfLabels.intValue() );
			  assertEquals( NODE_COUNT / 2, numberOfNodesWithSecondSetOfLabels.intValue() );
			  assertEquals( RELATIONSHIP_COUNT / 2, numberOfRelationshipsWithFirstType.intValue() );
			  assertEquals( RELATIONSHIP_COUNT / 2, numberOfRelationshipsWithSecondType.intValue() );
		 }

		 private static string LabelsOf( Node node )
		 {
			  StringBuilder builder = new StringBuilder();
			  foreach ( Label label in node.Labels )
			  {
					builder.Append( label.Name() + " " );
			  }
			  return builder.ToString();
		 }

		 private bool NodeHasLabels( Node node, string[] labels )
		 {
			  foreach ( string name in labels )
			  {
					if ( !node.HasLabel( Label.label( name ) ) )
					{
						 return false;
					}
			  }
			  return true;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldImportOnlyNodes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldImportOnlyNodes()
		 {
			  // GIVEN
			  IList<string> nodeIds = nodeIds();
			  Configuration config = Configuration.COMMAS;

			  // WHEN
			  ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--nodes", NodeData( true, config, nodeIds, @true ).AbsolutePath );
			  // no relationships

			  // THEN
			  IGraphDatabaseService db = DbRule.GraphDatabaseAPI;
			  using ( Transaction tx = Db.beginTx() )
			  {
					int nodeCount = 0;
					foreach ( Node node in Db.AllNodes )
					{
						 assertTrue( node.HasProperty( "name" ) );
						 nodeCount++;
						 assertFalse( node.HasRelationship() );
					}
					assertEquals( NODE_COUNT, nodeCount );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldImportGroupsOfOverlappingIds() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldImportGroupsOfOverlappingIds()
		 {
			  // GIVEN
			  IList<string> groupOneNodeIds = new IList<string> { "1", "2", "3" };
			  IList<string> groupTwoNodeIds = new IList<string> { "4", "5", "2" };
			  IList<RelationshipDataLine> rels = new IList<RelationshipDataLine> { Relationship( "1", "4", "TYPE" ), Relationship( "2", "5", "TYPE" ), Relationship( "3", "2", "TYPE" ) };
			  Configuration config = Configuration.COMMAS;
			  string groupOne = "Actor";
			  string groupTwo = "Movie";

			  // WHEN
			  ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--nodes", NodeHeader( config, groupOne ) + MULTI_FILE_DELIMITER + NodeData( false, config, groupOneNodeIds, @true ), "--nodes", NodeHeader( config, groupTwo ) + MULTI_FILE_DELIMITER + NodeData( false, config, groupTwoNodeIds, @true ), "--relationships", RelationshipHeader( config, groupOne, groupTwo, true ) + MULTI_FILE_DELIMITER + RelationshipData( false, config, rels.GetEnumerator(), @true, true ) );

			  // THEN
			  IGraphDatabaseService db = DbRule.GraphDatabaseAPI;
			  using ( Transaction tx = Db.beginTx() )
			  {
					int nodeCount = 0;
					foreach ( Node node in Db.AllNodes )
					{
						 assertTrue( node.HasProperty( "name" ) );
						 nodeCount++;
						 assertEquals( 1, Iterables.count( node.Relationships ) );
					}
					assertEquals( 6, nodeCount );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToMixSpecifiedAndUnspecifiedGroups() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToMixSpecifiedAndUnspecifiedGroups()
		 {
			  // GIVEN
			  IList<string> groupOneNodeIds = new IList<string> { "1", "2", "3" };
			  IList<string> groupTwoNodeIds = new IList<string> { "4", "5", "2" };
			  Configuration config = Configuration.COMMAS;

			  // WHEN
			  ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--nodes", NodeHeader( config, "MyGroup" ).AbsolutePath + MULTI_FILE_DELIMITER + NodeData( false, config, groupOneNodeIds, @true ).AbsolutePath, "--nodes", NodeHeader( config ).AbsolutePath + MULTI_FILE_DELIMITER + NodeData( false, config, groupTwoNodeIds, @true ).AbsolutePath );

			  // THEN
			  VerifyData( 6, 0, Validators.emptyValidator(), Validators.emptyValidator() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldImportWithoutTypeSpecifiedInRelationshipHeaderbutWithDefaultTypeInArgument() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldImportWithoutTypeSpecifiedInRelationshipHeaderbutWithDefaultTypeInArgument()
		 {
			  // GIVEN
			  IList<string> nodeIds = nodeIds();
			  Configuration config = Configuration.COMMAS;
			  string type = RandomType();

			  // WHEN
			  ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--nodes", NodeData( true, config, nodeIds, @true ).AbsolutePath, "--relationships:" + type, RelationshipData( true, config, nodeIds, @true, false ).AbsolutePath );

			  // THEN
			  VerifyData();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIncludeSourceInformationInNodeIdCollisionError() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIncludeSourceInformationInNodeIdCollisionError()
		 {
			  // GIVEN
			  IList<string> nodeIds = new IList<string> { "a", "b", "c", "d", "e", "f", "a", "g" };
			  Configuration config = Configuration.COMMAS;
			  File nodeHeaderFile = NodeHeader( config );
			  File nodeData1 = NodeData( false, config, nodeIds, Lines( 0, 4 ) );
			  File nodeData2 = NodeData( false, config, nodeIds, Lines( 4, nodeIds.Count ) );

			  // WHEN
			  try
			  {
					ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--nodes", nodeHeaderFile.AbsolutePath + MULTI_FILE_DELIMITER + nodeData1.AbsolutePath + MULTI_FILE_DELIMITER + nodeData2.AbsolutePath );
					fail( "Should have failed with duplicate node IDs" );
			  }
			  catch ( Exception e )
			  {
					// THEN
					AssertExceptionContains( e, "'a' is defined more than once", typeof( DuplicateInputIdException ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSkipDuplicateNodesIfToldTo() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSkipDuplicateNodesIfToldTo()
		 {
			  // GIVEN
			  IList<string> nodeIds = new IList<string> { "a", "b", "c", "d", "e", "f", "a", "g" };
			  Configuration config = Configuration.COMMAS;
			  File nodeHeaderFile = NodeHeader( config );
			  File nodeData1 = NodeData( false, config, nodeIds, Lines( 0, 4 ) );
			  File nodeData2 = NodeData( false, config, nodeIds, Lines( 4, nodeIds.Count ) );

			  // WHEN
			  ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--skip-duplicate-nodes", "--nodes", nodeHeaderFile.AbsolutePath + MULTI_FILE_DELIMITER + nodeData1.AbsolutePath + MULTI_FILE_DELIMITER + nodeData2.AbsolutePath );

			  // THEN there should not be duplicates of any node
			  IGraphDatabaseService db = DbRule.GraphDatabaseAPI;
			  ISet<string> expectedNodeIds = new HashSet<string>( nodeIds );
			  try
			  {
					  using ( Transaction tx = Db.beginTx() )
					  {
						ISet<string> foundNodesIds = new HashSet<string>();
						foreach ( Node node in Db.AllNodes )
						{
							 string id = ( string ) node.GetProperty( "id" );
							 assertTrue( id + ", " + foundNodesIds, foundNodesIds.Add( id ) );
							 assertTrue( expectedNodeIds.Contains( id ) );
						}
						assertEquals( expectedNodeIds, foundNodesIds );
      
						// also all nodes in the label index should exist
						for ( int i = 0; i < MAX_LABEL_ID; i++ )
						{
							 Label label = label( LabelName( i ) );
							 using ( ResourceIterator<Node> nodesByLabel = Db.findNodes( label ) )
							 {
								  while ( nodesByLabel.MoveNext() )
								  {
										Node node = nodesByLabel.Current;
										if ( !node.HasLabel( label ) )
										{
											 fail( "Expected " + node + " to have label " + label.Name() + ", but instead had " + asList(node.Labels) );
										}
								  }
							 }
						}
      
						tx.Success();
					  }
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogRelationshipsReferringToMissingNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogRelationshipsReferringToMissingNode()
		 {
			  // GIVEN
			  IList<string> nodeIds = new IList<string> { "a", "b", "c" };
			  Configuration config = Configuration.COMMAS;
			  File nodeData = nodeData( true, config, nodeIds, @true );
			  IList<RelationshipDataLine> relationships = Arrays.asList( Relationship( "a", "b", "TYPE", "aa" ), Relationship( "c", "bogus", "TYPE", "bb" ), Relationship( "b", "c", "KNOWS", "cc" ), Relationship( "c", "a", "KNOWS", "dd" ), Relationship( "missing", "a", "KNOWS", "ee" ) ); // line 3 of file2
			  File relationshipData1 = RelationshipData( true, config, relationships.GetEnumerator(), Lines(0, 2), true );
			  File relationshipData2 = RelationshipData( false, config, relationships.GetEnumerator(), Lines(2, 5), true );
			  File bad = BadFile();

			  // WHEN importing data where some relationships refer to missing nodes
			  ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--nodes", nodeData.AbsolutePath, "--bad", bad.AbsolutePath, "--bad-tolerance", "2", "--relationships", relationshipData1.AbsolutePath + MULTI_FILE_DELIMITER + relationshipData2.AbsolutePath );

			  // THEN
			  string badContents = FileUtils.readTextFile( bad, Charset.defaultCharset() );
			  assertTrue( "Didn't contain first bad relationship", badContents.Contains( "bogus" ) );
			  assertTrue( "Didn't contain second bad relationship", badContents.Contains( "missing" ) );
			  VerifyRelationships( relationships );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void skipLoggingOfBadEntries() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SkipLoggingOfBadEntries()
		 {
			  // GIVEN
			  IList<string> nodeIds = new IList<string> { "a", "b", "c" };
			  Configuration config = Configuration.COMMAS;
			  File nodeData = nodeData( true, config, nodeIds, @true );
			  IList<RelationshipDataLine> relationships = Arrays.asList( Relationship( "a", "b", "TYPE", "aa" ), Relationship( "c", "bogus", "TYPE", "bb" ), Relationship( "b", "c", "KNOWS", "cc" ), Relationship( "c", "a", "KNOWS", "dd" ), Relationship( "missing", "a", "KNOWS", "ee" ) ); // line 3 of file2
			  File relationshipData1 = RelationshipData( true, config, relationships.GetEnumerator(), Lines(0, 2), true );
			  File relationshipData2 = RelationshipData( false, config, relationships.GetEnumerator(), Lines(2, 5), true );

			  // WHEN importing data where some relationships refer to missing nodes
			  ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--nodes", nodeData.AbsolutePath, "--bad-tolerance", "2", "--skip-bad-entries-logging", "true", "--relationships", relationshipData1.AbsolutePath + MULTI_FILE_DELIMITER + relationshipData2.AbsolutePath );

			  assertFalse( BadFile().exists() );
			  VerifyRelationships( relationships );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailIfTooManyBadRelationships() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailIfTooManyBadRelationships()
		 {
			  // GIVEN
			  IList<string> nodeIds = new IList<string> { "a", "b", "c" };
			  Configuration config = Configuration.COMMAS;
			  File nodeData = nodeData( true, config, nodeIds, @true );
			  IList<RelationshipDataLine> relationships = Arrays.asList( Relationship( "a", "b", "TYPE" ), Relationship( "c", "bogus", "TYPE" ), Relationship( "b", "c", "KNOWS" ), Relationship( "c", "a", "KNOWS" ), Relationship( "missing", "a", "KNOWS" ) ); // line 3 of file2
			  File relationshipData = relationshipData( true, config, relationships.GetEnumerator(), @true, true );
			  File bad = BadFile();

			  // WHEN importing data where some relationships refer to missing nodes
			  try
			  {
					ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--nodes", nodeData.AbsolutePath, "--bad", bad.AbsolutePath, "--bad-tolerance", "1", "--relationships", relationshipData.AbsolutePath );
					fail();
			  }
			  catch ( Exception e )
			  {
					// THEN
					AssertExceptionContains( e, relationshipData.AbsolutePath, typeof( InputException ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToDisableSkippingOfBadRelationships() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToDisableSkippingOfBadRelationships()
		 {
			  // GIVEN
			  IList<string> nodeIds = new IList<string> { "a", "b", "c" };
			  Configuration config = Configuration.COMMAS;
			  File nodeData = nodeData( true, config, nodeIds, @true );

			  IList<RelationshipDataLine> relationships = Arrays.asList( Relationship( "a", "b", "TYPE" ), Relationship( "c", "bogus", "TYPE" ) ); //    line 3 of file1

			  File relationshipData1 = RelationshipData( true, config, relationships.GetEnumerator(), Lines(0, 2), true );
			  File relationshipData2 = RelationshipData( false, config, relationships.GetEnumerator(), Lines(2, 5), true );
			  File bad = BadFile();

			  // WHEN importing data where some relationships refer to missing nodes
			  try
			  {
					ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--nodes", nodeData.AbsolutePath, "--bad", bad.AbsolutePath, "--stacktrace", "--skip-bad-relationships", "false", "--relationships", relationshipData1.AbsolutePath + MULTI_FILE_DELIMITER + relationshipData2.AbsolutePath );
					fail();
			  }
			  catch ( Exception e )
			  {
					// THEN
					AssertExceptionContains( e, relationshipData1.AbsolutePath, typeof( InputException ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleAdditiveLabelsWithSpaces() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleAdditiveLabelsWithSpaces()
		 {
			  // GIVEN
			  IList<string> nodeIds = nodeIds();
			  Configuration config = Configuration.COMMAS;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.graphdb.Label label1 = label("My First Label");
			  Label label1 = label( "My First Label" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.graphdb.Label label2 = label("My Other Label");
			  Label label2 = label( "My Other Label" );

			  // WHEN
			  ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--nodes:My First Label:My Other Label", NodeData( true, config, nodeIds, @true ).AbsolutePath, "--relationships", RelationshipData( true, config, nodeIds, @true, true ).AbsolutePath );

			  // THEN
			  VerifyData(node =>
			  {
				assertTrue( node.hasLabel( label1 ) );
				assertTrue( node.hasLabel( label2 ) );
			  }, Validators.emptyValidator());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldImportFromInputDataEncodedWithSpecificCharset() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldImportFromInputDataEncodedWithSpecificCharset()
		 {
			  // GIVEN
			  IList<string> nodeIds = nodeIds();
			  Configuration config = Configuration.COMMAS;
			  Charset charset = StandardCharsets.UTF_16;

			  // WHEN
			  ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--input-encoding", charset.name(), "--nodes", NodeData(true, config, nodeIds, @true, charset).AbsolutePath, "--relationships", RelationshipData(true, config, nodeIds, @true, true, charset).AbsolutePath );

			  // THEN
			  VerifyData();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDisallowImportWithoutNodesInput() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDisallowImportWithoutNodesInput()
		 {
			  // GIVEN
			  IList<string> nodeIds = nodeIds();
			  Configuration config = Configuration.COMMAS;

			  // WHEN
			  try
			  {
					ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--relationships", RelationshipData( true, config, nodeIds, @true, true ).AbsolutePath );
					fail( "Should have failed" );
			  }
			  catch ( System.ArgumentException e )
			  {
					// THEN
					assertThat( e.Message, containsString( "No node input" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToImportAnonymousNodes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToImportAnonymousNodes()
		 {
			  // GIVEN
			  IList<string> nodeIds = new IList<string> { "1", "", "", "", "3", "", "", "", "", "", "5" };
			  Configuration config = Configuration.COMMAS;
			  IList<RelationshipDataLine> relationshipData = new IList<RelationshipDataLine> { Relationship( "1", "3", "KNOWS" ) };

			  // WHEN
			  ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--nodes", NodeData( true, config, nodeIds, @true ).AbsolutePath, "--relationships", relationshipData( true, config, relationshipData.GetEnumerator(), @true, true ).AbsolutePath );

			  // THEN
			  IGraphDatabaseService db = DbRule.GraphDatabaseAPI;
			  using ( Transaction tx = Db.beginTx() )
			  {
					IEnumerable<Node> allNodes = Db.AllNodes;
					int anonymousCount = 0;
					foreach ( String id in nodeIds )
					{
						 if ( id.Empty )
						 {
							  anonymousCount++;
						 }
						 else
						 {
							  assertNotNull( Iterators.single( Iterators.filter( NodeFilter( id ), allNodes.GetEnumerator() ) ) );
						 }
					}
					assertEquals( anonymousCount, count( Iterators.filter( NodeFilter( "" ), allNodes.GetEnumerator() ) ) );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDisallowMultilineFieldsByDefault() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDisallowMultilineFieldsByDefault()
		 {
			  // GIVEN
			  File data = data( ":ID,name", "1,\"This is a line with\nnewlines in\"" );

			  // WHEN
			  try
			  {
					ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--nodes", data.AbsolutePath );
					fail();
			  }
			  catch ( Exception e )
			  {
					// THEN
					AssertExceptionContains( e, "Multi-line", typeof( IllegalMultilineFieldException ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotTrimStringsByDefault() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotTrimStringsByDefault()
		 {
			  // GIVEN
			  string name = "  This is a line with leading and trailing whitespaces   ";
			  File data = data( ":ID,name", "1,\"" + name + "\"" );

			  // WHEN
			  ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--nodes", data.AbsolutePath );

			  // THEN
			  IGraphDatabaseService db = DbRule.GraphDatabaseAPI;
			  using ( Transaction tx = Db.beginTx() )
			  {
					ResourceIterator<Node> allNodes = Db.AllNodes.GetEnumerator();
					Node node = Iterators.single( allNodes );
					allNodes.Close();

					assertEquals( name, node.GetProperty( "name" ) );

					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTrimStringsIfConfiguredTo() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTrimStringsIfConfiguredTo()
		 {
			  // GIVEN
			  string name = "  This is a line with leading and trailing whitespaces   ";
			  File data = data( ":ID,name", "1,\"" + name + "\"", "2," + name );

			  // WHEN
			  ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--nodes", data.AbsolutePath, "--trim-strings", "true" );

			  // THEN
			  IGraphDatabaseService db = DbRule.GraphDatabaseAPI;
			  using ( Transaction tx = Db.beginTx(), ResourceIterator<Node> allNodes = Db.AllNodes.GetEnumerator() )
			  {
					ISet<string> names = new HashSet<string>();
					while ( allNodes.MoveNext() )
					{
						 names.Add( allNodes.Current.getProperty( "name" ).ToString() );
					}

					assertTrue( names.remove( name ) );
					assertTrue( names.remove( name.Trim() ) );
					assertTrue( names.Count == 0 );

					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPrintReferenceLinkOnDataImportErrors()
		 public virtual void ShouldPrintReferenceLinkOnDataImportErrors()
		 {
			  string[] versionParts = Version.Neo4NetVersion.Split( "-", true );
			  versionParts[0] = versionParts[0].Substring( 0, 3 );
			  string docsVersion = string.join( "-", versionParts );

			  ShouldPrintReferenceLinkAsPartOfErrorMessage( NodeIds(), Iterators.iterator(new RelationshipDataLine("1", "", "type", "name")), "Relationship missing mandatory field 'END_ID', read more about relationship " + "format in the manual:  https://Neo4Net.com/docs/operations-manual/" + docsVersion + "/tools/import/file-header-format/#import-tool-header-format-rels" );
			  ShouldPrintReferenceLinkAsPartOfErrorMessage( NodeIds(), Iterators.iterator(new RelationshipDataLine("", "1", "type", "name")), "Relationship missing mandatory field 'START_ID', read more about relationship " + "format in the manual:  https://Neo4Net.com/docs/operations-manual/" + docsVersion + "/tools/import/file-header-format/#import-tool-header-format-rels" );
			  ShouldPrintReferenceLinkAsPartOfErrorMessage( NodeIds(), Iterators.iterator(new RelationshipDataLine("1", "2", "", "name")), "Relationship missing mandatory field 'TYPE', read more about relationship " + "format in the manual:  https://Neo4Net.com/docs/operations-manual/" + docsVersion + "/tools/import/file-header-format/#import-tool-header-format-rels" );
			  ShouldPrintReferenceLinkAsPartOfErrorMessage( Arrays.asList( "1", "1" ), Iterators.iterator( new RelationshipDataLine( "1", "2", "type", "name" ) ), "Duplicate input ids that would otherwise clash can be put into separate id space, read more " + "about how to use id spaces in the manual: https://Neo4Net.com/docs/operations-manual/" + docsVersion + "/tools/import/file-header-format/#import-tool-id-spaces" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCollectUnlimitedNumberOfBadEntries() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCollectUnlimitedNumberOfBadEntries()
		 {
			  // GIVEN
			  IList<string> nodeIds = Collections.nCopies( 10_000, "A" );

			  // WHEN
			  ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--nodes", NodeData( true, Configuration.COMMAS, nodeIds, @true ).AbsolutePath, "--skip-duplicate-nodes", "--bad-tolerance", "true" );

			  // THEN
			  // all those duplicates should just be accepted using the - for specifying bad tolerance
		 }

		 private void ShouldPrintReferenceLinkAsPartOfErrorMessage( IList<string> nodeIds, IEnumerator<RelationshipDataLine> relationshipDataLines, string message )
		 {
			  Configuration config = Configuration.COMMAS;
			  try
			  {
					// WHEN
					ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--nodes", NodeData( true, config, nodeIds, @true ).AbsolutePath, "--skip-bad-relationships", "false", "--relationships", RelationshipData( true, config, relationshipDataLines, @true, true ).AbsolutePath );
					fail( " Should fail during import." );
			  }
			  catch ( Exception )
			  {
					// EXPECT
					assertTrue( SuppressOutput.ErrorVoice.containsMessage( message ) );
			  }

			  foreach ( StoreType storeType in StoreType.values() )
			  {
					if ( storeType.RecordStore )
					{
						 DbRule.databaseLayout().file(storeType.DatabaseFile).forEach(File.delete);
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowMultilineFieldsWhenEnabled() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowMultilineFieldsWhenEnabled()
		 {
			  // GIVEN
			  File data = data( ":ID,name", "1,\"This is a line with\nnewlines in\"" );

			  // WHEN
			  ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--nodes", data.AbsolutePath, "--multiline-fields", "true" );

			  // THEN
			  IGraphDatabaseService db = DbRule.GraphDatabaseAPI;
			  using ( Transaction tx = Db.beginTx() )
			  {
					ResourceIterator<Node> allNodes = Db.AllNodes.GetEnumerator();
					Node node = Iterators.single( allNodes );
					allNodes.Close();

					assertEquals( "This is a line with\nnewlines in", node.GetProperty( "name" ) );

					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSkipEmptyFiles() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSkipEmptyFiles()
		 {
			  // GIVEN
			  File data = data( "" );

			  // WHEN
			  ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--nodes", data.AbsolutePath );

			  // THEN
			  IGraphDatabaseService IGraphDatabaseService = DbRule.GraphDatabaseAPI;
			  using ( Transaction tx = IGraphDatabaseService.BeginTx() )
			  {
					ResourceIterator<Node> allNodes = IGraphDatabaseService.AllNodes.GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( "Expected database to be empty", allNodes.hasNext() );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreEmptyQuotedStringsIfConfiguredTo() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIgnoreEmptyQuotedStringsIfConfiguredTo()
		 {
			  // GIVEN
			  File data = data( ":ID,one,two,three", "1,\"\",,value" );

			  // WHEN
			  ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--nodes", data.AbsolutePath, "--ignore-empty-strings", "true" );

			  // THEN
			  IGraphDatabaseService db = DbRule.GraphDatabaseAPI;
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Iterables.single( Db.AllNodes );
					assertFalse( node.HasProperty( "one" ) );
					assertFalse( node.HasProperty( "two" ) );
					assertEquals( "value", node.GetProperty( "three" ) );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPrintUserFriendlyMessageAboutUnsupportedMultilineFields() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPrintUserFriendlyMessageAboutUnsupportedMultilineFields()
		 {
			  // GIVEN
			  File data = data( ":ID,name", "1,\"one\ntwo\nthree\"", "2,four" );

			  try
			  {
					ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--nodes", data.AbsolutePath, "--multiline-fields", "false" );
					fail( "Should have failed" );
			  }
			  catch ( InputException )
			  {
					// THEN
					assertTrue( SuppressOutput.ErrorVoice.containsMessage( "Detected field which spanned multiple lines" ) );
					assertTrue( SuppressOutput.ErrorVoice.containsMessage( "multiline-fields" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcceptRawAsciiCharacterCodeAsQuoteConfiguration() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAcceptRawAsciiCharacterCodeAsQuoteConfiguration()
		 {
			  // GIVEN
			  char weirdDelimiter = ( char )1; // not '1', just the character represented with code 1, which seems to be SOH
			  string name1 = weirdDelimiter + "Weird" + weirdDelimiter;
			  string name2 = "Start " + weirdDelimiter + "middle thing" + weirdDelimiter + " end!";
			  File data = data( ":ID,name", "1," + name1, "2," + name2 );

			  // WHEN
			  ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--nodes", data.AbsolutePath, "--quote", weirdDelimiter.ToString() );

			  // THEN
			  ISet<string> names = asSet( "Weird", name2 );
			  IGraphDatabaseService db = DbRule.GraphDatabaseAPI;
			  using ( Transaction tx = Db.beginTx() )
			  {
					foreach ( Node node in Db.AllNodes )
					{
						 string name = ( string ) node.GetProperty( "name" );
						 assertTrue( "Didn't expect node with name '" + name + "'", names.remove( name ) );
					}
					assertTrue( names.Count == 0 );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcceptSpecialTabCharacterAsDelimiterConfiguration() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAcceptSpecialTabCharacterAsDelimiterConfiguration()
		 {
			  // GIVEN
			  IList<string> nodeIds = nodeIds();
			  Configuration config = Configuration.TABS;

			  // WHEN
			  ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--delimiter", "\\t", "--array-delimiter", config.ArrayDelimiter().ToString(), "--nodes", NodeData(true, config, nodeIds, @true).AbsolutePath, "--relationships", RelationshipData(true, config, nodeIds, @true, true).AbsolutePath );

			  // THEN
			  VerifyData();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportBadDelimiterConfiguration() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportBadDelimiterConfiguration()
		 {
			  // GIVEN
			  IList<string> nodeIds = nodeIds();
			  Configuration config = Configuration.TABS;

			  // WHEN
			  try
			  {
					ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--delimiter", "\\bogus", "--array-delimiter", config.ArrayDelimiter().ToString(), "--nodes", NodeData(true, config, nodeIds, @true).AbsolutePath, "--relationships", RelationshipData(true, config, nodeIds, @true, true).AbsolutePath );
					fail( "Should have failed" );
			  }
			  catch ( System.ArgumentException e )
			  {
					// THEN
					assertThat( e.Message, containsString( "bogus" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailAndReportStartingLineForUnbalancedQuoteInMiddle() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailAndReportStartingLineForUnbalancedQuoteInMiddle()
		 {
			  // GIVEN
			  int unbalancedStartLine = 10;

			  // WHEN
			  try
			  {
					ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--nodes", NodeDataWithMissingQuote( 2 * unbalancedStartLine, unbalancedStartLine ).AbsolutePath );
					fail( "Should have failed" );
			  }
			  catch ( InputException e )
			  {
					// THEN
					assertThat( e.Message, containsString( string.Format( "Multi-line fields are illegal", unbalancedStartLine ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcceptRawEscapedAsciiCodeAsQuoteConfiguration() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAcceptRawEscapedAsciiCodeAsQuoteConfiguration()
		 {
			  // GIVEN
			  char weirdDelimiter = ( char )1; // not '1', just the character represented with code 1, which seems to be SOH
			  string name1 = weirdDelimiter + "Weird" + weirdDelimiter;
			  string name2 = "Start " + weirdDelimiter + "middle thing" + weirdDelimiter + " end!";
			  File data = data( ":ID,name", "1," + name1, "2," + name2 );

			  // WHEN
			  ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--nodes", data.AbsolutePath, "--quote", "\\x0001" );

			  // THEN
			  ISet<string> names = asSet( "Weird", name2 );
			  IGraphDatabaseService db = DbRule.GraphDatabaseAPI;
			  using ( Transaction tx = Db.beginTx() )
			  {
					foreach ( Node node in Db.AllNodes )
					{
						 string name = ( string ) node.GetProperty( "name" );
						 assertTrue( "Didn't expect node with name '" + name + "'", names.remove( name ) );
					}
					assertTrue( names.Count == 0 );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailAndReportStartingLineForUnbalancedQuoteAtEnd() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailAndReportStartingLineForUnbalancedQuoteAtEnd()
		 {
			  // GIVEN
			  int unbalancedStartLine = 10;

			  // WHEN
			  try
			  {
					ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--nodes", NodeDataWithMissingQuote( unbalancedStartLine, unbalancedStartLine ).AbsolutePath );
					fail( "Should have failed" );
			  }
			  catch ( InputException e )
			  {
					// THEN
					assertThat( e.Message, containsString( string.Format( "Multi-line fields" ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeEquivalentToUseRawAsciiOrCharacterAsQuoteConfiguration1() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeEquivalentToUseRawAsciiOrCharacterAsQuoteConfiguration1()
		 {
			  // GIVEN
			  char weirdDelimiter = ( char )126; // 126 ~ (tilde)
			  string weirdStringDelimiter = "\\x0056";
			  string name1 = weirdDelimiter + "Weird" + weirdDelimiter;
			  string name2 = "Start " + weirdDelimiter + "middle thing" + weirdDelimiter + " end!";
			  File data = data( ":ID,name", "1," + name1, "2," + name2 );

			  // WHEN given as raw ascii
			  ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--nodes", data.AbsolutePath, "--quote", weirdStringDelimiter );

			  // THEN
			  assertEquals( "~", "" + weirdDelimiter );
			  assertEquals( "~"[0], weirdDelimiter );

			  ISet<string> names = asSet( "Weird", name2 );
			  IGraphDatabaseService db = DbRule.GraphDatabaseAPI;
			  using ( Transaction tx = Db.beginTx() )
			  {
					foreach ( Node node in Db.AllNodes )
					{
						 string name = ( string ) node.GetProperty( "name" );
						 assertTrue( "Didn't expect node with name '" + name + "'", names.remove( name ) );
					}
					assertTrue( names.Count == 0 );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailOnUnbalancedQuoteWithMultilinesEnabled() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailOnUnbalancedQuoteWithMultilinesEnabled()
		 {
			  // GIVEN
			  int unbalancedStartLine = 10;

			  // WHEN
			  try
			  {
					ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--multiline-fields", "true", "--nodes", NodeDataWithMissingQuote( 2 * unbalancedStartLine, unbalancedStartLine ).AbsolutePath );
					fail( "Should have failed" );
			  }
			  catch ( InputException )
			  { // THEN OK
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File nodeDataWithMissingQuote(int totalLines, int unbalancedStartLine) throws Exception
		 private File NodeDataWithMissingQuote( int totalLines, int unbalancedStartLine )
		 {
			  string[] lines = new string[totalLines + 1];

			  lines[0] = "ID,:LABEL";

			  for ( int i = 1; i <= totalLines; i++ )
			  {
					StringBuilder line = new StringBuilder( string.Format( "{0:D},", i ) );
					if ( i == unbalancedStartLine )
					{
						 // Missing the end quote
						 line.Append( "\"Secret Agent" );
					}
					else
					{
						 line.Append( "Agent" );
					}
					lines[i] = line.ToString();
			  }

			  return Data( lines );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeEquivalentToUseRawAsciiOrCharacterAsQuoteConfiguration2() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeEquivalentToUseRawAsciiOrCharacterAsQuoteConfiguration2()
		 {
			  // GIVEN
			  char weirdDelimiter = ( char )126; // 126 ~ (tilde)
			  string weirdStringDelimiter = "~";
			  string name1 = weirdDelimiter + "Weird" + weirdDelimiter;
			  string name2 = "Start " + weirdDelimiter + "middle thing" + weirdDelimiter + " end!";
			  File data = data( ":ID,name", "1," + name1, "2," + name2 );

			  // WHEN given as string
			  ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--nodes", data.AbsolutePath, "--quote", weirdStringDelimiter );

			  // THEN
			  assertEquals( weirdStringDelimiter, "" + weirdDelimiter );
			  assertEquals( weirdStringDelimiter[0], weirdDelimiter );

			  ISet<string> names = asSet( "Weird", name2 );
			  IGraphDatabaseService db = DbRule.GraphDatabaseAPI;
			  using ( Transaction tx = Db.beginTx() )
			  {
					foreach ( Node node in Db.AllNodes )
					{
						 string name = ( string ) node.GetProperty( "name" );
						 assertTrue( "Didn't expect node with name '" + name + "'", names.remove( name ) );
					}
					assertTrue( names.Count == 0 );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespectDbConfig() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespectDbConfig()
		 {
			  // GIVEN
			  int arrayBlockSize = 10;
			  int stringBlockSize = 12;
			  File dbConfig = File( "Neo4Net.properties" );
			  store( stringMap( GraphDatabaseSettings.array_block_size.name(), arrayBlockSize.ToString(), GraphDatabaseSettings.string_block_size.name(), stringBlockSize.ToString() ), dbConfig );
			  IList<string> nodeIds = nodeIds();

			  // WHEN
			  ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--db-config", dbConfig.AbsolutePath, "--nodes", NodeData( true, Configuration.COMMAS, nodeIds, value => true ).AbsolutePath );

			  // THEN
			  NeoStores stores = DbRule.GraphDatabaseAPI.DependencyResolver.resolveDependency( typeof( RecordStorageEngine ) ).testAccessNeoStores();
			  int headerSize = Standard.LATEST_RECORD_FORMATS.dynamic().RecordHeaderSize;
			  assertEquals( arrayBlockSize + headerSize, stores.PropertyStore.ArrayStore.RecordSize );
			  assertEquals( stringBlockSize + headerSize, stores.PropertyStore.StringStore.RecordSize );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void useProvidedAdditionalConfig() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UseProvidedAdditionalConfig()
		 {
			  // GIVEN
			  int arrayBlockSize = 10;
			  int stringBlockSize = 12;
			  File dbConfig = File( "Neo4Net.properties" );
			  store( stringMap( GraphDatabaseSettings.array_block_size.name(), arrayBlockSize.ToString(), GraphDatabaseSettings.string_block_size.name(), stringBlockSize.ToString() ), dbConfig );
			  IList<string> nodeIds = nodeIds();

			  // WHEN
			  ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--additional-config", dbConfig.AbsolutePath, "--nodes", NodeData( true, Configuration.COMMAS, nodeIds, value => true ).AbsolutePath );

			  // THEN
			  NeoStores stores = DbRule.GraphDatabaseAPI.DependencyResolver.resolveDependency( typeof( RecordStorageEngine ) ).testAccessNeoStores();
			  int headerSize = Standard.LATEST_RECORD_FORMATS.dynamic().RecordHeaderSize;
			  assertEquals( arrayBlockSize + headerSize, stores.PropertyStore.ArrayStore.RecordSize );
			  assertEquals( stringBlockSize + headerSize, stores.PropertyStore.StringStore.RecordSize );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void combineProvidedDbAndAdditionalConfig() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CombineProvidedDbAndAdditionalConfig()
		 {
			  // GIVEN
			  int arrayBlockSize = 10;
			  int stringBlockSize = 12;
			  File dbConfig = File( "Neo4Net.properties" );
			  File additionalConfig = File( "additional.properties" );
			  store( stringMap( GraphDatabaseSettings.string_block_size.name(), stringBlockSize.ToString() ), dbConfig );
			  store( stringMap( GraphDatabaseSettings.array_block_size.name(), arrayBlockSize.ToString() ), additionalConfig );
			  IList<string> nodeIds = nodeIds();

			  // WHEN
			  ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--db-config", dbConfig.AbsolutePath, "--additional-config", additionalConfig.AbsolutePath, "--nodes", NodeData( true, Configuration.COMMAS, nodeIds, value => true ).AbsolutePath );

			  // THEN
			  NeoStores stores = DbRule.GraphDatabaseAPI.DependencyResolver.resolveDependency( typeof( RecordStorageEngine ) ).testAccessNeoStores();
			  int headerSize = Standard.LATEST_RECORD_FORMATS.dynamic().RecordHeaderSize;
			  assertEquals( arrayBlockSize + headerSize, stores.PropertyStore.ArrayStore.RecordSize );
			  assertEquals( stringBlockSize + headerSize, stores.PropertyStore.StringStore.RecordSize );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPrintStackTraceOnInputExceptionIfToldTo() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPrintStackTraceOnInputExceptionIfToldTo()
		 {
			  // GIVEN
			  IList<string> nodeIds = nodeIds();
			  Configuration config = Configuration.COMMAS;

			  // WHEN data file contains more columns than header file
			  int extraColumns = 3;
			  try
			  {
					ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--nodes", NodeHeader( config ).AbsolutePath + MULTI_FILE_DELIMITER + NodeData( false, config, nodeIds, @true, Charset.defaultCharset(), extraColumns ).AbsolutePath, "--stacktrace" );

					fail( "Should have thrown exception" );
			  }
			  catch ( InputException e )
			  {
					// THEN
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					assertTrue( SuppressOutput.ErrorVoice.containsMessage( e.GetType().FullName ) );
					assertTrue( e.Message.contains( "Extra column not present in header on line" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDisableLegacyStyleQuotingIfToldTo() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDisableLegacyStyleQuotingIfToldTo()
		 {
			  // GIVEN
			  string nodeId = "me";
			  string labelName = "Alive";
			  IList<string> lines = new List<string>();
			  lines.Add( ":ID,name,:LABEL" );
			  lines.Add( nodeId + "," + "\"abc\"\"def\\\"\"ghi\"" + "," + labelName );

			  // WHEN
			  ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--nodes", Data( lines.ToArray() ).AbsolutePath, "--legacy-style-quoting", "false", "--stacktrace" );

			  // THEN
			  IGraphDatabaseService db = DbRule.GraphDatabaseAPI;
			  using ( Transaction tx = Db.beginTx() )
			  {
					assertNotNull( Db.findNode( label( labelName ), "name", "abc\"def\\\"ghi" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespectBufferSizeSetting() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespectBufferSizeSetting()
		 {
			  // GIVEN
			  IList<string> lines = new List<string>();
			  lines.Add( ":ID,name,:LABEL" );
			  lines.Add( "id," + repeat( 'l', 2_000 ) + ",Person" );

			  // WHEN
			  try
			  {
					ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--nodes", Data( lines.ToArray() ).AbsolutePath, "--read-buffer-size", "1k" );
					fail( "Should've failed" );
			  }
			  catch ( System.InvalidOperationException e )
			  {
					// THEN good
					assertThat( e.Message, containsString( "input data" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespectMaxMemoryPercentageSetting() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespectMaxMemoryPercentageSetting()
		 {
			  // GIVEN
			  IList<string> nodeIds = nodeIds( 10 );

			  // WHEN
			  ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--nodes", NodeData( true, Configuration.COMMAS, nodeIds, @true ).AbsolutePath, "--max-memory", "60%" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailOnInvalidMaxMemoryPercentageSetting() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailOnInvalidMaxMemoryPercentageSetting()
		 {
			  // GIVEN
			  IList<string> nodeIds = nodeIds( 10 );

			  try
			  {
					// WHEN
					ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--nodes", NodeData( true, Configuration.COMMAS, nodeIds, @true ).AbsolutePath, "--max-memory", "110%" );
					fail( "Should have failed" );
			  }
			  catch ( System.ArgumentException e )
			  {
					// THEN good
					assertThat( e.Message, containsString( "percent" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespectMaxMemorySuffixedSetting() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespectMaxMemorySuffixedSetting()
		 {
			  // GIVEN
			  IList<string> nodeIds = nodeIds( 10 );

			  // WHEN
			  ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--nodes", NodeData( true, Configuration.COMMAS, nodeIds, @true ).AbsolutePath, "--max-memory", "100M" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTreatRelationshipWithMissingStartOrEndIdOrTypeAsBadRelationship() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTreatRelationshipWithMissingStartOrEndIdOrTypeAsBadRelationship()
		 {
			  // GIVEN
			  IList<string> nodeIds = new IList<string> { "a", "b", "c" };
			  Configuration config = Configuration.COMMAS;
			  File nodeData = nodeData( true, config, nodeIds, @true );

			  IList<RelationshipDataLine> relationships = Arrays.asList( Relationship( "a", null, "TYPE" ), Relationship( null, "b", "TYPE" ), Relationship( "a", "b", null ) );

			  File relationshipData = relationshipData( true, config, relationships.GetEnumerator(), @true, true );
			  File bad = BadFile();

			  // WHEN importing data where some relationships refer to missing nodes
			  ImportTool( "--into", DbRule.DatabaseDirAbsolutePath, "--nodes", nodeData.AbsolutePath, "--bad", bad.AbsolutePath, "--skip-bad-relationships", "true", "--relationships", relationshipData.AbsolutePath );

			  string badContents = FileUtils.readTextFile( bad, Charset.defaultCharset() );
			  assertEquals( badContents, 3, OccurencesOf( badContents, "is missing data" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldKeepStoreFilesAfterFailedImport() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldKeepStoreFilesAfterFailedImport()
		 {
			  // GIVEN
			  IList<string> nodeIds = nodeIds();
			  Configuration config = Configuration.COMMAS;

			  // WHEN data file contains more columns than header file
			  int extraColumns = 3;
			  string databaseDir = DbRule.DatabaseDirAbsolutePath;
			  try
			  {
					ImportTool( "--into", databaseDir, "--nodes", NodeHeader( config ).AbsolutePath + MULTI_FILE_DELIMITER + NodeData( false, config, nodeIds, @true, Charset.defaultCharset(), extraColumns ).AbsolutePath );
					fail( "Should have thrown exception" );
			  }
			  catch ( InputException )
			  {
					// THEN the store files should be there
					foreach ( StoreType storeType in StoreType.values() )
					{
						 if ( storeType.RecordStore )
						 {
							  DbRule.databaseLayout().file(storeType.DatabaseFile).forEach(f => assertTrue(f.exists()));
						 }
					}

					IList<string> errorLines = SuppressOutput.ErrorVoice.lines();
					AssertContains( errorLines, "Starting a database on these store files will likely fail or observe inconsistent records" );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSupplyArgumentsAsFile() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSupplyArgumentsAsFile()
		 {
			  // given
			  IList<string> nodeIds = nodeIds();
			  Configuration config = Configuration.COMMAS;
			  File argumentFile = File( "args" );
			  string nodesEscapedSpaces = EscapePath( NodeData( true, config, nodeIds, @true ).AbsolutePath );
			  string relationshipsEscapedSpaced = EscapePath( RelationshipData( true, config, nodeIds, @true, true ).AbsolutePath );
			  string arguments = format( "--into %s%n" + "--nodes %s --relationships %s", EscapePath( DbRule.DatabaseDirAbsolutePath ), nodesEscapedSpaces, relationshipsEscapedSpaced );
			  writeToFile( argumentFile, arguments, false );

			  // when
			  ImportTool( "-f", argumentFile.AbsolutePath );

			  // then
			  VerifyData();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailIfSupplyingBothFileArgumentAndAnyOtherArgument() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailIfSupplyingBothFileArgumentAndAnyOtherArgument()
		 {
			  // given
			  IList<string> nodeIds = nodeIds();
			  Configuration config = Configuration.COMMAS;
			  File argumentFile = File( "args" );
			  string arguments = format( "--into %s%n" + "--nodes %s --relationships %s", DbRule.DatabaseDirAbsolutePath, NodeData( true, config, nodeIds, @true ).AbsolutePath, RelationshipData( true, config, nodeIds, @true, true ).AbsolutePath );
			  writeToFile( argumentFile, arguments, false );

			  try
			  {
					// when
					ImportTool( "-f", argumentFile.AbsolutePath, "--into", DbRule.DatabaseDirAbsolutePath );
					fail( "Should have failed" );
			  }
			  catch ( System.ArgumentException e )
			  {
					// then good
					assertThat( e.Message, containsString( "in addition to" ) );
					assertThat( e.Message, containsString( ImportTool.Options.File.argument() ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateDebugLogInExpectedPlace() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateDebugLogInExpectedPlace()
		 {
			  // The ImportTool is more embedded-db-focused where typically the debug.log ends up in in a `logs/debug.log` next to the db directory,
			  // i.e. in <dbDir>/../logs/debug.log

			  // given
			  string dbDir = DbRule.DatabaseDirAbsolutePath;
			  ImportTool( "--into", dbDir, "--nodes", NodeData( true, Configuration.COMMAS, NodeIds(), @true ).AbsolutePath );

			  // THEN go and read the debug.log where it's expected to be and see if there's an IMPORT DONE line in it
			  File dbDirParent = ( new File( dbDir ) ).ParentFile;
			  File logsDir = new File( dbDirParent, logs_directory.DefaultValue );
			  File internalLogFile = new File( logsDir, Config.defaults().get(store_internal_log_path).Name );
			  assertTrue( internalLogFile.exists() );
			  IList<string> lines = Files.readAllLines( internalLogFile.toPath() );
			  assertTrue( lines.Any( line => line.contains( "Import completed successfully" ) ) );
		 }

		 private static void AssertContains( IList<string> errorLines, string @string )
		 {
			  foreach ( string line in errorLines )
			  {
					if ( line.Contains( @string ) )
					{
						 return;
					}
			  }
			  fail( "Expected error lines " + join( errorLines.ToArray(), format("%n") ) + " to have at least one line containing the string '" + @string + "'" );
		 }

		 private static int OccurencesOf( string text, string lookFor )
		 {
			  int index = -1;
			  int count = -1;
			  do
			  {
					count++;
					index = text.IndexOf( lookFor, index + 1, StringComparison.Ordinal );
			  } while ( index != -1 );
			  return count;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File writeArrayCsv(String[] headers, String[] values) throws java.io.FileNotFoundException
		 private File WriteArrayCsv( string[] headers, string[] values )
		 {
			  File data = File( FileName( "whitespace.csv" ) );
			  using ( PrintStream writer = new PrintStream( data ) )
			  {
					writer.print( ":LABEL" );
					foreach ( string header in headers )
					{
						 writer.print( "," + header );
					}
					// End line
					writer.println();

					// Save value as a String in name
					writer.print( "PERSON" );
					// For each type
					foreach ( string ignored in headers )
					{
						 bool comma = true;
						 foreach ( string value in values )
						 {
							  if ( comma )
							  {
									writer.print( "," );
									comma = false;
							  }
							  else
							  {
									writer.print( ";" );
							  }
							  writer.print( value );
						 }
					}
					// End line
					writer.println();
			  }
			  return data;
		 }

		 private string JoinStringArray( string[] values )
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  return java.util.values.Select( string.Trim ).collect( joining( ", ", "[", "]" ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File data(String... lines) throws Exception
		 private File Data( params string[] lines )
		 {
			  File file = file( FileName( "data.csv" ) );
			  using ( PrintStream writer = writer( file, Charset.defaultCharset() ) )
			  {
					foreach ( string line in lines )
					{
						 writer.println( line );
					}
			  }
			  return file;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private System.Predicate<org.Neo4Net.graphdb.Node> nodeFilter(final String id)
		 private System.Predicate<Node> NodeFilter( string id )
		 {
			  return node => node.getProperty( "id", "" ).Equals( id );
		 }

		 private void VerifyData()
		 {
			  VerifyData( Validators.emptyValidator(), Validators.emptyValidator() );
		 }

		 private void VerifyData( Validator<Node> nodeAdditionalValidation, Validator<Relationship> relationshipAdditionalValidation )
		 {
			  VerifyData( NODE_COUNT, RELATIONSHIP_COUNT, nodeAdditionalValidation, relationshipAdditionalValidation );
		 }

		 private void VerifyData( int expectedNodeCount, int expectedRelationshipCount, Validator<Node> nodeAdditionalValidation, Validator<Relationship> relationshipAdditionalValidation )
		 {
			  IGraphDatabaseService db = DbRule.GraphDatabaseAPI;
			  using ( Transaction tx = Db.beginTx() )
			  {
					int nodeCount = 0;
					int relationshipCount = 0;
					foreach ( Node node in Db.AllNodes )
					{
						 assertTrue( node.HasProperty( "name" ) );
						 nodeAdditionalValidation.Validate( node );
						 nodeCount++;
					}
					assertEquals( expectedNodeCount, nodeCount );
					foreach ( Relationship relationship in Db.AllRelationships )
					{
						 assertTrue( relationship.HasProperty( "created" ) );
						 relationshipAdditionalValidation.Validate( relationship );
						 relationshipCount++;
					}
					assertEquals( expectedRelationshipCount, relationshipCount );
					tx.Success();
			  }
		 }

		 private void VerifyRelationships( IList<RelationshipDataLine> relationships )
		 {
			  IGraphDatabaseService db = DbRule.GraphDatabaseAPI;
			  IDictionary<string, Node> nodesById = AllNodesById( db );
			  using ( Transaction tx = Db.beginTx() )
			  {
					foreach ( RelationshipDataLine relationship in relationships )
					{
						 Node startNode = nodesById[relationship.StartNodeId];
						 Node endNode = nodesById[relationship.EndNodeId];
						 if ( startNode == null || endNode == null )
						 {
							  // OK this is a relationship referring to a missing node, skip it
							  continue;
						 }
						 assertNotNull( relationship.ToString(), FindRelationship(startNode, endNode, relationship) );
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private org.Neo4Net.graphdb.Relationship findRelationship(org.Neo4Net.graphdb.Node startNode, final org.Neo4Net.graphdb.Node endNode, final RelationshipDataLine relationship)
		 private Relationship FindRelationship( Node startNode, Node endNode, RelationshipDataLine relationship )
		 {
			  return Iterators.singleOrNull( Iterators.filter( item => item.EndNode.Equals( endNode ) && item.getProperty( "name" ).Equals( relationship.Name ), startNode.GetRelationships( withName( relationship.Type ) ).GetEnumerator() ) );
		 }

		 private IDictionary<string, Node> AllNodesById( IGraphDatabaseService db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					IDictionary<string, Node> nodes = new Dictionary<string, Node>();
					foreach ( Node node in Db.AllNodes )
					{
						 nodes[IdOf( node )] = node;
					}
					tx.Success();
					return nodes;
			  }
		 }

		 private string IdOf( Node node )
		 {
			  return ( string ) node.GetProperty( "id" );
		 }

		 private IList<string> NodeIds()
		 {
			  return NodeIds( NODE_COUNT );
		 }

		 private IList<string> NodeIds( int count )
		 {
			  IList<string> ids = new List<string>();
			  for ( int i = 0; i < count; i++ )
			  {
					ids.Add( RandomNodeId() );
			  }
			  return ids;
		 }

		 private string RandomNodeId()
		 {
			  return System.Guid.randomUUID().ToString();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File nodeData(boolean includeHeader, org.Neo4Net.unsafe.impl.batchimport.input.csv.Configuration config, java.util.List<String> nodeIds, System.Func<int, boolean> linePredicate) throws Exception
		 private File NodeData( bool includeHeader, Configuration config, IList<string> nodeIds, System.Func<int, bool> linePredicate )
		 {
			  return NodeData( includeHeader, config, nodeIds, linePredicate, Charset.defaultCharset() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File nodeData(boolean includeHeader, org.Neo4Net.unsafe.impl.batchimport.input.csv.Configuration config, java.util.List<String> nodeIds, System.Func<int, boolean> linePredicate, java.nio.charset.Charset encoding) throws Exception
		 private File NodeData( bool includeHeader, Configuration config, IList<string> nodeIds, System.Func<int, bool> linePredicate, Charset encoding )
		 {
			  return NodeData( includeHeader, config, nodeIds, linePredicate, encoding, 0 );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File nodeData(boolean includeHeader, org.Neo4Net.unsafe.impl.batchimport.input.csv.Configuration config, java.util.List<String> nodeIds, System.Func<int, boolean> linePredicate, java.nio.charset.Charset encoding, int extraColumns) throws Exception
		 private File NodeData( bool includeHeader, Configuration config, IList<string> nodeIds, System.Func<int, bool> linePredicate, Charset encoding, int extraColumns )
		 {
			  File file = file( FileName( "nodes.csv" ) );
			  using ( PrintStream writer = writer( file, encoding ) )
			  {
					if ( includeHeader )
					{
						 WriteNodeHeader( writer, config, null );
					}
					WriteNodeData( writer, config, nodeIds, linePredicate, extraColumns );
			  }
			  return file;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.PrintStream writer(java.io.File file, java.nio.charset.Charset encoding) throws Exception
		 private PrintStream Writer( File file, Charset encoding )
		 {
			  return new PrintStream( file, encoding.name() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File nodeHeader(org.Neo4Net.unsafe.impl.batchimport.input.csv.Configuration config) throws Exception
		 private File NodeHeader( Configuration config )
		 {
			  return NodeHeader( config, null );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File nodeHeader(org.Neo4Net.unsafe.impl.batchimport.input.csv.Configuration config, String idGroup) throws Exception
		 private File NodeHeader( Configuration config, string idGroup )
		 {
			  return NodeHeader( config, idGroup, Charset.defaultCharset() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File nodeHeader(org.Neo4Net.unsafe.impl.batchimport.input.csv.Configuration config, String idGroup, java.nio.charset.Charset encoding) throws Exception
		 private File NodeHeader( Configuration config, string idGroup, Charset encoding )
		 {
			  File file = file( FileName( "nodes-header.csv" ) );
			  using ( PrintStream writer = writer( file, encoding ) )
			  {
					WriteNodeHeader( writer, config, idGroup );
			  }
			  return file;
		 }

		 private void WriteNodeHeader( PrintStream writer, Configuration config, string idGroup )
		 {
			  char delimiter = config.Delimiter();
			  writer.println( IdEntry( "id", Type.ID, idGroup ) + delimiter + "name" + delimiter + "labels:LABEL" );
		 }

		 private string IdEntry( string name, Type type, string idGroup )
		 {
			  return ( !string.ReferenceEquals( name, null ) ? name : "" ) + ":" + type.name() + (!string.ReferenceEquals(idGroup, null) ? "(" + idGroup + ")" : "");
		 }

		 private void WriteNodeData( PrintStream writer, Configuration config, IList<string> nodeIds, System.Func<int, bool> linePredicate, int extraColumns )
		 {
			  char delimiter = config.Delimiter();
			  char arrayDelimiter = config.ArrayDelimiter();
			  for ( int i = 0; i < nodeIds.Count; i++ )
			  {
					if ( linePredicate( i ) )
					{
						 writer.println( GetLine( nodeIds[i], delimiter, arrayDelimiter, extraColumns ) );
					}
			  }
		 }

		 private string GetLine( string nodeId, char delimiter, char arrayDelimiter, int extraColumns )
		 {
			  StringBuilder stringBuilder = ( new StringBuilder() ).Append(nodeId).Append(delimiter).Append(RandomName()).Append(delimiter).Append(RandomLabels(arrayDelimiter));

			  for ( int i = 0; i < extraColumns; i++ )
			  {
					stringBuilder.Append( delimiter ).Append( "ExtraColumn" ).Append( i );
			  }

			  return stringBuilder.ToString();
		 }

		 private string RandomLabels( char arrayDelimiter )
		 {
			  int length = Random.Next( 3 );
			  StringBuilder builder = new StringBuilder();
			  for ( int i = 0; i < length; i++ )
			  {
					if ( i > 0 )
					{
						 builder.Append( arrayDelimiter );
					}
					builder.Append( LabelName( Random.Next( MAX_LABEL_ID ) ) );
			  }
			  return builder.ToString();
		 }

		 private string LabelName( int number )
		 {
			  return "LABEL_" + number;
		 }

		 private string RandomName()
		 {
			  int length = Random.Next( 10 ) + 5;
			  StringBuilder builder = new StringBuilder();
			  for ( int i = 0; i < length; i++ )
			  {
					builder.Append( ( char )( 'a' + Random.Next( 20 ) ) );
			  }
			  return builder.ToString();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File relationshipData(boolean includeHeader, org.Neo4Net.unsafe.impl.batchimport.input.csv.Configuration config, java.util.List<String> nodeIds, System.Func<int, boolean> linePredicate, boolean specifyType) throws Exception
		 private File RelationshipData( bool includeHeader, Configuration config, IList<string> nodeIds, System.Func<int, bool> linePredicate, bool specifyType )
		 {
			  return RelationshipData( includeHeader, config, nodeIds, linePredicate, specifyType, Charset.defaultCharset() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File relationshipData(boolean includeHeader, org.Neo4Net.unsafe.impl.batchimport.input.csv.Configuration config, java.util.List<String> nodeIds, System.Func<int, boolean> linePredicate, boolean specifyType, java.nio.charset.Charset encoding) throws Exception
		 private File RelationshipData( bool includeHeader, Configuration config, IList<string> nodeIds, System.Func<int, bool> linePredicate, bool specifyType, Charset encoding )
		 {
			  return RelationshipData( includeHeader, config, RandomRelationships( nodeIds ), linePredicate, specifyType, encoding );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File relationshipData(boolean includeHeader, org.Neo4Net.unsafe.impl.batchimport.input.csv.Configuration config, java.util.Iterator<RelationshipDataLine> data, System.Func<int, boolean> linePredicate, boolean specifyType) throws Exception
		 private File RelationshipData( bool includeHeader, Configuration config, IEnumerator<RelationshipDataLine> data, System.Func<int, bool> linePredicate, bool specifyType )
		 {
			  return RelationshipData( includeHeader, config, data, linePredicate, specifyType, Charset.defaultCharset() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File relationshipData(boolean includeHeader, org.Neo4Net.unsafe.impl.batchimport.input.csv.Configuration config, java.util.Iterator<RelationshipDataLine> data, System.Func<int, boolean> linePredicate, boolean specifyType, java.nio.charset.Charset encoding) throws Exception
		 private File RelationshipData( bool includeHeader, Configuration config, IEnumerator<RelationshipDataLine> data, System.Func<int, bool> linePredicate, bool specifyType, Charset encoding )
		 {
			  File file = file( FileName( "relationships.csv" ) );
			  using ( PrintStream writer = writer( file, encoding ) )
			  {
					if ( includeHeader )
					{
						 WriteRelationshipHeader( writer, config, null, null, specifyType );
					}
					WriteRelationshipData( writer, config, data, linePredicate, specifyType );
			  }
			  return file;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File relationshipHeader(org.Neo4Net.unsafe.impl.batchimport.input.csv.Configuration config) throws Exception
		 private File RelationshipHeader( Configuration config )
		 {
			  return RelationshipHeader( config, Charset.defaultCharset() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File relationshipHeader(org.Neo4Net.unsafe.impl.batchimport.input.csv.Configuration config, java.nio.charset.Charset encoding) throws Exception
		 private File RelationshipHeader( Configuration config, Charset encoding )
		 {
			  return RelationshipHeader( config, null, null, true, encoding );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File relationshipHeader(org.Neo4Net.unsafe.impl.batchimport.input.csv.Configuration config, String startIdGroup, String endIdGroup, boolean specifyType) throws Exception
		 private File RelationshipHeader( Configuration config, string startIdGroup, string endIdGroup, bool specifyType )
		 {
			  return RelationshipHeader( config, startIdGroup, endIdGroup, specifyType, Charset.defaultCharset() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File relationshipHeader(org.Neo4Net.unsafe.impl.batchimport.input.csv.Configuration config, String startIdGroup, String endIdGroup, boolean specifyType, java.nio.charset.Charset encoding) throws Exception
		 private File RelationshipHeader( Configuration config, string startIdGroup, string endIdGroup, bool specifyType, Charset encoding )
		 {
			  File file = file( FileName( "relationships-header.csv" ) );
			  using ( PrintStream writer = writer( file, encoding ) )
			  {
					WriteRelationshipHeader( writer, config, startIdGroup, endIdGroup, specifyType );
			  }
			  return file;
		 }

		 private string FileName( string name )
		 {
			  return _dataIndex++ + "-" + name;
		 }

		 private File File( string localname )
		 {
			  return DbRule.databaseLayout().file(localname);
		 }

		 private File BadFile()
		 {
			  return DbRule.databaseLayout().file(BAD_FILE_NAME);
		 }

		 private void WriteRelationshipHeader( PrintStream writer, Configuration config, string startIdGroup, string endIdGroup, bool specifyType )
		 {
			  char delimiter = config.Delimiter();
			  writer.println( IdEntry( null, Type.START_ID, startIdGroup ) + delimiter + IdEntry( null, Type.END_ID, endIdGroup ) + ( specifyType ? ( delimiter + ":" + Type.TYPE ) : "" ) + delimiter + "created:long" + delimiter + "name:String" );
		 }

		 private class RelationshipDataLine
		 {
			  internal readonly string StartNodeId;
			  internal readonly string EndNodeId;
			  internal readonly string Type;
			  internal readonly string Name;

			  internal RelationshipDataLine( string startNodeId, string endNodeId, string type, string name )
			  {
					this.StartNodeId = startNodeId;
					this.EndNodeId = endNodeId;
					this.Type = type;
					this.Name = name;
			  }

			  public override string ToString()
			  {
					return "RelationshipDataLine [startNodeId=" + StartNodeId + ", endNodeId=" + EndNodeId + ", type=" + Type + ", name=" + Name + "]";
			  }
		 }

		 private static RelationshipDataLine Relationship( string startNodeId, string endNodeId, string type )
		 {
			  return Relationship( startNodeId, endNodeId, type, null );
		 }

		 private static RelationshipDataLine Relationship( string startNodeId, string endNodeId, string type, string name )
		 {
			  return new RelationshipDataLine( startNodeId, endNodeId, type, name );
		 }

		 private void WriteRelationshipData( PrintStream writer, Configuration config, IEnumerator<RelationshipDataLine> data, System.Func<int, bool> linePredicate, bool specifyType )
		 {
			  char delimiter = config.Delimiter();
			  for ( int i = 0; i < RELATIONSHIP_COUNT; i++ )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					if ( !data.hasNext() )
					{
						 break;
					}
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					RelationshipDataLine entry = data.next();
					if ( linePredicate( i ) )
					{
						 writer.println( NullSafeString( entry.StartNodeId ) + delimiter + NullSafeString( entry.EndNodeId ) + ( specifyType ? ( delimiter + NullSafeString( entry.Type ) ) : "" ) + delimiter + currentTimeMillis() + delimiter + (!string.ReferenceEquals(entry.Name, null) ? entry.Name : "") );
					}
			  }
		 }

		 private static string NullSafeString( string endNodeId )
		 {
			  return !string.ReferenceEquals( endNodeId, null ) ? endNodeId : "";
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private java.util.Iterator<RelationshipDataLine> randomRelationships(final java.util.List<String> nodeIds)
		 private IEnumerator<RelationshipDataLine> RandomRelationships( IList<string> nodeIds )
		 {
			  return new PrefetchingIteratorAnonymousInnerClass( this, nodeIds );
		 }

		 private class PrefetchingIteratorAnonymousInnerClass : PrefetchingIterator<RelationshipDataLine>
		 {
			 private readonly ImportToolTest _outerInstance;

			 private IList<string> _nodeIds;

			 public PrefetchingIteratorAnonymousInnerClass( ImportToolTest outerInstance, IList<string> nodeIds )
			 {
				 this.outerInstance = outerInstance;
				 this._nodeIds = nodeIds;
			 }

			 protected internal override RelationshipDataLine fetchNextOrNull()
			 {
				  return new RelationshipDataLine( _nodeIds[_outerInstance.random.Next( _nodeIds.Count )], _nodeIds[_outerInstance.random.Next( _nodeIds.Count )], _outerInstance.randomType(), null );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static void assertExceptionContains(Exception e, String message, Class type) throws Exception
		 internal static void AssertExceptionContains( Exception e, string message, Type type )
		 {
			  if ( !contains( e, message, type ) )
			  { // Rethrow the exception since we'd like to see what it was instead
					throw withMessage( e, format( "Expected exception to contain cause '%s', %s. but was %s", message, type, e ) );
			  }
		 }

		 private string RandomType()
		 {
			  return "TYPE_" + Random.Next( 4 );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private System.Func<int, boolean> lines(final int startingAt, final int endingAt)
		 private System.Func<int, bool> Lines( int startingAt, int endingAt )
		 {
			  return line => line >= startingAt && line < endingAt;
		 }

		 private static string EscapePath( string path )
		 {
			  return path.replaceAll( " ", "\\\\ " );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static void importTool(String... arguments) throws java.io.IOException
		 internal static void ImportTool( params string[] arguments )
		 {
			  ImportTool.Main( arguments, true );
		 }
	}

}