﻿/*
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
namespace Org.Neo4j.Kernel.Api.Index
{

	using Value = Org.Neo4j.Values.Storable.Value;

	/// <summary>
	/// Inherited by <seealso cref="IndexAccessor"/> and <seealso cref="IndexPopulator"/>.
	/// </summary>
	public interface IndexConfigProvider
	{
		 /// <summary>
		 /// Get index configurations used by this index at runtime.
		 /// </summary>
		 /// <returns> <seealso cref="System.Collections.IDictionary"/> describing index configurations for this index. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default java.util.Map<String, org.neo4j.values.storable.Value> indexConfig()
	//	 {
	//		  return Collections.emptyMap();
	//	 }

		 /// <summary>
		 /// Add all entries from source to target and make sure </summary>
		 /// <param name="target"> <seealso cref="System.Collections.IDictionary"/> to which entries are added. </param>
		 /// <param name="source"> <seealso cref="System.Collections.IDictionary"/> from which entries are taken, will not be modified. </param>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static void putAllNoOverwrite(java.util.Map<String, org.neo4j.values.storable.Value> target, java.util.Map<String, org.neo4j.values.storable.Value> source)
	//	 {
	//		  for (Map.Entry<String,Value> partEntry : source.entrySet())
	//		  {
	//				String key = partEntry.getKey();
	//				Value value = partEntry.getValue();
	//				if (target.containsKey(key))
	//				{
	//					 throw new IllegalStateException(String.format("Adding config would overwrite existing value: key=%s, newValue=%s, oldValue=%s", key, value, target.get(key)));
	//				}
	//				target.put(key, value);
	//		  }
	//	 }
	}

}