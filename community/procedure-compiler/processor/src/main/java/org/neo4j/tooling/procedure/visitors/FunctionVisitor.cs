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

	using TypeMirrorUtils = Org.Neo4j.Tooling.procedure.compilerutils.TypeMirrorUtils;
	using CompilationMessage = Org.Neo4j.Tooling.procedure.messages.CompilationMessage;
	using FunctionInRootNamespaceError = Org.Neo4j.Tooling.procedure.messages.FunctionInRootNamespaceError;
	using ReturnTypeError = Org.Neo4j.Tooling.procedure.messages.ReturnTypeError;
	using AllowedTypesValidator = Org.Neo4j.Tooling.procedure.validators.AllowedTypesValidator;

	public class FunctionVisitor<T> where T : Annotation
	{

		 private readonly ElementVisitor<Stream<CompilationMessage>, Void> _parameterVisitor;
		 private readonly Elements _elements;
		 private readonly ElementVisitor<Stream<CompilationMessage>, Void> _classVisitor;
		 private readonly System.Func<T, Optional<string>> _customNameExtractor;
		 private readonly Type<T> _annotationType;
		 private readonly AllowedTypesValidator _allowedTypesValidator;

		 public FunctionVisitor( Type annotationType, Types types, Elements elements, TypeMirrorUtils typeMirrorUtils, System.Func<T, Optional<string>> customNameExtractor, bool ignoresWarnings )
		 {
				 annotationType = typeof( T );
			  this._customNameExtractor = customNameExtractor;
			  this._annotationType = annotationType;
			  this._classVisitor = new ExtensionClassVisitor( types, elements, ignoresWarnings );
			  this._parameterVisitor = new ParameterVisitor( new ParameterTypeVisitor( types, typeMirrorUtils ) );
			  this._elements = elements;
			  this._allowedTypesValidator = new AllowedTypesValidator( typeMirrorUtils, types );
		 }

		 public virtual Stream<CompilationMessage> ValidateEnclosingClass( ExecutableElement method )
		 {
			  return _classVisitor.visit( method.EnclosingElement );
		 }

		 public virtual Stream<CompilationMessage> ValidateParameters<T1>( IList<T1> parameters ) where T1 : javax.lang.model.element.VariableElement
		 {
			  return parameters.stream().flatMap(_parameterVisitor.visit);
		 }

		 public virtual Stream<CompilationMessage> ValidateName( ExecutableElement method )
		 {
			  Optional<string> customName = _customNameExtractor.apply( method.getAnnotation( _annotationType ) );
			  if ( customName.Present )
			  {
					if ( IsInRootNamespace( customName.get() ) )
					{
						 return Stream.of( RootNamespaceError( method, customName.get() ) );
					}
					return Stream.empty();
			  }

			  PackageElement @namespace = _elements.getPackageOf( method );
			  if ( @namespace == null )
			  {
					return Stream.of( RootNamespaceError( method ) );
			  }
			  return Stream.empty();
		 }

		 public virtual Stream<CompilationMessage> ValidateReturnType( ExecutableElement method )
		 {
			  TypeMirror returnType = method.ReturnType;
			  if ( !_allowedTypesValidator.test( returnType ) )
			  {
					return Stream.of( new ReturnTypeError( method, "Unsupported return type <%s> of function defined in <%s#%s>.", returnType, method.EnclosingElement, method.SimpleName ) );
			  }
			  return Stream.empty();
		 }

		 private bool IsInRootNamespace( string name )
		 {
			  return !name.Contains( "." ) || name.Split( "\\.", true )[0].Empty;
		 }

		 private FunctionInRootNamespaceError RootNamespaceError( ExecutableElement method, string name )
		 {
			  return new FunctionInRootNamespaceError( method, "Function <%s> cannot be defined in the root namespace. Valid name example: com.acme.my_function", name );
		 }

		 private FunctionInRootNamespaceError RootNamespaceError( ExecutableElement method )
		 {
			  return new FunctionInRootNamespaceError( method, "Function defined in <%s#%s> cannot be defined in the root namespace. " + "Valid name example: com.acme.my_function", method.EnclosingElement.SimpleName, method.SimpleName );
		 }

	}

}