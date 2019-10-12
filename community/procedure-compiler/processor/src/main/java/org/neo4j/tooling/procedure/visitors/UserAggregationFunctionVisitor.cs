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
namespace Org.Neo4j.Tooling.procedure.visitors
{

	using UserAggregationFunction = Org.Neo4j.Procedure.UserAggregationFunction;
	using UserAggregationResult = Org.Neo4j.Procedure.UserAggregationResult;
	using UserAggregationUpdate = Org.Neo4j.Procedure.UserAggregationUpdate;
	using AggregationError = Org.Neo4j.Tooling.procedure.messages.AggregationError;
	using CompilationMessage = Org.Neo4j.Tooling.procedure.messages.CompilationMessage;

	public class UserAggregationFunctionVisitor : SimpleElementVisitor8<Stream<CompilationMessage>, Void>
	{

		 private readonly FunctionVisitor<UserAggregationFunction> _functionVisitor;
		 private readonly Types _types;
		 private readonly ElementVisitor<CharSequence, Void> _typeVisitor;

		 public UserAggregationFunctionVisitor( FunctionVisitor<UserAggregationFunction> baseFunctionVisitor, Types types )
		 {
			  this._functionVisitor = baseFunctionVisitor;
			  this._types = types;
			  this._typeVisitor = new QualifiedTypeVisitor();
		 }

		 public override Stream<CompilationMessage> VisitExecutable( ExecutableElement aggregationFunction, Void ignored )
		 {
			  return Stream.of( _functionVisitor.validateEnclosingClass( aggregationFunction ), ValidateParameters( aggregationFunction, typeof( UserAggregationFunction ) ), _functionVisitor.validateName( aggregationFunction ), ValidateAggregationType( aggregationFunction ) ).flatMap( System.Func.identity() );
		 }

		 private Stream<CompilationMessage> ValidateAggregationType( ExecutableElement aggregationFunction )
		 {
			  TypeMirror returnType = aggregationFunction.ReturnType;
			  Element returnTypeElement = _types.asElement( returnType );
			  if ( returnTypeElement == null )
			  {
					return Stream.of( new AggregationError( aggregationFunction, "Unsupported return type <%s> of aggregation function.", returnType.ToString(), aggregationFunction.EnclosingElement ) );
			  }

			  IList<ExecutableElement> updateMethods = MethodsAnnotatedWith( returnTypeElement, typeof( UserAggregationUpdate ) );
			  IList<ExecutableElement> resultMethods = MethodsAnnotatedWith( returnTypeElement, typeof( UserAggregationResult ) );

			  return Stream.concat( ValidateAggregationUpdateMethod( aggregationFunction, returnTypeElement, updateMethods ), ValidateAggregationResultMethod( aggregationFunction, returnTypeElement, resultMethods ) );
		 }

		 private IList<ExecutableElement> MethodsAnnotatedWith( Element returnType, Type annotationType )
		 {
			  return ElementFilter.methodsIn( returnType.EnclosedElements ).Where( m => m.getAnnotation( annotationType ) != null ).ToList();
		 }

		 private Stream<CompilationMessage> ValidateAggregationUpdateMethod( ExecutableElement aggregationFunction, Element returnType, IList<ExecutableElement> updateMethods )
		 {
			  if ( updateMethods.Count != 1 )
			  {
					return Stream.of( MissingAnnotation( aggregationFunction, returnType, updateMethods, typeof( UserAggregationUpdate ) ) );
			  }

			  Stream<CompilationMessage> errors = Stream.empty();

			  ExecutableElement updateMethod = updateMethods.GetEnumerator().next();

			  if ( !IsValidUpdateSignature( updateMethod ) )
			  {
					errors = Stream.of( new AggregationError( updateMethod, "@%s usage error: method should be public, non-static and define 'void' as return type.", typeof( UserAggregationUpdate ).Name ) );
			  }
			  return Stream.concat( errors, _functionVisitor.validateParameters( updateMethod.Parameters ) );
		 }

		 private Stream<CompilationMessage> ValidateAggregationResultMethod( ExecutableElement aggregationFunction, Element returnType, IList<ExecutableElement> resultMethods )
		 {
			  if ( resultMethods.Count != 1 )
			  {
					return Stream.of( MissingAnnotation( aggregationFunction, returnType, resultMethods, typeof( UserAggregationResult ) ) );
			  }

			  ExecutableElement resultMethod = resultMethods.GetEnumerator().next();
			  return Stream.concat( ValidateParameters( resultMethod, typeof( UserAggregationUpdate ) ), _functionVisitor.validateReturnType( resultMethod ) );
		 }

		 private Stream<CompilationMessage> ValidateParameters( ExecutableElement resultMethod, Type annotation )
		 {
			  if ( !IsValidAggregationSignature( resultMethod ) )
			  {
					return Stream.of( new AggregationError( resultMethod, "@%s usage error: method should be public, non-static and without parameters.", annotation.Name ) );
			  }
			  return Stream.empty();
		 }

		 private AggregationError MissingAnnotation( ExecutableElement aggregationFunction, Element returnType, IList<ExecutableElement> updateMethods, Type annotation )
		 {
			  return new AggregationError( aggregationFunction, "@%s usage error: expected aggregation type <%s> to define exactly 1 method with this annotation. %s.", annotation.Name, _typeVisitor.visit( returnType ), updateMethods.Count == 0 ? "Found none" : "Several methods found: " + MethodNames( updateMethods ) );
		 }

		 private bool IsValidUpdateSignature( ExecutableElement updateMethod )
		 {
			  // note: parameters are checked subsequently
			  return IsPublicNonStatic( updateMethod.Modifiers ) && updateMethod.ReturnType.Kind.Equals( VOID );
		 }

		 private bool IsValidAggregationSignature( ExecutableElement resultMethod )
		 {
			  // note: return type is checked subsequently
			  return IsPublicNonStatic( resultMethod.Modifiers ) && resultMethod.Parameters.Empty;
		 }

		 private bool IsPublicNonStatic( ISet<Modifier> modifiers )
		 {
			  return modifiers.Contains( Modifier.PUBLIC ) && !modifiers.Contains( Modifier.STATIC );
		 }

		 private string MethodNames( IList<ExecutableElement> updateMethods )
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  return updateMethods.Select( ExecutableElement.getSimpleName ).collect( Collectors.joining( "," ) );
		 }
	}

}