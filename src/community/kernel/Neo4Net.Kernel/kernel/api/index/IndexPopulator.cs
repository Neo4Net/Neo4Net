using System.Collections.Generic;

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
namespace Neo4Net.Kernel.Api.Index
{

	using InternalIndexState = Neo4Net.Kernel.Api.Internal.InternalIndexState;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using PhaseTracker = Neo4Net.Kernel.Impl.Api.index.PhaseTracker;
	using UpdateMode = Neo4Net.Kernel.Impl.Api.index.UpdateMode;
	using SwallowingIndexUpdater = Neo4Net.Kernel.Impl.Api.index.updater.SwallowingIndexUpdater;
	using NodePropertyAccessor = Neo4Net.Kernel.Api.StorageEngine.NodePropertyAccessor;
	using IndexSample = Neo4Net.Kernel.Api.StorageEngine.schema.IndexSample;
	using PopulationProgress = Neo4Net.Kernel.Api.StorageEngine.schema.PopulationProgress;
	using StoreIndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.StoreIndexDescriptor;

	/// <summary>
	/// Used for initial population of an index.
	/// </summary>
	public interface IndexPopulator : IndexConfigProvider
	{
		 /// <summary>
		 /// Remove all data in the index and paves the way for populating an index.
		 /// </summary>
		 /// <exception cref="UncheckedIOException"> on I/O error. </exception>
		 void Create();

		 /// <summary>
		 /// Closes and deletes this index.
		 /// </summary>
		 void Drop();

		 /// <summary>
		 /// Called when initially populating an index over existing data. Guaranteed to be
		 /// called by the same thread every time. All data coming in here is guaranteed to not
		 /// have been added to this index previously, so no checks needs to be performed before applying it.
		 /// Implementations may verify constraints at this time, or defer them until the first verification
		 /// of <seealso cref="verifyDeferredConstraints(NodePropertyAccessor)"/>.
		 /// </summary>
		 /// <param name="updates"> batch of node property updates that needs to be inserted. Node ids will be retrieved using
		 /// <seealso cref="IndexEntryUpdate.getEntityId()"/> method and property values will be retrieved using
		 /// <seealso cref="IndexEntryUpdate.values()"/> method. </param>
		 /// <exception cref="IndexEntryConflictException"> if this is a uniqueness index and any of the updates are detected
		 /// to violate that constraint. Implementations may choose to not detect in this call, but instead do one efficient
		 /// pass over the index in <seealso cref="verifyDeferredConstraints(NodePropertyAccessor)"/>. </exception>
		 /// <exception cref="UncheckedIOException"> on I/O error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void add(java.util.Collection<? extends IndexEntryUpdate<?>> updates) throws org.Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 void add<T1>( ICollection<T1> updates );

		 /// <summary>
		 /// Verifies that each value in this index is unique.
		 /// This method is called after the index has been fully populated and is guaranteed to not have
		 /// concurrent changes while executing.
		 /// </summary>
		 /// <param name="nodePropertyAccessor"> <seealso cref="NodePropertyAccessor"/> for accessing properties from database storage
		 /// in the event of conflicting values. </param>
		 /// <exception cref="IndexEntryConflictException"> for first detected uniqueness conflict, if any. </exception>
		 /// <exception cref="UncheckedIOException"> on error reading from source files. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void verifyDeferredConstraints(org.Neo4Net.Kernel.Api.StorageEngine.NodePropertyAccessor nodePropertyAccessor) throws org.Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException;
		 void VerifyDeferredConstraints( NodePropertyAccessor nodePropertyAccessor );

		 /// <summary>
		 /// Return an updater for applying a set of changes to this index, generally this will be a set of changes from a
		 /// transaction.
		 /// 
		 /// Index population goes through the existing data in the graph and feeds relevant data to this populator.
		 /// Simultaneously as population progresses there might be incoming updates
		 /// from committing transactions, which needs to be applied as well. This populator will only receive updates
		 /// for nodes that it already has seen. Updates coming in here must be applied idempotently as the same data
		 /// may have been <seealso cref="add(System.Collections.ICollection) added previously"/>.
		 /// Updates can come in two different <seealso cref="IndexEntryUpdate.updateMode()"/> modes}.
		 /// <ol>
		 ///   <li><seealso cref="UpdateMode.ADDED"/> means that there's an added property to a node already seen by this
		 ///   populator and so needs to be added. Note that this addition needs to be applied idempotently.
		 ///   <li><seealso cref="UpdateMode.CHANGED"/> means that there's a change to a property for a node already seen by
		 ///   this populator and that this new change needs to be applied. Note that this change needs to be
		 ///   applied idempotently.</li>
		 ///   <li><seealso cref="UpdateMode.REMOVED"/> means that a property already seen by this populator or even the node itself
		 ///   has been removed and need to be removed from this index as well. Note that this removal needs to be
		 ///   applied idempotently.</li>
		 /// </ol>
		 /// </summary>
		 /// <param name="accessor"> accesses property data if implementation needs to be able look up property values while populating. </param>
		 /// <returns> an <seealso cref="IndexUpdater"/> which will funnel changes that happen concurrently with index population
		 /// into the population and incorporating them as part of the index population. </returns>
		 IndexUpdater NewPopulatingUpdater( NodePropertyAccessor accessor );

		 /// <summary>
		 /// Close this populator and releases any resources related to it.
		 /// If {@code populationCompletedSuccessfully} is {@code true} then it must mark this index
		 /// as <seealso cref="InternalIndexState.ONLINE"/> so that future invocations of its parent
		 /// <seealso cref="IndexProvider.getInitialState(StoreIndexDescriptor)"/> also returns <seealso cref="InternalIndexState.ONLINE"/>.
		 /// </summary>
		 /// <param name="populationCompletedSuccessfully"> {@code true} if the index population was successful, where the index should
		 /// be marked as <seealso cref="InternalIndexState.ONLINE"/>. Supplying {@code false} can have two meanings:
		 /// <ul>
		 ///     <li>if <seealso cref="markAsFailed(string)"/> have been called the end state should be <seealso cref="InternalIndexState.FAILED"/>.
		 ///     This method call should also make sure that the failure message gets stored for retrieval the next open too.</li>
		 ///     <li>if <seealso cref="markAsFailed(string)"/> have NOT been called the end state should be <seealso cref="InternalIndexState.POPULATING"/></li>
		 /// </ul> </param>
		 void Close( bool populationCompletedSuccessfully );

		 /// <summary>
		 /// Called then a population failed. The failure string should be stored for future retrieval by
		 /// <seealso cref="IndexProvider.getPopulationFailure(StoreIndexDescriptor)"/>. Called before <seealso cref="close(bool)"/>
		 /// if there was a failure during population.
		 /// </summary>
		 /// <param name="failure"> the description of the failure. </param>
		 /// <exception cref="UncheckedIOException"> if marking failed. </exception>
		 void MarkAsFailed( string failure );

		 /// <summary>
		 /// Add the given <seealso cref="IndexEntryUpdate update"/> to the sampler for this index.
		 /// </summary>
		 /// <param name="update"> update to include in sample </param>
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: void includeSample(IndexEntryUpdate<?> update);
		 void includeSample<T1>( IndexEntryUpdate<T1> update );

		 /// <returns> <seealso cref="IndexSample"/> from samples collected by <seealso cref="includeSample(IndexEntryUpdate)"/> calls. </returns>
		 IndexSample SampleResult();

		 /// <summary>
		 /// Returns actual population progress, given the progress of the scan. This is for when a populator needs to do
		 /// significant work after scan has completed where the scan progress can be seen as only a part of the whole progress. </summary>
		 /// <param name="scanProgress"> progress of the scan. </param>
		 /// <returns> progress of the population of this index as a whole. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default org.Neo4Net.Kernel.Api.StorageEngine.schema.PopulationProgress progress(org.Neo4Net.Kernel.Api.StorageEngine.schema.PopulationProgress scanProgress)
	//	 {
	//		  return scanProgress;
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default void scanCompleted(org.Neo4Net.kernel.impl.api.index.PhaseTracker phaseTracker) throws org.Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException
	//	 { // no-op by default
	//	 }
	}

	public static class IndexPopulator_Fields
	{
		 public static readonly IndexPopulator Empty = new IndexPopulator_Adapter();
	}

	 public class IndexPopulator_Adapter : IndexPopulator
	 {
		  public override void Create()
		  {
		  }

		  public override void Drop()
		  {
		  }

		  public override void Add<T1>( ICollection<T1> updates ) where T1 : IndexEntryUpdate<T1>
		  {
		  }

		  public override IndexUpdater NewPopulatingUpdater( NodePropertyAccessor accessor )
		  {
				return SwallowingIndexUpdater.INSTANCE;
		  }

		  public override void ScanCompleted( PhaseTracker phaseTracker )
		  {
		  }

		  public override void Close( bool populationCompletedSuccessfully )
		  {
		  }

		  public override void MarkAsFailed( string failure )
		  {
		  }

		  public override void IncludeSample<T1>( IndexEntryUpdate<T1> update )
		  {
		  }

		  public override IndexSample SampleResult()
		  {
				return new IndexSample();
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void verifyDeferredConstraints(org.Neo4Net.Kernel.Api.StorageEngine.NodePropertyAccessor nodePropertyAccessor) throws org.Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException
		  public override void VerifyDeferredConstraints( NodePropertyAccessor nodePropertyAccessor )
		  {
		  }
	 }

}