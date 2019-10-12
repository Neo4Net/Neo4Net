using System;
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
namespace Synchronization
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using ConsistencyCheckService = Org.Neo4j.Consistency.ConsistencyCheckService;
	using ConsistencyCheckIncompleteException = Org.Neo4j.Consistency.checking.full.ConsistencyCheckIncompleteException;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Label = Org.Neo4j.Graphdb.Label;
	using Node = Org.Neo4j.Graphdb.Node;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using ProgressMonitorFactory = Org.Neo4j.Helpers.progress.ProgressMonitorFactory;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using FormattedLogProvider = Org.Neo4j.Logging.FormattedLogProvider;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using SuppressOutput = Org.Neo4j.Test.rule.SuppressOutput;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class ConcurrentChangesOnEntitiesTest
	{
		private bool InstanceFieldsInitialized = false;

		public ConcurrentChangesOnEntitiesTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _suppressOutput ).around( _testDirectory );
		}

		 private readonly SuppressOutput _suppressOutput = SuppressOutput.suppressAll();
		 private readonly TestDirectory _testDirectory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(suppressOutput).around(testDirectory);
		 public RuleChain RuleChain;

		 private readonly CyclicBarrier _barrier = new CyclicBarrier( 2 );
		 private readonly AtomicReference<Exception> _ex = new AtomicReference<Exception>();
		 private GraphDatabaseService _db;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _db = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(_testDirectory.databaseDir()).newGraphDatabase();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addConcurrentlySameLabelToANode() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AddConcurrentlySameLabelToANode()
		 {

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long nodeId = initWithNode(db);
			  long nodeId = InitWithNode( _db );

			  Thread t1 = NewThreadForNodeAction( nodeId, node => node.addLabel( Label.label( "A" ) ) );

			  Thread t2 = NewThreadForNodeAction( nodeId, node => node.addLabel( Label.label( "A" ) ) );

			  StartAndWait( t1, t2 );

			  _db.shutdown();

			  AssertDatabaseConsistent();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void setConcurrentlySamePropertyWithDifferentValuesOnANode() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetConcurrentlySamePropertyWithDifferentValuesOnANode()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long nodeId = initWithNode(db);
			  long nodeId = InitWithNode( _db );

			  Thread t1 = NewThreadForNodeAction( nodeId, node => node.setProperty( "a", 0.788 ) );

			  Thread t2 = NewThreadForNodeAction( nodeId, node => node.setProperty( "a", new double[]{ 0.999, 0.77 } ) );

			  StartAndWait( t1, t2 );

			  _db.shutdown();

			  AssertDatabaseConsistent();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void setConcurrentlySamePropertyWithDifferentValuesOnARelationship() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetConcurrentlySamePropertyWithDifferentValuesOnARelationship()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long relId = initWithRel(db);
			  long relId = InitWithRel( _db );

			  Thread t1 = NewThreadForRelationshipAction( relId, relationship => relationship.setProperty( "a", 0.788 ) );

			  Thread t2 = NewThreadForRelationshipAction( relId, relationship => relationship.setProperty( "a", new double[]{ 0.999, 0.77 } ) );

			  StartAndWait( t1, t2 );

			  _db.shutdown();

			  AssertDatabaseConsistent();
		 }

		 private long InitWithNode( GraphDatabaseService db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node theNode = Db.createNode();
					long id = theNode.Id;
					tx.Success();
					return id;
			  }

		 }

		 private long InitWithRel( GraphDatabaseService db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode();
					node.SetProperty( "a", "prop" );
					Relationship rel = node.CreateRelationshipTo( Db.createNode(), RelationshipType.withName("T") );
					long id = rel.Id;
					tx.Success();
					return id;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private Thread newThreadForNodeAction(final long nodeId, final System.Action<org.neo4j.graphdb.Node> nodeConsumer)
		 private Thread NewThreadForNodeAction( long nodeId, System.Action<Node> nodeConsumer )
		 {
			  return new Thread(() =>
			  {
			  try
			  {
				  using ( Transaction tx = _db.beginTx() )
				  {
					  Node node = _db.getNodeById( nodeId );
					  _barrier.await();
					  nodeConsumer( node );
					  tx.success();
				  }
			  }
			  catch ( Exception e )
			  {
				  _ex.set( e );
			  }
			  });
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private Thread newThreadForRelationshipAction(final long relationshipId, final System.Action<org.neo4j.graphdb.Relationship> relConsumer)
		 private Thread NewThreadForRelationshipAction( long relationshipId, System.Action<Relationship> relConsumer )
		 {
			  return new Thread(() =>
			  {
			  try
			  {
				  using ( Transaction tx = _db.beginTx() )
				  {
					  Relationship relationship = _db.getRelationshipById( relationshipId );
					  _barrier.await();
					  relConsumer( relationship );
					  tx.success();
				  }
			  }
			  catch ( Exception e )
			  {
				  _ex.set( e );
			  }
			  });
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void startAndWait(Thread t1, Thread t2) throws Exception
		 private void StartAndWait( Thread t1, Thread t2 )
		 {
			  t1.Start();
			  t2.Start();

			  t1.Join();
			  t2.Join();

			  if ( _ex.get() != null )
			  {
					throw _ex.get();
			  }
		 }

		 private void AssertDatabaseConsistent()
		 {
			  LogProvider logProvider = FormattedLogProvider.toOutputStream( System.out );
			  try
			  {
					ConsistencyCheckService.Result result = ( new ConsistencyCheckService() ).runFullConsistencyCheck(_testDirectory.databaseLayout(), Config.defaults(), ProgressMonitorFactory.textual(System.err), logProvider, false);
					assertTrue( result.Successful );
			  }
			  catch ( ConsistencyCheckIncompleteException e )
			  {
					fail( e.Message );
			  }
		 }
	}

}