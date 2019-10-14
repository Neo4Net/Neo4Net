using System;
using System.Collections.Generic;
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
namespace Neo4Net.Kernel.impl.util.dbstructure
{
	using Test = org.junit.Test;


	using Neo4Net.Helpers.Collections;
	using ConstraintDescriptorFactory = Neo4Net.Kernel.api.schema.constraints.ConstraintDescriptorFactory;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.api.schema.index.TestIndexDescriptorFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;

	public class DbStructureInvocationTracingAcceptanceTest
	{
		private bool InstanceFieldsInitialized = false;

		public DbStructureInvocationTracingAcceptanceTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_classNameWithPackage = _packageName + "." + _className;
		}

		 private readonly string _packageName = "org.neo4j.kernel.impl.util.data";
		 private readonly string _className = "XXYYZZData";
		 private string _classNameWithPackage;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void outputCompilesWithoutErrors() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void OutputCompilesWithoutErrors()
		 {
			  // GIVEN
			  StringBuilder output = new StringBuilder();
			  InvocationTracer<DbStructureVisitor> tracer = new InvocationTracer<DbStructureVisitor>( "Test", _packageName, _className, typeof( DbStructureVisitor ), DbStructureArgumentFormatter.Instance, output );
			  DbStructureVisitor visitor = tracer.NewProxy();

			  // WHEN
			  ExerciseVisitor( from => visitor );
			  tracer.Close();

			  // THEN
			  AssertCompiles( _classNameWithPackage, output.ToString() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void compiledOutputCreatesInputTrace() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CompiledOutputCreatesInputTrace()
		 {
			  // GIVEN
			  StringBuilder output = new StringBuilder();
			  InvocationTracer<DbStructureVisitor> tracer = new InvocationTracer<DbStructureVisitor>( "Test", _packageName, _className, typeof( DbStructureVisitor ), DbStructureArgumentFormatter.Instance, output );
			  ExerciseVisitor( from => tracer.NewProxy() );
			  tracer.Close();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.helpers.collection.Visitable<DbStructureVisitor> visitable = compileVisitable(classNameWithPackage, output.toString());
			  Visitable<DbStructureVisitor> visitable = CompileVisitable( _classNameWithPackage, output.ToString() );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final DbStructureVisitor visitor = mock(DbStructureVisitor.class);
			  DbStructureVisitor visitor = mock( typeof( DbStructureVisitor ) );

			  // WHEN
			  visitable.Accept( visitor );

			  // THEN
			  ExerciseVisitor( o => verify( visitor ) );
			  verifyNoMoreInteractions( visitor );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void compiledOutputProducesSameCompiledOutputIfCompiledAgain() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CompiledOutputProducesSameCompiledOutputIfCompiledAgain()
		 {
			  // GIVEN
			  StringBuilder output1 = new StringBuilder();
			  InvocationTracer<DbStructureVisitor> tracer1 = new InvocationTracer<DbStructureVisitor>( "Test", _packageName, _className, typeof( DbStructureVisitor ), DbStructureArgumentFormatter.Instance, output1 );
			  DbStructureVisitor visitor1 = tracer1.NewProxy();
			  ExerciseVisitor( from => visitor1 );
			  tracer1.Close();
			  string source1 = output1.ToString();
			  Visitable<DbStructureVisitor> visitable = CompileVisitable( _classNameWithPackage, source1 );

			  // WHEN
			  StringBuilder output2 = new StringBuilder();
			  InvocationTracer<DbStructureVisitor> tracer2 = new InvocationTracer<DbStructureVisitor>( "Test", _packageName, _className, typeof( DbStructureVisitor ), DbStructureArgumentFormatter.Instance, output2 );
			  DbStructureVisitor visitor2 = tracer2.NewProxy();
			  visitable.Accept( visitor2 );
			  tracer2.Close();
			  string source2 = output2.ToString();

			  // THEN
			  assertEquals( source1, source2 );
		 }

		 private void ExerciseVisitor( System.Func<object, DbStructureVisitor> visitor )
		 {
			  visitor( null ).visitLabel( 0, "Person" );
			  visitor( null ).visitLabel( 1, "Party" );
			  visitor( null ).visitPropertyKey( 0, "name" );
			  visitor( null ).visitPropertyKey( 2, "lastName" );
			  visitor( null ).visitPropertyKey( 1, "age" );
			  visitor( null ).visitRelationshipType( 0, "ACCEPTS" );
			  visitor( null ).visitRelationshipType( 1, "REJECTS" );
			  visitor( null ).visitIndex( TestIndexDescriptorFactory.forLabel( 0, 1 ), ":Person(age)", 0.5d, 1L );
			  visitor( null ).visitIndex( TestIndexDescriptorFactory.uniqueForLabel( 0, 0, 2 ), ":Person(name, lastName)", 0.5d, 1L );
			  visitor( null ).visitUniqueConstraint( ConstraintDescriptorFactory.uniqueForLabel( 1, 0 ), ":Party(name)" );
			  visitor( null ).visitNodeKeyConstraint( ConstraintDescriptorFactory.nodeKeyForLabel( 0, 1, 2 ), ":Person(name, lastName)" );
			  visitor( null ).visitAllNodesCount( 55 );
			  visitor( null ).visitNodeCount( 0, "Person", 50 );
			  visitor( null ).visitNodeCount( 0, "Party", 5 );
			  visitor( null ).visitRelCount( 0, 1, -1, "MATCH (:Person)-[:REJECTS]->() RETURN count(*)", 5 );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private void assertCompiles(final String className, String source)
		 private void AssertCompiles( string className, string source )
		 {
			  Compile(className, source, (success, manager, diagnostics) =>
			  {
						  AssertSuccessfullyCompiled( success, diagnostics, className );
						  return true;
			  });
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private org.neo4j.helpers.collection.Visitable<DbStructureVisitor> compileVisitable(final String className, String inputSource)
		 private Visitable<DbStructureVisitor> CompileVisitable( string className, string inputSource )
		 {
			  return Compile(className, inputSource, (success, manager, diagnostics) =>
			  {
			  AssertSuccessfullyCompiled( success, diagnostics, className );
			  object instance;
			  try
			  {
				  ClassLoader classLoader = manager.getClassLoader( null );
				  Type clazz = classLoader.loadClass( className );
				  instance = clazz.getDeclaredField( "INSTANCE" ).get( null );
			  }
			  catch ( Exception e ) when ( e is IllegalAccessException || e is ClassNotFoundException || e is NoSuchFieldException )
			  {
				  throw new AssertionError( "Failed to instantiate compiled class", e );
			  }
			  return ( Visitable<DbStructureVisitor> ) instance;
			  });
		 }

		 private void AssertSuccessfullyCompiled<T1>( bool? success, IList<T1> diagnostics, string className ) where T1 : javax.tools.JavaFileObject
		 {
			  if ( success == null || !success )
			  {
					StringBuilder builder = new StringBuilder();
					builder.Append( "Failed to compile: " );
					builder.Append( className );
					builder.Append( "\n\n" );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (javax.tools.Diagnostic<?> diagnostic : diagnostics)
					foreach ( Diagnostic<object> diagnostic in diagnostics )
					{
						 builder.Append( diagnostic.ToString() );
						 builder.Append( "\n" );
					}
					throw new AssertionError( builder.ToString() );
			  }
		 }

		 private T Compile<T>( string className, string source, CompilationListener<T> listener )
		 {
			  JavaCompiler systemCompiler = ToolProvider.SystemJavaCompiler;
			  JavaFileManager manager = new InMemFileManager();
			  DiagnosticCollector<JavaFileObject> diagnosticsCollector = new DiagnosticCollector<JavaFileObject>();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Iterable<? extends javax.tools.JavaFileObject> sources = java.util.Collections.singletonList(new InMemSource(className, source));
			  IEnumerable<JavaFileObject> sources = Collections.singletonList( new InMemSource( className, source ) );
			  JavaCompiler.CompilationTask task = systemCompiler.getTask( null, manager, diagnosticsCollector, null, null, sources );
			  bool? success = task.call();
			  return listener.Compiled( success, manager, diagnosticsCollector.Diagnostics );
		 }

		 private interface CompilationListener<T>
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: T compiled(System.Nullable<bool> success, javax.tools.JavaFileManager manager, java.util.List<javax.tools.Diagnostic<? extends javax.tools.JavaFileObject>> diagnostics);
			  T compiled<T1>( bool? success, JavaFileManager manager, IList<T1> diagnostics );
		 }

		 private class InMemSource : SimpleJavaFileObject
		 {
			  internal readonly string JavaSource;

			  internal InMemSource( string className, string javaSource ) : base( URI.create( "string:///" + className.Replace( '.', '/' ) + JavaFileObject.Kind.SOURCE.extension ), JavaFileObject.Kind.SOURCE )
			  {
					this.JavaSource = javaSource;
			  }

			  public override CharSequence GetCharContent( bool ignoreEncodingErrors )
			  {
					return JavaSource;
			  }
		 }

		 private class InMemSink : SimpleJavaFileObject
		 {
			  internal MemoryStream ByteCodeStream = new MemoryStream();

			  internal InMemSink( string className ) : base( URI.create( "mem:///" + className + JavaFileObject.Kind.CLASS.extension ), JavaFileObject.Kind.CLASS )
			  {
			  }

			  public virtual sbyte[] Bytes
			  {
				  get
				  {
						return ByteCodeStream.toByteArray();
				  }
			  }

			  public override Stream OpenOutputStream()
			  {
					return ByteCodeStream;
			  }
		 }

		 private class InMemFileManager : ForwardingJavaFileManager
		 {
			  internal readonly IDictionary<string, InMemSink> Classes = new Dictionary<string, InMemSink>();

			  internal InMemFileManager() : base(ToolProvider.SystemJavaCompiler.getStandardFileManager(null, null, null))
			  {
			  }

			  public override ClassLoader GetClassLoader( Location location )
			  {
					return new SecureClassLoaderAnonymousInnerClass( this );
			  }

			  private class SecureClassLoaderAnonymousInnerClass : SecureClassLoader
			  {
				  private readonly InMemFileManager _outerInstance;

				  public SecureClassLoaderAnonymousInnerClass( InMemFileManager outerInstance )
				  {
					  this.outerInstance = outerInstance;
				  }

				  protected internal override Type findClass( string name )
				  {
						sbyte[] byteCode = outerInstance.Classes[name].Bytes;
						return base.defineClass( name, byteCode, 0, byteCode.Length );
				  }
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public javax.tools.JavaFileObject getJavaFileForOutput(Location location, String className, javax.tools.JavaFileObject.Kind kind, javax.tools.FileObject sibling) throws java.io.IOException
			  public override JavaFileObject GetJavaFileForOutput( Location location, string className, JavaFileObject.Kind kind, FileObject sibling )
			  {
					if ( StandardLocation.CLASS_OUTPUT == location && JavaFileObject.Kind.CLASS == kind )
					{
						 InMemSink clazz = new InMemSink( className );
						 Classes[className] = clazz;
						 return clazz;
					}
					else
					{
						 return base.GetJavaFileForOutput( location, className, kind, sibling );
					}
			  }
		 }
	}

}