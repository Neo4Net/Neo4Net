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
	using TypeMirrorUtils = Neo4Net.Tooling.procedure.compilerutils.TypeMirrorUtils;
	using CompilationMessage = Neo4Net.Tooling.procedure.messages.CompilationMessage;
	using TypeMirrorTestUtils = Neo4Net.Tooling.procedure.testutils.TypeMirrorTestUtils;
	using InvalidRecord = Neo4Net.Tooling.procedure.visitors.examples.InvalidRecord;
	using ValidRecord = Neo4Net.Tooling.procedure.visitors.examples.ValidRecord;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.assertj.core.api.Assertions.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.assertj.core.api.Assertions.tuple;

	public class RecordTypeVisitorTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public com.google.testing.compile.CompilationRule compilation = new com.google.testing.compile.CompilationRule();
		 public CompilationRule Compilation = new CompilationRule();
		 private TypeMirrorTestUtils _typeMirrorTestUtils;
		 private RecordTypeVisitor _visitor;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void prepare()
		 public virtual void Prepare()
		 {
			  Types types = Compilation.Types;
			  Elements elements = Compilation.Elements;
			  TypeMirrorUtils typeMirrors = new TypeMirrorUtils( types, elements );

			  _typeMirrorTestUtils = new TypeMirrorTestUtils( Compilation );
			  _visitor = new RecordTypeVisitor( types, typeMirrors );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void validates_supported_record()
		 public virtual void ValidatesSupportedRecord()
		 {
			  TypeMirror recordStreamType = _typeMirrorTestUtils.typeOf( typeof( Stream ), typeof( ValidRecord ) );

			  assertThat( _visitor.visit( recordStreamType ) ).Empty;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void does_not_validate_record_with_nonpublic_fields()
		 public virtual void DoesNotValidateRecordWithNonpublicFields()
		 {
			  TypeMirror recordStreamType = _typeMirrorTestUtils.typeOf( typeof( Stream ), typeof( InvalidRecord ) );

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertThat( _visitor.visit( recordStreamType ) ).hasSize( 1 ).extracting( CompilationMessage::getCategory, CompilationMessage::getContents ).containsExactly( tuple( Diagnostic.Kind.ERROR, "Record definition error: field InvalidRecord#foo must" + " be public" ) );
		 }

	}

}