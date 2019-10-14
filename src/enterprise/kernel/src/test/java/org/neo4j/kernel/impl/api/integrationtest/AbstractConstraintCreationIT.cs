using System;
using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.Kernel.Impl.Api.integrationtest
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using QueryExecutionException = Neo4Net.Graphdb.QueryExecutionException;
	using TransientTransactionFailureException = Neo4Net.Graphdb.TransientTransactionFailureException;
	using ConstraintDefinition = Neo4Net.Graphdb.schema.ConstraintDefinition;
	using IndexDefinition = Neo4Net.Graphdb.schema.IndexDefinition;
	using Schema = Neo4Net.Graphdb.schema.Schema;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using SchemaWrite = Neo4Net.Internal.Kernel.Api.SchemaWrite;
	using TokenWrite = Neo4Net.Internal.Kernel.Api.TokenWrite;
	using Transaction = Neo4Net.Internal.Kernel.Api.Transaction;
	using KernelException = Neo4Net.Internal.Kernel.Api.exceptions.KernelException;
	using TransactionFailureException = Neo4Net.Internal.Kernel.Api.exceptions.TransactionFailureException;
	using SchemaDescriptor = Neo4Net.Internal.Kernel.Api.schema.SchemaDescriptor;
	using ConstraintDescriptor = Neo4Net.Internal.Kernel.Api.schema.constraints.ConstraintDescriptor;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using AlreadyConstrainedException = Neo4Net.Kernel.Api.Exceptions.schema.AlreadyConstrainedException;
	using DropConstraintFailureException = Neo4Net.Kernel.Api.Exceptions.schema.DropConstraintFailureException;
	using NoSuchConstraintException = Neo4Net.Kernel.Api.Exceptions.schema.NoSuchConstraintException;
	using TestEnterpriseGraphDatabaseFactory = Neo4Net.Test.TestEnterpriseGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.startsWith;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asCollection;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.single;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.Internal.kernel.api.security.LoginContext.AUTH_DISABLED;

	public abstract class AbstractConstraintCreationIT<Constraint, DESCRIPTOR> : KernelIntegrationTest where Constraint : Neo4Net.Internal.Kernel.Api.schema.constraints.ConstraintDescriptor where DESCRIPTOR : Neo4Net.Internal.Kernel.Api.schema.SchemaDescriptor
	{
		 internal const string KEY = "Foo";
		 internal const string PROP = "bar";

		 internal int TypeId;
		 internal int PropertyKeyId;
		 internal DESCRIPTOR Descriptor;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract int initializeLabelOrRelType(org.neo4j.internal.kernel.api.TokenWrite tokenWrite, String name) throws org.neo4j.internal.kernel.api.exceptions.KernelException;
		 internal abstract int InitializeLabelOrRelType( TokenWrite tokenWrite, string name );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract Constraint createConstraint(org.neo4j.internal.kernel.api.SchemaWrite writeOps, DESCRIPTOR descriptor) throws Exception;
		 internal abstract Constraint CreateConstraint( SchemaWrite writeOps, DESCRIPTOR descriptor );

		 internal abstract void CreateConstraintInRunningTx( GraphDatabaseService db, string type, string property );

		 internal abstract Constraint NewConstraintObject( DESCRIPTOR descriptor );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract void dropConstraint(org.neo4j.internal.kernel.api.SchemaWrite writeOps, Constraint constraint) throws Exception;
		 internal abstract void DropConstraint( SchemaWrite writeOps, Constraint constraint );

		 internal abstract void CreateOffendingDataInRunningTx( GraphDatabaseService db );

		 internal abstract void RemoveOffendingDataInRunningTx( GraphDatabaseService db );

		 internal abstract DESCRIPTOR MakeDescriptor( int typeId, int propertyKeyId );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void createKeys() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CreateKeys()
		 {
			  TokenWrite tokenWrite = tokenWriteInNewTransaction();
			  this.TypeId = InitializeLabelOrRelType( tokenWrite, KEY );
			  this.PropertyKeyId = tokenWrite.PropertyKeyGetOrCreateForName( PROP );
			  this.Descriptor = MakeDescriptor( TypeId, PropertyKeyId );
			  commit();
		 }

		 protected internal override GraphDatabaseService CreateGraphDatabase()
		 {
			  return ( new TestEnterpriseGraphDatabaseFactory() ).setFileSystem(fileSystemRule.get()).newEmbeddedDatabase(testDir.storeDir());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToStoreAndRetrieveConstraint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToStoreAndRetrieveConstraint()
		 {
			  // given
			  Transaction transaction = newTransaction( AUTH_DISABLED );

			  // when
			  ConstraintDescriptor constraint = CreateConstraint( transaction.SchemaWrite(), Descriptor );

			  // then
			  assertEquals( constraint, single( transaction.SchemaRead().constraintsGetAll() ) );

			  // given
			  commit();
			  transaction = newTransaction();

			  // when
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Iterator<?> constraints = transaction.schemaRead().constraintsGetAll();
			  IEnumerator<object> constraints = transaction.SchemaRead().constraintsGetAll();

			  // then
			  assertEquals( constraint, single( constraints ) );
			  commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToStoreAndRetrieveConstraintAfterRestart() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToStoreAndRetrieveConstraintAfterRestart()
		 {
			  // given
			  Transaction transaction = newTransaction( AUTH_DISABLED );

			  // when
			  ConstraintDescriptor constraint = CreateConstraint( transaction.SchemaWrite(), Descriptor );

			  // then
			  assertEquals( constraint, single( transaction.SchemaRead().constraintsGetAll() ) );

			  // given
			  commit();
			  restartDb();
			  transaction = newTransaction();

			  // when
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Iterator<?> constraints = transaction.schemaRead().constraintsGetAll();
			  IEnumerator<object> constraints = transaction.SchemaRead().constraintsGetAll();

			  // then
			  assertEquals( constraint, single( constraints ) );

			  commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotPersistConstraintCreatedInAbortedTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotPersistConstraintCreatedInAbortedTransaction()
		 {
			  // given
			  SchemaWrite schemaWriteOperations = schemaWriteInNewTransaction();

			  CreateConstraint( schemaWriteOperations, Descriptor );

			  // when
			  rollback();

			 Transaction transaction = newTransaction();

			  // then
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Iterator<?> constraints = transaction.schemaRead().constraintsGetAll();
			  IEnumerator<object> constraints = transaction.SchemaRead().constraintsGetAll();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( "should not have any constraints", constraints.hasNext() );
			  commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotStoreConstraintThatIsRemovedInTheSameTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotStoreConstraintThatIsRemovedInTheSameTransaction()
		 {
			  // given
			  Transaction transaction = newTransaction( AUTH_DISABLED );

			  Constraint constraint = CreateConstraint( transaction.SchemaWrite(), Descriptor );

			  // when
			  DropConstraint( transaction.SchemaWrite(), constraint );

			  // then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( "should not have any constraints", transaction.SchemaRead().constraintsGetAll().hasNext() );

			  // when
			  commit();

			 transaction = newTransaction();

			  // then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( "should not have any constraints", transaction.SchemaRead().constraintsGetAll().hasNext() );
			  commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDropConstraint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDropConstraint()
		 {
			  // given
			  Constraint constraint;
			  {
					SchemaWrite statement = schemaWriteInNewTransaction();
					constraint = CreateConstraint( statement, Descriptor );
					commit();
			  }

			  {
			  // when
					SchemaWrite statement = schemaWriteInNewTransaction();
					DropConstraint( statement, constraint );
					commit();
			  }

			  {
			  // then
					Transaction transaction = newTransaction();

					// then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( "should not have any constraints", transaction.SchemaRead().constraintsGetAll().hasNext() );
					commit();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCreateConstraintThatAlreadyExists() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotCreateConstraintThatAlreadyExists()
		 {
			  {
			  // given
					SchemaWrite statement = schemaWriteInNewTransaction();
					CreateConstraint( statement, Descriptor );
					commit();
			  }

			  // when
			  try
			  {
					SchemaWrite statement = schemaWriteInNewTransaction();

					CreateConstraint( statement, Descriptor );

					fail( "Should not have validated" );
			  }
			  // then
			  catch ( AlreadyConstrainedException )
			  {
					// good
			  }
			  commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotRemoveConstraintThatGetsReAdded() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotRemoveConstraintThatGetsReAdded()
		 {
			  // given
			  Constraint constraint;
			  {
					SchemaWrite statement = schemaWriteInNewTransaction();
					constraint = CreateConstraint( statement, Descriptor );
					commit();
			  }
			  using ( Neo4Net.Graphdb.Transaction tx = Db.beginTx() )
			  {
					// Make sure all schema changes are stable, to avoid any synchronous schema state invalidation
					Db.schema().awaitIndexesOnline(10, TimeUnit.SECONDS);
			  }
			  SchemaStateCheck schemaState = ( new SchemaStateCheck( this ) ).SetUp();
			  {
					SchemaWrite statement = schemaWriteInNewTransaction();

					// when
					DropConstraint( statement, constraint );
					CreateConstraint( statement, Descriptor );
					commit();
			  }
			  {
				  Transaction transaction = newTransaction();

					// then
					assertEquals( singletonList( constraint ), asCollection( transaction.SchemaRead().constraintsGetAll() ) );
					schemaState.AssertNotCleared( transaction );
					commit();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldClearSchemaStateWhenConstraintIsCreated() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldClearSchemaStateWhenConstraintIsCreated()
		 {
			  // given
			  SchemaStateCheck schemaState = ( new SchemaStateCheck( this ) ).SetUp();

			  SchemaWrite statement = schemaWriteInNewTransaction();

			  // when
			  CreateConstraint( statement, Descriptor );
			  commit();

			  // then
			  schemaState.AssertCleared( newTransaction() );
			  rollback();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldClearSchemaStateWhenConstraintIsDropped() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldClearSchemaStateWhenConstraintIsDropped()
		 {
			  // given
			  Constraint constraint;
			  SchemaStateCheck schemaState;
			  {
					SchemaWrite statement = schemaWriteInNewTransaction();
					constraint = CreateConstraint( statement, Descriptor );
					commit();

					schemaState = ( new SchemaStateCheck( this ) ).SetUp();
			  }

			  {
					SchemaWrite statement = schemaWriteInNewTransaction();

					// when
					DropConstraint( statement, constraint );
					commit();
			  }

			  // then
			  schemaState.AssertCleared( newTransaction() );
			  rollback();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotDropConstraintThatDoesNotExist() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotDropConstraintThatDoesNotExist()
		 {
			  // given
			  Constraint constraint = NewConstraintObject( Descriptor );

			  {
			  // when
					SchemaWrite statement = schemaWriteInNewTransaction();

					try
					{
						 DropConstraint( statement, constraint );
						 fail( "Should not have dropped constraint" );
					}
					catch ( DropConstraintFailureException e )
					{
						 assertThat( e.InnerException, instanceOf( typeof( NoSuchConstraintException ) ) );
					}
					commit();
			  }

			  {
			  // then
					Transaction transaction = newTransaction();
					assertEquals( emptySet(), asSet(transaction.SchemaRead().indexesGetAll()) );
					commit();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotLeaveAnyStateBehindAfterFailingToCreateConstraint()
		 public virtual void ShouldNotLeaveAnyStateBehindAfterFailingToCreateConstraint()
		 {
			  // given
			  using ( Neo4Net.Graphdb.Transaction tx = Db.beginTx() )
			  {
					CreateOffendingDataInRunningTx( db );
					tx.Success();
			  }

			  // when
			  try
			  {
					  using ( Neo4Net.Graphdb.Transaction tx = Db.beginTx() )
					  {
						CreateConstraintInRunningTx( db, KEY, PROP );
      
						tx.Success();
						fail( "expected failure" );
					  }
			  }
			  catch ( QueryExecutionException e )
			  {
					assertThat( e.Message, startsWith( "Unable to create CONSTRAINT" ) );
			  }

			  // then
			  using ( Neo4Net.Graphdb.Transaction tx = Db.beginTx() )
			  {
					assertEquals( System.Linq.Enumerable.Empty<ConstraintDefinition>(), Iterables.asList(Db.schema().Constraints) );
					assertEquals( System.Linq.Enumerable.Empty<IndexDefinition, Neo4Net.Graphdb.schema.Schema_IndexState>(), IndexesWithState(Db.schema()) );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToResolveConflictsAndRecreateConstraintAfterFailingToCreateItDueToConflict() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToResolveConflictsAndRecreateConstraintAfterFailingToCreateItDueToConflict()
		 {
			  // given
			  using ( Neo4Net.Graphdb.Transaction tx = Db.beginTx() )
			  {
					CreateOffendingDataInRunningTx( db );
					tx.Success();
			  }

			  // when
			  try
			  {
					  using ( Neo4Net.Graphdb.Transaction tx = Db.beginTx() )
					  {
						CreateConstraintInRunningTx( db, KEY, PROP );
      
						tx.Success();
						fail( "expected failure" );
					  }
			  }
			  catch ( QueryExecutionException e )
			  {
					assertThat( e.Message, startsWith( "Unable to create CONSTRAINT" ) );
			  }

			  using ( Neo4Net.Graphdb.Transaction tx = Db.beginTx() )
			  {
					RemoveOffendingDataInRunningTx( db );
					tx.Success();
			  }

			  // then - this should not fail
			  SchemaWrite statement = schemaWriteInNewTransaction();
			  CreateConstraint( statement, Descriptor );
			  commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void changedConstraintsShouldResultInTransientFailure()
		 public virtual void ChangedConstraintsShouldResultInTransientFailure()
		 {
			  // Given
			  ThreadStart constraintCreation = () =>
			  {
				using ( Neo4Net.Graphdb.Transaction tx = Db.beginTx() )
				{
					 CreateConstraintInRunningTx( db, KEY, PROP );
					 tx.Success();
				}
			  };

			  // When
			  try
			  {
					using ( Neo4Net.Graphdb.Transaction tx = Db.beginTx() )
					{
						 Executors.newSingleThreadExecutor().submit(constraintCreation).get();
						 Db.createNode();
						 tx.Success();
					}
					fail( "Exception expected" );
			  }
			  catch ( Exception e )
			  {
					// Then
					assertThat( e, instanceOf( typeof( TransientTransactionFailureException ) ) );
					assertThat( e.InnerException, instanceOf( typeof( TransactionFailureException ) ) );
					TransactionFailureException cause = ( TransactionFailureException ) e.InnerException;
					assertEquals( Neo4Net.Kernel.Api.Exceptions.Status_Transaction.ConstraintsChanged, cause.Status() );
			  }
		 }

		 private static IDictionary<IndexDefinition, Neo4Net.Graphdb.schema.Schema_IndexState> IndexesWithState( Schema schema )
		 {
			  IDictionary<IndexDefinition, Neo4Net.Graphdb.schema.Schema_IndexState> result = new Dictionary<IndexDefinition, Neo4Net.Graphdb.schema.Schema_IndexState>();
			  foreach ( IndexDefinition definition in Schema.Indexes )
			  {
					result[definition] = Schema.getIndexState( definition );
			  }
			  return result;
		 }

		 private class SchemaStateCheck : System.Func<string, int>
		 {
			 private readonly AbstractConstraintCreationIT<Constraint, DESCRIPTOR> _outerInstance;

			 public SchemaStateCheck( AbstractConstraintCreationIT<Constraint, DESCRIPTOR> outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal int InvocationCount;

			  public override int? Apply( string s )
			  {
					InvocationCount++;
					return int.Parse( s );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public SchemaStateCheck setUp() throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
			  public virtual SchemaStateCheck SetUp()
			  {
					Transaction transaction = newTransaction();
					CheckState( transaction );
					commit();
					return this;
			  }

			  internal virtual void AssertCleared( Transaction transaction )
			  {
					int count = InvocationCount;
					CheckState( transaction );
					assertEquals( "schema state should have been cleared.", count + 1, InvocationCount );
			  }

			  internal virtual void AssertNotCleared( Transaction transaction )
			  {
					int count = InvocationCount;
					CheckState( transaction );
					assertEquals( "schema state should not have been cleared.", count, InvocationCount );
			  }

			  internal virtual SchemaStateCheck CheckState( Transaction transaction )
			  {
					assertEquals( Convert.ToInt32( 7 ), transaction.SchemaRead().schemaStateGetOrCreate("7", this) );
					return this;
			  }
		 }
	}

}