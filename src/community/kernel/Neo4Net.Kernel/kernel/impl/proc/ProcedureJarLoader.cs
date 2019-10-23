using System;
using System.Collections.Generic;
using System.Reflection;

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
namespace Neo4Net.Kernel.impl.proc
{

	using Neo4Net.Collections;
	using Neo4Net.Collections;
	using KernelException = Neo4Net.Kernel.Api.Internal.Exceptions.KernelException;
	using CallableProcedure = Neo4Net.Kernel.api.proc.CallableProcedure;
	using CallableUserAggregationFunction = Neo4Net.Kernel.api.proc.CallableUserAggregationFunction;
	using CallableUserFunction = Neo4Net.Kernel.api.proc.CallableUserFunction;
	using Log = Neo4Net.Logging.Log;

	/// <summary>
	/// Given the location of a jarfile, reads the contents of the jar and returns compiled <seealso cref="CallableProcedure"/>
	/// instances.
	/// </summary>
	public class ProcedureJarLoader
	{
		 private readonly ReflectiveProcedureCompiler _compiler;
		 private readonly Log _log;

		 internal ProcedureJarLoader( ReflectiveProcedureCompiler compiler, Log log )
		 {
			  this._compiler = compiler;
			  this._log = log;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Callables loadProceduresFromDir(java.io.File root) throws java.io.IOException, org.Neo4Net.Kernel.Api.Internal.Exceptions.KernelException
		 public virtual Callables LoadProceduresFromDir( File root )
		 {
			  if ( root == null || !root.exists() )
			  {
					return Callables.Empty();
			  }

			  Callables @out = new Callables();

			  File[] dirListing = root.listFiles( ( dir, name ) => name.EndsWith( ".jar" ) );

			  if ( dirListing == null )
			  {
					return Callables.Empty();
			  }

			  if ( !AllJarFilesAreValidZipFiles( Stream.of( dirListing ) ) )
			  {
					throw new ZipException( "Some jar procedure files are invalid, see log for details." );
			  }

//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  URL[] jarFilesURLs = Stream.of( dirListing ).map( this.toURL ).toArray( URL[]::new );

			  URLClassLoader loader = new URLClassLoader( jarFilesURLs, this.GetType().ClassLoader );

			  foreach ( URL jarFile in jarFilesURLs )
			  {
					LoadProcedures( jarFile, loader, @out );
			  }
			  return @out;
		 }

		 private bool AllJarFilesAreValidZipFiles( Stream<File> jarFiles )
		 {
			  return jarFiles.allMatch(jarFile =>
			  {
				try
				{
					 ( new ZipFile( jarFile ) ).close();
					 return true;
				}
				catch ( IOException )
				{
					 _log.error( string.Format( "Plugin jar file: {0} corrupted.", jarFile ) );
					 return false;
				}
			  });
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Callables loadProcedures(java.net.URL jar, ClassLoader loader, Callables target) throws java.io.IOException, org.Neo4Net.Kernel.Api.Internal.Exceptions.KernelException
		 private Callables LoadProcedures( URL jar, ClassLoader loader, Callables target )
		 {
			  RawIterator<Type, IOException> classes = ListClassesIn( jar, loader );
			  while ( classes.HasNext() )
			  {
					Type next = classes.Next();
					target.AddAllProcedures( _compiler.compileProcedure( next, null, false ) );
					target.AddAllFunctions( _compiler.compileFunction( next ) );
					target.AddAllAggregationFunctions( _compiler.compileAggregationFunction( next ) );
			  }
			  return target;
		 }

		 private URL ToURL( File f )
		 {
			  try
			  {
					return f.toURI().toURL();
			  }
			  catch ( MalformedURLException e )
			  {
					throw new Exception( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.collection.RawIterator<Class,java.io.IOException> listClassesIn(java.net.URL jar, ClassLoader loader) throws java.io.IOException
		 private RawIterator<Type, IOException> ListClassesIn( URL jar, ClassLoader loader )
		 {
			  ZipInputStream zip = new ZipInputStream( jar.openStream() );

			  return new PrefetchingRawIteratorAnonymousInnerClass( this, jar, loader, zip );
		 }

		 private class PrefetchingRawIteratorAnonymousInnerClass : PrefetchingRawIterator<Type, IOException>
		 {
			 private readonly ProcedureJarLoader _outerInstance;

			 private URL _jar;
			 private ClassLoader _loader;
			 private ZipInputStream _zip;

			 public PrefetchingRawIteratorAnonymousInnerClass( ProcedureJarLoader outerInstance, URL jar, ClassLoader loader, ZipInputStream zip )
			 {
				 this.outerInstance = outerInstance;
				 this._jar = jar;
				 this._loader = loader;
				 this._zip = zip;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected Class fetchNextOrNull() throws java.io.IOException
			 protected internal override Type fetchNextOrNull()
			 {
				  try
				  {
						while ( true )
						{
							 ZipEntry nextEntry = _zip.NextEntry;
							 if ( nextEntry == null )
							 {
								  _zip.close();
								  return null;
							 }

							 string name = nextEntry.Name;
							 if ( name.EndsWith( ".class", StringComparison.Ordinal ) )
							 {
								  string className = name.Substring( 0, name.Length - ".class".Length ).Replace( '/', '.' );

								  try
								  {
										Type aClass = _loader.loadClass( className );
										// We do getDeclaredMethods to trigger NoClassDefErrors, which loadClass above does
										// not do.
										// This way, even if some of the classes in a jar cannot be loaded, we still check
										// the others.
										aClass.GetMethods( BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance );
										return aClass;
								  }
								  catch ( Exception e ) when ( e is UnsatisfiedLinkError || e is NoClassDefFoundError || e is Exception )
								  {
										_outerInstance.log.warn( "Failed to load `%s` from plugin jar `%s`: %s", className, _jar.File, e.Message );
								  }
							 }
						}
				  }
				  catch ( Exception e ) when ( e is IOException || e is Exception )
				  {
						_zip.close();
						throw e;
				  }
			 }
		 }

		 public class Callables
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly IList<CallableProcedure> ProceduresConflict = new List<CallableProcedure>();
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly IList<CallableUserFunction> FunctionsConflict = new List<CallableUserFunction>();
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly IList<CallableUserAggregationFunction> AggregationFunctionsConflict = new List<CallableUserAggregationFunction>();

			  public virtual void Add( CallableProcedure proc )
			  {
					ProceduresConflict.Add( proc );
			  }

			  public virtual void Add( CallableUserFunction func )
			  {
					FunctionsConflict.Add( func );
			  }

			  public virtual IList<CallableProcedure> Procedures()
			  {
					return ProceduresConflict;
			  }

			  public virtual IList<CallableUserFunction> Functions()
			  {
					return FunctionsConflict;
			  }

			  public virtual IList<CallableUserAggregationFunction> AggregationFunctions()
			  {
					return AggregationFunctionsConflict;
			  }

			  internal virtual void AddAllProcedures( IList<CallableProcedure> callableProcedures )
			  {
					( ( IList<CallableProcedure> )ProceduresConflict ).AddRange( callableProcedures );
			  }

			  internal virtual void AddAllFunctions( IList<CallableUserFunction> callableFunctions )
			  {
					( ( IList<CallableUserFunction> )FunctionsConflict ).AddRange( callableFunctions );
			  }

			  public virtual void AddAllAggregationFunctions( IList<CallableUserAggregationFunction> callableFunctions )
			  {
					( ( IList<CallableUserAggregationFunction> )AggregationFunctionsConflict ).AddRange( callableFunctions );
			  }

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal static Callables EmptyConflict = new Callables();

			  public static Callables Empty()
			  {
					return EmptyConflict;
			  }
		 }
	}

}