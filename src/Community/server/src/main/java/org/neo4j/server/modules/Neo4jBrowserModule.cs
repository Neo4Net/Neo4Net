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
namespace Neo4Net.Server.modules
{
	using WebServer = Neo4Net.Server.web.WebServer;

	public class Neo4jBrowserModule : ServerModule
	{
		 private const string DEFAULT_NEO4_J_BROWSER_PATH = "/browser";
		 private const string DEFAULT_NEO4_J_BROWSER_STATIC_WEB_CONTENT_LOCATION = "browser";

		 private readonly WebServer _webServer;

		 public Neo4jBrowserModule( WebServer webServer )
		 {
			  this._webServer = webServer;
		 }

		 public override void Start()
		 {
			  _webServer.addStaticContent( DEFAULT_NEO4_J_BROWSER_STATIC_WEB_CONTENT_LOCATION, DEFAULT_NEO4_J_BROWSER_PATH );
		 }

		 public override void Stop()
		 {
			  _webServer.removeStaticContent( DEFAULT_NEO4_J_BROWSER_STATIC_WEB_CONTENT_LOCATION, DEFAULT_NEO4_J_BROWSER_PATH );
		 }

	}

}