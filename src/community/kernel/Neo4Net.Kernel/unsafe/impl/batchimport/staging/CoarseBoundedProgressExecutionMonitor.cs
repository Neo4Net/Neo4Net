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
	using Iterables = Neo4Net.Collections.Helpers.Iterables;
	using Keys = Neo4Net.@unsafe.Impl.Batchimport.stats.Keys;

	/// <summary>
	/// An <seealso cref="ExecutionMonitor"/> that prints progress in percent, knowing the max number of nodes and relationships
	/// in advance.
	/// </summary>
	public abstract class CoarseBoundedProgressExecutionMonitor : ExecutionMonitor_Adapter
	{
		 private readonly long _totalNumberOfBatches;
		 private long _prevDoneBatches;
		 private long _totalReportedBatches;

		 public CoarseBoundedProgressExecutionMonitor( long highNodeId, long highRelationshipId, Configuration configuration ) : base( 1, SECONDS )
		 {
			  // This calculation below is aware of internals of the parallel importer and may
			  // be wrong for other importers.
			  this._totalNumberOfBatches = ( highNodeId / configuration.BatchSize() ) * 3 + (highRelationshipId / configuration.BatchSize()) * 4; // rel records encountered four times
		 }

		 protected internal virtual long Total()
		 {
			  return _totalNumberOfBatches;
		 }

		 public override void Check( StageExecution execution )
		 {
			  Update( execution );
		 }

		 public override void Start( StageExecution execution )
		 {
			  _prevDoneBatches = 0;
		 }

		 private void Update( StageExecution execution )
		 {
			  long diff = 0;
			  long doneBatches = doneBatches( execution );
			  diff += doneBatches - _prevDoneBatches;
			  _prevDoneBatches = doneBatches;

			  if ( diff > 0 )
			  {
					_totalReportedBatches += diff;
					Progress( diff );
			  }
		 }

		 /// <param name="progress"> Relative progress. </param>
		 protected internal abstract void Progress( long progress );

		 private long DoneBatches( StageExecution execution )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Step<?> step = Neo4Net.helpers.collection.Iterables.last(execution.steps());
			  Step<object> step = Iterables.last( execution.Steps() );
			  return step.Stats().stat(Keys.done_batches).asLong();
		 }

		 public override void Done( bool successful, long totalTimeMillis, string additionalInformation )
		 {
			  // Just report the last progress
			  Progress( _totalNumberOfBatches - _totalReportedBatches );
		 }
	}

}