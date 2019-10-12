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
namespace Neo4Net.Kernel.configuration
{

	using InvalidSettingException = Neo4Net.Graphdb.config.InvalidSettingException;
	using SettingValidator = Neo4Net.Graphdb.config.SettingValidator;
	using Log = Neo4Net.Logging.Log;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.strict_config_validation;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;

	/// <summary>
	/// Validates individual settings by delegating to the settings themselves without taking other aspects into
	/// consideration.
	/// </summary>
	public class IndividualSettingsValidator : ConfigurationValidator
	{
		 private static readonly IList<string> _reservedPrefixes = Arrays.asList( "dbms.", "metrics.", "ha.", "causal_clustering.", "browser.", "tools.", "unsupported." );

		 private readonly ICollection<SettingValidator> _settingValidators;
		 private readonly bool _warnOnUnknownSettings;

		 internal IndividualSettingsValidator( ICollection<SettingValidator> settingValidators, bool warnOnUnknownSettings )
		 {
			  this._settingValidators = settingValidators;
			  this._warnOnUnknownSettings = warnOnUnknownSettings;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public java.util.Map<String,String> validate(@Nonnull Config config, @Nonnull Log log) throws org.neo4j.graphdb.config.InvalidSettingException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public override IDictionary<string, string> Validate( Config config, Log log )
		 {
			  IDictionary<string, string> rawConfig = config.Raw;
			  IDictionary<string, string> validConfig = stringMap();
			  foreach ( SettingValidator validator in _settingValidators )
			  {
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
					validConfig.putAll( validator.Validate( rawConfig, log.warn ) );
			  }

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean strictValidation = config.get(strict_config_validation);
			  bool strictValidation = config.Get( strict_config_validation );

			  rawConfig.forEach((key, value) =>
			  {
				if ( !validConfig.ContainsKey( key ) )
				{
					 // Plugins rely on custom config options being present.
					 // As a compromise, we only warn (and discard) for settings in our own "namespace"
					 if ( _reservedPrefixes.Any( key.StartsWith ) )
					 {
						  if ( _warnOnUnknownSettings )
						  {
								log.warn( "Unknown config option: %s", key );
						  }

						  if ( strictValidation )
						  {
								throw new InvalidSettingException( string.Format( "Unknown config option '{0}'. To resolve either remove it from your configuration " + "or set '{1}' to false.", key, strict_config_validation.name() ) );
						  }
						  else
						  {
								validConfig[key] = value;
						  }
					 }
					 else
					 {
						  validConfig[key] = value;
					 }
				}
			  });

			  return Collections.emptyMap();
		 }
	}

}