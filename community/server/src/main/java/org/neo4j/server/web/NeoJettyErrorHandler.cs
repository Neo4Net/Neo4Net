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
namespace Org.Neo4j.Server.web
{
	using ErrorHandler = org.eclipse.jetty.server.handler.ErrorHandler;


	public class NeoJettyErrorHandler : ErrorHandler
	{

		 protected internal override void HandleErrorPage( HttpServletRequest request, Writer writer, int code, string message )
		 {
			  WriteErrorPage( request, writer, code, message, false );
		 }

		 protected internal override void WriteErrorPage( HttpServletRequest request, Writer writer, int code, string message, bool showStacks )
		 {

			  // we don't want any Jetty output

		 }

		 protected internal override void WriteErrorPageHead( HttpServletRequest request, Writer writer, int code, string message )
		 {
			  // we don't want any Jetty output

		 }

		 protected internal override void WriteErrorPageBody( HttpServletRequest request, Writer writer, int code, string message, bool showStacks )
		 {
			  // we don't want any Jetty output

		 }

		 protected internal override void WriteErrorPageMessage( HttpServletRequest request, Writer writer, int code, string message, string uri )
		 {
			  // we don't want any Jetty output

		 }

		 protected internal override void WriteErrorPageStacks( HttpServletRequest request, Writer writer )
		 {
			  // we don't want any stack output

		 }
	}

}