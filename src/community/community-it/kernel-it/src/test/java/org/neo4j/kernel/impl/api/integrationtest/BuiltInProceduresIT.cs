using System;
using System.Collections.Generic;
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
namespace Neo4Net.Kernel.Impl.Api.integrationtest
{
	using Matcher = org.hamcrest.Matcher;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;


	using Neo4Net.Collections;
	using Resource = Neo4Net.GraphDb.Resource;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using MapUtil = Neo4Net.Collections.Helpers.MapUtil;
	using SchemaWrite = Neo4Net.Kernel.Api.Internal.SchemaWrite;
	using TokenWrite = Neo4Net.Kernel.Api.Internal.TokenWrite;
	using Transaction = Neo4Net.Kernel.Api.Internal.Transaction;
	using ProcedureException = Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
	using ProcedureCallContext = Neo4Net.Kernel.Api.Internal.procs.ProcedureCallContext;
	using ProcedureHandle = Neo4Net.Kernel.Api.Internal.procs.ProcedureHandle;
	using ResourceTracker = Neo4Net.Kernel.api.ResourceTracker;
	using StubResourceManager = Neo4Net.Kernel.api.StubResourceManager;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using LabelSchemaDescriptor = Neo4Net.Kernel.api.schema.LabelSchemaDescriptor;
	using SchemaDescriptorFactory = Neo4Net.Kernel.api.schema.SchemaDescriptorFactory;
	using AnonymousContext = Neo4Net.Kernel.api.security.AnonymousContext;
	using IndexProviderMap = Neo4Net.Kernel.Impl.Api.index.IndexProviderMap;
	using IndexingService = Neo4Net.Kernel.Impl.Api.index.IndexingService;
	using Version = Neo4Net.Kernel.Internal.Version;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.arrayContaining;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.startsWith;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsEqual.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.ProcedureSignature.procedureName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.security.LoginContext.AUTH_DISABLED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.schema.SchemaDescriptorFactory.forLabel;

	public class BuiltInProceduresIT : KernelIntegrationTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException exception = org.junit.rules.ExpectedException.none();
		 public ExpectedException Exception = ExpectedException.none();

		 private readonly ResourceTracker _resourceTracker = new StubResourceManager();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void listAllLabels() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ListAllLabels()
		 {
			  // Given
			  Transaction transaction = NewTransaction( AnonymousContext.WriteToken() );
			  long nodeId = transaction.DataWrite().nodeCreate();
			  int labelId = transaction.TokenWrite().labelGetOrCreateForName("MyLabel");
			  transaction.DataWrite().nodeAddLabel(nodeId, labelId);
			  Commit();

			  // When
			  RawIterator<object[], ProcedureException> stream = Procs().procedureCallRead(Procs().procedureGet(procedureName("db", "labels")).id(), new object[0], ProcedureCallContext.EMPTY);

			  // Then
			  assertThat( asList( stream ), contains( equalTo( new object[]{ "MyLabel" } ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 360_000) public void listAllLabelsMustNotBlockOnConstraintCreatingTransaction() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ListAllLabelsMustNotBlockOnConstraintCreatingTransaction()
		 {
			  // Given
			  Transaction transaction = NewTransaction( AnonymousContext.WriteToken() );
			  long nodeId = transaction.DataWrite().nodeCreate();
			  int labelId = transaction.TokenWrite().labelGetOrCreateForName("MyLabel");
			  int propKey = transaction.TokenWrite().propertyKeyCreateForName("prop");
			  transaction.DataWrite().nodeAddLabel(nodeId, labelId);
			  Commit();

			  System.Threading.CountdownEvent constraintLatch = new System.Threading.CountdownEvent( 1 );
			  System.Threading.CountdownEvent commitLatch = new System.Threading.CountdownEvent( 1 );
			  FutureTask<Void> createConstraintTask = new FutureTask<Void>(() =>
			  {
				SchemaWrite schemaWrite = SchemaWriteInNewTransaction();
				using ( Resource ignore = CaptureTransaction() )
				{
					 schemaWrite.uniquePropertyConstraintCreate( SchemaDescriptorFactory.forLabel( labelId, propKey ) );
					 // We now hold a schema lock on the "MyLabel" label. Let the procedure calling transaction have a go.
					 constraintLatch.Signal();
					 commitLatch.await();
				}
				Rollback();
				return null;
			  });
			  Thread constraintCreator = new Thread( createConstraintTask );
			  constraintCreator.Start();

			  // When
			  constraintLatch.await();
			  RawIterator<object[], ProcedureException> stream = Procs().procedureCallRead(Procs().procedureGet(procedureName("db", "labels")).id(), new object[0], ProcedureCallContext.EMPTY);

			  // Then
			  try
			  {
					assertThat( asList( stream ), contains( equalTo( new object[]{ "MyLabel" } ) ) );
			  }
			  finally
			  {
					commitLatch.Signal();
			  }
			  createConstraintTask.get();
			  constraintCreator.Join();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void listPropertyKeys() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ListPropertyKeys()
		 {
			  // Given
			  TokenWrite ops = TokenWriteInNewTransaction();
			  ops.PropertyKeyGetOrCreateForName( "MyProp" );
			  Commit();

			  // When
			  RawIterator<object[], ProcedureException> stream = Procs().procedureCallRead(Procs().procedureGet(procedureName("db", "propertyKeys")).id(), new object[0], ProcedureCallContext.EMPTY);

			  // Then
			  assertThat( asList( stream ), contains( equalTo( new object[]{ "MyProp" } ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void listRelationshipTypes() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ListRelationshipTypes()
		 {
			  // Given
			  Transaction transaction = NewTransaction( AnonymousContext.WriteToken() );
			  int relType = transaction.TokenWrite().relationshipTypeGetOrCreateForName("MyRelType");
			  long startNodeId = transaction.DataWrite().nodeCreate();
			  long endNodeId = transaction.DataWrite().nodeCreate();
			  transaction.DataWrite().relationshipCreate(startNodeId, relType, endNodeId);
			  Commit();

			  // When
			  RawIterator<object[], ProcedureException> stream = Procs().procedureCallRead(Procs().procedureGet(procedureName("db", "relationshipTypes")).id(), new object[0], ProcedureCallContext.EMPTY);

			  // Then
			  assertThat( asList( stream ), contains( equalTo( new object[]{ "MyRelType" } ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void listProcedures() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ListProcedures()
		 {
			  // When
			  ProcedureHandle procedures = Procs().procedureGet(procedureName("dbms", "procedures"));
			  RawIterator<object[], ProcedureException> stream = Procs().procedureCallRead(procedures.Id(), new object[0], ProcedureCallContext.EMPTY);

			  // Then
			  //noinspection unchecked
			  assertThat( asList( stream ), containsInAnyOrder( Proc( "dbms.listConfig", "(searchString =  :: STRING?) :: (name :: STRING?, description :: STRING?, value :: STRING?, dynamic :: BOOLEAN?)", "List the currently active config of Neo4Net.", "DBMS" ), Proc( "db.constraints", "() :: (description :: STRING?)", "List all constraints in the database.", "READ" ), Proc( "db.indexes", "() :: (description :: STRING?, indexName :: STRING?, tokenNames :: LIST? OF STRING?, properties :: " + "LIST? OF STRING?, state :: STRING?, type :: STRING?, progress :: FLOAT?, provider :: MAP?, id :: INTEGER?, " + "failureMessage :: STRING?)", "List all indexes in the database.", "READ" ), Proc( "db.awaitIndex", "(index :: STRING?, timeOutSeconds = 300 :: INTEGER?) :: VOID", "Wait for an index to come online (for example: CALL db.awaitIndex(\":Person(name)\")).", "READ" ), Proc( "db.awaitIndexes", "(timeOutSeconds = 300 :: INTEGER?) :: VOID", "Wait for all indexes to come online (for example: CALL db.awaitIndexes(\"500\")).", "READ" ), Proc( "db.resampleIndex", "(index :: STRING?) :: VOID", "Schedule resampling of an index (for example: CALL db.resampleIndex(\":Person(name)\")).", "READ" ), Proc( "db.resampleOutdatedIndexes", "() :: VOID", "Schedule resampling of all outdated indexes.", "READ" ), Proc( "db.propertyKeys", "() :: (propertyKey :: STRING?)", "List all property keys in the database.", "READ" ), Proc( "db.labels", "() :: (label :: STRING?)", "List all labels in the database.", "READ" ), Proc( "db.schema", "() :: (nodes :: LIST? OF NODE?, relationships :: LIST? " + "OF " + "RELATIONSHIP?)", "Show the schema of the data.", "READ" ), Proc( "db.schema.visualization","() :: (nodes :: LIST? OF NODE?, relationships :: LIST? OF RELATIONSHIP?)", "Visualize the schema of the data. Replaces db.schema.", "READ" ), Proc( "db.schema.nodeTypeProperties", "() :: (nodeType :: STRING?, nodeLabels :: LIST? OF STRING?, propertyName :: STRING?, " + "propertyTypes :: LIST? OF STRING?, mandatory :: BOOLEAN?)", "Show the derived property schema of the nodes in tabular form.", "READ" ), Proc( "db.schema.relTypeProperties", "() :: (relType :: STRING?, " + "propertyName :: STRING?, propertyTypes :: LIST? OF STRING?, mandatory :: BOOLEAN?)", "Show the derived property schema of the relationships in tabular form.", "READ" ), Proc( "db.relationshipTypes", "() :: (relationshipType :: " + "STRING?)", "List all relationship types in the database.", "READ" ), Proc( "dbms.procedures", "() :: (name :: STRING?, signature :: " + "STRING?, description :: STRING?, mode :: STRING?)", "List all procedures in the DBMS.", "DBMS" ), Proc( "dbms.functions", "() :: (name :: STRING?, signature :: " + "STRING?, description :: STRING?)", "List all user functions in the DBMS.", "DBMS" ), Proc( "dbms.components", "() :: (name :: STRING?, versions :: LIST? OF" + " STRING?, edition :: STRING?)", "List DBMS components and their versions.", "DBMS" ), Proc( "dbms.queryJmx", "(query :: STRING?) :: (name :: STRING?, " + "description :: STRING?, attributes :: MAP?)", "Query JMX management data by domain and name." + " For instance, \"org.Neo4Net:*\"", "DBMS" ), Proc( "db.createLabel", "(newLabel :: STRING?) :: VOID", "Create a label", "WRITE" ), Proc( "db.createProperty", "(newProperty :: STRING?) :: VOID", "Create a Property", "WRITE" ), Proc( "db.createRelationshipType", "(newRelationshipType :: STRING?) :: VOID", "Create a RelationshipType", "WRITE" ), Proc( "db.index.explicit.searchNodes", "(indexName :: STRING?, query :: ANY?) :: (node :: NODE?, weight :: FLOAT?)", "Search nodes in explicit index. Replaces `START n=node:nodes('key:foo*')`", "READ" ), Proc( "db.index.explicit.seekNodes", "(indexName :: STRING?, key :: STRING?, value :: ANY?) :: (node :: NODE?)", "Get node from explicit index. Replaces `START n=node:nodes(key = 'A')`", "READ" ), Proc( "db.index.explicit.searchRelationships", "(indexName :: STRING?, query :: ANY?) :: (relationship :: RELATIONSHIP?, weight :: FLOAT?)", "Search relationship in explicit index. Replaces `START r=relationship:relIndex('key:foo*')`", "READ" ), Proc( "db.index.explicit.auto.searchNodes", "(query :: ANY?) :: (node :: NODE?, weight :: FLOAT?)", "Search nodes in explicit automatic index. Replaces `START n=node:node_auto_index('key:foo*')`", "READ" ), Proc( "db.index.explicit.auto.seekNodes", "(key :: STRING?, value :: ANY?) :: (node :: NODE?)", "Get node from explicit automatic index. Replaces `START n=node:node_auto_index(key = 'A')`", "READ" ), Proc( "db.index.explicit.auto.searchRelationships", "(query :: ANY?) :: (relationship :: RELATIONSHIP?, weight :: FLOAT?)", "Search relationship in explicit automatic index. Replaces `START r=relationship:relationship_auto_index('key:foo*')`", "READ" ), Proc( "db.index.explicit.auto.seekRelationships", "(key :: STRING?, value :: ANY?) :: " + "(relationship :: RELATIONSHIP?)", "Get relationship from explicit automatic index. Replaces `START r=relationship:relationship_auto_index(key = 'A')`", "READ" ), Proc( "db.index.explicit.addNode", "(indexName :: STRING?, node :: NODE?, key :: STRING?, value :: ANY?) :: (success :: BOOLEAN?)", "Add a node to an explicit index based on a specified key and value", "WRITE" ), Proc( "db.index.explicit.addRelationship", "(indexName :: STRING?, relationship :: RELATIONSHIP?, key :: STRING?, value :: ANY?) :: " + "(success :: BOOLEAN?)", "Add a relationship to an explicit index based on a specified key and value", "WRITE" ), Proc( "db.index.explicit.removeNode", "(indexName :: STRING?, node :: NODE?, key =  <[9895b15e-8693-4a21-a58b-4b7b87e09b8e]>  :: STRING?) " + ":: (success :: BOOLEAN?)", "Remove a node from an explicit index with an optional key", "WRITE" ), Proc( "db.index.explicit.removeRelationship", "(indexName :: STRING?, relationship :: RELATIONSHIP?, " + "key =  <[9895b15e-8693-4a21-a58b-4b7b87e09b8e]>  :: STRING?) :: (success :: BOOLEAN?)", "Remove a relationship from an explicit index with an optional key", "WRITE" ), Proc( "db.index.explicit.drop", "(indexName :: STRING?) :: (type :: STRING?, name :: STRING?, config :: MAP?)", "Remove an explicit index - YIELD type,name,config", "WRITE" ), Proc( "db.index.explicit.forNodes", "(indexName :: STRING?, config = {} :: MAP?) :: (type :: STRING?, name :: STRING?, config :: MAP?)", "Get or create a node explicit index - YIELD type,name,config", "WRITE" ), Proc( "db.index.explicit.forRelationships", "(indexName :: STRING?, config = {} :: MAP?) :: " + "(type :: STRING?, name :: STRING?, config :: MAP?)", "Get or create a relationship explicit index - YIELD type,name,config", "WRITE" ), Proc( "db.index.explicit.existsForNodes", "(indexName :: STRING?) :: (success :: BOOLEAN?)", "Check if a node explicit index exists", "READ" ), Proc( "db.index.explicit.existsForRelationships", "(indexName :: STRING?) :: (success :: BOOLEAN?)", "Check if a relationship explicit index exists", "READ" ), Proc( "db.index.explicit.list", "() :: (type :: STRING?, name :: STRING?, config :: MAP?)", "List all explicit indexes - YIELD type,name,config", "READ" ), Proc( "db.index.explicit.seekRelationships", "(indexName :: STRING?, key :: STRING?, value :: ANY?) :: (relationship :: RELATIONSHIP?)", "Get relationship from explicit index. Replaces `START r=relationship:relIndex(key = 'A')`", "READ" ), Proc( "db.index.explicit.searchRelationshipsBetween", "(indexName :: STRING?, in :: NODE?, out :: NODE?, query :: ANY?) :: " + "(relationship :: RELATIONSHIP?, weight :: FLOAT?)", "Search relationship in explicit index, starting at the node 'in' and ending at 'out'.", "READ" ), Proc( "db.index.explicit.searchRelationshipsIn", "(indexName :: STRING?, in :: NODE?, query :: ANY?) :: " + "(relationship :: RELATIONSHIP?, weight :: FLOAT?)", "Search relationship in explicit index, starting at the node 'in'.", "READ" ), Proc( "db.index.explicit.searchRelationshipsOut", "(indexName :: STRING?, out :: NODE?, query :: ANY?) :: " + "(relationship :: RELATIONSHIP?, weight :: FLOAT?)", "Search relationship in explicit index, ending at the node 'out'.", "READ" ), Proc( "dbms.clearQueryCaches", "() :: (value :: STRING?)", "Clears all query caches.", "DBMS" ), Proc( "db.createIndex", "(index :: STRING?, providerName :: STRING?) :: (index :: STRING?, providerName :: STRING?, status :: STRING?)", "Create a schema index with specified index provider (for example: CALL db.createIndex(\":Person(name)\", \"lucene+native-2.0\")) - " + "YIELD index, providerName, status", "SCHEMA" ), Proc( "db.createUniquePropertyConstraint", "(index :: STRING?, providerName :: STRING?) :: " + "(index :: STRING?, providerName :: STRING?, status :: STRING?)", "Create a unique property constraint with index backed by specified index provider " + "(for example: CALL db.createUniquePropertyConstraint(\":Person(name)\", \"lucene+native-2.0\")) - " + "YIELD index, providerName, status", "SCHEMA" ), Proc( "db.index.fulltext.awaitEventuallyConsistentIndexRefresh", "() :: VOID", "Wait for the updates from recently committed transactions to be applied to any eventually-consistent fulltext indexes.", "READ" ), Proc( "db.index.fulltext.awaitIndex", "(index :: STRING?, timeOutSeconds = 300 :: INTEGER?) :: VOID", "Similar to db.awaitIndex(index, timeout), except instead of an index pattern, the index is specified by name. " + "The name can be quoted by backticks, if necessary.", "READ" ), Proc( "db.index.fulltext.createNodeIndex", "(indexName :: STRING?, labels :: LIST? OF STRING?, propertyNames :: LIST? OF STRING?, " + "config = {} :: MAP?) :: VOID", startsWith( "Create a node fulltext index for the given labels and properties." ), "SCHEMA" ), Proc( "db.index.fulltext.createRelationshipIndex", "(indexName :: STRING?, relationshipTypes :: LIST? OF STRING?, propertyNames :: LIST? OF STRING?, config = {} :: MAP?) :: VOID", startsWith( "Create a relationship fulltext index for the given relationship types and properties." ), "SCHEMA" ), Proc( "db.index.fulltext.drop", "(indexName :: STRING?) :: VOID", "Drop the specified index.", "SCHEMA" ), Proc( "db.index.fulltext.listAvailableAnalyzers", "() :: (analyzer :: STRING?, description :: STRING?)", "List the available analyzers that the fulltext indexes can be configured with.", "READ" ), Proc( "db.index.fulltext.queryNodes", "(indexName :: STRING?, queryString :: STRING?) :: (node :: NODE?, score :: FLOAT?)", "Query the given fulltext index. Returns the matching nodes and their lucene query score, ordered by score.", "READ" ), Proc( "db.index.fulltext.queryRelationships", "(indexName :: STRING?, queryString :: STRING?) :: (relationship :: RELATIONSHIP?, " + "score :: FLOAT?)", "Query the given fulltext index. Returns the matching relationships and their lucene query score, ordered by " + "score.", "READ" ), Proc( "db.stats.retrieve", "(section :: STRING?, config = {} :: MAP?) :: (section :: STRING?, data :: MAP?)", "Retrieve statistical data about the current database. Valid sections are 'GRAPH COUNTS', 'TOKENS', 'QUERIES', 'META'", "READ" ), Proc( "db.stats.retrieveAllAnonymized", "(graphToken :: STRING?, config = {} :: MAP?) :: (section :: STRING?, data :: MAP?)", "Retrieve all available statistical data about the current database, in an anonymized form.", "READ" ), Proc( "db.stats.status", "() :: (section :: STRING?, status :: STRING?, data :: MAP?)", "Retrieve the status of all available collector daemons, for this database.", "READ" ), Proc( "db.stats.collect", "(section :: STRING?, config = {} :: MAP?) :: (section :: STRING?, success :: BOOLEAN?, message :: STRING?)", "Start data collection of a given data section. Valid sections are 'QUERIES'", "READ" ), Proc( "db.stats.stop", "(section :: STRING?) :: (section :: STRING?, success :: BOOLEAN?, message :: STRING?)", "Stop data collection of a given data section. Valid sections are 'QUERIES'", "READ" ), Proc( "db.stats.clear", "(section :: STRING?) :: (section :: STRING?, success :: BOOLEAN?, message :: STRING?)", "Clear collected data of a given data section. Valid sections are 'QUERIES'", "READ" ) ) );
			  Commit();
		 }

		 private Matcher<object[]> Proc( string procName, string procSignature, string description, string mode )
		 {
			  return equalTo( new object[]{ procName, procName + procSignature, description, mode } );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings({"unchecked", "TypeParameterExplicitlyExtendsObject"}) private org.hamcrest.Matcher<Object[]> proc(String procName, String procSignature, org.hamcrest.Matcher<String> description, String mode)
		 private Matcher<object[]> Proc( string procName, string procSignature, Matcher<string> description, string mode )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.hamcrest.Matcher<Object> desc = (org.hamcrest.Matcher<Object>)(org.hamcrest.Matcher<? extends Object>) description;
			  Matcher<object> desc = ( Matcher<object> )( Matcher<object> ) description;
			  Matcher<object>[] matchers = new Matcher[]{ equalTo( procName ), equalTo( procName + procSignature ), desc, equalTo( mode ) };
			  return arrayContaining( matchers );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void failWhenCallingNonExistingProcedures()
		 public virtual void FailWhenCallingNonExistingProcedures()
		 {
			  try
			  {
					DbmsOperations().procedureCallDbms(procedureName("dbms", "iDoNotExist"), new object[0], DependencyResolver, AnonymousContext.none().authorize(s => -1, GraphDatabaseSettings.DEFAULT_DATABASE_NAME), _resourceTracker, ProcedureCallContext.EMPTY);
					fail( "This should never get here" );
			  }
			  catch ( Exception e )
			  {
					// Then
					assertThat( e.GetType(), equalTo(typeof(ProcedureException)) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void listAllComponents() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ListAllComponents()
		 {
			  // Given a running database

			  // When
			  RawIterator<object[], ProcedureException> stream = Procs().procedureCallRead(Procs().procedureGet(procedureName("dbms", "components")).id(), new object[0], ProcedureCallContext.EMPTY);

			  // Then
			  assertThat( asList( stream ), contains( equalTo( new object[]{ "Neo4Net Kernel", singletonList( Version.Neo4NetVersion ), "community" } ) ) );

			  Commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void listAllIndexes() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ListAllIndexes()
		 {
			  // Given
			  Transaction transaction = NewTransaction( AUTH_DISABLED );
			  int labelId1 = transaction.TokenWrite().labelGetOrCreateForName("Person");
			  int labelId2 = transaction.TokenWrite().labelGetOrCreateForName("Age");
			  int propertyKeyId1 = transaction.TokenWrite().propertyKeyGetOrCreateForName("foo");
			  int propertyKeyId2 = transaction.TokenWrite().propertyKeyGetOrCreateForName("bar");
			  LabelSchemaDescriptor personFooDescriptor = forLabel( labelId1, propertyKeyId1 );
			  LabelSchemaDescriptor ageFooDescriptor = forLabel( labelId2, propertyKeyId1 );
			  LabelSchemaDescriptor personFooBarDescriptor = forLabel( labelId1, propertyKeyId1, propertyKeyId2 );
			  transaction.SchemaWrite().indexCreate(personFooDescriptor);
			  transaction.SchemaWrite().uniquePropertyConstraintCreate(ageFooDescriptor);
			  transaction.SchemaWrite().indexCreate(personFooBarDescriptor);
			  Commit();

			  //let indexes come online
			  using ( Neo4Net.GraphDb.Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(2, MINUTES);
					tx.Success();
			  }

			  // When
			  RawIterator<object[], ProcedureException> stream = Procs().procedureCallRead(Procs().procedureGet(procedureName("db", "indexes")).id(), new object[0], ProcedureCallContext.EMPTY);

			  ISet<object[]> result = new HashSet<object[]>();
			  while ( stream.HasNext() )
			  {
					result.Add( stream.Next() );
			  }

			  // Then
			  IndexProviderMap indexProviderMap = Db.DependencyResolver.resolveDependency( typeof( IndexProviderMap ) );
			  IndexingService indexingService = Db.DependencyResolver.resolveDependency( typeof( IndexingService ) );
			  IndexProvider provider = indexProviderMap.DefaultProvider;
			  IDictionary<string, string> pdm = MapUtil.stringMap( "key", provider.ProviderDescriptor.Key, "version", provider.ProviderDescriptor.Version );
			  assertThat( result, containsInAnyOrder( new object[]{ "INDEX ON :Age(foo)", "index_1", singletonList( "Age" ), singletonList( "foo" ), "ONLINE", "node_unique_property", 100D, pdm, indexingService.GetIndexId( ageFooDescriptor ), "" }, new object[]{ "INDEX ON :Person(foo)", "Unnamed index", singletonList( "Person" ), singletonList( "foo" ), "ONLINE", "node_label_property", 100D, pdm, indexingService.GetIndexId( personFooDescriptor ), "" }, new object[]{ "INDEX ON :Person(foo, bar)", "Unnamed index", singletonList( "Person" ), Arrays.asList( "foo", "bar" ), "ONLINE", "node_label_property", 100D, pdm, indexingService.GetIndexId( personFooBarDescriptor ), "" } ) );
			  Commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 360_000) public void listAllIndexesMustNotBlockOnConstraintCreatingTransaction() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ListAllIndexesMustNotBlockOnConstraintCreatingTransaction()
		 {
			  // Given
			  Transaction transaction = NewTransaction( AUTH_DISABLED );
			  int labelId1 = transaction.TokenWrite().labelGetOrCreateForName("Person");
			  int labelId2 = transaction.TokenWrite().labelGetOrCreateForName("Age");
			  int propertyKeyId1 = transaction.TokenWrite().propertyKeyGetOrCreateForName("foo");
			  int propertyKeyId2 = transaction.TokenWrite().propertyKeyGetOrCreateForName("bar");
			  int propertyKeyId3 = transaction.TokenWrite().propertyKeyGetOrCreateForName("baz");
			  LabelSchemaDescriptor personFooDescriptor = forLabel( labelId1, propertyKeyId1 );
			  LabelSchemaDescriptor ageFooDescriptor = forLabel( labelId2, propertyKeyId1 );
			  LabelSchemaDescriptor personFooBarDescriptor = forLabel( labelId1, propertyKeyId1, propertyKeyId2 );
			  LabelSchemaDescriptor personBazDescriptor = forLabel( labelId1, propertyKeyId3 );
			  transaction.SchemaWrite().indexCreate(personFooDescriptor);
			  transaction.SchemaWrite().uniquePropertyConstraintCreate(ageFooDescriptor);
			  transaction.SchemaWrite().indexCreate(personFooBarDescriptor);
			  Commit();

			  //let indexes come online
			  using ( Neo4Net.GraphDb.Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(2, MINUTES);
					tx.Success();
			  }

			  System.Threading.CountdownEvent constraintLatch = new System.Threading.CountdownEvent( 1 );
			  System.Threading.CountdownEvent commitLatch = new System.Threading.CountdownEvent( 1 );
			  FutureTask<Void> createConstraintTask = new FutureTask<Void>(() =>
			  {
				SchemaWrite schemaWrite = SchemaWriteInNewTransaction();
				using ( Resource ignore = CaptureTransaction() )
				{
					 schemaWrite.uniquePropertyConstraintCreate( SchemaDescriptorFactory.forLabel( labelId1, propertyKeyId3 ) );
					 // We now hold a schema lock on the "MyLabel" label. Let the procedure calling transaction have a go.
					 constraintLatch.Signal();
					 commitLatch.await();
				}
				Rollback();
				return null;
			  });
			  Thread constraintCreator = new Thread( createConstraintTask );
			  constraintCreator.Start();

			  // When
			  constraintLatch.await();
			  RawIterator<object[], ProcedureException> stream = Procs().procedureCallRead(Procs().procedureGet(procedureName("db", "indexes")).id(), new object[0], ProcedureCallContext.EMPTY);

			  ISet<object[]> result = new HashSet<object[]>();
			  while ( stream.HasNext() )
			  {
					result.Add( stream.Next() );
			  }

			  // Then
			  try
			  {
					IndexProviderMap indexProviderMap = Db.DependencyResolver.resolveDependency( typeof( IndexProviderMap ) );
					IndexingService indexing = Db.DependencyResolver.resolveDependency( typeof( IndexingService ) );
					IndexProvider provider = indexProviderMap.DefaultProvider;
					IDictionary<string, string> pdm = MapUtil.stringMap( "key", provider.ProviderDescriptor.Key, "version", provider.ProviderDescriptor.Version );
					assertThat( result, containsInAnyOrder( new object[]{ "INDEX ON :Age(foo)", "index_1", singletonList( "Age" ), singletonList( "foo" ), "ONLINE", "node_unique_property", 100D, pdm, indexing.GetIndexId( ageFooDescriptor ), "" }, new object[]{ "INDEX ON :Person(foo)", "Unnamed index", singletonList( "Person" ), singletonList( "foo" ), "ONLINE", "node_label_property", 100D, pdm, indexing.GetIndexId( personFooDescriptor ), "" }, new object[]{ "INDEX ON :Person(foo, bar)", "Unnamed index", singletonList( "Person" ), Arrays.asList( "foo", "bar" ), "ONLINE", "node_label_property", 100D, pdm, indexing.GetIndexId( personFooBarDescriptor ), "" }, new object[]{ "INDEX ON :Person(baz)", "Unnamed index", singletonList( "Person" ), singletonList( "baz" ), "POPULATING", "node_unique_property", 100D, pdm, indexing.GetIndexId( personBazDescriptor ), "" } ) );
					Commit();
			  }
			  finally
			  {
					commitLatch.Signal();
			  }
			  createConstraintTask.get();
			  constraintCreator.Join();
		 }
	}

}