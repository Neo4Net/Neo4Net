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

	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using Config = Neo4Net.Kernel.configuration.Config;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.stream;

	public class ProcedureConfig
	{
		 public const string PROC_ALLOWED_SETTING_DEFAULT_NAME = "dbms.security.procedures.default_allowed";
		 public const string PROC_ALLOWED_SETTING_ROLES = "dbms.security.procedures.roles";

		 private const string ROLES_DELIMITER = ",";
		 private const string SETTING_DELIMITER = ";";
		 private const string MAPPING_DELIMITER = ":";
		 private const string PROCEDURE_DELIMITER = ",";

		 private readonly string _defaultValue;
		 private readonly IList<ProcMatcher> _matchers;
		 private readonly IList<Pattern> _accessPatterns;
		 private readonly IList<Pattern> _whiteList;
		 private readonly ZoneId _defaultTemporalTimeZone;

		 private ProcedureConfig()
		 {
			  this._defaultValue = "";
			  this._matchers = Collections.emptyList();
			  this._accessPatterns = Collections.emptyList();
			  this._whiteList = Collections.singletonList( CompilePattern( "*" ) );
			  this._defaultTemporalTimeZone = UTC;
		 }

		 public ProcedureConfig( Config config )
		 {
			  this._defaultValue = config.GetValue( PROC_ALLOWED_SETTING_DEFAULT_NAME ).map( object.toString ).orElse( "" );

			  string allowedRoles = config.GetValue( PROC_ALLOWED_SETTING_ROLES ).map( object.toString ).orElse( "" );
			  this._matchers = Stream.of( allowedRoles.Split( SETTING_DELIMITER, true ) ).map( procToRoleSpec => procToRoleSpec.Split( MAPPING_DELIMITER ) ).filter( spec => spec.length > 1 ).map(spec =>
			  {
						  string[] roles = stream( spec[1].Split( ROLES_DELIMITER ) ).map( string.Trim ).toArray( string[]::new );
						  return new ProcMatcher( spec[0].Trim(), roles );
			  }).collect( Collectors.toList() );

			  this._accessPatterns = ParseMatchers( GraphDatabaseSettings.procedure_unrestricted.name(), config, PROCEDURE_DELIMITER, ProcedureConfig.compilePattern );
			  this._whiteList = ParseMatchers( GraphDatabaseSettings.procedure_whitelist.name(), config, PROCEDURE_DELIMITER, ProcedureConfig.compilePattern );
			  this._defaultTemporalTimeZone = config.Get( GraphDatabaseSettings.db_temporal_timezone );
		 }

		 private IList<T> ParseMatchers<T>( string configName, Config config, string delimiter, System.Func<string, T> matchFunc )
		 {
			  string fullAccessProcedures = config.GetValue( configName ).map( object.toString ).orElse( "" );
			  if ( fullAccessProcedures.Length == 0 )
			  {
					return Collections.emptyList();
			  }
			  else
			  {
					return Stream.of( fullAccessProcedures.Split( delimiter, true ) ).map( matchFunc ).collect( Collectors.toList() );
			  }
		 }

		 internal virtual string[] RolesFor( string procedureName )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  string[] wildCardRoles = _matchers.Where( matcher => matcher.matches( procedureName ) ).Select( ProcMatcher::roles ).Aggregate( new string[0], ( acc, next ) => Stream.concat( stream( acc ), stream( next ) ).toArray( string[]::new ) );
			  if ( wildCardRoles.Length > 0 )
			  {
					return wildCardRoles;
			  }
			  else
			  {
					return DefaultValue;
			  }
		 }

		 internal virtual bool FullAccessFor( string procedureName )
		 {
			  return _accessPatterns.Any( pattern => pattern.matcher( procedureName ).matches() );
		 }

		 internal virtual bool IsWhitelisted( string procedureName )
		 {
			  return _whiteList.Any( pattern => pattern.matcher( procedureName ).matches() );
		 }

		 private static Pattern CompilePattern( string procedure )
		 {
			  procedure = procedure.Trim().replaceAll("([\\[\\]\\\\?()^${}+|.])", "\\\\$1");
			  return Pattern.compile( procedure.replaceAll( "\\*", ".*" ) );
		 }

		 private string[] DefaultValue
		 {
			 get
			 {
				  return string.ReferenceEquals( _defaultValue, null ) || _defaultValue.Length == 0 ? new string[0] : new string[]{ _defaultValue };
			 }
		 }

		 internal static readonly ProcedureConfig Default = new ProcedureConfig();

		 public virtual ZoneId DefaultTemporalTimeZone
		 {
			 get
			 {
				  return _defaultTemporalTimeZone;
			 }
		 }

		 private class ProcMatcher
		 {
			  internal readonly Pattern Pattern;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly string[] RolesConflict;

			  internal ProcMatcher( string procedurePattern, string[] roles )
			  {
					this.Pattern = Pattern.compile( procedurePattern.replaceAll( "\\.", "\\\\." ).replaceAll( "\\*", ".*" ) );
					this.RolesConflict = roles;
			  }

			  internal virtual bool Matches( string procedureName )
			  {
					return Pattern.matcher( procedureName ).matches();
			  }

			  internal virtual string[] Roles()
			  {
					return RolesConflict;
			  }
		 }
	}

}