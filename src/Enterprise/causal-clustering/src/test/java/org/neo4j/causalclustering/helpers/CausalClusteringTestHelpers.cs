using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.causalclustering.helpers
{

	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using Config = Neo4Net.Kernel.configuration.Config;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;

	public class CausalClusteringTestHelpers
	{
		 public static string TransactionAddress( GraphDatabaseFacade graphDatabase )
		 {
			  AdvertisedSocketAddress hostnamePort = graphDatabase.DependencyResolver.resolveDependency( typeof( Config ) ).get( CausalClusteringSettings.transaction_advertised_address );
			  return string.Format( "{0}:{1}", hostnamePort.Hostname, hostnamePort.Port );
		 }

		 public static string BackupAddress( GraphDatabaseFacade graphDatabaseFacade )
		 {
			  HostnamePort backupAddress = graphDatabaseFacade.DependencyResolver.resolveDependency( typeof( Config ) ).get( OnlineBackupSettings.online_backup_server );
			  return string.Format( "{0}:{1}", backupAddress.Host, backupAddress.Port );
		 }

		 public static IDictionary<int, string> DistributeDatabaseNamesToHostNums( int nHosts, ISet<string> databaseNames )
		 {
			  //Max number of hosts per database is (nHosts / nDatabases) or (nHosts / nDatabases) + 1
			  int nDatabases = databaseNames.Count;
			  int maxCapacity = ( nHosts % nDatabases == 0 ) ? ( nHosts / nDatabases ) : ( nHosts / nDatabases ) + 1;

			  IList<string> repeated = databaseNames.stream().flatMap(db => IntStream.range(0, maxCapacity).mapToObj(ignored => db)).collect(Collectors.toList());

			  IDictionary<int, string> mapping = new Dictionary<int, string>( nHosts );

			  for ( int hostId = 0; hostId < nHosts; hostId++ )
			  {
					mapping[hostId] = repeated[hostId];
			  }
			  return mapping;
		 }
	}

}