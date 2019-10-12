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
namespace Org.Neo4j.causalclustering.catchup.storecopy
{

	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using LogFiles = Org.Neo4j.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Org.Neo4j.Kernel.impl.transaction.log.files.LogFilesBuilder;

	public class TemporaryStoreDirectory : AutoCloseable
	{
		 private const string TEMP_COPY_DIRECTORY_NAME = "temp-copy";

		 private readonly File _tempStoreDir;
		 private readonly DatabaseLayout _tempDatabaseLayout;
		 private readonly StoreFiles _storeFiles;
		 private LogFiles _tempLogFiles;
		 private bool _keepStore;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public TemporaryStoreDirectory(org.neo4j.io.fs.FileSystemAbstraction fs, org.neo4j.io.pagecache.PageCache pageCache, java.io.File parent) throws java.io.IOException
		 public TemporaryStoreDirectory( FileSystemAbstraction fs, PageCache pageCache, File parent )
		 {
			  this._tempStoreDir = new File( parent, TEMP_COPY_DIRECTORY_NAME );
			  this._tempDatabaseLayout = DatabaseLayout.of( _tempStoreDir, GraphDatabaseSettings.DEFAULT_DATABASE_NAME );
			  _storeFiles = new StoreFiles( fs, pageCache, ( directory, name ) => true );
			  _tempLogFiles = LogFilesBuilder.logFilesBasedOnlyBuilder( _tempDatabaseLayout.databaseDirectory(), fs ).build();
			  _storeFiles.delete( _tempStoreDir, _tempLogFiles );
		 }

		 public virtual File StoreDir()
		 {
			  return _tempStoreDir;
		 }

		 public virtual DatabaseLayout DatabaseLayout()
		 {
			  return _tempDatabaseLayout;
		 }

		 internal virtual void KeepStore()
		 {
			  this._keepStore = true;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  if ( !_keepStore )
			  {
					_storeFiles.delete( _tempStoreDir, _tempLogFiles );
			  }
		 }
	}

}