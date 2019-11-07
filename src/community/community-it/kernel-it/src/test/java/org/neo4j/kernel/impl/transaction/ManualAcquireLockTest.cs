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
namespace Neo4Net.Kernel.impl.transaction
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;
	using TestRule = org.junit.rules.TestRule;

	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Lock = Neo4Net.GraphDb.Lock;
	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Neo4Net.Test;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using GraphTransactionRule = Neo4Net.Test.rule.GraphTransactionRule;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class ManualAcquireLockTest
	{
		private bool InstanceFieldsInitialized = false;

		public ManualAcquireLockTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			Tx = new GraphTransactionRule( Db );
			Chain = RuleChain.outerRule( Db ).around( Tx );
		}

		 public readonly DatabaseRule Db = new ImpermanentDatabaseRule();
		 public GraphTransactionRule Tx;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.TestRule chain = org.junit.rules.RuleChain.outerRule(db).around(tx);
		 public TestRule Chain;

		 private Worker _worker;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void doBefore()
		 public virtual void DoBefore()
		 {
			  _worker = new Worker( this );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void doAfter()
		 public virtual void DoAfter()
		 {
			  _worker.Dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void releaseReleaseManually() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReleaseReleaseManually()
		 {
			  string key = "name";
			  Node node = GraphDb.createNode();

			  Tx.success();
			  Transaction current = Tx.begin();
			  Lock nodeLock = current.AcquireWriteLock( node );
			  _worker.beginTx();
			  try
			  {
					_worker.setProperty( node, key, "ksjd" );
					fail( "Shouldn't be able to grab it" );
			  }
			  catch ( Exception )
			  {
			  }
			  nodeLock.Release();
			  _worker.setProperty( node, key, "yo" );

			  try
			  {
					_worker.finishTx();
			  }
			  catch ( ExecutionException )
			  {
					// Ok, interrupting the thread while it's waiting for a lock will lead to tx failure.
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canOnlyReleaseOnce()
		 public virtual void CanOnlyReleaseOnce()
		 {
			  Node node = GraphDb.createNode();

			  Tx.success();
			  Transaction current = Tx.begin();
			  Lock nodeLock = current.AcquireWriteLock( node );
			  nodeLock.Release();
			  try
			  {
					nodeLock.Release();
					fail( "Shouldn't be able to release more than once" );
			  }
			  catch ( System.InvalidOperationException )
			  { // Good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureNodeStaysLockedEvenAfterManualRelease() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MakeSureNodeStaysLockedEvenAfterManualRelease()
		 {
			  string key = "name";
			  Node node = GraphDb.createNode();

			  Tx.success();
			  Transaction current = Tx.begin();
			  Lock nodeLock = current.AcquireWriteLock( node );
			  node.SetProperty( key, "value" );
			  nodeLock.Release();

			  _worker.beginTx();
			  try
			  {
					_worker.setProperty( node, key, "ksjd" );
					fail( "Shouldn't be able to grab it" );
			  }
			  catch ( Exception )
			  {
			  }

			  Tx.success();

			  try
			  {
					_worker.finishTx();
			  }
			  catch ( ExecutionException )
			  {
					// Ok, interrupting the thread while it's waiting for a lock will lead to tx failure.
			  }
		 }

		 private IGraphDatabaseService GraphDb
		 {
			 get
			 {
				  return Db.GraphDatabaseAPI;
			 }
		 }

		 private class State
		 {
			 private readonly ManualAcquireLockTest _outerInstance;

			  internal readonly IGraphDatabaseService GraphDb;
			  internal Transaction Tx;

			  internal State( ManualAcquireLockTest outerInstance, IGraphDatabaseService graphDb )
			  {
				  this._outerInstance = outerInstance;
					this.GraphDb = graphDb;
			  }
		 }

		 private class Worker : OtherThreadExecutor<State>
		 {
			 private readonly ManualAcquireLockTest _outerInstance;

			  internal Worker( ManualAcquireLockTest outerInstance ) : base( "other thread", new State( outerInstance, outerInstance.GraphDb ) )
			  {
				  this._outerInstance = outerInstance;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void beginTx() throws Exception
			  internal virtual void BeginTx()
			  {
					Execute((WorkerCommand<State, Void>) StateConflict =>
					{
					 StateConflict.tx = StateConflict.graphDb.beginTx();
					 return null;
					});
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void finishTx() throws Exception
			  internal virtual void FinishTx()
			  {
					Execute((WorkerCommand<State, Void>) StateConflict =>
					{
					 StateConflict.tx.success();
					 StateConflict.tx.close();
					 return null;
					});
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void setProperty(final Neo4Net.graphdb.Node node, final String key, final Object value) throws Exception
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
			  internal virtual void SetProperty( Node node, string key, object value )
			  {
					Execute(StateConflict =>
					{
					 node.SetProperty( key, value );
					 return null;
					}, 2, SECONDS);
			  }
		 }
	}

}