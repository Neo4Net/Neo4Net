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
namespace Neo4Net.Diagnostics
{

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using RotatingFileOutputStreamSupplier = Neo4Net.Logging.RotatingFileOutputStreamSupplier;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.RotatingFileOutputStreamSupplier.getAllArchives;

	/// <summary>
	/// Contains helper methods to create create <seealso cref="DiagnosticsReportSource"/>.
	/// </summary>
	public class DiagnosticsReportSources
	{
		 private DiagnosticsReportSources()
		 {
			  throw new AssertionError( "No instances" );
		 }

		 /// <summary>
		 /// Create a diagnostics source the will copy a file into the archive.
		 /// </summary>
		 /// <param name="destination"> final destination in archive. </param>
		 /// <param name="fs"> filesystem abstraction to use. </param>
		 /// <param name="source"> source file to archive </param>
		 /// <returns> a diagnostics source consuming a file. </returns>
		 public static DiagnosticsReportSource NewDiagnosticsFile( string destination, FileSystemAbstraction fs, File source )
		 {
			  return new DiagnosticsFileReportSource( destination, fs, source );
		 }

		 /// <summary>
		 /// This is to be used by loggers that uses <seealso cref="RotatingFileOutputStreamSupplier"/>.
		 /// </summary>
		 /// <param name="destination"> final destination in archive. </param>
		 /// <param name="fs"> filesystem abstraction to use. </param>
		 /// <param name="file"> input log file, should be without rotation numbers. </param>
		 /// <returns> a list diagnostics sources consisting of the log file including all rotated away files. </returns>
		 public static IList<DiagnosticsReportSource> NewDiagnosticsRotatingFile( string destination, FileSystemAbstraction fs, File file )
		 {
			  List<DiagnosticsReportSource> files = new List<DiagnosticsReportSource>();

			  Files.Add( NewDiagnosticsFile( destination, fs, file ) );

			  IList<File> allArchives = getAllArchives( fs, file );
			  foreach ( File archive in allArchives )
			  {
					string name = archive.Name;
					string n = name.Substring( name.LastIndexOf( '.' ) );
					Files.Add( NewDiagnosticsFile( destination + "." + n, fs, archive ) );
			  }
			  return files;
		 }

		 /// <summary>
		 /// Create a diagnostics source from a string. Can be used to dump simple messages to a file in the archive. Files
		 /// are opened with append option so this method can be used to accumulate messages from multiple source to a single
		 /// file in the archive.
		 /// </summary>
		 /// <param name="destination"> final destination in archive. </param>
		 /// <param name="messageSupplier"> a string supplier with the final message. </param>
		 /// <returns> a diagnostics source consuming a string. </returns>
		 public static DiagnosticsReportSource NewDiagnosticsString( string destination, System.Func<string> messageSupplier )
		 {
			  return new DiagnosticsStringReportSource( destination, messageSupplier );
		 }

		 private class DiagnosticsFileReportSource : DiagnosticsReportSource
		 {
			  internal readonly string Destination;
			  internal readonly FileSystemAbstraction Fs;
			  internal readonly File Source;

			  internal DiagnosticsFileReportSource( string destination, FileSystemAbstraction fs, File source )
			  {
					this.Destination = destination;
					this.Fs = fs;
					this.Source = source;
			  }

			  public override string DestinationPath()
			  {
					return Destination;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void addToArchive(java.nio.file.Path archiveDestination, DiagnosticsReporterProgress progress) throws java.io.IOException
			  public override void AddToArchive( Path archiveDestination, DiagnosticsReporterProgress progress )
			  {
					long size = Fs.getFileSize( Source );
					Stream @in = Fs.openAsInputStream( Source );

					// Track progress of the file reading, source might be a very large file
					using ( ProgressAwareInputStream inStream = new ProgressAwareInputStream( @in, size, progress.percentChanged ) )
					{
						 Files.copy( inStream, archiveDestination );
					}
			  }

			  public override long EstimatedSize( DiagnosticsReporterProgress progress )
			  {
					return Fs.getFileSize( Source );
			  }
		 }

		 private class DiagnosticsStringReportSource : DiagnosticsReportSource
		 {
			  internal readonly string Destination;
			  internal readonly System.Func<string> MessageSupplier;

			  internal DiagnosticsStringReportSource( string destination, System.Func<string> messageSupplier )
			  {
					this.Destination = destination;
					this.MessageSupplier = messageSupplier;
			  }

			  public override string DestinationPath()
			  {
					return Destination;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void addToArchive(java.nio.file.Path archiveDestination, DiagnosticsReporterProgress progress) throws java.io.IOException
			  public override void AddToArchive( Path archiveDestination, DiagnosticsReporterProgress progress )
			  {
					string message = MessageSupplier.get();
					Files.write( archiveDestination, message.GetBytes( Encoding.UTF8 ), StandardOpenOption.CREATE, StandardOpenOption.APPEND );
			  }

			  public override long EstimatedSize( DiagnosticsReporterProgress progress )
			  {
					return 0; // Size of strings should be negligible
			  }
		 }
	}

}