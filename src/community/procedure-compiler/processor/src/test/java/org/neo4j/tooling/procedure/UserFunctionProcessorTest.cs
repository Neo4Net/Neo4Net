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
	using UnsuccessfulCompilationClause = com.google.testing.compile.CompileTester.UnsuccessfulCompilationClause;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using JavaFileObjectUtils = Neo4Net.Tooling.procedure.testutils.JavaFileObjectUtils;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.truth.Truth.assert_;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.testing.compile.JavaSourceSubjectFactory.javaSource;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.testing.compile.JavaSourcesSubjectFactory.javaSources;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;

	public class UserFunctionProcessorTest : ExtensionTestBase
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public com.google.testing.compile.CompilationRule compilation = new com.google.testing.compile.CompilationRule();
		 public CompilationRule Compilation = new CompilationRule();

		 private Processor _processor = new UserFunctionProcessor();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void fails_if_parameters_are_not_properly_annotated()
		 public virtual void FailsIfParametersAreNotProperlyAnnotated()
		 {
			  JavaFileObject function = JavaFileObjectUtils.INSTANCE.procedureSource( "invalid/missing_name/MissingNameUserFunction.java" );

			  UnsuccessfulCompilationClause compilation = assert_().about(javaSource()).that(function).processedWith(Processor()).failsToCompile().withErrorCount(2);

			  compilation.withErrorContaining( "@org.Neo4Net.procedure.Name usage error: missing on parameter <parameter>" ).@in( function ).onLine( 28 );

			  compilation.withErrorContaining( "@org.Neo4Net.procedure.Name usage error: missing on parameter <otherParam>" ).@in( function ).onLine( 28 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void fails_if_return_type_is_incorrect()
		 public virtual void FailsIfReturnTypeIsIncorrect()
		 {
			  JavaFileObject function = JavaFileObjectUtils.INSTANCE.procedureSource( "invalid/bad_return_type/BadReturnTypeUserFunction.java" );

			  assert_().about(javaSource()).that(function).processedWith(Processor()).failsToCompile().withErrorCount(1).withErrorContaining("Unsupported return type <java.util.stream.Stream<java.lang.Long>> of function defined in " + "<org.Neo4Net.tooling.procedure.procedures.invalid.bad_return_type.BadReturnTypeUserFunction#wrongReturnTypeFunction>").@in(function).onLine(36);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void fails_if_function_primitive_input_type_is_not_supported()
		 public virtual void FailsIfFunctionPrimitiveInputTypeIsNotSupported()
		 {
			  JavaFileObject function = JavaFileObjectUtils.INSTANCE.procedureSource( "invalid/bad_proc_input_type/BadPrimitiveInputUserFunction.java" );

			  assert_().about(javaSource()).that(function).processedWith(Processor()).failsToCompile().withErrorCount(1).withErrorContaining("Unsupported parameter type <short> of procedure|function BadPrimitiveInputUserFunction#doSomething").@in(function).onLine(32);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void fails_if_function_generic_input_type_is_not_supported()
		 public virtual void FailsIfFunctionGenericInputTypeIsNotSupported()
		 {
			  JavaFileObject function = JavaFileObjectUtils.INSTANCE.procedureSource( "invalid/bad_proc_input_type/BadGenericInputUserFunction.java" );

			  UnsuccessfulCompilationClause compilation = assert_().about(javaSource()).that(function).processedWith(Processor()).failsToCompile().withErrorCount(3);

			  compilation.withErrorContaining( "Unsupported parameter type " + "<java.util.List<java.util.List<java.util.Map<java.lang.String,java.lang.Thread>>>>" + " of procedure|function BadGenericInputUserFunction#doSomething" ).@in( function ).onLine( 36 );

			  compilation.withErrorContaining( "Unsupported parameter type " + "<java.util.Map<java.lang.String,java.util.List<java.util.concurrent.ExecutorService>>>" + " of procedure|function BadGenericInputUserFunction#doSomething2" ).@in( function ).onLine( 42 );

			  compilation.withErrorContaining( "Unsupported parameter type <java.util.Map> of procedure|function BadGenericInputUserFunction#doSomething3" ).@in( function ).onLine( 48 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void fails_if_duplicate_functions_are_declared()
		 public virtual void FailsIfDuplicateFunctionsAreDeclared()
		 {
			  JavaFileObject firstDuplicate = JavaFileObjectUtils.INSTANCE.procedureSource( "invalid/duplicated/UserFunction1.java" );
			  JavaFileObject secondDuplicate = JavaFileObjectUtils.INSTANCE.procedureSource( "invalid/duplicated/UserFunction2.java" );

			  assert_().about(javaSources()).that(asList(firstDuplicate, secondDuplicate)).processedWith(Processor()).failsToCompile().withErrorCount(2).withErrorContaining("Procedure|function name <org.Neo4Net.tooling.procedure.procedures.invalid.duplicated.foobar> is " + "already defined 2 times. It should be defined only once!");
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void succeeds_to_process_valid_stored_procedures()
		 public virtual void SucceedsToProcessValidStoredProcedures()
		 {
			  assert_().about(javaSource()).that(JavaFileObjectUtils.INSTANCE.procedureSource("valid/UserFunctions.java")).processedWith(Processor()).compilesWithoutError();

		 }

		 internal override Processor Processor()
		 {
			  return _processor;
		 }
	}

}