using System.Collections.Generic;

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
namespace Neo4Net.Kernel.Impl.Api.index
{

	using Neo4Net.Graphdb;
	using InternalIndexState = Neo4Net.@internal.Kernel.Api.InternalIndexState;
	using IndexNotFoundKernelException = Neo4Net.@internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using IndexActivationFailedKernelException = Neo4Net.Kernel.Api.Exceptions.index.IndexActivationFailedKernelException;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using IndexPopulationFailedKernelException = Neo4Net.Kernel.Api.Exceptions.index.IndexPopulationFailedKernelException;
	using UniquePropertyValueValidationException = Neo4Net.Kernel.Api.Exceptions.schema.UniquePropertyValueValidationException;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using NodePropertyAccessor = Neo4Net.Storageengine.Api.NodePropertyAccessor;
	using CapableIndexDescriptor = Neo4Net.Storageengine.Api.schema.CapableIndexDescriptor;
	using IndexReader = Neo4Net.Storageengine.Api.schema.IndexReader;
	using PopulationProgress = Neo4Net.Storageengine.Api.schema.PopulationProgress;
	using Value = Neo4Net.Values.Storable.Value;

	public abstract class AbstractDelegatingIndexProxy : IndexProxy
	{
		public abstract void PutAllNoOverwrite( IDictionary<string, Value> target, IDictionary<string, Value> source );
		 protected internal abstract IndexProxy Delegate { get; }

		 public override void Start()
		 {
			  Delegate.start();
		 }

		 public override IndexUpdater NewUpdater( IndexUpdateMode mode )
		 {
			  return Delegate.newUpdater( mode );
		 }

		 public override void Drop()
		 {
			  Delegate.drop();
		 }

		 public virtual InternalIndexState State
		 {
			 get
			 {
				  return Delegate.State;
			 }
		 }

		 public virtual CapableIndexDescriptor Descriptor
		 {
			 get
			 {
				  return Delegate.Descriptor;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void force(org.neo4j.io.pagecache.IOLimiter ioLimiter) throws java.io.IOException
		 public override void Force( IOLimiter ioLimiter )
		 {
			  Delegate.force( ioLimiter );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void refresh() throws java.io.IOException
		 public override void Refresh()
		 {
			  Delegate.refresh();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  Delegate.close();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.storageengine.api.schema.IndexReader newReader() throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 public override IndexReader NewReader()
		 {
			  return Delegate.newReader();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean awaitStoreScanCompleted(long time, java.util.concurrent.TimeUnit unit) throws org.neo4j.kernel.api.exceptions.index.IndexPopulationFailedKernelException, InterruptedException
		 public override bool AwaitStoreScanCompleted( long time, TimeUnit unit )
		 {
			  return Delegate.awaitStoreScanCompleted( time, unit );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void activate() throws org.neo4j.kernel.api.exceptions.index.IndexActivationFailedKernelException
		 public override void Activate()
		 {
			  Delegate.activate();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void validate() throws org.neo4j.kernel.api.exceptions.index.IndexPopulationFailedKernelException, org.neo4j.kernel.api.exceptions.schema.UniquePropertyValueValidationException
		 public override void Validate()
		 {
			  Delegate.validate();
		 }

		 public override void ValidateBeforeCommit( Value[] tuple )
		 {
			  Delegate.validateBeforeCommit( tuple );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public IndexPopulationFailure getPopulationFailure() throws IllegalStateException
		 public virtual IndexPopulationFailure PopulationFailure
		 {
			 get
			 {
				  return Delegate.PopulationFailure;
			 }
		 }

		 public virtual PopulationProgress IndexPopulationProgress
		 {
			 get
			 {
				  return Delegate.IndexPopulationProgress;
			 }
		 }

		 public override string ToString()
		 {
			  return string.Format( "{0} -> {1}", this.GetType().Name, Delegate.ToString() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.graphdb.ResourceIterator<java.io.File> snapshotFiles() throws java.io.IOException
		 public override ResourceIterator<File> SnapshotFiles()
		 {
			  return Delegate.snapshotFiles();
		 }

		 public override IDictionary<string, Value> IndexConfig()
		 {
			  return Delegate.indexConfig();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void verifyDeferredConstraints(org.neo4j.storageengine.api.NodePropertyAccessor accessor) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException, java.io.IOException
		 public override void VerifyDeferredConstraints( NodePropertyAccessor accessor )
		 {
			  Delegate.verifyDeferredConstraints( accessor );
		 }
	}

}