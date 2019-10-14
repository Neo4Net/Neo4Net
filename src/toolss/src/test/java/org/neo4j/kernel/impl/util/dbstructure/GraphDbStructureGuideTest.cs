/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.Kernel.impl.util.dbstructure
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using DependencyResolver = Neo4Net.Graphdb.DependencyResolver;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using IndexReference = Neo4Net.@internal.Kernel.Api.IndexReference;
	using TokenRead = Neo4Net.@internal.Kernel.Api.TokenRead;
	using ConstraintDescriptor = Neo4Net.@internal.Kernel.Api.schema.constraints.ConstraintDescriptor;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using SchemaDescriptorFactory = Neo4Net.Kernel.api.schema.SchemaDescriptorFactory;
	using UniquenessConstraintDescriptor = Neo4Net.Kernel.api.schema.constraints.UniquenessConstraintDescriptor;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.RelationshipType.withName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.StatementConstants.ANY_LABEL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.StatementConstants.ANY_RELATIONSHIP_TYPE;

	public class GraphDbStructureGuideTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void visitsLabelIds()
		 public virtual void VisitsLabelIds()
		 {
			  // GIVEN
			  DbStructureVisitor visitor = mock( typeof( DbStructureVisitor ) );
			  _graph.createNode( label( "Person" ) );
			  _graph.createNode( label( "Party" ) );
			  _graph.createNode( label( "Animal" ) );
			  int personLabelId;
			  int partyLabelId;
			  int animalLabelId;

			  KernelTransaction ktx = ktx();
			  TokenRead tokenRead = ktx.TokenRead();
			  personLabelId = tokenRead.NodeLabel( "Person" );
			  partyLabelId = tokenRead.NodeLabel( "Party" );
			  animalLabelId = tokenRead.NodeLabel( "Animal" );

			  // WHEN
			  Accept( visitor );

			  // THEN
			  verify( visitor ).visitLabel( personLabelId, "Person" );
			  verify( visitor ).visitLabel( partyLabelId, "Party" );
			  verify( visitor ).visitLabel( animalLabelId, "Animal" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void visitsPropertyKeyIds() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void VisitsPropertyKeyIds()
		 {
			  // GIVEN
			  DbStructureVisitor visitor = mock( typeof( DbStructureVisitor ) );
			  int nameId = CreatePropertyKey( "name" );
			  int ageId = CreatePropertyKey( "age" );
			  int osId = CreatePropertyKey( "os" );

			  // WHEN
			  Accept( visitor );

			  // THEN
			  verify( visitor ).visitPropertyKey( nameId, "name" );
			  verify( visitor ).visitPropertyKey( ageId, "age" );
			  verify( visitor ).visitPropertyKey( osId, "os" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void visitsRelationshipTypeIds()
		 public virtual void VisitsRelationshipTypeIds()
		 {
			  // GIVEN
			  DbStructureVisitor visitor = mock( typeof( DbStructureVisitor ) );
			  Node lhs = _graph.createNode();
			  Node rhs = _graph.createNode();
			  lhs.CreateRelationshipTo( rhs, withName( "KNOWS" ) );
			  lhs.CreateRelationshipTo( rhs, withName( "LOVES" ) );
			  lhs.CreateRelationshipTo( rhs, withName( "FAWNS_AT" ) );
			  int knowsId;
			  int lovesId;
			  int fawnsAtId;
			  KernelTransaction ktx = ktx();

			  TokenRead tokenRead = ktx.TokenRead();
			  knowsId = tokenRead.RelationshipType( "KNOWS" );
			  lovesId = tokenRead.RelationshipType( "LOVES" );
			  fawnsAtId = tokenRead.RelationshipType( "FAWNS_AT" );

			  // WHEN
			  Accept( visitor );

			  // THEN
			  verify( visitor ).visitRelationshipType( knowsId, "KNOWS" );
			  verify( visitor ).visitRelationshipType( lovesId, "LOVES" );
			  verify( visitor ).visitRelationshipType( fawnsAtId, "FAWNS_AT" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void visitsIndexes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void VisitsIndexes()
		 {
			  DbStructureVisitor visitor = mock( typeof( DbStructureVisitor ) );
			  int labelId = CreateLabel( "Person" );
			  int pkId = CreatePropertyKey( "name" );

			  CommitAndReOpen();

			  IndexReference reference = CreateSchemaIndex( labelId, pkId );

			  // WHEN
			  Accept( visitor );

			  // THEN
			  verify( visitor ).visitIndex( ( IndexDescriptor ) reference, ":Person(name)", 1.0d, 0L );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void visitsUniqueConstraintsAndIndices() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void VisitsUniqueConstraintsAndIndices()
		 {
			  DbStructureVisitor visitor = mock( typeof( DbStructureVisitor ) );
			  int labelId = CreateLabel( "Person" );
			  int pkId = CreatePropertyKey( "name" );

			  CommitAndReOpen();

			  ConstraintDescriptor constraint = CreateUniqueConstraint( labelId, pkId );
			  IndexDescriptor descriptor = TestIndexDescriptorFactory.uniqueForLabel( labelId, pkId );

			  // WHEN
			  Accept( visitor );

			  // THEN
			  verify( visitor ).visitIndex( descriptor, ":Person(name)", 1.0d, 0L );
			  verify( visitor ).visitUniqueConstraint( ( UniquenessConstraintDescriptor ) constraint, "CONSTRAINT ON ( person:Person ) ASSERT person.name IS " + "UNIQUE" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void visitsNodeCounts() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void VisitsNodeCounts()
		 {
			  // GIVEN
			  DbStructureVisitor visitor = mock( typeof( DbStructureVisitor ) );
			  int personLabelId = CreateLabeledNodes( "Person", 40 );
			  int partyLabelId = CreateLabeledNodes( "Party", 20 );
			  int animalLabelId = CreateLabeledNodes( "Animal", 30 );

			  // WHEN
			  Accept( visitor );

			  // THEN
			  verify( visitor ).visitAllNodesCount( 90 );
			  verify( visitor ).visitNodeCount( personLabelId, "Person", 40 );
			  verify( visitor ).visitNodeCount( partyLabelId, "Party", 20 );
			  verify( visitor ).visitNodeCount( animalLabelId, "Animal", 30 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void visitsRelCounts() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void VisitsRelCounts()
		 {
			  // GIVEN
			  DbStructureVisitor visitor = mock( typeof( DbStructureVisitor ) );

			  int personLabelId = CreateLabeledNodes( "Person", 40 );
			  int partyLabelId = CreateLabeledNodes( "Party", 20 );

			  int knowsId = CreateRelTypeId( "KNOWS" );
			  int lovesId = CreateRelTypeId( "LOVES" );

			  long personNode = CreateLabeledNode( personLabelId );
			  long partyNode = CreateLabeledNode( partyLabelId );

			  CreateRel( personNode, knowsId, personNode );
			  CreateRel( personNode, lovesId, partyNode );

			  // WHEN
			  Accept( visitor );

			  // THEN
			  verify( visitor ).visitRelCount( ANY_LABEL, knowsId, ANY_LABEL, "MATCH ()-[:KNOWS]->() RETURN count(*)", 1L );
			  verify( visitor ).visitRelCount( ANY_LABEL, lovesId, ANY_LABEL, "MATCH ()-[:LOVES]->() RETURN count(*)", 1L );
			  verify( visitor ).visitRelCount( ANY_LABEL, ANY_LABEL, ANY_LABEL, "MATCH ()-[]->() RETURN count(*)", 2L );

			  verify( visitor ).visitRelCount( personLabelId, knowsId, ANY_LABEL, "MATCH (:Person)-[:KNOWS]->() RETURN count(*)", 1L );
			  verify( visitor ).visitRelCount( ANY_LABEL, knowsId, personLabelId, "MATCH ()-[:KNOWS]->(:Person) RETURN count(*)", 1L );

			  verify( visitor ).visitRelCount( personLabelId, lovesId, ANY_LABEL, "MATCH (:Person)-[:LOVES]->() RETURN count(*)", 1L );
			  verify( visitor ).visitRelCount( ANY_LABEL, lovesId, personLabelId, "MATCH ()-[:LOVES]->(:Person) RETURN count(*)", 0L );

			  verify( visitor ).visitRelCount( personLabelId, ANY_RELATIONSHIP_TYPE, ANY_LABEL, "MATCH (:Person)-[]->() RETURN count(*)", 2L );
			  verify( visitor ).visitRelCount( ANY_LABEL, ANY_RELATIONSHIP_TYPE, personLabelId, "MATCH ()-[]->(:Person) RETURN count(*)", 1L );

			  verify( visitor ).visitRelCount( partyLabelId, knowsId, ANY_LABEL, "MATCH (:Party)-[:KNOWS]->() RETURN count(*)", 0L );
			  verify( visitor ).visitRelCount( ANY_LABEL, knowsId, partyLabelId, "MATCH ()-[:KNOWS]->(:Party) RETURN count(*)", 0L );

			  verify( visitor ).visitRelCount( partyLabelId, lovesId, ANY_LABEL, "MATCH (:Party)-[:LOVES]->() RETURN count(*)", 0L );
			  verify( visitor ).visitRelCount( ANY_LABEL, lovesId, partyLabelId, "MATCH ()-[:LOVES]->(:Party) RETURN count(*)", 1L );

			  verify( visitor ).visitRelCount( partyLabelId, ANY_RELATIONSHIP_TYPE, ANY_LABEL, "MATCH (:Party)-[]->() RETURN count(*)", 0L );
			  verify( visitor ).visitRelCount( ANY_LABEL, ANY_RELATIONSHIP_TYPE, partyLabelId, "MATCH ()-[]->(:Party) RETURN count(*)", 1L );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void createRel(long startId, int relTypeId, long endId) throws Exception
		 private void CreateRel( long startId, int relTypeId, long endId )
		 {
			  KernelTransaction ktx = ktx();

			  ktx.DataWrite().relationshipCreate(startId, relTypeId, endId);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.internal.kernel.api.IndexReference createSchemaIndex(int labelId, int pkId) throws Exception
		 private IndexReference CreateSchemaIndex( int labelId, int pkId )
		 {
			  KernelTransaction ktx = ktx();

			  return ktx.SchemaWrite().indexCreate(SchemaDescriptorFactory.forLabel(labelId, pkId));

		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.internal.kernel.api.schema.constraints.ConstraintDescriptor createUniqueConstraint(int labelId, int pkId) throws Exception
		 private ConstraintDescriptor CreateUniqueConstraint( int labelId, int pkId )
		 {
			  KernelTransaction ktx = ktx();

			  return ktx.SchemaWrite().uniquePropertyConstraintCreate(SchemaDescriptorFactory.forLabel(labelId, pkId));
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int createLabeledNodes(String labelName, int amount) throws Exception
		 private int CreateLabeledNodes( string labelName, int amount )
		 {
			  int labelId = CreateLabel( labelName );
			  for ( int i = 0; i < amount; i++ )
			  {
					CreateLabeledNode( labelId );
			  }
			  return labelId;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long createLabeledNode(int labelId) throws Exception
		 private long CreateLabeledNode( int labelId )
		 {
			  KernelTransaction ktx = ktx();

			  long nodeId = ktx.DataWrite().nodeCreate();
			  ktx.DataWrite().nodeAddLabel(nodeId, labelId);
			  return nodeId;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int createLabel(String name) throws Exception
		 private int CreateLabel( string name )
		 {
			  KernelTransaction ktx = ktx();
			  return ktx.TokenWrite().labelGetOrCreateForName(name);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int createPropertyKey(String name) throws Exception
		 private int CreatePropertyKey( string name )
		 {
			  KernelTransaction ktx = ktx();
			  return ktx.TokenWrite().propertyKeyGetOrCreateForName(name);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int createRelTypeId(String name) throws Exception
		 private int CreateRelTypeId( string name )
		 {
			  KernelTransaction ktx = ktx();

			  return ktx.TokenWrite().relationshipTypeGetOrCreateForName(name);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.ImpermanentDatabaseRule dbRule = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public ImpermanentDatabaseRule DbRule = new ImpermanentDatabaseRule();
		 private GraphDatabaseService _graph;
		 private ThreadToStatementContextBridge _bridge;
		 private Transaction _tx;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  GraphDatabaseAPI api = DbRule.GraphDatabaseAPI;
			  _graph = api;
			  DependencyResolver dependencyResolver = api.DependencyResolver;
			  this._bridge = dependencyResolver.ResolveDependency( typeof( ThreadToStatementContextBridge ) );
			  this._tx = _graph.beginTx();

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  if ( _bridge.hasTransaction() )
			  {
					_tx.failure();
					_tx.close();
			  }
		 }

		 internal virtual KernelTransaction Ktx()
		 {
			  return _bridge.getKernelTransactionBoundToThisThread( true );
		 }

		 public virtual void CommitAndReOpen()
		 {
			  Commit();

			  _tx = _graph.beginTx();
		 }

		 public virtual void Accept( DbStructureVisitor visitor )
		 {
			  CommitAndReOpen();

			  _graph.schema().awaitIndexesOnline(10, TimeUnit.SECONDS);
			  Commit();

			  if ( _bridge.hasTransaction() )
			  {
					throw new System.InvalidOperationException( "Dangling transaction before running visitable" );
			  }

			  GraphDbStructureGuide analyzer = new GraphDbStructureGuide( _graph );
			  analyzer.Accept( visitor );
		 }

		 private void Commit()
		 {
			  try
			  {
					_tx.success();
			  }
			  finally
			  {
					_tx.close();
			  }
		 }
	}

}