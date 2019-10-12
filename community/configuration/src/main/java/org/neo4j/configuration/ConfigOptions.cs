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
namespace Org.Neo4j.Configuration
{

	using Org.Neo4j.Graphdb.config;
	using Org.Neo4j.Graphdb.config;
	using Org.Neo4j.Graphdb.config;

	/// <summary>
	/// Describes one or several configuration options.
	/// </summary>
	public class ConfigOptions
	{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final org.neo4j.graphdb.config.SettingGroup<?> settingGroup;
		 private readonly SettingGroup<object> _settingGroup;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public ConfigOptions(@Nonnull SettingGroup<?> settingGroup)
		 public ConfigOptions<T1>( SettingGroup<T1> settingGroup )
		 {
			  this._settingGroup = settingGroup;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public org.neo4j.graphdb.config.SettingGroup<?> settingGroup()
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public virtual SettingGroup<object> SettingGroup()
		 {
			  return _settingGroup;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public java.util.List<ConfigValue> asConfigValues(@Nonnull Map<String,String> validConfig)
		 public virtual IList<ConfigValue> AsConfigValues( IDictionary<string, string> validConfig )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<String,org.neo4j.graphdb.config.Setting<?>> settings = settingGroup.settings(validConfig).stream().collect(java.util.stream.Collectors.toMap(org.neo4j.graphdb.config.Setting::name, s -> s));
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  IDictionary<string, Setting<object>> settings = _settingGroup.settings( validConfig ).ToDictionary( Setting::name, s => s );

			  return _settingGroup.values( validConfig ).SetOfKeyValuePairs().Select(val =>
			  {
						  BaseSetting<object> setting = ( BaseSetting ) settings[val.Key];
						  return new ConfigValue( setting.name(), setting.description(), setting.documentedDefaultValue(), Optional.ofNullable(val.Value), setting.valueDescription(), setting.@internal(), setting.dynamic(), setting.deprecated(), setting.replacement(), setting.secret() );
			  }).ToList();
		 }
	}

}