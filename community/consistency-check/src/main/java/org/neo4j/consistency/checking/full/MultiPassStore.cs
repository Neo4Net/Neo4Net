﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Consistency.checking.full
{

	using CacheAccess = Org.Neo4j.Consistency.checking.cache.CacheAccess;
	using ConsistencyReporter = Org.Neo4j.Consistency.report.ConsistencyReporter;
	using Monitor = Org.Neo4j.Consistency.report.ConsistencyReporter.Monitor;
	using InconsistencyReport = Org.Neo4j.Consistency.report.InconsistencyReport;
	using FilteringRecordAccess = Org.Neo4j.Consistency.store.FilteringRecordAccess;
	using RecordAccess = Org.Neo4j.Consistency.store.RecordAccess;
	using Org.Neo4j.Kernel.impl.store;
	using StoreAccess = Org.Neo4j.Kernel.impl.store.StoreAccess;

	public abstract class MultiPassStore
	{
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       NODES { RecordStore<JavaToDotNetGenericWildcard> getRecordStore(org.neo4j.kernel.impl.store.StoreAccess storeAccess) { return storeAccess.getNodeStore(); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       RELATIONSHIPS { RecordStore<JavaToDotNetGenericWildcard> getRecordStore(org.neo4j.kernel.impl.store.StoreAccess storeAccess) { return storeAccess.getRelationshipStore(); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       PROPERTIES { RecordStore<JavaToDotNetGenericWildcard> getRecordStore(org.neo4j.kernel.impl.store.StoreAccess storeAccess) { return storeAccess.getPropertyStore(); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       PROPERTY_KEYS { RecordStore<JavaToDotNetGenericWildcard> getRecordStore(org.neo4j.kernel.impl.store.StoreAccess storeAccess) { return storeAccess.getPropertyKeyTokenStore(); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       STRINGS { RecordStore<JavaToDotNetGenericWildcard> getRecordStore(org.neo4j.kernel.impl.store.StoreAccess storeAccess) { return storeAccess.getNodeStore(); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       ARRAYS { RecordStore<JavaToDotNetGenericWildcard> getRecordStore(org.neo4j.kernel.impl.store.StoreAccess storeAccess) { return storeAccess.getNodeStore(); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       LABELS { RecordStore<JavaToDotNetGenericWildcard> getRecordStore(org.neo4j.kernel.impl.store.StoreAccess storeAccess) { return storeAccess.getLabelTokenStore(); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       RELATIONSHIP_GROUPS { RecordStore<JavaToDotNetGenericWildcard> getRecordStore(org.neo4j.kernel.impl.store.StoreAccess storeAccess) { return storeAccess.getRelationshipGroupStore(); } };

		 private static readonly IList<MultiPassStore> valueList = new List<MultiPassStore>();

		 static MultiPassStore()
		 {
			 valueList.Add( NODES );
			 valueList.Add( RELATIONSHIPS );
			 valueList.Add( PROPERTIES );
			 valueList.Add( PROPERTY_KEYS );
			 valueList.Add( STRINGS );
			 valueList.Add( ARRAYS );
			 valueList.Add( LABELS );
			 valueList.Add( RELATIONSHIP_GROUPS );
		 }

		 public enum InnerEnum
		 {
			 NODES,
			 RELATIONSHIPS,
			 PROPERTIES,
			 PROPERTY_KEYS,
			 STRINGS,
			 ARRAYS,
			 LABELS,
			 RELATIONSHIP_GROUPS
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private MultiPassStore( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public IList<Org.Neo4j.Consistency.store.RecordAccess> MultiPassFilters( Org.Neo4j.Consistency.store.RecordAccess recordAccess, MultiPassStore[] stores )
		 {
			  IList<RecordAccess> filteringStores = new List<RecordAccess>();
			  filteringStores.Add( new FilteringRecordAccess( recordAccess, this, stores ) );
			  return filteringStores;
		 }

		 public Org.Neo4j.Consistency.store.RecordAccess MultiPassFilter( Org.Neo4j.Consistency.store.RecordAccess recordAccess, params MultiPassStore[] stores )
		 {
			  return new FilteringRecordAccess( recordAccess, this, stores );
		 }

		 internal abstract Org.Neo4j.Kernel.impl.store.RecordStore<JavaToDotNetGenericWildcard> getRecordStore( Org.Neo4j.Kernel.impl.store.StoreAccess storeAccess );

//JAVA TO C# CONVERTER TODO TASK: Java to C# Converter does not convert types within enums:
//		 static class Factory
	//	 {
	//		  private final CheckDecorator decorator;
	//		  private final RecordAccess recordAccess;
	//		  private final InconsistencyReport report;
	//		  private final CacheAccess cacheAccess;
	//		  private final Monitor monitor;
	//
	//		  Factory(CheckDecorator decorator, RecordAccess recordAccess, CacheAccess cacheAccess, InconsistencyReport report, Monitor monitor)
	//		  {
	//				this.decorator = decorator;
	//				this.recordAccess = recordAccess;
	//				this.cacheAccess = cacheAccess;
	//				this.report = report;
	//				this.monitor = monitor;
	//		  }
	//
	//		  ConsistencyReporter[] reporters(MultiPassStore... stores)
	//		  {
	//				List<ConsistencyReporter> result = new ArrayList<>();
	//				for (MultiPassStore store : stores)
	//				{
	//					 List<RecordAccess> filters = store.multiPassFilters(recordAccess, stores);
	//					 for (RecordAccess filter : filters)
	//					 {
	//						  result.add(new ConsistencyReporter(filter, report));
	//					 }
	//				}
	//				return result.toArray(new ConsistencyReporter[result.size()]);
	//		  }
	//
	//		  ConsistencyReporter reporter(MultiPassStore store)
	//		  {
	//				RecordAccess filter = store.multiPassFilter(recordAccess, store);
	//				return new ConsistencyReporter(filter, report, monitor);
	//		  }
	//
	//		  StoreProcessor processor(Stage stage, MultiPassStore store)
	//		  {
	//				return new StoreProcessor(decorator, reporter(store), stage, cacheAccess);
	//		  }
	//
	//		  public void reDecorateNode(StoreProcessor processer, NodeRecordCheck newChecker, boolean sparseNode)
	//		  {
	//				processer.reDecorateNode(decorator, newChecker, sparseNode);
	//		  }
	//
	//		  public void reDecorateRelationship(StoreProcessor processer, RelationshipRecordCheck newChecker)
	//		  {
	//				processer.reDecorateRelationship(decorator, newChecker);
	//		  }
	//	 }

		public static IList<MultiPassStore> values()
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

		public static MultiPassStore valueOf( string name )
		{
			foreach ( MultiPassStore enumInstance in MultiPassStore.valueList )
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