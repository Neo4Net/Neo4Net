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
	internal class ExpressionToString : ExpressionVisitor
	{
		 private readonly StringBuilder _result;

		 internal ExpressionToString( StringBuilder result )
		 {
			  this._result = result;
		 }

		 public override void Invoke( Expression target, MethodReference method, Expression[] arguments )
		 {
			  _result.Append( "invoke{target=" );
			  target.Accept( this );
			  _result.Append( ", method=" );
			  method.WriteTo( _result );
			  _result.Append( "}(" );
			  string sep = "";
			  foreach ( Expression argument in arguments )
			  {
					_result.Append( sep );
					argument.Accept( this );
					sep = ", ";
			  }
			  _result.Append( ")" );
		 }

		 public override void Invoke( MethodReference method, Expression[] arguments )
		 {
			  _result.Append( "invoke{method=" );
			  method.WriteTo( _result );
			  _result.Append( "}(" );
			  string sep = "";
			  foreach ( Expression argument in arguments )
			  {
					_result.Append( sep );
					argument.Accept( this );
					sep = ", ";
			  }
			  _result.Append( ")" );
		 }

		 public override void Load( LocalVariable variable )
		 {
			  _result.Append( "load{type=" );
			  if ( variable.Type() == null )
			  {
					_result.Append( "null" );
			  }
			  else
			  {
					variable.Type().writeTo(_result);
			  }
			  _result.Append( ", name=" ).Append( variable.Name() ).Append("}");
		 }

		 public override void GetField( Expression target, FieldReference field )
		 {
			  _result.Append( "get{target=" );
			  target.Accept( this );
			  _result.Append( ", field=" ).Append( field.Name() ).Append("}");
		 }

		 public override void Constant( object value )
		 {
			  _result.Append( "constant(" ).Append( value ).Append( ")" );
		 }

		 public override void GetStatic( FieldReference field )
		 {
			  _result.Append( "get{class=" ).Append( field.Owner() );
			  _result.Append( ", field=" ).Append( field.Name() ).Append("}");
		 }

		 public override void LoadThis( string sourceName )
		 {
			  _result.Append( "load{" ).Append( sourceName ).Append( "}" );
		 }

		 public override void NewInstance( TypeReference type )
		 {
			  _result.Append( "new{type=" );
			  type.WriteTo( _result );
			  _result.Append( "}" );
		 }

		 public override void Not( Expression expression )
		 {
			  _result.Append( "not(" );
			  expression.Accept( this );
			  _result.Append( ")" );
		 }

		 public override void Ternary( Expression test, Expression onTrue, Expression onFalse )
		 {
			  _result.Append( "ternary{test=" );
			  test.Accept( this );
			  _result.Append( ", onTrue=" );
			  onTrue.Accept( this );
			  _result.Append( ", onFalse=" );
			  onFalse.Accept( this );
			  _result.Append( "}" );
		 }

		 public override void Equal( Expression lhs, Expression rhs )
		 {
			  _result.Append( "equal(" );
			  lhs.Accept( this );
			  _result.Append( ", " );
			  rhs.Accept( this );
			  _result.Append( ")" );
		 }

		 public override void NotEqual( Expression lhs, Expression rhs )
		 {
			  _result.Append( "notEqual(" );
			  lhs.Accept( this );
			  _result.Append( ", " );
			  rhs.Accept( this );
			  _result.Append( ")" );
		 }

		 public override void IsNull( Expression expression )
		 {
			  _result.Append( "isNull(" );
			  expression.Accept( this );
			  _result.Append( ")" );
		 }

		 public override void NotNull( Expression expression )
		 {
			  _result.Append( "notNull(" );
			  expression.Accept( this );
			  _result.Append( ")" );
		 }

		 public override void Or( params Expression[] expressions )
		 {
			  BoolOp( "or(", expressions );
		 }

		 public override void And( params Expression[] expressions )
		 {
			  BoolOp( "and(", expressions );
		 }

		 private void BoolOp( string sep, Expression[] expressions )
		 {
			  foreach ( Expression expression in expressions )
			  {
					_result.Append( sep );
					expression.Accept( this );
					sep = ", ";
			  }
			  _result.Append( ")" );
		 }

		 public override void Add( Expression lhs, Expression rhs )
		 {
			  _result.Append( "add(" );
			  lhs.Accept( this );
			  _result.Append( " + " );
			  rhs.Accept( this );
			  _result.Append( ")" );
		 }

		 public override void Gt( Expression lhs, Expression rhs )
		 {
			  _result.Append( "gt(" );
			  lhs.Accept( this );
			  _result.Append( " > " );
			  rhs.Accept( this );
			  _result.Append( ")" );
		 }

		 public override void Gte( Expression lhs, Expression rhs )
		 {
			  _result.Append( "gt(" );
			  lhs.Accept( this );
			  _result.Append( " >= " );
			  rhs.Accept( this );
			  _result.Append( ")" );
		 }

		 public override void Lt( Expression lhs, Expression rhs )
		 {
			  _result.Append( "lt(" );
			  lhs.Accept( this );
			  _result.Append( " < " );
			  rhs.Accept( this );
			  _result.Append( ")" );
		 }

		 public override void Lte( Expression lhs, Expression rhs )
		 {
			  _result.Append( "gt(" );
			  lhs.Accept( this );
			  _result.Append( " <= " );
			  rhs.Accept( this );
			  _result.Append( ")" );
		 }

		 public override void Subtract( Expression lhs, Expression rhs )
		 {
			  _result.Append( "sub(" );
			  lhs.Accept( this );
			  _result.Append( " - " );
			  rhs.Accept( this );
			  _result.Append( ")" );
		 }

		 public override void Multiply( Expression lhs, Expression rhs )
		 {
			  _result.Append( "mul(" );
			  lhs.Accept( this );
			  _result.Append( " * " );
			  rhs.Accept( this );
			  _result.Append( ")" );
		 }

		 private void Div( Expression lhs, Expression rhs )
		 {
			  _result.Append( "div(" );
			  lhs.Accept( this );
			  _result.Append( " / " );
			  rhs.Accept( this );
			  _result.Append( ")" );
		 }

		 public override void Cast( TypeReference type, Expression expression )
		 {
			  _result.Append( "cast{type=" );
			  type.WriteTo( _result );
			  _result.Append( ", expression=" );
			  expression.Accept( this );
			  _result.Append( "}" );
		 }

		 public override void InstanceOf( TypeReference type, Expression expression )
		 {
			  _result.Append( "instanceOf{type=" );
			  type.WriteTo( _result );
			  _result.Append( ", expression=" );
			  expression.Accept( this );
			  _result.Append( "}" );
		 }

		 public override void NewArray( TypeReference type, params Expression[] constants )
		 {
			  _result.Append( "newArray{type=" );
			  type.WriteTo( _result );
			  _result.Append( ", constants=" );
			  string sep = "";
			  foreach ( Expression constant in constants )
			  {
					_result.Append( sep );
					constant.Accept( this );
					sep = ", ";
			  }
			  _result.Append( "}" );
		 }

		 public override void LongToDouble( Expression expression )
		 {
			  _result.Append( "(double)" );
			  expression.Accept( this );
		 }

		 public override void Pop( Expression expression )
		 {
			  _result.Append( "pop(" );
			  expression.Accept( this );
			  _result.Append( ")" );
		 }

		 public override void Box( Expression expression )
		 {
			  _result.Append( "box(" );
			  expression.Accept( this );
			  _result.Append( ")" );
		 }

		 public override void Unbox( Expression expression )
		 {
			  _result.Append( "unbox(" );
			  expression.Accept( this );
			  _result.Append( ")" );
		 }
	}

}