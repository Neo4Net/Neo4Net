using System;
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
namespace Neo4Net.Dbms.diagnostics.jmx
{

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.commandline.dbms.DiagnosticsReportCommand.PID_KEY;

	/// <summary>
	/// Facilitates JMX Dump for current running Neo4j instance.
	/// </summary>
	public class JMXDumper
	{
		 private readonly Path _homeDir;
		 private readonly FileSystemAbstraction _fs;
		 private readonly PrintStream _err;
		 private readonly bool _verbose;
		 private PrintStream @out;

		 public JMXDumper( Path homeDir, FileSystemAbstraction fs, PrintStream @out, PrintStream err, bool verbose )
		 {
			  this._homeDir = homeDir;
			  this._fs = fs;
			  this._err = err;
			  this._verbose = verbose;
			  this.@out = @out;
		 }

		 public virtual Optional<JmxDump> JMXDump
		 {
			 get
			 {
				  @out.println( "Finding running instance of neo4j" );
   
				  long? pid = Pid;
				  if ( pid.HasValue )
				  {
						return GetJMXDump( pid.Value );
				  }
				  else
				  {
						@out.println( "No running instance of neo4j was found. Online reports will be omitted." );
						@out.println( "If neo4j is running but not detected, you can supply the process id of the running instance with --" + PID_KEY );
						return null;
				  }
			 }
		 }

		 public virtual Optional<JmxDump> getJMXDump( long pid )
		 {
			  try
			  {
					LocalVirtualMachine vm = LocalVirtualMachine.From( pid );
					@out.println( "Attached to running process with process id " + pid );
					try
					{
						 JmxDump jmxDump = JmxDump.ConnectTo( vm.JmxAddress );
						 jmxDump.AttachSystemProperties( vm.SystemProperties );
						 @out.println( "Connected to JMX endpoint" );
						 return jmxDump;
					}
					catch ( IOException e )
					{
						 PrintError( "Unable to communicate with JMX endpoint. Reason: " + e.Message, e );
					}
			  }
			  catch ( java.lang.NoClassDefFoundError e )
			  {
					PrintError( "Unable to attach to process. Reason: JDK is not available, please point " + "environment variable JAVA_HOME to a valid JDK location.", e );
			  }
			  catch ( IOException e )
			  {
					PrintError( "Unable to connect to process with process id " + pid + ". Reason: " + e.Message, e );
			  }

			  return null;
		 }

		 private void PrintError( string message, Exception e )
		 {
			  _err.println( message );
			  if ( _verbose && e != null )
			  {
					e.printStackTrace( _err );
			  }
		 }

		 private void PrintError( string message )
		 {
			  PrintError( message, null );
		 }

		 private long? Pid
		 {
			 get
			 {
				  Path pidFile = this._homeDir.resolve( "run/neo4j.pid" );
				  if ( this._fs.fileExists( pidFile.toFile() ) )
				  {
						// The file cannot be opened with write permissions on Windows
						try
						{
								using ( Stream inputStream = Files.newInputStream( pidFile, StandardOpenOption.READ ), StreamReader reader = new StreamReader( inputStream ) )
								{
								 string pidFileContent = reader.ReadLine();
								 try
								 {
									  return long.Parse( pidFileContent );
								 }
         
								 catch ( System.FormatException )
								 {
									  PrintError( pidFile.ToString() + " does not contain a valid id. Found: " + pidFileContent );
								 }
								}
						}
						catch ( IOException e )
						{
							 PrintError( "Error reading the .pid file. Reason: " + e.Message, e );
						}
				  }
				  return null;
			 }
		 }
	}

}