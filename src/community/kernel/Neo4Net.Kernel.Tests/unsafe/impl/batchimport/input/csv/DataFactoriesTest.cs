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
	using Test = org.junit.Test;

	using CharReadable = Neo4Net.Csv.Reader.CharReadable;
	using CharSeeker = Neo4Net.Csv.Reader.CharSeeker;
	using CharSeekers = Neo4Net.Csv.Reader.CharSeekers;
	using Neo4Net.Csv.Reader;
	using Extractors = Neo4Net.Csv.Reader.Extractors;
	using MultiReadable = Neo4Net.Csv.Reader.MultiReadable;
	using Readables = Neo4Net.Csv.Reader.Readables;
	using IOFunctions = Neo4Net.Functions.IOFunctions;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.csv.reader.Readables.wrap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.ArrayUtil.array;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.input.csv.DataFactories.defaultFormatNodeFileHeader;

	public class DataFactoriesTest
	{
		 private const int BUFFER_SIZE = 10_000;
		 private static readonly Configuration _commas = WithBufferSize( Configuration.COMMAS, BUFFER_SIZE );
		 private static readonly Configuration _tabs = WithBufferSize( Configuration.TABS, BUFFER_SIZE );

		 private readonly Groups _groups = new Groups();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParseDefaultNodeFileHeaderCorrectly() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldParseDefaultNodeFileHeaderCorrectly()
		 {
			  // GIVEN
			  CharSeeker seeker = seeker( "ID:ID,label-one:label,also-labels:LABEL,name,age:long" );
			  IdType idType = IdType.String;
			  Extractors extractors = new Extractors( ',' );

			  // WHEN
			  Header header = DataFactories.DefaultFormatNodeFileHeader().create(seeker, _commas, idType, _groups);

			  // THEN
			  assertArrayEquals( array( Entry( "ID", Type.Id, idType.extractor( extractors ) ), Entry( "label-one", Type.Label, extractors.StringArray() ), Entry("also-labels", Type.Label, extractors.StringArray()), Entry("name", Type.Property, extractors.String()), Entry("age", Type.Property, extractors.Long_()) ), header.Entries() );
			  seeker.Dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParseDefaultRelationshipFileHeaderCorrectly() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldParseDefaultRelationshipFileHeaderCorrectly()
		 {
			  // GIVEN
			  CharSeeker seeker = seeker( ":START_ID\t:END_ID\ttype:TYPE\tdate:long\tmore:long[]" );
			  IdType idType = IdType.Actual;
			  Extractors extractors = new Extractors( '\t' );

			  // WHEN
			  Header header = DataFactories.DefaultFormatRelationshipFileHeader().create(seeker, _tabs, idType, _groups);

			  // THEN
			  assertArrayEquals( array( Entry( null, Type.StartId, idType.extractor( extractors ) ), Entry( null, Type.EndId, idType.extractor( extractors ) ), Entry( "type", Type.Type, extractors.String() ), Entry("date", Type.Property, extractors.Long_()), Entry("more", Type.Property, extractors.LongArray()) ), header.Entries() );
			  seeker.Dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveEmptyHeadersBeInterpretedAsIgnored() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHaveEmptyHeadersBeInterpretedAsIgnored()
		 {
			  // GIVEN
			  CharSeeker seeker = seeker( "one:id\ttwo\t\tdate:long" );
			  IdType idType = IdType.Actual;
			  Extractors extractors = new Extractors( '\t' );

			  // WHEN
			  Header header = DataFactories.DefaultFormatNodeFileHeader().create(seeker, _tabs, idType, _groups);

			  // THEN
			  assertArrayEquals( array( Entry( "one", Type.Id, extractors.Long_() ), Entry("two", Type.Property, extractors.String()), Entry(null, Type.Ignore, null), Entry("date", Type.Property, extractors.Long_()) ), header.Entries() );
			  seeker.Dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailForDuplicatePropertyHeaderEntries() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailForDuplicatePropertyHeaderEntries()
		 {
			  // GIVEN
			  CharSeeker seeker = seeker( "one:id\tname\tname:long" );
			  IdType idType = IdType.Actual;
			  Extractors extractors = new Extractors( '\t' );

			  // WHEN
			  try
			  {
					DataFactories.DefaultFormatNodeFileHeader().create(seeker, _tabs, idType, _groups);
					fail( "Should fail" );
			  }
			  catch ( DuplicateHeaderException e )
			  {
					assertEquals( Entry( "name", Type.Property, extractors.String() ), e.First );
					assertEquals( Entry( "name", Type.Property, extractors.Long_() ), e.Other );
			  }
			  seeker.Dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailForDuplicateIdHeaderEntries() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailForDuplicateIdHeaderEntries()
		 {
			  // GIVEN
			  CharSeeker seeker = seeker( "one:id\ttwo:id" );
			  IdType idType = IdType.Actual;
			  Extractors extractors = new Extractors( '\t' );

			  // WHEN
			  try
			  {
					DataFactories.DefaultFormatNodeFileHeader().create(seeker, _tabs, idType, _groups);
					fail( "Should fail" );
			  }
			  catch ( DuplicateHeaderException e )
			  {
					assertEquals( Entry( "one", Type.Id, extractors.Long_() ), e.First );
					assertEquals( Entry( "two", Type.Id, extractors.Long_() ), e.Other );
			  }
			  seeker.Dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowMissingIdHeaderEntry() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowMissingIdHeaderEntry()
		 {
			  // GIVEN
			  CharSeeker seeker = seeker( "one\ttwo" );
			  Extractors extractors = new Extractors( ';' );

			  // WHEN
			  Header header = DataFactories.DefaultFormatNodeFileHeader().create(seeker, _tabs, IdType.Actual, _groups);

			  // THEN
			  assertArrayEquals( array( Entry( "one", Type.Property, extractors.String() ), Entry("two", Type.Property, extractors.String()) ), header.Entries() );
			  seeker.Dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParseHeaderFromFirstLineOfFirstInputFile() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldParseHeaderFromFirstLineOfFirstInputFile()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.csv.reader.CharReadable firstSource = wrap("id:ID\tname:String\tbirth_date:long");
			  CharReadable firstSource = wrap( "id:ID\tname:String\tbirth_date:long" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.csv.reader.CharReadable secondSource = wrap("0\tThe node\t123456789");
			  CharReadable secondSource = wrap( "0\tThe node\t123456789" );
			  DataFactory dataFactory = DataFactories.Data( value => value, () => new MultiReadable(Readables.iterator(IOFunctions.identity(), firstSource, secondSource)) );
			  Header.Factory headerFactory = defaultFormatNodeFileHeader();
			  Extractors extractors = new Extractors( ';' );

			  // WHEN
			  CharSeeker seeker = CharSeekers.charSeeker( new MultiReadable( dataFactory.Create( _tabs ).stream() ), _tabs, false );
			  Header header = headerFactory.Create( seeker, _tabs, IdType.Actual, _groups );

			  // THEN
			  assertArrayEquals( array( Entry( "id", Type.Id, extractors.Long_() ), Entry("name", Type.Property, extractors.String()), Entry("birth_date", Type.Property, extractors.Long_()) ), header.Entries() );
			  seeker.Dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParseGroupName() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldParseGroupName()
		 {
			  // GIVEN
			  string groupOneName = "GroupOne";
			  string groupTwoName = "GroupTwo";
			  CharSeeker seeker = seeker( ":START_ID(" + groupOneName + ")\t:END_ID(" + groupTwoName + ")\ttype:TYPE\tdate:long\tmore:long[]" );
			  IdType idType = IdType.Actual;
			  Extractors extractors = new Extractors( '\t' );
			  _groups.getOrCreate( groupOneName );
			  _groups.getOrCreate( groupTwoName );

			  // WHEN
			  Header header = DataFactories.DefaultFormatRelationshipFileHeader().create(seeker, _tabs, idType, _groups);

			  // THEN
			  assertArrayEquals( array( Entry( null, Type.StartId, "GroupOne", idType.extractor( extractors ) ), Entry( null, Type.EndId, "GroupTwo", idType.extractor( extractors ) ), Entry( "type", Type.Type, extractors.String() ), Entry("date", Type.Property, extractors.Long_()), Entry("more", Type.Property, extractors.LongArray()) ), header.Entries() );
			  seeker.Dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailOnUnexpectedNodeHeaderType()
		 public virtual void ShouldFailOnUnexpectedNodeHeaderType()
		 {
			  // GIVEN
			  CharSeeker seeker = seeker( ":ID,:START_ID" );
			  IdType idType = IdType.Actual;

			  // WHEN
			  try
			  {
					Header header = DataFactories.DefaultFormatNodeFileHeader().create(seeker, _commas, idType, _groups);
					fail( "Should have failed" );
			  }
			  catch ( InputException e )
			  {
					// THEN
					assertThat( e.Message, containsString( "START_ID" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailOnUnexpectedRelationshipHeaderType()
		 public virtual void ShouldFailOnUnexpectedRelationshipHeaderType()
		 {
			  // GIVEN
			  CharSeeker seeker = seeker( ":LABEL,:START_ID,:END_ID,:TYPE" );
			  IdType idType = IdType.Actual;

			  // WHEN
			  try
			  {
					Header header = DataFactories.DefaultFormatRelationshipFileHeader().create(seeker, _commas, idType, _groups);
					fail( "Should have failed" );
			  }
			  catch ( InputException e )
			  {
					// THEN
					assertThat( e.Message, containsString( "LABEL" ) );
			  }
		 }

		 private static readonly Neo4Net.Csv.Reader.Configuration SEEKER_CONFIG = new Configuration_OverriddenAnonymousInnerClass();

		 private class Configuration_OverriddenAnonymousInnerClass : Neo4Net.Csv.Reader.Configuration_Overridden
		 {
			 public Configuration_OverriddenAnonymousInnerClass() : base(new Neo4Net.csv.reader.Configuration_Default())
			 {
			 }

			 public override int bufferSize()
			 {
				  return 1_000;
			 }
		 }

		 private CharSeeker Seeker( string data )
		 {
			  return CharSeekers.charSeeker( wrap( data ), SEEKER_CONFIG, false );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static Configuration withBufferSize(Configuration config, final int bufferSize)
		 private static Configuration WithBufferSize( Configuration config, int bufferSize )
		 {
			  return new Configuration_OverriddenAnonymousInnerClass( config, bufferSize );
		 }

		 private class Configuration_OverriddenAnonymousInnerClass : Configuration_Overridden
		 {
			 private int _bufferSize;

			 public Configuration_OverriddenAnonymousInnerClass( Neo4Net.@unsafe.Impl.Batchimport.Configuration config, int bufferSize ) : base( config )
			 {
				 this._bufferSize = bufferSize;
			 }

			 public override int bufferSize()
			 {
				  return _bufferSize;
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
	}

}