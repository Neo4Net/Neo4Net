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
	using Matcher = org.hamcrest.Matcher;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;


	using Iterators = Neo4Net.Helpers.Collections.Iterators;
	using ProcedureException = Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
	using ResourceTracker = Neo4Net.Kernel.api.ResourceTracker;
	using StubResourceManager = Neo4Net.Kernel.api.StubResourceManager;
	using BasicContext = Neo4Net.Kernel.api.proc.BasicContext;
	using CallableProcedure = Neo4Net.Kernel.api.proc.CallableProcedure;
	using CallableUserAggregationFunction = Neo4Net.Kernel.api.proc.CallableUserAggregationFunction;
	using CallableUserFunction = Neo4Net.Kernel.api.proc.CallableUserFunction;
	using Log = Neo4Net.Logging.Log;
	using Context = Neo4Net.Procedure.Context;
	using Procedure = Neo4Net.Procedure.Procedure;
	using UserAggregationFunction = Neo4Net.Procedure.UserAggregationFunction;
	using UserAggregationResult = Neo4Net.Procedure.UserAggregationResult;
	using UserAggregationUpdate = Neo4Net.Procedure.UserAggregationUpdate;
	using UserFunction = Neo4Net.Procedure.UserFunction;
	using AnyValue = Neo4Net.Values.AnyValue;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.allOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.hamcrest.MockitoHamcrest.argThat;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("WeakerAccess") public class ResourceInjectionTest
	public class ResourceInjectionTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException exception = org.junit.rules.ExpectedException.none();
		 public ExpectedException Exception = ExpectedException.none();

		 private ReflectiveProcedureCompiler _compiler;
		 private readonly ResourceTracker _resourceTracker = new StubResourceManager();

		 private Log _log = mock( typeof( Log ) );

		 public static string NotAvailableMessage( string procName )
		 {
			  return argThat( NotAvailableMessageMatcher( procName ) );
		 }

		 private static Matcher<string> NotAvailableMessageMatcher( string procName )
		 {
			  return allOf( containsString( procName ), containsString( "unavailable" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  ComponentRegistry safeComponents = new ComponentRegistry();
			  ComponentRegistry allComponents = new ComponentRegistry();
			  safeComponents.Register( typeof( MyAwesomeAPI ), ctx => new MyAwesomeAPI() );
			  allComponents.Register( typeof( MyAwesomeAPI ), ctx => new MyAwesomeAPI() );
			  allComponents.Register( typeof( MyUnsafeAPI ), ctx => new MyUnsafeAPI() );

			  _compiler = new ReflectiveProcedureCompiler( new TypeMappers(), safeComponents, allComponents, _log, ProcedureConfig.Default );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCompileAndRunProcedure() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCompileAndRunProcedure()
		 {
			  // Given
			  CallableProcedure proc = _compiler.compileProcedure( typeof( ProcedureWithInjectedAPI ), null, true )[0];

			  // Then
			  IList<object[]> @out = Iterators.asList( proc.Apply( new BasicContext(), new object[0], _resourceTracker ) );

			  // Then
			  assertThat( @out[0], equalTo( new object[]{ "Bonnie" } ) );
			  assertThat( @out[1], equalTo( new object[]{ "Clyde" } ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailNicelyWhenUnknownAPI() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailNicelyWhenUnknownAPI()
		 {
			  //When
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( "Unable to set up injection for procedure `ProcedureWithUnknownAPI`, " + "the field `api` has type `class org.Neo4Net.kernel.impl.proc.ResourceInjectionTest$UnknownAPI` " + "which is not a known injectable component." );

			  // Then
			  _compiler.compileProcedure( typeof( ProcedureWithUnknownAPI ), null, true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCompileAndRunUnsafeProcedureUnsafeMode() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCompileAndRunUnsafeProcedureUnsafeMode()
		 {
			  // Given
			  CallableProcedure proc = _compiler.compileProcedure( typeof( ProcedureWithUnsafeAPI ), null, true )[0];

			  // Then
			  IList<object[]> @out = Iterators.asList( proc.Apply( new BasicContext(), new object[0], _resourceTracker ) );

			  // Then
			  assertThat( @out[0], equalTo( new object[]{ "Morpheus" } ) );
			  assertThat( @out[1], equalTo( new object[]{ "Trinity" } ) );
			  assertThat( @out[2], equalTo( new object[]{ "Neo" } ) );
			  assertThat( @out[3], equalTo( new object[]{ "Emil" } ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailNicelyWhenUnsafeAPISafeMode() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailNicelyWhenUnsafeAPISafeMode()
		 {
			  //When
			  IList<CallableProcedure> procList = _compiler.compileProcedure( typeof( ProcedureWithUnsafeAPI ), null, false );
			  verify( _log ).warn( NotAvailableMessage( "org.Neo4Net.kernel.impl.proc.listCoolPeople" ) );

			  assertThat( procList.Count, equalTo( 1 ) );
			  try
			  {
					procList[0].Apply( new BasicContext(), new object[0], _resourceTracker );
					fail();
			  }
			  catch ( ProcedureException e )
			  {
					assertThat( e.Message, NotAvailableMessageMatcher( "org.Neo4Net.kernel.impl.proc.listCoolPeople" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCompileAndRunUserFunctions() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCompileAndRunUserFunctions()
		 {
			  // Given
			  CallableUserFunction proc = _compiler.compileFunction( typeof( FunctionWithInjectedAPI ) )[0];

			  // When
			  object @out = proc.Apply( new BasicContext(), new AnyValue[0] );

			  // Then
			  assertThat( @out, equalTo( Values.of( "[Bonnie, Clyde]" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailNicelyWhenFunctionUsesUnknownAPI() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailNicelyWhenFunctionUsesUnknownAPI()
		 {
			  //When
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( "Unable to set up injection for procedure `FunctionWithUnknownAPI`, " + "the field `api` has type `class org.Neo4Net.kernel.impl.proc.ResourceInjectionTest$UnknownAPI` " + "which is not a known injectable component." );

			  // Then
			  _compiler.compileFunction( typeof( FunctionWithUnknownAPI ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailNicelyWhenUnsafeAPISafeModeFunction() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailNicelyWhenUnsafeAPISafeModeFunction()
		 {
			  //When
			  IList<CallableUserFunction> procList = _compiler.compileFunction( typeof( FunctionWithUnsafeAPI ) );
			  verify( _log ).warn( NotAvailableMessage( "org.Neo4Net.kernel.impl.proc.listCoolPeople" ) );

			  assertThat( procList.Count, equalTo( 1 ) );
			  try
			  {
					procList[0].Apply( new BasicContext(), new AnyValue[0] );
					fail();
			  }
			  catch ( ProcedureException e )
			  {
					assertThat( e.Message, NotAvailableMessageMatcher( "org.Neo4Net.kernel.impl.proc.listCoolPeople" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCompileAndRunUserAggregationFunctions() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCompileAndRunUserAggregationFunctions()
		 {
			  // Given
			  CallableUserAggregationFunction proc = _compiler.compileAggregationFunction( typeof( AggregationFunctionWithInjectedAPI ) )[0];
			  // When
			  proc.Create( new BasicContext() ).update(new object[]{});
			  object @out = proc.Create( new BasicContext() ).result();

			  // Then
			  assertThat( @out, equalTo( "[Bonnie, Clyde]" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailNicelyWhenAggregationFunctionUsesUnknownAPI() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailNicelyWhenAggregationFunctionUsesUnknownAPI()
		 {
			  //When
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( "Unable to set up injection for procedure `AggregationFunctionWithUnknownAPI`, " + "the field `api` has type `class org.Neo4Net.kernel.impl.proc.ResourceInjectionTest$UnknownAPI` " + "which is not a known injectable component." );

			  // Then
			  _compiler.compileAggregationFunction( typeof( AggregationFunctionWithUnknownAPI ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailNicelyWhenUnsafeAPISafeModeAggregationFunction() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailNicelyWhenUnsafeAPISafeModeAggregationFunction()
		 {
			  //When
			  IList<CallableUserAggregationFunction> procList = _compiler.compileAggregationFunction( typeof( AggregationFunctionWithUnsafeAPI ) );
			  verify( _log ).warn( NotAvailableMessage( "org.Neo4Net.kernel.impl.proc.listCoolPeople" ) );

			  assertThat( procList.Count, equalTo( 1 ) );
			  try
			  {
					procList[0].Create( new BasicContext() ).update(new object[]{});
					object @out = procList[0].Create( new BasicContext() ).result();
					fail();
			  }
			  catch ( ProcedureException e )
			  {
					assertThat( e.Message, NotAvailableMessageMatcher( "org.Neo4Net.kernel.impl.proc.listCoolPeople" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailNicelyWhenAllUsesUnsafeAPI() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailNicelyWhenAllUsesUnsafeAPI()
		 {
			  //When
			  _compiler.compileFunction( typeof( FunctionsAndProcedureUnsafe ) );
			  _compiler.compileProcedure( typeof( FunctionsAndProcedureUnsafe ), null, false );
			  _compiler.compileAggregationFunction( typeof( FunctionsAndProcedureUnsafe ) );
			  // Then

			  verify( _log ).warn( NotAvailableMessage( "org.Neo4Net.kernel.impl.proc.safeUserFunctionInUnsafeAPIClass" ) );
			  verify( _log ).warn( NotAvailableMessage( "org.Neo4Net.kernel.impl.proc.listCoolPeopleProcedure" ) );
			  // With extra ' ' space at the end to distinguish from procedure form:
			  verify( _log ).warn( NotAvailableMessage( "org.Neo4Net.kernel.impl.proc.listCoolPeople " ) );
		 }

		 public class MyOutputRecord
		 {
			  public string Name;

			  public MyOutputRecord( string name )
			  {
					this.Name = name;
			  }
		 }

		 public class MyAwesomeAPI
		 {

			  internal virtual IList<string> ListCoolPeople()
			  {
					return new IList<string> { "Bonnie", "Clyde" };
			  }
		 }

		 public class UnknownAPI
		 {

			  internal virtual IList<string> ListCoolPeople()
			  {
					return singletonList( "booh!" );
			  }
		 }

		 public class MyUnsafeAPI
		 {
			  internal virtual IList<string> ListCoolPeople()
			  {
					return new IList<string> { "Morpheus", "Trinity", "Neo", "Emil" };
			  }
		 }

		 public class ProcedureWithInjectedAPI
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public MyAwesomeAPI api;
			  public MyAwesomeAPI Api;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<MyOutputRecord> listCoolPeople()
			  public virtual Stream<MyOutputRecord> ListCoolPeople()
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
					return Api.listCoolPeople().Select(MyOutputRecord::new);
			  }
		 }

		 public class FunctionWithInjectedAPI
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public MyAwesomeAPI api;
			  public MyAwesomeAPI Api;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String listCoolPeople()
			  public virtual string ListCoolPeople()
			  {
					return Api.listCoolPeople().ToString();
			  }
		 }

		 public class AggregationFunctionWithInjectedAPI
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public MyAwesomeAPI api;
			  public MyAwesomeAPI Api;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationFunction public VoidOutput listCoolPeople()
			  public virtual VoidOutput ListCoolPeople()
			  {
					return new VoidOutput( Api );
			  }

			  public class VoidOutput
			  {
					internal MyAwesomeAPI Api;

					public VoidOutput( MyAwesomeAPI api )
					{
						 this.Api = api;
					}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationUpdate public void update()
					public virtual void Update()
					{
					}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationResult public String result()
					public virtual string Result()
					{
						 return Api.listCoolPeople().ToString();
					}
			  }
		 }

		 public class ProcedureWithUnknownAPI
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public UnknownAPI api;
			  public UnknownAPI Api;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<MyOutputRecord> listCoolPeople()
			  public virtual Stream<MyOutputRecord> ListCoolPeople()
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
					return Api.listCoolPeople().Select(MyOutputRecord::new);
			  }
		 }

		 public class FunctionWithUnknownAPI
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public UnknownAPI api;
			  public UnknownAPI Api;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String listCoolPeople()
			  public virtual string ListCoolPeople()
			  {
					return Api.listCoolPeople().ToString();
			  }
		 }

		 public class AggregationFunctionWithUnknownAPI
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public UnknownAPI api;
			  public UnknownAPI Api;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationFunction public VoidOutput listCoolPeople()
			  public virtual VoidOutput ListCoolPeople()
			  {
					return new VoidOutput( Api );
			  }

			  public class VoidOutput
			  {
					internal UnknownAPI Api;

					public VoidOutput( UnknownAPI api )
					{
						 this.Api = api;
					}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationUpdate public void update()
					public virtual void Update()
					{
					}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationResult public String result()
					public virtual string Result()
					{
						 return Api.listCoolPeople().ToString();
					}
			  }
		 }

		 public class ProcedureWithUnsafeAPI
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public MyUnsafeAPI api;
			  public MyUnsafeAPI Api;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<MyOutputRecord> listCoolPeople()
			  public virtual Stream<MyOutputRecord> ListCoolPeople()
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
					return Api.listCoolPeople().Select(MyOutputRecord::new);
			  }
		 }

		 public class FunctionWithUnsafeAPI
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public MyUnsafeAPI api;
			  public MyUnsafeAPI Api;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String listCoolPeople()
			  public virtual string ListCoolPeople()
			  {
					return Api.listCoolPeople().ToString();
			  }
		 }
		 public class AggregationFunctionWithUnsafeAPI
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public MyUnsafeAPI api;
			  public MyUnsafeAPI Api;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationFunction public VoidOutput listCoolPeople()
			  public virtual VoidOutput ListCoolPeople()
			  {
					return new VoidOutput( Api );
			  }

			  public class VoidOutput
			  {
					internal MyUnsafeAPI Api;

					public VoidOutput( MyUnsafeAPI api )
					{
						 this.Api = api;
					}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationUpdate public void update()
					public virtual void Update()
					{
					}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationResult public String result()
					public virtual string Result()
					{
						 return Api.listCoolPeople().ToString();
					}
			  }
		 }

		 public class FunctionsAndProcedureUnsafe
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public MyUnsafeAPI api;
			  public MyUnsafeAPI Api;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationFunction public VoidOutput listCoolPeople()
			  public virtual VoidOutput ListCoolPeople()
			  {
					return new VoidOutput( Api );
			  }

			  public class VoidOutput
			  {
					internal MyUnsafeAPI Api;

					public VoidOutput( MyUnsafeAPI api )
					{
						 this.Api = api;
					}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationUpdate public void update()
					public virtual void Update()
					{
					}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationResult public String result()
					public virtual string Result()
					{
						 return Api.listCoolPeople().ToString();
					}
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<MyOutputRecord> listCoolPeopleProcedure()
			  public virtual Stream<MyOutputRecord> ListCoolPeopleProcedure()
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
					return Api.listCoolPeople().Select(MyOutputRecord::new);
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String safeUserFunctionInUnsafeAPIClass()
			  public virtual string SafeUserFunctionInUnsafeAPIClass()
			  {
					return "a safe function";
			  }
		 }
	}

}