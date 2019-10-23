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
namespace Neo4Net.Kernel.impl.store
{

	using DatabaseFile = Neo4Net.Io.layout.DatabaseFile;
	using CountsTracker = Neo4Net.Kernel.impl.store.counts.CountsTracker;

	public abstract class StoreType
	{
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       NODE_LABEL(org.Neo4Net.io.layout.DatabaseFile.NODE_LABEL_STORE, true, false) { public CommonAbstractStore open(NeoStores neoStores) { return neoStores.createNodeLabelStore(); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       NODE(org.Neo4Net.io.layout.DatabaseFile.NODE_STORE, true, false) { public CommonAbstractStore open(NeoStores neoStores) { return neoStores.createNodeStore(); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       PROPERTY_KEY_TOKEN_NAME(org.Neo4Net.io.layout.DatabaseFile.PROPERTY_KEY_TOKEN_NAMES_STORE, true, true) { public CommonAbstractStore open(NeoStores neoStores) { return neoStores.createPropertyKeyTokenNamesStore(); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       PROPERTY_KEY_TOKEN(org.Neo4Net.io.layout.DatabaseFile.PROPERTY_KEY_TOKEN_STORE, true, true) { public CommonAbstractStore open(NeoStores neoStores) { return neoStores.createPropertyKeyTokenStore(); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       PROPERTY_STRING(org.Neo4Net.io.layout.DatabaseFile.PROPERTY_STRING_STORE, true, false) { public CommonAbstractStore open(NeoStores neoStores) { return neoStores.createPropertyStringStore(); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       PROPERTY_ARRAY(org.Neo4Net.io.layout.DatabaseFile.PROPERTY_ARRAY_STORE, true, false) { public CommonAbstractStore open(NeoStores neoStores) { return neoStores.createPropertyArrayStore(); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       PROPERTY(org.Neo4Net.io.layout.DatabaseFile.PROPERTY_STORE, true, false) { public CommonAbstractStore open(NeoStores neoStores) { return neoStores.createPropertyStore(); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       RELATIONSHIP(org.Neo4Net.io.layout.DatabaseFile.RELATIONSHIP_STORE, true, false) { public CommonAbstractStore open(NeoStores neoStores) { return neoStores.createRelationshipStore(); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       RELATIONSHIP_TYPE_TOKEN_NAME(org.Neo4Net.io.layout.DatabaseFile.RELATIONSHIP_TYPE_TOKEN_NAMES_STORE, true, true) { public CommonAbstractStore open(NeoStores neoStores) { return neoStores.createRelationshipTypeTokenNamesStore(); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       RELATIONSHIP_TYPE_TOKEN(org.Neo4Net.io.layout.DatabaseFile.RELATIONSHIP_TYPE_TOKEN_STORE, true, true) { public CommonAbstractStore open(NeoStores neoStores) { return neoStores.createRelationshipTypeTokenStore(); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       LABEL_TOKEN_NAME(org.Neo4Net.io.layout.DatabaseFile.LABEL_TOKEN_NAMES_STORE, true, true) { public CommonAbstractStore open(NeoStores neoStores) { return neoStores.createLabelTokenNamesStore(); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       LABEL_TOKEN(org.Neo4Net.io.layout.DatabaseFile.LABEL_TOKEN_STORE, true, true) { public CommonAbstractStore open(NeoStores neoStores) { return neoStores.createLabelTokenStore(); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       SCHEMA(org.Neo4Net.io.layout.DatabaseFile.SCHEMA_STORE, true, true) { public CommonAbstractStore open(NeoStores neoStores) { return neoStores.createSchemaStore(); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       RELATIONSHIP_GROUP(org.Neo4Net.io.layout.DatabaseFile.RELATIONSHIP_GROUP_STORE, true, false) { public CommonAbstractStore open(NeoStores neoStores) { return neoStores.createRelationshipGroupStore(); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       COUNTS(org.Neo4Net.io.layout.DatabaseFile.COUNTS_STORES, false, false) { public org.Neo4Net.kernel.impl.store.counts.CountsTracker open(NeoStores neoStores) { return neoStores.createCountStore(); } void close(Object object) { try { ((org.Neo4Net.kernel.impl.store.counts.CountsTracker) object).shutdown(); } catch(java.io.IOException e) { throw new UnderlyingStorageException(e); } } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       META_DATA(org.Neo4Net.io.layout.DatabaseFile.METADATA_STORE, true, true) { public CommonAbstractStore open(NeoStores neoStores) { return neoStores.createMetadataStore(); } };

		 private static readonly IList<StoreType> valueList = new List<StoreType>();

		 static StoreType()
		 {
			 valueList.Add( NODE_LABEL );
			 valueList.Add( NODE );
			 valueList.Add( PROPERTY_KEY_TOKEN_NAME );
			 valueList.Add( PROPERTY_KEY_TOKEN );
			 valueList.Add( PROPERTY_STRING );
			 valueList.Add( PROPERTY_ARRAY );
			 valueList.Add( PROPERTY );
			 valueList.Add( RELATIONSHIP );
			 valueList.Add( RELATIONSHIP_TYPE_TOKEN_NAME );
			 valueList.Add( RELATIONSHIP_TYPE_TOKEN );
			 valueList.Add( LABEL_TOKEN_NAME );
			 valueList.Add( LABEL_TOKEN );
			 valueList.Add( SCHEMA );
			 valueList.Add( RELATIONSHIP_GROUP );
			 valueList.Add( COUNTS );
			 valueList.Add( META_DATA );
		 }

		 public enum InnerEnum
		 {
			 NODE_LABEL,
			 NODE,
			 PROPERTY_KEY_TOKEN_NAME,
			 PROPERTY_KEY_TOKEN,
			 PROPERTY_STRING,
			 PROPERTY_ARRAY,
			 PROPERTY,
			 RELATIONSHIP,
			 RELATIONSHIP_TYPE_TOKEN_NAME,
			 RELATIONSHIP_TYPE_TOKEN,
			 LABEL_TOKEN_NAME,
			 LABEL_TOKEN,
			 SCHEMA,
			 RELATIONSHIP_GROUP,
			 COUNTS,
			 META_DATA
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private StoreType( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 private readonly bool recordStore;
		 private readonly bool limitedIdStore;
		 private readonly Neo4Net.Io.layout.DatabaseFile databaseFile;

		 internal StoreType( Neo4Net.Io.layout.DatabaseFile databaseFile, bool recordStore, bool limitedIdStore ) { this.databaseFile = databaseFile; this.recordStore = recordStore; this.limitedIdStore = limitedIdStore; } abstract object open( NeoStores neoStores );

		 public bool IsRecordStore()
		 {
			  return _recordStore;
		 }

		 /// <returns> {@code true} to signal that this store has a quite limited id space and is more of a meta data store.
		 /// Originally came about when adding transaction-local id batching, to avoid id generator batching on certain stores. </returns>
		 public bool LimitedIdStore
		 {
			 get
			 {
				  return _limitedIdStore;
			 }
		 }

		 public Neo4Net.Io.layout.DatabaseFile DatabaseFile
		 {
			 get
			 {
				  return _databaseFile;
			 }
		 }

		 internal void Close( object @object )
		 {
			  ( ( CommonAbstractStore ) @object ).close();
		 }

		 /// <summary>
		 /// Determine type of a store base on provided database file.
		 /// </summary>
		 /// <param name="databaseFile"> - database file to map </param>
		 /// <returns> an <seealso cref="Optional"/> that wraps the matching store type of the specified file,
		 /// or <seealso cref="Optional.empty()"/> if the given file name does not match any store files. </returns>
		 public static Optional<StoreType> TypeOf( Neo4Net.Io.layout.DatabaseFile databaseFile )
		 {
			  Objects.requireNonNull( databaseFile );
			  StoreType[] values = StoreType.values();
			  foreach ( StoreType value in values )
			  {
					if ( value.DatabaseFile.Equals( databaseFile ) )
					{
						 return value;
					}
			  }
			  return null;
		 }

		public static IList<StoreType> values()
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

		public static StoreType ValueOf( string name )
		{
			foreach ( StoreType enumInstance in StoreType.valueList )
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