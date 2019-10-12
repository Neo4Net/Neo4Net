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
namespace Neo4Net.Server
{

	using HttpContext = com.sun.jersey.api.core.HttpContext;

	using Neo4Net.Server.database;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Provider public class NeoServerProvider extends org.neo4j.server.database.InjectableProvider<NeoServer>
	public class NeoServerProvider : InjectableProvider<NeoServer>
	{
		 public NeoServer NeoServer;

		 public NeoServerProvider( NeoServer db ) : base( typeof( NeoServer ) )
		 {
			  this.NeoServer = db;
		 }

		 public override NeoServer GetValue( HttpContext httpContext )
		 {
			  return NeoServer;
		 }
	}

}