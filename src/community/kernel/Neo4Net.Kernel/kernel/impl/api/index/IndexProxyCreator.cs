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

	using TokenNameLookup = Neo4Net.Internal.Kernel.Api.TokenNameLookup;
	using IndexAccessor = Neo4Net.Kernel.Api.Index.IndexAccessor;
	using IndexPopulator = Neo4Net.Kernel.Api.Index.IndexPopulator;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using IndexSamplingConfig = Neo4Net.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using ByteBufferFactory = Neo4Net.Kernel.Impl.Index.Schema.ByteBufferFactory;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using CapableIndexDescriptor = Neo4Net.Storageengine.Api.schema.CapableIndexDescriptor;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.schema.ByteBufferFactory.heapBufferFactory;

	/// <summary>
	/// Helper class of <seealso cref="IndexingService"/>. Used mainly as factory of index proxies.
	/// </summary>
	internal class IndexProxyCreator
	{
		 private readonly IndexSamplingConfig _samplingConfig;
		 private readonly IndexStoreView _storeView;
		 private readonly IndexProviderMap _providerMap;
		 private readonly TokenNameLookup _tokenNameLookup;
		 private readonly LogProvider _logProvider;

		 internal IndexProxyCreator( IndexSamplingConfig samplingConfig, IndexStoreView storeView, IndexProviderMap providerMap, TokenNameLookup tokenNameLookup, LogProvider logProvider )
		 {
			  this._samplingConfig = samplingConfig;
			  this._storeView = storeView;
			  this._providerMap = providerMap;
			  this._tokenNameLookup = tokenNameLookup;
			  this._logProvider = logProvider;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: IndexProxy createPopulatingIndexProxy(final org.Neo4Net.storageengine.api.schema.StoreIndexDescriptor descriptor, final boolean flipToTentative, final IndexingService.Monitor monitor, final IndexPopulationJob populationJob)
		 internal virtual IndexProxy CreatePopulatingIndexProxy( StoreIndexDescriptor descriptor, bool flipToTentative, IndexingService.Monitor monitor, IndexPopulationJob populationJob )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final FlippableIndexProxy flipper = new FlippableIndexProxy();
			  FlippableIndexProxy flipper = new FlippableIndexProxy();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String indexUserDescription = indexUserDescription(descriptor);
			  string indexUserDescription = indexUserDescription( descriptor );
			  IndexPopulator populator = PopulatorFromProvider( descriptor, _samplingConfig, populationJob.BufferFactory() );
			  CapableIndexDescriptor capableIndexDescriptor = _providerMap.withCapabilities( descriptor );

			  FailedIndexProxyFactory failureDelegateFactory = new FailedPopulatingIndexProxyFactory( capableIndexDescriptor, populator, indexUserDescription, new IndexCountsRemover( _storeView, descriptor.Id ), _logProvider );

			  MultipleIndexPopulator.IndexPopulation indexPopulation = populationJob.AddPopulator( populator, capableIndexDescriptor, indexUserDescription, flipper, failureDelegateFactory );
			  PopulatingIndexProxy populatingIndex = new PopulatingIndexProxy( capableIndexDescriptor, populationJob, indexPopulation );

			  flipper.FlipTo( populatingIndex );

			  // Prepare for flipping to online mode
			  flipper.FlipTarget = () =>
			  {
				monitor.PopulationCompleteOn( descriptor );
				IndexAccessor accessor = OnlineAccessorFromProvider( descriptor, _samplingConfig );
				OnlineIndexProxy onlineProxy = new OnlineIndexProxy( capableIndexDescriptor, accessor, _storeView, true );
				if ( flipToTentative )
				{
					 return new TentativeConstraintIndexProxy( flipper, onlineProxy );
				}
				return onlineProxy;
			  };

			  return new ContractCheckingIndexProxy( flipper, false );
		 }

		 internal virtual IndexProxy CreateRecoveringIndexProxy( StoreIndexDescriptor descriptor )
		 {
			  CapableIndexDescriptor capableIndexDescriptor = _providerMap.withCapabilities( descriptor );
			  IndexProxy proxy = new RecoveringIndexProxy( capableIndexDescriptor );
			  return new ContractCheckingIndexProxy( proxy, true );
		 }

		 internal virtual IndexProxy CreateOnlineIndexProxy( StoreIndexDescriptor descriptor )
		 {
			  try
			  {
					IndexAccessor onlineAccessor = OnlineAccessorFromProvider( descriptor, _samplingConfig );
					CapableIndexDescriptor capableIndexDescriptor = _providerMap.withCapabilities( descriptor );
					IndexProxy proxy;
					proxy = new OnlineIndexProxy( capableIndexDescriptor, onlineAccessor, _storeView, false );
					proxy = new ContractCheckingIndexProxy( proxy, true );
					return proxy;
			  }
			  catch ( IOException e )
			  {
					_logProvider.getLog( this.GetType() ).error("Failed to open index: " + descriptor.Id + " (" + descriptor.UserDescription(_tokenNameLookup) + "), requesting re-population.", e);
					return CreateRecoveringIndexProxy( descriptor );
			  }
		 }

		 internal virtual IndexProxy CreateFailedIndexProxy( StoreIndexDescriptor descriptor, IndexPopulationFailure populationFailure )
		 {
			  // Note about the buffer factory instantiation here. Question is why an index populator is instantiated for a failed index proxy to begin with.
			  // The byte buffer factory should not be used here anyway so the buffer size doesn't actually matter.
			  IndexPopulator indexPopulator = PopulatorFromProvider( descriptor, _samplingConfig, heapBufferFactory( 1024 ) );
			  CapableIndexDescriptor capableIndexDescriptor = _providerMap.withCapabilities( descriptor );
			  string indexUserDescription = indexUserDescription( descriptor );
			  IndexProxy proxy;
			  proxy = new FailedIndexProxy( capableIndexDescriptor, indexUserDescription, indexPopulator, populationFailure, new IndexCountsRemover( _storeView, descriptor.Id ), _logProvider );
			  proxy = new ContractCheckingIndexProxy( proxy, true );
			  return proxy;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private String indexUserDescription(final org.Neo4Net.storageengine.api.schema.StoreIndexDescriptor descriptor)
		 private string IndexUserDescription( StoreIndexDescriptor descriptor )
		 {
			  return format( "%s [provider: %s]", descriptor.Schema().userDescription(_tokenNameLookup), descriptor.ProviderDescriptor().ToString() );
		 }

		 private IndexPopulator PopulatorFromProvider( StoreIndexDescriptor descriptor, IndexSamplingConfig samplingConfig, ByteBufferFactory bufferFactory )
		 {
			  IndexProvider indexProvider = _providerMap.lookup( descriptor.ProviderDescriptor() );
			  return indexProvider.GetPopulator( descriptor, samplingConfig, bufferFactory );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.kernel.api.index.IndexAccessor onlineAccessorFromProvider(org.Neo4Net.storageengine.api.schema.StoreIndexDescriptor descriptor, org.Neo4Net.kernel.impl.api.index.sampling.IndexSamplingConfig samplingConfig) throws java.io.IOException
		 private IndexAccessor OnlineAccessorFromProvider( StoreIndexDescriptor descriptor, IndexSamplingConfig samplingConfig )
		 {
			  IndexProvider indexProvider = _providerMap.lookup( descriptor.ProviderDescriptor() );
			  return indexProvider.GetOnlineAccessor( descriptor, samplingConfig );
		 }
	}

}