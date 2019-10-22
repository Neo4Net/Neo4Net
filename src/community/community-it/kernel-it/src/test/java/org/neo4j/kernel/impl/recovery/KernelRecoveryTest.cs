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
namespace Neo4Net.Kernel.impl.recovery
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using EphemeralFileSystemAbstraction = Neo4Net.GraphDb.mockfs.EphemeralFileSystemAbstraction;
	using UncloseableDelegatingFileSystemAbstraction = Neo4Net.GraphDb.mockfs.UncloseableDelegatingFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using StatementConstants = Neo4Net.Kernel.api.StatementConstants;
	using NodeCommand = Neo4Net.Kernel.impl.transaction.command.Command.NodeCommand;
	using NodeCountsCommand = Neo4Net.Kernel.impl.transaction.command.Command.NodeCountsCommand;
	using LogPosition = Neo4Net.Kernel.impl.transaction.log.LogPosition;
	using TransactionLogFiles = Neo4Net.Kernel.impl.transaction.log.files.TransactionLogFiles;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.mockito.matcher.LogMatchers.checkPoint;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.mockito.matcher.LogMatchers.commandEntry;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.mockito.matcher.LogMatchers.commitEntry;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.mockito.matcher.LogMatchers.containsExactly;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.mockito.matcher.LogMatchers.logEntries;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.mockito.matcher.LogMatchers.startEntry;

	public class KernelRecoveryTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.fs.EphemeralFileSystemRule fsRule = new org.Neo4Net.test.rule.fs.EphemeralFileSystemRule();
		 public readonly EphemeralFileSystemRule FsRule = new EphemeralFileSystemRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.TestDirectory testDirectory = org.Neo4Net.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleWritesProperlyAfterRecovery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleWritesProperlyAfterRecovery()
		 {
			  // Given
			  EphemeralFileSystemAbstraction fs = FsRule.get();
			  IGraphDatabaseService db = NewDB( fs );

			  long node1 = CreateNode( db );

			  // And given the power goes out
			  using ( EphemeralFileSystemAbstraction crashedFs = fs.Snapshot() )
			  {
					Db.shutdown();
					db = NewDB( crashedFs );

					long node2 = CreateNode( db );
					Db.shutdown();

					// Then the logical log should be in sync
					File logFile = TestDirectory.databaseLayout().file(TransactionLogFiles.DEFAULT_NAME + ".0");
					assertThat( logEntries( crashedFs, logFile ), containsExactly( startEntry( -1, -1 ), commandEntry( node1, typeof( NodeCommand ) ), commandEntry( StatementConstants.ANY_LABEL, typeof( NodeCountsCommand ) ), commitEntry( 2 ), startEntry( -1, -1 ), commandEntry( node2, typeof( NodeCommand ) ), commandEntry( StatementConstants.ANY_LABEL, typeof( NodeCountsCommand ) ), commitEntry( 3 ), checkPoint( new LogPosition( 0, 250 ) ) ) );
			  }
		 }

		 private IGraphDatabaseService NewDB( FileSystemAbstraction fs )
		 {
			  return ( new TestGraphDatabaseFactory() ).setFileSystem(new UncloseableDelegatingFileSystemAbstraction(fs)).newImpermanentDatabase(TestDirectory.databaseDir());
		 }

		 private static long CreateNode( IGraphDatabaseService db )
		 {
			  long node1;
			  using ( Transaction tx = Db.beginTx() )
			  {
					node1 = Db.createNode().Id;
					tx.Success();
			  }
			  return node1;
		 }
	}

}