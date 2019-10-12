using System.Collections.Generic;

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
namespace Neo4Net.Codegen.source
{

	internal class FileManager : ForwardingJavaFileManager<StandardJavaFileManager>
	{
		 private readonly IDictionary<string, ClassFile> _classes = new Dictionary<string, ClassFile>();

		 internal FileManager( StandardJavaFileManager fileManager ) : base( fileManager )
		 {
		 }

		 public override JavaFileObject GetJavaFileForOutput( Location location, string className, JavaFileObject.Kind kind, FileObject sibling )
		 {
			  ClassFile file = new ClassFile( className );
			  _classes[className] = file;
			  return file;
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public Iterable<? extends org.neo4j.codegen.ByteCodes> bytecodes()
		 public virtual IEnumerable<ByteCodes> Bytecodes()
		 {
			  return _classes.Values;
		 }

		 private class ClassFile : SimpleJavaFileObject, ByteCodes
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly MemoryStream BytesConflict = new MemoryStream();
			  internal readonly string ClassName;

			  protected internal ClassFile( string className ) : base( URI.create( "classes:/" + className.Replace( '.', '/' ) + Kind.CLASS.extension ), Kind.CLASS )
			  {
					this.ClassName = className;
			  }

			  public override Stream OpenOutputStream()
			  {
					return BytesConflict;
			  }

			  public override string Name()
			  {
					return ClassName;
			  }

			  public override ByteBuffer Bytes()
			  {
					return ByteBuffer.wrap( BytesConflict.toByteArray() );
			  }
		 }
	}

}