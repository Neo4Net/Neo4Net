using System;
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
namespace Neo4Net.Kernel.impl.index
{

	using Node = Neo4Net.Graphdb.Node;
	using PropertyContainer = Neo4Net.Graphdb.PropertyContainer;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using TransactionFailureException = Neo4Net.Graphdb.TransactionFailureException;
	using MapUtil = Neo4Net.Helpers.Collection.MapUtil;
	using Kernel = Neo4Net.@internal.Kernel.Api.Kernel;
	using Transaction = Neo4Net.@internal.Kernel.Api.Transaction;
	using ExplicitIndexNotFoundKernelException = Neo4Net.@internal.Kernel.Api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using Statement = Neo4Net.Kernel.api.Statement;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ExplicitIndexProvider = Neo4Net.Kernel.Impl.Api.ExplicitIndexProvider;
	using IndexImplementation = Neo4Net.Kernel.spi.explicitindex.IndexImplementation;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.index.IndexManager_Fields.PROVIDER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.security.LoginContext.AUTH_DISABLED;

	/// <summary>
	/// Uses an <seealso cref="IndexConfigStore"/> and puts logic around providers and configuration comparison.
	/// </summary>
	public class ExplicitIndexStore
	{
		 private readonly IndexConfigStore _indexStore;
		 private readonly Config _config;
		 private readonly ExplicitIndexProvider _explicitIndexProvider;
		 private readonly System.Func<Kernel> _kernel;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public ExplicitIndexStore(@Nonnull Config config, IndexConfigStore indexStore, java.util.function.Supplier<org.neo4j.internal.kernel.api.Kernel> kernel, org.neo4j.kernel.impl.api.ExplicitIndexProvider defaultExplicitIndexProvider)
		 public ExplicitIndexStore( Config config, IndexConfigStore indexStore, System.Func<Kernel> kernel, ExplicitIndexProvider defaultExplicitIndexProvider )
		 {
			  this._config = config;
			  this._indexStore = indexStore;
			  this._kernel = kernel;
			  this._explicitIndexProvider = defaultExplicitIndexProvider;
		 }

		 public virtual IDictionary<string, string> GetOrCreateNodeIndexConfig( string indexName, IDictionary<string, string> customConfiguration )
		 {
			  return GetOrCreateIndexConfig( IndexEntityType.Node, indexName, customConfiguration );
		 }

		 public virtual IDictionary<string, string> GetOrCreateRelationshipIndexConfig( string indexName, IDictionary<string, string> customConfiguration )
		 {
			  return GetOrCreateIndexConfig( IndexEntityType.Relationship, indexName, customConfiguration );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: private java.util.Map<String, String> findIndexConfig(Class cls, String indexName, java.util.Map<String, String> suppliedConfig, @Nonnull Config dbConfig)
		 private IDictionary<string, string> FindIndexConfig( Type cls, string indexName, IDictionary<string, string> suppliedConfig, Config dbConfig )
		 {
			  // Check stored config (has this index been created previously?)
			  IDictionary<string, string> storedConfig = _indexStore.get( cls, indexName );
			  if ( storedConfig != null && suppliedConfig == null )
			  {
					// Fill in "provider" if not already filled in, backwards compatibility issue
					IDictionary<string, string> newConfig = InjectDefaultProviderIfMissing( indexName, dbConfig, storedConfig );
					if ( newConfig != storedConfig )
					{
						 _indexStore.set( cls, indexName, newConfig );
					}
					return newConfig;
			  }

			  IDictionary<string, string> configToUse = suppliedConfig;

			  // Check db config properties for provider
			  string provider;
			  IndexImplementation indexProvider;
			  if ( configToUse == null )
			  {
					provider = GetDefaultProvider( indexName, dbConfig );
					configToUse = MapUtil.stringMap( PROVIDER, provider );
			  }
			  else
			  {
					provider = configToUse[PROVIDER];
					provider = string.ReferenceEquals( provider, null ) ? GetDefaultProvider( indexName, dbConfig ) : provider;
			  }
			  indexProvider = _explicitIndexProvider.getProviderByName( provider );
			  configToUse = indexProvider.FillInDefaults( configToUse );
			  configToUse = InjectDefaultProviderIfMissing( indexName, dbConfig, configToUse );

			  // Do they match (stored vs. supplied)?
			  if ( storedConfig != null )
			  {
					AssertConfigMatches( indexProvider, indexName, storedConfig, suppliedConfig );
					// Fill in "provider" if not already filled in, backwards compatibility issue
					IDictionary<string, string> newConfig = InjectDefaultProviderIfMissing( indexName, dbConfig, storedConfig );
					if ( newConfig != storedConfig )
					{
						 _indexStore.set( cls, indexName, newConfig );
					}
					configToUse = newConfig;
			  }

			  return Collections.unmodifiableMap( configToUse );
		 }

		 public static void AssertConfigMatches( IndexImplementation indexProvider, string indexName, IDictionary<string, string> storedConfig, IDictionary<string, string> suppliedConfig )
		 {
			  if ( suppliedConfig != null && !indexProvider.ConfigMatches( storedConfig, suppliedConfig ) )
			  {
					throw new System.ArgumentException( "Supplied index configuration:\n" + suppliedConfig + "\ndoesn't match stored config in a valid way:\n" + storedConfig + "\nfor '" + indexName + "'" );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull private java.util.Map<String, String> injectDefaultProviderIfMissing(@Nullable String indexName, @Nonnull Config dbConfig, @Nonnull Map<String, String> config)
		 private IDictionary<string, string> InjectDefaultProviderIfMissing( string indexName, Config dbConfig, IDictionary<string, string> config )
		 {
			  string provider = config.get( PROVIDER );
			  if ( string.ReferenceEquals( provider, null ) )
			  {
					config = new Dictionary<string, string>( config );
					config.put( PROVIDER, GetDefaultProvider( indexName, dbConfig ) );
			  }
			  return config;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull private String getDefaultProvider(@Nullable String indexName, @Nonnull Config dbConfig)
		 private string GetDefaultProvider( string indexName, Config dbConfig )
		 {
			  return dbConfig.getRaw( "index." + indexName ).orElseGet( () => dbConfig.getRaw("index").orElse("lucene") );
		 }

		 private IDictionary<string, string> GetOrCreateIndexConfig( IndexEntityType entityType, string indexName, IDictionary<string, string> suppliedConfig )
		 {
			  IDictionary<string, string> config = FindIndexConfig( entityType.entityClass(), indexName, suppliedConfig, this._config );
			  if ( !_indexStore.has( entityType.entityClass(), indexName ) )
			  { // Ok, we need to create this config
					lock ( this )
					{ // Were we the first ones to get here?
						 IDictionary<string, string> existing = _indexStore.get( entityType.entityClass(), indexName );
						 if ( existing != null )
						 {
							  // No, someone else made it before us, cool
							  AssertConfigMatches( _explicitIndexProvider.getProviderByName( existing[PROVIDER] ), indexName, existing, config );
							  return config;
						 }

						 // We were the first one here, let's create this config
						 try
						 {
								 using ( Transaction transaction = _kernel.get().beginTransaction(Neo4Net.@internal.Kernel.Api.Transaction_Type.Implicit, AUTH_DISABLED), Statement statement = ((KernelTransaction)transaction).acquireStatement() )
								 {
								  switch ( entityType.innerEnumValue )
								  {
								  case Node:
										transaction.IndexWrite().nodeExplicitIndexCreate(indexName, config);
										break;
      
								  case Relationship:
										transaction.IndexWrite().relationshipExplicitIndexCreate(indexName, config);
										break;
      
								  default:
										throw new System.ArgumentException( "Unknown entity type: " + entityType );
								  }
      
								  transaction.Success();
								 }
						 }
						 catch ( Exception ex )
						 {
							  throw new TransactionFailureException( "Index creation failed for " + indexName + ", " + config, ex );
						 }
					}
			  }
			  return config;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public String setNodeIndexConfiguration(String indexName, String key, String value) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 public virtual string SetNodeIndexConfiguration( string indexName, string key, string value )
		 {
			  AssertLegalConfigKey( key );
			  IDictionary<string, string> config = new Dictionary<string, string>( GetNodeIndexConfiguration( indexName ) );
			  string oldValue = config[key] = value;
			  _indexStore.set( typeof( Node ), indexName, config );
			  return oldValue;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public String setRelationshipIndexConfiguration(String indexName, String key, String value) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 public virtual string SetRelationshipIndexConfiguration( string indexName, string key, string value )
		 {
			  AssertLegalConfigKey( key );
			  IDictionary<string, string> config = new Dictionary<string, string>( GetRelationshipIndexConfiguration( indexName ) );
			  string oldValue = config[key] = value;
			  _indexStore.set( typeof( Relationship ), indexName, config );
			  return oldValue;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public String removeNodeIndexConfiguration(String indexName, String key) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 public virtual string RemoveNodeIndexConfiguration( string indexName, string key )
		 {
			  AssertLegalConfigKey( key );
			  IDictionary<string, string> config = new Dictionary<string, string>( GetNodeIndexConfiguration( indexName ) );
			  string value = config.Remove( key );
			  if ( !string.ReferenceEquals( value, null ) )
			  {
					_indexStore.set( typeof( Node ), indexName, config );
			  }
			  return value;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public String removeRelationshipIndexConfiguration(String indexName, String key) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 public virtual string RemoveRelationshipIndexConfiguration( string indexName, string key )
		 {
			  AssertLegalConfigKey( key );
			  IDictionary<string, string> config = new Dictionary<string, string>( GetRelationshipIndexConfiguration( indexName ) );
			  string value = config.Remove( key );
			  if ( !string.ReferenceEquals( value, null ) )
			  {
					_indexStore.set( typeof( Relationship ), indexName, config );
			  }
			  return value;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.Map<String, String> getNodeIndexConfiguration(String indexName) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 public virtual IDictionary<string, string> GetNodeIndexConfiguration( string indexName )
		 {
			  IDictionary<string, string> config = _indexStore.get( typeof( Node ), indexName );
			  if ( config == null )
			  {
					throw new ExplicitIndexNotFoundKernelException( "No node index '" + indexName + "' found" );
			  }
			  return config;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.Map<String, String> getRelationshipIndexConfiguration(String indexName) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 public virtual IDictionary<string, string> GetRelationshipIndexConfiguration( string indexName )
		 {
			  IDictionary<string, string> config = _indexStore.get( typeof( Relationship ), indexName );
			  if ( config == null )
			  {
					throw new ExplicitIndexNotFoundKernelException( "No relationship index '" + indexName + "' found" );
			  }
			  return config;
		 }

		 private void AssertLegalConfigKey( string key )
		 {
			  if ( key.Equals( PROVIDER ) )
			  {
					throw new System.ArgumentException( "'" + key + "' cannot be modified" );
			  }
		 }

		 public virtual string[] AllNodeIndexNames
		 {
			 get
			 {
				  return _indexStore.getNames( typeof( Node ) );
			 }
		 }

		 public virtual string[] AllRelationshipIndexNames
		 {
			 get
			 {
				  return _indexStore.getNames( typeof( Relationship ) );
			 }
		 }
	}

}