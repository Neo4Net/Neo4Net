using System.Collections.Generic;

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

	internal class JdkCompiler : SourceCompiler
	{
		 public static readonly SourceCompiler_Factory FACTORY = new SourceCompiler_FactoryAnonymousInnerClass();

		 private class SourceCompiler_FactoryAnonymousInnerClass : SourceCompiler_Factory
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: SourceCompiler sourceCompilerFor(Configuration configuration, org.neo4j.codegen.CodeGenerationStrategy<?> strategy) throws org.neo4j.codegen.CodeGenerationStrategyNotSupportedException
			 internal override SourceCompiler sourceCompilerFor<T1>( Configuration configuration, CodeGenerationStrategy<T1> strategy )
			 {
				  JavaCompiler jdkCompiler = ToolProvider.SystemJavaCompiler;
				  if ( jdkCompiler == null )
				  {
						throw new CodeGenerationStrategyNotSupportedException( strategy, "no java source compiler available" );
				  }
				  return new JdkCompiler( jdkCompiler, configuration );
			 }
		 }
		 private readonly JavaCompiler _compiler;
		 private readonly Configuration _configuration;

		 internal JdkCompiler( JavaCompiler compiler, Configuration configuration )
		 {
			  this._compiler = compiler;
			  this._configuration = configuration;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Iterable<? extends org.neo4j.codegen.ByteCodes> compile(java.util.List<JavaSourceFile> sourceFiles, ClassLoader loader) throws org.neo4j.codegen.CompilationFailureException
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public override IEnumerable<ByteCodes> Compile( IList<JavaSourceFile> sourceFiles, ClassLoader loader )
		 {
			  DiagnosticCollector<JavaFileObject> diagnostics = new DiagnosticCollector<JavaFileObject>();

			  FileManager fileManager = new FileManager( _compiler.getStandardFileManager( diagnostics, _configuration.locale(), _configuration.charset() ) );

			  JavaCompiler.CompilationTask task = _compiler.getTask( _configuration.errorWriter(), fileManager, diagnostics, _configuration.options(), null, sourceFiles );

			  _configuration.processors( task );
			  if ( task.call() )
			  {
					_configuration.warningsHandler().handle(diagnostics.Diagnostics);
					return fileManager.Bytecodes();
			  }
			  else
			  {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<javax.tools.Diagnostic<?>> issues = (java.util.List) diagnostics.getDiagnostics();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
					IList<Diagnostic<object>> issues = ( System.Collections.IList ) diagnostics.Diagnostics;
					throw new CompilationFailureException( issues );
			  }
		 }
	}

}