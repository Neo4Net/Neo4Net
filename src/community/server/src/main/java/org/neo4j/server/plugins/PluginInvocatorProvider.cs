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
namespace Neo4Net.Server.plugins
{

	using Neo4Net.Server.database;

	using HttpContext = com.sun.jersey.api.core.HttpContext;

	/// @deprecated Server plugins are deprecated for removal in the next major release. Please use unmanaged extensions instead. 
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Deprecated("Server plugins are deprecated for removal in the next major release. Please use unmanaged extensions instead.") @Provider public class PluginInvocatorProvider extends Neo4Net.server.database.InjectableProvider<PluginInvocator>
	[Obsolete("Server plugins are deprecated for removal in the next major release. Please use unmanaged extensions instead.")]
	public class PluginInvocatorProvider : InjectableProvider<PluginInvocator>
	{
		 private readonly AbstractNeoServer _neoServer;

		 [Obsolete]
		 public PluginInvocatorProvider( AbstractNeoServer neoServer ) : base( typeof( PluginInvocator ) )
		 {
			  this._neoServer = neoServer;
		 }

		 [Obsolete]
		 public override PluginInvocator GetValue( HttpContext c )
		 {
			  return _neoServer.ExtensionManager;
		 }
	}

}