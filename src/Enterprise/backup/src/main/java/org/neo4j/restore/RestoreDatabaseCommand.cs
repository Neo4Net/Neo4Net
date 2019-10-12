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
namespace Neo4Net.restore
{

	using CommandFailed = Neo4Net.Commandline.Admin.CommandFailed;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using Config = Neo4Net.Kernel.configuration.Config;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Neo4Net.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using Validators = Neo4Net.Kernel.impl.util.Validators;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.commandline.Util.checkLock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.commandline.Util.isSameOrChildFile;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.database_path;

	public class RestoreDatabaseCommand
	{
		 private FileSystemAbstraction _fs;
		 private readonly File _fromDatabasePath;
		 private readonly File _toDatabaseDir;
		 private readonly File _transactionLogsDirectory;
		 private string _toDatabaseName;
		 private bool _forceOverwrite;

		 public RestoreDatabaseCommand( FileSystemAbstraction fs, File fromDatabasePath, Config config, string toDatabaseName, bool forceOverwrite )
		 {
			  this._fs = fs;
			  this._fromDatabasePath = fromDatabasePath;
			  this._forceOverwrite = forceOverwrite;
			  this._toDatabaseName = toDatabaseName;
			  this._toDatabaseDir = config.Get( database_path ).AbsoluteFile;
			  this._transactionLogsDirectory = config.Get( GraphDatabaseSettings.logical_logs_location ).AbsoluteFile;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void execute() throws java.io.IOException, org.neo4j.commandline.admin.CommandFailed
		 public virtual void Execute()
		 {
			  if ( !_fs.fileExists( _fromDatabasePath ) )
			  {
					throw new System.ArgumentException( format( "Source directory does not exist [%s]", _fromDatabasePath ) );
			  }

			  try
			  {
					Validators.CONTAINS_EXISTING_DATABASE.validate( _fromDatabasePath );
			  }
			  catch ( System.ArgumentException )
			  {
					throw new System.ArgumentException( format( "Source directory is not a database backup [%s]", _fromDatabasePath ) );
			  }

			  if ( _fs.fileExists( _toDatabaseDir ) && !_forceOverwrite )
			  {
					throw new System.ArgumentException( format( "Database with name [%s] already exists at %s", _toDatabaseName, _toDatabaseDir ) );
			  }

			  checkLock( DatabaseLayout.of( _toDatabaseDir ).StoreLayout );

			  _fs.deleteRecursively( _toDatabaseDir );

			  if ( !isSameOrChildFile( _toDatabaseDir, _transactionLogsDirectory ) )
			  {
					_fs.deleteRecursively( _transactionLogsDirectory );
			  }
			  LogFiles backupLogFiles = LogFilesBuilder.logFilesBasedOnlyBuilder( _fromDatabasePath, _fs ).build();
			  RestoreDatabaseFiles( backupLogFiles, _fromDatabasePath.listFiles() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void restoreDatabaseFiles(org.neo4j.kernel.impl.transaction.log.files.LogFiles backupLogFiles, java.io.File[] files) throws java.io.IOException
		 private void RestoreDatabaseFiles( LogFiles backupLogFiles, File[] files )
		 {
			  if ( files != null )
			  {
					foreach ( File file in files )
					{
						 if ( file.Directory )
						 {
							  File destination = new File( _toDatabaseDir, file.Name );
							  _fs.mkdirs( destination );
							  _fs.copyRecursively( file, destination );
						 }
						 else
						 {
							  _fs.copyToDirectory( file, backupLogFiles.IsLogFile( file ) ? _transactionLogsDirectory : _toDatabaseDir );
						 }
					}
			  }
		 }
	}

}