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


	internal class ByteCodeGenerator : CodeGenerator
	{
		 private readonly Configuration _configuration;
		 private readonly IDictionary<TypeReference, ClassByteCodeWriter> _classes = new Dictionary<TypeReference, ClassByteCodeWriter>();

		 internal ByteCodeGenerator( ClassLoader parentClassLoader, Configuration configuration ) : base( parentClassLoader )
		 {
			  this._configuration = configuration;
		 }

		 protected internal override ClassEmitter Generate( TypeReference type, TypeReference @base, TypeReference[] interfaces )
		 {
			  ClassByteCodeWriter codeWriter = new ClassByteCodeWriter( type, @base, interfaces );
			  lock ( this )
			  {
					ClassByteCodeWriter old = _classes[type] = codeWriter;
					if ( old != null )
					{
						 _classes[type] = old;
						 throw new System.InvalidOperationException( "Trying to generate class twice: " + type );
					}
			  }

			  return codeWriter;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected Iterable<? extends Neo4Net.codegen.ByteCodes> compile(ClassLoader classpathLoader) throws Neo4Net.codegen.CompilationFailureException
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 protected internal override IEnumerable<ByteCodes> Compile( ClassLoader classpathLoader )
		 {
			  IList<ByteCodes> byteCodes = new List<ByteCodes>( _classes.Count );
			  foreach ( ClassByteCodeWriter writer in _classes.Values )
			  {
					byteCodes.Add( writer.ToByteCodes() );
			  }
			  ByteCodeChecker checker = _configuration.bytecodeChecker();
			  if ( checker != null )
			  {
					checker.Check( classpathLoader, byteCodes );
			  }
			  return byteCodes;
		 }
	}

}