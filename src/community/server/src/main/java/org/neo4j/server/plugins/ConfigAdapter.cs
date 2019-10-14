using System;
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
namespace Neo4Net.Server.plugins
{
	using AbstractConfiguration = org.apache.commons.configuration.AbstractConfiguration;

	using Config = Neo4Net.Kernel.configuration.Config;

	/// @deprecated Server plugins are deprecated for removal in the next major release. Please use unmanaged extensions instead. 
	[Obsolete("Server plugins are deprecated for removal in the next major release. Please use unmanaged extensions instead.")]
	public class ConfigAdapter : AbstractConfiguration
	{
		 private readonly Config _config;

		 [Obsolete]
		 public ConfigAdapter( Config config )
		 {
			  this._config = config;
		 }

		 [Obsolete]
		 public override bool Empty
		 {
			 get
			 {
				  // non-null config is always non-empty as some properties have default values
				  return _config == null;
			 }
		 }

		 [Obsolete]
		 public override bool ContainsKey( string key )
		 {
			  return _config.getValue( key ).Present;
		 }

		 [Obsolete]
		 public override object GetProperty( string key )
		 {
			  return _config.getValue( key ).orElse( null );
		 }

		 [Obsolete]
		 public override IEnumerator<string> Keys
		 {
			 get
			 {
				  return _config.ConfigValues.Keys.GetEnumerator();
			 }
		 }

		 [Obsolete]
		 protected internal override void AddPropertyDirect( string key, object value )
		 {
			  _config.augment( key, value.ToString() );
		 }
	}

}