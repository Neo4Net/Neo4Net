using System;
using System.Threading;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Helpers
{

	[Obsolete]
	public class NamedThreadFactory : ThreadFactory
	{
		 [Obsolete]
		 public interface Monitor
		 {
			  [Obsolete]
			  void ThreadCreated( string threadNamePrefix );

			  [Obsolete]
			  void ThreadFinished( string threadNamePrefix );
		 }

		 [Obsolete]
		 public static readonly Monitor NO_OP_MONITOR = new MonitorAnonymousInnerClass();

		 private class MonitorAnonymousInnerClass : Monitor
		 {
			 public void threadCreated( string threadNamePrefix )
			 {
			 }

			 public void threadFinished( string threadNamePrefix )
			 {
			 }
		 }

		 private static readonly int _defaultThreadPriority = Thread.NORM_PRIORITY;

		 private readonly ThreadGroup _group;
		 private readonly AtomicInteger _threadCounter = new AtomicInteger( 1 );
		 private string _threadNamePrefix;
		 private readonly int _priority;
		 private readonly bool _daemon;
		 private readonly Monitor _monitor;

		 [Obsolete]
		 public NamedThreadFactory( string threadNamePrefix ) : this( threadNamePrefix, _defaultThreadPriority )
		 {
		 }

		 [Obsolete]
		 public NamedThreadFactory( string threadNamePrefix, int priority ) : this( threadNamePrefix, priority, NO_OP_MONITOR )
		 {
		 }

		 [Obsolete]
		 public NamedThreadFactory( string threadNamePrefix, Monitor monitor ) : this( threadNamePrefix, _defaultThreadPriority, monitor )
		 {
		 }

		 [Obsolete]
		 public NamedThreadFactory( string threadNamePrefix, int priority, Monitor monitor ) : this( threadNamePrefix, priority, monitor, false )
		 {
		 }

		 [Obsolete]
		 public NamedThreadFactory( string threadNamePrefix, int priority, bool daemon ) : this( threadNamePrefix, priority, NO_OP_MONITOR, daemon )
		 {
		 }

		 [Obsolete]
		 public NamedThreadFactory( string threadNamePrefix, bool daemon ) : this( threadNamePrefix, _defaultThreadPriority, NO_OP_MONITOR, daemon )
		 {
		 }

		 [Obsolete]
		 public NamedThreadFactory( string threadNamePrefix, int priority, Monitor monitor, bool daemon )
		 {
			  this._threadNamePrefix = threadNamePrefix;
			  SecurityManager securityManager = System.SecurityManager;
			  _group = ( securityManager != null ) ? securityManager.ThreadGroup : Thread.CurrentThread.ThreadGroup;
			  this._priority = priority;
			  this._daemon = daemon;
			  this._monitor = monitor;
		 }

		 public override Thread NewThread( ThreadStart runnable )
		 {
			  int id = _threadCounter.AndIncrement;

			  Thread result = new Thread(_group, runnable, _threadNamePrefix + "-" + id() =>
			  {
				 try
				 {
					  base.run();
				 }
				 finally
				 {
					  _monitor.threadFinished( _threadNamePrefix );
				 }
			  });

			  result.Daemon = _daemon;
			  result.Priority = _priority;
			  _monitor.threadCreated( _threadNamePrefix );
			  return result;
		 }

		 [Obsolete]
		 public static NamedThreadFactory Named( string threadNamePrefix )
		 {
			  return new NamedThreadFactory( threadNamePrefix );
		 }

		 [Obsolete]
		 public static NamedThreadFactory Named( string threadNamePrefix, int priority )
		 {
			  return new NamedThreadFactory( threadNamePrefix, priority );
		 }

		 [Obsolete]
		 public static NamedThreadFactory Daemon( string threadNamePrefix )
		 {
			  return Daemon( threadNamePrefix, NO_OP_MONITOR );
		 }

		 [Obsolete]
		 public static NamedThreadFactory Daemon( string threadNamePrefix, Monitor monitor )
		 {
			  return new NamedThreadFactory( threadNamePrefix, _defaultThreadPriority, monitor, true );
		 }
	}

}