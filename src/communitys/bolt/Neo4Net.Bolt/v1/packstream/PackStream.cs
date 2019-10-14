using System.Diagnostics;

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
namespace Neo4Net.Bolt.v1.packstream
{

	using StructType = Neo4Net.Bolt.messaging.StructType;
	using UTF8Encoder = Neo4Net.Bolt.v1.packstream.utf8.UTF8Encoder;

	/// <summary>
	/// PackStream is a messaging serialisation format heavily inspired by MessagePack.
	/// The key differences are in the type system itself which (among other things) replaces extensions with structures.
	/// The Packer and Unpacker implementations are also faster than their MessagePack counterparts.
	/// <p/>
	/// Note that several marker byte values are RESERVED for future use.
	/// Extra markers should <em>not</em> be added casually and such additions must be follow a strict process involving both
	/// client and server software.
	/// <p/>
	/// The table below shows all allocated marker byte values.
	/// <p/>
	/// <table>
	/// <tr><th>Marker</th><th>Binary</th><th>Type</th><th>Description</th></tr>
	/// <tr><td><code>00..7F</code></td><td><code>0xxxxxxx</code></td><td>+TINY_INT</td><td>Integer 0 to 127</td></tr>
	/// <tr><td><code>80..8F</code></td><td><code>1000xxxx</code></td><td>TINY_STRING</td><td></td></tr>
	/// <tr><td><code>90..9F</code></td><td><code>1001xxxx</code></td><td>TINY_LIST</td><td></td></tr>
	/// <tr><td><code>A0..AF</code></td><td><code>1010xxxx</code></td><td>TINY_MAP</td><td></td></tr>
	/// <tr><td><code>B0..BF</code></td><td><code>1011xxxx</code></td><td>TINY_STRUCT</td><td></td></tr>
	/// <tr><td><code>C0</code></td><td><code>11000000</code></td><td>NULL</td><td></td></tr>
	/// <tr><td><code>C1</code></td><td><code>11000001</code></td><td>FLOAT_64</td><td>64-bit floating point number
	/// (double)</td></tr>
	/// <tr><td><code>C2</code></td><td><code>11000010</code></td><td>FALSE</td><td>Boolean false</td></tr>
	/// <tr><td><code>C3</code></td><td><code>11000011</code></td><td>TRUE</td><td>Boolean true</td></tr>
	/// <tr><td><code>C4..C7</code></td><td><code>110001xx</code></td><td><em>RESERVED</em></td><td></td></tr>
	/// <tr><td><code>C8</code></td><td><code>11001000</code></td><td>INT_8</td><td>8-bit signed integer</td></tr>
	/// <tr><td><code>C9</code></td><td><code>11001001</code></td><td>INT_8</td><td>16-bit signed integer</td></tr>
	/// <tr><td><code>CA</code></td><td><code>11001010</code></td><td>INT_8</td><td>32-bit signed integer</td></tr>
	/// <tr><td><code>CB</code></td><td><code>11001011</code></td><td>INT_8</td><td>64-bit signed integer</td></tr>
	/// <tr><td><code>CC</code></td><td><code>11001100</code></td><td>BYTES_8</td><td>Byte string (fewer than 2<sup>8</sup>
	/// bytes)</td></tr>
	/// <tr><td><code>CD</code></td><td><code>11001101</code></td><td>BYTES_16</td><td>Byte string (fewer than 2<sup>16</sup>
	/// bytes)</td></tr>
	/// <tr><td><code>CE</code></td><td><code>11001110</code></td><td>BYTES_32</td><td>Byte string (fewer than 2<sup>32</sup>
	/// bytes)</td></tr>
	/// <tr><td><code>CF</code></td><td><code>11001111</code></td><td><em>RESERVED</em></td><td></td></tr>
	/// <tr><td><code>D0</code></td><td><code>11010000</code></td><td>STRING_8</td><td>UTF-8 encoded string (fewer than
	/// 2<sup>8</sup> bytes)</td></tr>
	/// <tr><td><code>D1</code></td><td><code>11010001</code></td><td>STRING_16</td><td>UTF-8 encoded string (fewer than
	/// 2<sup>16</sup> bytes)</td></tr>
	/// <tr><td><code>D2</code></td><td><code>11010010</code></td><td>STRING_32</td><td>UTF-8 encoded string (fewer than
	/// 2<sup>32</sup> bytes)</td></tr>
	/// <tr><td><code>D3</code></td><td><code>11010011</code></td><td><em>RESERVED</em></td><td></td></tr>
	/// <tr><td><code>D4</code></td><td><code>11010100</code></td><td>LIST_8</td><td>List (fewer than 2<sup>8</sup>
	/// items)</td></tr>
	/// <tr><td><code>D5</code></td><td><code>11010101</code></td><td>LIST_16</td><td>List (fewer than 2<sup>16</sup>
	/// items)</td></tr>
	/// <tr><td><code>D6</code></td><td><code>11010110</code></td><td>LIST_32</td><td>List (fewer than 2<sup>32</sup>
	/// items)</td></tr>
	/// <tr><td><code>D7</code></td><td><code>11010111</code></td><td><em>RESERVED</em></td><td></td></tr>
	/// <tr><td><code>D8</code></td><td><code>11011000</code></td><td>MAP_8</td><td>Map (fewer than 2<sup>8</sup> key:value
	/// pairs)</td></tr>
	/// <tr><td><code>D9</code></td><td><code>11011001</code></td><td>MAP_16</td><td>Map (fewer than 2<sup>16</sup> key:value
	/// pairs)</td></tr>
	/// <tr><td><code>DA</code></td><td><code>11011010</code></td><td>MAP_32</td><td>Map (fewer than 2<sup>32</sup> key:value
	/// pairs)</td></tr>
	/// <tr><td><code>DB</code></td><td><code>11011011</code></td><td><em>RESERVED</em></td><td></td></tr>
	/// <tr><td><code>DC</code></td><td><code>11011100</code></td><td>STRUCT_8</td><td>Structure (fewer than 2<sup>8</sup>
	/// fields)</td></tr>
	/// <tr><td><code>DD</code></td><td><code>11011101</code></td><td>STRUCT_16</td><td>Structure (fewer than 2<sup>16</sup>
	/// fields)</td></tr>
	/// <tr><td><code>DE</code></td><td><code>11011110</code></td><td>STRUCT_32</td><td>Structure (fewer than 2<sup>32</sup>
	/// fields)</td></tr>
	/// <tr><td><code>DF</code></td><td><code>11011111</code></td><td><em>RESERVED</em></td><td></td></tr>
	/// <tr><td><code>E0..EF</code></td><td><code>1110xxxx</code></td><td><em>RESERVED</em></td><td></td></tr>
	/// <tr><td><code>F0..FF</code></td><td><code>1111xxxx</code></td><td>-TINY_INT</td><td>Integer -1 to -16</td></tr>
	/// </table>
	/// </summary>
	public class PackStream
	{

		 public static readonly sbyte TinyString = unchecked( ( sbyte ) 0x80 );
		 public static readonly sbyte TinyList = unchecked( ( sbyte ) 0x90 );
		 public static readonly sbyte TinyMap = unchecked( ( sbyte ) 0xA0 );
		 public static readonly sbyte TinyStruct = unchecked( ( sbyte ) 0xB0 );
		 public static readonly sbyte Null = unchecked( ( sbyte ) 0xC0 );
		 public static readonly sbyte Float_64 = unchecked( ( sbyte ) 0xC1 );
		 public static readonly sbyte False = unchecked( ( sbyte ) 0xC2 );
		 public static readonly sbyte True = unchecked( ( sbyte ) 0xC3 );
		 public static readonly sbyte ReservedC4 = unchecked( ( sbyte ) 0xC4 );
		 public static readonly sbyte ReservedC5 = unchecked( ( sbyte ) 0xC5 );
		 public static readonly sbyte ReservedC6 = unchecked( ( sbyte ) 0xC6 );
		 public static readonly sbyte ReservedC7 = unchecked( ( sbyte ) 0xC7 );
		 public static readonly sbyte Int_8 = unchecked( ( sbyte ) 0xC8 );
		 public static readonly sbyte Int_16 = unchecked( ( sbyte ) 0xC9 );
		 public static readonly sbyte Int_32 = unchecked( ( sbyte ) 0xCA );
		 public static readonly sbyte Int_64 = unchecked( ( sbyte ) 0xCB );
		 public static readonly sbyte Bytes_8 = unchecked( ( sbyte ) 0xCC );
		 public static readonly sbyte Bytes_16 = unchecked( ( sbyte ) 0xCD );
		 public static readonly sbyte Bytes_32 = unchecked( ( sbyte ) 0xCE );
		 public static readonly sbyte ReservedCf = unchecked( ( sbyte ) 0xCF );
		 public static readonly sbyte String_8 = unchecked( ( sbyte ) 0xD0 );
		 public static readonly sbyte String_16 = unchecked( ( sbyte ) 0xD1 );
		 public static readonly sbyte String_32 = unchecked( ( sbyte ) 0xD2 );
		 public static readonly sbyte ReservedD3 = unchecked( ( sbyte ) 0xD3 );
		 public static readonly sbyte List_8 = unchecked( ( sbyte ) 0xD4 );
		 public static readonly sbyte List_16 = unchecked( ( sbyte ) 0xD5 );
		 public static readonly sbyte List_32 = unchecked( ( sbyte ) 0xD6 );
		 public static readonly sbyte ListStream = unchecked( ( sbyte ) 0xD7 );
		 public static readonly sbyte Map_8 = unchecked( ( sbyte ) 0xD8 );
		 public static readonly sbyte Map_16 = unchecked( ( sbyte ) 0xD9 );
		 public static readonly sbyte Map_32 = unchecked( ( sbyte ) 0xDA );
		 public static readonly sbyte MapStream = unchecked( ( sbyte ) 0xDB );
		 public static readonly sbyte Struct_8 = unchecked( ( sbyte ) 0xDC );
		 public static readonly sbyte Struct_16 = unchecked( ( sbyte ) 0xDD );
		 public static readonly sbyte ReservedDe = unchecked( ( sbyte ) 0xDE );
		 public static readonly sbyte EndOfStream = unchecked( ( sbyte ) 0xDF );
		 public static readonly sbyte ReservedE0 = unchecked( ( sbyte ) 0xE0 );
		 public static readonly sbyte ReservedE1 = unchecked( ( sbyte ) 0xE1 );
		 public static readonly sbyte ReservedE2 = unchecked( ( sbyte ) 0xE2 );
		 public static readonly sbyte ReservedE3 = unchecked( ( sbyte ) 0xE3 );
		 public static readonly sbyte ReservedE4 = unchecked( ( sbyte ) 0xE4 );
		 public static readonly sbyte ReservedE5 = unchecked( ( sbyte ) 0xE5 );
		 public static readonly sbyte ReservedE6 = unchecked( ( sbyte ) 0xE6 );
		 public static readonly sbyte ReservedE7 = unchecked( ( sbyte ) 0xE7 );
		 public static readonly sbyte ReservedE8 = unchecked( ( sbyte ) 0xE8 );
		 public static readonly sbyte ReservedE9 = unchecked( ( sbyte ) 0xE9 );
		 public static readonly sbyte ReservedEa = unchecked( ( sbyte ) 0xEA );
		 public static readonly sbyte ReservedEb = unchecked( ( sbyte ) 0xEB );
		 public static readonly sbyte ReservedEc = unchecked( ( sbyte ) 0xEC );
		 public static readonly sbyte ReservedEd = unchecked( ( sbyte ) 0xED );
		 public static readonly sbyte ReservedEe = unchecked( ( sbyte ) 0xEE );
		 public static readonly sbyte ReservedEf = unchecked( ( sbyte ) 0xEF );

		 public const long UNKNOWN_SIZE = -1;

		 private const long PLUS_2_TO_THE_31 = 2147483648L;
		 private const long PLUS_2_TO_THE_15 = 32768L;
		 private const long PLUS_2_TO_THE_7 = 128L;
		 private const long MINUS_2_TO_THE_4 = -16L;
		 private const long MINUS_2_TO_THE_7 = -128L;
		 private const long MINUS_2_TO_THE_15 = -32768L;
		 private const long MINUS_2_TO_THE_31 = -2147483648L;

		 private PackStream()
		 {
		 }

		 private static PackType Type( sbyte markerByte )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte markerHighNibble = (byte)(markerByte & 0xF0);
			  sbyte markerHighNibble = unchecked( ( sbyte )( markerByte & 0xF0 ) );

			  switch ( markerHighNibble )
			  {
			  case TinyString:
					return PackType.String;
			  case TinyList:
					return PackType.List;
			  case TinyMap:
					return PackType.Map;
			  case TinyStruct:
					return PackType.Struct;
			  default:
					break;
			  }

			  if ( markerByte >= MINUS_2_TO_THE_4 )
			  {
					return PackType.Integer;
			  }

			  switch ( markerByte )
			  {
			  case Null:
					return PackType.Null;
			  case True:
			  case False:
					return PackType.Boolean;
			  case Float_64:
					return PackType.Float;
			  case Bytes_8:
			  case Bytes_16:
			  case Bytes_32:
					return PackType.Bytes;
			  case String_8:
			  case String_16:
			  case String_32:
					return PackType.String;
			  case List_8:
			  case List_16:
			  case List_32:
			  case ListStream:
					return PackType.List;
			  case Map_8:
			  case Map_16:
			  case Map_32:
			  case MapStream:
					return PackType.Map;
			  case Struct_8:
			  case Struct_16:
					return PackType.Struct;
			  case EndOfStream:
					return PackType.EndOfStream;
			  case Int_8:
			  case Int_16:
			  case Int_32:
			  case Int_64:
					return PackType.Integer;
			  default:
					return PackType.Reserved;
			  }
		 }

		 public class Packer
		 {
			  internal static readonly char PackedCharStartChar = ( char ) 32;
			  internal static readonly char PackedCharEndChar = ( char ) 126;
			  internal static readonly string[] PackedChars = PrePackChars();
			  internal UTF8Encoder Utf8 = UTF8Encoder.fastestAvailableEncoder();

			  protected internal PackOutput Out;

			  public Packer( PackOutput @out )
			  {
					this.Out = @out;
			  }

			  internal static string[] PrePackChars()
			  {
					int size = PackedCharEndChar + 1 - PackedCharStartChar;
					string[] packedChars = new string[size];
					for ( int i = 0; i < size; i++ )
					{
						 packedChars[i] = ( ( char )( i + PackedCharStartChar ) ).ToString();
					}
					return packedChars;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void flush() throws java.io.IOException
			  public virtual void Flush()
			  {
					Out.flush();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void packNull() throws java.io.IOException
			  public virtual void PackNull()
			  {
					Out.writeByte( Null );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void pack(boolean value) throws java.io.IOException
			  public virtual void Pack( bool value )
			  {
					Out.writeByte( value ? True : False );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void pack(long value) throws java.io.IOException
			  public virtual void Pack( long value )
			  {
					if ( value >= MINUS_2_TO_THE_4 && value < PLUS_2_TO_THE_7 )
					{
						 Out.writeByte( ( sbyte ) value );
					}
					else if ( value >= MINUS_2_TO_THE_7 && value < MINUS_2_TO_THE_4 )
					{
						 Out.writeByte( Int_8 ).writeByte( ( sbyte ) value );
					}
					else if ( value >= MINUS_2_TO_THE_15 && value < PLUS_2_TO_THE_15 )
					{
						 Out.writeByte( Int_16 ).writeShort( ( short ) value );
					}
					else if ( value >= MINUS_2_TO_THE_31 && value < PLUS_2_TO_THE_31 )
					{
						 Out.writeByte( Int_32 ).writeInt( ( int ) value );
					}
					else
					{
						 Out.writeByte( Int_64 ).writeLong( value );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void pack(double value) throws java.io.IOException
			  public virtual void Pack( double value )
			  {
					Out.writeByte( Float_64 ).writeDouble( value );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void pack(char character) throws java.io.IOException
			  public virtual void Pack( char character )
			  {
					if ( character >= PackedCharStartChar && character <= PackedCharEndChar )
					{
						 Pack( PackedChars[character - PackedCharStartChar] );
					}
					else
					{
						 Pack( character.ToString() );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void pack(byte[] value) throws java.io.IOException
			  public virtual void Pack( sbyte[] value )
			  {
					if ( value == null )
					{
						 PackNull();
					}
					else
					{
						 PackBytesHeader( value.Length );
						 Out.writeBytes( value, 0, value.Length );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void pack(String value) throws java.io.IOException
			  public virtual void Pack( string value )
			  {
					if ( string.ReferenceEquals( value, null ) )
					{
						 PackNull();
					}
					else
					{
						 ByteBuffer encoded = Utf8.encode( value );
						 PackStringHeader( encoded.remaining() );
						 Out.writeBytes( encoded );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void packUTF8(byte[] bytes, int offset, int length) throws java.io.IOException
			  public virtual void PackUTF8( sbyte[] bytes, int offset, int length )
			  {
					if ( bytes == null )
					{
						 PackNull();
					}
					else
					{
						 PackStringHeader( length );
						 Out.writeBytes( bytes, offset, length );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void packBytesHeader(int size) throws java.io.IOException
			  protected internal virtual void PackBytesHeader( int size )
			  {
					PackHeader( size, Bytes_8, Bytes_16, Bytes_32 );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void packStringHeader(int size) throws java.io.IOException
			  internal virtual void PackStringHeader( int size )
			  {
					PackHeader( size, TinyString, String_8, String_16, String_32 );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void packListHeader(int size) throws java.io.IOException
			  public virtual void PackListHeader( int size )
			  {
					PackHeader( size, TinyList, List_8, List_16, List_32 );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void packListStreamHeader() throws java.io.IOException
			  public virtual void PackListStreamHeader()
			  {
					Out.writeByte( ListStream );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void packMapHeader(int size) throws java.io.IOException
			  public virtual void PackMapHeader( int size )
			  {
					PackHeader( size, TinyMap, Map_8, Map_16, Map_32 );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void packMapStreamHeader() throws java.io.IOException
			  public virtual void PackMapStreamHeader()
			  {
					Out.writeByte( MapStream );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void packStructHeader(int size, byte signature) throws java.io.IOException
			  public virtual void PackStructHeader( int size, sbyte signature )
			  {
					if ( size < 0x10 )
					{
						 Out.writeShort( ( short )( ( sbyte )( TinyStruct | size ) << 8 | ( signature & 0xFF ) ) );
					}
					else if ( size <= sbyte.MaxValue )
					{
						 Out.writeByte( Struct_8 ).writeByte( ( sbyte ) size ).writeByte( signature );
					}
					else if ( size <= short.MaxValue )
					{
						 Out.writeByte( Struct_16 ).writeShort( ( short ) size ).writeByte( signature );
					}
					else
					{
						 throw new Overflow( "Structures cannot have more than " + short.MaxValue + " fields" );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void packEndOfStream() throws java.io.IOException
			  public virtual void PackEndOfStream()
			  {
					Out.writeByte( EndOfStream );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void packHeader(int size, byte marker8, byte marker16, byte marker32) throws java.io.IOException
			  internal virtual void PackHeader( int size, sbyte marker8, sbyte marker16, sbyte marker32 )
			  {
					/*
					* The code here is on purpose to test against the maximum value of a signed byte rather than a unsigned byte.
					* We pack values that in range 2^7 ~ 2^8-1 with marker16 instead of marker8
					* to prevent us from breaking any clients that are reading this size as a signed value.
					* Similar case applies to Short.MAX_VALUE
					* */
					if ( size <= sbyte.MaxValue )
					{
						 Out.writeShort( ( short )( marker8 << 8 | size ) );
					}
					else if ( size <= short.MaxValue )
					{
						 Out.writeByte( marker16 ).writeShort( ( short ) size );
					}
					else
					{
						 Out.writeByte( marker32 ).writeInt( size );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void packHeader(int size, byte marker4, byte marker8, byte marker16, byte marker32) throws java.io.IOException
			  internal virtual void PackHeader( int size, sbyte marker4, sbyte marker8, sbyte marker16, sbyte marker32 )
			  {
					if ( size < 0x10 )
					{
						 Out.writeByte( ( sbyte )( marker4 | size ) );
					}
					else
					{
						 PackHeader( size, marker8, marker16, marker32 );
					}
			  }
		 }

		 public class Unpacker
		 {
			  internal static readonly sbyte[] EmptyByteArray = new sbyte[] {};

			  protected internal PackInput In;

			  public Unpacker( PackInput @in )
			  {
					this.In = @in;
			  }

			  // TODO: This currently returns the number of fields in the struct. In 99% of cases we will look at the struct
			  // signature to determine how to read it, suggest we make that what we return here,
			  // and have the number of fields available through some alternate optional mechanism.
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long unpackStructHeader() throws java.io.IOException
			  public virtual long UnpackStructHeader()
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte markerByte = in.readByte();
					sbyte markerByte = In.readByte();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte markerHighNibble = (byte)(markerByte & 0xF0);
					sbyte markerHighNibble = unchecked( ( sbyte )( markerByte & 0xF0 ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte markerLowNibble = (byte)(markerByte & 0x0F);
					sbyte markerLowNibble = ( sbyte )( markerByte & 0x0F );

					if ( markerHighNibble == TinyStruct )
					{
						 return markerLowNibble;
					}
					switch ( markerByte )
					{
					case Struct_8:
						 return UnpackUINT8();
					case Struct_16:
						 return UnpackUINT16();
					default:
						 throw new Unexpected( PackType.Struct, markerByte );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public char unpackStructSignature() throws java.io.IOException
			  public virtual char UnpackStructSignature()
			  {
					return ( char ) In.readByte();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long unpackListHeader() throws java.io.IOException
			  public virtual long UnpackListHeader()
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte markerByte = in.readByte();
					sbyte markerByte = In.readByte();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte markerHighNibble = (byte)(markerByte & 0xF0);
					sbyte markerHighNibble = unchecked( ( sbyte )( markerByte & 0xF0 ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte markerLowNibble = (byte)(markerByte & 0x0F);
					sbyte markerLowNibble = ( sbyte )( markerByte & 0x0F );

					if ( markerHighNibble == TinyList )
					{
						 return markerLowNibble;
					}
					switch ( markerByte )
					{
					case List_8:
						 return UnpackUINT8();
					case List_16:
						 return UnpackUINT16();
					case List_32:
						 return UnpackUINT32( PackType.List );
					case ListStream:
						 return UNKNOWN_SIZE;
					default:
						 throw new Unexpected( PackType.List, markerByte );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long unpackMapHeader() throws java.io.IOException
			  public virtual long UnpackMapHeader()
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte markerByte = in.readByte();
					sbyte markerByte = In.readByte();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte markerHighNibble = (byte)(markerByte & 0xF0);
					sbyte markerHighNibble = unchecked( ( sbyte )( markerByte & 0xF0 ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte markerLowNibble = (byte)(markerByte & 0x0F);
					sbyte markerLowNibble = ( sbyte )( markerByte & 0x0F );

					if ( markerHighNibble == TinyMap )
					{
						 return markerLowNibble;
					}
					switch ( markerByte )
					{
					case Map_8:
						 return UnpackUINT8();
					case Map_16:
						 return UnpackUINT16();
					case Map_32:
						 return UnpackUINT32( PackType.Map );
					case MapStream:
						 return UNKNOWN_SIZE;
					default:
						 throw new Unexpected( PackType.Map, markerByte );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int unpackInteger() throws java.io.IOException
			  public virtual int UnpackInteger()
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte markerByte = in.readByte();
					sbyte markerByte = In.readByte();
					if ( markerByte >= MINUS_2_TO_THE_4 )
					{
						 return markerByte;
					}
					switch ( markerByte )
					{
					case Int_8:
						 return In.readByte();
					case Int_16:
						 return In.readShort();
					case Int_32:
						 return In.readInt();
					case Int_64:
						 throw new Overflow( "Unexpectedly large Integer value unpacked (" + In.readLong() + ")" );
					default:
						 throw new Unexpected( PackType.Integer, markerByte );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long unpackLong() throws java.io.IOException
			  public virtual long UnpackLong()
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte markerByte = in.readByte();
					sbyte markerByte = In.readByte();
					if ( markerByte >= MINUS_2_TO_THE_4 )
					{
						 return markerByte;
					}
					switch ( markerByte )
					{
					case Int_8:
						 return In.readByte();
					case Int_16:
						 return In.readShort();
					case Int_32:
						 return In.readInt();
					case Int_64:
						 return In.readLong();
					default:
						 throw new Unexpected( PackType.Integer, markerByte );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public double unpackDouble() throws java.io.IOException
			  public virtual double UnpackDouble()
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte markerByte = in.readByte();
					sbyte markerByte = In.readByte();
					if ( markerByte == Float_64 )
					{
						 return In.readDouble();
					}
					throw new Unexpected( PackType.Float, markerByte );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public byte[] unpackBytes() throws java.io.IOException
			  public virtual sbyte[] UnpackBytes()
			  {
					int size = UnpackBytesHeader();
					return UnpackRawBytes( size );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public String unpackString() throws java.io.IOException
			  public virtual string UnpackString()
			  {
					return StringHelper.NewString( UnpackUTF8(), StandardCharsets.UTF_8 );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int unpackBytesHeader() throws java.io.IOException
			  public virtual int UnpackBytesHeader()
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte markerByte = in.readByte();
					sbyte markerByte = In.readByte();
					int size;
					switch ( markerByte )
					{
					case Bytes_8:
						 size = UnpackUINT8();
						 break;
					case Bytes_16:
						 size = UnpackUINT16();
						 break;
					case Bytes_32:
					{
						 size = UnpackUINT32( PackType.Bytes );
						 break;
					}
					default:
						 throw new Unexpected( PackType.Bytes, markerByte );
					}
					return size;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int unpackStringHeader() throws java.io.IOException
			  public virtual int UnpackStringHeader()
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte markerByte = in.readByte();
					sbyte markerByte = In.readByte();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte markerHighNibble = (byte)(markerByte & 0xF0);
					sbyte markerHighNibble = unchecked( ( sbyte )( markerByte & 0xF0 ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte markerLowNibble = (byte)(markerByte & 0x0F);
					sbyte markerLowNibble = ( sbyte )( markerByte & 0x0F );

					int size;

					if ( markerHighNibble == TinyString )
					{
						 size = markerLowNibble;
					}
					else
					{
						 switch ( markerByte )
						 {
						 case String_8:
							  size = UnpackUINT8();
							  break;
						 case String_16:
							  size = UnpackUINT16();
							  break;
						 case String_32:
						 {
							  size = UnpackUINT32( PackType.String );
							  break;
						 }
						 default:
							  throw new Unexpected( PackType.String, markerByte );
						 }
					}

					return size;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public byte[] unpackUTF8() throws java.io.IOException
			  public virtual sbyte[] UnpackUTF8()
			  {
					int size = UnpackStringHeader();
					return UnpackRawBytes( size );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean unpackBoolean() throws java.io.IOException
			  public virtual bool UnpackBoolean()
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte markerByte = in.readByte();
					sbyte markerByte = In.readByte();
					switch ( markerByte )
					{
					case True:
						 return true;
					case False:
						 return false;
					default:
						 throw new Unexpected( PackType.Boolean, markerByte );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void unpackNull() throws java.io.IOException
			  public virtual void UnpackNull()
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte markerByte = in.readByte();
					sbyte markerByte = In.readByte();
					Debug.Assert( markerByte == Null );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int unpackUINT8() throws java.io.IOException
			  internal virtual int UnpackUINT8()
			  {
					return In.readByte() & 0xFF;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int unpackUINT16() throws java.io.IOException
			  internal virtual int UnpackUINT16()
			  {
					return In.readShort() & 0xFFFF;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long unpackUINT32() throws java.io.IOException
			  internal virtual long UnpackUINT32()
			  {
					return In.readInt() & 0xFFFFFFFFL;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int unpackUINT32(PackType type) throws java.io.IOException
			  internal virtual int UnpackUINT32( PackType type )
			  {
					long longSize = UnpackUINT32();
					if ( longSize <= int.MaxValue )
					{
						 return ( int ) longSize;
					}
					else
					{
						 throw new Overflow( string.Format( "{0}_32 too long for Java", type ) );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void unpackEndOfStream() throws java.io.IOException
			  public virtual void UnpackEndOfStream()
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte markerByte = in.readByte();
					sbyte markerByte = In.readByte();
					Debug.Assert( markerByte == EndOfStream );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private byte[] unpackRawBytes(int size) throws java.io.IOException
			  internal virtual sbyte[] UnpackRawBytes( int size )
			  {
					if ( size == 0 )
					{
						 return EmptyByteArray;
					}
					else
					{
						 sbyte[] heapBuffer = new sbyte[size];
						 UnpackRawBytesInto( heapBuffer, 0, heapBuffer.Length );
						 return heapBuffer;
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void unpackRawBytesInto(byte[] buffer, int offset, int size) throws java.io.IOException
			  internal virtual void UnpackRawBytesInto( sbyte[] buffer, int offset, int size )
			  {
					if ( size > 0 )
					{
						 In.readBytes( buffer, offset, size );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public PackType peekNextType() throws java.io.IOException
			  public virtual PackType PeekNextType()
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte markerByte = in.peekByte();
					sbyte markerByte = In.peekByte();
					return Type( markerByte );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void ensureCorrectStructSize(org.neo4j.bolt.messaging.StructType structType, int expected, long actual) throws java.io.IOException
			  public static void EnsureCorrectStructSize( StructType structType, int expected, long actual )
			  {
					if ( expected != actual )
					{
						 throw new PackStreamException( string.Format( "Invalid message received, serialized {0} structures should have {1:D} fields, " + "received {2} structure has {3:D} fields.", structType.description(), expected, structType.description(), actual ) );
					}
			  }
		 }

		 public class PackStreamException : IOException
		 {
			  public PackStreamException( string message ) : base( message )
			  {
			  }
		 }

		 public class EndOfStream : PackStreamException
		 {
			  public EndOfStream( string message ) : base( message )
			  {
			  }
		 }

		 public class Overflow : PackStreamException
		 {
			  public Overflow( string message ) : base( message )
			  {
			  }
		 }

		 public class Unexpected : PackStreamException
		 {
			  public Unexpected( PackType expectedType, sbyte unexpectedMarkerByte ) : base( "Wrong type received. Expected " + expectedType + ", received: " + Type( unexpectedMarkerByte ) + " uniquetempvar." )
			  {
			  }
		 }
	}

}