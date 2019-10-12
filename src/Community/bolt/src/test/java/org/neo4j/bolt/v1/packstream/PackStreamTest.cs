using System;
using System.Collections.Generic;

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
namespace Neo4Net.Bolt.v1.packstream
{
	using Test = org.junit.jupiter.api.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.packstream.PackType.BYTES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.packstream.PackType.LIST;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.packstream.PackType.MAP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.packstream.PackType.STRING;

	internal class PackStreamTest
	{

		 private static IDictionary<string, object> AsMap( params object[] keysAndValues )
		 {
			  IDictionary<string, object> map = new LinkedHashMap<string, object>( keysAndValues.Length / 2 );
			  string key = null;
			  foreach ( object keyOrValue in keysAndValues )
			  {
					if ( string.ReferenceEquals( key, null ) )
					{
						 key = keyOrValue.ToString();
					}
					else
					{
						 map[key] = keyOrValue;
						 key = null;
					}
			  }
			  return map;
		 }

		 private class Machine
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly MemoryStream OutputConflict;
			  internal readonly WritableByteChannel Writable;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly PackStream.Packer PackerConflict;

			  internal Machine()
			  {
					this.OutputConflict = new MemoryStream();
					this.Writable = Channels.newChannel( this.OutputConflict );
					this.PackerConflict = new PackStream.Packer( new BufferedChannelOutput( this.Writable ) );
			  }

			  internal Machine( int bufferSize )
			  {
					this.OutputConflict = new MemoryStream();
					this.Writable = Channels.newChannel( this.OutputConflict );
					this.PackerConflict = new PackStream.Packer( new BufferedChannelOutput( this.Writable, bufferSize ) );
			  }

			  public virtual void Reset()
			  {
					OutputConflict.reset();
			  }

			  public virtual sbyte[] Output()
			  {
					return OutputConflict.toByteArray();
			  }

			  public virtual PackStream.Packer Packer()
			  {
					return PackerConflict;
			  }
		 }

		 private class MachineClient
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly PackStream.Unpacker UnpackerConflict;
			  internal readonly ResetableReadableByteChannel Readable;

			  internal MachineClient( int capacity )
			  {
					Readable = new ResetableReadableByteChannel();
					BufferedChannelInput input = ( new BufferedChannelInput( capacity ) ).Reset( Readable );
					UnpackerConflict = new PackStream.Unpacker( input );
			  }

			  public virtual void Reset( sbyte[] input )
			  {
					Readable.reset( input );
			  }

			  public virtual PackStream.Unpacker Unpacker()
			  {
					return this.UnpackerConflict;
			  }
		 }

		 private class ResetableReadableByteChannel : ReadableByteChannel
		 {
			  internal sbyte[] Bytes;
			  internal int Pos;

			  public virtual void Reset( sbyte[] input )
			  {
					Bytes = input;
					Pos = 0;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int read(ByteBuffer dst) throws java.io.IOException
			  public override int Read( ByteBuffer dst )
			  {
					dst.put( Bytes );
					int read = Bytes.Length;
					Pos += read;
					return read;
			  }

			  public override bool Open
			  {
				  get
				  {
						return Pos < Bytes.Length;
				  }
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
			  public override void Close()
			  {
			  }
		 }

		 private PackStream.Unpacker NewUnpacker( sbyte[] bytes )
		 {
			  MemoryStream input = new MemoryStream( bytes );
			  return new PackStream.Unpacker( ( new BufferedChannelInput( 16 ) ).Reset( Channels.newChannel( input ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testCanPackAndUnpackNull() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestCanPackAndUnpackNull()
		 {
			  // Given
			  Machine machine = new Machine();

			  // When
			  machine.Packer().packNull();
			  machine.Packer().flush();

			  // Then
			  sbyte[] bytes = machine.Output();
			  assertThat( bytes, equalTo( new sbyte[]{ unchecked( ( sbyte ) 0xC0 ) } ) );

			  // When
			  PackStream.Unpacker unpacker = NewUnpacker( bytes );
			  unpacker.UnpackNull();

			  // Then
			  // it does not blow up
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testCanPackAndUnpackTrue() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestCanPackAndUnpackTrue()
		 {
			  // Given
			  Machine machine = new Machine();

			  // When
			  machine.Packer().pack(true);
			  machine.Packer().flush();

			  // Then
			  sbyte[] bytes = machine.Output();
			  assertThat( bytes, equalTo( new sbyte[]{ unchecked( ( sbyte ) 0xC3 ) } ) );

			  // When
			  PackStream.Unpacker unpacker = NewUnpacker( bytes );

			  // Then
			  assertThat( unpacker.UnpackBoolean(), equalTo(true) );

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testCanPackAndUnpackFalse() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestCanPackAndUnpackFalse()
		 {
			  // Given
			  Machine machine = new Machine();

			  // When
			  machine.Packer().pack(false);
			  machine.Packer().flush();

			  // Then
			  sbyte[] bytes = machine.Output();
			  assertThat( bytes, equalTo( new sbyte[]{ unchecked( ( sbyte ) 0xC2 ) } ) );

			  // When
			  PackStream.Unpacker unpacker = NewUnpacker( bytes );

			  // Then
			  assertThat( unpacker.UnpackBoolean(), equalTo(false) );

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testCanPackAndUnpackTinyIntegers() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestCanPackAndUnpackTinyIntegers()
		 {
			  // Given
			  Machine machine = new Machine();

			  for ( long i = -16; i < 128; i++ )
			  {
					// When
					machine.Reset();
					machine.Packer().pack(i);
					machine.Packer().flush();

					// Then
					sbyte[] bytes = machine.Output();
					assertThat( bytes.Length, equalTo( 1 ) );

					// When
					PackStream.Unpacker unpacker = NewUnpacker( bytes );

					// Then
					assertThat( unpacker.UnpackLong(), equalTo(i) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testCanPackAndUnpackShortIntegers() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestCanPackAndUnpackShortIntegers()
		 {
			  // Given
			  Machine machine = new Machine();

			  for ( long i = -32768; i < 32768; i++ )
			  {
					// When
					machine.Reset();
					machine.Packer().pack(i);
					machine.Packer().flush();

					// Then
					sbyte[] bytes = machine.Output();
					assertThat( bytes.Length, lessThanOrEqualTo( 3 ) );

					// When
					PackStream.Unpacker unpacker = NewUnpacker( bytes );

					// Then
					assertThat( unpacker.UnpackLong(), equalTo(i) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testCanPackAndUnpackPowersOfTwoAsIntegers() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestCanPackAndUnpackPowersOfTwoAsIntegers()
		 {
			  // Given
			  Machine machine = new Machine();

			  for ( int i = 0; i < 32; i++ )
			  {
					long n = ( long ) Math.Pow( 2, i );

					// When
					machine.Reset();
					machine.Packer().pack(n);
					machine.Packer().flush();

					// Then
					long value = NewUnpacker( machine.Output() ).unpackLong();
					assertThat( value, equalTo( n ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testCanPackAndUnpackPowersOfTwoPlusABitAsDoubles() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestCanPackAndUnpackPowersOfTwoPlusABitAsDoubles()
		 {
			  // Given
			  Machine machine = new Machine();

			  for ( int i = 0; i < 32; i++ )
			  {
					double n = Math.Pow( 2, i ) + 0.5;

					// When
					machine.Reset();
					machine.Packer().pack(n);
					machine.Packer().flush();

					double value = NewUnpacker( machine.Output() ).unpackDouble();

					// Then
					assertThat( value, equalTo( n ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testCanPackAndUnpackPowersOfTwoMinusABitAsDoubles() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestCanPackAndUnpackPowersOfTwoMinusABitAsDoubles()
		 {
			  // Given
			  Machine machine = new Machine();

			  for ( int i = 0; i < 32; i++ )
			  {
					double n = Math.Pow( 2, i ) - 0.5;

					// When
					machine.Reset();
					machine.Packer().pack(n);
					machine.Packer().flush();

					// Then
					double value = NewUnpacker( machine.Output() ).unpackDouble();
					assertThat( value, equalTo( n ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testCanPackCommonlyUsedCharAndUnpackAsString() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestCanPackCommonlyUsedCharAndUnpackAsString()
		 {
			  // Given
			  Machine machine = new Machine();

			  for ( int i = 32; i < 127; i++ )
			  {
					char c = ( char ) i;

					// When
					machine.Reset();
					machine.Packer().pack(c);
					machine.Packer().flush();

					// Then
					string value = NewUnpacker( machine.Output() ).unpackString();
					assertThat( value, equalTo( c.ToString() ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testCanPackRandomCharAndUnpackAsString() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestCanPackRandomCharAndUnpackAsString()
		 {
			  // Given
			  Machine machine = new Machine();
			  char[] chars = new char[] { 'ø', 'å', '´', '†', 'œ', '≈' };

			  foreach ( char c in chars )
			  {
					// When
					machine.Reset();
					machine.Packer().pack(c);
					machine.Packer().flush();

					// Then
					string value = NewUnpacker( machine.Output() ).unpackString();
					assertThat( value, equalTo( c.ToString() ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testCanPackAndUnpackStrings() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestCanPackAndUnpackStrings()
		 {
			  // Given
			  Machine machine = new Machine( 17000000 );

			  for ( int i = 0; i < 24; i++ )
			  {
					string @string = StringHelper.NewString( new sbyte[( int ) Math.Pow( 2, i )] );

					// When
					machine.Reset();
					machine.Packer().pack(@string);
					machine.Packer().flush();

					// Then
					string value = NewUnpacker( machine.Output() ).unpackString();
					assertThat( value, equalTo( @string ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testCanPackAndUnpackBytes() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestCanPackAndUnpackBytes()
		 {
			  // Given
			  Machine machine = new Machine();
			  sbyte[] abcdefghij = "ABCDEFGHIJ".GetBytes();

			  // When
			  PackStream.Packer packer = machine.Packer();
			  packer.pack( abcdefghij );
			  packer.Flush();

			  // Then
			  sbyte[] value = NewUnpacker( machine.Output() ).unpackBytes();
			  assertThat( value, equalTo( abcdefghij ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testCanPackAndUnpackString() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestCanPackAndUnpackString()
		 {
			  // Given
			  Machine machine = new Machine();
			  string abcdefghij = "ABCDEFGHIJ";

			  // When
			  PackStream.Packer packer = machine.Packer();
			  packer.Pack( abcdefghij );
			  packer.Flush();

			  // Then
			  string value = NewUnpacker( machine.Output() ).unpackString();
			  assertThat( value, equalTo( abcdefghij ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testCanPackAndUnpackListInOneCall() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestCanPackAndUnpackListInOneCall()
		 {
			  // Given
			  Machine machine = new Machine();

			  // When
			  PackStream.Packer packer = machine.Packer();
			  packer.PackListHeader( 3 );
			  packer.pack( 12 );
			  packer.pack( 13 );
			  packer.pack( 14 );
			  packer.Flush();
			  PackStream.Unpacker unpacker = NewUnpacker( machine.Output() );

			  // Then
			  assertThat( unpacker.UnpackListHeader(), equalTo(3L) );

			  assertThat( unpacker.UnpackLong(), equalTo(12L) );
			  assertThat( unpacker.UnpackLong(), equalTo(13L) );
			  assertThat( unpacker.UnpackLong(), equalTo(14L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testCanPackAndUnpackListOneItemAtATime() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestCanPackAndUnpackListOneItemAtATime()
		 {
			  // Given
			  Machine machine = new Machine();

			  // When
			  PackStream.Packer packer = machine.Packer();
			  packer.PackListHeader( 3 );
			  packer.pack( 12 );
			  packer.pack( 13 );
			  packer.pack( 14 );
			  packer.Flush();
			  PackStream.Unpacker unpacker = NewUnpacker( machine.Output() );

			  // Then
			  assertThat( unpacker.UnpackListHeader(), equalTo(3L) );

			  assertThat( unpacker.UnpackLong(), equalTo(12L) );
			  assertThat( unpacker.UnpackLong(), equalTo(13L) );
			  assertThat( unpacker.UnpackLong(), equalTo(14L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testCanPackAndUnpackListOfString() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestCanPackAndUnpackListOfString()
		 {
			  // Given
			  Machine machine = new Machine();

			  // When
			  PackStream.Packer packer = machine.Packer();
			  packer.PackListHeader( 3 );
			  packer.Flush();
			  packer.Pack( "eins" );
			  packer.Flush();
			  packer.Pack( "zwei" );
			  packer.Flush();
			  packer.Pack( "drei" );
			  packer.Flush();
			  PackStream.Unpacker unpacker = NewUnpacker( machine.Output() );

			  // Then
			  assertThat( unpacker.UnpackListHeader(), equalTo(3L) );

			  assertThat( unpacker.UnpackString(), equalTo("eins") );
			  assertThat( unpacker.UnpackString(), equalTo("zwei") );
			  assertThat( unpacker.UnpackString(), equalTo("drei") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testCanPackAndUnpackListStream() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestCanPackAndUnpackListStream()
		 {
			  // Given
			  Machine machine = new Machine();

			  // When
			  PackStream.Packer packer = machine.Packer();
			  packer.PackListStreamHeader();
			  packer.Pack( "eins" );
			  packer.Pack( "zwei" );
			  packer.Pack( "drei" );
			  packer.PackEndOfStream();
			  packer.Flush();
			  PackStream.Unpacker unpacker = NewUnpacker( machine.Output() );

			  // Then

			  assertThat( unpacker.UnpackListHeader(), equalTo(PackStream.UNKNOWN_SIZE) );

			  assertThat( unpacker.UnpackString(), equalTo("eins") );
			  assertThat( unpacker.UnpackString(), equalTo("zwei") );
			  assertThat( unpacker.UnpackString(), equalTo("drei") );

			  unpacker.UnpackEndOfStream();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testCanPackAndUnpackMap() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestCanPackAndUnpackMap()
		 {
			  // Given
			  Machine machine = new Machine();

			  // When
			  PackStream.Packer packer = machine.Packer();
			  packer.PackMapHeader( 2 );
			  packer.Pack( "one" );
			  packer.pack( 1 );
			  packer.Pack( "two" );
			  packer.pack( 2 );
			  packer.Flush();
			  PackStream.Unpacker unpacker = NewUnpacker( machine.Output() );

			  // Then

			  assertThat( unpacker.UnpackMapHeader(), equalTo(2L) );

			  assertThat( unpacker.UnpackString(), equalTo("one") );
			  assertThat( unpacker.UnpackLong(), equalTo(1L) );
			  assertThat( unpacker.UnpackString(), equalTo("two") );
			  assertThat( unpacker.UnpackLong(), equalTo(2L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testCanPackAndUnpackMapStream() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestCanPackAndUnpackMapStream()
		 {
			  // Given
			  Machine machine = new Machine();

			  // When
			  PackStream.Packer packer = machine.Packer();
			  packer.PackMapStreamHeader();
			  packer.Pack( "one" );
			  packer.pack( 1 );
			  packer.Pack( "two" );
			  packer.pack( 2 );
			  packer.PackEndOfStream();
			  packer.Flush();
			  PackStream.Unpacker unpacker = NewUnpacker( machine.Output() );

			  // Then

			  assertThat( unpacker.UnpackMapHeader(), equalTo(PackStream.UNKNOWN_SIZE) );

			  assertThat( unpacker.UnpackString(), equalTo("one") );
			  assertThat( unpacker.UnpackLong(), equalTo(1L) );
			  assertThat( unpacker.UnpackString(), equalTo("two") );
			  assertThat( unpacker.UnpackLong(), equalTo(2L) );

			  unpacker.UnpackEndOfStream();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testCanPackAndUnpackStruct() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestCanPackAndUnpackStruct()
		 {
			  // Given
			  Machine machine = new Machine();

			  // When
			  PackStream.Packer packer = machine.Packer();
			  packer.PackStructHeader( 3, ( sbyte )'N' );
			  packer.pack( 12 );
			  packer.PackListHeader( 2 );
			  packer.Pack( "Person" );
			  packer.Pack( "Employee" );
			  packer.PackMapHeader( 2 );
			  packer.Pack( "name" );
			  packer.Pack( "Alice" );
			  packer.Pack( "age" );
			  packer.pack( 33 );
			  packer.Flush();
			  PackStream.Unpacker unpacker = NewUnpacker( machine.Output() );

			  // Then
			  assertThat( unpacker.UnpackStructHeader(), equalTo(3L) );
			  assertThat( unpacker.UnpackStructSignature(), equalTo('N') );

			  assertThat( unpacker.UnpackLong(), equalTo(12L) );

			  assertThat( unpacker.UnpackListHeader(), equalTo(2L) );
			  assertThat( unpacker.UnpackString(), equalTo("Person") );
			  assertThat( unpacker.UnpackString(), equalTo("Employee") );

			  assertThat( unpacker.UnpackMapHeader(), equalTo(2L) );
			  assertThat( unpacker.UnpackString(), equalTo("name") );
			  assertThat( unpacker.UnpackString(), equalTo("Alice") );
			  assertThat( unpacker.UnpackString(), equalTo("age") );
			  assertThat( unpacker.UnpackLong(), equalTo(33L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testCanPackStructIncludingSignature() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestCanPackStructIncludingSignature()
		 {
			  // Given
			  Machine machine = new Machine();

			  // When
			  PackStream.Packer packer = machine.Packer();
			  packer.PackStructHeader( 3, ( sbyte )'N' );
			  packer.pack( 12 );
			  packer.PackListHeader( 2 );
			  packer.Pack( "Person" );
			  packer.Pack( "Employee" );
			  packer.PackMapHeader( 2 );
			  packer.Pack( "name" );
			  packer.Pack( "Alice" );
			  packer.Pack( "age" );
			  packer.pack( 33 );
			  packer.Flush();

			  // Then
			  sbyte[] bytes = machine.Output();
			  sbyte[] expected = new sbyte[]{ PackStream.TinyStruct | 3, ( sbyte )'N', 12, PackStream.TinyList | 2, PackStream.TinyString | 6, ( sbyte )'P', ( sbyte )'e', ( sbyte )'r', ( sbyte )'s', ( sbyte )'o', ( sbyte )'n', PackStream.TinyString | 8, ( sbyte )'E', ( sbyte )'m', ( sbyte )'p', ( sbyte )'l', ( sbyte )'o', ( sbyte )'y', ( sbyte )'e', ( sbyte )'e', PackStream.TinyMap | 2, PackStream.TinyString | 4, ( sbyte )'n', ( sbyte )'a', ( sbyte )'m', ( sbyte )'e', PackStream.TinyString | 5, ( sbyte )'A', ( sbyte )'l', ( sbyte )'i', ( sbyte )'c', ( sbyte )'e', PackStream.TinyString | 3, ( sbyte )'a', ( sbyte )'g', ( sbyte )'e', 33 };
			  assertThat( bytes, equalTo( expected ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testCanDoStreamingListUnpacking() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestCanDoStreamingListUnpacking()
		 {
			  // Given
			  Machine machine = new Machine();
			  PackStream.Packer packer = machine.Packer();
			  packer.PackListHeader( 4 );
			  packer.pack( 1 );
			  packer.pack( 2 );
			  packer.pack( 3 );
			  packer.PackListHeader( 2 );
			  packer.pack( 4 );
			  packer.pack( 5 );
			  packer.Flush();

			  // When I unpack this value
			  PackStream.Unpacker unpacker = NewUnpacker( machine.Output() );

			  // Then I can do streaming unpacking
			  long size = unpacker.UnpackListHeader();
			  long a = unpacker.UnpackLong();
			  long b = unpacker.UnpackLong();
			  long c = unpacker.UnpackLong();

			  long innerSize = unpacker.UnpackListHeader();
			  long d = unpacker.UnpackLong();
			  long e = unpacker.UnpackLong();

			  // And all the values should be sane
			  assertEquals( 4, size );
			  assertEquals( 2, innerSize );
			  assertEquals( 1, a );
			  assertEquals( 2, b );
			  assertEquals( 3, c );
			  assertEquals( 4, d );
			  assertEquals( 5, e );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testCanDoStreamingStructUnpacking() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestCanDoStreamingStructUnpacking()
		 {
			  // Given
			  Machine machine = new Machine();
			  PackStream.Packer packer = machine.Packer();
			  packer.PackStructHeader( 4, ( sbyte )'~' );
			  packer.pack( 1 );
			  packer.pack( 2 );
			  packer.pack( 3 );
			  packer.PackListHeader( 2 );
			  packer.pack( 4 );
			  packer.pack( 5 );
			  packer.Flush();

			  // When I unpack this value
			  PackStream.Unpacker unpacker = NewUnpacker( machine.Output() );

			  // Then I can do streaming unpacking
			  long size = unpacker.UnpackStructHeader();
			  char signature = unpacker.UnpackStructSignature();
			  long a = unpacker.UnpackLong();
			  long b = unpacker.UnpackLong();
			  long c = unpacker.UnpackLong();

			  long innerSize = unpacker.UnpackListHeader();
			  long d = unpacker.UnpackLong();
			  long e = unpacker.UnpackLong();

			  // And all the values should be sane
			  assertEquals( 4, size );
			  assertEquals( '~', signature );
			  assertEquals( 2, innerSize );
			  assertEquals( 1, a );
			  assertEquals( 2, b );
			  assertEquals( 3, c );
			  assertEquals( 4, d );
			  assertEquals( 5, e );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testCanDoStreamingMapUnpacking() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestCanDoStreamingMapUnpacking()
		 {
			  // Given
			  Machine machine = new Machine();
			  PackStream.Packer packer = machine.Packer();
			  packer.PackMapHeader( 2 );
			  packer.Pack( "name" );
			  packer.Pack( "Bob" );
			  packer.Pack( "cat_ages" );
			  packer.PackListHeader( 2 );
			  packer.Pack( 4.3 );
			  packer.Pack( true );
			  packer.Flush();

			  // When I unpack this value
			  PackStream.Unpacker unpacker = NewUnpacker( machine.Output() );

			  // Then I can do streaming unpacking
			  long size = unpacker.UnpackMapHeader();
			  string k1 = unpacker.UnpackString();
			  string v1 = unpacker.UnpackString();
			  string k2 = unpacker.UnpackString();

			  long innerSize = unpacker.UnpackListHeader();
			  double d = unpacker.UnpackDouble();
			  bool e = unpacker.UnpackBoolean();

			  // And all the values should be sane
			  assertEquals( 2, size );
			  assertEquals( 2, innerSize );
			  assertEquals( "name", k1 );
			  assertEquals( "Bob", v1 );
			  assertEquals( "cat_ages", k2 );
			  assertEquals( 4.3, d, 0.0001 );
			  assertTrue( e );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void handlesDataCrossingBufferBoundaries() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void HandlesDataCrossingBufferBoundaries()
		 {
			  // Given
			  Machine machine = new Machine();
			  PackStream.Packer packer = machine.Packer();
			  packer.pack( long.MaxValue );
			  packer.pack( long.MaxValue );
			  packer.Flush();

			  ReadableByteChannel ch = Channels.newChannel( new MemoryStream( machine.Output() ) );
			  PackStream.Unpacker unpacker = new PackStream.Unpacker( ( new BufferedChannelInput( 11 ) ).Reset( ch ) );

			  // Serialized ch will look like, and misalign with the 11-byte unpack buffer:

			  // [XX][XX][XX][XX][XX][XX][XX][XX][XX][XX][XX][XX][XX][XX][XX][XX][XX][XX]
			  //  mkr \___________data______________/ mkr \___________data______________/
			  // \____________unpack buffer_________________/

			  // When
			  long first = unpacker.UnpackLong();
			  long second = unpacker.UnpackLong();

			  // Then
			  assertEquals( long.MaxValue, first );
			  assertEquals( long.MaxValue, second );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testCanPeekOnNextType() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestCanPeekOnNextType()
		 {
			  // When & Then
			  AssertPeekType( PackType.String, "a string" );
			  AssertPeekType( PackType.Integer, 123L );
			  AssertPeekType( PackType.Float, 123.123d );
			  AssertPeekType( PackType.Boolean, true );
			  AssertPeekType( PackType.List, asList( 1, 2, 3 ) );
			  AssertPeekType( PackType.Map, AsMap( "l", 3 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldPackUnpackBytesHeaderWithCorrectBufferSize() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldPackUnpackBytesHeaderWithCorrectBufferSize()
		 {
			  Machine machine = new Machine();
			  PackStream.Packer packer = machine.Packer();

			  MachineClient client = new MachineClient( 8 );
			  PackStream.Unpacker unpacker = client.Unpacker();

			  for ( int size = 0; size <= 65536; size++ )
			  {
					machine.Reset();
					packer.PackBytesHeader( size );
					packer.Flush();

					// Then
					int bufferSize = ComputeOutputBufferSize( size, false );
					sbyte[] output = machine.Output();
					assertThat( output.Length, equalTo( bufferSize ) );

					client.Reset( output );
					int value = unpacker.UnpackBytesHeader();
					assertThat( value, equalTo( size ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldPackUnpackStringHeaderWithCorrectBufferSize() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldPackUnpackStringHeaderWithCorrectBufferSize()
		 {
			  ShouldPackUnpackHeaderWithCorrectBufferSize( PackType.String );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldPackUnpackMapHeaderWithCorrectBufferSize() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldPackUnpackMapHeaderWithCorrectBufferSize()
		 {
			  ShouldPackUnpackHeaderWithCorrectBufferSize( MAP );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldPackUnpackListHeaderWithCorrectBufferSize() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldPackUnpackListHeaderWithCorrectBufferSize()
		 {
			  ShouldPackUnpackHeaderWithCorrectBufferSize( PackType.List );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldThrowErrorWhenUnPackHeaderSizeGreaterThanIntMaxValue() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldThrowErrorWhenUnPackHeaderSizeGreaterThanIntMaxValue()
		 {
			  assertThrows( typeof( PackStream.Overflow ), () => unpackHeaderSizeGreaterThanIntMaxValue(MAP) );
			  assertThrows( typeof( PackStream.Overflow ), () => unpackHeaderSizeGreaterThanIntMaxValue(LIST) );
			  assertThrows( typeof( PackStream.Overflow ), () => unpackHeaderSizeGreaterThanIntMaxValue(STRING) );
			  assertThrows( typeof( PackStream.Overflow ), () => unpackHeaderSizeGreaterThanIntMaxValue(BYTES) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void unpackHeaderSizeGreaterThanIntMaxValue(PackType type) throws Throwable
		 private void UnpackHeaderSizeGreaterThanIntMaxValue( PackType type )
		 {
			  sbyte marker32;
			  switch ( type )
			  {
			  case Neo4Net.Bolt.v1.packstream.PackType.Map:
					marker32 = PackStream.Map_32;
					break;
			  case Neo4Net.Bolt.v1.packstream.PackType.List:
					marker32 = PackStream.List_32;
					break;
			  case Neo4Net.Bolt.v1.packstream.PackType.String:
					marker32 = PackStream.String_32;
					break;
			  case Neo4Net.Bolt.v1.packstream.PackType.Bytes:
					marker32 = PackStream.Bytes_32;
					break;
			  default:
					throw new System.ArgumentException( "Unsupported type: " + type + "." );
			  }

			  sbyte[] input = new sbyte[]{ marker32, unchecked( ( sbyte ) 0xff ), unchecked( ( sbyte ) 0xff ), unchecked( ( sbyte ) 0xff ), unchecked( ( sbyte ) 0xff ) };
			  MachineClient client = new MachineClient( 8 );

			  client.Reset( input );
			  PackStream.Unpacker unpacker = client.Unpacker();

			  switch ( type )
			  {
			  case Neo4Net.Bolt.v1.packstream.PackType.Map:
					unpacker.UnpackMapHeader();
					break;
			  case Neo4Net.Bolt.v1.packstream.PackType.List:
					unpacker.UnpackListHeader();
					break;
			  case Neo4Net.Bolt.v1.packstream.PackType.String:
					unpacker.UnpackStringHeader();
					break;
			  case Neo4Net.Bolt.v1.packstream.PackType.Bytes:
					unpacker.UnpackBytesHeader();
					break;
			  default:
					throw new System.ArgumentException( "Unsupported type: " + type + "." );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void shouldPackUnpackHeaderWithCorrectBufferSize(PackType type) throws Throwable
		 private void ShouldPackUnpackHeaderWithCorrectBufferSize( PackType type )
		 {
			  Machine machine = new Machine();
			  PackStream.Packer packer = machine.Packer();

			  MachineClient client = new MachineClient( 8 );
			  PackStream.Unpacker unpacker = client.Unpacker();

			  for ( int size = 0; size <= 65536; size++ )
			  {
					machine.Reset();
					switch ( type )
					{
					case Neo4Net.Bolt.v1.packstream.PackType.Map:
						 packer.PackMapHeader( size );
						 break;
					case Neo4Net.Bolt.v1.packstream.PackType.List:
						 packer.PackListHeader( size );
						 break;
					case Neo4Net.Bolt.v1.packstream.PackType.String:
						 packer.PackStringHeader( size );
						 break;
					default:
						 throw new System.ArgumentException( "Unsupported type: " + type + "." );
					}
					packer.Flush();

					int bufferSize = ComputeOutputBufferSize( size, true );
					sbyte[] output = machine.Output();
					assertThat( output.Length, equalTo( bufferSize ) );

					client.Reset( output );
					int value = 0;
					switch ( type )
					{
					case Neo4Net.Bolt.v1.packstream.PackType.Map:
						 value = ( int ) unpacker.UnpackMapHeader();
						 break;
					case Neo4Net.Bolt.v1.packstream.PackType.List:
						 value = ( int ) unpacker.UnpackListHeader();
						 break;
					case Neo4Net.Bolt.v1.packstream.PackType.String:
						 value = unpacker.UnpackStringHeader();
						 break;
					default:
						 throw new System.ArgumentException( "Unsupported type: " + type + "." );
					}

					assertThat( value, equalTo( size ) );
			  }
		 }

		 private int ComputeOutputBufferSize( int size, bool withMarker8 )
		 {
			  int bufferSize;
			  if ( withMarker8 && size < 16 )
			  {
					bufferSize = 1;
			  }
			  else if ( size < 128 )
			  {
					bufferSize = 2;
			  }
			  else if ( size < 32768 )
			  {
					bufferSize = 1 + 2;
			  }
			  else
			  {
					bufferSize = 1 + 4;
			  }
			  return bufferSize;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertPeekType(PackType type, Object value) throws java.io.IOException
		 private void AssertPeekType( PackType type, object value )
		 {
			  // Given
			  Machine machine = new Machine();
			  PackStream.Packer packer = machine.Packer();
			  DoTheThing( packer, value );
			  packer.Flush();

			  PackStream.Unpacker unpacker = NewUnpacker( machine.Output() );

			  // When & Then
			  assertEquals( type, unpacker.PeekNextType() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void doTheThing(PackStream.Packer packer, Object value) throws java.io.IOException
		 private void DoTheThing( PackStream.Packer packer, object value )
		 {
			  if ( value is string )
			  {
					packer.Pack( ( string ) value );
			  }
			  else if ( value is long? || value is int? )
			  {
					packer.pack( ( ( Number ) value ).longValue() );
			  }
			  else if ( value is double? || value is float? )
			  {
					packer.pack( ( ( Number ) value ).doubleValue() );
			  }
			  else if ( value is bool? )
			  {
					packer.pack( ( bool? ) value );
			  }
			  else if ( value is System.Collections.IList )
			  {
					System.Collections.IList list = ( System.Collections.IList ) value;
					packer.PackListHeader( list.Count );
					foreach ( object o in list )
					{
						 DoTheThing( packer, o );
					}
			  }
			  else if ( value is System.Collections.IDictionary )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<?,?> map = (java.util.Map<?,?>) value;
					IDictionary<object, ?> map = ( IDictionary<object, ?> ) value;
					packer.PackMapHeader( map.Count );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (java.util.Map.Entry<?,?> o : map.entrySet())
					foreach ( KeyValuePair<object, ?> o in map.SetOfKeyValuePairs() )
					{
						 DoTheThing( packer, o.Key );
						 DoTheThing( packer, o.Value );
					}
			  }
		 }
	}

}