using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
namespace Org.Neo4j.Server.security.enterprise.auth
{

	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using FileRepository = Org.Neo4j.Server.Security.Auth.FileRepository;
	using Org.Neo4j.Server.Security.Auth;
	using FormatException = Org.Neo4j.Server.Security.Auth.exception.FormatException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.auth.ListSnapshot.FROM_MEMORY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.auth.ListSnapshot.FROM_PERSISTED;

	/// <summary>
	/// Stores role data. In memory, but backed by persistent storage so changes to this repository will survive
	/// JVM restarts and crashes.
	/// </summary>
	public class FileRoleRepository : AbstractRoleRepository, FileRepository
	{
		 private readonly File _roleFile;
		 private readonly Log _log;
		 private readonly RoleSerialization _serialization = new RoleSerialization();
		 private readonly FileSystemAbstraction _fileSystem;

		 public FileRoleRepository( FileSystemAbstraction fileSystem, File file, LogProvider logProvider )
		 {
			  this._roleFile = file;
			  this._log = logProvider.getLog( this.GetType() );
			  this._fileSystem = fileSystem;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start() throws Throwable
		 public override void Start()
		 {
			  Clear();

			  FileRepository.assertNotMigrated( _roleFile, _fileSystem, _log );

			  ListSnapshot<RoleRecord> onDiskRoles = ReadPersistedRoles();
			  if ( onDiskRoles != null )
			  {
					Roles = onDiskRoles;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.neo4j.server.security.auth.ListSnapshot<RoleRecord> readPersistedRoles() throws java.io.IOException
		 protected internal override ListSnapshot<RoleRecord> ReadPersistedRoles()
		 {
			  if ( _fileSystem.fileExists( _roleFile ) )
			  {
					long readTime;
					IList<RoleRecord> readRoles;
					try
					{
						 readTime = _fileSystem.lastModifiedTime( _roleFile );
						 readRoles = _serialization.loadRecordsFromFile( _fileSystem, _roleFile );
					}
					catch ( FormatException e )
					{
						 _log.error( "Failed to read role file \"%s\" (%s)", _roleFile.AbsolutePath, e.Message );
						 throw new System.InvalidOperationException( "Failed to read role file '" + _roleFile + "'." );
					}

					return new ListSnapshot<RoleRecord>( readTime, readRoles, FROM_PERSISTED );
			  }
			  return null;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void persistRoles() throws java.io.IOException
		 protected internal override void PersistRoles()
		 {
			  _serialization.saveRecordsToFile( _fileSystem, _roleFile, RolesConflict );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.server.security.auth.ListSnapshot<RoleRecord> getPersistedSnapshot() throws java.io.IOException
		 public override ListSnapshot<RoleRecord> PersistedSnapshot
		 {
			 get
			 {
				  if ( LastLoaded.get() < _fileSystem.lastModifiedTime(_roleFile) )
				  {
						return ReadPersistedRoles();
				  }
				  lock ( this )
				  {
						return new ListSnapshot<RoleRecord>( LastLoaded.get(), new List<RoleRecord>(RolesConflict), FROM_MEMORY );
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void purge() throws java.io.IOException
		 public override void Purge()
		 {
			  base.Purge(); // Clears all cached data

			  // Delete the file
			  if ( !_fileSystem.deleteFile( _roleFile ) )
			  {
					throw new IOException( "Failed to delete file '" + _roleFile.AbsolutePath + "'" );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void markAsMigrated() throws java.io.IOException
		 public override void MarkAsMigrated()
		 {
			  base.MarkAsMigrated(); // Clears all cached data

			  // Rename the file
			  File destinationFile = FileRepository.getMigratedFile( _roleFile );
			  _fileSystem.renameFile( _roleFile, destinationFile, StandardCopyOption.REPLACE_EXISTING, StandardCopyOption.COPY_ATTRIBUTES );
		 }
	}

}