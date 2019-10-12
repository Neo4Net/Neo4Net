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
namespace Neo4Net.Tooling.procedure.visitors
{

	using Context = Neo4Net.Procedure.Context;
	using CompilationMessage = Neo4Net.Tooling.procedure.messages.CompilationMessage;
	using FieldError = Neo4Net.Tooling.procedure.messages.FieldError;

	public class FieldVisitor : SimpleElementVisitor8<Stream<CompilationMessage>, Void>
	{

		 private readonly ElementVisitor<Stream<CompilationMessage>, Void> _contextFieldVisitor;

		 public FieldVisitor( Types types, Elements elements, bool ignoresWarnings )
		 {
			  _contextFieldVisitor = new ContextFieldVisitor( types, elements, ignoresWarnings );
		 }

		 private static Stream<CompilationMessage> ValidateNonContextField( VariableElement field )
		 {
			  ISet<Modifier> modifiers = field.Modifiers;
			  if ( !modifiers.Contains( Modifier.STATIC ) )
			  {
					return Stream.of( new FieldError( field, "Field %s#%s should be static", field.EnclosingElement.SimpleName, field.SimpleName ) );
			  }
			  return Stream.empty();
		 }

		 public override Stream<CompilationMessage> VisitVariable( VariableElement field, Void ignored )
		 {
			  if ( field.getAnnotation( typeof( Context ) ) != null )
			  {
					return _contextFieldVisitor.visitVariable( field, ignored );
			  }
			  return ValidateNonContextField( field );

		 }

	}

}