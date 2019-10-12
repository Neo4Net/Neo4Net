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

	using TypeMirrorUtils = Org.Neo4j.Tooling.procedure.compilerutils.TypeMirrorUtils;
	using AllowedTypesValidator = Org.Neo4j.Tooling.procedure.validators.AllowedTypesValidator;

	internal class RecordFieldTypeVisitor : SimpleTypeVisitor8<bool, Void>
	{

		 private readonly System.Predicate<TypeMirror> _allowedTypesValidator;

		 internal RecordFieldTypeVisitor( Types typeUtils, TypeMirrorUtils typeMirrors )
		 {
			  _allowedTypesValidator = new AllowedTypesValidator( typeMirrors, typeUtils );
		 }

		 public override bool? VisitDeclared( DeclaredType declaredType, Void ignored )
		 {
			  return _allowedTypesValidator.test( declaredType ) && declaredType.TypeArguments.Select( this.visit ).Aggregate( ( a, b ) => a && b ).orElse( true );
		 }

		 public override bool? VisitPrimitive( PrimitiveType primitiveType, Void ignored )
		 {
			  return _allowedTypesValidator.test( primitiveType );
		 }
	}

}