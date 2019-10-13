﻿using System.Collections.Generic;

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

	using TypeMirrorUtils = Neo4Net.Tooling.procedure.compilerutils.TypeMirrorUtils;
	using CompilationMessage = Neo4Net.Tooling.procedure.messages.CompilationMessage;
	using RecordTypeError = Neo4Net.Tooling.procedure.messages.RecordTypeError;


	internal class RecordTypeVisitor : SimpleTypeVisitor8<Stream<CompilationMessage>, Void>
	{

		 private readonly Types _typeUtils;
		 private readonly TypeVisitor<bool, Void> _fieldTypeVisitor;

		 internal RecordTypeVisitor( Types typeUtils, TypeMirrorUtils typeMirrors )
		 {
			  this._typeUtils = typeUtils;
			  _fieldTypeVisitor = new RecordFieldTypeVisitor( typeUtils, typeMirrors );
		 }

		 public override Stream<CompilationMessage> VisitDeclared( DeclaredType returnType, Void ignored )
		 {
			  return returnType.TypeArguments.stream().flatMap(this.validateRecord);
		 }

		 private Stream<CompilationMessage> ValidateRecord( TypeMirror recordType )
		 {
			  Element recordElement = _typeUtils.asElement( recordType );
			  return Stream.concat( ValidateFieldModifiers( recordElement ), ValidateFieldType( recordElement ) );
		 }

		 private Stream<CompilationMessage> ValidateFieldModifiers( Element recordElement )
		 {
			  return fieldsIn( recordElement.EnclosedElements ).Where(element =>
			  {
				ISet<Modifier> modifiers = element.Modifiers;
				return !modifiers.contains( PUBLIC ) && !modifiers.contains( STATIC );
			  }).Select( element => new RecordTypeError( element, "Record definition error: field %s#%s must be public", recordElement.SimpleName, element.SimpleName ) );
		 }

		 private Stream<CompilationMessage> ValidateFieldType( Element recordElement )
		 {
			  return fieldsIn( recordElement.EnclosedElements ).Where( element => !element.Modifiers.contains( STATIC ) ).Where( element => !_fieldTypeVisitor.visit( element.asType() ) ).Select(element => new RecordTypeError(element, "Record definition error: type of field %s#%s is not supported", recordElement.SimpleName, element.SimpleName));
		 }

	}

}