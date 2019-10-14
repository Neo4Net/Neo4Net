using System;
using System.Diagnostics;
using System.Text;

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

	using AnyValue = Neo4Net.Values.AnyValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.TypeReference.BOOLEAN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.TypeReference.DOUBLE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.TypeReference.INT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.TypeReference.LONG;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.TypeReference.OBJECT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.TypeReference.VALUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.TypeReference.VOID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.TypeReference.arrayOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.TypeReference.typeReference;

	public abstract class Expression : ExpressionTemplate
	{
		 public static readonly Expression TRUE = new ConstantAnonymousInnerClass( BOOLEAN );

		 private class ConstantAnonymousInnerClass : Constant
		 {
			 public ConstantAnonymousInnerClass( UnknownType boolean ) : base( boolean, true )
			 {
			 }

			 internal override Expression not()
			 {
				  return FALSE;
			 }
		 }
		 public static readonly Expression FALSE = new ConstantAnonymousInnerClass2( BOOLEAN );

		 private class ConstantAnonymousInnerClass2 : Constant
		 {
			 public ConstantAnonymousInnerClass2( UnknownType boolean ) : base( boolean, false )
			 {
			 }

			 internal override Expression not()
			 {
				  return TRUE;
			 }
		 }
		 public static readonly Expression Null = new Constant( OBJECT, null );

		 protected internal Expression( TypeReference type ) : base( type )
		 {
		 }

		 public abstract void Accept( ExpressionVisitor visitor );

		 internal static readonly Expression SUPER = new ExpressionAnonymousInnerClass( OBJECT );

		 private class ExpressionAnonymousInnerClass : Expression
		 {
			 public ExpressionAnonymousInnerClass( UnknownType @object ) : base( @object )
			 {
			 }

			 public override void accept( ExpressionVisitor visitor )
			 {
				  visitor.LoadThis( "super" );
			 }
		 }

		 public static readonly Expression EMPTY = new ExpressionAnonymousInnerClass2( VOID );

		 private class ExpressionAnonymousInnerClass2 : Expression
		 {
			 public ExpressionAnonymousInnerClass2( UnknownType @void ) : base( @void )
			 {
			 }

			 public override void accept( ExpressionVisitor visitor )
			 {
				  //do nothing
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Expression gt(final Expression lhs, final Expression rhs)
		 public static Expression Gt( Expression lhs, Expression rhs )
		 {
			  return new ExpressionAnonymousInnerClass3( BOOLEAN, lhs, rhs );
		 }

		 private class ExpressionAnonymousInnerClass3 : Expression
		 {
			 private Neo4Net.CodeGen.Expression _lhs;
			 private Neo4Net.CodeGen.Expression _rhs;

			 public ExpressionAnonymousInnerClass3( UnknownType boolean, Neo4Net.CodeGen.Expression lhs, Neo4Net.CodeGen.Expression rhs ) : base( boolean )
			 {
				 this._lhs = lhs;
				 this._rhs = rhs;
			 }

			 public override void accept( ExpressionVisitor visitor )
			 {
				  visitor.Gt( _lhs, _rhs );
			 }

			 internal override Expression not()
			 {
				  return Lte( _lhs, _rhs );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Expression gte(final Expression lhs, final Expression rhs)
		 public static Expression Gte( Expression lhs, Expression rhs )
		 {
			  return new ExpressionAnonymousInnerClass4( BOOLEAN, lhs, rhs );
		 }

		 private class ExpressionAnonymousInnerClass4 : Expression
		 {
			 private Neo4Net.CodeGen.Expression _lhs;
			 private Neo4Net.CodeGen.Expression _rhs;

			 public ExpressionAnonymousInnerClass4( UnknownType boolean, Neo4Net.CodeGen.Expression lhs, Neo4Net.CodeGen.Expression rhs ) : base( boolean )
			 {
				 this._lhs = lhs;
				 this._rhs = rhs;
			 }

			 public override void accept( ExpressionVisitor visitor )
			 {
				  visitor.Gte( _lhs, _rhs );
			 }

			 internal override Expression not()
			 {
				  return Lt( _lhs, _rhs );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Expression lt(final Expression lhs, final Expression rhs)
		 public static Expression Lt( Expression lhs, Expression rhs )
		 {
			  return new ExpressionAnonymousInnerClass5( BOOLEAN, lhs, rhs );
		 }

		 private class ExpressionAnonymousInnerClass5 : Expression
		 {
			 private Neo4Net.CodeGen.Expression _lhs;
			 private Neo4Net.CodeGen.Expression _rhs;

			 public ExpressionAnonymousInnerClass5( UnknownType boolean, Neo4Net.CodeGen.Expression lhs, Neo4Net.CodeGen.Expression rhs ) : base( boolean )
			 {
				 this._lhs = lhs;
				 this._rhs = rhs;
			 }

			 public override void accept( ExpressionVisitor visitor )
			 {
				  visitor.Lt( _lhs, _rhs );
			 }

			 internal override Expression not()
			 {
				  return Gte( _lhs, _rhs );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Expression lte(final Expression lhs, final Expression rhs)
		 public static Expression Lte( Expression lhs, Expression rhs )
		 {
			  return new ExpressionAnonymousInnerClass6( BOOLEAN, lhs, rhs );
		 }

		 private class ExpressionAnonymousInnerClass6 : Expression
		 {
			 private Neo4Net.CodeGen.Expression _lhs;
			 private Neo4Net.CodeGen.Expression _rhs;

			 public ExpressionAnonymousInnerClass6( UnknownType boolean, Neo4Net.CodeGen.Expression lhs, Neo4Net.CodeGen.Expression rhs ) : base( boolean )
			 {
				 this._lhs = lhs;
				 this._rhs = rhs;
			 }

			 public override void accept( ExpressionVisitor visitor )
			 {
				  visitor.Lte( _lhs, _rhs );
			 }

			 internal override Expression not()
			 {
				  return Gt( _lhs, _rhs );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Expression and(final Expression lhs, final Expression rhs)
		 public static Expression And( Expression lhs, Expression rhs )
		 {
			  if ( lhs == FALSE || rhs == FALSE )
			  {
					return FALSE;
			  }
			  if ( lhs == TRUE )
			  {
					return rhs;
			  }
			  if ( rhs == TRUE )
			  {
					return lhs;
			  }
			  Expression[] expressions;
			  if ( lhs is And )
			  {
					if ( rhs is And )
					{
						 expressions = expressions( ( ( And ) lhs ).Expressions, ( ( And ) rhs ).Expressions );
					}
					else
					{
						 expressions = expressions( ( ( And ) lhs ).Expressions, rhs );
					}
			  }
			  else if ( rhs is And )
			  {
					expressions = expressions( lhs, ( ( And ) rhs ).Expressions );
			  }
			  else
			  {
					expressions = new Expression[] { lhs, rhs };
			  }
			  return new And( expressions );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Expression or(final Expression lhs, final Expression rhs)
		 public static Expression Or( Expression lhs, Expression rhs )
		 {
			  if ( lhs == TRUE || rhs == TRUE )
			  {
					return TRUE;
			  }
			  if ( lhs == FALSE )
			  {
					return rhs;
			  }
			  if ( rhs == FALSE )
			  {
					return lhs;
			  }
			  Expression[] expressions;
			  if ( lhs is Or )
			  {
					if ( rhs is Or )
					{
						 expressions = expressions( ( ( Or ) lhs ).Expressions, ( ( Or ) rhs ).Expressions );
					}
					else
					{
						 expressions = expressions( ( ( Or ) lhs ).Expressions, rhs );
					}
			  }
			  else if ( rhs is Or )
			  {
					expressions = expressions( lhs, ( ( Or ) rhs ).Expressions );
			  }
			  else
			  {
					expressions = new Expression[] { lhs, rhs };
			  }
			  return new Or( expressions );
		 }

		 private class And : Expression
		 {
			  internal readonly Expression[] Expressions;

			  internal And( Expression[] expressions ) : base( BOOLEAN )
			  {
					this.Expressions = expressions;
			  }

			  public override void Accept( ExpressionVisitor visitor )
			  {
					visitor.And( Expressions );
			  }
		 }

		 private class Or : Expression
		 {
			  internal readonly Expression[] Expressions;

			  internal Or( Expression[] expressions ) : base( BOOLEAN )
			  {
					this.Expressions = expressions;
			  }

			  public override void Accept( ExpressionVisitor visitor )
			  {
					visitor.Or( Expressions );
			  }
		 }

		 private static Expression[] Expressions( Expression[] some, Expression[] more )
		 {
			  Expression[] result = Arrays.copyOf( some, some.Length + more.Length );
			  Array.Copy( more, 0, result, some.Length, more.Length );
			  return result;
		 }

		 private static Expression[] Expressions( Expression[] some, Expression last )
		 {
			  Expression[] result = Arrays.copyOf( some, some.Length + 1 );
			  result[some.Length] = last;
			  return result;
		 }

		 private static Expression[] Expressions( Expression first, Expression[] more )
		 {
			  Expression[] result = new Expression[more.Length + 1];
			  result[0] = first;
			  Array.Copy( more, 0, result, 1, more.Length );
			  return result;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Expression equal(final Expression lhs, final Expression rhs)
		 public static Expression Equal( Expression lhs, Expression rhs )
		 {
			  if ( lhs == Null )
			  {
					if ( rhs == Null )
					{
						 return Constant( true );
					}
					else
					{
						 return IsNull( rhs );
					}
			  }
			  else if ( rhs == Null )
			  {
					return IsNull( lhs );
			  }
			  return new ExpressionAnonymousInnerClass( BOOLEAN, lhs, rhs );
		 }

		 private class ExpressionAnonymousInnerClass : Expression
		 {
			 private Neo4Net.CodeGen.Expression _lhs;
			 private Neo4Net.CodeGen.Expression _rhs;

			 public ExpressionAnonymousInnerClass( UnknownType boolean, Neo4Net.CodeGen.Expression lhs, Neo4Net.CodeGen.Expression rhs ) : base( boolean )
			 {
				 this._lhs = lhs;
				 this._rhs = rhs;
			 }

			 public override void accept( ExpressionVisitor visitor )
			 {
				  visitor.Equal( _lhs, _rhs );
			 }

			 internal override Expression not()
			 {
				  return NotEqual( _lhs, _rhs );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Expression isNull(final Expression expression)
		 public static Expression IsNull( Expression expression )
		 {
			  return new ExpressionAnonymousInnerClass2( BOOLEAN, expression );
		 }

		 private class ExpressionAnonymousInnerClass2 : Expression
		 {
			 private Neo4Net.CodeGen.Expression _expression;

			 public ExpressionAnonymousInnerClass2( UnknownType boolean, Neo4Net.CodeGen.Expression expression ) : base( boolean )
			 {
				 this._expression = expression;
			 }

			 public override void accept( ExpressionVisitor visitor )
			 {
				  visitor.IsNull( _expression );
			 }

			 internal override Expression not()
			 {
				  return NotNull( _expression );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Expression notNull(final Expression expression)
		 public static Expression NotNull( Expression expression )
		 {
			  return new ExpressionAnonymousInnerClass3( BOOLEAN, expression );
		 }

		 private class ExpressionAnonymousInnerClass3 : Expression
		 {
			 private Neo4Net.CodeGen.Expression _expression;

			 public ExpressionAnonymousInnerClass3( UnknownType boolean, Neo4Net.CodeGen.Expression expression ) : base( boolean )
			 {
				 this._expression = expression;
			 }

			 public override void accept( ExpressionVisitor visitor )
			 {
				  visitor.NotNull( _expression );
			 }

			 internal override Expression not()
			 {
				  return IsNull( _expression );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Expression notEqual(final Expression lhs, final Expression rhs)
		 public static Expression NotEqual( Expression lhs, Expression rhs )
		 {
			  return new ExpressionAnonymousInnerClass4( BOOLEAN, lhs, rhs );
		 }

		 private class ExpressionAnonymousInnerClass4 : Expression
		 {
			 private Neo4Net.CodeGen.Expression _lhs;
			 private Neo4Net.CodeGen.Expression _rhs;

			 public ExpressionAnonymousInnerClass4( UnknownType boolean, Neo4Net.CodeGen.Expression lhs, Neo4Net.CodeGen.Expression rhs ) : base( boolean )
			 {
				 this._lhs = lhs;
				 this._rhs = rhs;
			 }

			 public override void accept( ExpressionVisitor visitor )
			 {
				  visitor.NotEqual( _lhs, _rhs );
			 }

			 internal override Expression not()
			 {
				  return Equal( _lhs, _rhs );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Expression load(final LocalVariable variable)
		 public static Expression Load( LocalVariable variable )
		 {
			  return new ExpressionAnonymousInnerClass5( variable.Type(), variable );
		 }

		 private class ExpressionAnonymousInnerClass5 : Expression
		 {
			 private Neo4Net.CodeGen.LocalVariable _variable;

			 public ExpressionAnonymousInnerClass5( Neo4Net.CodeGen.TypeReference type, Neo4Net.CodeGen.LocalVariable variable ) : base( type )
			 {
				 this._variable = variable;
			 }

			 public override void accept( ExpressionVisitor visitor )
			 {
				  visitor.Load( _variable );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Expression add(final Expression lhs, final Expression rhs)
		 public static Expression Add( Expression lhs, Expression rhs )
		 {
			  if ( !lhs.TypeConflict.Equals( rhs.TypeConflict ) )
			  {
					throw new System.ArgumentException( string.Format( "Cannot add variables with different types. LHS {0}, RHS {1}", lhs.TypeConflict.simpleName(), rhs.TypeConflict.simpleName() ) );
			  }

			  return new ExpressionAnonymousInnerClass6( lhs.TypeConflict, lhs, rhs );
		 }

		 private class ExpressionAnonymousInnerClass6 : Expression
		 {
			 private Neo4Net.CodeGen.Expression _lhs;
			 private Neo4Net.CodeGen.Expression _rhs;

			 public ExpressionAnonymousInnerClass6( Neo4Net.CodeGen.TypeReference type, Neo4Net.CodeGen.Expression lhs, Neo4Net.CodeGen.Expression rhs ) : base( type )
			 {
				 this._lhs = lhs;
				 this._rhs = rhs;
			 }

			 public override void accept( ExpressionVisitor visitor )
			 {
				  visitor.Add( _lhs, _rhs );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Expression subtract(final Expression lhs, final Expression rhs)
		 public static Expression Subtract( Expression lhs, Expression rhs )
		 {
			  if ( !lhs.TypeConflict.Equals( rhs.TypeConflict ) )
			  {
					throw new System.ArgumentException( string.Format( "Cannot subtract variables with different types. LHS {0}, RHS {1}", lhs.TypeConflict.simpleName(), rhs.TypeConflict.simpleName() ) );
			  }
			  return new ExpressionAnonymousInnerClass7( lhs.TypeConflict, lhs, rhs );
		 }

		 private class ExpressionAnonymousInnerClass7 : Expression
		 {
			 private Neo4Net.CodeGen.Expression _lhs;
			 private Neo4Net.CodeGen.Expression _rhs;

			 public ExpressionAnonymousInnerClass7( Neo4Net.CodeGen.TypeReference type, Neo4Net.CodeGen.Expression lhs, Neo4Net.CodeGen.Expression rhs ) : base( type )
			 {
				 this._lhs = lhs;
				 this._rhs = rhs;
			 }

			 public override void accept( ExpressionVisitor visitor )
			 {
				  visitor.Subtract( _lhs, _rhs );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Expression multiply(final Expression lhs, final Expression rhs)
		 public static Expression Multiply( Expression lhs, Expression rhs )
		 {
			  if ( !lhs.TypeConflict.Equals( rhs.TypeConflict ) )
			  {
					throw new System.ArgumentException( string.Format( "Cannot multiply variables with different types. LHS {0}, RHS {1}", lhs.TypeConflict.simpleName(), rhs.TypeConflict.simpleName() ) );
			  }
			  return new ExpressionAnonymousInnerClass8( lhs.TypeConflict, lhs, rhs );
		 }

		 private class ExpressionAnonymousInnerClass8 : Expression
		 {
			 private Neo4Net.CodeGen.Expression _lhs;
			 private Neo4Net.CodeGen.Expression _rhs;

			 public ExpressionAnonymousInnerClass8( Neo4Net.CodeGen.TypeReference type, Neo4Net.CodeGen.Expression lhs, Neo4Net.CodeGen.Expression rhs ) : base( type )
			 {
				 this._lhs = lhs;
				 this._rhs = rhs;
			 }

			 public override void accept( ExpressionVisitor visitor )
			 {
				  visitor.Multiply( _lhs, _rhs );
			 }
		 }

		 public static Expression ConstantInt( int value )
		 {
			  return Constant( value );
		 }

		 public static Expression ConstantLong( long value )
		 {
			  return Constant( value );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Expression constant(final Object value)
		 public static Expression Constant( object value )
		 {
			  TypeReference reference;
			  if ( value == null )
			  {
					return Null;
			  }
			  else if ( value is string )
			  {
					reference = TypeReference.TypeReferenceConflict( typeof( string ) );
			  }
			  else if ( value is long? )
			  {
					reference = LONG;
			  }
			  else if ( value is int? )
			  {
					reference = INT;
			  }
			  else if ( value is double? )
			  {
					reference = DOUBLE;
			  }
			  else if ( value is bool? )
			  {
					return ( bool? ) value ? TRUE : FALSE;
			  }
			  else if ( value is AnyValue )
			  {
					reference = VALUE;
			  }
			  else
			  {
					throw new System.ArgumentException( "Not a valid constant: " + value );
			  }

			  return new ExpressionAnonymousInnerClass9( reference, value );
		 }

		 private class ExpressionAnonymousInnerClass9 : Expression
		 {
			 private object _value;

			 public ExpressionAnonymousInnerClass9( Neo4Net.CodeGen.TypeReference reference, object value ) : base( reference )
			 {
				 this._value = value;
			 }

			 public override void accept( ExpressionVisitor visitor )
			 {
				  visitor.Constant( _value );
			 }
		 }

		 private class Constant : Expression
		 {
			  internal readonly object Value;

			  internal Constant( TypeReference type, object value ) : base( type )
			  {
					this.Value = value;
			  }

			  public override void Accept( ExpressionVisitor visitor )
			  {
					visitor.Constant( Value );
			  }
		 }

		 //TODO deduce type from constants
		 public static Expression NewArray( TypeReference baseType, params Expression[] constants )
		 {
			  return new ExpressionAnonymousInnerClass( arrayOf( baseType ), baseType, constants );
		 }

		 private class ExpressionAnonymousInnerClass : Expression
		 {
			 private Neo4Net.CodeGen.TypeReference _baseType;
			 private Neo4Net.CodeGen.Expression[] _constants;

			 public ExpressionAnonymousInnerClass( UnknownType arrayOf, Neo4Net.CodeGen.TypeReference baseType, Neo4Net.CodeGen.Expression[] constants ) : base( arrayOf )
			 {
				 this._baseType = baseType;
				 this._constants = constants;
			 }

			 public override void accept( ExpressionVisitor visitor )
			 {
				  visitor.NewArray( _baseType, _constants );
			 }
		 }

		 /// <summary>
		 /// get instance field </summary>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Expression get(final Expression target, final FieldReference field)
		 public static Expression Get( Expression target, FieldReference field )
		 {
			  return new ExpressionAnonymousInnerClass2( field.Type(), target, field );
		 }

		 private class ExpressionAnonymousInnerClass2 : Expression
		 {
			 private Neo4Net.CodeGen.Expression _target;
			 private Neo4Net.CodeGen.FieldReference _field;

			 public ExpressionAnonymousInnerClass2( Neo4Net.CodeGen.TypeReference type, Neo4Net.CodeGen.Expression target, Neo4Net.CodeGen.FieldReference field ) : base( type )
			 {
				 this._target = target;
				 this._field = field;
			 }

			 public override void accept( ExpressionVisitor visitor )
			 {
				  visitor.GetField( _target, _field );
			 }
		 }

		 /// <summary>
		 /// box expression </summary>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Expression box(final Expression expression)
		 public static Expression Box( Expression expression )
		 {
			  TypeReference type = expression.TypeConflict;
			  if ( type.Primitive )
			  {
					switch ( type.Name() )
					{
					case "byte":
						 type = TypeReference.TypeReferenceConflict( typeof( Byte ) );
						 break;
					case "short":
						 type = TypeReference.TypeReferenceConflict( typeof( Short ) );
						 break;
					case "int":
						 type = TypeReference.TypeReferenceConflict( typeof( Integer ) );
						 break;
					case "long":
						 type = TypeReference.TypeReferenceConflict( typeof( Long ) );
						 break;
					case "char":
						 type = TypeReference.TypeReferenceConflict( typeof( Character ) );
						 break;
					case "boolean":
						 type = TypeReference.TypeReferenceConflict( typeof( Boolean ) );
						 break;
					case "float":
						 type = TypeReference.TypeReferenceConflict( typeof( Float ) );
						 break;
					case "double":
						 type = TypeReference.TypeReferenceConflict( typeof( Double ) );
						 break;
					default:
						 break;
					}
			  }
			  return new ExpressionAnonymousInnerClass3( type, expression );
		 }

		 private class ExpressionAnonymousInnerClass3 : Expression
		 {
			 private Neo4Net.CodeGen.Expression _expression;

			 public ExpressionAnonymousInnerClass3( Neo4Net.CodeGen.TypeReference type, Neo4Net.CodeGen.Expression expression ) : base( type )
			 {
				 this._expression = expression;
			 }

			 public override void accept( ExpressionVisitor visitor )
			 {
				  visitor.Box( _expression );
			 }
		 }

		 /// <summary>
		 /// unbox expression </summary>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Expression unbox(final Expression expression)
		 public static Expression Unbox( Expression expression )
		 {
			  TypeReference type;
			  switch ( expression.TypeConflict.fullName() )
			  {
			  case "java.lang.Byte":
					type = TypeReference.TypeReferenceConflict( typeof( sbyte ) );
					break;
			  case "java.lang.Short":
					type = TypeReference.TypeReferenceConflict( typeof( short ) );
					break;
			  case "java.lang.Integer":
					type = TypeReference.TypeReferenceConflict( typeof( int ) );
					break;
			  case "java.lang.Long":
					type = TypeReference.TypeReferenceConflict( typeof( long ) );
					break;
			  case "java.lang.Character":
					type = TypeReference.TypeReferenceConflict( typeof( char ) );
					break;
			  case "java.lang.Boolean":
					type = TypeReference.TypeReferenceConflict( typeof( bool ) );
					break;
			  case "java.lang.Float":
					type = TypeReference.TypeReferenceConflict( typeof( float ) );
					break;
			  case "java.lang.Double":
					type = TypeReference.TypeReferenceConflict( typeof( double ) );
					break;
			  default:
					throw new System.InvalidOperationException( "Cannot unbox " + expression.TypeConflict.fullName() );
			  }
			  return new ExpressionAnonymousInnerClass4( type, expression );
		 }

		 private class ExpressionAnonymousInnerClass4 : Expression
		 {
			 private Neo4Net.CodeGen.Expression _expression;

			 public ExpressionAnonymousInnerClass4( Neo4Net.CodeGen.TypeReference type, Neo4Net.CodeGen.Expression expression ) : base( type )
			 {
				 this._expression = expression;
			 }

			 public override void accept( ExpressionVisitor visitor )
			 {
				  visitor.Unbox( _expression );
			 }
		 }

		 /// <summary>
		 /// get static field </summary>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Expression getStatic(final FieldReference field)
		 public static Expression GetStatic( FieldReference field )
		 {
			  return new ExpressionAnonymousInnerClass5( field.Type(), field );
		 }

		 private class ExpressionAnonymousInnerClass5 : Expression
		 {
			 private Neo4Net.CodeGen.FieldReference _field;

			 public ExpressionAnonymousInnerClass5( Neo4Net.CodeGen.TypeReference type, Neo4Net.CodeGen.FieldReference field ) : base( type )
			 {
				 this._field = field;
			 }

			 public override void accept( ExpressionVisitor visitor )
			 {
				  visitor.GetStatic( _field );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Expression ternary(final Expression test, final Expression onTrue, final Expression onFalse)
		 public static Expression Ternary( Expression test, Expression onTrue, Expression onFalse )
		 {
			  TypeReference reference = onTrue.TypeConflict.Equals( onFalse.TypeConflict ) ? onTrue.TypeConflict : OBJECT;
			  return new ExpressionAnonymousInnerClass6( reference, test, onTrue, onFalse );
		 }

		 private class ExpressionAnonymousInnerClass6 : Expression
		 {
			 private Neo4Net.CodeGen.Expression _test;
			 private Neo4Net.CodeGen.Expression _onTrue;
			 private Neo4Net.CodeGen.Expression _onFalse;

			 public ExpressionAnonymousInnerClass6( Neo4Net.CodeGen.TypeReference reference, Neo4Net.CodeGen.Expression test, Neo4Net.CodeGen.Expression onTrue, Neo4Net.CodeGen.Expression onFalse ) : base( reference )
			 {
				 this._test = test;
				 this._onTrue = onTrue;
				 this._onFalse = onFalse;
			 }

			 public override void accept( ExpressionVisitor visitor )
			 {
				  visitor.Ternary( _test, _onTrue, _onFalse );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Expression invoke(final Expression target, final MethodReference method, final Expression... arguments)
		 public static Expression Invoke( Expression target, MethodReference method, params Expression[] arguments )
		 {
			  return new ExpressionAnonymousInnerClass7( method.Returns(), target, method, arguments );
		 }

		 private class ExpressionAnonymousInnerClass7 : Expression
		 {
			 private Neo4Net.CodeGen.Expression _target;
			 private Neo4Net.CodeGen.MethodReference _method;
			 private Neo4Net.CodeGen.Expression[] _arguments;

			 public ExpressionAnonymousInnerClass7( Neo4Net.CodeGen.TypeReference returns, Neo4Net.CodeGen.Expression target, Neo4Net.CodeGen.MethodReference method, Neo4Net.CodeGen.Expression[] arguments ) : base( returns )
			 {
				 this._target = target;
				 this._method = method;
				 this._arguments = arguments;
			 }

			 public override void accept( ExpressionVisitor visitor )
			 {
				  visitor.Invoke( _target, _method, _arguments );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Expression invoke(final MethodReference method, final Expression... parameters)
		 public static Expression Invoke( MethodReference method, params Expression[] parameters )
		 {
			  return new ExpressionAnonymousInnerClass8( method.Returns(), method, parameters );
		 }

		 private class ExpressionAnonymousInnerClass8 : Expression
		 {
			 private Neo4Net.CodeGen.MethodReference _method;
			 private Neo4Net.CodeGen.Expression[] _parameters;

			 public ExpressionAnonymousInnerClass8( Neo4Net.CodeGen.TypeReference returns, Neo4Net.CodeGen.MethodReference method, Neo4Net.CodeGen.Expression[] parameters ) : base( returns )
			 {
				 this._method = method;
				 this._parameters = parameters;
			 }

			 public override void accept( ExpressionVisitor visitor )
			 {
				  visitor.Invoke( _method, _parameters );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Expression invokeSuper(TypeReference parent, final Expression... parameters)
		 public static Expression InvokeSuper( TypeReference parent, params Expression[] parameters )
		 {
			  TypeReference[] parameterTypes = new TypeReference[parameters.Length];
			  for ( int i = 0; i < parameters.Length; i++ )
			  {
					parameterTypes[i] = parameters[i].Type();
			  }

			  return new ExpressionAnonymousInnerClass9( OBJECT, parent, parameters, parameterTypes );
		 }

		 private class ExpressionAnonymousInnerClass9 : Expression
		 {
			 private Neo4Net.CodeGen.TypeReference _parent;
			 private Neo4Net.CodeGen.Expression[] _parameters;
			 private Neo4Net.CodeGen.TypeReference[] _parameterTypes;

			 public ExpressionAnonymousInnerClass9( UnknownType @object, Neo4Net.CodeGen.TypeReference parent, Neo4Net.CodeGen.Expression[] parameters, Neo4Net.CodeGen.TypeReference[] parameterTypes ) : base( @object )
			 {
				 this._parent = parent;
				 this._parameters = parameters;
				 this._parameterTypes = parameterTypes;
			 }

			 public override void accept( ExpressionVisitor visitor )
			 {
				  visitor.Invoke( Expression.SUPER, new MethodReference( _parent, "<init>", VOID, Modifier.PUBLIC, _parameterTypes ), _parameters );
			 }
		 }

		 public static Expression Cast( Type type, Expression expression )
		 {
			  return Cast( typeReference( type ), expression );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Expression instanceOf(final TypeReference typeToCheck, Expression expression)
		 public static Expression InstanceOf( TypeReference typeToCheck, Expression expression )
		 {
			  return new ExpressionAnonymousInnerClass10( typeReference( typeof( bool ) ), typeToCheck, expression );
		 }

		 private class ExpressionAnonymousInnerClass10 : Expression
		 {
			 private Neo4Net.CodeGen.TypeReference _typeToCheck;
			 private Neo4Net.CodeGen.Expression _expression;

			 public ExpressionAnonymousInnerClass10( UnknownType typeReference, Neo4Net.CodeGen.TypeReference typeToCheck, Neo4Net.CodeGen.Expression expression ) : base( typeReference )
			 {
				 this._typeToCheck = typeToCheck;
				 this._expression = expression;
			 }

			 public override void accept( ExpressionVisitor visitor )
			 {
				  visitor.InstanceOf( _typeToCheck, _expression );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Expression cast(final TypeReference type, Expression expression)
		 public static Expression Cast( TypeReference type, Expression expression )
		 {
			  return new ExpressionAnonymousInnerClass11( type, expression );
		 }

		 private class ExpressionAnonymousInnerClass11 : Expression
		 {
			 private new Neo4Net.CodeGen.TypeReference _type;
			 private Neo4Net.CodeGen.Expression _expression;

			 public ExpressionAnonymousInnerClass11( Neo4Net.CodeGen.TypeReference type, Neo4Net.CodeGen.Expression expression ) : base( type )
			 {
				 this._type = type;
				 this._expression = expression;
			 }

			 public override void accept( ExpressionVisitor visitor )
			 {
				  visitor.Cast( _type, _expression );
			 }
		 }

		 public static Expression NewInstance( Type type )
		 {
			  return NewInstance( typeReference( type ) );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Expression newInstance(final TypeReference type)
		 public static Expression NewInstance( TypeReference type )
		 {
			  return new ExpressionAnonymousInnerClass12( type );
		 }

		 private class ExpressionAnonymousInnerClass12 : Expression
		 {
			 private new Neo4Net.CodeGen.TypeReference _type;

			 public ExpressionAnonymousInnerClass12( Neo4Net.CodeGen.TypeReference type ) : base( type )
			 {
				 this._type = type;
			 }

			 public override void accept( ExpressionVisitor visitor )
			 {
				  visitor.NewInstance( _type );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Expression not(final Expression expression)
		 public static Expression Not( Expression expression )
		 {
			  return expression.Not();
		 }

		 internal virtual Expression Not()
		 {
			  return NotExpr( this );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static Expression notExpr(final Expression expression)
		 private static Expression NotExpr( Expression expression )
		 {
			  Debug.Assert( expression.TypeConflict == BOOLEAN, "Can only apply not() to boolean expressions" );
			  return new ExpressionAnonymousInnerClass13( BOOLEAN, expression );
		 }

		 private class ExpressionAnonymousInnerClass13 : Expression
		 {
			 private Neo4Net.CodeGen.Expression _expression;

			 public ExpressionAnonymousInnerClass13( UnknownType boolean, Neo4Net.CodeGen.Expression expression ) : base( boolean )
			 {
				 this._expression = expression;
			 }

			 public override void accept( ExpressionVisitor visitor )
			 {
				  visitor.Not( _expression );
			 }

			 internal override Expression not()
			 {
				  return _expression;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Expression toDouble(final Expression expression)
		 public static Expression ToDouble( Expression expression )
		 {
			  return new ExpressionAnonymousInnerClass14( DOUBLE, expression );
		 }

		 private class ExpressionAnonymousInnerClass14 : Expression
		 {
			 private Neo4Net.CodeGen.Expression _expression;

			 public ExpressionAnonymousInnerClass14( UnknownType @double, Neo4Net.CodeGen.Expression expression ) : base( @double )
			 {
				 this._expression = expression;
			 }

			 public override void accept( ExpressionVisitor visitor )
			 {
				  visitor.LongToDouble( _expression );
			 }
		 }

		 public static Expression Pop( Expression expression )
		 {
			  return new ExpressionAnonymousInnerClass15( expression.TypeConflict, expression );
		 }

		 private class ExpressionAnonymousInnerClass15 : Expression
		 {
			 private Neo4Net.CodeGen.Expression _expression;

			 public ExpressionAnonymousInnerClass15( Neo4Net.CodeGen.TypeReference type, Neo4Net.CodeGen.Expression expression ) : base( type )
			 {
				 this._expression = expression;
			 }

			 public override void accept( ExpressionVisitor visitor )
			 {
				  visitor.Pop( _expression );
			 }
		 }

		 internal override Expression Materialize( CodeBlock method )
		 {
			  return this;
		 }

		 internal override void TemplateAccept( CodeBlock method, ExpressionVisitor visitor )
		 {
			  throw new System.NotSupportedException( "simple expressions should not be invoked as templates" );
		 }

		 public override string ToString()
		 {
			  StringBuilder result = ( new StringBuilder() ).Append("Expression[");
			  Accept( new ExpressionToString( result ) );
			  return result.Append( ']' ).ToString();
		 }
	}

}