/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.catchup.tx
{

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Config = Neo4Net.Kernel.configuration.Config;
	using LogProvider = Neo4Net.Logging.LogProvider;

	public class TransactionLogCatchUpFactory
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public TransactionLogCatchUpWriter create(org.Neo4Net.io.layout.DatabaseLayout databaseLayout, org.Neo4Net.io.fs.FileSystemAbstraction fs, org.Neo4Net.io.pagecache.PageCache pageCache, org.Neo4Net.kernel.configuration.Config config, org.Neo4Net.logging.LogProvider logProvider, long fromTxId, boolean asPartOfStoreCopy, boolean keepTxLogsInStoreDir, boolean rotateTransactionsManually) throws java.io.IOException
		 public virtual TransactionLogCatchUpWriter Create( DatabaseLayout databaseLayout, FileSystemAbstraction fs, PageCache pageCache, Config config, LogProvider logProvider, long fromTxId, bool asPartOfStoreCopy, bool keepTxLogsInStoreDir, bool rotateTransactionsManually )
		 {
			  return new TransactionLogCatchUpWriter( databaseLayout, fs, pageCache, config, logProvider, fromTxId, asPartOfStoreCopy, keepTxLogsInStoreDir, rotateTransactionsManually );
		 }
	}

}