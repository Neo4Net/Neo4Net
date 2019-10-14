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
namespace Neo4Net.@unsafe.Impl.Batchimport.staging
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.round;

	/// <summary>
	/// Takes a value range and projects it to a very discrete number of integer values, quantizing based
	/// on float precision.
	/// </summary>
	public class QuantizedProjection
	{
		 private readonly long _max;
		 private readonly long _projectedMax;

		 private double _absoluteWay;
		 private long _step;

		 public QuantizedProjection( long max, long projectedMax )
		 {
			  this._max = max;
			  this._projectedMax = projectedMax;
		 }

		 /// <param name="step"> a part of the max, not the projection. </param>
		 /// <returns> {@code true} if the total so far including {@code step} is equal to or less than the max allowed,
		 /// otherwise {@code false} -- meaning that we stepped beyond max. </returns>
		 public virtual bool Next( long step )
		 {
			  double absoluteStep = ( double )step / ( double )_max;
			  if ( _absoluteWay + absoluteStep > 1f )
			  {
					return false;
			  }

			  long prevProjection = round( _absoluteWay * _projectedMax );
			  _absoluteWay += absoluteStep;
			  long projection = round( _absoluteWay * _projectedMax );
			  this._step = projection - prevProjection;

			  return true;
		 }

		 public virtual long Step()
		 {
			  return _step;
		 }
	}

}