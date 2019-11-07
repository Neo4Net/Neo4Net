using System;

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
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.ByteCodeUtils.desc;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.ByteCodeUtils.exceptions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.ByteCodeUtils.signature;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.ByteCodeUtils.typeName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.MethodDeclaration.method;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.Parameter.param;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.TypeReference.extending;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.TypeReference.typeParameter;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.TypeReference.typeReference;

	public class ByteCodeUtilsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTranslateTypeNames()
		 public virtual void ShouldTranslateTypeNames()
		 {
			  //primitive types
			  AssertTypeName( typeof( int ), "I" );
			  AssertTypeName( typeof( sbyte ), "B" );
			  AssertTypeName( typeof( short ), "S" );
			  AssertTypeName( typeof( char ), "C" );
			  AssertTypeName( typeof( float ), "F" );
			  AssertTypeName( typeof( double ), "D" );
			  AssertTypeName( typeof( bool ), "Z" );
			  AssertTypeName( typeof( void ), "V" );

			  //primitive array types
			  AssertTypeName( typeof( int[] ), "[I" );
			  AssertTypeName( typeof( sbyte[] ), "[B" );
			  AssertTypeName( typeof( short[] ), "[S" );
			  AssertTypeName( typeof( char[] ), "[C" );
			  AssertTypeName( typeof( float[] ), "[F" );
			  AssertTypeName( typeof( double[] ), "[D" );
			  AssertTypeName( typeof( bool[] ), "[Z" );

			  //reference type
			  AssertTypeName( typeof( string ), "Ljava/lang/String;" );

			  //reference array type
			  AssertTypeName( typeof( string[] ), "[Ljava/lang/String;" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDescribeMethodWithNoParameters()
		 public virtual void ShouldDescribeMethodWithNoParameters()
		 {
			  // GIVEN
			  TypeReference owner = typeReference( typeof( ByteCodeUtilsTest ) );
			  MethodDeclaration declaration = method( typeof( bool ), "foo" ).build( owner );

			  // WHEN
			  string description = desc( declaration );

			  // THEN
			  assertThat( description, equalTo( "()Z" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDescribeMethodWithParameters()
		 public virtual void ShouldDescribeMethodWithParameters()
		 {
			  // GIVEN
			  TypeReference owner = typeReference( typeof( ByteCodeUtilsTest ) );
			  MethodDeclaration declaration = method( typeof( System.Collections.IList ), "foo", param( typeof( string ), "string" ), param( typeof( char[] ), "chararray" ) ).build( owner );

			  // WHEN
			  string description = desc( declaration );

			  // THEN
			  assertThat( description, equalTo( "(Ljava/lang/String;[C)Ljava/util/List;" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void signatureShouldBeNullWhenNotGeneric()
		 public virtual void SignatureShouldBeNullWhenNotGeneric()
		 {
			  // GIVEN
			  TypeReference reference = typeReference( typeof( string ) );

			  // WHEN
			  string signature = signature( reference );

			  // THEN
			  assertNull( signature );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void signatureShouldBeCorrectWhenGeneric()
		 public virtual void SignatureShouldBeCorrectWhenGeneric()
		 {
			  // GIVEN
			  TypeReference reference = TypeReference.ParameterizedType( typeof( System.Collections.IList ), typeof( string ) );

			  // WHEN
			  string signature = signature( reference );

			  // THEN
			  assertThat( signature, equalTo( "Ljava/util/List<Ljava/lang/String;>;" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void methodSignatureShouldBeNullWhenNotGeneric()
		 public virtual void MethodSignatureShouldBeNullWhenNotGeneric()
		 {
			  // GIVEN
			  TypeReference owner = typeReference( typeof( ByteCodeUtilsTest ) );
			  MethodDeclaration declaration = method( typeof( string ), "foo", param( typeof( string ), "string" ), param( typeof( char[] ), "chararray" ) ).build( owner );

			  // WHEN
			  string signature = signature( declaration );

			  // THEN
			 assertNull( signature );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void methodSignatureShouldBeCorrectWhenGeneric()
		 public virtual void MethodSignatureShouldBeCorrectWhenGeneric()
		 {
			  // GIVEN
			  TypeReference owner = typeReference( typeof( ByteCodeUtilsTest ) );
			  MethodDeclaration declaration = method( TypeReference.ParameterizedType( typeof( System.Collections.IList ), typeof( string ) ), "foo", param( typeof( string ), "string" ) ).build( owner );

			  // WHEN
			  string signature = signature( declaration );

			  // THEN
			  assertThat( signature, equalTo( "(Ljava/lang/String;)Ljava/util/List<Ljava/lang/String;>;" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleGenericReturnType()
		 public virtual void ShouldHandleGenericReturnType()
		 {
			  // GIVEN
			  TypeReference owner = typeReference( typeof( ByteCodeUtilsTest ) );
			  MethodDeclaration declaration = MethodDeclaration.Method( typeParameter( "T" ), "fail" ).parameterizedWith( "T", extending( typeof( object ) ) ).build( owner );

			  // WHEN
			  string desc = desc( declaration );
			  string signature = signature( declaration );

			  // THEN
			  assertThat( desc, equalTo( "()Ljava/lang/Object;" ) );
			  assertThat( signature, equalTo( "<T:Ljava/lang/Object;>()TT;" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleGenericThrows()
		 public virtual void ShouldHandleGenericThrows()
		 {
			  // GIVEN
			  TypeReference owner = typeReference( typeof( ByteCodeUtilsTest ) );
			  MethodDeclaration declaration = MethodDeclaration.Method( typeof( void ), "fail", param( TypeReference.ParameterizedType( typeof( CodeGenerationTest.Thrower ), typeParameter( "E" ) ), "thrower" ) ).parameterizedWith( "E", extending( typeof( Exception ) ) ).throwsException( typeParameter( "E" ) ).build( owner );

			  // WHEN
			  string signature = signature( declaration );
			  string[] exceptions = exceptions( declaration );
			  // THEN
			  assertThat( signature, equalTo( "<E:Ljava/lang/Exception;>(Lorg/Neo4Net/codegen/CodeGenerationTest$Thrower<TE;>;)V^TE;" ) );
			  assertThat( exceptions, equalTo( new string[]{ "java/lang/Exception" } ) );
		 }

		 private void AssertTypeName( Type type, string expected )
		 {
			  // GIVEN
			  TypeReference reference = typeReference( type );

			  // WHEN
			  string byteCodeName = typeName( reference );

			  // THEN
			  assertThat( byteCodeName, equalTo( expected ) );
		 }
	}

}