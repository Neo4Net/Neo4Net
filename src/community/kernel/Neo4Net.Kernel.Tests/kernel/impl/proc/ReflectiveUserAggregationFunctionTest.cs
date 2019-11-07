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
	using Matchers = org.hamcrest.Matchers;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;


	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using KernelException = Neo4Net.Kernel.Api.Internal.Exceptions.KernelException;
	using ProcedureException = Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
	using Neo4NetTypes = Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes;
	using UserAggregator = Neo4Net.Kernel.Api.Internal.procs.UserAggregator;
	using BasicContext = Neo4Net.Kernel.Api.Procs.BasicContext;
	using CallableUserAggregationFunction = Neo4Net.Kernel.Api.Procs.CallableUserAggregationFunction;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Log = Neo4Net.Logging.Log;
	using NullLog = Neo4Net.Logging.NullLog;
	using Context = Neo4Net.Procedure.Context;
	using Name = Neo4Net.Procedure.Name;
	using UserAggregationFunction = Neo4Net.Procedure.UserAggregationFunction;
	using UserAggregationResult = Neo4Net.Procedure.UserAggregationResult;
	using UserAggregationUpdate = Neo4Net.Procedure.UserAggregationUpdate;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
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
//	import static Neo4Net.Kernel.Api.Internal.procs.UserFunctionSignature.functionSignature;

	public class ReflectiveUserAggregationFunctionTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException exception = org.junit.rules.ExpectedException.none();
		 public ExpectedException Exception = ExpectedException.none();

		 private ReflectiveProcedureCompiler _procedureCompiler;
		 private ComponentRegistry _components;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _components = new ComponentRegistry();
			  _procedureCompiler = new ReflectiveProcedureCompiler( new TypeMappers(), _components, _components, NullLog.Instance, ProcedureConfig.Default );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCompileAggregationFunction() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCompileAggregationFunction()
		 {
			  // When
			  IList<CallableUserAggregationFunction> function = Compile( typeof( SingleAggregationFunction ) );

			  // Then
			  assertEquals( 1, function.Count );
			  assertThat( function[0].Signature(), Matchers.equalTo(functionSignature("org", "Neo4Net", "kernel", "impl", "proc", "collectCool").@in("name", Neo4NetTypes.NTString).@out(Neo4NetTypes.NTList(Neo4NetTypes.NTAny)).build()) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRunAggregationFunction() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRunAggregationFunction()
		 {
			  // Given
			  CallableUserAggregationFunction func = Compile( typeof( SingleAggregationFunction ) )[0];

			  // When
			  UserAggregator aggregator = func.Create( new BasicContext() );

			  aggregator.Update( new object[]{ "Harry" } );
			  aggregator.Update( new object[]{ "Bonnie" } );
			  aggregator.Update( new object[]{ "Sally" } );
			  aggregator.Update( new object[]{ "Clyde" } );

			  // Then
			  assertThat( aggregator.Result(), equalTo(Arrays.asList("Bonnie", "Clyde")) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInjectLogging() throws Neo4Net.Kernel.Api.Internal.Exceptions.KernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInjectLogging()
		 {
			  // Given
			  Log log = spy( typeof( Log ) );
			  _components.register( typeof( Log ), ctx => log );
			  CallableUserAggregationFunction function = _procedureCompiler.compileAggregationFunction( typeof( LoggingFunction ) )[0];

			  // When
			  UserAggregator aggregator = function.Create( new BasicContext() );
			  aggregator.Update( new object[]{} );
			  aggregator.Result();

			  // Then
			  verify( log ).debug( "1" );
			  verify( log ).info( "2" );
			  verify( log ).warn( "3" );
			  verify( log ).error( "4" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreClassesWithNoFunctions() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIgnoreClassesWithNoFunctions()
		 {
			  // When
			  IList<CallableUserAggregationFunction> functions = Compile( typeof( PrivateConstructorButNoFunctions ) );

			  // Then
			  assertEquals( 0, functions.Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRunClassWithMultipleFunctionsDeclared() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRunClassWithMultipleFunctionsDeclared()
		 {
			  // Given
			  IList<CallableUserAggregationFunction> compiled = Compile( typeof( MultiFunction ) );
			  CallableUserAggregationFunction f1 = compiled[0];
			  CallableUserAggregationFunction f2 = compiled[1];

			  // When
			  UserAggregator f1Aggregator = f1.Create( new BasicContext() );
			  f1Aggregator.Update( new object[]{ "Bonnie" } );
			  f1Aggregator.Update( new object[]{ "Clyde" } );
			  UserAggregator f2Aggregator = f2.Create( new BasicContext() );
			  f2Aggregator.Update( new object[]{ "Bonnie", 1337L } );
			  f2Aggregator.Update( new object[]{ "Bonnie", 42L } );

			  // Then
			  assertThat( f1Aggregator.Result(), equalTo(Arrays.asList("Bonnie", "Clyde")) );
			  assertThat( ( ( System.Collections.IDictionary ) f2Aggregator.Result() )["Bonnie"], equalTo(1337L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveHelpfulErrorOnConstructorThatRequiresArgument() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGiveHelpfulErrorOnConstructorThatRequiresArgument()
		 {
			  // Expect
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( "Unable to find a usable public no-argument constructor " + "in the class `WierdConstructorFunction`. Please add a " + "valid, public constructor, recompile the class and try again." );

			  // When
			  Compile( typeof( WierdConstructorFunction ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveHelpfulErrorOnNoPublicConstructor() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGiveHelpfulErrorOnNoPublicConstructor()
		 {
			  // Expect
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( "Unable to find a usable public no-argument constructor " + "in the class `PrivateConstructorFunction`. Please add " + "a valid, public constructor, recompile the class and try again." );

			  // When
			  Compile( typeof( PrivateConstructorFunction ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowVoidOutput() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowVoidOutput()
		 {
			  // Expect
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( "Don't know how to map `void` to the Neo4Net Type System." );

			  // When
			  Compile( typeof( FunctionWithVoidOutput ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowNonVoidUpdate() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowNonVoidUpdate()
		 {
			  // Expect
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( "Update method 'update' in VoidOutput has type 'long' but must have return type 'void'." );

			  // When
			  Compile( typeof( FunctionWithNonVoidUpdate ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowMissingAnnotations() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowMissingAnnotations()
		 {
			  // Expect
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( "Class 'MissingAggregator' must contain methods annotated with " + "both '@UserAggregationResult' as well as '@UserAggregationUpdate'." );

			  // When
			  Compile( typeof( FunctionWithMissingAnnotations ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowMultipleUpdateAnnotations() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowMultipleUpdateAnnotations()
		 {
			  // Expect
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( "Class 'MissingAggregator' contains multiple methods annotated " + "with '@UserAggregationUpdate'." );

			  // When
			  Compile( typeof( FunctionWithDuplicateUpdateAnnotations ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowMultipleResultAnnotations() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowMultipleResultAnnotations()
		 {
			  // Expect
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( "Class 'MissingAggregator' contains multiple methods annotated " + "with '@UserAggregationResult'." );

			  // When
			  Compile( typeof( FunctionWithDuplicateResultAnnotations ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowNonPublicMethod() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowNonPublicMethod()
		 {
			  // Expect
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( "Aggregation method 'test' in NonPublicTestMethod must be public." );

			  // When
			  Compile( typeof( NonPublicTestMethod ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowNonPublicUpdateMethod() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowNonPublicUpdateMethod()
		 {
			  // Expect
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( "Aggregation update method 'update' in InnerAggregator must be public." );

			  // When
			  Compile( typeof( NonPublicUpdateMethod ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowNonPublicResultMethod() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowNonPublicResultMethod()
		 {
			  // Expect
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( "Aggregation result method 'result' in InnerAggregator must be public." );

			  // When
			  Compile( typeof( NonPublicResultMethod ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveHelpfulErrorOnFunctionReturningInvalidType() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGiveHelpfulErrorOnFunctionReturningInvalidType()
		 {
			  // Expect
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( string.Format( "Don't know how to map `char[]` to the Neo4Net Type System.%n" + "Please refer to to the documentation for full details.%n" + "For your reference, known types are: [boolean, byte[], double, java.lang.Boolean, " + "java.lang.Double, java.lang.Long, java.lang.Number, java.lang.Object, " + "java.lang.String, java.time.LocalDate, java.time.LocalDateTime, " + "java.time.LocalTime, java.time.OffsetTime, java.time.ZonedDateTime, " + "java.time.temporal.TemporalAmount, java.util.List, java.util.Map, long]" ) );

			  // When
			  Compile( typeof( FunctionWithInvalidOutput ) )[0];
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveHelpfulErrorOnContextAnnotatedStaticField() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGiveHelpfulErrorOnContextAnnotatedStaticField()
		 {
			  // Expect
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( string.Format( "The field `gdb` in the class named `FunctionWithStaticContextAnnotatedField` is " + "annotated as a @Context field,%n" + "but it is static. @Context fields must be public, non-final and non-static,%n" + "because they are reset each time a procedure is invoked." ) );

			  // When
			  Compile( typeof( FunctionWithStaticContextAnnotatedField ) )[0];
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowOverridingProcedureName() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowOverridingProcedureName()
		 {
			  // When
			  CallableUserAggregationFunction method = Compile( typeof( FunctionWithOverriddenName ) )[0];

			  // Then
			  assertEquals( "org.mystuff.thisisActuallyTheName", method.Signature().name().ToString() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowOverridingFunctionNameWithoutNamespace() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowOverridingFunctionNameWithoutNamespace()
		 {
			  // Expect
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( "It is not allowed to define functions in the root namespace please use a " + "namespace, e.g. `@UserFunction(\"org.example.com.singleName\")" );

			  // When
			  Compile( typeof( FunctionWithSingleName ) )[0];
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveHelpfulErrorOnNullMessageException() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGiveHelpfulErrorOnNullMessageException()
		 {
			  // Given
			  CallableUserAggregationFunction method = Compile( typeof( FunctionThatThrowsNullMsgExceptionAtInvocation ) )[0];

			  // Expect
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( "Failed to invoke function `Neo4Net.kernel.impl.proc.test`: " + "Caused by: java.lang.IndexOutOfBoundsException" );

			  // When
			  method.Create( new BasicContext() ).update(new object[] {});
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLoadWhiteListedFunction() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLoadWhiteListedFunction()
		 {
			  // Given
			  _procedureCompiler = new ReflectiveProcedureCompiler( new TypeMappers(), _components, new ComponentRegistry(), NullLog.Instance, new ProcedureConfig(Config.defaults(GraphDatabaseSettings.procedure_whitelist, "Neo4Net.kernel.impl.proc.collectCool")) );

			  CallableUserAggregationFunction method = Compile( typeof( SingleAggregationFunction ) )[0];

			  // Expect
			  UserAggregator created = method.Create( new BasicContext() );
			  created.Update( new object[]{ "Bonnie" } );
			  assertThat( created.Result(), equalTo(Collections.singletonList("Bonnie")) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotLoadNoneWhiteListedFunction() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotLoadNoneWhiteListedFunction()
		 {
			  // Given
			  Log log = spy( typeof( Log ) );
			  _procedureCompiler = new ReflectiveProcedureCompiler( new TypeMappers(), _components, new ComponentRegistry(), log, new ProcedureConfig(Config.defaults(GraphDatabaseSettings.procedure_whitelist, "WrongName")) );

			  IList<CallableUserAggregationFunction> method = Compile( typeof( SingleAggregationFunction ) );
			  verify( log ).warn( "The function 'Neo4Net.kernel.impl.proc.collectCool' is not on the whitelist and won't be loaded." );
			  assertThat( method.Count, equalTo( 0 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotLoadAnyFunctionIfConfigIsEmpty() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotLoadAnyFunctionIfConfigIsEmpty()
		 {
			  // Given
			  Log log = spy( typeof( Log ) );
			  _procedureCompiler = new ReflectiveProcedureCompiler( new TypeMappers(), _components, new ComponentRegistry(), log, new ProcedureConfig(Config.defaults(GraphDatabaseSettings.procedure_whitelist, "")) );

			  IList<CallableUserAggregationFunction> method = Compile( typeof( SingleAggregationFunction ) );
			  verify( log ).warn( "The function 'Neo4Net.kernel.impl.proc.collectCool' is not on the whitelist and won't be loaded." );
			  assertThat( method.Count, equalTo( 0 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSupportFunctionDeprecation() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSupportFunctionDeprecation()
		 {
			  // Given
			  Log log = mock( typeof( Log ) );
			  ReflectiveProcedureCompiler procedureCompiler = new ReflectiveProcedureCompiler( new TypeMappers(), _components, new ComponentRegistry(), log, ProcedureConfig.Default );

			  // When
			  IList<CallableUserAggregationFunction> funcs = procedureCompiler.CompileAggregationFunction( typeof( FunctionWithDeprecation ) );

			  // Then
			  verify( log ).warn( "Use of @UserAggregationFunction(deprecatedBy) without @Deprecated in Neo4Net.kernel.impl.proc.badFunc" );
			  verifyNoMoreInteractions( log );
			  foreach ( CallableUserAggregationFunction func in funcs )
			  {
					string name = func.Signature().name().name();
					func.Create( new BasicContext() );
					switch ( name )
					{
					case "newFunc":
						 assertFalse( "Should not be deprecated", func.Signature().deprecated().Present );
						 break;
					case "oldFunc":
					case "badFunc":
						 assertTrue( "Should be deprecated", func.Signature().deprecated().Present );
						 assertThat( func.Signature().deprecated().get(), equalTo("newFunc") );
						 break;
					default:
						 fail( "Unexpected function: " + name );
					 break;
					}
			  }
		 }

		 public class SingleAggregationFunction
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationFunction public CoolPeopleAggregator collectCool()
			  public virtual CoolPeopleAggregator CollectCool()
			  {
					return new CoolPeopleAggregator();
			  }
		 }

		 public class CoolPeopleAggregator
		 {
			  internal IList<string> CoolPeople = new List<string>();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationUpdate public void update(@Name("name") String name)
			  public virtual void Update( string name )
			  {
					if ( name.Equals( "Bonnie" ) || name.Equals( "Clyde" ) )
					{
						 CoolPeople.Add( name );
					}
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationResult public java.util.List<String> result()
			  public virtual IList<string> Result()
			  {
					return CoolPeople;
			  }
		 }

		 public class FunctionWithVoidOutput
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationFunction public VoidOutput voidOutput()
			  public virtual VoidOutput VoidOutput()
			  {
					return new VoidOutput();
			  }

			  public class VoidOutput
			  {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationUpdate public void update()
					public virtual void Update()
					{
					}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationResult public void result()
					public virtual void Result()
					{
					}
			  }
		 }

		 public class FunctionWithMissingAnnotations
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationFunction public MissingAggregator test()
			  public virtual MissingAggregator Test()
			  {
					return new MissingAggregator();
			  }

			  public class MissingAggregator
			  {
					public virtual void Update()
					{
					}

					public virtual string Result()
					{
						 return "test";
					}
			  }
		 }

		 public class FunctionWithDuplicateUpdateAnnotations
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationFunction public MissingAggregator test()
			  public virtual MissingAggregator Test()
			  {
					return new MissingAggregator();
			  }

			  public class MissingAggregator
			  {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationUpdate public void update1()
					public virtual void Update1()
					{
					}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationUpdate public void update2()
					public virtual void Update2()
					{
					}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationResult public String result()
					public virtual string Result()
					{
						 return "test";
					}
			  }
		 }

		 public class FunctionWithDuplicateResultAnnotations
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationFunction public MissingAggregator test()
			  public virtual MissingAggregator Test()
			  {
					return new MissingAggregator();
			  }

			  public class MissingAggregator
			  {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationUpdate public void update()
					public virtual void Update()
					{
					}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationResult public String result1()
					public virtual string Result1()
					{
						 return "test";
					}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationResult public String result2()
					public virtual string Result2()
					{
						 return "test";
					}
			  }
		 }

		 public class FunctionWithNonVoidUpdate
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationFunction public VoidOutput voidOutput()
			  public virtual VoidOutput VoidOutput()
			  {
					return new VoidOutput();
			  }

			  public class VoidOutput
			  {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationUpdate public long update()
					public virtual long Update()
					{
						 return 42L;
					}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationResult public long result()
					public virtual long Result()
					{
						 return 42L;
					}
			  }
		 }

		 public class LoggingFunction
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public Neo4Net.logging.Log log;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  public Log LogConflict;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationFunction public LoggingAggregator log()
			  public virtual LoggingAggregator Log()
			  {
					return new LoggingAggregator( this );
			  }

			  public class LoggingAggregator
			  {
				  private readonly ReflectiveUserAggregationFunctionTest.LoggingFunction _outerInstance;

				  public LoggingAggregator( ReflectiveUserAggregationFunctionTest.LoggingFunction outerInstance )
				  {
					  this._outerInstance = outerInstance;
				  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationUpdate public void logAround()
					public virtual void LogAround()
					{
						 outerInstance.LogConflict.debug( "1" );
						 outerInstance.LogConflict.info( "2" );
						 outerInstance.LogConflict.warn( "3" );
						 outerInstance.LogConflict.error( "4" );
					}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationResult public long result()
					public virtual long Result()
					{
						 return 1337L;
					}
			  }
		 }

		 public class MapAggregator
		 {
			  internal IDictionary<string, object> Map = new Dictionary<string, object>();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationUpdate public void update(@Name("name") String name, @Name("value") long value)
			  public virtual void Update( string name, long value )
			  {
					long? prev = ( long? ) Map.getOrDefault( name, 0L );
					if ( value > prev.Value )
					{
						 Map[name] = value;
					}
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationResult public java.util.Map<String,Object> result()
			  public virtual IDictionary<string, object> Result()
			  {
					return Map;
			  }
		 }

		 public class MultiFunction
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationFunction public CoolPeopleAggregator collectCool()
			  public virtual CoolPeopleAggregator CollectCool()
			  {
					return new CoolPeopleAggregator();
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationFunction public MapAggregator collectMap()
			  public virtual MapAggregator CollectMap()
			  {
					return new MapAggregator();
			  }
		 }

		 public class WierdConstructorFunction
		 {
			  public WierdConstructorFunction( WierdConstructorFunction wat )
			  {

			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationFunction public CoolPeopleAggregator collectCool()
			  public virtual CoolPeopleAggregator CollectCool()
			  {
					return new CoolPeopleAggregator();
			  }
		 }

		 public class FunctionWithInvalidOutput
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationFunction public InvalidAggregator test()
			  public virtual InvalidAggregator Test()
			  {
					return new InvalidAggregator();
			  }

			  public class InvalidAggregator
			  {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationUpdate public void update()
					public virtual void Update()
					{
						 //dd nothing
					}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationResult public char[] result()
					public virtual char[] Result()
					{
						 return "Testing" .ToCharArray();
					}
			  }

		 }

		 public class FunctionWithStaticContextAnnotatedField
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public static Neo4Net.graphdb.GraphDatabaseService gdb;
			  public static IGraphDatabaseService Gdb;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationFunction public InvalidAggregator test()
			  public virtual InvalidAggregator Test()
			  {
					return new InvalidAggregator();
			  }

			  public class InvalidAggregator
			  {

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationUpdate public void update()
					public virtual void Update()
					{
						 //dd nothing
					}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationResult public String result()
					public virtual string Result()
					{
						 return "Testing";
					}
			  }
		 }

		 public class FunctionThatThrowsNullMsgExceptionAtInvocation
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationFunction public ThrowingAggregator test()
			  public virtual ThrowingAggregator Test()
			  {
					return new ThrowingAggregator();
			  }

			  public class ThrowingAggregator
			  {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationUpdate public void update()
					public virtual void Update()
					{
						 throw new System.IndexOutOfRangeException();
					}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationResult public String result()
					public virtual string Result()
					{
						 return "Testing";
					}
			  }
		 }

		 public class PrivateConstructorFunction
		 {
			  internal PrivateConstructorFunction()
			  {

			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationFunction public CoolPeopleAggregator collectCool()
			  public virtual CoolPeopleAggregator CollectCool()
			  {
					return new CoolPeopleAggregator();
			  }
		 }

		 public class PrivateConstructorButNoFunctions
		 {
			  internal PrivateConstructorButNoFunctions()
			  {

			  }

			  public virtual string ThisIsNotAFunction()
			  {
					return null;
			  }
		 }

		 public class FunctionWithOverriddenName
		 {
			  [UserAggregationFunction("org.mystuff.thisisActuallyTheName")]
			  public virtual CoolPeopleAggregator CollectCool()
			  {
					return new CoolPeopleAggregator();
			  }
		 }

		 public class FunctionWithSingleName
		 {
			  [UserAggregationFunction("singleName")]
			  public virtual CoolPeopleAggregator CollectCool()
			  {
					return new CoolPeopleAggregator();
			  }
		 }

		 public class FunctionWithDeprecation
		 {
			  [UserAggregationFunction()]
			  public virtual CoolPeopleAggregator NewFunc()
			  {
					return new CoolPeopleAggregator();
			  }

			  [Obsolete, UserAggregationFunction(deprecatedBy : "newFunc")]
			  public virtual CoolPeopleAggregator OldFunc()
			  {
					return new CoolPeopleAggregator();
			  }

			  [UserAggregationFunction(deprecatedBy : "newFunc")]
			  public virtual CoolPeopleAggregator BadFunc()
			  {
					return new CoolPeopleAggregator();
			  }
		 }

		 public class NonPublicTestMethod
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationFunction InnerAggregator test()
			  internal virtual InnerAggregator Test()
			  {
					return new InnerAggregator();
			  }

			  public class InnerAggregator
			  {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationUpdate public void update()
					public virtual void Update()
					{
					}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationResult public String result()
					public virtual string Result()
					{
						 return "Testing";
					}
			  }
		 }

		 public class NonPublicUpdateMethod
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationFunction public InnerAggregator test()
			  public virtual InnerAggregator Test()
			  {
					return new InnerAggregator();
			  }

			  public class InnerAggregator
			  {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationUpdate void update()
					internal virtual void Update()
					{
					}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationResult public String result()
					public virtual string Result()
					{
						 return "Testing";
					}
			  }
		 }

		 public class NonPublicResultMethod
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationFunction public InnerAggregator test()
			  public virtual InnerAggregator Test()
			  {
					return new InnerAggregator();
			  }

			  public class InnerAggregator
			  {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationUpdate public void update()
					public virtual void Update()
					{
					}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationResult String result()
					internal virtual string Result()
					{
						 return "Testing";
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.List<Neo4Net.kernel.api.proc.CallableUserAggregationFunction> compile(Class clazz) throws Neo4Net.Kernel.Api.Internal.Exceptions.KernelException
		 private IList<CallableUserAggregationFunction> Compile( Type clazz )
		 {
			  return _procedureCompiler.compileAggregationFunction( clazz );
		 }
	}

}