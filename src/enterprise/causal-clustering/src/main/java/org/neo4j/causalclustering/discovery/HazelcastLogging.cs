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
namespace Neo4Net.causalclustering.discovery
{
	using ILogger = com.hazelcast.logging.ILogger;
	using LoggerFactory = com.hazelcast.logging.LoggerFactory;

	using LogProvider = Neo4Net.Logging.LogProvider;

	public class HazelcastLogging : LoggerFactory
	{
		 // there is no constant in the hazelcast library for this
		 private const string HZ_LOGGING_CLASS_PROPERTY = "hazelcast.logging.class";

		 private static LogProvider _logProvider;
		 private static Level _minLevel;

		 internal static void Enable( LogProvider logProvider, Level minLevel )
		 {
			  HazelcastLogging._logProvider = logProvider;
			  HazelcastLogging._minLevel = minLevel;

			  // hazelcast only allows configuring logging through system properties
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  System.setProperty( HZ_LOGGING_CLASS_PROPERTY, typeof( HazelcastLogging ).FullName );
		 }

		 public override ILogger GetLogger( string name )
		 {
			  return new HazelcastLogger( _logProvider.getLog( name ), _minLevel );
		 }
	}

}