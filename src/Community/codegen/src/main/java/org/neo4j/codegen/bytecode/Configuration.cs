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
	internal class Configuration
	{
		 private ByteCodeChecker _bytecodeChecker;

		 public virtual Configuration WithFlag( ByteCode flag )
		 {
			  return this;
		 }

		 public virtual void WithBytecodeChecker( ByteCodeChecker checker )
		 {
			  if ( _bytecodeChecker == null )
			  {
					_bytecodeChecker = checker;
			  }
			  else
			  {
					throw new System.NotSupportedException( "multiple bytecode checkers" );
			  }
		 }

		 internal virtual ByteCodeChecker BytecodeChecker()
		 {
			  return _bytecodeChecker;
		 }
	}

}