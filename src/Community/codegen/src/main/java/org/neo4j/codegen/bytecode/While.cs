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
namespace Neo4Net.Codegen.bytecode
{

	using Label = org.objectweb.asm.Label;
	using MethodVisitor = org.objectweb.asm.MethodVisitor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.Opcodes.GOTO;

	public class While : Block
	{
		 private readonly MethodVisitor _methodVisitor;
		 private readonly Label _repeat;
		 private readonly Label _done;

		 public While( MethodVisitor methodVisitor, Label repeat, Label done )
		 {
			  this._methodVisitor = methodVisitor;
			  this._repeat = repeat;
			  this._done = done;
		 }

		 public virtual void ContinueBlock()
		 {
			  _methodVisitor.visitJumpInsn( GOTO, _repeat );
		 }

		 public override void EndBlock()
		 {
			  _methodVisitor.visitJumpInsn( GOTO, _repeat );
			  _methodVisitor.visitLabel( _done );
		 }
	}

}