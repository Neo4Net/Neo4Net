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
namespace Neo4Net.Kernel.impl.transaction
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Neo4Net.Collections.Helpers;
	using OpenMode = Neo4Net.Io.fs.OpenMode;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using LogVersionBridge = Neo4Net.Kernel.impl.transaction.log.LogVersionBridge;
	using LogVersionedStoreChannel = Neo4Net.Kernel.impl.transaction.log.LogVersionedStoreChannel;
	using PhysicalLogVersionedStoreChannel = Neo4Net.Kernel.impl.transaction.log.PhysicalLogVersionedStoreChannel;
	using ReadAheadLogChannel = Neo4Net.Kernel.impl.transaction.log.ReadAheadLogChannel;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.transaction.log.LogVersionBridge_Fields.NO_MORE_CHANNELS;

	public class ReadAheadLogChannelTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.fs.DefaultFileSystemRule fileSystemRule = new org.Neo4Net.test.rule.fs.DefaultFileSystemRule();
		 public readonly DefaultFileSystemRule FileSystemRule = new DefaultFileSystemRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.TestDirectory directory = org.Neo4Net.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory Directory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadFromSingleChannel() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadFromSingleChannel()
		 {
			  // GIVEN
			  File file = file( 0 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte byteValue = (byte) 5;
			  sbyte byteValue = ( sbyte ) 5;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final short shortValue = (short) 56;
			  short shortValue = ( short ) 56;
			  const int intValue = 32145;
			  const long longValue = 5689456895869L;
			  const float floatValue = 12.12345f;
			  const double doubleValue = 3548.45748D;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte[] byteArrayValue = new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9};
			  sbyte[] byteArrayValue = new sbyte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
			  WriteSomeData(file, element =>
			  {
				element.put( byteValue );
				element.putShort( shortValue );
				element.putInt( intValue );
				element.putLong( longValue );
				element.putFloat( floatValue );
				element.putDouble( doubleValue );
				element.put( byteArrayValue );
				return true;
			  });

			  StoreChannel storeChannel = FileSystemRule.get().open(file, OpenMode.READ);
			  PhysicalLogVersionedStoreChannel versionedStoreChannel = new PhysicalLogVersionedStoreChannel( storeChannel, -1, ( sbyte ) - 1 );
			  using ( ReadAheadLogChannel channel = new ReadAheadLogChannel( versionedStoreChannel, NO_MORE_CHANNELS, 16 ) )
			  {
					// THEN
					assertEquals( byteValue, channel.Get() );
					assertEquals( shortValue, channel.Short );
					assertEquals( intValue, channel.Int );
					assertEquals( longValue, channel.Long );
					assertEquals( floatValue, channel.Float, 0.1f );
					assertEquals( doubleValue, channel.Double, 0.1d );

					sbyte[] bytes = new sbyte[byteArrayValue.Length];
					channel.Get( bytes, byteArrayValue.Length );
					assertArrayEquals( byteArrayValue, bytes );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadFromMultipleChannels() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadFromMultipleChannels()
		 {
			  // GIVEN
			  WriteSomeData(File(0), element =>
			  {
				for ( int i = 0; i < 10; i++ )
				{
					 element.putLong( i );
				}
				return true;
			  });
			  WriteSomeData(File(1), element =>
			  {
				for ( int i = 10; i < 20; i++ )
				{
					 element.putLong( i );
				}
				return true;
			  });

			  StoreChannel storeChannel = FileSystemRule.get().open(File(0), OpenMode.READ);
			  PhysicalLogVersionedStoreChannel versionedStoreChannel = new PhysicalLogVersionedStoreChannel( storeChannel, -1, ( sbyte ) - 1 );
			  try (ReadAheadLogChannel channel = new ReadAheadLogChannel(versionedStoreChannel, new LogVersionBridgeAnonymousInnerClass(this)
			 , 10))
			 {
					// THEN
					for ( long i = 0; i < 20; i++ )
					{
						 assertEquals( i, channel.Long );
					}
			 }
		 }

		 private class LogVersionBridgeAnonymousInnerClass : LogVersionBridge
		 {
			 private readonly ReadAheadLogChannelTest _outerInstance;

			 public LogVersionBridgeAnonymousInnerClass( ReadAheadLogChannelTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 private bool returned;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.kernel.impl.transaction.log.LogVersionedStoreChannel next(org.Neo4Net.kernel.impl.transaction.log.LogVersionedStoreChannel channel) throws java.io.IOException
			 public LogVersionedStoreChannel next( LogVersionedStoreChannel channel )
			 {
				  if ( !returned )
				  {
						returned = true;
						channel.close();
						return new PhysicalLogVersionedStoreChannel( _outerInstance.fileSystemRule.get().open(_outerInstance.file(1), OpenMode.READ), -1, (sbyte) -1 );
				  }
				  return channel;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeSomeData(java.io.File file, org.Neo4Net.helpers.collection.Visitor<ByteBuffer, java.io.IOException> visitor) throws java.io.IOException
		 private void WriteSomeData( File file, Visitor<ByteBuffer, IOException> visitor )
		 {
			  using ( StoreChannel channel = FileSystemRule.get().open(file, OpenMode.READ_WRITE) )
			  {
					ByteBuffer buffer = ByteBuffer.allocate( 1024 );
					visitor.Visit( buffer );
					buffer.flip();
					channel.write( buffer );
			  }
		 }

		 private File File( int index )
		 {
			  return new File( Directory.directory(), "" + index );
		 }
	}

}