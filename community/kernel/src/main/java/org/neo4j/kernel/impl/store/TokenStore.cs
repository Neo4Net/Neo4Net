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
namespace Org.Neo4j.Kernel.impl.store
{

	using NamedToken = Org.Neo4j.@internal.Kernel.Api.NamedToken;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Org.Neo4j.Kernel.impl.store.format;
	using IdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.IdGeneratorFactory;
	using IdType = Org.Neo4j.Kernel.impl.store.id.IdType;
	using DynamicRecord = Org.Neo4j.Kernel.impl.store.record.DynamicRecord;
	using Record = Org.Neo4j.Kernel.impl.store.record.Record;
	using RecordLoad = Org.Neo4j.Kernel.impl.store.record.RecordLoad;
	using TokenRecord = Org.Neo4j.Kernel.impl.store.record.TokenRecord;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.NoStoreHeaderFormat.NO_STORE_HEADER_FORMAT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.PropertyStore.decodeString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.RecordLoad.NORMAL;

	public abstract class TokenStore<RECORD> : CommonAbstractStore<RECORD, NoStoreHeader> where RECORD : Org.Neo4j.Kernel.impl.store.record.TokenRecord
	{
		 public const int NAME_STORE_BLOCK_SIZE = 30;

		 private readonly DynamicStringStore _nameStore;

		 public TokenStore( File file, File idFile, Config configuration, IdType idType, IdGeneratorFactory idGeneratorFactory, PageCache pageCache, LogProvider logProvider, DynamicStringStore nameStore, string typeDescriptor, RecordFormat<RECORD> recordFormat, string storeVersion, params OpenOption[] openOptions ) : base( file, idFile, configuration, idType, idGeneratorFactory, pageCache, logProvider, typeDescriptor, recordFormat, NO_STORE_HEADER_FORMAT, storeVersion, openOptions )
		 {
			  this._nameStore = nameStore;
		 }

		 public virtual DynamicStringStore NameStore
		 {
			 get
			 {
				  return _nameStore;
			 }
		 }

		 protected internal override bool IsOnlyFastIdGeneratorRebuildEnabled( Config config )
		 {
			  return false;
		 }

		 public virtual IList<NamedToken> Tokens
		 {
			 get
			 {
				  LinkedList<NamedToken> records = new LinkedList<NamedToken>();
				  long maxIdInUse = HighestPossibleIdInUse;
				  int found = 0;
				  RECORD record = newRecord();
				  for ( int i = 0; i <= maxIdInUse; i++ )
				  {
						if ( !getRecord( i, record, RecordLoad.CHECK ).inUse() )
						{
							 continue;
						}
   
						found++;
						if ( record != null && record.inUse() && record.NameId != Record.RESERVED.intValue() )
						{
							 records.AddLast( new NamedToken( GetStringFor( record ), i ) );
						}
				  }
   
				  return records;
			 }
		 }

		 public virtual NamedToken GetToken( int id )
		 {
			  RECORD record = getRecord( id, newRecord(), NORMAL );
			  return new NamedToken( GetStringFor( record ), record.IntId );
		 }

		 public virtual ICollection<DynamicRecord> AllocateNameRecords( sbyte[] chars )
		 {
			  ICollection<DynamicRecord> records = new List<DynamicRecord>();
			  _nameStore.allocateRecordsFromBytes( records, chars );
			  return records;
		 }

		 public override void UpdateRecord( RECORD record )
		 {
			  base.UpdateRecord( record );
			  if ( !record.Light )
			  {
					foreach ( DynamicRecord keyRecord in record.NameRecords )
					{
						 _nameStore.updateRecord( keyRecord );
					}
			  }
		 }

		 public override void EnsureHeavy( RECORD record )
		 {
			  if ( !record.Light )
			  {
					return;
			  }

			  record.addNameRecords( _nameStore.getRecords( record.NameId, NORMAL ) );
		 }

		 public virtual string GetStringFor( RECORD nameRecord )
		 {
			  EnsureHeavy( nameRecord );
			  int recordToFind = nameRecord.NameId;
			  IEnumerator<DynamicRecord> records = nameRecord.NameRecords.GetEnumerator();
			  ICollection<DynamicRecord> relevantRecords = new List<DynamicRecord>();
			  while ( recordToFind != Record.NO_NEXT_BLOCK.intValue() && records.MoveNext() )
			  {
					DynamicRecord record = records.Current;
					if ( record.InUse() && record.Id == recordToFind )
					{
						 recordToFind = ( int ) record.NextBlock;
						 // TODO: optimize here, high chance next is right one
						 relevantRecords.Add( record );
						 records = nameRecord.NameRecords.GetEnumerator();
					}
			  }
			  return decodeString( _nameStore.readFullByteArray( relevantRecords, PropertyType.String ).other() );
		 }
	}

}