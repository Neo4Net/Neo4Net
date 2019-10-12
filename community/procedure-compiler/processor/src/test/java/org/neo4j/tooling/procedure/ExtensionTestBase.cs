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
namespace Org.Neo4j.Tooling.procedure
{
	using CompileTester = com.google.testing.compile.CompileTester;
	using Test = org.junit.Test;


	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using SecurityContext = Org.Neo4j.@internal.Kernel.Api.security.SecurityContext;
	using Log = Org.Neo4j.Logging.Log;
	using ProcedureTransaction = Org.Neo4j.Procedure.ProcedureTransaction;
	using TerminationGuard = Org.Neo4j.Procedure.TerminationGuard;
	using JavaFileObjectUtils = Org.Neo4j.Tooling.procedure.testutils.JavaFileObjectUtils;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.truth.Truth.assert_;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.testing.compile.JavaSourceSubjectFactory.javaSource;

	public abstract class ExtensionTestBase
	{

		 internal abstract Processor Processor();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void fails_if_context_injected_fields_have_wrong_modifiers()
		 public virtual void FailsIfContextInjectedFieldsHaveWrongModifiers()
		 {
			  JavaFileObject sproc = JavaFileObjectUtils.INSTANCE.procedureSource( "invalid/bad_context_field/BadContextFields.java" );

			  CompileTester.UnsuccessfulCompilationClause unsuccessfulCompilationClause = assert_().about(javaSource()).that(sproc).processedWith(Processor()).failsToCompile().withErrorCount(4);

			  unsuccessfulCompilationClause.withErrorContaining( "@org.neo4j.procedure.Context usage error: field BadContextFields#shouldBeNonStatic should be public, " + "non-static and non-final" ).@in( sproc ).onLine( 35 );

			  unsuccessfulCompilationClause.withErrorContaining( "@org.neo4j.procedure.Context usage error: field BadContextFields#shouldBeNonFinal should be public, " + "non-static and non-final" ).@in( sproc ).onLine( 38 );

			  unsuccessfulCompilationClause.withErrorContaining( "@org.neo4j.procedure.Context usage error: field BadContextFields#shouldBePublic should be public, " + "non-static and non-final" ).@in( sproc ).onLine( 42 );

			  unsuccessfulCompilationClause.withErrorContaining( "Field BadContextFields#shouldBeStatic should be static" ).@in( sproc ).onLine( 43 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void emits_warnings_if_context_injected_field_types_are_restricted()
		 public virtual void EmitsWarningsIfContextInjectedFieldTypesAreRestricted()
		 {
			  JavaFileObject sproc = JavaFileObjectUtils.INSTANCE.procedureSource( "invalid/bad_context_field/BadContextRestrictedTypeField.java" );

			  assert_().about(javaSource()).that(sproc).processedWith(Processor()).compilesWithoutError().withWarningCount(2).withWarningContaining("@org.neo4j.procedure.Context usage warning: found unsupported restricted type <org.neo4j.kernel.internal" + ".GraphDatabaseAPI> on BadContextRestrictedTypeField#notOfficiallySupported.\n" + "  The procedure will not load unless declared via the configuration option 'dbms.security.procedures.unrestricted'.\n" + "  You can ignore this warning by passing the option -AIgnoreContextWarnings to the Java compiler").@in(sproc).onLine(35);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void does_not_emit_warnings_if_context_injected_field_types_are_restricted_when_context_warnings_disabled()
		 public virtual void DoesNotEmitWarningsIfContextInjectedFieldTypesAreRestrictedWhenContextWarningsDisabled()
		 {
			  JavaFileObject sproc = JavaFileObjectUtils.INSTANCE.procedureSource( "invalid/bad_context_field/BadContextRestrictedTypeField.java" );

			  assert_().about(javaSource()).that(sproc).withCompilerOptions("-AIgnoreContextWarnings").processedWith(Processor()).compilesWithoutError().withWarningCount(1);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void fails_if_context_injected_fields_have_unsupported_types()
		 public virtual void FailsIfContextInjectedFieldsHaveUnsupportedTypes()
		 {
			  JavaFileObject sproc = JavaFileObjectUtils.INSTANCE.procedureSource( "invalid/bad_context_field/BadContextUnsupportedTypeError.java" );

//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  assert_().about(javaSource()).that(sproc).processedWith(Processor()).failsToCompile().withErrorCount(1).withErrorContaining("@org.neo4j.procedure.Context usage error: found unknown type <java.lang.String> on field " + "BadContextUnsupportedTypeError#foo, expected one of: <" + typeof(GraphDatabaseService).FullName + ">, <" + typeof(Log).FullName + ">, <" + typeof(TerminationGuard).FullName + ">, <" + typeof(SecurityContext).FullName + ">, <" + typeof(ProcedureTransaction).FullName + ">").@in(sproc).onLine(33);
		 }
	}

}