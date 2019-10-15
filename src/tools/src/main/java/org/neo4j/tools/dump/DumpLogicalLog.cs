using System;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.tools.dump
{

	using Args = Neo4Net.Helpers.Args;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using NodeCommand = Neo4Net.Kernel.impl.transaction.command.Command.NodeCommand;
	using PropertyCommand = Neo4Net.Kernel.impl.transaction.command.Command.PropertyCommand;
	using RelationshipCommand = Neo4Net.Kernel.impl.transaction.command.Command.RelationshipCommand;
	using RelationshipGroupCommand = Neo4Net.Kernel.impl.transaction.command.Command.RelationshipGroupCommand;
	using SchemaRuleCommand = Neo4Net.Kernel.impl.transaction.command.Command.SchemaRuleCommand;
	using LogPosition = Neo4Net.Kernel.impl.transaction.log.LogPosition;
	using CheckPoint = Neo4Net.Kernel.impl.transaction.log.entry.CheckPoint;
	using InvalidLogEntryHandler = Neo4Net.Kernel.impl.transaction.log.entry.InvalidLogEntryHandler;
	using LogEntry = Neo4Net.Kernel.impl.transaction.log.entry.LogEntry;
	using LogEntryCommand = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryCommand;
	using LogHeader = Neo4Net.Kernel.impl.transaction.log.entry.LogHeader;
	using LogHeaderReader = Neo4Net.Kernel.impl.transaction.log.entry.LogHeaderReader;
	using StorageCommand = Neo4Net.Storageengine.Api.StorageCommand;
	using Monitor = Neo4Net.tools.dump.TransactionLogAnalyzer.Monitor;
	using ReportInconsistencies = Neo4Net.tools.dump.inconsistency.ReportInconsistencies;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Format.DEFAULT_TIME_ZONE;

	/// <summary>
	/// Tool to represent logical logs in readable format for further analysis.
	/// </summary>
	public class DumpLogicalLog
	{
		 private const string TO_FILE = "tofile";
		 private const string TX_FILTER = "txfilter";
		 private const string CC_FILTER = "ccfilter";
		 private const string LENIENT = "lenient";

		 private readonly FileSystemAbstraction _fileSystem;

		 public DumpLogicalLog( FileSystemAbstraction fileSystem )
		 {
			  this._fileSystem = fileSystem;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void dump(String filenameOrDirectory, java.io.PrintStream out, System.Predicate<org.neo4j.kernel.impl.transaction.log.entry.LogEntry[]> filter, System.Func<org.neo4j.kernel.impl.transaction.log.entry.LogEntry,String> serializer, org.neo4j.kernel.impl.transaction.log.entry.InvalidLogEntryHandler invalidLogEntryHandler) throws java.io.IOException
		 public virtual void Dump( string filenameOrDirectory, PrintStream @out, System.Predicate<LogEntry[]> filter, System.Func<LogEntry, string> serializer, InvalidLogEntryHandler invalidLogEntryHandler )
		 {
			  TransactionLogAnalyzer.Analyze( _fileSystem, new File( filenameOrDirectory ), invalidLogEntryHandler, new MonitorAnonymousInnerClass( this, @out, filter, serializer ) );
		 }

		 private class MonitorAnonymousInnerClass : Monitor
		 {
			 private readonly DumpLogicalLog _outerInstance;

			 private PrintStream @out;
			 private System.Predicate<LogEntry[]> _filter;
			 private System.Func<LogEntry, string> _serializer;

			 public MonitorAnonymousInnerClass( DumpLogicalLog outerInstance, PrintStream @out, System.Predicate<LogEntry[]> filter, System.Func<LogEntry, string> serializer )
			 {
				 this.outerInstance = outerInstance;
				 this.@out = @out;
				 this._filter = filter;
				 this._serializer = serializer;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void logFile(java.io.File file, long logVersion) throws java.io.IOException
			 public void logFile( File file, long logVersion )
			 {
				  LogHeader logHeader = LogHeaderReader.readLogHeader( _outerInstance.fileSystem, file );
				  @out.println( "=== " + file.AbsolutePath + "[" + logHeader + "] ===" );
			 }

			 public void transaction( LogEntry[] transactionEntries )
			 {
				  if ( _filter == null || _filter( transactionEntries ) )
				  {
						foreach ( LogEntry entry in transactionEntries )
						{
							 @out.println( _serializer( entry ) );
						}
				  }
			 }

			 public void checkpoint( CheckPoint checkpoint, LogPosition checkpointEntryPosition )
			 {
				  if ( _filter == null || _filter( new LogEntry[] { checkpoint } ) )
				  {
						@out.println( _serializer( checkpoint ) );
				  }
			 }
		 }

		 private class TransactionRegexCriteria : System.Predicate<LogEntry[]>
		 {
			  internal readonly Pattern Pattern;
			  internal readonly TimeZone TimeZone;

			  internal TransactionRegexCriteria( string regex, TimeZone timeZone )
			  {
					this.Pattern = Pattern.compile( regex );
					this.TimeZone = timeZone;
			  }

			  public override bool Test( LogEntry[] transaction )
			  {
					foreach ( LogEntry entry in transaction )
					{
						 if ( Pattern.matcher( entry.ToString( TimeZone ) ).find() )
						 {
							  return true;
						 }
					}
					return false;
			  }
		 }

		 public class ConsistencyCheckOutputCriteria : System.Predicate<LogEntry[]>, System.Func<LogEntry, string>
		 {
			  internal readonly TimeZone TimeZone;
			  internal readonly ReportInconsistencies Inconsistencies;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public ConsistencyCheckOutputCriteria(String ccFile, java.util.TimeZone timeZone) throws java.io.IOException
			  public ConsistencyCheckOutputCriteria( string ccFile, TimeZone timeZone )
			  {
					this.TimeZone = timeZone;
					Inconsistencies = new ReportInconsistencies();
					( new InconsistencyReportReader( Inconsistencies ) ).read( new File( ccFile ) );
			  }

			  public override bool Test( LogEntry[] transaction )
			  {
					foreach ( LogEntry logEntry in transaction )
					{
						 if ( Matches( logEntry ) )
						 {
							  return true;
						 }
					}
					return false;
			  }

			  internal virtual bool Matches( LogEntry logEntry )
			  {
					if ( logEntry is LogEntryCommand )
					{
						 if ( Matches( ( ( LogEntryCommand )logEntry ).Command ) )
						 {
							  return true;
						 }
					}
					return false;
			  }

			  internal virtual bool Matches( StorageCommand command )
			  {
					if ( command is NodeCommand )
					{
						 return Inconsistencies.containsNodeId( ( ( NodeCommand ) command ).Key );
					}
					if ( command is RelationshipCommand )
					{
						 return Inconsistencies.containsRelationshipId( ( ( RelationshipCommand ) command ).Key );
					}
					if ( command is PropertyCommand )
					{
						 return Inconsistencies.containsPropertyId( ( ( PropertyCommand ) command ).Key );
					}
					if ( command is RelationshipGroupCommand )
					{
						 return Inconsistencies.containsRelationshipGroupId( ( ( RelationshipGroupCommand ) command ).Key );
					}
					if ( command is SchemaRuleCommand )
					{
						 return Inconsistencies.containsSchemaIndexId( ( ( SchemaRuleCommand ) command ).Key );
					}
					return false;
			  }

			  public override string Apply( LogEntry logEntry )
			  {
					string result = logEntry.ToString( TimeZone );
					if ( Matches( logEntry ) )
					{
						 result += "  <----";
					}
					return result;
			  }
		 }

		 /// <summary>
		 /// Usage: [--txfilter "regex"] [--ccfilter cc-report-file] [--tofile] [--lenient] storeDirOrFile1 storeDirOrFile2 ...
		 /// 
		 /// --txfilter
		 /// Will match regex against each <seealso cref="LogEntry"/> and if there is a match,
		 /// include transaction containing the LogEntry in the dump.
		 /// regex matching is done with <seealso cref="Pattern"/>
		 /// 
		 /// --ccfilter
		 /// Will look at an inconsistency report file from consistency checker and
		 /// include transactions that are relevant to them
		 /// 
		 /// --tofile
		 /// Redirects output to dump-logical-log.txt in the store directory
		 /// 
		 /// --lenient
		 /// Will attempt to read log entries even if some look broken along the way
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void main(String[] args) throws java.io.IOException
		 public static void Main( string[] args )
		 {
			  Args arguments = Args.withFlags( TO_FILE, LENIENT ).parse( args );
			  TimeZone timeZone = ParseTimeZoneConfig( arguments );
			  System.Predicate<LogEntry[]> filter = ParseFilter( arguments, timeZone );
			  System.Func<LogEntry, string> serializer = ParseSerializer( filter, timeZone );
			  System.Func<PrintStream, InvalidLogEntryHandler> invalidLogEntryHandler = ParseInvalidLogEntryHandler( arguments );
			  using ( FileSystemAbstraction fileSystem = new DefaultFileSystemAbstraction(), Printer printer = GetPrinter(arguments) )
			  {
					foreach ( string fileAsString in arguments.Orphans() )
					{
						 PrintStream @out = printer.GetFor( fileAsString );
						 ( new DumpLogicalLog( fileSystem ) ).Dump( fileAsString, @out, filter, serializer, invalidLogEntryHandler( @out ) );
					}
			  }
		 }

		 private static System.Func<PrintStream, InvalidLogEntryHandler> ParseInvalidLogEntryHandler( Args arguments )
		 {
			  if ( arguments.GetBoolean( LENIENT ) )
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
					return LenientInvalidLogEntryHandler::new;
			  }
			  return @out => InvalidLogEntryHandler.STRICT;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private static System.Func<org.neo4j.kernel.impl.transaction.log.entry.LogEntry,String> parseSerializer(System.Predicate<org.neo4j.kernel.impl.transaction.log.entry.LogEntry[]> filter, java.util.TimeZone timeZone)
		 private static System.Func<LogEntry, string> ParseSerializer( System.Predicate<LogEntry[]> filter, TimeZone timeZone )
		 {
			  if ( filter is System.Func )
			  {
					return ( System.Func<LogEntry, string> ) filter;
			  }
			  return logEntry => logEntry.ToString( timeZone );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static System.Predicate<org.neo4j.kernel.impl.transaction.log.entry.LogEntry[]> parseFilter(org.neo4j.helpers.Args arguments, java.util.TimeZone timeZone) throws java.io.IOException
		 private static System.Predicate<LogEntry[]> ParseFilter( Args arguments, TimeZone timeZone )
		 {
			  string regex = arguments.Get( TX_FILTER );
			  if ( !string.ReferenceEquals( regex, null ) )
			  {
					return new TransactionRegexCriteria( regex, timeZone );
			  }
			  string cc = arguments.Get( CC_FILTER );
			  if ( !string.ReferenceEquals( cc, null ) )
			  {
					return new ConsistencyCheckOutputCriteria( cc, timeZone );
			  }
			  return null;
		 }

		 public static Printer GetPrinter( Args args )
		 {
			  bool toFile = args.GetBoolean( TO_FILE, false, true ).Value;
			  return toFile ? new FilePrinter() : SYSTEM_OUT_PRINTER;
		 }

		 public interface Printer : IDisposable
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.io.PrintStream getFor(String file) throws java.io.FileNotFoundException;
			  PrintStream GetFor( string file );

			  void Close();
		 }

		 private static readonly Printer SYSTEM_OUT_PRINTER = new PrinterAnonymousInnerClass();

		 private class PrinterAnonymousInnerClass : Printer
		 {
			 public PrintStream getFor( string file )
			 {
				  return System.out;
			 }

			 public void close()
			 { // Don't close System.out
			 }
		 }

		 private class FilePrinter : Printer
		 {
			  internal File Directory;
			  internal PrintStream Out;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.io.PrintStream getFor(String file) throws java.io.FileNotFoundException
			  public override PrintStream GetFor( string file )
			  {
					File absoluteFile = ( new File( file ) ).AbsoluteFile;
					File dir = absoluteFile.Directory ? absoluteFile : absoluteFile.ParentFile;
					if ( !dir.Equals( Directory ) )
					{
						 Close();
						 File dumpFile = new File( dir, "dump-logical-log.txt" );
						 Console.WriteLine( "Redirecting the output to " + dumpFile.Path );
						 Out = new PrintStream( dumpFile );
						 Directory = dir;
					}
					return Out;
			  }

			  public override void Close()
			  {
					if ( Out != null )
					{
						 Out.close();
					}
			  }
		 }

		 public static TimeZone ParseTimeZoneConfig( Args arguments )
		 {
			  return getTimeZone( arguments.Get( "timezone", DEFAULT_TIME_ZONE.ID ) );
		 }
	}

}