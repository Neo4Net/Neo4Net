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
namespace Neo4Net.Kernel.impl.proc
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;

	using Neo4Net.Collections;
	using Iterables = Neo4Net.Collections.Helpers.Iterables;
	using ProcedureException = Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
	using ProcedureSignature = Neo4Net.Kernel.Api.Internal.procs.ProcedureSignature;
	using ResourceTracker = Neo4Net.Kernel.api.ResourceTracker;
	using StubResourceManager = Neo4Net.Kernel.api.StubResourceManager;
	using BasicContext = Neo4Net.Kernel.api.proc.BasicContext;
	using CallableProcedure = Neo4Net.Kernel.api.proc.CallableProcedure;
	using Context = Neo4Net.Kernel.api.proc.Context;
	using Neo4Net.Kernel.api.proc;
	using Mode = Neo4Net.Procedure.Mode;
	using PerformsWrites = Neo4Net.Procedure.PerformsWrites;
	using Procedure = Neo4Net.Procedure.Procedure;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTAny;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTInteger;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.ProcedureSignature.procedureSignature;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.proc.Key.key;

	public class ProceduresTest
	{
		private bool InstanceFieldsInitialized = false;

		public ProceduresTest()
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

		 private readonly Procedures _procs = new Procedures();
		 private readonly ProcedureSignature _signature = procedureSignature( "org", "myproc" ).@out( "name", NTString ).build();
		 private CallableProcedure _procedure;
		 private readonly ResourceTracker _resourceTracker = new StubResourceManager();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetRegisteredProcedure() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGetRegisteredProcedure()
		 {
			  // When
			  _procs.register( _procedure );

			  // Then
			  assertThat( _procs.procedure( _signature.name() ).signature(), equalTo(_signature) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetAllRegisteredProcedures() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGetAllRegisteredProcedures()
		 {
			  // When
			  _procs.register( Procedure( procedureSignature( "org", "myproc1" ).@out( "age", NTInteger ).build() ) );
			  _procs.register( Procedure( procedureSignature( "org", "myproc2" ).@out( "age", NTInteger ).build() ) );
			  _procs.register( Procedure( procedureSignature( "org", "myproc3" ).@out( "age", NTInteger ).build() ) );

			  // Then
			  IList<ProcedureSignature> signatures = Iterables.asList( _procs.AllProcedures );
			  assertThat( signatures, containsInAnyOrder( procedureSignature( "org", "myproc1" ).@out( "age", NTInteger ).build(), procedureSignature("org", "myproc2").@out("age", NTInteger).build(), procedureSignature("org", "myproc3").@out("age", NTInteger).build() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallRegisteredProcedure() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCallRegisteredProcedure()
		 {
			  // Given
			  _procs.register( _procedure );

			  // When
			  RawIterator<object[], ProcedureException> result = _procs.callProcedure( new BasicContext(), _signature.name(), new object[]{ 1337 }, _resourceTracker );

			  // Then
			  assertThat( asList( result ), contains( equalTo( new object[]{ 1337 } ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowCallingNonExistingProcedure() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowCallingNonExistingProcedure()
		 {
			  // Expect
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( "There is no procedure with the name `org.myproc` registered for this " + "database instance. Please ensure you've spelled the " + "procedure name correctly and that the procedure is properly deployed." );

			  // When
			  _procs.callProcedure( new BasicContext(), _signature.name(), new object[]{ 1337 }, _resourceTracker );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowRegisteringConflictingName() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowRegisteringConflictingName()
		 {
			  // Given
			  _procs.register( _procedure );

			  // Expect
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( "Unable to register procedure, because the name `org.myproc` is already in use." );

			  // When
			  _procs.register( _procedure );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowDuplicateFieldNamesInInput() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowDuplicateFieldNamesInInput()
		 {
			  // Expect
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( "Procedure `asd(a :: ANY?, a :: ANY?) :: ()` cannot be " + "registered, because it contains a duplicated input field, 'a'. " + "You need to rename or remove one of the duplicate fields." );

			  // When
			  _procs.register( ProcedureWithSignature( procedureSignature( "asd" ).@in( "a", NTAny ).@in( "a", NTAny ).build() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowDuplicateFieldNamesInOutput() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowDuplicateFieldNamesInOutput()
		 {
			  // Expect
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( "Procedure `asd() :: (a :: ANY?, a :: ANY?)` cannot be registered, " + "because it contains a duplicated output field, 'a'. " + "You need to rename or remove one of the duplicate fields." );

			  // When
			  _procs.register( ProcedureWithSignature( procedureSignature( "asd" ).@out( "a", NTAny ).@out( "a", NTAny ).build() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSignalNonExistingProcedure() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSignalNonExistingProcedure()
		 {
			  // Expect
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( "There is no procedure with the name `org.myproc` registered for this " + "database instance. Please ensure you've spelled the " + "procedure name correctly and that the procedure is properly deployed." );

			  // When
			  _procs.procedure( _signature.name() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMakeContextAvailable() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldMakeContextAvailable()
		 {
			  // Given
			  Key<string> someKey = key( "someKey", typeof( string ) );

			  _procs.register( new CallableProcedure_BasicProcedureAnonymousInnerClass( this, _signature, someKey ) );

			  BasicContext ctx = new BasicContext();
			  ctx.Put( someKey, "hello, world" );

			  // When
			  RawIterator<object[], ProcedureException> result = _procs.callProcedure( ctx, _signature.name(), new object[0], _resourceTracker );

			  // Then
			  assertThat( asList( result ), contains( equalTo( new object[]{ "hello, world" } ) ) );
		 }

		 private class CallableProcedure_BasicProcedureAnonymousInnerClass : Neo4Net.Kernel.api.proc.CallableProcedure_BasicProcedure
		 {
			 private readonly ProceduresTest _outerInstance;

			 private Key<string> _someKey;

			 public CallableProcedure_BasicProcedureAnonymousInnerClass( ProceduresTest outerInstance, ProcedureSignature signature, Key<string> someKey ) : base( signature )
			 {
				 this.outerInstance = outerInstance;
				 this._someKey = someKey;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.collection.RawIterator<Object[], org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException> apply(org.Neo4Net.kernel.api.proc.Context ctx, Object[] input, org.Neo4Net.kernel.api.ResourceTracker resourceTracker) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
			 public override RawIterator<object[], ProcedureException> apply( Context ctx, object[] input, ResourceTracker resourceTracker )
			 {
				  return RawIterator.of<object[], ProcedureException>( new object[]{ ctx.Get( _someKey ) } );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailCompileProcedureWithReadConflict() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailCompileProcedureWithReadConflict()
		 {
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( "Conflicting procedure annotation, cannot use PerformsWrites and mode" );
			  _procs.registerProcedure( typeof( ProcedureWithReadConflictAnnotation ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailCompileProcedureWithWriteConflict() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailCompileProcedureWithWriteConflict()
		 {
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( "Conflicting procedure annotation, cannot use PerformsWrites and mode" );
			  _procs.registerProcedure( typeof( ProcedureWithWriteConflictAnnotation ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailCompileProcedureWithSchemaConflict() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailCompileProcedureWithSchemaConflict()
		 {
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( "Conflicting procedure annotation, cannot use PerformsWrites and mode" );
			  _procs.registerProcedure( typeof( ProcedureWithSchemaConflictAnnotation ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailCompileProcedureWithDBMSConflict() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailCompileProcedureWithDBMSConflict()
		 {
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( "Conflicting procedure annotation, cannot use PerformsWrites and mode" );
			  _procs.registerProcedure( typeof( ProcedureWithDBMSConflictAnnotation ) );
		 }

		 public class ProcedureWithReadConflictAnnotation
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PerformsWrites @Procedure(mode = org.Neo4Net.procedure.Mode.READ) public void shouldCompile()
			  [Procedure(mode : Neo4Net.Procedure.Mode.READ)]
			  public virtual void ShouldCompile()
			  {
			  }
		 }

		 public class ProcedureWithWriteConflictAnnotation
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PerformsWrites @Procedure(mode = org.Neo4Net.procedure.Mode.WRITE) public void shouldCompileToo()
			  [Procedure(mode : Neo4Net.Procedure.Mode.WRITE)]
			  public virtual void ShouldCompileToo()
			  {
			  }
		 }

		 public class ProcedureWithDBMSConflictAnnotation
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PerformsWrites @Procedure(mode = org.Neo4Net.procedure.Mode.DBMS) public void shouldNotCompile()
			  [Procedure(mode : Neo4Net.Procedure.Mode.DBMS)]
			  public virtual void ShouldNotCompile()
			  {
			  }
		 }

		 public class ProcedureWithSchemaConflictAnnotation
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PerformsWrites @Procedure(mode = org.Neo4Net.procedure.Mode.SCHEMA) public void shouldNotCompile()
			  [Procedure(mode : Neo4Net.Procedure.Mode.SCHEMA)]
			  public virtual void ShouldNotCompile()
			  {
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private org.Neo4Net.kernel.api.proc.CallableProcedure_BasicProcedure procedureWithSignature(final org.Neo4Net.Kernel.Api.Internal.procs.ProcedureSignature signature)
		 private Neo4Net.Kernel.api.proc.CallableProcedure_BasicProcedure ProcedureWithSignature( ProcedureSignature signature )
		 {
			  return new CallableProcedure_BasicProcedureAnonymousInnerClass( this, signature );
		 }

		 private class CallableProcedure_BasicProcedureAnonymousInnerClass : Neo4Net.Kernel.api.proc.CallableProcedure_BasicProcedure
		 {
			 private readonly ProceduresTest _outerInstance;

			 public CallableProcedure_BasicProcedureAnonymousInnerClass( ProceduresTest outerInstance, ProcedureSignature signature ) : base( signature )
			 {
				 this.outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.collection.RawIterator<Object[], org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException> apply(org.Neo4Net.kernel.api.proc.Context ctx, Object[] input, org.Neo4Net.kernel.api.ResourceTracker resourceTracker) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
			 public override RawIterator<object[], ProcedureException> apply( Context ctx, object[] input, ResourceTracker resourceTracker )
			 {
				  return null;
			 }
		 }

		 private CallableProcedure Procedure( ProcedureSignature signature )
		 {
			  return new CallableProcedure_BasicProcedureAnonymousInnerClass2( this, signature );
		 }

		 private class CallableProcedure_BasicProcedureAnonymousInnerClass2 : Neo4Net.Kernel.api.proc.CallableProcedure_BasicProcedure
		 {
			 private readonly ProceduresTest _outerInstance;

			 public CallableProcedure_BasicProcedureAnonymousInnerClass2( ProceduresTest outerInstance, ProcedureSignature signature ) : base( signature )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override RawIterator<object[], ProcedureException> apply( Context ctx, object[] input, ResourceTracker resourceTracker )
			 {
				  return RawIterator.of<object[], ProcedureException>( input );
			 }
		 }
	}

}