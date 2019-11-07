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
namespace Neo4Net.@unsafe.Impl.Batchimport.input.csv
{
	using Neo4Net.Csv.Reader;
	using Extractors = Neo4Net.Csv.Reader.Extractors;
	using NumberArrayFactory = Neo4Net.@unsafe.Impl.Batchimport.cache.NumberArrayFactory;
	using IdMapper = Neo4Net.@unsafe.Impl.Batchimport.cache.idmapping.IdMapper;
	using IdMappers = Neo4Net.@unsafe.Impl.Batchimport.cache.idmapping.IdMappers;

	/// <summary>
	/// Defines different types that input ids can come in. Enum names in here are user facing.
	/// </summary>
	/// <seealso cref= Header.Entry#extractor() </seealso>
	public abstract class IdType
	{
		 /// <summary>
		 /// Used when node ids int input data are any string identifier.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       STRING { public Neo4Net.csv.reader.Extractor<JavaToDotNetGenericWildcard> extractor(Neo4Net.csv.reader.Extractors extractors) { return extractors.string(); } public Neo4Net.unsafe.impl.batchimport.cache.idmapping.IdMapper idMapper(Neo4Net.unsafe.impl.batchimport.cache.NumberArrayFactory numberArrayFactory, Neo4Net.unsafe.impl.batchimport.input.Groups groups) { return Neo4Net.unsafe.impl.batchimport.cache.idmapping.IdMappers.strings(numberArrayFactory, groups); } },

		 /// <summary>
		 /// Used when node ids int input data are any integer identifier. It uses 8b longs for storage,
		 /// but as a user facing enum a better name is integer
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       INTEGER { public Neo4Net.csv.reader.Extractor<JavaToDotNetGenericWildcard> extractor(Neo4Net.csv.reader.Extractors extractors) { return extractors.long_(); } public Neo4Net.unsafe.impl.batchimport.cache.idmapping.IdMapper idMapper(Neo4Net.unsafe.impl.batchimport.cache.NumberArrayFactory numberArrayFactory, Neo4Net.unsafe.impl.batchimport.input.Groups groups) { return Neo4Net.unsafe.impl.batchimport.cache.idmapping.IdMappers.longs(numberArrayFactory, groups); } },

		 /// <summary>
		 /// Used when node ids int input data are specified as long values and points to actual record ids.
		 /// ADVANCED usage. Performance advantage, but requires carefully planned input data.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       ACTUAL { public Neo4Net.csv.reader.Extractor<JavaToDotNetGenericWildcard> extractor(Neo4Net.csv.reader.Extractors extractors) { return extractors.long_(); } public Neo4Net.unsafe.impl.batchimport.cache.idmapping.IdMapper idMapper(Neo4Net.unsafe.impl.batchimport.cache.NumberArrayFactory numberArrayFactory, Neo4Net.unsafe.impl.batchimport.input.Groups groups) { return Neo4Net.unsafe.impl.batchimport.cache.idmapping.IdMappers.actual(); } };

		 private static readonly IList<IdType> valueList = new List<IdType>();

		 static IdType()
		 {
			 valueList.Add( STRING );
			 valueList.Add( INTEGER );
			 valueList.Add( ACTUAL );
		 }

		 public enum InnerEnum
		 {
			 STRING,
			 INTEGER,
			 ACTUAL
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private IdType( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public abstract Neo4Net.@unsafe.Impl.Batchimport.cache.idmapping.IdMapper idMapper( Neo4Net.@unsafe.Impl.Batchimport.cache.NumberArrayFactory numberArrayFactory, Neo4Net.@unsafe.Impl.Batchimport.input.Groups groups );

		 public abstract Neo4Net.Csv.Reader.Extractor<JavaToDotNetGenericWildcard> extractor( Neo4Net.Csv.Reader.Extractors extractors );

		public static IList<IdType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public override string ToString()
		{
			return nameValue;
		}

		public static IdType ValueOf( string name )
		{
			foreach ( IdType enumInstance in IdType.valueList )
			{
				if ( enumInstance.nameValue == name )
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException( name );
		}
	}

}