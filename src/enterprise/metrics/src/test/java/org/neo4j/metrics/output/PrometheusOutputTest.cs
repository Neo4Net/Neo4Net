using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.metrics.output
{
	using Gauge = com.codahale.metrics.Gauge;
	using MetricRegistry = com.codahale.metrics.MetricRegistry;
	using Test = org.junit.Test;


	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using ConnectorPortRegister = Neo4Net.Kernel.configuration.ConnectorPortRegister;
	using Log = Neo4Net.Logging.Log;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;

	public class PrometheusOutputTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void eventsShouldBeRedirectedToGauges() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void EventsShouldBeRedirectedToGauges()
		 {
			  MetricRegistry registry = new MetricRegistry();
			  DynamicAddressPrometheusOutput dynamicOutput = new DynamicAddressPrometheusOutput( "localhost", registry, mock( typeof( Log ) ) );

			  System.Action<long> callback = l =>
			  {
				SortedDictionary<string, Gauge> gauges = new SortedDictionary<string, Gauge>();
				gauges.put( "my.event", () => l );
				dynamicOutput.Report( gauges, emptySortedMap(), emptySortedMap(), emptySortedMap(), emptySortedMap() );
			  };

			  callback( 10 );

			  dynamicOutput.Init();
			  dynamicOutput.Start();

			  string serverAddress = dynamicOutput.ServerAddress;
			  assertTrue( GetResponse( serverAddress ).Contains( "my_event 10.0" ) );
			  assertTrue( GetResponse( serverAddress ).Contains( "my_event 10.0" ) );

			  callback( 20 );
			  assertTrue( GetResponse( serverAddress ).Contains( "my_event 20.0" ) );
			  assertTrue( GetResponse( serverAddress ).Contains( "my_event 20.0" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void metricsRegisteredAfterStartShouldBeIncluded() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MetricsRegisteredAfterStartShouldBeIncluded()
		 {
			  MetricRegistry registry = new MetricRegistry();
			  DynamicAddressPrometheusOutput dynamicOutput = new DynamicAddressPrometheusOutput( "localhost", registry, mock( typeof( Log ) ) );

			  System.Action<long> callback = l =>
			  {
				SortedDictionary<string, Gauge> gauges = new SortedDictionary<string, Gauge>();
				gauges.put( "my.event", () => l );
				dynamicOutput.Report( gauges, emptySortedMap(), emptySortedMap(), emptySortedMap(), emptySortedMap() );
			  };

			  registry.register( "my.metric", ( Gauge )() => 10 );

			  dynamicOutput.Init();
			  dynamicOutput.Start();

			  callback( 20 );

			  string serverAddress = dynamicOutput.ServerAddress;
			  string response = GetResponse( serverAddress );
			  assertTrue( response.Contains( "my_metric 10.0" ) );
			  assertTrue( response.Contains( "my_event 20.0" ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static String getResponse(String serverAddress) throws java.io.IOException
		 private static string GetResponse( string serverAddress )
		 {
			  string url = "http://" + serverAddress + "/metrics";
			  URLConnection connection = ( new URL( url ) ).openConnection();
			  connection.DoOutput = true;
			  connection.connect();
			  using ( Scanner s = ( new Scanner( connection.InputStream, "UTF-8" ) ).useDelimiter( "\\A" ) )
			  {
					assertTrue( s.hasNext() );
					string ret = s.next();
					assertFalse( s.hasNext() );
					return ret;
			  }
		 }

		 private class DynamicAddressPrometheusOutput : PrometheusOutput
		 {
			  internal DynamicAddressPrometheusOutput( string host, MetricRegistry registry, Log logger ) : base( new HostnamePort( host ), registry, logger, mock( typeof( ConnectorPortRegister ) ) )
			  {
			  }

			  internal virtual string ServerAddress
			  {
				  get
				  {
						InetSocketAddress address = Server.Address;
						return address.HostString + ":" + address.Port;
				  }
			  }
		 }
	}

}