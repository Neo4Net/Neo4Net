using System.Collections;
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
namespace Neo4Net.Helpers.Collection
{

	public class CollectorsUtil
	{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public static <K,V> java.util.stream.Collector<java.util.Map.Entry<K,V>,?,java.util.Map<K,V>> entriesToMap()
		 public static Collector<KeyValuePair<K, V>, ?, IDictionary<K, V>> EntriesToMap<K, V>()
		 {
			  return Collectors.toMap( DictionaryEntry.getKey, DictionaryEntry.getValue );
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public static <K,V> java.util.stream.Collector<Pair<K,V>,?,java.util.Map<K,V>> pairsToMap()
		 public static Collector<Pair<K, V>, ?, IDictionary<K, V>> PairsToMap<K, V>()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return Collectors.toMap( Pair::first, Pair::other );
		 }
	}

}