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
namespace Neo4Net.Test.rule
{
	using DigestUtils = org.apache.commons.codec.digest.DigestUtils;
	using Rule = org.junit.Rule;
	using ExternalResource = org.junit.rules.ExternalResource;
	using Description = org.junit.runner.Description;
	using Statement = org.junit.runners.model.Statement;


	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using StoreLayout = Neo4Net.Io.layout.StoreLayout;
	using VisibleForTesting = Neo4Net.Util.VisibleForTesting;

	/// <summary>
	/// This class defines a JUnit rule which ensures that the test's working directory is cleaned up. The clean-up
	/// only happens if the test passes, to help diagnose test failures.  For example:
	/// <pre>
	///   public class SomeTest
	///   {
	///     @Rule
	///     public TestDirectory dir = TestDirectory.testDirectory();
	/// 
	///     @Test
	///     public void shouldDoSomething()
	///     {
	///       File storeDir = dir.databaseDir();
	///       // do stuff with store dir
	///     }
	///   }
	/// </pre>
	/// </summary>
	public class TestDirectory : ExternalResource
	{
		 private const string DEFAULT_DATABASE_DIRECTORY = "graph.db";
		 /// <summary>
		 /// This value is mixed into the hash string, along with the test name,
		 /// that we use for uniquely naming test directories.
		 /// By getting a new value here, every time the JVM is started, we the same
		 /// tests will get different directory names when executed many times in
		 /// different JVMs.
		 /// This way, the test results for many runs of the same tests are kept
		 /// around, so they can easily be compared with each other. This is useful
		 /// when you need to investigate a flaky test, for instance.
		 /// </summary>
		 private static readonly long _jvmExecutionHash = new Random().nextLong();

		 private readonly FileSystemAbstraction _fileSystem;
		 private File _testClassBaseFolder;
		 private Type _owningTest;
		 private bool _keepDirectoryAfterSuccessfulTest;
		 private File _testDirectory;
		 private StoreLayout _storeLayout;
		 private DatabaseLayout _defaultDatabaseLayout;

		 private TestDirectory( FileSystemAbstraction fileSystem )
		 {
			  this._fileSystem = fileSystem;
		 }

		 private TestDirectory( FileSystemAbstraction fileSystem, Type owningTest )
		 {
			  this._fileSystem = fileSystem;
			  this._owningTest = owningTest;
		 }

//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 public static TestDirectory TestDirectoryConflict()
		 {
			  return new TestDirectory( new DefaultFileSystemAbstraction() );
		 }

//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 public static TestDirectory TestDirectoryConflict( FileSystemAbstraction fs )
		 {
			  return new TestDirectory( fs );
		 }

//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 public static TestDirectory TestDirectoryConflict( Type owningTest )
		 {
			  return new TestDirectory( new DefaultFileSystemAbstraction(), owningTest );
		 }

//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 public static TestDirectory TestDirectoryConflict( Type owningTest, FileSystemAbstraction fs )
		 {
			  return new TestDirectory( fs, owningTest );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.junit.runners.model.Statement apply(final org.junit.runners.model.Statement super, final org.junit.runner.Description description)
		 public override Statement Apply( Statement @base, Description description )
		 {
			  return new StatementAnonymousInnerClass( this, @base, description );
		 }

		 private class StatementAnonymousInnerClass : Statement
		 {
			 private readonly TestDirectory _outerInstance;

			 private Statement @base;
			 private Description _description;

			 public StatementAnonymousInnerClass( TestDirectory outerInstance, Statement @base, Description description )
			 {
				 this.outerInstance = outerInstance;
				 this.@base = @base;
				 this._description = description;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void evaluate() throws Throwable
			 public override void evaluate()
			 {
				  outerInstance.directoryForDescription( _description );
				  bool success = false;
				  try
				  {
						@base.evaluate();
						success = true;
				  }
				  finally
				  {
						outerInstance.Complete( success );
				  }
			 }
		 }

		 /// <summary>
		 /// Tell this <seealso cref="Rule"/> to keep the store directory, even after a successful test.
		 /// It's just a useful debug mechanism to have for analyzing store after a test.
		 /// by default directories aren't kept.
		 /// </summary>
		 public virtual TestDirectory KeepDirectoryAfterSuccessfulTest()
		 {
			  _keepDirectoryAfterSuccessfulTest = true;
			  return this;
		 }

		 public virtual File AbsolutePath()
		 {
			  return Directory().AbsoluteFile;
		 }

		 public virtual File Directory()
		 {
			  if ( _testDirectory == null )
			  {
					throw new System.InvalidOperationException( "Not initialized" );
			  }
			  return _testDirectory;
		 }

		 public virtual File Directory( string name )
		 {
			  File dir = new File( Directory(), name );
			  CreateDirectory( dir );
			  return dir;
		 }

		 public virtual File File( string name )
		 {
			  return new File( Directory(), name );
		 }

		 public virtual File CreateFile( string name )
		 {
			  File file = file( name );
			  EnsureFileExists( file );
			  return file;
		 }

		 public virtual File DatabaseDir()
		 {
			  return DatabaseLayout().databaseDirectory();
		 }

		 public virtual StoreLayout StoreLayout()
		 {
			  return _storeLayout;
		 }

		 public virtual DatabaseLayout DatabaseLayout()
		 {
			  CreateDirectory( _defaultDatabaseLayout.databaseDirectory() );
			  return _defaultDatabaseLayout;
		 }

		 public virtual DatabaseLayout DatabaseLayout( File storeDir )
		 {
			  DatabaseLayout databaseLayout = StoreLayout.of( storeDir ).databaseLayout( DEFAULT_DATABASE_DIRECTORY );
			  CreateDirectory( databaseLayout.DatabaseDirectory() );
			  return databaseLayout;
		 }

		 public virtual DatabaseLayout DatabaseLayout( string name )
		 {
			  DatabaseLayout databaseLayout = _storeLayout.databaseLayout( name );
			  CreateDirectory( databaseLayout.DatabaseDirectory() );
			  return databaseLayout;
		 }

		 public virtual File StoreDir()
		 {
			  return _storeLayout.storeDirectory();
		 }

		 public virtual File StoreDir( string storeDirName )
		 {
			  return Directory( storeDirName );
		 }

		 public virtual File DatabaseDir( File storeDirectory )
		 {
			  File databaseDirectory = DatabaseLayout( storeDirectory ).databaseDirectory();
			  CreateDirectory( databaseDirectory );
			  return databaseDirectory;
		 }

		 public virtual File DatabaseDir( string customStoreDirectoryName )
		 {
			  return DatabaseDir( StoreDir( customStoreDirectoryName ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void cleanup() throws java.io.IOException
		 public virtual void Cleanup()
		 {
			  Clean( _fileSystem, _testClassBaseFolder );
		 }

		 public override string ToString()
		 {
			  string testDirectoryName = _testDirectory == null ? "<uninitialized>" : _testDirectory.ToString();
			  return format( "%s[\"%s\"]", this.GetType().Name, testDirectoryName );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.io.File cleanDirectory(String name) throws java.io.IOException
		 public virtual File CleanDirectory( string name )
		 {
			  return Clean( _fileSystem, new File( EnsureBase(), name ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void complete(boolean success) throws java.io.IOException
		 public virtual void Complete( bool success )
		 {
			  try
			  {
					if ( success && _testDirectory != null && !_keepDirectoryAfterSuccessfulTest )
					{
						 _fileSystem.deleteRecursively( _testDirectory );
					}
					_testDirectory = null;
					_storeLayout = null;
					_defaultDatabaseLayout = null;
			  }
			  finally
			  {
					_fileSystem.Dispose();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void prepareDirectory(Class testClass, String test) throws java.io.IOException
		 public virtual void PrepareDirectory( Type testClass, string test )
		 {
			  if ( _owningTest == null )
			  {
					_owningTest = testClass;
			  }
			  if ( string.ReferenceEquals( test, null ) )
			  {
					test = "static";
			  }
			  _testDirectory = PrepareDirectoryForTest( test );
			  _storeLayout = StoreLayout.of( _testDirectory );
			  _defaultDatabaseLayout = _storeLayout.databaseLayout( DEFAULT_DATABASE_DIRECTORY );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.io.File prepareDirectoryForTest(String test) throws java.io.IOException
		 public virtual File PrepareDirectoryForTest( string test )
		 {
			  string dir = DigestUtils.md5Hex( _jvmExecutionHash + test );
			  EvaluateClassBaseTestFolder();
			  Register( test, dir );
			  return CleanDirectory( dir );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting public org.neo4j.io.fs.FileSystemAbstraction getFileSystem()
		 public virtual FileSystemAbstraction FileSystem
		 {
			 get
			 {
				  return _fileSystem;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void directoryForDescription(org.junit.runner.Description description) throws java.io.IOException
		 private void DirectoryForDescription( Description description )
		 {
			  PrepareDirectory( description.TestClass, description.MethodName );
		 }

		 private void EnsureFileExists( File file )
		 {
			  try
			  {
					if ( !_fileSystem.fileExists( file ) )
					{
						 _fileSystem.create( file ).close();
					}
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( "Failed to create file: " + file, e );
			  }
		 }

		 private void CreateDirectory( File databaseDirectory )
		 {
			  try
			  {
					_fileSystem.mkdirs( databaseDirectory );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( "Failed to create directory: " + databaseDirectory, e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static java.io.File clean(org.neo4j.io.fs.FileSystemAbstraction fs, java.io.File dir) throws java.io.IOException
		 private static File Clean( FileSystemAbstraction fs, File dir )
		 {
			  if ( fs.FileExists( dir ) )
			  {
					fs.DeleteRecursively( dir );
			  }
			  fs.Mkdirs( dir );
			  return dir;
		 }

		 private void EvaluateClassBaseTestFolder()
		 {
			  if ( _owningTest == null )
			  {
					throw new System.InvalidOperationException( " Test owning class is not defined" );
			  }
			  _testClassBaseFolder = TestDataDirectoryOf( _owningTest );
		 }

		 private static File TestDataDirectoryOf( Type owningTest )
		 {
			  File testData = new File( LocateTarget( owningTest ), "test data" );
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  return ( new File( testData, Shorten( owningTest.FullName ) ) ).AbsoluteFile;
		 }

		 private static string Shorten( string owningTestName )
		 {
			  int targetPartLength = 5;
			  string[] parts = owningTestName.Split( "\\.", true );
			  for ( int i = 0; i < parts.Length - 1; i++ )
			  {
					string part = parts[i];
					if ( part.Length > targetPartLength )
					{
						 parts[i] = part.Substring( 0, targetPartLength - 1 ) + "~";
					}
			  }
			  return string.join( ".", parts );
		 }

		 private void Register( string test, string dir )
		 {
			  try
			  {
					  using ( PrintStream printStream = new PrintStream( _fileSystem.openAsOutputStream( new File( EnsureBase(), ".register" ), true ) ) )
					  {
						printStream.print( format( "%s = %s%n", dir, test ) );
					  }
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }

		 private File EnsureBase()
		 {
			  if ( _testClassBaseFolder == null )
			  {
					EvaluateClassBaseTestFolder();
			  }
			  if ( _fileSystem.fileExists( _testClassBaseFolder ) && !_fileSystem.isDirectory( _testClassBaseFolder ) )
			  {
					throw new System.InvalidOperationException( _testClassBaseFolder + " exists and is not a directory!" );
			  }

			  try
			  {
					_fileSystem.mkdirs( _testClassBaseFolder );
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
			  return _testClassBaseFolder;
		 }

		 private static File LocateTarget( Type owningTest )
		 {
			  try
			  {
					File codeSource = new File( owningTest.ProtectionDomain.CodeSource.Location.toURI() );
					if ( codeSource.Directory )
					{
						 // code loaded from a directory
						 return codeSource.ParentFile;
					}
			  }
			  catch ( URISyntaxException )
			  {
					// ignored
			  }
			  return new File( "target" );
		 }
	}

}