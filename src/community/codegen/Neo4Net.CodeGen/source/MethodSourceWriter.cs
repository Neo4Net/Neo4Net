using System.Collections.Generic;
using System.Text;
using System.Threading;

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
namespace Neo4Net.CodeGen.source
{
	using StringEscapeUtils = org.apache.commons.text.StringEscapeUtils;



	internal class MethodSourceWriter : MethodEmitter, ExpressionVisitor
	{
		 private static readonly ThreadStart _bottom = () =>
		 {
		 throw new System.InvalidOperationException( "Popped too many levels!" );
		 };
		 private static readonly ThreadStart _level = () =>
		 {
		 };
		 private const string INDENTATION = "    ";
		 private readonly StringBuilder _target;
		 private readonly ClassSourceWriter _classSourceWriter;
		 private readonly Deque<ThreadStart> _levels = new LinkedList<ThreadStart>();

		 internal MethodSourceWriter( StringBuilder target, ClassSourceWriter classSourceWriter )
		 {
			  this._target = target;
			  this._classSourceWriter = classSourceWriter;
			  this._levels.push( _bottom );
			  this._levels.push( _level );
		 }

		 private StringBuilder Indent()
		 {
			  for ( int level = this._levels.size(); level-- > 0; )
			  {
					_target.Append( INDENTATION );
			  }
			  return _target;
		 }

		 private StringBuilder Append( CharSequence text )
		 {
			  return _target.Append( text );
		 }

		 public override void Done()
		 {
			  if ( _levels.size() != 1 )
			  {
					throw new System.InvalidOperationException( "unbalanced blocks!" );
			  }
			  _classSourceWriter.append( _target );
		 }

		 public override void Expression( Expression expression )
		 {
			  Indent();
			  expression.Accept( this );
			  _target.Append( ";\n" );
		 }

		 public override void Put( Expression target, FieldReference field, Expression value )
		 {
			  Indent();
			  target.Accept( this );
			  Append( "." );
			  Append( field.Name() );
			  Append( " = " );
			  value.Accept( this );
			  Append( ";\n" );
		 }

		 public override void Returns()
		 {
			  Indent().Append("return;\n");
		 }

		 public override void Returns( Expression value )
		 {
			  Indent().Append("return ");
			  value.Accept( this );
			  Append( ";\n" );
		 }

		 public override void Continues()
		 {
			  Indent().Append("continue;\n");
		 }

		 public override void Declare( LocalVariable local )
		 {
			  Indent().Append(local.Type().fullName()).Append(' ').Append(local.Name()).Append(";\n");
		 }

		 public override void AssignVariableInScope( LocalVariable local, Expression value )
		 {
			  Indent().Append(local.Name()).Append(" = ");
			  value.Accept( this );
			  Append( ";\n" );
		 }

		 public override void Assign( LocalVariable variable, Expression value )
		 {
			  Indent().Append(variable.Type().fullName()).Append(' ').Append(variable.Name()).Append(" = ");
			  value.Accept( this );
			  Append( ";\n" );
		 }

		 public override void BeginWhile( Expression test )
		 {
			  Indent().Append("while( ");
			  test.Accept( this );
			  Append( " )\n" );
			  Indent().Append("{\n");
			  _levels.push( _level );
		 }

		 public override void BeginIf( Expression test )
		 {
			  Indent().Append("if ( ");
			  test.Accept( this );
			  Append( " )\n" );
			  Indent().Append("{\n");
			  _levels.push( _level );
		 }

		 public override void BeginBlock()
		 {
			  Indent().Append("{\n");
			  _levels.push( _level );
		 }

		 public override void TryCatchBlock<T>( System.Action<T> body, System.Action<T> handler, LocalVariable exception, T block )
		 {

			  Indent().Append("try\n");
			  Indent().Append("{\n");
			  _levels.push( _level );
			  body( block );
			  _levels.pop();
			  Indent().Append("}\n");
			  Indent().Append("catch ( ").Append(exception.Type().fullName()).Append(" ").Append(exception.Name()).Append(" )\n");
			  Indent().Append("{\n");
			  _levels.push( _level );
			  handler( block );
			  _levels.pop();
			  Indent().Append("}\n");
		 }

		 public override void ThrowException( Expression exception )
		 {
			  Indent().Append("throw ");
			  exception.Accept( this );
			  Append( ";\n" );
		 }

		 public override void EndBlock()
		 {
			  ThreadStart action = _levels.pop();
			  Indent().Append("}\n");
			  action.run();
		 }

		 public override void Invoke( Expression target, MethodReference method, Expression[] arguments )
		 {
			  target.Accept( this );
			  if ( !method.Constructor )
			  {
					Append( "." ).Append( method.Name() );
			  }
			  Arglist( arguments );
		 }

		 public override void Invoke( MethodReference method, Expression[] arguments )
		 {
			  Append( method.Owner().fullName() ).Append('.').Append(method.Name());
			  Arglist( arguments );
		 }

		 private void Arglist( Expression[] arguments )
		 {
			  Append( "(" );
			  string sep = " ";
			  foreach ( Expression argument in arguments )
			  {
					Append( sep );
					argument.Accept( this );
					sep = ", ";
			  }
			  if ( sep.Length > 1 )
			  {
					Append( " " );
			  }
			  Append( ")" );
		 }

		 public override void Load( LocalVariable variable )
		 {
			  Append( variable.Name() );
		 }

		 public override void GetField( Expression target, FieldReference field )
		 {
			  target.Accept( this );
			  Append( "." ).Append( field.Name() );
		 }

		 public override void Constant( object value )
		 {
			  if ( value == null )
			  {
					Append( "null" );
			  }
			  else if ( value is string )
			  {
					Append( "\"" ).Append( StringEscapeUtils.escapeJava( ( string ) value ) ).Append( '"' );
			  }
			  else if ( value is int? )
			  {
					Append( value.ToString() );
			  }
			  else if ( value is long? )
			  {
					Append( value.ToString() ).Append('L');
			  }
			  else if ( value is double? )
			  {
					double? doubleValue = ( double? ) value;
					if ( double.IsNaN( doubleValue ) )
					{
						 Append( "Double.NaN" );
					}
					else if ( doubleValue == double.PositiveInfinity )
					{
						 Append( "Double.POSITIVE_INFINITY" );
					}
					else if ( doubleValue == double.NegativeInfinity )
					{
						 Append( "Double.NEGATIVE_INFINITY" );
					}
					else
					{
						 Append( value.ToString() );
					}
			  }
			  else if ( value is bool? )
			  {
					Append( value.ToString() );
			  }
			  else
			  {
					throw new System.NotSupportedException( value.GetType() + " constants" );
			  }
		 }

		 public override void GetStatic( FieldReference field )
		 {
			  Append( field.Owner().fullName() ).Append(".").Append(field.Name());
		 }

		 public override void LoadThis( string sourceName )
		 {
			  Append( sourceName );
		 }

		 public override void NewInstance( TypeReference type )
		 {
			  Append( "new " ).Append( type.FullName() );
		 }

		 public override void Not( Expression expression )
		 {
			  Append( "!( " );
			  expression.Accept( this );
			  Append( " )" );
		 }

		 public override void Ternary( Expression test, Expression onTrue, Expression onFalse )
		 {
			  Append( "((" );
			  test.Accept( this );
			  Append( ") ? (" );
			  onTrue.Accept( this );
			  Append( ") : (" );
			  onFalse.Accept( this );
			  Append( "))" );
		 }

		 public override void Equal( Expression lhs, Expression rhs )
		 {
			  BinaryOperation( lhs, rhs, " == " );
		 }

		 public override void NotEqual( Expression lhs, Expression rhs )
		 {
			  BinaryOperation( lhs, rhs, " != " );
		 }

		 public override void IsNull( Expression expression )
		 {
			  expression.Accept( this );
			  Append( " == null" );
		 }

		 public override void NotNull( Expression expression )
		 {
			  expression.Accept( this );
			  Append( " != null" );
		 }

		 public override void Or( params Expression[] expressions )
		 {
			  BoolOp( expressions, " || " );
		 }

		 public override void And( params Expression[] expressions )
		 {
			  BoolOp( expressions, " && " );
		 }

		 private void BoolOp( Expression[] expressions, string op )
		 {
			  string sep = "";
			  foreach ( Expression expression in expressions )
			  {
					Append( sep );
					expression.Accept( this );
					sep = op;
			  }
		 }

		 public override void Add( Expression lhs, Expression rhs )
		 {
			  BinaryOperation( lhs, rhs, " + " );
		 }

		 public override void Gt( Expression lhs, Expression rhs )
		 {
			  BinaryOperation( lhs, rhs, " > " );
		 }

		 public override void Gte( Expression lhs, Expression rhs )
		 {
			  BinaryOperation( lhs, rhs, " >= " );
		 }

		 public override void Lt( Expression lhs, Expression rhs )
		 {
			  BinaryOperation( lhs, rhs, " < " );
		 }

		 public override void Lte( Expression lhs, Expression rhs )
		 {
			  BinaryOperation( lhs, rhs, " <= " );
		 }

		 public override void Subtract( Expression lhs, Expression rhs )
		 {
			  lhs.Accept( this );
			  Append( " - " );
			  rhs.Accept( this );
		 }

		 public override void Multiply( Expression lhs, Expression rhs )
		 {
			  lhs.Accept( this );
			  Append( " * " );
			  rhs.Accept( this );
		 }

		 private void Div( Expression lhs, Expression rhs )
		 {
			  lhs.Accept( this );
			  Append( " / " );
			  rhs.Accept( this );
		 }

		 public override void Cast( TypeReference type, Expression expression )
		 {
			  Append( "(" );
			  Append( "(" ).Append( type.FullName() ).Append(") ");
			  expression.Accept( this );
			  Append( ")" );
		 }

		 public override void InstanceOf( TypeReference type, Expression expression )
		 {
			  expression.Accept( this );
			  Append( " instanceof " ).Append( type.FullName() );
		 }

		 public override void NewArray( TypeReference type, params Expression[] constants )
		 {
			  Append( "new " ).Append( type.FullName() ).Append("[]{");
			  string sep = "";
			  foreach ( Expression constant in constants )
			  {
					Append( sep );
					constant.Accept( this );
					sep = ", ";
			  }
			  Append( "}" );
		 }

		 public override void LongToDouble( Expression expression )
		 {
			  Cast( TypeReference.typeReference( typeof( double ) ), expression );
		 }

		 public override void Pop( Expression expression )
		 {
			  expression.Accept( this );
		 }

		 public override void Box( Expression expression )
		 {
			  //For source code we rely on autoboxing
			  Append( "(/*box*/ " );
			  expression.Accept( this );
			  Append( ")" );
		 }

		 public override void Unbox( Expression expression )
		 {
			  //For source code we rely on autoboxing
			  expression.Accept( this );
		 }

		 private void BinaryOperation( Expression lhs, Expression rhs, string @operator )
		 {
			  lhs.Accept( this );
			  Append( @operator );
			  rhs.Accept( this );
		 }
	}

}