﻿using System;

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
namespace Org.Neo4j.Kernel.impl.transaction.log.entry
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;

	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Node = Org.Neo4j.Graphdb.Node;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using EphemeralFileSystemAbstraction = Org.Neo4j.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Org.Neo4j.Test.rule.fs.EphemeralFileSystemRule;

	public class TestTxEntries
	{
		private bool InstanceFieldsInitialized = false;

		public TestTxEntries()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _fs ).around( _testDirectory );
		}

		 private readonly EphemeralFileSystemRule _fs = new EphemeralFileSystemRule();
		 private readonly TestDirectory _testDirectory = TestDirectory.testDirectory();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(fs).around(testDirectory);
		 public RuleChain RuleChain;

		 /*
		  * Starts a JVM, executes a tx that fails on prepare and rollbacks,
		  * triggering a bug where an extra start entry for that tx is written
		  * in the xa log.
		  */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testStartEntryWrittenOnceOnRollback() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestStartEntryWrittenOnceOnRollback()
		 {
			  File storeDir = _testDirectory.databaseDir();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.GraphDatabaseService db = new org.neo4j.test.TestGraphDatabaseFactory().setFileSystem(fs.get()).newImpermanentDatabase(storeDir);
			  GraphDatabaseService db = ( new TestGraphDatabaseFactory() ).setFileSystem(_fs.get()).newImpermanentDatabase(storeDir);
			  CreateSomeTransactions( db );
			  EphemeralFileSystemAbstraction snapshot = _fs.snapshot( Db.shutdown );

			  ( new TestGraphDatabaseFactory() ).setFileSystem(snapshot).newImpermanentDatabase(storeDir).shutdown();
		 }

		 private void CreateSomeTransactions( GraphDatabaseService db )
		 {
			  Transaction tx = Db.beginTx();
			  Node node1 = Db.createNode();
			  Node node2 = Db.createNode();
			  node1.CreateRelationshipTo( node2, RelationshipType.withName( "relType1" ) );
			  tx.Success();
			  tx.Close();

			  tx = Db.beginTx();
			  node1.Delete();
			  tx.Success();
			  try
			  {
					// Will throw exception, causing the tx to be rolledback.
					tx.Close();
			  }
			  catch ( Exception )
			  {
					// InvalidRecordException coming, node1 has rels
			  }
			  /*
			   *  The damage has already been done. The following just makes sure
			   *  the corrupting tx is flushed to disk, since we will exit
			   *  uncleanly.
			   */
			  tx = Db.beginTx();
			  node1.SetProperty( "foo", "bar" );
			  tx.Success();
			  tx.Close();
		 }
	}

}