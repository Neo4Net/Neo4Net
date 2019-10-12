using System;
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
namespace Neo4Net.Graphdb.config
{

	/// <summary>
	/// This interface represents a setting group. One example can be group defined by a common prefix, such as
	/// `dbms.connector.*`. The important aspect is that config keys can only be known after a config has been parsed.
	/// </summary>
	/// @deprecated The settings API will be completely rewritten in 4.0 
	[Obsolete("The settings API will be completely rewritten in 4.0")]
	public interface SettingGroup<T> : SettingValidator
	{
		 /// <summary>
		 /// Apply this setting group to the config and return all of its configured keys and their corresponding values.
		 /// </summary>
		 /// <param name="validConfig"> which can be examined. </param>
		 /// <returns> the map of this group's configured keys and values. </returns>
		 IDictionary<string, T> Values( IDictionary<string, string> validConfig );

		 /// <summary>
		 /// This will return a list of all settings belonging to this group based on the settings in {@code params} </summary>
		 /// <param name="params"> a map of all settings </param>
		 /// <returns> a list of the settings this group contains. </returns>
		 IList<Setting<T>> Settings( IDictionary<string, string> @params );

		 /// <returns> {@code true} if this setting is deprecated, false otherwise. </returns>
		 bool Deprecated();

		 /// <returns> the key of the setting which replaces this when its deprecated, empty if not deprecated. </returns>
		 Optional<string> Replacement();

		 /// <returns> {@code true} if internal setting, false otherwise. </returns>
		 bool Internal();

		 /// <returns> {@code true} if secret setting (should be hidden), false otherwise. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default boolean secret()
	//	 {
	//		  return false;
	//	 }

		 /// <returns> the documented default value if it needs special documentation, empty if default value is good as is. </returns>
		 Optional<string> DocumentedDefaultValue();

		 /// <returns> description of which values are good </returns>
		 string ValueDescription();

		 /// <returns> description of setting, empty in case no description exists. </returns>
		 Optional<string> Description();

		 /// <returns> {@code true} if the setting can be changed at runtime. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default boolean dynamic()
	//	 {
	//		  return false;
	//	 }
	}

}