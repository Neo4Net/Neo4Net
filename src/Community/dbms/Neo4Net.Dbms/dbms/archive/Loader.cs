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
namespace Neo4Net.Dbms.archive
{
	using ArchiveEntry = org.apache.commons.compress.archivers.ArchiveEntry;
	using ArchiveInputStream = org.apache.commons.compress.archivers.ArchiveInputStream;
	using TarArchiveInputStream = org.apache.commons.compress.archivers.tar.TarArchiveInputStream;


	using Resource = Neo4Net.GraphDb.Resource;
	using TransactionLogFiles = Neo4Net.Kernel.impl.transaction.log.files.TransactionLogFiles;
	using VisibleForTesting = Neo4Net.Utils.VisibleForTesting;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.dbms.archive.Utils.checkWritableDirectory;

	public class Loader
	{
		 private readonly ArchiveProgressPrinter _progressPrinter;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting Loader()
		 internal Loader()
		 {
			  _progressPrinter = new ArchiveProgressPrinter( null );
		 }

		 public Loader( PrintStream output )
		 {
			  _progressPrinter = new ArchiveProgressPrinter( output );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void load(java.nio.file.Path archive, java.nio.file.Path databaseDestination, java.nio.file.Path transactionLogsDirectory) throws java.io.IOException, IncorrectFormat
		 public virtual void Load( Path archive, Path databaseDestination, Path transactionLogsDirectory )
		 {
			  ValidatePath( databaseDestination );
			  ValidatePath( transactionLogsDirectory );

			  CreateDestination( databaseDestination );
			  CreateDestination( transactionLogsDirectory );

			  using ( ArchiveInputStream stream = OpenArchiveIn( archive ), Resource ignore = _progressPrinter.startPrinting() )
			  {
					ArchiveEntry entry;
					while ( ( entry = NextEntry( stream, archive ) ) != null )
					{
						 Path destination = DetermineEntryDestination( entry, databaseDestination, transactionLogsDirectory );
						 LoadEntry( destination, stream, entry );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void createDestination(java.nio.file.Path destination) throws java.io.IOException
		 private void CreateDestination( Path destination )
		 {
			  if ( !destination.toFile().exists() )
			  {
					Files.createDirectories( destination );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void validatePath(java.nio.file.Path path) throws java.nio.file.FileSystemException
		 private void ValidatePath( Path path )
		 {
			  if ( exists( path ) )
			  {
					throw new FileAlreadyExistsException( path.ToString() );
			  }
			  checkWritableDirectory( path.Parent );
		 }

		 private static Path DetermineEntryDestination( ArchiveEntry entry, Path databaseDestination, Path transactionLogsDirectory )
		 {
			  string entryName = Paths.get( entry.Name ).FileName.ToString();
			  return TransactionLogFiles.DEFAULT_FILENAME_FILTER.accept( null, entryName ) ? transactionLogsDirectory : databaseDestination;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.apache.commons.compress.archivers.ArchiveEntry nextEntry(org.apache.commons.compress.archivers.ArchiveInputStream stream, java.nio.file.Path archive) throws IncorrectFormat
		 private ArchiveEntry NextEntry( ArchiveInputStream stream, Path archive )
		 {
			  try
			  {
					return stream.NextEntry;
			  }
			  catch ( IOException e )
			  {
					throw new IncorrectFormat( archive, e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void loadEntry(java.nio.file.Path destination, org.apache.commons.compress.archivers.ArchiveInputStream stream, org.apache.commons.compress.archivers.ArchiveEntry entry) throws java.io.IOException
		 private void LoadEntry( Path destination, ArchiveInputStream stream, ArchiveEntry entry )
		 {
			  Path file = destination.resolve( entry.Name );

			  if ( !file.normalize().StartsWith(destination) )
			  {
					throw new IOException( "Zip entry outside destination path." );
			  }

			  if ( entry.Directory )
			  {
					Files.createDirectories( file );
			  }
			  else
			  {
					using ( Stream output = Files.newOutputStream( file ) )
					{
						 Utils.Copy( stream, output, _progressPrinter );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.apache.commons.compress.archivers.ArchiveInputStream openArchiveIn(java.nio.file.Path archive) throws java.io.IOException, IncorrectFormat
		 private ArchiveInputStream OpenArchiveIn( Path archive )
		 {
			  Stream input = Files.newInputStream( archive );
			  Stream decompressor;
			  try
			  {
					decompressor = CompressionFormat.Gzip.decompress( input );
			  }
			  catch ( IOException e )
			  {
					input.Close(); // Reopen to reset file position.
					input = Files.newInputStream( archive );
					try
					{
						 decompressor = CompressionFormat.ZSTD.decompress( input );
						 // Important: Only the ZSTD compressed archives have any archive metadata.
						 ReadArchiveMetadata( decompressor );
					}
					catch ( IOException ex )
					{
						 input.Close();
						 ex.addSuppressed( e );
						 throw new IncorrectFormat( archive, ex );
					}
			  }

			  return new TarArchiveInputStream( decompressor );
		 }

		 /// <seealso cref= Dumper#writeArchiveMetadata(OutputStream) </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void readArchiveMetadata(java.io.InputStream stream) throws java.io.IOException
		 internal virtual void ReadArchiveMetadata( Stream stream )
		 {
			  DataInputStream metadata = new DataInputStream( stream ); // Unbuffered. Will not play naughty tricks with the file position.
			  int version = metadata.readInt();
			  if ( version == 1 )
			  {
					_progressPrinter.maxFiles = metadata.readLong();
					_progressPrinter.maxBytes = metadata.readLong();
			  }
			  else
			  {
					throw new IOException( "Cannot read archive meta-data. I don't recognise this archive version: " + version + "." );
			  }
		 }
	}

}