using System;
using System.IO;

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
namespace Org.Neo4j.Kernel.impl.proc
{

	/// <summary>
	/// Utility to create jar files containing classes from the current classpath.
	/// </summary>
	public class JarBuilder
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.net.URL createJarFor(java.io.File f, Class... classesToInclude) throws java.io.IOException
		 public virtual URL CreateJarFor( File f, params Type[] classesToInclude )
		 {
			  using ( FileStream fout = new FileStream( f, FileMode.Create, FileAccess.Write ), JarOutputStream jarOut = new JarOutputStream( fout ) )
			  {
					foreach ( Type target in classesToInclude )
					{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
						 string fileName = target.FullName.Replace( ".", "/" ) + ".class";
						 jarOut.putNextEntry( new ZipEntry( fileName ) );
						 jarOut.write( ClassCompiledBytes( fileName ) );
						 jarOut.closeEntry();
					}
			  }
			  return f.toURI().toURL();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private byte[] classCompiledBytes(String fileName) throws java.io.IOException
		 private sbyte[] ClassCompiledBytes( string fileName )
		 {
			  using ( Stream @in = this.GetType().ClassLoader.getResourceAsStream(fileName) )
			  {
					MemoryStream @out = new MemoryStream();
					while ( @in.available() > 0 )
					{
						 @out.WriteByte( @in.Read() );
					}

					return @out.toByteArray();
			  }
		 }
	}

}