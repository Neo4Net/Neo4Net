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
	using HttpChannel = org.eclipse.jetty.server.HttpChannel;
	using Request = org.eclipse.jetty.server.Request;
	using RequestLog = org.eclipse.jetty.server.RequestLog;
	using Response = org.eclipse.jetty.server.Response;
	using RequestLogHandler = org.eclipse.jetty.server.handler.RequestLogHandler;


	using InternalJettyServletRequest = Org.Neo4j.Server.rest.web.InternalJettyServletRequest;

	/// <summary>
	/// This is the log handler used for http logging.
	/// This class overrides the original <seealso cref="RequestLogHandler"/>
	/// and rewrite the <seealso cref="RequestLogHandler.handle(string, Request, HttpServletRequest, HttpServletResponse)"/>
	/// to be able to accept <seealso cref="Request"/> who does not have a http channel attached with it, such as <seealso cref="InternalJettyServletRequest"/>.
	/// </summary>
	public class HttpChannelOptionalRequestLogHandler : RequestLogHandler
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void handle(String target, org.eclipse.jetty.server.Request baseRequest, javax.servlet.http.HttpServletRequest request, javax.servlet.http.HttpServletResponse response) throws java.io.IOException, javax.servlet.ServletException
		 public override void Handle( string target, Request baseRequest, HttpServletRequest request, HttpServletResponse response )
		 {
			  HttpChannel httpChannel = baseRequest.HttpChannel;
			  if ( httpChannel != null ) // if the channel is not null, all good, you handle yourself.
			  {
					base.Handle( target, baseRequest, request, response );
			  }
			  else // if we do not have a real channel, then we just log ourselves
			  {
					try
					{
						 if ( _handler != null )
						 {
							  _handler.handle( target, baseRequest, request, response );
						 }
					}
					finally
					{
						 RequestLog requestLog = RequestLog;
						 if ( requestLog != null && baseRequest.DispatcherType == DispatcherType.REQUEST )
						 {
							  requestLog.log( baseRequest, ( Response ) response );
						 }
					}
			  }
		 }
	}

}