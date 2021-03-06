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
namespace Org.Neo4j.Kernel.Impl.Api.state
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Node = Org.Neo4j.Graphdb.Node;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using Org.Neo4j.Graphdb.index;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using TestLabels = Org.Neo4j.Test.TestLabels;
	using DatabaseRule = Org.Neo4j.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Org.Neo4j.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.index.IndexManager_Fields.PROVIDER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.DummyIndexExtensionFactory.IDENTIFIER;

	public class NoChangeWriteTransactionTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.DatabaseRule dbr = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public readonly DatabaseRule Dbr = new ImpermanentDatabaseRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIdentifyTransactionWithNetZeroChangesAsReadOnly()
		 public virtual void ShouldIdentifyTransactionWithNetZeroChangesAsReadOnly()
		 {
			  // GIVEN a transaction that has seen some changes, where all those changes result in a net 0 change set
			  // a good way of producing such state is to add a label to an existing node, and then remove it.
			  GraphDatabaseAPI db = Dbr.GraphDatabaseAPI;
			  TransactionIdStore txIdStore = Db.DependencyResolver.resolveDependency( typeof( TransactionIdStore ) );
			  long startTxId = txIdStore.LastCommittedTransactionId;
			  Node node = CreateEmptyNode( db );
			  using ( Transaction tx = Db.beginTx() )
			  {
					node.AddLabel( TestLabels.LABEL_ONE );
					node.RemoveLabel( TestLabels.LABEL_ONE );
					tx.Success();
			  } // WHEN closing that transaction

			  // THEN it should not have been committed
			  assertEquals( "Expected last txId to be what it started at + 2 (1 for the empty node, and one for the label)", startTxId + 2, txIdStore.LastCommittedTransactionId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectNoChangesInCommitsAlsoForTheIndexes()
		 public virtual void ShouldDetectNoChangesInCommitsAlsoForTheIndexes()
		 {
			  // GIVEN a transaction that has seen some changes, where all those changes result in a net 0 change set
			  // a good way of producing such state is to add a label to an existing node, and then remove it.
			  GraphDatabaseAPI db = Dbr.GraphDatabaseAPI;
			  TransactionIdStore txIdStore = Db.DependencyResolver.resolveDependency( typeof( TransactionIdStore ) );
			  long startTxId = txIdStore.LastCommittedTransactionId;
			  Node node = CreateEmptyNode( db );
			  Index<Node> index = CreateNodeIndex( db );
			  using ( Transaction tx = Db.beginTx() )
			  {
					node.AddLabel( TestLabels.LABEL_ONE );
					node.RemoveLabel( TestLabels.LABEL_ONE );
					index.Add( node, "key", "value" );
					index.Remove( node, "key", "value" );
					tx.Success();
			  } // WHEN closing that transaction

			  // THEN it should not have been committed
			  assertEquals( "Expected last txId to be what it started at + 3 " + "(1 for the empty node, 1 for index, and one for the label)", startTxId + 3, txIdStore.LastCommittedTransactionId );
		 }

		 private Index<Node> CreateNodeIndex( GraphDatabaseAPI db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Index<Node> index = Db.index().forNodes("test", stringMap(PROVIDER, IDENTIFIER));
					tx.Success();
					return index;
			  }
		 }

		 private Node CreateEmptyNode( GraphDatabaseService db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode();
					tx.Success();
					return node;
			  }
		 }

	}

}