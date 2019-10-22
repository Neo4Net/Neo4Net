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
	using RandomStringUtils = org.apache.commons.lang3.RandomStringUtils;
	using RandomUtils = org.apache.commons.lang3.RandomUtils;
	using Test = org.junit.Test;


	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using NotFoundException = Neo4Net.GraphDb.NotFoundException;
	using IPropertyContainer = Neo4Net.GraphDb.PropertyContainer;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using Iterators = Neo4Net.Helpers.Collections.Iterators;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.NamedThreadFactory.named;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.DoubleLatch.awaitLatch;

	public class NodeProxyTest : IPropertyContainerProxyTest
	{
		 private readonly string _propertyKey = "PROPERTY_KEY";

		 protected internal override long CreatePropertyContainer()
		 {
			  return Db.createNode().Id;
		 }

		 protected internal override IPropertyContainer LookupPropertyContainer( long id )
		 {
			  return Db.getNodeById( id );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowHumaneExceptionsWhenPropertyDoesNotExistOnNode()
		 public virtual void ShouldThrowHumaneExceptionsWhenPropertyDoesNotExistOnNode()
		 {
			  // Given a database with PROPERTY_KEY in it
			  CreateNodeWith( _propertyKey );

			  // When trying to get property from node without it
			  try
			  {
					  using ( Transaction ignored = Db.beginTx() )
					  {
						Node node = Db.createNode();
						node.GetProperty( _propertyKey );
						fail( "Expected exception to have been thrown" );
					  }
			  }
			  // Then
			  catch ( NotFoundException exception )
			  {
					assertThat( exception.Message, containsString( _propertyKey ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createDropNodeLongStringProperty()
		 public virtual void CreateDropNodeLongStringProperty()
		 {
			  Label markerLabel = Label.label( "marker" );
			  string testPropertyKey = "testProperty";
			  string propertyValue = RandomStringUtils.randomAscii( 255 );

			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode( markerLabel );
					node.SetProperty( testPropertyKey, propertyValue );
					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Iterators.single( Db.findNodes( markerLabel ) );
					assertEquals( propertyValue, node.GetProperty( testPropertyKey ) );
					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Iterators.single( Db.findNodes( markerLabel ) );
					node.RemoveProperty( testPropertyKey );
					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Iterators.single( Db.findNodes( markerLabel ) );
					assertFalse( node.HasProperty( testPropertyKey ) );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createDropNodeLongArrayProperty()
		 public virtual void CreateDropNodeLongArrayProperty()
		 {
			  Label markerLabel = Label.label( "marker" );
			  string testPropertyKey = "testProperty";
			  sbyte[] propertyValue = RandomUtils.NextBytes( 1024 );

			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode( markerLabel );
					node.SetProperty( testPropertyKey, propertyValue );
					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Iterators.single( Db.findNodes( markerLabel ) );
					assertArrayEquals( propertyValue, ( sbyte[] ) node.GetProperty( testPropertyKey ) );
					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Iterators.single( Db.findNodes( markerLabel ) );
					node.RemoveProperty( testPropertyKey );
					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Iterators.single( Db.findNodes( markerLabel ) );
					assertFalse( node.HasProperty( testPropertyKey ) );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowHumaneExceptionsWhenPropertyDoesNotExist()
		 public virtual void ShouldThrowHumaneExceptionsWhenPropertyDoesNotExist()
		 {
			  // Given a database without PROPERTY_KEY in it

			  // When
			  try
			  {
					  using ( Transaction ignored = Db.beginTx() )
					  {
						Node node = Db.createNode();
						node.GetProperty( _propertyKey );
					  }
			  }
			  // Then
			  catch ( NotFoundException exception )
			  {
					assertThat( exception.Message, containsString( _propertyKey ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = org.Neo4Net.graphdb.NotFoundException.class) public void deletionOfSameNodeTwiceInOneTransactionShouldNotRollbackIt()
		 public virtual void DeletionOfSameNodeTwiceInOneTransactionShouldNotRollbackIt()
		 {
			  // Given
			  Node node;
			  using ( Transaction tx = Db.beginTx() )
			  {
					node = Db.createNode();
					tx.Success();
			  }

			  // When
			  Exception exceptionThrownBySecondDelete = null;

			  using ( Transaction tx = Db.beginTx() )
			  {
					node.Delete();
					try
					{
						 node.Delete();
					}
					catch ( Exception e )
					{
						 exceptionThrownBySecondDelete = e;
					}
					tx.Success();
			  }

			  // Then
			  assertThat( exceptionThrownBySecondDelete, instanceOf( typeof( NotFoundException ) ) );

			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.getNodeById( node.Id ); // should throw NotFoundException
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = org.Neo4Net.graphdb.NotFoundException.class) public void deletionOfAlreadyDeletedNodeShouldThrow()
		 public virtual void DeletionOfAlreadyDeletedNodeShouldThrow()
		 {
			  // Given
			  Node node;
			  using ( Transaction tx = Db.beginTx() )
			  {
					node = Db.createNode();
					tx.Success();
			  }
			  using ( Transaction tx = Db.beginTx() )
			  {
					node.Delete();
					tx.Success();
			  }

			  // When
			  using ( Transaction tx = Db.beginTx() )
			  {
					node.Delete(); // should throw NotFoundException as this node is already deleted
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getAllPropertiesShouldWorkFineWithConcurrentPropertyModifications() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void getAllPropertiesShouldWorkFineWithConcurrentPropertyModifications()
		 {
			  // Given
			  ExecutorService executor = Cleanup.add( Executors.newFixedThreadPool( 2, named( "Test-executor-thread" ) ) );

			  const int propertiesCount = 100;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long nodeId;
			  long nodeId;
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode();
					nodeId = node.Id;
					for ( int i = 0; i < propertiesCount; i++ )
					{
						 node.SetProperty( "property-" + i, i );
					}
					tx.Success();
			  }

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch start = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent start = new System.Threading.CountdownEvent( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicBoolean writerDone = new java.util.concurrent.atomic.AtomicBoolean();
			  AtomicBoolean writerDone = new AtomicBoolean();

			  ThreadStart writer = () =>
			  {
				try
				{
					 awaitLatch( start );
					 int propertyKey = 0;
					 while ( propertyKey < propertiesCount )
					 {
						  using ( Transaction tx = Db.beginTx() )
						  {
								Node node = Db.getNodeById( nodeId );
								for ( int i = 0; i < 10 && propertyKey < propertiesCount; i++, propertyKey++ )
								{
									 node.setProperty( "property-" + propertyKey, System.Guid.randomUUID().ToString() );
								}
								tx.Success();
						  }
					 }
				}
				finally
				{
					 writerDone.set( true );
				}
			  };
			  ThreadStart reader = () =>
			  {
				using ( Transaction tx = Db.beginTx() )
				{
					 Node node = Db.getNodeById( nodeId );
					 awaitLatch( start );
					 while ( !writerDone.get() )
					 {
						  int size = node.AllProperties.size();
						  assertThat( size, greaterThan( 0 ) );
					 }
					 tx.Success();
				}
			  };

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> readerFuture = executor.submit(reader);
			  Future<object> readerFuture = executor.submit( reader );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> writerFuture = executor.submit(writer);
			  Future<object> writerFuture = executor.submit( writer );

			  start.Signal();

			  // When
			  writerFuture.get();
			  readerFuture.get();

			  // Then
			  using ( Transaction tx = Db.beginTx() )
			  {
					assertEquals( propertiesCount, Db.getNodeById( nodeId ).AllProperties.size() );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToForceTypeChangeOfProperty()
		 public virtual void ShouldBeAbleToForceTypeChangeOfProperty()
		 {
			  // Given
			  Node node;
			  using ( Transaction tx = Db.beginTx() )
			  {
					node = Db.createNode();
					node.SetProperty( "prop", 1337 );
					tx.Success();
			  }

			  // When
			  using ( Transaction tx = Db.beginTx() )
			  {
					node.SetProperty( "prop", 1337.0 );
					tx.Success();
			  }

			  // Then
			  using ( Transaction ignore = Db.beginTx() )
			  {
					assertThat( node.GetProperty( "prop" ), instanceOf( typeof( Double ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOnlyReturnTypeOnce()
		 public virtual void ShouldOnlyReturnTypeOnce()
		 {
			  // Given
			  Node node;
			  using ( Transaction tx = Db.beginTx() )
			  {
					node = Db.createNode();
					node.CreateRelationshipTo( Db.createNode(), RelationshipType.withName("R") );
					node.CreateRelationshipTo( Db.createNode(), RelationshipType.withName("R") );
					node.CreateRelationshipTo( Db.createNode(), RelationshipType.withName("R") );
					tx.Success();
			  }

			  // Then
			  using ( Transaction tx = Db.beginTx() )
			  {
					assertThat( Iterables.asList( node.RelationshipTypes ), equalTo( singletonList( RelationshipType.withName( "R" ) ) ) );
			  }
		 }

		 private void CreateNodeWith( string key )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode();
					node.SetProperty( key, 1 );
					tx.Success();
			  }
		 }
	}

}