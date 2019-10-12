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
namespace Org.Neo4j.tools.dump
{
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;
	using Mockito = org.mockito.Mockito;


	using AbstractBaseRecord = Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord;
	using SuppressOutputExtension = Org.Neo4j.Test.extension.SuppressOutputExtension;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(SuppressOutputExtension.class) class DumpStoreTest
	internal class DumpStoreTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void dumpStoreShouldPrintBufferWithContent()
		 internal virtual void DumpStoreShouldPrintBufferWithContent()
		 {
			  // Given
			  MemoryStream outStream = new MemoryStream();
			  PrintStream @out = new PrintStream( outStream );
			  DumpStore dumpStore = new DumpStore( @out );
			  ByteBuffer buffer = ByteBuffer.allocate( 1024 );
			  for ( sbyte i = 0; i < 10; i++ )
			  {
					buffer.put( i );
			  }
			  buffer.flip();

			  AbstractBaseRecord record = Mockito.mock( typeof( AbstractBaseRecord ) );

			  // When
			  //when( record.inUse() ).thenReturn( true );
			  dumpStore.dumpHex( record, buffer, 2, 4 );

			  // Then
			  assertEquals( format( "@ 0x00000008: 00 01 02 03  04 05 06 07  08 09%n" ), outStream.ToString() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void dumpStoreShouldPrintShorterMessageForAllZeroBuffer()
		 internal virtual void DumpStoreShouldPrintShorterMessageForAllZeroBuffer()
		 {
			  // Given
			  MemoryStream outStream = new MemoryStream();
			  PrintStream @out = new PrintStream( outStream );
			  DumpStore dumpStore = new DumpStore( @out );
			  ByteBuffer buffer = ByteBuffer.allocate( 1024 );
			  AbstractBaseRecord record = Mockito.mock( typeof( AbstractBaseRecord ) );

			  // When
			  //when( record.inUse() ).thenReturn( true );
			  dumpStore.dumpHex( record, buffer, 2, 4 );

			  // Then
			  assertEquals( format( ": all zeros @ 0x8 - 0xc%n" ), outStream.ToString() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void canDumpNeoStoreFileContent() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void CanDumpNeoStoreFileContent()
		 {
			  URL neostore = this.GetType().ClassLoader.getResource("neostore");
			  string neostoreFile = neostore.File;
			  DumpStore.Main( neostoreFile );
		 }
	}

}