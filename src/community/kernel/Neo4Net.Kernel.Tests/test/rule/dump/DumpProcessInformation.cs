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
	using Matcher = org.hamcrest.Matcher;


	using Args = Neo4Net.Helpers.Args;
	using Neo4Net.Collections.Helpers;
	using FormattedLogProvider = Neo4Net.Logging.FormattedLogProvider;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.isIn;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.Format.time;

	public class DumpProcessInformation
	{
		 private const string HEAP = "heap";
		 private const string DIR = "dir";

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void main(String[] args) throws Exception
		 public static void Main( string[] args )
		 {
			  Args arg = Args.withFlags( HEAP ).parse( args == null ? new string[0] : args );
			  bool doHeapDump = arg.GetBoolean( HEAP, false, true ).Value;
			  string[] containing = arg.Orphans().ToArray();
			  string dumpDir = arg.Get( DIR, "data" );
			  ( new DumpProcessInformation( FormattedLogProvider.toOutputStream( System.out ), new File( dumpDir ) ) ).dumpRunningProcesses( doHeapDump, containing );
		 }

		 private readonly Log _log;
		 private readonly File _outputDirectory;

		 public DumpProcessInformation( LogProvider logProvider, File outputDirectory )
		 {
			  this._log = logProvider.getLog( this.GetType() );
			  this._outputDirectory = outputDirectory;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void dumpRunningProcesses(boolean includeHeapDump, String... javaPidsContainingClassNames) throws Exception
		 public virtual void DumpRunningProcesses( bool includeHeapDump, params string[] javaPidsContainingClassNames )
		 {
			  _outputDirectory.mkdirs();
			  foreach ( Pair<long, string> pid in GetJPids( isIn( javaPidsContainingClassNames ) ) )
			  {
					DoThreadDump( pid );
					if ( includeHeapDump )
					{
						 DoHeapDump( pid );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.io.File doThreadDump(org.Neo4Net.helpers.collection.Pair<long, String> pid) throws Exception
		 public virtual File DoThreadDump( Pair<long, string> pid )
		 {
			  File outputFile = new File( _outputDirectory, FileName( "threaddump", pid ) );
			  _log.info( "Creating thread dump of " + pid + " to " + outputFile.AbsolutePath );
			  string[] cmdarray = new string[] { "jstack", "" + pid.First() };
			  Process process = Runtime.Runtime.exec( cmdarray );
			  WriteProcessOutputToFile( process, outputFile );
			  return outputFile;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void doHeapDump(org.Neo4Net.helpers.collection.Pair<long, String> pid) throws Exception
		 public virtual void DoHeapDump( Pair<long, string> pid )
		 {
			  File outputFile = new File( _outputDirectory, FileName( "heapdump", pid ) );
			  _log.info( "Creating heap dump of " + pid + " to " + outputFile.AbsolutePath );
			  string[] cmdarray = new string[] { "jmap", "-dump:file=" + outputFile.AbsolutePath, "" + pid.First() };
			  Runtime.Runtime.exec( cmdarray ).waitFor();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void doThreadDump(org.hamcrest.Matcher<String> processFilter) throws Exception
		 public virtual void DoThreadDump( Matcher<string> processFilter )
		 {
			  foreach ( Pair<long, string> pid in GetJPids( processFilter ) )
			  {
					DoThreadDump( pid );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.Collection<org.Neo4Net.helpers.collection.Pair<long, String>> getJPids(org.hamcrest.Matcher<String> filter) throws Exception
		 public virtual ICollection<Pair<long, string>> GetJPids( Matcher<string> filter )
		 {
			  Process process = Runtime.Runtime.exec( new string[] { "jps", "-l" } );
			  StreamReader reader = new StreamReader( process.InputStream );
			  string line = null;
			  ICollection<Pair<long, string>> jPids = new List<Pair<long, string>>();
			  ICollection<Pair<long, string>> excludedJPids = new List<Pair<long, string>>();
			  while ( !string.ReferenceEquals( ( line = reader.ReadLine() ), null ) )
			  {
					int spaceIndex = line.IndexOf( ' ' );
					string name = line.Substring( spaceIndex + 1 );
					// Work-around for a windows problem where if your java.exe is in a directory
					// containing spaces the value in the second column from jps output will be
					// something like "C:\Program" if it was under "C:\Program Files\Java..."
					// If that's the case then use the PID instead
					if ( name.Contains( ":" ) )
					{
						 string pid = line.Substring( 0, spaceIndex );
						 name = pid;
					}

					Pair<long, string> pid = Pair.of( long.Parse( line.Substring( 0, spaceIndex ) ), name );
					if ( name.Contains( typeof( DumpProcessInformation ).Name ) || name.Contains( "Jps" ) || name.Contains( "eclipse.equinox" ) || !filter.matches( name ) )
					{
						 excludedJPids.Add( pid );
						 continue;
					}
					jPids.Add( pid );
			  }
			  process.waitFor();

			  _log.info( "Found jPids:" + jPids + ", excluded:" + excludedJPids );

			  return jPids;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeProcessOutputToFile(Process process, java.io.File outputFile) throws Exception
		 private void WriteProcessOutputToFile( Process process, File outputFile )
		 {
			  StreamReader reader = new StreamReader( process.InputStream );
			  string line = null;
			  using ( PrintStream @out = new PrintStream( outputFile ) )
			  {
					while ( !string.ReferenceEquals( ( line = reader.ReadLine() ), null ) )
					{
						 @out.println( line );
					}
			  }
			  process.waitFor();
		 }

		 private static string FileName( string category, Pair<long, string> pid )
		 {
			  return time().replace(':', '_').replace('.', '_') +
						 "-" + category +
						 "-" + pid.First() +
						 "-" + pid.Other();
		 }
	}

}