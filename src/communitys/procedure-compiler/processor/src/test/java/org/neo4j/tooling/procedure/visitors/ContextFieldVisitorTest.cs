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


	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using SecurityContext = Neo4Net.@internal.Kernel.Api.security.SecurityContext;
	using Log = Neo4Net.Logging.Log;
	using ProcedureTransaction = Neo4Net.Procedure.ProcedureTransaction;
	using TerminationGuard = Neo4Net.Procedure.TerminationGuard;
	using CompilationMessage = Neo4Net.Tooling.procedure.messages.CompilationMessage;
	using ElementTestUtils = Neo4Net.Tooling.procedure.testutils.ElementTestUtils;
	using FinalContextMisuse = Neo4Net.Tooling.procedure.visitors.examples.FinalContextMisuse;
	using NonPublicContextMisuse = Neo4Net.Tooling.procedure.visitors.examples.NonPublicContextMisuse;
	using StaticContextMisuse = Neo4Net.Tooling.procedure.visitors.examples.StaticContextMisuse;
	using RestrictedContextTypes = Neo4Net.Tooling.procedure.visitors.examples.RestrictedContextTypes;
	using UnknownContextType = Neo4Net.Tooling.procedure.visitors.examples.UnknownContextType;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.assertj.core.api.Assertions.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.assertj.core.api.Assertions.tuple;

	public class ContextFieldVisitorTest
	{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		 private static readonly org.assertj.core.groups.Tuple _unknownContextErrorMsg = tuple( Diagnostic.Kind.ERROR, "@org.neo4j.procedure.Context usage error: found unknown type <java.lang.String> on field " + "UnknownContextType#unsupportedType, expected one of: <" + typeof( GraphDatabaseService ).FullName + ">, <" + typeof( Log ).FullName + ">, <" + typeof( TerminationGuard ).FullName + ">, <" + typeof( SecurityContext ).FullName + ">, <" + typeof( ProcedureTransaction ).FullName + ">" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public com.google.testing.compile.CompilationRule compilationRule = new com.google.testing.compile.CompilationRule();
		 public CompilationRule CompilationRule = new CompilationRule();
		 private ElementTestUtils _elementTestUtils;
		 private ElementVisitor<Stream<CompilationMessage>, Void> _contextFieldVisitor;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void prepare()
		 public virtual void Prepare()
		 {
			  _elementTestUtils = new ElementTestUtils( CompilationRule );
			  _contextFieldVisitor = new ContextFieldVisitor( CompilationRule.Types, CompilationRule.Elements, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void rejects_non_public_context_fields()
		 public virtual void RejectsNonPublicContextFields()
		 {
			  Stream<VariableElement> fields = _elementTestUtils.getFields( typeof( NonPublicContextMisuse ) );

			  Stream<CompilationMessage> result = fields.flatMap( _contextFieldVisitor.visit );

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertThat( result ).extracting( CompilationMessage::getCategory, CompilationMessage::getContents ).containsExactly( tuple( Diagnostic.Kind.ERROR, "@org.neo4j.procedure.Context usage error: field NonPublicContextMisuse#arithm should be public, " + "non-static and non-final" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void rejects_static_context_fields()
		 public virtual void RejectsStaticContextFields()
		 {
			  Stream<VariableElement> fields = _elementTestUtils.getFields( typeof( StaticContextMisuse ) );

			  Stream<CompilationMessage> result = fields.flatMap( _contextFieldVisitor.visit );

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertThat( result ).extracting( CompilationMessage::getCategory, CompilationMessage::getContents ).containsExactly( tuple( Diagnostic.Kind.ERROR, "@org.neo4j.procedure.Context usage error: field StaticContextMisuse#db should be public, non-static " + "and non-final" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void rejects_final_context_fields()
		 public virtual void RejectsFinalContextFields()
		 {
			  Stream<VariableElement> fields = _elementTestUtils.getFields( typeof( FinalContextMisuse ) );

			  Stream<CompilationMessage> result = fields.flatMap( _contextFieldVisitor.visit );

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertThat( result ).extracting( CompilationMessage::getCategory, CompilationMessage::getContents ).containsExactly( tuple( Diagnostic.Kind.ERROR, "@org.neo4j.procedure.Context usage error: field FinalContextMisuse#graphDatabaseService should be " + "public, non-static and non-final" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void warns_against_restricted_injected_types()
		 public virtual void WarnsAgainstRestrictedInjectedTypes()
		 {
			  Stream<VariableElement> fields = _elementTestUtils.getFields( typeof( RestrictedContextTypes ) );

			  Stream<CompilationMessage> result = fields.flatMap( _contextFieldVisitor.visit );

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertThat( result ).extracting( CompilationMessage::getCategory, CompilationMessage::getContents ).containsExactlyInAnyOrder( tuple( Diagnostic.Kind.WARNING, Warning( "org.neo4j.kernel.internal.GraphDatabaseAPI", "RestrictedContextTypes#graphDatabaseAPI" ) ), tuple( Diagnostic.Kind.WARNING, Warning( "org.neo4j.kernel.api.KernelTransaction", "RestrictedContextTypes#kernelTransaction" ) ), tuple( Diagnostic.Kind.WARNING, Warning( "org.neo4j.graphdb.DependencyResolver", "RestrictedContextTypes#dependencyResolver" ) ), tuple( Diagnostic.Kind.WARNING, Warning( "org.neo4j.kernel.api.security.UserManager", "RestrictedContextTypes#userManager" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void does_not_warn_against_restricted_injected_types_when_warnings_are_suppressed()
		 public virtual void DoesNotWarnAgainstRestrictedInjectedTypesWhenWarningsAreSuppressed()
		 {
			  ContextFieldVisitor contextFieldVisitor = new ContextFieldVisitor( CompilationRule.Types, CompilationRule.Elements, true );
			  Stream<VariableElement> fields = _elementTestUtils.getFields( typeof( RestrictedContextTypes ) );

			  Stream<CompilationMessage> result = fields.flatMap( contextFieldVisitor.visit );

			  assertThat( result ).Empty;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void rejects_unsupported_injected_type()
		 public virtual void RejectsUnsupportedInjectedType()
		 {
			  Stream<VariableElement> fields = _elementTestUtils.getFields( typeof( UnknownContextType ) );

			  Stream<CompilationMessage> result = fields.flatMap( _contextFieldVisitor.visit );

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertThat( result ).extracting( CompilationMessage::getCategory, CompilationMessage::getContents ).containsExactly( _unknownContextErrorMsg );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void rejects_unsupported_injected_type_when_warnings_are_suppressed()
		 public virtual void RejectsUnsupportedInjectedTypeWhenWarningsAreSuppressed()
		 {
			  ContextFieldVisitor contextFieldVisitor = new ContextFieldVisitor( CompilationRule.Types, CompilationRule.Elements, true );
			  Stream<VariableElement> fields = _elementTestUtils.getFields( typeof( UnknownContextType ) );

			  Stream<CompilationMessage> result = fields.flatMap( contextFieldVisitor.visit );

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertThat( result ).extracting( CompilationMessage::getCategory, CompilationMessage::getContents ).containsExactly( _unknownContextErrorMsg );
		 }

		 private string Warning( string fieldType, string fieldName )
		 {
			  return string.Format( "@org.neo4j.procedure.Context usage warning: found unsupported restricted type <{0}> on {1}.\n" + "The procedure will not load unless declared via the configuration option 'dbms.security.procedures.unrestricted'.\n" + "You can ignore this warning by passing the option -AIgnoreContextWarnings to the Java compiler", fieldType, fieldName );
		 }
	}

}