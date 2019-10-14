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
namespace Neo4Net.Kernel.Impl.Api
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using IndexReference = Neo4Net.Internal.Kernel.Api.IndexReference;
	using Kernel = Neo4Net.Internal.Kernel.Api.Kernel;
	using Transaction = Neo4Net.Internal.Kernel.Api.Transaction;
	using KernelException = Neo4Net.Internal.Kernel.Api.exceptions.KernelException;
	using TransactionFailureException = Neo4Net.Internal.Kernel.Api.exceptions.TransactionFailureException;
	using IndexNotFoundKernelException = Neo4Net.Internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using ConstraintDescriptor = Neo4Net.Internal.Kernel.Api.schema.constraints.ConstraintDescriptor;
	using SchemaDescriptorFactory = Neo4Net.Kernel.api.schema.SchemaDescriptorFactory;
	using SchemaIndexTestHelper = Neo4Net.Kernel.Impl.Api.index.SchemaIndexTestHelper;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.Internal.kernel.api.Transaction_Type.@implicit;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.Internal.kernel.api.security.LoginContext.AUTH_DISABLED;

	public class KernelSchemaStateFlushingTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.ImpermanentDatabaseRule dbRule = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public ImpermanentDatabaseRule DbRule = new ImpermanentDatabaseRule();

		 private GraphDatabaseAPI _db;
		 private Kernel _kernel;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _db = DbRule.GraphDatabaseAPI;
			  _kernel = _db.DependencyResolver.resolveDependency( typeof( Kernel ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void after()
		 public virtual void After()
		 {
			  _db.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldKeepSchemaStateIfSchemaIsNotModified() throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldKeepSchemaStateIfSchemaIsNotModified()
		 {
			  // given
			  string before = CommitToSchemaState( "test", "before" );

			  // then
			  assertEquals( "before", before );

			  // given
			  string after = CommitToSchemaState( "test", "after" );

			  // then
			  assertEquals( "before", after );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInvalidateSchemaStateOnCreateIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInvalidateSchemaStateOnCreateIndex()
		 {
			  // given
			  CommitToSchemaState( "test", "before" );

			  AwaitIndexOnline( CreateIndex(), "test" );

			  // when
			  string after = CommitToSchemaState( "test", "after" );

			  // then
			  assertEquals( "after", after );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInvalidateSchemaStateOnDropIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInvalidateSchemaStateOnDropIndex()
		 {
			  IndexReference @ref = CreateIndex();

			  AwaitIndexOnline( @ref, "test" );

			  CommitToSchemaState( "test", "before" );

			  DropIndex( @ref );

			  // when
			  string after = CommitToSchemaState( "test", "after" );

			  // then
			  assertEquals( "after", after );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInvalidateSchemaStateOnCreateConstraint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInvalidateSchemaStateOnCreateConstraint()
		 {
			  // given
			  CommitToSchemaState( "test", "before" );

			  CreateConstraint();

			  // when
			  string after = CommitToSchemaState( "test", "after" );

			  // then
			  assertEquals( "after", after );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInvalidateSchemaStateOnDropConstraint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInvalidateSchemaStateOnDropConstraint()
		 {
			  // given
			  ConstraintDescriptor descriptor = CreateConstraint();

			  CommitToSchemaState( "test", "before" );

			  DropConstraint( descriptor );

			  // when
			  string after = CommitToSchemaState( "test", "after" );

			  // then
			  assertEquals( "after", after );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.internal.kernel.api.schema.constraints.ConstraintDescriptor createConstraint() throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 private ConstraintDescriptor CreateConstraint()
		 {

			  using ( Transaction transaction = _kernel.beginTransaction( @implicit, AUTH_DISABLED ) )
			  {
					ConstraintDescriptor descriptor = transaction.SchemaWrite().uniquePropertyConstraintCreate(SchemaDescriptorFactory.forLabel(1, 1));
					transaction.Success();
					return descriptor;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void dropConstraint(org.neo4j.internal.kernel.api.schema.constraints.ConstraintDescriptor descriptor) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 private void DropConstraint( ConstraintDescriptor descriptor )
		 {
			  using ( Transaction transaction = _kernel.beginTransaction( @implicit, AUTH_DISABLED ) )
			  {
					transaction.SchemaWrite().constraintDrop(descriptor);
					transaction.Success();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.internal.kernel.api.IndexReference createIndex() throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 private IndexReference CreateIndex()
		 {
			  using ( Transaction transaction = _kernel.beginTransaction( @implicit, AUTH_DISABLED ) )
			  {
					IndexReference reference = transaction.SchemaWrite().indexCreate(SchemaDescriptorFactory.forLabel(1, 1));
					transaction.Success();
					return reference;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void dropIndex(org.neo4j.internal.kernel.api.IndexReference reference) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 private void DropIndex( IndexReference reference )
		 {
			  using ( Transaction transaction = _kernel.beginTransaction( @implicit, AUTH_DISABLED ) )
			  {
					transaction.SchemaWrite().indexDrop(reference);
					transaction.Success();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void awaitIndexOnline(org.neo4j.internal.kernel.api.IndexReference descriptor, String keyForProbing) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException, org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 private void AwaitIndexOnline( IndexReference descriptor, string keyForProbing )
		 {
			  using ( Transaction transaction = _kernel.beginTransaction( @implicit, AUTH_DISABLED ) )
			  {
					SchemaIndexTestHelper.awaitIndexOnline( transaction.SchemaRead(), descriptor );
					transaction.Success();
			  }
			  AwaitSchemaStateCleared( keyForProbing );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void awaitSchemaStateCleared(String keyForProbing) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 private void AwaitSchemaStateCleared( string keyForProbing )
		 {
			  using ( Transaction transaction = _kernel.beginTransaction( @implicit, AUTH_DISABLED ) )
			  {
					while ( transaction.SchemaRead().schemaStateGetOrCreate(keyForProbing, ignored => null) != null )
					{
						 LockSupport.parkNanos( MILLISECONDS.toNanos( 10 ) );
					}
					transaction.Success();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private String commitToSchemaState(String key, String value) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 private string CommitToSchemaState( string key, string value )
		 {
			  using ( Transaction transaction = _kernel.beginTransaction( @implicit, AUTH_DISABLED ) )
			  {
					string result = GetOrCreateFromState( transaction, key, value );
					transaction.Success();
					return result;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private String getOrCreateFromState(org.neo4j.internal.kernel.api.Transaction tx, String key, final String value)
		 private string GetOrCreateFromState( Transaction tx, string key, string value )
		 {
			  return tx.SchemaRead().schemaStateGetOrCreate(key, from => value);
		 }

	}

}