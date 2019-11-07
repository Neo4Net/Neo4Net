using System;

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

	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Config = Neo4Net.Kernel.configuration.Config;
	using RecordFormats = Neo4Net.Kernel.impl.store.format.RecordFormats;
	using IdGeneratorFactory = Neo4Net.Kernel.impl.store.id.IdGeneratorFactory;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using LabelTokenRecord = Neo4Net.Kernel.Impl.Store.Records.LabelTokenRecord;
	using LogProvider = Neo4Net.Logging.LogProvider;

	/// <summary>
	/// Implementation of the label store.
	/// </summary>
	public class LabelTokenStore : TokenStore<LabelTokenRecord>
	{
		 public const string TYPE_DESCRIPTOR = "LabelTokenStore";

		 public LabelTokenStore( File file, File idFile, Config config, IdGeneratorFactory idGeneratorFactory, PageCache pageCache, LogProvider logProvider, DynamicStringStore nameStore, RecordFormats recordFormats, params OpenOption[] openOptions ) : base( file, idFile, config, IdType.LABEL_TOKEN, idGeneratorFactory, pageCache, logProvider, nameStore, TYPE_DESCRIPTOR, recordFormats.LabelToken(), recordFormats.StoreVersion(), openOptions )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <FAILURE extends Exception> void accept(Processor<FAILURE> processor, Neo4Net.kernel.impl.store.record.LabelTokenRecord record) throws FAILURE
		 public override void Accept<FAILURE>( Processor<FAILURE> processor, LabelTokenRecord record ) where FAILURE : Exception
		 {
			  processor.processLabelToken( this, record );
		 }
	}

}