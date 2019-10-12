using System;
using System.Collections.Generic;
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
namespace Org.Neo4j.Codegen.source
{

	internal sealed class ClasspathHelper
	{
		 private ClasspathHelper()
		 {
			  throw new AssertionError( "Not for instantiation!" );
		 }

		 internal static string JavaClasspathString()
		 {
			  return System.getProperty( "java.class.path" );
		 }

		 internal static ISet<string> JavaClasspath()
		 {
			  string[] classpathElements = JavaClasspathString().Split(File.pathSeparator, true);
			  ISet<string> result = new LinkedHashSet<string>();

			  foreach ( string element in classpathElements )
			  {
					result.Add( CanonicalPath( element ) );
			  }

			  return result;
		 }

		 internal static string FullClasspathStringFor( ClassLoader classLoader )
		 {
			  ISet<string> classpathElements = FullClasspathFor( classLoader );
			  return FormClasspathString( classpathElements );
		 }

		 internal static ISet<string> FullClasspathFor( ClassLoader classLoader )
		 {
			  ISet<string> result = new LinkedHashSet<string>();

			  result.addAll( JavaClasspath() );

			  ClassLoader loader = classLoader;
			  while ( loader != null )
			  {
					if ( loader is URLClassLoader )
					{
						 foreach ( URL url in ( ( URLClassLoader ) loader ).URLs )
						 {
							  result.Add( CanonicalPath( url ) );
						 }
					}
					loader = loader.Parent;
			  }

			  return result;
		 }

		 private static string CanonicalPath( URL url )
		 {
			  return canonicalPath( url.Path );
		 }

		 private static string CanonicalPath( string path )
		 {
			  try
			  {
					File file = new File( path );
					return file.CanonicalPath;
			  }
			  catch ( IOException e )
			  {
					throw new Exception( "Failed to get canonical path for: '" + path + "'", e );
			  }
		 }

		 private static string FormClasspathString( ISet<string> classPathElements )
		 {
			  StringBuilder classpath = new StringBuilder();

			  IEnumerator<string> classPathElementsIterator = classPathElements.GetEnumerator();
			  while ( classPathElementsIterator.MoveNext() )
			  {
					classpath.Append( classPathElementsIterator.Current );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					if ( classPathElementsIterator.hasNext() )
					{
						 classpath.Append( File.pathSeparator );
					}
			  }

			  return classpath.ToString();
		 }
	}

}