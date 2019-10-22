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
namespace Neo4Net.causalclustering.upstream
{

	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using TopologyService = Neo4Net.causalclustering.discovery.TopologyService;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Service = Neo4Net.Helpers.Service;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

	public abstract class UpstreamDatabaseSelectionStrategy : Service
	{
		 protected internal TopologyService TopologyService;
		 protected internal Config Config;
		 protected internal Log Log;
		 protected internal MemberId Myself;
		 protected internal string ReadableName;
		 protected internal string DbName;

		 public UpstreamDatabaseSelectionStrategy( string key, params string[] altKeys ) : base( key, altKeys )
		 {
		 }

		 // Service loader can't inject via the constructor
		 public virtual void Inject( TopologyService topologyService, Config config, LogProvider logProvider, MemberId myself )
		 {
			  this.TopologyService = topologyService;
			  this.Config = config;
			  this.Log = logProvider.getLog( this.GetType() );
			  this.Myself = myself;
			  this.DbName = config.Get( CausalClusteringSettings.database );

			  ReadableName = StreamSupport.stream( Keys.spliterator(), false ).collect(Collectors.joining(", "));
			  Log.info( "Using upstream selection strategy " + ReadableName );
			  Init();
		 }

		 public virtual void Init()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract java.util.Optional<org.Neo4Net.causalclustering.identity.MemberId> upstreamDatabase() throws UpstreamDatabaseSelectionException;
		 public abstract Optional<MemberId> UpstreamDatabase();

		 public override string ToString()
		 {
			  return NicelyCommaSeparatedList( Keys );
		 }

		 private static string NicelyCommaSeparatedList( IEnumerable<string> keys )
		 {
			  return StreamSupport.stream( keys.spliterator(), false ).collect(Collectors.joining(", "));
		 }
	}

}