﻿using System;
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
namespace Neo4Net.causalclustering.core
{

	using Neo4Net.causalclustering.protocol;
	using ApplicationSupportedProtocols = Neo4Net.causalclustering.protocol.handshake.ApplicationSupportedProtocols;
	using ModifierSupportedProtocols = Neo4Net.causalclustering.protocol.handshake.ModifierSupportedProtocols;
	using Neo4Net.Collections.Helpers;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using Streams = Neo4Net.Stream.Streams;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.protocol.Protocol_ApplicationProtocolCategory.CATCHUP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.protocol.Protocol_ApplicationProtocolCategory.RAFT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.protocol.Protocol_ModifierProtocolCategory.COMPRESSION;

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

		 private ApplicationSupportedProtocols GetApplicationSupportedProtocols( IList<int> configVersions, Neo4Net.causalclustering.protocol.Protocol_ApplicationProtocolCategory category )
		 {
			  if ( configVersions.Count == 0 )
			  {
					return new ApplicationSupportedProtocols( category, Collections.emptyList() );
			  }
			  else
			  {
					IList<int> knownVersions = ProtocolsForConfig( category, configVersions, version => Neo4Net.causalclustering.protocol.Protocol_ApplicationProtocols.find( category, version ) );
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
			  IList<string> implementations = ProtocolsForConfig( COMPRESSION, _config.get( CausalClusteringSettings.CompressionImplementations ), implementation => Neo4Net.causalclustering.protocol.Protocol_ModifierProtocols.find( COMPRESSION, implementation ) );

			  return new ModifierSupportedProtocols( COMPRESSION, implementations );
		 }

		 private IList<IMPL> ProtocolsForConfig<IMPL, T>( Neo4Net.causalclustering.protocol.Protocol_Category<T> category, IList<IMPL> implementations, System.Func<IMPL, Optional<T>> finder ) where IMPL : IComparable<IMPL> where T : Neo4Net.causalclustering.protocol.Protocol<IMPL>
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return implementations.Select( impl => Pair.of( impl, finder( impl ) ) ).peek( protocolWithImplementation => logUnknownProtocol( category, protocolWithImplementation ) ).Select( Pair::other ).flatMap( Streams.ofOptional ).Select( Protocol::implementation ).ToList();
		 }

		 private void LogUnknownProtocol<IMPL, T>( Neo4Net.causalclustering.protocol.Protocol_Category<T> category, Pair<IMPL, Optional<T>> protocolWithImplementation ) where IMPL : IComparable<IMPL> where T : Neo4Net.causalclustering.protocol.Protocol<IMPL>
		 {
			  if ( !protocolWithImplementation.Other().Present )
			  {
					_log.warn( "Configured %s protocol implementation %s unknown. Ignoring.", category, protocolWithImplementation.First() );
			  }
		 }
	}

}