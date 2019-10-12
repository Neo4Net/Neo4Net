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
namespace Neo4Net.Codegen
{

	public class CompilationFailureException : Exception
	{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.List<? extends javax.tools.Diagnostic<?>> diagnostics;
		 private readonly IList<Diagnostic<object>> _diagnostics;

		 public CompilationFailureException<T1>( IList<T1> diagnostics ) where T1 : javax.tools.Diagnostic<T1> : base( string.Format( "Compilation failed with {0:D} reported issues.{1}", diagnostics.Count, Source( diagnostics ) ) )
		 {
			  this._diagnostics = diagnostics;
		 }

		 public override string ToString()
		 {
			  StringWriter result = ( new StringWriter() ).append(base.ToString());
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (javax.tools.Diagnostic<?> diagnostic : diagnostics)
			  foreach ( Diagnostic<object> diagnostic in _diagnostics )
			  {
					Format( result.append( "\n\t\t" ), diagnostic );
			  }
			  return result.ToString();
		 }

		 private static string Source<T1>( IList<T1> diagnostics ) where T1 : javax.tools.Diagnostic<T1>
		 {
			  ISet<JavaFileObject> sources = null;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (javax.tools.Diagnostic<?> diagnostic : diagnostics)
			  foreach ( Diagnostic<object> diagnostic in diagnostics )
			  {
					object source = diagnostic.Source;
					if ( source is JavaFileObject )
					{
						 JavaFileObject file = ( JavaFileObject ) source;
						 if ( file.Kind == JavaFileObject.Kind.SOURCE )
						 {
							  if ( sources == null )
							  {
									sources = Collections.newSetFromMap( new IdentityHashMap<JavaFileObject, bool>() );
							  }
							  sources.Add( file );
						 }
					}
			  }
			  if ( sources == null )
			  {
					return "";
			  }
			  StringBuilder result = new StringBuilder();
			  foreach ( JavaFileObject source in sources )
			  {
					int pos = result.Length;
					result.Append( "\nSource file " ).Append( source.Name ).Append( ":\n" );
					try
					{
						 CharSequence content = source.getCharContent( true );
						 result.Append( string.Format( "{0,4:D}: ", 1 ) );
						 for ( int line = 1, i = 0; i < content.length(); i++ )
						 {
							  char c = content.charAt( i );
							  result.Append( c );
							  if ( c == '\n' )
							  {
									result.Append( string.Format( "{0,4:D}: ", ++line ) );
							  }
						 }
					}
					catch ( IOException )
					{
						 result.Length = pos;
					}
			  }
			  return result.ToString();
		 }

		 public static void Format<T1>( Appendable result, Diagnostic<T1> diagnostic )
		 {
			  try
			  {
					object source = diagnostic.Source;
					if ( source != null )
					{
						 result.append( diagnostic.Kind.name() ).append(" on line ").append(Convert.ToString(diagnostic.LineNumber)).append(" in ").append(source.ToString()).append(": ").append(diagnostic.getMessage(null));
					}
					else
					{
						 result.append( diagnostic.getMessage( null ) );
					}
			  }
			  catch ( IOException e )
			  {
					throw new Exception( "Failed to append.", e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public java.util.List<javax.tools.Diagnostic<?>> getDiagnostics()
		 public virtual IList<Diagnostic<object>> Diagnostics
		 {
			 get
			 {
				  return unmodifiableList( _diagnostics );
			 }
		 }
	}

}