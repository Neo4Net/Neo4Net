using System;

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

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;

	/// <summary>
	/// Settings that can be provided in configurations are represented by instances of this interface, and are available
	/// as static fields in various *Settings classes.
	/// <para>
	/// This interface is available only for use, not for implementing. Implementing this interface is not expected, and
	/// backwards compatibility is not guaranteed for implementors.
	/// 
	/// </para>
	/// </summary>
	/// @param <T> type of value this setting will parse input string into and return. </param>
	/// @deprecated The settings API will be completely rewritten in 4.0 
	[Obsolete("The settings API will be completely rewritten in 4.0")]
	public interface Setting<T> : System.Func<System.Func<string, string>, T>, SettingValidator, SettingGroup<T>
	{
		 /// <summary>
		 /// Get the name of the setting. This typically corresponds to a key in a properties file, or similar.
		 /// </summary>
		 /// <returns> the name </returns>
		 string Name();

		 /// <summary>
		 /// Make this setting bound to a scope
		 /// </summary>
		 /// <param name="scopingRule"> The scoping rule to be applied to this setting </param>
		 void WithScope( System.Func<string, string> scopingRule );

		 /// <summary>
		 /// Get the default value of this setting, as a string.
		 /// </summary>
		 /// <returns> the default value </returns>
		 string DefaultValue { get; }

		 [Obsolete]
		 T From( Configuration config );

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default java.util.Map<String, T> values(java.util.Map<String, String> validConfig)
	//	 {
	//		  return singletonMap(name(), apply(validConfig::get));
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default java.util.Map<String, String> validate(java.util.Map<String, String> rawConfig, System.Action<String> warningConsumer) throws InvalidSettingException
	//	 {
	//		  // Validate setting, if present or default value otherwise
	//		  try
	//		  {
	//				apply(rawConfig::get);
	//				// only return if it was present though
	//				if (rawConfig.containsKey(name()))
	//				{
	//					 return stringMap(name(), rawConfig.get(name()));
	//				}
	//				else
	//				{
	//					 return emptyMap();
	//				}
	//		  }
	//		  catch (RuntimeException e)
	//		  {
	//				throw new InvalidSettingException(e.getMessage(), e);
	//		  }
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default java.util.List<Setting<T>> settings(java.util.Map<String, String> @params)
	//	 {
	//		  return Collections.singletonList(this);
	//	 }

		 /// <summary>
		 /// Get the function used to parse this setting.
		 /// </summary>
		 /// <returns> the parser function </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default java.util.Optional<System.Func<String, T>> getParser()
	//	 {
	//		  return Optional.empty();
	//	 }
	}

}