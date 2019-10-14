using System;

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
namespace Neo4Net.Bolt.v1.packstream.utf8
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.util.FeatureToggles.getInteger;

	/// <summary>
	/// This is a specialized UTF-8 encoder that solves two predicaments:
	/// <para>
	/// 1) There's no way using public APIs to do GC-free string encoding unless
	/// you build a custom encoder, and GC output from UTF-8 encoding causes
	/// production instability
	/// 2) The ArrayEncoder provided by HotSpot is 2 orders faster for UTF-8 encoding
	/// for a massive amount of real-world strings due to specialized handling of
	/// ascii, and we can't import that since we need to compile on IBM J9
	/// </para>
	/// <para>
	/// We can't solve (1) without solving (2), because the default GC-spewing String#getBytes()
	/// uses the optimized ArrayEncoder, meaning it's easy to write an encoder that
	/// is GC-free, but then it'll be two orders slower than the stdlib, and vice
	/// versa.
	/// </para>
	/// <para>
	/// This solves both issues using MethodHandles. Future work here could include
	/// writing a custom UTF-8 encoder (which could then avoid using ArrayEncoder),
	/// as well as stopping use of String's for the main database paths.
	/// We already have  Token, which
	/// could easily contain pre-encoded UTF-8 data, and "runtime" Strings could be
	/// handled with a custom type that is more stability friendly, for instance
	/// by building on to StringProperty.
	/// </para>
	/// </summary>
	public class SunMiscUTF8Encoder : UTF8Encoder
	{
		private bool InstanceFieldsInitialized = false;

		public SunMiscUTF8Encoder()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_outBuf = ByteBuffer.wrap( @out );
		}

		 private static readonly int _bufferSize = getInteger( typeof( SunMiscUTF8Encoder ), "buffer_size", 1024 * 16 );
		 private static readonly int _fallbackAtStringLength = ( int )( _bufferSize / StandardCharsets.UTF_8.newEncoder().averageBytesPerChar() );
		 private static readonly MethodHandle _getCharArray = CharArrayGetter();
		 private static readonly MethodHandle _arrayEncode = _arrayEncode();
		 private static readonly MethodHandle _getOffset = OffsetHandle();
		 private readonly CharsetEncoder _charsetEncoder = StandardCharsets.UTF_8.newEncoder();

		 private readonly sbyte[] @out = new sbyte[_bufferSize];
		 private ByteBuffer _outBuf;
		 private readonly UTF8Encoder _fallbackEncoder = new VanillaUTF8Encoder();

		 public override ByteBuffer Encode( string input )
		 {
			  try
			  {
					// If it's unlikely we will fit the encoded data, just use stdlib encoder
					if ( input.Length > _fallbackAtStringLength )
					{
						 return _fallbackEncoder.encode( input );
					}

					char[] rawChars = ( char[] ) _getCharArray.invoke( input );
					int len = ( int ) _arrayEncode.invoke( _charsetEncoder, rawChars, Offset( input ), input.Length, @out );

					if ( len == -1 )
					{
						 return _fallbackEncoder.encode( input );
					}

					_outBuf.position( 0 );
					_outBuf.limit( len );
					return _outBuf;
			  }
			  catch ( System.IndexOutOfRangeException )
			  {
					// This happens when we can't fit the encoded string.
					// We try and avoid this altogether by falling back to the
					// vanilla encoder if the string looks like it'll not fit -
					// but this is probabilistic since we don't know until we've encoded.
					// So, if our guess is wrong, we fall back here instead.
					return _fallbackEncoder.encode( input );
			  }
			  catch ( Exception e )
			  {
					throw new AssertionError( "This encoder depends on sun.nio.cs.ArrayEncoder, which failed to load: " + e.Message, e );
			  }
		 }

		 private static MethodHandle ArrayEncode()
		 {
			  // Because we need to be able to compile on IBM's JVM, we can't
			  // depend on ArrayEncoder. Unfortunately, ArrayEncoders encode method
			  // is twoish orders of magnitude faster than regular encoders for ascii
			  // so we go through the hurdle of calling that encode method via
			  // a MethodHandle.
			  MethodHandles.Lookup lookup = MethodHandles.lookup();
			  try
			  {
					return lookup.unreflect( Type.GetType( "sun.nio.cs.ArrayEncoder" ).GetMethod( "encode", typeof( char[] ), typeof( int ), typeof( int ), typeof( sbyte[] ) ) );
			  }
			  catch ( Exception e )
			  {
					throw new AssertionError( "This encoder depends on sun.nio.cs.ArrayEncoder, which failed to load: " + e.Message, e );
			  }
		 }

		 private static MethodHandle CharArrayGetter()
		 {
			  MethodHandles.Lookup lookup = MethodHandles.lookup();
			  try
			  {
					System.Reflection.FieldInfo value = typeof( string ).getDeclaredField( "value" );
					if ( value.Type != typeof( char[] ) )
					{
						 throw new AssertionError( "This encoder depends being able to access raw char[] in java.lang.String, but the class is backed by a " + value.Type.CanonicalName );
					}
					value.Accessible = true;
					return lookup.unreflectGetter( value );
			  }
			  catch ( Exception e )
			  {
					throw new AssertionError( "This encoder depends being able to access raw char[] in java.lang.String, which failed: " + e.Message, e );
			  }
		 }

		 /*
		  * If String.class is backed by a char[] together with an offset, return
		  * the offset otherwise return 0.
		  */
		 private static int Offset( string value )
		 {
			  try
			  {
					return _getOffset == null ? 0 : ( int ) _getOffset.invoke( value );
			  }
			  catch ( Exception e )
			  {
					throw new AssertionError( "This encoder depends being able to access the offset in the char[] array in java.lang.String, " + "which failed: " + e.Message, e );
			  }
		 }

		 private static MethodHandle OffsetHandle()
		 {
			  //We need access to the internal char[] in order to do gc free
			  //encoding. However for ibm jdk it is not always true that
			  //"foo" is backed by exactly ['f', 'o', 'o'], for example single
			  //ascii characters strings like "a" is backed by:
			  //
			  //    value = ['0', '1', ..., 'A', 'B', ..., 'a', 'b', ...]
			  //    offset = 'a'
			  //
			  //Hence we need access both to `value` and `offset`
			  MethodHandles.Lookup lookup = MethodHandles.lookup();
			  try
			  {
					System.Reflection.FieldInfo value = typeof( string ).getDeclaredField( "offset" );
					value.Accessible = true;
					return lookup.unreflectGetter( value );
			  }
			  catch ( Exception )
			  {
					//there is no offset in String implementation
					return null;
			  }
		 }
	}

}