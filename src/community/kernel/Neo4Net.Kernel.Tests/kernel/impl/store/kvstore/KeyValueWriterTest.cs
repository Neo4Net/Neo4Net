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
namespace Neo4Net.Kernel.impl.store.kvstore
{
	using After = org.junit.After;
	using Test = org.junit.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class KeyValueWriterTest
	{
		private bool InstanceFieldsInitialized = false;

		public KeyValueWriterTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_writer = new KeyValueWriter( _collector, _stub );
		}

		 private const int ENTRIES_PER_PAGE = 4 * 1024 / 16;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private StubCollector collector = new StubCollector(ENTRIES_PER_PAGE);
		 private StubCollector _collector = new StubCollector( ENTRIES_PER_PAGE );
		 private readonly StubWriter _stub = new StubWriter();
		 private KeyValueWriter _writer;
		 private readonly BigEndianByteArrayBuffer _key = new BigEndianByteArrayBuffer( new sbyte[8] );
		 private readonly BigEndianByteArrayBuffer _value = new BigEndianByteArrayBuffer( new sbyte[8] );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void closeWriter() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CloseWriter()
		 {
			  _writer.Dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcceptNoHeadersAndNoData() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAcceptNoHeadersAndNoData()
		 {
			  // given
			  _value.putByte( 0, ( sbyte ) 0x7F );
			  _value.putByte( 7, ( sbyte ) 0x7F );

			  // when
			  assertTrue( "format specifier", _writer.writeHeader( _key, _value ) );
			  assertTrue( "end-of-header marker", _writer.writeHeader( _key, _value ) );
			  assertTrue( "end marker + number of data items", _writer.writeHeader( _key, _value ) );

			  // then

			  _stub.assertData( new sbyte[]{ 0x00, 0, 0, 0, 0, 0, 0, 0x00, 0x7F, 0, 0, 0, 0, 0, 0, 0x7F, 0x00, 0, 0, 0, 0, 0, 0, 0x00, 0x00, 0, 0, 0, 0, 0, 0, 0x00, 0x00, 0, 0, 0, 0, 0, 0, 0x00, 0x00, 0, 0, 0, 0, 0, 0, 0x00 } );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRequireNonZeroFormatSpecifier() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRequireNonZeroFormatSpecifier()
		 {
			  assertFalse( "format-specifier", _writer.writeHeader( _key, _value ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRejectInvalidHeaderKeyWhenAssertionsAreEnabled() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRejectInvalidHeaderKeyWhenAssertionsAreEnabled()
		 {
			  // given
			  _key.putByte( 3, ( sbyte ) 1 );
			  _value.putByte( 0, ( sbyte ) 0x7F );
			  _value.putByte( 7, ( sbyte ) 0x7F );

			  // when
			  try
			  {
					_writer.writeHeader( _key, _value );
					// don't assert that we throw an exception - because:
					// 1) we'd catch that AssertionError
					// 2) we want this test to pass even without assertions enabled...
			  }
			  // then
			  catch ( AssertionError e )
			  {
					assertEquals( "key should have been cleared by previous call", e.Message );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRejectInvalidDataKey() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRejectInvalidDataKey()
		 {
			  // given
			  _value.putByte( 0, ( sbyte ) 0x7F );
			  _value.putByte( 7, ( sbyte ) 0x7F );
			  _writer.writeHeader( _key, _value );
			  _writer.writeHeader( _key, _value );

			  // when
			  try
			  {
					_writer.writeData( _key, _value );

					fail( "expected exception" );
			  }
			  // then
			  catch ( System.ArgumentException e )
			  {
					assertEquals( "All-zero keys are not allowed.", e.Message );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRejectDataBeforeHeaders() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRejectDataBeforeHeaders()
		 {
			  // given
			  _key.putByte( 2, ( sbyte ) 0x77 );

			  // when
			  try
			  {
					_writer.writeData( _key, _value );

					fail( "expected exception" );
			  }
			  // then
			  catch ( System.InvalidOperationException e )
			  {
					assertEquals( "Cannot write data when expecting format specifier.", e.Message );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRejectDataAfterInsufficientHeaders() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRejectDataAfterInsufficientHeaders()
		 {
			  // given
			  _value.fill( unchecked( ( sbyte ) 0xFF ) );
			  assertTrue( _writer.writeHeader( _key, _value ) );
			  _key.putByte( 2, ( sbyte ) 0x77 );

			  // when
			  try
			  {
					_writer.writeData( _key, _value );

					fail( "expected exception" );
			  }
			  // then
			  catch ( System.InvalidOperationException e )
			  {
					assertEquals( "Cannot write data when expecting header.", e.Message );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotOpenStoreFileIfWritingHasNotCompleted() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotOpenStoreFileIfWritingHasNotCompleted()
		 {
			  for ( int i = 0; i <= 10; i++ )
			  {
					// given
					string[] headers;
					switch ( i )
					{
					case 0:
					case 1:
					case 8:
					case 9:
					case 10:
						 headers = new string[0];
						 break;
					case 2:
						 headers = new string[]{ "foo" };
						 break;
					default:
						 headers = new string[]{ "foo", "bar" };
						 break;
					}
					ResetWriter( headers );
					for ( int field = 1; field <= i; field++ )
					{
						 switch ( field )
						 {
						 // header fields
						 case 3:
							  if ( i >= 8 ) // no headers
							  {
									_writer.writeHeader( _key, _value ); // padding
							  }
							 goto case 2;
						 case 2:
							  if ( i >= 8 ) // no headers
							  {
									break;
							  }
						 case 1:
							  _value.putByte( 0, ( sbyte ) 0x7F );
							  _value.putByte( 7, ( sbyte ) 0x7F );
							  _writer.writeHeader( _key, _value );
							  break;
						 default: // data fields
							  if ( ( i < 8 ) || ( field > 8 ) )
							  {
									_key.putByte( _key.size() - 1, (sbyte) field );
									_writer.writeData( _key, _value );
							  }
						  break;
						 }
					}

					// when
					try
					{
						 _writer.openStoreFile();

						 fail( "expected exception" );
					}
					// then
					catch ( System.InvalidOperationException e )
					{
						 assertTrue( e.Message.StartsWith( "Cannot open store file when " ) );
					}
			  }
		 }

		 private void ResetWriter( params string[] header )
		 {
			  _collector = new StubCollector( ENTRIES_PER_PAGE, header );
			  _writer = new KeyValueWriter( _collector, _stub );
		 }

		 private class StubWriter : KeyValueWriter.Writer
		 {
			  internal IOException Next;
			  internal MemoryStream Data = new MemoryStream();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void write(byte[] data) throws java.io.IOException
			  internal override void Write( sbyte[] data )
			  {
					Io();
					this.Data.Write( data, 0, data.Length );
			  }

			  internal override KeyValueStoreFile Open( Metadata metadata, int keySize, int valueSize )
			  {
					return null;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void close() throws java.io.IOException
			  internal override void Close()
			  {
					Io();
			  }

			  public virtual void AssertData( params sbyte[] expected )
			  {
					assertArrayEquals( expected, this.Data.toByteArray() );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void io() throws java.io.IOException
			  internal virtual void Io()
			  {
					try
					{
						 if ( Next != null )
						 {
							  throw Next;
						 }
					}
					finally
					{
						 Next = null;
					}
			  }
		 }
	}

}