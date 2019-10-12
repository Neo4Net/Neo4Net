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
namespace Neo4Net.@unsafe.Impl.Batchimport.input
{

	using RecordFormats = Neo4Net.Kernel.impl.store.format.RecordFormats;

	public class EstimationSanityChecker
	{
		 private readonly RecordFormats _formats;
		 private readonly ImportLogic.Monitor _monitor;

		 public EstimationSanityChecker( RecordFormats formats, ImportLogic.Monitor monitor )
		 {
			  this._formats = formats;
			  this._monitor = monitor;
		 }

		 public virtual void SanityCheck( Input_Estimates estimates )
		 {
			  SanityCheckEstimateWithMaxId( estimates.NumberOfNodes(), _formats.node().MaxId, _monitor.mayExceedNodeIdCapacity );
			  SanityCheckEstimateWithMaxId( estimates.NumberOfRelationships(), _formats.relationship().MaxId, _monitor.mayExceedRelationshipIdCapacity );
		 }

		 private void SanityCheckEstimateWithMaxId( long estimate, long max, System.Action<long, long> reporter )
		 {
			  if ( estimate > max * 0.8 )
			  {
					reporter( max, estimate );
			  }
		 }
	}

}