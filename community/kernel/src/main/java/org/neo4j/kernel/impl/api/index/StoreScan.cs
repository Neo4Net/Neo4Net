using System;

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
namespace Org.Neo4j.Kernel.Impl.Api.index
{
	using Org.Neo4j.Kernel.Api.Index;
	using PopulationProgress = Org.Neo4j.Storageengine.Api.schema.PopulationProgress;

	public interface StoreScan<FAILURE> where FAILURE : Exception
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void run() throws FAILURE;
		 void Run();

		 void Stop();

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: void acceptUpdate(MultipleIndexPopulator.MultipleIndexUpdater updater, org.neo4j.kernel.api.index.IndexEntryUpdate<?> update, long currentlyIndexedNodeId);
		 void acceptUpdate<T1>( MultipleIndexPopulator.MultipleIndexUpdater updater, IndexEntryUpdate<T1> update, long currentlyIndexedNodeId );

		 PopulationProgress Progress { get; }

		 /// <summary>
		 /// Give this <seealso cref="StoreScan"/> a <seealso cref="PhaseTracker"/> to report to.
		 /// Must not be called once scan has already started. </summary>
		 /// <param name="phaseTracker"> <seealso cref="PhaseTracker"/> this store scan shall report to. </param>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default void setPhaseTracker(PhaseTracker phaseTracker)
	//	 { // no-op
	//	 }
	}

}