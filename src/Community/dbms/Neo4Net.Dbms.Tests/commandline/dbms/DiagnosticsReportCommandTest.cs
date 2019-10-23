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
namespace Neo4Net.Dbms.CommandLine
{
	using AfterEach = org.junit.jupiter.api.AfterEach;
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;
	using JRE = org.junit.jupiter.api.condition.JRE;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using CommandFailed = Neo4Net.CommandLine.Admin.CommandFailed;
	using IncorrectUsage = Neo4Net.CommandLine.Admin.IncorrectUsage;
	using OutsideWorld = Neo4Net.CommandLine.Admin.OutsideWorld;
	using RealOutsideWorld = Neo4Net.CommandLine.Admin.RealOutsideWorld;
	using DiagnosticsOfflineReportProvider = Neo4Net.Diagnostics.DiagnosticsOfflineReportProvider;
	using DiagnosticsReportSource = Neo4Net.Diagnostics.DiagnosticsReportSource;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using Config = Neo4Net.Kernel.configuration.Config;
	using DefaultFileSystemExtension = Neo4Net.Test.extension.DefaultFileSystemExtension;
	using Inject = Neo4Net.Test.extension.Inject;
	using SuppressOutputExtension = Neo4Net.Test.extension.SuppressOutputExtension;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.commandline.dbms.DiagnosticsReportCommand.DEFAULT_CLASSIFIERS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.commandline.dbms.DiagnosticsReportCommand.describeClassifier;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith({TestDirectoryExtension.class, DefaultFileSystemExtension.class, SuppressOutputExtension.class}) public class DiagnosticsReportCommandTest
	public class DiagnosticsReportCommandTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.Neo4Net.test.rule.TestDirectory testDirectory;
		 private TestDirectory _testDirectory;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.Neo4Net.io.fs.DefaultFileSystemAbstraction fs;
		 private DefaultFileSystemAbstraction _fs;

		 private Path _homeDir;
		 private Path _configDir;
		 private Path _configFile;
		 private string _originalUserDir;

		 public class MyDiagnosticsOfflineReportProvider : DiagnosticsOfflineReportProvider
		 {
			  public MyDiagnosticsOfflineReportProvider() : base("my-provider", "logs", "tx")
			  {
			  }

			  public override void Init( FileSystemAbstraction fs, Config config, File storeDirectory )
			  {
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override protected java.util.List<org.Neo4Net.diagnostics.DiagnosticsReportSource> provideSources(java.util.Set<String> classifiers)
			  protected internal override IList<DiagnosticsReportSource> ProvideSources( ISet<string> classifiers )
			  {
					return Collections.emptyList();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void setUp() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void SetUp()
		 {
			  _homeDir = _testDirectory.directory( "home-dir" ).toPath();
			  _configDir = _testDirectory.directory( "config-dir" ).toPath();

			  // Touch config
			  _configFile = _configDir.resolve( "Neo4Net.conf" );
			  Files.createFile( _configFile );

			  // To make sure files are resolved from the working directory
			  _originalUserDir = System.setProperty( "user.dir", _testDirectory.absolutePath().AbsolutePath );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterEach void tearDown()
		 internal virtual void TearDown()
		 {
			  // Restore directory
			  System.setProperty( "user.dir", _originalUserDir );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void exitIfConfigFileIsMissing() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ExitIfConfigFileIsMissing()
		 {
			  Files.delete( _configFile );
			  string[] args = new string[] { "--list" };
			  using ( RealOutsideWorld outsideWorld = new RealOutsideWorld() )
			  {
					DiagnosticsReportCommand diagnosticsReportCommand = new DiagnosticsReportCommand( _homeDir, _configDir, outsideWorld );

					CommandFailed commandFailed = assertThrows( typeof( CommandFailed ), () => diagnosticsReportCommand.execute(args) );
					assertThat( commandFailed.Message, containsString( "Unable to find config file, tried: " ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void allHasToBeOnlyClassifier() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void AllHasToBeOnlyClassifier()
		 {
			  string[] args = new string[] { "all", "logs", "tx" };
			  using ( RealOutsideWorld outsideWorld = new RealOutsideWorld() )
			  {
					DiagnosticsReportCommand diagnosticsReportCommand = new DiagnosticsReportCommand( _homeDir, _configDir, outsideWorld );

					IncorrectUsage incorrectUsage = assertThrows( typeof( IncorrectUsage ), () => diagnosticsReportCommand.execute(args) );
					assertEquals( "If you specify 'all' this has to be the only classifier. Found ['logs','tx'] as well.", incorrectUsage.Message );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void printUnrecognizedClassifiers() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void PrintUnrecognizedClassifiers()
		 {
			  string[] args = new string[] { "logs", "tx", "invalid" };
			  using ( RealOutsideWorld outsideWorld = new RealOutsideWorld() )
			  {
					DiagnosticsReportCommand diagnosticsReportCommand = new DiagnosticsReportCommand( _homeDir, _configDir, outsideWorld );

					IncorrectUsage incorrectUsage = assertThrows( typeof( IncorrectUsage ), () => diagnosticsReportCommand.execute(args) );
					assertEquals( "Unknown classifier: invalid", incorrectUsage.Message );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("ResultOfMethodCallIgnored") @Test void defaultValuesShouldBeValidClassifiers()
		 internal virtual void DefaultValuesShouldBeValidClassifiers()
		 {
			  foreach ( string classifier in DEFAULT_CLASSIFIERS )
			  {
					describeClassifier( classifier );
			  }

			  // Make sure the above actually catches bad classifiers
			  System.ArgumentException exception = assertThrows( typeof( System.ArgumentException ), () => describeClassifier("invalid") );
			  assertEquals( "Unknown classifier: invalid", exception.Message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void listShouldDisplayAllClassifiers() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ListShouldDisplayAllClassifiers()
		 {
			  using ( MemoryStream baos = new MemoryStream() )
			  {
					PrintStream ps = new PrintStream( baos );
					string[] args = new string[] { "--list" };
					OutsideWorld outsideWorld = mock( typeof( OutsideWorld ) );
					when( outsideWorld.FileSystem() ).thenReturn(_fs);
					when( outsideWorld.OutStream() ).thenReturn(ps);

					DiagnosticsReportCommand diagnosticsReportCommand = new DiagnosticsReportCommand( _homeDir, _configDir, outsideWorld );
					diagnosticsReportCommand.Execute( args );

					assertThat( baos.ToString(), @is(string.Format("Finding running instance of Neo4Net%n" + "No running instance of Neo4Net was found. Online reports will be omitted.%n" + "If Neo4Net is running but not detected, you can supply the process id of the running instance with --pid%n" + "All available classifiers:%n" + "  config     include configuration file%n" + "  logs       include log files%n" + "  plugins    include a view of the plugin directory%n" + "  ps         include a list of running processes%n" + "  tree       include a view of the tree structure of the data directory%n" + "  tx         include transaction logs%n")) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void overrideDestination() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void OverrideDestination()
		 {
			  // because of https://bugs.openjdk.java.net/browse/JDK-8202127 and current surefire behaviour we need to have custom value for JRE >= 11
			  string toArgument = JRE.JAVA_11.CurrentVersion ? "--to=" + System.getProperty( "user.dir" ) + "/other/" : "--to=other/";
			  string[] args = new string[] { toArgument, "all" };
			  using ( RealOutsideWorld outsideWorld = new RealOutsideWorld() )
			  {
					DiagnosticsReportCommand diagnosticsReportCommand = new DiagnosticsReportCommand( _homeDir, _configDir, outsideWorld );
					diagnosticsReportCommand.Execute( args );

					File other = _testDirectory.directory( "other" );
					FileSystemAbstraction fs = outsideWorld.FileSystem();
					assertThat( fs.FileExists( other ), @is( true ) );
					assertThat( fs.ListFiles( other ).Length, @is( 1 ) );

					// Default should be empty
					File reports = new File( _testDirectory.directory(), "reports" );
					assertThat( fs.FileExists( reports ), @is( false ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void errorOnInvalidPid() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ErrorOnInvalidPid()
		 {
			  string[] args = new string[] { "--pid=a", "all" };
			  using ( RealOutsideWorld outsideWorld = new RealOutsideWorld() )
			  {
					DiagnosticsReportCommand diagnosticsReportCommand = new DiagnosticsReportCommand( _homeDir, _configDir, outsideWorld );
					CommandFailed commandFailed = assertThrows( typeof( CommandFailed ), () => diagnosticsReportCommand.execute(args) );
					assertEquals( "Unable to parse --pid", commandFailed.Message );
			  }
		 }
	}

}