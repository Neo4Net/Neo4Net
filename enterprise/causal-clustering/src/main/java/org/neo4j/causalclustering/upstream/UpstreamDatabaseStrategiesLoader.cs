﻿using System.Collections.Generic;

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
namespace Org.Neo4j.causalclustering.upstream
{

	using CausalClusteringSettings = Org.Neo4j.causalclustering.core.CausalClusteringSettings;
	using TopologyService = Org.Neo4j.causalclustering.discovery.TopologyService;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using Service = Org.Neo4j.Helpers.Service;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

	/// <summary>
	/// Loads and initialises any service implementations of <class>UpstreamDatabaseSelectionStrategy</class>.
	/// Exposes configured instances of that interface via an iterator.
	/// </summary>
	public class UpstreamDatabaseStrategiesLoader : IEnumerable<UpstreamDatabaseSelectionStrategy>
	{
		 private readonly TopologyService _topologyService;
		 private readonly Config _config;
		 private readonly MemberId _myself;
		 private readonly Log _log;
		 private readonly LogProvider _logProvider;

		 public UpstreamDatabaseStrategiesLoader( TopologyService topologyService, Config config, MemberId myself, LogProvider logProvider )
		 {
			  this._topologyService = topologyService;
			  this._config = config;
			  this._myself = myself;
			  this._log = logProvider.getLog( this.GetType() );
			  this._logProvider = logProvider;
		 }

		 public override IEnumerator<UpstreamDatabaseSelectionStrategy> Iterator()
		 {
			  IEnumerable<UpstreamDatabaseSelectionStrategy> allImplementationsOnClasspath = Service.load( typeof( UpstreamDatabaseSelectionStrategy ) );

			  LinkedHashSet<UpstreamDatabaseSelectionStrategy> candidates = new LinkedHashSet<UpstreamDatabaseSelectionStrategy>();
			  foreach ( string key in _config.get( CausalClusteringSettings.upstream_selection_strategy ) )
			  {
					foreach ( UpstreamDatabaseSelectionStrategy candidate in allImplementationsOnClasspath )
					{
						 if ( candidate.Keys.GetEnumerator().next().Equals(key) )
						 {
							  candidate.Inject( _topologyService, _config, _logProvider, _myself );
							  candidates.add( candidate );
						 }
					}
			  }

			  Log( candidates );

			  return candidates.GetEnumerator();
		 }

		 private void Log( LinkedHashSet<UpstreamDatabaseSelectionStrategy> candidates )
		 {
			  _log.debug( "Upstream database strategies loaded in order of precedence: " + NicelyCommaSeparatedList( candidates ) );
		 }

		 private static string NicelyCommaSeparatedList( ICollection<UpstreamDatabaseSelectionStrategy> items )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  return items.Select( UpstreamDatabaseSelectionStrategy::toString ).collect( Collectors.joining( ", " ) );
		 }
	}

}