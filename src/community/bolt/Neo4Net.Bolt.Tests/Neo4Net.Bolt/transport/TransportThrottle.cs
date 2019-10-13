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
namespace Neo4Net.Bolt.transport
{
	using Channel = io.netty.channel.Channel;

	public interface TransportThrottle
	{

		 /// <summary>
		 /// Installs the throttle to the given channel.
		 /// </summary>
		 /// <param name="channel"> the netty channel to which this throttle should be installed </param>
		 void Install( Channel channel );

		 /// <summary>
		 /// Apply throttling logic for the given channel..
		 /// </summary>
		 /// <param name="channel"> the netty channel to which this throttling logic should be applied </param>
		 /// <exception cref="TransportThrottleException"> when throttle decides this connection should be halted </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void acquire(io.netty.channel.Channel channel) throws TransportThrottleException;
		 void Acquire( Channel channel );

		 /// <summary>
		 /// Release throttling for the given channel (if applied)..
		 /// </summary>
		 /// <param name="channel"> the netty channel from which this throttling logic should be released </param>
		 void Release( Channel channel );

		 /// <summary>
		 /// Uninstalls the throttle from the given channel.
		 /// </summary>
		 /// <param name="channel"> the netty channel from which this throttle should be uninstalled </param>
		 void Uninstall( Channel channel );
	}

}