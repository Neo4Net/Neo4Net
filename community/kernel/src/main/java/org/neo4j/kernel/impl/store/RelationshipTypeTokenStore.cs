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
namespace Org.Neo4j.Kernel.impl.store
{

	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using PageCursor = Org.Neo4j.Io.pagecache.PageCursor;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using RecordFormats = Org.Neo4j.Kernel.impl.store.format.RecordFormats;
	using IdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.IdGeneratorFactory;
	using IdType = Org.Neo4j.Kernel.impl.store.id.IdType;
	using Record = Org.Neo4j.Kernel.impl.store.record.Record;
	using RelationshipTypeTokenRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipTypeTokenRecord;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

	/// <summary>
	/// Implementation of the relationship type store. Uses a dynamic store to store
	/// relationship type names.
	/// </summary>
	public class RelationshipTypeTokenStore : TokenStore<RelationshipTypeTokenRecord>
	{
		 public const string TYPE_DESCRIPTOR = "RelationshipTypeStore";

		 public RelationshipTypeTokenStore( File file, File idFile, Config config, IdGeneratorFactory idGeneratorFactory, PageCache pageCache, LogProvider logProvider, DynamicStringStore nameStore, RecordFormats recordFormats, params OpenOption[] openOptions ) : base( file, idFile, config, IdType.RELATIONSHIP_TYPE_TOKEN, idGeneratorFactory, pageCache, logProvider, nameStore, TYPE_DESCRIPTOR, recordFormats.RelationshipTypeToken(), recordFormats.StoreVersion(), openOptions )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <FAILURE extends Exception> void accept(Processor<FAILURE> processor, org.neo4j.kernel.impl.store.record.RelationshipTypeTokenRecord record) throws FAILURE
		 public override void Accept<FAILURE>( Processor<FAILURE> processor, RelationshipTypeTokenRecord record ) where FAILURE : Exception
		 {
			  processor.processRelationshipTypeToken( this, record );
		 }

		 protected internal override bool IsRecordReserved( PageCursor cursor )
		 {
			  return cursor.Int == Record.RESERVED.intValue();
		 }
	}

}