using System;
using System.Collections.Generic;
using System.Threading;

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
namespace Org.Neo4j.Kernel.impl.core
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Node = Org.Neo4j.Graphdb.Node;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using RelationshipIterator = Org.Neo4j.Kernel.Impl.Api.store.RelationshipIterator;
	using ImpermanentDatabaseRule = Org.Neo4j.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Thread.sleep;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Direction.OUTGOING;

	/// <summary>
	/// Ensures the absence of an issue where iterating through a <seealso cref="RelationshipIterator"/> would result in
	/// <seealso cref="System.IndexOutOfRangeException"/> due to incrementing an array index too eagerly so that a consecutive
	/// call to <seealso cref="RelationshipIterator.next()"/> would try to get the internal type iterator with a too high index.
	/// 
	/// This test is probabilistic in trying to produce the issue. There's a chance this test will be unsuccessful in
	/// reproducing the issue (test being successful where it should have failed), but it will never randomly fail
	/// where it should have been successful. After the point where the issue has been fixed this test will use
	/// the full 0.5 seconds to try to reproduce it.
	/// 
	/// </summary>
	public class ConcurrentCreateAndGetRelationshipsIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.ImpermanentDatabaseRule dbRule = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public readonly ImpermanentDatabaseRule DbRule = new ImpermanentDatabaseRule();
		 private const RelationshipType RELTYPE = MyRelTypes.TEST;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void tryToReproduceTheIssue() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TryToReproduceTheIssue()
		 {
			  // GIVEN
			  GraphDatabaseService db = DbRule.GraphDatabaseAPI;
			  System.Threading.CountdownEvent startSignal = new System.Threading.CountdownEvent( 1 );
			  AtomicBoolean stopSignal = new AtomicBoolean();
			  AtomicReference<Exception> failure = new AtomicReference<Exception>();
			  Node parentNode = CreateNode( db );
			  ICollection<Worker> workers = CreateWorkers( db, startSignal, stopSignal, failure, parentNode );

			  // WHEN
			  startSignal.Signal();
			  sleep( 500 );
			  stopSignal.set( true );
			  AwaitWorkersToEnd( workers );

			  // THEN
			  if ( failure.get() != null )
			  {
					throw new Exception( "A worker failed", failure.get() );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void awaitWorkersToEnd(java.util.Collection<Worker> workers) throws InterruptedException
		 private void AwaitWorkersToEnd( ICollection<Worker> workers )
		 {
			  foreach ( Worker worker in workers )
			  {
					worker.Join();
			  }
		 }

		 private ICollection<Worker> CreateWorkers( GraphDatabaseService db, System.Threading.CountdownEvent startSignal, AtomicBoolean stopSignal, AtomicReference<Exception> failure, Node parentNode )
		 {
			  ICollection<Worker> workers = new List<Worker>();
			  for ( int i = 0; i < 2; i++ )
			  {
					workers.Add( NewWorker( db, startSignal, stopSignal, failure, parentNode ) );
			  }
			  return workers;
		 }

		 private Worker NewWorker( GraphDatabaseService db, System.Threading.CountdownEvent startSignal, AtomicBoolean stopSignal, AtomicReference<Exception> failure, Node parentNode )
		 {
			  Worker worker = new Worker( db, startSignal, stopSignal, failure, parentNode );
			  worker.Start();
			  return worker;
		 }

		 private Node CreateNode( GraphDatabaseService db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode();
					tx.Success();
					return node;
			  }
		 }

		 private class Worker : Thread
		 {
			  internal readonly GraphDatabaseService Db;
			  internal readonly System.Threading.CountdownEvent StartSignal;
			  internal readonly AtomicReference<Exception> Failure;
			  internal readonly Node ParentNode;
			  internal readonly AtomicBoolean StopSignal;

			  internal Worker( GraphDatabaseService db, System.Threading.CountdownEvent startSignal, AtomicBoolean stopSignal, AtomicReference<Exception> failure, Node parentNode )
			  {
					this.Db = db;
					this.StartSignal = startSignal;
					this.StopSignal = stopSignal;
					this.Failure = failure;
					this.ParentNode = parentNode;
			  }

			  public override void Run()
			  {
					AwaitStartSignal();
					while ( Failure.get() == null && !StopSignal.get() )
					{
						 try
						 {
								 using ( Transaction tx = Db.beginTx() )
								 {
								  // ArrayIndexOutOfBoundsException happens here
								  Iterables.count( ParentNode.getRelationships( RELTYPE, OUTGOING ) );
      
								  ParentNode.createRelationshipTo( Db.createNode(), RELTYPE );
								  tx.Success();
								 }
						 }
						 catch ( Exception e )
						 {
							  Failure.compareAndSet( null, e );
						 }
					}
			  }

			  internal virtual void AwaitStartSignal()
			  {
					try
					{
						 StartSignal.await( 10, SECONDS );
					}
					catch ( InterruptedException e )
					{
						 throw new Exception( e );
					}
			  }
		 }
	}

}