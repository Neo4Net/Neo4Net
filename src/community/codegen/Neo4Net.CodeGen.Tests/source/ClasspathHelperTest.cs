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
namespace Neo4Net.CodeGen.Source
{
	using Test = org.junit.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.empty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasItems;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.source.ClasspathHelper.fullClasspathFor;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.codegen.source.ClasspathHelper.fullClasspathStringFor;

	public class ClasspathHelperTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFailForNullClassLoader()
		 public virtual void ShouldNotFailForNullClassLoader()
		 {
			  assertThat( fullClasspathFor( null ), not( empty() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWorkForClassLoaderWithNoParent() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWorkForClassLoaderWithNoParent()
		 {
			  // Given
			  ClassLoader loader = new URLClassLoader( Urls( "file:///file1", "file:///file2" ), null );

			  // When
			  ISet<string> elements = fullClasspathFor( loader );

			  // Then
			  assertThat( elements, hasItems( PathTo( "file1" ), PathTo( "file2" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWorkForClassLoaderWithSingleParent() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWorkForClassLoaderWithSingleParent()
		 {
			  // Given
			  ClassLoader parent = new URLClassLoader( Urls( "file:///file1", "file:///file2" ), null );
			  ClassLoader child = new URLClassLoader( Urls( "file:///file3" ), parent );

			  // When
			  ISet<string> elements = fullClasspathFor( child );

			  // Then
			  assertThat( elements, hasItems( PathTo( "file1" ), PathTo( "file2" ), PathTo( "file3" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWorkForClassLoaderHierarchy() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWorkForClassLoaderHierarchy()
		 {
			  // Given
			  ClassLoader loader1 = new URLClassLoader( Urls( "file:///file1" ), null );
			  ClassLoader loader2 = new URLClassLoader( Urls( "file:///file2" ), loader1 );
			  ClassLoader loader3 = new URLClassLoader( Urls( "file:///file3" ), loader2 );
			  ClassLoader loader4 = new URLClassLoader( Urls( "file:///file4" ), loader3 );

			  // When
			  ISet<string> elements = fullClasspathFor( loader4 );

			  // Then
			  assertThat( elements, hasItems( PathTo( "file1" ), PathTo( "file2" ), PathTo( "file3" ), PathTo( "file4" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnCorrectClasspathString() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnCorrectClasspathString()
		 {
			  // Given
			  ClassLoader parent = new URLClassLoader( Urls( "file:///foo" ), null );
			  ClassLoader child = new URLClassLoader( Urls( "file:///bar" ), parent );

			  // When
			  string classpath = fullClasspathStringFor( child );

			  // Then
			  assertThat( classpath, containsString( PathTo( "bar" ) + File.pathSeparator + PathTo( "foo" ) ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static java.net.URL[] urls(String... strings) throws java.net.MalformedURLException
		 private static URL[] Urls( params string[] strings )
		 {
			  URL[] urls = new URL[strings.Length];
			  for ( int i = 0; i < strings.Length; i++ )
			  {
					urls[i] = new URL( strings[i] );
			  }
			  return urls;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static String pathTo(String fileName) throws java.io.IOException
		 private static string PathTo( string fileName )
		 {
			  File currentDir = ( new File( "." ) ).CanonicalFile;
			  File root = currentDir.ParentFile.CanonicalFile;
			  while ( root.ParentFile != null )
			  {
					root = root.ParentFile.CanonicalFile;
			  }
			  return ( new File( root, fileName ) ).CanonicalPath;
		 }
	}

}