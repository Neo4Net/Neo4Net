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
namespace Neo4Net.Kernel.Impl.Index.Schema
{
	using Test = org.junit.jupiter.api.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.index.Internal.gbptree.GBPTree.NO_HEADER_READER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.index.schema.NativeIndexPopulator.BYTE_FAILED;

	internal class NativeIndexHeaderReaderTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustReportFailedIfNoHeader()
		 internal virtual void MustReportFailedIfNoHeader()
		 {
			  ByteBuffer emptyBuffer = ByteBuffer.wrap( new sbyte[0] );
			  NativeIndexHeaderReader nativeIndexHeaderReader = new NativeIndexHeaderReader( NO_HEADER_READER );
			  nativeIndexHeaderReader.Read( emptyBuffer );
			  assertSame( BYTE_FAILED, nativeIndexHeaderReader.State );
			  assertThat( nativeIndexHeaderReader.FailureMessage, containsString( "Could not read header, most likely caused by index not being fully constructed. Index needs to be recreated. Stacktrace:" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustReportFailedIfHeaderTooShort()
		 internal virtual void MustReportFailedIfHeaderTooShort()
		 {
			  ByteBuffer emptyBuffer = ByteBuffer.wrap( new sbyte[1] );
			  NativeIndexHeaderReader nativeIndexHeaderReader = new NativeIndexHeaderReader( ByteBuffer.get );
			  nativeIndexHeaderReader.Read( emptyBuffer );
			  assertSame( BYTE_FAILED, nativeIndexHeaderReader.State );
			  assertThat( nativeIndexHeaderReader.FailureMessage, containsString( "Could not read header, most likely caused by index not being fully constructed. Index needs to be recreated. Stacktrace:" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustNotThrowIfHeaderLongEnough()
		 internal virtual void MustNotThrowIfHeaderLongEnough()
		 {
			  ByteBuffer emptyBuffer = ByteBuffer.wrap( new sbyte[1] );
			  NativeIndexHeaderReader nativeIndexHeaderReader = new NativeIndexHeaderReader( NO_HEADER_READER );
			  nativeIndexHeaderReader.Read( emptyBuffer );
		 }
	}

}