using System.Diagnostics;
using System.Threading;

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
//	import static org.neo4j.codegen.ByteCodeUtils.byteCodeName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.ByteCodeUtils.desc;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.ByteCodeUtils.typeName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.AASTORE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.ACONST_NULL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.ALOAD;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.ANEWARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.BASTORE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.BIPUSH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.CASTORE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.CHECKCAST;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.DADD;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.DASTORE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.DCMPG;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.DCMPL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.DLOAD;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.DMUL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.DSUB;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.DUP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.FADD;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.FASTORE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.FCMPG;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.FCMPL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.FLOAD;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.FMUL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.FSUB;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.GETFIELD;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.GETSTATIC;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.GOTO;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.IADD;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.IASTORE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.ICONST_0;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.ICONST_1;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.IFEQ;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.IFGE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.IFGT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.IFLE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.IFLT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.IFNE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.IFNONNULL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.IFNULL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.IF_ACMPEQ;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.IF_ACMPNE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.IF_ICMPEQ;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.IF_ICMPGE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.IF_ICMPGT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.IF_ICMPLE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.IF_ICMPLT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.IF_ICMPNE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.ILOAD;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.IMUL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.INSTANCEOF;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.INVOKEINTERFACE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.INVOKESPECIAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.INVOKESTATIC;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.INVOKEVIRTUAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.ISUB;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.L2D;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.LADD;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.LASTORE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.LCMP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.LCONST_0;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.LCONST_1;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.LLOAD;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.LMUL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.LSUB;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.NEW;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.NEWARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.POP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.POP2;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.SASTORE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.SIPUSH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.T_BOOLEAN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.T_BYTE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.T_CHAR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.T_DOUBLE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.T_FLOAT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.T_INT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.T_LONG;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.T_SHORT;

	internal class ByteCodeExpressionVisitor : ExpressionVisitor
	{
		 private readonly MethodVisitor _methodVisitor;

		 internal ByteCodeExpressionVisitor( MethodVisitor methodVisitor )
		 {
			  this._methodVisitor = methodVisitor;
		 }

		 public override void Invoke( Expression target, MethodReference method, Expression[] arguments )
		 {
			  target.Accept( this );
			  foreach ( Expression argument in arguments )
			  {
					argument.Accept( this );
			  }
			  if ( Modifier.isInterface( method.Owner().modifiers() ) )
			  {
					_methodVisitor.visitMethodInsn( INVOKEINTERFACE, byteCodeName( method.Owner() ), method.Name(), desc(method), true );
			  }
			  else if ( method.Constructor )
			  {
					_methodVisitor.visitMethodInsn( INVOKESPECIAL, byteCodeName( method.Owner() ), method.Name(), desc(method), false );
			  }
			  else
			  {
					_methodVisitor.visitMethodInsn( INVOKEVIRTUAL, byteCodeName( method.Owner() ), method.Name(), desc(method), false );
			  }
		 }

		 public override void Invoke( MethodReference method, Expression[] arguments )
		 {
			  foreach ( Expression argument in arguments )
			  {
					argument.Accept( this );
			  }
			  _methodVisitor.visitMethodInsn( INVOKESTATIC, byteCodeName( method.Owner() ), method.Name(), desc(method), Modifier.isInterface(method.Owner().modifiers()) );
		 }

		 public override void Load( LocalVariable variable )
		 {
			  if ( variable.Type().Primitive )
			  {
					switch ( variable.Type().name() )
					{
					case "int":
					case "byte":
					case "short":
					case "char":
					case "boolean":
						 _methodVisitor.visitVarInsn( ILOAD, variable.Index() );
						 break;
					case "long":
						 _methodVisitor.visitVarInsn( LLOAD, variable.Index() );
						 break;
					case "float":
						 _methodVisitor.visitVarInsn( FLOAD, variable.Index() );
						 break;
					case "double":
						 _methodVisitor.visitVarInsn( DLOAD, variable.Index() );
						 break;
					default:
						 _methodVisitor.visitVarInsn( ALOAD, variable.Index() );
					 break;
					}
			  }
			  else
			  {
					_methodVisitor.visitVarInsn( ALOAD, variable.Index() );
			  }
		 }

		 public override void GetField( Expression target, FieldReference field )
		 {
			  target.Accept( this );
			  _methodVisitor.visitFieldInsn( GETFIELD, byteCodeName( field.Owner() ), field.Name(), typeName(field.Type()) );
		 }

		 public override void Constant( object value )
		 {
			  if ( value == null )
			  {
					_methodVisitor.visitInsn( ACONST_NULL );
			  }
			  else if ( value is int? )
			  {
					PushInteger( ( int? ) value.Value );
			  }
			  else if ( value is sbyte? )
			  {
					PushInteger( ( sbyte? ) value );
			  }
			  else if ( value is short? )
			  {
					PushInteger( ( short? ) value );
			  }
			  else if ( value is long? )
			  {
					PushLong( ( long? ) value.Value );
			  }
			  else if ( value is double? )
			  {
					_methodVisitor.visitLdcInsn( value );
			  }
			  else if ( value is float? )
			  {
					_methodVisitor.visitLdcInsn( value );
			  }
			  else if ( value is bool? )
			  {
					bool b = ( bool ) value;
					_methodVisitor.visitInsn( b ? ICONST_1 : ICONST_0 );
			  }
			  else
			  {
					_methodVisitor.visitLdcInsn( value );
			  }
		 }

		 public override void GetStatic( FieldReference field )
		 {
			  _methodVisitor.visitFieldInsn( GETSTATIC, byteCodeName( field.Owner() ), field.Name(), typeName(field.Type()) );
		 }

		 public override void LoadThis( string sourceName )
		 {
			  _methodVisitor.visitVarInsn( ALOAD, 0 );
		 }

		 public override void NewInstance( TypeReference type )
		 {
			  _methodVisitor.visitTypeInsn( NEW, byteCodeName( type ) );
			  _methodVisitor.visitInsn( DUP );
		 }

		 public override void Not( Expression expression )
		 {
			  Test( IFNE, expression, Expression.TRUE, Expression.FALSE );
		 }

		 public override void IsNull( Expression expression )
		 {
			  Test( IFNONNULL, expression, Expression.TRUE, Expression.FALSE );
		 }

		 public override void NotNull( Expression expression )
		 {
			  Test( IFNULL, expression, Expression.TRUE, Expression.FALSE );
		 }

		 public override void Ternary( Expression test, Expression onTrue, Expression onFalse )
		 {
			  test( IFEQ, test, onTrue, onFalse );
		 }

		 public virtual void TernaryOnNull( Expression test, Expression onTrue, Expression onFalse )
		 {
			  test( IFNONNULL, test, onTrue, onFalse );
		 }

		 public virtual void TernaryOnNonNull( Expression test, Expression onTrue, Expression onFalse )
		 {
			  test( IFNULL, test, onTrue, onFalse );
		 }

		 private void Test( int test, Expression predicate, Expression onTrue, Expression onFalse )
		 {
			  predicate.Accept( this );
			  Label isFalse = new Label();
			  _methodVisitor.visitJumpInsn( test, isFalse );
			  onTrue.Accept( this );
			  Label after = new Label();
			  _methodVisitor.visitJumpInsn( GOTO, after );
			  _methodVisitor.visitLabel( isFalse );
			  onFalse.Accept( this );
			  _methodVisitor.visitLabel( after );
		 }

		 public override void Equal( Expression lhs, Expression rhs )
		 {
			  Equal( lhs, rhs, true );
		 }

		 public override void NotEqual( Expression lhs, Expression rhs )
		 {
			  Equal( lhs, rhs, false );
		 }

		 private void Equal( Expression lhs, Expression rhs, bool equal )
		 {
			  if ( lhs.Type().Primitive )
			  {
					Debug.Assert( rhs.Type().Primitive );

					switch ( lhs.Type().name() )
					{
					case "int":
					case "byte":
					case "short":
					case "char":
					case "boolean":
						 AssertSameType( lhs, rhs, "compare" );
						 CompareIntOrReferenceType( lhs, rhs, equal ? IF_ICMPNE : IF_ICMPEQ );
						 break;
					case "long":
						 AssertSameType( lhs, rhs, "compare" );
						 CompareLongOrFloatType( lhs, rhs, LCMP, equal ? IFNE : IFEQ );
						 break;
					case "float":
						 AssertSameType( lhs, rhs, "compare" );
						 CompareLongOrFloatType( lhs, rhs, FCMPL, equal ? IFNE : IFEQ );
						 break;
					case "double":
						 AssertSameType( lhs, rhs, "compare" );
						 CompareLongOrFloatType( lhs, rhs, DCMPL, equal ? IFNE : IFEQ );
						 break;
					default:
						 CompareIntOrReferenceType( lhs, rhs, equal ? IF_ACMPNE : IF_ACMPEQ );
					 break;
					}
			  }
			  else
			  {
					Debug.Assert( !( rhs.Type().Primitive ) );
					CompareIntOrReferenceType( lhs, rhs, equal ? IF_ACMPNE : IF_ACMPEQ );
			  }
		 }

		 public override void Or( params Expression[] expressions )
		 {
			  Debug.Assert( expressions.Length >= 2 );
			  /*
			   * something like:
			   *
			   * LOAD expression1
			   * IF TRUE GOTO 0
			   * LOAD expression2
			   * IF TRUE GOTO 0
			   * ...
			   * LOAD expressionN
			   * IF FALSE GOTO 1
			   * 0: // The reason we have this extra block for the true case is because we mimic what javac does
			   *    // hoping that it will be nice to the JIT compiler
			   *  LOAD TRUE
			   *  GOTO 2
			   * 1:
			   *  LOAD FALSE
			   * 2:
			   *  ...continue doing stuff
			   */
			  Label l0 = new Label();
			  Label l1 = new Label();
			  Label l2 = new Label();
			  for ( int i = 0; i < expressions.Length; i++ )
			  {
					expressions[i].Accept( this );
					if ( i < expressions.Length - 1 )
					{
						 _methodVisitor.visitJumpInsn( IFNE, l0 );
					}
			  }
			  _methodVisitor.visitJumpInsn( IFEQ, l1 );
			  _methodVisitor.visitLabel( l0 );
			  _methodVisitor.visitInsn( ICONST_1 );
			  _methodVisitor.visitJumpInsn( GOTO, l2 );
			  _methodVisitor.visitLabel( l1 );
			  _methodVisitor.visitInsn( ICONST_0 );
			  _methodVisitor.visitLabel( l2 );
		 }

		 public override void And( params Expression[] expressions )
		 {
			  Debug.Assert( expressions.Length >= 2 );
			  /*
			   * something like:
			   *
			   * LOAD expression1
			   * IF FALSE GOTO 0
			   * LOAD expression2
			   * IF FALSE GOTO 0
			   * LOAD TRUE
			   * ...
			   * LOAD expressionN
			   * IF FALSE GOTO 0
			   * GOTO 1
			   * 0:
			   *  LOAD FALSE
			   * 1:
			   *  ...continue doing stuff
			   */
			  Label l0 = new Label();
			  Label l1 = new Label();
			  foreach ( Expression expression in expressions )
			  {
					expression.Accept( this );
					_methodVisitor.visitJumpInsn( IFEQ, l0 );
			  }
			  _methodVisitor.visitInsn( ICONST_1 );
			  _methodVisitor.visitJumpInsn( GOTO, l1 );
			  _methodVisitor.visitLabel( l0 );
			  _methodVisitor.visitInsn( ICONST_0 );
			  _methodVisitor.visitLabel( l1 );
		 }

		 public override void Add( Expression lhs, Expression rhs )
		 {
			  AssertSameType( lhs, rhs, "add" );
			  lhs.Accept( this );
			  rhs.Accept( this );

			  NumberOperation( lhs.Type(), () => _methodVisitor.visitInsn(IADD), () => _methodVisitor.visitInsn(LADD), () => _methodVisitor.visitInsn(FADD), () => _methodVisitor.visitInsn(DADD) );
		 }

		 public override void Gt( Expression lhs, Expression rhs )
		 {
			  AssertSameType( lhs, rhs, "compare" );
			  NumberOperation( lhs.Type(), () => compareIntOrReferenceType(lhs, rhs, IF_ICMPLE), () => compareLongOrFloatType(lhs, rhs, LCMP, IFLE), () => compareLongOrFloatType(lhs, rhs, FCMPL, IFLE), () => compareLongOrFloatType(lhs, rhs, DCMPL, IFLE) );
		 }

		 public override void Gte( Expression lhs, Expression rhs )
		 {
			  AssertSameType( lhs, rhs, "compare" );
			  NumberOperation( lhs.Type(), () => compareIntOrReferenceType(lhs, rhs, IF_ICMPLT), () => compareLongOrFloatType(lhs, rhs, LCMP, IFLT), () => compareLongOrFloatType(lhs, rhs, FCMPL, IFLT), () => compareLongOrFloatType(lhs, rhs, DCMPL, IFLT) );
		 }

		 public override void Lt( Expression lhs, Expression rhs )
		 {
			  AssertSameType( lhs, rhs, "compare" );
			  NumberOperation( lhs.Type(), () => compareIntOrReferenceType(lhs, rhs, IF_ICMPGE), () => compareLongOrFloatType(lhs, rhs, LCMP, IFGE), () => compareLongOrFloatType(lhs, rhs, FCMPG, IFGE), () => compareLongOrFloatType(lhs, rhs, DCMPG, IFGE) );
		 }

		 public override void Lte( Expression lhs, Expression rhs )
		 {
			  AssertSameType( lhs, rhs, "compare" );
			  NumberOperation( lhs.Type(), () => compareIntOrReferenceType(lhs, rhs, IF_ICMPGT), () => compareLongOrFloatType(lhs, rhs, LCMP, IFGT), () => compareLongOrFloatType(lhs, rhs, FCMPG, IFGT), () => compareLongOrFloatType(lhs, rhs, DCMPG, IFGT) );
		 }

		 public override void Subtract( Expression lhs, Expression rhs )
		 {
			  AssertSameType( lhs, rhs, "subtract" );
			  lhs.Accept( this );
			  rhs.Accept( this );
			  NumberOperation( lhs.Type(), () => _methodVisitor.visitInsn(ISUB), () => _methodVisitor.visitInsn(LSUB), () => _methodVisitor.visitInsn(FSUB), () => _methodVisitor.visitInsn(DSUB) );
		 }

		 public override void Multiply( Expression lhs, Expression rhs )
		 {
			  AssertSameType( lhs, rhs, "multiply" );
			  lhs.Accept( this );
			  rhs.Accept( this );
			  NumberOperation( lhs.Type(), () => _methodVisitor.visitInsn(IMUL), () => _methodVisitor.visitInsn(LMUL), () => _methodVisitor.visitInsn(FMUL), () => _methodVisitor.visitInsn(DMUL) );
		 }

		 public override void Cast( TypeReference type, Expression expression )
		 {
			  expression.Accept( this );
			  if ( !type.Equals( expression.Type() ) )
			  {
					_methodVisitor.visitTypeInsn( CHECKCAST, byteCodeName( type ) );
			  }
		 }

		 public override void InstanceOf( TypeReference type, Expression expression )
		 {
			  expression.Accept( this );
			  _methodVisitor.visitTypeInsn( INSTANCEOF, byteCodeName( type ) );
		 }

		 public override void NewArray( TypeReference type, params Expression[] exprs )
		 {
			  PushInteger( exprs.Length );
			  CreateArray( type );
			  for ( int i = 0; i < exprs.Length; i++ )
			  {
					_methodVisitor.visitInsn( DUP );
					PushInteger( i );
					exprs[i].Accept( this );
					ArrayStore( type );
			  }
		 }

		 public override void LongToDouble( Expression expression )
		 {
			  expression.Accept( this );
			  _methodVisitor.visitInsn( L2D );
		 }

		 public override void Pop( Expression expression )
		 {
			  expression.Accept( this );
			  switch ( expression.Type().simpleName() )
			  {
			  case "long":
			  case "double":
					_methodVisitor.visitInsn( POP2 );
					break;
			  default:
					_methodVisitor.visitInsn( POP );
					break;
			  }
		 }

		 public override void Box( Expression expression )
		 {
			  expression.Accept( this );
			  if ( expression.Type().Primitive )
			  {
					switch ( expression.Type().name() )
					{
					case "byte":
						 _methodVisitor.visitMethodInsn( INVOKESTATIC, "java/lang/Byte", "valueOf", "(B)Ljava/lang/Byte;", false );
						 break;
					case "short":
						 _methodVisitor.visitMethodInsn( INVOKESTATIC, "java/lang/Short", "valueOf", "(S)Ljava/lang/Short;", false );
						 break;
					case "int":
						 _methodVisitor.visitMethodInsn( INVOKESTATIC, "java/lang/Integer", "valueOf", "(I)Ljava/lang/Integer;", false );
						 break;
					case "long":
						 _methodVisitor.visitMethodInsn( INVOKESTATIC, "java/lang/Long", "valueOf", "(J)Ljava/lang/Long;", false );
						 break;
					case "char":
						 _methodVisitor.visitMethodInsn( INVOKESTATIC, "java/lang/Character", "valueOf", "(C)Ljava/lang/Character;", false );
						 break;
					case "boolean":
						 _methodVisitor.visitMethodInsn( INVOKESTATIC, "java/lang/Boolean", "valueOf", "(Z)Ljava/lang/Boolean;", false );
						 break;
					case "float":
						 _methodVisitor.visitMethodInsn( INVOKESTATIC, "java/lang/Float", "valueOf", "(F)Ljava/lang/Float;", false );
						 break;
					case "double":
						 _methodVisitor.visitMethodInsn( INVOKESTATIC, "java/lang/Double", "valueOf", "(D)Ljava/lang/Double;", false );
						 break;
					default:
						 //do nothing, expression is already boxed
				break;
					}
			  }
		 }

		 public override void Unbox( Expression expression )
		 {
			  expression.Accept( this );
			  switch ( expression.Type().fullName() )
			  {
			  case "java.lang.Byte":
					_methodVisitor.visitMethodInsn( INVOKEVIRTUAL, "java/lang/Byte", "byteValue", "()B", false );
					break;
			  case "java.lang.Short":
					_methodVisitor.visitMethodInsn( INVOKEVIRTUAL, "java/lang/Short", "shortValue", "()S", false );
					break;
			  case "java.lang.Integer":
					_methodVisitor.visitMethodInsn( INVOKEVIRTUAL, "java/lang/Integer", "intValue", "()I", false );
					break;
			  case "java.lang.Long":
					_methodVisitor.visitMethodInsn( INVOKEVIRTUAL, "java/lang/Long", "longValue", "()J", false );
					break;
			  case "java.lang.Character":
					_methodVisitor.visitMethodInsn( INVOKEVIRTUAL, "java/lang/Character", "charValue", "()C", false );
					break;
			  case "java.lang.Boolean":
					_methodVisitor.visitMethodInsn( INVOKEVIRTUAL, "java/lang/Boolean", "booleanValue", "()Z", false );
					break;
			  case "java.lang.Float":
					_methodVisitor.visitMethodInsn( INVOKEVIRTUAL, "java/lang/Float", "floatValue", "()F", false );
					break;
			  case "java.lang.Double":
					_methodVisitor.visitMethodInsn( INVOKEVIRTUAL, "java/lang/Double", "doubleValue", "()D", false );
					break;
			  default:
					throw new System.InvalidOperationException( "Cannot unbox " + expression.Type().fullName() );
			  }
		 }

		 private void CompareIntOrReferenceType( Expression lhs, Expression rhs, int opcode )
		 {
			  lhs.Accept( this );
			  rhs.Accept( this );

			  Label l0 = new Label();
			  _methodVisitor.visitJumpInsn( opcode, l0 );
			  _methodVisitor.visitInsn( ICONST_1 );
			  Label l1 = new Label();
			  _methodVisitor.visitJumpInsn( GOTO, l1 );
			  _methodVisitor.visitLabel( l0 );
			  _methodVisitor.visitInsn( ICONST_0 );
			  _methodVisitor.visitLabel( l1 );
		 }

		 private void CompareLongOrFloatType( Expression lhs, Expression rhs, int opcode, int compare )
		 {
			  lhs.Accept( this );
			  rhs.Accept( this );

			  _methodVisitor.visitInsn( opcode );
			  Label l0 = new Label();
			  _methodVisitor.visitJumpInsn( compare, l0 );
			  _methodVisitor.visitInsn( ICONST_1 );
			  Label l1 = new Label();
			  _methodVisitor.visitJumpInsn( GOTO, l1 );
			  _methodVisitor.visitLabel( l0 );
			  _methodVisitor.visitInsn( ICONST_0 );
			  _methodVisitor.visitLabel( l1 );
		 }

		 private void PushInteger( int integer )
		 {
			  if ( integer < 6 && integer >= -1 )
			  {
					//LOAD fast, specialized constant instructions
					//ICONST_M1 = 2;
					//ICONST_0 = 3;
					//ICONST_1 = 4;
					//ICONST_2 = 5;
					//ICONST_3 = 6;
					//ICONST_4 = 7;
					//ICONST_5 = 8;
					_methodVisitor.visitInsn( ICONST_0 + integer );
			  }
			  else if ( integer < sbyte.MaxValue && integer > sbyte.MinValue )
			  {
					_methodVisitor.visitIntInsn( BIPUSH, integer );
			  }
			  else if ( integer < short.MaxValue && integer > short.MinValue )
			  {
					_methodVisitor.visitIntInsn( SIPUSH, integer );
			  }
			  else
			  {
					_methodVisitor.visitLdcInsn( integer );
			  }
		 }

		 private void PushLong( long integer )
		 {
			  if ( integer == 0L )
			  {
					_methodVisitor.visitInsn( LCONST_0 );
			  }
			  else if ( integer == 1L )
			  {
					_methodVisitor.visitInsn( LCONST_1 );
			  }
			  else
			  {
					_methodVisitor.visitLdcInsn( integer );
			  }
		 }

		 private void CreateArray( TypeReference reference )
		 {
			  if ( reference.Primitive )
			  {
					switch ( reference.Name() )
					{
					case "int":
						 _methodVisitor.visitIntInsn( NEWARRAY, T_INT );
						 break;
					case "long":
						 _methodVisitor.visitIntInsn( NEWARRAY, T_LONG );
						 break;
					case "byte":
						 _methodVisitor.visitIntInsn( NEWARRAY, T_BYTE );
						 break;
					case "short":
						 _methodVisitor.visitIntInsn( NEWARRAY, T_SHORT );
						 break;
					case "char":
						 _methodVisitor.visitIntInsn( NEWARRAY, T_CHAR );
						 break;
					case "float":
						 _methodVisitor.visitIntInsn( NEWARRAY, T_FLOAT );
						 break;
					case "double":
						 _methodVisitor.visitIntInsn( NEWARRAY, T_DOUBLE );
						 break;
					case "boolean":
						 _methodVisitor.visitIntInsn( NEWARRAY, T_BOOLEAN );
						 break;
					default:
						 _methodVisitor.visitTypeInsn( ANEWARRAY, byteCodeName( reference ) );
					 break;
					}
			  }
			  else
			  {
					_methodVisitor.visitTypeInsn( ANEWARRAY, byteCodeName( reference ) );
			  }
		 }

		 private void ArrayStore( TypeReference reference )
		 {
			  if ( reference.Primitive )
			  {
					switch ( reference.Name() )
					{
					case "int":
						 _methodVisitor.visitInsn( IASTORE );
						 break;
					case "long":
						 _methodVisitor.visitInsn( LASTORE );
						 break;
					case "byte":
						 _methodVisitor.visitInsn( BASTORE );
						 break;
					case "short":
						 _methodVisitor.visitInsn( SASTORE );
						 break;
					case "char":
						 _methodVisitor.visitInsn( CASTORE );
						 break;
					case "float":
						 _methodVisitor.visitInsn( FASTORE );
						 break;
					case "double":
						 _methodVisitor.visitInsn( DASTORE );
						 break;
					case "boolean":
						 _methodVisitor.visitInsn( BASTORE );
						 break;
					default:
						 _methodVisitor.visitInsn( AASTORE );
					 break;
					}
			  }
			  else
			  {
					_methodVisitor.visitInsn( AASTORE );
			  }
		 }

		 private void NumberOperation( TypeReference type, ThreadStart onInt, ThreadStart onLong, ThreadStart onFloat, ThreadStart onDouble )
		 {
			  if ( !type.Primitive )
			  {
					throw new System.InvalidOperationException( "Cannot compare reference types" );
			  }

			  switch ( type.Name() )
			  {
			  case "int":
			  case "byte":
			  case "short":
			  case "char":
			  case "boolean":
					onInt.run();
					break;
			  case "long":
					onLong.run();
					break;
			  case "float":
					onFloat.run();
					break;
			  case "double":
					onDouble.run();
					break;
			  default:
					throw new System.InvalidOperationException( "Cannot compare reference types" );
			  }
		 }

		 private void AssertSameType( Expression lhs, Expression rhs, string operation )
		 {
			  if ( !lhs.Type().Equals(rhs.Type()) )
			  {
					throw new System.ArgumentException( string.Format( "Can only {0} values of the same type (lhs: {1}, rhs: {2})", operation, lhs.Type().ToString(), rhs.Type().ToString() ) );
			  }
		 }

	}

}