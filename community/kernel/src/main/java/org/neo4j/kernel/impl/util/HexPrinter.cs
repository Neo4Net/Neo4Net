using System;

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
namespace Org.Neo4j.Kernel.impl.util
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static ByteBuffer.wrap;

	/// <summary>
	/// Prints streams of bytes as hex, printed in columns and rows neatly ordered. For example:
	///  <para>
	///  @ 0x000A: FF B9 E2 5B 95 2B 69 21  CF 01 10 89 1E 05 67 51  0C 91 32 20 40 8A 4B 92  01 8C C7 93 F8 66 58 F0
	///  <br>
	///  @ 0x000B: 39 C8 F1 2B 84 3B AF 8E  C7 50 F7 82 E7 1C DB 20  BF E3 C1 08 68 12 46 72  BA 72 5F 82 13 9A C1 DF
	///  <br>
	///  @ 0x000C: 56 A6 83 85 36 25 00 DA  B4 57 02 FF E6 97 1C 69  F9 16 56 AF 78 C9 0F A4  CD A4 1F A8 08 08 3B 3B
	/// </para>
	///  <para>
	/// where number of bytes per line, number of bytes per group, byte group separator, length of line number, prefix
	/// or suffix of line number can be controlled. If the length of line number is set to a non-positive number,
	/// then no line number, prefix, or suffix will be added.
	/// 
	/// </para>
	/// </summary>
	public class HexPrinter
	{
		 private readonly PrintStream @out;
		 private int _bytesPerLine;
		 private int _bytesPerGroup;
		 private string _byteSeparator;
		 private string _groupSeparator;
		 private int _maxLineNumberDigits;
		 private string _lineNumberPrefix;
		 private string _lineNumberSuffix;

		 private long _currentLineNumber;
		 private int _bytesOnThisLine;

		 private const int DEFAULT_BYTES_PER_GROUP = 8;
		 private const int DEFAULT_GROUPS_PER_LINE = 4;
		 private const int DEFAULT_MAX_LINE_NUMBER_DIGITS = 0;
		 private const string DEFAULT_GROUP_SEPARATOR = "    ";
		 private const string DEFAULT_LINE_NUMBER_PREFIX = "@ ";
		 private const string DEFAULT_LINE_NUMBER_SUFFIX = ": ";
		 private const string DEFAULT_BYTE_SEPARATOR = " ";

		 public virtual HexPrinter WithBytesPerLine( int bytesPerLine )
		 {
			  this._bytesPerLine = bytesPerLine;
			  return this;
		 }

		 public virtual HexPrinter WithBytesPerGroup( int bytesPerGroup )
		 {
			  this._bytesPerGroup = bytesPerGroup;
			  return this;
		 }

		 public virtual HexPrinter WithGroupSeparator( string separator )
		 {
			  this._groupSeparator = separator;
			  return this;
		 }

		 public virtual HexPrinter WithLineNumberDigits( int maxLineNumberDigits )
		 {
			  this._maxLineNumberDigits = maxLineNumberDigits;
			  return this;
		 }

		 public virtual HexPrinter WithLineNumberPrefix( string prefix )
		 {
			  this._lineNumberPrefix = prefix;
			  return this;
		 }

		 public virtual HexPrinter WithLineNumberSuffix( string suffix )
		 {
			  this._lineNumberSuffix = suffix;
			  return this;
		 }

		 public virtual HexPrinter WithLineNumberOffset( long offset )
		 {
			  this._currentLineNumber = offset;
			  return this;
		 }

		 public virtual HexPrinter WithByteSeparator( string byteSeparator )
		 {
			  this._byteSeparator = byteSeparator;
			  return this;
		 }

		 public virtual HexPrinter WithBytesGroupingFormat( int bytesPerLine, int bytesPerGroup, string separator )
		 {
			  this._bytesPerLine = bytesPerLine;
			  this._bytesPerGroup = bytesPerGroup;
			  this._groupSeparator = separator;
			  return this;
		 }

		 public virtual HexPrinter WithLineNumberFormat( int maxLineNumberDigits, string prefix, string suffix )
		 {
			  this._maxLineNumberDigits = maxLineNumberDigits;
			  this._lineNumberPrefix = prefix;
			  this._lineNumberSuffix = suffix;
			  return this;
		 }

		 /// <summary>
		 /// Using no line number, 8 bytes per group, 32 bytes per line, 4-space separator as default formating to
		 /// print bytes as hex. Output looks like:
		 /// <para>
		 /// 01 02 03 04 05 06 07 08    01 02 03 04 05 06 07 08    01 02 03 04 05 06 07 08    01 02 03 04 05 06 07 08
		 /// <br>
		 /// 01 02 03 04 05 06 07 08    01 02 03 04 05 06 07 08    01 02 03 04 05 06 07 08    01 02 03 04 05 06 07 08
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="out"> </param>
		 public HexPrinter( PrintStream @out ) : this( @out, DEFAULT_BYTES_PER_GROUP, DEFAULT_GROUP_SEPARATOR )
		 {
		 }

		 public HexPrinter( PrintStream @out, int bytesPerGroup, string groupSep )
		 {
			  this.@out = @out;
			  this._bytesPerLine = DEFAULT_GROUPS_PER_LINE * bytesPerGroup;
			  this._bytesPerGroup = bytesPerGroup;
			  this._groupSeparator = groupSep;
			  this._maxLineNumberDigits = DEFAULT_MAX_LINE_NUMBER_DIGITS;
			  this._lineNumberPrefix = DEFAULT_LINE_NUMBER_PREFIX;
			  this._lineNumberSuffix = DEFAULT_LINE_NUMBER_SUFFIX;
			  this._byteSeparator = DEFAULT_BYTE_SEPARATOR;
		 }

		 /// <summary>
		 /// Append one byte into the print stream </summary>
		 /// <param name="value">
		 /// @return </param>
		 public virtual HexPrinter Append( sbyte value )
		 {
			  CheckNewLine();
			  AddHexValue( value );
			  return this;
		 }

		 /// <summary>
		 /// Append all the bytes in the channel into print stream </summary>
		 /// <param name="source">
		 /// @return </param>
		 /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public HexPrinter append(java.nio.channels.ReadableByteChannel source) throws java.io.IOException
		 public virtual HexPrinter Append( ReadableByteChannel source )
		 {
			  return Append( source, -1 );
		 }

		 /// <summary>
		 /// Append {@code atMost} count of bytes into print stream </summary>
		 /// <param name="source"> </param>
		 /// <param name="atMost">
		 /// @return </param>
		 /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public HexPrinter append(java.nio.channels.ReadableByteChannel source, int atMost) throws java.io.IOException
		 public virtual HexPrinter Append( ReadableByteChannel source, int atMost )
		 {
			  bool indefinite = atMost == -1;
			  ByteBuffer buffer = ByteBuffer.allocate( 4 * 1024 );
			  while ( true )
			  {
					buffer.clear();
					if ( !indefinite )
					{
						 buffer.limit( Math.Min( buffer.capacity(), atMost ) );
					}
					int read = source.read( buffer );
					if ( read == -1 )
					{
						 break;
					}

					atMost -= read;
					buffer.flip();
					while ( buffer.hasRemaining() )
					{
						 append( buffer.get() );
					}
			  }
			  return this;
		 }

		 /// <summary>
		 /// Append a part of byte buffer into print stream </summary>
		 /// <param name="bytes"> </param>
		 /// <param name="offset"> </param>
		 /// <param name="length">
		 /// @return </param>
		 public virtual HexPrinter Append( ByteBuffer bytes, int offset, int length )
		 {
			  for ( int i = offset; i < offset + length; i++ )
			  {
					append( bytes.get( i ) );
			  }
			  return this;
		 }

		 /// <summary>
		 /// Append the bytes in the byte buffer, from its current position to its limit into print stream. This operation
		 /// will not move the buffers current position. </summary>
		 /// <param name="bytes">
		 /// @return </param>
		 public virtual HexPrinter Append( ByteBuffer bytes )
		 {
			  return Append( bytes, bytes.position(), bytes.remaining() );
		 }

		 /// <summary>
		 /// Append the whole byte array into print stream </summary>
		 /// <param name="bytes">
		 /// @return </param>
		 public virtual HexPrinter Append( sbyte[] bytes )
		 {
			  return Append( wrap( bytes ), 0, bytes.Length );
		 }

		 private void AddHexValue( sbyte value )
		 {
			  if ( _bytesOnThisLine == 1 )
			  {
					// it is the first byte
					// out.append( NOTHING )
			  }
			  else if ( _bytesOnThisLine % _bytesPerGroup == 1 )
			  {
					// it is the first byte for a new byte group
					@out.append( _groupSeparator );
			  }
			  else
			  {
					@out.append( _byteSeparator );
			  }
			  @out.printf( "%X%X", 0xF & ( value >> 4 ), 0xF & value );
		 }

		 private void CheckNewLine()
		 {
			  if ( _bytesOnThisLine >= _bytesPerLine )
			  {
					@out.println();
					_bytesOnThisLine = 0;
					_currentLineNumber++;
			  }
			  if ( _bytesOnThisLine == 0 && _maxLineNumberDigits > 0 )
			  {
					// a new line and line number enabled
					@out.append( _lineNumberPrefix );
					@out.printf( "0x%0" + _maxLineNumberDigits + "X", _currentLineNumber );
					@out.append( _lineNumberSuffix );
			  }
			  _bytesOnThisLine++;
		 }

		 // Some static methods that could be used directly

		 /// <summary>
		 /// Convert a subsection of a byte buffer to a human readable string of nicely formatted hex numbers.
		 /// Output looks like:
		 /// 
		 /// 01 02 03 04 05 06 07 08    01 02 03 04 05 06 07 08    01 02 03 04 05 06 07 08    01 02 03 04 05 06 07 08
		 /// 01 02 03 04 05 06 07 08    01 02 03 04 05 06 07 08    01 02 03 04 05 06 07 08    01 02 03 04 05 06 07 08
		 /// </summary>
		 /// <param name="bytes"> </param>
		 /// <param name="offset"> </param>
		 /// <param name="length"> </param>
		 /// <returns> formatted hex numbers in string </returns>
		 public static string Hex( ByteBuffer bytes, int offset, int length )
		 {
			  return Hex( bytes, offset, length, DEFAULT_BYTES_PER_GROUP, DEFAULT_GROUP_SEPARATOR );
		 }

		 public static string Hex( ByteBuffer bytes, int offset, int length, int bytesPerBlock, string groupSep )
		 {
			  MemoryStream outStream = new MemoryStream();
			  PrintStream @out = new PrintStream( outStream );

			  ( new HexPrinter( @out, bytesPerBlock, groupSep ) ).Append( bytes, offset, length );
			  @out.flush();
			  return outStream.ToString();
		 }

		 /// <summary>
		 /// Convert a full byte buffer to a human readable string of nicely formatted hex numbers using default hex format.
		 /// Output looks like:
		 /// 
		 /// 01 02 03 04 05 06 07 08    01 02 03 04 05 06 07 08    01 02 03 04 05 06 07 08    01 02 03 04 05 06 07 08
		 /// 01 02 03 04 05 06 07 08    01 02 03 04 05 06 07 08    01 02 03 04 05 06 07 08    01 02 03 04 05 06 07 08
		 /// </summary>
		 /// <param name="bytes"> </param>
		 /// <returns> formatted hex numbers in string </returns>
		 public static string Hex( ByteBuffer bytes )
		 {
			  return Hex( bytes, DEFAULT_BYTES_PER_GROUP, DEFAULT_GROUP_SEPARATOR );
		 }

		 public static string Hex( ByteBuffer bytes, int bytesPerBlock, string groupSep )
		 {
			  return Hex( bytes, bytes.position(), bytes.limit(), bytesPerBlock, groupSep );
		 }

		 /// <summary>
		 /// Convert a full byte buffer to a human readable string of nicely formatted hex numbers.
		 /// Output looks like:
		 /// 
		 /// 01 02 03 04 05 06 07 08    01 02 03 04 05 06 07 08    01 02 03 04 05 06 07 08    01 02 03 04 05 06 07 08
		 /// 01 02 03 04 05 06 07 08    01 02 03 04 05 06 07 08    01 02 03 04 05 06 07 08    01 02 03 04 05 06 07 08
		 /// </summary>
		 /// <param name="bytes"> </param>
		 /// <returns> formatted hex numbers in string </returns>
		 public static string Hex( sbyte[] bytes )
		 {
			  return hex( bytes, DEFAULT_BYTES_PER_GROUP, DEFAULT_GROUP_SEPARATOR );
		 }

		 public static string Hex( sbyte[] bytes, int bytesPerBlock, string groupSep )
		 {
			  return hex( wrap( bytes ), bytesPerBlock, groupSep );
		 }

		 /// <summary>
		 /// Convert a single byte to a human-readable hex number. The number will always be two characters wide. </summary>
		 /// <param name="b"> </param>
		 /// <returns> formatted hex numbers in string </returns>
		 public static string Hex( sbyte b )
		 {
			  return string.Format( "{0:X2}", b );
		 }
	}

}