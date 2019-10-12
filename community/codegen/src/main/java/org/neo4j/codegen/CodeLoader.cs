using System;
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
namespace Org.Neo4j.Codegen
{

	internal class CodeLoader : ClassLoader
	{
		 private readonly IDictionary<string, ByteCodes> _bytecodes = new Dictionary<string, ByteCodes>();

		 internal CodeLoader( ClassLoader parent ) : base( parent )
		 {
		 }

		 internal virtual void AddSources<T1>( IEnumerable<T1> sources, ByteCodeVisitor visitor ) where T1 : ByteCodes
		 {
			 lock ( this )
			 {
				  foreach ( ByteCodes source in sources )
				  {
						visitor.VisitByteCode( source.Name(), source.Bytes().duplicate() );
						_bytecodes[source.Name()] = source;
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected synchronized Class findClass(String name) throws ClassNotFoundException
		 protected internal override Type FindClass( string name )
		 {
			 lock ( this )
			 {
				  ByteCodes codes = _bytecodes.Remove( name );
				  if ( codes == null )
				  {
						throw new ClassNotFoundException( name );
				  }
				  string packageName = name.Substring( 0, name.LastIndexOf( '.' ) );
				  if ( getPackage( packageName ) == null )
				  {
						definePackage( packageName, "", "", "", "", "", "", null );
				  }
				  return defineClass( name, codes.Bytes(), null );
			 }
		 }
	}

}