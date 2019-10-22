﻿using System.Collections.Generic;
using System.Text;

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


	internal class SourceCodeGenerator : CodeGenerator
	{
		 private readonly Configuration _configuration;
		 private readonly IDictionary<TypeReference, StringBuilder> _classes = new Dictionary<TypeReference, StringBuilder>();
		 private readonly SourceCompiler _compiler;

		 internal SourceCodeGenerator( ClassLoader parentClassLoader, Configuration configuration, SourceCompiler compiler ) : base( parentClassLoader )
		 {
			  this._configuration = configuration;
			  this._compiler = compiler;
		 }

		 protected internal override ClassEmitter Generate( TypeReference type, TypeReference @base, TypeReference[] interfaces )
		 {
			  StringBuilder target = new StringBuilder();
			  lock ( this )
			  {
					StringBuilder old = _classes[type] = target;
					if ( old != null )
					{
						 _classes[type] = old;
						 throw new System.InvalidOperationException( "Trying to generate class twice: " + type );
					}
			  }
			  ClassSourceWriter writer = new ClassSourceWriter( target, _configuration );
			  writer.DeclarePackage( type );
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  writer.Javadoc( "Generated by " + this.GetType().FullName );
			  writer.PublicClass( type );
			  writer.ExtendClass( @base );
			  writer.Implement( interfaces );
			  writer.Begin();
			  return writer;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected Iterable<? extends org.Neo4Net.codegen.ByteCodes> compile(ClassLoader classpathLoader) throws org.Neo4Net.codegen.CompilationFailureException
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 protected internal override IEnumerable<ByteCodes> Compile( ClassLoader classpathLoader )
		 {
			  return _compiler.compile( SourceFiles(), classpathLoader );
		 }

		 private IList<JavaSourceFile> SourceFiles()
		 {
			 lock ( this )
			 {
				  IList<JavaSourceFile> sourceFiles = new List<JavaSourceFile>( _classes.Count );
				  foreach ( KeyValuePair<TypeReference, StringBuilder> entry in _classes.SetOfKeyValuePairs() )
				  {
						TypeReference reference = entry.Key;
						StringBuilder source = entry.Value;
						_configuration.visit( reference, source );
						sourceFiles.Add( new JavaSourceFile( _configuration.sourceBase().uri(reference.PackageName(), reference.Name(), JavaFileObject.Kind.SOURCE), source ) );
				  }
				  return sourceFiles;
			 }
		 }
	}

}