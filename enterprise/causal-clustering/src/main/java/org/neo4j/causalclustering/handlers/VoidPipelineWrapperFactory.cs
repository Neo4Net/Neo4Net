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
namespace Org.Neo4j.causalclustering.handlers
{
	using Channel = io.netty.channel.Channel;
	using ChannelHandler = io.netty.channel.ChannelHandler;

	using Org.Neo4j.Graphdb.config;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Dependencies = Org.Neo4j.Kernel.impl.util.Dependencies;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

	public class VoidPipelineWrapperFactory : DuplexPipelineWrapperFactory
	{
		 public static readonly PipelineWrapper VOID_WRAPPER = new PipelineWrapperAnonymousInnerClass();

		 private class PipelineWrapperAnonymousInnerClass : PipelineWrapper
		 {
			 public IList<ChannelHandler> handlersFor( Channel channel )
			 {
				  return emptyList();
			 }

			 public string name()
			 {
				  return "void";
			 }
		 }

		 public override PipelineWrapper ForServer( Config config, Dependencies dependencies, LogProvider logProvider, Setting<string> policyName )
		 {
			  VerifyNoEncryption( config, policyName );
			  return VOID_WRAPPER;
		 }

		 public override PipelineWrapper ForClient( Config config, Dependencies dependencies, LogProvider logProvider, Setting<string> policyName )
		 {
			  VerifyNoEncryption( config, policyName );
			  return VOID_WRAPPER;
		 }

		 private static void VerifyNoEncryption( Config config, Setting<string> policyName )
		 {
			  if ( !string.ReferenceEquals( config.Get( policyName ), null ) )
			  {
					throw new System.ArgumentException( "Unexpected SSL policy " + policyName );
			  }
		 }
	}

}