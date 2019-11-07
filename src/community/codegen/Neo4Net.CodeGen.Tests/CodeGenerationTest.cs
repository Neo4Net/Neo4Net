using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.CodeGen
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;
	using InOrder = org.mockito.InOrder;


	using ByteCode = Neo4Net.CodeGen.ByteCode.ByteCode;
	using SourceCode = Neo4Net.CodeGen.Source.SourceCode;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.inOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyZeroInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.Expression.add;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.Expression.and;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.Expression.constant;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.Expression.equal;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.Expression.invoke;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.Expression.isNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.Expression.multiply;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.Expression.newArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.Expression.newInstance;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.Expression.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.Expression.notNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.Expression.or;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.Expression.subtract;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.Expression.ternary;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.ExpressionTemplate.cast;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.ExpressionTemplate.load;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.ExpressionTemplate.self;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.MethodReference.constructorReference;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.MethodReference.methodReference;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.Parameter.param;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.TypeReference.extending;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.TypeReference.parameterizedType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.TypeReference.typeParameter;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.TypeReference.typeReference;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class CodeGenerationTest
	public class CodeGenerationTest
	{
		 private static readonly MethodReference _run = CreateMethod( typeof( ThreadStart ), typeof( void ), "run" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static java.util.Collection<Object[]> generators()
		 public static ICollection<object[]> Generators()
		 {
			  return Arrays.asList( new object[]{ SourceCode.SOURCECODE }, new object[]{ ByteCode.BYTECODE } );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter() public CodeGenerationStrategy<?> strategy;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public CodeGenerationStrategy<object> Strategy;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void createGenerator()
		 public virtual void CreateGenerator()
		 {
			  try
			  {
					_generator = CodeGenerator.GenerateCode( Strategy );
			  }
			  catch ( CodeGenerationNotSupportedException e )
			  {
					throw new AssertionError( "Cannot compile code.", e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateClass() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGenerateClass()
		 {
			  // given
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					handle = simple.Handle();
			  }

			  // when
			  Type aClass = handle.LoadClass();

			  // then
			  assertNotNull( "null class loaded", aClass );
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  assertNotNull( "null package of: " + aClass.FullName, aClass.Assembly );
			  assertEquals( PACKAGE, aClass.Assembly.GetName().Name );
			  assertEquals( "SimpleClass", aClass.Name );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateTwoClassesInTheSamePackage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGenerateTwoClassesInTheSamePackage()
		 {
			  // given
			  ClassHandle one;
			  ClassHandle two;
			  using ( ClassGenerator simple = GenerateClass( "One" ) )
			  {
					one = simple.Handle();
			  }
			  using ( ClassGenerator simple = GenerateClass( "Two" ) )
			  {
					two = simple.Handle();
			  }

			  // when
			  Type classOne = one.LoadClass();
			  Type classTwo = two.LoadClass();

			  // then
			  assertNotNull( classOne.Assembly );
			  assertSame( classOne.Assembly, classTwo.Assembly );
			  assertEquals( "One", classOne.Name );
			  assertEquals( "Two", classTwo.Name );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateDefaultConstructor() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGenerateDefaultConstructor()
		 {
			  // given
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( typeof( NamedBase ), "SimpleClass" ) )
			  {
					handle = simple.Handle();
			  }

			  // when
			  object instance = Constructor( handle.LoadClass() ).invoke();
			  object constructorCalled = InstanceMethod( instance, "defaultConstructorCalled" ).invoke();

			  // then
			  assertTrue( ( bool? ) constructorCalled );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateField() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGenerateField()
		 {
			  // given
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					simple.Field( typeof( string ), "theField" );
					handle = simple.Handle();
			  }

			  // when
			  Type clazz = handle.LoadClass();

			  // then
			  System.Reflection.FieldInfo theField = clazz.getDeclaredField( "theField" );
			  assertSame( typeof( string ), theField.Type );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateParameterizedTypeField() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGenerateParameterizedTypeField()
		 {
			  // given
			  ClassHandle handle;
			  TypeReference stringList = TypeReference.ParameterizedType( typeof( System.Collections.IList ), typeof( string ) );
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					simple.Field( stringList, "theField" );
					handle = simple.Handle();
			  }

			  // when
			  Type clazz = handle.LoadClass();

			  // then
			  System.Reflection.FieldInfo theField = clazz.getDeclaredField( "theField" );
			  assertSame( typeof( System.Collections.IList ), theField.Type );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateMethodReturningFieldValue() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGenerateMethodReturningFieldValue()
		 {
			  AssertMethodReturningField( typeof( sbyte ), ( sbyte ) 42 );
			  AssertMethodReturningField( typeof( short ), ( short ) 42 );
			  AssertMethodReturningField( typeof( char ), ( char ) 42 );
			  AssertMethodReturningField( typeof( int ), 42 );
			  AssertMethodReturningField( typeof( long ), 42L );
			  AssertMethodReturningField( typeof( float ), 42F );
			  AssertMethodReturningField( typeof( double ), 42D );
			  AssertMethodReturningField( typeof( string ), "42" );
			  AssertMethodReturningField( typeof( int[] ), new int[]{ 42 } );
			  AssertMethodReturningField( typeof( DictionaryEntry[] ), Collections.singletonMap( 42, "42" ).entrySet().toArray(new DictionaryEntry[0]) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateMethodReturningArrayValue() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGenerateMethodReturningArrayValue()
		 {
			  // given
			  CreateGenerator();
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {

					simple.Generate( MethodTemplate.Method( typeof( int[] ), "value" ).returns( newArray( typeReference( typeof( int ) ), constant( 1 ), constant( 2 ), constant( 3 ) ) ).build() );
					handle = simple.Handle();
			  }

			  // when
			  object instance = Constructor( handle.LoadClass() ).invoke();

			  // then
			  assertArrayEquals( new int[]{ 1, 2, 3 }, ( int[] ) InstanceMethod( instance, "value" ).invoke() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateMethodReturningParameterizedTypeValue() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGenerateMethodReturningParameterizedTypeValue()
		 {
			  // given
			  CreateGenerator();
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					TypeReference stringList = parameterizedType( typeof( System.Collections.IList ), typeof( string ) );
					simple.Generate( MethodTemplate.Method( stringList, "value" ).returns( Expression.Invoke( methodReference( typeof( Arrays ), stringList, "asList", typeof( object[] ) ), newArray( typeReference( typeof( string ) ), constant( "a" ), constant( "b" ) ) ) ).build() );
					handle = simple.Handle();
			  }

			  // when
			  object instance = Constructor( handle.LoadClass() ).invoke();

			  // then
			  assertEquals( Arrays.asList( "a", "b" ), InstanceMethod( instance, "value" ).invoke() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateStaticPrimitiveField() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGenerateStaticPrimitiveField()
		 {
			  // given
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					FieldReference foo = simple.StaticField( typeof( int ), "FOO", constant( 42 ) );
					using ( CodeBlock get = simple.GenerateMethod( typeof( int ), "get" ) )
					{
						 get.Returns( Expression.GetStatic( foo ) );
					}
					handle = simple.Handle();
			  }

			  // when
			  object foo = InstanceMethod( handle.NewInstance(), "get" ).invoke();

			  // then
			  assertEquals( 42, foo );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateStaticReferenceTypeField() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGenerateStaticReferenceTypeField()
		 {
			  // given
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					FieldReference foo = simple.StaticField( typeof( string ), "FOO", constant( "42" ) );
					using ( CodeBlock get = simple.GenerateMethod( typeof( string ), "get" ) )
					{
						 get.Returns( Expression.GetStatic( foo ) );
					}
					handle = simple.Handle();
			  }

			  // when
			  object foo = InstanceMethod( handle.NewInstance(), "get" ).invoke();

			  // then
			  assertEquals( "42", foo );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateStaticParameterizedTypeField() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGenerateStaticParameterizedTypeField()
		 {
			  // given
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					TypeReference stringList = TypeReference.ParameterizedType( typeof( System.Collections.IList ), typeof( string ) );
					FieldReference foo = simple.StaticField( stringList, "FOO", Expression.Invoke( methodReference( typeof( Arrays ), stringList, "asList", typeof( object[] ) ), newArray( typeReference( typeof( string ) ), constant( "FOO" ), constant( "BAR" ), constant( "BAZ" ) ) ) );
					using ( CodeBlock get = simple.GenerateMethod( stringList, "get" ) )
					{
						 get.Returns( Expression.GetStatic( foo ) );
					}
					handle = simple.Handle();
			  }

			  // when
			  object foo = InstanceMethod( handle.NewInstance(), "get" ).invoke();

			  // then
			  assertEquals( Arrays.asList( "FOO", "BAR", "BAZ" ), foo );
		 }

		 public interface Thrower<E> where E : Exception
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void doThrow() throws E;
			  void DoThrow();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowParameterizedCheckedException() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowParameterizedCheckedException()
		 {
			  // given
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					using ( CodeBlock fail = simple.Generate( MethodDeclaration.Method( typeof( void ), "fail", param( TypeReference.ParameterizedType( typeof( Thrower ), typeParameter( "E" ) ), "thrower" ) ).parameterizedWith( "E", extending( typeof( Exception ) ) ).throwsException( typeParameter( "E" ) ) ) )
					{
						 fail.Expression( invoke( fail.Load( "thrower" ), methodReference( typeof( Thrower ), typeof( void ), "doThrow" ) ) );
					}
					handle = simple.Handle();
			  }

			  // when
			  try
			  {
					InstanceMethod( handle.NewInstance(), "fail", typeof(Thrower) ).invoke((Thrower<IOException>)() =>
					{
					 throw new IOException( "Hello from the inside" );
					});

					fail( "expected exception" );
			  }
			  // then
			  catch ( IOException e )
			  {
					assertEquals( "Hello from the inside", e.Message );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAssignLocalVariable() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAssignLocalVariable()
		 {
			  // given
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					using ( CodeBlock create = simple.generateMethod( typeof( SomeBean ), "createBean", param( typeof( string ), "foo" ), param( typeof( string ), "bar" ) ) )
					{
						 create.Assign( typeof( SomeBean ), "bean", invoke( newInstance( typeof( SomeBean ) ), constructorReference( typeof( SomeBean ) ) ) );
						 create.Expression( invoke( create.Load( "bean" ), methodReference( typeof( SomeBean ), typeof( void ), "setFoo", typeof( string ) ), create.Load( "foo" ) ) );
						 create.Expression( invoke( create.Load( "bean" ), methodReference( typeof( SomeBean ), typeof( void ), "setBar", typeof( string ) ), create.Load( "bar" ) ) );
						 create.Returns( create.Load( "bean" ) );
					}
					handle = simple.Handle();
			  }

			  // when
			  MethodHandle method = InstanceMethod( handle.NewInstance(), "createBean", typeof(string), typeof(string) );
			  SomeBean bean = ( SomeBean ) method.invoke( "hello", "world" );

			  // then
			  assertEquals( "hello", bean.FooConflict );
			  assertEquals( "world", bean.BarConflict );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDeclareAndAssignLocalVariable() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDeclareAndAssignLocalVariable()
		 {
			  // given
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					using ( CodeBlock create = simple.generateMethod( typeof( SomeBean ), "createBean", param( typeof( string ), "foo" ), param( typeof( string ), "bar" ) ) )
					{
						 LocalVariable localVariable = create.Declare( typeReference( typeof( SomeBean ) ), "bean" );
						 create.Assign( localVariable, invoke( newInstance( typeof( SomeBean ) ), constructorReference( typeof( SomeBean ) ) ) );
						 create.Expression( invoke( create.Load( "bean" ), methodReference( typeof( SomeBean ), typeof( void ), "setFoo", typeof( string ) ), create.Load( "foo" ) ) );
						 create.Expression( invoke( create.Load( "bean" ), methodReference( typeof( SomeBean ), typeof( void ), "setBar", typeof( string ) ), create.Load( "bar" ) ) );
						 create.Returns( create.Load( "bean" ) );
					}
					handle = simple.Handle();
			  }

			  // when
			  MethodHandle method = InstanceMethod( handle.NewInstance(), "createBean", typeof(string), typeof(string) );
			  SomeBean bean = ( SomeBean ) method.invoke( "hello", "world" );

			  // then
			  assertEquals( "hello", bean.FooConflict );
			  assertEquals( "world", bean.BarConflict );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateWhileLoop() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGenerateWhileLoop()
		 {
			  // given
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					using ( CodeBlock callEach = simple.generateMethod( typeof( void ), "callEach", param( TypeReference.ParameterizedType( typeof( System.Collections.IEnumerator ), typeof( ThreadStart ) ), "targets" ) ) )
					{
						 using ( CodeBlock loop = callEach.WhileLoop( invoke( callEach.Load( "targets" ), methodReference( typeof( System.Collections.IEnumerator ), typeof( bool ), "hasNext" ) ) ) )
						 {
							  loop.Expression( invoke( Expression.Cast( typeof( ThreadStart ), invoke( callEach.Load( "targets" ), methodReference( typeof( System.Collections.IEnumerator ), typeof( object ), "next" ) ) ), methodReference( typeof( ThreadStart ), typeof( void ), "run" ) ) );
						 }
					}

					handle = simple.Handle();
			  }
			  ThreadStart a = mock( typeof( ThreadStart ) );
			  ThreadStart b = mock( typeof( ThreadStart ) );
			  ThreadStart c = mock( typeof( ThreadStart ) );

			  // when
			  MethodHandle callEach = InstanceMethod( handle.NewInstance(), "callEach", typeof(System.Collections.IEnumerator) );
			  callEach.invoke( Arrays.asList( a, b, c ).GetEnumerator() );

			  // then
			  InOrder order = inOrder( a, b, c );
			  order.verify( a ).run();
			  order.verify( b ).run();
			  order.verify( c ).run();
			  verifyNoMoreInteractions( a, b, c );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateWhileLoopWithMultipleTestExpressions() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGenerateWhileLoopWithMultipleTestExpressions()
		 {
			  // given
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					using ( CodeBlock callEach = simple.generateMethod( typeof( void ), "check", param( typeof( bool ), "a" ), param( typeof( bool ), "b" ), param( typeof( ThreadStart ), "runner" ) ) )
					{
						 using ( CodeBlock loop = callEach.WhileLoop( and( callEach.Load( "a" ), callEach.Load( "b" ) ) ) )
						 {
							  loop.Expression( invoke( loop.Load( "runner" ), methodReference( typeof( ThreadStart ), typeof( void ), "run" ) ) );
							  loop.Returns();
						 }
					}

					handle = simple.Handle();
			  }
			  ThreadStart a = mock( typeof( ThreadStart ) );
			  ThreadStart b = mock( typeof( ThreadStart ) );
			  ThreadStart c = mock( typeof( ThreadStart ) );
			  ThreadStart d = mock( typeof( ThreadStart ) );

			  // when
			  MethodHandle callEach = InstanceMethod( handle.NewInstance(), "check", typeof(bool), typeof(bool), typeof(ThreadStart) );
			  callEach.invoke( true, true, a );
			  callEach.invoke( true, false, b );
			  callEach.invoke( false, true, c );
			  callEach.invoke( false, false, d );

			  // then
			  verify( a ).run();
			  verifyNoMoreInteractions( a );
			  verifyZeroInteractions( b );
			  verifyZeroInteractions( c );
			  verifyZeroInteractions( d );

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateNestedWhileLoop() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGenerateNestedWhileLoop()
		 {
			  // given
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					using ( CodeBlock callEach = simple.generateMethod( typeof( void ), "callEach", param( TypeReference.ParameterizedType( typeof( System.Collections.IEnumerator ), typeof( ThreadStart ) ), "targets" ) ) )
					{
						 using ( CodeBlock loop = callEach.WhileLoop( invoke( callEach.Load( "targets" ), methodReference( typeof( System.Collections.IEnumerator ), typeof( bool ), "hasNext" ) ) ) )
						 {
							  using ( CodeBlock inner = loop.WhileLoop( invoke( callEach.Load( "targets" ), methodReference( typeof( System.Collections.IEnumerator ), typeof( bool ), "hasNext" ) ) ) )
							  {

									inner.Expression( invoke( Expression.Cast( typeof( ThreadStart ), invoke( callEach.Load( "targets" ), methodReference( typeof( System.Collections.IEnumerator ), typeof( object ), "next" ) ) ), methodReference( typeof( ThreadStart ), typeof( void ), "run" ) ) );
							  }
						 }
					}

					handle = simple.Handle();
			  }
			  ThreadStart a = mock( typeof( ThreadStart ) );
			  ThreadStart b = mock( typeof( ThreadStart ) );
			  ThreadStart c = mock( typeof( ThreadStart ) );

			  // when
			  MethodHandle callEach = InstanceMethod( handle.NewInstance(), "callEach", typeof(System.Collections.IEnumerator) );
			  callEach.invoke( Arrays.asList( a, b, c ).GetEnumerator() );

			  // then
			  InOrder order = inOrder( a, b, c );
			  order.verify( a ).run();
			  order.verify( b ).run();
			  order.verify( c ).run();
			  verifyNoMoreInteractions( a, b, c );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateWhileLoopContinue() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGenerateWhileLoopContinue()
		 {
			  // given
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					using ( CodeBlock callEach = simple.generateMethod( typeof( void ), "callEach", param( TypeReference.ParameterizedType( typeof( System.Collections.IEnumerator ), typeof( ThreadStart ) ), "targets" ), param( TypeReference.ParameterizedType( typeof( System.Collections.IEnumerator ), typeof( Boolean ) ), "skipTargets" ) ) )
					{
						 using ( CodeBlock loop = callEach.WhileLoop( invoke( callEach.Load( "targets" ), methodReference( typeof( System.Collections.IEnumerator ), typeof( bool ), "hasNext" ) ) ) )
						 {
							  loop.Declare( TypeReference.TypeReferenceConflict( typeof( ThreadStart ) ), "target" );
							  loop.Assign( loop.Local( "target" ), Expression.Cast( typeof( ThreadStart ), invoke( callEach.Load( "targets" ), methodReference( typeof( System.Collections.IEnumerator ), typeof( object ), "next" ) ) ) );

							  loop.Declare( TypeReference.Boolean, "skip" );
							  loop.Assign( loop.Local( "skip" ), invoke( Expression.Cast( typeof( Boolean ), invoke( callEach.Load( "skipTargets" ), methodReference( typeof( System.Collections.IEnumerator ), typeof( object ), "next" ) ) ), methodReference( typeof( Boolean ), typeof( bool ), "booleanValue" ) ) );

							  using ( CodeBlock ifBlock = loop.IfStatement( loop.Load( "skip" ) ) )
							  {
									ifBlock.ContinueIfPossible();
							  }

							  loop.Expression( invoke( loop.Load( "target" ), methodReference( typeof( ThreadStart ), typeof( void ), "run" ) ) );
						 }
					}

					handle = simple.Handle();
			  }
			  ThreadStart a = mock( typeof( ThreadStart ) );
			  ThreadStart b = mock( typeof( ThreadStart ) );
			  ThreadStart c = mock( typeof( ThreadStart ) );

			  // when
			  MethodHandle callEach = InstanceMethod( handle.NewInstance(), "callEach", typeof(System.Collections.IEnumerator), typeof(System.Collections.IEnumerator) );
			  callEach.invoke( Arrays.asList( a, b, c ).GetEnumerator(), Arrays.asList(false, true, false).GetEnumerator() );

			  // then
			  InOrder order = inOrder( a, b, c );
			  order.verify( a ).run();
			  order.verify( c ).run();
			  verifyNoMoreInteractions( a, b, c );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateNestedWhileLoopInnerContinue() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGenerateNestedWhileLoopInnerContinue()
		 {
			  // given
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					using ( CodeBlock callEach = simple.generateMethod( typeof( void ), "callEach", param( TypeReference.ParameterizedType( typeof( System.Collections.IEnumerator ), typeof( ThreadStart ) ), "targetTargets" ), param( TypeReference.ParameterizedType( typeof( System.Collections.IEnumerator ), typeof( Boolean ) ), "skipTargets" ) ) )
					{
						 using ( CodeBlock outer = callEach.WhileLoop( invoke( callEach.Load( "targetTargets" ), methodReference( typeof( System.Collections.IEnumerator ), typeof( bool ), "hasNext" ) ) ) )
						 {
							  outer.Declare( TypeReference.TypeReferenceConflict( typeof( System.Collections.IEnumerator ) ), "targets" );
							  outer.Assign( outer.Local( "targets" ), Expression.Cast( typeof( System.Collections.IEnumerator ), invoke( callEach.Load( "targetTargets" ), methodReference( typeof( System.Collections.IEnumerator ), typeof( object ), "next" ) ) ) );

							  using ( CodeBlock inner = outer.WhileLoop( invoke( outer.Load( "targets" ), methodReference( typeof( System.Collections.IEnumerator ), typeof( bool ), "hasNext" ) ) ) )
							  {
									inner.Declare( TypeReference.TypeReferenceConflict( typeof( ThreadStart ) ), "target" );
									inner.Assign( inner.Local( "target" ), Expression.Cast( typeof( ThreadStart ), invoke( outer.Load( "targets" ), methodReference( typeof( System.Collections.IEnumerator ), typeof( object ), "next" ) ) ) );

									inner.Declare( TypeReference.Boolean, "skip" );
									inner.Assign( inner.Local( "skip" ), invoke( Expression.Cast( typeof( Boolean ), invoke( callEach.Load( "skipTargets" ), methodReference( typeof( System.Collections.IEnumerator ), typeof( object ), "next" ) ) ), methodReference( typeof( Boolean ), typeof( bool ), "booleanValue" ) ) );

									using ( CodeBlock ifBlock = inner.IfStatement( inner.Load( "skip" ) ) )
									{
										 ifBlock.ContinueIfPossible();
									}

									inner.Expression( invoke( inner.Load( "target" ), methodReference( typeof( ThreadStart ), typeof( void ), "run" ) ) );
							  }
						 }
					}

					handle = simple.Handle();
			  }

			  ThreadStart a = mock( typeof( ThreadStart ) );
			  ThreadStart b = mock( typeof( ThreadStart ) );
			  ThreadStart c = mock( typeof( ThreadStart ) );
			  ThreadStart d = mock( typeof( ThreadStart ) );
			  ThreadStart e = mock( typeof( ThreadStart ) );
			  ThreadStart f = mock( typeof( ThreadStart ) );

			  // when
			  IEnumerator<IEnumerator<ThreadStart>> input = Arrays.asList( Arrays.asList( a, b ).GetEnumerator(), Arrays.asList(c, d).GetEnumerator(), Arrays.asList(e, f).GetEnumerator() ).GetEnumerator();
			  IEnumerator<bool> skips = Arrays.asList( false, true, true, false, false, true ).GetEnumerator();

			  MethodHandle callEach = InstanceMethod( handle.NewInstance(), "callEach", typeof(System.Collections.IEnumerator), typeof(System.Collections.IEnumerator) );
			  callEach.invoke( input, skips );

			  // then
			  InOrder order = inOrder( a, b, c, d, e, f );
			  order.verify( a ).run();
			  order.verify( d ).run();
			  order.verify( e ).run();
			  verifyNoMoreInteractions( a, b, c, d, e, f );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateNestedWhileLoopDoubleContinue() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGenerateNestedWhileLoopDoubleContinue()
		 {
			  // given
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					using ( CodeBlock callEach = simple.generateMethod( typeof( void ), "callEach", param( TypeReference.ParameterizedType( typeof( System.Collections.IEnumerator ), typeof( ThreadStart ) ), "targetTargets" ), param( TypeReference.ParameterizedType( typeof( System.Collections.IEnumerator ), typeof( Boolean ) ), "skipOuters" ), param( TypeReference.ParameterizedType( typeof( System.Collections.IEnumerator ), typeof( Boolean ) ), "skipInners" ) ) )
					{
						 using ( CodeBlock outer = callEach.WhileLoop( invoke( callEach.Load( "targetTargets" ), methodReference( typeof( System.Collections.IEnumerator ), typeof( bool ), "hasNext" ) ) ) )
						 {
							  outer.Declare( TypeReference.TypeReferenceConflict( typeof( System.Collections.IEnumerator ) ), "targets" );
							  outer.Assign( outer.Local( "targets" ), Expression.Cast( typeof( System.Collections.IEnumerator ), invoke( callEach.Load( "targetTargets" ), methodReference( typeof( System.Collections.IEnumerator ), typeof( object ), "next" ) ) ) );

							  outer.Declare( TypeReference.Boolean, "skipOuter" );
							  outer.Assign( outer.Local( "skipOuter" ), invoke( Expression.Cast( typeof( Boolean ), invoke( callEach.Load( "skipOuters" ), methodReference( typeof( System.Collections.IEnumerator ), typeof( object ), "next" ) ) ), methodReference( typeof( Boolean ), typeof( bool ), "booleanValue" ) ) );

							  using ( CodeBlock ifBlock = outer.IfStatement( outer.Load( "skipOuter" ) ) )
							  {
									ifBlock.ContinueIfPossible();
							  }

							  using ( CodeBlock inner = outer.WhileLoop( invoke( outer.Load( "targets" ), methodReference( typeof( System.Collections.IEnumerator ), typeof( bool ), "hasNext" ) ) ) )
							  {
									inner.Declare( TypeReference.TypeReferenceConflict( typeof( ThreadStart ) ), "target" );
									inner.Assign( inner.Local( "target" ), Expression.Cast( typeof( ThreadStart ), invoke( outer.Load( "targets" ), methodReference( typeof( System.Collections.IEnumerator ), typeof( object ), "next" ) ) ) );

									inner.Declare( TypeReference.Boolean, "skipInner" );
									inner.Assign( inner.Local( "skipInner" ), invoke( Expression.Cast( typeof( Boolean ), invoke( callEach.Load( "skipInners" ), methodReference( typeof( System.Collections.IEnumerator ), typeof( object ), "next" ) ) ), methodReference( typeof( Boolean ), typeof( bool ), "booleanValue" ) ) );

									using ( CodeBlock ifBlock = inner.IfStatement( inner.Load( "skipInner" ) ) )
									{
										 ifBlock.ContinueIfPossible();
									}

									inner.Expression( invoke( inner.Load( "target" ), methodReference( typeof( ThreadStart ), typeof( void ), "run" ) ) );
							  }
						 }
					}

					handle = simple.Handle();
			  }

			  ThreadStart a1 = mock( typeof( ThreadStart ) );
			  ThreadStart a2 = mock( typeof( ThreadStart ) );
			  ThreadStart b1 = mock( typeof( ThreadStart ) );
			  ThreadStart b2 = mock( typeof( ThreadStart ) );
			  ThreadStart b3 = mock( typeof( ThreadStart ) );
			  ThreadStart b4 = mock( typeof( ThreadStart ) );
			  ThreadStart c1 = mock( typeof( ThreadStart ) );

			  // when
			  IEnumerator<IEnumerator<ThreadStart>> input = Arrays.asList( Arrays.asList( a1, a2 ).GetEnumerator(), Arrays.asList(b1, b2, b3, b4).GetEnumerator(), Arrays.asList(c1).GetEnumerator() ).GetEnumerator();
			  IEnumerator<bool> skipOuter = Arrays.asList( true, false, true ).GetEnumerator();
			  IEnumerator<bool> skipInner = Arrays.asList( false, true, false, true ).GetEnumerator();

			  MethodHandle callEach = InstanceMethod( handle.NewInstance(), "callEach", typeof(System.Collections.IEnumerator), typeof(System.Collections.IEnumerator), typeof(System.Collections.IEnumerator) );
			  callEach.invoke( input, skipOuter, skipInner );

			  // then
			  InOrder order = inOrder( a1, a2, b1, b2, b3, b4, c1 );
			  order.verify( b1 ).run();
			  order.verify( b3 ).run();
			  verifyNoMoreInteractions( a1, a2, b1, b2, b3, b4, c1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateForEachLoop() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGenerateForEachLoop()
		 {
			  // given
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					using ( CodeBlock callEach = simple.generateMethod( typeof( void ), "callEach", param( TypeReference.ParameterizedType( typeof( System.Collections.IEnumerable ), typeof( ThreadStart ) ), "targets" ) ) )
					{
						 using ( CodeBlock loop = callEach.ForEach( param( typeof( ThreadStart ), "runner" ), callEach.Load( "targets" ) ) )
						 {
							  loop.Expression( invoke( loop.Load( "runner" ), methodReference( typeof( ThreadStart ), typeof( void ), "run" ) ) );
						 }
					}

					handle = simple.Handle();
			  }
			  ThreadStart a = mock( typeof( ThreadStart ) );
			  ThreadStart b = mock( typeof( ThreadStart ) );
			  ThreadStart c = mock( typeof( ThreadStart ) );

			  // when
			  MethodHandle callEach = InstanceMethod( handle.NewInstance(), "callEach", typeof(System.Collections.IEnumerable) );
			  callEach.invoke( Arrays.asList( a, b, c ) );

			  // then
			  InOrder order = inOrder( a, b, c );
			  order.verify( a ).run();
			  order.verify( b ).run();
			  order.verify( c ).run();
			  verifyNoMoreInteractions( a, b, c );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateIfStatement() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGenerateIfStatement()
		 {
			  // given
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					using ( CodeBlock conditional = simple.generateMethod( typeof( void ), "conditional", param( typeof( bool ), "test" ), param( typeof( ThreadStart ), "runner" ) ) )
					{
						 using ( CodeBlock doStuff = conditional.IfStatement( conditional.Load( "test" ) ) )
						 {
							  doStuff.Expression( invoke( doStuff.Load( "runner" ), _run ) );
						 }
					}

					handle = simple.Handle();
			  }

			  ThreadStart runner1 = mock( typeof( ThreadStart ) );
			  ThreadStart runner2 = mock( typeof( ThreadStart ) );

			  // when
			  MethodHandle conditional = InstanceMethod( handle.NewInstance(), "conditional", typeof(bool), typeof(ThreadStart) );
			  conditional.invoke( true, runner1 );
			  conditional.invoke( false, runner2 );

			  // then
			  verify( runner1 ).run();
			  verifyZeroInteractions( runner2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateIfEqualsStatement() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGenerateIfEqualsStatement()
		 {
			  // given
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					using ( CodeBlock conditional = simple.generateMethod( typeof( void ), "conditional", param( typeof( object ), "lhs" ), param( typeof( object ), "rhs" ), param( typeof( ThreadStart ), "runner" ) ) )
					{
						 using ( CodeBlock doStuff = conditional.IfStatement( equal( conditional.Load( "lhs" ), conditional.Load( "rhs" ) ) ) )
						 {
							  doStuff.Expression( invoke( doStuff.Load( "runner" ), _run ) );
						 }
					}

					handle = simple.Handle();
			  }

			  ThreadStart runner1 = mock( typeof( ThreadStart ) );
			  ThreadStart runner2 = mock( typeof( ThreadStart ) );
			  object a = "a";
			  object b = "b";

			  // when
			  MethodHandle conditional = InstanceMethod( handle.NewInstance(), "conditional", typeof(object), typeof(object), typeof(ThreadStart) );
			  conditional.invoke( a, b, runner1 );
			  conditional.invoke( a, a, runner2 );

			  // then
			  verify( runner2 ).run();
			  verifyZeroInteractions( runner1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateIfNotEqualsStatement() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGenerateIfNotEqualsStatement()
		 {
			  // given
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					using ( CodeBlock conditional = simple.generateMethod( typeof( void ), "conditional", param( typeof( object ), "lhs" ), param( typeof( object ), "rhs" ), param( typeof( ThreadStart ), "runner" ) ) )
					{
						 using ( CodeBlock doStuff = conditional.IfStatement( not( equal( conditional.Load( "lhs" ), conditional.Load( "rhs" ) ) ) ) )
						 {
							  doStuff.Expression( invoke( doStuff.Load( "runner" ), _run ) );
						 }
					}

					handle = simple.Handle();
			  }

			  ThreadStart runner1 = mock( typeof( ThreadStart ) );
			  ThreadStart runner2 = mock( typeof( ThreadStart ) );
			  object a = "a";
			  object b = "b";

			  // when
			  MethodHandle conditional = InstanceMethod( handle.NewInstance(), "conditional", typeof(object), typeof(object), typeof(ThreadStart) );
			  conditional.invoke( a, a, runner1 );
			  conditional.invoke( a, b, runner2 );

			  // then
			  verify( runner2 ).run();
			  verifyZeroInteractions( runner1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateIfNotExpressionStatement() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGenerateIfNotExpressionStatement()
		 {
			  // given
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					using ( CodeBlock conditional = simple.generateMethod( typeof( void ), "conditional", param( typeof( bool ), "test" ), param( typeof( ThreadStart ), "runner" ) ) )
					{
						 using ( CodeBlock doStuff = conditional.IfStatement( not( conditional.Load( "test" ) ) ) )
						 {
							  doStuff.Expression( invoke( doStuff.Load( "runner" ), _run ) );
						 }
					}

					handle = simple.Handle();
			  }

			  ThreadStart runner1 = mock( typeof( ThreadStart ) );
			  ThreadStart runner2 = mock( typeof( ThreadStart ) );

			  // when
			  MethodHandle conditional = InstanceMethod( handle.NewInstance(), "conditional", typeof(bool), typeof(ThreadStart) );
			  conditional.invoke( true, runner1 );
			  conditional.invoke( false, runner2 );

			  // then
			  verify( runner2 ).run();
			  verifyZeroInteractions( runner1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateIfNullStatement() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGenerateIfNullStatement()
		 {
			  // given
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					using ( CodeBlock conditional = simple.generateMethod( typeof( void ), "conditional", param( typeof( object ), "test" ), param( typeof( ThreadStart ), "runner" ) ) )
					{
						 using ( CodeBlock doStuff = conditional.IfStatement( isNull( conditional.Load( "test" ) ) ) )
						 {
							  doStuff.Expression( invoke( doStuff.Load( "runner" ), _run ) );
						 }
					}

					handle = simple.Handle();
			  }

			  ThreadStart runner1 = mock( typeof( ThreadStart ) );
			  ThreadStart runner2 = mock( typeof( ThreadStart ) );

			  // when
			  MethodHandle conditional = InstanceMethod( handle.NewInstance(), "conditional", typeof(object), typeof(ThreadStart) );
			  conditional.invoke( null, runner1 );
			  conditional.invoke( new object(), runner2 );

			  // then
			  verify( runner1 ).run();
			  verifyZeroInteractions( runner2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateIfNonNullStatement() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGenerateIfNonNullStatement()
		 {
			  // given
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					using ( CodeBlock conditional = simple.generateMethod( typeof( void ), "conditional", param( typeof( object ), "test" ), param( typeof( ThreadStart ), "runner" ) ) )
					{
						 using ( CodeBlock doStuff = conditional.IfStatement( notNull( conditional.Load( "test" ) ) ) )
						 {
							  doStuff.Expression( invoke( doStuff.Load( "runner" ), _run ) );
						 }
					}

					handle = simple.Handle();
			  }

			  ThreadStart runner1 = mock( typeof( ThreadStart ) );
			  ThreadStart runner2 = mock( typeof( ThreadStart ) );

			  // when
			  MethodHandle conditional = InstanceMethod( handle.NewInstance(), "conditional", typeof(object), typeof(ThreadStart) );
			  conditional.invoke( new object(), runner1 );
			  conditional.invoke( null, runner2 );

			  // then
			  verify( runner1 ).run();
			  verifyZeroInteractions( runner2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateTryWithNestedWhileIfLoop() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGenerateTryWithNestedWhileIfLoop()
		 {
			  // given
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					using ( CodeBlock callEach = simple.generateMethod( typeof( void ), "callEach", param( TypeReference.ParameterizedType( typeof( System.Collections.IEnumerator ), typeof( ThreadStart ) ), "targets" ), param( typeof( bool ), "test" ), param( typeof( ThreadStart ), "runner" ) ) )
					{
						 callEach.TryCatch(tryBlock =>
						 {
									 using ( CodeBlock loop = tryBlock.whileLoop( invoke( callEach.Load( "targets" ), methodReference( typeof( System.Collections.IEnumerator ), typeof( bool ), "hasNext" ) ) ) )
									 {

										  using ( CodeBlock doStuff = loop.IfStatement( not( callEach.Load( "test" ) ) ) )
										  {
												doStuff.Expression( invoke( doStuff.Load( "runner" ), _run ) );
										  }
										  loop.Expression( invoke( Expression.Cast( typeof( ThreadStart ), invoke( callEach.Load( "targets" ), methodReference( typeof( System.Collections.IEnumerator ), typeof( object ), "next" ) ) ), methodReference( typeof( ThreadStart ), typeof( void ), "run" ) ) );

									 }
						 }, catchBlock => catchBlock.expression( invoke( catchBlock.load( "runner" ), _run ) ), param( typeof( Exception ), "e" ));
					}

					handle = simple.Handle();
			  }
			  ThreadStart a = mock( typeof( ThreadStart ) );
			  ThreadStart b = mock( typeof( ThreadStart ) );
			  ThreadStart c = mock( typeof( ThreadStart ) );

			  ThreadStart runner1 = mock( typeof( ThreadStart ) );
			  ThreadStart runner2 = mock( typeof( ThreadStart ) );

			  // when
			  MethodHandle callEach = InstanceMethod( handle.NewInstance(), "callEach", typeof(System.Collections.IEnumerator), typeof(bool), typeof(ThreadStart) );

			  callEach.invoke( Arrays.asList( a, b, c ).GetEnumerator(), false, runner1 );
			  callEach.invoke( Arrays.asList( a, b, c ).GetEnumerator(), true, runner2 );

			  // then
			  verify( runner1, times( 3 ) ).run();
			  verify( runner2, never() ).run();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateWhileWithNestedIfLoop() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGenerateWhileWithNestedIfLoop()
		 {
			  // given
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					using ( CodeBlock callEach = simple.generateMethod( typeof( void ), "callEach", param( TypeReference.ParameterizedType( typeof( System.Collections.IEnumerator ), typeof( ThreadStart ) ), "targets" ), param( typeof( bool ), "test" ), param( typeof( ThreadStart ), "runner" ) ) )
					{
						 using ( CodeBlock loop = callEach.WhileLoop( invoke( callEach.Load( "targets" ), methodReference( typeof( System.Collections.IEnumerator ), typeof( bool ), "hasNext" ) ) ) )
						 {
							  using ( CodeBlock doStuff = loop.IfStatement( not( callEach.Load( "test" ) ) ) )
							  {
									doStuff.Expression( invoke( doStuff.Load( "runner" ), _run ) );
							  }
							  loop.Expression( invoke( Expression.Cast( typeof( ThreadStart ), invoke( callEach.Load( "targets" ), methodReference( typeof( System.Collections.IEnumerator ), typeof( object ), "next" ) ) ), methodReference( typeof( ThreadStart ), typeof( void ), "run" ) ) );
						 }
					}

					handle = simple.Handle();
			  }
			  ThreadStart a = mock( typeof( ThreadStart ) );
			  ThreadStart b = mock( typeof( ThreadStart ) );
			  ThreadStart c = mock( typeof( ThreadStart ) );

			  ThreadStart runner1 = mock( typeof( ThreadStart ) );
			  ThreadStart runner2 = mock( typeof( ThreadStart ) );
			  // when
			  MethodHandle callEach = InstanceMethod( handle.NewInstance(), "callEach", typeof(System.Collections.IEnumerator), typeof(bool), typeof(ThreadStart) );

			  callEach.invoke( Arrays.asList( a, b, c ).GetEnumerator(), false, runner1 );
			  callEach.invoke( Arrays.asList( a, b, c ).GetEnumerator(), true, runner2 );

			  // then
			  verify( runner1, times( 3 ) ).run();
			  verify( runner2, never() ).run();

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateOr() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGenerateOr()
		 {
			  // given
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					using ( CodeBlock conditional = simple.generateMethod( typeof( void ), "conditional", param( typeof( bool ), "test1" ), param( typeof( bool ), "test2" ), param( typeof( ThreadStart ), "runner" ) ) )
					{
						 using ( CodeBlock doStuff = conditional.IfStatement( Expression.Or( conditional.Load( "test1" ), conditional.Load( "test2" ) ) ) )
						 {
							  doStuff.Expression( invoke( doStuff.Load( "runner" ), _run ) );
						 }
					}

					handle = simple.Handle();
			  }

			  ThreadStart runner1 = mock( typeof( ThreadStart ) );
			  ThreadStart runner2 = mock( typeof( ThreadStart ) );
			  ThreadStart runner3 = mock( typeof( ThreadStart ) );
			  ThreadStart runner4 = mock( typeof( ThreadStart ) );

			  // when
			  MethodHandle conditional = InstanceMethod( handle.NewInstance(), "conditional", typeof(bool), typeof(bool), typeof(ThreadStart) );
			  conditional.invoke( true, true, runner1 );
			  conditional.invoke( true, false, runner2 );
			  conditional.invoke( false, true, runner3 );
			  conditional.invoke( false, false, runner4 );

			  // then
			  verify( runner1 ).run();
			  verify( runner2 ).run();
			  verify( runner3 ).run();
			  verifyZeroInteractions( runner4 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateMethodUsingOr() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGenerateMethodUsingOr()
		 {
			  // given
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					using ( CodeBlock conditional = simple.generateMethod( typeof( bool ), "conditional", param( typeof( bool ), "test1" ), param( typeof( bool ), "test2" ) ) )
					{
						 conditional.Returns( or( conditional.Load( "test1" ), conditional.Load( "test2" ) ) );
					}

					handle = simple.Handle();
			  }

			  // when
			  MethodHandle conditional = InstanceMethod( handle.NewInstance(), "conditional", typeof(bool), typeof(bool) );

			  // then
			  assertThat( conditional.invoke( true, true ), equalTo( true ) );
			  assertThat( conditional.invoke( true, false ), equalTo( true ) );
			  assertThat( conditional.invoke( false, true ), equalTo( true ) );
			  assertThat( conditional.invoke( false, false ), equalTo( false ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateAnd() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGenerateAnd()
		 {
			  // given
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					using ( CodeBlock conditional = simple.generateMethod( typeof( void ), "conditional", param( typeof( bool ), "test1" ), param( typeof( bool ), "test2" ), param( typeof( ThreadStart ), "runner" ) ) )
					{
						 using ( CodeBlock doStuff = conditional.IfStatement( and( conditional.Load( "test1" ), conditional.Load( "test2" ) ) ) )
						 {
							  doStuff.Expression( invoke( doStuff.Load( "runner" ), _run ) );
						 }
					}

					handle = simple.Handle();
			  }

			  ThreadStart runner1 = mock( typeof( ThreadStart ) );
			  ThreadStart runner2 = mock( typeof( ThreadStart ) );
			  ThreadStart runner3 = mock( typeof( ThreadStart ) );
			  ThreadStart runner4 = mock( typeof( ThreadStart ) );

			  // when
			  MethodHandle conditional = InstanceMethod( handle.NewInstance(), "conditional", typeof(bool), typeof(bool), typeof(ThreadStart) );
			  conditional.invoke( true, true, runner1 );
			  conditional.invoke( true, false, runner2 );
			  conditional.invoke( false, true, runner3 );
			  conditional.invoke( false, false, runner4 );

			  // then
			  verify( runner1 ).run();
			  verifyZeroInteractions( runner2 );
			  verifyZeroInteractions( runner3 );
			  verifyZeroInteractions( runner4 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateMethodUsingAnd() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGenerateMethodUsingAnd()
		 {
			  // given
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					using ( CodeBlock conditional = simple.generateMethod( typeof( bool ), "conditional", param( typeof( bool ), "test1" ), param( typeof( bool ), "test2" ) ) )
					{
						 conditional.Returns( and( conditional.Load( "test1" ), conditional.Load( "test2" ) ) );
					}

					handle = simple.Handle();
			  }

			  // when
			  MethodHandle conditional = InstanceMethod( handle.NewInstance(), "conditional", typeof(bool), typeof(bool) );

			  // then
			  assertThat( conditional.invoke( true, true ), equalTo( true ) );
			  assertThat( conditional.invoke( true, false ), equalTo( false ) );
			  assertThat( conditional.invoke( false, true ), equalTo( false ) );
			  assertThat( conditional.invoke( false, false ), equalTo( false ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateMethodUsingMultipleAnds() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGenerateMethodUsingMultipleAnds()
		 {
			  // given
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					using ( CodeBlock conditional = simple.generateMethod( typeof( bool ), "conditional", param( typeof( bool ), "test1" ), param( typeof( bool ), "test2" ), param( typeof( bool ), "test3" ) ) )
					{
						 conditional.Returns( and( conditional.Load( "test1" ), and( conditional.Load( "test2" ), conditional.Load( "test3" ) ) ) );
					}

					handle = simple.Handle();
			  }

			  // when
			  MethodHandle conditional = InstanceMethod( handle.NewInstance(), "conditional", typeof(bool), typeof(bool), typeof(bool) );

			  // then
			  assertThat( conditional.invoke( true, true, true ), equalTo( true ) );
			  assertThat( conditional.invoke( true, false, true ), equalTo( false ) );
			  assertThat( conditional.invoke( false, true, true ), equalTo( false ) );
			  assertThat( conditional.invoke( false, false, true ), equalTo( false ) );
			  assertThat( conditional.invoke( true, true, false ), equalTo( false ) );
			  assertThat( conditional.invoke( true, false, false ), equalTo( false ) );
			  assertThat( conditional.invoke( false, true, false ), equalTo( false ) );
			  assertThat( conditional.invoke( false, false, false ), equalTo( false ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateMethodUsingMultipleAnds2() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGenerateMethodUsingMultipleAnds2()
		 {
			  // given
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					using ( CodeBlock conditional = simple.generateMethod( typeof( bool ), "conditional", param( typeof( bool ), "test1" ), param( typeof( bool ), "test2" ), param( typeof( bool ), "test3" ) ) )
					{
						 conditional.Returns( and( and( conditional.Load( "test1" ), conditional.Load( "test2" ) ), conditional.Load( "test3" ) ) );
					}

					handle = simple.Handle();
			  }

			  // when
			  MethodHandle conditional = InstanceMethod( handle.NewInstance(), "conditional", typeof(bool), typeof(bool), typeof(bool) );

			  // then
			  assertThat( conditional.invoke( true, true, true ), equalTo( true ) );
			  assertThat( conditional.invoke( true, false, true ), equalTo( false ) );
			  assertThat( conditional.invoke( false, true, true ), equalTo( false ) );
			  assertThat( conditional.invoke( false, false, true ), equalTo( false ) );
			  assertThat( conditional.invoke( true, true, false ), equalTo( false ) );
			  assertThat( conditional.invoke( true, false, false ), equalTo( false ) );
			  assertThat( conditional.invoke( false, true, false ), equalTo( false ) );
			  assertThat( conditional.invoke( false, false, false ), equalTo( false ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateMethodUsingMultipleOrs() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGenerateMethodUsingMultipleOrs()
		 {
			  // given
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					using ( CodeBlock conditional = simple.generateMethod( typeof( bool ), "conditional", param( typeof( bool ), "test1" ), param( typeof( bool ), "test2" ), param( typeof( bool ), "test3" ) ) )
					{
						 conditional.Returns( or( conditional.Load( "test1" ), or( conditional.Load( "test2" ), conditional.Load( "test3" ) ) ) );
					}

					handle = simple.Handle();
			  }

			  // when
			  MethodHandle conditional = InstanceMethod( handle.NewInstance(), "conditional", typeof(bool), typeof(bool), typeof(bool) );

			  // then
			  assertThat( conditional.invoke( true, true, true ), equalTo( true ) );
			  assertThat( conditional.invoke( true, false, true ), equalTo( true ) );
			  assertThat( conditional.invoke( false, true, true ), equalTo( true ) );
			  assertThat( conditional.invoke( false, false, true ), equalTo( true ) );
			  assertThat( conditional.invoke( true, true, false ), equalTo( true ) );
			  assertThat( conditional.invoke( true, false, false ), equalTo( true ) );
			  assertThat( conditional.invoke( false, true, false ), equalTo( true ) );
			  assertThat( conditional.invoke( false, false, false ), equalTo( false ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateMethodUsingMultipleOrs2() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGenerateMethodUsingMultipleOrs2()
		 {
			  // given
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					using ( CodeBlock conditional = simple.generateMethod( typeof( bool ), "conditional", param( typeof( bool ), "test1" ), param( typeof( bool ), "test2" ), param( typeof( bool ), "test3" ) ) )
					{
						 conditional.Returns( or( or( conditional.Load( "test1" ), conditional.Load( "test2" ) ), conditional.Load( "test3" ) ) );
					}

					handle = simple.Handle();
			  }

			  // when
			  MethodHandle conditional = InstanceMethod( handle.NewInstance(), "conditional", typeof(bool), typeof(bool), typeof(bool) );

			  // then
			  assertThat( conditional.invoke( true, true, true ), equalTo( true ) );
			  assertThat( conditional.invoke( true, false, true ), equalTo( true ) );
			  assertThat( conditional.invoke( false, true, true ), equalTo( true ) );
			  assertThat( conditional.invoke( false, false, true ), equalTo( true ) );
			  assertThat( conditional.invoke( true, true, false ), equalTo( true ) );
			  assertThat( conditional.invoke( true, false, false ), equalTo( true ) );
			  assertThat( conditional.invoke( false, true, false ), equalTo( true ) );
			  assertThat( conditional.invoke( false, false, false ), equalTo( false ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleNot() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleNot()
		 {
			  // given
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					using ( CodeBlock conditional = simple.generateMethod( typeof( bool ), "conditional", param( typeof( bool ), "test" ) ) )
					{
						 conditional.Returns( not( conditional.Load( "test" ) ) );
					}

					handle = simple.Handle();
			  }

			  // when
			  MethodHandle conditional = InstanceMethod( handle.NewInstance(), "conditional", typeof(bool) );

			  // then
			  assertThat( conditional.invoke( true ), equalTo( false ) );
			  assertThat( conditional.invoke( false ), equalTo( true ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleTernaryOperator() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleTernaryOperator()
		 {
			  // given
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					using ( CodeBlock ternaryBlock = simple.generateMethod( typeof( string ), "ternary", param( typeof( bool ), "test" ), param( typeof( TernaryChecker ), "check" ) ) )
					{
						 ternaryBlock.Returns( ternary( ternaryBlock.Load( "test" ), invoke( ternaryBlock.Load( "check" ), methodReference( typeof( TernaryChecker ), typeof( string ), "onTrue" ) ), invoke( ternaryBlock.Load( "check" ), methodReference( typeof( TernaryChecker ), typeof( string ), "onFalse" ) ) ) );
					}

					handle = simple.Handle();
			  }

			  // when
			  MethodHandle ternary = InstanceMethod( handle.NewInstance(), "ternary", typeof(bool), typeof(TernaryChecker) );

			  // then
			  TernaryChecker checker1 = new TernaryChecker();
			  assertThat( ternary.invoke( true, checker1 ), equalTo( "on true" ) );
			  assertTrue( checker1.RanOnTrue );
			  assertFalse( checker1.RanOnFalse );

			  TernaryChecker checker2 = new TernaryChecker();
			  assertThat( ternary.invoke( false, checker2 ), equalTo( "on false" ) );
			  assertFalse( checker2.RanOnTrue );
			  assertTrue( checker2.RanOnFalse );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleTernaryOnNullOperator() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleTernaryOnNullOperator()
		 {
			  // given
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					using ( CodeBlock ternaryBlock = simple.generateMethod( typeof( string ), "ternary", param( typeof( object ), "test" ), param( typeof( TernaryChecker ), "check" ) ) )
					{
						 ternaryBlock.Returns( ternary( isNull( ternaryBlock.Load( "test" ) ), invoke( ternaryBlock.Load( "check" ), methodReference( typeof( TernaryChecker ), typeof( string ), "onTrue" ) ), invoke( ternaryBlock.Load( "check" ), methodReference( typeof( TernaryChecker ), typeof( string ), "onFalse" ) ) ) );
					}

					handle = simple.Handle();
			  }

			  // when
			  MethodHandle ternary = InstanceMethod( handle.NewInstance(), "ternary", typeof(object), typeof(TernaryChecker) );

			  // then
			  TernaryChecker checker1 = new TernaryChecker();
			  assertThat( ternary.invoke( null, checker1 ), equalTo( "on true" ) );
			  assertTrue( checker1.RanOnTrue );
			  assertFalse( checker1.RanOnFalse );

			  TernaryChecker checker2 = new TernaryChecker();
			  assertThat( ternary.invoke( new object(), checker2 ), equalTo("on false") );
			  assertFalse( checker2.RanOnTrue );
			  assertTrue( checker2.RanOnFalse );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleTernaryOnNonNullOperator() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleTernaryOnNonNullOperator()
		 {
			  // given
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					using ( CodeBlock ternaryBlock = simple.generateMethod( typeof( string ), "ternary", param( typeof( object ), "test" ), param( typeof( TernaryChecker ), "check" ) ) )
					{
						 ternaryBlock.Returns( ternary( notNull( ternaryBlock.Load( "test" ) ), invoke( ternaryBlock.Load( "check" ), methodReference( typeof( TernaryChecker ), typeof( string ), "onTrue" ) ), invoke( ternaryBlock.Load( "check" ), methodReference( typeof( TernaryChecker ), typeof( string ), "onFalse" ) ) ) );
					}

					handle = simple.Handle();
			  }

			  // when
			  MethodHandle ternary = InstanceMethod( handle.NewInstance(), "ternary", typeof(object), typeof(TernaryChecker) );

			  // then
			  TernaryChecker checker1 = new TernaryChecker();
			  assertThat( ternary.invoke( new object(), checker1 ), equalTo("on true") );
			  assertTrue( checker1.RanOnTrue );
			  assertFalse( checker1.RanOnFalse );

			  TernaryChecker checker2 = new TernaryChecker();
			  assertThat( ternary.invoke( null, checker2 ), equalTo( "on false" ) );
			  assertFalse( checker2.RanOnTrue );
			  assertTrue( checker2.RanOnFalse );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleEquality() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleEquality()
		 {
			  // boolean
			  assertTrue( CompareForType( typeof( bool ), true, true, Expression.equal ) );
			  assertTrue( CompareForType( typeof( bool ), false, false, Expression.equal ) );
			  assertFalse( CompareForType( typeof( bool ), true, false, Expression.equal ) );
			  assertFalse( CompareForType( typeof( bool ), false, true, Expression.equal ) );

			  // byte
			  assertTrue( CompareForType( typeof( sbyte ), ( sbyte ) 42, ( sbyte ) 42, Expression.equal ) );
			  assertFalse( CompareForType( typeof( sbyte ), ( sbyte ) 43, ( sbyte ) 42, Expression.equal ) );
			  assertFalse( CompareForType( typeof( sbyte ), ( sbyte ) 42, ( sbyte ) 43, Expression.equal ) );

			  // short
			  assertTrue( CompareForType( typeof( short ), ( short ) 42, ( short ) 42, Expression.equal ) );
			  assertFalse( CompareForType( typeof( short ), ( short ) 43, ( short ) 42, Expression.equal ) );
			  assertFalse( CompareForType( typeof( short ), ( short ) 42, ( short ) 43, Expression.equal ) );

			  // char
			  assertTrue( CompareForType( typeof( char ), ( char ) 42, ( char ) 42, Expression.equal ) );
			  assertFalse( CompareForType( typeof( char ), ( char ) 43, ( char ) 42, Expression.equal ) );
			  assertFalse( CompareForType( typeof( char ), ( char ) 42, ( char ) 43, Expression.equal ) );

			  //int
			  assertTrue( CompareForType( typeof( int ), 42, 42, Expression.equal ) );
			  assertFalse( CompareForType( typeof( int ), 43, 42, Expression.equal ) );
			  assertFalse( CompareForType( typeof( int ), 42, 43, Expression.equal ) );

			  //long
			  assertTrue( CompareForType( typeof( long ), 42L, 42L, Expression.equal ) );
			  assertFalse( CompareForType( typeof( long ), 43L, 42L, Expression.equal ) );
			  assertFalse( CompareForType( typeof( long ), 42L, 43L, Expression.equal ) );

			  //float
			  assertTrue( CompareForType( typeof( float ), 42F, 42F, Expression.equal ) );
			  assertFalse( CompareForType( typeof( float ), 43F, 42F, Expression.equal ) );
			  assertFalse( CompareForType( typeof( float ), 42F, 43F, Expression.equal ) );

			  //double
			  assertTrue( CompareForType( typeof( double ), 42D, 42D, Expression.equal ) );
			  assertFalse( CompareForType( typeof( double ), 43D, 42D, Expression.equal ) );
			  assertFalse( CompareForType( typeof( double ), 42D, 43D, Expression.equal ) );

			  //reference
			  object obj1 = new object();
			  object obj2 = new object();
			  assertTrue( CompareForType( typeof( object ), obj1, obj1, Expression.equal ) );
			  assertFalse( CompareForType( typeof( object ), obj1, obj2, Expression.equal ) );
			  assertFalse( CompareForType( typeof( object ), obj2, obj1, Expression.equal ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleGreaterThan() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleGreaterThan()
		 {
			  assertTrue( CompareForType( typeof( float ), 43F, 42F, Expression.gt ) );
			  assertTrue( CompareForType( typeof( long ), 43L, 42L, Expression.gt ) );

			  // byte
			  assertTrue( CompareForType( typeof( sbyte ), ( sbyte ) 43, ( sbyte ) 42, Expression.gt ) );
			  assertFalse( CompareForType( typeof( sbyte ), ( sbyte ) 42, ( sbyte ) 42, Expression.gt ) );
			  assertFalse( CompareForType( typeof( sbyte ), ( sbyte ) 42, ( sbyte ) 43, Expression.gt ) );

			  // short
			  assertTrue( CompareForType( typeof( short ), ( short ) 43, ( short ) 42, Expression.gt ) );
			  assertFalse( CompareForType( typeof( short ), ( short ) 42, ( short ) 42, Expression.gt ) );
			  assertFalse( CompareForType( typeof( short ), ( short ) 42, ( short ) 43, Expression.gt ) );

			  // char
			  assertTrue( CompareForType( typeof( char ), ( char ) 43, ( char ) 42, Expression.gt ) );
			  assertFalse( CompareForType( typeof( char ), ( char ) 42, ( char ) 42, Expression.gt ) );
			  assertFalse( CompareForType( typeof( char ), ( char ) 42, ( char ) 43, Expression.gt ) );

			  //int
			  assertTrue( CompareForType( typeof( int ), 43, 42, Expression.gt ) );
			  assertFalse( CompareForType( typeof( int ), 42, 42, Expression.gt ) );
			  assertFalse( CompareForType( typeof( int ), 42, 43, Expression.gt ) );

			  //long
			  assertTrue( CompareForType( typeof( long ), 43L, 42L, Expression.gt ) );
			  assertFalse( CompareForType( typeof( long ), 42L, 42L, Expression.gt ) );
			  assertFalse( CompareForType( typeof( long ), 42L, 43L, Expression.gt ) );

			  //float
			  assertTrue( CompareForType( typeof( float ), 43F, 42F, Expression.gt ) );
			  assertFalse( CompareForType( typeof( float ), 42F, 42F, Expression.gt ) );
			  assertFalse( CompareForType( typeof( float ), 42F, 43F, Expression.gt ) );

			  //double
			  assertTrue( CompareForType( typeof( double ), 43D, 42D, Expression.gt ) );
			  assertFalse( CompareForType( typeof( double ), 42D, 42D, Expression.gt ) );
			  assertFalse( CompareForType( typeof( double ), 42D, 43D, Expression.gt ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleAddition() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleAddition()
		 {
			  assertThat( AddForType( typeof( int ), 17, 18 ), equalTo( 35 ) );
			  assertThat( AddForType( typeof( long ), 17L, 18L ), equalTo( 35L ) );
			  assertThat( AddForType( typeof( double ), 17D, 18D ), equalTo( 35D ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleSubtraction() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleSubtraction()
		 {
			  assertThat( SubtractForType( typeof( int ), 19, 18 ), equalTo( 1 ) );
			  assertThat( SubtractForType( typeof( long ), 19L, 18L ), equalTo( 1L ) );
			  assertThat( SubtractForType( typeof( double ), 19D, 18D ), equalTo( 1D ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleMultiplication() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleMultiplication()
		 {
			  assertThat( MultiplyForType( typeof( int ), 17, 18 ), equalTo( 306 ) );
			  assertThat( MultiplyForType( typeof( long ), 17L, 18L ), equalTo( 306L ) );
			  assertThat( MultiplyForType( typeof( double ), 17D, 18D ), equalTo( 306D ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private <T> T addForType(Class<T> clazz, T lhs, T rhs) throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 private T AddForType<T>( Type clazz, T lhs, T rhs )
		 {
				 clazz = typeof( T );
			  // given
			  CreateGenerator();
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					using ( CodeBlock block = simple.generateMethod( clazz, "add", param( clazz, "a" ), param( clazz, "b" ) ) )
					{
						 block.Returns( add( block.Load( "a" ), block.Load( "b" ) ) );
					}

					handle = simple.Handle();
			  }

			  // when
			  MethodHandle add = InstanceMethod( handle.NewInstance(), "add", clazz, clazz );

			  // then
			  return ( T ) add.invoke( lhs, rhs );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private <T> T subtractForType(Class<T> clazz, T lhs, T rhs) throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 private T SubtractForType<T>( Type clazz, T lhs, T rhs )
		 {
				 clazz = typeof( T );
			  // given
			  CreateGenerator();
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					using ( CodeBlock block = simple.generateMethod( clazz, "sub", param( clazz, "a" ), param( clazz, "b" ) ) )
					{
							  block.Returns( subtract( block.Load( "a" ), block.Load( "b" ) ) );
					}

					handle = simple.Handle();
			  }

			  // when
			  MethodHandle sub = InstanceMethod( handle.NewInstance(), "sub", clazz, clazz );

			  // then
			  return ( T ) sub.invoke( lhs, rhs );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private <T> T multiplyForType(Class<T> clazz, T lhs, T rhs) throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 private T MultiplyForType<T>( Type clazz, T lhs, T rhs )
		 {
				 clazz = typeof( T );
			  // given
			  CreateGenerator();
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					using ( CodeBlock block = simple.generateMethod( clazz, "multiply", param( clazz, "a" ), param( clazz, "b" ) ) )
					{
						 block.Returns( multiply( block.Load( "a" ), block.Load( "b" ) ) );
					}

					handle = simple.Handle();
			  }

			  // when
			  MethodHandle sub = InstanceMethod( handle.NewInstance(), "multiply", clazz, clazz );

			  // then
			  return ( T ) sub.invoke( lhs, rhs );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private <T> boolean compareForType(Class<T> clazz, T lhs, T rhs, System.Func<Expression,Expression,Expression> compare) throws Throwable
		 private bool CompareForType<T>( Type clazz, T lhs, T rhs, System.Func<Expression, Expression, Expression> compare )
		 {
				 clazz = typeof( T );
			  // given
			  CreateGenerator();
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					using ( CodeBlock block = simple.generateMethod( typeof( bool ), "compare", param( clazz, "a" ), param( clazz, "b" ) ) )
					{
						 block.Returns( compare( block.Load( "a" ), block.Load( "b" ) ) );
					}

					handle = simple.Handle();
			  }

			  // when
			  MethodHandle compareFcn = InstanceMethod( handle.NewInstance(), "compare", clazz, clazz );

			  // then
			  return ( bool ) compareFcn.invoke( lhs, rhs );
		 }

		 public class TernaryChecker
		 {
			  internal bool RanOnTrue;
			  internal bool RanOnFalse;

			  public virtual string OnTrue()
			  {
					RanOnTrue = true;
					return "on true";
			  }

			  public virtual string OnFalse()
			  {
					RanOnFalse = true;
					return "on false";
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateTryCatch() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGenerateTryCatch()
		 {
			  // given
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					using ( CodeBlock run = simple.generateMethod( typeof( void ), "run", param( typeof( ThreadStart ), "body" ), param( typeof( ThreadStart ), "catcher" ) ) )
					{
						 run.TryCatch( body => body.expression( invoke( run.Load( "body" ), _run ) ), handler => handler.expression( invoke( run.Load( "catcher" ), _run ) ), param( typeof( Exception ), "E" ) );
					}
					handle = simple.Handle();
			  }

			  // when
			  ThreadStart successBody = mock( typeof( ThreadStart ) );
			  ThreadStart failBody = mock( typeof( ThreadStart ) );
			  ThreadStart successCatch = mock( typeof( ThreadStart ) );
			  ThreadStart failCatch = mock( typeof( ThreadStart ) );
			  Exception theFailure = new Exception();
			  doThrow( theFailure ).when( failBody ).run();
			  MethodHandle run = InstanceMethod( handle.NewInstance(), "run", typeof(ThreadStart), typeof(ThreadStart) );

			  //success
			  run.invoke( successBody, successCatch );
			  verify( successBody ).run();
			  verify( successCatch, never() ).run();

			  //failure
			  run.invoke( failBody, failCatch );
			  InOrder orderFailure = inOrder( failBody, failCatch );
			  orderFailure.verify( failBody ).run();
			  orderFailure.verify( failCatch ).run();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateTryCatchWithNestedBlock() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGenerateTryCatchWithNestedBlock()
		 {
			  // given
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					using ( CodeBlock run = simple.generateMethod( typeof( void ), "run", param( typeof( ThreadStart ), "body" ), param( typeof( ThreadStart ), "catcher" ), param( typeof( bool ), "test" ) ) )
					{

						 run.TryCatch(tryBlock =>
						 {
									 using ( CodeBlock ifBlock = tryBlock.ifStatement( run.Load( "test" ) ) )
									 {
										  ifBlock.Expression( invoke( run.Load( "body" ), _run ) );
									 }
						 }, catchBlock => catchBlock.expression( invoke( run.Load( "catcher" ), _run ) ), param( typeof( Exception ), "E" ));
					}
					handle = simple.Handle();
			  }

			  // when
			  ThreadStart runnable = mock( typeof( ThreadStart ) );
			  MethodHandle run = InstanceMethod( handle.NewInstance(), "run", typeof(ThreadStart), typeof(ThreadStart), typeof(bool) );

			  // then
			  run.invoke( runnable, mock( typeof( ThreadStart ) ), false );
			  verify( runnable, never() ).run();
			  run.invoke( runnable, mock( typeof( ThreadStart ) ), true );
			  verify( runnable ).run();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateTryAndMultipleCatch() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGenerateTryAndMultipleCatch()
		 {
			  // given
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					using ( CodeBlock run = simple.generateMethod( typeof( void ), "run", param( typeof( ThreadStart ), "body" ), param( typeof( ThreadStart ), "catcher1" ), param( typeof( ThreadStart ), "catcher2" ) ) )
					{

						 run.TryCatch( tryBlock => tryBlock.tryCatch( innerTry => innerTry.expression( invoke( run.Load( "body" ), _run ) ), catchBlock1 => catchBlock1.expression( invoke( run.Load( "catcher1" ), _run ) ), param( typeof( MyFirstException ), "E1" ) ), catchBlock2 => catchBlock2.expression( invoke( run.Load( "catcher2" ), _run ) ), param( typeof( MySecondException ), "E2" ) );

					}
					handle = simple.Handle();
			  }

			  // when
			  ThreadStart body1 = mock( typeof( ThreadStart ) );
			  ThreadStart body2 = mock( typeof( ThreadStart ) );
			  ThreadStart catcher11 = mock( typeof( ThreadStart ) );
			  ThreadStart catcher12 = mock( typeof( ThreadStart ) );
			  ThreadStart catcher21 = mock( typeof( ThreadStart ) );
			  ThreadStart catcher22 = mock( typeof( ThreadStart ) );
			  doThrow( typeof( MyFirstException ) ).when( body1 ).run();
			  doThrow( typeof( MySecondException ) ).when( body2 ).run();

			  MethodHandle run = InstanceMethod( handle.NewInstance(), "run", typeof(ThreadStart), typeof(ThreadStart), typeof(ThreadStart) );

			  run.invoke( body1, catcher11, catcher12 );
			  verify( body1 ).run();
			  verify( catcher11 ).run();
			  verify( catcher12, never() ).run();

			  run.invoke( body2, catcher21, catcher22 );
			  verify( body2 ).run();
			  verify( catcher22 ).run();
			  verify( catcher21, never() ).run();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowException() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowException()
		 {
			  // given
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					using ( CodeBlock thrower = simple.GenerateMethod( typeof( void ), "thrower" ) )
					{
						 thrower.ThrowException( invoke( newInstance( typeof( Exception ) ), constructorReference( typeof( Exception ), typeof( string ) ), constant( "hello world" ) ) );
					}
					handle = simple.Handle();
			  }

			  // when
			  try
			  {
					InstanceMethod( handle.NewInstance(), "thrower" ).invoke();
					fail( "expected exception" );
			  }
			  // then
			  catch ( Exception exception )
			  {
					assertEquals( "hello world", exception.Message );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToCast() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToCast()
		 {
			  // given
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( typeof( NamedBase ), "SimpleClass" ) )
			  {
					simple.Field( typeof( string ), "foo" );
					simple.Generate( MethodTemplate.Constructor( param( typeof( string ), "name" ), param( typeof( object ), "foo" ) ).invokeSuper( new ExpressionTemplate[]{ load( "name", typeReference( typeof( string ) ) ) }, new TypeReference[]{ typeReference( typeof( string ) ) } ).put( self( simple.Handle() ), typeof(string), "foo", cast(typeof(string), load("foo", typeReference(typeof(object)))) ).build() );
					handle = simple.Handle();
			  }

			  // when
			  object instance = Constructor( handle.LoadClass(), typeof(string), typeof(object) ).invoke("Pontus", "Tobias");

			  // then
			  assertEquals( "SimpleClass", instance.GetType().Name );
			  assertThat( instance, instanceOf( typeof( NamedBase ) ) );
			  assertEquals( "Pontus", ( ( NamedBase ) instance ).Name );
			  assertEquals( "Tobias", GetField( instance, "foo" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToBox() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToBox()
		 {
			  assertThat( BoxTest( typeof( bool ), true ), equalTo( true ) );
			  assertThat( BoxTest( typeof( bool ), false ), equalTo( false ) );
			  assertThat( BoxTest( typeof( sbyte ), ( sbyte ) 12 ), equalTo( ( sbyte ) 12 ) );
			  assertThat( BoxTest( typeof( short ), ( short ) 12 ), equalTo( ( short ) 12 ) );
			  assertThat( BoxTest( typeof( int ), 12 ), equalTo( 12 ) );
			  assertThat( BoxTest( typeof( long ), 12L ), equalTo( 12L ) );
			  assertThat( BoxTest( typeof( float ), 12F ), equalTo( 12F ) );
			  assertThat( BoxTest( typeof( double ), 12D ), equalTo( 12D ) );
			  assertThat( BoxTest( typeof( char ), 'a' ), equalTo( 'a' ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToUnbox() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToUnbox()
		 {
			  assertThat( UnboxTest( typeof( Boolean ), typeof( bool ), true ), equalTo( true ) );
			  assertThat( UnboxTest( typeof( Boolean ), typeof( bool ), false ), equalTo( false ) );
			  assertThat( UnboxTest( typeof( Byte ), typeof( sbyte ), ( sbyte ) 12 ), equalTo( ( sbyte ) 12 ) );
			  assertThat( UnboxTest( typeof( Short ), typeof( short ), ( short ) 12 ), equalTo( ( short ) 12 ) );
			  assertThat( UnboxTest( typeof( Integer ), typeof( int ), 12 ), equalTo( 12 ) );
			  assertThat( UnboxTest( typeof( Long ), typeof( long ), 12L ), equalTo( 12L ) );
			  assertThat( UnboxTest( typeof( Float ), typeof( float ), 12F ), equalTo( 12F ) );
			  assertThat( UnboxTest( typeof( Double ), typeof( double ), 12D ), equalTo( 12D ) );
			  assertThat( UnboxTest( typeof( Character ), typeof( char ), 'a' ), equalTo( 'a' ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleInfinityAndNan() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleInfinityAndNan()
		 {
			  assertTrue( double.IsInfinity( GenerateDoubleMethod( double.PositiveInfinity ).get() ) );
			  assertTrue( double.IsInfinity( GenerateDoubleMethod( double.NegativeInfinity ).get() ) );
			  assertTrue( double.IsNaN( GenerateDoubleMethod( Double.NaN ).get() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateInstanceOf() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGenerateInstanceOf()
		 {
			  // given
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					using ( CodeBlock conditional = simple.generateMethod( typeof( bool ), "isString", param( typeof( object ), "test" ) ) )
					{
						conditional.Returns( Expression.InstanceOf( typeReference( typeof( string ) ), conditional.Load( "test" ) ) );
					}

					handle = simple.Handle();
			  }

			  // when
			  MethodHandle isString = InstanceMethod( handle.NewInstance(), "isString", typeof(object) );

			  // then
			  assertTrue( ( bool? ) isString.invoke( "this is surely a string" ) );
			  assertFalse( ( bool? ) isString.invoke( "this is surely a string".Length ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private System.Func<double> generateDoubleMethod(double toBeReturned) throws Throwable
		 private System.Func<double> GenerateDoubleMethod( double toBeReturned )
		 {
			  CreateGenerator();
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					simple.Generate( MethodTemplate.Method( typeof( double ), "value" ).returns( constant( toBeReturned ) ).build() );
					handle = simple.Handle();
			  }

			  // when
			  object instance = Constructor( handle.LoadClass() ).invoke();

			  MethodHandle method = InstanceMethod( instance, "value" );
			  return () =>
			  {
				try
				{
					 return ( double? ) method.invoke();
				}
				catch ( Exception throwable )
				{
					 throw new AssertionError( throwable );
				}
			  };
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private <T> Object unboxTest(Class<T> boxedType, Class unboxedType, T value) throws Throwable
		 private object UnboxTest<T>( Type boxedType, Type unboxedType, T value )
		 {
				 boxedType = typeof( T );
			  CreateGenerator();
			  // given
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					using ( CodeBlock method = simple.generateMethod( unboxedType, "unbox", param( boxedType, "test" ) ) )
					{
						 method.Returns( Expression.Unbox( method.Load( "test" ) ) );
					}

					handle = simple.Handle();
			  }

			  // when
			  return InstanceMethod( handle.NewInstance(), "unbox", boxedType ).invoke(value);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private <T> Object boxTest(Class<T> unboxedType, T value) throws Throwable
		 private object BoxTest<T>( Type unboxedType, T value )
		 {
				 unboxedType = typeof( T );
			  CreateGenerator();
			  // given
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					using ( CodeBlock method = simple.generateMethod( typeof( object ), "box", param( unboxedType, "test" ) ) )
					{
						 method.Returns( Expression.Box( method.Load( "test" ) ) );
					}

					handle = simple.Handle();
			  }

			  // when
			  return InstanceMethod( handle.NewInstance(), "box", unboxedType ).invoke(value);
		 }

		 private MethodHandle Conditional( System.Func<CodeBlock, Expression> test, params Parameter[] @params )
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static MethodHandle method(Class target, String name, Class... parameters) throws Exception
		 private static MethodHandle Method( Type target, string name, params Type[] parameters )
		 {
			  return MethodHandles.lookup().unreflect(target.GetMethod(name, parameters));
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static MethodHandle instanceMethod(Object instance, String name, Class... parameters) throws Exception
		 private static MethodHandle InstanceMethod( object instance, string name, params Type[] parameters )
		 {
			  return Method( instance.GetType(), name, parameters ).bindTo(instance);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static Object getField(Object instance, String field) throws Exception
		 private static object GetField( object instance, string field )
		 {
			  return instance.GetType().GetField(field).get(instance);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static MethodHandle constructor(Class target, Class... parameters) throws Exception
		 private static MethodHandle Constructor( Type target, params Type[] parameters )
		 {
			  return MethodHandles.lookup().unreflectConstructor(target.GetConstructor(parameters));
		 }

		 public const string PACKAGE = "Neo4Net.codegen.test";
		 private CodeGenerator _generator;

		 internal virtual ClassGenerator GenerateClass( string name, Type firstInterface, params Type[] more )
		 {
			  return _generator.generateClass( PACKAGE, name, firstInterface, more );
		 }

		 private ClassGenerator GenerateClass( Type @base, string name, params Type[] interfaces )
		 {
			  return _generator.generateClass( @base, PACKAGE, name, interfaces );
		 }

		 private ClassGenerator GenerateClass( string name, params TypeReference[] interfaces )
		 {
			  return _generator.generateClass( PACKAGE, name, interfaces );
		 }

		 public class NamedBase
		 {
			  internal readonly string Name;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool DefaultConstructorCalledConflict;

			  public NamedBase()
			  {
					this.DefaultConstructorCalledConflict = true;
					this.Name = null;
			  }

			  public NamedBase( string name )
			  {
					this.Name = name;
			  }

			  public virtual bool DefaultConstructorCalled()
			  {
					return DefaultConstructorCalledConflict;
			  }
		 }

		 public class SomeBean
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal string FooConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal string BarConflict;

			  public virtual string Foo
			  {
				  set
				  {
						this.FooConflict = value;
				  }
			  }

			  public virtual string Bar
			  {
				  set
				  {
						this.BarConflict = value;
				  }
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private <T> void assertMethodReturningField(Class<T> clazz, T argument) throws Throwable
		 private void AssertMethodReturningField<T>( Type clazz, T argument )
		 {
				 clazz = typeof( T );
			  // given
			  CreateGenerator();
			  ClassHandle handle;
			  using ( ClassGenerator simple = GenerateClass( "SimpleClass" ) )
			  {
					FieldReference value = simple.Field( clazz, "value" );
					simple.Generate( MethodTemplate.Constructor( param( clazz, "value" ) ).invokeSuper().put(self(simple.Handle()), value.Type(), value.Name(), load("value", value.Type())).build() );
					simple.Generate( MethodTemplate.Method( clazz, "value" ).returns( ExpressionTemplate.get( self( simple.Handle() ), clazz, "value" ) ).build() );
					handle = simple.Handle();
			  }

			  // when
			  object instance = Constructor( handle.LoadClass(), clazz ).invoke(argument);

			  // then
			  assertEquals( argument, InstanceMethod( instance, "value" ).invoke() );
		 }

		 private static MethodReference CreateMethod( Type owner, Type returnType, string name )
		 {
			  return methodReference( typeof( ThreadStart ), typeof( void ), "run" );
		 }

		 public class MyFirstException : Exception
		 {
		 }

		 public class MySecondException : Exception
		 {
		 }

	}

}