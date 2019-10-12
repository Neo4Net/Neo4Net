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

	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using SecurityContext = Org.Neo4j.@internal.Kernel.Api.security.SecurityContext;
	using UserManager = Org.Neo4j.Kernel.api.security.UserManager;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using Log = Org.Neo4j.Logging.Log;
	using Context = Org.Neo4j.Procedure.Context;
	using ProcedureTransaction = Org.Neo4j.Procedure.ProcedureTransaction;
	using TerminationGuard = Org.Neo4j.Procedure.TerminationGuard;
	using CompilationMessage = Org.Neo4j.Tooling.procedure.messages.CompilationMessage;
	using ContextFieldError = Org.Neo4j.Tooling.procedure.messages.ContextFieldError;
	using ContextFieldWarning = Org.Neo4j.Tooling.procedure.messages.ContextFieldWarning;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.tooling.procedure.CompilerOptions.IGNORE_CONTEXT_WARNINGS_OPTION;

	internal class ContextFieldVisitor : SimpleElementVisitor8<Stream<CompilationMessage>, Void>
	{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		 private static readonly ISet<string> _supportedTypes = new LinkedHashSet<string>( Arrays.asList( typeof( GraphDatabaseService ).FullName, typeof( Log ).FullName, typeof( TerminationGuard ).FullName, typeof( SecurityContext ).FullName, typeof( ProcedureTransaction ).FullName ) );
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		 private static readonly ISet<string> _restrictedTypes = new LinkedHashSet<string>( Arrays.asList( typeof( GraphDatabaseAPI ).FullName, typeof( KernelTransaction ).FullName, typeof( DependencyResolver ).FullName, typeof( UserManager ).FullName, "org.neo4j.kernel.enterprise.api.security.EnterpriseAuthManager", "org.neo4j.server.security.enterprise.log.SecurityLog" ) );

		 private readonly Elements _elements;
		 private readonly Types _types;
		 private readonly bool _ignoresWarnings;

		 internal ContextFieldVisitor( Types types, Elements elements, bool ignoresWarnings )
		 {
			  this._elements = elements;
			  this._types = types;
			  this._ignoresWarnings = ignoresWarnings;
		 }

		 public override Stream<CompilationMessage> VisitVariable( VariableElement field, Void ignored )
		 {
			  return Stream.concat( ValidateModifiers( field ), ValidateInjectedTypes( field ) );
		 }

		 private Stream<CompilationMessage> ValidateModifiers( VariableElement field )
		 {
			  if ( !HasValidModifiers( field ) )
			  {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					return Stream.of( new ContextFieldError( field, "@%s usage error: field %s should be public, non-static and non-final", typeof( Context ).FullName, FieldFullName( field ) ) );
			  }

			  return Stream.empty();
		 }

		 private Stream<CompilationMessage> ValidateInjectedTypes( VariableElement field )
		 {
			  TypeMirror fieldType = field.asType();
			  if ( InjectsAllowedType( fieldType ) )
			  {
					return Stream.empty();
			  }

			  if ( InjectsRestrictedType( fieldType ) )
			  {
					if ( _ignoresWarnings )
					{
						 return Stream.empty();
					}

//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					return Stream.of( new ContextFieldWarning( field, "@%s usage warning: found unsupported restricted type <%s> on %s.\n" + "The procedure will not load unless declared via the configuration option 'dbms.security.procedures.unrestricted'.\n" + "You can ignore this warning by passing the option -A%s to the Java compiler", typeof( Context ).FullName, fieldType.ToString(), FieldFullName(field), IGNORE_CONTEXT_WARNINGS_OPTION ) );
			  }

//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  return Stream.of( new ContextFieldError( field, "@%s usage error: found unknown type <%s> on field %s, expected one of: %s", typeof( Context ).FullName, fieldType.ToString(), FieldFullName(field), JoinTypes(_supportedTypes) ) );
		 }

		 private bool InjectsAllowedType( TypeMirror fieldType )
		 {
			  return Matches( fieldType, _supportedTypes );
		 }

		 private bool InjectsRestrictedType( TypeMirror fieldType )
		 {
			  return Matches( fieldType, _restrictedTypes );
		 }

		 private bool Matches( TypeMirror fieldType, ISet<string> typeNames )
		 {
			  return TypeMirrors( typeNames ).anyMatch( t => _types.isSameType( t, fieldType ) );
		 }

		 private bool HasValidModifiers( VariableElement field )
		 {
			  ISet<Modifier> modifiers = field.Modifiers;
			  return modifiers.Contains( Modifier.PUBLIC ) && !modifiers.Contains( Modifier.STATIC ) && !modifiers.Contains( Modifier.FINAL );
		 }

		 private Stream<TypeMirror> TypeMirrors( ISet<string> typeNames )
		 {
			  return typeNames.Select( _elements.getTypeElement ).Where( Objects.nonNull ).Select( Element.asType );
		 }

		 private string FieldFullName( VariableElement field )
		 {
			  return string.Format( "{0}#{1}", field.EnclosingElement.SimpleName, field.SimpleName );
		 }

		 private static string JoinTypes( ISet<string> types )
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  return types.collect( Collectors.joining( ">, <", "<", ">" ) );
		 }
	}

}