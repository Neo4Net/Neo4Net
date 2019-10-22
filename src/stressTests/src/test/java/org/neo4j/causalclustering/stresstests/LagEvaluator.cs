using System;

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
namespace Neo4Net.causalclustering.stresstests
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.floorDiv;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.multiplyExact;

	internal class LagEvaluator
	{
		 internal class Lag
		 {
			 private readonly LagEvaluator _outerInstance;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly long TimeLagMillisConflict;
			  internal readonly long ValueLag;

			  internal Lag( LagEvaluator outerInstance, long timeLagMillis, long valueLag )
			  {
				  this._outerInstance = outerInstance;
					this.TimeLagMillisConflict = timeLagMillis;
					this.ValueLag = valueLag;
			  }

			  internal virtual long TimeLagMillis()
			  {
					return TimeLagMillisConflict;
			  }

			  public override string ToString()
			  {
					return "Lag{" + "timeLagMillis=" + TimeLagMillisConflict + ", valueLag=" + ValueLag + '}';
			  }
		 }

		 private readonly System.Func<long?> _leader;
		 private readonly System.Func<long?> _follower;
		 private readonly Clock _clock;

		 private Sample _previous = Sample.INCOMPLETE;

		 internal LagEvaluator( System.Func<long?> leader, System.Func<long?> follower, Clock clock )
		 {
			  this._leader = leader;
			  this._follower = follower;
			  this._clock = clock;
		 }

		 internal virtual Optional<Lag> Evaluate()
		 {
			  Sample current = SampleNow();
			  Optional<Lag> lag = EstimateLag( _previous, current );
			  _previous = current;
			  return lag;
		 }

		 private Optional<Lag> EstimateLag( Sample previous, Sample current )
		 {
			  if ( previous.Incomplete() || current.Incomplete() )
			  {
					return null;
			  }

			  if ( current.TimeStampMillis <= previous.TimeStampMillis )
			  {
					throw new Exception( format( "Time not progressing: %s -> %s", previous, current ) );
			  }
			  else if ( current.Follower < previous.Follower )
			  {
					throw new Exception( format( "Follower going backwards: %s -> %s", previous, current ) );
			  }
			  else if ( current.Follower == previous.Follower )
			  {
					return null;
			  }

			  long valueLag = current.Leader - current.Follower;
			  long dTime = current.TimeStampMillis - previous.TimeStampMillis;
			  long dFollower = current.Follower - previous.Follower;
			  long timeLagMillis = floorDiv( multiplyExact( valueLag, dTime ), dFollower );

			  return ( new Lag( this, timeLagMillis, valueLag ) );
		 }

		 private Sample SampleNow()
		 {
			  // sample follower before leader, to avoid erroneously observing the follower as being ahead
			  long? followerSample = _follower.get();
			  long? leaderSample = _leader.get();

			  if ( !followerSample.HasValue || !leaderSample.HasValue )
			  {
					return Sample.INCOMPLETE;
			  }

			  return new Sample( leaderSample.Value, followerSample.Value, _clock.millis() );
		 }

		 private class Sample
		 {
			  internal static readonly Sample INCOMPLETE = new SampleAnonymousInnerClass();

			  private class SampleAnonymousInnerClass : Sample
			  {
				  internal override bool incomplete()
				  {
						return true;
				  }
			  }

			  internal readonly long TimeStampMillis;
			  internal readonly long Leader;
			  internal readonly long Follower;

			  internal Sample()
			  {
					TimeStampMillis = 0;
					Follower = 0;
					Leader = 0;
			  }

			  internal Sample( long leader, long follower, long timeStampMillis )
			  {
					this.TimeStampMillis = timeStampMillis;
					this.Leader = leader;
					this.Follower = follower;
			  }

			  internal virtual bool Incomplete()
			  {
					return false;
			  }
		 }
	}

}