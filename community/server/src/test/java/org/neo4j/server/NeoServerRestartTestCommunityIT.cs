﻿/*
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
namespace Org.Neo4j.Server
{

	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using CommunityServerBuilder = Org.Neo4j.Server.helpers.CommunityServerBuilder;

	public class NeoServerRestartTestCommunityIT : NeoServerRestartTestIT
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected NeoServer getNeoServer(String customPageSwapperName) throws java.io.IOException
		 protected internal override NeoServer GetNeoServer( string customPageSwapperName )
		 {
			  CommunityServerBuilder builder = CommunityServerBuilder.serverOnRandomPorts().withProperty(GraphDatabaseSettings.pagecache_swapper.name(), customPageSwapperName);
			  return builder.Build();
		 }
	}

}