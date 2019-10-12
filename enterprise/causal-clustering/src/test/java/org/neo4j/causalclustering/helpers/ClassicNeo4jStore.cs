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
namespace Org.Neo4j.causalclustering.helpers
{

	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Label = Org.Neo4j.Graphdb.Label;
	using Node = Org.Neo4j.Graphdb.Node;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using OnlineBackupSettings = Org.Neo4j.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using Standard = Org.Neo4j.Kernel.impl.store.format.standard.Standard;
	using TransactionLogFiles = Org.Neo4j.Kernel.impl.transaction.log.files.TransactionLogFiles;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;

	public class ClassicNeo4jStore
	{
		 private readonly File _storeDir;
		 private readonly File _logicalLogsDir;

		 private ClassicNeo4jStore( File storeDir, File logicalLogsDir )
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

		 public static Neo4jStoreBuilder Builder( File baseDir, FileSystemAbstraction fsa )
		 {
			  return new Neo4jStoreBuilder( baseDir, fsa );
		 }

		 public class Neo4jStoreBuilder
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

			  internal Neo4jStoreBuilder( File baseDir, FileSystemAbstraction fsa )
			  {

					this.BaseDir = baseDir;
					this.Fsa = fsa;
			  }

			  public virtual Neo4jStoreBuilder DbName( string @string )
			  {
					DbNameConflict = @string;
					return this;
			  }

			  public virtual Neo4jStoreBuilder NeedToRecover()
			  {
					NeedRecover = true;
					return this;
			  }

			  public virtual Neo4jStoreBuilder AmountOfNodes( int nodes )
			  {
					NrOfNodes = nodes;
					return this;
			  }

			  public virtual Neo4jStoreBuilder RecordFormats( string format )
			  {
					RecordsFormat = format;
					return this;
			  }

			  public virtual Neo4jStoreBuilder LogicalLogsLocation( string logicalLogsLocation )
			  {
					this.LogicalLogsLocationConflict = logicalLogsLocation;
					return this;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public ClassicNeo4jStore build() throws java.io.IOException
			  public virtual ClassicNeo4jStore Build()
			  {
					CreateStore( BaseDir, Fsa, DbNameConflict, NrOfNodes, RecordsFormat, NeedRecover, LogicalLogsLocationConflict );
					File storeDir = new File( BaseDir, DbNameConflict );
					return new ClassicNeo4jStore( storeDir, new File( storeDir, LogicalLogsLocationConflict ) );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void createStore(java.io.File super, org.neo4j.io.fs.FileSystemAbstraction fileSystem, String dbName, int nodesToCreate, String recordFormat, boolean recoveryNeeded, String logicalLogsLocation) throws java.io.IOException
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