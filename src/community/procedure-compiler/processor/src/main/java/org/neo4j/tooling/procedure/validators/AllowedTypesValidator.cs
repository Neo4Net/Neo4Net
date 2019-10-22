using System;
using System.Collections.Generic;

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
namespace Neo4Net.Tooling.procedure.validators
{

	using TypeMirrorUtils = Neo4Net.Tooling.procedure.compilerutils.TypeMirrorUtils;

	/// <summary>
	/// This predicate makes sure that a given declared type (record field type,
	/// procedure parameter type...) is supported by Neo4Net stored procedures.
	/// </summary>
	public class AllowedTypesValidator : System.Predicate<TypeMirror>
	{

		 private readonly TypeMirrorUtils _typeMirrors;
		 private readonly ICollection<TypeMirror> _whitelistedTypes;
		 private readonly Types _typeUtils;

		 public AllowedTypesValidator( TypeMirrorUtils typeMirrors, Types typeUtils )
		 {

			  this._typeMirrors = typeMirrors;
			  this._whitelistedTypes = typeMirrors.ProcedureAllowedTypes();
			  this._typeUtils = typeUtils;
		 }

		 public override bool Test( TypeMirror typeMirror )
		 {
			  TypeMirror erasedActualType = _typeUtils.erasure( typeMirror );

			  return IsValidErasedType( erasedActualType ) && ( !IsSameErasedType( typeof( System.Collections.IList ), typeMirror ) || IsValidListType( typeMirror ) ) && ( !IsSameErasedType( typeof( System.Collections.IDictionary ), typeMirror ) || IsValidMapType( typeMirror ) );
		 }

		 private bool IsValidErasedType( TypeMirror actualType )
		 {
			  return _whitelistedTypes.Any(type =>
			  {
				TypeMirror erasedAllowedType = _typeUtils.erasure( type );

				TypeMirror map = _typeUtils.erasure( _typeMirrors.typeMirror( typeof( System.Collections.IDictionary ) ) );
				TypeMirror list = _typeUtils.erasure( _typeMirrors.typeMirror( typeof( System.Collections.IList ) ) );
				if ( _typeUtils.isSameType( erasedAllowedType, map ) || _typeUtils.isSameType( erasedAllowedType, list ) )
				{
					 return _typeUtils.isSubtype( actualType, erasedAllowedType );
				}

				return _typeUtils.isSameType( actualType, erasedAllowedType );
			  });
		 }

		 /// <summary>
		 /// Recursively visits List type arguments
		 /// </summary>
		 /// <param name="typeMirror"> the List type mirror </param>
		 /// <returns> true if the declaration is valid, false otherwise </returns>
		 private bool IsValidListType( TypeMirror typeMirror )
		 {
			  return new SimpleTypeVisitor8AnonymousInnerClass( this )
			  .visit( typeMirror );
		 }

		 private class SimpleTypeVisitor8AnonymousInnerClass : SimpleTypeVisitor8<bool, Void>
		 {
			 private readonly AllowedTypesValidator _outerInstance;

			 public SimpleTypeVisitor8AnonymousInnerClass( AllowedTypesValidator outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override bool? visitDeclared( DeclaredType list, Void aVoid )
			 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<? extends javax.lang.model.type.TypeMirror> typeArguments = list.getTypeArguments();
				  IList<TypeMirror> typeArguments = list.TypeArguments;
				  return typeArguments.Count == 1 && outerInstance.Test( typeArguments[0] );
			 }
		 }

		 /// <summary>
		 /// Recursively visits Map type arguments
		 /// Map key type argument must be a String as of Neo4Net stored procedure specification
		 /// Map value type argument is recursively visited
		 /// </summary>
		 /// <param name="typeMirror"> Map type mirror </param>
		 /// <returns> true if the declaration is valid, false otherwise </returns>
		 private bool IsValidMapType( TypeMirror typeMirror )
		 {
			  return new SimpleTypeVisitor8AnonymousInnerClass2( this )
			  .visit( typeMirror );
		 }

		 private class SimpleTypeVisitor8AnonymousInnerClass2 : SimpleTypeVisitor8<bool, Void>
		 {
			 private readonly AllowedTypesValidator _outerInstance;

			 public SimpleTypeVisitor8AnonymousInnerClass2( AllowedTypesValidator outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override bool? visitDeclared( DeclaredType map, Void ignored )
			 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<? extends javax.lang.model.type.TypeMirror> typeArguments = map.getTypeArguments();
				  IList<TypeMirror> typeArguments = map.TypeArguments;
				  if ( typeArguments.Count != 2 )
				  {
						return false;
				  }

				  TypeMirror key = typeArguments[0];
				  return _outerInstance.typeUtils.isSameType( key, _outerInstance.typeMirrors.typeMirror( typeof( string ) ) ) && _outerInstance.test( typeArguments[1] );
			 }
		 }

		 private bool IsSameErasedType( Type type, TypeMirror typeMirror )
		 {
			  return _typeUtils.isSameType( _typeUtils.erasure( _typeMirrors.typeMirror( type ) ), _typeUtils.erasure( typeMirror ) );
		 }

	}

}