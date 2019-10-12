using System;
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
namespace Neo4Net.Kernel.impl.proc
{
	using Matchers = org.hamcrest.Matchers;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;


	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using KernelException = Neo4Net.@internal.Kernel.Api.exceptions.KernelException;
	using ProcedureException = Neo4Net.@internal.Kernel.Api.exceptions.ProcedureException;
	using Neo4jTypes = Neo4Net.@internal.Kernel.Api.procs.Neo4jTypes;
	using BasicContext = Neo4Net.Kernel.api.proc.BasicContext;
	using CallableUserFunction = Neo4Net.Kernel.api.proc.CallableUserFunction;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ValueUtils = Neo4Net.Kernel.impl.util.ValueUtils;
	using Log = Neo4Net.Logging.Log;
	using NullLog = Neo4Net.Logging.NullLog;
	using Context = Neo4Net.Procedure.Context;
	using UserFunction = Neo4Net.Procedure.UserFunction;
	using AnyValue = Neo4Net.Values.AnyValue;
	using MapValue = Neo4Net.Values.@virtual.MapValue;

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
//	import static org.neo4j.@internal.kernel.api.procs.UserFunctionSignature.functionSignature;

	public class ReflectiveUserFunctionTest
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
//ORIGINAL LINE: @Test public void shouldInjectLogging() throws org.neo4j.internal.kernel.api.exceptions.KernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInjectLogging()
		 {
			  // Given
			  Log log = spy( typeof( Log ) );
			  _components.register( typeof( Log ), ctx => log );
			  CallableUserFunction function = _procedureCompiler.compileFunction( typeof( LoggingFunction ) )[0];

			  // When
			  function.Apply( new BasicContext(), new AnyValue[0] );

			  // Then
			  verify( log ).debug( "1" );
			  verify( log ).info( "2" );
			  verify( log ).warn( "3" );
			  verify( log ).error( "4" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCompileFunction() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCompileFunction()
		 {
			  // When
			  IList<CallableUserFunction> function = Compile( typeof( SingleReadOnlyFunction ) );

			  // Then
			  assertEquals( 1, function.Count );
			  assertThat( function[0].Signature(), Matchers.equalTo(functionSignature("org", "neo4j", "kernel", "impl", "proc", "listCoolPeople").@out(Neo4jTypes.NTList(Neo4jTypes.NTAny)).build()) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRunSimpleReadOnlyFunction() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRunSimpleReadOnlyFunction()
		 {
			  // Given
			  CallableUserFunction func = Compile( typeof( SingleReadOnlyFunction ) )[0];

			  // When
			  object @out = func.Apply( new BasicContext(), new AnyValue[0] );

			  // Then
			  assertThat( @out, equalTo( ValueUtils.of( Arrays.asList( "Bonnie", "Clyde" ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreClassesWithNoFunctions() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIgnoreClassesWithNoFunctions()
		 {
			  // When
			  IList<CallableUserFunction> functions = Compile( typeof( PrivateConstructorButNoFunctions ) );

			  // Then
			  assertEquals( 0, functions.Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRunClassWithMultipleFunctionsDeclared() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRunClassWithMultipleFunctionsDeclared()
		 {
			  // Given
			  IList<CallableUserFunction> compiled = Compile( typeof( ReflectiveUserFunctionTest.MultiFunction ) );
			  CallableUserFunction bananaPeople = compiled[0];
			  CallableUserFunction coolPeople = compiled[1];

			  // When
			  object coolOut = coolPeople.Apply( new BasicContext(), new AnyValue[0] );
			  object bananaOut = bananaPeople.Apply( new BasicContext(), new AnyValue[0] );

			  // Then
			  assertThat( coolOut, equalTo( ValueUtils.of( Arrays.asList( "Bonnie", "Clyde" ) ) ) );

			  assertThat( ( ( MapValue ) bananaOut ).get( "foo" ), equalTo( ValueUtils.of( Arrays.asList( "bar", "baz" ) ) ) );
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
			  Exception.expectMessage( "Don't know how to map `void` to the Neo4j Type System." );

			  // When
			  Compile( typeof( FunctionWithVoidOutput ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveHelpfulErrorOnFunctionReturningInvalidType() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGiveHelpfulErrorOnFunctionReturningInvalidType()
		 {
			  // Expect
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( string.Format( "Don't know how to map `char[]` to the Neo4j Type System.%n" + "Please refer to to the documentation for full details.%n" + "For your reference, known types are: [boolean, byte[], double, java.lang.Boolean, " + "java.lang.Double, java.lang.Long, java.lang.Number, java.lang.Object, " + "java.lang.String, java.time.LocalDate, java.time.LocalDateTime, " + "java.time.LocalTime, java.time.OffsetTime, java.time.ZonedDateTime, " + "java.time.temporal.TemporalAmount, java.util.List, java.util.Map, long]" ) );

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
			  CallableUserFunction proc = Compile( typeof( FunctionWithOverriddenName ) )[0];

			  // Then
			  assertEquals( "org.mystuff.thisisActuallyTheName", proc.Signature().name().ToString() );
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
			  CallableUserFunction proc = Compile( typeof( FunctionThatThrowsNullMsgExceptionAtInvocation ) )[0];

			  // Expect
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( "Failed to invoke function `org.neo4j.kernel.impl.proc.throwsAtInvocation`: " + "Caused by: java.lang.IndexOutOfBoundsException" );

			  // When
			  proc.Apply( new BasicContext(), new AnyValue[0] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLoadWhiteListedFunction() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLoadWhiteListedFunction()
		 {
			  // Given
			  _procedureCompiler = new ReflectiveProcedureCompiler( new TypeMappers(), _components, new ComponentRegistry(), NullLog.Instance, new ProcedureConfig(Config.defaults(GraphDatabaseSettings.procedure_whitelist, "org.neo4j.kernel.impl.proc.listCoolPeople")) );

			  CallableUserFunction method = Compile( typeof( SingleReadOnlyFunction ) )[0];

			  // Expect
			  object @out = method.Apply( new BasicContext(), new AnyValue[0] );
			  assertThat( @out, equalTo( ValueUtils.of( Arrays.asList( "Bonnie", "Clyde" ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotLoadNoneWhiteListedFunction() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotLoadNoneWhiteListedFunction()
		 {
			  // Given
			  Log log = spy( typeof( Log ) );
			  _procedureCompiler = new ReflectiveProcedureCompiler( new TypeMappers(), _components, new ComponentRegistry(), log, new ProcedureConfig(Config.defaults(GraphDatabaseSettings.procedure_whitelist, "WrongName")) );

			  IList<CallableUserFunction> method = Compile( typeof( SingleReadOnlyFunction ) );
			  verify( log ).warn( "The function 'org.neo4j.kernel.impl.proc.listCoolPeople' is not on the whitelist and won't be loaded." );
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

			  IList<CallableUserFunction> method = Compile( typeof( SingleReadOnlyFunction ) );
			  verify( log ).warn( "The function 'org.neo4j.kernel.impl.proc.listCoolPeople' is not on the whitelist and won't be loaded." );
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
			  IList<CallableUserFunction> funcs = procedureCompiler.CompileFunction( typeof( FunctionWithDeprecation ) );

			  // Then
			  verify( log ).warn( "Use of @UserFunction(deprecatedBy) without @Deprecated in org.neo4j.kernel.impl.proc.badFunc" );
			  verifyNoMoreInteractions( log );
			  foreach ( CallableUserFunction func in funcs )
			  {
					string name = func.Signature().name().name();
					func.Apply( new BasicContext(), new AnyValue[0] );
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

		 public class LoggingFunction
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.neo4j.logging.Log log;
			  public Log Log;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public long logAround()
			  public virtual long LogAround()
			  {
					Log.debug( "1" );
					Log.info( "2" );
					Log.warn( "3" );
					Log.error( "4" );
					return -1L;
			  }
		 }

		 public class SingleReadOnlyFunction
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public java.util.List<String> listCoolPeople()
			  public virtual IList<string> ListCoolPeople()
			  {
					return Arrays.asList( "Bonnie", "Clyde" );
			  }
		 }

		 public class FunctionWithVoidOutput
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public void voidOutput()
			  public virtual void VoidOutput()
			  {
			  }
		 }

		 public class MultiFunction
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public java.util.List<String> listCoolPeople()
			  public virtual IList<string> ListCoolPeople()
			  {
					return Arrays.asList( "Bonnie", "Clyde" );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public java.util.Map<String, Object> listBananaOwningPeople()
			  public virtual IDictionary<string, object> ListBananaOwningPeople()
			  {
					Dictionary<string, object> map = new Dictionary<string, object>();
					map["foo"] = Arrays.asList( "bar", "baz" );
					return map;
			  }
		 }

		 public class WierdConstructorFunction
		 {
			  public WierdConstructorFunction( WierdConstructorFunction wat )
			  {

			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public java.util.List<String> listCoolPeople()
			  public virtual IList<string> ListCoolPeople()
			  {
					return Arrays.asList( "Bonnie", "Clyde" );
			  }
		 }

		 public class FunctionWithInvalidOutput
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public char[] test()
			  public virtual char[] Test()
			  {
					return "Testing".ToCharArray();
			  }
		 }

		 public class FunctionWithStaticContextAnnotatedField
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public static org.neo4j.graphdb.GraphDatabaseService gdb;
			  public static GraphDatabaseService Gdb;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public Object test()
			  public virtual object Test()
			  {
					return null;
			  }
		 }

		 public class FunctionThatThrowsNullMsgExceptionAtInvocation
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String throwsAtInvocation()
			  public virtual string ThrowsAtInvocation()
			  {
					throw new System.IndexOutOfRangeException();
			  }
		 }

		 public class PrivateConstructorFunction
		 {
			  internal PrivateConstructorFunction()
			  {

			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public java.util.List<String> listCoolPeople()
			  public virtual IList<string> ListCoolPeople()
			  {
					return Arrays.asList( "Bonnie", "Clyde" );
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
			  [UserFunction("org.mystuff.thisisActuallyTheName")]
			  public virtual object SomethingThatShouldntMatter()
			  {
					return null;
			  }

		 }

		 public class FunctionWithSingleName
		 {
			  [UserFunction("singleName")]
			  public virtual string BlahDoesntMatterEither()
			  {
					return null;
			  }
		 }

		 public class FunctionWithDeprecation
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public Object newFunc()
			  public virtual object NewFunc()
			  {
					return null;
			  }

			  [Obsolete, UserFunction(deprecatedBy : "newFunc")]
			  public virtual string OldFunc()
			  {
					return null;
			  }

			  [UserFunction(deprecatedBy : "newFunc")]
			  public virtual object BadFunc()
			  {
					return null;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.List<org.neo4j.kernel.api.proc.CallableUserFunction> compile(Class clazz) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 private IList<CallableUserFunction> Compile( Type clazz )
		 {
			  return _procedureCompiler.compileFunction( clazz );
		 }
	}

}