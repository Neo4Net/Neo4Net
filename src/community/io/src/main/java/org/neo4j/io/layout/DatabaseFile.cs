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
namespace Neo4Net.Io.layout
{

	using Preconditions = Neo4Net.Utils.Preconditions;

	/// <summary>
	/// Enumeration of storage implementation specific files for particular database.
	/// Any internal details of this enumeration is hidden and should not be visible to anyone except of implementation of specific database layout.
	/// Should be used only for referencing back specific files in the database layout based on different store types.
	/// 
	/// Any database file that represented here can have internal details like several actual file names, other internal characteristic that are store specific.
	/// </summary>
	/// <seealso cref= DatabaseLayout </seealso>
	/// <seealso cref= StoreLayout </seealso>
	public sealed class DatabaseFile
	{
		 public static readonly DatabaseFile NodeStore = new DatabaseFile( "NodeStore", InnerEnum.NodeStore, DatabaseFileNames.NODE_STORE );

		 public static readonly DatabaseFile NodeLabelStore = new DatabaseFile( "NodeLabelStore", InnerEnum.NodeLabelStore, DatabaseFileNames.NODE_LABELS_STORE );

		 public static readonly DatabaseFile PropertyStore = new DatabaseFile( "PropertyStore", InnerEnum.PropertyStore, DatabaseFileNames.PROPERTY_STORE );

		 public static readonly DatabaseFile PropertyArrayStore = new DatabaseFile( "PropertyArrayStore", InnerEnum.PropertyArrayStore, DatabaseFileNames.PROPERTY_ARRAY_STORE );

		 public static readonly DatabaseFile PropertyStringStore = new DatabaseFile( "PropertyStringStore", InnerEnum.PropertyStringStore, DatabaseFileNames.PROPERTY_STRING_STORE );

		 public static readonly DatabaseFile PropertyKeyTokenStore = new DatabaseFile( "PropertyKeyTokenStore", InnerEnum.PropertyKeyTokenStore, DatabaseFileNames.PROPERTY_KEY_TOKEN_STORE );

		 public static readonly DatabaseFile PropertyKeyTokenNamesStore = new DatabaseFile( "PropertyKeyTokenNamesStore", InnerEnum.PropertyKeyTokenNamesStore, DatabaseFileNames.PROPERTY_KEY_TOKEN_NAMES_STORE );

		 public static readonly DatabaseFile RelationshipStore = new DatabaseFile( "RelationshipStore", InnerEnum.RelationshipStore, DatabaseFileNames.RELATIONSHIP_STORE );

		 public static readonly DatabaseFile RelationshipGroupStore = new DatabaseFile( "RelationshipGroupStore", InnerEnum.RelationshipGroupStore, DatabaseFileNames.RELATIONSHIP_GROUP_STORE );

		 public static readonly DatabaseFile RelationshipTypeTokenStore = new DatabaseFile( "RelationshipTypeTokenStore", InnerEnum.RelationshipTypeTokenStore, DatabaseFileNames.RELATIONSHIP_TYPE_TOKEN_STORE );

		 public static readonly DatabaseFile RelationshipTypeTokenNamesStore = new DatabaseFile( "RelationshipTypeTokenNamesStore", InnerEnum.RelationshipTypeTokenNamesStore, DatabaseFileNames.RELATIONSHIP_TYPE_TOKEN_NAMES_STORE );

		 public static readonly DatabaseFile LabelTokenStore = new DatabaseFile( "LabelTokenStore", InnerEnum.LabelTokenStore, DatabaseFileNames.LABEL_TOKEN_STORE );

		 public static readonly DatabaseFile LabelTokenNamesStore = new DatabaseFile( "LabelTokenNamesStore", InnerEnum.LabelTokenNamesStore, DatabaseFileNames.LABEL_TOKEN_NAMES_STORE );

		 public static readonly DatabaseFile SchemaStore = new DatabaseFile( "SchemaStore", InnerEnum.SchemaStore, DatabaseFileNames.SCHEMA_STORE );

		 public static readonly DatabaseFile CountsStores = new DatabaseFile( "CountsStores", InnerEnum.CountsStores, false, DatabaseFileNames.COUNTS_STORE_A, DatabaseFileNames.COUNTS_STORE_B );
		 public static readonly DatabaseFile CountsStoreA = new DatabaseFile( "CountsStoreA", InnerEnum.CountsStoreA, false, DatabaseFileNames.COUNTS_STORE_A );
		 public static readonly DatabaseFile CountsStoreB = new DatabaseFile( "CountsStoreB", InnerEnum.CountsStoreB, false, DatabaseFileNames.COUNTS_STORE_B );

		 public static readonly DatabaseFile MetadataStore = new DatabaseFile( "MetadataStore", InnerEnum.MetadataStore, DatabaseFileNames.METADATA_STORE );

		 public static readonly DatabaseFile LabelScanStore = new DatabaseFile( "LabelScanStore", InnerEnum.LabelScanStore, false, DatabaseFileNames.LABEL_SCAN_STORE );

		 private static readonly IList<DatabaseFile> valueList = new List<DatabaseFile>();

		 static DatabaseFile()
		 {
			 valueList.Add( NodeStore );
			 valueList.Add( NodeLabelStore );
			 valueList.Add( PropertyStore );
			 valueList.Add( PropertyArrayStore );
			 valueList.Add( PropertyStringStore );
			 valueList.Add( PropertyKeyTokenStore );
			 valueList.Add( PropertyKeyTokenNamesStore );
			 valueList.Add( RelationshipStore );
			 valueList.Add( RelationshipGroupStore );
			 valueList.Add( RelationshipTypeTokenStore );
			 valueList.Add( RelationshipTypeTokenNamesStore );
			 valueList.Add( LabelTokenStore );
			 valueList.Add( LabelTokenNamesStore );
			 valueList.Add( SchemaStore );
			 valueList.Add( CountsStores );
			 valueList.Add( CountsStoreA );
			 valueList.Add( CountsStoreB );
			 valueList.Add( MetadataStore );
			 valueList.Add( LabelScanStore );
		 }

		 public enum InnerEnum
		 {
			 NodeStore,
			 NodeLabelStore,
			 PropertyStore,
			 PropertyArrayStore,
			 PropertyStringStore,
			 PropertyKeyTokenStore,
			 PropertyKeyTokenNamesStore,
			 RelationshipStore,
			 RelationshipGroupStore,
			 RelationshipTypeTokenStore,
			 RelationshipTypeTokenNamesStore,
			 LabelTokenStore,
			 LabelTokenNamesStore,
			 SchemaStore,
			 CountsStores,
			 CountsStoreA,
			 CountsStoreB,
			 MetadataStore,
			 LabelScanStore
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 internal Private readonly;
		 internal Private readonly;

		 internal DatabaseFile( string name, InnerEnum innerEnum, string name ) : this( true, name )
		 {

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 internal DatabaseFile( string name, InnerEnum innerEnum, bool hasIdFile, params string[] names )
		 {
			  this._names = Arrays.asList( names );
			  this._hasIdFile = hasIdFile;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 internal IEnumerable<string> Names
		 {
			 get
			 {
				  return _names;
			 }
		 }

		 public string Name
		 {
			 get
			 {
				  Preconditions.checkState( _names.size() == 1, "Database file has more then one file names." );
				  return _names.get( 0 );
			 }
		 }

		 internal bool HasIdFile()
		 {
			  return _hasIdFile;
		 }

		 /// <summary>
		 /// Determine database file for provided file name.
		 /// </summary>
		 /// <param name="name"> - database file name to map </param>
		 /// <returns> an <seealso cref="Optional"/> that wraps the matching database file that matches to the specified name,
		 /// or <seealso cref="Optional.empty()"/> if the given file name does not match to any of database files. </returns>
		 public static Optional<DatabaseFile> FileOf( string name )
		 {
			  requireNonNull( name );
			  DatabaseFile[] databaseFiles = DatabaseFile.values();
			  foreach ( DatabaseFile databaseFile in databaseFiles )
			  {
					if ( databaseFile._names.contains( name ) )
					{
						 return databaseFile;
					}
			  }
			  return null;
		 }

		public static IList<DatabaseFile> values()
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

		public static DatabaseFile ValueOf( string name )
		{
			foreach ( DatabaseFile enumInstance in DatabaseFile.valueList )
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