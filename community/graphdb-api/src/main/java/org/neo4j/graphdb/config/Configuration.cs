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
namespace Org.Neo4j.Graphdb.config
{
	/// <summary>
	/// Provide the basic operation that one could perform on a set of configurations. </summary>
	/// @deprecated The settings API will be completely rewritten in 4.0 
	[Obsolete("The settings API will be completely rewritten in 4.0")]
	public interface Configuration
	{
		 /// <summary>
		 /// Retrieve the value of a configuration <seealso cref="Setting"/>.
		 /// </summary>
		 /// <param name="setting"> The configuration property </param>
		 /// @param <T> The type of the configuration property </param>
		 /// <returns> The value of the configuration property if the property is found, otherwise, return the default value
		 /// of the given property. </returns>
		 T get<T>( Setting<T> setting );

		 /// <summary>
		 /// Empty configuration without any settings.
		 /// </summary>
	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 Configuration EMPTY = new Configuration()
	//	 {
	//		  @@Override public <T> T get(Setting<T> setting)
	//		  {
	//				return null;
	//		  }
	//	 };

	}

}