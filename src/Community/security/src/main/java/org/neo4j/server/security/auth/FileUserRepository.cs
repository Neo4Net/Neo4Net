using System.Collections.Generic;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Server.Security.Auth
{

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using User = Neo4Net.Kernel.impl.security.User;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using FormatException = Neo4Net.Server.Security.Auth.exception.FormatException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.auth.ListSnapshot.FROM_MEMORY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.auth.ListSnapshot.FROM_PERSISTED;

	/// <summary>
	/// Stores user auth data. In memory, but backed by persistent storage so changes to this repository will survive
	/// JVM restarts and crashes.
	/// </summary>
	public class FileUserRepository : AbstractUserRepository, FileRepository
	{
		 private readonly File _authFile;
		 private readonly FileSystemAbstraction _fileSystem;

		 // TODO: We could improve concurrency by using a ReadWriteLock

		 private readonly Log _log;

		 private readonly UserSerialization _serialization = new UserSerialization();

		 public FileUserRepository( FileSystemAbstraction fileSystem, File file, LogProvider logProvider )
		 {
			  this._fileSystem = fileSystem;
			  this._authFile = file;
			  this._log = logProvider.getLog( this.GetType() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start() throws Throwable
		 public override void Start()
		 {
			  Clear();

			  FileRepository.assertNotMigrated( _authFile, _fileSystem, _log );

			  ListSnapshot<User> onDiskUsers = ReadPersistedUsers();
			  if ( onDiskUsers != null )
			  {
					Users = onDiskUsers;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected ListSnapshot<org.neo4j.kernel.impl.security.User> readPersistedUsers() throws java.io.IOException
		 protected internal override ListSnapshot<User> ReadPersistedUsers()
		 {
			  if ( _fileSystem.fileExists( _authFile ) )
			  {
					long readTime;
					IList<User> readUsers;
					try
					{
						 readTime = _fileSystem.lastModifiedTime( _authFile );
						 readUsers = _serialization.loadRecordsFromFile( _fileSystem, _authFile );
					}
					catch ( FormatException e )
					{
						 _log.error( "Failed to read authentication file \"%s\" (%s)", _authFile.AbsolutePath, e.Message );
						 throw new System.InvalidOperationException( "Failed to read authentication file: " + _authFile );
					}

					return new ListSnapshot<User>( readTime, readUsers, FROM_PERSISTED );
			  }
			  return null;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void persistUsers() throws java.io.IOException
		 protected internal override void PersistUsers()
		 {
			  _serialization.saveRecordsToFile( _fileSystem, _authFile, UsersConflict );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public ListSnapshot<org.neo4j.kernel.impl.security.User> getPersistedSnapshot() throws java.io.IOException
		 public override ListSnapshot<User> PersistedSnapshot
		 {
			 get
			 {
				  if ( LastLoaded.get() < _fileSystem.lastModifiedTime(_authFile) )
				  {
						return ReadPersistedUsers();
				  }
				  lock ( this )
				  {
						return new ListSnapshot<User>( LastLoaded.get(), new List<User>(UsersConflict), FROM_MEMORY );
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void purge() throws java.io.IOException
		 public override void Purge()
		 {
			  base.Purge(); // Clears all cached data

			  // Delete the file
			  if ( !_fileSystem.deleteFile( _authFile ) )
			  {
					throw new IOException( "Failed to delete file '" + _authFile.AbsolutePath + "'" );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void markAsMigrated() throws java.io.IOException
		 public override void MarkAsMigrated()
		 {
			  base.MarkAsMigrated(); // Clears all cached data

			  // Rename the file
			  File destinationFile = FileRepository.getMigratedFile( _authFile );
			  _fileSystem.renameFile( _authFile, destinationFile, StandardCopyOption.REPLACE_EXISTING, StandardCopyOption.COPY_ATTRIBUTES );
		 }
	}

}