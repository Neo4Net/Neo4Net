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
namespace Neo4Net.Tooling.procedure.visitors
{
	using CompilationRule = com.google.testing.compile.CompilationRule;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using JRE = org.junit.jupiter.api.condition.JRE;


	using UserFunction = Neo4Net.Procedure.UserFunction;
	using CustomNameExtractor = Neo4Net.Tooling.procedure.compilerutils.CustomNameExtractor;
	using TypeMirrorUtils = Neo4Net.Tooling.procedure.compilerutils.TypeMirrorUtils;
	using CompilationMessage = Neo4Net.Tooling.procedure.messages.CompilationMessage;
	using ElementTestUtils = Neo4Net.Tooling.procedure.testutils.ElementTestUtils;
	using UserFunctionsExamples = Neo4Net.Tooling.procedure.visitors.examples.UserFunctionsExamples;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.assertj.core.api.Assertions.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.assertj.core.groups.Tuple.tuple;

	public class UserFunctionVisitorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public com.google.testing.compile.CompilationRule compilationRule = new com.google.testing.compile.CompilationRule();
		 public CompilationRule CompilationRule = new CompilationRule();
		 private ElementTestUtils _elementTestUtils;
		 private ElementVisitor<Stream<CompilationMessage>, Void> _visitor;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void prepare()
		 public virtual void Prepare()
		 {
			  Types types = CompilationRule.Types;
			  Elements elements = CompilationRule.Elements;

			  _elementTestUtils = new ElementTestUtils( CompilationRule );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.tooling.procedure.compilerutils.TypeMirrorUtils typeMirrorUtils = new Neo4Net.tooling.procedure.compilerutils.TypeMirrorUtils(types, elements);
			  TypeMirrorUtils typeMirrorUtils = new TypeMirrorUtils( types, elements );
			  _visitor = new UserFunctionVisitor( new FunctionVisitor<UserFunction>( typeof( UserFunction ), types, elements, typeMirrorUtils, function => CustomNameExtractor.getName( function.name, function.value ), false ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void functions_with_specified_name_cannot_be_in_root_namespace()
		 public virtual void FunctionsWithSpecifiedNameCannotBeInRootNamespace()
		 {
			  Element function = _elementTestUtils.findMethodElement( typeof( UserFunctionsExamples ), "functionWithName" );

			  Stream<CompilationMessage> errors = _visitor.visit( function );

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertThat( errors ).hasSize( 1 ).extracting( CompilationMessage::getCategory, CompilationMessage::getElement, CompilationMessage::getContents ).contains( tuple( Diagnostic.Kind.ERROR, function, "Function <in_root_namespace> cannot be defined in the root namespace. Valid name example: com.acme.my_function" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void functions_with_specified_value_cannot_be_in_root_namespace()
		 public virtual void FunctionsWithSpecifiedValueCannotBeInRootNamespace()
		 {
			  Element function = _elementTestUtils.findMethodElement( typeof( UserFunctionsExamples ), "functionWithValue" );

			  Stream<CompilationMessage> errors = _visitor.visit( function );

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertThat( errors ).hasSize( 1 ).extracting( CompilationMessage::getCategory, CompilationMessage::getElement, CompilationMessage::getContents ).contains( tuple( Diagnostic.Kind.ERROR, function, "Function <in_root_namespace_again> cannot be defined in the root namespace. Valid name example: com.acme.my_function" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void functions_in_non_root_namespace_are_valid()
		 public virtual void FunctionsInNonRootNamespaceAreValid()
		 {
			  Element function = _elementTestUtils.findMethodElement( typeof( UserFunctionsExamples ), "ok" );

			  Stream<CompilationMessage> errors = _visitor.visit( function );

			  assertThat( errors ).Empty;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void functions_with_unsupported_return_types_are_invalid()
		 public virtual void FunctionsWithUnsupportedReturnTypesAreInvalid()
		 {
			  Element function = _elementTestUtils.findMethodElement( typeof( UserFunctionsExamples ), "wrongReturnType" );

			  Stream<CompilationMessage> errors = _visitor.visit( function );

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertThat( errors ).hasSize( 1 ).extracting( CompilationMessage::getCategory, CompilationMessage::getElement, CompilationMessage::getContents ).contains( tuple( Diagnostic.Kind.ERROR, function, "Unsupported return type <void> of function defined in " + "<Neo4Net.tooling.procedure.visitors.examples.UserFunctionsExamples#wrongReturnType>." ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void functions_with_unsupported_parameter_types_are_invalid()
		 public virtual void FunctionsWithUnsupportedParameterTypesAreInvalid()
		 {
			  Element function = _elementTestUtils.findMethodElement( typeof( UserFunctionsExamples ), "wrongParameterType" );

			  Stream<CompilationMessage> errors = _visitor.visit( function );

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertThat( errors ).hasSize( 1 ).extracting( CompilationMessage::getCategory, CompilationMessage::getContents ).contains( tuple( Diagnostic.Kind.ERROR, "Unsupported parameter type <java.lang.Thread> of procedure|function " + "UserFunctionsExamples#wrongParameterType" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void functions_with_non_annotated_parameters_are_invalid()
		 public virtual void FunctionsWithNonAnnotatedParametersAreInvalid()
		 {
			  Element function = _elementTestUtils.findMethodElement( typeof( UserFunctionsExamples ), "missingParameterAnnotation" );

			  Stream<CompilationMessage> errors = _visitor.visit( function );

			  string errorMessage = JRE.JAVA_11.CurrentVersion ? "@Neo4Net.procedure.Name usage error: missing on parameter <oops>" : "@Neo4Net.procedure.Name usage error: missing on parameter <arg1>";
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertThat( errors ).hasSize( 1 ).extracting( CompilationMessage::getCategory, CompilationMessage::getContents ).contains( tuple( Diagnostic.Kind.ERROR, errorMessage ) );
		 }
	}

}