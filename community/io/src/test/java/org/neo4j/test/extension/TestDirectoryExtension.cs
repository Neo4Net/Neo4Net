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
	using AfterAllCallback = org.junit.jupiter.api.extension.AfterAllCallback;
	using AfterEachCallback = org.junit.jupiter.api.extension.AfterEachCallback;
	using BeforeEachCallback = org.junit.jupiter.api.extension.BeforeEachCallback;
	using ExtensionContext = org.junit.jupiter.api.extension.ExtensionContext;
	using Namespace = org.junit.jupiter.api.extension.ExtensionContext.Namespace;
	using JUnitException = org.junit.platform.commons.JUnitException;

	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.rule.TestDirectory.testDirectory;

	public class TestDirectoryExtension : StatefullFieldExtension<TestDirectory>, BeforeEachCallback, AfterEachCallback, AfterAllCallback
	{
		 internal const string TEST_DIRECTORY = "testDirectory";
		 internal static readonly ExtensionContext.Namespace TestDirectoryNamespace = ExtensionContext.Namespace.create( TEST_DIRECTORY );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void beforeEach(org.junit.jupiter.api.extension.ExtensionContext context) throws Exception
		 public override void BeforeEach( ExtensionContext context )
		 {
			  TestDirectory testDirectory = GetStoredValue( context );
			  testDirectory.PrepareDirectory( context.RequiredTestClass, context.RequiredTestMethod.Name );
		 }

		 public override void AfterEach( ExtensionContext context )
		 {
			  TestDirectory testDirectory = GetStoredValue( context );
			  try
			  {
					testDirectory.Complete( !context.ExecutionException.Present );
			  }
			  catch ( Exception e )
			  {
					throw new JUnitException( format( "Fail to cleanup test directory for %s test.", context.DisplayName ), e );
			  }
		 }

		 protected internal override string FieldKey
		 {
			 get
			 {
				  return TEST_DIRECTORY;
			 }
		 }

		 protected internal override Type<TestDirectory> FieldType
		 {
			 get
			 {
				  return typeof( TestDirectory );
			 }
		 }

		 protected internal override TestDirectory CreateField( ExtensionContext extensionContext )
		 {
			  ExtensionContext.Store fileSystemStore = GetStore( extensionContext, FileSystemExtension.FileSystemNamespace );
			  FileSystemAbstraction fileSystemAbstraction = fileSystemStore.get( FileSystemExtension.FILE_SYSTEM, typeof( FileSystemAbstraction ) );
			  return fileSystemAbstraction != null ? testDirectory( fileSystemAbstraction ) : testDirectory();
		 }

		 protected internal override ExtensionContext.Namespace NameSpace
		 {
			 get
			 {
				  return TestDirectoryNamespace;
			 }
		 }
	}

}