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
namespace Org.Neo4j.Kernel.impl.recovery
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using EphemeralFileSystemAbstraction = Org.Neo4j.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using UncloseableDelegatingFileSystemAbstraction = Org.Neo4j.Graphdb.mockfs.UncloseableDelegatingFileSystemAbstraction;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using StatementConstants = Org.Neo4j.Kernel.api.StatementConstants;
	using NodeCommand = Org.Neo4j.Kernel.impl.transaction.command.Command.NodeCommand;
	using NodeCountsCommand = Org.Neo4j.Kernel.impl.transaction.command.Command.NodeCountsCommand;
	using LogPosition = Org.Neo4j.Kernel.impl.transaction.log.LogPosition;
	using TransactionLogFiles = Org.Neo4j.Kernel.impl.transaction.log.files.TransactionLogFiles;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Org.Neo4j.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.LogMatchers.checkPoint;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.LogMatchers.commandEntry;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.LogMatchers.commitEntry;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.LogMatchers.containsExactly;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.LogMatchers.logEntries;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.LogMatchers.startEntry;

	public class KernelRecoveryTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.EphemeralFileSystemRule fsRule = new org.neo4j.test.rule.fs.EphemeralFileSystemRule();
		 public readonly EphemeralFileSystemRule FsRule = new EphemeralFileSystemRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleWritesProperlyAfterRecovery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleWritesProperlyAfterRecovery()
		 {
			  // Given
			  EphemeralFileSystemAbstraction fs = FsRule.get();
			  GraphDatabaseService db = NewDB( fs );

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

		 private GraphDatabaseService NewDB( FileSystemAbstraction fs )
		 {
			  return ( new TestGraphDatabaseFactory() ).setFileSystem(new UncloseableDelegatingFileSystemAbstraction(fs)).newImpermanentDatabase(TestDirectory.databaseDir());
		 }

		 private static long CreateNode( GraphDatabaseService db )
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