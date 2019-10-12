using System.Collections.Generic;

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
namespace Org.Neo4j.Server
{

	using Args = Org.Neo4j.Helpers.Args;
	using Org.Neo4j.Helpers.Collection;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Converters = Org.Neo4j.Kernel.impl.util.Converters;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Pair.pair;

	/// <summary>
	/// Parses command line arguments for the server bootstrappers. Format is as follows:
	/// <ul>
	/// <li>Configuration file can be specified by <strong>--config=path/to/config.properties</strong> or
	/// <strong>-C=path/to/config.properties</strong></li>
	/// <li>Specific overridden configuration options, directly specified as arguments can be specified with
	/// <strong>-c key=value</strong>, for example <strong>-c dbms.active_database=foo.db</strong>
	/// or enabled boolean properties with <strong>-c key</strong>, f.ex <strong>-c dbms.readonly</strong>
	/// </ul>
	/// </summary>
	public class ServerCommandLineArgs
	{
		 public const string CONFIG_DIR_ARG = "config-dir";
		 public const string HOME_DIR_ARG = "home-dir";
		 public const string VERSION_ARG = "version";
		 private readonly Args _args;
		 private readonly IDictionary<string, string> _configOverrides;

		 private ServerCommandLineArgs( Args args, IDictionary<string, string> configOverrides )
		 {
			  this._args = args;
			  this._configOverrides = configOverrides;
		 }

		 public static ServerCommandLineArgs Parse( string[] argv )
		 {
			  Args args = Args.withFlags( VERSION_ARG ).parse( argv );
			  return new ServerCommandLineArgs( args, ParseConfigOverrides( args ) );
		 }

		 public virtual IDictionary<string, string> ConfigOverrides()
		 {
			  return _configOverrides;
		 }

		 public virtual Optional<File> ConfigFile()
		 {
			  return Optional.ofNullable( _args.get( CONFIG_DIR_ARG ) ).map( dirPath => new File( dirPath, Config.DEFAULT_CONFIG_FILE_NAME ) );
		 }

		 private static IDictionary<string, string> ParseConfigOverrides( Args arguments )
		 {
			  ICollection<Pair<string, string>> options = arguments.InterpretOptions("c", Converters.optional(), s =>
			  {
						  if ( s.contains( "=" ) )
						  {
								string[] keyVal = s.Split( "=", 2 );
								return pair( keyVal[0], keyVal[1] );
						  }
						  // Shortcut to specify boolean flags ("-c dbms.enableTheFeature")
						  return pair( s, "true" );
			  });

			  IDictionary<string, string> ret = stringMap();
			  options.forEach( pair => ret.put( pair.first(), pair.other() ) );

			  return ret;
		 }

		 public virtual File HomeDir()
		 {
			  if ( string.ReferenceEquals( _args.get( HOME_DIR_ARG ), null ) )
			  {
					return null;
			  }

			  return new File( _args.get( HOME_DIR_ARG ) );
		 }

		 public virtual bool Version()
		 {
			  return _args.getBoolean( VERSION_ARG, false ).Value;
		 }
	}

}