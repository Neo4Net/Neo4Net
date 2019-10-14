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
namespace Neo4Net.Kernel.Impl.Api.integrationtest
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;

	using Neo4Net.Collections;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using ProcedureException = Neo4Net.Internal.Kernel.Api.exceptions.ProcedureException;
	using TransactionFailureException = Neo4Net.Internal.Kernel.Api.exceptions.TransactionFailureException;
	using ProcedureCallContext = Neo4Net.Internal.Kernel.Api.procs.ProcedureCallContext;
	using ProcedureSignature = Neo4Net.Internal.Kernel.Api.procs.ProcedureSignature;
	using QualifiedName = Neo4Net.Internal.Kernel.Api.procs.QualifiedName;
	using ResourceTracker = Neo4Net.Kernel.api.ResourceTracker;
	using CallableProcedure = Neo4Net.Kernel.api.proc.CallableProcedure;
	using Context = Neo4Net.Kernel.api.proc.Context;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsCollectionContaining.hasItems;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.Internal.kernel.api.procs.Neo4jTypes.NTString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.Internal.kernel.api.procs.ProcedureSignature.procedureName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.Internal.kernel.api.procs.ProcedureSignature.procedureSignature;

	public class ProceduresKernelIT : KernelIntegrationTest
	{
		private bool InstanceFieldsInitialized = false;

		public ProceduresKernelIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_procedure = _procedure( _signature );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException exception = org.junit.rules.ExpectedException.none();
		 public ExpectedException Exception = ExpectedException.none();

		 private readonly ProcedureSignature _signature = procedureSignature( "example", "exampleProc" ).@in( "name", NTString ).@out( "name", NTString ).build();

		 private CallableProcedure _procedure;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetProcedureByName() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGetProcedureByName()
		 {
			  // Given
			  InternalKernel().registerProcedure(_procedure);

			  // When
			  ProcedureSignature found = Procs().procedureGet(new QualifiedName(new string[]{ "example" }, "exampleProc")).signature();

			  // Then
			  assertThat( found, equalTo( _signature ) );
			  Commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetBuiltInProcedureByName() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGetBuiltInProcedureByName()
		 {
			  // When
			  ProcedureSignature found = Procs().procedureGet(procedureName("db", "labels")).signature();

			  // Then
			  assertThat( found, equalTo( procedureSignature( procedureName( "db", "labels" ) ).@out( "label", NTString ).build() ) );
			  Commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetAllProcedures() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGetAllProcedures()
		 {
			  // Given
			  InternalKernel().registerProcedure(_procedure);
			  InternalKernel().registerProcedure(Procedure(procedureSignature("example", "exampleProc2").@out("name", NTString).build()));
			  InternalKernel().registerProcedure(Procedure(procedureSignature("example", "exampleProc3").@out("name", NTString).build()));

			  // When
			  IList<ProcedureSignature> signatures = Iterables.asList( NewTransaction().procedures().proceduresGetAll() );

			  // Then
			  assertThat( signatures, hasItems( _procedure.signature(), procedureSignature("example", "exampleProc2").@out("name", NTString).build(), procedureSignature("example", "exampleProc3").@out("name", NTString).build() ) );
			  Commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRefuseToRegisterNonVoidProcedureWithoutOutputs() throws org.neo4j.internal.kernel.api.exceptions.ProcedureException, org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRefuseToRegisterNonVoidProcedureWithoutOutputs()
		 {
			  // Then
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( "Procedures with zero output fields must be declared as VOID" );

			  // When
			  InternalKernel().registerProcedure(Procedure(procedureSignature("example", "exampleProc2").build()));
			  Commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallReadOnlyProcedure() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCallReadOnlyProcedure()
		 {
			  // Given
			  InternalKernel().registerProcedure(_procedure);

			  // When
			  RawIterator<object[], ProcedureException> found = Procs().procedureCallRead(Procs().procedureGet(new QualifiedName(new string[]{ "example" }, "exampleProc")).id(), new object[]{ 1337 }, ProcedureCallContext.EMPTY);

			  // Then
			  assertThat( asList( found ), contains( equalTo( new object[]{ 1337 } ) ) );
			  Commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void registeredProcedureShouldGetRead() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RegisteredProcedureShouldGetRead()
		 {
			  // Given
			  InternalKernel().registerProcedure(new CallableProcedure_BasicProcedureAnonymousInnerClass(this, _signature));

			  // When
			  RawIterator<object[], ProcedureException> stream = Procs().procedureCallRead(Procs().procedureGet(_signature.name()).id(), new object[]{ "" }, ProcedureCallContext.EMPTY);

			  // Then
			  assertNotNull( asList( stream ).get( 0 )[0] );
			  Commit();
		 }

		 private class CallableProcedure_BasicProcedureAnonymousInnerClass : Neo4Net.Kernel.api.proc.CallableProcedure_BasicProcedure
		 {
			 private readonly ProceduresKernelIT _outerInstance;

			 public CallableProcedure_BasicProcedureAnonymousInnerClass( ProceduresKernelIT outerInstance, ProcedureSignature signature ) : base( signature )
			 {
				 this.outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.collection.RawIterator<Object[],org.neo4j.internal.kernel.api.exceptions.ProcedureException> apply(org.neo4j.kernel.api.proc.Context ctx, Object[] input, org.neo4j.kernel.api.ResourceTracker resourceTracker) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
			 public override RawIterator<object[], ProcedureException> apply( Context ctx, object[] input, ResourceTracker resourceTracker )
			 {
				  return RawIterator.of<object[], ProcedureException>( new object[]{ ctx.Get( Neo4Net.Kernel.api.proc.Context_Fields.KernelTransaction ).dataRead() } );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static org.neo4j.kernel.api.proc.CallableProcedure procedure(final org.neo4j.internal.kernel.api.procs.ProcedureSignature signature)
		 private static CallableProcedure Procedure( ProcedureSignature signature )
		 {
			  return new CallableProcedure_BasicProcedureAnonymousInnerClass2( signature );
		 }

		 private class CallableProcedure_BasicProcedureAnonymousInnerClass2 : Neo4Net.Kernel.api.proc.CallableProcedure_BasicProcedure
		 {
			 public CallableProcedure_BasicProcedureAnonymousInnerClass2( ProcedureSignature signature ) : base( signature )
			 {
			 }

			 public override RawIterator<object[], ProcedureException> apply( Context ctx, object[] input, ResourceTracker resourceTracker )
			 {
				  return RawIterator.of<object[], ProcedureException>( input );
			 }
		 }
	}

}