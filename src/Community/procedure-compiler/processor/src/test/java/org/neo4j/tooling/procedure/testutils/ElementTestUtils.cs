using System;

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
namespace Neo4Net.Tooling.procedure.testutils
{
	using CompilationRule = com.google.testing.compile.CompilationRule;


	public class ElementTestUtils
	{

		 private readonly Elements _elements;
		 private readonly Types _types;
		 private readonly TypeMirrorTestUtils _typeMirrorTestUtils;

		 public ElementTestUtils( CompilationRule rule ) : this( rule.Elements, rule.Types, new TypeMirrorTestUtils( rule ) )
		 {
		 }

		 private ElementTestUtils( Elements elements, Types types, TypeMirrorTestUtils typeMirrorTestUtils )
		 {
			  this._elements = elements;
			  this._types = types;
			  this._typeMirrorTestUtils = typeMirrorTestUtils;
		 }

		 public virtual Stream<VariableElement> GetFields( Type type )
		 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  TypeElement procedure = _elements.getTypeElement( type.FullName );

			  return fieldsIn( procedure.EnclosedElements ).stream();
		 }

		 public virtual Element FindMethodElement( Type type, string methodName )
		 {
			  TypeMirror mirror = _typeMirrorTestUtils.typeOf( type );
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  return ElementFilter.methodsIn( _types.asElement( mirror ).EnclosedElements ).Where( method => method.SimpleName.contentEquals( methodName ) ).First().orElseThrow(() => new AssertionError(string.Format("Could not find method {0} of class {1}", methodName, type.FullName)));
		 }
	}

}