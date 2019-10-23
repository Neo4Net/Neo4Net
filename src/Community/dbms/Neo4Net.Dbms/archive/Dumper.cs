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
namespace Neo4Net.Dbms.archive
{
	using ArchiveEntry = org.apache.commons.compress.archivers.ArchiveEntry;
	using ArchiveOutputStream = org.apache.commons.compress.archivers.ArchiveOutputStream;
	using TarArchiveOutputStream = org.apache.commons.compress.archivers.tar.TarArchiveOutputStream;


	using Util = Neo4Net.CommandLine.Util;
	using Neo4Net.Functions;
	using Resource = Neo4Net.GraphDb.Resource;
	using VisibleForTesting = Neo4Net.Utils.VisibleForTesting;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.dbms.archive.Utils.checkWritableDirectory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.dbms.archive.Utils.copy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.function.Predicates.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.fs.FileVisitors.justContinue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.fs.FileVisitors.onDirectory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.fs.FileVisitors.onFile;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.fs.FileVisitors.onlyMatching;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.fs.FileVisitors.throwExceptions;

	public class Dumper
	{
		 private readonly IList<ArchiveOperation> _operations;
		 private readonly ArchiveProgressPrinter _progressPrinter;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting Dumper()
		 internal Dumper()
		 {
			  _operations = new List<ArchiveOperation>();
			  _progressPrinter = new ArchiveProgressPrinter( null );
		 }

		 public Dumper( PrintStream output )
		 {
			  _operations = new List<ArchiveOperation>();
			  _progressPrinter = new ArchiveProgressPrinter( output );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void dump(java.nio.file.Path dbPath, java.nio.file.Path transactionalLogsPath, java.nio.file.Path archive, CompressionFormat format, System.Predicate<java.nio.file.Path> exclude) throws java.io.IOException
		 public virtual void Dump( Path dbPath, Path transactionalLogsPath, Path archive, CompressionFormat format, System.Predicate<Path> exclude )
		 {
			  checkWritableDirectory( archive.Parent );
			  _operations.Clear();

			  VisitPath( dbPath, exclude );
			  if ( !Util.isSameOrChildPath( dbPath, transactionalLogsPath ) )
			  {
					VisitPath( transactionalLogsPath, exclude );
			  }

			  _progressPrinter.reset();
			  foreach ( ArchiveOperation operation in _operations )
			  {
					_progressPrinter.maxBytes += operation.Size;
					_progressPrinter.maxFiles += operation.IsFile ? 1 : 0;
			  }

			  using ( ArchiveOutputStream stream = OpenArchiveOut( archive, format ), Resource ignore = _progressPrinter.startPrinting() )
			  {
					foreach ( ArchiveOperation operation in _operations )
					{
						 operation.AddToArchive( stream );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void visitPath(java.nio.file.Path transactionalLogsPath, System.Predicate<java.nio.file.Path> exclude) throws java.io.IOException
		 private void VisitPath( Path transactionalLogsPath, System.Predicate<Path> exclude )
		 {
			  Files.walkFileTree( transactionalLogsPath, onlyMatching( not( exclude ), throwExceptions( onDirectory( dir => dumpDirectory( transactionalLogsPath, dir ), onFile( file => dumpFile( transactionalLogsPath, file ), justContinue() ) ) ) ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.apache.commons.compress.archivers.ArchiveOutputStream openArchiveOut(java.nio.file.Path archive, CompressionFormat format) throws java.io.IOException
		 private ArchiveOutputStream OpenArchiveOut( Path archive, CompressionFormat format )
		 {
			  // StandardOpenOption.CREATE_NEW is important here because it atomically asserts that the file doesn't
			  // exist as it is opened, avoiding a TOCTOU race condition which results in a security vulnerability. I
			  // can't see a way to write a test to verify that we are using this option rather than just implementing
			  // the check ourselves non-atomically.
			  Stream @out = Files.newOutputStream( archive, StandardOpenOption.CREATE_NEW );
			  Stream compress = format.compress( @out );

			  // Add enough archive meta-data that the load command can print a meaningful progress indicator.
			  if ( format == CompressionFormat.ZSTD )
			  {
					WriteArchiveMetadata( compress );
			  }

			  TarArchiveOutputStream tarball = new TarArchiveOutputStream( compress );
			  tarball.LongFileMode = TarArchiveOutputStream.LONGFILE_POSIX;
			  tarball.BigNumberMode = TarArchiveOutputStream.BIGNUMBER_POSIX;
			  return tarball;
		 }

		 /// <seealso cref= Loader#readArchiveMetadata(InputStream) </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void writeArchiveMetadata(java.io.OutputStream stream) throws java.io.IOException
		 internal virtual void WriteArchiveMetadata( Stream stream )
		 {
			  DataOutputStream metadata = new DataOutputStream( stream ); // Unbuffered. No need for flushing.
			  metadata.writeInt( 1 ); // Archive format version. Increment whenever the metadata format changes.
			  metadata.writeLong( _progressPrinter.maxFiles );
			  metadata.writeLong( _progressPrinter.maxBytes );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void dumpFile(java.nio.file.Path root, java.nio.file.Path file) throws java.io.IOException
		 private void DumpFile( Path root, Path file )
		 {
			  WithEntry( stream => writeFile( file, stream ), root, file );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void dumpDirectory(java.nio.file.Path root, java.nio.file.Path dir) throws java.io.IOException
		 private void DumpDirectory( Path root, Path dir )
		 {
			  WithEntry(stream =>
			  {
			  }, root, dir);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void withEntry(org.Neo4Net.function.ThrowingConsumer<org.apache.commons.compress.archivers.ArchiveOutputStream, java.io.IOException> operation, java.nio.file.Path root, java.nio.file.Path file) throws java.io.IOException
		 private void WithEntry( ThrowingConsumer<ArchiveOutputStream, IOException> operation, Path root, Path file )
		 {
			  _operations.Add( new ArchiveOperation( operation, root, file ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeFile(java.nio.file.Path file, org.apache.commons.compress.archivers.ArchiveOutputStream archiveStream) throws java.io.IOException
		 private void WriteFile( Path file, ArchiveOutputStream archiveStream )
		 {
			  using ( Stream @in = Files.newInputStream( file ) )
			  {
					copy( @in, archiveStream, _progressPrinter );
			  }
		 }

		 private class ArchiveOperation
		 {
			  internal readonly ThrowingConsumer<ArchiveOutputStream, IOException> Operation;
			  internal readonly long Size;
			  internal readonly bool IsFile;
			  internal readonly Path Root;
			  internal readonly Path File;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private ArchiveOperation(org.Neo4Net.function.ThrowingConsumer<org.apache.commons.compress.archivers.ArchiveOutputStream, java.io.IOException> operation, java.nio.file.Path root, java.nio.file.Path file) throws java.io.IOException
			  internal ArchiveOperation( ThrowingConsumer<ArchiveOutputStream, IOException> operation, Path root, Path file )
			  {
					this.Operation = operation;
					this.IsFile = Files.isRegularFile( file );
					this.Size = IsFile ? Files.size( file ) : 0;
					this.Root = root;
					this.File = file;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void addToArchive(org.apache.commons.compress.archivers.ArchiveOutputStream stream) throws java.io.IOException
			  internal virtual void AddToArchive( ArchiveOutputStream stream )
			  {
					ArchiveEntry entry = CreateEntry( File, Root, stream );
					stream.putArchiveEntry( entry );
					Operation.accept( stream );
					stream.closeArchiveEntry();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.apache.commons.compress.archivers.ArchiveEntry createEntry(java.nio.file.Path file, java.nio.file.Path root, org.apache.commons.compress.archivers.ArchiveOutputStream archive) throws java.io.IOException
			  internal virtual ArchiveEntry CreateEntry( Path file, Path root, ArchiveOutputStream archive )
			  {
					return archive.createArchiveEntry( file.toFile(), "./" + root.relativize(file).ToString() );
			  }
		 }
	}

}