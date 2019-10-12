using System;
using System.Collections.Generic;
using System.Threading;

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
namespace Org.Neo4j.Server.enterprise
{
	using ChannelException = org.jboss.netty.channel.ChannelException;


	using ClusterSettings = Org.Neo4j.cluster.ClusterSettings;
	using ClusterClientModule = Org.Neo4j.cluster.client.ClusterClientModule;
	using NotElectableElectionCredentialsProvider = Org.Neo4j.cluster.protocol.election.NotElectableElectionCredentialsProvider;
	using Predicates = Org.Neo4j.Function.Predicates;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using DefaultFileSystemAbstraction = Org.Neo4j.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using FileSystemLifecycleAdapter = Org.Neo4j.Io.fs.FileSystemLifecycleAdapter;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Dependencies = Org.Neo4j.Kernel.impl.util.Dependencies;
	using LifeSupport = Org.Neo4j.Kernel.Lifecycle.LifeSupport;
	using LifecycleException = Org.Neo4j.Kernel.Lifecycle.LifecycleException;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using FormattedLogProvider = Org.Neo4j.Logging.FormattedLogProvider;
	using LogService = Org.Neo4j.Logging.@internal.LogService;
	using StoreLogService = Org.Neo4j.Logging.@internal.StoreLogService;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.store_internal_log_path;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Exceptions.peel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.scheduler.JobSchedulerFactory.createScheduler;

	public class ArbiterBootstrapper : Bootstrapper, AutoCloseable
	{
		 private readonly LifeSupport _life = new LifeSupport();
		 private readonly Timer _timer = new Timer( true );

		 public override int Start( File homeDir, Optional<File> configFile, IDictionary<string, string> configOverrides )
		 {
			  Config config = GetConfig( configFile, configOverrides );
			  try
			  {
					DefaultFileSystemAbstraction fileSystem = new DefaultFileSystemAbstraction();
					_life.add( new FileSystemLifecycleAdapter( fileSystem ) );
					_life.add( createScheduler() );
					new ClusterClientModule( _life, new Dependencies(), new Monitors(), config, LogService(fileSystem, config), new NotElectableElectionCredentialsProvider() );
			  }
			  catch ( LifecycleException e )
			  {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings({"ThrowableResultOfMethodCallIgnored", "unchecked"}) Throwable cause = peel(e, org.neo4j.function.Predicates.instanceOf(org.neo4j.kernel.lifecycle.LifecycleException.class));
					Exception cause = peel( e, Predicates.instanceOf( typeof( LifecycleException ) ) );
					if ( cause is ChannelException )
					{
						 Console.Error.WriteLine( "ERROR: " + cause.Message + ( cause.InnerException != null ? ", caused by:" + cause.InnerException.Message : "" ) );
					}
					else
					{
						 Console.Error.WriteLine( "ERROR: Unknown error" );
						 throw e;
					}
			  }
			  AddShutdownHook();
			  _life.start();

			  return 0;
		 }

		 public override int Stop()
		 {
			  _life.shutdown();
			  return 0;
		 }

		 public override void Close()
		 {
			  Stop();
		 }

		 private static Config GetConfig( Optional<File> configFile, IDictionary<string, string> configOverrides )
		 {
			  Config config = Config.builder().withFile(configFile).withSettings(configOverrides).build();
			  VerifyConfig( config.Raw );
			  return config;
		 }

		 private static void VerifyConfig( IDictionary<string, string> config )
		 {
			  if ( !config.ContainsKey( ClusterSettings.initial_hosts.name() ) )
			  {
					throw new System.ArgumentException( "No initial hosts to connect to supplied" );
			  }
			  if ( !config.ContainsKey( ClusterSettings.server_id.name() ) )
			  {
					throw new System.ArgumentException( "No server id specified" );
			  }
		 }

		 private static LogService LogService( FileSystemAbstraction fileSystem, Config config )
		 {
			  File logFile = config.Get( store_internal_log_path );
			  try
			  {
					ZoneId zoneId = config.Get( GraphDatabaseSettings.db_timezone ).ZoneId;
					FormattedLogProvider logProvider = FormattedLogProvider.withZoneId( zoneId ).toOutputStream( System.out );
					return StoreLogService.withUserLogProvider( logProvider ).withInternalLog( logFile ).build( fileSystem );
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }

		 private void AddShutdownHook()
		 {
			  Runtime.Runtime.addShutdownHook(new Thread(() =>
			  {
			  _timer.schedule(new TimerTaskAnonymousInnerClass(this)
			 , 4_000L);
			  ArbiterBootstrapper.this.Stop();
			  }));
		 }

		 private class TimerTaskAnonymousInnerClass : TimerTask
		 {
			 private readonly ArbiterBootstrapper _outerInstance;

			 public TimerTaskAnonymousInnerClass( ArbiterBootstrapper outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override void run()
			 {
				 Console.Error.WriteLine( "Failed to stop in a reasonable time, terminating..." );
				 Runtime.Runtime.halt( 1 );
			 }
		 }
	}

}