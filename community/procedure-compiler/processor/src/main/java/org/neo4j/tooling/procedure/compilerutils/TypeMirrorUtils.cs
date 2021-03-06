﻿using System;
using System.Collections.Generic;

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
namespace Org.Neo4j.Tooling.procedure.compilerutils
{

	using Node = Org.Neo4j.Graphdb.Node;
	using Path = Org.Neo4j.Graphdb.Path;
	using Relationship = Org.Neo4j.Graphdb.Relationship;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;

	public class TypeMirrorUtils
	{

		 private Types _typeUtils;
		 private Elements _elementUtils;

		 public TypeMirrorUtils( Types typeUtils, Elements elementUtils )
		 {
			  this._typeUtils = typeUtils;
			  this._elementUtils = elementUtils;
		 }

		 public ICollection<TypeMirror> ProcedureAllowedTypes()
		 {
			  PrimitiveType @bool = Primitive( TypeKind.BOOLEAN );
			  PrimitiveType longType = Primitive( TypeKind.LONG );
			  PrimitiveType doubleType = Primitive( TypeKind.DOUBLE );
			  return asList( @bool, Boxed( @bool ), longType, Boxed( longType ), doubleType, Boxed( doubleType ), TypeMirror( typeof( string ) ), TypeMirror( typeof( Number ) ), TypeMirror( typeof( object ) ), TypeMirror( typeof( System.Collections.IDictionary ) ), TypeMirror( typeof( System.Collections.IList ) ), TypeMirror( typeof( Node ) ), TypeMirror( typeof( Relationship ) ), TypeMirror( typeof( Path ) ) );
		 }

		 public virtual PrimitiveType Primitive( TypeKind kind )
		 {
			  return _typeUtils.getPrimitiveType( kind );
		 }

		 public virtual TypeMirror TypeMirror( Type type )
		 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  return _elementUtils.getTypeElement( type.FullName ).asType();
		 }

		 private TypeMirror Boxed( PrimitiveType @bool )
		 {
			  return _typeUtils.boxedClass( @bool ).asType();
		 }
	}

}