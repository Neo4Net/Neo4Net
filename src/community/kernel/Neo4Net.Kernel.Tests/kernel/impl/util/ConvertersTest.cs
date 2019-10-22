using System;
using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.util
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Neo4Net.Helpers.Collections;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.util.Converters.regexFiles;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.util.Converters.toOptionalHostnamePortFromRawAddress;

	public class ConvertersTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.TestDirectory directory = org.Neo4Net.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory Directory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSortFilesByNumberCleverly() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSortFilesByNumberCleverly()
		 {
			  // GIVEN
			  File file1 = ExistenceOfFile( "file1" );
			  File file123 = ExistenceOfFile( "file123" );
			  File file12 = ExistenceOfFile( "file12" );
			  File file2 = ExistenceOfFile( "file2" );
			  File file32 = ExistenceOfFile( "file32" );

			  // WHEN
			  File[] files = regexFiles( true ).apply( Directory.file( "file.*" ).AbsolutePath );

			  // THEN
			  assertArrayEquals( new File[]{ file1, file2, file12, file32, file123 }, files );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canProcessPortFromAGivenString()
		 public virtual void CanProcessPortFromAGivenString()
		 {
			  // given
			  string addressWithPorts = "hostname:1234";

			  // when
			  int? port = toOptionalHostnamePortFromRawAddress( addressWithPorts ).Port;

			  // then
			  assertTrue( port.HasValue );
			  assertEquals( Convert.ToInt32( 1234 ), port.Value );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void emptyOptionalWhenPortIsMissing()
		 public virtual void EmptyOptionalWhenPortIsMissing()
		 {
			  //given
			  string addressWithoutPorts = "hostname";

			  // when
			  int? port = toOptionalHostnamePortFromRawAddress( addressWithoutPorts ).Port;

			  // then
			  assertFalse( port.HasValue );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canProcessHostnameFromAGivenAddress()
		 public virtual void CanProcessHostnameFromAGivenAddress()
		 {
			  // given
			  string addressWithPorts = "hostname:1234";

			  // when
			  Optional<string> hostname = toOptionalHostnamePortFromRawAddress( addressWithPorts ).Hostname;

			  // then
			  assertTrue( hostname.Present );
			  assertEquals( "hostname", hostname.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canProcessHostnameWithoutPort()
		 public virtual void CanProcessHostnameWithoutPort()
		 {
			  // given
			  string addressWithoutPort = "hostname";

			  // when
			  Optional<string> hostname = toOptionalHostnamePortFromRawAddress( addressWithoutPort ).Hostname;

			  // then
			  assertTrue( hostname.Present );
			  assertEquals( "hostname", hostname.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void emptyOptionalWhenOnlyPort()
		 public virtual void EmptyOptionalWhenOnlyPort()
		 {
			  // given
			  string portOnlyAddress = ":1234";

			  // when
			  Optional<string> hostname = toOptionalHostnamePortFromRawAddress( portOnlyAddress ).Hostname;

			  // then
			  assertFalse( hostname.Present );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void ipv6Works()
		 public virtual void Ipv6Works()
		 {
			  // with
			  string full = "1234:5678:9abc:def0:1234:5678:9abc:def0";
			  IList<Pair<string, OptionalHostnamePort>> cases = Arrays.asList( Pair.of( "[::1]", new OptionalHostnamePort( "::1", null, null ) ), Pair.of( "[3FFe::1]", new OptionalHostnamePort( "3FFe::1", null, null ) ), Pair.of( "[::1]:2", new OptionalHostnamePort( "::1", 2, 2 ) ), Pair.of( "[" + full + "]", new OptionalHostnamePort( full, null, null ) ), Pair.of( "[" + full + "]" + ":5432", new OptionalHostnamePort( full, 5432, 5432 ) ), Pair.of( "[1::2]:3-4", new OptionalHostnamePort( "1::2", 3, 4 ) ) );
			  foreach ( Pair<string, OptionalHostnamePort> useCase in cases )
			  {
					// given
					string caseInput = useCase.First();
					OptionalHostnamePort caseOutput = useCase.Other();

					// when
					OptionalHostnamePort optionalHostnamePort = toOptionalHostnamePortFromRawAddress( caseInput );

					// then
					string msg = string.Format( "\"{0}\" -> {1}", caseInput, caseOutput );
					assertEquals( msg, caseOutput.Hostname, optionalHostnamePort.Hostname );
					assertEquals( msg, caseOutput.Port, optionalHostnamePort.Port );
					assertEquals( msg, caseOutput.UpperRangePort, optionalHostnamePort.UpperRangePort );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void trailingColonIgnored()
		 public virtual void TrailingColonIgnored()
		 {
			  // when
			  OptionalHostnamePort optionalHostnamePort = toOptionalHostnamePortFromRawAddress( "localhost::" );

			  // then
			  assertEquals( "localhost", optionalHostnamePort.Hostname.get() );
			  assertFalse( optionalHostnamePort.Port.HasValue );
			  assertFalse( optionalHostnamePort.UpperRangePort.HasValue );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File existenceOfFile(String name) throws java.io.IOException
		 private File ExistenceOfFile( string name )
		 {
			  File file = Directory.file( name );
			  file.createNewFile();
			  return file;
		 }
	}

}