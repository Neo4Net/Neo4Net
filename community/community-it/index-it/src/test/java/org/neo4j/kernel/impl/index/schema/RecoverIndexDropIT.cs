﻿/*
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
namespace Org.Neo4j.Kernel.Impl.Index.Schema
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using IndexDefinition = Org.Neo4j.Graphdb.schema.IndexDefinition;
	using OpenMode = Org.Neo4j.Io.fs.OpenMode;
	using StoreChannel = Org.Neo4j.Io.fs.StoreChannel;
	using IndexMap = Org.Neo4j.Kernel.Impl.Api.index.IndexMap;
	using IndexProxy = Org.Neo4j.Kernel.Impl.Api.index.IndexProxy;
	using IndexingService = Org.Neo4j.Kernel.Impl.Api.index.IndexingService;
	using CommittedTransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.CommittedTransactionRepresentation;
	using LogPosition = Org.Neo4j.Kernel.impl.transaction.log.LogPosition;
	using LogicalTransactionStore = Org.Neo4j.Kernel.impl.transaction.log.LogicalTransactionStore;
	using PhysicalFlushableChannel = Org.Neo4j.Kernel.impl.transaction.log.PhysicalFlushableChannel;
	using TransactionCursor = Org.Neo4j.Kernel.impl.transaction.log.TransactionCursor;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;
	using LogEntryWriter = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntryWriter;
	using LogFiles = Org.Neo4j.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Org.Neo4j.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using RecoveryMonitor = Org.Neo4j.Kernel.recovery.RecoveryMonitor;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.count;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.TestLabels.LABEL_ONE;

	/// <summary>
	/// Issue came up when observing that recovering an INDEX DROP command didn't actually call <seealso cref="IndexProxy.drop()"/>,
	/// and actually did nothing to that <seealso cref="IndexProxy"/> except removing it from its <seealso cref="IndexMap"/>.
	/// This would have <seealso cref="IndexingService"/> forget about that index and at shutdown not call <seealso cref="IndexProxy.close()"/>,
	/// resulting in open page cache files, for any page cache mapped native index files.
	/// 
	/// This would be a problem if the INDEX DROP command was present in the transaction log, but the db had been killed
	/// before the command had been applied and so the files would still remain, and not be dropped either when that command
	/// was recovered.
	/// </summary>
	public class RecoverIndexDropIT
	{
		private bool InstanceFieldsInitialized = false;

		public RecoverIndexDropIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			Directory = TestDirectory.testDirectory( Fs );
		}

		 private const string KEY = "key";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.DefaultFileSystemRule fs = new org.neo4j.test.rule.fs.DefaultFileSystemRule();
		 public readonly DefaultFileSystemRule Fs = new DefaultFileSystemRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory directory = org.neo4j.test.rule.TestDirectory.testDirectory(fs);
		 public TestDirectory Directory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDropIndexOnRecovery() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDropIndexOnRecovery()
		 {
			  // given a transaction stream ending in an INDEX DROP command.
			  CommittedTransactionRepresentation dropTransaction = PrepareDropTransaction();
			  File storeDir = Directory.databaseDir();
			  GraphDatabaseService db = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabase(storeDir);
			  CreateIndex( db );
			  Db.shutdown();
			  AppendDropTransactionToTransactionLog( Directory.databaseDir(), dropTransaction );

			  // when recovering this (the drop transaction with the index file intact)
			  Monitors monitors = new Monitors();
			  AssertRecoveryIsPerformed recoveryMonitor = new AssertRecoveryIsPerformed();
			  monitors.AddMonitorListener( recoveryMonitor );
			  db = ( new TestGraphDatabaseFactory() ).setMonitors(monitors).newEmbeddedDatabase(storeDir);
			  try
			  {
					assertTrue( recoveryMonitor.RecoveryWasPerformed );

					// then
					using ( Transaction tx = Db.beginTx() )
					{
						 assertEquals( 0, count( Db.schema().Indexes ) );
						 tx.Success();
					}
			  }
			  finally
			  {
					// and the ability to shut down w/o failing on still open files
					Db.shutdown();
			  }
		 }

		 private static IndexDefinition CreateIndex( GraphDatabaseService db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					IndexDefinition index = Db.schema().indexFor(LABEL_ONE).on(KEY).create();
					tx.Success();
					return index;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void appendDropTransactionToTransactionLog(java.io.File databaseDirectory, org.neo4j.kernel.impl.transaction.CommittedTransactionRepresentation dropTransaction) throws java.io.IOException
		 private void AppendDropTransactionToTransactionLog( File databaseDirectory, CommittedTransactionRepresentation dropTransaction )
		 {
			  LogFiles logFiles = LogFilesBuilder.logFilesBasedOnlyBuilder( databaseDirectory, Fs ).build();
			  File logFile = logFiles.GetLogFileForVersion( logFiles.HighestLogVersion );
			  StoreChannel writeStoreChannel = Fs.open( logFile, OpenMode.READ_WRITE );
			  writeStoreChannel.Position( writeStoreChannel.size() );
			  using ( PhysicalFlushableChannel writeChannel = new PhysicalFlushableChannel( writeStoreChannel ) )
			  {
					( new LogEntryWriter( writeChannel ) ).serialize( dropTransaction );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.kernel.impl.transaction.CommittedTransactionRepresentation prepareDropTransaction() throws java.io.IOException
		 private CommittedTransactionRepresentation PrepareDropTransaction()
		 {
			  GraphDatabaseAPI db = ( GraphDatabaseAPI ) ( new TestGraphDatabaseFactory() ).newEmbeddedDatabase(Directory.directory("preparation"));
			  try
			  {
					// Create index
					IndexDefinition index;
					index = CreateIndex( db );
					using ( Transaction tx = Db.beginTx() )
					{
						 index.Drop();
						 tx.Success();
					}
					return ExtractLastTransaction( db );
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static org.neo4j.kernel.impl.transaction.CommittedTransactionRepresentation extractLastTransaction(org.neo4j.kernel.internal.GraphDatabaseAPI db) throws java.io.IOException
		 private static CommittedTransactionRepresentation ExtractLastTransaction( GraphDatabaseAPI db )
		 {
			  LogicalTransactionStore txStore = Db.DependencyResolver.resolveDependency( typeof( LogicalTransactionStore ) );
			  CommittedTransactionRepresentation transaction = null;
			  using ( TransactionCursor cursor = txStore.GetTransactions( Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID + 1 ) )
			  {
					while ( cursor.next() )
					{
						 transaction = cursor.get();
					}
			  }
			  return transaction;
		 }

		 private class AssertRecoveryIsPerformed : RecoveryMonitor
		 {
			  internal bool RecoveryWasPerformed;

			  public override void RecoveryRequired( LogPosition recoveryPosition )
			  {
					RecoveryWasPerformed = true;
			  }
		 }
	}

}