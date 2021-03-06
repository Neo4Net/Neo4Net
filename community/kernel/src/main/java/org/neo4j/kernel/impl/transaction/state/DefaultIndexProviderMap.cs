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
namespace Org.Neo4j.Kernel.impl.transaction.state
{

	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using IndexProviderDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.IndexProviderDescriptor;
	using IndexProvider = Org.Neo4j.Kernel.Api.Index.IndexProvider;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using IndexProviderMap = Org.Neo4j.Kernel.Impl.Api.index.IndexProviderMap;
	using IndexProviderNotFoundException = Org.Neo4j.Kernel.Impl.Api.index.IndexProviderNotFoundException;
	using LifecycleAdapter = Org.Neo4j.Kernel.Lifecycle.LifecycleAdapter;


	public class DefaultIndexProviderMap : LifecycleAdapter, IndexProviderMap
	{
		 private readonly IDictionary<IndexProviderDescriptor, IndexProvider> _indexProvidersByDescriptor = new Dictionary<IndexProviderDescriptor, IndexProvider>();
		 private readonly IDictionary<string, IndexProvider> _indexProvidersByName = new Dictionary<string, IndexProvider>();
		 private readonly DependencyResolver _dependencies;
		 private IndexProvider _defaultIndexProvider;
		 private readonly Config _config;

		 public DefaultIndexProviderMap( DependencyResolver dependencies, Config config )
		 {
			  this._dependencies = dependencies;
			  this._config = config;
		 }

		 public override void Init()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Iterable<? extends org.neo4j.kernel.api.index.IndexProvider> indexProviders = dependencies.resolveTypeDependencies(org.neo4j.kernel.api.index.IndexProvider.class);
			  IEnumerable<IndexProvider> indexProviders = _dependencies.resolveTypeDependencies( typeof( IndexProvider ) );
			  foreach ( IndexProvider provider in indexProviders )
			  {
					IndexProviderDescriptor providerDescriptor = provider.ProviderDescriptor;
					requireNonNull( providerDescriptor );
					IndexProvider existing = Put( providerDescriptor, provider );
					if ( existing != null )
					{
						 throw new System.ArgumentException( "Tried to load multiple schema index providers with the same provider descriptor " + providerDescriptor + ". First loaded " + existing + " then " + provider );
					}
			  }
			  InitDefaultProvider();
		 }

		 public virtual IndexProvider DefaultProvider
		 {
			 get
			 {
				  AssertInit();
				  return _defaultIndexProvider;
			 }
		 }

		 public override IndexProvider Lookup( IndexProviderDescriptor providerDescriptor )
		 {
			  AssertInit();
			  IndexProvider provider = _indexProvidersByDescriptor[providerDescriptor];
			  AssertProviderFound( provider, providerDescriptor.Name() );
			  return provider;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.kernel.api.index.IndexProvider lookup(String providerDescriptorName) throws org.neo4j.kernel.impl.api.index.IndexProviderNotFoundException
		 public override IndexProvider Lookup( string providerDescriptorName )
		 {
			  AssertInit();
			  IndexProvider provider = _indexProvidersByName[providerDescriptorName];
			  AssertProviderFound( provider, providerDescriptorName );
			  return provider;
		 }

		 public override void Accept( System.Action<IndexProvider> visitor )
		 {
			  AssertInit();
			  _indexProvidersByDescriptor.Values.forEach( visitor );
		 }

		 private void AssertProviderFound( IndexProvider provider, string providerDescriptorName )
		 {
			  if ( provider == null )
			  {
					throw new IndexProviderNotFoundException( "Tried to get index provider with name " + providerDescriptorName + " whereas available providers in this session being " + _indexProvidersByName.Keys + ", and default being " + _defaultIndexProvider.ProviderDescriptor.name() );
			  }
		 }

		 private void AssertInit()
		 {
			  if ( _defaultIndexProvider == null )
			  {
					throw new System.InvalidOperationException( "DefaultIndexProviderMap must be part of life cycle and initialized before getting providers." );
			  }
		 }

		 private void InitDefaultProvider()
		 {
			  string providerName = _config.get( GraphDatabaseSettings.default_schema_provider );
			  IndexProvider configuredDefaultProvider = _indexProvidersByName[providerName];
			  requireNonNull( configuredDefaultProvider, () => format("Configured default provider: `%s` not found. Available index providers: %s.", providerName, _indexProvidersByName.Keys.ToString()) );
			  _defaultIndexProvider = configuredDefaultProvider;
		 }

		 private IndexProvider Put( IndexProviderDescriptor providerDescriptor, IndexProvider provider )
		 {
			  IndexProvider existing = _indexProvidersByDescriptor.putIfAbsent( providerDescriptor, provider );
			  if ( !_indexProvidersByName.ContainsKey( providerDescriptor.Name() ) ) _indexProvidersByName.Add(providerDescriptor.Name(), provider);
			  return existing;
		 }
	}

}