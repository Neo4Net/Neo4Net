using System;
using System.IO;

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
namespace Neo4Net.Test
{

	public class Unzip
	{
		 private Unzip()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static java.io.File unzip(Class testClass, String resource, java.io.File targetDirectory) throws java.io.IOException
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 public static File UnzipConflict( Type testClass, string resource, File targetDirectory )
		 {
			  Stream source = testClass.getResourceAsStream( resource );
			  if ( source == null )
			  {
					throw new FileNotFoundException( "Could not find resource '" + resource + "' to unzip" );
			  }

			  try
			  {
					  using ( ZipInputStream zipStream = new ZipInputStream( source ) )
					  {
						ZipEntry entry;
						sbyte[] scratch = new sbyte[8096];
						while ( ( entry = zipStream.NextEntry ) != null )
						{
							 if ( entry.Directory )
							 {
								  ( new File( targetDirectory, entry.Name ) ).mkdirs();
							 }
							 else
							 {
								  using ( Stream file = new BufferedOutputStream( new FileStream( targetDirectory, entry.Name, FileMode.Create, FileAccess.Write ) ) )
								  {
										long toCopy = entry.Size;
										while ( toCopy > 0 )
										{
											 int read = zipStream.read( scratch );
											 file.Write( scratch, 0, read );
											 toCopy -= read;
										}
								  }
							 }
							 zipStream.closeEntry();
						}
					  }
			  }
			  finally
			  {
					source.Close();
			  }
			  return targetDirectory;
		 }
	}

}