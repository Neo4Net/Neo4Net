﻿using System.Collections.Generic;

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

	using Org.Neo4j.Graphdb;
	using InternalIndexState = Org.Neo4j.@internal.Kernel.Api.InternalIndexState;
	using IndexNotFoundKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using IOLimiter = Org.Neo4j.Io.pagecache.IOLimiter;
	using Org.Neo4j.Kernel.Api.Index;
	using IndexUpdater = Org.Neo4j.Kernel.Api.Index.IndexUpdater;
	using CapableIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.CapableIndexDescriptor;
	using IndexReader = Org.Neo4j.Storageengine.Api.schema.IndexReader;
	using PopulationProgress = Org.Neo4j.Storageengine.Api.schema.PopulationProgress;
	using Value = Org.Neo4j.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.emptyResourceIterator;

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
//ORIGINAL LINE: public org.neo4j.kernel.api.index.IndexUpdater newUpdater(final IndexUpdateMode mode)
		 public override IndexUpdater NewUpdater( IndexUpdateMode mode )
		 {
			  switch ( mode.innerEnumValue )
			  {
					case Org.Neo4j.Kernel.Impl.Api.index.IndexUpdateMode.InnerEnum.ONLINE:
					case Org.Neo4j.Kernel.Impl.Api.index.IndexUpdateMode.InnerEnum.RECOVERY:
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

			 private Org.Neo4j.Kernel.Impl.Api.index.IndexUpdateMode _mode;

			 public PopulatingIndexUpdaterAnonymousInnerClass2( PopulatingIndexProxy outerInstance, Org.Neo4j.Kernel.Impl.Api.index.IndexUpdateMode mode ) : base( outerInstance )
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
//ORIGINAL LINE: public org.neo4j.storageengine.api.schema.IndexReader newReader() throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
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