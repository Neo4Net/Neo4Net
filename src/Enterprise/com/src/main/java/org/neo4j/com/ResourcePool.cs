using System;
using System.Collections.Concurrent;
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
namespace Neo4Net.com
{

	using Clocks = Neo4Net.Time.Clocks;

	public abstract class ResourcePool<R>
	{
		 public interface Monitor<R>
		 {
			  void UpdatedCurrentPeakSize( int currentPeakSize );

			  void UpdatedTargetSize( int targetSize );

			  void Created( R resource );

			  void Acquired( R resource );

			  void Disposed( R resource );
		 }

		  public class Monitor_Adapter<R> : Monitor<R>
		  {
			  private readonly ResourcePool<R> _outerInstance;

			  public Monitor_Adapter( ResourcePool<R> outerInstance )
			  {
				  this._outerInstance = outerInstance;
			  }

				public override void UpdatedCurrentPeakSize( int currentPeakSize )
				{
				}

				public override void UpdatedTargetSize( int targetSize )
				{
				}

				public override void Created( R resource )
				{
				}

				public override void Acquired( R resource )
				{
				}

				public override void Disposed( R resource )
				{
				}
		  }

		 public interface CheckStrategy
		 {
			  bool ShouldCheck();
		 }

		  public class CheckStrategy_TimeoutCheckStrategy : CheckStrategy
		  {
			  private readonly ResourcePool<R> _outerInstance;

				internal readonly long Interval;
				internal volatile long LastCheckTime;
				internal readonly Clock Clock;

				public CheckStrategy_TimeoutCheckStrategy( ResourcePool<R> outerInstance, long interval, Clock clock )
				{
					this._outerInstance = outerInstance;
					 this.Interval = interval;
					 this.LastCheckTime = clock.millis();
					 this.Clock = clock;
				}

				public override bool ShouldCheck()
				{
					 long currentTime = Clock.millis();
					 if ( currentTime > LastCheckTime + Interval )
					 {
						  LastCheckTime = currentTime;
						  return true;
					 }
					 return false;
				}
		  }

		 public const int DEFAULT_CHECK_INTERVAL = 60 * 1000;

		 // protected for testing
		 protected internal readonly LinkedList<R> Unused = new LinkedList<R>();
		 private readonly IDictionary<Thread, R> _current = new ConcurrentDictionary<Thread, R>();
		 private readonly Monitor<R> _monitor;
		 private readonly int _minSize;
		 private readonly CheckStrategy _checkStrategy;
		 // Guarded by nothing. Those are estimates, losing some values doesn't matter much
		 private int _currentPeakSize;
		 private int _targetSize;

		 protected internal ResourcePool( int minSize ) : this( minSize, new CheckStrategy_TimeoutCheckStrategy( this, DEFAULT_CHECK_INTERVAL, Clocks.systemClock() ), new Monitor_Adapter<>(this) )
		 {
		 }

		 protected internal ResourcePool( int minSize, CheckStrategy strategy, Monitor<R> monitor )
		 {
			  this._minSize = minSize;
			  this._currentPeakSize = 0;
			  this._targetSize = minSize;
			  this._checkStrategy = strategy;
			  this._monitor = monitor;
		 }

		 protected internal abstract R Create();

		 protected internal virtual void Dispose( R resource )
		 {
		 }

		 protected internal virtual int CurrentSize()
		 {
			  return _current.Count;
		 }

		 protected internal virtual bool IsAlive( R resource )
		 {
			  return true;
		 }

		 public R Acquire()
		 {
			  Thread thread = Thread.CurrentThread;
			  R resource = _current[thread];
			  if ( resource == default( R ) )
			  {
					IList<R> garbage = null;
					lock ( Unused )
					{
						 for ( ; ; )
						 {
							  resource = Unused.RemoveFirst();
							  if ( resource == default( R ) )
							  {
									break;
							  }
							  if ( IsAlive( resource ) )
							  {
									break;
							  }
							  if ( garbage == null )
							  {
									garbage = new LinkedList<R>();
							  }
							  garbage.Add( resource );
						 }
					}
					if ( resource == default( R ) )
					{
						 resource = Create();
						 _monitor.created( resource );
					}
					_current[thread] = resource;
					_monitor.acquired( resource );
					if ( garbage != null )
					{
						 foreach ( R dead in garbage )
						 {
							  Dispose( dead );
							  _monitor.disposed( dead );
						 }
					}
			  }
			  _currentPeakSize = Math.Max( _currentPeakSize, _current.Count );
			  if ( _checkStrategy.shouldCheck() )
			  {
					_targetSize = Math.Max( _minSize, _currentPeakSize );
					_monitor.updatedCurrentPeakSize( _currentPeakSize );
					_currentPeakSize = 0;
					_monitor.updatedTargetSize( _targetSize );
			  }

			  return resource;
		 }

		 public void Release()
		 {
			  Thread thread = Thread.CurrentThread;
			  R resource = _current.Remove( thread );
			  if ( resource != default( R ) )
			  {
					bool dead = false;
					lock ( Unused )
					{
						 if ( Unused.Count < _targetSize )
						 {
							  Unused.AddLast( resource );
						 }
						 else
						 {
							  dead = true;
						 }
					}
					if ( dead )
					{
						 Dispose( resource );
						 _monitor.disposed( resource );
					}
			  }
		 }

		 public void Close( bool force )
		 {
			  IList<R> dead = new LinkedList<R>();
			  lock ( Unused )
			  {
					( ( IList<R> )dead ).AddRange( Unused );
					Unused.Clear();
			  }
			  if ( force )
			  {
					( ( IList<R> )dead ).AddRange( _current.Values );
			  }
			  foreach ( R resource in dead )
			  {
					Dispose( resource );
			  }
		 }
	}

}