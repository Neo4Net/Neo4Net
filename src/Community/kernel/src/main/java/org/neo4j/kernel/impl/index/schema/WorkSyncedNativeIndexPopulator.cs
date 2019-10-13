using System;
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
namespace Neo4Net.Kernel.Impl.Index.Schema
{

	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using Neo4Net.Kernel.Api.Index;
	using IndexPopulator = Neo4Net.Kernel.Api.Index.IndexPopulator;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using ReporterFactory = Neo4Net.Kernel.Impl.Annotations.ReporterFactory;
	using PhaseTracker = Neo4Net.Kernel.Impl.Api.index.PhaseTracker;
	using NodePropertyAccessor = Neo4Net.Storageengine.Api.NodePropertyAccessor;
	using IndexSample = Neo4Net.Storageengine.Api.schema.IndexSample;
	using Neo4Net.Utils.Concurrent;
	using Neo4Net.Utils.Concurrent;
	using Value = Neo4Net.Values.Storable.Value;

	/// <summary>
	/// Takes a <seealso cref="NativeIndexPopulator"/>, which is intended for single-threaded population and wraps it in a populator
	/// which applies <seealso cref="WorkSync"/> to multi-threaded inserts, making them look like single-threaded inserts.
	/// </summary>
	/// @param <KEY> type of <seealso cref="NativeIndexKey"/> </param>
	/// @param <VALUE> type of <seealso cref="NativeIndexValue"/> </param>
	internal class WorkSyncedNativeIndexPopulator<KEY, VALUE> : IndexPopulator, ConsistencyCheckable where KEY : NativeIndexKey<KEY> where VALUE : NativeIndexValue
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			_workSync = new WorkSync<IndexUpdateApply, IndexUpdateWork>( new IndexUpdateApply( this ) );
		}

		 private readonly NativeIndexPopulator<KEY, VALUE> _actual;
		 private WorkSync<IndexUpdateApply, IndexUpdateWork> _workSync;

		 internal WorkSyncedNativeIndexPopulator( NativeIndexPopulator<KEY, VALUE> actual )
		 {
			 if ( !InstanceFieldsInitialized )
			 {
				 InitializeInstanceFields();
				 InstanceFieldsInitialized = true;
			 }
			  this._actual = actual;
		 }

		 /// <summary>
		 /// Method visible due to the complex nature of the "part" populators in the legacy temporal/spatial implementations.
		 /// This can go as soon as they disappear.
		 /// </summary>
		 internal virtual NativeIndexPopulator<KEY, VALUE> Actual
		 {
			 get
			 {
				  return _actual;
			 }
		 }

		 public override void Create()
		 {
			  _actual.create();
		 }

		 public override void Drop()
		 {
			  _actual.drop();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void add(java.util.Collection<? extends org.neo4j.kernel.api.index.IndexEntryUpdate<?>> updates) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 public override void Add<T1>( ICollection<T1> updates ) where T1 : Neo4Net.Kernel.Api.Index.IndexEntryUpdate<T1>
		 {
			  try
			  {
					_workSync.apply( new IndexUpdateWork( this, updates ) );
			  }
			  catch ( ExecutionException e )
			  {
					Exception cause = e.InnerException;
					if ( cause is IOException )
					{
						 throw new UncheckedIOException( ( IOException ) cause );
					}
					if ( cause is IndexEntryConflictException )
					{
						 throw ( IndexEntryConflictException ) cause;
					}
					throw new Exception( cause );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void verifyDeferredConstraints(org.neo4j.storageengine.api.NodePropertyAccessor nodePropertyAccessor) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 public override void VerifyDeferredConstraints( NodePropertyAccessor nodePropertyAccessor )
		 {
			  _actual.verifyDeferredConstraints( nodePropertyAccessor );
		 }

		 public override IndexUpdater NewPopulatingUpdater( NodePropertyAccessor accessor )
		 {
			  return _actual.newPopulatingUpdater( accessor );
		 }

		 public override void Close( bool populationCompletedSuccessfully )
		 {
			  _actual.close( populationCompletedSuccessfully );
		 }

		 public override void MarkAsFailed( string failure )
		 {
			  _actual.markAsFailed( failure );
		 }

		 public override void IncludeSample<T1>( IndexEntryUpdate<T1> update )
		 {
			  _actual.includeSample( update );
		 }

		 public override IndexSample SampleResult()
		 {
			  return _actual.sampleResult();
		 }

		 public override bool ConsistencyCheck( ReporterFactory reporterFactory )
		 {
			  return _actual.consistencyCheck( reporterFactory );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void scanCompleted(org.neo4j.kernel.impl.api.index.PhaseTracker phaseTracker) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 public override void ScanCompleted( PhaseTracker phaseTracker )
		 {
			  _actual.scanCompleted( phaseTracker );
		 }

		 public override IDictionary<string, Value> IndexConfig()
		 {
			  return _actual.indexConfig();
		 }

		 private class IndexUpdateApply
		 {
			 private readonly WorkSyncedNativeIndexPopulator<KEY, VALUE> _outerInstance;

			 public IndexUpdateApply( WorkSyncedNativeIndexPopulator<KEY, VALUE> outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void process(java.util.Collection<? extends org.neo4j.kernel.api.index.IndexEntryUpdate<?>> indexEntryUpdates) throws Exception
			  internal virtual void Process<T1>( ICollection<T1> indexEntryUpdates ) where T1 : Neo4Net.Kernel.Api.Index.IndexEntryUpdate<T1>
			  {
					outerInstance.actual.Add( indexEntryUpdates );
			  }
		 }

		 private class IndexUpdateWork : Work<IndexUpdateApply, IndexUpdateWork>
		 {
			 private readonly WorkSyncedNativeIndexPopulator<KEY, VALUE> _outerInstance;

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.Collection<? extends org.neo4j.kernel.api.index.IndexEntryUpdate<?>> updates;
			  internal readonly ICollection<IndexEntryUpdate<object>> Updates;

			  internal IndexUpdateWork<T1>( WorkSyncedNativeIndexPopulator<KEY, VALUE> outerInstance, ICollection<T1> updates ) where T1 : Neo4Net.Kernel.Api.Index.IndexEntryUpdate<T1>
			  {
				  this._outerInstance = outerInstance;
					this.Updates = updates;
			  }

			  public override IndexUpdateWork Combine( IndexUpdateWork work )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.ArrayList<org.neo4j.kernel.api.index.IndexEntryUpdate<?>> combined = new java.util.ArrayList<>(updates);
					List<IndexEntryUpdate<object>> combined = new List<IndexEntryUpdate<object>>( Updates );
					combined.AddRange( work.Updates );
					return new IndexUpdateWork( _outerInstance, combined );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void apply(IndexUpdateApply indexUpdateApply) throws Exception
			  public override void Apply( IndexUpdateApply indexUpdateApply )
			  {
					indexUpdateApply.Process( Updates );
			  }
		 }
	}

}