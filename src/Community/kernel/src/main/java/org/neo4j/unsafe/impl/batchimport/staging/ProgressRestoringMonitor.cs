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
namespace Neo4Net.@unsafe.Impl.Batchimport.staging
{
	using ExternalMonitor = Neo4Net.@unsafe.Impl.Batchimport.staging.HumanUnderstandableExecutionMonitor.ExternalMonitor;
	using Monitor = Neo4Net.@unsafe.Impl.Batchimport.staging.OnDemandDetailsExecutionMonitor.Monitor;

	/// <summary>
	/// Monitor connecting a <seealso cref="HumanUnderstandableExecutionMonitor"/> and <seealso cref="OnDemandDetailsExecutionMonitor"/>,
	/// their monitors at least for the sole purpose of notifying <seealso cref="HumanUnderstandableExecutionMonitor"/> about when
	/// there are other output interfering with it's nice progress printing. If something else gets printed it can restore its
	/// progress from 0..current.
	/// </summary>
	public class ProgressRestoringMonitor : ExternalMonitor, Monitor
	{
		 private volatile bool _detailsPrinted;

		 public override void DetailsPrinted()
		 {
			  this._detailsPrinted = true;
		 }

		 public override bool SomethingElseBrokeMyNiceOutput()
		 {
			  if ( _detailsPrinted )
			  {
					_detailsPrinted = false;
					return true;
			  }
			  return false;
		 }
	}

}