using System;
using System.Collections.Generic;
using System.IO;

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
namespace Neo4Net.Test.rule.dump
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using Neo4Net.Helpers.Collections;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assume.assumeThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assume.assumeTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.proc.ProcessUtil.getClassPath;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.proc.ProcessUtil.getJavaExecutable;

	public class DumpProcessInformationTest
	{
		 private const string SIGNAL = "here";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.TestDirectory testDirectory = org.Neo4Net.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void checkEnvironment()
		 public virtual void CheckEnvironment()
		 {
			  assumeTrue( CommandExists( "jps" ) );
			  assumeTrue( CommandExists( "jstack -h" ) );
		 }

		 private bool CommandExists( string command )
		 {
			  try
			  {
					return Runtime.Runtime.exec( command ).waitFor() == 0;
			  }
			  catch ( Exception )
			  {
					return false;
			  }
		 }
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDumpProcessInformation() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDumpProcessInformation()
		 {
			  // GIVEN
			  File directory = TestDirectory.directory( "dump" );
			  // a process spawned from this test which pauses at a specific point of execution
			  string java = JavaExecutable.ToString();
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  Process process = Runtime.exec( new string[] { java, "-cp", ClassPath, typeof( DumpableProcess ).FullName, SIGNAL } );
			  AwaitSignal( process );

			  // WHEN
			  // dumping process information for that spawned process (knowing it's in the expected position)
			  DumpProcessInformation dumper = new DumpProcessInformation( NullLogProvider.Instance, directory );
			  ICollection<Pair<long, string>> pids = dumper.GetJPids( containsString( typeof( DumpableProcess ).Name ) );

			  // bail if our Java installation is wonky and `jps` doesn't work
			  assumeThat( pids.Count, greaterThan( 0 ) );

			  Pair<long, string> pid = Iterables.single( pids );
			  File threaddumpFile = dumper.DoThreadDump( pid );
			  process.destroy();

			  // THEN
			  // the produced thread dump should contain that expected method at least
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  assertTrue( FileContains( threaddumpFile, "traceableMethod", typeof( DumpableProcess ).FullName ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static boolean fileContains(java.io.File file, String... expectedStrings) throws java.io.IOException
		 private static bool FileContains( File file, params string[] expectedStrings )
		 {
			  ISet<string> expectedStringSet = asSet( expectedStrings );
			  using ( Stream<string> lines = Files.lines( file.toPath() ) )
			  {
					lines.forEach( line => expectedStringSet.removeIf( line.contains ) );
			  }
			  return expectedStringSet.Count == 0;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void awaitSignal(Process process) throws java.io.IOException
		 private static void AwaitSignal( Process process )
		 {
			  using ( StreamReader reader = new StreamReader( process.InputStream ) )
			  {
					string line = reader.ReadLine();
					if ( !SIGNAL.Equals( line ) )
					{
						 fail( "Got weird signal " + line );
					}
					// We got signal, great
			  }
		 }
	}

}