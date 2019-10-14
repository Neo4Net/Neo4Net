using System;
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

	using Neo4Net.CodeGen;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.CompilationFailureException.format;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.codegen.source.ClasspathHelper.fullClasspathStringFor;

	public sealed class SourceCode : CodeGeneratorOption
	{
		 public static readonly SourceCode SimplifyTryWithResource = new SourceCode( "SimplifyTryWithResource", InnerEnum.SimplifyTryWithResource );

		 private static readonly IList<SourceCode> valueList = new List<SourceCode>();

		 static SourceCode()
		 {
			 valueList.Add( SimplifyTryWithResource );
		 }

		 public enum InnerEnum
		 {
			 SimplifyTryWithResource
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private SourceCode( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }
		 internal Public const;
		 internal Public const;
		 internal Static readonly;
		 internal Public const;

		 private static Neo4Net.CodeGen.CodeGeneratorOption PrintWarningsTo( java.io.PrintStream err )
		 {
			  return new PrintWarningsOption( err );
		 }

//JAVA TO C# CONVERTER TODO TASK: Java to C# Converter does not convert types within enums:
//		 private static class CodeGenerationStrategyAnonymousInnerClass extends org.neo4j.codegen.CodeGenerationStrategy<Configuration>
	//	 {
	//		 @@Override protected Configuration createConfigurator(ClassLoader loader)
	//		 {
	//			  return new Configuration().withOptions("-classpath", fullClasspathStringFor(loader));
	//		 }
	//
	//		 @@Override protected CodeGenerator createCodeGenerator(ClassLoader loader, Configuration configuration) throws CodeGenerationStrategyNotSupportedException
	//		 {
	//			  return new SourceCodeGenerator(loader, configuration, configuration.sourceCompilerFor(this));
	//		 }
	//
	//		 @@Override protected String name()
	//		 {
	//			  return "SOURCECODE";
	//		 }
	//	 }

//JAVA TO C# CONVERTER TODO TASK: Java to C# Converter does not convert types within enums:
//		 private static class SourceVisitorAnonymousInnerClass extends SourceVisitor
	//	 {
	//		 @@Override protected void visitSource(TypeReference reference, CharSequence sourceCode)
	//		 {
	//			  System.out.println("=== Generated class " + reference.fullName() + " ===\n" + sourceCode);
	//		 }
	//
	//		 @@Override public String toString()
	//		 {
	//			  return "PRINT_SOURCE";
	//		 }
	//	 }

		 public static Neo4Net.CodeGen.CodeGeneratorOption AnnotationProcessor( javax.annotation.processing.Processor processor )
		 {
			  return new AnnotationProcessorOption( requireNonNull( processor ) );
		 }

		 public static Neo4Net.CodeGen.CodeGeneratorOption SourceLocation( java.nio.file.Path path )
		 {
			  return new SourceLocationOption( requireNonNull( path ) );
		 }

		 public static Neo4Net.CodeGen.CodeGeneratorOption TemporarySourceCodeLocation()
		 {
			  try
			  {
					return new SourceLocationOption( Files.createTempDirectory( null ) );
			  }
			  catch ( IOException e )
			  {
					throw new Exception( "Failed to create temporary directory.", e );
			  }
		 }

		 public void ApplyTo( object target )
		 {
			  if ( target is Configuration )
			  {
					( ( Configuration ) target ).WithFlag( this );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Java to C# Converter does not convert types within enums:
//		 private static class AnnotationProcessorOption implements org.neo4j.codegen.CodeGeneratorOption
	//	 {
	//		  private final Processor processor;
	//
	//		  AnnotationProcessorOption(Processor processor)
	//		  {
	//				this.processor = processor;
	//		  }
	//
	//		  @@Override public void applyTo(Object target)
	//		  {
	//				if (target instanceof Configuration)
	//				{
	//					 ((Configuration) target).withAnnotationProcessor(processor);
	//				}
	//		  }
	//
	//		  @@Override public String toString()
	//		  {
	//				return "annotationProcessor( " + processor + " )";
	//		  }
	//	 }

//JAVA TO C# CONVERTER TODO TASK: Java to C# Converter does not convert types within enums:
//		 private static class SourceLocationOption extends SourceVisitor
	//	 {
	//		  private final Path path;
	//
	//		  SourceLocationOption(Path path)
	//		  {
	//				this.path = path;
	//		  }
	//
	//		  @@Override public String toString()
	//		  {
	//				return "sourceLocation( " + path + " )";
	//		  }
	//
	//		  @@Override protected void visitSource(TypeReference reference, CharSequence sourceCode)
	//		  {
	//				try
	//				{
	//					 Path location = path(reference);
	//					 Files.createDirectories(location.getParent());
	//					 Files.write(location, Collections.singletonList(sourceCode), Charset.defaultCharset());
	//				}
	//				catch (IOException e)
	//				{
	//					 throw new RuntimeException("Failed to write source code", e);
	//				}
	//		  }
	//
	//		  private Path path(TypeReference reference)
	//		  {
	//				return path.resolve(reference.packageName().replace('.', '/') + "/" + reference.name() + ".java");
	//		  }
	//	 }

//JAVA TO C# CONVERTER TODO TASK: Java to C# Converter does not convert types within enums:
//		 private static class PrintWarningsOption implements org.neo4j.codegen.CodeGeneratorOption, WarningsHandler
	//	 {
	//		  private final PrintStream target;
	//
	//		  PrintWarningsOption(PrintStream target)
	//		  {
	//				this.target = target;
	//		  }
	//
	//		  @@Override public void applyTo(Object target)
	//		  {
	//				if (target instanceof Configuration)
	//				{
	//					 ((Configuration) target).withWarningsHandler(this);
	//				}
	//		  }
	//
	//		  @@Override public String toString()
	//		  {
	//				return "PRINT_WARNINGS";
	//		  }
	//
	//		  @@Override public void handle(List<Diagnostic<? extends JavaFileObject>> diagnostics)
	//		  {
	//				for (Diagnostic<? extends JavaFileObject> diagnostic : diagnostics)
	//				{
	//					 format(target, diagnostic);
	//				}
	//		  }
	//	 }

		public static IList<SourceCode> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public override string ToString()
		{
			return nameValue;
		}

		public static SourceCode valueOf( string name )
		{
			foreach ( SourceCode enumInstance in SourceCode.valueList )
			{
				if ( enumInstance.nameValue == name )
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException( name );
		}
	}

}