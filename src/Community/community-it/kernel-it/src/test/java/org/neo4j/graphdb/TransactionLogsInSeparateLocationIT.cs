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
namespace Neo4Net.Graphdb
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PhysicalLogVersionedStoreChannel = Neo4Net.Kernel.impl.transaction.log.PhysicalLogVersionedStoreChannel;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Neo4Net.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;
	using Neo4Net.Test.rule.fs;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class TransactionLogsInSeparateLocationIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();

		 private class ImpermanentDatabaseRuleAnonymousInnerClass : Neo4Net.Test.rule.ImpermanentDatabaseRule
		 {
			 protected internal override void configure( GraphDatabaseFactory databaseFactory )
			 {
				  base.configure( databaseFactory );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: List<KernelExtensionFactory<?>> extensions = Collections.singletonList(singleInstanceIndexProviderFactory("test", provider));
				  IList<KernelExtensionFactory<object>> extensions = Collections.singletonList( singleInstanceIndexProviderFactory( "test", provider ) );
				  ( ( TestGraphDatabaseFactory ) databaseFactory ).KernelExtensions = extensions;
			 }
		 }
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.FileSystemRule fileSystemRule = new org.neo4j.test.rule.fs.DefaultFileSystemRule();
		 public readonly FileSystemRule FileSystemRule = new DefaultFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void databaseWithTransactionLogsInSeparateRelativeLocation() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DatabaseWithTransactionLogsInSeparateRelativeLocation()
		 {
			  File databaseDirectory = TestDirectory.databaseDir();
			  File txDirectory = new File( databaseDirectory, "transaction-logs" );
			  PerformTransactions( txDirectory.Name, TestDirectory.databaseDir() );
			  VerifyTransactionLogs( txDirectory, databaseDirectory );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void databaseWithTransactionLogsInSeparateAbsoluteLocation() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DatabaseWithTransactionLogsInSeparateAbsoluteLocation()
		 {
			  File databaseDirectory = TestDirectory.databaseDir();
			  File txDirectory = TestDirectory.directory( "transaction-logs" );
			  PerformTransactions( txDirectory.AbsolutePath, TestDirectory.databaseDir() );
			  VerifyTransactionLogs( txDirectory, databaseDirectory );
		 }

		 private static void PerformTransactions( string txPath, File storeDir )
		 {
			  GraphDatabaseService database = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(storeDir).setConfig(GraphDatabaseSettings.logical_logs_location, txPath).newGraphDatabase();
			  for ( int i = 0; i < 10; i++ )
			  {
					using ( Transaction transaction = database.BeginTx() )
					{
						 Node node = database.CreateNode();
						 node.SetProperty( "a", "b" );
						 node.SetProperty( "c", "d" );
						 transaction.Success();
					}
			  }
			  database.Shutdown();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyTransactionLogs(java.io.File txDirectory, java.io.File storeDir) throws java.io.IOException
		 private void VerifyTransactionLogs( File txDirectory, File storeDir )
		 {
			  FileSystemAbstraction fileSystem = FileSystemRule.get();
			  LogFiles storeDirLogs = LogFilesBuilder.logFilesBasedOnlyBuilder( storeDir, fileSystem ).build();
			  assertFalse( storeDirLogs.VersionExists( 0 ) );

			  LogFiles txDirectoryLogs = LogFilesBuilder.logFilesBasedOnlyBuilder( txDirectory, fileSystem ).build();
			  assertTrue( txDirectoryLogs.VersionExists( 0 ) );
			  using ( PhysicalLogVersionedStoreChannel physicalLogVersionedStoreChannel = txDirectoryLogs.OpenForVersion( 0 ) )
			  {
					assertThat( physicalLogVersionedStoreChannel.Size(), greaterThan(0L) );
			  }
		 }

	}

}