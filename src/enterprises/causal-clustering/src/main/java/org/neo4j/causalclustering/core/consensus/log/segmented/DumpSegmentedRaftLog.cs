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
namespace Neo4Net.causalclustering.core.consensus.log.segmented
{

	using ReplicatedContent = Neo4Net.causalclustering.core.replication.ReplicatedContent;
	using Neo4Net.causalclustering.messaging.marshalling;
	using CoreReplicatedContentMarshal = Neo4Net.causalclustering.messaging.marshalling.CoreReplicatedContentMarshal;
	using Neo4Net.Cursors;
	using Args = Neo4Net.Helpers.Args;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using Clocks = Neo4Net.Time.Clocks;

	internal class DumpSegmentedRaftLog
	{
		 private readonly FileSystemAbstraction _fileSystem;
		 private const string TO_FILE = "tofile";
		 private ChannelMarshal<ReplicatedContent> _marshal;

		 private DumpSegmentedRaftLog( FileSystemAbstraction fileSystem, ChannelMarshal<ReplicatedContent> marshal )
		 {
			  this._fileSystem = fileSystem;
			  this._marshal = marshal;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int dump(String filenameOrDirectory, java.io.PrintStream out) throws java.io.IOException, DamagedLogStorageException, DisposedException
		 private int Dump( string filenameOrDirectory, PrintStream @out )
		 {
			  LogProvider logProvider = NullLogProvider.Instance;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int[] logsFound = {0};
			  int[] logsFound = new int[] { 0 };
			  FileNames fileNames = new FileNames( new File( filenameOrDirectory ) );
			  ReaderPool readerPool = new ReaderPool( 0, logProvider, fileNames, _fileSystem, Clocks.systemClock() );
			  RecoveryProtocol recoveryProtocol = new RecoveryProtocol( _fileSystem, fileNames, readerPool, _marshal, logProvider );
			  Segments segments = recoveryProtocol.Run().Segments;

			  segments.Visit(segment =>
			  {
			  logsFound[0]++;
			  @out.println( "=== " + segment.Filename + " ===" );
			  SegmentHeader header = segment.header();
			  @out.println( header.ToString() );
			  try
			  {
				  using ( IOCursor<EntryRecord> cursor = segment.getCursor( header.PrevIndex() + 1 ) )
				  {
					  while ( cursor.next() )
					  {
						  @out.println( cursor.get().ToString() );
					  }
				  }
			  }
			  catch ( Exception e ) when ( e is DisposedException || e is IOException )
			  {
				  e.printStackTrace();
				  Environment.Exit( -1 );
				  return true;
			  }
			  return false;
			  });

			  return logsFound[0];
		 }

		 public static void Main( string[] args )
		 {
			  Args arguments = Args.withFlags( TO_FILE ).parse( args );
			  using ( Printer printer = GetPrinter( arguments ) )
			  {
					foreach ( string fileAsString in arguments.Orphans() )
					{
						 Console.WriteLine( "Reading file " + fileAsString );

						 try
						 {
								 using ( DefaultFileSystemAbstraction fileSystem = new DefaultFileSystemAbstraction() )
								 {
								  ( new DumpSegmentedRaftLog( fileSystem, CoreReplicatedContentMarshal.marshaller() ) ).dump(fileAsString, printer.GetFor(fileAsString));
								 }
						 }
						 catch ( Exception e ) when ( e is IOException || e is DisposedException || e is DamagedLogStorageException )
						 {
							  e.printStackTrace();
						 }
					}
			  }
		 }

		 private static Printer GetPrinter( Args args )
		 {
			  bool toFile = args.GetBoolean( TO_FILE, false, true ).Value;
			  return toFile ? new DumpSegmentedRaftLog.FilePrinter() : SYSTEM_OUT_PRINTER;
		 }

		 internal interface Printer : AutoCloseable
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
	}

}