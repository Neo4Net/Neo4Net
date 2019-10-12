using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Helpers.progress
{

	internal sealed class Aggregator
	{
		 private readonly IDictionary<ProgressListener, ProgressListener_MultiPartProgressListener.State> _states = new Dictionary<ProgressListener, ProgressListener_MultiPartProgressListener.State>();
		 private readonly Indicator _indicator;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") private volatile long progress;
		 private volatile long _progress;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") private volatile int last;
		 private volatile int _last;
		 private static readonly AtomicLongFieldUpdater<Aggregator> _progressUpdater = newUpdater( typeof( Aggregator ), "progress" );
		 private static readonly AtomicIntegerFieldUpdater<Aggregator> _lastUpdater = AtomicIntegerFieldUpdater.newUpdater( typeof( Aggregator ), "last" );
		 private long _totalCount;

		 internal Aggregator( Indicator indicator )
		 {
			  this._indicator = indicator;
		 }

		 internal void Add( ProgressListener progress, long totalCount )
		 {
			 lock ( this )
			 {
				  _states[progress] = ProgressListener_MultiPartProgressListener.State.Init;
				  this._totalCount += totalCount;
			 }
		 }

		 internal void Initialize()
		 {
			 lock ( this )
			 {
				  _indicator.startProcess( _totalCount );
				  if ( _states.Count == 0 )
				  {
						_indicator.progress( 0, _indicator.reportResolution() );
						_indicator.completeProcess();
				  }
			 }
		 }

		 internal void Update( long delta )
		 {
			  long progress = _progressUpdater.addAndGet( this, delta );
			  int current = ( int )( ( progress * _indicator.reportResolution() ) / _totalCount );
			  for ( int last = this._last; current > last; last = this._last )
			  {
					if ( _lastUpdater.compareAndSet( this, last, current ) )
					{
						 lock ( this )
						 {
							  _indicator.progress( last, current );
						 }
					}
			  }
		 }

		 internal void Start( ProgressListener_MultiPartProgressListener part )
		 {
			 lock ( this )
			 {
				  if ( _states.put( part, ProgressListener_MultiPartProgressListener.State.Live ) == ProgressListener_MultiPartProgressListener.State.Init )
				  {
						_indicator.startPart( part.Part, part.TotalCount );
				  }
			 }
		 }

		 internal void Complete( ProgressListener_MultiPartProgressListener part )
		 {
			 lock ( this )
			 {
				  if ( _states.Remove( part ) != null )
				  {
						_indicator.completePart( part.Part );
						if ( _states.Count == 0 )
						{
							 _indicator.completeProcess();
						}
				  }
			 }
		 }

		 internal void SignalFailure( Exception e )
		 {
			 lock ( this )
			 {
				  _indicator.failure( e );
			 }
		 }
	}

}