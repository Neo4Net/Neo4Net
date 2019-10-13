﻿using System.Collections.Generic;

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
namespace Neo4Net.Helpers
{
	using ExternalResource = org.junit.rules.ExternalResource;


	using LoadableConfig = Neo4Net.Configuration.LoadableConfig;
	using Neo4Net.Graphdb.config;
	using Config = Neo4Net.Kernel.configuration.Config;

	public class Configuration : ExternalResource
	{
		 public const string DEFAULT = null;
		 private readonly IDictionary<string, string> _configuration = new Dictionary<string, string>();
		 private readonly IDictionary<string, string> _sysProperties = new Dictionary<string, string>();

		 public virtual Config Config( LoadableConfig settingsClasses )
		 {
			  return Config.builder().withSettings(_configuration).withConfigClasses(Collections.singletonList(settingsClasses)).build();
		 }

		 public virtual Configuration With<T1>( Setting<T1> setting, string value )
		 {
			  string key = setting.Name();
			  if ( string.ReferenceEquals( value, null ) )
			  {
					_configuration.Remove( key );
			  }
			  else
			  {
					_configuration[key] = value;
			  }
			  return this;
		 }

		 public virtual Configuration WithSystemProperty( string key, string value )
		 {
			  value = _sysProperties[key] = UpdateSystemProperty( key, value );
			  if ( !string.ReferenceEquals( value, null ) )
			  {
					// restore before we throw
					_sysProperties.Remove( key );
					UpdateSystemProperty( key, value );
					throw new System.ArgumentException( "Cannot update '" + key + "' more than once." );
			  }
			  return this;
		 }

		 protected internal override void After()
		 {
			  foreach ( KeyValuePair<string, string> entry in _sysProperties.SetOfKeyValuePairs() )
			  {
					UpdateSystemProperty( entry.Key, entry.Value );
			  }
		 }

		 private static string UpdateSystemProperty( string key, string value )
		 {
			  if ( string.ReferenceEquals( value, null ) )
			  {
					return System.clearProperty( key );
			  }
			  else
			  {
					return System.setProperty( key, value );
			  }
		 }
	}

}