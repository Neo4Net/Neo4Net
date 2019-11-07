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
namespace Neo4Net.CodeGen.ByteCode
{
	using ClassVisitor = org.objectweb.asm.ClassVisitor;
	using Label = org.objectweb.asm.Label;
	using MethodVisitor = org.objectweb.asm.MethodVisitor;



//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.ByteCodeUtils.byteCodeName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.ByteCodeUtils.desc;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.ByteCodeUtils.exceptions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.ByteCodeUtils.outerName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.ByteCodeUtils.signature;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.ByteCodeUtils.typeName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.ACC_PUBLIC;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.ARETURN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.ASTORE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.ATHROW;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.DRETURN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.DSTORE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.FRETURN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.FSTORE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.GOTO;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.IRETURN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.ISTORE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.LRETURN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.LSTORE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.PUTFIELD;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.RETURN;

	internal class MethodByteCodeEmitter : MethodEmitter
	{
		 private readonly MethodVisitor _methodVisitor;
		 private readonly MethodDeclaration _declaration;
		 private readonly ExpressionVisitor _expressionVisitor;
		 private Deque<Block> _stateStack = new LinkedList<Block>();

		 internal MethodByteCodeEmitter( ClassVisitor classVisitor, MethodDeclaration declaration, TypeReference ignore )
		 {
			  this._declaration = declaration;
			  foreach ( Parameter parameter in declaration.Parameters() )
			  {
					TypeReference type = parameter.Type();
					if ( type.InnerClass && !type.Array )
					{
						 classVisitor.visitInnerClass( byteCodeName( type ), outerName( type ), type.SimpleName(), type.Modifiers() );
					}
			  }
			  this._methodVisitor = classVisitor.visitMethod( ACC_PUBLIC, declaration.Name(), desc(declaration), signature(declaration), exceptions(declaration) );
			  this._methodVisitor.visitCode();
			  this._expressionVisitor = new ByteCodeExpressionVisitor( this._methodVisitor );
			  _stateStack.push( new Method( _methodVisitor, declaration.ReturnType().Void ) );
		 }

		 public override void Done()
		 {
			  _methodVisitor.visitEnd();
		 }

		 public override void Expression( Expression expression )
		 {
			  expression.Accept( _expressionVisitor );
		 }

		 public override void Put( Expression target, FieldReference field, Expression value )
		 {
			  target.Accept( _expressionVisitor );
			  value.Accept( _expressionVisitor );
			  _methodVisitor.visitFieldInsn( PUTFIELD, byteCodeName( field.Owner() ), field.Name(), typeName(field.Type()) );
		 }

		 public override void Returns()
		 {
			  _methodVisitor.visitInsn( RETURN );
		 }

		 public override void Returns( Expression value )
		 {
			  value.Accept( _expressionVisitor );
			  if ( _declaration.returnType().Primitive )
			  {
					switch ( _declaration.returnType().name() )
					{
					case "int":
					case "byte":
					case "short":
					case "char":
					case "boolean":
						 _methodVisitor.visitInsn( IRETURN );
						 break;
					case "long":
						 _methodVisitor.visitInsn( LRETURN );
						 break;
					case "float":
						 _methodVisitor.visitInsn( FRETURN );
						 break;
					case "double":
						 _methodVisitor.visitInsn( DRETURN );
						 break;
					default:
						 _methodVisitor.visitInsn( ARETURN );
					 break;
					}
			  }
			  else
			  {
					_methodVisitor.visitInsn( ARETURN );
			  }
		 }

		 public override void Continues()
		 {
			  foreach ( Block block in _stateStack )
			  {
					if ( block is While )
					{
						 ( ( While )block ).ContinueBlock();
						 return;
					}
			  }
			  throw new System.InvalidOperationException( "Found no block to continue" );
		 }

		 public override void Assign( LocalVariable variable, Expression value )
		 {
			  value.Accept( _expressionVisitor );
			  if ( variable.Type().Primitive )
			  {
					switch ( variable.Type().name() )
					{
					case "int":
					case "byte":
					case "short":
					case "char":
					case "boolean":
						 _methodVisitor.visitVarInsn( ISTORE, variable.Index() );
						 break;
					case "long":
						 _methodVisitor.visitVarInsn( LSTORE, variable.Index() );
						 break;
					case "float":
						 _methodVisitor.visitVarInsn( FSTORE, variable.Index() );
						 break;
					case "double":
						 _methodVisitor.visitVarInsn( DSTORE, variable.Index() );
						 break;
					default:
						 _methodVisitor.visitVarInsn( ASTORE, variable.Index() );
					 break;
					}
			  }
			  else
			  {
					_methodVisitor.visitVarInsn( ASTORE, variable.Index() );
			  }
		 }

		 public override void BeginWhile( Expression test )
		 {
			  Label repeat = new Label();
			  Label done = new Label();
			  _methodVisitor.visitLabel( repeat );
			  test.Accept( new JumpVisitor( _expressionVisitor, _methodVisitor, done ) );

			  _stateStack.push( new While( _methodVisitor, repeat, done ) );
		 }

		 public override void BeginIf( Expression test )
		 {
			  Label after = new Label();
			  test.Accept( new JumpVisitor( _expressionVisitor, _methodVisitor, after ) );
			  _stateStack.push( new If( _methodVisitor, after ) );
		 }

		 public override void BeginBlock()
		 {
			  _stateStack.push(() =>
			  {
			  });
		 }

		 public override void EndBlock()
		 {
			  if ( _stateStack.Empty )
			  {
					throw new System.InvalidOperationException( "Unbalanced blocks" );
			  }
			  _stateStack.pop().endBlock();
		 }

		 public override void TryCatchBlock<T>( System.Action<T> body, System.Action<T> handler, LocalVariable exception, T block )
		 {
			  Label start = new Label();
			  Label end = new Label();
			  Label handle = new Label();
			  Label after = new Label();
			  _methodVisitor.visitTryCatchBlock( start, end, handle, byteCodeName( exception.Type() ) );
			  _methodVisitor.visitLabel( start );
			  body( block );
			  _methodVisitor.visitLabel( end );
			  _methodVisitor.visitJumpInsn( GOTO, after );
			  //handle catch
			  _methodVisitor.visitLabel( handle );
			  _methodVisitor.visitVarInsn( ASTORE, exception.Index() );

			  handler( block );
			  _methodVisitor.visitLabel( after );
		 }

		 public override void ThrowException( Expression exception )
		 {
			  exception.Accept( _expressionVisitor );
			  _methodVisitor.visitInsn( ATHROW );
		 }

		 public override void Declare( LocalVariable local )
		 {
			  //declare is a noop bytecode wise
		 }

		 public override void AssignVariableInScope( LocalVariable local, Expression value )
		 {
			  //these are equivalent when it comes to bytecode
			  Assign( local, value );
		 }
	}

}