using System;
using System.Collections.Generic;
using System.Text;

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
namespace Neo4Net.CommandLine.dbms
{
	using JProcesses = org.jutils.jprocesses.JProcesses;
	using ProcessInfo = org.jutils.jprocesses.model.ProcessInfo;


	using AdminCommand = Neo4Net.CommandLine.Admin.AdminCommand;
	using CommandFailed = Neo4Net.CommandLine.Admin.CommandFailed;
	using IncorrectUsage = Neo4Net.CommandLine.Admin.IncorrectUsage;
	using OutsideWorld = Neo4Net.CommandLine.Admin.OutsideWorld;
	using Usage = Neo4Net.CommandLine.Admin.Usage;
	using Arguments = Neo4Net.CommandLine.Args.Arguments;
	using MandatoryNamedArg = Neo4Net.CommandLine.Args.MandatoryNamedArg;
	using OptionalNamedArg = Neo4Net.CommandLine.Args.OptionalNamedArg;
	using PositionalArgument = Neo4Net.CommandLine.Args.PositionalArgument;
	using OptionalCanonicalPath = Neo4Net.CommandLine.Args.Common.OptionalCanonicalPath;
	using JMXDumper = Neo4Net.Dbms.diagnostics.jmx.JMXDumper;
	using JmxDump = Neo4Net.Dbms.diagnostics.jmx.JmxDump;
	using DiagnosticsReportSource = Neo4Net.Diagnostics.DiagnosticsReportSource;
	using DiagnosticsReportSources = Neo4Net.Diagnostics.DiagnosticsReportSources;
	using DiagnosticsReporter = Neo4Net.Diagnostics.DiagnosticsReporter;
	using DiagnosticsReporterProgress = Neo4Net.Diagnostics.DiagnosticsReporterProgress;
	using InteractiveProgress = Neo4Net.Diagnostics.InteractiveProgress;
	using NonInteractiveProgress = Neo4Net.Diagnostics.NonInteractiveProgress;
	using Args = Neo4Net.Helpers.Args;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using Config = Neo4Net.Kernel.configuration.Config;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.commons.text.StringEscapeUtils.escapeCsv;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.factory.GraphDatabaseSettings.database_path;

	public class DiagnosticsReportCommand : AdminCommand
	{
		 private static readonly OptionalNamedArg _destinationArgument = new OptionalCanonicalPath( "to", System.getProperty( "java.io.tmpdir" ), "reports" + File.separator, "Destination directory for reports" );
		 public const string PID_KEY = "pid";
		 private const long NO_PID = 0;
		 private static readonly Arguments _arguments = new Arguments().withArgument(new OptionalListArgument()).withArgument(_destinationArgument).withArgument(new OptionalVerboseArgument()).withArgument(new OptionalForceArgument()).withArgument(new OptionalNamedArg(PID_KEY, "1234", "", "Specify process id of running Neo4Net instance")).withPositionalArgument(new ClassifierFiltersArgument());

		 private readonly Path _homeDir;
		 private readonly Path _configDir;
		 internal static readonly string[] DefaultClassifiers = new string[]{ "logs", "config", "plugins", "tree", "metrics", "threads", "env", "sysprop", "ps" };

		 private JMXDumper _jmxDumper;
		 private bool _verbose;
		 private readonly PrintStream @out;
		 private readonly FileSystemAbstraction _fs;
		 private readonly PrintStream _err;
		 private static readonly DateTimeFormatter _filenameDateTimeFormatter = new DateTimeFormatterBuilder().appendPattern("yyyy-MM-dd_HHmmss").toFormatter();
		 private long _pid;

		 internal DiagnosticsReportCommand( Path homeDir, Path configDir, OutsideWorld outsideWorld )
		 {
			  this._homeDir = homeDir;
			  this._configDir = configDir;
			  this._fs = outsideWorld.FileSystem();
			  this.@out = outsideWorld.OutStream();
			  _err = outsideWorld.ErrorStream();
		 }

		 public static Arguments AllArguments()
		 {
			  return _arguments;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void execute(String[] stringArgs) throws org.Neo4Net.commandline.admin.IncorrectUsage, org.Neo4Net.commandline.admin.CommandFailed
		 public override void Execute( string[] stringArgs )
		 {
			  Args args = Args.withFlags( "list", "to", "verbose", "force", PID_KEY ).parse( stringArgs );
			  _verbose = args.Has( "verbose" );
			  _jmxDumper = new JMXDumper( _homeDir, _fs, @out, _err, _verbose );
			  _pid = ParsePid( args );
			  bool force = args.Has( "force" );

			  DiagnosticsReporter reporter = CreateAndRegisterSources();

			  Optional<ISet<string>> classifiers = ParseAndValidateArguments( args, reporter );
			  if ( !classifiers.Present )
			  {
					return;
			  }

			  DiagnosticsReporterProgress progress = BuildProgress();

			  // Start dumping
			  Path destinationDir = ( new File( _destinationArgument.parse( args ) ) ).toPath();
			  try
			  {
					Path reportFile = destinationDir.resolve( DefaultFilename );
					@out.println( "Writing report to " + reportFile.toAbsolutePath().ToString() );
					reporter.Dump( classifiers.get(), reportFile, progress, force );
			  }
			  catch ( IOException e )
			  {
					throw new CommandFailed( "Creating archive failed", e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static long parsePid(org.Neo4Net.helpers.Args args) throws org.Neo4Net.commandline.admin.CommandFailed
		 private static long ParsePid( Args args )
		 {
			  if ( args.Has( PID_KEY ) )
			  {
					try
					{
						 return long.Parse( args.Get( PID_KEY, "" ) );
					}
					catch ( System.FormatException e )
					{
						 throw new CommandFailed( "Unable to parse --" + PID_KEY, e );
					}
			  }
			  return NO_PID;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private String getDefaultFilename() throws java.net.UnknownHostException
		 private string DefaultFilename
		 {
			 get
			 {
				  string hostName = InetAddress.LocalHost.HostName;
				  string safeFilename = hostName.replaceAll( "[^a-zA-Z0-9._]+", "_" );
				  return safeFilename + "-" + DateTime.Now.format( _filenameDateTimeFormatter ) + ".zip";
			 }
		 }

		 private DiagnosticsReporterProgress BuildProgress()
		 {
			  DiagnosticsReporterProgress progress;
			  if ( System.console() != null )
			  {
					progress = new InteractiveProgress( @out, _verbose );
			  }
			  else
			  {
					progress = new NonInteractiveProgress( @out, _verbose );
			  }
			  return progress;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.Optional<java.util.Set<String>> parseAndValidateArguments(org.Neo4Net.helpers.Args args, org.Neo4Net.diagnostics.DiagnosticsReporter reporter) throws org.Neo4Net.commandline.admin.IncorrectUsage
		 private Optional<ISet<string>> ParseAndValidateArguments( Args args, DiagnosticsReporter reporter )
		 {
			  ISet<string> availableClassifiers = reporter.AvailableClassifiers;

			  // Passing '--list' should print list and end execution
			  if ( args.Has( "list" ) )
			  {
					ListClassifiers( availableClassifiers );
					return null;
			  }

			  // Make sure 'all' is the only classifier if specified
			  ISet<string> classifiers = new SortedSet<string>( args.Orphans() );
			  if ( classifiers.Contains( "all" ) )
			  {
					if ( classifiers.Count != 1 )
					{
						 classifiers.remove( "all" );
						 throw new IncorrectUsage( "If you specify 'all' this has to be the only classifier. Found ['" + string.join( "','", classifiers ) + "'] as well." );
					}
			  }
			  else
			  {
					// Add default classifiers that are available
					if ( classifiers.Count == 0 )
					{
						 AddDefaultClassifiers( availableClassifiers, classifiers );
					}

					ValidateClassifiers( availableClassifiers, classifiers );
			  }
			  return classifiers;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void validateClassifiers(java.util.Set<String> availableClassifiers, java.util.Set<String> orphans) throws org.Neo4Net.commandline.admin.IncorrectUsage
		 private void ValidateClassifiers( ISet<string> availableClassifiers, ISet<string> orphans )
		 {
			  foreach ( string classifier in orphans )
			  {
					if ( !availableClassifiers.Contains( classifier ) )
					{
						 throw new IncorrectUsage( "Unknown classifier: " + classifier );
					}
			  }
		 }

		 private void AddDefaultClassifiers( ISet<string> availableClassifiers, ISet<string> orphans )
		 {
			  foreach ( string classifier in DefaultClassifiers )
			  {
					if ( availableClassifiers.Contains( classifier ) )
					{
						 orphans.Add( classifier );
					}
			  }
		 }

		 private void ListClassifiers( ISet<string> availableClassifiers )
		 {
			  @out.println( "All available classifiers:" );
			  foreach ( string classifier in availableClassifiers )
			  {
					@out.printf( "  %-10s %s%n", classifier, DescribeClassifier( classifier ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.diagnostics.DiagnosticsReporter createAndRegisterSources() throws org.Neo4Net.commandline.admin.CommandFailed
		 private DiagnosticsReporter CreateAndRegisterSources()
		 {
			  DiagnosticsReporter reporter = new DiagnosticsReporter();
			  File configFile = _configDir.resolve( Config.DEFAULT_CONFIG_FILE_NAME ).toFile();
			  Config config = GetConfig( configFile );

			  File storeDirectory = config.Get( database_path );

			  reporter.RegisterAllOfflineProviders( config, storeDirectory, this._fs );

			  // Register sources provided by this tool
			  reporter.RegisterSource( "config", DiagnosticsReportSources.newDiagnosticsFile( "Neo4Net.conf", _fs, configFile ) );

			  reporter.RegisterSource( "ps", RunningProcesses() );

			  // Online connection
			  RegisterJMXSources( reporter );
			  return reporter;
		 }

		 private void RegisterJMXSources( DiagnosticsReporter reporter )
		 {
			  Optional<JmxDump> jmxDump;
			  if ( _pid == NO_PID )
			  {
					jmxDump = _jmxDumper.JMXDump;
			  }
			  else
			  {
					jmxDump = _jmxDumper.getJMXDump( _pid );
			  }
			  jmxDump.ifPresent(jmx =>
			  {
				reporter.RegisterSource( "threads", Jmx.threadDump() );
				reporter.RegisterSource( "heap", Jmx.heapDump() );
				reporter.RegisterSource( "sysprop", Jmx.systemProperties() );
				reporter.RegisterSource( "env", Jmx.environmentVariables() );
				reporter.RegisterSource( "activetxs", Jmx.listTransactions() );
			  });
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.kernel.configuration.Config getConfig(java.io.File configFile) throws org.Neo4Net.commandline.admin.CommandFailed
		 private Config GetConfig( File configFile )
		 {
			  if ( !_fs.fileExists( configFile ) )
			  {
					throw new CommandFailed( "Unable to find config file, tried: " + configFile.AbsolutePath );
			  }
			  try
			  {
					return Config.fromFile( configFile ).withHome( _homeDir ).withConnectorsDisabled().build();
			  }
			  catch ( Exception e )
			  {
					throw new CommandFailed( "Failed to read config file: " + configFile.AbsolutePath, e );
			  }
		 }

		 internal static string DescribeClassifier( string classifier )
		 {
			  switch ( classifier )
			  {
			  case "logs":
					return "include log files";
			  case "config":
					return "include configuration file";
			  case "plugins":
					return "include a view of the plugin directory";
			  case "tree":
					return "include a view of the tree structure of the data directory";
			  case "tx":
					return "include transaction logs";
			  case "metrics":
					return "include metrics";
			  case "threads":
					return "include a thread dump of the running instance";
			  case "heap":
					return "include a heap dump";
			  case "env":
					return "include a list of all environment variables";
			  case "sysprop":
					return "include a list of java system properties";
			  case "raft":
					return "include the raft log";
			  case "ccstate":
					return "include the current cluster state";
			  case "activetxs":
					return "include the output of dbms.listTransactions()";
			  case "ps":
					return "include a list of running processes";
			  default:
		  break;
			  }
			  throw new System.ArgumentException( "Unknown classifier: " + classifier );
		 }

		 private static DiagnosticsReportSource RunningProcesses()
		 {
			  return DiagnosticsReportSources.newDiagnosticsString("ps.csv", () =>
			  {
				IList<ProcessInfo> processesList = JProcesses.ProcessList;

				StringBuilder sb = new StringBuilder();
				sb.Append( escapeCsv( "Process PID" ) ).Append( ',' ).Append( escapeCsv( "Process Name" ) ).Append( ',' ).Append( escapeCsv( "Process Time" ) ).Append( ',' ).Append( escapeCsv( "User" ) ).Append( ',' ).Append( escapeCsv( "Virtual Memory" ) ).Append( ',' ).Append( escapeCsv( "Physical Memory" ) ).Append( ',' ).Append( escapeCsv( "CPU usage" ) ).Append( ',' ).Append( escapeCsv( "Start Time" ) ).Append( ',' ).Append( escapeCsv( "Priority" ) ).Append( ',' ).Append( escapeCsv( "Full command" ) ).Append( '\n' );

				foreach ( ProcessInfo processInfo in processesList )
				{
					 sb.Append( processInfo.Pid ).Append( ',' ).Append( escapeCsv( processInfo.Name ) ).Append( ',' ).Append( processInfo.Time ).Append( ',' ).Append( escapeCsv( processInfo.User ) ).Append( ',' ).Append( processInfo.VirtualMemory ).Append( ',' ).Append( processInfo.PhysicalMemory ).Append( ',' ).Append( processInfo.CpuUsage ).Append( ',' ).Append( processInfo.StartTime ).Append( ',' ).Append( processInfo.Priority ).Append( ',' ).Append( escapeCsv( processInfo.Command ) ).Append( '\n' );
				}
				return sb.ToString();
			  });
		 }

		 /// <summary>
		 /// Helper class to format output of <seealso cref="Usage"/>. Parsing is done manually in this command module.
		 /// </summary>
		 public class OptionalListArgument : MandatoryNamedArg
		 {
			  internal OptionalListArgument() : base("list", "", "List all available classifiers")
			  {
			  }

			  public override string OptionsListing()
			  {
					return "--list";
			  }

			  public override string Usage()
			  {
					return "[--list]";
			  }
		 }

		 /// <summary>
		 /// Helper class to format output of <seealso cref="Usage"/>. Parsing is done manually in this command module.
		 /// </summary>
		 public class OptionalVerboseArgument : MandatoryNamedArg
		 {
			  internal OptionalVerboseArgument() : base("verbose", "", "More verbose error messages")
			  {
			  }

			  public override string OptionsListing()
			  {
					return "--verbose";
			  }

			  public override string Usage()
			  {
					return "[--verbose]";
			  }
		 }

		 /// <summary>
		 /// Helper class to format output of <seealso cref="Usage"/>. Parsing is done manually in this command module.
		 /// </summary>
		 public class OptionalForceArgument : MandatoryNamedArg
		 {
			  internal OptionalForceArgument() : base("force", "", "Ignore disk full warning")
			  {
			  }

			  public override string OptionsListing()
			  {
					return "--force";
			  }

			  public override string Usage()
			  {
					return "[--force]";
			  }
		 }

		 /// <summary>
		 /// Helper class to format output of <seealso cref="Usage"/>. Parsing is done manually in this command module.
		 /// </summary>
		 public class ClassifierFiltersArgument : PositionalArgument
		 {
			  public override int Position()
			  {
					return 1;
			  }

			  public override string Usage()
			  {
					return "[all] [<classifier1> <classifier2> ...]";
			  }

			  public override string Parse( Args parsedArgs )
			  {
					throw new System.NotSupportedException( "no parser exists" );
			  }
		 }
	}

}