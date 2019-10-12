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
	using TypeMirrorUtils = Org.Neo4j.Tooling.procedure.compilerutils.TypeMirrorUtils;
	using TypeMirrorTestUtils = Org.Neo4j.Tooling.procedure.testutils.TypeMirrorTestUtils;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;


	public class RecordFieldTypeVisitorTest : TypeValidationTestSuite
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public com.google.testing.compile.CompilationRule compilationRule = new com.google.testing.compile.CompilationRule();
		 public CompilationRule CompilationRule = new CompilationRule();
		 private Types _types;
		 private TypeMirrorUtils _typeMirrorUtils;
		 private TypeMirrorTestUtils _typeMirrorTestUtils;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void prepare()
		 public virtual void Prepare()
		 {
			  Elements elements = CompilationRule.Elements;
			  _types = CompilationRule.Types;
			  _typeMirrorUtils = new TypeMirrorUtils( _types, elements );
			  _typeMirrorTestUtils = new TypeMirrorTestUtils( CompilationRule );
		 }

		 protected internal override TypeVisitor<bool, Void> Visitor()
		 {
			  return new RecordFieldTypeVisitor( _types, _typeMirrorUtils );
		 }

		 protected internal override TypeMirrorTestUtils TypeMirrorTestUtils()
		 {
			  return _typeMirrorTestUtils;
		 }
	}

}