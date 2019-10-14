using System.Diagnostics;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.metrics.source.server
{
	using Gauge = com.codahale.metrics.Gauge;
	using MetricRegistry = com.codahale.metrics.MetricRegistry;

	using Documented = Neo4Net.Kernel.Impl.Annotations.Documented;
	using DependencySatisfier = Neo4Net.Kernel.impl.util.DependencySatisfier;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using Log = Neo4Net.Logging.Log;
	using LogService = Neo4Net.Logging.Internal.LogService;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.codahale.metrics.MetricRegistry.name;

	[Documented(".Server metrics")]
	public class ServerMetrics : LifecycleAdapter
	{
		 private const string NAME_PREFIX = "neo4j.server";

		 [Documented("The total number of idle threads in the jetty pool")]
		 public static readonly string ThreadJettyIdle = name( NAME_PREFIX, "threads.jetty.idle" );
		 [Documented("The total number of threads (both idle and busy) in the jetty pool")]
		 public static readonly string ThreadJettyAll = name( NAME_PREFIX, "threads.jetty.all" );

		 private readonly MetricRegistry _registry;
		 private volatile ServerThreadView _serverThreadView;

		 public ServerMetrics( MetricRegistry registry, LogService logService, DependencySatisfier satisfier )
		 {
			  Log userLog = logService.GetUserLog( this.GetType() );
			  this._registry = registry;
			  this._serverThreadView = new ServerThreadViewAnonymousInnerClass( this, userLog );
			  satisfier.SatisfyDependency((ServerThreadViewSetter) _serverThreadView =>
			  {
				Debug.Assert( ServerMetrics.this._serverThreadView != null );
				ServerMetrics.this._serverThreadView = _serverThreadView;
				userLog.Info( "Server thread metrics have been registered successfully" );
			  });
		 }

		 private class ServerThreadViewAnonymousInnerClass : ServerThreadView
		 {
			 private readonly ServerMetrics _outerInstance;

			 private Log _userLog;

			 public ServerThreadViewAnonymousInnerClass( ServerMetrics outerInstance, Log userLog )
			 {
				 this.outerInstance = outerInstance;
				 this._userLog = userLog;
			 }

			 private volatile bool warnedAboutIdle;
			 private volatile bool warnedAboutAll;
			 public int idleThreads()
			 {
				  if ( !warnedAboutIdle )
				  {
						_userLog.warn( "Server thread metrics not available (missing " + ThreadJettyIdle + ")" );
						warnedAboutIdle = true;
				  }
				  return -1;
			 }

			 public int allThreads()
			 {
				  if ( !warnedAboutAll )
				  {
						_userLog.warn( "Server thread metrics not available (missing " + ThreadJettyAll + ")" );
						warnedAboutAll = true;
				  }
				  return -1;
			 }
		 }

		 public override void Start()
		 {
			  _registry.register( ThreadJettyIdle, ( Gauge<int> )() => _serverThreadView.idleThreads() );
			  _registry.register( ThreadJettyAll, ( Gauge<int> )() => _serverThreadView.allThreads() );
		 }

		 public override void Stop()
		 {
			  _registry.remove( ThreadJettyIdle );
			  _registry.remove( ThreadJettyAll );
		 }
	}

}