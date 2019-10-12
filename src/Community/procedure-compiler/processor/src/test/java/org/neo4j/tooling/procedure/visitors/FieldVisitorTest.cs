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


	using CompilationMessage = Neo4Net.Tooling.procedure.messages.CompilationMessage;
	using ElementTestUtils = Neo4Net.Tooling.procedure.testutils.ElementTestUtils;
	using GoodContextUse = Neo4Net.Tooling.procedure.visitors.examples.GoodContextUse;
	using StaticNonContextMisuse = Neo4Net.Tooling.procedure.visitors.examples.StaticNonContextMisuse;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.assertj.core.api.Assertions.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.assertj.core.api.Assertions.tuple;

	public class FieldVisitorTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public com.google.testing.compile.CompilationRule compilationRule = new com.google.testing.compile.CompilationRule();
		 public CompilationRule CompilationRule = new CompilationRule();
		 private ElementVisitor<Stream<CompilationMessage>, Void> _fieldVisitor;
		 private ElementTestUtils _elementTestUtils;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void prepare()
		 public virtual void Prepare()
		 {
			  _elementTestUtils = new ElementTestUtils( CompilationRule );
			  _fieldVisitor = new FieldVisitor( CompilationRule.Types, CompilationRule.Elements, true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void validates_visibility_of_fields()
		 public virtual void ValidatesVisibilityOfFields()
		 {
			  Stream<VariableElement> fields = _elementTestUtils.getFields( typeof( GoodContextUse ) );

			  Stream<CompilationMessage> result = fields.flatMap( _fieldVisitor.visit );

			  assertThat( result ).Empty;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void rejects_non_static_non_context_fields()
		 public virtual void RejectsNonStaticNonContextFields()
		 {
			  Stream<VariableElement> fields = _elementTestUtils.getFields( typeof( StaticNonContextMisuse ) );

			  Stream<CompilationMessage> result = fields.flatMap( _fieldVisitor.visit );

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertThat( result ).extracting( CompilationMessage::getCategory, CompilationMessage::getContents ).containsExactly( tuple( Diagnostic.Kind.ERROR, "Field StaticNonContextMisuse#value should be static" ) );
		 }

	}


}