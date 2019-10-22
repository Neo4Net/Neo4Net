using System.Collections.Generic;

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
namespace Neo4Net.Kernel.builtinprocs
{
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using Neo4Net.Collections;
	using ConstraintViolationException = Neo4Net.GraphDb.ConstraintViolationException;
	using Label = Neo4Net.GraphDb.Label;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using IndexOrder = Neo4Net.Internal.Kernel.Api.IndexOrder;
	using IndexQuery = Neo4Net.Internal.Kernel.Api.IndexQuery;
	using IndexReference = Neo4Net.Internal.Kernel.Api.IndexReference;
	using NodeValueIndexCursor = Neo4Net.Internal.Kernel.Api.NodeValueIndexCursor;
	using SchemaRead = Neo4Net.Internal.Kernel.Api.SchemaRead;
	using TokenRead = Neo4Net.Internal.Kernel.Api.TokenRead;
	using Transaction = Neo4Net.Internal.Kernel.Api.Transaction;
	using KernelException = Neo4Net.Internal.Kernel.Api.exceptions.KernelException;
	using ProcedureException = Neo4Net.Internal.Kernel.Api.exceptions.ProcedureException;
	using TransactionFailureException = Neo4Net.Internal.Kernel.Api.exceptions.TransactionFailureException;
	using IllegalTokenNameException = Neo4Net.Internal.Kernel.Api.exceptions.schema.IllegalTokenNameException;
	using ProcedureCallContext = Neo4Net.Internal.Kernel.Api.procs.ProcedureCallContext;
	using ProcedureSignature = Neo4Net.Internal.Kernel.Api.procs.ProcedureSignature;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using UniquePropertyValueValidationException = Neo4Net.Kernel.Api.Exceptions.schema.UniquePropertyValueValidationException;
	using AnonymousContext = Neo4Net.Kernel.api.security.AnonymousContext;
	using KernelIntegrationTest = Neo4Net.Kernel.Impl.Api.integrationtest.KernelIntegrationTest;
	using TestEnterpriseGraphDatabaseFactory = Neo4Net.Test.TestEnterpriseGraphDatabaseFactory;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using TextValue = Neo4Net.Values.Storable.TextValue;

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
//	import static org.Neo4Net.graphdb.factory.GraphDatabaseSettings.SchemaIndex.NATIVE10;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.Exceptions.rootCause;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.stringValue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class EnterpriseCreateIndexProcedureIT extends org.Neo4Net.kernel.impl.api.integrationtest.KernelIntegrationTest
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
//ORIGINAL LINE: @Test public void createIndexWithGivenProvider() throws org.Neo4Net.internal.kernel.api.exceptions.KernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CreateIndexWithGivenProvider()
		 {
			  TestCreateIndexWithGivenProvider( "Person", "name" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createIndexWithGivenProviderComposite() throws org.Neo4Net.internal.kernel.api.exceptions.KernelException
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
			  assertEquals( "label token should not exist", Neo4Net.Internal.Kernel.Api.TokenRead_Fields.NO_TOKEN, transaction.TokenRead().nodeLabel(label) );
			  assertEquals( "property token should not exist", Neo4Net.Internal.Kernel.Api.TokenRead_Fields.NO_TOKEN, transaction.TokenRead().propertyKey(propKey) );
			  Commit();

			  // when
			  NewTransaction( AnonymousContext.full() );
			  CallIndexProcedure( IndexPattern( label, propKey ), GraphDatabaseSettings.SchemaIndex.NATIVE20.providerName() );
			  Commit();

			  // then
			  transaction = NewTransaction( AnonymousContext.read() );
			  assertNotEquals( "label token should exist", Neo4Net.Internal.Kernel.Api.TokenRead_Fields.NO_TOKEN, transaction.TokenRead().nodeLabel(label) );
			  assertNotEquals( "property token should exist", Neo4Net.Internal.Kernel.Api.TokenRead_Fields.NO_TOKEN, transaction.TokenRead().propertyKey(propKey) );
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
			  using ( Neo4Net.GraphDb.Transaction tx = Db.beginTx() )
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
//ORIGINAL LINE: private int[] createProperties(org.Neo4Net.internal.kernel.api.Transaction transaction, String... properties) throws org.Neo4Net.internal.kernel.api.exceptions.schema.IllegalTokenNameException
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
//ORIGINAL LINE: private long createNodeWithPropertiesAndLabel(org.Neo4Net.internal.kernel.api.Transaction transaction, int labelId, int[] propertyKeyIds, org.Neo4Net.values.storable.TextValue value) throws org.Neo4Net.internal.kernel.api.exceptions.KernelException
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
//ORIGINAL LINE: private org.Neo4Net.collection.RawIterator<Object[],org.Neo4Net.internal.kernel.api.exceptions.ProcedureException> callIndexProcedure(String pattern, String specifiedProvider) throws org.Neo4Net.internal.kernel.api.exceptions.ProcedureException, org.Neo4Net.internal.kernel.api.exceptions.TransactionFailureException
		 private RawIterator<object[], ProcedureException> CallIndexProcedure( string pattern, string specifiedProvider )
		 {
			  return ProcsSchema().procedureCallSchema(ProcedureSignature.procedureName("db", IndexProcedureName), new object[] { pattern, specifiedProvider }, ProcedureCallContext.EMPTY);
		 }

		 private void AwaitIndexOnline()
		 {
			  using ( Neo4Net.GraphDb.Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(10, TimeUnit.SECONDS);
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void testCreateIndexWithGivenProvider(String label, String... properties) throws org.Neo4Net.internal.kernel.api.exceptions.KernelException
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
//ORIGINAL LINE: private void assertIndexData(org.Neo4Net.internal.kernel.api.Transaction transaction, int[] propertyKeyIds, org.Neo4Net.values.storable.TextValue value, long node, org.Neo4Net.internal.kernel.api.IndexReference index) throws org.Neo4Net.internal.kernel.api.exceptions.KernelException
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
					using ( Neo4Net.GraphDb.Transaction tx = Db.beginTx() )
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
//ORIGINAL LINE: private void createConstraint(String label, String... properties) throws org.Neo4Net.internal.kernel.api.exceptions.TransactionFailureException, org.Neo4Net.internal.kernel.api.exceptions.ProcedureException
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