﻿/*
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

	using IndexReference = Neo4Net.Kernel.Api.Internal.IndexReference;
	using Kernel = Neo4Net.Kernel.Api.Internal.Kernel;
	using Transaction = Neo4Net.Kernel.Api.Internal.Transaction;
	using KernelException = Neo4Net.Kernel.Api.Internal.Exceptions.KernelException;
	using TransactionFailureException = Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException;
	using IndexNotFoundKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotFoundKernelException;
	using ConstraintDescriptor = Neo4Net.Kernel.Api.Internal.schema.constraints.ConstraintDescriptor;
	using SchemaDescriptorFactory = Neo4Net.Kernel.api.schema.SchemaDescriptorFactory;
	using SchemaIndexTestHelper = Neo4Net.Kernel.Impl.Api.index.SchemaIndexTestHelper;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.Transaction_Type.@implicit;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.security.LoginContext.AUTH_DISABLED;

	public class KernelSchemaStateFlushingTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.ImpermanentDatabaseRule dbRule = new org.Neo4Net.test.rule.ImpermanentDatabaseRule();
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
//ORIGINAL LINE: @Test public void shouldKeepSchemaStateIfSchemaIsNotModified() throws org.Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException
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
//ORIGINAL LINE: private org.Neo4Net.Kernel.Api.Internal.schema.constraints.ConstraintDescriptor createConstraint() throws org.Neo4Net.Kernel.Api.Internal.Exceptions.KernelException
		 private ConstraintDescriptor CreateConstraint()
		 {

			  using ( Transaction transaction = _kernel.BeginTransaction( @implicit, AUTH_DISABLED ) )
			  {
					ConstraintDescriptor descriptor = transaction.SchemaWrite().uniquePropertyConstraintCreate(SchemaDescriptorFactory.forLabel(1, 1));
					transaction.Success();
					return descriptor;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void dropConstraint(org.Neo4Net.Kernel.Api.Internal.schema.constraints.ConstraintDescriptor descriptor) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.KernelException
		 private void DropConstraint( ConstraintDescriptor descriptor )
		 {
			  using ( Transaction transaction = _kernel.BeginTransaction( @implicit, AUTH_DISABLED ) )
			  {
					transaction.SchemaWrite().constraintDrop(descriptor);
					transaction.Success();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.Kernel.Api.Internal.IndexReference createIndex() throws org.Neo4Net.Kernel.Api.Internal.Exceptions.KernelException
		 private IndexReference CreateIndex()
		 {
			  using ( Transaction transaction = _kernel.BeginTransaction( @implicit, AUTH_DISABLED ) )
			  {
					IndexReference reference = transaction.SchemaWrite().indexCreate(SchemaDescriptorFactory.forLabel(1, 1));
					transaction.Success();
					return reference;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void dropIndex(org.Neo4Net.Kernel.Api.Internal.IndexReference reference) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.KernelException
		 private void DropIndex( IndexReference reference )
		 {
			  using ( Transaction transaction = _kernel.BeginTransaction( @implicit, AUTH_DISABLED ) )
			  {
					transaction.SchemaWrite().indexDrop(reference);
					transaction.Success();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void awaitIndexOnline(org.Neo4Net.Kernel.Api.Internal.IndexReference descriptor, String keyForProbing) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotFoundKernelException, org.Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException
		 private void AwaitIndexOnline( IndexReference descriptor, string keyForProbing )
		 {
			  using ( Transaction transaction = _kernel.BeginTransaction( @implicit, AUTH_DISABLED ) )
			  {
					SchemaIndexTestHelper.awaitIndexOnline( transaction.SchemaRead(), descriptor );
					transaction.Success();
			  }
			  AwaitSchemaStateCleared( keyForProbing );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void awaitSchemaStateCleared(String keyForProbing) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException
		 private void AwaitSchemaStateCleared( string keyForProbing )
		 {
			  using ( Transaction transaction = _kernel.BeginTransaction( @implicit, AUTH_DISABLED ) )
			  {
					while ( transaction.SchemaRead().schemaStateGetOrCreate(keyForProbing, ignored => null) != null )
					{
						 LockSupport.parkNanos( MILLISECONDS.toNanos( 10 ) );
					}
					transaction.Success();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private String commitToSchemaState(String key, String value) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException
		 private string CommitToSchemaState( string key, string value )
		 {
			  using ( Transaction transaction = _kernel.BeginTransaction( @implicit, AUTH_DISABLED ) )
			  {
					string result = GetOrCreateFromState( transaction, key, value );
					transaction.Success();
					return result;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private String getOrCreateFromState(org.Neo4Net.Kernel.Api.Internal.Transaction tx, String key, final String value)
		 private string GetOrCreateFromState( Transaction tx, string key, string value )
		 {
			  return tx.SchemaRead().schemaStateGetOrCreate(key, from => value);
		 }

	}

}