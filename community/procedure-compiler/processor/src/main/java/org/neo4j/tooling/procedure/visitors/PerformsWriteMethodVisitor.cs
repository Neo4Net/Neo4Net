﻿/*
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
	using CompilationMessage = Org.Neo4j.Tooling.procedure.messages.CompilationMessage;
	using PerformsWriteMisuseError = Org.Neo4j.Tooling.procedure.messages.PerformsWriteMisuseError;


	using Mode = Org.Neo4j.Procedure.Mode;
	using PerformsWrites = Org.Neo4j.Procedure.PerformsWrites;
	using Procedure = Org.Neo4j.Procedure.Procedure;

	public class PerformsWriteMethodVisitor : SimpleElementVisitor8<Stream<CompilationMessage>, Void>
	{

		 public override Stream<CompilationMessage> VisitExecutable( ExecutableElement method, Void ignored )
		 {
			  Procedure procedure = method.getAnnotation( typeof( Procedure ) );
			  if ( procedure == null )
			  {
					return Stream.of( new PerformsWriteMisuseError( method, "@%s usage error: missing @%s annotation on method", typeof( PerformsWrites ).Name, typeof( Procedure ).Name ) );
			  }

			  if ( procedure.mode() != Mode.DEFAULT )
			  {
					return Stream.of( new PerformsWriteMisuseError( method, "@%s usage error: cannot use mode other than Mode.DEFAULT", typeof( PerformsWrites ).Name ) );
			  }
			  return Stream.empty();
		 }

	}

}