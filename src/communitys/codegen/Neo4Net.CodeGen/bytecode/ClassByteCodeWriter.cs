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
	using ClassWriter = org.objectweb.asm.ClassWriter;
	using FieldVisitor = org.objectweb.asm.FieldVisitor;
	using MethodVisitor = org.objectweb.asm.MethodVisitor;



//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.ByteCodeUtils.byteCodeName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.ByteCodeUtils.outerName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.ByteCodeUtils.signature;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.ByteCodeUtils.typeName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.ACC_PUBLIC;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.ACC_STATIC;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.ACC_SUPER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.PUTSTATIC;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.RETURN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.V1_8;

	internal class ClassByteCodeWriter : ClassEmitter
	{
		 private readonly ClassWriter _classWriter;
		 private readonly ClassVisitor _classVisitor;
		 private readonly TypeReference _type;
		 private readonly IDictionary<FieldReference, Expression> _staticFields = new Dictionary<FieldReference, Expression>();
		 private readonly TypeReference @base;

		 internal ClassByteCodeWriter( TypeReference type, TypeReference @base, TypeReference[] interfaces )
		 {
			  this._classWriter = new ClassWriter( ClassWriter.COMPUTE_FRAMES );
			  this._classVisitor = _classWriter; // this separation is useful if we want to add intermediary visitors
			  string[] iNames = new string[interfaces.Length];
			  for ( int i = 0; i < interfaces.Length; i++ )
			  {
					iNames[i] = byteCodeName( interfaces[i] );
			  }
			  _classVisitor.visit( V1_8, ACC_PUBLIC + ACC_SUPER, byteCodeName( type ), signature( type ), byteCodeName( @base ), iNames.Length != 0 ? iNames : null );
			  if ( @base.InnerClass )
			  {
					_classVisitor.visitInnerClass( byteCodeName( @base ), outerName( @base ), @base.SimpleName(), ACC_PUBLIC + ACC_STATIC );
			  }
			  this._type = type;
			  this.@base = @base;
		 }

		 public override MethodEmitter Method( MethodDeclaration signature )
		 {
			  return new MethodByteCodeEmitter( _classVisitor, signature, @base );
		 }

		 public override void Field( FieldReference field, Expression value )
		 {
			  //keep track of all static field->value, and initiate in <clinit> in done
			  if ( Modifier.isStatic( field.Modifiers() ) && value != null )
			  {
					_staticFields[field] = value;
			  }
			  FieldVisitor fieldVisitor = _classVisitor.visitField( field.Modifiers(), field.Name(), typeName(field.Type()), signature(field.Type()), null );
			  fieldVisitor.visitEnd();
		 }

		 public override void Done()
		 {
			  if ( _staticFields.Count > 0 )
			  {
					MethodVisitor methodVisitor = _classVisitor.visitMethod( ACC_STATIC, "<clinit>", "()V", null, null );
					ByteCodeExpressionVisitor expressionVisitor = new ByteCodeExpressionVisitor( methodVisitor );
					methodVisitor.visitCode();
					foreach ( KeyValuePair<FieldReference, Expression> entry in _staticFields.SetOfKeyValuePairs() )
					{
						 FieldReference field = entry.Key;
						 Expression value = entry.Value;
						 value.Accept( expressionVisitor );
						 methodVisitor.visitFieldInsn( PUTSTATIC, byteCodeName( field.Owner() ), field.Name(), typeName(field.Type()) );
					}
					methodVisitor.visitInsn( RETURN );
					methodVisitor.visitMaxs( 0, 0 );
					methodVisitor.visitEnd();
			  }
			  _classVisitor.visitEnd();
		 }

		 internal virtual ByteCodes ToByteCodes()
		 {
			  sbyte[] bytecode = _classWriter.toByteArray();
			  return new ByteCodesAnonymousInnerClass( this, bytecode );
		 }

		 private class ByteCodesAnonymousInnerClass : ByteCodes
		 {
			 private readonly ClassByteCodeWriter _outerInstance;

			 private sbyte[] _bytecode;

			 public ByteCodesAnonymousInnerClass( ClassByteCodeWriter outerInstance, sbyte[] bytecode )
			 {
				 this.outerInstance = outerInstance;
				 this._bytecode = bytecode;
			 }

			 public string name()
			 {
				  return _outerInstance.type.fullName();
			 }

			 public ByteBuffer bytes()
			 {
				  return ByteBuffer.wrap( _bytecode );
			 }
		 }
	}

}