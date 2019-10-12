using System.Collections.Generic;
using System.IO;

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
namespace Neo4Net.Csv.Reader
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;
	using Parameter = org.junit.runners.Parameterized.Parameter;
	using Parameters = org.junit.runners.Parameterized.Parameters;


	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class ReadablesTest
	public class ReadablesTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameters public static java.util.Collection<ReadMethod> readMethods()
		 public static ICollection<ReadMethod> ReadMethods()
		 {
			  return Arrays.asList((readable, length) =>
			  {
						  SectionedCharBuffer readText = new SectionedCharBuffer( length );
						  readable.read( readText, readText.Front() );
						  return copyOfRange( readText.Array(), readText.Pivot(), readText.Front() );
			  }, ( readable, length ) =>
			  {
						  char[] result = new char[length];
						  readable.read( result, 0, length );
						  return result;
					 });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory directory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory Directory = TestDirectory.testDirectory();

		 internal interface ReadMethod
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: char[] read(CharReadable readable, int length) throws java.io.IOException;
			  char[] Read( CharReadable readable, int length );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameter public ReadMethod readMethod;
		 public ReadMethod ReadMethod;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadTextCompressedInZipArchiveWithSingleFileIn() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadTextCompressedInZipArchiveWithSingleFileIn()
		 {
			  // GIVEN
			  string text = "abcdefghijlkmnopqrstuvxyz";

			  // WHEN
			  File compressed = CompressWithZip( text );

			  // THEN
			  AssertReadText( compressed, text );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadTextCompressedInGZipFile() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadTextCompressedInGZipFile()
		 {
			  // GIVEN
			  string text = "abcdefghijlkmnopqrstuvxyz";

			  // WHEN
			  File compressed = CompressWithGZip( text );

			  // THEN
			  AssertReadText( compressed, text );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadPlainTextFile() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadPlainTextFile()
		 {
			  // GIVEN
			  string text = "abcdefghijlkmnopqrstuvxyz";

			  // WHEN
			  File plainText = Write( text );

			  // THEN
			  AssertReadText( plainText, text );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadTheOnlyRealFileInThere() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadTheOnlyRealFileInThere()
		 {
			  // GIVEN
			  string text = "abcdefghijlkmnopqrstuvxyz";

			  // WHEN
			  File compressed = CompressWithZip( text, ".nothing", ".DS_Store", "__MACOSX/", "__MACOSX/file" );

			  // THEN
			  AssertReadText( compressed, text );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailWhenThereAreMoreThanOneSuitableFileInThere() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailWhenThereAreMoreThanOneSuitableFileInThere()
		 {
			  // GIVEN
			  string text = "abcdefghijlkmnopqrstuvxyz";
			  File compressed = CompressWithZip( text, ".nothing", ".DS_Store", "somewhere/something" );

			  // WHEN
			  CharReadable readable;
			  try
			  {
					readable = Readables.Files( Charset.defaultCharset(), compressed );
					fail( "Should fail since there are multiple suitable files in the zip archive" );
			  }
			  catch ( IOException e )
			  { // Good
					assertThat( e.Message, containsString( "Multiple" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTrackPosition() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTrackPosition()
		 {
			  // GIVEN
			  string data = "1234567890";
			  //                 ^   ^
			  CharReadable reader = Readables.Wrap( data );
			  SectionedCharBuffer buffer = new SectionedCharBuffer( 4 );

			  // WHEN
			  int expected = 0;
			  do
			  {
					buffer = reader.Read( buffer, buffer.Front() );
					expected += buffer.Available();

					// THEN
					assertEquals( expected, reader.Position() );
			  } while ( buffer.HasAvailable() );

			  // and THEN
			  assertEquals( data.ToCharArray().length, expected );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldComplyWithUtf8CharsetForExample() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldComplyWithUtf8CharsetForExample()
		 {
			  ShouldComplyWithSpecifiedCharset( StandardCharsets.UTF_8 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldComplyWithIso88591CharsetForExample() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldComplyWithIso88591CharsetForExample()
		 {
			  ShouldComplyWithSpecifiedCharset( StandardCharsets.ISO_8859_1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSkipBOM() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSkipBOM()
		 {
			  // GIVEN
			  string text = "abcdefghijklmnop";

			  // WHEN/THEN
			  ShouldReadTextFromFileWithBom( Magic.BomUtf_32Be, text );
			  ShouldReadTextFromFileWithBom( Magic.BomUtf_32Le, text );
			  ShouldReadTextFromFileWithBom( Magic.BomUtf_16Be, text );
			  ShouldReadTextFromFileWithBom( Magic.BomUtf_16Le, text );
			  ShouldReadTextFromFileWithBom( Magic.BomUtf_8, text );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadTextFromWrappedInputStream() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadTextFromWrappedInputStream()
		 {
			  // GIVEN
			  string text = "abcdefghijklmnop";

			  // WHEN
			  File file = WriteToFile( text, Charset.defaultCharset() );

			  // THEN
			  AssertReadTextAsInputStream( file, text );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSkipBomWhenWrappingInputStream() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSkipBomWhenWrappingInputStream()
		 {
			  // GIVEN
			  string text = "abcdefghijklmnop";

			  // WHEN/THEN
			  ShouldReadTextFromInputStreamWithBom( Magic.BomUtf_32Be, text );
			  ShouldReadTextFromInputStreamWithBom( Magic.BomUtf_32Le, text );
			  ShouldReadTextFromInputStreamWithBom( Magic.BomUtf_16Be, text );
			  ShouldReadTextFromInputStreamWithBom( Magic.BomUtf_16Le, text );
			  ShouldReadTextFromInputStreamWithBom( Magic.BomUtf_8, text );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExtractFirstLine() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldExtractFirstLine()
		 {
			  // given
			  string firstLine = "characters of first line";
			  string secondLine = "here on the second line";
			  CharReadable reader = Readables.Wrap( firstLine + "\n" + secondLine );

			  // when
			  char[] firstLineCharacters = Readables.ExtractFirstLineFrom( reader );
			  char[] secondLineCharacters = new char[secondLine.Length];
			  reader.Read( secondLineCharacters, 0, secondLineCharacters.Length );

			  // then
			  assertArrayEquals( firstLine.ToCharArray(), firstLineCharacters );
			  assertArrayEquals( secondLine.ToCharArray(), secondLineCharacters );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExtractOnlyLine() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldExtractOnlyLine()
		 {
			  // given
			  string firstLine = "characters of only line";
			  CharReadable reader = Readables.Wrap( firstLine );

			  // when
			  char[] firstLineCharacters = Readables.ExtractFirstLineFrom( reader );
			  int readAfterwards = reader.Read( new char[1], 0, 1 );

			  // then
			  assertArrayEquals( firstLine.ToCharArray(), firstLineCharacters );
			  assertEquals( -1, readAfterwards );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void shouldReadTextFromFileWithBom(Magic bom, String text) throws java.io.IOException
		 private void ShouldReadTextFromFileWithBom( Magic bom, string text )
		 {
			  AssertReadText( WriteToFile( bom.Bytes(), text, bom.Encoding() ), text );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void shouldReadTextFromInputStreamWithBom(Magic bom, String text) throws java.io.IOException
		 private void ShouldReadTextFromInputStreamWithBom( Magic bom, string text )
		 {
			  AssertReadTextAsInputStream( WriteToFile( bom.Bytes(), text, bom.Encoding() ), text );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void shouldComplyWithSpecifiedCharset(java.nio.charset.Charset charset) throws Exception
		 private void ShouldComplyWithSpecifiedCharset( Charset charset )
		 {
			  // GIVEN
			  string data = "abcåäö[]{}";
			  File file = WriteToFile( data, charset );

			  // WHEN
			  CharReadable reader = Readables.Files( charset, file );
			  SectionedCharBuffer buffer = new SectionedCharBuffer( 100 );
			  buffer = reader.Read( buffer, buffer.Front() );

			  // THEN
			  char[] expected = data.ToCharArray();
			  char[] array = buffer.Array();
			  assertEquals( expected.Length, buffer.Available() );
			  for ( int i = 0; i < expected.Length; i++ )
			  {
					assertEquals( expected[i], array[buffer.Pivot() + i] );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File writeToFile(String data, java.nio.charset.Charset charset) throws java.io.IOException
		 private File WriteToFile( string data, Charset charset )
		 {
			  File file = new File( Directory.directory(), "text-" + charset.name() );
			  using ( Writer writer = new StreamWriter( new FileStream( file, FileMode.Create, FileAccess.Write ), charset ) )
			  {
					writer.append( data );
			  }
			  return file;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File writeToFile(byte[] header, String data, java.nio.charset.Charset charset) throws java.io.IOException
		 private File WriteToFile( sbyte[] header, string data, Charset charset )
		 {
			  File file = new File( Directory.directory(), "text-" + charset.name() );
			  using ( Stream @out = new FileStream( file, FileMode.Create, FileAccess.Write ), Writer writer = new StreamWriter( @out, charset ) )
			  {
					@out.Write( header, 0, header.Length );
					writer.append( data );
			  }
			  return file;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File write(String text) throws java.io.IOException
		 private File Write( string text )
		 {
			  File file = Directory.file( "plain-text" );
			  using ( Stream @out = new FileStream( file, FileMode.Create, FileAccess.Write ) )
			  {
					@out.WriteByte( text.GetBytes() );
			  }
			  return file;
		 }

		 // TODO test for failing reading a ZIP archive with multiple files in

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File compressWithZip(String text, String... otherEntries) throws java.io.IOException
		 private File CompressWithZip( string text, params string[] otherEntries )
		 {
			  File file = Directory.file( "compressed" );
			  using ( ZipOutputStream @out = new ZipOutputStream( new FileStream( file, FileMode.Create, FileAccess.Write ) ) )
			  {
					foreach ( string otherEntry in otherEntries )
					{
						 @out.putNextEntry( new ZipEntry( otherEntry ) );
					}

					@out.putNextEntry( new ZipEntry( "file" ) );
					@out.write( text.GetBytes() );
			  }
			  return file;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File compressWithGZip(String text) throws java.io.IOException
		 private File CompressWithGZip( string text )
		 {
			  File file = Directory.file( "compressed" );
			  using ( GZIPOutputStream @out = new GZIPOutputStream( new FileStream( file, FileMode.Create, FileAccess.Write ) ) )
			  {
					@out.write( text.GetBytes() );
			  }
			  return file;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertReadText(java.io.File file, String text) throws java.io.IOException
		 private void AssertReadText( File file, string text )
		 {
			  AssertReadText( Readables.Files( Charset.defaultCharset(), file ), text );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertReadTextAsInputStream(java.io.File file, String text) throws java.io.IOException
		 private void AssertReadTextAsInputStream( File file, string text )
		 {
			  using ( Stream stream = new FileStream( file, FileMode.Open, FileAccess.Read ) )
			  {
					AssertReadText( Readables.Wrap( stream, file.Path, Charset.defaultCharset(), file.length() ), text );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertReadText(CharReadable readable, String text) throws java.io.IOException
		 private void AssertReadText( CharReadable readable, string text )
		 {
			  char[] readText = ReadMethod.read( readable, text.ToCharArray().length );
			  assertArrayEquals( readText, text.ToCharArray() );
		 }
	}

}