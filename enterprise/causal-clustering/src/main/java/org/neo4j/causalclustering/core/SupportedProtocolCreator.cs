using System;
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
namespace Org.Neo4j.causalclustering.core
{

	using Org.Neo4j.causalclustering.protocol;
	using ApplicationSupportedProtocols = Org.Neo4j.causalclustering.protocol.handshake.ApplicationSupportedProtocols;
	using ModifierSupportedProtocols = Org.Neo4j.causalclustering.protocol.handshake.ModifierSupportedProtocols;
	using Org.Neo4j.Helpers.Collection;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using Streams = Org.Neo4j.Stream.Streams;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.protocol.Protocol_ApplicationProtocolCategory.CATCHUP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.protocol.Protocol_ApplicationProtocolCategory.RAFT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.protocol.Protocol_ModifierProtocolCategory.COMPRESSION;

	public class SupportedProtocolCreator
	{
		 private readonly Config _config;
		 private readonly Log _log;

		 public SupportedProtocolCreator( Config config, LogProvider logProvider )
		 {
			  this._config = config;
			  this._log = logProvider.getLog( this.GetType() );
		 }

		 public virtual ApplicationSupportedProtocols CreateSupportedCatchupProtocol()
		 {
			  return GetApplicationSupportedProtocols( _config.get( CausalClusteringSettings.CatchupImplementations ), CATCHUP );
		 }

		 public virtual ApplicationSupportedProtocols CreateSupportedRaftProtocol()
		 {
			  return GetApplicationSupportedProtocols( _config.get( CausalClusteringSettings.RaftImplementations ), RAFT );
		 }

		 private ApplicationSupportedProtocols GetApplicationSupportedProtocols( IList<int> configVersions, Org.Neo4j.causalclustering.protocol.Protocol_ApplicationProtocolCategory category )
		 {
			  if ( configVersions.Count == 0 )
			  {
					return new ApplicationSupportedProtocols( category, Collections.emptyList() );
			  }
			  else
			  {
					IList<int> knownVersions = ProtocolsForConfig( category, configVersions, version => Org.Neo4j.causalclustering.protocol.Protocol_ApplicationProtocols.find( category, version ) );
					if ( knownVersions.Count == 0 )
					{
						 throw new System.ArgumentException( format( "None of configured %s implementations %s are known", category.canonicalName(), configVersions ) );
					}
					else
					{
						 return new ApplicationSupportedProtocols( category, knownVersions );
					}
			  }
		 }

		 public virtual IList<ModifierSupportedProtocols> CreateSupportedModifierProtocols()
		 {
			  ModifierSupportedProtocols supportedCompression = CompressionProtocolVersions();

			  return Stream.of( supportedCompression ).filter( supportedProtocols => !supportedProtocols.versions().Empty ).collect(Collectors.toList());
		 }

		 private ModifierSupportedProtocols CompressionProtocolVersions()
		 {
			  IList<string> implementations = ProtocolsForConfig( COMPRESSION, _config.get( CausalClusteringSettings.CompressionImplementations ), implementation => Org.Neo4j.causalclustering.protocol.Protocol_ModifierProtocols.find( COMPRESSION, implementation ) );

			  return new ModifierSupportedProtocols( COMPRESSION, implementations );
		 }

		 private IList<IMPL> ProtocolsForConfig<IMPL, T>( Org.Neo4j.causalclustering.protocol.Protocol_Category<T> category, IList<IMPL> implementations, System.Func<IMPL, Optional<T>> finder ) where IMPL : IComparable<IMPL> where T : Org.Neo4j.causalclustering.protocol.Protocol<IMPL>
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return implementations.Select( impl => Pair.of( impl, finder( impl ) ) ).peek( protocolWithImplementation => logUnknownProtocol( category, protocolWithImplementation ) ).Select( Pair::other ).flatMap( Streams.ofOptional ).Select( Protocol::implementation ).ToList();
		 }

		 private void LogUnknownProtocol<IMPL, T>( Org.Neo4j.causalclustering.protocol.Protocol_Category<T> category, Pair<IMPL, Optional<T>> protocolWithImplementation ) where IMPL : IComparable<IMPL> where T : Org.Neo4j.causalclustering.protocol.Protocol<IMPL>
		 {
			  if ( !protocolWithImplementation.Other().Present )
			  {
					_log.warn( "Configured %s protocol implementation %s unknown. Ignoring.", category, protocolWithImplementation.First() );
			  }
		 }
	}

}