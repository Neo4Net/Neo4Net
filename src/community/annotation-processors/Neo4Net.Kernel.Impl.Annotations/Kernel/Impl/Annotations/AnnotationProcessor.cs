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
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
//
namespace Neo4Net.Kernel.Impl.Annotations
{

	public abstract class AnnotationProcessor : AbstractProcessor
	{
		 public override bool Process<T1>( ISet<T1> annotations, RoundEnvironment roundEnv ) where T1 : javax.lang.model.element.TypeElement
		 {
			  foreach ( TypeElement type in annotations )
			  {
					foreach ( Element annotated in roundEnv.getElementsAnnotatedWith( type ) )
					{
						 foreach ( AnnotationMirror mirror in annotated.AnnotationMirrors )
						 {
							  if ( mirror.AnnotationType.asElement().Equals(type) )
							  {
									try
									{
										 Process( type, annotated, mirror, processingEnv.ElementUtils.getElementValuesWithDefaults( mirror ) );
									}
									catch ( Exception e )
									{
										 Console.WriteLine( e.ToString() );
										 Console.Write( e.StackTrace );
										 processingEnv.Messager.printMessage( Kind.ERROR, "Internal error: " + e, annotated, mirror );
									}
							  }
						 }
					}
			  }
			  return false;
		 }

		 protected internal void Warn( Element element, string message )
		 {
			  processingEnv.Messager.printMessage( Kind.WARNING, message, element );
		 }

		 protected internal void Warn( Element element, AnnotationMirror annotation, string message )
		 {
			  processingEnv.Messager.printMessage( Kind.WARNING, message, element, annotation );
		 }

		 protected internal void Error( Element element, string message )
		 {
			  processingEnv.Messager.printMessage( Kind.ERROR, message, element );
		 }

		 protected internal void Error( Element element, AnnotationMirror annotation, string message )
		 {
			  processingEnv.Messager.printMessage( Kind.ERROR, message, element, annotation );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract void process(javax.lang.model.element.TypeElement annotationType, javax.lang.model.element.Element annotated, javax.lang.model.element.AnnotationMirror annotation, java.util.Map<? extends javax.lang.model.element.ExecutableElement, ? extends javax.lang.model.element.AnnotationValue> values) throws java.io.IOException;
		 protected internal abstract void process<T1>( TypeElement annotationType, Element annotated, AnnotationMirror annotation, IDictionary<T1> values ) where T1 : javax.lang.model.element.ExecutableElement;

		 private static Pattern _nl = Pattern.compile( "\n" );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void addTo(String line, String... path) throws java.io.IOException
		 internal virtual void AddTo( string line, params string[] path )
		 {
			  FileObject fo = processingEnv.Filer.getResource( StandardLocation.CLASS_OUTPUT, "", path( path ) );
			  URI uri = fo.toUri();
			  File file;
			  try
			  {
					file = new File( uri );
			  }
			  catch ( Exception )
			  {
					file = new File( uri.ToString() );
			  }
			  if ( file.exists() )
			  {
					foreach ( string previous in _nl.split( fo.getCharContent( true ), 0 ) )
					{
						 if ( line.Equals( previous ) )
						 {
							  return;
						 }
					}
			  }
			  else
			  {
					file.ParentFile.mkdirs();
			  }

			  using ( PrintWriter writer = new PrintWriter( new StreamWriter( new FileStream( file, true ), Encoding.UTF8 ) ) )
			  {
					writer.append( line ).append( "\n" );
			  }
		 }

		 private string Path( string[] path )
		 {
			  StringBuilder filename = new StringBuilder();
			  string sep = "";
			  foreach ( string part in path )
			  {
					filename.Append( sep ).Append( part );
					sep = "/";
			  }
			  return filename.ToString();
		 }
	}

}