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
namespace Neo4Net.Server.web
{

	using HttpContext = com.sun.jersey.api.core.HttpContext;
	using Neo4Net.Server.database;
	using Neo4Net.Server.plugins;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Provider public class InjectableWrapper extends Neo4Net.server.database.InjectableProvider<Object>
	public class InjectableWrapper : InjectableProvider<object>
	{
		 private readonly Injectable _injectable;

		 public InjectableWrapper( Injectable injectable ) : base( injectable.Type )
		 {
			  this._injectable = injectable;
		 }

		 public override object GetValue( HttpContext c )
		 {
			  return _injectable.Value;
		 }
	}

}