using System.Text;

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
namespace Org.Neo4j.Codegen
{
	using ClassReader = org.objectweb.asm.ClassReader;
	using ClassVisitor = org.objectweb.asm.ClassVisitor;
	using FieldVisitor = org.objectweb.asm.FieldVisitor;
	using Handle = org.objectweb.asm.Handle;
	using Label = org.objectweb.asm.Label;
	using MethodVisitor = org.objectweb.asm.MethodVisitor;
	using Opcodes = org.objectweb.asm.Opcodes;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Type.getType;

	internal interface ByteCodeVisitor
	{

		 void VisitByteCode( string name, ByteBuffer bytes );

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static ByteCodeVisitor_Printer printer(java.io.PrintWriter @out)
	//	 {
	//		  return new Printer()
	//		  {
	//				@@Override void printf(String format, Object... args)
	//				{
	//					 @out.format(format, args);
	//				}
	//
	//				@@Override void println(CharSequence line)
	//				{
	//					 @out.println(line);
	//				}
	//		  };
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static ByteCodeVisitor_Printer printer(java.io.PrintStream @out)
	//	 {
	//		  return new Printer()
	//		  {
	//				@@Override void printf(String format, Object... args)
	//				{
	//					 @out.format(format, args);
	//				}
	//
	//				@@Override void println(CharSequence line)
	//				{
	//					 @out.println(line);
	//				}
	//		  };
	//	 }
	}

	public static class ByteCodeVisitor_Fields
	{
		 public static readonly ByteCodeVisitor DoNothing = ( name, bytes ) =>
		 {
		 };

	}

	 internal interface ByteCodeVisitor_Configurable
	 {
		  void AddByteCodeVisitor( ByteCodeVisitor visitor );
	 }

	 internal class ByteCodeVisitor_Multiplex : ByteCodeVisitor
	 {
		  internal readonly ByteCodeVisitor[] Visitors;

		  internal ByteCodeVisitor_Multiplex( ByteCodeVisitor[] visitors )
		  {
				this.Visitors = visitors;
		  }

		  public override void VisitByteCode( string name, ByteBuffer bytes )
		  {
				foreach ( ByteCodeVisitor visitor in Visitors )
				{
					 visitor.VisitByteCode( name, bytes.duplicate() );
				}
		  }
	 }

	 internal abstract class ByteCodeVisitor_Printer : ClassVisitor, ByteCodeVisitor, CodeGeneratorOption
	 {
		 public abstract ByteCodeVisitor_Printer Printer( PrintStream @out );
		 public abstract ByteCodeVisitor_Printer Printer( PrintWriter @out );
		  internal ByteCodeVisitor_Printer() : base(Opcodes.ASM4)
		  {
		  }

		  public override void ApplyTo( object target )
		  {
				if ( target is ByteCodeVisitor_Configurable )
				{
					 ( ( ByteCodeVisitor_Configurable ) target ).AddByteCodeVisitor( this );
				}
		  }

		  internal abstract void Printf( string format, params object[] args );

		  internal abstract void Println( CharSequence line );

		  public override void VisitByteCode( string name, ByteBuffer bytes )
		  {
				( new ClassReader( bytes.array() ) ).accept(this, 0);
		  }

		  public override void Visit( int version, int access, string name, string signature, string superName, string[] interfaces )
		  {
				StringBuilder iFaces = new StringBuilder();
				string prefix = " implements ";
				foreach ( string iFace in interfaces )
				{
					 iFaces.Append( prefix ).Append( iFace );
					 prefix = ", ";
				}
				Printf( "%s class %s extends %s%s%n{%n", Modifier.ToString( access ), name, superName, iFaces );
		  }

		  public override FieldVisitor VisitField( int access, string name, string desc, string signature, object value )
		  {
				Printf( "  %s %s %s%s;%n", Modifier.ToString( access ), getType( desc ).ClassName, name, value == null ? "" : ( " = " + value ) );
				return base.VisitField( access, name, desc, signature, value );
		  }

		  public override MethodVisitor VisitMethod( int access, string name, string desc, string signature, string[] exceptions )
		  {
				Printf( "  %s %s%s%n  {%n", Modifier.ToString( access ), name, desc );
				return new MethodVisitorAnonymousInnerClass( this, api, name, desc, signature );
		  }

		  private class MethodVisitorAnonymousInnerClass : MethodVisitor
		  {
			  private readonly ByteCodeVisitor_Printer _outerInstance;

			  private string _name;
			  private string _desc;
			  private string _signature;

			  public MethodVisitorAnonymousInnerClass( ByteCodeVisitor_Printer outerInstance, UnknownType api, string name, string desc, string signature ) : base( api )
			  {
				  this.outerInstance = outerInstance;
				  this._name = name;
				  this._desc = desc;
				  this._signature = signature;
			  }

			  internal int offset;

			  public override void visitFrame( int type, int nLocal, object[] local, int nStack, object[] stack )
			  {
					StringBuilder frame = ( new StringBuilder() ).Append("    [FRAME:");
					switch ( type )
					{
					case Opcodes.F_NEW:
						 frame.Append( "NEW" );
						 break;
					case Opcodes.F_FULL:
						 frame.Append( "FULL" );
						 break;
					case Opcodes.F_APPEND:
						 frame.Append( "APPEND" );
						 break;
					case Opcodes.F_CHOP:
						 frame.Append( "CHOP" );
						 break;
					case Opcodes.F_SAME:
						 frame.Append( "SAME" );
						 break;
					case Opcodes.F_SAME1:
						 frame.Append( "SAME1" );
						 break;
					default:
						 frame.Append( type );
					 break;
					}
					frame.Append( ", " ).Append( nLocal ).Append( " locals: [" );
					string prefix = "";
					for ( int i = 0; i < nLocal; i++ )
					{
						 frame.Append( prefix );
						 if ( local[i] is string )
						 {
							  frame.Append( local[i] );
						 }
						 else if ( local[i] == Opcodes.TOP )
						 {
							  frame.Append( "TOP" );
						 }
						 else if ( local[i] == Opcodes.INTEGER )
						 {
							  frame.Append( "INTEGER" );
						 }
						 else if ( local[i] == Opcodes.FLOAT )
						 {
							  frame.Append( "FLOAT" );
						 }
						 else if ( local[i] == Opcodes.DOUBLE )
						 {
							  frame.Append( "DOUBLE" );
						 }
						 else if ( local[i] == Opcodes.LONG )
						 {
							  frame.Append( "LONG" );
						 }
						 else if ( local[i] == Opcodes.NULL )
						 {
							  frame.Append( "NULL" );
						 }
						 else if ( local[i] == Opcodes.UNINITIALIZED_THIS )
						 {
							  frame.Append( "UNINITIALIZED_THIS" );
						 }
						 else
						 {
							  frame.Append( local[i] );
						 }
						 prefix = ", ";
					}
					frame.Append( "], " ).Append( nStack ).Append( " items on stack: [" );
					prefix = "";
					for ( int i = 0; i < nStack; i++ )
					{
						 frame.Append( prefix ).Append( Objects.ToString( stack[i] ) );
						 prefix = ", ";
					}
					outerInstance.Println( frame.Append( "]" ) );
			  }

			  public override void visitInsn( int opcode )
			  {
					outerInstance.Printf( "    @%03d: %s%n", offset, opcode( opcode ) );
					offset += 1;
			  }

			  public override void visitIntInsn( int opcode, int operand )
			  {
					outerInstance.Printf( "    @%03d: %s %d%n", offset, opcode( opcode ), operand );
					offset += opcode == Opcodes.SIPUSH ? 3 : 2;
			  }

			  public override void visitVarInsn( int opcode, int var )
			  {
					outerInstance.Printf( "    @%03d: %s var:%d%n", offset, opcode( opcode ), var );
					// guessing the most efficient encoding was used:
					if ( var <= 0x3 )
					{
						 offset += 1;
					}
					else if ( var <= 0xFF )
					{
						 offset += 2;
					}
					else
					{
						 offset += 4;
					}
			  }

			  public override void visitTypeInsn( int opcode, string type )
			  {
					outerInstance.Printf( "    @%03d: %s %s%n", offset, opcode( opcode ), type );
					offset += 3;
			  }

			  public override void visitFieldInsn( int opcode, string owner, string name, string desc )
			  {
					outerInstance.Printf( "    @%03d: %s %s.%s:%s%n", offset, opcode( opcode ), owner, name, desc );
					offset += 3;
			  }

			  public override void visitMethodInsn( int opcode, string owner, string name, string desc, bool itf )
			  {
					outerInstance.Printf( "    @%03d: %s %s.%s%s%n", offset, opcode( opcode ), owner, name, desc );
					offset += opcode == Opcodes.INVOKEINTERFACE ? 5 : 3;
			  }

			  public override void visitInvokeDynamicInsn( string name, string desc, Handle bsm, params object[] bsmArgs )
			  {
					outerInstance.Printf( "    @%03d: InvokeDynamic %s%s / bsm:%s%s%n", offset, name, desc, bsm, Arrays.ToString( bsmArgs ) );
					offset += 5;
			  }

			  public override void visitJumpInsn( int opcode, Label label )
			  {
					outerInstance.Printf( "    @%03d: %s %s%n", offset, opcode( opcode ), label );
					offset += 3; // TODO: how do we tell if a wide (+=5) instruction (GOTO_W=200, JSR_W=201) was used?
					// wide instructions get simplified to their basic counterpart, but are used for long jumps
			  }

			  public override void visitLabel( Label label )
			  {
					outerInstance.Printf( "   %s:%n", label );
			  }

			  public override void visitLdcInsn( object cst )
			  {
					outerInstance.Printf( "    @%03d: LDC %s%n", offset, cst );
					offset += 2; // TODO: how do we tell if the WIDE instruction prefix (+=3) was used?
					// we don't know index of the constant in the pool, wide instructions are used for high indexes
			  }

			  public override void visitIincInsn( int var, int increment )
			  {
					outerInstance.Printf( "    @%03d: IINC %d += %d%n", offset, var, increment );
					// guessing the most efficient encoding was used:
					if ( var <= 0xFF && increment <= 0xFF )
					{
						 offset += 3;
					}
					else
					{
						 offset += 6;
					}
			  }

			  public override void visitTableSwitchInsn( int min, int max, Label dflt, params Label[] labels )
			  {
					outerInstance.Printf( "    @%03d: TABLE_SWITCH(min=%d, max=%d)%n    {%n", offset, min, max );
					for ( int i = 0, val = min; i < labels.Length; i++, val++ )
					{
						 outerInstance.Printf( "      case %d goto %s%n", val, labels[i] );
					}
					outerInstance.Printf( "      default goto %s%n    }%n", dflt );
					offset += 4 - ( offset & 3 ); // padding bytes, table starts at aligned offset
					offset += 12; // default, min, max
					offset += 4 * labels.Length; // table of offsets
			  }

			  public override void visitLookupSwitchInsn( Label dflt, int[] keys, Label[] labels )
			  {
					outerInstance.Printf( "    @%03d: LOOKUP_SWITCH%n    {%n", offset );
					for ( int i = 0; i < labels.Length; i++ )
					{
						 outerInstance.Printf( "      case %d goto %s%n", keys[i], labels[i] );
					}
					outerInstance.Printf( "      default goto %s%n    }%n", dflt );
					offset += 4 - ( offset & 3 ); // padding bytes, table starts at aligned offset
					offset += 8; // default, length
					offset += 8 * labels.Length; // table of key+offset
			  }

			  public override void visitMultiANewArrayInsn( string desc, int dims )
			  {
					outerInstance.Printf( "    @%03d: MULTI_ANEW_ARRAY %s, dims:%d%n", offset, desc, dims );
					offset += 4;
			  }

			  public override void visitTryCatchBlock( Label start, Label end, Label handler, string type )
			  {
					outerInstance.Printf( "    [try/catch %s start@%s, end@%s, handler@%s]%n", type, start, end, handler );
			  }

			  public override void visitLocalVariable( string name, string desc, string signature, Label start, Label end, int index )
			  {
					outerInstance.Printf( "    [local %s:%s, from %s to %s @offset=%d]%n", name, desc, start, end, index );
			  }

			  public override void visitLineNumber( int line, Label start )
			  {
					outerInstance.Printf( "    [line %d @ %s]%n", line, start );
			  }

			  public override void visitEnd()
			  {
					outerInstance.Println( "  }" );
			  }
		  }

		  public override void VisitEnd()
		  {
				Println( "}" );
		  }

		  internal static string Opcode( int opcode )
		  {
				switch ( opcode )
				{
				// visitInsn
				case Opcodes.NOP:
					 return "NOP";
				case Opcodes.ACONST_NULL:
					 return "ACONST_NULL";
				case Opcodes.ICONST_M1:
					 return "ICONST_M1";
				case Opcodes.ICONST_0:
					 return "ICONST_0";
				case Opcodes.ICONST_1:
					 return "ICONST_1";
				case Opcodes.ICONST_2:
					 return "ICONST_2";
				case Opcodes.ICONST_3:
					 return "ICONST_3";
				case Opcodes.ICONST_4:
					 return "ICONST_4";
				case Opcodes.ICONST_5:
					 return "ICONST_5";
				case Opcodes.LCONST_0:
					 return "LCONST_0";
				case Opcodes.LCONST_1:
					 return "LCONST_1";
				case Opcodes.FCONST_0:
					 return "FCONST_0";
				case Opcodes.FCONST_1:
					 return "FCONST_1";
				case Opcodes.FCONST_2:
					 return "FCONST_2";
				case Opcodes.DCONST_0:
					 return "DCONST_0";
				case Opcodes.DCONST_1:
					 return "DCONST_1";
				case Opcodes.IALOAD:
					 return "IALOAD";
				case Opcodes.LALOAD:
					 return "LALOAD";
				case Opcodes.FALOAD:
					 return "FALOAD";
				case Opcodes.DALOAD:
					 return "DALOAD";
				case Opcodes.AALOAD:
					 return "AALOAD";
				case Opcodes.BALOAD:
					 return "BALOAD";
				case Opcodes.CALOAD:
					 return "CALOAD";
				case Opcodes.SALOAD:
					 return "SALOAD";
				case Opcodes.IASTORE:
					 return "IASTORE";
				case Opcodes.LASTORE:
					 return "LASTORE";
				case Opcodes.FASTORE:
					 return "FASTORE";
				case Opcodes.DASTORE:
					 return "DASTORE";
				case Opcodes.AASTORE:
					 return "AASTORE";
				case Opcodes.BASTORE:
					 return "BASTORE";
				case Opcodes.CASTORE:
					 return "CASTORE";
				case Opcodes.SASTORE:
					 return "SASTORE";
				case Opcodes.POP:
					 return "POP";
				case Opcodes.POP2:
					 return "POP2";
				case Opcodes.DUP:
					 return "DUP";
				case Opcodes.DUP_X1:
					 return "DUP_X1";
				case Opcodes.DUP_X2:
					 return "DUP_X2";
				case Opcodes.DUP2:
					 return "DUP2";
				case Opcodes.DUP2_X1:
					 return "DUP2_X1";
				case Opcodes.DUP2_X2:
					 return "DUP2_X2";
				case Opcodes.SWAP:
					 return "SWAP";
				case Opcodes.IADD:
					 return "IADD";
				case Opcodes.LADD:
					 return "LADD";
				case Opcodes.FADD:
					 return "FADD";
				case Opcodes.DADD:
					 return "DADD";
				case Opcodes.ISUB:
					 return "ISUB";
				case Opcodes.LSUB:
					 return "LSUB";
				case Opcodes.FSUB:
					 return "FSUB";
				case Opcodes.DSUB:
					 return "DSUB";
				case Opcodes.IMUL:
					 return "IMUL";
				case Opcodes.LMUL:
					 return "LMUL";
				case Opcodes.FMUL:
					 return "FMUL";
				case Opcodes.DMUL:
					 return "DMUL";
				case Opcodes.IDIV:
					 return "IDIV";
				case Opcodes.LDIV:
					 return "LDIV";
				case Opcodes.FDIV:
					 return "FDIV";
				case Opcodes.DDIV:
					 return "DDIV";
				case Opcodes.IREM:
					 return "IREM";
				case Opcodes.LREM:
					 return "LREM";
				case Opcodes.FREM:
					 return "FREM";
				case Opcodes.DREM:
					 return "DREM";
				case Opcodes.INEG:
					 return "INEG";
				case Opcodes.LNEG:
					 return "LNEG";
				case Opcodes.FNEG:
					 return "FNEG";
				case Opcodes.DNEG:
					 return "DNEG";
				case Opcodes.ISHL:
					 return "ISHL";
				case Opcodes.LSHL:
					 return "LSHL";
				case Opcodes.ISHR:
					 return "ISHR";
				case Opcodes.LSHR:
					 return "LSHR";
				case Opcodes.IUSHR:
					 return "IUSHR";
				case Opcodes.LUSHR:
					 return "LUSHR";
				case Opcodes.IAND:
					 return "IAND";
				case Opcodes.LAND:
					 return "LAND";
				case Opcodes.IOR:
					 return "IOR";
				case Opcodes.LOR:
					 return "LOR";
				case Opcodes.IXOR:
					 return "IXOR";
				case Opcodes.LXOR:
					 return "LXOR";
				case Opcodes.I2L:
					 return "I2L";
				case Opcodes.I2F:
					 return "I2F";
				case Opcodes.I2D:
					 return "I2D";
				case Opcodes.L2I:
					 return "L2I";
				case Opcodes.L2F:
					 return "L2F";
				case Opcodes.L2D:
					 return "L2D";
				case Opcodes.F2I:
					 return "F2I";
				case Opcodes.F2L:
					 return "F2L";
				case Opcodes.F2D:
					 return "F2D";
				case Opcodes.D2I:
					 return "D2I";
				case Opcodes.D2L:
					 return "D2L";
				case Opcodes.D2F:
					 return "D2F";
				case Opcodes.I2B:
					 return "I2B";
				case Opcodes.I2C:
					 return "I2C";
				case Opcodes.I2S:
					 return "I2S";
				case Opcodes.LCMP:
					 return "LCMP";
				case Opcodes.FCMPL:
					 return "FCMPL";
				case Opcodes.FCMPG:
					 return "FCMPG";
				case Opcodes.DCMPL:
					 return "DCMPL";
				case Opcodes.DCMPG:
					 return "DCMPG";
				case Opcodes.IRETURN:
					 return "IRETURN";
				case Opcodes.LRETURN:
					 return "LRETURN";
				case Opcodes.FRETURN:
					 return "FRETURN";
				case Opcodes.DRETURN:
					 return "DRETURN";
				case Opcodes.ARETURN:
					 return "ARETURN";
				case Opcodes.RETURN:
					 return "RETURN";
				case Opcodes.ARRAYLENGTH:
					 return "ARRAYLENGTH";
				case Opcodes.ATHROW:
					 return "ATHROW";
				case Opcodes.MONITORENTER:
					 return "MONITORENTER";
				case Opcodes.MONITOREXIT:
					 return "MONITOREXIT";
				// visitIntInsn
				case Opcodes.BIPUSH:
					 return "BIPUSH";
				case Opcodes.SIPUSH:
					 return "SIPUSH";
				case Opcodes.NEWARRAY:
					 return "NEWARRAY";
				// visitVarInsn
				case Opcodes.ILOAD:
					 return "ILOAD";
				case Opcodes.LLOAD:
					 return "LLOAD";
				case Opcodes.FLOAD:
					 return "FLOAD";
				case Opcodes.DLOAD:
					 return "DLOAD";
				case Opcodes.ALOAD:
					 return "ALOAD";
				case Opcodes.ISTORE:
					 return "ISTORE";
				case Opcodes.LSTORE:
					 return "LSTORE";
				case Opcodes.FSTORE:
					 return "FSTORE";
				case Opcodes.DSTORE:
					 return "DSTORE";
				case Opcodes.ASTORE:
					 return "ASTORE";
				case Opcodes.RET:
					 return "RET";
				// visitTypeInsn
				case Opcodes.NEW:
					 return "NEW";
				case Opcodes.ANEWARRAY:
					 return "ANEWARRAY";
				case Opcodes.CHECKCAST:
					 return "CHECKCAST";
				case Opcodes.INSTANCEOF:
					 return "INSTANCEOF";
				// visitFieldInsn
				case Opcodes.GETSTATIC:
					 return "GETSTATIC";
				case Opcodes.PUTSTATIC:
					 return "PUTSTATIC";
				case Opcodes.GETFIELD:
					 return "GETFIELD";
				case Opcodes.PUTFIELD:
					 return "PUTFIELD";
				// visitMethodInsn
				case Opcodes.INVOKEVIRTUAL:
					 return "INVOKEVIRTUAL";
				case Opcodes.INVOKESPECIAL:
					 return "INVOKESPECIAL";
				case Opcodes.INVOKESTATIC:
					 return "INVOKESTATIC";
				case Opcodes.INVOKEINTERFACE:
					 return "INVOKEINTERFACE";
				// visitJumpInsn
				case Opcodes.IFEQ:
					 return "IFEQ";
				case Opcodes.IFNE:
					 return "IFNE";
				case Opcodes.IFLT:
					 return "IFLT";
				case Opcodes.IFGE:
					 return "IFGE";
				case Opcodes.IFGT:
					 return "IFGT";
				case Opcodes.IFLE:
					 return "IFLE";
				case Opcodes.IF_ICMPEQ:
					 return "IF_ICMPEQ";
				case Opcodes.IF_ICMPNE:
					 return "IF_ICMPNE";
				case Opcodes.IF_ICMPLT:
					 return "IF_ICMPLT";
				case Opcodes.IF_ICMPGE:
					 return "IF_ICMPGE";
				case Opcodes.IF_ICMPGT:
					 return "IF_ICMPGT";
				case Opcodes.IF_ICMPLE:
					 return "IF_ICMPLE";
				case Opcodes.IF_ACMPEQ:
					 return "IF_ACMPEQ";
				case Opcodes.IF_ACMPNE:
					 return "IF_ACMPNE";
				case Opcodes.GOTO:
					 return "GOTO";
				case Opcodes.JSR:
					 return "JSR";
				case Opcodes.IFNULL:
					 return "IFNULL";
				case Opcodes.IFNONNULL:
					 return "IFNONNULL";
				default:
					 throw new System.ArgumentException( "unknown opcode: " + opcode );
				}
		  }
	 }

}