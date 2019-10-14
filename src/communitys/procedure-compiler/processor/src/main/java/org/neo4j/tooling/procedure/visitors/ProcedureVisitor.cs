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

	using PerformsWrites = Neo4Net.Procedure.PerformsWrites;
	using TypeMirrorUtils = Neo4Net.Tooling.procedure.compilerutils.TypeMirrorUtils;
	using CompilationMessage = Neo4Net.Tooling.procedure.messages.CompilationMessage;
	using ReturnTypeError = Neo4Net.Tooling.procedure.messages.ReturnTypeError;

	public class ProcedureVisitor : SimpleElementVisitor8<Stream<CompilationMessage>, Void>
	{

		 private readonly Types _typeUtils;
		 private readonly Elements _elementUtils;
		 private readonly ElementVisitor<Stream<CompilationMessage>, Void> _classVisitor;
		 private readonly TypeVisitor<Stream<CompilationMessage>, Void> _recordVisitor;
		 private readonly ElementVisitor<Stream<CompilationMessage>, Void> _parameterVisitor;
		 private readonly ElementVisitor<Stream<CompilationMessage>, Void> _performsWriteVisitor;

		 public ProcedureVisitor( Types typeUtils, Elements elementUtils, bool ignoresWarnings )
		 {
			  TypeMirrorUtils typeMirrors = new TypeMirrorUtils( typeUtils, elementUtils );

			  this._typeUtils = typeUtils;
			  this._elementUtils = elementUtils;
			  this._classVisitor = new ExtensionClassVisitor( typeUtils, elementUtils, ignoresWarnings );
			  this._recordVisitor = new RecordTypeVisitor( typeUtils, typeMirrors );
			  this._parameterVisitor = new ParameterVisitor( new ParameterTypeVisitor( typeUtils, typeMirrors ) );
			  this._performsWriteVisitor = new PerformsWriteMethodVisitor();
		 }

		 /// <summary>
		 /// Validates method parameters and return type
		 /// </summary>
		 public override Stream<CompilationMessage> VisitExecutable( ExecutableElement executableElement, Void ignored )
		 {
			  return Stream.of( _classVisitor.visit( executableElement.EnclosingElement ), ValidateParameters( executableElement.Parameters ), ValidateReturnType( executableElement ), ValidatePerformsWriteUsage( executableElement ) ).flatMap( System.Func.identity() );
		 }

		 private Stream<CompilationMessage> ValidateParameters<T1>( IList<T1> parameters ) where T1 : javax.lang.model.element.VariableElement
		 {
			  return parameters.stream().flatMap(_parameterVisitor.visit);
		 }

		 private Stream<CompilationMessage> ValidateReturnType( ExecutableElement method )
		 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getCanonicalName method:
			  string streamClassName = typeof( Stream ).FullName;

			  TypeMirror streamType = _typeUtils.erasure( _elementUtils.getTypeElement( streamClassName ).asType() );
			  TypeMirror returnType = method.ReturnType;
			  TypeMirror erasedReturnType = _typeUtils.erasure( returnType );

			  TypeMirror voidType = _typeUtils.getNoType( TypeKind.VOID );
			  if ( _typeUtils.isSameType( returnType, voidType ) )
			  {
					return Stream.empty();
			  }

			  if ( !_typeUtils.isSubtype( erasedReturnType, streamType ) )
			  {
					return Stream.of( new ReturnTypeError( method, "Return type of %s#%s must be %s", method.EnclosingElement.SimpleName, method.SimpleName, streamClassName ) );
			  }

			  return _recordVisitor.visit( returnType );
		 }

		 private Stream<CompilationMessage> ValidatePerformsWriteUsage( ExecutableElement executableElement )
		 {
			  if ( executableElement.getAnnotation( typeof( PerformsWrites ) ) != null )
			  {
					return _performsWriteVisitor.visit( executableElement );
			  }
			  return Stream.empty();
		 }

	}

}