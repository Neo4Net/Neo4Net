using System.Text;

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
namespace Org.Neo4j.Csv.Reader
{
	using Test = org.junit.jupiter.api.Test;


	using Org.Neo4j.Collection;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;

	internal class MultiReadableTest
	{
		 private static readonly Configuration CONFIG = new Configuration_OverriddenAnonymousInnerClass( Configuration_Fields.Default );

		 private class Configuration_OverriddenAnonymousInnerClass : Configuration_Overridden
		 {
			 public Configuration_OverriddenAnonymousInnerClass( Org.Neo4j.Csv.Reader.Configuration configurationFields ) : base( configurationFields.Default )
			 {
			 }

			 public override int bufferSize()
			 {
				  return 200;
			 }
		 }
		 private readonly Mark _mark = new Mark();
		 private readonly Extractors _extractors = new Extractors( ';' );
		 private readonly int _delimiter = ',';

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReadFromMultipleReaders() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldReadFromMultipleReaders()
		 {
			  // GIVEN
			  string[][] data = new string[][]
			  {
				  new string[] { "this is", "the first line" },
				  new string[] { "where this", "is the second line" },
				  new string[] { "and here comes", "the third line" }
			  };
			  RawIterator<CharReadable, IOException> readers = ReaderIteratorFromStrings( data, null );
			  CharSeeker seeker = CharSeekers.CharSeeker( new MultiReadable( readers ), CONFIG, true );

			  // WHEN/THEN
			  foreach ( string[] line in data )
			  {
					AssertNextLine( line, seeker, _mark, _extractors );
			  }
			  assertFalse( seeker.Seek( _mark, _delimiter ) );
			  seeker.Dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleSourcesEndingWithNewLine() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldHandleSourcesEndingWithNewLine()
		 {
			  // GIVEN
			  string[][] data = new string[][]
			  {
				  new string[] { "this is", "the first line" },
				  new string[] { "where this", "is the second line" }
			  };

			  // WHEN
			  RawIterator<CharReadable, IOException> readers = ReaderIteratorFromStrings( data, '\n' );
			  CharSeeker seeker = CharSeekers.CharSeeker( new MultiReadable( readers ), CONFIG, true );

			  // WHEN/THEN
			  foreach ( string[] line in data )
			  {
					AssertNextLine( line, seeker, _mark, _extractors );
			  }
			  assertFalse( seeker.Seek( _mark, _delimiter ) );
			  seeker.Dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldTrackAbsolutePosition() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldTrackAbsolutePosition()
		 {
			  // GIVEN
			  string[][] data = new string[][]
			  {
				  new string[] { "this is", "the first line" },
				  new string[] { "where this", "is the second line" }
			  };
			  RawIterator<CharReadable, IOException> readers = ReaderIteratorFromStrings( data, '\n' );
			  CharReadable reader = new MultiReadable( readers );
			  assertEquals( 0L, reader.Position() );
			  SectionedCharBuffer buffer = new SectionedCharBuffer( 15 );

			  // WHEN
			  reader.Read( buffer, buffer.Front() );
			  assertEquals( 15, reader.Position() );
			  reader.Read( buffer, buffer.Front() );
			  assertEquals( 23, reader.Position(), "Should not transition to a new reader in the middle of a read" );
			  assertEquals( "Reader1", reader.SourceDescription() );

			  // we will transition to the new reader in the call below
			  reader.Read( buffer, buffer.Front() );
			  assertEquals( 23 + 15, reader.Position() );
			  reader.Read( buffer, buffer.Front() );
			  assertEquals( 23 + 30, reader.Position() );
			  reader.Read( buffer, buffer.Front() );
			  assertFalse( buffer.HasAvailable() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotCrossSourcesInOneRead() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldNotCrossSourcesInOneRead()
		 {
			  // given
			  string source1 = "abcdefghijklm";
			  string source2 = "nopqrstuvwxyz";
			  string[][] data = new string[][]
			  {
				  new string[] { source1 },
				  new string[] { source2 }
			  };
			  CharReadable readable = new MultiReadable( ReaderIteratorFromStrings( data, '\n' ) );

			  // when
			  char[] target = new char[source1.Length + source2.Length / 2];
			  int read = readable.Read( target, 0, target.Length );

			  // then
			  assertEquals( source1.Length + 1, read );

			  // and when
			  target = new char[source2.Length];
			  read = readable.Read( target, 0, target.Length );

			  // then
			  assertEquals( source2.Length, read );

			  read = readable.Read( target, 0, target.Length );
			  assertEquals( 1, read );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertNextLine(String[] line, CharSeeker seeker, Mark mark, Extractors extractors) throws java.io.IOException
		 private void AssertNextLine( string[] line, CharSeeker seeker, Mark mark, Extractors extractors )
		 {
			  foreach ( string value in line )
			  {
					assertTrue( seeker.Seek( mark, _delimiter ) );
					assertEquals( value, seeker.Extract( mark, extractors.String() ).value() );
			  }
			  assertTrue( mark.EndOfLine );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private org.neo4j.collection.RawIterator<CharReadable,java.io.IOException> readerIteratorFromStrings(final String[][] data, final System.Nullable<char> lineEnding)
		 private RawIterator<CharReadable, IOException> ReaderIteratorFromStrings( string[][] data, char? lineEnding )
		 {
			  return new RawIteratorAnonymousInnerClass( this, data, lineEnding );
		 }

		 private class RawIteratorAnonymousInnerClass : RawIterator<CharReadable, IOException>
		 {
			 private readonly MultiReadableTest _outerInstance;

			 private string[][] _data;
			 private char? _lineEnding;

			 public RawIteratorAnonymousInnerClass( MultiReadableTest outerInstance, string[][] data, char? lineEnding )
			 {
				 this.outerInstance = outerInstance;
				 this._data = data;
				 this._lineEnding = lineEnding;
			 }

			 private int cursor;

			 public bool hasNext()
			 {
				  return cursor < _data.Length;
			 }

			 public CharReadable next()
			 {
				  string @string = join( _data[cursor++] );
				  return Readables.wrap(new StringReaderAnonymousInnerClass(this, @string)
				 , @string.Length * 2);
			 }

			 private class StringReaderAnonymousInnerClass : StringReader
			 {
				 private readonly RawIteratorAnonymousInnerClass _outerInstance;

				 public StringReaderAnonymousInnerClass( RawIteratorAnonymousInnerClass outerInstance, string @string ) : base( @string )
				 {
					 this.outerInstance = outerInstance;
				 }

				 public override string ToString()
				 {
					  return "Reader" + cursor;
				 }
			 }

			 private string join( string[] strings )
			 {
				  StringBuilder builder = new StringBuilder();
				  foreach ( string @string in strings )
				  {
						builder.Append( builder.Length > 0 ? "," : "" ).Append( @string );
				  }
				  if ( _lineEnding != null )
				  {
						builder.Append( _lineEnding );
				  }
				  return builder.ToString();
			 }

			 public void remove()
			 {
				  throw new System.NotSupportedException();
			 }
		 }

	}

}