using System;
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

	using MalformedSchemaRuleException = Neo4Net.@internal.Kernel.Api.exceptions.schema.MalformedSchemaRuleException;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ReusableRecordsCompositeAllocator = Neo4Net.Kernel.impl.store.allocator.ReusableRecordsCompositeAllocator;
	using RecordFormats = Neo4Net.Kernel.impl.store.format.RecordFormats;
	using IdGeneratorFactory = Neo4Net.Kernel.impl.store.id.IdGeneratorFactory;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using DynamicRecord = Neo4Net.Kernel.impl.store.record.DynamicRecord;
	using SchemaRuleSerialization = Neo4Net.Kernel.impl.store.record.SchemaRuleSerialization;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using SchemaRule = Neo4Net.Storageengine.Api.schema.SchemaRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.RecordLoad.CHECK;

	public class SchemaStore : AbstractDynamicStore, IEnumerable<SchemaRule>
	{
		 // store version, each store ends with this string (byte encoded)
		 public const string TYPE_DESCRIPTOR = "SchemaStore";
		 public const int BLOCK_SIZE = 56;

		 public SchemaStore( File file, File idFile, Config conf, IdType idType, IdGeneratorFactory idGeneratorFactory, PageCache pageCache, LogProvider logProvider, RecordFormats recordFormats, params OpenOption[] openOptions ) : base( file, idFile, conf, idType, idGeneratorFactory, pageCache, logProvider, TYPE_DESCRIPTOR, BLOCK_SIZE, recordFormats.Dynamic(), recordFormats.StoreVersion(), openOptions )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <FAILURE extends Exception> void accept(RecordStore_Processor<FAILURE> processor, org.neo4j.kernel.impl.store.record.DynamicRecord record) throws FAILURE
		 public override void Accept<FAILURE>( RecordStore_Processor<FAILURE> processor, DynamicRecord record ) where FAILURE : Exception
		 {
			  processor.ProcessSchema( this, record );
		 }

		 public virtual IList<DynamicRecord> AllocateFrom( SchemaRule rule )
		 {
			  IList<DynamicRecord> records = new List<DynamicRecord>();
			  DynamicRecord record = GetRecord( rule.Id, NextRecord(), CHECK );
			  DynamicRecordAllocator recordAllocator = new ReusableRecordsCompositeAllocator( singleton( record ), this );
			  AllocateRecordsFromBytes( records, SchemaRuleSerialization.serialize( rule ), recordAllocator );
			  return records;
		 }

		 public virtual IEnumerator<SchemaRule> LoadAllSchemaRules()
		 {
			  return ( new SchemaStorage( this ) ).LoadAllSchemaRules();
		 }

		 public override IEnumerator<SchemaRule> Iterator()
		 {
			  return LoadAllSchemaRules();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static org.neo4j.storageengine.api.schema.SchemaRule readSchemaRule(long id, java.util.Collection<org.neo4j.kernel.impl.store.record.DynamicRecord> records, byte[] buffer) throws org.neo4j.internal.kernel.api.exceptions.schema.MalformedSchemaRuleException
		 internal static SchemaRule ReadSchemaRule( long id, ICollection<DynamicRecord> records, sbyte[] buffer )
		 {
			  ByteBuffer scratchBuffer = ConcatData( records, buffer );
			  return SchemaRuleSerialization.deserialize( id, scratchBuffer );
		 }
	}

}