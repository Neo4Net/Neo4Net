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
namespace Neo4Net.Tooling.procedure.visitors
{

	using Name = Neo4Net.Procedure.Name;
	using CompilationMessage = Neo4Net.Tooling.procedure.messages.CompilationMessage;
	using ParameterMissingAnnotationError = Neo4Net.Tooling.procedure.messages.ParameterMissingAnnotationError;
	using ParameterTypeError = Neo4Net.Tooling.procedure.messages.ParameterTypeError;

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