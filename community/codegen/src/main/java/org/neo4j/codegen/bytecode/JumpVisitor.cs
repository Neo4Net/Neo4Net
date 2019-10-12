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
namespace Org.Neo4j.Codegen.bytecode
{
	using Label = org.objectweb.asm.Label;
	using MethodVisitor = org.objectweb.asm.MethodVisitor;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.IFEQ;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.IFNE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.IFNONNULL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.IFNULL;

	internal class JumpVisitor : ExpressionVisitor
	{
		 private readonly ExpressionVisitor _eval;
		 private readonly MethodVisitor _methodVisitor;
		 private readonly Label _target;

		 internal JumpVisitor( ExpressionVisitor eval, MethodVisitor methodVisitor, Label target )
		 {
			  this._eval = eval;
			  this._methodVisitor = methodVisitor;
			  this._target = target;
		 }

		 public override void Invoke( Expression target, MethodReference method, Expression[] arguments )
		 {
			  _eval.invoke( target, method, arguments );
			  _methodVisitor.visitJumpInsn( IFEQ, this._target );
		 }

		 public override void Invoke( MethodReference method, Expression[] arguments )
		 {
			  _eval.invoke( method, arguments );
			  _methodVisitor.visitJumpInsn( IFEQ, this._target );
		 }

		 public override void Load( LocalVariable variable )
		 {
			  _eval.load( variable );
			  _methodVisitor.visitJumpInsn( IFEQ, this._target );
		 }

		 public override void GetField( Expression target, FieldReference field )
		 {
			  _eval.getField( target, field );
			  _methodVisitor.visitJumpInsn( IFEQ, this._target );
		 }

		 public override void Constant( object value )
		 {
			  _eval.constant( value );
			  _methodVisitor.visitJumpInsn( IFEQ, this._target );
		 }

		 public override void GetStatic( FieldReference field )
		 {
			  _eval.getStatic( field );
			  _methodVisitor.visitJumpInsn( IFEQ, this._target );
		 }

		 public override void Not( Expression expression )
		 {
			  expression.Accept( _eval );
			  _methodVisitor.visitJumpInsn( IFNE, this._target );
		 }

		 public override void Ternary( Expression test, Expression onTrue, Expression onFalse )
		 {
			  _eval.ternary( test, onTrue, onFalse );
			  _methodVisitor.visitJumpInsn( IFEQ, this._target );
		 }

		 public override void Equal( Expression lhs, Expression rhs )
		 {
			  _eval.equal( lhs, rhs );
			  _methodVisitor.visitJumpInsn( IFEQ, this._target );
		 }

		 public override void NotEqual( Expression lhs, Expression rhs )
		 {
			  _eval.equal( lhs, rhs );
			  _methodVisitor.visitJumpInsn( IFNE, this._target );
		 }

		 public override void IsNull( Expression expression )
		 {
			  expression.Accept( _eval );
			  _methodVisitor.visitJumpInsn( IFNONNULL, this._target );
		 }

		 public override void NotNull( Expression expression )
		 {
			  expression.Accept( _eval );
			  _methodVisitor.visitJumpInsn( IFNULL, this._target );
		 }

		 public override void Or( params Expression[] expressions )
		 {
			  _eval.or( expressions );
			  _methodVisitor.visitJumpInsn( IFEQ, this._target );
		 }

		 public override void And( params Expression[] expressions )
		 {
			  foreach ( Expression expression in expressions )
			  {
					expression.Accept( this );
			  }
		 }

		 public override void Gt( Expression lhs, Expression rhs )
		 {
			  _eval.gt( lhs, rhs );
			  _methodVisitor.visitJumpInsn( IFEQ, this._target );
		 }

		 public override void Gte( Expression lhs, Expression rhs )
		 {
			  _eval.gte( lhs, rhs );
			  _methodVisitor.visitJumpInsn( IFEQ, this._target );
		 }

		 public override void Lt( Expression lhs, Expression rhs )
		 {
			  _eval.lt( lhs, rhs );
			  _methodVisitor.visitJumpInsn( IFEQ, this._target );
		 }

		 public override void Lte( Expression lhs, Expression rhs )
		 {
			  _eval.lte( lhs, rhs );
			  _methodVisitor.visitJumpInsn( IFEQ, this._target );
		 }

		 public override void Unbox( Expression expression )
		 {
			  _eval.unbox( expression );
			  _methodVisitor.visitJumpInsn( IFEQ, this._target );
		 }

		 public override void LoadThis( string sourceName )
		 {
			  throw new System.ArgumentException( "'" + sourceName + "' is not a boolean expression" );
		 }

		 public override void NewInstance( TypeReference type )
		 {
			  throw new System.ArgumentException( "'new' is not a boolean expression" );
		 }

		 public override void Add( Expression lhs, Expression rhs )
		 {
			  throw new System.ArgumentException( "'+' is not a boolean expression" );
		 }

		 public override void Subtract( Expression lhs, Expression rhs )
		 {
			  throw new System.ArgumentException( "'-' is not a boolean expression" );
		 }

		 public override void Multiply( Expression lhs, Expression rhs )
		 {
			  throw new System.ArgumentException( "'*' is not a boolean expression" );
		 }

		 public override void Cast( TypeReference type, Expression expression )
		 {
			  throw new System.ArgumentException( "cast is not a boolean expression" );
		 }

		 public override void InstanceOf( TypeReference type, Expression expression )
		 {
			  throw new System.ArgumentException( "cast is not a boolean expression" );
		 }

		 public override void NewArray( TypeReference type, params Expression[] constants )
		 {
			  throw new System.ArgumentException( "'new' (array) is not a boolean expression" );
		 }

		 public override void LongToDouble( Expression expression )
		 {
			  throw new System.ArgumentException( "cast is not a boolean expression" );
		 }

		 public override void Pop( Expression expression )
		 {
			  throw new System.ArgumentException( "pop is not a boolean expression" );
		 }

		 public override void Box( Expression expression )
		 {
			  throw new System.ArgumentException( "box is not a boolean expression" );
		 }
	}

}