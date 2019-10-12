using System;
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

	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using RecordFormats = Org.Neo4j.Kernel.impl.store.format.RecordFormats;
	using IdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.IdGeneratorFactory;
	using IdType = Org.Neo4j.Kernel.impl.store.id.IdType;
	using DynamicRecord = Org.Neo4j.Kernel.impl.store.record.DynamicRecord;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using RecordLoad = Org.Neo4j.Kernel.impl.store.record.RecordLoad;
	using Bits = Org.Neo4j.Kernel.impl.util.Bits;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.NoStoreHeaderFormat.NO_STORE_HEADER_FORMAT;

	/// <summary>
	/// Implementation of the node store.
	/// </summary>
	public class NodeStore : CommonAbstractStore<NodeRecord, NoStoreHeader>
	{
		 public const string TYPE_DESCRIPTOR = "NodeStore";
		 private readonly DynamicArrayStore _dynamicLabelStore;

		 public static long? ReadOwnerFromDynamicLabelsRecord( DynamicRecord record )
		 {
			  sbyte[] data = record.Data;
			  sbyte[] header = PropertyType.Array.readDynamicRecordHeader( data );
			  sbyte[] array = Arrays.copyOfRange( data, header.Length, data.Length );

			  int requiredBits = header[2];
			  if ( requiredBits == 0 )
			  {
					return null;
			  }
			  Bits bits = Bits.bitsFromBytes( array );
			  return bits.GetLong( requiredBits );
		 }

		 public NodeStore( File file, File idFile, Config config, IdGeneratorFactory idGeneratorFactory, PageCache pageCache, LogProvider logProvider, DynamicArrayStore dynamicLabelStore, RecordFormats recordFormats, params OpenOption[] openOptions ) : base( file, idFile, config, IdType.NODE, idGeneratorFactory, pageCache, logProvider, TYPE_DESCRIPTOR, recordFormats.Node(), NO_STORE_HEADER_FORMAT, recordFormats.StoreVersion(), openOptions )
		 {
			  this._dynamicLabelStore = dynamicLabelStore;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <FAILURE extends Exception> void accept(RecordStore_Processor<FAILURE> processor, org.neo4j.kernel.impl.store.record.NodeRecord record) throws FAILURE
		 public override void Accept<FAILURE>( RecordStore_Processor<FAILURE> processor, NodeRecord record ) where FAILURE : Exception
		 {
			  processor.ProcessNode( this, record );
		 }

		 public override void EnsureHeavy( NodeRecord node )
		 {
			  if ( NodeLabelsField.FieldPointsToDynamicRecordOfLabels( node.LabelField ) )
			  {
					EnsureHeavy( node, NodeLabelsField.FirstDynamicLabelRecordId( node.LabelField ) );
			  }
		 }

		 public virtual void EnsureHeavy( NodeRecord node, long firstDynamicLabelRecord )
		 {
			  if ( !node.Light )
			  {
					return;
			  }

			  // Load any dynamic labels and populate the node record
			  node.SetLabelField( node.LabelField, _dynamicLabelStore.getRecords( firstDynamicLabelRecord, RecordLoad.NORMAL ) );
		 }

		 public override void UpdateRecord( NodeRecord record )
		 {
			  base.UpdateRecord( record );
			  UpdateDynamicLabelRecords( record.DynamicLabelRecords );
		 }

		 public virtual DynamicArrayStore DynamicLabelStore
		 {
			 get
			 {
				  return _dynamicLabelStore;
			 }
		 }

		 public virtual void UpdateDynamicLabelRecords( IEnumerable<DynamicRecord> dynamicLabelRecords )
		 {
			  foreach ( DynamicRecord record in dynamicLabelRecords )
			  {
					_dynamicLabelStore.updateRecord( record );
			  }
		 }
	}

}