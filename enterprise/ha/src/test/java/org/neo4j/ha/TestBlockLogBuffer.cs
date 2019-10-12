/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.ha
{
	using BaseMatcher = org.hamcrest.BaseMatcher;
	using Description = org.hamcrest.Description;
	using ChannelBuffer = org.jboss.netty.buffer.ChannelBuffer;
	using ChannelBuffers = org.jboss.netty.buffer.ChannelBuffers;
	using Test = org.junit.Test;


	using BlockLogBuffer = Org.Neo4j.com.BlockLogBuffer;
	using BlockLogReader = Org.Neo4j.com.BlockLogReader;
	using ByteCounterMonitor = Org.Neo4j.Kernel.monitoring.ByteCounterMonitor;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;

	public class TestBlockLogBuffer
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void onlyOneNonFullBlock()
		 public virtual void OnlyOneNonFullBlock()
		 {
			  sbyte[] bytes = new sbyte[255];
			  ChannelBuffer wrappedBuffer = ChannelBuffers.wrappedBuffer( bytes );
			  wrappedBuffer.resetWriterIndex();
			  BlockLogBuffer buffer = new BlockLogBuffer( wrappedBuffer, ( new Monitors() ).newMonitor(typeof(ByteCounterMonitor)) );

			  sbyte byteValue = 5;
			  int intValue = 1234;
			  long longValue = 574853;
			  float floatValue = 304985.5f;
			  double doubleValue = 48493.22d;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte[] bytesValue = new byte[] { 1, 5, 2, 6, 3 };
			  sbyte[] bytesValue = new sbyte[] { 1, 5, 2, 6, 3 };
			  buffer.Put( byteValue );
			  buffer.PutInt( intValue );
			  buffer.PutLong( longValue );
			  buffer.PutFloat( floatValue );
			  buffer.PutDouble( doubleValue );
			  buffer.Put( bytesValue, bytesValue.Length );
			  buffer.Dispose();

			  ByteBuffer verificationBuffer = ByteBuffer.wrap( bytes );
			  assertEquals( 30, verificationBuffer.get() );
			  assertEquals( byteValue, verificationBuffer.get() );
			  assertEquals( intValue, verificationBuffer.Int );
			  assertEquals( longValue, verificationBuffer.Long );
			  assertEquals( floatValue, verificationBuffer.Float, 0.0 );
			  assertEquals( doubleValue, verificationBuffer.Double, 0.0 );
			  sbyte[] actualBytes = new sbyte[bytesValue.Length];
			  verificationBuffer.get( actualBytes );
			  assertThat( actualBytes, new ArrayMatches<>( this, bytesValue ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readSmallPortions() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReadSmallPortions()
		 {
			  sbyte[] bytes = new sbyte[255];
			  ChannelBuffer wrappedBuffer = ChannelBuffers.wrappedBuffer( bytes );
			  wrappedBuffer.resetWriterIndex();
			  BlockLogBuffer buffer = new BlockLogBuffer( wrappedBuffer, ( new Monitors() ).newMonitor(typeof(ByteCounterMonitor)) );

			  sbyte byteValue = 5;
			  int intValue = 1234;
			  long longValue = 574853;
			  buffer.Put( byteValue );
			  buffer.PutInt( intValue );
			  buffer.PutLong( longValue );
			  buffer.Dispose();

			  ReadableByteChannel reader = new BlockLogReader( wrappedBuffer );
			  ByteBuffer verificationBuffer = ByteBuffer.wrap( new sbyte[1] );
			  reader.read( verificationBuffer );
			  verificationBuffer.flip();
			  assertEquals( byteValue, verificationBuffer.get() );
			  verificationBuffer = ByteBuffer.wrap( new sbyte[4] );
			  reader.read( verificationBuffer );
			  verificationBuffer.flip();
			  assertEquals( intValue, verificationBuffer.Int );
			  verificationBuffer = ByteBuffer.wrap( new sbyte[8] );
			  reader.read( verificationBuffer );
			  verificationBuffer.flip();
			  assertEquals( longValue, verificationBuffer.Long );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readOnlyOneNonFullBlock() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReadOnlyOneNonFullBlock()
		 {
			  sbyte[] bytes = new sbyte[255];
			  ChannelBuffer wrappedBuffer = ChannelBuffers.wrappedBuffer( bytes );
			  wrappedBuffer.resetWriterIndex();
			  BlockLogBuffer buffer = new BlockLogBuffer( wrappedBuffer, ( new Monitors() ).newMonitor(typeof(ByteCounterMonitor)) );

			  sbyte byteValue = 5;
			  int intValue = 1234;
			  long longValue = 574853;
			  float floatValue = 304985.5f;
			  double doubleValue = 48493.22d;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte[] bytesValue = new byte[] { 1, 5, 2, 6, 3 };
			  sbyte[] bytesValue = new sbyte[] { 1, 5, 2, 6, 3 };
			  buffer.Put( byteValue );
			  buffer.PutInt( intValue );
			  buffer.PutLong( longValue );
			  buffer.PutFloat( floatValue );
			  buffer.PutDouble( doubleValue );
			  buffer.Put( bytesValue, bytesValue.Length );
			  buffer.Dispose();

			  ReadableByteChannel reader = new BlockLogReader( wrappedBuffer );
			  ByteBuffer verificationBuffer = ByteBuffer.wrap( new sbyte[1000] );
			  reader.read( verificationBuffer );
			  verificationBuffer.flip();
			  assertEquals( byteValue, verificationBuffer.get() );
			  assertEquals( intValue, verificationBuffer.Int );
			  assertEquals( longValue, verificationBuffer.Long );
			  assertEquals( floatValue, verificationBuffer.Float, 0.0 );
			  assertEquals( doubleValue, verificationBuffer.Double, 0.0 );
			  sbyte[] actualBytes = new sbyte[bytesValue.Length];
			  verificationBuffer.get( actualBytes );
			  assertThat( actualBytes, new ArrayMatches<>( this, bytesValue ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void onlyOneFullBlock()
		 public virtual void OnlyOneFullBlock()
		 {
			  sbyte[] bytes = new sbyte[256];
			  ChannelBuffer wrappedBuffer = ChannelBuffers.wrappedBuffer( bytes );
			  wrappedBuffer.resetWriterIndex();
			  BlockLogBuffer buffer = new BlockLogBuffer( wrappedBuffer, ( new Monitors() ).newMonitor(typeof(ByteCounterMonitor)) );

			  sbyte[] bytesValue = new sbyte[255];
			  bytesValue[0] = 1;
			  bytesValue[254] = -1;
			  buffer.Put( bytesValue, bytesValue.Length );
			  buffer.Dispose();

			  ByteBuffer verificationBuffer = ByteBuffer.wrap( bytes );
			  assertEquals( unchecked( ( sbyte ) 255 ), verificationBuffer.get() );
			  sbyte[] actualBytes = new sbyte[bytesValue.Length];
			  verificationBuffer.get( actualBytes );
			  assertThat( actualBytes, new ArrayMatches<>( this, bytesValue ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readOnlyOneFullBlock() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReadOnlyOneFullBlock()
		 {
			  sbyte[] bytes = new sbyte[256];
			  ChannelBuffer wrappedBuffer = ChannelBuffers.wrappedBuffer( bytes );
			  wrappedBuffer.resetWriterIndex();
			  BlockLogBuffer buffer = new BlockLogBuffer( wrappedBuffer, ( new Monitors() ).newMonitor(typeof(ByteCounterMonitor)) );

			  sbyte[] bytesValue = new sbyte[255];
			  bytesValue[0] = 1;
			  bytesValue[254] = -1;
			  buffer.Put( bytesValue, bytesValue.Length );
			  buffer.Dispose();

			  ReadableByteChannel reader = new BlockLogReader( wrappedBuffer );
			  ByteBuffer verificationBuffer = ByteBuffer.wrap( new sbyte[1000] );
			  reader.read( verificationBuffer );
			  verificationBuffer.flip();
			  sbyte[] actualBytes = new sbyte[bytesValue.Length];
			  verificationBuffer.get( actualBytes );
			  assertThat( actualBytes, new ArrayMatches<>( this, bytesValue ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canWriteLargestAtomAfterFillingBuffer()
		 public virtual void CanWriteLargestAtomAfterFillingBuffer()
		 {
			  sbyte[] bytes = new sbyte[300];
			  ChannelBuffer wrappedBuffer = ChannelBuffers.wrappedBuffer( bytes );
			  wrappedBuffer.resetWriterIndex();
			  BlockLogBuffer buffer = new BlockLogBuffer( wrappedBuffer, ( new Monitors() ).newMonitor(typeof(ByteCounterMonitor)) );

			  sbyte[] bytesValue = new sbyte[255];
			  bytesValue[0] = 1;
			  bytesValue[254] = -1;
			  long longValue = 123456;
			  buffer.Put( bytesValue, bytesValue.Length );
			  buffer.PutLong( longValue );
			  buffer.Dispose();

			  ByteBuffer verificationBuffer = ByteBuffer.wrap( bytes );
			  assertEquals( ( sbyte ) 0, verificationBuffer.get() );
			  sbyte[] actualBytes = new sbyte[bytesValue.Length];
			  verificationBuffer.get( actualBytes );
			  assertThat( actualBytes, new ArrayMatches<>( this, bytesValue ) );
			  assertEquals( ( sbyte ) 8, verificationBuffer.get() );
			  assertEquals( longValue, verificationBuffer.Long );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canWriteReallyLargeByteArray()
		 public virtual void CanWriteReallyLargeByteArray()
		 {
			  sbyte[] bytes = new sbyte[650];
			  ChannelBuffer wrappedBuffer = ChannelBuffers.wrappedBuffer( bytes );
			  wrappedBuffer.resetWriterIndex();
			  BlockLogBuffer buffer = new BlockLogBuffer( wrappedBuffer, ( new Monitors() ).newMonitor(typeof(ByteCounterMonitor)) );

			  sbyte[] bytesValue = new sbyte[600];
			  bytesValue[1] = 1;
			  bytesValue[99] = 2;
			  bytesValue[199] = 3;
			  bytesValue[299] = 4;
			  bytesValue[399] = 5;
			  bytesValue[499] = 6;
			  bytesValue[599] = 7;
			  buffer.Put( bytesValue, bytesValue.Length );
			  buffer.Dispose();

			  sbyte[] actual;
			  ByteBuffer verificationBuffer = ByteBuffer.wrap( bytes );
			  assertEquals( ( sbyte ) 0, verificationBuffer.get() );
			  actual = new sbyte[255];
			  verificationBuffer.get( actual );
			  assertThat( actual, new ArrayMatches<>( this, Arrays.copyOfRange( bytesValue, 0, 255 ) ) );
			  assertEquals( ( sbyte ) 0, verificationBuffer.get() );
			  actual = new sbyte[255];
			  verificationBuffer.get( actual );
			  assertThat( actual, new ArrayMatches<>( this, Arrays.copyOfRange( bytesValue, 255, 510 ) ) );
			  assertEquals( ( sbyte ) 90, verificationBuffer.get() );
			  actual = new sbyte[90];
			  verificationBuffer.get( actual );
			  assertThat( actual, new ArrayMatches<>( this, Arrays.copyOfRange( bytesValue, 510, 600 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canReaderReallyLargeByteArray()
		 public virtual void CanReaderReallyLargeByteArray()
		 {
			  sbyte[] bytes = new sbyte[650];
			  ChannelBuffer wrappedBuffer = ChannelBuffers.wrappedBuffer( bytes );
			  wrappedBuffer.resetWriterIndex();
			  BlockLogBuffer buffer = new BlockLogBuffer( wrappedBuffer, ( new Monitors() ).newMonitor(typeof(ByteCounterMonitor)) );

			  sbyte[] bytesValue = new sbyte[600];
			  bytesValue[1] = 1;
			  bytesValue[99] = 2;
			  bytesValue[199] = 3;
			  bytesValue[299] = 4;
			  bytesValue[399] = 5;
			  bytesValue[499] = 6;
			  bytesValue[599] = 7;
			  buffer.Put( bytesValue, bytesValue.Length );
			  buffer.Dispose();

			  sbyte[] actual;
			  BlockLogReader reader = new BlockLogReader( wrappedBuffer );
			  ByteBuffer verificationBuffer = ByteBuffer.wrap( new sbyte[1000] );
			  reader.Read( verificationBuffer );
			  verificationBuffer.flip();
			  actual = new sbyte[255];
			  verificationBuffer.get( actual );
			  assertThat( actual, new ArrayMatches<>( this, Arrays.copyOfRange( bytesValue, 0, 255 ) ) );
			  actual = new sbyte[255];
			  verificationBuffer.get( actual );
			  assertThat( actual, new ArrayMatches<>( this, Arrays.copyOfRange( bytesValue, 255, 510 ) ) );
			  actual = new sbyte[90];
			  verificationBuffer.get( actual );
			  assertThat( actual, new ArrayMatches<>( this, Arrays.copyOfRange( bytesValue, 510, 600 ) ) );
		 }

		 private class ArrayMatches<T> : BaseMatcher<T>
		 {
			 private readonly TestBlockLogBuffer _outerInstance;

			  internal readonly T Expected;
			  internal object Actual;

			  internal ArrayMatches( TestBlockLogBuffer outerInstance, T expected )
			  {
				  this._outerInstance = outerInstance;
					this.Expected = expected;
			  }

			  public override bool Matches( object actual )
			  {
					this.Actual = actual;
					if ( Expected is sbyte[] && actual is sbyte[] )
					{
						 return Arrays.Equals( ( sbyte[] ) actual, ( sbyte[] ) Expected );
					}
					else if ( Expected is char[] && actual is char[] )
					{
						 return Arrays.Equals( ( char[] ) actual, ( char[] ) Expected );
					}
					return false;
			  }

			  public override void DescribeTo( Description descr )
			  {
					descr.appendText( string.Format( "expected {0}, got {1}", ToString( Expected ), ToString( Actual ) ) );
			  }

			  internal virtual string ToString( object value )
			  {
					if ( value is sbyte[] )
					{
						 return Arrays.ToString( ( sbyte[] ) value ) + "; len=" + ( ( sbyte[] ) value ).Length;
					}
					if ( value is char[] )
					{
						 return Arrays.ToString( ( char[] ) value ) + "; len=" + ( ( char[] ) value ).Length;
					}
					return "" + value;
			  }
		 }
	}

}