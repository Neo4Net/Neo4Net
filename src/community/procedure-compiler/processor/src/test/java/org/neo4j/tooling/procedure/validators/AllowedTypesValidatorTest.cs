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
namespace Neo4Net.Tooling.procedure.validators
{
	using CompilationRule = com.google.testing.compile.CompilationRule;
	using TypeMirrorUtils = Neo4Net.Tooling.procedure.compilerutils.TypeMirrorUtils;
	using TypeMirrorTestUtils = Neo4Net.Tooling.procedure.testutils.TypeMirrorTestUtils;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Node = Neo4Net.GraphDb.Node;
	using Path = Neo4Net.GraphDb.Path;
	using Relationship = Neo4Net.GraphDb.Relationship;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.assertj.core.api.Assertions.assertThat;

	public class AllowedTypesValidatorTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public com.google.testing.compile.CompilationRule compilation = new com.google.testing.compile.CompilationRule();
		 public CompilationRule Compilation = new CompilationRule();
		 private TypeMirrorTestUtils _typeMirrorTestUtils;
		 private System.Predicate<TypeMirror> _validator;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void prepare()
		 public virtual void Prepare()
		 {
			  Types types = Compilation.Types;
			  Elements elements = Compilation.Elements;
			  TypeMirrorUtils typeMirrors = new TypeMirrorUtils( types, elements );

			  _typeMirrorTestUtils = new TypeMirrorTestUtils( Compilation );
			  _validator = new AllowedTypesValidator( typeMirrors, types );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unsupported_simple_type_is_invalid()
		 public virtual void UnsupportedSimpleTypeIsInvalid()
		 {
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( typeof( CharSequence ) ) ) ).False;
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( typeof( Thread ) ) ) ).False;
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( typeof( Character ) ) ) ).False;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void supported_simple_type_is_valid()
		 public virtual void SupportedSimpleTypeIsValid()
		 {
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( BOOLEAN ) ) ).True;
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( LONG ) ) ).True;
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( DOUBLE ) ) ).True;
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( typeof( Boolean ) ) ) ).True;
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( typeof( Long ) ) ) ).True;
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( typeof( Double ) ) ) ).True;
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( typeof( string ) ) ) ).True;
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( typeof( Number ) ) ) ).True;
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( typeof( object ) ) ) ).True;
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( typeof( Node ) ) ) ).True;
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( typeof( Relationship ) ) ) ).True;
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( typeof( Path ) ) ) ).True;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void supported_list_type_is_valid()
		 public virtual void SupportedListTypeIsValid()
		 {
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( typeof( System.Collections.IList ), typeof( Boolean ) ) ) ).True;
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( typeof( System.Collections.IList ), typeof( Long ) ) ) ).True;
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( typeof( System.Collections.IList ), typeof( Double ) ) ) ).True;
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( typeof( System.Collections.IList ), typeof( string ) ) ) ).True;
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( typeof( System.Collections.IList ), typeof( Number ) ) ) ).True;
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( typeof( System.Collections.IList ), typeof( object ) ) ) ).True;
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( typeof( System.Collections.IList ), typeof( Node ) ) ) ).True;
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( typeof( System.Collections.IList ), typeof( Relationship ) ) ) ).True;
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( typeof( System.Collections.IList ), typeof( Path ) ) ) ).True;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unsupported_list_type_is_invalid()
		 public virtual void UnsupportedListTypeIsInvalid()
		 {
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( typeof( System.Collections.IList ), typeof( CharSequence ) ) ) ).False;
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( typeof( System.Collections.IList ), typeof( Thread ) ) ) ).False;
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( typeof( System.Collections.IList ), typeof( Character ) ) ) ).False;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void supported_recursive_list_type_is_valid()
		 public virtual void SupportedRecursiveListTypeIsValid()
		 {
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( typeof( System.Collections.IList ), _typeMirrorTestUtils.typeOf( typeof( System.Collections.IList ), typeof( Boolean ) ) ) ) ).True;
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( typeof( System.Collections.IList ), _typeMirrorTestUtils.typeOf( typeof( System.Collections.IList ), _typeMirrorTestUtils.typeOf( typeof( System.Collections.IList ), typeof( object ) ) ) ) ) ).True;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unsupported_recursive_list_type_is_invalid()
		 public virtual void UnsupportedRecursiveListTypeIsInvalid()
		 {
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( typeof( System.Collections.IList ), _typeMirrorTestUtils.typeOf( typeof( System.Collections.IList ), typeof( CharSequence ) ) ) ) ).False;
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( typeof( System.Collections.IList ), _typeMirrorTestUtils.typeOf( typeof( System.Collections.IList ), _typeMirrorTestUtils.typeOf( typeof( System.Collections.IList ), typeof( Thread ) ) ) ) ) ).False;
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( typeof( System.Collections.IList ), _typeMirrorTestUtils.typeOf( typeof( System.Collections.IList ), _typeMirrorTestUtils.typeOf( typeof( System.Collections.IList ), typeof( Character ) ) ) ) ) ).False;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void supported_map_type_is_valid()
		 public virtual void SupportedMapTypeIsValid()
		 {
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( typeof( System.Collections.IDictionary ), typeof( string ), typeof( Boolean ) ) ) ).True;
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( typeof( System.Collections.IDictionary ), typeof( string ), typeof( Long ) ) ) ).True;
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( typeof( System.Collections.IDictionary ), typeof( string ), typeof( Double ) ) ) ).True;
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( typeof( System.Collections.IDictionary ), typeof( string ), typeof( string ) ) ) ).True;
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( typeof( System.Collections.IDictionary ), typeof( string ), typeof( Number ) ) ) ).True;
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( typeof( System.Collections.IDictionary ), typeof( string ), typeof( object ) ) ) ).True;
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( typeof( System.Collections.IDictionary ), typeof( string ), typeof( Node ) ) ) ).True;
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( typeof( System.Collections.IDictionary ), typeof( string ), typeof( Relationship ) ) ) ).True;
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( typeof( System.Collections.IDictionary ), typeof( string ), typeof( Path ) ) ) ).True;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unsupported_map_type_is_invalid()
		 public virtual void UnsupportedMapTypeIsInvalid()
		 {
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( typeof( System.Collections.IDictionary ), typeof( object ), typeof( Boolean ) ) ) ).False;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void supported_recursive_map_type_is_valid()
		 public virtual void SupportedRecursiveMapTypeIsValid()
		 {
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( typeof( System.Collections.IDictionary ), _typeMirrorTestUtils.typeOf( typeof( string ) ), _typeMirrorTestUtils.typeOf( typeof( System.Collections.IDictionary ), typeof( string ), typeof( Boolean ) ) ) ) ).True;
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( typeof( System.Collections.IDictionary ), _typeMirrorTestUtils.typeOf( typeof( string ) ), _typeMirrorTestUtils.typeOf( typeof( System.Collections.IDictionary ), _typeMirrorTestUtils.typeOf( typeof( string ) ), _typeMirrorTestUtils.typeOf( typeof( System.Collections.IDictionary ), typeof( string ), typeof( Boolean ) ) ) ) ) ).True;
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( typeof( System.Collections.IList ), _typeMirrorTestUtils.typeOf( typeof( System.Collections.IList ), _typeMirrorTestUtils.typeOf( typeof( System.Collections.IDictionary ), typeof( string ), typeof( Boolean ) ) ) ) ) ).True;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unsupported_recursive_map_type_is_invalid()
		 public virtual void UnsupportedRecursiveMapTypeIsInvalid()
		 {
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( typeof( System.Collections.IDictionary ), _typeMirrorTestUtils.typeOf( typeof( string ) ), _typeMirrorTestUtils.typeOf( typeof( System.Collections.IDictionary ), typeof( string ), typeof( Thread ) ) ) ) ).False;
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( typeof( System.Collections.IDictionary ), _typeMirrorTestUtils.typeOf( typeof( string ) ), _typeMirrorTestUtils.typeOf( typeof( System.Collections.IDictionary ), _typeMirrorTestUtils.typeOf( typeof( string ) ), _typeMirrorTestUtils.typeOf( typeof( System.Collections.IDictionary ), typeof( string ), typeof( CharSequence ) ) ) ) ) ).False;
			  assertThat( _validator.test( _typeMirrorTestUtils.typeOf( typeof( System.Collections.IList ), _typeMirrorTestUtils.typeOf( typeof( System.Collections.IList ), _typeMirrorTestUtils.typeOf( typeof( System.Collections.IDictionary ), typeof( string ), typeof( Character ) ) ) ) ) ).False;
		 }

	}

}