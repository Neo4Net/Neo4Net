using System;

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
//	import static Neo4Net.codegen.LocalVariables.copy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.MethodReference.methodReference;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.TypeReference.typeReference;

	public class CodeBlock : IDisposable
	{

		 internal readonly ClassGenerator Clazz;
		 private MethodEmitter _emitter;
		 private readonly CodeBlock _parent;
		 private bool _done;
		 private bool _continuableBlock;

		 protected internal LocalVariables LocalVariables = new LocalVariables();

		 internal CodeBlock( CodeBlock parent ) : this( parent, parent._continuableBlock )
		 {
		 }

		 internal CodeBlock( CodeBlock parent, bool continuableBlock )
		 {
			  this.Clazz = parent.Clazz;
			  this._emitter = parent._emitter;
			  parent._emitter = InvalidState.InSubBlock;
			  this._parent = parent;
			  //copy over local variables from parent
			  this.LocalVariables = copy( parent.LocalVariables );
			  this._continuableBlock = continuableBlock;
		 }

		 internal CodeBlock( ClassGenerator clazz, MethodEmitter emitter, params Parameter[] parameters )
		 {
			  this.Clazz = clazz;
			  this._emitter = emitter;
			  this._parent = null;
			  this._continuableBlock = false;
			  LocalVariables.createNew( clazz.Handle(), "this" );
			  foreach ( Parameter parameter in parameters )
			  {
					LocalVariables.createNew( parameter.Type(), parameter.Name() );
			  }
		 }

		 public virtual ClassGenerator ClassGenerator()
		 {
			  return Clazz;
		 }

		 public virtual CodeBlock Parent()
		 {
			  return _parent;
		 }

		 public override void Close()
		 {
			  EndBlock();
			  if ( _parent != null )
			  {
					_parent._emitter = _emitter;
			  }
			  else
			  {
					_emitter.done();
			  }
			  this._emitter = InvalidState.BlockClosed;
		 }

		 protected internal virtual void EndBlock()
		 {
			  if ( !_done )
			  {
					_emitter.endBlock();
					_done = true;
			  }
		 }

		 public virtual void Expression( Expression expression )
		 {
			  _emitter.expression( expression );
		 }

		 public virtual LocalVariable Local( string name )
		 {
			  return LocalVariables.get( name );
		 }

		 public virtual LocalVariable Declare( TypeReference type, string name )
		 {
			  LocalVariable local = LocalVariables.createNew( type, name );
			  _emitter.declare( local );
			  return local;
		 }

		 public virtual void Assign( LocalVariable local, Expression value )
		 {
			  _emitter.assignVariableInScope( local, value );
		 }

		 public virtual void Assign( Type type, string name, Expression value )
		 {
			  Assign( typeReference( type ), name, value );
		 }

		 public virtual void Assign( TypeReference type, string name, Expression value )
		 {
			  LocalVariable variable = LocalVariables.createNew( type, name );
			  _emitter.assign( variable, value );
		 }

		 public virtual void Put( Expression target, FieldReference field, Expression value )
		 {
			  _emitter.put( target, field, value );
		 }

		 public virtual Expression Self()
		 {
			  return Load( "this" );
		 }

		 public virtual Expression Load( string name )
		 {
			  return Expression.Load( Local( name ) );
		 }

		 /*
		  * Foreach is just syntactic sugar for a while loop.
		  *
		  */
		 public virtual CodeBlock ForEach( Parameter local, Expression iterable )
		 {
			  string iteratorName = local.Name() + "Iter";

			  Assign( typeof( System.Collections.IEnumerator ), iteratorName, Expression.Invoke( iterable, MethodReference.MethodReferenceConflict( typeof( System.Collections.IEnumerable ), typeof( System.Collections.IEnumerator ), "iterator" ) ) );
			  CodeBlock block = WhileLoop( Expression.Invoke( Load( iteratorName ), methodReference( typeof( System.Collections.IEnumerator ), typeof( bool ), "hasNext" ) ) );
			  block.Assign( local.Type(), local.Name(), Expression.Cast(local.Type(), Expression.Invoke(block.Load(iteratorName), methodReference(typeof(System.Collections.IEnumerator), typeof(object), "next"))) );

			  return block;
		 }

		 public virtual CodeBlock WhileLoop( Expression test )
		 {
			  _emitter.beginWhile( test );
			  return new CodeBlock( this, true );
		 }

		 public virtual CodeBlock IfStatement( Expression test )
		 {
			  _emitter.beginIf( test );
			  return new CodeBlock( this );
		 }

		 public virtual CodeBlock Block()
		 {
			  _emitter.beginBlock();
			  return new CodeBlock( this );
		 }

		 public virtual void TryCatch( System.Action<CodeBlock> body, System.Action<CodeBlock> onError, Parameter exception )
		 {
			  _emitter.tryCatchBlock( body, onError, LocalVariables.createNew( exception.Type(), exception.Name() ), this );
		 }

		 public virtual void Returns()
		 {
			  _emitter.returns();
		 }

		 public virtual void Returns( Expression value )
		 {
			  _emitter.returns( value );
		 }

		 public virtual void ContinueIfPossible()
		 {
			  if ( _continuableBlock )
			  {
					_emitter.continues();
			  }
		 }

		 public virtual void ThrowException( Expression exception )
		 {
			  _emitter.throwException( exception );
		 }

		 public virtual TypeReference Owner()
		 {
			  return Clazz.handle();
		 }
	}

}