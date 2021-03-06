﻿using System.Collections.Generic;

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

	using Org.Neo4j.Codegen;

	internal interface SourceCompiler
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Iterable<? extends org.neo4j.codegen.ByteCodes> compile(java.util.List<JavaSourceFile> sourceFiles, ClassLoader loader) throws org.neo4j.codegen.CompilationFailureException;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 IEnumerable<ByteCodes> Compile( IList<JavaSourceFile> sourceFiles, ClassLoader loader );
	}

	 internal abstract class SourceCompiler_Factory : CodeGeneratorOption
	 {
		  public override void ApplyTo( object target )
		  {
				if ( target is Configuration )
				{
					 Configuration configuration = ( Configuration ) target;
					 configuration.Compiler = this;
					 Configure( configuration );
				}
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract SourceCompiler sourceCompilerFor(Configuration configuration, org.neo4j.codegen.CodeGenerationStrategy<?> strategy) throws org.neo4j.codegen.CodeGenerationStrategyNotSupportedException;
		  internal abstract SourceCompiler sourceCompilerFor<T1>( Configuration configuration, CodeGenerationStrategy<T1> strategy );

		  internal virtual void Configure( Configuration configuration )
		  {
		  }
	 }

}