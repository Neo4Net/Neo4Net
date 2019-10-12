using System;

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
namespace Neo4Net.Kernel.impl.store
{

	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Config = Neo4Net.Kernel.configuration.Config;
	using RecordFormats = Neo4Net.Kernel.impl.store.format.RecordFormats;
	using IdGeneratorFactory = Neo4Net.Kernel.impl.store.id.IdGeneratorFactory;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using RelationshipRecord = Neo4Net.Kernel.impl.store.record.RelationshipRecord;
	using LogProvider = Neo4Net.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.NoStoreHeaderFormat.NO_STORE_HEADER_FORMAT;

	/// <summary>
	/// Implementation of the relationship store.
	/// </summary>
	public class RelationshipStore : CommonAbstractStore<RelationshipRecord, NoStoreHeader>
	{
		 public const string TYPE_DESCRIPTOR = "RelationshipStore";

		 public RelationshipStore( File file, File idFile, Config configuration, IdGeneratorFactory idGeneratorFactory, PageCache pageCache, LogProvider logProvider, RecordFormats recordFormats, params OpenOption[] openOptions ) : base( file, idFile, configuration, IdType.RELATIONSHIP, idGeneratorFactory, pageCache, logProvider, TYPE_DESCRIPTOR, recordFormats.Relationship(), NO_STORE_HEADER_FORMAT, recordFormats.StoreVersion(), openOptions )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <FAILURE extends Exception> void accept(RecordStore_Processor<FAILURE> processor, org.neo4j.kernel.impl.store.record.RelationshipRecord record) throws FAILURE
		 public override void Accept<FAILURE>( RecordStore_Processor<FAILURE> processor, RelationshipRecord record ) where FAILURE : Exception
		 {
			  processor.ProcessRelationship( this, record );
		 }
	}

}