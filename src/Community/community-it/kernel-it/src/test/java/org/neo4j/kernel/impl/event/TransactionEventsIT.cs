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
namespace Neo4Net.Kernel.Impl.@event
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using RuleChain = org.junit.rules.RuleChain;


	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using PropertyContainer = Neo4Net.Graphdb.PropertyContainer;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using TransactionData = Neo4Net.Graphdb.@event.TransactionData;
	using Neo4Net.Graphdb.@event;
	using AccessMode = Neo4Net.@internal.Kernel.Api.security.AccessMode;
	using AuthSubject = Neo4Net.@internal.Kernel.Api.security.AuthSubject;
	using LoginContext = Neo4Net.@internal.Kernel.Api.security.LoginContext;
	using SecurityContext = Neo4Net.@internal.Kernel.Api.security.SecurityContext;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using Statement = Neo4Net.Kernel.api.Statement;
	using AnonymousContext = Neo4Net.Kernel.api.security.AnonymousContext;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using Neo4Net.Test.mockito.matcher;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using BinaryLatch = Neo4Net.Util.concurrent.BinaryLatch;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.sameInstance;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.genericMap;

	/// <summary>
	/// Test for randomly creating data and verifying transaction data seen in transaction event handlers.
	/// </summary>
	public class TransactionEventsIT
	{
		private bool InstanceFieldsInitialized = false;

		public TransactionEventsIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _random ).around( _expectedException ).around( _db );
		}

		 private readonly DatabaseRule _db = new ImpermanentDatabaseRule();
		 private readonly RandomRule _random = new RandomRule();
		 private readonly ExpectedException _expectedException = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(random).around(expectedException).around(db);
		 public RuleChain RuleChain;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeExpectedTransactionData()
		 public virtual void ShouldSeeExpectedTransactionData()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Graph state = new Graph(db, random);
			  Graph state = new Graph( _db, _random );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ExpectedTransactionData expected = new ExpectedTransactionData(true);
			  ExpectedTransactionData expected = new ExpectedTransactionData( true );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.event.TransactionEventHandler<Object> handler = new VerifyingTransactionEventHandler(expected);
			  TransactionEventHandler<object> handler = new VerifyingTransactionEventHandler( expected );
			  using ( Transaction tx = _db.beginTx() )
			  {
					for ( int i = 0; i < 100; i++ )
					{
						 Operation.CreateNode.perform( state, expected );
					}
					for ( int i = 0; i < 20; i++ )
					{
						 Operation.CreateRelationship.perform( state, expected );
					}
					tx.Success();
			  }

			  _db.registerTransactionEventHandler( handler );

			  // WHEN
			  Operation[] operations = Operation.values();
			  for ( int i = 0; i < 1_000; i++ )
			  {
					expected.Clear();
					using ( Transaction tx = _db.beginTx() )
					{
						 int transactionSize = _random.intBetween( 1, 20 );
						 for ( int j = 0; j < transactionSize; j++ )
						 {
							  _random.among( operations ).perform( state, expected );
						 }
						 tx.Success();
					}
			  }

			  // THEN the verifications all happen inside the transaction event handler
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void transactionIdAndCommitTimeAccessibleAfterCommit()
		 public virtual void TransactionIdAndCommitTimeAccessibleAfterCommit()
		 {
			  TransactionIdCommitTimeTracker commitTimeTracker = new TransactionIdCommitTimeTracker();
			  _db.registerTransactionEventHandler( commitTimeTracker );

			  RunTransaction();

			  long firstTransactionId = commitTimeTracker.TransactionIdAfterCommit;
			  long firstTransactionCommitTime = commitTimeTracker.CommitTimeAfterCommit;
			  assertTrue( "Should be positive tx id.", firstTransactionId > 0 );
			  assertTrue( "Should be positive.", firstTransactionCommitTime > 0 );

			  RunTransaction();

			  long secondTransactionId = commitTimeTracker.TransactionIdAfterCommit;
			  long secondTransactionCommitTime = commitTimeTracker.CommitTimeAfterCommit;
			  assertTrue( "Should be positive tx id.", secondTransactionId > 0 );
			  assertTrue( "Should be positive commit time value.", secondTransactionCommitTime > 0 );

			  assertTrue( "Second tx id should be higher then first one.", secondTransactionId > firstTransactionId );
			  assertTrue( "Second commit time should be higher or equals then first one.", secondTransactionCommitTime >= firstTransactionCommitTime );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void transactionIdNotAccessibleBeforeCommit()
		 public virtual void TransactionIdNotAccessibleBeforeCommit()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  _db.registerTransactionEventHandler( GetBeforeCommitHandler( TransactionData::getTransactionId ) );
			  string message = "Transaction id is not assigned yet. It will be assigned during transaction commit.";
			  _expectedException.expectCause( new RootCauseMatcher<>( typeof( System.InvalidOperationException ), message ) );
			  RunTransaction();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void commitTimeNotAccessibleBeforeCommit()
		 public virtual void CommitTimeNotAccessibleBeforeCommit()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  _db.registerTransactionEventHandler( GetBeforeCommitHandler( TransactionData::getCommitTime ) );
			  string message = "Transaction commit time is not assigned yet. It will be assigned during transaction commit.";
			  _expectedException.expectCause( new RootCauseMatcher<>( typeof( System.InvalidOperationException ), message ) );
			  RunTransaction();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetEmptyUsernameOnAuthDisabled()
		 public virtual void ShouldGetEmptyUsernameOnAuthDisabled()
		 {
			  _db.registerTransactionEventHandler(GetBeforeCommitHandler(txData =>
			  {
				assertThat( "Should have no username", txData.username(), equalTo("") );
				assertThat( "Should have no metadata", txData.metaData(), equalTo(java.util.Collections.emptyMap()) );
			  }));
			  RunTransaction();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetSpecifiedUsernameAndMetaDataInTXData()
		 public virtual void ShouldGetSpecifiedUsernameAndMetaDataInTXData()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicReference<String> usernameRef = new java.util.concurrent.atomic.AtomicReference<>();
			  AtomicReference<string> usernameRef = new AtomicReference<string>();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicReference<java.util.Map<String,Object>> metaDataRef = new java.util.concurrent.atomic.AtomicReference<>();
			  AtomicReference<IDictionary<string, object>> metaDataRef = new AtomicReference<IDictionary<string, object>>();
			  _db.registerTransactionEventHandler(GetBeforeCommitHandler(txData =>
			  {
				usernameRef.set( txData.username() );
				metaDataRef.set( txData.metaData() );
			  }));
			  AuthSubject subject = mock( typeof( AuthSubject ) );
			  when( subject.Username() ).thenReturn("Christof");
			  LoginContext loginContext = new LoginContextAnonymousInnerClass( this, subject );
			  IDictionary<string, object> metadata = genericMap( "username", "joe" );
			  RunTransaction( loginContext, metadata );

			  assertThat( "Should have specified username", usernameRef.get(), equalTo("Christof") );
			  assertThat( "Should have metadata with specified username", metaDataRef.get(), equalTo(metadata) );
		 }

		 private class LoginContextAnonymousInnerClass : LoginContext
		 {
			 private readonly TransactionEventsIT _outerInstance;

			 private AuthSubject _subject;

			 public LoginContextAnonymousInnerClass( TransactionEventsIT outerInstance, AuthSubject subject )
			 {
				 this.outerInstance = outerInstance;
				 this._subject = subject;
			 }

			 public AuthSubject subject()
			 {
				  return _subject;
			 }

			 public SecurityContext authorize( System.Func<string, int> propertyIdLookup, string dbName )
			 {
				  return new SecurityContext( _subject, Neo4Net.@internal.Kernel.Api.security.AccessMode_Static.Write );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void registerUnregisterWithConcurrentTransactions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RegisterUnregisterWithConcurrentTransactions()
		 {
			  ExecutorService executor = Executors.newFixedThreadPool( 2 );
			  AtomicInteger runningCounter = new AtomicInteger();
			  AtomicInteger doneCounter = new AtomicInteger();
			  BinaryLatch startLatch = new BinaryLatch();
			  RelationshipType relationshipType = RelationshipType.withName( "REL" );
			  CountingTransactionEventHandler[] handlers = new CountingTransactionEventHandler[20];
			  for ( int i = 0; i < handlers.Length; i++ )
			  {
					handlers[i] = new CountingTransactionEventHandler();
			  }
			  long relNodeId;
			  using ( Transaction tx = _db.beginTx() )
			  {
					relNodeId = _db.createNode().Id;
					tx.Success();
			  }
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> nodeCreator = executor.submit(() ->
			  Future<object> nodeCreator = executor.submit(() =>
			  {
				try
				{
					 runningCounter.incrementAndGet();
					 startLatch.Await();
					 for ( int i = 0; i < 2_000; i++ )
					 {
						  using ( Transaction tx = _db.beginTx() )
						  {
								_db.createNode();
								if ( ThreadLocalRandom.current().nextBoolean() )
								{
									 tx.Success();
								}
						  }
					 }
				}
				finally
				{
					 doneCounter.incrementAndGet();
				}
			  });
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> relationshipCreator = executor.submit(() ->
			  Future<object> relationshipCreator = executor.submit(() =>
			  {
				try
				{
					 runningCounter.incrementAndGet();
					 startLatch.Await();
					 for ( int i = 0; i < 1_000; i++ )
					 {
						  using ( Transaction tx = _db.beginTx() )
						  {
								Node relNode = _db.getNodeById( relNodeId );
								relNode.createRelationshipTo( relNode, relationshipType );
								if ( ThreadLocalRandom.current().nextBoolean() )
								{
									 tx.Success();
								}
						  }
					 }
				}
				finally
				{
					 doneCounter.incrementAndGet();
				}
			  });
			  while ( runningCounter.get() < 2 )
			  {
					Thread.yield();
			  }
			  int i = 0;
			  _db.registerTransactionEventHandler( handlers[i] );
			  CountingTransactionEventHandler currentlyRegistered = handlers[i];
			  i++;
			  startLatch.Release();
			  while ( doneCounter.get() < 2 )
			  {
					_db.registerTransactionEventHandler( handlers[i] );
					i++;
					if ( i == handlers.Length )
					{
						 i = 0;
					}
					_db.unregisterTransactionEventHandler( currentlyRegistered );
					currentlyRegistered = handlers[i];
			  }
			  nodeCreator.get();
			  relationshipCreator.get();
			  foreach ( CountingTransactionEventHandler handler in handlers )
			  {
					assertEquals( 0, handler.get() );
			  }
		 }

		 private Neo4Net.Graphdb.@event.TransactionEventHandler_Adapter<object> GetBeforeCommitHandler( System.Action<TransactionData> dataConsumer )
		 {
			  return new TransactionEventHandler_AdapterAnonymousInnerClass( this, dataConsumer );
		 }

		 private class TransactionEventHandler_AdapterAnonymousInnerClass : Neo4Net.Graphdb.@event.TransactionEventHandler_Adapter<object>
		 {
			 private readonly TransactionEventsIT _outerInstance;

			 private System.Action<TransactionData> _dataConsumer;

			 public TransactionEventHandler_AdapterAnonymousInnerClass( TransactionEventsIT outerInstance, System.Action<TransactionData> dataConsumer )
			 {
				 this.outerInstance = outerInstance;
				 this._dataConsumer = dataConsumer;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Object beforeCommit(org.neo4j.graphdb.event.TransactionData data) throws Exception
			 public override object beforeCommit( TransactionData data )
			 {
				  _dataConsumer( data );
				  return base.beforeCommit( data );
			 }
		 }

		 private void RunTransaction()
		 {
			  RunTransaction( AnonymousContext.write(), Collections.emptyMap() );
		 }

		 private void RunTransaction( LoginContext loginContext, IDictionary<string, object> metaData )
		 {
			  using ( Transaction transaction = _db.beginTransaction( KernelTransaction.Type.@explicit, loginContext ), Statement statement = _db.DependencyResolver.resolveDependency( typeof( ThreadToStatementContextBridge ) ).get() )
			  {
					statement.QueryRegistration().MetaData = metaData;
					_db.createNode();
					transaction.Success();
			  }
		 }

		 internal abstract class Operation
		 {
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           createNode { void perform(Graph graph, ExpectedTransactionData expectations) { org.neo4j.graphdb.Node node = graph.createNode(); expectations.createdNode(node); debug(node); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           deleteNode { void perform(Graph graph, ExpectedTransactionData expectations) { org.neo4j.graphdb.Node node = graph.randomNode(); if(node != null) { for(org.neo4j.graphdb.Relationship relationship : node.getRelationships()) { graph.deleteRelationship(relationship); expectations.deletedRelationship(relationship); debug(relationship); } graph.deleteNode(node); expectations.deletedNode(node); debug(node); } } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           assignLabel { void perform(Graph graph, ExpectedTransactionData expectations) { org.neo4j.graphdb.Node node = graph.randomNode(); if(node != null) { org.neo4j.graphdb.Label label = graph.randomLabel(); if(!node.hasLabel(label)) { node.addLabel(label); expectations.assignedLabel(node, label); debug(node + " " + label); } } } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           removeLabel { void perform(Graph graph, ExpectedTransactionData expectations) { org.neo4j.graphdb.Node node = graph.randomNode(); if(node != null) { org.neo4j.graphdb.Label label = graph.randomLabel(); if(node.hasLabel(label)) { node.removeLabel(label); expectations.removedLabel(node, label); debug(node + " " + label); } } } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           setNodeProperty { void perform(Graph graph, ExpectedTransactionData expectations) { org.neo4j.graphdb.Node node = graph.randomNode(); if(node != null) { String key = graph.randomPropertyKey(); Object valueBefore = node.getProperty(key, null); Object value = graph.randomPropertyValue(); node.setProperty(key, value); expectations.assignedProperty(node, key, value, valueBefore); debug(node + " " + key + "=" + value + " prev " + valueBefore); } } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           removeNodeProperty { void perform(Graph graph, ExpectedTransactionData expectations) { org.neo4j.graphdb.Node node = graph.randomNode(); if(node != null) { String key = graph.randomPropertyKey(); if(node.hasProperty(key)) { Object valueBefore = node.removeProperty(key); expectations.removedProperty(node, key, valueBefore); debug(node + " " + key + "=" + valueBefore); } } } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           setRelationshipProperty { void perform(Graph graph, ExpectedTransactionData expectations) { org.neo4j.graphdb.Relationship relationship = graph.randomRelationship(); if(relationship != null) { String key = graph.randomPropertyKey(); Object valueBefore = relationship.getProperty(key, null); Object value = graph.randomPropertyValue(); relationship.setProperty(key, value); expectations.assignedProperty(relationship, key, value, valueBefore); debug(relationship + " " + key + "=" + value + " prev " + valueBefore); } } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           removeRelationshipProperty { void perform(Graph graph, ExpectedTransactionData expectations) { org.neo4j.graphdb.Relationship relationship = graph.randomRelationship(); if(relationship != null) { String key = graph.randomPropertyKey(); if(relationship.hasProperty(key)) { Object valueBefore = relationship.removeProperty(key); expectations.removedProperty(relationship, key, valueBefore); debug(relationship + " " + key + "=" + valueBefore); } } } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           createRelationship { void perform(Graph graph, ExpectedTransactionData expectations) { while (graph.nodeCount() < 2) { createNode.perform(graph, expectations); } org.neo4j.graphdb.Node node1 = graph.randomNode(); org.neo4j.graphdb.Node node2 = graph.randomNode(); org.neo4j.graphdb.Relationship relationship = graph.createRelationship(node1, node2, graph.randomRelationshipType()); expectations.createdRelationship(relationship); debug(relationship); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           deleteRelationship { void perform(Graph graph, ExpectedTransactionData expectations) { org.neo4j.graphdb.Relationship relationship = graph.randomRelationship(); if(relationship != null) { graph.deleteRelationship(relationship); expectations.deletedRelationship(relationship); debug(relationship); } } };

			  private static readonly IList<Operation> valueList = new List<Operation>();

			  static Operation()
			  {
				  valueList.Add( createNode );
				  valueList.Add( deleteNode );
				  valueList.Add( assignLabel );
				  valueList.Add( removeLabel );
				  valueList.Add( setNodeProperty );
				  valueList.Add( removeNodeProperty );
				  valueList.Add( setRelationshipProperty );
				  valueList.Add( removeRelationshipProperty );
				  valueList.Add( createRelationship );
				  valueList.Add( deleteRelationship );
			  }

			  public enum InnerEnum
			  {
				  createNode,
				  deleteNode,
				  assignLabel,
				  removeLabel,
				  setNodeProperty,
				  removeNodeProperty,
				  setRelationshipProperty,
				  removeRelationshipProperty,
				  createRelationship,
				  deleteRelationship
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private Operation( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  internal abstract void perform( Graph graph, ExpectedTransactionData expectations );

			  internal void Debug( object value )
			  { // Add a system.out here if you need to debug this case a bit easier
			  }

			 public static IList<Operation> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static Operation valueOf( string name )
			 {
				 foreach ( Operation enumInstance in Operation.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

		 private class Graph
		 {
			  internal static readonly string[] Tokens = new string[] { "A", "B", "C", "D", "E" };

			  internal readonly GraphDatabaseService Db;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly RandomRule RandomConflict;
			  internal readonly IList<Node> Nodes = new List<Node>();
			  internal readonly IList<Relationship> Relationships = new List<Relationship>();

			  internal Graph( GraphDatabaseService db, RandomRule random )
			  {
					this.Db = db;
					this.RandomConflict = random;
			  }

			  internal virtual E Random<E>( IList<E> entities ) where E : Neo4Net.Graphdb.PropertyContainer
			  {
					return entities.Count == 0 ? default( E ) : entities[RandomConflict.Next( entities.Count )];
			  }

			  internal virtual Node RandomNode()
			  {
					return Random( Nodes );
			  }

			  internal virtual Relationship RandomRelationship()
			  {
					return Random( Relationships );
			  }

			  internal virtual Node CreateNode()
			  {
					Node node = Db.createNode();
					Nodes.Add( node );
					return node;
			  }

			  internal virtual void DeleteRelationship( Relationship relationship )
			  {
					relationship.Delete();
					Relationships.Remove( relationship );
			  }

			  internal virtual void DeleteNode( Node node )
			  {
					node.Delete();
					Nodes.Remove( node );
			  }

			  internal virtual string RandomToken()
			  {
					return RandomConflict.among( Tokens );
			  }

			  internal virtual Label RandomLabel()
			  {
					return Label.label( RandomToken() );
			  }

			  internal virtual RelationshipType RandomRelationshipType()
			  {
					return RelationshipType.withName( RandomToken() );
			  }

			  internal virtual string RandomPropertyKey()
			  {
					return RandomToken();
			  }

			  internal virtual object RandomPropertyValue()
			  {
					return RandomConflict.nextValueAsObject();
			  }

			  internal virtual int NodeCount()
			  {
					return Nodes.Count;
			  }

			  internal virtual Relationship CreateRelationship( Node node1, Node node2, RelationshipType type )
			  {
					Relationship relationship = node1.CreateRelationshipTo( node2, type );
					Relationships.Add( relationship );
					return relationship;
			  }
		 }

		 private class TransactionIdCommitTimeTracker : Neo4Net.Graphdb.@event.TransactionEventHandler_Adapter<object>
		 {

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long TransactionIdAfterCommitConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long CommitTimeAfterCommitConflict;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Object beforeCommit(org.neo4j.graphdb.event.TransactionData data) throws Exception
			  public override object BeforeCommit( TransactionData data )
			  {
					return base.BeforeCommit( data );
			  }

			  public override void AfterCommit( TransactionData data, object state )
			  {
					CommitTimeAfterCommitConflict = data.CommitTime;
					TransactionIdAfterCommitConflict = data.TransactionId;
					base.AfterCommit( data, state );
			  }

			  public virtual long TransactionIdAfterCommit
			  {
				  get
				  {
						return TransactionIdAfterCommitConflict;
				  }
			  }

			  public virtual long CommitTimeAfterCommit
			  {
				  get
				  {
						return CommitTimeAfterCommitConflict;
				  }
			  }
		 }

		 private class CountingTransactionEventHandler : AtomicInteger, TransactionEventHandler<CountingTransactionEventHandler>
		 {

			  public override CountingTransactionEventHandler BeforeCommit( TransactionData data )
			  {
					AndIncrement;
					return this;
			  }

			  public override void AfterCommit( TransactionData data, CountingTransactionEventHandler state )
			  {
					AndDecrement;
					assertThat( state, sameInstance( this ) );
			  }

			  public override void AfterRollback( TransactionData data, CountingTransactionEventHandler state )
			  {
					AndDecrement;
					assertThat( state, sameInstance( this ) );
			  }
		 }
	}

}