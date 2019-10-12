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
	using TypeMirrorUtils = Neo4Net.Tooling.procedure.compilerutils.TypeMirrorUtils;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.stream;

	public class TypeMirrorTestUtils
	{

		 private readonly Types _types;
		 private readonly Elements _elements;
		 private readonly TypeMirrorUtils _typeMirrors;

		 public TypeMirrorTestUtils( CompilationRule rule ) : this( rule.Types, rule.Elements, new TypeMirrorUtils( rule.Types, rule.Elements ) )
		 {
		 }

		 private TypeMirrorTestUtils( Types types, Elements elements, TypeMirrorUtils typeMirrors )
		 {
			  this._types = types;
			  this._elements = elements;
			  this._typeMirrors = typeMirrors;
		 }

		 public virtual TypeMirror TypeOf( Type type, params Type[] parameterTypes )
		 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  return _types.getDeclaredType( _elements.getTypeElement( type.FullName ), TypesOf( parameterTypes ) );
		 }

		 public virtual TypeMirror TypeOf( Type type, params TypeMirror[] parameterTypes )
		 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  return _types.getDeclaredType( _elements.getTypeElement( type.FullName ), parameterTypes );
		 }

		 public virtual PrimitiveType TypeOf( TypeKind kind )
		 {
			  return _typeMirrors.primitive( kind );
		 }

		 public virtual TypeMirror TypeOf( Type type )
		 {
			  return _typeMirrors.typeMirror( type );
		 }

		 private TypeMirror[] TypesOf( params Type[] parameterTypes )
		 {
			  Stream<TypeMirror> mirrorStream = stream( parameterTypes ).map( this.typeOf );
			  return mirrorStream.collect( toList() ).toArray(new TypeMirror[parameterTypes.Length]);
		 }
	}

}