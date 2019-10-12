using System;
using System.Collections.Generic;

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
namespace Neo4Net.Kernel.Impl.@event
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using TransactionFailureException = Neo4Net.Graphdb.TransactionFailureException;
	using LabelEntry = Neo4Net.Graphdb.@event.LabelEntry;
	using Neo4Net.Graphdb.@event;
	using TransactionData = Neo4Net.Graphdb.@event.TransactionData;
	using Neo4Net.Graphdb.@event;
	using Exceptions = Neo4Net.Helpers.Exceptions;
	using TestLabels = Neo4Net.Test.TestLabels;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.RelationshipType.withName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.index.IndexManager_Fields.PROVIDER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.count;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.DummyIndexExtensionFactory.IDENTIFIER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.Neo4jMatchers.hasProperty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.Neo4jMatchers.inTx;

	public class TestTransactionEvents
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.DatabaseRule dbRule = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public readonly DatabaseRule DbRule = new ImpermanentDatabaseRule();
		 private static readonly TimeUnit _awaitIndexUnit = TimeUnit.SECONDS;
		 private const int AWAIT_INDEX_DURATION = 60;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRegisterUnregisterHandlers()
		 public virtual void TestRegisterUnregisterHandlers()
		 {
			  object value1 = 10;
			  object value2 = 3.5D;
			  DummyTransactionEventHandler<int> handler1 = new DummyTransactionEventHandler<int>( ( int? ) value1 );
			  DummyTransactionEventHandler<double> handler2 = new DummyTransactionEventHandler<double>( ( double? ) value2 );

			  GraphDatabaseService db = DbRule.GraphDatabaseAPI;
			  try
			  {
					Db.unregisterTransactionEventHandler( handler1 );
					fail( "Shouldn't be able to do unregister on a unregistered handler" );
			  }
			  catch ( System.InvalidOperationException )
			  { // Good
			  }

			  assertSame( handler1, Db.registerTransactionEventHandler( handler1 ) );
			  assertSame( handler1, Db.registerTransactionEventHandler( handler1 ) );
			  assertSame( handler1, Db.unregisterTransactionEventHandler( handler1 ) );

			  try
			  {
					Db.unregisterTransactionEventHandler( handler1 );
					fail( "Shouldn't be able to do unregister on a unregistered handler" );
			  }
			  catch ( System.InvalidOperationException )
			  { // Good
			  }

			  assertSame( handler1, Db.registerTransactionEventHandler( handler1 ) );
			  assertSame( handler2, Db.registerTransactionEventHandler( handler2 ) );
			  assertSame( handler1, Db.unregisterTransactionEventHandler( handler1 ) );
			  assertSame( handler2, Db.unregisterTransactionEventHandler( handler2 ) );

			  Db.registerTransactionEventHandler( handler1 );
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.createNode().delete();
					tx.Success();
			  }

			  assertNotNull( handler1.BeforeCommitConflict );
			  assertNotNull( handler1.AfterCommitConflict );
			  assertNull( handler1.AfterRollbackConflict );
			  assertEquals( value1, handler1.ReceivedState );
			  assertNotNull( handler1.ReceivedTransactionData );
			  Db.unregisterTransactionEventHandler( handler1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureHandlersCantBeRegisteredTwice()
		 public virtual void MakeSureHandlersCantBeRegisteredTwice()
		 {
			  DummyTransactionEventHandler<object> handler = new DummyTransactionEventHandler<object>( null );
			  GraphDatabaseService db = DbRule.GraphDatabaseAPI;
			  Db.registerTransactionEventHandler( handler );
			  Db.registerTransactionEventHandler( handler );
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.createNode().delete();
					tx.Success();
			  }
			  assertEquals( Convert.ToInt32( 0 ), handler.BeforeCommitConflict );
			  assertEquals( Convert.ToInt32( 1 ), handler.AfterCommitConflict );
			  assertNull( handler.AfterRollbackConflict );

			  Db.unregisterTransactionEventHandler( handler );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetCorrectTransactionDataUponCommit()
		 public virtual void ShouldGetCorrectTransactionDataUponCommit()
		 {
			  // Create new data, nothing modified, just added/created
			  ExpectedTransactionData expectedData = new ExpectedTransactionData();
			  VerifyingTransactionEventHandler handler = new VerifyingTransactionEventHandler( expectedData );
			  GraphDatabaseService db = DbRule.GraphDatabaseAPI;
			  Db.registerTransactionEventHandler( handler );
			  Node node1;
			  Node node2;
			  Node node3;
			  Relationship rel1;
			  Relationship rel2;
			  try
			  {
					using ( Transaction tx = Db.beginTx() )
					{
						 node1 = Db.createNode();
						 expectedData.ExpectedCreatedNodes.Add( node1 );

						 node2 = Db.createNode();
						 expectedData.ExpectedCreatedNodes.Add( node2 );

						 rel1 = node1.CreateRelationshipTo( node2, RelTypes.Txevent );
						 expectedData.ExpectedCreatedRelationships.Add( rel1 );

						 node1.SetProperty( "name", "Mattias" );
						 expectedData.AssignedProperty( node1, "name", "Mattias", null );

						 node1.SetProperty( "last name", "Persson" );
						 expectedData.AssignedProperty( node1, "last name", "Persson", null );

						 node1.SetProperty( "counter", 10 );
						 expectedData.AssignedProperty( node1, "counter", 10, null );

						 rel1.SetProperty( "description", "A description" );
						 expectedData.AssignedProperty( rel1, "description", "A description", null );

						 rel1.SetProperty( "number", 4.5D );
						 expectedData.AssignedProperty( rel1, "number", 4.5D, null );

						 node3 = Db.createNode();
						 expectedData.ExpectedCreatedNodes.Add( node3 );
						 rel2 = node3.CreateRelationshipTo( node2, RelTypes.Txevent );
						 expectedData.ExpectedCreatedRelationships.Add( rel2 );

						 node3.SetProperty( "name", "Node 3" );
						 expectedData.AssignedProperty( node3, "name", "Node 3", null );
						 tx.Success();
					}

					assertTrue( "Should have been invoked", handler.HasBeenCalled() );
					Exception failure = handler.Failure();
					if ( failure != null )
					{
						 throw new Exception( failure );
					}
			  }
			  finally
			  {
					Db.unregisterTransactionEventHandler( handler );
			  }

			  // Use the above data and modify it, change properties, delete stuff
			  expectedData = new ExpectedTransactionData();
			  handler = new VerifyingTransactionEventHandler( expectedData );
			  Db.registerTransactionEventHandler( handler );
			  try
			  {
					using ( Transaction tx = Db.beginTx() )
					{
						 Node newNode = Db.createNode();
						 expectedData.ExpectedCreatedNodes.Add( newNode );

						 Node tempNode = Db.createNode();
						 Relationship tempRel = tempNode.CreateRelationshipTo( node1, RelTypes.Txevent );
						 tempNode.SetProperty( "something", "Some value" );
						 tempRel.SetProperty( "someproperty", 101010 );
						 tempNode.RemoveProperty( "nothing" );

						 node3.SetProperty( "test", "hello" );
						 node3.SetProperty( "name", "No name" );
						 node3.Delete();
						 expectedData.ExpectedDeletedNodes.Add( node3 );
						 expectedData.RemovedProperty( node3, "name", "Node 3" );

						 node1.SetProperty( "new name", "A name" );
						 node1.SetProperty( "new name", "A better name" );
						 expectedData.AssignedProperty( node1, "new name", "A better name", null );
						 node1.SetProperty( "name", "Nothing" );
						 node1.SetProperty( "name", "Mattias Persson" );
						 expectedData.AssignedProperty( node1, "name", "Mattias Persson", "Mattias" );
						 node1.RemoveProperty( "counter" );
						 expectedData.RemovedProperty( node1, "counter", 10 );
						 node1.RemoveProperty( "last name" );
						 node1.SetProperty( "last name", "Hi" );
						 expectedData.AssignedProperty( node1, "last name", "Hi", "Persson" );

						 rel2.Delete();
						 expectedData.ExpectedDeletedRelationships.Add( rel2 );

						 rel1.RemoveProperty( "number" );
						 expectedData.RemovedProperty( rel1, "number", 4.5D );
						 rel1.SetProperty( "description", "Ignored" );
						 rel1.SetProperty( "description", "New" );
						 expectedData.AssignedProperty( rel1, "description", "New", "A description" );

						 tempRel.Delete();
						 tempNode.Delete();
						 tx.Success();
					}

					assertTrue( "Should have been invoked", handler.HasBeenCalled() );
					Exception failure = handler.Failure();
					if ( failure != null )
					{
						 throw new Exception( failure );
					}
			  }
			  finally
			  {
					Db.unregisterTransactionEventHandler( handler );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureBeforeAfterAreCalledCorrectly()
		 public virtual void MakeSureBeforeAfterAreCalledCorrectly()
		 {
			  IList<TransactionEventHandler<object>> handlers = new List<TransactionEventHandler<object>>();
			  handlers.Add( new FailingEventHandler<>( new DummyTransactionEventHandler<>( null ), false ) );
			  handlers.Add( new FailingEventHandler<>( new DummyTransactionEventHandler<>( null ), false ) );
			  handlers.Add( new FailingEventHandler<>( new DummyTransactionEventHandler<>( null ), true ) );
			  handlers.Add( new FailingEventHandler<>( new DummyTransactionEventHandler<>( null ), false ) );
			  GraphDatabaseService db = DbRule.GraphDatabaseAPI;
			  foreach ( TransactionEventHandler<object> handler in handlers )
			  {
					Db.registerTransactionEventHandler( handler );
			  }

			  try
			  {
					Transaction tx = Db.beginTx();
					try
					{
						 Db.createNode().delete();
						 tx.Success();
						 tx.Close();
						 fail( "Should fail commit" );
					}
					catch ( TransactionFailureException )
					{ // OK
					}
					VerifyHandlerCalls( handlers, false );

					Db.unregisterTransactionEventHandler( handlers.RemoveAt( 2 ) );
					foreach ( TransactionEventHandler<object> handler in handlers )
					{
						 ( ( DummyTransactionEventHandler<object> )( ( FailingEventHandler<object> )handler ).source ).reset();
					}
					using ( Transaction transaction = Db.beginTx() )
					{
						 Db.createNode().delete();
						 transaction.Success();
					}
					VerifyHandlerCalls( handlers, true );
			  }
			  finally
			  {
					foreach ( TransactionEventHandler<object> handler in handlers )
					{
						 Db.unregisterTransactionEventHandler( handler );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToAccessExceptionThrownInEventHook()
		 public virtual void ShouldBeAbleToAccessExceptionThrownInEventHook()
		 {
//JAVA TO C# CONVERTER TODO TASK: Local classes are not converted by Java to C# Converter:
//			  class MyFancyException extends Exception
	//		  {
	//
	//		  }

			  ExceptionThrowingEventHandler handler = new ExceptionThrowingEventHandler( new MyFancyException(), null, null );
			  GraphDatabaseService db = DbRule.GraphDatabaseAPI;
			  Db.registerTransactionEventHandler( handler );

			  try
			  {
					Transaction tx = Db.beginTx();
					try
					{
						 Db.createNode().delete();
						 tx.Success();
						 tx.Close();
						 fail( "Should fail commit" );
					}
					catch ( TransactionFailureException e )
					{
						 Exception currentEx = e;
						 do
						 {
							  currentEx = currentEx.InnerException;
							  if ( currentEx is MyFancyException )
							  {
									return;
							  }
						 } while ( currentEx.InnerException != null );
						 fail( "Expected to find the exception thrown in the event hook as the cause of transaction failure." );
					}
			  }
			  finally
			  {
					Db.unregisterTransactionEventHandler( handler );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void deleteNodeRelTriggerPropertyRemoveEvents()
		 public virtual void DeleteNodeRelTriggerPropertyRemoveEvents()
		 {
			  GraphDatabaseService db = DbRule.GraphDatabaseAPI;
			  Node node1;
			  Node node2;
			  Relationship rel;
			  using ( Transaction tx = Db.beginTx() )
			  {
					node1 = Db.createNode();
					node2 = Db.createNode();
					rel = node1.CreateRelationshipTo( node2, RelTypes.Txevent );
					node1.SetProperty( "test1", "stringvalue" );
					node1.SetProperty( "test2", 1L );
					rel.SetProperty( "test1", "stringvalue" );
					rel.SetProperty( "test2", 1L );
					rel.SetProperty( "test3", new int[] { 1, 2, 3 } );
					tx.Success();
			  }
			  MyTxEventHandler handler = new MyTxEventHandler();
			  Db.registerTransactionEventHandler( handler );
			  using ( Transaction tx = Db.beginTx() )
			  {
					rel.Delete();
					node1.Delete();
					node2.Delete();
					tx.Success();
			  }
			  assertEquals( "stringvalue", handler.NodeProps["test1"] );
			  assertEquals( "stringvalue", handler.RelProps["test1"] );
			  assertEquals( 1L, handler.NodeProps["test2"] );
			  assertEquals( 1L, handler.RelProps["test2"] );
			  int[] intArray = ( int[] ) handler.RelProps["test3"];
			  assertEquals( 3, intArray.Length );
			  assertEquals( 1, intArray[0] );
			  assertEquals( 2, intArray[1] );
			  assertEquals( 3, intArray[2] );
		 }

		 private class MyTxEventHandler : TransactionEventHandler<object>
		 {
			  internal IDictionary<string, object> NodeProps = new Dictionary<string, object>();
			  internal IDictionary<string, object> RelProps = new Dictionary<string, object>();

			  public override void AfterCommit( TransactionData data, object state )
			  {
					foreach ( PropertyEntry<Node> entry in data.RemovedNodeProperties() )
					{
						 string key = entry.Key();
						 object value = entry.PreviouslyCommitedValue();
						 NodeProps[key] = value;
					}
					foreach ( PropertyEntry<Relationship> entry in data.RemovedRelationshipProperties() )
					{
						 RelProps[entry.Key()] = entry.PreviouslyCommitedValue();
					}
			  }

			  public override void AfterRollback( TransactionData data, object state )
			  {
			  }

			  public override object BeforeCommit( TransactionData data )
			  {
					return null;
			  }
		 }

		 private void VerifyHandlerCalls( IList<TransactionEventHandler<object>> handlers, bool txSuccess )
		 {
			  foreach ( TransactionEventHandler<object> handler in handlers )
			  {
					DummyTransactionEventHandler<object> realHandler = ( DummyTransactionEventHandler<object> )( ( FailingEventHandler<object> ) handler ).source;
					if ( txSuccess )
					{
						 assertEquals( Convert.ToInt32( 0 ), realHandler.BeforeCommitConflict );
						 assertEquals( Convert.ToInt32( 1 ), realHandler.AfterCommitConflict );
					}
					else
					{
						 if ( realHandler.Counter > 0 )
						 {
							  assertEquals( Convert.ToInt32( 0 ), realHandler.BeforeCommitConflict );
							  assertEquals( Convert.ToInt32( 1 ), realHandler.AfterRollbackConflict );
						 }
					}
			  }
		 }

		 private enum RelTypes
		 {
			  Txevent
		 }

		 private class FailingEventHandler<T> : TransactionEventHandler<T>
		 {
			  internal readonly TransactionEventHandler<T> Source;
			  internal readonly bool WillFail;

			  internal FailingEventHandler( TransactionEventHandler<T> source, bool willFail )
			  {
					this.Source = source;
					this.WillFail = willFail;
			  }

			  public override void AfterCommit( TransactionData data, T state )
			  {
					Source.afterCommit( data, state );
			  }

			  public override void AfterRollback( TransactionData data, T state )
			  {
					Source.afterRollback( data, state );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public T beforeCommit(org.neo4j.graphdb.event.TransactionData data) throws Exception
			  public override T BeforeCommit( TransactionData data )
			  {
					try
					{
						 return Source.beforeCommit( data );
					}
					finally
					{
						 if ( WillFail )
						 {
							  throw new Exception( "Just failing commit, that's all" );
						 }
					}
			  }
		 }

		 private class ExceptionThrowingEventHandler : TransactionEventHandler<object>
		 {
			  internal readonly Exception BeforeCommitException;
			  internal readonly Exception AfterCommitException;
			  internal readonly Exception AfterRollbackException;

			  internal ExceptionThrowingEventHandler( Exception exceptionForAll ) : this( exceptionForAll, exceptionForAll, exceptionForAll )
			  {
			  }

			  internal ExceptionThrowingEventHandler( Exception beforeCommitException, Exception afterCommitException, Exception afterRollbackException )
			  {
					this.BeforeCommitException = beforeCommitException;
					this.AfterCommitException = afterCommitException;
					this.AfterRollbackException = afterRollbackException;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Object beforeCommit(org.neo4j.graphdb.event.TransactionData data) throws Exception
			  public override object BeforeCommit( TransactionData data )
			  {
					if ( BeforeCommitException != null )
					{
						 throw BeforeCommitException;
					}
					return null;
			  }

			  public override void AfterCommit( TransactionData data, object state )
			  {
					if ( AfterCommitException != null )
					{
						 throw new Exception( AfterCommitException );
					}
			  }

			  public override void AfterRollback( TransactionData data, object state )
			  {
					if ( AfterRollbackException != null )
					{
						 throw new Exception( AfterRollbackException );
					}
			  }
		 }

		 private class DummyTransactionEventHandler<T> : TransactionEventHandler<T>
		 {
			  internal readonly T Object;
			  internal TransactionData ReceivedTransactionData;
			  internal T ReceivedState;
			  internal int Counter;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal int? BeforeCommitConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal int? AfterCommitConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal int? AfterRollbackConflict;

			  internal DummyTransactionEventHandler( T @object )
			  {
					this.Object = @object;
			  }

			  public override void AfterCommit( TransactionData data, T state )
			  {
					assertNotNull( data );
					this.ReceivedState = state;
					this.AfterCommitConflict = Counter++;
			  }

			  public override void AfterRollback( TransactionData data, T state )
			  {
					assertNotNull( data );
					this.ReceivedState = state;
					this.AfterRollbackConflict = Counter++;
			  }

			  public override T BeforeCommit( TransactionData data )
			  {
					assertNotNull( data );
					this.ReceivedTransactionData = data;
					this.BeforeCommitConflict = Counter++;
					if ( this.BeforeCommitConflict == 2 )
					{
						 Console.WriteLine( ( new Exception( "blabla" ) ).ToString() );
						 Console.Write( ( new Exception( "blabla" ) ).StackTrace );
					}
					return Object;
			  }

			  internal virtual void Reset()
			  {
					ReceivedTransactionData = null;
					ReceivedState = default( T );
					Counter = 0;
					BeforeCommitConflict = null;
					AfterCommitConflict = null;
					AfterRollbackConflict = null;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureHandlerIsntCalledWhenTxRolledBack()
		 public virtual void MakeSureHandlerIsntCalledWhenTxRolledBack()
		 {
			  DummyTransactionEventHandler<int> handler = new DummyTransactionEventHandler<int>( 10 );
			  GraphDatabaseService db = DbRule.GraphDatabaseAPI;
			  Db.registerTransactionEventHandler( handler );
			  try
			  {
					using ( Transaction ignore = Db.beginTx() )
					{
						 Db.createNode().delete();
					}
					assertNull( handler.BeforeCommitConflict );
					assertNull( handler.AfterCommitConflict );
					assertNull( handler.AfterRollbackConflict );
			  }
			  finally
			  {
					Db.unregisterTransactionEventHandler( handler );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void modifiedPropertyCanByFurtherModifiedInBeforeCommit()
		 public virtual void ModifiedPropertyCanByFurtherModifiedInBeforeCommit()
		 {
			  // Given
			  // -- create node and set property on it in one transaction
			  GraphDatabaseService db = DbRule.GraphDatabaseAPI;
			  const string key = "key";
			  const object value1 = "the old value";
			  const object value2 = "the new value";
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.Node node;
			  Node node;
			  using ( Transaction tx = Db.beginTx() )
			  {
					node = Db.createNode();
					node.SetProperty( key, "initial value" );
					tx.Success();
			  }
			  // -- register a tx handler which will override a property
			  TransactionEventHandler<Void> handler = new TransactionEventHandler_AdapterAnonymousInnerClass( this, key, value2, node );
			  Db.registerTransactionEventHandler( handler );

			  using ( Transaction tx = Db.beginTx() )
			  {
					// When
					node.SetProperty( key, value1 );
					tx.Success();
			  }
			  // Then
			  assertThat( node, inTx( db, hasProperty( key ).withValue( value2 ) ) );
			  Db.unregisterTransactionEventHandler( handler );
		 }

		 private class TransactionEventHandler_AdapterAnonymousInnerClass : Neo4Net.Graphdb.@event.TransactionEventHandler_Adapter<Void>
		 {
			 private readonly TestTransactionEvents _outerInstance;

			 private string _key;
			 private object _value2;
			 private Node _node;

			 public TransactionEventHandler_AdapterAnonymousInnerClass( TestTransactionEvents outerInstance, string key, object value2, Node node )
			 {
				 this.outerInstance = outerInstance;
				 this._key = key;
				 this._value2 = value2;
				 this._node = node;
			 }

			 public override Void beforeCommit( TransactionData data )
			 {
				  Node modifiedNode = data.AssignedNodeProperties().GetEnumerator().next().entity();
				  assertEquals( _node, modifiedNode );
				  modifiedNode.SetProperty( _key, _value2 );
				  return null;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nodeCanBecomeSchemaIndexableInBeforeCommitByAddingProperty()
		 public virtual void NodeCanBecomeSchemaIndexableInBeforeCommitByAddingProperty()
		 {
			  // Given we have a schema index...
			  GraphDatabaseService db = DbRule.GraphDatabaseAPI;
			  Label label = label( "Label" );
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().indexFor(label).on("indexed").create();
					tx.Success();
			  }

			  // ... and a transaction event handler that likes to add the indexed property on nodes
			  Db.registerTransactionEventHandler( new TransactionEventHandler_AdapterAnonymousInnerClass2( this ) );

			  // When we create a node with the right label, but not the right property...
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(AWAIT_INDEX_DURATION, _awaitIndexUnit);
					Node node = Db.createNode( label );
					node.SetProperty( "random", 42 );
					tx.Success();
			  }

			  // Then we should be able to look it up through the index.
			  using ( Transaction ignore = Db.beginTx() )
			  {
					Node node = Db.findNode( label, "indexed", "value" );
					assertThat( node.GetProperty( "random" ), @is( 42 ) );
			  }
		 }

		 private class TransactionEventHandler_AdapterAnonymousInnerClass2 : Neo4Net.Graphdb.@event.TransactionEventHandler_Adapter<object>
		 {
			 private readonly TestTransactionEvents _outerInstance;

			 public TransactionEventHandler_AdapterAnonymousInnerClass2( TestTransactionEvents outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override object beforeCommit( TransactionData data )
			 {
				  IEnumerator<Node> nodes = data.CreatedNodes().GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
				  if ( nodes.hasNext() )
				  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						Node node = nodes.next();
						node.SetProperty( "indexed", "value" );
				  }
				  return null;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nodeCanBecomeSchemaIndexableInBeforeCommitByAddingLabel()
		 public virtual void NodeCanBecomeSchemaIndexableInBeforeCommitByAddingLabel()
		 {
			  // Given we have a schema index...
			  GraphDatabaseService db = DbRule.GraphDatabaseAPI;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.Label label = label("Label");
			  Label label = label( "Label" );
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().indexFor(label).on("indexed").create();
					tx.Success();
			  }

			  // ... and a transaction event handler that likes to add the indexed property on nodes
			  Db.registerTransactionEventHandler( new TransactionEventHandler_AdapterAnonymousInnerClass3( this, label ) );

			  // When we create a node with the right property, but not the right label...
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(AWAIT_INDEX_DURATION, _awaitIndexUnit);
					Node node = Db.createNode();
					node.SetProperty( "indexed", "value" );
					node.SetProperty( "random", 42 );
					tx.Success();
			  }

			  // Then we should be able to look it up through the index.
			  using ( Transaction ignore = Db.beginTx() )
			  {
					Node node = Db.findNode( label, "indexed", "value" );
					assertThat( node.GetProperty( "random" ), @is( 42 ) );
			  }
		 }

		 private class TransactionEventHandler_AdapterAnonymousInnerClass3 : Neo4Net.Graphdb.@event.TransactionEventHandler_Adapter<object>
		 {
			 private readonly TestTransactionEvents _outerInstance;

			 private Label _label;

			 public TransactionEventHandler_AdapterAnonymousInnerClass3( TestTransactionEvents outerInstance, Label label )
			 {
				 this.outerInstance = outerInstance;
				 this._label = label;
			 }

			 public override object beforeCommit( TransactionData data )
			 {
				  IEnumerator<Node> nodes = data.CreatedNodes().GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
				  if ( nodes.hasNext() )
				  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						Node node = nodes.next();
						node.AddLabel( _label );
				  }
				  return null;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAccessAssignedLabels()
		 public virtual void ShouldAccessAssignedLabels()
		 {
			  // given
			  GraphDatabaseService db = DbRule.GraphDatabaseAPI;

			  ChangedLabels labels = ( ChangedLabels ) Db.registerTransactionEventHandler( new ChangedLabels() );
			  try
			  {
					// when
					using ( Transaction tx = Db.beginTx() )
					{
						 Node node1 = Db.createNode();
						 Node node2 = Db.createNode();
						 Node node3 = Db.createNode();

						 labels.Add( node1, "Foo" );
						 labels.Add( node2, "Bar" );
						 labels.Add( node3, "Baz" );
						 labels.Add( node3, "Bar" );

						 labels.Activate();
						 tx.Success();
					}
					// then
					assertTrue( labels.Empty );
			  }
			  finally
			  {
					Db.unregisterTransactionEventHandler( labels );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAccessRemovedLabels()
		 public virtual void ShouldAccessRemovedLabels()
		 {
			  // given
			  GraphDatabaseService db = DbRule.GraphDatabaseAPI;

			  ChangedLabels labels = ( ChangedLabels ) Db.registerTransactionEventHandler( new ChangedLabels() );
			  try
			  {
					Node node1;
					Node node2;
					Node node3;
					using ( Transaction tx = Db.beginTx() )
					{
						 node1 = Db.createNode();
						 node2 = Db.createNode();
						 node3 = Db.createNode();

						 labels.Add( node1, "Foo" );
						 labels.Add( node2, "Bar" );
						 labels.Add( node3, "Baz" );
						 labels.Add( node3, "Bar" );

						 tx.Success();
					}
					labels.Clear();

					// when
					using ( Transaction tx = Db.beginTx() )
					{
						 labels.Remove( node1, "Foo" );
						 labels.Remove( node2, "Bar" );
						 labels.Remove( node3, "Baz" );
						 labels.Remove( node3, "Bar" );

						 labels.Activate();
						 tx.Success();
					}
					// then
					assertTrue( labels.Empty );
			  }
			  finally
			  {
					Db.unregisterTransactionEventHandler( labels );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAccessRelationshipDataInAfterCommit()
		 public virtual void ShouldAccessRelationshipDataInAfterCommit()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.GraphDatabaseService db = dbRule.getGraphDatabaseAPI();
			  GraphDatabaseService db = DbRule.GraphDatabaseAPI;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicInteger accessCount = new java.util.concurrent.atomic.AtomicInteger();
			  AtomicInteger accessCount = new AtomicInteger();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Map<long,RelationshipData> expectedRelationshipData = new java.util.HashMap<>();
			  IDictionary<long, RelationshipData> expectedRelationshipData = new Dictionary<long, RelationshipData>();
			  TransactionEventHandler<Void> handler = new TransactionEventHandler_AdapterAnonymousInnerClass4( this, db, accessCount, expectedRelationshipData );
			  Db.registerTransactionEventHandler( handler );

			  // WHEN
			  try
			  {
					Relationship relationship;
					using ( Transaction tx = Db.beginTx() )
					{
						 relationship = Db.createNode().createRelationshipTo(Db.createNode(), MyRelTypes.TEST);
						 expectedRelationshipData[relationship.Id] = new RelationshipData( relationship );
						 tx.Success();
					}
					// THEN
					assertEquals( 1, accessCount.get() );

					// and WHEN
					using ( Transaction tx = Db.beginTx() )
					{
						 relationship.SetProperty( "name", "Smith" );
						 Relationship otherRelationship = Db.createNode().createRelationshipTo(Db.createNode(), MyRelTypes.TEST2);
						 expectedRelationshipData[otherRelationship.Id] = new RelationshipData( otherRelationship );
						 tx.Success();
					}
					// THEN
					assertEquals( 2, accessCount.get() );

					// and WHEN
					using ( Transaction tx = Db.beginTx() )
					{
						 relationship.Delete();
						 tx.Success();
					}
					// THEN
					assertEquals( 1, accessCount.get() );
			  }
			  finally
			  {
					Db.unregisterTransactionEventHandler( handler );
			  }
		 }

		 private class TransactionEventHandler_AdapterAnonymousInnerClass4 : Neo4Net.Graphdb.@event.TransactionEventHandler_Adapter<Void>
		 {
			 private readonly TestTransactionEvents _outerInstance;

			 private GraphDatabaseService _db;
			 private AtomicInteger _accessCount;
			 private IDictionary<long, RelationshipData> _expectedRelationshipData;

			 public TransactionEventHandler_AdapterAnonymousInnerClass4( TestTransactionEvents outerInstance, GraphDatabaseService db, AtomicInteger accessCount, IDictionary<long, RelationshipData> expectedRelationshipData )
			 {
				 this.outerInstance = outerInstance;
				 this._db = db;
				 this._accessCount = accessCount;
				 this._expectedRelationshipData = expectedRelationshipData;
			 }

			 public override void afterCommit( TransactionData data, Void state )
			 {
				  _accessCount.set( 0 );
				  using ( Transaction tx = _db.beginTx() )
				  {
						foreach ( Relationship relationship in data.CreatedRelationships() )
						{
							 accessData( relationship );
						}
						foreach ( PropertyEntry<Relationship> change in data.AssignedRelationshipProperties() )
						{
							 accessData( change.Entity() );
						}
						foreach ( PropertyEntry<Relationship> change in data.RemovedRelationshipProperties() )
						{
							 accessData( change.Entity() );
						}
						tx.Success();
				  }
			 }

			 private void accessData( Relationship relationship )
			 {
				  _accessCount.incrementAndGet();
				  RelationshipData expectancy = _expectedRelationshipData[relationship.Id];
				  assertNotNull( expectancy );
				  assertEquals( expectancy.StartNode, relationship.StartNode );
				  assertEquals( expectancy.Type, relationship.Type.name() );
				  assertEquals( expectancy.EndNode, relationship.EndNode );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProvideTheCorrectRelationshipData()
		 public virtual void ShouldProvideTheCorrectRelationshipData()
		 {
			  GraphDatabaseService db = DbRule.GraphDatabaseAPI;

			  // create a rel type so the next type id is non zero
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.createNode().createRelationshipTo(Db.createNode(), withName("TYPE"));
			  }

			  RelationshipType livesIn = withName( "LIVES_IN" );
			  long relId;

			  using ( Transaction tx = Db.beginTx() )
			  {
					Node person = Db.createNode( label( "Person" ) );

					Node city = Db.createNode( label( "City" ) );

					Relationship rel = person.CreateRelationshipTo( city, livesIn );
					rel.SetProperty( "since", 2009 );
					relId = rel.Id;
					tx.Success();
			  }

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Set<String> changedRelationships = new java.util.HashSet<>();
			  ISet<string> changedRelationships = new HashSet<string>();

			  Db.registerTransactionEventHandler( new TransactionEventHandler_AdapterAnonymousInnerClass5( this, changedRelationships ) );

			  using ( Transaction tx = Db.beginTx() )
			  {
					Relationship rel = Db.getRelationshipById( relId );
					rel.SetProperty( "since", 2010 );
					tx.Success();
			  }

			  assertEquals( 1, changedRelationships.Count );
			  assertTrue( livesIn + " not in " + changedRelationships.ToString(), changedRelationships.Contains(livesIn.Name()) );
		 }

		 private class TransactionEventHandler_AdapterAnonymousInnerClass5 : Neo4Net.Graphdb.@event.TransactionEventHandler_Adapter<Void>
		 {
			 private readonly TestTransactionEvents _outerInstance;

			 private ISet<string> _changedRelationships;

			 public TransactionEventHandler_AdapterAnonymousInnerClass5( TestTransactionEvents outerInstance, ISet<string> changedRelationships )
			 {
				 this.outerInstance = outerInstance;
				 this._changedRelationships = changedRelationships;
			 }

			 public override Void beforeCommit( TransactionData data )
			 {
				  foreach ( PropertyEntry<Relationship> entry in data.AssignedRelationshipProperties() )
				  {
						_changedRelationships.Add( entry.Entity().Type.name() );
				  }

				  return null;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFireEventForReadOnlyTransaction()
		 public virtual void ShouldNotFireEventForReadOnlyTransaction()
		 {
			  // GIVEN
			  Node root = CreateTree( 3, 3 );
			  DbRule.GraphDatabaseAPI.registerTransactionEventHandler( new ExceptionThrowingEventHandler( new Exception( "Just failing" ) ) );

			  // WHEN
			  using ( Transaction tx = DbRule.beginTx() )
			  {
					count( DbRule.GraphDatabaseAPI.traversalDescription().traverse(root) );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFireEventForNonDataTransactions()
		 public virtual void ShouldNotFireEventForNonDataTransactions()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicInteger counter = new java.util.concurrent.atomic.AtomicInteger();
			  AtomicInteger counter = new AtomicInteger();
			  DbRule.GraphDatabaseAPI.registerTransactionEventHandler( new TransactionEventHandler_AdapterAnonymousInnerClass6( this, counter ) );
			  Label label = label( "Label" );
			  string key = "key";
			  assertEquals( 0, counter.get() );

			  // WHEN creating a label token
			  using ( Transaction tx = DbRule.beginTx() )
			  {
					DbRule.createNode( label );
					tx.Success();
			  }
			  assertEquals( 1, counter.get() );
			  // ... a property key token
			  using ( Transaction tx = DbRule.beginTx() )
			  {
					DbRule.createNode().setProperty(key, "value");
					tx.Success();
			  }
			  assertEquals( 2, counter.get() );
			  // ... and a relationship type
			  using ( Transaction tx = DbRule.beginTx() )
			  {
					DbRule.createNode().createRelationshipTo(DbRule.createNode(), withName("A_TYPE"));
					tx.Success();
			  }
			  assertEquals( 3, counter.get() );
			  // ... also when creating an index
			  using ( Transaction tx = DbRule.beginTx() )
			  {
					DbRule.schema().indexFor(label).on(key).create();
					tx.Success();
			  }
			  // ... or a constraint
			  using ( Transaction tx = DbRule.beginTx() )
			  {
					DbRule.schema().constraintFor(label).assertPropertyIsUnique("otherkey").create();
					tx.Success();
			  }
			  // ... or even an explicit index
			  using ( Transaction tx = DbRule.beginTx() )
			  {
					DbRule.index().forNodes("some index", stringMap(PROVIDER, IDENTIFIER));
					tx.Success();
			  }

			  // THEN only three transaction events (all including graph data) should've been fired
			  assertEquals( 3, counter.get() );
		 }

		 private class TransactionEventHandler_AdapterAnonymousInnerClass6 : Neo4Net.Graphdb.@event.TransactionEventHandler_Adapter<Void>
		 {
			 private readonly TestTransactionEvents _outerInstance;

			 private AtomicInteger _counter;

			 public TransactionEventHandler_AdapterAnonymousInnerClass6( TestTransactionEvents outerInstance, AtomicInteger counter )
			 {
				 this.outerInstance = outerInstance;
				 this._counter = counter;
			 }

			 public override Void beforeCommit( TransactionData data )
			 {
				  assertTrue( "Expected only transactions that had nodes or relationships created", data.CreatedNodes().GetEnumerator().hasNext() || data.CreatedRelationships().GetEnumerator().hasNext() );
				  _counter.incrementAndGet();
				  return null;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToTouchDataOutsideTxDataInAfterCommit()
		 public virtual void ShouldBeAbleToTouchDataOutsideTxDataInAfterCommit()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.Node node = createNode("one", "Two", "three", "Four");
			  Node node = CreateNode( "one", "Two", "three", "Four" );
			  DbRule.GraphDatabaseAPI.registerTransactionEventHandler( new TransactionEventHandler_AdapterAnonymousInnerClass7( this, node ) );

			  using ( Transaction tx = DbRule.beginTx() )
			  {
					// WHEN/THEN
					DbRule.createNode();
					node.SetProperty( "five", "Six" );
					tx.Success();
			  }
		 }

		 private class TransactionEventHandler_AdapterAnonymousInnerClass7 : Neo4Net.Graphdb.@event.TransactionEventHandler_Adapter<object>
		 {
			 private readonly TestTransactionEvents _outerInstance;

			 private Node _node;

			 public TransactionEventHandler_AdapterAnonymousInnerClass7( TestTransactionEvents outerInstance, Node node )
			 {
				 this.outerInstance = outerInstance;
				 this._node = node;
			 }

			 public override void afterCommit( TransactionData data, object nothing )
			 {
				  using ( Transaction tx = _outerInstance.dbRule.beginTx() )
				  {
						foreach ( string key in _node.PropertyKeys )
						{ // Just to see if one can reach them
							 _node.getProperty( key );
						}
						tx.Success();
				  }
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowToStringOnCreatedRelationshipInAfterCommit()
		 public virtual void ShouldAllowToStringOnCreatedRelationshipInAfterCommit()
		 {
			  // GIVEN
			  Relationship relationship;
			  Node startNode;
			  Node endNode;
			  RelationshipType type = MyRelTypes.TEST;
			  using ( Transaction tx = DbRule.beginTx() )
			  {
					startNode = DbRule.createNode();
					endNode = DbRule.createNode();
					relationship = startNode.CreateRelationshipTo( endNode, type );
					tx.Success();
			  }

			  // WHEN
			  AtomicReference<string> deletedToString = new AtomicReference<string>();
			  DbRule.registerTransactionEventHandler( new TransactionEventHandler_AdapterAnonymousInnerClass8( this, relationship, deletedToString ) );
			  using ( Transaction tx = DbRule.beginTx() )
			  {
					relationship.Delete();
					tx.Success();
			  }

			  // THEN
			  assertNotNull( deletedToString.get() );
			  assertThat( deletedToString.get(), containsString(type.Name()) );
			  assertThat( deletedToString.get(), containsString(format("(%d)", startNode.Id)) );
			  assertThat( deletedToString.get(), containsString(format("(%d)", endNode.Id)) );
		 }

		 private class TransactionEventHandler_AdapterAnonymousInnerClass8 : Neo4Net.Graphdb.@event.TransactionEventHandler_Adapter<object>
		 {
			 private readonly TestTransactionEvents _outerInstance;

			 private Relationship _relationship;
			 private AtomicReference<string> _deletedToString;

			 public TransactionEventHandler_AdapterAnonymousInnerClass8( TestTransactionEvents outerInstance, Relationship relationship, AtomicReference<string> deletedToString )
			 {
				 this.outerInstance = outerInstance;
				 this._relationship = relationship;
				 this._deletedToString = deletedToString;
			 }

			 public override void afterCommit( TransactionData data, object state )
			 {
				  foreach ( Relationship relationship in data.DeletedRelationships() )
				  {
						_deletedToString.set( relationship.ToString() );
				  }
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetCallToAfterRollbackEvenIfBeforeCommitFailed()
		 public virtual void ShouldGetCallToAfterRollbackEvenIfBeforeCommitFailed()
		 {
			  // given
			  CapturingEventHandler<int> firstWorkingHandler = new CapturingEventHandler<int>( () => 5 );
			  string failureMessage = "Massive fail";
			  CapturingEventHandler<int> faultyHandler = new CapturingEventHandler<int>(() =>
			  {
				throw new Exception( failureMessage );
			  });
			  CapturingEventHandler<int> otherWorkingHandler = new CapturingEventHandler<int>( () => 10 );
			  DbRule.registerTransactionEventHandler( firstWorkingHandler );
			  DbRule.registerTransactionEventHandler( faultyHandler );
			  DbRule.registerTransactionEventHandler( otherWorkingHandler );

			  bool failed = false;
			  try
			  {
					  using ( Transaction tx = DbRule.beginTx() )
					  {
						// when
						DbRule.createNode();
						tx.Success();
					  }
			  }
			  catch ( Exception e )
			  {
					assertTrue( Exceptions.contains( e, failureMessage, typeof( Exception ) ) );
					failed = true;
			  }
			  assertTrue( failed );

			  // then
			  assertTrue( firstWorkingHandler.BeforeCommitCalled );
			  assertTrue( firstWorkingHandler.AfterRollbackCalled );
			  assertEquals( 5, firstWorkingHandler.AfterRollbackState );
			  assertTrue( faultyHandler.BeforeCommitCalled );
			  assertTrue( faultyHandler.AfterRollbackCalled );
			  assertNull( faultyHandler.AfterRollbackState );
			  assertTrue( otherWorkingHandler.BeforeCommitCalled );
			  assertTrue( otherWorkingHandler.AfterRollbackCalled );
			  assertEquals( 10, otherWorkingHandler.AfterRollbackState );
		 }

		 private Node CreateNode( params string[] properties )
		 {
			  using ( Transaction tx = DbRule.beginTx() )
			  {
					Node node = DbRule.createNode();
					for ( int i = 0; i < properties.Length; i++ )
					{
						 node.SetProperty( properties[i++], properties[i] );
					}
					tx.Success();
					return node;
			  }
		 }

		 private Node CreateTree( int depth, int width )
		 {
			  using ( Transaction tx = DbRule.beginTx() )
			  {
					Node root = DbRule.createNode( TestLabels.LABEL_ONE );
					CreateTree( root, depth, width, 0 );
					tx.Success();
					return root;
			  }
		 }

		 private void CreateTree( Node parent, int maxDepth, int width, int currentDepth )
		 {
			  if ( currentDepth > maxDepth )
			  {
					return;
			  }
			  for ( int i = 0; i < width; i++ )
			  {
					Node child = DbRule.createNode( TestLabels.LABEL_TWO );
					parent.CreateRelationshipTo( child, MyRelTypes.TEST );
					CreateTree( child, maxDepth, width, currentDepth + 1 );
			  }
		 }

		 private sealed class ChangedLabels : Neo4Net.Graphdb.@event.TransactionEventHandler_Adapter<Void>
		 {
			  internal readonly IDictionary<Node, ISet<string>> Added = new Dictionary<Node, ISet<string>>();
			  internal readonly IDictionary<Node, ISet<string>> Removed = new Dictionary<Node, ISet<string>>();
			  internal bool Active;

			  public override Void BeforeCommit( TransactionData data )
			  {
					if ( Active )
					{
						 Check( Added, "added to", data.AssignedLabels() );
						 Check( Removed, "removed from", data.RemovedLabels() );
					}
					Active = false;
					return null;
			  }

			  internal void Check( IDictionary<Node, ISet<string>> expected, string change, IEnumerable<LabelEntry> changes )
			  {
					foreach ( LabelEntry entry in changes )
					{
						 ISet<string> labels = expected[entry.Node()];
						 string message = string.Format( "':{0}' should not be {1} {2}", entry.Label().name(), change, entry.Node() );
						 assertNotNull( message, labels );
						 assertTrue( message, labels.remove( entry.Label().name() ) );
						 if ( labels.Count == 0 )
						 {
							  expected.Remove( entry.Node() );
						 }
					}
					assertTrue( string.Format( "Expected more labels {0} nodes: {1}", change, expected ), expected.Count == 0 );
			  }

			  public bool Empty
			  {
				  get
				  {
						return Added.Count == 0 && Removed.Count == 0;
				  }
			  }

			  public void Add( Node node, string label )
			  {
					node.AddLabel( label( label ) );
					Put( Added, node, label );
			  }

			  public void Remove( Node node, string label )
			  {
					node.RemoveLabel( label( label ) );
					Put( Removed, node, label );
			  }

			  internal void Put( IDictionary<Node, ISet<string>> changes, Node node, string label )
			  {
					ISet<string> labels = changes.computeIfAbsent( node, k => new HashSet<string>() );
					labels.Add( label );
			  }

			  public void Activate()
			  {
					assertFalse( Empty );
					Active = true;
			  }

			  public void Clear()
			  {
					Added.Clear();
					Removed.Clear();
					Active = false;
			  }
		 }

		 private class RelationshipData
		 {
			  internal readonly Node StartNode;
			  internal readonly string Type;
			  internal readonly Node EndNode;

			  internal RelationshipData( Relationship relationship )
			  {
					this.StartNode = relationship.StartNode;
					this.Type = relationship.Type.name();
					this.EndNode = relationship.EndNode;
			  }
		 }

		 internal class CapturingEventHandler<T> : TransactionEventHandler<T>
		 {
			  internal readonly System.Func<T> StateSource;
			  internal bool BeforeCommitCalled;
			  internal bool AfterCommitCalled;
			  internal T AfterCommitState;
			  internal bool AfterRollbackCalled;
			  internal T AfterRollbackState;

			  internal CapturingEventHandler( System.Func<T> stateSource )
			  {
					this.StateSource = stateSource;
			  }

			  public override T BeforeCommit( TransactionData data )
			  {
					BeforeCommitCalled = true;
					return StateSource.get();
			  }

			  public override void AfterCommit( TransactionData data, T state )
			  {
					AfterCommitCalled = true;
					AfterCommitState = state;
			  }

			  public override void AfterRollback( TransactionData data, T state )
			  {
					AfterRollbackCalled = true;
					AfterRollbackState = state;
			  }
		 }
	}

}