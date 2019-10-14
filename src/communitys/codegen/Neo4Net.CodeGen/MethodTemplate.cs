using System;
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
namespace Neo4Net.CodeGen
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.Parameter.NO_PARAMETERS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.TypeReference.NO_TYPES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.TypeReference.typeReference;

	public class MethodTemplate
	{
		 public static Builder Method( Type returnType, string name, params Parameter[] parameters )
		 {
			  return Method( typeReference( returnType ), name, parameters );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Builder method(final TypeReference returnType, final String name, Parameter... parameters)
		 public static Builder Method( TypeReference returnType, string name, params Parameter[] parameters )
		 {
			  try
			  {
					return new BuilderAnonymousInnerClass( parameters, returnType, name );
			  }
			  catch ( Exception e ) when ( e is System.ArgumentException || e is System.NullReferenceException )
			  {
					throw new System.ArgumentException( "Invalid signature for " + name + ": " + e.Message, e );
			  }
		 }

		 private class BuilderAnonymousInnerClass : Builder
		 {
			 private Neo4Net.CodeGen.TypeReference _returnType;
			 private string _name;
			 private new Neo4Net.CodeGen.Parameter[] _parameters;

			 public BuilderAnonymousInnerClass( Neo4Net.CodeGen.Parameter[] parameters, Neo4Net.CodeGen.TypeReference returnType, string name ) : base( parameters )
			 {
				 this._returnType = returnType;
				 this._name = name;
				 this._parameters = parameters;
			 }

			 public override MethodTemplate build()
			 {
				  return BuildMethod( this, _returnType, _name );
			 }

			 internal override MethodDeclaration.Builder declaration()
			 {
				  return MethodDeclaration.Method( _returnType, _name, _parameters );
			 }
		 }

		 public static ConstructorBuilder Constructor( params Parameter[] parameters )
		 {
			  try
			  {
					return new ConstructorBuilder( parameters );
			  }
			  catch ( Exception e ) when ( e is System.ArgumentException || e is System.NullReferenceException )
			  {
					throw new System.ArgumentException( "Invalid constructor signature: " + e.Message, e );
			  }
		 }

		 public class ConstructorBuilder : Builder
		 {
			  internal ConstructorBuilder( Parameter[] parameters ) : base( parameters )
			  {
			  }

			  public virtual Builder InvokeSuper()
			  {
					return Expression( ExpressionTemplate.InvokeSuperConstructor( new ExpressionTemplate[]{}, TypeReference.NoTypes ) );
			  }

			  public virtual Builder InvokeSuper( ExpressionTemplate[] parameters, Type[] parameterTypes )
			  {
					TypeReference[] references = new TypeReference[parameterTypes.Length];
					for ( int i = 0; i < parameterTypes.Length; i++ )
					{
						 references[i] = typeReference( parameterTypes[i] );
					}

					return InvokeSuper( parameters, references );
			  }

			  public virtual Builder InvokeSuper( ExpressionTemplate[] parameters, TypeReference[] parameterTypes )
			  {
					return Expression( ExpressionTemplate.InvokeSuperConstructor( parameters, parameterTypes ) );
			  }

			  public override MethodTemplate Build()
			  {
					return BuildConstructor( this );
			  }

			  internal override MethodDeclaration.Builder Declaration()
			  {
					return MethodDeclaration.Constructor( Parameters );
			  }
		 }

		 public abstract class Builder
		 {
			  internal readonly Parameter[] Parameters;
			  internal readonly IDictionary<string, TypeReference> Locals = new Dictionary<string, TypeReference>();
			  internal readonly IList<Statement> Statements = new List<Statement>();
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal int ModifiersConflict = Modifier.PUBLIC;

			  internal Builder( Parameter[] parameters )
			  {
					if ( parameters == null || parameters.Length == 0 )
					{
						 this.Parameters = NO_PARAMETERS;
					}
					else
					{
						 this.Parameters = parameters.Clone();
					}
					for ( int i = 0; i < this.Parameters.Length; i++ )
					{
						 Parameter parameter = requireNonNull( this.Parameters[i], "Parameter " + i );
						 if ( null != Locals[parameter.Name()] = parameter.Type() )
						 {
							  throw new System.ArgumentException( "Duplicate parameters named \"" + parameter.Name() + "\"." );
						 }
					}
			  }

			  public abstract MethodTemplate Build();

			  public virtual Builder Expression( ExpressionTemplate expression )
			  {
					Statements.Add( Statement.Expression( expression ) );
					return this;
			  }

			  public virtual Builder Put( ExpressionTemplate target, Type fieldType, string fieldName, ExpressionTemplate expression )
			  {
					return Put( target, typeReference( fieldType ), fieldName, expression );
			  }

			  public virtual Builder Put( ExpressionTemplate target, TypeReference fieldType, string fieldName, ExpressionTemplate expression )
			  {
					Statements.Add( Statement.Put( target, Lookup.Field( fieldType, fieldName ), expression ) );
					return this;
			  }

			  public virtual Builder Modifiers( int modifiers )
			  {
					this.ModifiersConflict = modifiers;
					return this;
			  }

			  public virtual Builder Returns( ExpressionTemplate value )
			  {
					Statements.Add( Statement.Returns( value ) );
					return this;
			  }

			  internal abstract MethodDeclaration.Builder Declaration();
		 }

		 public virtual TypeReference ReturnType()
		 {
			  return _returnType;
		 }

		 public virtual string Name()
		 {
			  return _name;
		 }

		 public virtual int Modifiers()
		 {
			  return _modifiers;
		 }

		 public virtual TypeReference[] ParameterTypes()
		 {
			  if ( _parameters.Length == 0 )
			  {
					return NO_TYPES;
			  }
			  TypeReference[] result = new TypeReference[_parameters.Length];
			  for ( int i = 0; i < result.Length; i++ )
			  {
					result[i] = _parameters[i].type();
			  }
			  return result;
		 }

		 internal virtual MethodDeclaration Declaration( ClassHandle handle )
		 {
			  return _declaration.build( handle );
		 }

		 internal virtual void Generate( CodeBlock generator )
		 {
			  foreach ( Statement statement in _statements )
			  {
					statement.Generate( generator );
			  }
		 }

		 private static MethodTemplate BuildMethod( Builder builder, TypeReference returnType, string name )
		 {
			  return new MethodTemplate( builder, returnType, name );
		 }

		 private static MethodTemplate BuildConstructor( Builder builder )
		 {
			  return new MethodTemplate( builder, TypeReference.VoidConflict, "<init>" );
		 }

		 private readonly MethodDeclaration.Builder _declaration;
		 private readonly Parameter[] _parameters;
		 private readonly Statement[] _statements;
		 private readonly TypeReference _returnType;
		 private readonly string _name;
		 private readonly int _modifiers;

		 private MethodTemplate( Builder builder, TypeReference returnType, string name )
		 {
			  this._returnType = returnType;
			  this._name = name;
			  this._declaration = builder.Declaration();
			  this._parameters = builder.Parameters;
			  this._statements = builder.Statements.ToArray();
			  this._modifiers = builder.ModifiersConflict;
		 }
	}

}