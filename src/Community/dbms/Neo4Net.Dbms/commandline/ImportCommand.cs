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

	using AdminCommand = Neo4Net.CommandLine.Admin.AdminCommand;
	using CommandFailed = Neo4Net.CommandLine.Admin.CommandFailed;
	using IncorrectUsage = Neo4Net.CommandLine.Admin.IncorrectUsage;
	using OutsideWorld = Neo4Net.CommandLine.Admin.OutsideWorld;
	using Arguments = Neo4Net.CommandLine.Args.Arguments;
	using MandatoryNamedArg = Neo4Net.CommandLine.Args.MandatoryNamedArg;
	using OptionalBooleanArg = Neo4Net.CommandLine.Args.OptionalBooleanArg;
	using OptionalNamedArg = Neo4Net.CommandLine.Args.OptionalNamedArg;
	using OptionalNamedArgWithMetadata = Neo4Net.CommandLine.Args.OptionalNamedArgWithMetadata;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Args = Neo4Net.Helpers.Args;
	using Config = Neo4Net.Kernel.configuration.Config;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.commandline.arguments.common.Database.ARG_DATABASE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.csv.reader.Configuration_Fields.DEFAULT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.tooling.ImportTool.parseFileArgumentList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.Configuration_Fields.DEFAULT_MAX_MEMORY_PERCENT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.input.csv.Configuration.COMMAS;

	public class ImportCommand : AdminCommand
	{
		 public const string DEFAULT_REPORT_FILE_NAME = "import.report";
		 private static readonly string[] _allowedModes = new string[] { "database", "csv" };
		 private static readonly Arguments databaseArguments = new Arguments().withArgument(new MandatoryNamedArgAnonymousInnerClass())private static final Arguments csvArguments = new Arguments().withArgument(new OptionalNamedArg("mode", "csv", "csv", "Import a collection of CSV files."){@Override public string usage(){return string.Format("[--{0}={1}]", name(), exampleValue());

		 private class MandatoryNamedArgAnonymousInnerClass : MandatoryNamedArg
		 {
			 public MandatoryNamedArgAnonymousInnerClass() : base("mode", "database", "Import a pre-3.0 installation.")
			 {
			 }

			 public override string usage()
			 {
				  return string.Format( "--{0}={1}", name(), exampleValue() );
			 }
		 }
				  .withDatabase().withAdditionalConfig();
	}
}
				  ).withDatabase().withAdditionalConfig();

		 private static final Arguments allArguments = ( new Arguments() ).withDatabase().withAdditionalConfig().withArgument(new OptionalNamedArg("mode", allowedModes, "csv", "Import a collection of CSV files or a pre-3.0 installation."));

		 private static void includeDatabaseArguments( Arguments arguments )
		 {
			  arguments.withArgument( new OptionalNamedArg( "from", "source-directory", "", "The location of the pre-3.0 database (e.g. <Neo4Net-root>/data/graph.db)." ) );
		 }

		 private static void includeCsvArguments( Arguments arguments )
		 {
			  arguments.withArgument( new OptionalNamedArg( "report-file", "filename", DEFAULT_REPORT_FILE_NAME, "File in which to store the report of the csv-import." ) ).withArgument( new OptionalNamedArgWithMetadata( "nodes", ":Label1:Label2", "\"file1,file2,...\"", "", "Node CSV header and data. Multiple files will be logically seen as " + "one big file from the perspective of the importer. The first line " + "must contain the header. Multiple data sources like these can be " + "specified in one import, where each data source has its own header. " + "Note that file groups must be enclosed in quotation marks." ) ).withArgument( new OptionalNamedArgWithMetadata( "relationships", ":RELATIONSHIP_TYPE", "\"file1,file2,...\"", "", "Relationship CSV header and data. Multiple files will be logically " + "seen as one big file from the perspective of the importer. The first " + "line must contain the header. Multiple data sources like these can be " + "specified in one import, where each data source has its own header. " + "Note that file groups must be enclosed in quotation marks." ) ).withArgument( new OptionalNamedArg( "id-type", new string[]{ "STRING", "INTEGER", "ACTUAL" }, "STRING", "Each node must provide a unique id. This is used to find the correct " + "nodes when creating relationships. Possible values are:\n" + "  STRING: arbitrary strings for identifying nodes,\n" + "  INTEGER: arbitrary integer values for identifying nodes,\n" + "  ACTUAL: (advanced) actual node ids.\n" + "For more information on id handling, please see the Neo4Net Manual: " + "https://Neo4Net.com/docs/operations-manual/current/tools/import/" ) ).withArgument( new OptionalNamedArg( "input-encoding", "character-set", "UTF-8", "Character set that input data is encoded in." ) ).withArgument( new OptionalBooleanArg( "ignore-extra-columns", false, "If un-specified columns should be ignored during the import." ) ).withArgument( new OptionalBooleanArg( "ignore-duplicate-nodes", false, "If duplicate nodes should be ignored during the import." ) ).withArgument( new OptionalBooleanArg( "ignore-missing-nodes", false, "If relationships referring to missing nodes should be ignored during the import." ) ).withArgument( new OptionalBooleanArg( "multiline-fields", DEFAULT.multilineFields(), "Whether or not fields from input source can span multiple lines," + " i.e. contain newline characters." ) ).withArgument(new OptionalNamedArg("delimiter", "delimiter-character", COMMAS.delimiter().ToString(), "Delimiter character between values in CSV data.")).withArgument(new OptionalNamedArg("array-delimiter", "array-delimiter-character", COMMAS.arrayDelimiter().ToString(), "Delimiter character between array elements within a value in CSV data.")).withArgument(new OptionalNamedArg("quote", "quotation-character", COMMAS.quotationCharacter().ToString(), "Character to treat as quotation character for values in CSV data. " + "Quotes can be escaped as per RFC 4180 by doubling them, for example \"\" would be " + "interpreted as a literal \". You cannot escape using \\.")).withArgument(new OptionalNamedArg("max-memory", "max-memory-that-importer-can-use", DEFAULT_MAX_MEMORY_PERCENT.ToString() + "%", "Maximum memory that Neo4Net-admin can use for various data structures and caching " + "to improve performance. " + "Values can be plain numbers, like 10000000 or e.g. 20G for 20 gigabyte, or even e.g. 70%" + ".")).withArgument(new OptionalNamedArg("f", "File containing all arguments to this import", "", "File containing all arguments, used as an alternative to supplying all arguments on the command line directly." + "Each argument can be on a separate line or multiple arguments per line separated by space." + "Arguments containing spaces needs to be quoted." + "Supplying other arguments in addition to this file argument is not supported.")).withArgument(new OptionalNamedArg("high-io", "true/false", null, "Ignore environment-based heuristics, and assume that the target storage subsystem can support parallel IO with high throughput."));
		 }

//JAVA TO C# CONVERTER NOTE: This static initializer block is converted to a static constructor, but there is no current class:
		 static ImpliedClass()
		 {
			  includeDatabaseArguments( databaseArguments );
			  includeDatabaseArguments( allArguments );

			  includeCsvArguments( csvArguments );
			  includeCsvArguments( allArguments );
		 }

		 public static Arguments databaseArguments()
		 {
			  return databaseArguments;
		 }

		 public static Arguments csvArguments()
		 {
			  return csvArguments;
		 }

		 public static Arguments allArguments()
		 {
			  return allArguments;
		 }

		 private final Path homeDir;
		 private final Path configDir;
		 private final OutsideWorld outsideWorld;
		 private final ImporterFactory importerFactory;

		 public ImportCommand( Path homeDir, Path configDir, OutsideWorld outsideWorld )
		 {
			  this( homeDir, configDir, outsideWorld, new ImporterFactory() );
		 }

		 ImportCommand( Path homeDir, Path configDir, OutsideWorld outsideWorld, ImporterFactory importerFactory )
		 {
			  this.homeDir = homeDir;
			  this.configDir = configDir;
			  this.outsideWorld = outsideWorld;
			  this.importerFactory = importerFactory;
		 }

		 public void execute( string[] userSupplierArguments ) throws IncorrectUsage, CommandFailed
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String[] args;
			  string[] args;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String mode;
			  string mode;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Optional<java.nio.file.Path> additionalConfigFile;
			  Optional<Path> additionalConfigFile;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String database;
			  string database;

			  try
			  {
					args = getImportToolArgs( userSupplierArguments );
					allArguments.parse( args );
					mode = allArguments.get( "mode" );
					database = allArguments.get( ARG_DATABASE );
					additionalConfigFile = allArguments.getOptionalPath( "additional-config" );
			  }
			  catch ( System.ArgumentException e )
			  {
					throw new IncorrectUsage( e.Message );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }

			  try
			  {
					Config config = loadNeo4NetConfig( homeDir, configDir, database, loadAdditionalConfig( additionalConfigFile ) );

					Importer importer = importerFactory.getImporterForMode( mode, Args.parse( args ), config, outsideWorld );
					importer.DoImport();
			  }
			  catch ( System.ArgumentException e )
			  {
					throw new IncorrectUsage( e.Message );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 private static string[] getImportToolArgs( string[] userSupplierArguments ) throws IOException, IncorrectUsage
		 {
			  allArguments.parse( userSupplierArguments );
			  Optional<Path> fileArgument = allArguments.getOptionalPath( "f" );
			  return fileArgument.Present ? parseFileArgumentList( fileArgument.get().toFile() ) : userSupplierArguments;
		 }

		 private static Config loadAdditionalConfig( Optional<Path> additionalConfigFile )
		 {
			  return additionalConfigFile.map( path => Config.fromFile( path ).build() ).orElseGet(Config.defaults);
		 }

		 private static Config loadNeo4NetConfig( Path homeDir, Path configDir, string databaseName, Config additionalConfig )
		 {
			  Config config = Config.fromFile( configDir.resolve( Config.DEFAULT_CONFIG_FILE_NAME ) ).withHome( homeDir ).withConnectorsDisabled().withNoThrowOnFileLoadFailure().build();
			  config.Augment( additionalConfig );
			  config.augment( GraphDatabaseSettings.active_database, databaseName );
			  return config;
		 }
	}

}