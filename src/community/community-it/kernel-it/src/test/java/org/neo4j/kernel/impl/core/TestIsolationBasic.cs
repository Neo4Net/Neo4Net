using System;
using System.Threading;

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
namespace Neo4Net.Kernel.impl.core
{
	using Test = org.junit.Test;


	using Node = Neo4Net.GraphDb.Node;
	using IPropertyContainer = Neo4Net.GraphDb.PropertyContainer;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Transaction = Neo4Net.GraphDb.Transaction;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class TestIsolationBasic : AbstractNeo4NetTestCase
	{
		 /*
		  * Tests that changes performed in a transaction before commit are not apparent in another.
		  */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSimpleTransactionIsolation() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestSimpleTransactionIsolation()
		 {
			  // Start setup - create base data
			  Commit();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch latch1 = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent latch1 = new System.Threading.CountdownEvent( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch latch2 = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent latch2 = new System.Threading.CountdownEvent( 1 );
			  Node n1;
			  Node n2;
			  Relationship r1;
			  using ( Transaction tx = GraphDb.beginTx() )
			  {
					n1 = GraphDb.createNode();
					n2 = GraphDb.createNode();
					r1 = n1.CreateRelationshipTo( n2, RelationshipType.withName( "TEST" ) );
					tx.Success();
			  }

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.graphdb.Node node1 = n1;
			  Node node1 = n1;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.graphdb.Node node2 = n2;
			  Node node2 = n2;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.graphdb.Relationship rel1 = r1;
			  Relationship rel1 = r1;

			  using ( Transaction tx = GraphDb.beginTx() )
			  {
					node1.SetProperty( "key", "old" );
					rel1.SetProperty( "key", "old" );
					tx.Success();
			  }
			  AssertPropertyEqual( node1, "key", "old" );
			  AssertPropertyEqual( rel1, "key", "old" );
			  AssertRelationshipCount( node1, 1 );
			  AssertRelationshipCount( node2, 1 );

			  // This is the mutating transaction - it will change stuff which will be read in between
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicReference<Exception> t1Exception = new java.util.concurrent.atomic.AtomicReference<>();
			  AtomicReference<Exception> t1Exception = new AtomicReference<Exception>();
			  Thread t1 = new Thread(() =>
			  {

				try
				{
					using ( Transaction tx = GraphDb.beginTx() )
					{
						 node1.SetProperty( "key", "new" );
						 rel1.SetProperty( "key", "new" );
						 node1.CreateRelationshipTo( node2, RelationshipType.withName( "TEST" ) );
						 AssertPropertyEqual( node1, "key", "new" );
						 AssertPropertyEqual( rel1, "key", "new" );
						 AssertRelationshipCount( node1, 2 );
						 AssertRelationshipCount( node2, 2 );
						 latch1.Signal();
						 latch2.await();
						 AssertPropertyEqual( node1, "key", "new" );
						 AssertPropertyEqual( rel1, "key", "new" );
						 AssertRelationshipCount( node1, 2 );
						 AssertRelationshipCount( node2, 2 );
						 // no tx.success();
					}
				}
				catch ( Exception e )
				{
					 Console.WriteLine( e.ToString() );
					 Console.Write( e.StackTrace );
					 Thread.interrupted();
					 t1Exception.set( e );
				}
				finally
				{
					 try
					 {
						  AssertPropertyEqual( node1, "key", "old" );
						  AssertPropertyEqual( rel1, "key", "old" );
						  AssertRelationshipCount( node1, 1 );
						  AssertRelationshipCount( node2, 1 );
					 }
					 catch ( Exception e )
					 {
						  t1Exception.compareAndSet( null, e );
					 }
				}
			  });
			  t1.Start();

			  latch1.await();

			  // The transaction started above that runs in t1 has not finished. The old values should still be visible.
			  AssertPropertyEqual( node1, "key", "old" );
			  AssertPropertyEqual( rel1, "key", "old" );
			  AssertRelationshipCount( node1, 1 );
			  AssertRelationshipCount( node2, 1 );

			  latch2.Signal();
			  t1.Join();

			  // The transaction in t1 has finished but not committed. Its changes should still not be visible.
			  AssertPropertyEqual( node1, "key", "old" );
			  AssertPropertyEqual( rel1, "key", "old" );
			  AssertRelationshipCount( node1, 1 );
			  AssertRelationshipCount( node2, 1 );

			  if ( t1Exception.get() != null )
			  {
					throw t1Exception.get();
			  }

			  using ( Transaction tx = GraphDb.beginTx() )
			  {
					foreach ( Relationship rel in node1.Relationships )
					{
						 rel.Delete();
					}
					node1.Delete();
					node2.Delete();
					tx.Success();
			  }
		 }

		 private void AssertPropertyEqual( IPropertyContainer primitive, string key, string value )
		 {
			  using ( Transaction tx = GraphDb.beginTx() )
			  {
					assertEquals( value, primitive.GetProperty( key ) );
			  }
		 }

		 private void AssertRelationshipCount( Node node, int count )
		 {

			  using ( Transaction tx = GraphDb.beginTx() )
			  {
					int actualCount = 0;
					foreach ( Relationship rel in node.Relationships )
					{
						 actualCount++;
					}
					assertEquals( count, actualCount );
			  }
		 }
	}

}