using System;
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
namespace Neo4Net.Csv.Reader
{

	using Neo4Net.Collections;
	using Neo4Net.Functions;
	using Neo4Net.Functions;

	/// <summary>
	/// Means of instantiating common <seealso cref="CharReadable"/> instances.
	/// 
	/// There are support for compressed files as well for those methods accepting a <seealso cref="File"/> argument.
	/// <ol>
	/// <li>ZIP: is both an archive and a compression format. In many cases the order of files
	/// is important and for a ZIP archive with multiple files, the order of the files are whatever the order
	/// set by the tool that created the ZIP archive. Therefore only single-file-zip files are supported.
	/// The single file in the given ZIP archive will be decompressed on the fly, while reading.</li>
	/// <li>GZIP: is only a compression format and so will be decompressed on the fly, while reading.</li>
	/// </ol>
	/// </summary>
	public class Readables
	{
		 private Readables()
		 {
			  throw new AssertionError( "No instances allowed" );
		 }

		 public static readonly CharReadable EMPTY = new CharReadable_AdapterAnonymousInnerClass();

		 private class CharReadable_AdapterAnonymousInnerClass : CharReadable_Adapter
		 {
			 public override SectionedCharBuffer read( SectionedCharBuffer buffer, int from )
			 {
				  return buffer;
			 }

			 public override void close()
			 { // Nothing to close
			 }

			 public override string sourceDescription()
			 {
				  return "EMPTY";
			 }

			 public override int read( char[] into, int offset, int length )
			 {
				  return -1;
			 }

			 public override long length()
			 {
				  return 0;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static CharReadable wrap(final java.io.InputStream stream, final String sourceName, java.nio.charset.Charset charset) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public static CharReadable Wrap( Stream stream, string sourceName, Charset charset )
		 {
			  return Wrap( stream, sourceName, charset, 0 );
		 }

		 /// <summary>
		 /// Wraps a <seealso cref="System.IO.Stream_Input"/> in a <seealso cref="CharReadable"/>.
		 /// </summary>
		 /// <param name="stream"> <seealso cref="Reader"/> to wrap. </param>
		 /// <param name="sourceName"> name or description of the source of the stream. </param>
		 /// <param name="charset"> <seealso cref="Charset"/> to use for reading. </param>
		 /// <param name="length"> total number of bytes provided by the reader. </param>
		 /// <returns> a <seealso cref="CharReadable"/> for the <seealso cref="Reader"/>. </returns>
		 /// <exception cref="IOException"> on I/O error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static CharReadable wrap(final java.io.InputStream stream, final String sourceName, java.nio.charset.Charset charset, long length) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public static CharReadable Wrap( Stream stream, string sourceName, Charset charset, long length )
		 {
			  sbyte[] bytes = new sbyte[Magic.Longest()];
			  PushbackInputStream pushbackStream = new PushbackInputStream( stream, bytes.Length );
			  Charset usedCharset = charset;
			  int read = stream.Read( bytes, 0, bytes.Length );
			  if ( read >= 0 )
			  {
					bytes = read < bytes.Length ? Arrays.copyOf( bytes, read ) : bytes;
					Magic magic = Magic.Of( bytes );
					int excessiveBytes = read;
					if ( magic.ImpliesEncoding() )
					{
						 // Unread the diff between the BOM and the longest magic we gathered bytes for
						 excessiveBytes -= magic.Length();
						 usedCharset = magic.Encoding();
					}
					pushbackStream.unread( bytes, read - excessiveBytes, excessiveBytes );
			  }
			  return wrap(new InputStreamReaderAnonymousInnerClass(pushbackStream, usedCharset, sourceName)
			 , length);
		 }

		 private class InputStreamReaderAnonymousInnerClass : StreamReader
		 {
			 private string _sourceName;

			 public InputStreamReaderAnonymousInnerClass( PushbackInputStream pushbackStream, Charset usedCharset, string sourceName ) : base( pushbackStream, usedCharset )
			 {
				 this._sourceName = sourceName;
			 }

			 public override string ToString()
			 {
				  return _sourceName;
			 }
		 }

		 public static CharReadable Wrap( string data )
		 {
			  return Wrap( new StringReader( data ), data.Length );
		 }

		 /// <summary>
		 /// Wraps a <seealso cref="Reader"/> in a <seealso cref="CharReadable"/>.
		 /// Remember that the <seealso cref="Reader.toString()"/> must provide a description of the data source.
		 /// </summary>
		 /// <param name="reader"> <seealso cref="Reader"/> to wrap. </param>
		 /// <param name="length"> total number of bytes provided by the reader. </param>
		 /// <returns> a <seealso cref="CharReadable"/> for the <seealso cref="Reader"/>. </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static CharReadable wrap(final java.io.Reader reader, long length)
		 public static CharReadable Wrap( Reader reader, long length )
		 {
			  return new WrappedCharReadable( length, reader );
		 }

		 private class FromFile : IOFunction<File, CharReadable>
		 {
			  internal readonly Charset Charset;

			  internal FromFile( Charset charset )
			  {
					this.Charset = charset;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public CharReadable apply(final java.io.File file) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
			  public override CharReadable Apply( File file )
			  {
					Magic magic = Magic.Of( file );
					if ( magic == Magic.Zip )
					{ // ZIP file
						 ZipFile zipFile = new ZipFile( file );
						 ZipEntry entry = GetSingleSuitableEntry( zipFile );
						 return wrap(new InputStreamReaderAnonymousInnerClass2(this, zipFile.getInputStream(entry), Charset, file)
						, file.length());
					}
					else if ( magic == Magic.Gzip )
					{ // GZIP file. GZIP isn't an archive like ZIP, so this is purely data that is compressed.
						 // Although a very common way of compressing with GZIP is to use TAR which can combine many
						 // files into one blob, which is then compressed. If that's the case then
						 // the data will look like garbage and the reader will fail for whatever it will be used for.
						 // TODO add tar support
						 GZIPInputStream zipStream = new GZIPInputStream( new FileStream( file, FileMode.Open, FileAccess.Read ) );
						 return wrap(new InputStreamReaderAnonymousInnerClass3(this, zipStream, Charset, file)
						, file.length());
					}
					else
					{
						 Stream @in = new FileStream( file, FileMode.Open, FileAccess.Read );
						 Charset usedCharset = this.Charset;
						 if ( magic.ImpliesEncoding() )
						 {
							  // Read (and skip) the magic (BOM in this case) from the file we're returning out
							  @in.skip( magic.Length() );
							  usedCharset = magic.Encoding();
						 }
						 return wrap(new InputStreamReaderAnonymousInnerClass4(this, @in, usedCharset, file)
						, file.length());
					}
			  }

			  private class InputStreamReaderAnonymousInnerClass2 : StreamReader
			  {
				  private readonly FromFile _outerInstance;

				  private File _file;

				  public InputStreamReaderAnonymousInnerClass2( FromFile outerInstance, UnknownType getInputStream, Charset charset, File file ) : base( getInputStream, charset )
				  {
					  this.outerInstance = outerInstance;
					  this._file = file;
				  }

				  public override string ToString()
				  {
						return _file.Path;
				  }
			  }

			  private class InputStreamReaderAnonymousInnerClass3 : StreamReader
			  {
				  private readonly FromFile _outerInstance;

				  private File _file;

				  public InputStreamReaderAnonymousInnerClass3( FromFile outerInstance, GZIPInputStream zipStream, Charset charset, File file ) : base( zipStream, charset )
				  {
					  this.outerInstance = outerInstance;
					  this._file = file;
				  }

				  public override string ToString()
				  {
						return _file.Path;
				  }
			  }

			  private class InputStreamReaderAnonymousInnerClass4 : StreamReader
			  {
				  private readonly FromFile _outerInstance;

				  private File _file;

				  public InputStreamReaderAnonymousInnerClass4( FromFile outerInstance, Stream @in, Charset usedCharset, File file ) : base( @in, usedCharset )
				  {
					  this.outerInstance = outerInstance;
					  this._file = file;
				  }

				  public override string ToString()
				  {
						return _file.Path;
				  }
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.zip.ZipEntry getSingleSuitableEntry(java.util.zip.ZipFile zipFile) throws java.io.IOException
			  internal virtual ZipEntry GetSingleSuitableEntry( ZipFile zipFile )
			  {
					IList<string> unsuitableEntries = new List<string>();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Iterator<? extends java.util.zip.ZipEntry> enumeration = zipFile.entries();
					IEnumerator<ZipEntry> enumeration = zipFile.entries();
					ZipEntry found = null;
					while ( enumeration.MoveNext() )
					{
						 ZipEntry entry = enumeration.Current;
						 if ( entry.Directory || InvalidZipEntry( entry.Name ) )
						 {
							  unsuitableEntries.Add( entry.Name );
							  continue;
						 }

						 if ( found != null )
						 {
							  throw new IOException( "Multiple suitable files found in zip file " + zipFile.Name + ", at least " + found.Name + " and " + entry.Name + ". Only a single file per zip file is supported" );
						 }
						 found = entry;
					}

					if ( found == null )
					{
						 throw new IOException( "No suitable file found in zip file " + zipFile.Name + "." + ( unsuitableEntries.Count > 0 ? " Although found these unsuitable entries " + unsuitableEntries : "" ) );
					}
					return found;
			  }
		 }

		 private static bool InvalidZipEntry( string name )
		 {
			  return name.Contains( "__MACOSX" ) || name.StartsWith( ".", StringComparison.Ordinal ) || name.Contains( "/." );
		 }

		 public static RawIterator<CharReadable, IOException> IndividualFiles( Charset charset, params File[] files )
		 {
			  return Iterator( new FromFile( charset ), files );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static CharReadable files(java.nio.charset.Charset charset, java.io.File... files) throws java.io.IOException
		 public static CharReadable Files( Charset charset, params File[] files )
		 {
			  IOFunction<File, CharReadable> opener = new FromFile( charset );
			  switch ( Files.Length )
			  {
			  case 0:
				  return EMPTY;
			  case 1:
				  return opener.apply( files[0] );
			  default:
				  return new MultiReadable( Iterator( opener, files ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs public static <IN,OUT> org.neo4j.collection.RawIterator<OUT,java.io.IOException> iterator(org.neo4j.function.ThrowingFunction<IN,OUT,java.io.IOException> converter, IN... items)
		 public static RawIterator<OUT, IOException> Iterator<IN, OUT>( ThrowingFunction<IN, OUT, IOException> converter, params IN[] items )
		 {
			  if ( items.Length == 0 )
			  {
					throw new System.InvalidOperationException( "No source items specified" );
			  }

			  return new RawIteratorAnonymousInnerClass( converter, items );
		 }

		 private class RawIteratorAnonymousInnerClass : RawIterator<OUT, IOException>
		 {
			 private ThrowingFunction<IN, OUT, IOException> _converter;
			 private IN[] _items;

			 public RawIteratorAnonymousInnerClass( ThrowingFunction<IN, OUT, IOException> converter, IN[] items )
			 {
				 this._converter = converter;
				 this._items = items;
			 }

			 private int cursor;

			 public bool hasNext()
			 {
				  return cursor < _items.Length;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public OUT next() throws java.io.IOException
			 public OUT next()
			 {
				  if ( !hasNext() )
				  {
						throw new System.InvalidOperationException();
				  }
				  return _converter.apply( _items[cursor++] );
			 }

			 public void remove()
			 {
				  throw new System.NotSupportedException();
			 }
		 }

		 /// <summary>
		 /// Extracts the first line, i.e characters until the first newline or end of stream.
		 /// Reads one character at a time to be sure not to read too far ahead. The stream is left
		 /// in a state of either exhausted or at the beginning of the next line of data.
		 /// </summary>
		 /// <param name="source"> <seealso cref="CharReadable"/> to read from. </param>
		 /// <returns> char[] containing characters until the first newline character or end of stream. </returns>
		 /// <exception cref="IOException"> on I/O reading error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static char[] extractFirstLineFrom(CharReadable source) throws java.io.IOException
		 public static char[] ExtractFirstLineFrom( CharReadable source )
		 {
			  char[] result = new char[100];
			  int cursor = 0;
			  int read;
			  bool foundEol = false;
			  do
			  {
					// Grow on demand
					if ( cursor >= result.Length )
					{
						 result = Arrays.copyOf( result, cursor * 2 );
					}

					// Read one character
					read = source.Read( result, cursor, 1 );
					if ( read > 0 )
					{
						 foundEol = BufferedCharSeeker.IsEolChar( result[cursor] );
						 if ( !foundEol )
						 {
							  cursor++;
						 }
					}
			  } while ( read > 0 && !foundEol );
			  return Arrays.copyOf( result, cursor );
		 }
	}

}