using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.backup.impl
{

	using FileMoveAction = Neo4Net.com.storecopy.FileMoveAction;
	using FileMoveProvider = Neo4Net.com.storecopy.FileMoveProvider;
	using Exceptions = Neo4Net.Helpers.Exceptions;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using IdGeneratorImpl = Neo4Net.Kernel.impl.store.id.IdGeneratorImpl;

	internal class BackupCopyService
	{
		 private const int MAX_OLD_BACKUPS = 1000;

		 private readonly FileSystemAbstraction _fs;
		 private readonly FileMoveProvider _fileMoveProvider;

		 internal BackupCopyService( FileSystemAbstraction fs, FileMoveProvider fileMoveProvider )
		 {
			  this._fs = fs;
			  this._fileMoveProvider = fileMoveProvider;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void moveBackupLocation(java.nio.file.Path oldLocation, java.nio.file.Path newLocation) throws java.io.IOException
		 internal virtual void MoveBackupLocation( Path oldLocation, Path newLocation )
		 {
			  try
			  {
					File source = oldLocation.toFile();
					File target = newLocation.toFile();
					IEnumerator<FileMoveAction> moves = _fileMoveProvider.traverseForMoving( source ).GetEnumerator();
					while ( moves.MoveNext() )
					{
						 moves.Current.move( target );
					}
					oldLocation.toFile().delete();
			  }
			  catch ( IOException e )
			  {
					throw new IOException( "Failed to rename backup directory from " + oldLocation + " to " + newLocation, e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void clearIdFiles(java.nio.file.Path backupLocation) throws java.io.IOException
		 internal virtual void ClearIdFiles( Path backupLocation )
		 {
			  IOException exception = null;
			  File targetDirectory = backupLocation.toFile();
			  File[] files = _fs.listFiles( targetDirectory );
			  foreach ( File file in files )
			  {
					if ( !_fs.isDirectory( file ) && file.Name.EndsWith( ".id" ) )
					{
						 try
						 {
							  long highId = IdGeneratorImpl.readHighId( _fs, file );
							  _fs.deleteFile( file );
							  IdGeneratorImpl.createGenerator( _fs, file, highId, true );
						 }
						 catch ( IOException e )
						 {
							  exception = Exceptions.chain( exception, e );
						 }
					}
			  }
			  if ( exception != null )
			  {
					throw exception;
			  }
		 }

		 internal virtual bool BackupExists( DatabaseLayout databaseLayout )
		 {
			  return databaseLayout.MetadataStore().exists();
		 }

		 internal virtual Path FindNewBackupLocationForBrokenExisting( Path existingBackup )
		 {
			  return FindAnAvailableBackupLocation( existingBackup, "%s.err.%d" );
		 }

		 internal virtual Path FindAnAvailableLocationForNewFullBackup( Path desiredBackupLocation )
		 {
			  return FindAnAvailableBackupLocation( desiredBackupLocation, "%s.temp.%d" );
		 }

		 /// <summary>
		 /// Given a desired file name, find an available name that is similar to the given one that doesn't conflict with already existing backups
		 /// </summary>
		 /// <param name="file"> desired ideal file name </param>
		 /// <param name="pattern"> pattern to follow if desired name is taken (requires %s for original name, and %d for iteration) </param>
		 /// <returns> the resolved file name which can be the original desired, or a variation that matches the pattern </returns>
		 private Path FindAnAvailableBackupLocation( Path file, string pattern )
		 {
			  if ( BackupExists( DatabaseLayout.of( file.toFile() ) ) )
			  {
					// find alternative name
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicLong counter = new java.util.concurrent.atomic.AtomicLong(0);
					AtomicLong counter = new AtomicLong( 0 );
					System.Action<Path> countNumberOfFilesProcessedForPotentialErrorMessage = generatedBackupFile => counter.AndIncrement;

					return AvailableAlternativeNames( file, pattern ).peek( countNumberOfFilesProcessedForPotentialErrorMessage ).filter( f => !BackupExists( DatabaseLayout.of( f.toFile() ) ) ).findFirst().orElseThrow(NoFreeBackupLocation(file, counter));
			  }
			  return file;
		 }

		 private static System.Func<Exception> NoFreeBackupLocation( Path file, AtomicLong counter )
		 {
			  return () => new Exception(string.Format("Unable to find a free backup location for the provided {0}. {1:D} possible locations were already taken.", file, counter.get()));
		 }

		 private static Stream<Path> AvailableAlternativeNames( Path originalBackupDirectory, string pattern )
		 {
			  return IntStream.range( 0, MAX_OLD_BACKUPS ).mapToObj( iteration => AlteredBackupDirectoryName( pattern, originalBackupDirectory, iteration ) );
		 }

		 private static Path AlteredBackupDirectoryName( string pattern, Path directory, int iteration )
		 {
			  Path directoryName = directory.getName( directory.NameCount - 1 );
			  return directory.resolveSibling( format( pattern, directoryName, iteration ) );
		 }
	}

}