﻿using System.Collections.Generic;

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

	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using ProcedureException = Org.Neo4j.@internal.Kernel.Api.exceptions.ProcedureException;
	using Neo4jTypes = Org.Neo4j.@internal.Kernel.Api.procs.Neo4jTypes;
	using UserAggregator = Org.Neo4j.@internal.Kernel.Api.procs.UserAggregator;
	using UserFunctionSignature = Org.Neo4j.@internal.Kernel.Api.procs.UserFunctionSignature;
	using BasicContext = Org.Neo4j.Kernel.api.proc.BasicContext;
	using CallableUserAggregationFunction = Org.Neo4j.Kernel.api.proc.CallableUserAggregationFunction;
	using CallableUserFunction = Org.Neo4j.Kernel.api.proc.CallableUserFunction;
	using Context = Org.Neo4j.Kernel.api.proc.Context;
	using Org.Neo4j.Kernel.api.proc;
	using AnyValue = Org.Neo4j.Values.AnyValue;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.nullValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.UserFunctionSignature.functionSignature;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.proc.Key.key;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.numberValue;

	public class UserFunctionsTest
	{
		private bool InstanceFieldsInitialized = false;

		public UserFunctionsTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_function = _function( _signature );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException exception = org.junit.rules.ExpectedException.none();
		 public ExpectedException Exception = ExpectedException.none();

		 private readonly Procedures _procs = new Procedures();
		 private readonly UserFunctionSignature _signature = functionSignature( "org", "myproc" ).@out( Neo4jTypes.NTAny ).build();
		 private CallableUserFunction _function;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetRegisteredFunction() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGetRegisteredFunction()
		 {
			  // When
			  _procs.register( _function );

			  // Then
			  assertThat( _procs.function( _signature.name() ).signature(), equalTo(_signature) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetAllRegisteredFunctions() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGetAllRegisteredFunctions()
		 {
			  // When
			  _procs.register( Function( functionSignature( "org", "myproc1" ).@out( Neo4jTypes.NTAny ).build() ) );
			  _procs.register( Function( functionSignature( "org", "myproc2" ).@out( Neo4jTypes.NTAny ).build() ) );
			  _procs.register( Function( functionSignature( "org", "myproc3" ).@out( Neo4jTypes.NTAny ).build() ) );

			  // Then
			  IList<UserFunctionSignature> signatures = Iterables.asList( _procs.AllFunctions );
			  assertThat( signatures, containsInAnyOrder( functionSignature( "org", "myproc1" ).@out( Neo4jTypes.NTAny ).build(), functionSignature("org", "myproc2").@out(Neo4jTypes.NTAny).build(), functionSignature("org", "myproc3").@out(Neo4jTypes.NTAny).build() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetRegisteredAggregationFunctions() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGetRegisteredAggregationFunctions()
		 {
			  // When
			  _procs.register( Function( functionSignature( "org", "myfunc1" ).@out( Neo4jTypes.NTAny ).build() ) );
			  _procs.register( Function( functionSignature( "org", "myfunc2" ).@out( Neo4jTypes.NTAny ).build() ) );
			  _procs.register( AggregationFunction( functionSignature( "org", "myaggrfunc1" ).@out( Neo4jTypes.NTAny ).build() ) );

			  // Then
			  IList<UserFunctionSignature> signatures = Iterables.asList( _procs.AllFunctions );
			  assertThat( signatures, containsInAnyOrder( functionSignature( "org", "myfunc1" ).@out( Neo4jTypes.NTAny ).build(), functionSignature("org", "myfunc2").@out(Neo4jTypes.NTAny).build(), functionSignature("org", "myaggrfunc1").@out(Neo4jTypes.NTAny).build() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallRegisteredFunction() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCallRegisteredFunction()
		 {
			  // Given
			  _procs.register( _function );

			  // When
			  object result = _procs.callFunction( new BasicContext(), _signature.name(), new AnyValue[] { numberValue(1337) } );

			  // Then
			  assertThat( result, equalTo( Values.of( 1337 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowCallingNonExistingFunction() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowCallingNonExistingFunction()
		 {
			  // Expect
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( "There is no function with the name `org.myproc` registered for this " + "database instance. Please ensure you've spelled the " + "function name correctly and that the function is properly deployed." );

			  // When
			  _procs.callFunction( new BasicContext(), _signature.name(), new AnyValue[] { numberValue(1337) } );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowRegisteringConflictingName() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowRegisteringConflictingName()
		 {
			  // Given
			  _procs.register( _function );

			  // Expect
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( "Unable to register function, because the name `org.myproc` is already in use." );

			  // When
			  _procs.register( _function );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSignalNonExistingFunction()
		 public virtual void ShouldSignalNonExistingFunction()
		 {
			  // When
			  assertThat( _procs.function( _signature.name() ), nullValue() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMakeContextAvailable() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldMakeContextAvailable()
		 {
			  // Given
			  Key<string> someKey = key( "someKey", typeof( string ) );

			  _procs.register( new CallableUserFunction_BasicUserFunctionAnonymousInnerClass( this, _signature, someKey ) );

			  BasicContext ctx = new BasicContext();
			  ctx.Put( someKey, "hello, world" );

			  // When
			  object result = _procs.callFunction( ctx, _signature.name(), new AnyValue[0] );

			  // Then
			  assertThat( result, equalTo( Values.of( "hello, world" ) ) );
		 }

		 private class CallableUserFunction_BasicUserFunctionAnonymousInnerClass : Org.Neo4j.Kernel.api.proc.CallableUserFunction_BasicUserFunction
		 {
			 private readonly UserFunctionsTest _outerInstance;

			 private Key<string> _someKey;

			 public CallableUserFunction_BasicUserFunctionAnonymousInnerClass( UserFunctionsTest outerInstance, UserFunctionSignature signature, Key<string> someKey ) : base( signature )
			 {
				 this.outerInstance = outerInstance;
				 this._someKey = someKey;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.values.AnyValue apply(org.neo4j.kernel.api.proc.Context ctx, org.neo4j.values.AnyValue[] input) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
			 public override AnyValue apply( Context ctx, AnyValue[] input )
			 {
				  return Values.stringValue( ctx.Get( _someKey ) );
			 }
		 }

		 private CallableUserFunction Function( UserFunctionSignature signature )
		 {
			  return new CallableUserFunction_BasicUserFunctionAnonymousInnerClass2( this, signature );
		 }

		 private class CallableUserFunction_BasicUserFunctionAnonymousInnerClass2 : Org.Neo4j.Kernel.api.proc.CallableUserFunction_BasicUserFunction
		 {
			 private readonly UserFunctionsTest _outerInstance;

			 public CallableUserFunction_BasicUserFunctionAnonymousInnerClass2( UserFunctionsTest outerInstance, UserFunctionSignature signature ) : base( signature )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override AnyValue apply( Context ctx, AnyValue[] input )
			 {
				  return input[0];
			 }
		 }

		 private CallableUserAggregationFunction AggregationFunction( UserFunctionSignature signature )
		 {
			  return new CallableUserAggregationFunction_BasicUserAggregationFunctionAnonymousInnerClass( this, signature );
		 }

		 private class CallableUserAggregationFunction_BasicUserAggregationFunctionAnonymousInnerClass : Org.Neo4j.Kernel.api.proc.CallableUserAggregationFunction_BasicUserAggregationFunction
		 {
			 private readonly UserFunctionsTest _outerInstance;

			 public CallableUserAggregationFunction_BasicUserAggregationFunctionAnonymousInnerClass( UserFunctionsTest outerInstance, UserFunctionSignature signature ) : base( signature )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override UserAggregator create( Context ctx )
			 {
				  return null;
			 }
		 }
	}

}