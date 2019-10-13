using System;
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
	using BaseMatcher = org.hamcrest.BaseMatcher;
	using Description = org.hamcrest.Description;
	using Matchers = org.hamcrest.Matchers;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;


	using Neo4Net.Collections;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using KernelException = Neo4Net.@internal.Kernel.Api.exceptions.KernelException;
	using ProcedureException = Neo4Net.@internal.Kernel.Api.exceptions.ProcedureException;
	using Neo4jTypes = Neo4Net.@internal.Kernel.Api.procs.Neo4jTypes;
	using ResourceTracker = Neo4Net.Kernel.api.ResourceTracker;
	using StubResourceManager = Neo4Net.Kernel.api.StubResourceManager;
	using ResourceCloseFailureException = Neo4Net.Kernel.Api.Exceptions.ResourceCloseFailureException;
	using BasicContext = Neo4Net.Kernel.api.proc.BasicContext;
	using CallableProcedure = Neo4Net.Kernel.api.proc.CallableProcedure;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Log = Neo4Net.Logging.Log;
	using NullLog = Neo4Net.Logging.NullLog;
	using Context = Neo4Net.Procedure.Context;
	using Procedure = Neo4Net.Procedure.Procedure;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.Is.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.spy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.procedure_whitelist;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.ProcedureSignature.procedureSignature;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("WeakerAccess") public class ReflectiveProcedureTest
	public class ReflectiveProcedureTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException exception = org.junit.rules.ExpectedException.none();
		 public ExpectedException Exception = ExpectedException.none();

		 private ReflectiveProcedureCompiler _procedureCompiler;
		 private ComponentRegistry _components;
		 private readonly ResourceTracker _resourceTracker = new StubResourceManager();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _components = new ComponentRegistry();
			  _procedureCompiler = new ReflectiveProcedureCompiler( new TypeMappers(), _components, _components, NullLog.Instance, ProcedureConfig.Default );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInjectLogging() throws org.neo4j.internal.kernel.api.exceptions.KernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInjectLogging()
		 {
			  // Given
			  Log log = spy( typeof( Log ) );
			  _components.register( typeof( Log ), ctx => log );
			  CallableProcedure procedure = _procedureCompiler.compileProcedure( typeof( LoggingProcedure ), null, true )[0];

			  // When
			  procedure.Apply( new BasicContext(), new object[0], _resourceTracker );

			  // Then
			  verify( log ).debug( "1" );
			  verify( log ).info( "2" );
			  verify( log ).warn( "3" );
			  verify( log ).error( "4" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCompileProcedure() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCompileProcedure()
		 {
			  // When
			  IList<CallableProcedure> procedures = Compile( typeof( SingleReadOnlyProcedure ) );

			  // Then
			  assertEquals( 1, procedures.Count );
			  assertThat( procedures[0].Signature(), Matchers.equalTo(procedureSignature("org", "neo4j", "kernel", "impl", "proc", "listCoolPeople").@out("name", Neo4jTypes.NTString).build()) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRunSimpleReadOnlyProcedure() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRunSimpleReadOnlyProcedure()
		 {
			  // Given
			  CallableProcedure proc = Compile( typeof( SingleReadOnlyProcedure ) )[0];

			  // When
			  RawIterator<object[], ProcedureException> @out = proc.Apply( new BasicContext(), new object[0], _resourceTracker );

			  // Then
			  assertThat( asList( @out ), contains( new object[]{ "Bonnie" }, new object[]{ "Clyde" } ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreClassesWithNoProcedures() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIgnoreClassesWithNoProcedures()
		 {
			  // When
			  IList<CallableProcedure> procedures = Compile( typeof( PrivateConstructorButNoProcedures ) );

			  // Then
			  assertEquals( 0, procedures.Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRunClassWithMultipleProceduresDeclared() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRunClassWithMultipleProceduresDeclared()
		 {
			  // Given
			  IList<CallableProcedure> compiled = Compile( typeof( MultiProcedureProcedure ) );
			  CallableProcedure bananaPeople = compiled[0];
			  CallableProcedure coolPeople = compiled[1];

			  // When
			  RawIterator<object[], ProcedureException> coolOut = coolPeople.Apply( new BasicContext(), new object[0], _resourceTracker );
			  RawIterator<object[], ProcedureException> bananaOut = bananaPeople.Apply( new BasicContext(), new object[0], _resourceTracker );

			  // Then
			  assertThat( asList( coolOut ), contains( new object[]{ "Bonnie" }, new object[]{ "Clyde" } ) );

			  assertThat( asList( bananaOut ), contains( new object[]{ "Jake", 18L }, new object[]{ "Pontus", 2L } ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveHelpfulErrorOnConstructorThatRequiresArgument() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGiveHelpfulErrorOnConstructorThatRequiresArgument()
		 {
			  // Expect
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( "Unable to find a usable public no-argument constructor " + "in the class `WierdConstructorProcedure`. Please add a " + "valid, public constructor, recompile the class and try again." );

			  // When
			  Compile( typeof( WierdConstructorProcedure ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveHelpfulErrorOnNoPublicConstructor() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGiveHelpfulErrorOnNoPublicConstructor()
		 {
			  // Expect
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( "Unable to find a usable public no-argument constructor " + "in the class `PrivateConstructorProcedure`. Please add " + "a valid, public constructor, recompile the class and try again." );

			  // When
			  Compile( typeof( PrivateConstructorProcedure ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowVoidOutput() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowVoidOutput()
		 {
			  // When
			  CallableProcedure proc = Compile( typeof( ProcedureWithVoidOutput ) )[0];

			  // Then
			  assertEquals( 0, proc.Signature().outputSignature().size() );
			  assertFalse( proc.Apply( null, new object[0], _resourceTracker ).hasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveHelpfulErrorOnProcedureReturningInvalidRecordType() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGiveHelpfulErrorOnProcedureReturningInvalidRecordType()
		 {
			  // Expect
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( string.Format( "Procedures must return a Stream of records, where a record is a concrete class%n" + "that you define, with public non-final fields defining the fields in the record.%n" + "If you''d like your procedure to return `String`, you could define a record class " + "like:%n" + "public class Output '{'%n" + "    public String out;%n" + "'}'%n" + "%n" + "And then define your procedure as returning `Stream<Output>`." ) );

			  // When
			  Compile( typeof( ProcedureWithInvalidRecordOutput ) )[0];
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveHelpfulErrorOnContextAnnotatedStaticField() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGiveHelpfulErrorOnContextAnnotatedStaticField()
		 {
			  // Expect
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( string.Format( "The field `gdb` in the class named `ProcedureWithStaticContextAnnotatedField` is " + "annotated as a @Context field,%n" + "but it is static. @Context fields must be public, non-final and non-static,%n" + "because they are reset each time a procedure is invoked." ) );

			  // When
			  Compile( typeof( ProcedureWithStaticContextAnnotatedField ) )[0];
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowNonStaticOutput() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowNonStaticOutput()
		 {
			  // When
			  CallableProcedure proc = Compile( typeof( ProcedureWithNonStaticOutputRecord ) )[0];

			  // Then
			  assertEquals( 1, proc.Signature().outputSignature().size() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowOverridingProcedureName() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowOverridingProcedureName()
		 {
			  // When
			  CallableProcedure proc = Compile( typeof( ProcedureWithOverriddenName ) )[0];

			  // Then
			  assertEquals( "org.mystuff.thisisActuallyTheName", proc.Signature().name().ToString() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowOverridingProcedureNameWithoutNamespace() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowOverridingProcedureNameWithoutNamespace()
		 {
			  // When
			  CallableProcedure proc = Compile( typeof( ProcedureWithSingleName ) )[0];

			  // Then
			  assertEquals( "singleName", proc.Signature().name().ToString() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveHelpfulErrorOnNullMessageException() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGiveHelpfulErrorOnNullMessageException()
		 {
			  // Given
			  CallableProcedure proc = Compile( typeof( ProcedureThatThrowsNullMsgExceptionAtInvocation ) )[0];

			  // Expect
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( "Failed to invoke procedure `org.neo4j.kernel.impl.proc.throwsAtInvocation`: " + "Caused by: java.lang.IndexOutOfBoundsException" );

			  // When
			  proc.Apply( new BasicContext(), new object[0], _resourceTracker );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseResourcesAndGiveHelpfulErrorOnMidStreamException() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCloseResourcesAndGiveHelpfulErrorOnMidStreamException()
		 {
			  // Given
			  CallableProcedure proc = Compile( typeof( ProcedureThatThrowsNullMsgExceptionMidStream ) )[0];

			  // Expect
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( "Failed to invoke procedure `org.neo4j.kernel.impl.proc.throwsInStream`: " + "Caused by: java.lang.IndexOutOfBoundsException" );

			  // Expect that we get a suppressed exception from Stream.onClose (which also verifies that we actually call
			  // onClose on the first exception)
			  Exception.expect( new BaseMatcherAnonymousInnerClass( this ) );

			  // When
			  RawIterator<object[], ProcedureException> stream = proc.Apply( new BasicContext(), new object[0], _resourceTracker );
			  if ( stream.HasNext() )
			  {
					stream.Next();
			  }
		 }

		 private class BaseMatcherAnonymousInnerClass : BaseMatcher<Exception>
		 {
			 private readonly ReflectiveProcedureTest _outerInstance;

			 public BaseMatcherAnonymousInnerClass( ReflectiveProcedureTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( "a suppressed exception with cause ExceptionDuringClose" );
			 }

			 public override bool matches( object item )
			 {
				  Exception e = ( Exception ) item;
				  foreach ( Exception suppressed in e.Suppressed )
				  {
						if ( suppressed is ResourceCloseFailureException )
						{
							 if ( suppressed.InnerException is ExceptionDuringClose )
							 {
								  return true;
							 }
						}
				  }
				  return false;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSupportProcedureDeprecation() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSupportProcedureDeprecation()
		 {
			  // Given
			  Log log = mock( typeof( Log ) );
			  ReflectiveProcedureCompiler procedureCompiler = new ReflectiveProcedureCompiler( new TypeMappers(), _components, _components, log, ProcedureConfig.Default );

			  // When
			  IList<CallableProcedure> procs = procedureCompiler.CompileProcedure( typeof( ProcedureWithDeprecation ), null, true );

			  // Then
			  verify( log ).warn( "Use of @Procedure(deprecatedBy) without @Deprecated in badProc" );
			  verifyNoMoreInteractions( log );
			  foreach ( CallableProcedure proc in procs )
			  {
					string name = proc.Signature().name().name();
					proc.Apply( new BasicContext(), new object[0], _resourceTracker );
					switch ( name )
					{
					case "newProc":
						 assertFalse( "Should not be deprecated", proc.Signature().deprecated().Present );
						 break;
					case "oldProc":
					case "badProc":
						 assertTrue( "Should be deprecated", proc.Signature().deprecated().Present );
						 assertThat( proc.Signature().deprecated().get(), equalTo("newProc") );
						 break;
					default:
						 fail( "Unexpected procedure: " + name );
					 break;
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLoadWhiteListedProcedure() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLoadWhiteListedProcedure()
		 {
			  // Given
			  ProcedureConfig config = new ProcedureConfig( Config.defaults( procedure_whitelist, "org.neo4j.kernel.impl.proc.listCoolPeople" ) );

			  Log log = mock( typeof( Log ) );
			  ReflectiveProcedureCompiler procedureCompiler = new ReflectiveProcedureCompiler( new TypeMappers(), _components, _components, log, config );

			  // When
			  CallableProcedure proc = procedureCompiler.CompileProcedure( typeof( SingleReadOnlyProcedure ), null, false )[0];
			  // When
			  RawIterator<object[], ProcedureException> result = proc.Apply( new BasicContext(), new object[0], _resourceTracker );

			  // Then
			  assertEquals( result.Next()[0], "Bonnie" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotLoadNoneWhiteListedProcedure() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotLoadNoneWhiteListedProcedure()
		 {
			  // Given
			  ProcedureConfig config = new ProcedureConfig( Config.defaults( procedure_whitelist, "org.neo4j.kernel.impl.proc.NOTlistCoolPeople" ) );

			  Log log = mock( typeof( Log ) );
			  ReflectiveProcedureCompiler procedureCompiler = new ReflectiveProcedureCompiler( new TypeMappers(), _components, _components, log, config );

			  // When
			  IList<CallableProcedure> proc = procedureCompiler.CompileProcedure( typeof( SingleReadOnlyProcedure ), null, false );
			  // Then
			  verify( log ).warn( "The procedure 'org.neo4j.kernel.impl.proc.listCoolPeople' is not on the whitelist and won't be loaded." );
			  assertThat( proc.Count == 0, @is( true ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreWhiteListingIfFullAccess() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIgnoreWhiteListingIfFullAccess()
		 {
			  // Given
			  ProcedureConfig config = new ProcedureConfig( Config.defaults( procedure_whitelist, "empty" ) );
			  Log log = mock( typeof( Log ) );
			  ReflectiveProcedureCompiler procedureCompiler = new ReflectiveProcedureCompiler( new TypeMappers(), _components, _components, log, config );

			  // When
			  CallableProcedure proc = procedureCompiler.CompileProcedure( typeof( SingleReadOnlyProcedure ), null, true )[0];
			  // Then
			  RawIterator<object[], ProcedureException> result = proc.Apply( new BasicContext(), new object[0], _resourceTracker );
			  assertEquals( result.Next()[0], "Bonnie" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotLoadAnyProcedureIfConfigIsEmpty() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotLoadAnyProcedureIfConfigIsEmpty()
		 {
			  // Given
			  ProcedureConfig config = new ProcedureConfig( Config.defaults( procedure_whitelist, "" ) );
			  Log log = mock( typeof( Log ) );
			  ReflectiveProcedureCompiler procedureCompiler = new ReflectiveProcedureCompiler( new TypeMappers(), _components, _components, log, config );

			  // When
			  IList<CallableProcedure> proc = procedureCompiler.CompileProcedure( typeof( SingleReadOnlyProcedure ), null, false );
			  // Then
			  verify( log ).warn( "The procedure 'org.neo4j.kernel.impl.proc.listCoolPeople' is not on the whitelist and won't be loaded." );
			  assertThat( proc.Count == 0, @is( true ) );
		 }

		 public class MyOutputRecord
		 {
			  public string Name;

			  public MyOutputRecord( string name )
			  {
					this.Name = name;
			  }
		 }

		 public class SomeOtherOutputRecord
		 {
			  public string Name;
			  public long Bananas;

			  public SomeOtherOutputRecord( string name, long bananas )
			  {
					this.Name = name;
					this.Bananas = bananas;
			  }
		 }

		 public class LoggingProcedure
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.neo4j.logging.Log log;
			  public Log Log;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<MyOutputRecord> logAround()
			  public virtual Stream<MyOutputRecord> LogAround()
			  {
					Log.debug( "1" );
					Log.info( "2" );
					Log.warn( "3" );
					Log.error( "4" );
					return Stream.empty();
			  }
		 }

		 public class SingleReadOnlyProcedure
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<MyOutputRecord> listCoolPeople()
			  public virtual Stream<MyOutputRecord> ListCoolPeople()
			  {
					return Stream.of( new MyOutputRecord( "Bonnie" ), new MyOutputRecord( "Clyde" ) );
			  }
		 }

		 public class ProcedureWithVoidOutput
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public void voidOutput()
			  public virtual void VoidOutput()
			  {
			  }
		 }

		 public class ProcedureWithNonStaticOutputRecord
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<NonStatic> voidOutput()
			  public virtual Stream<NonStatic> VoidOutput()
			  {
					return Stream.of( new NonStatic( this ) );
			  }

			  public class NonStatic
			  {
				  private readonly ReflectiveProcedureTest.ProcedureWithNonStaticOutputRecord _outerInstance;

				  public NonStatic( ReflectiveProcedureTest.ProcedureWithNonStaticOutputRecord outerInstance )
				  {
					  this._outerInstance = outerInstance;
				  }

					public string Field = "hello, rodl!";
			  }
		 }

		 public class MultiProcedureProcedure
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<MyOutputRecord> listCoolPeople()
			  public virtual Stream<MyOutputRecord> ListCoolPeople()
			  {
					return Stream.of( new MyOutputRecord( "Bonnie" ), new MyOutputRecord( "Clyde" ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<SomeOtherOutputRecord> listBananaOwningPeople()
			  public virtual Stream<SomeOtherOutputRecord> ListBananaOwningPeople()
			  {
					return Stream.of( new SomeOtherOutputRecord( "Jake", 18 ), new SomeOtherOutputRecord( "Pontus", 2 ) );
			  }
		 }

		 public class WierdConstructorProcedure
		 {
			  public WierdConstructorProcedure( WierdConstructorProcedure wat )
			  {

			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<MyOutputRecord> listCoolPeople()
			  public virtual Stream<MyOutputRecord> ListCoolPeople()
			  {
					return Stream.of( new MyOutputRecord( "Bonnie" ), new MyOutputRecord( "Clyde" ) );
			  }
		 }

		 public class ProcedureWithInvalidRecordOutput
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public String test()
			  public virtual string Test()
			  {
					return "Testing";
			  }
		 }

		 public class ProcedureWithStaticContextAnnotatedField
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public static org.neo4j.graphdb.GraphDatabaseService gdb;
			  public static GraphDatabaseService Gdb;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<MyOutputRecord> test()
			  public virtual Stream<MyOutputRecord> Test()
			  {
					return null;
			  }
		 }

		 public class ProcedureThatThrowsNullMsgExceptionAtInvocation
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<MyOutputRecord> throwsAtInvocation()
			  public virtual Stream<MyOutputRecord> ThrowsAtInvocation()
			  {
					throw new System.IndexOutOfRangeException();
			  }
		 }

		 public class ProcedureThatThrowsNullMsgExceptionMidStream
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<MyOutputRecord> throwsInStream()
			  public virtual Stream<MyOutputRecord> ThrowsInStream()
			  {
					return Stream.generate<MyOutputRecord>(() =>
					{
					 throw new System.IndexOutOfRangeException();
					}).onClose(() =>
					{
					 throw new ExceptionDuringClose();
				});
			  }
		 }

		 public class PrivateConstructorProcedure
		 {
			  internal PrivateConstructorProcedure()
			  {

			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<MyOutputRecord> listCoolPeople()
			  public virtual Stream<MyOutputRecord> ListCoolPeople()
			  {
					return Stream.of( new MyOutputRecord( "Bonnie" ), new MyOutputRecord( "Clyde" ) );
			  }
		 }

		 public class PrivateConstructorButNoProcedures
		 {
			  internal PrivateConstructorButNoProcedures()
			  {

			  }

			  public virtual Stream<MyOutputRecord> ThisIsNotAProcedure()
			  {
					return null;
			  }
		 }

		 public class ProcedureWithOverriddenName
		 {
			  [Procedure("org.mystuff.thisisActuallyTheName")]
			  public virtual void SomethingThatShouldntMatter()
			  {

			  }

			  [Procedure("singleName")]
			  public virtual void BlahDoesntMatterEither()
			  {

			  }
		 }

		 public class ProcedureWithSingleName
		 {
			  [Procedure("singleName")]
			  public virtual void BlahDoesntMatterEither()
			  {

			  }
		 }

		 public class ProcedureWithDeprecation
		 {
			  [Procedure("newProc")]
			  public virtual void NewProc()
			  {
			  }

			  [Obsolete, Procedure(value : "oldProc", deprecatedBy : "newProc")]
			  public virtual void OldProc()
			  {
			  }

			  [Procedure(value : "badProc", deprecatedBy : "newProc")]
			  public virtual void BadProc()
			  {
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.List<org.neo4j.kernel.api.proc.CallableProcedure> compile(Class clazz) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 private IList<CallableProcedure> Compile( Type clazz )
		 {
			  return _procedureCompiler.compileProcedure( clazz, null, true );
		 }

		 private class ExceptionDuringClose : Exception
		 {
		 }
	}

}