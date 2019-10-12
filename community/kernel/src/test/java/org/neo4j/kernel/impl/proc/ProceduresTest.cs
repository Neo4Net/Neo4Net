using System.Collections.Generic;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Kernel.impl.proc
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;

	using Org.Neo4j.Collection;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using ProcedureException = Org.Neo4j.@internal.Kernel.Api.exceptions.ProcedureException;
	using ProcedureSignature = Org.Neo4j.@internal.Kernel.Api.procs.ProcedureSignature;
	using ResourceTracker = Org.Neo4j.Kernel.api.ResourceTracker;
	using StubResourceManager = Org.Neo4j.Kernel.api.StubResourceManager;
	using BasicContext = Org.Neo4j.Kernel.api.proc.BasicContext;
	using CallableProcedure = Org.Neo4j.Kernel.api.proc.CallableProcedure;
	using Context = Org.Neo4j.Kernel.api.proc.Context;
	using Org.Neo4j.Kernel.api.proc;
	using Mode = Org.Neo4j.Procedure.Mode;
	using PerformsWrites = Org.Neo4j.Procedure.PerformsWrites;
	using Procedure = Org.Neo4j.Procedure.Procedure;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.Neo4jTypes.NTAny;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.Neo4jTypes.NTInteger;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.Neo4jTypes.NTString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.ProcedureSignature.procedureSignature;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.proc.Key.key;

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

		 private class CallableProcedure_BasicProcedureAnonymousInnerClass : Org.Neo4j.Kernel.api.proc.CallableProcedure_BasicProcedure
		 {
			 private readonly ProceduresTest _outerInstance;

			 private Key<string> _someKey;

			 public CallableProcedure_BasicProcedureAnonymousInnerClass( ProceduresTest outerInstance, ProcedureSignature signature, Key<string> someKey ) : base( signature )
			 {
				 this.outerInstance = outerInstance;
				 this._someKey = someKey;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.collection.RawIterator<Object[], org.neo4j.internal.kernel.api.exceptions.ProcedureException> apply(org.neo4j.kernel.api.proc.Context ctx, Object[] input, org.neo4j.kernel.api.ResourceTracker resourceTracker) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
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
//ORIGINAL LINE: @PerformsWrites @Procedure(mode = org.neo4j.procedure.Mode.READ) public void shouldCompile()
			  [Procedure(mode : Org.Neo4j.Procedure.Mode.READ)]
			  public virtual void ShouldCompile()
			  {
			  }
		 }

		 public class ProcedureWithWriteConflictAnnotation
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PerformsWrites @Procedure(mode = org.neo4j.procedure.Mode.WRITE) public void shouldCompileToo()
			  [Procedure(mode : Org.Neo4j.Procedure.Mode.WRITE)]
			  public virtual void ShouldCompileToo()
			  {
			  }
		 }

		 public class ProcedureWithDBMSConflictAnnotation
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PerformsWrites @Procedure(mode = org.neo4j.procedure.Mode.DBMS) public void shouldNotCompile()
			  [Procedure(mode : Org.Neo4j.Procedure.Mode.DBMS)]
			  public virtual void ShouldNotCompile()
			  {
			  }
		 }

		 public class ProcedureWithSchemaConflictAnnotation
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PerformsWrites @Procedure(mode = org.neo4j.procedure.Mode.SCHEMA) public void shouldNotCompile()
			  [Procedure(mode : Org.Neo4j.Procedure.Mode.SCHEMA)]
			  public virtual void ShouldNotCompile()
			  {
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private org.neo4j.kernel.api.proc.CallableProcedure_BasicProcedure procedureWithSignature(final org.neo4j.internal.kernel.api.procs.ProcedureSignature signature)
		 private Org.Neo4j.Kernel.api.proc.CallableProcedure_BasicProcedure ProcedureWithSignature( ProcedureSignature signature )
		 {
			  return new CallableProcedure_BasicProcedureAnonymousInnerClass( this, signature );
		 }

		 private class CallableProcedure_BasicProcedureAnonymousInnerClass : Org.Neo4j.Kernel.api.proc.CallableProcedure_BasicProcedure
		 {
			 private readonly ProceduresTest _outerInstance;

			 public CallableProcedure_BasicProcedureAnonymousInnerClass( ProceduresTest outerInstance, ProcedureSignature signature ) : base( signature )
			 {
				 this.outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.collection.RawIterator<Object[], org.neo4j.internal.kernel.api.exceptions.ProcedureException> apply(org.neo4j.kernel.api.proc.Context ctx, Object[] input, org.neo4j.kernel.api.ResourceTracker resourceTracker) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
			 public override RawIterator<object[], ProcedureException> apply( Context ctx, object[] input, ResourceTracker resourceTracker )
			 {
				  return null;
			 }
		 }

		 private CallableProcedure Procedure( ProcedureSignature signature )
		 {
			  return new CallableProcedure_BasicProcedureAnonymousInnerClass2( this, signature );
		 }

		 private class CallableProcedure_BasicProcedureAnonymousInnerClass2 : Org.Neo4j.Kernel.api.proc.CallableProcedure_BasicProcedure
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