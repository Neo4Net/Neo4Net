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
namespace Neo4Net.@unsafe.Impl.Batchimport.input.csv
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;
	using Parameter = org.junit.runners.Parameterized.Parameter;
	using Parameters = org.junit.runners.Parameterized.Parameters;


	using Neo4Net.Collections;
	using CharReadable = Neo4Net.Csv.Reader.CharReadable;
	using CharSeeker = Neo4Net.Csv.Reader.CharSeeker;
	using Neo4Net.Csv.Reader;
	using Extractors = Neo4Net.Csv.Reader.Extractors;
	using Readables = Neo4Net.Csv.Reader.Readables;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using Iterators = Neo4Net.Helpers.Collections.Iterators;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using DateTimeValue = Neo4Net.Values.Storable.DateTimeValue;
	using DateValue = Neo4Net.Values.Storable.DateValue;
	using DurationValue = Neo4Net.Values.Storable.DurationValue;
	using LocalDateTimeValue = Neo4Net.Values.Storable.LocalDateTimeValue;
	using LocalTimeValue = Neo4Net.Values.Storable.LocalTimeValue;
	using TimeValue = Neo4Net.Values.Storable.TimeValue;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.csv.reader.Readables.wrap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.ArrayUtil.union;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.asRawIterator;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.iterator;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.input.Collectors.silentBadCollector;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.input.Group_Fields.GLOBAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.input.InputEntity.NO_PROPERTIES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.input.InputEntityDecorators.NO_DECORATOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.input.InputEntityDecorators.additiveLabels;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.input.InputEntityDecorators.defaultRelationshipType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.input.csv.Configuration.COMMAS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.input.csv.CsvInput.NO_MONITOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.input.csv.DataFactories.datas;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.input.csv.DataFactories.defaultFormatNodeFileHeader;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.input.csv.DataFactories.defaultFormatRelationshipFileHeader;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class CsvInputTest
	public class CsvInputTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameters public static java.util.Collection<bool> data()
		 public static ICollection<bool> Data()
		 {
			  // multi-line fields
			  return asList( true, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.RandomRule random = new org.Neo4Net.test.rule.RandomRule();
		 public readonly RandomRule Random = new RandomRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.TestDirectory directory = org.Neo4Net.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory Directory = TestDirectory.testDirectory();
		 private readonly Extractors _extractors = new Extractors( ',' );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameter public System.Nullable<bool> allowMultilineFields;
		 public bool? AllowMultilineFields;

		 private readonly InputEntity _visitor = new InputEntity();
		 private readonly Groups _groups = new Groups();
		 private InputChunk _chunk;
		 private InputIterator _referenceData;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProvideNodesFromCsvInput() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldProvideNodesFromCsvInput()
		 {
			  // GIVEN
			  IdType idType = IdType.Actual;
			  IEnumerable<DataFactory> data = DataIterable( data( "123,Mattias Persson,HACKER" ) );
			  Input input = new CsvInput( data, Header( Entry( null, Type.Id, idType.extractor( _extractors ) ), Entry( "name", Type.Property, _extractors.@string() ), Entry("labels", Type.Label, _extractors.@string()) ), datas(), defaultFormatRelationshipFileHeader(), idType, Config(COMMAS), silentBadCollector(0), NO_MONITOR );

			  // WHEN/THEN
			  using ( InputIterator nodes = input.Nodes().GetEnumerator() )
			  {
					AssertNextNode( nodes, 123L, Properties( "name", "Mattias Persson" ), Labels( "HACKER" ) );
					assertFalse( _chunk.next( _visitor ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProvideRelationshipsFromCsvInput() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldProvideRelationshipsFromCsvInput()
		 {
			  // GIVEN
			  IdType idType = IdType.String;
			  IEnumerable<DataFactory> data = DataIterable( data( "node1,node2,KNOWS,1234567\n" + "node2,node10,HACKS,987654" ) );
			  Input input = new CsvInput( datas(), defaultFormatNodeFileHeader(), data, Header(Entry("from", Type.StartId, idType.extractor(_extractors)), Entry("to", Type.EndId, idType.extractor(_extractors)), Entry("type", Type.Type, _extractors.@string()), Entry("since", Type.Property, _extractors.long_())), idType, Config(COMMAS), silentBadCollector(0), NO_MONITOR );

			  // WHEN/THEN
			  using ( InputIterator relationships = input.Relationships().GetEnumerator() )
			  {
					AssertNextRelationship( relationships, "node1", "node2", "KNOWS", Properties( "since", 1234567L ) );
					AssertNextRelationship( relationships, "node2", "node10", "HACKS", Properties( "since", 987654L ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseDataIteratorsInTheEnd() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCloseDataIteratorsInTheEnd()
		 {
			  // GIVEN
			  CapturingDataFactories nodeData = new CapturingDataFactories( config => CharReader( "1" ), NO_DECORATOR );
			  CapturingDataFactories relationshipData = new CapturingDataFactories( config => CharReader( "1,1" ), defaultRelationshipType( "TYPE" ) );

			  IdType idType = IdType.String;
			  Input input = new CsvInput( nodeData, Header( Entry( null, Type.Id, idType.extractor( _extractors ) ) ), relationshipData, Header( Entry( null, Type.StartId, idType.extractor( _extractors ) ), Entry( null, Type.EndId, idType.extractor( _extractors ) ) ), idType, Config( COMMAS ), silentBadCollector( 0 ), NO_MONITOR );

			  // WHEN
			  using ( InputIterator iterator = input.Nodes().GetEnumerator() )
			  {
					ReadNext( iterator );
			  }
			  using ( InputIterator iterator = input.Relationships().GetEnumerator() )
			  {
					ReadNext( iterator );
			  }

			  // THEN
			  AssertClosed( nodeData.Last() );
			  AssertClosed( relationshipData.Last() );
		 }

		 private void AssertClosed( CharReadable reader )
		 {
			  try
			  {
					reader.Read( new char[1], 0, 1 );
					fail( reader + " not closed" );
			  }
			  catch ( IOException e )
			  {
					assertTrue( e.Message.contains( "closed" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCopeWithLinesThatHasTooFewValuesButStillValidates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCopeWithLinesThatHasTooFewValuesButStillValidates()
		 {
			  // GIVEN
			  IEnumerable<DataFactory> data = DataIterable( data( "1,ultralisk,ZERG,10\n" + "2,corruptor,ZERG\n" + "3,mutalisk,ZERG,3" ) );
			  Input input = new CsvInput( data, Header( Entry( null, Type.Id, _extractors.long_() ), Entry("unit", Type.Property, _extractors.@string()), Entry("type", Type.Label, _extractors.@string()), Entry("kills", Type.Property, _extractors.int_()) ), datas(), defaultFormatRelationshipFileHeader(), IdType.Actual, Config(COMMAS), silentBadCollector(0), NO_MONITOR );

			  // WHEN
			  using ( InputIterator nodes = input.Nodes().GetEnumerator() )
			  {
					// THEN
					AssertNextNode( nodes, 1L, new object[] { "unit", "ultralisk", "kills", 10 }, Labels( "ZERG" ) );
					AssertNextNode( nodes, 2L, new object[] { "unit", "corruptor" }, Labels( "ZERG" ) );
					AssertNextNode( nodes, 3L, new object[] { "unit", "mutalisk", "kills", 3 }, Labels( "ZERG" ) );
					assertFalse( ReadNext( nodes ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreValuesAfterHeaderEntries() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIgnoreValuesAfterHeaderEntries()
		 {
			  // GIVEN
			  IEnumerable<DataFactory> data = DataIterable( data( "1,zergling,bubble,bobble\n" + "2,scv,pun,intended" ) );
			  Input input = new CsvInput( data, Header( Entry( null, Type.Id, _extractors.long_() ), Entry("name", Type.Property, _extractors.@string()) ), datas(), defaultFormatRelationshipFileHeader(), IdType.Actual, Config(COMMAS), silentBadCollector(4), NO_MONITOR );

			  // WHEN
			  using ( InputIterator nodes = input.Nodes().GetEnumerator() )
			  {
					// THEN
					AssertNextNode( nodes, 1L, new object[] { "name", "zergling" }, Labels() );
					AssertNextNode( nodes, 2L, new object[] { "name", "scv" }, Labels() );
					assertFalse( ReadNext( nodes ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleMultipleInputGroups() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleMultipleInputGroups()
		 {
			  // GIVEN multiple input groups, each with their own, specific, header
			  DataFactory group1 = Data( ":ID,name,kills:int,health:int\n" + "1,Jim,10,100\n" + "2,Abathur,0,200\n" );
			  DataFactory group2 = Data( ":ID,type\n" + "3,zergling\n" + "4,csv\n" );
			  IEnumerable<DataFactory> data = DataIterable( group1, group2 );
			  Input input = new CsvInput( data, defaultFormatNodeFileHeader(), datas(), defaultFormatRelationshipFileHeader(), IdType.String, Config(COMMAS), silentBadCollector(0), NO_MONITOR );

			  // WHEN iterating over them, THEN the expected data should come out
			  using ( InputIterator nodes = input.Nodes().GetEnumerator() )
			  {
					AssertNextNode( nodes, "1", Properties( "name", "Jim", "kills", 10, "health", 100 ), Labels() );
					AssertNextNode( nodes, "2", Properties( "name", "Abathur", "kills", 0, "health", 200 ), Labels() );
					AssertNextNode( nodes, "3", Properties( "type", "zergling" ), Labels() );
					AssertNextNode( nodes, "4", Properties( "type", "csv" ), Labels() );
					assertFalse( ReadNext( nodes ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProvideAdditiveLabels() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldProvideAdditiveLabels()
		 {
			  // GIVEN
			  string[] addedLabels = new string[] { "Two", "AddTwo" };
			  DataFactory data = data( ":ID,name,:LABEL\n" + "0,First,\n" + "1,Second,One\n" + "2,Third,One;Two", additiveLabels( addedLabels ) );
			  IEnumerable<DataFactory> dataIterable = dataIterable( data );
			  Input input = new CsvInput( dataIterable, defaultFormatNodeFileHeader(), datas(), defaultFormatRelationshipFileHeader(), IdType.Actual, Config(COMMAS), silentBadCollector(0), NO_MONITOR );

			  // WHEN/THEN
			  using ( InputIterator nodes = input.Nodes().GetEnumerator() )
			  {
					AssertNextNode( nodes, 0L, Properties( "name", "First" ), Labels( addedLabels ) );
					AssertNextNode( nodes, 1L, Properties( "name", "Second" ), Labels( union( new string[] { "One" }, addedLabels ) ) );
					AssertNextNode( nodes, 2L, Properties( "name", "Third" ), Labels( union( new string[] { "One" }, addedLabels ) ) );
					assertFalse( ReadNext( nodes ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProvideDefaultRelationshipType() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldProvideDefaultRelationshipType()
		 {
			  // GIVEN
			  string defaultType = "DEFAULT";
			  string customType = "CUSTOM";
			  DataFactory data = data( ":START_ID,:END_ID,:TYPE\n" + "0,1,\n" + "1,2," + customType + "\n" + "2,1," + defaultType, defaultRelationshipType( defaultType ) );
			  IEnumerable<DataFactory> dataIterable = dataIterable( data );
			  Input input = new CsvInput( datas(), defaultFormatNodeFileHeader(), dataIterable, defaultFormatRelationshipFileHeader(), IdType.Actual, Config(COMMAS), silentBadCollector(0), NO_MONITOR );

			  // WHEN/THEN
			  using ( InputIterator relationships = input.Relationships().GetEnumerator() )
			  {
					AssertNextRelationship( relationships, 0L, 1L, defaultType, NO_PROPERTIES );
					AssertNextRelationship( relationships, 1L, 2L, customType, NO_PROPERTIES );
					AssertNextRelationship( relationships, 2L, 1L, defaultType, NO_PROPERTIES );
					assertFalse( ReadNext( relationships ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowNodesWithoutIdHeader() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowNodesWithoutIdHeader()
		 {
			  // GIVEN
			  DataFactory data = data( "name:string,level:int\n" + "Mattias,1\n" + "Johan,2\n" );
			  IEnumerable<DataFactory> dataIterable = dataIterable( data );
			  Input input = new CsvInput( dataIterable, defaultFormatNodeFileHeader(), datas(), defaultFormatRelationshipFileHeader(), IdType.String, Config(COMMAS), silentBadCollector(0), NO_MONITOR );

			  // WHEN
			  using ( InputIterator nodes = input.Nodes().GetEnumerator() )
			  {
					// THEN
					AssertNextNode( nodes, null, new object[] { "name", "Mattias", "level", 1 }, Labels() );
					AssertNextNode( nodes, null, new object[] { "name", "Johan", "level", 2 }, Labels() );
					assertFalse( ReadNext( nodes ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowSomeNodesToBeAnonymous() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowSomeNodesToBeAnonymous()
		 {
			  // GIVEN
			  DataFactory data = data( ":ID,name:string,level:int\n" + "abc,Mattias,1\n" + ",Johan,2\n" ); // this node is anonymous
			  IEnumerable<DataFactory> dataIterable = dataIterable( data );
			  Input input = new CsvInput( dataIterable, defaultFormatNodeFileHeader(), datas(), defaultFormatRelationshipFileHeader(), IdType.String, Config(COMMAS), silentBadCollector(0), NO_MONITOR );

			  // WHEN
			  using ( InputIterator nodes = input.Nodes().GetEnumerator() )
			  {
					// THEN
					AssertNextNode( nodes, "abc", new object[] { "name", "Mattias", "level", 1 }, Labels() );
					AssertNextNode( nodes, null, new object[] { "name", "Johan", "level", 2 }, Labels() );
					assertFalse( ReadNext( nodes ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowNodesToBeAnonymousEvenIfIdHeaderIsNamed() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowNodesToBeAnonymousEvenIfIdHeaderIsNamed()
		 {
			  // GIVEN
			  DataFactory data = data( "id:ID,name:string,level:int\n" + "abc,Mattias,1\n" + ",Johan,2\n" ); // this node is anonymous
			  IEnumerable<DataFactory> dataIterable = dataIterable( data );
			  Input input = new CsvInput( dataIterable, defaultFormatNodeFileHeader(), datas(), defaultFormatRelationshipFileHeader(), IdType.String, Config(COMMAS), silentBadCollector(0), NO_MONITOR );

			  // WHEN
			  using ( InputIterator nodes = input.Nodes().GetEnumerator() )
			  {
					// THEN
					AssertNextNode( nodes, "abc", new object[] { "id", "abc", "name", "Mattias", "level", 1 }, Labels() );
					AssertNextNode( nodes, null, new object[] { "name", "Johan", "level", 2 }, Labels() );
					assertFalse( ReadNext( nodes ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotHaveIdSetAsPropertyIfIdHeaderEntryIsNamedForActualIds() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotHaveIdSetAsPropertyIfIdHeaderEntryIsNamedForActualIds()
		 {
			  // GIVEN
			  DataFactory data = data( "myId:ID,name:string,level:int\n" + "0,Mattias,1\n" + "1,Johan,2\n" ); // this node is anonymous
			  IEnumerable<DataFactory> dataIterable = dataIterable( data );
			  Input input = new CsvInput( dataIterable, defaultFormatNodeFileHeader(), datas(), defaultFormatRelationshipFileHeader(), IdType.Actual, Config(COMMAS), silentBadCollector(0), NO_MONITOR );

			  // WHEN
			  using ( InputIterator nodes = input.Nodes().GetEnumerator() )
			  {
					// THEN
					AssertNextNode( nodes, 0L, new object[] { "name", "Mattias", "level", 1 }, Labels() );
					AssertNextNode( nodes, 1L, new object[] { "name", "Johan", "level", 2 }, Labels() );
					assertFalse( ReadNext( nodes ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreEmptyPropertyValues() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIgnoreEmptyPropertyValues()
		 {
			  // GIVEN
			  DataFactory data = data( ":ID,name,extra\n" + "0,Mattias,\n" + "1,Johan,Additional\n" );
			  IEnumerable<DataFactory> dataIterable = dataIterable( data );
			  Input input = new CsvInput( dataIterable, defaultFormatNodeFileHeader(), datas(), defaultFormatRelationshipFileHeader(), IdType.Actual, Config(COMMAS), silentBadCollector(0), NO_MONITOR );

			  // WHEN
			  using ( InputIterator nodes = input.Nodes().GetEnumerator() )
			  {
					// THEN
					AssertNextNode( nodes, 0L, new object[] { "name", "Mattias" }, Labels() );
					AssertNextNode( nodes, 1L, new object[] { "name", "Johan", "extra", "Additional" }, Labels() );
					assertFalse( ReadNext( nodes ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreEmptyIntPropertyValues() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIgnoreEmptyIntPropertyValues()
		 {
			  // GIVEN
			  DataFactory data = data( ":ID,name,extra:int\n" + "0,Mattias,\n" + "1,Johan,10\n" );
			  IEnumerable<DataFactory> dataIterable = dataIterable( data );
			  Input input = new CsvInput( dataIterable, defaultFormatNodeFileHeader(), datas(), defaultFormatRelationshipFileHeader(), IdType.Actual, Config(COMMAS), silentBadCollector(0), NO_MONITOR );

			  // WHEN
			  using ( InputIterator nodes = input.Nodes().GetEnumerator() )
			  {
					// THEN
					AssertNextNode( nodes, 0L, new object[] { "name", "Mattias" }, Labels() );
					AssertNextNode( nodes, 1L, new object[] { "name", "Johan", "extra", 10 }, Labels() );
					assertFalse( ReadNext( nodes ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParsePointPropertyValues() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldParsePointPropertyValues()
		 {
			  // GIVEN
			  DataFactory data = data( ":ID,name,point:Point\n" + "0,Mattias,\"{x: 2.7, y:3.2 }\"\n" + "1,Johan,\" { height :0.01 ,longitude:5, latitude : -4.2 } \"\n" );
			  IEnumerable<DataFactory> dataIterable = dataIterable( data );
			  Input input = new CsvInput( dataIterable, defaultFormatNodeFileHeader(), datas(), defaultFormatRelationshipFileHeader(), IdType.Actual, Config(COMMAS), silentBadCollector(0), NO_MONITOR );

			  // WHEN
			  using ( InputIterator nodes = input.Nodes().GetEnumerator() )
			  {
					// THEN
					AssertNextNode( nodes, 0L, new object[]{ "name", "Mattias", "point", Values.pointValue( CoordinateReferenceSystem.Cartesian, 2.7, 3.2 ) }, Labels() );
					AssertNextNode( nodes, 1L, new object[]{ "name", "Johan", "point", Values.pointValue( CoordinateReferenceSystem.WGS84_3D, 5, -4.2, 0.01 ) }, Labels() );
					assertFalse( ReadNext( nodes ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotParsePointPropertyValuesWithDuplicateKeys() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotParsePointPropertyValuesWithDuplicateKeys()
		 {
			  // GIVEN
			  DataFactory data = data( ":ID,name,point:Point\n" + "1,Johan,\" { height :0.01 ,longitude:5, latitude : -4.2, latitude : 4.2 } \"\n" );
			  IEnumerable<DataFactory> dataIterable = dataIterable( data );
			  Input input = new CsvInput( dataIterable, defaultFormatNodeFileHeader(), datas(), defaultFormatRelationshipFileHeader(), IdType.Actual, Config(COMMAS), silentBadCollector(0), NO_MONITOR );

			  // WHEN
			  try
			  {
					  using ( InputIterator nodes = input.Nodes().GetEnumerator() )
					  {
						// THEN
						ReadNext( nodes );
						fail( "Should have failed when key assigned multiple times, but didn't." );
					  }
			  }
			  catch ( InputException )
			  {
					// this is fine
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParsePointPropertyValuesWithCRSInHeader() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldParsePointPropertyValuesWithCRSInHeader()
		 {
			  // GIVEN
			  DataFactory data = data( ":ID,name,point:Point{crs:WGS-84-3D}\n" + "0,Johan,\" { height :0.01 ,longitude:5, latitude : -4.2 } \"\n" );
			  IEnumerable<DataFactory> dataIterable = dataIterable( data );
			  Input input = new CsvInput( dataIterable, defaultFormatNodeFileHeader(), datas(), defaultFormatRelationshipFileHeader(), IdType.Actual, Config(COMMAS), silentBadCollector(0), NO_MONITOR );

			  // WHEN
			  using ( InputIterator nodes = input.Nodes().GetEnumerator() )
			  {
					// THEN
					AssertNextNode( nodes, 0L, new object[]{ "name", "Johan", "point", Values.pointValue( CoordinateReferenceSystem.WGS84_3D, 5, -4.2, 0.01 ) }, Labels() );
					assertFalse( ReadNext( nodes ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUseHeaderInformationToParsePoint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUseHeaderInformationToParsePoint()
		 {
			  // GIVEN
			  DataFactory data = data( ":ID,name,point:Point{crs:WGS-84}\n" + "0,Johan,\" { x :1 ,y:2 } \"\n" );
			  IEnumerable<DataFactory> dataIterable = dataIterable( data );
			  Input input = new CsvInput( dataIterable, defaultFormatNodeFileHeader(), datas(), defaultFormatRelationshipFileHeader(), IdType.Actual, Config(COMMAS), silentBadCollector(0), NO_MONITOR );

			  // WHEN
			  using ( InputIterator nodes = input.Nodes().GetEnumerator() )
			  {
					// THEN
					AssertNextNode( nodes, 0L, new object[]{ "name", "Johan", "point", Values.pointValue( CoordinateReferenceSystem.WGS84, 1, 2 ) }, Labels() );
					assertFalse( ReadNext( nodes ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParseDatePropertyValues() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldParseDatePropertyValues()
		 {
			  // GIVEN
			  DataFactory data = data( ":ID,name,date:Date\n" + "0,Mattias,2018-02-27\n" + "1,Johan,2018-03-01\n" );
			  IEnumerable<DataFactory> dataIterable = dataIterable( data );
			  Input input = new CsvInput( dataIterable, defaultFormatNodeFileHeader(), datas(), defaultFormatRelationshipFileHeader(), IdType.Actual, Config(COMMAS), silentBadCollector(0), NO_MONITOR );

			  // WHEN
			  using ( InputIterator nodes = input.Nodes().GetEnumerator() )
			  {
					// THEN
					AssertNextNode( nodes, 0L, new object[]{ "name", "Mattias", "date", DateValue.date( 2018, 2, 27 ) }, Labels() );
					AssertNextNode( nodes, 1L, new object[]{ "name", "Johan", "date", DateValue.date( 2018, 3, 1 ) }, Labels() );
					assertFalse( ReadNext( nodes ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParseTimePropertyValues() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldParseTimePropertyValues()
		 {
			  // GIVEN
			  DataFactory data = data( ":ID,name,time:Time\n" + "0,Mattias,13:37\n" + "1,Johan,\"16:20:01\"\n" + "2,Bob,07:30-05:00\n" );
			  IEnumerable<DataFactory> dataIterable = dataIterable( data );
			  Input input = new CsvInput( dataIterable, defaultFormatNodeFileHeader(), datas(), defaultFormatRelationshipFileHeader(), IdType.Actual, Config(COMMAS), silentBadCollector(0), NO_MONITOR );

			  // WHEN
			  using ( InputIterator nodes = input.Nodes().GetEnumerator() )
			  {
					// THEN
					AssertNextNode( nodes, 0L, new object[]{ "name", "Mattias", "time", TimeValue.time( 13, 37, 0, 0, "+00:00" ) }, Labels() );
					AssertNextNode( nodes, 1L, new object[]{ "name", "Johan", "time", TimeValue.time( 16, 20, 1, 0, "+00:00" ) }, Labels() );
					AssertNextNode( nodes, 2L, new object[]{ "name", "Bob", "time", TimeValue.time( 7, 30, 0, 0, "-05:00" ) }, Labels() );
					assertFalse( ReadNext( nodes ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParseTimePropertyValuesWithTimezoneInHeader() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldParseTimePropertyValuesWithTimezoneInHeader()
		 {
			  // GIVEN
			  DataFactory data = data( ":ID,name,time:Time{timezone:+02:00}\n" + "0,Mattias,13:37\n" + "1,Johan,\"16:20:01\"\n" + "2,Bob,07:30-05:00\n" );
			  IEnumerable<DataFactory> dataIterable = dataIterable( data );
			  Input input = new CsvInput( dataIterable, defaultFormatNodeFileHeader(), datas(), defaultFormatRelationshipFileHeader(), IdType.Actual, Config(COMMAS), silentBadCollector(0), NO_MONITOR );

			  // WHEN
			  using ( InputIterator nodes = input.Nodes().GetEnumerator() )
			  {
					// THEN
					AssertNextNode( nodes, 0L, new object[]{ "name", "Mattias", "time", TimeValue.time( 13, 37, 0, 0, "+02:00" ) }, Labels() );
					AssertNextNode( nodes, 1L, new object[]{ "name", "Johan", "time", TimeValue.time( 16, 20, 1, 0, "+02:00" ) }, Labels() );
					AssertNextNode( nodes, 2L, new object[]{ "name", "Bob", "time", TimeValue.time( 7, 30, 0, 0, "-05:00" ) }, Labels() );
					assertFalse( ReadNext( nodes ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParseDateTimePropertyValues() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldParseDateTimePropertyValues()
		 {
			  // GIVEN
			  DataFactory data = data( ":ID,name,time:DateTime\n" + "0,Mattias,2018-02-27T13:37\n" + "1,Johan,\"2018-03-01T16:20:01\"\n" + "2,Bob,1981-05-11T07:30-05:00\n" );

			  IEnumerable<DataFactory> dataIterable = dataIterable( data );
			  Input input = new CsvInput( dataIterable, defaultFormatNodeFileHeader(), datas(), defaultFormatRelationshipFileHeader(), IdType.Actual, Config(COMMAS), silentBadCollector(0), NO_MONITOR );

			  // WHEN
			  using ( InputIterator nodes = input.Nodes().GetEnumerator() )
			  {
					// THEN
					AssertNextNode( nodes, 0L, new object[]{ "name", "Mattias", "time", DateTimeValue.datetime( 2018, 2, 27, 13, 37, 0, 0, "+00:00" ) }, Labels() );
					AssertNextNode( nodes, 1L, new object[]{ "name", "Johan", "time", DateTimeValue.datetime( 2018, 3, 1, 16, 20, 1, 0, "+00:00" ) }, Labels() );
					AssertNextNode( nodes, 2L, new object[]{ "name", "Bob", "time", DateTimeValue.datetime( 1981, 5, 11, 7, 30, 0, 0, "-05:00" ) }, Labels() );
					assertFalse( ReadNext( nodes ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParseDateTimePropertyValuesWithTimezoneInHeader() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldParseDateTimePropertyValuesWithTimezoneInHeader()
		 {
			  // GIVEN
			  DataFactory data = data( ":ID,name,time:DateTime{timezone:Europe/Stockholm}\n" + "0,Mattias,2018-02-27T13:37\n" + "1,Johan,\"2018-03-01T16:20:01\"\n" + "2,Bob,1981-05-11T07:30-05:00\n" );

			  IEnumerable<DataFactory> dataIterable = dataIterable( data );
			  Input input = new CsvInput( dataIterable, defaultFormatNodeFileHeader(), datas(), defaultFormatRelationshipFileHeader(), IdType.Actual, Config(COMMAS), silentBadCollector(0), NO_MONITOR );

			  // WHEN
			  using ( InputIterator nodes = input.Nodes().GetEnumerator() )
			  {
					// THEN
					AssertNextNode( nodes, 0L, new object[]{ "name", "Mattias", "time", DateTimeValue.datetime( 2018, 2, 27, 13, 37, 0, 0, "Europe/Stockholm" ) }, Labels() );
					AssertNextNode( nodes, 1L, new object[]{ "name", "Johan", "time", DateTimeValue.datetime( 2018, 3, 1, 16, 20, 1, 0, "Europe/Stockholm" ) }, Labels() );
					AssertNextNode( nodes, 2L, new object[]{ "name", "Bob", "time", DateTimeValue.datetime( 1981, 5, 11, 7, 30, 0, 0, "-05:00" ) }, Labels() );
					assertFalse( ReadNext( nodes ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParseLocalTimePropertyValues() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldParseLocalTimePropertyValues()
		 {
			  // GIVEN
			  DataFactory data = data( ":ID,name,time:LocalTime\n" + "0,Mattias,13:37\n" + "1,Johan,\"16:20:01\"\n" );
			  IEnumerable<DataFactory> dataIterable = dataIterable( data );
			  Input input = new CsvInput( dataIterable, defaultFormatNodeFileHeader(), datas(), defaultFormatRelationshipFileHeader(), IdType.Actual, Config(COMMAS), silentBadCollector(0), NO_MONITOR );

			  // WHEN
			  using ( InputIterator nodes = input.Nodes().GetEnumerator() )
			  {
					// THEN
					AssertNextNode( nodes, 0L, new object[]{ "name", "Mattias", "time", LocalTimeValue.localTime( 13, 37, 0, 0 ) }, Labels() );
					AssertNextNode( nodes, 1L, new object[]{ "name", "Johan", "time", LocalTimeValue.localTime( 16, 20, 1, 0 ) }, Labels() );
					assertFalse( ReadNext( nodes ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParseLocalDateTimePropertyValues() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldParseLocalDateTimePropertyValues()
		 {
			  // GIVEN
			  DataFactory data = data( ":ID,name,time:LocalDateTime\n" + "0,Mattias,2018-02-27T13:37\n" + "1,Johan,\"2018-03-01T16:20:01\"\n" );
			  IEnumerable<DataFactory> dataIterable = dataIterable( data );
			  Input input = new CsvInput( dataIterable, defaultFormatNodeFileHeader(), datas(), defaultFormatRelationshipFileHeader(), IdType.Actual, Config(COMMAS), silentBadCollector(0), NO_MONITOR );

			  // WHEN
			  using ( InputIterator nodes = input.Nodes().GetEnumerator() )
			  {
					// THEN
					AssertNextNode( nodes, 0L, new object[]{ "name", "Mattias", "time", LocalDateTimeValue.localDateTime( 2018, 2, 27, 13, 37, 0, 0 ) }, Labels() );
					AssertNextNode( nodes, 1L, new object[]{ "name", "Johan", "time", LocalDateTimeValue.localDateTime( 2018, 3, 1, 16, 20, 1, 0 ) }, Labels() );
					assertFalse( ReadNext( nodes ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParseDurationPropertyValues() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldParseDurationPropertyValues()
		 {
			  // GIVEN
			  DataFactory data = data( ":ID,name,duration:Duration\n" + "0,Mattias,P3MT13H37M\n" + "1,Johan,\"P-1YT4H20M\"\n" );
			  IEnumerable<DataFactory> dataIterable = dataIterable( data );
			  Input input = new CsvInput( dataIterable, defaultFormatNodeFileHeader(), datas(), defaultFormatRelationshipFileHeader(), IdType.Actual, Config(COMMAS), silentBadCollector(0), NO_MONITOR );

			  // WHEN
			  using ( InputIterator nodes = input.Nodes().GetEnumerator() )
			  {
					// THEN
					AssertNextNode( nodes, 0L, new object[]{ "name", "Mattias", "duration", DurationValue.duration( 3, 0, 13 * 3600 + 37 * 60, 0 ) }, Labels() );
					AssertNextNode( nodes, 1L, new object[]{ "name", "Johan", "duration", DurationValue.duration( -12, 0, 4 * 3600 + 20 * 60, 0 ) }, Labels() );
					assertFalse( ReadNext( nodes ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailOnArrayDelimiterBeingSameAsDelimiter()
		 public virtual void ShouldFailOnArrayDelimiterBeingSameAsDelimiter()
		 {
			  // WHEN
			  try
			  {
					new CsvInput( null, null, null, null, IdType.Actual, CustomConfig( ',', ',', '"' ), silentBadCollector( 0 ), NO_MONITOR );
					fail( "Should not be possible" );
			  }
			  catch ( System.ArgumentException e )
			  {
					// THEN
					assertTrue( e.Message.contains( "array delimiter" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailOnQuotationCharacterBeingSameAsDelimiter()
		 public virtual void ShouldFailOnQuotationCharacterBeingSameAsDelimiter()
		 {
			  // WHEN
			  try
			  {
					new CsvInput( null, null, null, null, IdType.Actual, CustomConfig( ',', ';', ',' ), silentBadCollector( 0 ), NO_MONITOR );
					fail( "Should not be possible" );
			  }
			  catch ( System.ArgumentException e )
			  {
					// THEN
					assertTrue( e.Message.contains( "delimiter" ) );
					assertTrue( e.Message.contains( "quotation" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailOnQuotationCharacterBeingSameAsArrayDelimiter()
		 public virtual void ShouldFailOnQuotationCharacterBeingSameAsArrayDelimiter()
		 {
			  // WHEN
			  try
			  {
					new CsvInput( null, null, null, null, IdType.Actual, CustomConfig( ',', ';', ';' ), silentBadCollector( 0 ), NO_MONITOR );
					fail( "Should not be possible" );
			  }
			  catch ( System.ArgumentException e )
			  {
					// THEN
					assertTrue( e.Message.contains( "array delimiter" ) );
					assertTrue( e.Message.contains( "quotation" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveNodesBelongToGroupSpecifiedInHeader() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHaveNodesBelongToGroupSpecifiedInHeader()
		 {
			  // GIVEN
			  IdType idType = IdType.Integer;
			  IEnumerable<DataFactory> data = DataIterable( data( "123,one\n" + "456,two" ) );
			  Groups groups = new Groups();
			  Group group = groups.GetOrCreate( "MyGroup" );
			  Input input = new CsvInput( data, Header( Entry( null, Type.Id, group.Name(), idType.extractor(_extractors) ), Entry("name", Type.Property, _extractors.@string()) ), datas(), defaultFormatRelationshipFileHeader(), idType, Config(COMMAS), silentBadCollector(0), NO_MONITOR );

			  // WHEN/THEN
			  using ( InputIterator nodes = input.Nodes().GetEnumerator() )
			  {
					AssertNextNode( nodes, group, 123L, Properties( "name", "one" ), Labels() );
					AssertNextNode( nodes, group, 456L, Properties( "name", "two" ), Labels() );
					assertFalse( ReadNext( nodes ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveRelationshipsSpecifyStartEndNodeIdGroupsInHeader() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHaveRelationshipsSpecifyStartEndNodeIdGroupsInHeader()
		 {
			  // GIVEN
			  IdType idType = IdType.Integer;
			  IEnumerable<DataFactory> data = DataIterable( data( "123,TYPE,234\n" + "345,TYPE,456" ) );
			  Groups groups = new Groups();
			  Group startNodeGroup = groups.GetOrCreate( "StartGroup" );
			  Group endNodeGroup = groups.GetOrCreate( "EndGroup" );
			  IEnumerable<DataFactory> nodeHeader = DataIterable( data( ":ID(" + startNodeGroup.Name() + ")" ), data(":ID(" + endNodeGroup.Name() + ")") );
			  Input input = new CsvInput( nodeHeader, defaultFormatNodeFileHeader(), data, Header(Entry(null, Type.StartId, startNodeGroup.Name(), idType.extractor(_extractors)), Entry(null, Type.Type, _extractors.@string()), Entry(null, Type.EndId, endNodeGroup.Name(), idType.extractor(_extractors))), idType, Config(COMMAS), silentBadCollector(0), NO_MONITOR );

			  // WHEN/THEN
			  using ( InputIterator relationships = input.Relationships().GetEnumerator() )
			  {
					AssertRelationship( relationships, startNodeGroup, 123L, endNodeGroup, 234L, "TYPE", Properties() );
					AssertRelationship( relationships, startNodeGroup, 345L, endNodeGroup, 456L, "TYPE", Properties() );
					assertFalse( ReadNext( relationships ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDoWithoutRelationshipTypeHeaderIfDefaultSupplied() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDoWithoutRelationshipTypeHeaderIfDefaultSupplied()
		 {
			  // GIVEN relationship data w/o :TYPE header
			  string defaultType = "HERE";
			  DataFactory data = data( ":START_ID,:END_ID,name\n" + "0,1,First\n" + "2,3,Second\n", defaultRelationshipType( defaultType ) );
			  IEnumerable<DataFactory> dataIterable = dataIterable( data );
			  Input input = new CsvInput( datas(), defaultFormatNodeFileHeader(), dataIterable, defaultFormatRelationshipFileHeader(), IdType.Actual, Config(COMMAS), silentBadCollector(0), NO_MONITOR );

			  // WHEN
			  using ( InputIterator relationships = input.Relationships().GetEnumerator() )
			  {
					// THEN
					AssertNextRelationship( relationships, 0L, 1L, defaultType, Properties( "name", "First" ) );
					AssertNextRelationship( relationships, 2L, 3L, defaultType, Properties( "name", "Second" ) );
					assertFalse( ReadNext( relationships ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreNodeEntriesMarkedIgnoreUsingHeader() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIgnoreNodeEntriesMarkedIgnoreUsingHeader()
		 {
			  // GIVEN
			  IEnumerable<DataFactory> data = DataFactories.Datas( CsvInputTest.Data( ":ID,name:IGNORE,other:int,:LABEL\n" + "1,Mattias,10,Person\n" + "2,Johan,111,Person\n" + "3,Emil,12,Person" ) );
			  Input input = new CsvInput( data, defaultFormatNodeFileHeader(), datas(), defaultFormatNodeFileHeader(), IdType.Integer, Config(COMMAS), silentBadCollector(0), NO_MONITOR );

			  // WHEN
			  using ( InputIterator nodes = input.Nodes().GetEnumerator() )
			  {
					AssertNextNode( nodes, 1L, new object[] { "other", 10 }, Labels( "Person" ) );
					AssertNextNode( nodes, 2L, new object[] { "other", 111 }, Labels( "Person" ) );
					AssertNextNode( nodes, 3L, new object[] { "other", 12 }, Labels( "Person" ) );
					assertFalse( ReadNext( nodes ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreRelationshipEntriesMarkedIgnoreUsingHeader() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIgnoreRelationshipEntriesMarkedIgnoreUsingHeader()
		 {
			  // GIVEN
			  IEnumerable<DataFactory> data = DataFactories.Datas( CsvInputTest.Data( ":START_ID,:TYPE,:END_ID,prop:IGNORE,other:int\n" + "1,KNOWS,2,Mattias,10\n" + "2,KNOWS,3,Johan,111\n" + "3,KNOWS,4,Emil,12" ) );
			  Input input = new CsvInput( datas(), defaultFormatNodeFileHeader(), data, defaultFormatRelationshipFileHeader(), IdType.Integer, Config(COMMAS), silentBadCollector(0), NO_MONITOR );

			  // WHEN
			  using ( InputIterator relationships = input.Relationships().GetEnumerator() )
			  {
					AssertNextRelationship( relationships, 1L, 2L, "KNOWS", new object[] { "other", 10 } );
					AssertNextRelationship( relationships, 2L, 3L, "KNOWS", new object[] { "other", 111 } );
					AssertNextRelationship( relationships, 3L, 4L, "KNOWS", new object[] { "other", 12 } );
					assertFalse( ReadNext( relationships ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPropagateExceptionFromFailingDecorator() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPropagateExceptionFromFailingDecorator()
		 {
			  // GIVEN
			  Exception failure = new Exception( "FAILURE" );
			  IEnumerable<DataFactory> data = DataFactories.Datas( CsvInputTest.data( ":ID,name\n1,Mattias", new FailingNodeDecorator( failure ) ) );
			  Input input = new CsvInput( data, defaultFormatNodeFileHeader(), datas(), defaultFormatNodeFileHeader(), IdType.Integer, Config(COMMAS), silentBadCollector(0), NO_MONITOR );

			  // WHEN
			  try
			  {
					  using ( InputIterator nodes = input.Nodes().GetEnumerator() )
					  {
						ReadNext( nodes );
					  }
			  }
			  catch ( InputException e )
			  {
					// THEN
					assertSame( e.InnerException, failure );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotIncludeEmptyArraysInEntities() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotIncludeEmptyArraysInEntities()
		 {
			  // GIVEN
			  IEnumerable<DataFactory> data = DataFactories.Datas( CsvInputTest.Data( ":ID,sprop:String[],lprop:long[]\n" + "1,,\n" + "2,a;b,10;20" ) );
			  Input input = new CsvInput( data, defaultFormatNodeFileHeader(), datas(), defaultFormatNodeFileHeader(), IdType.Integer, Config(COMMAS), silentBadCollector(0), NO_MONITOR );

			  // WHEN/THEN
			  using ( InputIterator nodes = input.Nodes().GetEnumerator() )
			  {
					AssertNextNode( nodes, 1L, NO_PROPERTIES, Labels() );
					AssertNextNode( nodes, 2L, Properties( "sprop", new string[] { "a", "b" }, "lprop", new long[] { 10, 20 } ), Labels() );
					assertFalse( ReadNext( nodes ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTreatEmptyQuotedStringsAsNullIfConfiguredTo() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTreatEmptyQuotedStringsAsNullIfConfiguredTo()
		 {
			  // GIVEN
			  IEnumerable<DataFactory> data = DataFactories.Datas( CsvInputTest.Data( ":ID,one,two,three\n" + "1,\"\",,value" ) );
			  Configuration config = config( new Configuration_OverriddenAnonymousInnerClass( this, COMMAS ) );
			  Input input = new CsvInput( data, defaultFormatNodeFileHeader(), datas(), defaultFormatRelationshipFileHeader(), IdType.Integer, config, silentBadCollector(0), NO_MONITOR );

			  // WHEN
			  using ( InputIterator nodes = input.Nodes().GetEnumerator() )
			  {
					// THEN
					AssertNextNode( nodes, 1L, Properties( "three", "value" ), Labels() );
					assertFalse( ReadNext( nodes ) );
			  }
		 }

		 private class Configuration_OverriddenAnonymousInnerClass : Configuration_Overridden
		 {
			 private readonly CsvInputTest _outerInstance;

			 public Configuration_OverriddenAnonymousInnerClass( CsvInputTest outerInstance, UnknownType commas ) : base( commas )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override bool emptyQuotedStringsAsNull()
			 {
				  return true;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreEmptyExtraColumns() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIgnoreEmptyExtraColumns()
		 {
			  // GIVEN
			  IEnumerable<DataFactory> data = DataFactories.Datas( CsvInputTest.Data( ":ID,one\n" + "1,test,\n" + "2,test,,additional" ) );

			  // WHEN
			  Collector collector = mock( typeof( Collector ) );
			  Input input = new CsvInput( data, defaultFormatNodeFileHeader(), datas(), defaultFormatRelationshipFileHeader(), IdType.Integer, Config(COMMAS), collector, NO_MONITOR );

			  // THEN
			  using ( InputIterator nodes = input.Nodes().GetEnumerator() )
			  {
					// THEN
					AssertNextNode( nodes, 1L, Properties( "one", "test" ), Labels() );
					AssertNextNode( nodes, 2L, Properties( "one", "test" ), Labels() );
					assertFalse( ReadNext( nodes ) );
			  }
			  verify( collector, times( 1 ) ).collectExtraColumns( anyString(), eq(1L), eq(null) );
			  verify( collector, times( 1 ) ).collectExtraColumns( anyString(), eq(2L), eq(null) );
			  verify( collector, times( 1 ) ).collectExtraColumns( anyString(), eq(2L), eq("additional") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSkipRelationshipValidationIfToldTo() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSkipRelationshipValidationIfToldTo()
		 {
		  // GIVEN
			  IEnumerable<DataFactory> data = datas( CsvInputTest.Data( ":START_ID,:END_ID,:TYPE\n" + ",," ) );
			  Input input = new CsvInput( datas(), defaultFormatNodeFileHeader(), data, defaultFormatRelationshipFileHeader(), IdType.Integer, Config(COMMAS), silentBadCollector(0), NO_MONITOR );

			  // WHEN
			  using ( InputIterator relationships = input.Relationships().GetEnumerator() )
			  {
					ReadNext( relationships );
					assertNull( _visitor.startId() );
					assertNull( _visitor.endId() );
					assertNull( _visitor.stringType );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailOnUnparsableNodeHeader()
		 public virtual void ShouldFailOnUnparsableNodeHeader()
		 {
			  // given
			  IEnumerable<DataFactory> data = datas( data( ":SOMETHING,abcde#rtg:123," ) );

			  try
			  {
					// when
					new CsvInput( data, defaultFormatNodeFileHeader(), datas(), defaultFormatRelationshipFileHeader(), IdType.Integer, COMMAS, mock(typeof(Collector)), NO_MONITOR );
					fail( "Should not parse" );
			  }
			  catch ( InputException )
			  {
					// then
					// OK
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailOnUnparsableRelationshipHeader()
		 public virtual void ShouldFailOnUnparsableRelationshipHeader()
		 {
			  // given
			  IEnumerable<DataFactory> data = datas( data( ":SOMETHING,abcde#rtg:123," ) );

			  try
			  {
					// when
					new CsvInput( datas(), defaultFormatNodeFileHeader(), data, defaultFormatRelationshipFileHeader(), IdType.Integer, COMMAS, mock(typeof(Collector)), NO_MONITOR );
					fail( "Should not parse" );
			  }
			  catch ( InputException )
			  {
					// then
					// OK
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailOnUndefinedGroupInRelationshipHeader()
		 public virtual void ShouldFailOnUndefinedGroupInRelationshipHeader()
		 {
			  // given
			  IEnumerable<DataFactory> nodeData = datas( Data( ":ID(left)" ), Data( ":ID(right)" ) );
			  IEnumerable<DataFactory> relationshipData = datas( Data( ":START_ID(left),:END_ID(rite)" ) );

			  try
			  {
					// when
					new CsvInput( nodeData, defaultFormatNodeFileHeader(), relationshipData, defaultFormatRelationshipFileHeader(), IdType.Integer, COMMAS, mock(typeof(Collector)), NO_MONITOR );
					fail( "Should not validate" );
			  }
			  catch ( InputException )
			  {
					// then
					// OK
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailOnGlobalGroupInRelationshipHeaderIfNoGLobalGroupInNodeHeader()
		 public virtual void ShouldFailOnGlobalGroupInRelationshipHeaderIfNoGLobalGroupInNodeHeader()
		 {
			  // given
			  IEnumerable<DataFactory> nodeData = datas( Data( ":ID(left)" ), Data( ":ID(right)" ) );
			  IEnumerable<DataFactory> relationshipData = datas( Data( ":START_ID(left),:END_ID(rite)" ) );

			  try
			  {
					// when
					new CsvInput( nodeData, defaultFormatNodeFileHeader(), relationshipData, defaultFormatRelationshipFileHeader(), IdType.Integer, COMMAS, mock(typeof(Collector)), NO_MONITOR );
					fail( "Should not validate" );
			  }
			  catch ( InputException )
			  {
					// then
					// OK
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportDuplicateNodeSourceFiles()
		 public virtual void ShouldReportDuplicateNodeSourceFiles()
		 {
			  // given
			  string sourceDescription = "The single data source";
			  System.Func<CharReadable> source = () => wrap(DataWithSourceDescription(":ID", sourceDescription), 3);
			  IEnumerable<DataFactory> data = datas( config => new DataAnonymousInnerClass( this, source ) );
			  CsvInput.Monitor monitor = mock( typeof( CsvInput.Monitor ) );

			  // when
			  new CsvInput( data, defaultFormatNodeFileHeader(), datas(), defaultFormatRelationshipFileHeader(), IdType.Integer, COMMAS, mock(typeof(Collector)), monitor );

			  // then
			  verify( monitor ).duplicateSourceFile( sourceDescription );
		 }

		 private class DataAnonymousInnerClass : Data
		 {
			 private readonly CsvInputTest _outerInstance;

			 private System.Func<CharReadable> _source;

			 public DataAnonymousInnerClass( CsvInputTest outerInstance, System.Func<CharReadable> source )
			 {
				 this.outerInstance = outerInstance;
				 this._source = source;
			 }

			 public RawIterator<CharReadable, IOException> stream()
			 {
				  // Contains two of the same file
				  return asRawIterator( iterator( _source(), _source() ) );
			 }

			 public Decorator decorator()
			 {
				  return NO_DECORATOR;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportDuplicateRelationshipSourceFiles()
		 public virtual void ShouldReportDuplicateRelationshipSourceFiles()
		 {
			  // given
			  string sourceDescription = "The single data source";
			  System.Func<CharReadable> source = () => wrap(DataWithSourceDescription(":START_ID,:END_ID,:TYPE", sourceDescription), 3);
			  IEnumerable<DataFactory> data = datas( config => new DataAnonymousInnerClass2( this, source ) );
			  CsvInput.Monitor monitor = mock( typeof( CsvInput.Monitor ) );

			  // when
			  new CsvInput( datas(), defaultFormatNodeFileHeader(), data, defaultFormatRelationshipFileHeader(), IdType.Integer, COMMAS, mock(typeof(Collector)), monitor );

			  // then
			  verify( monitor ).duplicateSourceFile( sourceDescription );
		 }

		 private class DataAnonymousInnerClass2 : Data
		 {
			 private readonly CsvInputTest _outerInstance;

			 private System.Func<CharReadable> _source;

			 public DataAnonymousInnerClass2( CsvInputTest outerInstance, System.Func<CharReadable> source )
			 {
				 this.outerInstance = outerInstance;
				 this._source = source;
			 }

			 public RawIterator<CharReadable, IOException> stream()
			 {
				  // Contains two of the same file
				  return asRawIterator( iterator( _source(), _source() ) );
			 }

			 public Decorator decorator()
			 {
				  return NO_DECORATOR;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportDuplicateSourceFileUsedAsBothNodeAndRelationshipSourceFile()
		 public virtual void ShouldReportDuplicateSourceFileUsedAsBothNodeAndRelationshipSourceFile()
		 {
			  // given
			  string sourceDescription = "The single data source";
			  System.Func<CharReadable> nodeHeaderSource = () => wrap(DataWithSourceDescription(":ID", "node source"), 3);
			  System.Func<CharReadable> relationshipHeaderSource = () => wrap(DataWithSourceDescription(":START_ID,:END_ID,:TYPE", "relationship source"), 10);
			  System.Func<CharReadable> source = () => wrap(DataWithSourceDescription("1,2,3", sourceDescription), 6);
			  IEnumerable<DataFactory> nodeData = datas( config => new DataAnonymousInnerClass3( this, nodeHeaderSource, source ) );
			  IEnumerable<DataFactory> relationshipData = datas( config => new DataAnonymousInnerClass4( this, relationshipHeaderSource, source ) );
			  CsvInput.Monitor monitor = mock( typeof( CsvInput.Monitor ) );

			  // when
			  new CsvInput( nodeData, defaultFormatNodeFileHeader(), relationshipData, defaultFormatRelationshipFileHeader(), IdType.Integer, COMMAS, mock(typeof(Collector)), monitor );

			  // then
			  verify( monitor ).duplicateSourceFile( sourceDescription );
		 }

		 private class DataAnonymousInnerClass3 : Data
		 {
			 private readonly CsvInputTest _outerInstance;

			 private System.Func<CharReadable> _nodeHeaderSource;
			 private System.Func<CharReadable> _source;

			 public DataAnonymousInnerClass3( CsvInputTest outerInstance, System.Func<CharReadable> nodeHeaderSource, System.Func<CharReadable> source )
			 {
				 this.outerInstance = outerInstance;
				 this._nodeHeaderSource = nodeHeaderSource;
				 this._source = source;
			 }

			 public RawIterator<CharReadable, IOException> stream()
			 {
				  return asRawIterator( iterator( _nodeHeaderSource(), _source() ) );
			 }

			 public Decorator decorator()
			 {
				  return NO_DECORATOR;
			 }
		 }

		 private class DataAnonymousInnerClass4 : Data
		 {
			 private readonly CsvInputTest _outerInstance;

			 private System.Func<CharReadable> _relationshipHeaderSource;
			 private System.Func<CharReadable> _source;

			 public DataAnonymousInnerClass4( CsvInputTest outerInstance, System.Func<CharReadable> relationshipHeaderSource, System.Func<CharReadable> source )
			 {
				 this.outerInstance = outerInstance;
				 this._relationshipHeaderSource = relationshipHeaderSource;
				 this._source = source;
			 }

			 public RawIterator<CharReadable, IOException> stream()
			 {
				  return asRawIterator( iterator( _relationshipHeaderSource(), _source() ) );
			 }

			 public Decorator decorator()
			 {
				  return NO_DECORATOR;
			 }
		 }

		 private static Reader DataWithSourceDescription( string data, string sourceDescription )
		 {
			  return new StringReaderAnonymousInnerClass( data, sourceDescription );
		 }

		 private class StringReaderAnonymousInnerClass : StringReader
		 {
			 private string _sourceDescription;

			 public StringReaderAnonymousInnerClass( string data, string sourceDescription ) : base( data )
			 {
				 this._sourceDescription = sourceDescription;
			 }

			 public override string ToString()
			 {
				  return _sourceDescription;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private Configuration customConfig(final char delimiter, final char arrayDelimiter, final char quote)
		 private Configuration CustomConfig( char delimiter, char arrayDelimiter, char quote )
		 {
			  return Config( new Configuration_DefaultAnonymousInnerClass( this, delimiter, arrayDelimiter, quote ) );
		 }

		 private class Configuration_DefaultAnonymousInnerClass : Configuration_Default
		 {
			 private readonly CsvInputTest _outerInstance;

			 private char _delimiter;
			 private char _arrayDelimiter;
			 private char _quote;

			 public Configuration_DefaultAnonymousInnerClass( CsvInputTest outerInstance, char delimiter, char arrayDelimiter, char quote )
			 {
				 this.outerInstance = outerInstance;
				 this._delimiter = delimiter;
				 this._arrayDelimiter = arrayDelimiter;
				 this._quote = quote;
			 }

			 public override char quotationCharacter()
			 {
				  return _quote;
			 }

			 public override char delimiter()
			 {
				  return _delimiter;
			 }

			 public override char arrayDelimiter()
			 {
				  return _arrayDelimiter;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private DataFactory given(final org.Neo4Net.csv.reader.CharReadable data)
		 private DataFactory Given( CharReadable data )
		 {
			  return config => DataItem( data, InputEntityDecorators.NO_DECORATOR );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private DataFactory data(final org.Neo4Net.csv.reader.CharReadable data, final Decorator decorator)
		 private DataFactory Data( CharReadable data, Decorator decorator )
		 {
			  return config => DataItem( data, decorator );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static Data dataItem(final org.Neo4Net.csv.reader.CharReadable data, final Decorator decorator)
		 private static Data DataItem( CharReadable data, Decorator decorator )
		 {
			  return DataFactories.Data( decorator, () => data ).create(COMMAS);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertNextRelationship(org.Neo4Net.unsafe.impl.batchimport.InputIterator relationship, Object startNode, Object endNode, String type, Object[] properties) throws java.io.IOException
		 private void AssertNextRelationship( InputIterator relationship, object startNode, object endNode, string type, object[] properties )
		 {
			  AssertRelationship( relationship, GLOBAL, startNode, GLOBAL, endNode, type, properties );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertRelationship(org.Neo4Net.unsafe.impl.batchimport.InputIterator data, org.Neo4Net.unsafe.impl.batchimport.input.Group startNodeGroup, Object startNode, org.Neo4Net.unsafe.impl.batchimport.input.Group endNodeGroup, Object endNode, String type, Object[] properties) throws java.io.IOException
		 private void AssertRelationship( InputIterator data, Group startNodeGroup, object startNode, Group endNodeGroup, object endNode, string type, object[] properties )
		 {
			  assertTrue( ReadNext( data ) );
			  assertEquals( startNodeGroup, _visitor.startIdGroup );
			  assertEquals( startNode, _visitor.startId() );
			  assertEquals( endNodeGroup, _visitor.endIdGroup );
			  assertEquals( endNode, _visitor.endId() );
			  assertEquals( type, _visitor.stringType );
			  assertArrayEquals( properties, _visitor.properties() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertNextNode(org.Neo4Net.unsafe.impl.batchimport.InputIterator data, Object id, Object[] properties, java.util.Set<String> labels) throws java.io.IOException
		 private void AssertNextNode( InputIterator data, object id, object[] properties, ISet<string> labels )
		 {
			  AssertNextNode( data, GLOBAL, id, properties, labels );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertNextNode(org.Neo4Net.unsafe.impl.batchimport.InputIterator data, org.Neo4Net.unsafe.impl.batchimport.input.Group group, Object id, Object[] properties, java.util.Set<String> labels) throws java.io.IOException
		 private void AssertNextNode( InputIterator data, Group group, object id, object[] properties, ISet<string> labels )
		 {
			  assertTrue( ReadNext( data ) );
			  assertEquals( group.Id(), _visitor.idGroup.id() );
			  assertEquals( id, _visitor.id() );
			  assertArrayEquals( properties, _visitor.properties() );
			  assertEquals( labels, asSet( _visitor.labels() ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean readNext(org.Neo4Net.unsafe.impl.batchimport.InputIterator data) throws java.io.IOException
		 private bool ReadNext( InputIterator data )
		 {
			  if ( _referenceData != data )
			  {
					_chunk = null;
					_referenceData = data;
			  }

			  if ( _chunk == null )
			  {
					_chunk = data.NewChunk();
					if ( !data.Next( _chunk ) )
					{
						 return false;
					}
			  }

			  if ( _chunk.next( _visitor ) )
			  {
					return true;
			  }
			  if ( !data.Next( _chunk ) )
			  {
					return false;
			  }
			  return _chunk.next( _visitor );
		 }

		 private object[] Properties( params object[] keysAndValues )
		 {
			  return keysAndValues;
		 }

		 private ISet<string> Labels( params string[] labels )
		 {
			  return asSet( labels );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private Header.Factory header(final Header.Entry... entries)
		 private Header.Factory Header( params Header.Entry[] entries )
		 {
			  return new FactoryAnonymousInnerClass( this, entries );
		 }

		 private class FactoryAnonymousInnerClass : Header.Factory
		 {
			 private readonly CsvInputTest _outerInstance;

			 private Neo4Net.@unsafe.Impl.Batchimport.input.csv.Header.Entry[] _entries;

			 public FactoryAnonymousInnerClass( CsvInputTest outerInstance, Neo4Net.@unsafe.Impl.Batchimport.input.csv.Header.Entry[] entries )
			 {
				 this.outerInstance = outerInstance;
				 this._entries = entries;
			 }

			 public bool Defined
			 {
				 get
				 {
					  return true;
				 }
			 }

			 public Header create( CharSeeker dataSeeker, Configuration configuration, IdType idType, Groups groups )
			 {
				  return new Header( _entries );
			 }
		 }

		 private Header.Entry Entry<T1>( string name, Type type, Extractor<T1> extractor )
		 {
			  return Entry( name, type, null, extractor );
		 }

		 private Header.Entry Entry<T1>( string name, Type type, string groupName, Extractor<T1> extractor )
		 {
			  return new Header.Entry( name, type, _groups.getOrCreate( groupName ), extractor );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static DataFactory data(final String data)
		 private static DataFactory Data( string data )
		 {
			  return data( data, value => value );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static DataFactory data(final String data, final Decorator decorator)
		 private static DataFactory Data( string data, Decorator decorator )
		 {
			  return config => DataItem( CharReader( data ), decorator );
		 }

		 private static CharReadable CharReader( string data )
		 {
			  return wrap( data );
		 }

		 private IEnumerable<DataFactory> DataIterable( params DataFactory[] data )
		 {
			  return Iterables.iterable( data );
		 }

		 private class CapturingDataFactories : IEnumerable<DataFactory>
		 {
			  internal readonly System.Func<Configuration, CharReadable> Factory;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal CharReadable LastConflict;
			  internal readonly Decorator Decorator;

			  internal CapturingDataFactories( System.Func<Configuration, CharReadable> factory, Decorator decorator )
			  {
					this.Factory = factory;
					this.Decorator = decorator;
			  }

			  public override IEnumerator<DataFactory> Iterator()
			  {
					return Iterators.iterator( config => new DataAnonymousInnerClass5( this ) );
			  }

			  private class DataAnonymousInnerClass5 : Data
			  {
				  private readonly CapturingDataFactories _outerInstance;

				  public DataAnonymousInnerClass5( CapturingDataFactories outerInstance )
				  {
					  this.outerInstance = outerInstance;
				  }

				  public RawIterator<CharReadable, IOException> stream()
				  {
						_outerInstance.last = _outerInstance.factory.apply( config );
						return Readables.iterator( @in => @in, _outerInstance.last );
				  }

				  public Decorator decorator()
				  {
						return _outerInstance.decorator;
				  }
			  }

			  internal virtual CharReadable Last()
			  {
					return LastConflict;
			  }
		 }

		 private class FailingNodeDecorator : Decorator
		 {
			  internal readonly Exception Failure;

			  internal FailingNodeDecorator( Exception failure )
			  {
					this.Failure = failure;
			  }

			  public override InputEntityVisitor Apply( InputEntityVisitor t )
			  {
					return new InputEntityVisitor_DelegateAnonymousInnerClass( this, t );
			  }

			  private class InputEntityVisitor_DelegateAnonymousInnerClass : Neo4Net.@unsafe.Impl.Batchimport.input.InputEntityVisitor_Delegate
			  {
				  private readonly FailingNodeDecorator _outerInstance;

				  public InputEntityVisitor_DelegateAnonymousInnerClass( FailingNodeDecorator outerInstance, InputEntityVisitor t ) : base( t )
				  {
					  this.outerInstance = outerInstance;
				  }

				  public override void endOfEntity()
				  {
						throw _outerInstance.failure;
				  }
			  }
		 }

		 private Configuration Config( Configuration config )
		 {
			  return new Configuration_OverriddenAnonymousInnerClass( this, config );
		 }

		 private class Configuration_OverriddenAnonymousInnerClass : Configuration_Overridden
		 {
			 private readonly CsvInputTest _outerInstance;

			 public Configuration_OverriddenAnonymousInnerClass( CsvInputTest outerInstance, Neo4Net.@unsafe.Impl.Batchimport.Configuration config ) : base( config )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override bool multilineFields()
			 {
				  return _outerInstance.allowMultilineFields.Value;
			 }
		 }
	}

}