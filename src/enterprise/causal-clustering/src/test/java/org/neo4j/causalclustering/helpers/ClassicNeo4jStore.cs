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
namespace Neo4Net.causalclustering.helpers
{

	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using Standard = Neo4Net.Kernel.impl.store.format.standard.Standard;
	using TransactionLogFiles = Neo4Net.Kernel.impl.transaction.log.files.TransactionLogFiles;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;

	public class ClassicNeo4NetStore
	{
		 private readonly File _storeDir;
		 private readonly File _logicalLogsDir;

		 private ClassicNeo4NetStore( File storeDir, File logicalLogsDir )
		 {
			  this._storeDir = storeDir;
			  this._logicalLogsDir = logicalLogsDir;
		 }

		 public virtual File StoreDir
		 {
			 get
			 {
				  return _storeDir;
			 }
		 }

		 public virtual File LogicalLogsDir
		 {
			 get
			 {
				  return _logicalLogsDir;
			 }
		 }

		 public static Neo4NetStoreBuilder Builder( File baseDir, FileSystemAbstraction fsa )
		 {
			  return new Neo4NetStoreBuilder( baseDir, fsa );
		 }

		 public class Neo4NetStoreBuilder
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal string DbNameConflict = "graph.db";
			  internal bool NeedRecover;
			  internal int NrOfNodes = 10;
			  internal string RecordsFormat = Standard.LATEST_NAME;
			  internal readonly File BaseDir;
			  internal readonly FileSystemAbstraction Fsa;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal string LogicalLogsLocationConflict = "";

			  internal Neo4NetStoreBuilder( File baseDir, FileSystemAbstraction fsa )
			  {

					this.BaseDir = baseDir;
					this.Fsa = fsa;
			  }

			  public virtual Neo4NetStoreBuilder DbName( string @string )
			  {
					DbNameConflict = @string;
					return this;
			  }

			  public virtual Neo4NetStoreBuilder NeedToRecover()
			  {
					NeedRecover = true;
					return this;
			  }

			  public virtual Neo4NetStoreBuilder AmountOfNodes( int nodes )
			  {
					NrOfNodes = nodes;
					return this;
			  }

			  public virtual Neo4NetStoreBuilder RecordFormats( string format )
			  {
					RecordsFormat = format;
					return this;
			  }

			  public virtual Neo4NetStoreBuilder LogicalLogsLocation( string logicalLogsLocation )
			  {
					this.LogicalLogsLocationConflict = logicalLogsLocation;
					return this;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public ClassicNeo4NetStore build() throws java.io.IOException
			  public virtual ClassicNeo4NetStore Build()
			  {
					CreateStore( BaseDir, Fsa, DbNameConflict, NrOfNodes, RecordsFormat, NeedRecover, LogicalLogsLocationConflict );
					File storeDir = new File( BaseDir, DbNameConflict );
					return new ClassicNeo4NetStore( storeDir, new File( storeDir, LogicalLogsLocationConflict ) );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void createStore(java.io.File super, Neo4Net.io.fs.FileSystemAbstraction fileSystem, String dbName, int nodesToCreate, String recordFormat, boolean recoveryNeeded, String logicalLogsLocation) throws java.io.IOException
			  internal static void CreateStore( File @base, FileSystemAbstraction fileSystem, string dbName, int nodesToCreate, string recordFormat, bool recoveryNeeded, string logicalLogsLocation )
			  {
					File storeDir = new File( @base, dbName );
					GraphDatabaseService db = ( new TestGraphDatabaseFactory() ).setFileSystem(fileSystem).newEmbeddedDatabaseBuilder(storeDir).setConfig(GraphDatabaseSettings.record_format, recordFormat).setConfig(OnlineBackupSettings.online_backup_enabled, false.ToString()).setConfig(GraphDatabaseSettings.logical_logs_location, logicalLogsLocation).newGraphDatabase();

					for ( int i = 0; i < ( nodesToCreate / 2 ); i++ )
					{
						 using ( Transaction tx = Db.beginTx() )
						 {
							  Node node1 = Db.createNode( Label.label( "Label-" + i ) );
							  Node node2 = Db.createNode( Label.label( "Label-" + i ) );
							  node1.CreateRelationshipTo( node2, RelationshipType.withName( "REL-" + i ) );
							  tx.Success();
						 }
					}

					if ( recoveryNeeded )
					{
						 File tmpLogs = new File( @base, "unrecovered" );
						 fileSystem.Mkdir( tmpLogs );
						 File txLogsDir = new File( storeDir, logicalLogsLocation );
						 foreach ( File file in fileSystem.ListFiles( txLogsDir, TransactionLogFiles.DEFAULT_FILENAME_FILTER ) )
						 {
							  fileSystem.CopyFile( file, new File( tmpLogs, file.Name ) );
						 }

						 Db.shutdown();

						 foreach ( File file in fileSystem.ListFiles( txLogsDir, TransactionLogFiles.DEFAULT_FILENAME_FILTER ) )
						 {
							  fileSystem.DeleteFile( file );
						 }

						 foreach ( File file in fileSystem.ListFiles( tmpLogs, TransactionLogFiles.DEFAULT_FILENAME_FILTER ) )
						 {
							  fileSystem.CopyFile( file, new File( txLogsDir, file.Name ) );
						 }
					}
					else
					{
						 Db.shutdown();
					}
			  }
		 }
	}

}