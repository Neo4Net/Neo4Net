﻿/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.backup.impl
{
	using Test = org.junit.jupiter.api.Test;

	using EphemeralFileSystemAbstraction = Org.Neo4j.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.NullOutputStream.NULL_OUTPUT_STREAM;

	internal class BackupProtocolServiceTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void closePageCacheContainerOnClose() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ClosePageCacheContainerOnClose()
		 {
			  PageCache pageCache = mock( typeof( PageCache ) );
			  BackupPageCacheContainer container = BackupPageCacheContainer.Of( pageCache );
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  BackupProtocolService protocolService = new BackupProtocolService( EphemeralFileSystemAbstraction::new, NullLogProvider.Instance, NULL_OUTPUT_STREAM, new Monitors(), container );
			  protocolService.Close();

			  verify( pageCache ).close();
		 }
	}

}