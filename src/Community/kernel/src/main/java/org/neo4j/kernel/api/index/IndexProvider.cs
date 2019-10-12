using System;
using System.Diagnostics;

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
namespace Neo4Net.Kernel.Api.Index
{

	using IndexCapability = Neo4Net.@internal.Kernel.Api.IndexCapability;
	using InternalIndexState = Neo4Net.@internal.Kernel.Api.InternalIndexState;
	using MisconfiguredIndexException = Neo4Net.@internal.Kernel.Api.exceptions.schema.MisconfiguredIndexException;
	using IndexProviderDescriptor = Neo4Net.@internal.Kernel.Api.schema.IndexProviderDescriptor;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using IndexingService = Neo4Net.Kernel.Impl.Api.index.IndexingService;
	using IndexSamplingConfig = Neo4Net.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using ByteBufferFactory = Neo4Net.Kernel.Impl.Index.Schema.ByteBufferFactory;
	using StoreMigrationParticipant = Neo4Net.Kernel.impl.storemigration.StoreMigrationParticipant;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;

	/// <summary>
	/// Contract for implementing an index in Neo4j.
	/// 
	/// This is a sensitive thing to implement, because it manages data that is controlled by
	/// Neo4js logical log. As such, the implementation needs to behave under some rather strict rules.
	/// 
	/// <h3>Populating the index</h3>
	/// 
	/// When an index rule is added, the <seealso cref="IndexingService"/> is notified. It will, in turn, ask
	/// your <seealso cref="IndexProvider"/> for a
	/// <seealso cref="getPopulator(StoreIndexDescriptor, IndexSamplingConfig, ByteBufferFactory) batch index writer"/>.
	/// 
	/// A background index job is triggered, and all existing data that applies to the new rule, as well as new data
	/// from the "outside", will be inserted using the writer. You are guaranteed that usage of this writer,
	/// during population, will be single threaded.
	/// 
	/// These are the rules you must adhere to here:
	/// 
	/// <ul>
	/// <li>You CANNOT say that the state of the index is <seealso cref="InternalIndexState.ONLINE"/></li>
	/// <li>You MUST store all updates given to you</li>
	/// <li>You MAY persistently store the updates</li>
	/// </ul>
	/// 
	/// 
	/// <h3>The Flip</h3>
	/// 
	/// Once population is done, the index needs to be "flipped" to an online mode of operation.
	/// 
	/// The index will be notified, through the <seealso cref="org.neo4j.kernel.api.index.IndexPopulator.close(bool)"/>
	/// method, that population is done, and that the index should turn it's state to <seealso cref="InternalIndexState.ONLINE"/> or
	/// <seealso cref="InternalIndexState.FAILED"/> depending on the value given to the
	/// <seealso cref="org.neo4j.kernel.api.index.IndexPopulator.close(bool) close method"/>.
	/// 
	/// If the index is persisted to disk, this is a <i>vital</i> part of the index lifecycle.
	/// For a persisted index, the index MUST NOT store the state as online unless it first guarantees that the entire index
	/// is flushed to disk. Failure to do so could produce a situation where, after a crash,
	/// an index is believed to be online when it in fact was not yet fully populated. This would break the database
	/// recovery process.
	/// 
	/// If you are implementing this interface, you can choose to not store index state. In that case,
	/// you should report index state as <seealso cref="InternalIndexState.POPULATING"/> upon startup.
	/// This will cause the database to re-create the index from scratch again.
	/// 
	/// These are the rules you must adhere to here:
	/// 
	/// <ul>
	/// <li>You MUST have flushed the index to durable storage if you are to persist index state as <seealso cref="InternalIndexState.ONLINE"/></li>
	/// <li>You MAY decide not to store index state</li>
	/// <li>If you don't store index state, you MUST default to <seealso cref="InternalIndexState.POPULATING"/></li>
	/// </ul>
	/// 
	/// <h3>Online operation</h3>
	/// 
	/// Once the index is online, the database will move to using the
	/// <seealso cref="getOnlineAccessor(StoreIndexDescriptor, IndexSamplingConfig) online accessor"/> to
	/// write to the index.
	/// </summary>
	public abstract class IndexProvider : LifecycleAdapter
	{
		 public interface Monitor
		 {

			  void FailedToOpenIndex( StoreIndexDescriptor schemaIndexDescriptor, string action, Exception cause );

			  void RecoveryCleanupRegistered( File indexFile, IndexDescriptor indexDescriptor );

			  void RecoveryCleanupStarted( File indexFile, IndexDescriptor indexDescriptor );

			  void RecoveryCleanupFinished( File indexFile, IndexDescriptor indexDescriptor, long numberOfPagesVisited, long numberOfCleanedCrashPointers, long durationMillis );

			  void RecoveryCleanupClosed( File indexFile, IndexDescriptor indexDescriptor );

			  void RecoveryCleanupFailed( File indexFile, IndexDescriptor indexDescriptor, Exception throwable );
		 }

		 public static class Monitor_Fields
		 {
			 private readonly IndexProvider _outerInstance;

			 public Monitor_Fields( IndexProvider outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public static readonly Monitor Empty = new Monitor_Adaptor( _outerInstance );
		 }

		  public class Monitor_Adaptor : Monitor
		  {
			  private readonly IndexProvider _outerInstance;

			  public Monitor_Adaptor( IndexProvider outerInstance )
			  {
				  this._outerInstance = outerInstance;
			  }

				public override void FailedToOpenIndex( StoreIndexDescriptor schemaIndexDescriptor, string action, Exception cause )
				{ // no-op
				}

				public override void RecoveryCleanupRegistered( File indexFile, IndexDescriptor indexDescriptor )
				{ // no-op
				}

				public override void RecoveryCleanupStarted( File indexFile, IndexDescriptor indexDescriptor )
				{ // no-op
				}

				public override void RecoveryCleanupFinished( File indexFile, IndexDescriptor indexDescriptor, long numberOfPagesVisited, long numberOfCleanedCrashPointers, long durationMillis )
				{ // no-op
				}

				public override void RecoveryCleanupClosed( File indexFile, IndexDescriptor indexDescriptor )
				{ // no-op
				}

				public override void RecoveryCleanupFailed( File indexFile, IndexDescriptor indexDescriptor, Exception throwable )
				{ // no-op
				}
		  }

		 public static readonly IndexProvider EMPTY = new IndexProviderAnonymousInnerClass( IndexDirectoryStructure.None );

		 private class IndexProviderAnonymousInnerClass : IndexProvider
		 {
			 public IndexProviderAnonymousInnerClass( Neo4Net.Kernel.Api.Index.IndexDirectoryStructure.Factory none ) : base( new IndexProviderDescriptor( "no-index-provider", "1.0" ), none )
			 {
			 }

			 private readonly IndexAccessor singleWriter = IndexAccessor_Fields.Empty;
			 private readonly IndexPopulator singlePopulator = IndexPopulator_Fields.Empty;

			 public override IndexAccessor getOnlineAccessor( StoreIndexDescriptor descriptor, IndexSamplingConfig samplingConfig )
			 {
				  return singleWriter;
			 }

			 public override IndexPopulator getPopulator( StoreIndexDescriptor descriptor, IndexSamplingConfig samplingConfig, ByteBufferFactory bufferFactory )
			 {
				  return singlePopulator;
			 }

			 public override InternalIndexState getInitialState( StoreIndexDescriptor descriptor )
			 {
				  return InternalIndexState.ONLINE;
			 }

			 public override IndexCapability getCapability( StoreIndexDescriptor descriptor )
			 {
				  return IndexCapability.NO_CAPABILITY;
			 }

			 public override StoreMigrationParticipant storeMigrationParticipant( FileSystemAbstraction fs, PageCache pageCache )
			 {
				  return Neo4Net.Kernel.impl.storemigration.StoreMigrationParticipant_Fields.NotParticipating;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public String getPopulationFailure(org.neo4j.storageengine.api.schema.StoreIndexDescriptor descriptor) throws IllegalStateException
			 public override string getPopulationFailure( StoreIndexDescriptor descriptor )
			 {
				  throw new System.InvalidOperationException();
			 }
		 }

		 private readonly IndexProviderDescriptor _providerDescriptor;
		 private readonly IndexDirectoryStructure.Factory _directoryStructureFactory;
		 private readonly IndexDirectoryStructure _directoryStructure;

		 protected internal IndexProvider( IndexProvider copySource ) : this( copySource._providerDescriptor, copySource._directoryStructureFactory )
		 {
		 }

		 protected internal IndexProvider( IndexProviderDescriptor descriptor, IndexDirectoryStructure.Factory directoryStructureFactory )
		 {
			  this._directoryStructureFactory = directoryStructureFactory;
			  Debug.Assert( descriptor != null );
			  this._providerDescriptor = descriptor;
			  this._directoryStructure = directoryStructureFactory.ForProvider( descriptor );
		 }

		 /// <summary>
		 /// Before an index is created, the chosen index provider will be asked to bless the index descriptor by calling this method, giving the index descriptor
		 /// as an argument. The returned index descriptor is then blessed, and will be used for creating the index. This gives the provider an opportunity to check
		 /// the index configuration, and make sure that it is sensible and support by this provider.
		 /// </summary>
		 /// <param name="index"> The index descriptor to bless. </param>
		 /// <returns> The blessed index descriptor that will be used for creating the index. </returns>
		 /// <exception cref="MisconfiguredIndexException"> if the index descriptor cannot be blessed by this provider for some reason. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.storageengine.api.schema.IndexDescriptor bless(org.neo4j.storageengine.api.schema.IndexDescriptor index) throws org.neo4j.internal.kernel.api.exceptions.schema.MisconfiguredIndexException
		 public virtual IndexDescriptor Bless( IndexDescriptor index )
		 {
			  // Normal schema indexes accept all configurations by default. More specialised or custom providers, such as the fulltext index provider,
			  // can override this method to do whatever checking suits their needs.
			  return index;
		 }

		 /// <summary>
		 /// Used for initially populating a created index, using batch insertion.
		 /// </summary>
		 public abstract IndexPopulator GetPopulator( StoreIndexDescriptor descriptor, IndexSamplingConfig samplingConfig, ByteBufferFactory bufferFactory );

		 /// <summary>
		 /// Used for updating an index once initial population has completed.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract IndexAccessor getOnlineAccessor(org.neo4j.storageengine.api.schema.StoreIndexDescriptor descriptor, org.neo4j.kernel.impl.api.index.sampling.IndexSamplingConfig samplingConfig) throws java.io.IOException;
		 public abstract IndexAccessor GetOnlineAccessor( StoreIndexDescriptor descriptor, IndexSamplingConfig samplingConfig );

		 /// <summary>
		 /// Returns a failure previously gotten from <seealso cref="IndexPopulator.markAsFailed(string)"/>
		 /// 
		 /// Implementations are expected to persist this failure </summary>
		 /// <param name="descriptor"> <seealso cref="StoreIndexDescriptor"/> of the index. </param>
		 /// <returns> failure, in the form of a stack trace, that happened during population. </returns>
		 /// <exception cref="IllegalStateException"> If there was no failure during population. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract String getPopulationFailure(org.neo4j.storageengine.api.schema.StoreIndexDescriptor descriptor) throws IllegalStateException;
		 public abstract string GetPopulationFailure( StoreIndexDescriptor descriptor );

		 /// <summary>
		 /// Called during startup to find out which state an index is in. If <seealso cref="InternalIndexState.FAILED"/>
		 /// is returned then a further call to <seealso cref="getPopulationFailure(StoreIndexDescriptor)"/> is expected and should return
		 /// the failure accepted by any call to <seealso cref="IndexPopulator.markAsFailed(string)"/> call at the time
		 /// of failure.
		 /// </summary>
		 public abstract InternalIndexState GetInitialState( StoreIndexDescriptor descriptor );

		 /// <summary>
		 /// Return <seealso cref="IndexCapability"/> for this index provider.
		 /// </summary>
		 /// <param name="descriptor"> The specific <seealso cref="StoreIndexDescriptor"/> to get the capabilities for, in case it matters. </param>
		 public abstract IndexCapability GetCapability( StoreIndexDescriptor descriptor );

		 /// <returns> a description of this index provider </returns>
		 public virtual IndexProviderDescriptor ProviderDescriptor
		 {
			 get
			 {
				  return _providerDescriptor;
			 }
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }

			  IndexProvider other = ( IndexProvider ) o;

			  return _providerDescriptor.Equals( other._providerDescriptor );
		 }

		 public override int GetHashCode()
		 {
			  return _providerDescriptor.GetHashCode();
		 }

		 /// <returns> <seealso cref="IndexDirectoryStructure"/> for this schema index provider. From it can be retrieved directories
		 /// for individual indexes. </returns>
		 public virtual IndexDirectoryStructure DirectoryStructure()
		 {
			  return _directoryStructure;
		 }

		 public abstract StoreMigrationParticipant StoreMigrationParticipant( FileSystemAbstraction fs, PageCache pageCache );

		 public class Adaptor : IndexProvider
		 {
			  protected internal Adaptor( IndexProviderDescriptor descriptor, IndexDirectoryStructure.Factory directoryStructureFactory ) : base( descriptor, directoryStructureFactory )
			  {
			  }

			  public override IndexPopulator GetPopulator( StoreIndexDescriptor descriptor, IndexSamplingConfig samplingConfig, ByteBufferFactory bufferFactory )
			  {
					return null;
			  }

			  public override IndexAccessor GetOnlineAccessor( StoreIndexDescriptor descriptor, IndexSamplingConfig samplingConfig )
			  {
					return null;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public String getPopulationFailure(org.neo4j.storageengine.api.schema.StoreIndexDescriptor descriptor) throws IllegalStateException
			  public override string GetPopulationFailure( StoreIndexDescriptor descriptor )
			  {
					return null;
			  }

			  public override InternalIndexState GetInitialState( StoreIndexDescriptor descriptor )
			  {
					return null;
			  }

			  public override IndexCapability GetCapability( StoreIndexDescriptor descriptor )
			  {
					return null;
			  }

			  public override StoreMigrationParticipant StoreMigrationParticipant( FileSystemAbstraction fs, PageCache pageCache )
			  {
					return null;
			  }
		 }
	}

}