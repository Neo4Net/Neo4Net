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
namespace Neo4Net.Diagnostics
{
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using Config = Neo4Net.Kernel.configuration.Config;
	using DefaultFileSystemExtension = Neo4Net.Test.extension.DefaultFileSystemExtension;
	using Inject = Neo4Net.Test.extension.Inject;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.diagnostics.DiagnosticsReportSources.newDiagnosticsFile;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith({DefaultFileSystemExtension.class, TestDirectoryExtension.class}) class DiagnosticsReporterTest
	internal class DiagnosticsReporterTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.Neo4Net.test.rule.TestDirectory testDirectory;
		 private TestDirectory _testDirectory;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.Neo4Net.io.fs.DefaultFileSystemAbstraction fileSystem;
		 private DefaultFileSystemAbstraction _fileSystem;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void dumpFiles() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void DumpFiles()
		 {
			  DiagnosticsReporter reporter = SetupDiagnosticsReporter();

			  Path destination = _testDirectory.file( "logs.zip" ).toPath();

			  reporter.Dump( Collections.singleton( "logs" ), destination, mock( typeof( DiagnosticsReporterProgress ) ), true );

			  // Verify content
			  VerifyContent( destination );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldContinueAfterError() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldContinueAfterError()
		 {
			  DiagnosticsReporter reporter = new DiagnosticsReporter();
			  MyProvider myProvider = new MyProvider( _fileSystem );
			  reporter.RegisterOfflineProvider( myProvider );

			  myProvider.AddFile( "logs/a.txt", CreateNewFileWithContent( "a.txt", "file a" ) );

			  Path destination = _testDirectory.file( "logs.zip" ).toPath();
			  ISet<string> classifiers = new HashSet<string>();
			  classifiers.Add( "logs" );
			  classifiers.Add( "fail" );
			  using ( MemoryStream baos = new MemoryStream() )
			  {
					PrintStream @out = new PrintStream( baos );
					NonInteractiveProgress progress = new NonInteractiveProgress( @out, false );

					reporter.Dump( classifiers, destination, progress, true );

					assertThat( baos.ToString(), @is(string.Format("1/2 fail.txt%n" + "....................  20%%%n" + "..........%n" + "Error: Step failed%n" + "2/2 logs/a.txt%n" + "....................  20%%%n" + "....................  40%%%n" + "....................  60%%%n" + "....................  80%%%n" + ".................... 100%%%n%n")) );
			  }

			  // Verify content
			  URI uri = URI.create( "jar:file:" + destination.toAbsolutePath().toUri().RawPath );

			  using ( FileSystem fs = FileSystems.newFileSystem( uri, Collections.emptyMap() ) )
			  {
					IList<string> fileA = Files.readAllLines( fs.getPath( "logs/a.txt" ) );
					assertEquals( 1, fileA.Count );
					assertEquals( "file a", fileA[0] );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void supportPathsWithSpaces() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void SupportPathsWithSpaces()
		 {
			  DiagnosticsReporter reporter = SetupDiagnosticsReporter();

			  Path destination = _testDirectory.file( "log files.zip" ).toPath();

			  reporter.Dump( Collections.singleton( "logs" ), destination, mock( typeof( DiagnosticsReporterProgress ) ), true );

			  VerifyContent( destination );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File createNewFileWithContent(String name, String content) throws java.io.IOException
		 private File CreateNewFileWithContent( string name, string content )
		 {
			  Path file = _testDirectory.file( name ).toPath();
			  Files.write( file, content.GetBytes() );
			  return file.toFile();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private DiagnosticsReporter setupDiagnosticsReporter() throws java.io.IOException
		 private DiagnosticsReporter SetupDiagnosticsReporter()
		 {
			  DiagnosticsReporter reporter = new DiagnosticsReporter();
			  MyProvider myProvider = new MyProvider( _fileSystem );
			  reporter.RegisterOfflineProvider( myProvider );

			  myProvider.AddFile( "logs/a.txt", CreateNewFileWithContent( "a.txt", "file a" ) );
			  myProvider.AddFile( "logs/b.txt", CreateNewFileWithContent( "b.txt", "file b" ) );
			  return reporter;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void verifyContent(java.nio.file.Path destination) throws java.io.IOException
		 private static void VerifyContent( Path destination )
		 {
			  URI uri = URI.create( "jar:file:" + destination.toAbsolutePath().toUri().RawPath );

			  using ( FileSystem fs = FileSystems.newFileSystem( uri, Collections.emptyMap() ) )
			  {
					IList<string> fileA = Files.readAllLines( fs.getPath( "logs/a.txt" ) );
					assertEquals( 1, fileA.Count );
					assertEquals( "file a", fileA[0] );

					IList<string> fileB = Files.readAllLines( fs.getPath( "logs/b.txt" ) );
					assertEquals( 1, fileB.Count );
					assertEquals( "file b", fileB[0] );
			  }
		 }

		 private class MyProvider : DiagnosticsOfflineReportProvider
		 {
			  internal readonly FileSystemAbstraction Fs;
			  internal readonly IList<DiagnosticsReportSource> LogFiles = new List<DiagnosticsReportSource>();

			  internal MyProvider( FileSystemAbstraction fs ) : base( "my-provider", "logs" )
			  {
					this.Fs = fs;
			  }

			  internal virtual void AddFile( string destination, File file )
			  {
					LogFiles.Add( newDiagnosticsFile( destination, Fs, file ) );
			  }

			  public override void Init( FileSystemAbstraction fs, Config config, File storeDirectory )
			  {
			  }

			  public override IList<DiagnosticsReportSource> ProvideSources( ISet<string> classifiers )
			  {
					IList<DiagnosticsReportSource> sources = new List<DiagnosticsReportSource>();
					if ( classifiers.Contains( "fail" ) )
					{
						 sources.Add( new FailingSource() );
					}
					if ( classifiers.Contains( "logs" ) )
					{
						 ( ( IList<DiagnosticsReportSource> )sources ).AddRange( LogFiles );
					}

					return sources;
			  }
		 }
		 private class FailingSource : DiagnosticsReportSource
		 {

			  public override string DestinationPath()
			  {
					return "fail.txt";
			  }

			  public override void AddToArchive( Path archiveDestination, DiagnosticsReporterProgress progress )
			  {
					progress.PercentChanged( 30 );
					throw new Exception( "You had it coming..." );
			  }

			  public override long EstimatedSize( DiagnosticsReporterProgress progress )
			  {
					return 0;
			  }
		 }
	}

}