using System;

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
namespace Neo4Net.Kernel.impl.transaction.log.pruning
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Transaction = Neo4Net.GraphDb.Transaction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using Config = Neo4Net.Kernel.configuration.Config;
	using CheckPointer = Neo4Net.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using SimpleTriggerInfo = Neo4Net.Kernel.impl.transaction.log.checkpoint.SimpleTriggerInfo;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Neo4Net.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using LogRotation = Neo4Net.Kernel.impl.transaction.log.rotation.LogRotation;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using EmbeddedDatabaseRule = Neo4Net.Test.rule.EmbeddedDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.factory.GraphDatabaseSettings.keep_logical_logs;

	public class LogPruningIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.DatabaseRule db = new Neo4Net.test.rule.EmbeddedDatabaseRule().withSetting(keep_logical_logs, "true");
		 public readonly DatabaseRule Db = new EmbeddedDatabaseRule().withSetting(keep_logical_logs, "true");

		 private static readonly SimpleTriggerInfo _triggerInfo = new SimpleTriggerInfo( "forced trigger" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pruningStrategyShouldBeDynamic() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PruningStrategyShouldBeDynamic()
		 {
			  CheckPointer checkPointer = GetInstanceFromDb( typeof( CheckPointer ) );
			  Config config = GetInstanceFromDb( typeof( Config ) );
			  FileSystemAbstraction fs = GetInstanceFromDb( typeof( FileSystemAbstraction ) );

			  LogFiles logFiles = LogFilesBuilder.builder( Db.databaseLayout(), fs ).withLogVersionRepository(new SimpleLogVersionRepository()).withLastCommittedTransactionIdSupplier(() => 1).withTransactionIdStore(new SimpleTransactionIdStore()).build();

			  // Force transaction log rotation
			  WriteTransactionsAndRotateTwice();

			  // Checkpoint to make sure strategy is evaluated
			  checkPointer.ForceCheckPoint( _triggerInfo );

			  // Make sure file is still there since we have disable pruning
			  assertThat( CountTransactionLogs( logFiles ), @is( 3 ) );

			  // Change pruning to true
			  config.UpdateDynamicSetting( keep_logical_logs.name(), "false", "test" );

			  // Checkpoint to make sure strategy is evaluated
			  checkPointer.ForceCheckPoint( _triggerInfo );

			  // Make sure file is removed
			  assertThat( CountTransactionLogs( logFiles ), @is( 2 ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeTransactionsAndRotateTwice() throws java.io.IOException
		 private void WriteTransactionsAndRotateTwice()
		 {
			  LogRotation logRotation = Db.DependencyResolver.resolveDependency( typeof( LogRotation ) );
			  // Apparently we always keep an extra log file what even though the threshold is reached... produce two then
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.createNode();
					tx.Success();
			  }
			  logRotation.RotateLogFile();
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.createNode();
					tx.Success();
			  }
			  logRotation.RotateLogFile();
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.createNode();
					tx.Success();
			  }
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.createNode();
					tx.Success();
			  }
		 }

		 private T GetInstanceFromDb<T>( Type clazz )
		 {
				 clazz = typeof( T );
			  return Db.DependencyResolver.resolveDependency( clazz );
		 }

		 private int CountTransactionLogs( LogFiles logFiles )
		 {
			  return logFiles.LogFilesConflict().Length;
		 }
	}

}