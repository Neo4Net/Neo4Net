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
namespace Org.Neo4j.Kernel.counts
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Label = Org.Neo4j.Graphdb.Label;
	using Node = Org.Neo4j.Graphdb.Node;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using Read = Org.Neo4j.@internal.Kernel.Api.Read;
	using TokenRead = Org.Neo4j.@internal.Kernel.Api.TokenRead;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using StatementConstants = Org.Neo4j.Kernel.api.StatementConstants;
	using ThreadToStatementContextBridge = Org.Neo4j.Kernel.impl.core.ThreadToStatementContextBridge;
	using DatabaseRule = Org.Neo4j.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Org.Neo4j.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;

	public class LabelCountsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.DatabaseRule db = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public readonly DatabaseRule Db = new ImpermanentDatabaseRule();

		 private System.Func<KernelTransaction> _transactionSupplier;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void exposeGuts()
		 public virtual void ExposeGuts()
		 {
			  _transactionSupplier = () => Db.GraphDatabaseAPI.DependencyResolver.resolveDependency(typeof(ThreadToStatementContextBridge)).getKernelTransactionBoundToThisThread(true);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetNumberOfNodesWithLabel()
		 public virtual void ShouldGetNumberOfNodesWithLabel()
		 {
			  // given
			  GraphDatabaseService graphDb = Db.GraphDatabaseAPI;
			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					graphDb.CreateNode( label( "Foo" ) );
					graphDb.CreateNode( label( "Bar" ) );
					graphDb.CreateNode( label( "Bar" ) );

					tx.Success();
			  }

			  // when
			  long fooCount = NumberOfNodesWith( label( "Foo" ) );
			  long barCount = NumberOfNodesWith( label( "Bar" ) );

			  // then
			  assertEquals( 1, fooCount );
			  assertEquals( 2, barCount );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAccountForDeletedNodes()
		 public virtual void ShouldAccountForDeletedNodes()
		 {
			  // given
			  GraphDatabaseService graphDb = Db.GraphDatabaseAPI;
			  Node node;
			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					node = graphDb.CreateNode( label( "Foo" ) );
					graphDb.CreateNode( label( "Foo" ) );

					tx.Success();
			  }
			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					node.Delete();

					tx.Success();
			  }

			  // when
			  long fooCount = NumberOfNodesWith( label( "Foo" ) );

			  // then
			  assertEquals( 1, fooCount );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAccountForDeletedNodesWithMultipleLabels()
		 public virtual void ShouldAccountForDeletedNodesWithMultipleLabels()
		 {
			  // given
			  GraphDatabaseService graphDb = Db.GraphDatabaseAPI;
			  Node node;
			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					node = graphDb.CreateNode( label( "Foo" ), label( "Bar" ) );
					graphDb.CreateNode( label( "Foo" ) );
					graphDb.CreateNode( label( "Bar" ) );

					tx.Success();
			  }
			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					node.Delete();

					tx.Success();
			  }

			  // when
			  long fooCount = NumberOfNodesWith( label( "Foo" ) );
			  long barCount = NumberOfNodesWith( label( "Bar" ) );

			  // then
			  assertEquals( 1, fooCount );
			  assertEquals( 1, barCount );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAccountForAddedLabels()
		 public virtual void ShouldAccountForAddedLabels()
		 {
			  // given
			  GraphDatabaseService graphDb = Db.GraphDatabaseAPI;
			  Node n1;
			  Node n2;
			  Node n3;
			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					n1 = graphDb.CreateNode( label( "Foo" ) );
					n2 = graphDb.CreateNode();
					n3 = graphDb.CreateNode();

					tx.Success();
			  }
			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					n1.AddLabel( label( "Bar" ) );
					n2.AddLabel( label( "Bar" ) );
					n3.AddLabel( label( "Foo" ) );

					tx.Success();
			  }

			  // when
			  long fooCount = NumberOfNodesWith( label( "Foo" ) );
			  long barCount = NumberOfNodesWith( label( "Bar" ) );

			  // then
			  assertEquals( 2, fooCount );
			  assertEquals( 2, barCount );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAccountForRemovedLabels()
		 public virtual void ShouldAccountForRemovedLabels()
		 {
			  // given
			  GraphDatabaseService graphDb = Db.GraphDatabaseAPI;
			  Node n1;
			  Node n2;
			  Node n3;
			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					n1 = graphDb.CreateNode( label( "Foo" ), label( "Bar" ) );
					n2 = graphDb.CreateNode( label( "Bar" ) );
					n3 = graphDb.CreateNode( label( "Foo" ) );

					tx.Success();
			  }
			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					n1.RemoveLabel( label( "Bar" ) );
					n2.RemoveLabel( label( "Bar" ) );
					n3.RemoveLabel( label( "Foo" ) );

					tx.Success();
			  }

			  // when
			  long fooCount = NumberOfNodesWith( label( "Foo" ) );
			  long barCount = NumberOfNodesWith( label( "Bar" ) );

			  // then
			  assertEquals( 1, fooCount );
			  assertEquals( 0, barCount );
		 }

		 /// <summary>
		 /// Transactional version of <seealso cref="countsForNode(Label)"/> </summary>
		 private long NumberOfNodesWith( Label label )
		 {
			  using ( Transaction tx = Db.GraphDatabaseAPI.beginTx() )
			  {
					long nodeCount = CountsForNode( label );
					tx.Success();
					return nodeCount;
			  }
		 }

		 /// <param name="label"> the label to get the number of nodes of, or {@code null} to get the total number of nodes. </param>
		 private long CountsForNode( Label label )
		 {
			  KernelTransaction transaction = _transactionSupplier.get();
			  Read read = transaction.DataRead();
			  int labelId;
			  if ( label == null )
			  {
					labelId = StatementConstants.ANY_LABEL;
			  }
			  else
			  {
					if ( Org.Neo4j.@internal.Kernel.Api.TokenRead_Fields.NO_TOKEN == ( labelId = transaction.TokenRead().nodeLabel(label.Name()) ) )
					{
						 return 0;
					}
			  }
			  return read.CountsForNode( labelId );
		 }

	}

}