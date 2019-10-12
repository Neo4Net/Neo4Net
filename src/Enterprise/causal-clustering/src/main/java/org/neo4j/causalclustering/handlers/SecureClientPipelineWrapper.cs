using System.Collections.Generic;

/*
 * Copyright (c) 2019 "GraphFoundation" [https://graphfoundation.org]
 *
 * The included source code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html)
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 */

namespace Neo4Net.causalclustering.handlers
{
	using Channel = io.netty.channel.Channel;
	using ChannelHandler = io.netty.channel.ChannelHandler;


	using SslPolicy = Neo4Net.Ssl.SslPolicy;

	public class SecureClientPipelineWrapper : PipelineWrapper
	{
		 private readonly SslPolicy _sslPolicy;

		 /// <param name="sslPolicy"> </param>
		 internal SecureClientPipelineWrapper( SslPolicy sslPolicy )
		 {
			  this._sslPolicy = sslPolicy;
		 }

		 /// <param name="channel">
		 /// @return </param>
		 /// <exception cref="Exception"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.List<io.netty.channel.ChannelHandler> handlersFor(io.netty.channel.Channel channel) throws Exception
		 public virtual IList<ChannelHandler> HandlersFor( Channel channel )
		 {
			  return this._sslPolicy != null ? Collections.singletonList( this._sslPolicy.nettyClientHandler( channel ) ) : Collections.emptyList();
		 }

		 /// <summary>
		 /// @return
		 /// </summary>
		 public virtual string Name()
		 {
			  return "tls_client";
		 }
	}

}