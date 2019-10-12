using System;

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
namespace Org.Neo4j.Test.extension
{
	using AfterEachCallback = org.junit.jupiter.api.extension.AfterEachCallback;
	using BeforeEachCallback = org.junit.jupiter.api.extension.BeforeEachCallback;
	using ExtensionContext = org.junit.jupiter.api.extension.ExtensionContext;
	using Namespace = org.junit.jupiter.api.extension.ExtensionContext.Namespace;
	using JUnitException = org.junit.platform.commons.JUnitException;


	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using Profiler = Org.Neo4j.Resources.Profiler;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

	/// <summary>
	/// A sampling profiler extension for JUnit 5. This extension profiles a given set of threads that run in a unit test, and if the test fails, prints a profile
	/// of where the time was spent. This is particularly useful for tests that has a tendency to fail with a timeout, and this extension can be used to diagnose
	/// such flaky tests.
	/// <para>
	/// The profile output is printed to a {@code profiler-output.txt} file in the test directory by default.
	/// </para>
	/// <para>
	/// Here is an example of how to use it:
	/// 
	/// <pre><code>
	///     {@literal @}ExtendWith( {TestDirectoryExtension.class, PorfilerExtension.class} )
	///     public class MyTest
	///     {
	///         {@literal @}Inject
	///         public Profiler profiler;
	/// 
	///         {@literal @}Test
	///         void testSomeStuff()
	///         {
	///             profiler.profile();
	///             // ... do some stuff in this thread.
	///         }
	///     }
	/// </code></pre>
	/// 
	/// </para>
	/// </summary>
	/// <seealso cref= Profiler The Profiler interface, for more information on how to use the injected profiler instance. </seealso>
	public class ProfilerExtension : StatefullFieldExtension<Profiler>, BeforeEachCallback, AfterEachCallback
	{
		 internal const string PROFILER_KEY = "profiler";
		 internal static readonly ExtensionContext.Namespace ProfilerNamespace = ExtensionContext.Namespace.create( PROFILER_KEY );

		 protected internal override string FieldKey
		 {
			 get
			 {
				  return PROFILER_KEY;
			 }
		 }

		 protected internal override Type<Profiler> FieldType
		 {
			 get
			 {
				  return typeof( Profiler );
			 }
		 }

		 protected internal override Profiler CreateField( ExtensionContext extensionContext )
		 {
			  return Profiler.profiler();
		 }

		 protected internal override ExtensionContext.Namespace NameSpace
		 {
			 get
			 {
				  return ProfilerNamespace;
			 }
		 }

		 public override void BeforeEach( ExtensionContext context )
		 {
			  GetStoredValue( context ).reset();
		 }

		 public override void AfterEach( ExtensionContext context )
		 {
			  Profiler profiler = GetStoredValue( context );
			  try
			  {
					profiler.Finish();
					if ( context.ExecutionException.Present )
					{
						 ExtensionContext.Store testDirStore = GetStore( context, TestDirectoryExtension.TestDirectoryNamespace );
						 TestDirectory testDir = ( TestDirectory ) testDirStore.get( TestDirectoryExtension.TEST_DIRECTORY );
						 File profileOutputFile = testDir.CreateFile( "profiler-output.txt" );
						 FileSystemAbstraction fs = testDir.FileSystem;

						 using ( PrintStream @out = new PrintStream( fs.OpenAsOutputStream( profileOutputFile, false ) ) )
						 {
							  string displayName = context.TestClass.map( Type.getSimpleName ).orElse( "class" ) + "." + context.DisplayName;
							  profiler.PrintProfile( @out, displayName );
						 }
					}
			  }
			  catch ( Exception e )
			  {
					throw new JUnitException( "Failed to finish profiling and/or produce profiling output.", e );
			  }
		 }
	}

}