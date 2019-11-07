/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Procedure
{
	using Matchers = org.hamcrest.Matchers;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using TemporaryFolder = org.junit.rules.TemporaryFolder;


	using Direction = Neo4Net.GraphDb.Direction;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using NotFoundException = Neo4Net.GraphDb.NotFoundException;
	using QueryExecutionException = Neo4Net.GraphDb.QueryExecutionException;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Result = Neo4Net.GraphDb.Result;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using JarBuilder = Neo4Net.Kernel.impl.proc.JarBuilder;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.procedure.Mode.READ;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.procedure.Mode.WRITE;

	public class EagerProcedureIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.TemporaryFolder plugins = new org.junit.rules.TemporaryFolder();
		 public TemporaryFolder Plugins = new TemporaryFolder();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException exception = org.junit.rules.ExpectedException.none();
		 public ExpectedException Exception = ExpectedException.none();

		 private IGraphDatabaseService _db;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotGetPropertyAccessFailureWhenStreamingToAnEagerDestructiveProcedure()
		 public virtual void ShouldNotGetPropertyAccessFailureWhenStreamingToAnEagerDestructiveProcedure()
		 {
			  // When we have a simple graph (a)
			  SetUpTestData();

			  // Then we can run an eagerized destructive procedure
			  Result res = _db.execute( "MATCH (n) WHERE n.key = 'value' " + "WITH n CALL Neo4Net.procedure.deleteNeighboursEagerized(n, 'FOLLOWS') " + "YIELD value RETURN value" );
			  assertThat( "Should get as many rows as original nodes", res.ResultAsString(), containsString("2 rows") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetPropertyAccessFailureWhenStreamingToANonEagerDestructiveProcedure()
		 public virtual void ShouldGetPropertyAccessFailureWhenStreamingToANonEagerDestructiveProcedure()
		 {
			  // When we have a simple graph (a)
			  SetUpTestData();

			  // Expect a specific error
			  Exception.expect( typeof( QueryExecutionException ) );
			  Exception.expectMessage( "Node with id 1 has been deleted in this transaction" );

			  // When we try to run an eagerized destructive procedure
			  Result res = _db.execute( "MATCH (n) WHERE n.key = 'value' " + "WITH n CALL Neo4Net.procedure.deleteNeighboursNotEagerized(n, 'FOLLOWS') " + "YIELD value RETURN value" );
			  res.ResultAsString(); // pull all results. The second row will cause the exception
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotGetErrorBecauseOfNormalEagerizationWhenStreamingFromANormalReadProcedureToDestructiveCypher()
		 public virtual void ShouldNotGetErrorBecauseOfNormalEagerizationWhenStreamingFromANormalReadProcedureToDestructiveCypher()
		 {
			  // When we have a simple graph (a)
			  int count = 10;
			  UpTestData = count;

			  // Then we can run an normal read procedure and it will be eagerized by normal Cypher eagerization
			  Result res = _db.execute( "MATCH (n) WHERE n.key = 'value' " + "CALL Neo4Net.procedure.findNeighboursNotEagerized(n) " + "YIELD relationship AS r, node as m " + "DELETE r, m RETURN true" );
			  assertThat( "Should get one fewer rows than original nodes", res.ResultAsString(), containsString((count - 1) + " rows") );
			  assertThat( "The plan description should contain the 'Eager' operation", res.ExecutionPlanDescription.ToString(), containsString("+Eager") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetEagerPlanForAnEagerProcedure()
		 public virtual void ShouldGetEagerPlanForAnEagerProcedure()
		 {
			  // When explaining a call to an eagerized procedure
			  Result res = _db.execute( "EXPLAIN MATCH (n) WHERE n.key = 'value' " + "WITH n CALL Neo4Net.procedure.deleteNeighboursEagerized(n, 'FOLLOWS') " + "YIELD value RETURN value" );
			  assertThat( "The plan description should contain the 'Eager' operation", res.ExecutionPlanDescription.ToString(), containsString("+Eager") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotGetEagerPlanForANonEagerProcedure()
		 public virtual void ShouldNotGetEagerPlanForANonEagerProcedure()
		 {
			  // When explaining a call to an non-eagerized procedure
			  Result res = _db.execute( "EXPLAIN MATCH (n) WHERE n.key = 'value' " + "WITH n CALL Neo4Net.procedure.deleteNeighboursNotEagerized(n, 'FOLLOWS') " + "YIELD value RETURN value" );
			  assertThat( "The plan description shouldn't contain the 'Eager' operation", res.ExecutionPlanDescription.ToString(), Matchers.not(containsString("+Eager")) );
		 }

		 private void SetUpTestData()
		 {
			  UpTestData = 2;
		 }

		 private void SetUpTestData( int nodes )
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					CreateChainOfNodesWithLabelAndProperty( nodes, "FOLLOWS", "User", "key", "value" );
					tx.Success();
			  }
		 }

		 private void CreateChainOfNodesWithLabelAndProperty( int length, string relationshipName, string labelName, string property, string value )
		 {
			  RelationshipType relationshipType = RelationshipType.withName( relationshipName );
			  Label label = Label.label( labelName );
			  Node prev = null;
			  for ( int i = 0; i < length; i++ )
			  {
					Node node = _db.createNode( label );
					node.SetProperty( property, value );
					if ( !property.Equals( "name" ) )
					{
						 node.SetProperty( "name", labelName + " " + i );
					}
					if ( prev != null )
					{
						 prev.CreateRelationshipTo( node, relationshipType );
					}
					prev = node;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  ( new JarBuilder() ).createJarFor(Plugins.newFile("myProcedures.jar"), typeof(ClassWithProcedures));
			  _db = ( new TestGraphDatabaseFactory() ).newImpermanentDatabaseBuilder().setConfig(GraphDatabaseSettings.plugin_dir, Plugins.Root.AbsolutePath).newGraphDatabase();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  if ( this._db != null )
			  {
					this._db.shutdown();
			  }
		 }

		 public class Output
		 {
			  public readonly long Value;

			  public Output( long value )
			  {
					this.Value = value;
			  }
		 }

		 public class NeighbourOutput
		 {
			  public readonly Relationship Relationship;
			  public readonly Node Node;

			  public NeighbourOutput( Relationship relationship, Node node )
			  {
					this.Relationship = relationship;
					this.Node = node;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") public static class ClassWithProcedures
		 public class ClassWithProcedures
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public Neo4Net.graphdb.GraphDatabaseService db;
			  public IGraphDatabaseService Db;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure(mode = READ) public java.util.stream.Stream<NeighbourOutput> findNeighboursNotEagerized(@Name("node") Neo4Net.graphdb.Node node)
			  [Procedure(mode : READ)]
			  public virtual Stream<NeighbourOutput> FindNeighboursNotEagerized( Node node )
			  {
					return FindNeighbours( node );
			  }

			  internal virtual Stream<NeighbourOutput> FindNeighbours( Node node )
			  {
					return StreamSupport.stream( node.GetRelationships( Direction.OUTGOING ).spliterator(), false ).map(relationship => new NeighbourOutput(relationship, relationship.getOtherNode(node)));
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure(mode = WRITE, eager = true) public java.util.stream.Stream<Output> deleteNeighboursEagerized(@Name("node") Neo4Net.graphdb.Node node, @Name("relation") String relation)
			  [Procedure(mode : WRITE, eager : true)]
			  public virtual Stream<Output> DeleteNeighboursEagerized( Node node, string relation )
			  {
					return Stream.of( new Output( DeleteNeighbours( node, RelationshipType.withName( relation ) ) ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure(mode = WRITE) public java.util.stream.Stream<Output> deleteNeighboursNotEagerized(@Name("node") Neo4Net.graphdb.Node node, @Name("relation") String relation)
			  [Procedure(mode : WRITE)]
			  public virtual Stream<Output> DeleteNeighboursNotEagerized( Node node, string relation )
			  {
					return Stream.of( new Output( DeleteNeighbours( node, RelationshipType.withName( relation ) ) ) );
			  }

			  internal virtual long DeleteNeighbours( Node node, RelationshipType relType )
			  {
					try
					{
						 long deleted = 0;
						 foreach ( Relationship rel in node.Relationships )
						 {
							  Node other = rel.GetOtherNode( node );
							  rel.Delete();
							  other.Delete();
							  deleted++;
						 }
						 return deleted;
					}
					catch ( NotFoundException )
					{
						 // Procedures should internally handle missing nodes due to lazy interactions
						 return 0;
					}
			  }
		 }
	}

}