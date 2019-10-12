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
namespace Org.Neo4j.Tooling.procedure
{

	using Procedure = Org.Neo4j.Procedure.Procedure;
	using UserAggregationFunction = Org.Neo4j.Procedure.UserAggregationFunction;
	using UserAggregationResult = Org.Neo4j.Procedure.UserAggregationResult;
	using UserAggregationUpdate = Org.Neo4j.Procedure.UserAggregationUpdate;
	using UserFunction = Org.Neo4j.Procedure.UserFunction;
	using CompilationMessage = Org.Neo4j.Tooling.procedure.messages.CompilationMessage;
	using MessagePrinter = Org.Neo4j.Tooling.procedure.messages.MessagePrinter;
	using Org.Neo4j.Tooling.procedure.validators;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.tooling.procedure.CompilerOptions.IGNORE_CONTEXT_WARNINGS_OPTION;

	/// <summary>
	/// Base processor that processes <seealso cref="Element"/> annotated with {@code T}.
	/// It also detects and reports duplicated elements (duplication can obviously be detected within a compilation unit and
	/// not globally per Neo4j instance, as explained in <seealso cref="DuplicatedExtensionValidator"/>.
	/// </summary>
	/// @param <T> processed annotation type </param>
	public class DuplicationAwareBaseProcessor<T> : AbstractProcessor where T : Annotation
	{
		 private readonly ISet<Element> _visitedElements = new LinkedHashSet<Element>();
		 private readonly Type<T> _supportedAnnotationType;
		 private readonly System.Func<T, Optional<string>> _customNameFunction;
		 private readonly System.Func<ProcessingEnvironment, ElementVisitor<Stream<CompilationMessage>, Void>> _visitorSupplier;

		 private System.Func<ICollection<Element>, Stream<CompilationMessage>> _duplicationValidator;
		 private ElementVisitor<Stream<CompilationMessage>, Void> _visitor;
		 private MessagePrinter _messagePrinter;

		 /// <summary>
		 /// Base initialization of Neo4j extension processor (where extension can be <seealso cref="Procedure"/>, <seealso cref="UserFunction"/>,
		 /// <seealso cref="UserAggregationFunction"/>).
		 /// </summary>
		 /// <param name="supportedAnnotationType"> main annotation type supported by the processor. The main annotation may depend on
		 /// other annotations (e.g. <seealso cref="UserAggregationFunction"/> works with <seealso cref="UserAggregationResult"/> and
		 /// <seealso cref="UserAggregationUpdate"/>).
		 /// However, by design, these auxiliary annotations are processed by traversing the
		 /// element graph, rather than by standalone annotation processors. </param>
		 /// <param name="customNameFunction"> function allowing to extract the custom simple name of the annotated element </param>
		 /// <param name="visitorSupplier"> supplies the main <seealso cref="ElementVisitor"/> class in charge of traversing and validating the
		 /// annotated elements </param>
		 public DuplicationAwareBaseProcessor( Type supportedAnnotationType, System.Func<T, Optional<string>> customNameFunction, System.Func<ProcessingEnvironment, ElementVisitor<Stream<CompilationMessage>, Void>> visitorSupplier )
		 {
				 supportedAnnotationType = typeof( T );
			  this._supportedAnnotationType = supportedAnnotationType;
			  this._customNameFunction = customNameFunction;
			  this._visitorSupplier = visitorSupplier;
		 }

		 public override void Init( ProcessingEnvironment processingEnv )
		 {
			 lock ( this )
			 {
				  base.Init( processingEnv );
      
				  _messagePrinter = new MessagePrinter( processingEnv.Messager );
				  _duplicationValidator = new DuplicatedExtensionValidator<ICollection<Element>, Stream<CompilationMessage>>( processingEnv.ElementUtils, _supportedAnnotationType, _customNameFunction );
				  _visitor = _visitorSupplier.apply( processingEnv );
			 }
		 }

		 public override ISet<string> SupportedOptions
		 {
			 get
			 {
				  return Collections.singleton( IGNORE_CONTEXT_WARNINGS_OPTION );
			 }
		 }

		 public override ISet<string> SupportedAnnotationTypes
		 {
			 get
			 {
	//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				  return Collections.singleton( _supportedAnnotationType.FullName );
			 }
		 }

		 public override SourceVersion SupportedSourceVersion
		 {
			 get
			 {
				  return SourceVersion.latestSupported();
			 }
		 }

		 public override bool Process<T1>( ISet<T1> annotations, RoundEnvironment roundEnv ) where T1 : javax.lang.model.element.TypeElement
		 {
			  ProcessElements( roundEnv );
			  if ( roundEnv.processingOver() )
			  {
					_duplicationValidator.apply( _visitedElements ).forEach( _messagePrinter.print );
			  }
			  return false;
		 }

		 private void ProcessElements( RoundEnvironment roundEnv )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Set<? extends javax.lang.model.element.Element> functions = roundEnv.getElementsAnnotatedWith(supportedAnnotationType);
			  ISet<Element> functions = roundEnv.getElementsAnnotatedWith( _supportedAnnotationType );
			  _visitedElements.addAll( functions );
			  functions.stream().flatMap(this.validate).forEachOrdered(_messagePrinter.print);
		 }

		 private Stream<CompilationMessage> Validate( Element element )
		 {
			  return _visitor.visit( element );
		 }
	}

}