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
namespace Neo4Net.Kernel.Impl.Api.index
{

	using Neo4Net.GraphDb;
	using InternalIndexState = Neo4Net.Internal.Kernel.Api.InternalIndexState;
	using IndexNotFoundKernelException = Neo4Net.Internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using Neo4Net.Kernel.Api.Index;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using CapableIndexDescriptor = Neo4Net.Storageengine.Api.schema.CapableIndexDescriptor;
	using IndexReader = Neo4Net.Storageengine.Api.schema.IndexReader;
	using PopulationProgress = Neo4Net.Storageengine.Api.schema.PopulationProgress;
	using Value = Neo4Net.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.emptyResourceIterator;

	public class PopulatingIndexProxy : IndexProxy
	{
		 private readonly CapableIndexDescriptor _capableIndexDescriptor;
		 private readonly IndexPopulationJob _job;
		 private readonly MultipleIndexPopulator.IndexPopulation _indexPopulation;

		 internal PopulatingIndexProxy( CapableIndexDescriptor capableIndexDescriptor, IndexPopulationJob job, MultipleIndexPopulator.IndexPopulation indexPopulation )
		 {
			  this._capableIndexDescriptor = capableIndexDescriptor;
			  this._job = job;
			  this._indexPopulation = indexPopulation;
		 }

		 public override void Start()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.Neo4Net.kernel.api.index.IndexUpdater newUpdater(final IndexUpdateMode mode)
		 public override IndexUpdater NewUpdater( IndexUpdateMode mode )
		 {
			  switch ( mode.innerEnumValue )
			  {
					case Neo4Net.Kernel.Impl.Api.index.IndexUpdateMode.InnerEnum.ONLINE:
					case Neo4Net.Kernel.Impl.Api.index.IndexUpdateMode.InnerEnum.RECOVERY:
						 return new PopulatingIndexUpdaterAnonymousInnerClass( this );
					default:
						 return new PopulatingIndexUpdaterAnonymousInnerClass2( this, mode );
			  }
		 }

		 private class PopulatingIndexUpdaterAnonymousInnerClass : PopulatingIndexUpdater
		 {
			 private readonly PopulatingIndexProxy _outerInstance;

			 public PopulatingIndexUpdaterAnonymousInnerClass( PopulatingIndexProxy outerInstance ) : base( outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override void process<T1>( IndexEntryUpdate<T1> update )
			 {
				  _outerInstance.job.update( update );
			 }
		 }

		 private class PopulatingIndexUpdaterAnonymousInnerClass2 : PopulatingIndexUpdater
		 {
			 private readonly PopulatingIndexProxy _outerInstance;

			 private Neo4Net.Kernel.Impl.Api.index.IndexUpdateMode _mode;

			 public PopulatingIndexUpdaterAnonymousInnerClass2( PopulatingIndexProxy outerInstance, Neo4Net.Kernel.Impl.Api.index.IndexUpdateMode mode ) : base( outerInstance )
			 {
				 this.outerInstance = outerInstance;
				 this._mode = mode;
			 }

			 public override void process<T1>( IndexEntryUpdate<T1> update )
			 {
				  throw new System.ArgumentException( "Unsupported update mode: " + _mode );
			 }
		 }

		 public override void Drop()
		 {
			  _job.dropPopulation( _indexPopulation );
		 }

		 public virtual CapableIndexDescriptor Descriptor
		 {
			 get
			 {
				  return _capableIndexDescriptor;
			 }
		 }

		 public virtual InternalIndexState State
		 {
			 get
			 {
				  return InternalIndexState.POPULATING;
			 }
		 }

		 public override void Force( IOLimiter ioLimiter )
		 {
			  // Ignored... this isn't called from the outside while we're populating the index.
		 }

		 public override void Refresh()
		 {
			  // Ignored... this isn't called from the outside while we're populating the index.
		 }

		 public override void Close()
		 {
			  _job.cancelPopulation( _indexPopulation );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.storageengine.api.schema.IndexReader newReader() throws org.Neo4Net.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 public override IndexReader NewReader()
		 {
			  throw new IndexNotFoundKernelException( "Index is still populating: " + _job );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean awaitStoreScanCompleted(long time, java.util.concurrent.TimeUnit unit) throws InterruptedException
		 public override bool AwaitStoreScanCompleted( long time, TimeUnit unit )
		 {
			  return _job.awaitCompletion( time, unit );
		 }

		 public override void Activate()
		 {
			  throw new System.InvalidOperationException( "Cannot activate index while it is still populating: " + _job );
		 }

		 public override void Validate()
		 {
			  throw new System.InvalidOperationException( "Cannot validate index while it is still populating: " + _job );
		 }

		 public override void ValidateBeforeCommit( Value[] tuple )
		 {
			  // It's OK to put whatever values in while populating because it will take the natural path of failing the population.
		 }

		 public override ResourceIterator<File> SnapshotFiles()
		 {
			  return emptyResourceIterator();
		 }

		 public override IDictionary<string, Value> IndexConfig()
		 {
			  return _indexPopulation.populator.indexConfig();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public IndexPopulationFailure getPopulationFailure() throws IllegalStateException
		 public virtual IndexPopulationFailure PopulationFailure
		 {
			 get
			 {
				  throw new System.InvalidOperationException( this + " is POPULATING" );
			 }
		 }

		 public virtual PopulationProgress IndexPopulationProgress
		 {
			 get
			 {
				  return _job.getPopulationProgress( _indexPopulation );
			 }
		 }

		 public override string ToString()
		 {
			  return this.GetType().Name + "[job:" + _job + "]";
		 }

		 private abstract class PopulatingIndexUpdater : IndexUpdater
		 {
			 public abstract void process<T1>( IndexEntryUpdate<T1> update );
			 private readonly PopulatingIndexProxy _outerInstance;

			 public PopulatingIndexUpdater( PopulatingIndexProxy outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public override void Close()
			  {
			  }
		 }
	}

}