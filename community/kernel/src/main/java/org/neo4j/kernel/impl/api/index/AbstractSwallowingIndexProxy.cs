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
namespace Org.Neo4j.Kernel.Impl.Api.index
{
	using IOLimiter = Org.Neo4j.Io.pagecache.IOLimiter;
	using IndexUpdater = Org.Neo4j.Kernel.Api.Index.IndexUpdater;
	using SwallowingIndexUpdater = Org.Neo4j.Kernel.Impl.Api.index.updater.SwallowingIndexUpdater;
	using CapableIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.CapableIndexDescriptor;
	using IndexReader = Org.Neo4j.Storageengine.Api.schema.IndexReader;
	using PopulationProgress = Org.Neo4j.Storageengine.Api.schema.PopulationProgress;

	public abstract class AbstractSwallowingIndexProxy : IndexProxy
	{
		public abstract void PutAllNoOverwrite( IDictionary<string, Org.Neo4j.Values.Storable.Value> target, IDictionary<string, Org.Neo4j.Values.Storable.Value> source );
		public abstract IDictionary<string, Org.Neo4j.Values.Storable.Value> IndexConfig();
		public abstract void VerifyDeferredConstraints( Org.Neo4j.Storageengine.Api.NodePropertyAccessor accessor );
		public abstract Org.Neo4j.Graphdb.ResourceIterator<java.io.File> SnapshotFiles();
		public abstract void ValidateBeforeCommit( Org.Neo4j.Values.Storable.Value[] tuple );
		public abstract void Validate();
		public abstract void Activate();
		public abstract bool AwaitStoreScanCompleted( long time, java.util.concurrent.TimeUnit unit );
		public abstract Org.Neo4j.@internal.Kernel.Api.InternalIndexState State { get; }
		public abstract void Drop();
		 private readonly CapableIndexDescriptor _capableIndexDescriptor;
		 private readonly IndexPopulationFailure _populationFailure;

		 internal AbstractSwallowingIndexProxy( CapableIndexDescriptor capableIndexDescriptor, IndexPopulationFailure populationFailure )
		 {
			  this._capableIndexDescriptor = capableIndexDescriptor;
			  this._populationFailure = populationFailure;
		 }

		 public virtual IndexPopulationFailure PopulationFailure
		 {
			 get
			 {
				  return _populationFailure;
			 }
		 }

		 public virtual PopulationProgress IndexPopulationProgress
		 {
			 get
			 {
				  return Org.Neo4j.Storageengine.Api.schema.PopulationProgress_Fields.None;
			 }
		 }

		 public override void Start()
		 {
			  string message = "Unable to start index, it is in a " + State.name() + " state.";
			  throw new System.NotSupportedException( message + ", caused by: " + PopulationFailure );
		 }

		 public override IndexUpdater NewUpdater( IndexUpdateMode mode )
		 {
			  return SwallowingIndexUpdater.INSTANCE;
		 }

		 public override void Force( IOLimiter ioLimiter )
		 {
		 }

		 public override void Refresh()
		 {
		 }

		 public virtual CapableIndexDescriptor Descriptor
		 {
			 get
			 {
				  return _capableIndexDescriptor;
			 }
		 }

		 public override void Close()
		 {
		 }

		 public override IndexReader NewReader()
		 {
			  throw new System.NotSupportedException();
		 }
	}

}