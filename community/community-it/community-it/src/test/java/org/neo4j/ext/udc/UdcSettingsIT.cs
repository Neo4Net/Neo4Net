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
namespace Org.Neo4j.Ext.Udc
{
	using Test = org.junit.Test;

	using Config = Org.Neo4j.Kernel.configuration.Config;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class UdcSettingsIT
	{
		 private const string TEST_HOST_AND_PORT = "test.ucd.neo4j.org:8080";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testUdcHostSettingIsUnchanged()
		 public virtual void TestUdcHostSettingIsUnchanged()
		 {
			  //noinspection deprecation
			  GraphDatabaseAPI db = ( GraphDatabaseAPI ) ( new TestGraphDatabaseFactory() ).newImpermanentDatabaseBuilder().setConfig(UdcSettings.UdcHost, TEST_HOST_AND_PORT).newGraphDatabase();
			  Config config = Db.DependencyResolver.resolveDependency( typeof( Config ) );

			  assertEquals( TEST_HOST_AND_PORT, config.Get( UdcSettings.UdcHost ).ToString() );

			  Db.shutdown();
		 }
	}

}