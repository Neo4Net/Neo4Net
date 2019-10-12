using System;
using System.Collections.Generic;
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
namespace Neo4Net.Upgrade.Loader
{

	/// <summary>
	/// Loader that will try to load jar that is embedded into some other jar.
	/// <para>
	/// In case if we do not want jar file to be visible for classloaders we can embed it into other jar as resource.
	/// This loaded will allow us to use those jars later on and will provide facility to load classes from them.
	/// </para>
	/// <para>
	/// Please note that this class should not be used as a generic class loader.
	/// Example use case: ability to ship several versions of artifact for migration purposes (lucene indexes migrator as
	/// example)
	/// </para>
	/// </summary>
	public class EmbeddedJarLoader : AutoCloseable
	{
		 private string[] _jars;
		 private ICollection<File> _extractedFiles = new List<File>();
		 private URLClassLoader _jarsClassLoader;

		 internal EmbeddedJarLoader( params string[] jars )
		 {
			  this._jars = jars;
		 }

		 /// <summary>
		 /// Load class from embedded jar
		 /// </summary>
		 /// <param name="className"> fully qualified class name </param>
		 /// <returns> Loaded class </returns>
		 /// <exception cref="ClassNotFoundException"> in case if specified class not found </exception>
		 /// <exception cref="IOException"> if class cannot be extracted </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Class loadEmbeddedClass(String className) throws ClassNotFoundException, java.io.IOException
		 public virtual Type LoadEmbeddedClass( string className )
		 {
			  return JarsClassLoader.loadClass( className );
		 }

		 /// <summary>
		 /// Get class loaded of embedded jar files
		 /// </summary>
		 /// <returns> jar files class loader </returns>
		 /// <exception cref="IOException"> if exception occurred during class loader construction </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public ClassLoader getJarsClassLoader() throws java.io.IOException
		 public virtual ClassLoader JarsClassLoader
		 {
			 get
			 {
				  if ( _jarsClassLoader == null )
				  {
						_jarsClassLoader = BuildJarClassLoader();
				  }
				  return _jarsClassLoader;
			 }
		 }

		 /// <summary>
		 /// Release class loader that was used for class loading and attempt to delete all extracted jars.
		 /// If deletion will not succeed, they will be deleted on JVM exit.
		 /// </summary>
		 /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  if ( _jarsClassLoader != null )
			  {
					_jarsClassLoader.close();
			  }

			  _extractedFiles.forEach( File.delete );
			  _extractedFiles.Clear();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.net.URLClassLoader buildJarClassLoader() throws java.io.IOException
		 private URLClassLoader BuildJarClassLoader()
		 {
			  ICollection<File> jarFiles = Jars;
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  URL[] urls = jarFiles.Select( EmbeddedJarLoader.getFileURL ).ToArray( URL[]::new );
			  return new URLClassLoader( urls, null );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.Collection<java.io.File> getJars() throws java.io.IOException
		 private ICollection<File> Jars
		 {
			 get
			 {
				  ICollection<File> jarFiles = new List<File>();
				  foreach ( string jar in _jars )
				  {
						URL url = this.GetType().ClassLoader.getResource(jar);
						if ( url == null )
						{
							 // we can't find jar file as a resource (for example when running from IDE)
							 // will try to find it in relative parent directory
							 // build should be executed at least once for this to work
							 jarFiles.Add( LoadJarFromRelativePath( jar ) );
						}
						else
						{
							 // jar file can be found as resource, lets extract it and use
							 File extractedFile = ExtractJar( url, jar );
							 jarFiles.Add( extractedFile );
							 _extractedFiles.Add( extractedFile );
						}
				  }
				  return jarFiles;
			 }
		 }

		 /// <summary>
		 /// Extract jar that is stored ad resource in a parent jar into temporary file </summary>
		 /// <param name="resourceUrl"> resource jar resourceUrl </param>
		 /// <param name="jar"> jar resource path </param>
		 /// <returns> jar temporary file </returns>
		 /// <exception cref="IOException"> on exception during jar extractions </exception>
		 /// <exception cref="EmbeddedJarNotFoundException"> if jar not found or can't be extracted. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File extractJar(java.net.URL resourceUrl, String jar) throws java.io.IOException
		 private File ExtractJar( URL resourceUrl, string jar )
		 {
			  URLConnection connection = resourceUrl.openConnection();
			  if ( connection is JarURLConnection )
			  {
					JarURLConnection urlConnection = ( JarURLConnection ) connection;
					JarFile jarFile = urlConnection.JarFile;
					JarEntry jarEntry = urlConnection.JarEntry;
					if ( jarEntry == null )
					{
						 throw new EmbeddedJarNotFoundException( "Jar file '" + jar + "' not found." );
					}
					return Extract( jarFile, jarEntry );
			  }
			  else
			  {
					throw new EmbeddedJarNotFoundException( "Jar file '" + jar + "' not found." );
			  }
		 }

		 /// <summary>
		 /// Try to load jar from relative ../lib/ directory for cases when we do not have jars in a class path. </summary>
		 /// <param name="jar"> - path to a jar file to load. </param>
		 /// <returns> loaded jar file </returns>
		 /// <exception cref="EmbeddedJarNotFoundException"> if jar not exist or file name can't be represented as URI. </exception>
		 private File LoadJarFromRelativePath( string jar )
		 {
			  try
			  {
					CodeSource codeSource = this.GetType().ProtectionDomain.CodeSource;
					URI uri = codeSource.Location.toURI();
					File jarFile = new File( Path.GetDirectoryName( uri ), jar );
					if ( !jarFile.exists() )
					{
						 throw new EmbeddedJarNotFoundException( "Jar file '" + jar + "' not found." );
					}
					return jarFile;
			  }
			  catch ( URISyntaxException )
			  {
					throw new EmbeddedJarNotFoundException( "Jar file '" + jar + "' not found." );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File extract(java.util.jar.JarFile jarFile, java.util.jar.JarEntry jarEntry) throws java.io.IOException
		 private File Extract( JarFile jarFile, JarEntry jarEntry )
		 {
			  File extractedFile = CreateTempFile( jarEntry );
			  using ( Stream jarInputStream = jarFile.getInputStream( jarEntry ) )
			  {
					Files.copy( jarInputStream, extractedFile.toPath(), StandardCopyOption.REPLACE_EXISTING );
			  }
			  return extractedFile;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File createTempFile(java.util.jar.JarEntry jarEntry) throws java.io.IOException
		 private File CreateTempFile( JarEntry jarEntry )
		 {
			  File tempFile = File.createTempFile( jarEntry.Name, "jar" );
			  tempFile.deleteOnExit();
			  return tempFile;
		 }

		 private static URL GetFileURL( File file )
		 {
			  try
			  {
					return file.toURI().toURL();
			  }
			  catch ( MalformedURLException e )
			  {
					throw new Exception( "Can't convert file " + file + " URI into URL.", e );
			  }
		 }

	}

}