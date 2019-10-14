using System.Collections;
using System.Collections.Generic;
using System.Threading;

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
	using TypeMirrorTestUtils = Neo4Net.Tooling.procedure.testutils.TypeMirrorTestUtils;
	using Test = org.junit.Test;


	using Node = Neo4Net.Graphdb.Node;
	using Path = Neo4Net.Graphdb.Path;
	using Relationship = Neo4Net.Graphdb.Relationship;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.assertj.core.api.Assertions.assertThat;

	internal abstract class TypeValidationTestSuite
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void validates_supported_simple_types()
		 public virtual void ValidatesSupportedSimpleTypes()
		 {
			  assertThat( Visitor().visit(TypeMirrorTestUtils().typeOf(typeof(string))) ).True;
			  assertThat( Visitor().visit(TypeMirrorTestUtils().typeOf(typeof(Number))) ).True;
			  assertThat( Visitor().visit(TypeMirrorTestUtils().typeOf(typeof(Long))) ).True;
			  assertThat( Visitor().visit(TypeMirrorTestUtils().typeOf(TypeKind.LONG)) ).True;
			  assertThat( Visitor().visit(TypeMirrorTestUtils().typeOf(typeof(Double))) ).True;
			  assertThat( Visitor().visit(TypeMirrorTestUtils().typeOf(TypeKind.DOUBLE)) ).True;
			  assertThat( Visitor().visit(TypeMirrorTestUtils().typeOf(typeof(Boolean))) ).True;
			  assertThat( Visitor().visit(TypeMirrorTestUtils().typeOf(TypeKind.BOOLEAN)) ).True;
			  assertThat( Visitor().visit(TypeMirrorTestUtils().typeOf(typeof(Path))) ).True;
			  assertThat( Visitor().visit(TypeMirrorTestUtils().typeOf(typeof(Node))) ).True;
			  assertThat( Visitor().visit(TypeMirrorTestUtils().typeOf(typeof(Relationship))) ).True;
			  assertThat( Visitor().visit(TypeMirrorTestUtils().typeOf(typeof(object))) ).True;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void validates_supported_generic_types()
		 public virtual void ValidatesSupportedGenericTypes()
		 {
			  assertThat( Visitor().visit(TypeMirrorTestUtils().typeOf(typeof(System.Collections.IDictionary), typeof(string), typeof(object))) ).True;
			  assertThat( Visitor().visit(TypeMirrorTestUtils().typeOf(typeof(Hashtable), typeof(string), typeof(object))) ).True;
			  assertThat( Visitor().visit(TypeMirrorTestUtils().typeOf(typeof(LinkedHashMap), typeof(string), typeof(object))) ).True;
			  assertThat( Visitor().visit(TypeMirrorTestUtils().typeOf(typeof(System.Collections.IList), typeof(string))) ).True;
			  assertThat( Visitor().visit(TypeMirrorTestUtils().typeOf(typeof(LinkedList), typeof(Number))) ).True;
			  assertThat( Visitor().visit(TypeMirrorTestUtils().typeOf(typeof(List<object>), typeof(Long))) ).True;
			  assertThat( Visitor().visit(TypeMirrorTestUtils().typeOf(typeof(System.Collections.IList), typeof(Double))) ).True;
			  assertThat( Visitor().visit(TypeMirrorTestUtils().typeOf(typeof(System.Collections.IList), typeof(Boolean))) ).True;
			  assertThat( Visitor().visit(TypeMirrorTestUtils().typeOf(typeof(System.Collections.IList), typeof(Path))) ).True;
			  assertThat( Visitor().visit(TypeMirrorTestUtils().typeOf(typeof(System.Collections.IList), typeof(Node))) ).True;
			  assertThat( Visitor().visit(TypeMirrorTestUtils().typeOf(typeof(System.Collections.IList), typeof(Relationship))) ).True;
			  assertThat( Visitor().visit(TypeMirrorTestUtils().typeOf(typeof(System.Collections.IList), typeof(object))) ).True;
			  assertThat( Visitor().visit(TypeMirrorTestUtils().typeOf(typeof(System.Collections.IList), TypeMirrorTestUtils().typeOf(typeof(System.Collections.IDictionary), typeof(string), typeof(object)))) ).True;
			  assertThat( Visitor().visit(TypeMirrorTestUtils().typeOf(typeof(System.Collections.IList), TypeMirrorTestUtils().typeOf(typeof(LinkedList), typeof(Long)))) ).True;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void rejects_unsupported_types()
		 public virtual void RejectsUnsupportedTypes()
		 {
			  assertThat( Visitor().visit(TypeMirrorTestUtils().typeOf(typeof(Thread))) ).False;
			  assertThat( Visitor().visit(TypeMirrorTestUtils().typeOf(typeof(System.Collections.IDictionary), typeof(string), typeof(Integer))) ).False;
			  assertThat( Visitor().visit(TypeMirrorTestUtils().typeOf(typeof(System.Collections.IDictionary), typeof(Integer), typeof(object))) ).False;
			  assertThat( Visitor().visit(TypeMirrorTestUtils().typeOf(typeof(System.Collections.IDictionary), typeof(Integer), typeof(Integer))) ).False;
			  assertThat( Visitor().visit(TypeMirrorTestUtils().typeOf(typeof(System.Collections.IList), typeof(decimal))) ).False;
			  assertThat( Visitor().visit(TypeMirrorTestUtils().typeOf(typeof(System.Collections.IList), TypeMirrorTestUtils().typeOf(typeof(System.Collections.IDictionary), typeof(string), typeof(Integer)))) ).False;
			  assertThat( Visitor().visit(TypeMirrorTestUtils().typeOf(typeof(System.Collections.IList), TypeMirrorTestUtils().typeOf(typeof(System.Collections.IList), typeof(CharSequence)))) ).False;
		 }

		 protected internal abstract TypeVisitor<bool, Void> Visitor();

		 protected internal abstract TypeMirrorTestUtils TypeMirrorTestUtils();

	}

}