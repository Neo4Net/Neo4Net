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
	using Neo4Net.Kernel.impl.store.format;
	using IdGeneratorFactory = Neo4Net.Kernel.impl.store.id.IdGeneratorFactory;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using DynamicRecord = Neo4Net.Kernel.Impl.Store.Records.DynamicRecord;
	using LogProvider = Neo4Net.Logging.LogProvider;

	/// <summary>
	/// Dynamic store that stores strings.
	/// </summary>
	public class DynamicStringStore : AbstractDynamicStore
	{
		 // store version, each store ends with this string (byte encoded)
		 public const string TYPE_DESCRIPTOR = "StringPropertyStore";

		 public DynamicStringStore( File file, File idFile, Config configuration, IdType idType, IdGeneratorFactory idGeneratorFactory, PageCache pageCache, LogProvider logProvider, int dataSizeFromConfiguration, RecordFormat<DynamicRecord> recordFormat, string storeVersion, params OpenOption[] openOptions ) : base( file, idFile, configuration, idType, idGeneratorFactory, pageCache, logProvider, TYPE_DESCRIPTOR, dataSizeFromConfiguration, recordFormat, storeVersion, openOptions )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <FAILURE extends Exception> void accept(RecordStore_Processor<FAILURE> processor, org.Neo4Net.kernel.impl.store.record.DynamicRecord record) throws FAILURE
		 public override void Accept<FAILURE>( RecordStore_Processor<FAILURE> processor, DynamicRecord record ) where FAILURE : Exception
		 {
			  processor.ProcessString( this, record, IdTypeConflict );
		 }
	}

}