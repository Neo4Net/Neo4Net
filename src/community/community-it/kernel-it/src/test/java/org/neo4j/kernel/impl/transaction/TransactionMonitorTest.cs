using System;
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
namespace Neo4Net.Kernel.impl.transaction
{
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using Neo4Net.Functions;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using TransactionCounters = Neo4Net.Kernel.impl.transaction.stats.TransactionCounters;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class TransactionMonitorTest
	public class TransactionMonitorTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(0) public org.Neo4Net.function.ThrowingConsumer<org.Neo4Net.graphdb.GraphDatabaseService,Exception> dbConsumer;
		 public ThrowingConsumer<GraphDatabaseService, Exception> DbConsumer;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(1) public boolean isWriteTx;
		 public bool IsWriteTx;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(2) public String ignored;
		 public string Ignored; // to make JUnit happy...

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{2}") public static java.util.Collection<Object[]> parameters()
		 public static ICollection<object[]> Parameters()
		 {
			  return Arrays.asList(new object[]{(ThrowingConsumer<GraphDatabaseService, Exception>) db =>
			  {
			  }, false, "read"}, new object[]{ ( ThrowingConsumer<GraphDatabaseService,Exception> ) IGraphDatabaseService.createNode, true, "write" });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCountCommittedTransactions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void shouldCountCommittedTransactions()
		 {
			  GraphDatabaseAPI db = ( GraphDatabaseAPI ) ( new TestGraphDatabaseFactory() ).newImpermanentDatabase();
			  try
			  {
					TransactionCounters counts = Db.DependencyResolver.resolveDependency( typeof( TransactionCounters ) );
					TransactionCountersChecker checker = new TransactionCountersChecker( counts );
					using ( Transaction tx = Db.beginTx() )
					{
						 DbConsumer.accept( db );
						 tx.Success();
					}
					checker.VerifyCommitted( IsWriteTx, counts );
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCountRolledBackTransactions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void shouldCountRolledBackTransactions()
		 {
			  GraphDatabaseAPI db = ( GraphDatabaseAPI ) ( new TestGraphDatabaseFactory() ).newImpermanentDatabase();
			  try
			  {
					TransactionCounters counts = Db.DependencyResolver.resolveDependency( typeof( TransactionCounters ) );
					TransactionCountersChecker checker = new TransactionCountersChecker( counts );
					using ( Transaction tx = Db.beginTx() )
					{
						 DbConsumer.accept( db );
						 tx.Failure();
					}
					checker.VerifyRolledBacked( IsWriteTx, counts );
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCountTerminatedTransactions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void shouldCountTerminatedTransactions()
		 {
			  GraphDatabaseAPI db = ( GraphDatabaseAPI ) ( new TestGraphDatabaseFactory() ).newImpermanentDatabase();
			  try
			  {
					TransactionCounters counts = Db.DependencyResolver.resolveDependency( typeof( TransactionCounters ) );
					TransactionCountersChecker checker = new TransactionCountersChecker( counts );
					using ( Transaction tx = Db.beginTx() )
					{
						 DbConsumer.accept( db );
						 tx.Terminate();
					}
					checker.VerifyTerminated( IsWriteTx, counts );
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }
	}

}