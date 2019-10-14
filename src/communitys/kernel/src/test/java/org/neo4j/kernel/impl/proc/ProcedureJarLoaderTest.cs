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
namespace Neo4Net.Kernel.impl.proc
{
	using IsEqual = org.hamcrest.core.IsEqual;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using TemporaryFolder = org.junit.rules.TemporaryFolder;


	using ProcedureException = Neo4Net.@internal.Kernel.Api.exceptions.ProcedureException;
	using ProcedureSignature = Neo4Net.@internal.Kernel.Api.procs.ProcedureSignature;
	using ResourceTracker = Neo4Net.Kernel.api.ResourceTracker;
	using StubResourceManager = Neo4Net.Kernel.api.StubResourceManager;
	using BasicContext = Neo4Net.Kernel.api.proc.BasicContext;
	using CallableProcedure = Neo4Net.Kernel.api.proc.CallableProcedure;
	using Config = Neo4Net.Kernel.configuration.Config;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using Log = Neo4Net.Logging.Log;
	using NullLog = Neo4Net.Logging.NullLog;
	using Context = Neo4Net.Procedure.Context;
	using Name = Neo4Net.Procedure.Name;
	using Procedure = Neo4Net.Procedure.Procedure;
	using UserFunction = Neo4Net.Procedure.UserFunction;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.commons.lang3.StringEscapeUtils.escapeJava;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.procedure_unrestricted;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.Neo4jTypes.NTInteger;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.ProcedureSignature.procedureSignature;


//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("WeakerAccess") public class ProcedureJarLoaderTest
	public class ProcedureJarLoaderTest
	{
		private bool InstanceFieldsInitialized = false;

		public ProcedureJarLoaderTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_jarloader = new ProcedureJarLoader( new ReflectiveProcedureCompiler( new TypeMappers(), new ComponentRegistry(), RegistryWithUnsafeAPI(), _log, ProcedureConfig() ), NullLog.Instance );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.TemporaryFolder tmpdir = new org.junit.rules.TemporaryFolder();
		 public TemporaryFolder Tmpdir = new TemporaryFolder();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException exception = org.junit.rules.ExpectedException.none();
		 public ExpectedException Exception = ExpectedException.none();

		 private Log _log = mock( typeof( Log ) );
		 private ProcedureJarLoader _jarloader;
		 private readonly ResourceTracker _resourceTracker = new StubResourceManager();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLoadProcedureFromJar() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLoadProcedureFromJar()
		 {
			  // Given
			  URL jar = CreateJarFor( typeof( ClassWithOneProcedure ) );

			  // When
			  IList<CallableProcedure> procedures = _jarloader.loadProceduresFromDir( ParentDir( jar ) ).procedures();

			  // Then
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  IList<ProcedureSignature> signatures = procedures.Select( CallableProcedure::signature ).ToList();
			  assertThat( signatures, contains( procedureSignature( "org","neo4j", "kernel", "impl", "proc", "myProcedure" ).@out( "someNumber", NTInteger ).build() ) );

			  assertThat( asList( procedures[0].Apply( new BasicContext(), new object[0], _resourceTracker ) ), contains(IsEqual.equalTo(new object[]{ 1337L })) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLoadProcedureFromJarWithSpacesInFilename() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLoadProcedureFromJarWithSpacesInFilename()
		 {
			  // Given
			  URL jar = ( new JarBuilder() ).CreateJarFor(Tmpdir.newFile((new Random()).Next() + " some spaces in filename.jar"), typeof(ClassWithOneProcedure));

			  // When
			  IList<CallableProcedure> procedures = _jarloader.loadProceduresFromDir( ParentDir( jar ) ).procedures();

			  // Then
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  IList<ProcedureSignature> signatures = procedures.Select( CallableProcedure::signature ).ToList();
			  assertThat( signatures, contains( procedureSignature( "org", "neo4j", "kernel", "impl", "proc", "myProcedure" ).@out( "someNumber", NTInteger ).build() ) );

			  assertThat( asList( procedures[0].Apply( new BasicContext(), new object[0], _resourceTracker ) ), contains(IsEqual.equalTo(new object[]{ 1337L })) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLoadProcedureWithArgumentFromJar() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLoadProcedureWithArgumentFromJar()
		 {
			  // Given
			  URL jar = CreateJarFor( typeof( ClassWithProcedureWithArgument ) );

			  // When
			  IList<CallableProcedure> procedures = _jarloader.loadProceduresFromDir( ParentDir( jar ) ).procedures();

			  // Then
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  IList<ProcedureSignature> signatures = procedures.Select( CallableProcedure::signature ).ToList();
			  assertThat( signatures, contains( procedureSignature( "org","neo4j", "kernel", "impl", "proc", "myProcedure" ).@in( "value", NTInteger ).@out( "someNumber", NTInteger ).build() ) );

			  assertThat( asList( procedures[0].Apply( new BasicContext(), new object[]{ 42L }, _resourceTracker ) ), contains(IsEqual.equalTo(new object[]{ 42L })) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLoadProcedureFromJarWithMultipleProcedureClasses() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLoadProcedureFromJarWithMultipleProcedureClasses()
		 {
			  // Given
			  URL jar = CreateJarFor( typeof( ClassWithOneProcedure ), typeof( ClassWithAnotherProcedure ), typeof( ClassWithNoProcedureAtAll ) );

			  // When
			  IList<CallableProcedure> procedures = _jarloader.loadProceduresFromDir( ParentDir( jar ) ).procedures();

			  // Then
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  IList<ProcedureSignature> signatures = procedures.Select( CallableProcedure::signature ).ToList();
			  assertThat( signatures, containsInAnyOrder( procedureSignature( "org","neo4j", "kernel", "impl", "proc", "myOtherProcedure" ).@out( "someNumber", NTInteger ).build(), procedureSignature("org","neo4j", "kernel", "impl", "proc", "myProcedure").@out("someNumber", NTInteger).build() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveHelpfulErrorOnInvalidProcedure() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGiveHelpfulErrorOnInvalidProcedure()
		 {
			  // Given
			  URL jar = CreateJarFor( typeof( ClassWithOneProcedure ), typeof( ClassWithInvalidProcedure ) );

			  // Expect
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( string.Format( "Procedures must return a Stream of records, where a record is a concrete class%n" + "that you define, with public non-final fields defining the fields in the record.%n" + "If you''d like your procedure to return `boolean`, you could define a record class " + "like:%n" + "public class Output '{'%n" + "    public boolean out;%n" + "'}'%n" + "%n" + "And then define your procedure as returning `Stream<Output>`." ) );

			  // When
			  _jarloader.loadProceduresFromDir( ParentDir( jar ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLoadProceduresFromDirectory() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLoadProceduresFromDirectory()
		 {
			  // Given
			  CreateJarFor( typeof( ClassWithOneProcedure ) );
			  CreateJarFor( typeof( ClassWithAnotherProcedure ) );

			  // When
			  IList<CallableProcedure> procedures = _jarloader.loadProceduresFromDir( Tmpdir.Root ).procedures();

			  // Then
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  IList<ProcedureSignature> signatures = procedures.Select( CallableProcedure::signature ).ToList();
			  assertThat( signatures, containsInAnyOrder( procedureSignature( "org","neo4j", "kernel", "impl", "proc", "myOtherProcedure" ).@out( "someNumber", NTInteger ).build(), procedureSignature("org","neo4j", "kernel", "impl", "proc", "myProcedure").@out("someNumber", NTInteger).build() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveHelpfulErrorOnWildCardProcedure() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGiveHelpfulErrorOnWildCardProcedure()
		 {
			  // Given
			  URL jar = CreateJarFor( typeof( ClassWithWildCardStream ) );

			  // Expect
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( string.Format( "Procedures must return a Stream of records, where a record is a concrete class%n" + "that you define and not a Stream<?>." ) );

			  // When
			  _jarloader.loadProceduresFromDir( ParentDir( jar ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveHelpfulErrorOnRawStreamProcedure() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGiveHelpfulErrorOnRawStreamProcedure()
		 {
			  // Given
			  URL jar = CreateJarFor( typeof( ClassWithRawStream ) );

			  // Expect
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( string.Format( "Procedures must return a Stream of records, where a record is a concrete class%n" + "that you define and not a raw Stream." ) );

			  // When
			  _jarloader.loadProceduresFromDir( ParentDir( jar ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveHelpfulErrorOnGenericStreamProcedure() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGiveHelpfulErrorOnGenericStreamProcedure()
		 {
			  // Given
			  URL jar = CreateJarFor( typeof( ClassWithGenericStream ) );

			  // Expect
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( string.Format( "Procedures must return a Stream of records, where a record is a concrete class%n" + "that you define and not a parameterized type such as java.util.List<org.neo4j" + ".kernel.impl.proc.ProcedureJarLoaderTest$Output>." ) );

			  // When
			  _jarloader.loadProceduresFromDir( ParentDir( jar ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogHelpfullyWhenPluginJarIsCorrupt() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogHelpfullyWhenPluginJarIsCorrupt()
		 {
			  // given
			  URL theJar = CreateJarFor( typeof( ClassWithOneProcedure ), typeof( ClassWithAnotherProcedure ), typeof( ClassWithNoProcedureAtAll ) );
			  CorruptJar( theJar );

			  AssertableLogProvider logProvider = new AssertableLogProvider( true );

			  ProcedureJarLoader jarloader = new ProcedureJarLoader( new ReflectiveProcedureCompiler( new TypeMappers(), new ComponentRegistry(), RegistryWithUnsafeAPI(), _log, ProcedureConfig() ), logProvider.GetLog(typeof(ProcedureJarLoader)) );

			  // when
			  try
			  {
					jarloader.LoadProceduresFromDir( ParentDir( theJar ) );
					fail( "Should have logged and thrown exception." );
			  }
			  catch ( ZipException )
			  {
					// then
					logProvider.InternalToStringMessageMatcher().assertContains(escapeJava(string.Format("Plugin jar file: {0} corrupted.", (new File(theJar.toURI())).toPath())));
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWorkOnPathsWithSpaces() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWorkOnPathsWithSpaces()
		 {
			  // given
			  File fileWithSpacesInName = Tmpdir.newFile( ( new Random() ).Next() + "  some spaces in the filename" + ".jar" );
			  URL theJar = ( new JarBuilder() ).CreateJarFor(fileWithSpacesInName, typeof(ClassWithOneProcedure));
			  CorruptJar( theJar );

			  AssertableLogProvider logProvider = new AssertableLogProvider( true );

			  ProcedureJarLoader jarloader = new ProcedureJarLoader( new ReflectiveProcedureCompiler( new TypeMappers(), new ComponentRegistry(), RegistryWithUnsafeAPI(), _log, ProcedureConfig() ), logProvider.GetLog(typeof(ProcedureJarLoader)) );

			  // when
			  try
			  {
					jarloader.LoadProceduresFromDir( ParentDir( theJar ) );
					fail( "Should have logged and thrown exception." );
			  }
			  catch ( ZipException )
			  {
					// then
					logProvider.InternalToStringMessageMatcher().assertContains(escapeJava(string.Format("Plugin jar file: {0} corrupted.", fileWithSpacesInName.toPath())));
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnEmptySetOnNullArgument() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnEmptySetOnNullArgument()
		 {
			  // given
			  ProcedureJarLoader jarloader = new ProcedureJarLoader( new ReflectiveProcedureCompiler( new TypeMappers(), new ComponentRegistry(), RegistryWithUnsafeAPI(), _log, ProcedureConfig() ), NullLog.Instance );

			  // when
			  ProcedureJarLoader.Callables callables = jarloader.LoadProceduresFromDir( null );

			  // then
			  assertEquals( 0, callables.Procedures().Count + callables.Functions().Count );
		 }

		 private File ParentDir( URL jar )
		 {
			  return ( new File( jar.File ) ).ParentFile;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void corruptJar(java.net.URL jar) throws java.io.IOException, java.net.URISyntaxException
		 private void CorruptJar( URL jar )
		 {
			  File jarFile = ( new File( jar.toURI() ) ).CanonicalFile;
			  long fileLength = jarFile.length();
			  sbyte[] bytes = Files.readAllBytes( Paths.get( jar.toURI() ) );
			  for ( long i = fileLength / 2; i < fileLength; i++ )
			  {
					bytes[( int ) i] = 0;
			  }
			  Files.write( jarFile.toPath(), bytes );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.net.URL createJarFor(Class ... targets) throws java.io.IOException
		 private URL CreateJarFor( params Type [] targets )
		 {
			  return ( new JarBuilder() ).CreateJarFor(Tmpdir.newFile((new Random()).Next() + ".jar"), targets);
		 }

		 public class Output
		 {
			  public long SomeNumber = 1337; // Public because needed by a mapper

			  public Output()
			  {

			  }

			  public Output( long anotherNumber )
			  {
					this.SomeNumber = anotherNumber;
			  }
		 }

		 public class ClassWithInvalidProcedure
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public boolean booleansAreNotAcceptableReturnTypes()
			  public virtual bool BooleansAreNotAcceptableReturnTypes()
			  {
					return false;
			  }
		 }

		 public class ClassWithOneProcedure
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<Output> myProcedure()
			  public virtual Stream<Output> MyProcedure()
			  {
					return Stream.of( new Output() );
			  }
		 }

		 public class ClassWithNoProcedureAtAll
		 {
			  internal virtual void ThisMethodIsEntirelyUnrelatedToAllThisExcitement()
			  {

			  }
		 }

		 public class ClassWithAnotherProcedure
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<Output> myOtherProcedure()
			  public virtual Stream<Output> MyOtherProcedure()
			  {
					return Stream.of( new Output() );
			  }
		 }

		 public class ClassWithProcedureWithArgument
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<Output> myProcedure(@Name("value") long value)
			  public virtual Stream<Output> MyProcedure( long value )
			  {
					return Stream.of( new Output( value ) );
			  }
		 }

		 public class ClassWithWildCardStream
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<?> wildCardProc()
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
			  public virtual Stream<object> WildCardProc()
			  {
					return Stream.of( new Output() );
			  }
		 }

		 public class ClassWithRawStream
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream rawStreamProc()
			  public virtual Stream RawStreamProc()
			  {
					return Stream.of( new Output() );
			  }
		 }

		 public class ClassWithGenericStream
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<java.util.List<Output>> genericStream()
			  public virtual Stream<IList<Output>> GenericStream()
			  {
					return Stream.of( Collections.singletonList( new Output() ) );
			  }
		 }

		 public class ClassWithUnsafeComponent
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public UnsafeAPI api;
			  public UnsafeAPI Api;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<Output> unsafeProcedure()
			  public virtual Stream<Output> UnsafeProcedure()
			  {
					return Stream.of( new Output( Api.Number ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public long unsafeFunction()
			  public virtual long UnsafeFunction()
			  {
					return Api.Number;
			  }
		 }

		 public class ClassWithUnsafeConfiguredComponent
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public UnsafeAPI api;
			  public UnsafeAPI Api;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<Output> unsafeFullAccessProcedure()
			  public virtual Stream<Output> UnsafeFullAccessProcedure()
			  {
					return Stream.of( new Output( Api.Number ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public long unsafeFullAccessFunction()
			  public virtual long UnsafeFullAccessFunction()
			  {
					return Api.Number;
			  }
		 }

		 private class UnsafeAPI
		 {
			  public virtual long Number
			  {
				  get
				  {
						return 7331;
				  }
			  }
		 }

		 private ComponentRegistry RegistryWithUnsafeAPI()
		 {
			  ComponentRegistry allComponents = new ComponentRegistry();
			  allComponents.Register( typeof( UnsafeAPI ), ctx => new UnsafeAPI() );
			  return allComponents;
		 }

		 private ProcedureConfig ProcedureConfig()
		 {
			  Config config = Config.defaults( procedure_unrestricted, "org.neo4j.kernel.impl.proc.unsafeFullAccess*" );
			  return new ProcedureConfig( config );
		 }
	}

}