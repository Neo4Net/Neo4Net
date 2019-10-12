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
namespace Neo4Net.Codegen
{

	internal class InvalidState : MethodEmitter
	{
		 public static readonly ClassEmitter CLASS_DONE = new ClassEmitterAnonymousInnerClass();

		 private class ClassEmitterAnonymousInnerClass : ClassEmitter
		 {
			 public MethodEmitter method( MethodDeclaration method )
			 {
				  throw new System.InvalidOperationException( "class done" );
			 }

			 public void field( FieldReference field, Expression value )
			 {
				  throw new System.InvalidOperationException( "class done" );
			 }

			 public void done()
			 {
				  throw new System.InvalidOperationException( "class done" );
			 }
		 }
		 public static readonly MethodEmitter BlockClosed = new InvalidState( "this block has been closed" );
		 public static readonly MethodEmitter InSubBlock = new InvalidState( "currently generating a sub-block of this block" );
		 private readonly string _reason;

		 private InvalidState( string reason )
		 {
			  this._reason = reason;
		 }

		 public override void Done()
		 {
			  throw new System.InvalidOperationException( _reason );
		 }

		 public override void Expression( Expression expression )
		 {
			  throw new System.InvalidOperationException( _reason );
		 }

		 public override void Put( Expression target, FieldReference field, Expression value )
		 {
			  throw new System.InvalidOperationException( _reason );
		 }

		 public override void Returns()
		 {
			  throw new System.InvalidOperationException( _reason );
		 }

		 public override void Returns( Expression value )
		 {
			  throw new System.InvalidOperationException( _reason );
		 }

		 public override void Continues()
		 {
			  throw new System.InvalidOperationException( _reason );
		 }

		 public override void Assign( LocalVariable variable, Expression value )
		 {
			  throw new System.InvalidOperationException( _reason );
		 }

		 public override void BeginWhile( Expression test )
		 {
			  throw new System.InvalidOperationException( _reason );
		 }

		 public override void BeginIf( Expression test )
		 {
			  throw new System.InvalidOperationException( _reason );
		 }

		 public override void BeginBlock()
		 {
			  throw new System.InvalidOperationException( _reason );
		 }

		 public override void EndBlock()
		 {
			  throw new System.InvalidOperationException( _reason );
		 }

		 public override void TryCatchBlock<T>( System.Action<T> body, System.Action<T> handler, LocalVariable exception, T block )
		 {
			  throw new System.InvalidOperationException( _reason );
		 }

		 public override void ThrowException( Expression exception )
		 {
			  throw new System.InvalidOperationException( _reason );
		 }

		 public override void Declare( LocalVariable local )
		 {
			  throw new System.InvalidOperationException( _reason );
		 }

		 public override void AssignVariableInScope( LocalVariable local, Expression value )
		 {
			  throw new System.InvalidOperationException( _reason );
		 }

	}

}