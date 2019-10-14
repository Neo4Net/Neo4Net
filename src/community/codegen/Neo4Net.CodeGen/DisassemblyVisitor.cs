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
//	import static org.neo4j.codegen.ByteCodeVisitor.printer;

	public abstract class DisassemblyVisitor : ByteCodeVisitor, CodeGeneratorOption
	{
		public abstract ByteCodeVisitor_Printer Printer( java.io.PrintStream @out );
		public abstract ByteCodeVisitor_Printer Printer( PrintWriter @out );
		 public override void ApplyTo( object target )
		 {
			  if ( target is ByteCodeVisitor_Configurable )
			  {
					( ( ByteCodeVisitor_Configurable ) target ).AddByteCodeVisitor( this );
			  }
		 }

		 public override void VisitByteCode( string name, ByteBuffer bytes )
		 {
			  StringWriter target = new StringWriter();
			  using ( PrintWriter writer = new PrintWriter( target ) )
			  {
					Printer( writer ).visitByteCode( name, bytes );
			  }
			  VisitDisassembly( name, target.Buffer );
		 }

		 protected internal abstract void VisitDisassembly( string className, CharSequence disassembly );
	}

}