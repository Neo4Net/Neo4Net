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
namespace Neo4Net.Tooling.procedure.visitors
{
	using CompilationRule = com.google.testing.compile.CompilationRule;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using UserAggregationFunction = Neo4Net.Procedure.UserAggregationFunction;
	using CustomNameExtractor = Neo4Net.Tooling.procedure.compilerutils.CustomNameExtractor;
	using TypeMirrorUtils = Neo4Net.Tooling.procedure.compilerutils.TypeMirrorUtils;
	using CompilationMessage = Neo4Net.Tooling.procedure.messages.CompilationMessage;
	using ElementTestUtils = Neo4Net.Tooling.procedure.testutils.ElementTestUtils;
	using UserAggregationFunctionsExamples = Neo4Net.Tooling.procedure.visitors.examples.UserAggregationFunctionsExamples;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.assertj.core.api.Assertions.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.assertj.core.groups.Tuple.tuple;

	public class UserAggregationFunctionVisitorTest
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
//ORIGINAL LINE: final org.neo4j.tooling.procedure.compilerutils.TypeMirrorUtils typeMirrorUtils = new org.neo4j.tooling.procedure.compilerutils.TypeMirrorUtils(types, elements);
			  TypeMirrorUtils typeMirrorUtils = new TypeMirrorUtils( types, elements );
			  _visitor = new UserAggregationFunctionVisitor( new FunctionVisitor<UserAggregationFunction>( typeof( UserAggregationFunction ), types, elements, typeMirrorUtils, function => CustomNameExtractor.getName( function.name, function.value ), false ), types );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void aggregation_functions_with_specified_name_cannot_be_in_root_namespace()
		 public virtual void AggregationFunctionsWithSpecifiedNameCannotBeInRootNamespace()
		 {
			  Element function = _elementTestUtils.findMethodElement( typeof( UserAggregationFunctionsExamples ), "functionWithName" );

			  Stream<CompilationMessage> errors = _visitor.visit( function );

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertThat( errors ).hasSize( 1 ).extracting( CompilationMessage::getCategory, CompilationMessage::getElement, CompilationMessage::getContents ).contains( tuple( Diagnostic.Kind.ERROR, function, "Function <in_root_namespace> cannot be defined in the root namespace. Valid name example: com.acme.my_function" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void aggregation_functions_with_specified_value_cannot_be_in_root_namespace()
		 public virtual void AggregationFunctionsWithSpecifiedValueCannotBeInRootNamespace()
		 {
			  Element function = _elementTestUtils.findMethodElement( typeof( UserAggregationFunctionsExamples ), "functionWithValue" );

			  Stream<CompilationMessage> errors = _visitor.visit( function );

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertThat( errors ).hasSize( 1 ).extracting( CompilationMessage::getCategory, CompilationMessage::getElement, CompilationMessage::getContents ).contains( tuple( Diagnostic.Kind.ERROR, function, "Function <in_root_namespace_again> cannot be defined in the root namespace. Valid name example: com.acme.my_function" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void aggregation_functions_in_non_root_namespace_are_valid()
		 public virtual void AggregationFunctionsInNonRootNamespaceAreValid()
		 {
			  Element function = _elementTestUtils.findMethodElement( typeof( UserAggregationFunctionsExamples ), "ok" );

			  Stream<CompilationMessage> errors = _visitor.visit( function );

			  assertThat( errors ).Empty;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void aggregation_functions_with_unsupported_return_types_are_invalid()
		 public virtual void AggregationFunctionsWithUnsupportedReturnTypesAreInvalid()
		 {
			  Element function = _elementTestUtils.findMethodElement( typeof( UserAggregationFunctionsExamples ), "wrongReturnType" );

			  Stream<CompilationMessage> errors = _visitor.visit( function );

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertThat( errors ).hasSize( 1 ).extracting( CompilationMessage::getCategory, CompilationMessage::getElement, CompilationMessage::getContents ).contains( tuple( Diagnostic.Kind.ERROR, function, "Unsupported return type <void> of aggregation function." ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void aggregation_functions_with_parameters_are_invalid()
		 public virtual void AggregationFunctionsWithParametersAreInvalid()
		 {
			  Element function = _elementTestUtils.findMethodElement( typeof( UserAggregationFunctionsExamples ), "shouldNotHaveParameters" );

			  Stream<CompilationMessage> errors = _visitor.visit( function );

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertThat( errors ).hasSize( 1 ).extracting( CompilationMessage::getCategory, CompilationMessage::getElement, CompilationMessage::getContents ).contains( tuple( Diagnostic.Kind.ERROR, function, "@UserAggregationFunction usage error: method should be public, non-static and without parameters." ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void aggregation_update_functions_with_unsupported_parameter_types_are_invalid()
		 public virtual void AggregationUpdateFunctionsWithUnsupportedParameterTypesAreInvalid()
		 {
			  Element function = _elementTestUtils.findMethodElement( typeof( UserAggregationFunctionsExamples ), "updateWithWrongParameterType" );

			  Stream<CompilationMessage> errors = _visitor.visit( function );

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertThat( errors ).hasSize( 1 ).extracting( CompilationMessage::getCategory, CompilationMessage::getContents ).contains( tuple( Diagnostic.Kind.ERROR, "Unsupported parameter type <java.lang.Thread> of procedure|function " + "StringAggregatorWithWrongUpdateParameterType#doSomething" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void aggregation_functions_with_non_annotated_parameters_are_invalid()
		 public virtual void AggregationFunctionsWithNonAnnotatedParametersAreInvalid()
		 {
			  Element function = _elementTestUtils.findMethodElement( typeof( UserAggregationFunctionsExamples ), "missingParameterAnnotation" );

			  Stream<CompilationMessage> errors = _visitor.visit( function );

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertThat( errors ).hasSize( 1 ).extracting( CompilationMessage::getCategory, CompilationMessage::getContents ).contains( tuple( Diagnostic.Kind.ERROR, "@org.neo4j.procedure.Name usage error: missing on parameter <foo>" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void aggregation_result_functions_with_unsupported_return_types_are_invalid()
		 public virtual void AggregationResultFunctionsWithUnsupportedReturnTypesAreInvalid()
		 {
			  Element function = _elementTestUtils.findMethodElement( typeof( UserAggregationFunctionsExamples ), "resultWithWrongReturnType" );

			  Stream<CompilationMessage> errors = _visitor.visit( function );

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertThat( errors ).hasSize( 1 ).extracting( CompilationMessage::getCategory, CompilationMessage::getContents ).contains( tuple( Diagnostic.Kind.ERROR, "Unsupported return type <java.lang.Thread> of function defined in " + "<org.neo4j.tooling.procedure.visitors.examples.UserAggregationFunctionsExamples." + "StringAggregatorWithWrongResultReturnType#result>." ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void aggregation_result_functions_with_parameters_are_invalid()
		 public virtual void AggregationResultFunctionsWithParametersAreInvalid()
		 {
			  Element function = _elementTestUtils.findMethodElement( typeof( UserAggregationFunctionsExamples ), "resultWithParams" );

			  Stream<CompilationMessage> errors = _visitor.visit( function );

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertThat( errors ).hasSize( 1 ).extracting( CompilationMessage::getCategory, CompilationMessage::getContents ).contains( tuple( Diagnostic.Kind.ERROR, "@UserAggregationUpdate usage error: method should be public, non-static and without parameters." ) );
		 }
	}

}