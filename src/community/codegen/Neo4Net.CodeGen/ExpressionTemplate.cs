using System;
using System.Diagnostics;

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
//	import static Neo4Net.codegen.TypeReference.OBJECT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.TypeReference.VOID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.TypeReference.typeReference;

	public abstract class ExpressionTemplate
	{
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal readonly TypeReference TypeConflict;

		 protected internal ExpressionTemplate( TypeReference type )
		 {
			  this.TypeConflict = type;
		 }

		 public static ExpressionTemplate Self( TypeReference type )
		 {
			  return new ExpressionTemplateAnonymousInnerClass( type );
		 }

		 private class ExpressionTemplateAnonymousInnerClass : ExpressionTemplate
		 {
			 public ExpressionTemplateAnonymousInnerClass( Neo4Net.CodeGen.TypeReference type ) : base( type )
			 {
			 }

			 internal override void templateAccept( CodeBlock method, ExpressionVisitor visitor )
			 {
				  visitor.Load( new LocalVariable( method.Clazz.handle(), "this", 0 ) );
			 }
		 }

		 /// <summary>
		 /// invoke a static method or constructor </summary>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static ExpressionTemplate invoke(final MethodReference method, final ExpressionTemplate... arguments)
		 public static ExpressionTemplate Invoke( MethodReference method, params ExpressionTemplate[] arguments )
		 {
			  // Try to materialize this expression early
			  Expression[] materialized = TryMaterialize( arguments );
			  if ( materialized != null )
			  {
					return Expression.Invoke( method, materialized );
			  }
			  // some part needs reference to the method context, so this expression will transitively need it
			  return new ExpressionTemplateAnonymousInnerClass2( method.Returns(), method, arguments );
		 }

		 private class ExpressionTemplateAnonymousInnerClass2 : ExpressionTemplate
		 {
			 private Neo4Net.CodeGen.MethodReference _method;
			 private Neo4Net.CodeGen.ExpressionTemplate[] _arguments;

			 public ExpressionTemplateAnonymousInnerClass2( Neo4Net.CodeGen.TypeReference returns, Neo4Net.CodeGen.MethodReference method, Neo4Net.CodeGen.ExpressionTemplate[] arguments ) : base( returns )
			 {
				 this._method = method;
				 this._arguments = arguments;
			 }

			 protected internal override void templateAccept( CodeBlock generator, ExpressionVisitor visitor )
			 {
				  visitor.Invoke( _method, Materialize( generator, _arguments ) );
			 }
		 }

		 /// <summary>
		 /// invoke an instance method </summary>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static ExpressionTemplate invoke(final ExpressionTemplate target, final MethodReference method, final ExpressionTemplate... arguments)
		 public static ExpressionTemplate Invoke( ExpressionTemplate target, MethodReference method, params ExpressionTemplate[] arguments )
		 {
			  if ( target is Expression )
			  {
					Expression[] materialized = TryMaterialize( arguments );
					if ( materialized != null )
					{
						 return Expression.Invoke( ( Expression ) target, method, materialized );
					}
			  }
			  return new ExpressionTemplateAnonymousInnerClass3( method.Returns(), target, method, arguments );
		 }

		 private class ExpressionTemplateAnonymousInnerClass3 : ExpressionTemplate
		 {
			 private Neo4Net.CodeGen.ExpressionTemplate _target;
			 private Neo4Net.CodeGen.MethodReference _method;
			 private Neo4Net.CodeGen.ExpressionTemplate[] _arguments;

			 public ExpressionTemplateAnonymousInnerClass3( Neo4Net.CodeGen.TypeReference returns, Neo4Net.CodeGen.ExpressionTemplate target, Neo4Net.CodeGen.MethodReference method, Neo4Net.CodeGen.ExpressionTemplate[] arguments ) : base( returns )
			 {
				 this._target = target;
				 this._method = method;
				 this._arguments = arguments;
			 }

			 protected internal override void templateAccept( CodeBlock generator, ExpressionVisitor visitor )
			 {
				  visitor.Invoke( _target.materialize( generator ), _method, Materialize( generator, _arguments ) );
			 }
		 }

		 /// <summary>
		 /// load a local variable </summary>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static ExpressionTemplate load(final String name, final TypeReference type)
		 public static ExpressionTemplate Load( string name, TypeReference type )
		 {
			  return new ExpressionTemplateAnonymousInnerClass4( type, name );
		 }

		 private class ExpressionTemplateAnonymousInnerClass4 : ExpressionTemplate
		 {
			 private string _name;

			 public ExpressionTemplateAnonymousInnerClass4( Neo4Net.CodeGen.TypeReference type, string name ) : base( type )
			 {
				 this._name = name;
			 }

			 protected internal override void templateAccept( CodeBlock method, ExpressionVisitor visitor )
			 {
				  visitor.Load( method.Local( _name ) );
			 }
		 }

		 public static ExpressionTemplate Get( ExpressionTemplate target, Type fieldType, string fieldName )
		 {
			  return get( target, typeReference( fieldType ), fieldName );
		 }

		 /// <summary>
		 /// instance field </summary>
		 public static ExpressionTemplate Get( ExpressionTemplate target, TypeReference fieldType, string fieldName )
		 {
			  return Get( target, Lookup.Field( fieldType, fieldName ), fieldType );
		 }

		 /// <summary>
		 /// instance field </summary>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static ExpressionTemplate get(final ExpressionTemplate target, final FieldReference field)
		 public static ExpressionTemplate Get( ExpressionTemplate target, FieldReference field )
		 {
			  if ( target is Expression )
			  {
					return Expression.Get( ( Expression ) target, field );
			  }
			  return new ExpressionTemplateAnonymousInnerClass5( field.Type(), target, field );
		 }

		 private class ExpressionTemplateAnonymousInnerClass5 : ExpressionTemplate
		 {
			 private Neo4Net.CodeGen.ExpressionTemplate _target;
			 private Neo4Net.CodeGen.FieldReference _field;

			 public ExpressionTemplateAnonymousInnerClass5( Neo4Net.CodeGen.TypeReference type, Neo4Net.CodeGen.ExpressionTemplate target, Neo4Net.CodeGen.FieldReference field ) : base( type )
			 {
				 this._target = target;
				 this._field = field;
			 }

			 internal override void templateAccept( CodeBlock method, ExpressionVisitor visitor )
			 {
				  visitor.GetField( _target.materialize( method ), _field );
			 }
		 }

		 /// <summary>
		 /// static field from the class that will host this expression </summary>
		 public static ExpressionTemplate Get( TypeReference fieldType, string fieldName )
		 {
			  return Get( Lookup.Field( fieldType, fieldName ), fieldType );
		 }

		 /// <summary>
		 /// instance field </summary>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static ExpressionTemplate get(final ExpressionTemplate target, final Lookup<FieldReference> field, TypeReference type)
		 public static ExpressionTemplate Get( ExpressionTemplate target, Lookup<FieldReference> field, TypeReference type )
		 {
			  return new ExpressionTemplateAnonymousInnerClass6( type, target, field );
		 }

		 private class ExpressionTemplateAnonymousInnerClass6 : ExpressionTemplate
		 {
			 private Neo4Net.CodeGen.ExpressionTemplate _target;
			 private Neo4Net.CodeGen.Lookup<FieldReference> _field;

			 public ExpressionTemplateAnonymousInnerClass6( Neo4Net.CodeGen.TypeReference type, Neo4Net.CodeGen.ExpressionTemplate target, Neo4Net.CodeGen.Lookup<FieldReference> field ) : base( type )
			 {
				 this._target = target;
				 this._field = field;
			 }

			 internal override void templateAccept( CodeBlock method, ExpressionVisitor visitor )
			 {
				  visitor.GetField( _target.materialize( method ), _field.lookup( method ) );
			 }
		 }

		 /// <summary>
		 /// static field </summary>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static ExpressionTemplate get(final Lookup<FieldReference> field, TypeReference type)
		 public static ExpressionTemplate Get( Lookup<FieldReference> field, TypeReference type )
		 {
			  return new ExpressionTemplateAnonymousInnerClass7( type, field );
		 }

		 private class ExpressionTemplateAnonymousInnerClass7 : ExpressionTemplate
		 {
			 private Neo4Net.CodeGen.Lookup<FieldReference> _field;

			 public ExpressionTemplateAnonymousInnerClass7( Neo4Net.CodeGen.TypeReference type, Neo4Net.CodeGen.Lookup<FieldReference> field ) : base( type )
			 {
				 this._field = field;
			 }

			 internal override void templateAccept( CodeBlock method, ExpressionVisitor visitor )
			 {
				  visitor.GetStatic( _field.lookup( method ) );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: Expression materialize(final CodeBlock method)
		 internal virtual Expression Materialize( CodeBlock method )
		 {
			  return new ExpressionAnonymousInnerClass( this, TypeConflict, method );
		 }

		 private class ExpressionAnonymousInnerClass : Expression
		 {
			 private readonly ExpressionTemplate _outerInstance;

			 private Neo4Net.CodeGen.CodeBlock _method;

			 public ExpressionAnonymousInnerClass( ExpressionTemplate outerInstance, Neo4Net.CodeGen.TypeReference type, Neo4Net.CodeGen.CodeBlock method ) : base( type )
			 {
				 this.outerInstance = outerInstance;
				 this._method = method;
			 }

			 public override void accept( ExpressionVisitor visitor )
			 {
				  _outerInstance.templateAccept( _method, visitor );
			 }
		 }

		 public static ExpressionTemplate Cast( Type clazz, ExpressionTemplate expression )
		 {
			  return Cast( typeReference( clazz ), expression );
		 }

		 public static ExpressionTemplate Cast( TypeReference type, ExpressionTemplate expression )
		 {
			  return new ExpressionTemplateAnonymousInnerClass8( type, expression );
		 }

		 private class ExpressionTemplateAnonymousInnerClass8 : ExpressionTemplate
		 {
			 private new Neo4Net.CodeGen.TypeReference _type;
			 private Neo4Net.CodeGen.ExpressionTemplate _expression;

			 public ExpressionTemplateAnonymousInnerClass8( Neo4Net.CodeGen.TypeReference type, Neo4Net.CodeGen.ExpressionTemplate expression ) : base( type )
			 {
				 this._type = type;
				 this._expression = expression;
			 }

			 protected internal override void templateAccept( CodeBlock method, ExpressionVisitor visitor )
			 {
				  visitor.Cast( _type, _expression.materialize( method ) );
			 }
		 }

		 internal abstract void TemplateAccept( CodeBlock method, ExpressionVisitor visitor );

		 private static Expression[] TryMaterialize( ExpressionTemplate[] templates )
		 {
			  if ( templates is Expression[] )
			  {
					return ( Expression[] ) templates;
			  }
			  Expression[] materialized = new Expression[templates.Length];
			  for ( int i = 0; i < materialized.Length; i++ )
			  {
					if ( templates[i] is Expression )
					{
						 materialized[i] = ( Expression ) templates[i];
					}
					else
					{
						 return null;
					}
			  }
			  return materialized;
		 }

		 internal static Expression[] Materialize( CodeBlock method, ExpressionTemplate[] templates )
		 {
			  Expression[] expressions = new Expression[templates.Length];
			  for ( int i = 0; i < expressions.Length; i++ )
			  {
					expressions[i] = templates[i].Materialize( method );
			  }
			  return expressions;
		 }

		 //TODO I am not crazy about the way type parameters are sent here
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static ExpressionTemplate invokeSuperConstructor(final ExpressionTemplate[] parameters, final TypeReference[] parameterTypes)
		 public static ExpressionTemplate InvokeSuperConstructor( ExpressionTemplate[] parameters, TypeReference[] parameterTypes )
		 {
			  Debug.Assert( parameters.Length == parameterTypes.Length );
			  return new ExpressionTemplateAnonymousInnerClass9( OBJECT, parameters, parameterTypes );
		 }

		 private class ExpressionTemplateAnonymousInnerClass9 : ExpressionTemplate
		 {
			 private Neo4Net.CodeGen.ExpressionTemplate[] _parameters;
			 private Neo4Net.CodeGen.TypeReference[] _parameterTypes;

			 public ExpressionTemplateAnonymousInnerClass9( UnknownType @object, Neo4Net.CodeGen.ExpressionTemplate[] parameters, Neo4Net.CodeGen.TypeReference[] parameterTypes ) : base( @object )
			 {
				 this._parameters = parameters;
				 this._parameterTypes = parameterTypes;
			 }

			 internal override void templateAccept( CodeBlock method, ExpressionVisitor visitor )
			 {
				  visitor.Invoke( Expression.SUPER, new MethodReference( method.Clazz.handle().parent(), "<init>", VOID, Modifier.PUBLIC, _parameterTypes ), Materialize(method, _parameters) );
			 }
		 }

		 public virtual TypeReference Type()
		 {
			  return TypeConflict;
		 }
	}

}