using System;
using System.Collections;
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
namespace Neo4Net.Kernel.builtinprocs
{
	using Matcher = org.hamcrest.Matcher;
	using Before = org.junit.Before;
	using Test = org.junit.Test;
	using Answer = org.mockito.stubbing.Answer;


	using DependencyResolver = Neo4Net.Graphdb.DependencyResolver;
	using Node = Neo4Net.Graphdb.Node;
	using Path = Neo4Net.Graphdb.Path;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using Iterators = Neo4Net.Helpers.Collections.Iterators;
	using MapUtil = Neo4Net.Helpers.Collections.MapUtil;
	using IndexReference = Neo4Net.@internal.Kernel.Api.IndexReference;
	using InternalIndexState = Neo4Net.@internal.Kernel.Api.InternalIndexState;
	using NamedToken = Neo4Net.@internal.Kernel.Api.NamedToken;
	using Read = Neo4Net.@internal.Kernel.Api.Read;
	using SchemaRead = Neo4Net.@internal.Kernel.Api.SchemaRead;
	using SchemaReadCore = Neo4Net.@internal.Kernel.Api.SchemaReadCore;
	using TokenRead = Neo4Net.@internal.Kernel.Api.TokenRead;
	using ProcedureException = Neo4Net.@internal.Kernel.Api.exceptions.ProcedureException;
	using IndexNotFoundKernelException = Neo4Net.@internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using ProcedureCallContext = Neo4Net.@internal.Kernel.Api.procs.ProcedureCallContext;
	using ProcedureSignature = Neo4Net.@internal.Kernel.Api.procs.ProcedureSignature;
	using IndexProviderDescriptor = Neo4Net.@internal.Kernel.Api.schema.IndexProviderDescriptor;
	using SchemaDescriptor = Neo4Net.@internal.Kernel.Api.schema.SchemaDescriptor;
	using SecurityContext = Neo4Net.@internal.Kernel.Api.security.SecurityContext;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using ResourceTracker = Neo4Net.Kernel.api.ResourceTracker;
	using Statement = Neo4Net.Kernel.api.Statement;
	using StubResourceManager = Neo4Net.Kernel.api.StubResourceManager;
	using BasicContext = Neo4Net.Kernel.api.proc.BasicContext;
	using Neo4Net.Kernel.api.proc;
	using ConstraintDescriptor = Neo4Net.Kernel.api.schema.constraints.ConstraintDescriptor;
	using ConstraintDescriptorFactory = Neo4Net.Kernel.api.schema.constraints.ConstraintDescriptorFactory;
	using IndexingService = Neo4Net.Kernel.Impl.Api.index.IndexingService;
	using Edition = Neo4Net.Kernel.impl.factory.Edition;
	using Procedures = Neo4Net.Kernel.impl.proc.Procedures;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using Log = Neo4Net.Logging.Log;
	using IndexDescriptorFactory = Neo4Net.Storageengine.Api.schema.IndexDescriptorFactory;
	using PopulationProgress = Neo4Net.Storageengine.Api.schema.PopulationProgress;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.Neo4jTypes.NTNode;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.Neo4jTypes.NTPath;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.Neo4jTypes.NTRelationship;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.index.IndexProvider.EMPTY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.proc.Context_Fields.KERNEL_TRANSACTION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.proc.Context_Fields.SECURITY_CONTEXT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.proc.Key.key;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.schema.SchemaDescriptorFactory.forLabel;

	public class BuiltInProceduresTest
	{
		 private static readonly Key<DependencyResolver> _dependencyResolver = key( "DependencyResolver", typeof( DependencyResolver ) );
		 private static readonly Key<ProcedureCallContext> _callContext = key( "ProcedureCallContext", typeof( ProcedureCallContext ) );
		 private static readonly Key<GraphDatabaseAPI> _graphdatabaseapi = key( "GraphDatabaseAPI", typeof( GraphDatabaseAPI ) );
		 private static readonly Key<Log> _log = key( "Log", typeof( Log ) );

		 private readonly IList<IndexReference> _indexes = new LinkedList<IndexReference>();
		 private readonly IList<IndexReference> _uniqueIndexes = new LinkedList<IndexReference>();
		 private readonly IList<ConstraintDescriptor> _constraints = new LinkedList<ConstraintDescriptor>();
		 private readonly IDictionary<int, string> _labels = new Dictionary<int, string>();
		 private readonly IDictionary<int, string> _propKeys = new Dictionary<int, string>();
		 private readonly IDictionary<int, string> _relTypes = new Dictionary<int, string>();

		 private readonly Read _read = mock( typeof( Read ) );
		 private readonly TokenRead _tokens = mock( typeof( TokenRead ) );
		 private readonly SchemaRead _schemaRead = mock( typeof( SchemaRead ) );
		 private readonly SchemaReadCore _schemaReadCore = mock( typeof( SchemaReadCore ) );
		 private readonly Statement _statement = mock( typeof( Statement ) );
		 private readonly KernelTransaction _tx = mock( typeof( KernelTransaction ) );
		 private readonly DependencyResolver _resolver = mock( typeof( DependencyResolver ) );
		 private readonly GraphDatabaseAPI _graphDatabaseAPI = mock( typeof( GraphDatabaseAPI ) );
		 private readonly IndexingService _indexingService = mock( typeof( IndexingService ) );
		 private readonly Log _log = mock( typeof( Log ) );

		 private readonly Procedures _procs = new Procedures();
		 private readonly ResourceTracker _resourceTracker = new StubResourceManager();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  _procs.registerComponent( typeof( KernelTransaction ), ctx => ctx.get( KERNEL_TRANSACTION ), false );
			  _procs.registerComponent( typeof( DependencyResolver ), ctx => ctx.get( _dependencyResolver ), false );
			  _procs.registerComponent( typeof( GraphDatabaseAPI ), ctx => ctx.get( _graphdatabaseapi ), false );
			  _procs.registerComponent( typeof( SecurityContext ), ctx => ctx.get( SECURITY_CONTEXT ), true );

			  _procs.registerComponent( typeof( Log ), ctx => ctx.get( _log ), false );
			  _procs.registerType( typeof( Node ), NTNode );
			  _procs.registerType( typeof( Relationship ), NTRelationship );
			  _procs.registerType( typeof( Path ), NTPath );

			  ( new SpecialBuiltInProcedures( "1.3.37", Edition.enterprise.ToString() ) ).accept(_procs);
			  _procs.registerProcedure( typeof( BuiltInProcedures ) );
			  _procs.registerProcedure( typeof( BuiltInDbmsProcedures ) );

			  when( _tx.acquireStatement() ).thenReturn(_statement);
			  when( _tx.tokenRead() ).thenReturn(_tokens);
			  when( _tx.dataRead() ).thenReturn(_read);
			  when( _tx.schemaRead() ).thenReturn(_schemaRead);
			  when( _schemaRead.snapshot() ).thenReturn(_schemaReadCore);

			  when( _tokens.propertyKeyGetAllTokens() ).thenAnswer(AsTokens(_propKeys));
			  when( _tokens.labelsGetAllTokens() ).thenAnswer(AsTokens(_labels));
			  when( _tokens.relationshipTypesGetAllTokens() ).thenAnswer(AsTokens(_relTypes));
			  when( _schemaReadCore.indexesGetAll() ).thenAnswer(i => Iterators.concat(_indexes.GetEnumerator(), _uniqueIndexes.GetEnumerator()));
			  when( _schemaReadCore.index( any( typeof( SchemaDescriptor ) ) ) ).thenAnswer((Answer<IndexReference>) invocationOnMock =>
			  {
			  SchemaDescriptor schema = invocationOnMock.getArgument( 0 );
			  int label = Schema.keyId();
			  int prop = Schema.PropertyId;
			  return GetIndexReference( label, prop );
			  });
			  when( _schemaReadCore.constraintsGetAll() ).thenAnswer(i => _constraints.GetEnumerator());

			  when( _tokens.propertyKeyName( anyInt() ) ).thenAnswer(invocation => _propKeys[invocation.getArgument(0)]);
			  when( _tokens.nodeLabelName( anyInt() ) ).thenAnswer(invocation => _labels[invocation.getArgument(0)]);
			  when( _tokens.relationshipTypeName( anyInt() ) ).thenAnswer(invocation => _relTypes[invocation.getArgument(0)]);

			  when( _indexingService.getIndexId( any( typeof( SchemaDescriptor ) ) ) ).thenReturn( 42L );

			  when( _schemaReadCore.constraintsGetForRelationshipType( anyInt() ) ).thenReturn(emptyIterator());
			  when( _schemaReadCore.indexesGetForLabel( anyInt() ) ).thenReturn(emptyIterator());
			  when( _schemaReadCore.indexesGetForRelationshipType( anyInt() ) ).thenReturn(emptyIterator());
			  when( _schemaReadCore.constraintsGetForLabel( anyInt() ) ).thenReturn(emptyIterator());
			  when( _read.countsForNode( anyInt() ) ).thenReturn(1L);
			  when( _read.countsForRelationship( anyInt(), anyInt(), anyInt() ) ).thenReturn(1L);
			  when( _schemaReadCore.indexGetState( any( typeof( IndexReference ) ) ) ).thenReturn( InternalIndexState.ONLINE );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListAllIndexes() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListAllIndexes()
		 {
			  // Given
			  GivenIndex( "User", "name" );

			  // When/Then
			  assertThat( Call( "db.indexes" ), contains( Record( "INDEX ON :User(name)", "Unnamed index", singletonList( "User" ), singletonList( "name" ), "ONLINE", "node_label_property", 100D, GetIndexProviderDescriptorMap( EMPTY.ProviderDescriptor ), 42L, "" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListAllUniqueIndexes() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListAllUniqueIndexes()
		 {
			  // Given
			  GivenUniqueConstraint( "User", "name" );

			  // When/Then
			  assertThat( Call( "db.indexes" ), contains( Record( "INDEX ON :User(name)", "Unnamed index", singletonList( "User" ), singletonList( "name" ), "ONLINE", "node_unique_property", 100D, GetIndexProviderDescriptorMap( EMPTY.ProviderDescriptor ), 42L, "" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void listingIndexesShouldGiveMessageForConcurrentlyDeletedIndexes() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ListingIndexesShouldGiveMessageForConcurrentlyDeletedIndexes()
		 {
			  // Given
			  GivenIndex( "User", "name" );
			  when( _schemaReadCore.indexGetState( any( typeof( IndexReference ) ) ) ).thenThrow( new IndexNotFoundKernelException( "Not found." ) );

			  // When/Then
			  assertThat( Call( "db.indexes" ), contains( Record( "INDEX ON :User(name)", "Unnamed index", singletonList( "User" ), singletonList( "name" ), "NOT FOUND", "node_label_property", 0D, GetIndexProviderDescriptorMap( EMPTY.ProviderDescriptor ), 42L, "Index not found. It might have been concurrently dropped." ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListPropertyKeys() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListPropertyKeys()
		 {
			  // Given
			  GivenPropertyKeys( "name", "age" );

			  // When/Then
			  assertThat( Call( "db.propertyKeys" ), containsInAnyOrder( Record( "age" ), Record( "name" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListLabels() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListLabels()
		 {
			  // Given
			  GivenLabels( "Banana", "Fruit" );

			  // When/Then
			  assertThat( Call( "db.labels" ), containsInAnyOrder( Record( "Banana" ), Record( "Fruit" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListRelTypes() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListRelTypes()
		 {
			  // Given
			  GivenRelationshipTypes( "EATS", "SPROUTS" );

			  // When/Then
			  assertThat( Call( "db.relationshipTypes" ), containsInAnyOrder( Record( "EATS" ), Record( "SPROUTS" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListConstraints() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListConstraints()
		 {
			  // Given
			  GivenUniqueConstraint( "User", "name" );
			  GivenNodePropExistenceConstraint( "User", "name" );
			  GivenNodeKeys( "User", "name" );
			  // When/Then
			  assertThat( Call( "db.constraints" ), containsInAnyOrder( Record( "CONSTRAINT ON ( user:User ) ASSERT exists(user.name)" ), Record( "CONSTRAINT ON ( user:User ) ASSERT user.name IS UNIQUE" ), Record( "CONSTRAINT ON ( user:User ) ASSERT (user.name) IS NODE KEY" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldEscapeLabelNameContainingColons() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldEscapeLabelNameContainingColons()
		 {
			  // Given
			  GivenUniqueConstraint( "FOO:BAR", "x.y" );
			  GivenNodePropExistenceConstraint( "FOO:BAR", "x.y" );

			  // When/Then
			  IList<object[]> call = call( "db.constraints" );
			  assertThat( call, contains( Record( "CONSTRAINT ON ( `foo:bar`:`FOO:BAR` ) ASSERT `foo:bar`.x.y IS UNIQUE" ), Record( "CONSTRAINT ON ( `foo:bar`:`FOO:BAR` ) ASSERT exists(`foo:bar`.x.y)" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListCorrectBuiltinProcedures() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListCorrectBuiltinProcedures()
		 {
			  // When/Then
			  assertThat( Call( "dbms.procedures" ), containsInAnyOrder( Record( "dbms.listConfig", "dbms.listConfig(searchString =  :: STRING?) :: (name :: STRING?, description :: STRING?, value :: STRING?, dynamic :: BOOLEAN?)", "List the currently active config of Neo4j.", "DBMS" ), Record( "db.awaitIndex", "db.awaitIndex(index :: STRING?, timeOutSeconds = 300 :: INTEGER?) :: VOID", "Wait for an index to come online (for example: CALL db.awaitIndex(\":Person(name)\")).", "READ" ), Record( "db.awaitIndexes", "db.awaitIndexes(timeOutSeconds = 300 :: INTEGER?) :: VOID", "Wait for all indexes to come online (for example: CALL db.awaitIndexes(\"500\")).", "READ" ), Record( "db.constraints", "db.constraints() :: (description :: STRING?)", "List all constraints in the database.", "READ" ), Record( "db.indexes", "db.indexes() :: (description :: STRING?, indexName :: STRING?, " + "tokenNames :: LIST? OF STRING?, properties :: LIST? OF STRING?, state :: STRING?, " + "type :: STRING?, progress :: FLOAT?, provider :: MAP?, id :: INTEGER?, failureMessage :: STRING?)", "List all indexes in the database.", "READ" ), Record( "db.labels", "db.labels() :: (label :: STRING?)", "List all labels in the database.", "READ" ), Record( "db.propertyKeys", "db.propertyKeys() :: (propertyKey :: STRING?)", "List all property keys in the database.", "READ" ), Record( "db.relationshipTypes", "db.relationshipTypes() :: (relationshipType :: STRING?)", "List all relationship types in the database.", "READ" ), Record( "db.resampleIndex", "db.resampleIndex(index :: STRING?) :: VOID", "Schedule resampling of an index (for example: CALL db.resampleIndex(\":Person(name)\")).", "READ" ), Record( "db.resampleOutdatedIndexes", "db.resampleOutdatedIndexes() :: VOID", "Schedule resampling of all outdated indexes.", "READ" ), Record( "db.schema", "db.schema() :: (nodes :: LIST? OF NODE?, relationships :: LIST? OF RELATIONSHIP?)", "Show the schema of the data.", "READ" ), Record( "db.schema.visualization", "db.schema.visualization() :: (nodes :: LIST? OF NODE?, relationships :: LIST? OF RELATIONSHIP?)", "Visualize the schema of the data. Replaces db.schema.", "READ" ), Record( "db.schema.nodeTypeProperties", "db.schema.nodeTypeProperties() :: (nodeType :: STRING?, nodeLabels :: LIST? OF STRING?, propertyName :: STRING?, " + "propertyTypes :: LIST? OF STRING?, mandatory :: BOOLEAN?)", "Show the derived property schema of the nodes in tabular form.", "READ" ), Record( "db.schema.relTypeProperties", "db.schema.relTypeProperties() :: (relType :: STRING?, propertyName :: STRING?, propertyTypes :: LIST? OF STRING?," + " mandatory :: BOOLEAN?)", "Show the derived property schema of the relationships in tabular form.", "READ" ), Record( "db.index.explicit.searchNodes", "db.index.explicit.searchNodes(indexName :: STRING?, query :: ANY?) :: (node :: NODE?, weight :: FLOAT?)", "Search nodes in explicit index. Replaces `START n=node:nodes('key:foo*')`", "READ" ), Record( "db.index.explicit.seekNodes", "db.index.explicit.seekNodes(indexName :: STRING?, key :: STRING?, value :: ANY?) :: (node :: NODE?)", "Get node from explicit index. Replaces `START n=node:nodes(key = 'A')`", "READ" ), Record( "db.index.explicit.searchRelationships", "db.index.explicit.searchRelationships(indexName :: STRING?, query :: ANY?) :: " + "(relationship :: RELATIONSHIP?, weight :: FLOAT?)", "Search relationship in explicit index. Replaces `START r=relationship:relIndex('key:foo*')`", "READ" ), Record( "db.index.explicit.searchRelationshipsIn", "db.index.explicit.searchRelationshipsIn(indexName :: STRING?, in :: NODE?, query :: ANY?) :: " + "(relationship :: RELATIONSHIP?, weight :: FLOAT?)", "Search relationship in explicit index, starting at the node 'in'.", "READ" ), Record( "db.index.explicit.searchRelationshipsOut", "db.index.explicit.searchRelationshipsOut(indexName :: STRING?, out :: NODE?, query :: ANY?) :: " + "(relationship :: RELATIONSHIP?, weight :: FLOAT?)", "Search relationship in explicit index, ending at the node 'out'.", "READ" ), Record( "db.index.explicit.searchRelationshipsBetween", "db.index.explicit.searchRelationshipsBetween(indexName :: STRING?, in :: NODE?, out :: NODE?, query :: ANY?) :: " + "(relationship :: RELATIONSHIP?, weight :: FLOAT?)", "Search relationship in explicit index, starting at the node 'in' and ending at 'out'.", "READ" ), Record( "db.index.explicit.seekRelationships", "db.index.explicit.seekRelationships(indexName :: STRING?, key :: STRING?, value :: ANY?) :: " + "(relationship :: RELATIONSHIP?)", "Get relationship from explicit index. Replaces `START r=relationship:relIndex(key = 'A')`", "READ" ), Record( "db.index.explicit.auto.searchNodes", "db.index.explicit.auto.searchNodes(query :: ANY?) :: (node :: NODE?, weight :: FLOAT?)", "Search nodes in explicit automatic index. Replaces `START n=node:node_auto_index('key:foo*')`", "READ" ), Record( "db.index.explicit.auto.seekNodes", "db.index.explicit.auto.seekNodes(key :: STRING?, value :: ANY?) :: (node :: NODE?)", "Get node from explicit automatic index. Replaces `START n=node:node_auto_index(key = 'A')`", "READ" ), Record( "db.index.explicit.auto.searchRelationships", "db.index.explicit.auto.searchRelationships(query :: ANY?) :: (relationship :: RELATIONSHIP?, weight :: FLOAT?)", "Search relationship in explicit automatic index. Replaces `START r=relationship:relationship_auto_index('key:foo*')`", "READ" ), Record( "db.index.explicit.auto.seekRelationships", "db.index.explicit.auto.seekRelationships(key :: STRING?, value :: ANY?) :: " + "(relationship :: RELATIONSHIP?)", "Get relationship from explicit automatic index. Replaces `START r=relationship:relationship_auto_index(key = 'A')`", "READ" ), Record( "db.index.explicit.addNode", "db.index.explicit.addNode(indexName :: STRING?, node :: NODE?, key :: STRING?, value :: ANY?) :: (success :: BOOLEAN?)", "Add a node to an explicit index based on a specified key and value", "WRITE" ), Record( "db.index.explicit.addRelationship", "db.index.explicit.addRelationship(indexName :: STRING?, relationship :: RELATIONSHIP?, key :: STRING?, value :: ANY?) :: " + "(success :: BOOLEAN?)", "Add a relationship to an explicit index based on a specified key and value", "WRITE" ), Record( "db.index.explicit.removeNode", "db.index.explicit.removeNode(indexName :: STRING?, node :: NODE?, " + "key =  <[9895b15e-8693-4a21-a58b-4b7b87e09b8e]>  :: STRING?) :: (success :: BOOLEAN?)", "Remove a node from an explicit index with an optional key", "WRITE" ), Record( "db.index.explicit.removeRelationship", "db.index.explicit.removeRelationship(indexName :: STRING?, relationship :: RELATIONSHIP?, " + "key =  <[9895b15e-8693-4a21-a58b-4b7b87e09b8e]>  :: STRING?) :: " + "(success :: BOOLEAN?)", "Remove a relationship from an explicit index with an optional key", "WRITE" ), Record( "db.index.explicit.drop", "db.index.explicit.drop(indexName :: STRING?) :: " + "(type :: STRING?, name :: STRING?, config :: MAP?)", "Remove an explicit index - YIELD type,name,config", "WRITE" ), Record( "db.index.explicit.forNodes", "db.index.explicit.forNodes(indexName :: STRING?, config = {} :: MAP?) :: " + "(type :: STRING?, name :: STRING?, config :: MAP?)", "Get or create a node explicit index - YIELD type,name,config", "WRITE" ), Record( "db.index.explicit.forRelationships", "db.index.explicit.forRelationships(indexName :: STRING?, config = {} :: MAP?) :: " + "(type :: STRING?, name :: STRING?, config :: MAP?)", "Get or create a relationship explicit index - YIELD type,name,config", "WRITE" ), Record( "db.index.explicit.existsForNodes", "db.index.explicit.existsForNodes(indexName :: STRING?) :: (success :: BOOLEAN?)", "Check if a node explicit index exists", "READ" ), Record( "db.index.explicit.existsForRelationships", "db.index.explicit.existsForRelationships(indexName :: STRING?) :: (success :: BOOLEAN?)", "Check if a relationship explicit index exists", "READ" ), Record( "db.index.explicit.list", "db.index.explicit.list() :: (type :: STRING?, name :: STRING?, config :: MAP?)", "List all explicit indexes - YIELD type,name,config", "READ" ), Record( "dbms.components", "dbms.components() :: (name :: STRING?, versions :: LIST? OF STRING?, edition :: STRING?)", "List DBMS components and their versions.", "DBMS" ), Record( "dbms.procedures", "dbms.procedures() :: (name :: STRING?, signature :: STRING?, description :: STRING?, mode :: STRING?)", "List all procedures in the DBMS.", "DBMS" ), Record( "dbms.functions", "dbms.functions() :: (name :: STRING?, signature :: STRING?, description :: STRING?)", "List all user functions in the DBMS.", "DBMS" ), Record( "dbms.queryJmx", "dbms.queryJmx(query :: STRING?) :: (name :: STRING?, description :: STRING?, attributes :: " + "MAP?)", "Query JMX management data by domain and name. For instance, \"org.neo4j:*\"", "DBMS" ), Record( "dbms.clearQueryCaches", "dbms.clearQueryCaches() :: (value :: STRING?)", "Clears all query caches.", "DBMS" ), Record( "db.createIndex", "db.createIndex(index :: STRING?, providerName :: STRING?) :: (index :: STRING?, providerName :: STRING?, status :: STRING?)", "Create a schema index with specified index provider (for example: CALL db.createIndex(\":Person(name)\", \"lucene+native-2.0\")) - " + "YIELD index, providerName, status", "SCHEMA" ), Record( "db.createUniquePropertyConstraint", "db.createUniquePropertyConstraint(index :: STRING?, providerName :: STRING?) :: " + "(index :: STRING?, providerName :: STRING?, status :: STRING?)", "Create a unique property constraint with index backed by specified index provider " + "(for example: CALL db.createUniquePropertyConstraint(\":Person(name)\", \"lucene+native-2.0\")) - " + "YIELD index, providerName, status", "SCHEMA" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListSystemComponents() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListSystemComponents()
		 {
			  // When/Then
			  assertThat( Call( "dbms.components" ), contains( Record( "Neo4j Kernel", singletonList( "1.3.37" ), "enterprise" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseStatementIfExceptionIsThrownDbLabels()
		 public virtual void ShouldCloseStatementIfExceptionIsThrownDbLabels()
		 {
			  // Given
			  Exception runtimeException = new Exception();
			  when( _tokens.labelsGetAllTokens() ).thenThrow(runtimeException);

			  // When
			  try
			  {
					Call( "db.labels" );
					fail( "Procedure call should have failed" );
			  }
			  catch ( Exception e )
			  {
					assertThat( e.InnerException, @is( runtimeException ) );
					// expected
			  }

			  // Then
			  verify( _statement ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseStatementIfExceptionIsThrownDbPropertyKeys()
		 public virtual void ShouldCloseStatementIfExceptionIsThrownDbPropertyKeys()
		 {
			  // Given
			  Exception runtimeException = new Exception();
			  when( _tokens.propertyKeyGetAllTokens() ).thenThrow(runtimeException);

			  // When
			  try
			  {
					Call( "db.propertyKeys" );
					fail( "Procedure call should have failed" );
			  }
			  catch ( Exception e )
			  {
					assertThat( e.InnerException, @is( runtimeException ) );
					// expected
			  }

			  // Then
			  verify( _statement ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseStatementIfExceptionIsThrownDbRelationshipTypes()
		 public virtual void ShouldCloseStatementIfExceptionIsThrownDbRelationshipTypes()
		 {
			  // Given
			  Exception runtimeException = new Exception();
			  when( _tokens.relationshipTypesGetAllTokens() ).thenThrow(runtimeException);

			  // When
			  try
			  {
					Call( "db.relationshipTypes" );
					fail( "Procedure call should have failed" );
			  }
			  catch ( Exception e )
			  {
					assertThat( e.InnerException, @is( runtimeException ) );
					// expected
			  }

			  // Then
			  verify( _statement ).close();
		 }

		 private static IDictionary<string, string> GetIndexProviderDescriptorMap( IndexProviderDescriptor providerDescriptor )
		 {
			  return MapUtil.stringMap( "key", providerDescriptor.Key, "version", providerDescriptor.Version );
		 }

		 private static Matcher<object[]> Record( params object[] fields )
		 {
			  return equalTo( fields );
		 }

		 private void GivenIndex( string label, string propKey )
		 {
			  int labelId = Token( label, _labels ).Value;
			  int propId = Token( propKey, _propKeys ).Value;

			  IndexReference index = IndexDescriptorFactory.forSchema( forLabel( labelId, propId ), EMPTY.ProviderDescriptor );
			  _indexes.Add( index );
		 }

		 private void GivenUniqueConstraint( string label, string propKey )
		 {
			  int labelId = Token( label, _labels ).Value;
			  int propId = Token( propKey, _propKeys ).Value;

			  IndexReference index = IndexDescriptorFactory.uniqueForSchema( forLabel( labelId, propId ), EMPTY.ProviderDescriptor );
			  _uniqueIndexes.Add( index );
			  _constraints.Add( ConstraintDescriptorFactory.uniqueForLabel( labelId, propId ) );
		 }

		 private void GivenNodePropExistenceConstraint( string label, string propKey )
		 {
			  int labelId = Token( label, _labels ).Value;
			  int propId = Token( propKey, _propKeys ).Value;

			  _constraints.Add( ConstraintDescriptorFactory.existsForLabel( labelId, propId ) );
		 }

		 private void GivenNodeKeys( string label, params string[] props )
		 {
			  int labelId = Token( label, _labels ).Value;
			  int[] propIds = new int[props.Length];
			  for ( int i = 0; i < propIds.Length; i++ )
			  {
					propIds[i] = Token( props[i], _propKeys ).Value;
			  }

			  _constraints.Add( ConstraintDescriptorFactory.nodeKeyForLabel( labelId, propIds ) );
		 }

		 private void GivenPropertyKeys( params string[] keys )
		 {
			  foreach ( string key in keys )
			  {
					Token( key, _propKeys );
			  }
		 }

		 private void GivenLabels( params string[] labelNames )
		 {
			  foreach ( string key in labelNames )
			  {
					Token( key, _labels );
			  }
		 }

		 private void GivenRelationshipTypes( params string[] types )
		 {
			  foreach ( string key in types )
			  {
					Token( key, _relTypes );
			  }
		 }

		 private static int? Token( string name, IDictionary<int, string> tokens )
		 {
			  System.Func<int> allocateFromMap = () =>
			  {
				int newIndex = tokens.Count;
				tokens[newIndex] = name;
				return newIndex;
			  };
			  return tokens.SetOfKeyValuePairs().Where(entry => entry.Value.Equals(name)).Select(DictionaryEntry.getKey).First().orElseGet(allocateFromMap);
		 }

		 private IndexReference GetIndexReference( int label, int prop )
		 {
			  foreach ( IndexReference index in _indexes )
			  {
					if ( index.Schema().EntityTokenIds[0] == label && prop == index.Properties()[0] )
					{
						 return index;
					}
			  }
			  foreach ( IndexReference index in _uniqueIndexes )
			  {
					if ( index.Schema().EntityTokenIds[0] == label && prop == index.Properties()[0] )
					{
						 return index;
					}
			  }
			  throw new AssertionError();
		 }

		 private static Answer<IEnumerator<NamedToken>> AsTokens( IDictionary<int, string> tokens )
		 {
			  return i => tokens.SetOfKeyValuePairs().Select(entry => new NamedToken(entry.Value, entry.Key)).GetEnumerator();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.List<Object[]> call(String name, Object... args) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException, org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 private IList<object[]> Call( string name, params object[] args )
		 {
			  BasicContext ctx = new BasicContext();
			  ctx.Put( KERNEL_TRANSACTION, _tx );
			  ctx.Put( _dependencyResolver, _resolver );
			  ctx.Put( _graphdatabaseapi, _graphDatabaseAPI );
			  ctx.Put( SECURITY_CONTEXT, SecurityContext.AUTH_DISABLED );
			  ctx.Put( _log, _log );
			  when( _graphDatabaseAPI.DependencyResolver ).thenReturn( _resolver );
			  when( _resolver.resolveDependency( typeof( Procedures ) ) ).thenReturn( _procs );
			  when( _resolver.resolveDependency( typeof( IndexingService ) ) ).thenReturn( _indexingService );
			  when( _schemaReadCore.indexGetPopulationProgress( any( typeof( IndexReference ) ) ) ).thenReturn( Neo4Net.Storageengine.Api.schema.PopulationProgress_Fields.Done );
			  return Iterators.asList( _procs.callProcedure( ctx, ProcedureSignature.procedureName( name.Split( "\\.", true ) ), args, _resourceTracker ) );
		 }
	}

}