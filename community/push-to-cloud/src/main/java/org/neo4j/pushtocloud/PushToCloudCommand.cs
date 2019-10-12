using System;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace Org.Neo4j.Pushtocloud
{

	using AdminCommand = Org.Neo4j.Commandline.admin.AdminCommand;
	using CommandFailed = Org.Neo4j.Commandline.admin.CommandFailed;
	using IncorrectUsage = Org.Neo4j.Commandline.admin.IncorrectUsage;
	using OutsideWorld = Org.Neo4j.Commandline.admin.OutsideWorld;
	using Arguments = Org.Neo4j.Commandline.arguments.Arguments;
	using MandatoryNamedArg = Org.Neo4j.Commandline.arguments.MandatoryNamedArg;
	using OptionalNamedArg = Org.Neo4j.Commandline.arguments.OptionalNamedArg;
	using Database = Org.Neo4j.Commandline.arguments.common.Database;
	using Args = Org.Neo4j.Helpers.Args;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;

	public class PushToCloudCommand : AdminCommand
	{
		 internal const string ARG_DATABASE = "database";
		 internal const string ARG_BOLT_URI = "bolt-uri";
		 internal const string ARG_DUMP = "dump";
		 internal const string ARG_DUMP_TO = "dump-to";
		 internal const string ARG_VERBOSE = "v";
		 internal const string ARG_CONFIRMED = "confirmed";
		 internal const string ARG_USERNAME = "username";
		 internal const string ARG_PASSWORD = "password";
		 internal const string ENV_USERNAME = "NEO4J_USERNAME";
		 internal const string ENV_PASSWORD = "NEO4J_PASSWORD";

		 internal static readonly Arguments Arguments = new Arguments().withDatabase().withArgument(new OptionalNamedArg(ARG_DUMP, "/path/to/my-neo4j-database-dump-file", null, "Existing dump of a database, produced from the dump command")).withArgument(new OptionalNamedArg(ARG_DUMP_TO, "/path/to/dump-file-to-be-created", null, "Location to create the dump file if database is given. The database will be dumped to this file instead of a default location")).withArgument(new MandatoryNamedArg(ARG_BOLT_URI, "bolt+routing://mydatabaseid.databases.neo4j.io", "Bolt URI pointing out the target location to push the database to")).withArgument(new OptionalNamedArg(ARG_VERBOSE, "true/false", null, "Whether or not to be verbose about internal details and errors.")).withArgument(new OptionalNamedArg(ARG_USERNAME, "neo4j", null, "Optional: Username of the target database to push this database to. Prompt will ask for username if not provided. " + "Alternatively NEO4J_USERNAME environment variable can be used.")).withArgument(new OptionalNamedArg(ARG_PASSWORD, "true/false", null, "Optional: Password of the target database to push this database to. Prompt will ask for password if not provided. " + "Alternatively NEO4J_PASSWORD environment variable can be used.")).withArgument(new OptionalNamedArg(ARG_CONFIRMED, "true/false", "false", "Optional: Allow import to continue even if it will overwrite data in a non-empty cloud database."));

		 private readonly Path _homeDir;
		 private readonly Path _configDir;
		 private readonly OutsideWorld _outsideWorld;
		 private readonly Copier _copier;
		 private readonly DumpCreator _dumpCreator;

		 public PushToCloudCommand( Path homeDir, Path configDir, OutsideWorld outsideWorld, Copier copier, DumpCreator dumpCreator )
		 {
			  this._homeDir = homeDir;
			  this._configDir = configDir;
			  this._outsideWorld = outsideWorld;
			  this._copier = copier;
			  this._dumpCreator = dumpCreator;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void execute(String[] args) throws org.neo4j.commandline.admin.IncorrectUsage, org.neo4j.commandline.admin.CommandFailed
		 public override void Execute( string[] args )
		 {
			  Args arguments = Args.parse( args );
			  bool verbose = arguments.GetBoolean( ARG_VERBOSE );
			  try
			  {
					string passwordFromArg = arguments.Get( ARG_PASSWORD );
					string username = arguments.Get( ARG_USERNAME );

					string usernameFromEnv = Environment.GetEnvironmentVariable( ENV_USERNAME );
					string passwordFromEnv = Environment.GetEnvironmentVariable( ENV_PASSWORD );

					if ( ( string.ReferenceEquals( username, null ) && !string.ReferenceEquals( passwordFromArg, null ) ) || ( !string.ReferenceEquals( username, null ) && string.ReferenceEquals( passwordFromArg, null ) ) )
					{
						 throw new IncorrectUsage( "Provide either 'username' and 'password' as argument or none." );
					}
					if ( ( string.ReferenceEquals( usernameFromEnv, null ) && !string.ReferenceEquals( passwordFromEnv, null ) ) || ( !string.ReferenceEquals( usernameFromEnv, null ) && string.ReferenceEquals( passwordFromEnv, null ) ) )
					{
						 throw new IncorrectUsage( "Provide either 'ENV_USERNAME' and 'ENV_PASSWORD' as environment variable or none." );
					}
					if ( !string.ReferenceEquals( passwordFromEnv, null ) && !string.ReferenceEquals( passwordFromArg, null ) )
					{
						 throw new IncorrectUsage( "It is not allowed to provide 'username' and 'password' as argument and environment variable." );
					}

					if ( string.ReferenceEquals( username, null ) )
					{
						 if ( !string.ReferenceEquals( usernameFromEnv, null ) )
						 {
							  username = usernameFromEnv;
						 }
						 else
						 {
							  username = _outsideWorld.promptLine( "Neo4j cloud database user name: " );
						 }
					}
					char[] password;
					if ( !string.ReferenceEquals( passwordFromArg, null ) )
					{
						 password = passwordFromArg.ToCharArray();
					}
					else
					{
						 if ( !string.ReferenceEquals( passwordFromEnv, null ) )
						 {
							  password = passwordFromEnv.ToCharArray();
						 }
						 else
						 {
							  password = _outsideWorld.promptPassword( "Neo4j cloud database password: " );
						 }
					}

					string boltURI = arguments.Get( ARG_BOLT_URI );
					string confirmationViaArgument = arguments.Get( ARG_CONFIRMED );

					string consoleURL = BuildConsoleURI( boltURI );
					string bearerToken = _copier.authenticate( verbose, consoleURL, username, password, "true".Equals( confirmationViaArgument ) );

					Path source = InitiateSource( arguments );

					_copier.copy( verbose, consoleURL, source, bearerToken );
			  }
			  catch ( Exception e )
			  {
					if ( verbose )
					{
						 _outsideWorld.printStacktrace( e );
					}
					throw e;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private String buildConsoleURI(String boltURI) throws org.neo4j.commandline.admin.IncorrectUsage
		 private string BuildConsoleURI( string boltURI )
		 {
			  // A boltURI looks something like this:
			  //
			  //   bolt+routing://mydbid-myenvironment.databases.neo4j.io
			  //                  <─┬──><──────┬─────>
			  //                    │          └──────── environment
			  //                    └─────────────────── database id
			  //
			  // Constructing a console URI takes elements from the bolt URI and places them inside this URI:
			  //
			  //   https://console<environment>.neo4j.io/v1/databases/<database id>
			  //
			  // Examples:
			  //
			  //   bolt+routing://rogue.databases.neo4j.io  --> https://console.neo4j.io/v1/databases/rogue
			  //   bolt+routing://rogue-mattias.databases.neo4j.io  --> https://console-mattias.neo4j.io/v1/databases/rogue

			  Pattern pattern = Pattern.compile( "bolt\\+routing://([^-]+)(-(.+))?.databases.neo4j.io$" );
			  Matcher matcher = pattern.matcher( boltURI );
			  if ( !matcher.matches() )
			  {
					throw new IncorrectUsage( "Invalid Bolt URI '" + boltURI + "'" );
			  }

			  string databaseId = matcher.group( 1 );
			  string environment = matcher.group( 2 );
			  return string.Format( "https://console{0}.neo4j.io/v1/databases/{1}", string.ReferenceEquals( environment, null ) ? "" : environment, databaseId );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.nio.file.Path initiateSource(org.neo4j.helpers.Args arguments) throws org.neo4j.commandline.admin.IncorrectUsage, org.neo4j.commandline.admin.CommandFailed
		 private Path InitiateSource( Args arguments )
		 {
			  // Either a dump or database name (of a stopped database) can be provided
			  string dump = arguments.Get( ARG_DUMP );
			  string database = arguments.Get( ARG_DATABASE );
			  if ( !string.ReferenceEquals( dump, null ) && !string.ReferenceEquals( database, null ) )
			  {
					throw new IncorrectUsage( "Provide either a dump or database name, not both" );
			  }
			  else if ( !string.ReferenceEquals( dump, null ) )
			  {
					Path path = Paths.get( dump );
					if ( !Files.exists( path ) )
					{
						 throw new CommandFailed( format( "The provided dump '%s' file doesn't exist", path ) );
					}
					return path;
			  }
			  else
			  {
					if ( string.ReferenceEquals( database, null ) )
					{
						 database = ( new Database() ).defaultValue();
						 _outsideWorld.stdOutLine( "Selecting default database '" + database + "'" );
					}

					string to = arguments.Get( ARG_DUMP_TO );
					Path dumpFile = !string.ReferenceEquals( to, null ) ? Paths.get( to ) : _homeDir.resolve( "dump-of-" + database + "-" + currentTimeMillis() );
					if ( Files.exists( dumpFile ) )
					{
						 throw new CommandFailed( format( "The provided dump-to target '%s' file already exists", dumpFile ) );
					}
					_dumpCreator.dumpDatabase( database, dumpFile );
					return dumpFile;
			  }
		 }

		 public interface Copier
		 {
			  /// <summary>
			  /// Authenticates user by name and password.
			  /// </summary>
			  /// <param name="verbose"> whether or not to print verbose debug messages/statuses. </param>
			  /// <param name="consoleURL"> console URI to target. </param>
			  /// <param name="username"> the username. </param>
			  /// <param name="password"> the password. </param>
			  /// <param name="consentConfirmed"> user confirmed to overwrite existing database. </param>
			  /// <returns> a bearer token to pass into <seealso cref="copy(bool, string, Path, string)"/> later on. </returns>
			  /// <exception cref="CommandFailed"> on authentication failure or some other unexpected failure. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: String authenticate(boolean verbose, String consoleURL, String username, char[] password, boolean consentConfirmed) throws org.neo4j.commandline.admin.CommandFailed;
			  string Authenticate( bool verbose, string consoleURL, string username, char[] password, bool consentConfirmed );

			  /// <summary>
			  /// Copies the given dump to the console URI.
			  /// </summary>
			  /// <param name="verbose"> whether or not to print verbose debug messages/statuses. </param>
			  /// <param name="consoleURL"> console URI to target. </param>
			  /// <param name="source"> dump to copy to the target. </param>
			  /// <param name="bearerToken"> token from successful <seealso cref="authenticate(bool, string, string, char[])"/> call. </param>
			  /// <exception cref="CommandFailed"> on copy failure or some other unexpected failure. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void copy(boolean verbose, String consoleURL, java.nio.file.Path source, String bearerToken) throws org.neo4j.commandline.admin.CommandFailed;
			  void Copy( bool verbose, string consoleURL, Path source, string bearerToken );
		 }

		 public interface DumpCreator
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void dumpDatabase(String databaseName, java.nio.file.Path targetDumpFile) throws org.neo4j.commandline.admin.CommandFailed, org.neo4j.commandline.admin.IncorrectUsage;
			  void DumpDatabase( string databaseName, Path targetDumpFile );
		 }
	}

}