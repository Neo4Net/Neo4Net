using System;
using System.Diagnostics;

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
namespace Neo4Net.Adversaries
{
	/// <summary>
	/// An adversary that injects failures randomly, based on a configured probability.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public class RandomAdversary extends AbstractAdversary
	public class RandomAdversary : AbstractAdversary
	{
		 private const double STANDARD_PROBABILITY_FACTOR = 1.0;
		 private readonly double _mischiefRate;
		 private readonly double _failureRate;
		 private readonly double _errorRate;
		 private volatile double _probabilityFactor;

		 public RandomAdversary( double mischiefRate, double failureRate, double errorRate )
		 {
			  Debug.Assert( 0 <= mischiefRate && mischiefRate < 1.0, "Expected mischief rate in [0.0; 1.0[ but was " + mischiefRate );
			  Debug.Assert( 0 <= failureRate && failureRate < 1.0, "Expected failure rate in [0.0; 1.0[ but was " + failureRate );
			  Debug.Assert( 0 <= errorRate && errorRate < 1.0, "Expected error rate in [0.0; 1.0[ but was " + errorRate );
			  Debug.Assert( mischiefRate + errorRate + failureRate < 1.0, "Expected mischief rate + error rate + failure rate in [0.0; 1.0[ but was " + ( mischiefRate + errorRate + failureRate ) );

			  this._mischiefRate = mischiefRate;
			  this._failureRate = failureRate;
			  this._errorRate = errorRate;
			  _probabilityFactor = STANDARD_PROBABILITY_FACTOR;
		 }

		 public override void InjectFailure( params Type[] failureTypes )
		 {
			  MaybeDoBadStuff( failureTypes, false );
		 }

		 public override bool InjectFailureOrMischief( params Type[] failureTypes )
		 {
			  return MaybeDoBadStuff( failureTypes, true );
		 }

		 private bool MaybeDoBadStuff( Type[] failureTypes, bool includingMischeif )
		 {
			  double luckyDraw = Rng.NextDouble();
			  double factor = _probabilityFactor;
			  bool resetUponFailure = false;
			  if ( factor < 0 )
			  {
					resetUponFailure = true;
					factor = -factor;
			  }

			  if ( luckyDraw <= _errorRate * factor )
			  {
					if ( resetUponFailure )
					{
						 _probabilityFactor = STANDARD_PROBABILITY_FACTOR;
					}
					ThrowOneOf( typeof( System.OutOfMemoryException ), typeof( System.NullReferenceException ) );
			  }
			  if ( failureTypes.Length > 0 && luckyDraw <= ( _failureRate + _errorRate ) * factor )
			  {
					if ( resetUponFailure )
					{
						 _probabilityFactor = STANDARD_PROBABILITY_FACTOR;
					}
					ThrowOneOf( failureTypes );
			  }
			  return includingMischeif && luckyDraw <= ( _mischiefRate + _failureRate + _errorRate ) * factor;
		 }

		 public virtual double ProbabilityFactor
		 {
			 set
			 {
				  _probabilityFactor = value;
			 }
		 }

		 public virtual double AndResetProbabilityFactor
		 {
			 set
			 {
				  // The negative sign bit indicates that the rate should be reset upon failure
				  _probabilityFactor = -value;
			 }
		 }
	}

}