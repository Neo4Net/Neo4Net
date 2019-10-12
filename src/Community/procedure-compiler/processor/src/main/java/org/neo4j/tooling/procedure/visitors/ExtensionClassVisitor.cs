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

	using CompilationMessage = Neo4Net.Tooling.procedure.messages.CompilationMessage;
	using ExtensionMissingPublicNoArgConstructor = Neo4Net.Tooling.procedure.messages.ExtensionMissingPublicNoArgConstructor;

	public class ExtensionClassVisitor : SimpleElementVisitor8<Stream<CompilationMessage>, Void>
	{

		 private readonly ISet<TypeElement> _visitedElements = new HashSet<TypeElement>();
		 private readonly FieldVisitor _fieldVisitor;

		 public ExtensionClassVisitor( Types types, Elements elements, bool ignoresWarnings )
		 {
			  _fieldVisitor = new FieldVisitor( types, elements, ignoresWarnings );
		 }

		 public override Stream<CompilationMessage> VisitType( TypeElement extensionClass, Void ignored )
		 {
			  if ( IsFirstVisit( extensionClass ) )
			  {
					return Stream.concat( ValidateFields( extensionClass ), ValidateConstructor( extensionClass ) );
			  }
			  return Stream.empty();
		 }

		 /// <summary>
		 /// Check if the <seealso cref="TypeElement"/> is visited for the first time. A <seealso cref="TypeElement"/> will be visited once per
		 /// procedure it contains, but it only needs to be validated once.
		 /// </summary>
		 /// <param name="e"> The visited <seealso cref="TypeElement"/> </param>
		 /// <returns> true for the first visit of the <seealso cref="TypeElement"/>, false afterwards </returns>
		 private bool IsFirstVisit( TypeElement e )
		 {
			  return _visitedElements.Add( e );
		 }

		 private Stream<CompilationMessage> ValidateFields( TypeElement e )
		 {
			  return e.EnclosedElements.stream().flatMap(_fieldVisitor.visit);
		 }

		 private Stream<CompilationMessage> ValidateConstructor( Element extensionClass )
		 {
			  Optional<ExecutableElement> publicNoArgConstructor = constructorsIn( extensionClass.EnclosedElements ).Where( c => c.Modifiers.contains( Modifier.PUBLIC ) ).Where( c => c.Parameters.Empty ).First();

			  if ( !publicNoArgConstructor.Present )
			  {
					return Stream.of( new ExtensionMissingPublicNoArgConstructor( extensionClass, "Extension class %s should contain a public no-arg constructor, none found.", extensionClass ) );
			  }
			  return Stream.empty();
		 }
	}

}