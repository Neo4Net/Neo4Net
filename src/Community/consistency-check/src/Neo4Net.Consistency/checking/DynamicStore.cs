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
namespace Neo4Net.Consistency.checking
{
	using RecordAccess = Neo4Net.Consistency.Store.RecordAccess;
	using Neo4Net.Consistency.Store;
	using DynamicRecord = Neo4Net.Kernel.Impl.Store.Records.DynamicRecord;

	public abstract class DynamicStore
	{
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       SCHEMA(org.Neo4Net.consistency.RecordType.SCHEMA) { RecordReference<org.Neo4Net.kernel.impl.store.record.DynamicRecord> lookup(org.Neo4Net.consistency.store.RecordAccess records, long block) { return records.schema(block); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       STRING(org.Neo4Net.consistency.RecordType.STRING_PROPERTY) { RecordReference<org.Neo4Net.kernel.impl.store.record.DynamicRecord> lookup(org.Neo4Net.consistency.store.RecordAccess records, long block) { return records.string(block); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       ARRAY(org.Neo4Net.consistency.RecordType.ARRAY_PROPERTY) { RecordReference<org.Neo4Net.kernel.impl.store.record.DynamicRecord> lookup(org.Neo4Net.consistency.store.RecordAccess records, long block) { return records.array(block); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       PROPERTY_KEY(org.Neo4Net.consistency.RecordType.PROPERTY_KEY_NAME) { RecordReference<org.Neo4Net.kernel.impl.store.record.DynamicRecord> lookup(org.Neo4Net.consistency.store.RecordAccess records, long block) { return records.propertyKeyName((int) block); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       RELATIONSHIP_TYPE(org.Neo4Net.consistency.RecordType.RELATIONSHIP_TYPE_NAME) { RecordReference<org.Neo4Net.kernel.impl.store.record.DynamicRecord> lookup(org.Neo4Net.consistency.store.RecordAccess records, long block) { return records.relationshipTypeName((int) block); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       LABEL(org.Neo4Net.consistency.RecordType.LABEL_NAME) { RecordReference<org.Neo4Net.kernel.impl.store.record.DynamicRecord> lookup(org.Neo4Net.consistency.store.RecordAccess records, long block) { return records.labelName((int) block); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       NODE_LABEL(org.Neo4Net.consistency.RecordType.NODE_DYNAMIC_LABEL) { RecordReference<org.Neo4Net.kernel.impl.store.record.DynamicRecord> lookup(org.Neo4Net.consistency.store.RecordAccess records, long block) { return records.nodeLabels(block); } };

		 private static readonly IList<DynamicStore> valueList = new List<DynamicStore>();

		 static DynamicStore()
		 {
			 valueList.Add( SCHEMA );
			 valueList.Add( STRING );
			 valueList.Add( ARRAY );
			 valueList.Add( PROPERTY_KEY );
			 valueList.Add( RELATIONSHIP_TYPE );
			 valueList.Add( LABEL );
			 valueList.Add( NODE_LABEL );
		 }

		 public enum InnerEnum
		 {
			 SCHEMA,
			 STRING,
			 ARRAY,
			 PROPERTY_KEY,
			 RELATIONSHIP_TYPE,
			 LABEL,
			 NODE_LABEL
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private DynamicStore( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public readonly Neo4Net.Consistency.RecordType type;

		 internal DynamicStore( string name, InnerEnum innerEnum, Neo4Net.Consistency.RecordType type )
		 {
			  this.Type = type;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 internal abstract Neo4Net.Consistency.Store.RecordReference<Neo4Net.Kernel.Impl.Store.Records.DynamicRecord> lookup( Neo4Net.Consistency.Store.RecordAccess records, long block );

		public static IList<DynamicStore> values()
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

		public static DynamicStore ValueOf( string name )
		{
			foreach ( DynamicStore enumInstance in DynamicStore.valueList )
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