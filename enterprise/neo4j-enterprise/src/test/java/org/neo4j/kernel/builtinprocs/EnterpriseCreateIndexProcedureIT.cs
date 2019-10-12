using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
namespace Org.Neo4j.Kernel.builtinprocs
{
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using Org.Neo4j.Collection;
	using ConstraintViolationException = Org.Neo4j.Graphdb.ConstraintViolationException;
	using Label = Org.Neo4j.Graphdb.Label;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using IndexOrder = Org.Neo4j.@internal.Kernel.Api.IndexOrder;
	using IndexQuery = Org.Neo4j.@internal.Kernel.Api.IndexQuery;
	using IndexReference = Org.Neo4j.@internal.Kernel.Api.IndexReference;
	using NodeValueIndexCursor = Org.Neo4j.@internal.Kernel.Api.NodeValueIndexCursor;
	using SchemaRead = Org.Neo4j.@internal.Kernel.Api.SchemaRead;
	using TokenRead = Org.Neo4j.@internal.Kernel.Api.TokenRead;
	using Transaction = Org.Neo4j.@internal.Kernel.Api.Transaction;
	using KernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.KernelException;
	using ProcedureException = Org.Neo4j.@internal.Kernel.Api.exceptions.ProcedureException;
	using TransactionFailureException = Org.Neo4j.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using IllegalTokenNameException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.IllegalTokenNameException;
	using ProcedureCallContext = Org.Neo4j.@internal.Kernel.Api.procs.ProcedureCallContext;
	using ProcedureSignature = Org.Neo4j.@internal.Kernel.Api.procs.ProcedureSignature;
	using IndexEntryConflictException = Org.Neo4j.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using UniquePropertyValueValidationException = Org.Neo4j.Kernel.Api.Exceptions.schema.UniquePropertyValueValidationException;
	using AnonymousContext = Org.Neo4j.Kernel.api.security.AnonymousContext;
	using KernelIntegrationTest = Org.Neo4j.Kernel.Impl.Api.integrationtest.KernelIntegrationTest;
	using TestEnterpriseGraphDatabaseFactory = Org.Neo4j.Test.TestEnterpriseGraphDatabaseFactory;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using TextValue = Org.Neo4j.Values.Storable.TextValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.allOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assume.assumeThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.SchemaIndex.NATIVE10;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Exceptions.rootCause;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringValue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class EnterpriseCreateIndexProcedureIT extends org.neo4j.kernel.impl.api.integrationtest.KernelIntegrationTest
	public class EnterpriseCreateIndexProcedureIT : KernelIntegrationTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{2}") public static java.util.Collection<Object[]> parameters()
		 public static ICollection<object[]> Parameters()
		 {
			  return Arrays.asList( new object[]{ false, false, "createIndex", "index created" }, new object[]{ false, true, "createUniquePropertyConstraint", "uniqueness constraint online" }, new object[]{ true, true, "createNodeKey", "node key constraint online" } );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter() public static boolean existenceConstraint;
		 public static bool ExistenceConstraint;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(1) public static boolean uniquenessConstraint;
		 public static bool UniquenessConstraint;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(2) public static String indexProcedureName;
		 public static string IndexProcedureName;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(3) public static String expectedSuccessfulCreationStatus;
		 public static string ExpectedSuccessfulCreationStatus;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createIndexWithGivenProvider() throws org.neo4j.internal.kernel.api.exceptions.KernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CreateIndexWithGivenProvider()
		 {
			  TestCreateIndexWithGivenProvider( "Person", "name" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createIndexWithGivenProviderComposite() throws org.neo4j.internal.kernel.api.exceptions.KernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CreateIndexWithGivenProviderComposite()
		 {
			  TestCreateIndexWithGivenProvider( "NinjaTurtle", "favoritePizza", "favoriteBrother" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateNonExistingLabelAndPropertyToken() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateNonExistingLabelAndPropertyToken()
		 {
			  // given
			  string label = "MyLabel";
			  string propKey = "myKey";
			  Transaction transaction = NewTransaction( AnonymousContext.read() );
			  assertEquals( "label token should not exist", Org.Neo4j.@internal.Kernel.Api.TokenRead_Fields.NO_TOKEN, transaction.TokenRead().nodeLabel(label) );
			  assertEquals( "property token should not exist", Org.Neo4j.@internal.Kernel.Api.TokenRead_Fields.NO_TOKEN, transaction.TokenRead().propertyKey(propKey) );
			  Commit();

			  // when
			  NewTransaction( AnonymousContext.full() );
			  CallIndexProcedure( IndexPattern( label, propKey ), GraphDatabaseSettings.SchemaIndex.NATIVE20.providerName() );
			  Commit();

			  // then
			  transaction = NewTransaction( AnonymousContext.read() );
			  assertNotEquals( "label token should exist", Org.Neo4j.@internal.Kernel.Api.TokenRead_Fields.NO_TOKEN, transaction.TokenRead().nodeLabel(label) );
			  assertNotEquals( "property token should exist", Org.Neo4j.@internal.Kernel.Api.TokenRead_Fields.NO_TOKEN, transaction.TokenRead().propertyKey(propKey) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void throwIfNullProvider() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ThrowIfNullProvider()
		 {
			  // given
			  Transaction transaction = NewTransaction( AnonymousContext.writeToken() );
			  transaction.TokenWrite().labelGetOrCreateForName("Person");
			  CreateProperties( transaction, "name" );
			  Commit();

			  // when
			  NewTransaction( AnonymousContext.full() );
			  string pattern = IndexPattern( "Person", "name" );
			  ProcedureException e = assertThrows( typeof( ProcedureException ), () => CallIndexProcedure(pattern, null) );

			  // then
			  assertThat( e.Message, containsString( "Could not create index with specified index provider being null" ) );
			  Commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void throwIfNonExistingProvider() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ThrowIfNonExistingProvider()
		 {
			  // given
			  Transaction transaction = NewTransaction( AnonymousContext.writeToken() );
			  transaction.TokenWrite().labelGetOrCreateForName("Person");
			  CreateProperties( transaction, "name" );
			  Commit();

			  // when
			  NewTransaction( AnonymousContext.full() );
			  string pattern = IndexPattern( "Person", "name" );
			  try
			  {
					CallIndexProcedure( pattern, "non+existing-1.0" );
					fail( "Expected to fail" );
			  }
			  catch ( ProcedureException e )
			  {
					// then
					assertThat( e.Message, allOf( containsString( "Failed to invoke procedure" ), containsString( "Tried to get index provider" ), containsString( "available providers in this session being" ), containsString( "default being" ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void throwIfIndexAlreadyExists() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ThrowIfIndexAlreadyExists()
		 {
			  // given
			  string label = "Superhero";
			  string propertyKey = "primaryPower";
			  using ( Org.Neo4j.Graphdb.Transaction tx = Db.beginTx() )
			  {
					Db.schema().indexFor(Label.label(label)).on(propertyKey).create();
					tx.Success();
			  }
			  AwaitIndexOnline();

			  // when
			  NewTransaction( AnonymousContext.full() );
			  string pattern = IndexPattern( label, propertyKey );
			  try
			  {
					CallIndexProcedure( pattern, GraphDatabaseSettings.SchemaIndex.NATIVE20.providerName() );
					fail( "Should have failed" );
			  }
			  catch ( ProcedureException e )
			  {
					// then
					assertThat( e.Message, containsString( "There already exists an index " ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int[] createProperties(org.neo4j.internal.kernel.api.Transaction transaction, String... properties) throws org.neo4j.internal.kernel.api.exceptions.schema.IllegalTokenNameException
		 private int[] CreateProperties( Transaction transaction, params string[] properties )
		 {
			  int[] propertyKeyIds = new int[properties.Length];
			  for ( int i = 0; i < properties.Length; i++ )
			  {
					propertyKeyIds[i] = transaction.TokenWrite().propertyKeyGetOrCreateForName(properties[i]);
			  }
			  return propertyKeyIds;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long createNodeWithPropertiesAndLabel(org.neo4j.internal.kernel.api.Transaction transaction, int labelId, int[] propertyKeyIds, org.neo4j.values.storable.TextValue value) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 private long CreateNodeWithPropertiesAndLabel( Transaction transaction, int labelId, int[] propertyKeyIds, TextValue value )
		 {
			  long node = transaction.DataWrite().nodeCreate();
			  transaction.DataWrite().nodeAddLabel(node, labelId);
			  foreach ( int propertyKeyId in propertyKeyIds )
			  {
					transaction.DataWrite().nodeSetProperty(node, propertyKeyId, value);
			  }
			  return node;
		 }

		 private string IndexPattern( string label, params string[] properties )
		 {
			  StringJoiner pattern = new StringJoiner( ",", ":" + label + "(", ")" );
			  foreach ( string property in properties )
			  {
					pattern.add( property );
			  }
			  return pattern.ToString();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.collection.RawIterator<Object[],org.neo4j.internal.kernel.api.exceptions.ProcedureException> callIndexProcedure(String pattern, String specifiedProvider) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException, org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 private RawIterator<object[], ProcedureException> CallIndexProcedure( string pattern, string specifiedProvider )
		 {
			  return ProcsSchema().procedureCallSchema(ProcedureSignature.procedureName("db", IndexProcedureName), new object[] { pattern, specifiedProvider }, ProcedureCallContext.EMPTY);
		 }

		 private void AwaitIndexOnline()
		 {
			  using ( Org.Neo4j.Graphdb.Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(10, TimeUnit.SECONDS);
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void testCreateIndexWithGivenProvider(String label, String... properties) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 private void TestCreateIndexWithGivenProvider( string label, params string[] properties )
		 {
			  // given
			  Transaction transaction = NewTransaction( AnonymousContext.writeToken() );
			  int labelId = transaction.TokenWrite().labelGetOrCreateForName(label);
			  int[] propertyKeyIds = CreateProperties( transaction, properties );
			  TextValue value = stringValue( "some value" );
			  long node = CreateNodeWithPropertiesAndLabel( transaction, labelId, propertyKeyIds, value );
			  Commit();

			  // when
			  NewTransaction( AnonymousContext.full() );
			  string pattern = IndexPattern( label, properties );
			  string specifiedProvider = NATIVE10.providerName();
			  RawIterator<object[], ProcedureException> result = CallIndexProcedure( pattern, specifiedProvider );
			  // then
			  assertThat( Arrays.asList( result.Next() ), contains(pattern, specifiedProvider, ExpectedSuccessfulCreationStatus) );
			  Commit();
			  AwaitIndexOnline();

			  // and then
			  transaction = NewTransaction( AnonymousContext.read() );
			  SchemaRead schemaRead = transaction.SchemaRead();
			  IndexReference index = schemaRead.Index( labelId, propertyKeyIds );
			  AssertCorrectIndex( labelId, propertyKeyIds, UniquenessConstraint, index );
			  AssertIndexData( transaction, propertyKeyIds, value, node, index );
			  Commit();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertIndexData(org.neo4j.internal.kernel.api.Transaction transaction, int[] propertyKeyIds, org.neo4j.values.storable.TextValue value, long node, org.neo4j.internal.kernel.api.IndexReference index) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 private void AssertIndexData( Transaction transaction, int[] propertyKeyIds, TextValue value, long node, IndexReference index )
		 {
			  using ( NodeValueIndexCursor indexCursor = transaction.Cursors().allocateNodeValueIndexCursor() )
			  {
					IndexQuery[] query = new IndexQuery[propertyKeyIds.Length];
					for ( int i = 0; i < propertyKeyIds.Length; i++ )
					{
						 query[i] = IndexQuery.exact( propertyKeyIds[i], value );
					}
					transaction.DataRead().nodeIndexSeek(index, indexCursor, IndexOrder.NONE, false, query);
					assertTrue( indexCursor.Next() );
					assertEquals( node, indexCursor.NodeReference() );
					assertFalse( indexCursor.Next() );
			  }
		 }

		 private void AssertCorrectIndex( int labelId, int[] propertyKeyIds, bool expectedUnique, IndexReference index )
		 {
			  assertEquals( "provider key", "lucene+native", index.ProviderKey() );
			  assertEquals( "provider version", "1.0", index.ProviderVersion() );
			  assertEquals( expectedUnique, index.Unique );
			  assertEquals( "label id", labelId, index.Schema().EntityTokenIds[0] );
			  for ( int i = 0; i < propertyKeyIds.Length; i++ )
			  {
					assertEquals( "property key id", propertyKeyIds[i], index.Properties()[i] );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void throwOnUniquenessViolation() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ThrowOnUniquenessViolation()
		 {
			  TestThrowOnUniquenessViolation( "MyLabel", "oneKey" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void throwOnUniquenessViolationComposite() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ThrowOnUniquenessViolationComposite()
		 {
			  TestThrowOnUniquenessViolation( "MyLabel", "oneKey", "anotherKey" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void throwOnNonUniqueStore() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ThrowOnNonUniqueStore()
		 {
			  assumeThat( "Only relevant for uniqueness constraints", UniquenessConstraint, @is( true ) );

			  // given
			  string label = "SomeLabel";
			  string[] properties = new string[]{ "key1", "key2" };
			  Transaction transaction = NewTransaction( AnonymousContext.writeToken() );
			  int labelId = transaction.TokenWrite().labelGetOrCreateForName(label);
			  int[] propertyKeyIds = CreateProperties( transaction, properties );
			  TextValue value = stringValue( "some value" );
			  CreateNodeWithPropertiesAndLabel( transaction, labelId, propertyKeyIds, value );
			  CreateNodeWithPropertiesAndLabel( transaction, labelId, propertyKeyIds, value );
			  Commit();

			  // when
			  try
			  {
					CreateConstraint( label, properties );
					fail( "Should have failed" );
			  }
			  catch ( ProcedureException e )
			  {
					// then
					// good
					assertThat( rootCause( e ), instanceOf( typeof( IndexEntryConflictException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void throwOnExistenceViolation() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ThrowOnExistenceViolation()
		 {
			  assumeThat( "Only relevant for existence constraints", ExistenceConstraint, @is( true ) );

			  // given
			  string label = "label";
			  string prop = "key";
			  CreateConstraint( label, prop );

			  // when
			  try
			  {
					using ( Org.Neo4j.Graphdb.Transaction tx = Db.beginTx() )
					{
						 Db.createNode( Label.label( label ) );
						 tx.Success();
					}
					fail( "Should have failed" );
			  }
			  catch ( ConstraintViolationException )
			  {
					// then
					// good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("SameParameterValue") private void testThrowOnUniquenessViolation(String label, String... properties) throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 private void TestThrowOnUniquenessViolation( string label, params string[] properties )
		 {
			  assumeThat( "Only relevant for uniqueness constraints", UniquenessConstraint, @is( true ) );

			  // given
			  Transaction transaction = NewTransaction( AnonymousContext.writeToken() );
			  int labelId = transaction.TokenWrite().labelGetOrCreateForName(label);
			  int[] propertyKeyIds = CreateProperties( transaction, properties );
			  TextValue value = stringValue( "some value" );
			  CreateNodeWithPropertiesAndLabel( transaction, labelId, propertyKeyIds, value );
			  Commit();

			  CreateConstraint( label, properties );

			  // when
			  try
			  {
					transaction = NewTransaction( AnonymousContext.write() );
					CreateNodeWithPropertiesAndLabel( transaction, labelId, propertyKeyIds, value );
					fail( "Should have failed" );
			  }
			  catch ( UniquePropertyValueValidationException )
			  {
					// then
					// ok
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void createConstraint(String label, String... properties) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException, org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 private void CreateConstraint( string label, params string[] properties )
		 {
			  NewTransaction( AnonymousContext.full() );
			  string pattern = IndexPattern( label, properties );
			  string specifiedProvider = NATIVE10.providerName();
			  CallIndexProcedure( pattern, specifiedProvider );
			  Commit();
		 }

		 protected internal override TestGraphDatabaseFactory CreateGraphDatabaseFactory()
		 {
			  return new TestEnterpriseGraphDatabaseFactory();
		 }
	}

}