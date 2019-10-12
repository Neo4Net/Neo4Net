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
namespace Neo4Net.Commandline.dbms
{
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using CommandFailed = Neo4Net.Commandline.Admin.CommandFailed;
	using CommandLocator = Neo4Net.Commandline.Admin.CommandLocator;
	using IncorrectUsage = Neo4Net.Commandline.Admin.IncorrectUsage;
	using NullOutsideWorld = Neo4Net.Commandline.Admin.NullOutsideWorld;
	using OutsideWorld = Neo4Net.Commandline.Admin.OutsideWorld;
	using RealOutsideWorld = Neo4Net.Commandline.Admin.RealOutsideWorld;
	using Usage = Neo4Net.Commandline.Admin.Usage;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using Args = Neo4Net.Helpers.Args;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Inject = Neo4Net.Test.extension.Inject;
	using SuppressOutputExtension = Neo4Net.Test.extension.SuppressOutputExtension;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.NullOutputStream.NULL_OUTPUT_STREAM;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith({TestDirectoryExtension.class, SuppressOutputExtension.class}) class ImportCommandTest
	internal class ImportCommandTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.TestDirectory testDir;
		 private TestDirectory _testDir;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.SuppressOutput suppressOutput;
		 private SuppressOutput _suppressOutput;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void defaultsToCsvWhenModeNotSpecified() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void DefaultsToCsvWhenModeNotSpecified()
		 {
			  File homeDir = _testDir.directory( "home" );
			  ImporterFactory mockImporterFactory = mock( typeof( ImporterFactory ) );
			  Importer importer = mock( typeof( Importer ) );
			  when( mockImporterFactory.GetImporterForMode( eq( "csv" ), any( typeof( Args ) ), any( typeof( Config ) ), any( typeof( OutsideWorld ) ) ) ).thenReturn( importer );

			  using ( RealOutsideWorld outsideWorld = new RealOutsideWorld( System.out, System.err, new MemoryStream( new sbyte[0] ) ) )
			  {
					ImportCommand importCommand = new ImportCommand( homeDir.toPath(), _testDir.directory("conf").toPath(), outsideWorld, mockImporterFactory );

					string[] arguments = new string[] { "--database=foo", "--from=bar" };

					importCommand.Execute( arguments );

					verify( mockImporterFactory ).getImporterForMode( eq( "csv" ), any( typeof( Args ) ), any( typeof( Config ) ), any( typeof( OutsideWorld ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void acceptsNodeMetadata() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void AcceptsNodeMetadata()
		 {
			  File homeDir = _testDir.directory( "home" );
			  ImporterFactory mockImporterFactory = mock( typeof( ImporterFactory ) );
			  Importer importer = mock( typeof( Importer ) );
			  when( mockImporterFactory.GetImporterForMode( eq( "csv" ), any( typeof( Args ) ), any( typeof( Config ) ), any( typeof( OutsideWorld ) ) ) ).thenReturn( importer );

			  ImportCommand importCommand = new ImportCommand( homeDir.toPath(), _testDir.directory("conf").toPath(), new RealOutsideWorld(System.out, System.err, new MemoryStream(new sbyte[0])), mockImporterFactory );

			  string[] arguments = new string[] { "--database=foo", "--from=bar", "--nodes:PERSON:FRIEND=mock.csv" };

			  importCommand.Execute( arguments );

			  verify( mockImporterFactory ).getImporterForMode( eq( "csv" ), any( typeof( Args ) ), any( typeof( Config ) ), any( typeof( OutsideWorld ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void acceptsRelationshipsMetadata() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void AcceptsRelationshipsMetadata()
		 {
			  File homeDir = _testDir.directory( "home" );
			  ImporterFactory mockImporterFactory = mock( typeof( ImporterFactory ) );
			  Importer importer = mock( typeof( Importer ) );
			  when( mockImporterFactory.GetImporterForMode( eq( "csv" ), any( typeof( Args ) ), any( typeof( Config ) ), any( typeof( OutsideWorld ) ) ) ).thenReturn( importer );

			  ImportCommand importCommand = new ImportCommand( homeDir.toPath(), _testDir.directory("conf").toPath(), new RealOutsideWorld(System.out, System.err, new MemoryStream(new sbyte[0])), mockImporterFactory );

			  string[] arguments = new string[] { "--database=foo", "--from=bar", "--relationships:LIKES:HATES=mock.csv" };

			  importCommand.Execute( arguments );

			  verify( mockImporterFactory ).getImporterForMode( eq( "csv" ), any( typeof( Args ) ), any( typeof( Config ) ), any( typeof( OutsideWorld ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void requiresDatabaseArgument()
		 internal virtual void RequiresDatabaseArgument()
		 {
			  using ( NullOutsideWorld outsideWorld = new NullOutsideWorld() )
			  {
					ImportCommand importCommand = new ImportCommand( _testDir.directory( "home" ).toPath(), _testDir.directory("conf").toPath(), outsideWorld );

					string[] arguments = new string[] { "--mode=database", "--from=bar" };
					IncorrectUsage incorrectUsage = assertThrows( typeof( IncorrectUsage ), () => importCommand.execute(arguments) );
					assertThat( incorrectUsage.Message, containsString( "database" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void failIfInvalidModeSpecified()
		 internal virtual void FailIfInvalidModeSpecified()
		 {
			  using ( NullOutsideWorld outsideWorld = new NullOutsideWorld() )
			  {
					ImportCommand importCommand = new ImportCommand( _testDir.directory( "home" ).toPath(), _testDir.directory("conf").toPath(), outsideWorld );

					string[] arguments = new string[] { "--mode=foo", "--database=bar", "--from=baz" };
					IncorrectUsage incorrectUsage = assertThrows( typeof( IncorrectUsage ), () => importCommand.execute(arguments) );
					assertThat( incorrectUsage.Message, containsString( "foo" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void letImporterDecideAboutDatabaseExistence() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void LetImporterDecideAboutDatabaseExistence()
		 {
			  File report = _testDir.file( "report" );
			  Path homeDir = _testDir.directory( "home" ).toPath();
			  PrintStream nullOutput = new PrintStream( NULL_OUTPUT_STREAM );
			  OutsideWorld outsideWorld = new RealOutsideWorld( nullOutput, nullOutput, new MemoryStream( new sbyte[0] ) );
			  Path confPath = _testDir.directory( "conf" ).toPath();
			  ImportCommand importCommand = new ImportCommand( homeDir, confPath, outsideWorld );
			  File nodesFile = CreateTextFile( "nodes.csv", ":ID", "1", "2" );
			  string[] arguments = new string[] { "--mode=csv", "--database=existing.db", "--nodes=" + nodesFile.AbsolutePath, "--report-file=" + report.AbsolutePath };

			  // First run an import so that a database gets created
			  importCommand.Execute( arguments );

			  // When
			  ImporterFactory importerFactory = mock( typeof( ImporterFactory ) );
			  Importer importer = mock( typeof( Importer ) );
			  when( importerFactory.GetImporterForMode( any(), any(), any(), any() ) ).thenReturn(importer);
			  ( new ImportCommand( homeDir, confPath, outsideWorld, importerFactory ) ).Execute( arguments );

			  // Then no exception about database existence should be thrown
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldUseArgumentsFoundInside_f_Argument() throws java.io.FileNotFoundException, org.neo4j.commandline.admin.CommandFailed, org.neo4j.commandline.admin.IncorrectUsage
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldUseArgumentsFoundInsideFArgument()
		 {
			  // given
			  File report = _testDir.file( "report" );
			  ImportCommand importCommand = new ImportCommand( _testDir.directory( "home" ).toPath(), _testDir.directory("conf").toPath(), new RealOutsideWorld(System.out, System.err, new MemoryStream(new sbyte[0])) );
			  File nodesFile = CreateTextFile( "nodes.csv", ":ID", "1", "2" );
			  string pathWithEscapedSpaces = EscapeSpaces( nodesFile.AbsolutePath );
			  string reportEscapedPath = EscapeSpaces( report.AbsolutePath );
			  File argFile = CreateTextFile( "args.txt", "--database=foo", "--nodes=" + pathWithEscapedSpaces, "--report-file=" + reportEscapedPath );
			  string[] arguments = new string[] { "-f", argFile.AbsolutePath };

			  // when
			  importCommand.Execute( arguments );

			  // then
			  assertTrue( _suppressOutput.OutputVoice.containsMessage( "IMPORT DONE" ) );
			  assertTrue( _suppressOutput.ErrorVoice.containsMessage( nodesFile.AbsolutePath ) );
			  assertTrue( _suppressOutput.OutputVoice.containsMessage( "2 nodes" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldPrintNiceHelp() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldPrintNiceHelp()
		 {
			  using ( MemoryStream baos = new MemoryStream() )
			  {
					PrintStream ps = new PrintStream( baos );

					Usage usage = new Usage( "neo4j-admin", mock( typeof( CommandLocator ) ) );
					usage.PrintUsageForCommand( new ImportCommandProvider(), ps.println );

					assertEquals( string.Format( "usage: neo4j-admin import [--mode=csv] [--database=<name>]%n" + "                          [--additional-config=<config-file-path>]%n" + "                          [--report-file=<filename>]%n" + "                          [--nodes[:Label1:Label2]=<\"file1,file2,...\">]%n" + "                          [--relationships[:RELATIONSHIP_TYPE]=<\"file1,file2,...\">]%n" + "                          [--id-type=<STRING|INTEGER|ACTUAL>]%n" + "                          [--input-encoding=<character-set>]%n" + "                          [--ignore-extra-columns[=<true|false>]]%n" + "                          [--ignore-duplicate-nodes[=<true|false>]]%n" + "                          [--ignore-missing-nodes[=<true|false>]]%n" + "                          [--multiline-fields[=<true|false>]]%n" + "                          [--delimiter=<delimiter-character>]%n" + "                          [--array-delimiter=<array-delimiter-character>]%n" + "                          [--quote=<quotation-character>]%n" + "                          [--max-memory=<max-memory-that-importer-can-use>]%n" + "                          [--f=<File containing all arguments to this import>]%n" + "                          [--high-io=<true/false>]%n" + "usage: neo4j-admin import --mode=database [--database=<name>]%n" + "                          [--additional-config=<config-file-path>]%n" + "                          [--from=<source-directory>]%n" + "%n" + "environment variables:%n" + "    NEO4J_CONF    Path to directory which contains neo4j.conf.%n" + "    NEO4J_DEBUG   Set to anything to enable debug output.%n" + "    NEO4J_HOME    Neo4j home directory.%n" + "    HEAP_SIZE     Set JVM maximum heap size during command execution.%n" + "                  Takes a number and a unit, for example 512m.%n" + "%n" + "Import a collection of CSV files with --mode=csv (default), or a database from a%n" + "pre-3.0 installation with --mode=database.%n" + "%n" + "options:%n" + "  --database=<name>%n" + "      Name of database. [default:" + GraphDatabaseSettings.DEFAULT_DATABASE_NAME + "]%n" + "  --additional-config=<config-file-path>%n" + "      Configuration file to supply additional configuration in. [default:]%n" + "  --mode=<database|csv>%n" + "      Import a collection of CSV files or a pre-3.0 installation. [default:csv]%n" + "  --from=<source-directory>%n" + "      The location of the pre-3.0 database (e.g. <neo4j-root>/data/graph.db).%n" + "      [default:]%n" + "  --report-file=<filename>%n" + "      File in which to store the report of the csv-import.%n" + "      [default:import.report]%n" + "  --nodes[:Label1:Label2]=<\"file1,file2,...\">%n" + "      Node CSV header and data. Multiple files will be logically seen as one big%n" + "      file from the perspective of the importer. The first line must contain the%n" + "      header. Multiple data sources like these can be specified in one import,%n" + "      where each data source has its own header. Note that file groups must be%n" + "      enclosed in quotation marks. [default:]%n" + "  --relationships[:RELATIONSHIP_TYPE]=<\"file1,file2,...\">%n" + "      Relationship CSV header and data. Multiple files will be logically seen as%n" + "      one big file from the perspective of the importer. The first line must%n" + "      contain the header. Multiple data sources like these can be specified in%n" + "      one import, where each data source has its own header. Note that file%n" + "      groups must be enclosed in quotation marks. [default:]%n" + "  --id-type=<STRING|INTEGER|ACTUAL>%n" + "      Each node must provide a unique id. This is used to find the correct nodes%n" + "      when creating relationships. Possible values are:%n" + "        STRING: arbitrary strings for identifying nodes,%n" + "        INTEGER: arbitrary integer values for identifying nodes,%n" + "        ACTUAL: (advanced) actual node ids.%n" + "      For more information on id handling, please see the Neo4j Manual:%n" + "      https://neo4j.com/docs/operations-manual/current/tools/import/%n" + "      [default:STRING]%n" + "  --input-encoding=<character-set>%n" + "      Character set that input data is encoded in. [default:UTF-8]%n" + "  --ignore-extra-columns=<true|false>%n" + "      If un-specified columns should be ignored during the import.%n" + "      [default:false]%n" + "  --ignore-duplicate-nodes=<true|false>%n" + "      If duplicate nodes should be ignored during the import. [default:false]%n" + "  --ignore-missing-nodes=<true|false>%n" + "      If relationships referring to missing nodes should be ignored during the%n" + "      import. [default:false]%n" + "  --multiline-fields=<true|false>%n" + "      Whether or not fields from input source can span multiple lines, i.e.%n" + "      contain newline characters. [default:false]%n" + "  --delimiter=<delimiter-character>%n" + "      Delimiter character between values in CSV data. [default:,]%n" + "  --array-delimiter=<array-delimiter-character>%n" + "      Delimiter character between array elements within a value in CSV data.%n" + "      [default:;]%n" + "  --quote=<quotation-character>%n" + "      Character to treat as quotation character for values in CSV data. Quotes%n" + "      can be escaped as per RFC 4180 by doubling them, for example \"\" would be%n" + "      interpreted as a literal \". You cannot escape using \\. [default:\"]%n" + "  --max-memory=<max-memory-that-importer-can-use>%n" + "      Maximum memory that neo4j-admin can use for various data structures and%n" + "      caching to improve performance. Values can be plain numbers, like 10000000%n" + "      or e.g. 20G for 20 gigabyte, or even e.g. 70%%. [default:90%%]%n" + "  --f=<File containing all arguments to this import>%n" + "      File containing all arguments, used as an alternative to supplying all%n" + "      arguments on the command line directly.Each argument can be on a separate%n" + "      line or multiple arguments per line separated by space.Arguments%n" + "      containing spaces needs to be quoted.Supplying other arguments in addition%n" + "      to this file argument is not supported. [default:]%n" + "  --high-io=<true/false>%n" + "      Ignore environment-based heuristics, and assume that the target storage%n" + "      subsystem can support parallel IO with high throughput. [default:null]%n" ), baos.ToString() );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void putStoreInDirectory(java.nio.file.Path databaseDirectory) throws java.io.IOException
		 private static void PutStoreInDirectory( Path databaseDirectory )
		 {
			  Files.createDirectories( databaseDirectory );
			  Path storeFile = DatabaseLayout.of( databaseDirectory.toFile() ).metadataStore().toPath();
			  Files.createFile( storeFile );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File createTextFile(String name, String... lines) throws java.io.FileNotFoundException
		 private File CreateTextFile( string name, params string[] lines )
		 {
			  File file = _testDir.file( name );
			  using ( PrintStream @out = new PrintStream( file ) )
			  {
					foreach ( string line in lines )
					{
						 @out.println( line );
					}
			  }
			  return file;
		 }

		 private static string EscapeSpaces( string pathForFile )
		 {
			  return pathForFile.replaceAll( " ", "\\\\ " );
		 }
	}

}