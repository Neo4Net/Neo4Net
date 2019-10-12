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
namespace Org.Neo4j.Tooling.procedure.visitors
{
	using CompilationRule = com.google.testing.compile.CompilationRule;
	using CompilationMessage = Org.Neo4j.Tooling.procedure.messages.CompilationMessage;
	using ElementTestUtils = Org.Neo4j.Tooling.procedure.testutils.ElementTestUtils;
	using PerformsWriteProcedures = Org.Neo4j.Tooling.procedure.visitors.examples.PerformsWriteProcedures;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.assertj.core.api.Assertions.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.assertj.core.groups.Tuple.tuple;

	public class PerformsWriteMethodVisitorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public com.google.testing.compile.CompilationRule compilationRule = new com.google.testing.compile.CompilationRule();
		 public CompilationRule CompilationRule = new CompilationRule();

		 private ElementVisitor<Stream<CompilationMessage>, Void> _visitor = new PerformsWriteMethodVisitor();
		 private ElementTestUtils _elementTestUtils;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void prepare()
		 public virtual void Prepare()
		 {
			  _elementTestUtils = new ElementTestUtils( CompilationRule );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void rejects_non_procedure_methods()
		 public virtual void RejectsNonProcedureMethods()
		 {
			  Element element = _elementTestUtils.findMethodElement( typeof( PerformsWriteProcedures ), "missingProcedureAnnotation" );

			  Stream<CompilationMessage> errors = _visitor.visit( element );

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertThat( errors ).hasSize( 1 ).extracting( CompilationMessage::getCategory, CompilationMessage::getElement, CompilationMessage::getContents ).contains( tuple( Diagnostic.Kind.ERROR, element, "@PerformsWrites usage error: missing @Procedure annotation on method" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void rejects_conflicted_mode_usage()
		 public virtual void RejectsConflictedModeUsage()
		 {
			  Element element = _elementTestUtils.findMethodElement( typeof( PerformsWriteProcedures ), "conflictingMode" );

			  Stream<CompilationMessage> errors = _visitor.visit( element );

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertThat( errors ).hasSize( 1 ).extracting( CompilationMessage::getCategory, CompilationMessage::getElement, CompilationMessage::getContents ).contains( tuple( Diagnostic.Kind.ERROR, element, "@PerformsWrites usage error: cannot use mode other than Mode.DEFAULT" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void validates_regular_procedure()
		 public virtual void ValidatesRegularProcedure()
		 {
			  Element element = _elementTestUtils.findMethodElement( typeof( PerformsWriteProcedures ), "ok" );

			  Stream<CompilationMessage> errors = _visitor.visit( element );

			  assertThat( errors ).Empty;
		 }
	}

}