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
namespace Neo4Net.Bolt.transport
{
	using Channel = io.netty.channel.Channel;

	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using Config = Neo4Net.Kernel.configuration.Config;

	/// <summary>
	/// Serves as an entry point for throttling of transport related resources. Currently only
	/// write operations are throttled based on pending buffered data. If there will be new types
	/// of throttles (number of active channels, reads, etc.) they should be added and registered
	/// through this class.
	/// </summary>
	public class TransportThrottleGroup
	{
		 public static readonly TransportThrottleGroup NoThrottle = new TransportThrottleGroup();

		 private readonly TransportThrottle _writeThrottle;

		 private TransportThrottleGroup()
		 {
			  this._writeThrottle = NoOpTransportThrottle.Instance;
		 }

		 public TransportThrottleGroup( Config config, Clock clock )
		 {
			  this._writeThrottle = CreateWriteThrottle( config, clock );
		 }

		 public virtual TransportThrottle WriteThrottle()
		 {
			  return _writeThrottle;
		 }

		 public virtual void Install( Channel channel )
		 {
			  _writeThrottle.install( channel );
		 }

		 public virtual void Uninstall( Channel channel )
		 {
			  _writeThrottle.uninstall( channel );
		 }

		 private static TransportThrottle CreateWriteThrottle( Config config, Clock clock )
		 {
			  if ( config.Get( GraphDatabaseSettings.bolt_outbound_buffer_throttle ) )
			  {
					return new TransportWriteThrottle( config.Get( GraphDatabaseSettings.bolt_outbound_buffer_throttle_low_water_mark ), config.Get( GraphDatabaseSettings.bolt_outbound_buffer_throttle_high_water_mark ), clock, config.Get( GraphDatabaseSettings.bolt_outbound_buffer_throttle_max_duration ) );
			  }

			  return NoOpTransportThrottle.Instance;
		 }

		 private class NoOpTransportThrottle : TransportThrottle
		 {
			  internal static readonly TransportThrottle Instance = new NoOpTransportThrottle();

			  public override void Install( Channel channel )
			  {

			  }

			  public override void Acquire( Channel channel )
			  {

			  }

			  public override void Release( Channel channel )
			  {

			  }

			  public override void Uninstall( Channel channel )
			  {

			  }
		 }
	}

}