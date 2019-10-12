using System;
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
namespace Neo4Net.Tooling.procedure.validators
{
	using CompilationMessage = Neo4Net.Tooling.procedure.messages.CompilationMessage;
	using DuplicatedProcedureError = Neo4Net.Tooling.procedure.messages.DuplicatedProcedureError;
	using AnnotationTypeVisitor = Neo4Net.Tooling.procedure.visitors.AnnotationTypeVisitor;


	using Procedure = Neo4Net.Procedure.Procedure;

	/// <summary>
	/// Validates that a given extension name is not declared by multiple elements annotated with the same annotation of type
	/// {@code T}.
	/// This validation is done within an annotation processor. This means that the detection is detected only per
	/// compilation unit, not per Neo4j instance.
	/// 
	/// Indeed, a Neo4j instance can aggregate several extension JARs and its duplication detection cannot be entirely
	/// replaced by this.
	/// </summary>
	/// @param <T> annotation type </param>
	public class DuplicatedExtensionValidator<T> : System.Func<ICollection<Element>, Stream<CompilationMessage>> where T : Annotation
	{

		 private readonly Elements _elements;
		 private readonly Type<T> _annotationType;
		 private readonly System.Func<T, Optional<string>> _customNameExtractor;

		 public DuplicatedExtensionValidator( Elements elements, Type annotationType, System.Func<T, Optional<string>> customNameExtractor )
		 {
				 annotationType = typeof( T );
			  this._elements = elements;
			  this._annotationType = annotationType;
			  this._customNameExtractor = customNameExtractor;
		 }

		 public override Stream<CompilationMessage> Apply( ICollection<Element> visitedProcedures )
		 {
			  return FindDuplicates( visitedProcedures );
		 }

		 private Stream<CompilationMessage> FindDuplicates( ICollection<Element> visitedProcedures )
		 {
			  return IndexByName( visitedProcedures ).filter( index => index.Value.size() > 1 ).flatMap(this.asErrors);
		 }

		 private Stream<KeyValuePair<string, IList<Element>>> IndexByName( ICollection<Element> visitedProcedures )
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  return visitedProcedures.collect( groupingBy( this.getName ) ).entrySet().stream();
		 }

		 private string GetName( Element procedure )
		 {
			  T annotation = procedure.getAnnotation( _annotationType );
			  Optional<string> customName = _customNameExtractor.apply( annotation );
			  return customName.orElse( DefaultQualifiedName( procedure ) );
		 }

		 private string DefaultQualifiedName( Element procedure )
		 {
			  return string.Format( "{0}.{1}", _elements.getPackageOf( procedure ).ToString(), procedure.SimpleName );
		 }

		 private Stream<CompilationMessage> AsErrors( KeyValuePair<string, IList<Element>> indexedProcedures )
		 {
			  string duplicatedName = indexedProcedures.Key;
			  return indexedProcedures.Value.Select( procedure => AsError( procedure, duplicatedName, indexedProcedures.Value.size() ) );
		 }

		 private CompilationMessage AsError( Element procedure, string duplicatedName, int duplicateCount )
		 {
			  return new DuplicatedProcedureError( procedure, GetAnnotationMirror( procedure ), "Procedure|function name <%s> is already defined %s times. It should be defined only once!", duplicatedName, duplicateCount.ToString() );
		 }

		 private AnnotationMirror GetAnnotationMirror( Element procedure )
		 {
			  return procedure.AnnotationMirrors.Where( this.isProcedureAnnotationType ).First().orElse(null);
		 }

		 private bool IsProcedureAnnotationType( AnnotationMirror mirror )
		 {
			  return ( new AnnotationTypeVisitor( typeof( Procedure ) ) ).visit( mirror.AnnotationType.asElement() );
		 }

	}

}