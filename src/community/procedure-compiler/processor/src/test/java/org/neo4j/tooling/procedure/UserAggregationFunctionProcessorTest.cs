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
namespace Neo4Net.Tooling.procedure
{
	using CompilationRule = com.google.testing.compile.CompilationRule;
	using CompileTester = com.google.testing.compile.CompileTester;
	using Ignore = org.junit.Ignore;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using JavaFileObjectUtils = Neo4Net.Tooling.procedure.testutils.JavaFileObjectUtils;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.truth.Truth.assert_;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.testing.compile.JavaSourceSubjectFactory.javaSource;

	public class UserAggregationFunctionProcessorTest : ExtensionTestBase
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public com.google.testing.compile.CompilationRule compilation = new com.google.testing.compile.CompilationRule();
		 public CompilationRule Compilation = new CompilationRule();

		 private Processor _processor = new UserAggregationFunctionProcessor();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void fails_if_aggregation_function_directly_exposes_parameters()
		 public virtual void FailsIfAggregationFunctionDirectlyExposesParameters()
		 {
			  JavaFileObject function = JavaFileObjectUtils.INSTANCE.procedureSource( "invalid/aggregation/FunctionWithParameters.java" );

			  assert_().about(javaSource()).that(function).processedWith(Processor()).failsToCompile().withErrorCount(1).withErrorContaining("@UserAggregationFunction usage error: method should be public, non-static and without parameters.").@in(function).onLine(31);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void fails_if_aggregation_function_exposes_non_aggregation_return_type()
		 public virtual void FailsIfAggregationFunctionExposesNonAggregationReturnType()
		 {
			  JavaFileObject function = JavaFileObjectUtils.INSTANCE.procedureSource( "invalid/aggregation/FunctionWithWrongReturnType.java" );

			  assert_().about(javaSource()).that(function).processedWith(Processor()).failsToCompile().withErrorCount(1).withErrorContaining("Unsupported return type <void> of aggregation function.").@in(function).onLine(27);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Ignore("javac fails to publish the deferred diagnostic of the second error to com.google.testing.compile.Compiler") public void fails_if_aggregation_function_exposes_return_type_without_aggregation_methods()
		 public virtual void FailsIfAggregationFunctionExposesReturnTypeWithoutAggregationMethods()
		 {
			  JavaFileObject function = JavaFileObjectUtils.INSTANCE.procedureSource( "invalid/aggregation/FunctionWithoutAggregationMethods.java" );

			  CompileTester.UnsuccessfulCompilationClause unsuccessfulCompilationClause = assert_().about(javaSource()).that(function).processedWith(Processor()).failsToCompile().withErrorCount(2);

			  unsuccessfulCompilationClause.withErrorContaining( "@UserAggregationUpdate usage error: expected aggregation type " + "<Neo4Net.tooling.procedure.procedures.invalid.aggregation.FunctionWithoutAggregationMethods.MyAggregation> " + "to define exactly 1 method with this annotation. Found none." ).@in( function ).onLine( 31 );
			  unsuccessfulCompilationClause.withErrorContaining( "@UserAggregationResult usage error: expected aggregation type " + "<Neo4Net.tooling.procedure.procedures.invalid.aggregation.FunctionWithoutAggregationMethods.MyAggregation> " + "to define exactly 1 method with this annotation. Found none." ).@in( function ).onLine( 31 );
		 }

		 internal override Processor Processor()
		 {
			  return _processor;
		 }
	}

}