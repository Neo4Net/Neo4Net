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
namespace Org.Neo4j.Tooling.procedure.visitors
{

	using Name = Org.Neo4j.Procedure.Name;
	using CompilationMessage = Org.Neo4j.Tooling.procedure.messages.CompilationMessage;
	using ParameterMissingAnnotationError = Org.Neo4j.Tooling.procedure.messages.ParameterMissingAnnotationError;
	using ParameterTypeError = Org.Neo4j.Tooling.procedure.messages.ParameterTypeError;

	internal class ParameterVisitor : SimpleElementVisitor8<Stream<CompilationMessage>, Void>
	{

		 private readonly TypeVisitor<bool, Void> _parameterTypeVisitor;

		 internal ParameterVisitor( TypeVisitor<bool, Void> parameterTypeVisitor )
		 {
			  this._parameterTypeVisitor = parameterTypeVisitor;
		 }

		 public override Stream<CompilationMessage> VisitVariable( VariableElement parameter, Void ignored )
		 {
			  Name annotation = parameter.getAnnotation( typeof( Name ) );
			  if ( annotation == null )
			  {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					return Stream.of( new ParameterMissingAnnotationError( parameter, AnnotationMirror( parameter.AnnotationMirrors ), "@%s usage error: missing on parameter <%s>", typeof( Name ).FullName, NameOf( parameter ) ) );
			  }

			  if ( !_parameterTypeVisitor.visit( parameter.asType() ) )
			  {
					Element method = parameter.EnclosingElement;
					return Stream.of( new ParameterTypeError( parameter, "Unsupported parameter type <%s> of " + "procedure|function" + " %s#%s", parameter.asType().ToString(), method.EnclosingElement.SimpleName, method.SimpleName ) );
			  }
			  return Stream.empty();
		 }

		 private AnnotationMirror AnnotationMirror<T1>( IList<T1> mirrors ) where T1 : javax.lang.model.element.AnnotationMirror
		 {
			  AnnotationTypeVisitor nameVisitor = new AnnotationTypeVisitor( typeof( Name ) );
			  return mirrors.Where( mirror => nameVisitor.visit( mirror.AnnotationType.asElement() ) ).First().orElse(null);
		 }

		 private string NameOf( VariableElement parameter )
		 {
			  return parameter.SimpleName.ToString();
		 }
	}

}