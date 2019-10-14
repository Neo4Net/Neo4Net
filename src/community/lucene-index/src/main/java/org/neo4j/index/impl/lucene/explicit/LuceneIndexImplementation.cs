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
namespace Neo4Net.Index.impl.lucene.@explicit
{

	using Neo4Net.Graphdb;
	using IndexManager = Neo4Net.Graphdb.index.IndexManager;
	using MapUtil = Neo4Net.Helpers.Collections.MapUtil;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using Config = Neo4Net.Kernel.configuration.Config;
	using TransactionApplier = Neo4Net.Kernel.Impl.Api.TransactionApplier;
	using OperationalMode = Neo4Net.Kernel.impl.factory.OperationalMode;
	using IndexConfigStore = Neo4Net.Kernel.impl.index.IndexConfigStore;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using ExplicitIndexProviderTransaction = Neo4Net.Kernel.spi.explicitindex.ExplicitIndexProviderTransaction;
	using IndexCommandFactory = Neo4Net.Kernel.spi.explicitindex.IndexCommandFactory;
	using IndexImplementation = Neo4Net.Kernel.spi.explicitindex.IndexImplementation;

	public class LuceneIndexImplementation : LifecycleAdapter, IndexImplementation
	{
		 internal const string KEY_TYPE = "type";
		 internal const string KEY_ANALYZER = "analyzer";
		 internal const string KEY_TO_LOWER_CASE = "to_lower_case";
		 internal const string KEY_SIMILARITY = "similarity";
		 public const string SERVICE_NAME = "lucene";

		 public static readonly IDictionary<string, string> ExactConfig = Collections.unmodifiableMap( MapUtil.stringMap( Neo4Net.Graphdb.index.IndexManager_Fields.PROVIDER, SERVICE_NAME, KEY_TYPE, "exact" ) );

		 public static readonly IDictionary<string, string> FulltextConfig = Collections.unmodifiableMap( MapUtil.stringMap( Neo4Net.Graphdb.index.IndexManager_Fields.PROVIDER, SERVICE_NAME, KEY_TYPE, "fulltext", KEY_TO_LOWER_CASE, "true" ) );

		 private LuceneDataSource _dataSource;
		 private readonly DatabaseLayout _databaseLayout;
		 private readonly Config _config;
		 private readonly System.Func<IndexConfigStore> _indexStore;
		 private readonly FileSystemAbstraction _fileSystemAbstraction;
		 private readonly OperationalMode _operationalMode;

		 public LuceneIndexImplementation( DatabaseLayout databaseLayout, Config config, System.Func<IndexConfigStore> indexStore, FileSystemAbstraction fileSystemAbstraction, OperationalMode operationalMode )
		 {
			  this._databaseLayout = databaseLayout;
			  this._config = config;
			  this._indexStore = indexStore;
			  this._fileSystemAbstraction = fileSystemAbstraction;
			  this._operationalMode = operationalMode;
		 }

		 public override void Init()
		 {
			  this._dataSource = new LuceneDataSource( _databaseLayout, _config, _indexStore.get(), _fileSystemAbstraction, _operationalMode );
			  this._dataSource.init();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start() throws Throwable
		 public override void Start()
		 {
			  this._dataSource.start();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void stop() throws Throwable
		 public override void Stop()
		 {
			  this._dataSource.stop();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void shutdown() throws Throwable
		 public override void Shutdown()
		 {
			  this._dataSource.shutdown();
			  this._dataSource = null;
		 }

		 public override File GetIndexImplementationDirectory( DatabaseLayout directoryLayout )
		 {
			  return LuceneDataSource.GetLuceneIndexStoreDirectory( directoryLayout );
		 }

		 public override ExplicitIndexProviderTransaction NewTransaction( IndexCommandFactory commandFactory )
		 {
			  return new LuceneExplicitIndexTransaction( _dataSource, commandFactory );
		 }

		 public override IDictionary<string, string> FillInDefaults( IDictionary<string, string> source )
		 {
			  IDictionary<string, string> result = source != null ? new Dictionary<string, string>( source ) : new Dictionary<string, string>();
			  string analyzer = result[KEY_ANALYZER];
			  if ( string.ReferenceEquals( analyzer, null ) )
			  {
					// Type is only considered if "analyzer" isn't supplied
					string type = result.computeIfAbsent( KEY_TYPE, k => "exact" );
					if ( type.Equals( "fulltext" ) && !result.ContainsKey( LuceneIndexImplementation.KEY_TO_LOWER_CASE ) )
					{
						 result[KEY_TO_LOWER_CASE] = "true";
					}
			  }

			  // Try it on for size. Calling this will reveal configuration problems.
			  IndexType.GetIndexType( result );

			  return result;
		 }

		 public override bool ConfigMatches( IDictionary<string, string> storedConfig, IDictionary<string, string> config )
		 {
			  return Match( storedConfig, config, KEY_TYPE, null ) && Match( storedConfig, config, KEY_TO_LOWER_CASE, "true" ) && Match( storedConfig, config, KEY_ANALYZER, null ) && Match( storedConfig, config, KEY_SIMILARITY, null );
		 }

		 private bool Match( IDictionary<string, string> storedConfig, IDictionary<string, string> config, string key, string defaultValue )
		 {
			  string value1 = storedConfig[key];
			  string value2 = config[key];
			  if ( string.ReferenceEquals( value1, null ) || string.ReferenceEquals( value2, null ) )
			  {
					if ( string.ReferenceEquals( value1, value2 ) )
					{
						 return true;
					}
					if ( !string.ReferenceEquals( defaultValue, null ) )
					{
						 value1 = !string.ReferenceEquals( value1, null ) ? value1 : defaultValue;
						 value2 = !string.ReferenceEquals( value2, null ) ? value2 : defaultValue;
						 return value1.Equals( value2 );
					}
			  }
			  else
			  {
					return value1.Equals( value2 );
			  }
			  return false;
		 }

		 public override TransactionApplier NewApplier( bool recovery )
		 {
			  return new LuceneCommandApplier( _dataSource, recovery );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.graphdb.ResourceIterator<java.io.File> listStoreFiles() throws java.io.IOException
		 public override ResourceIterator<File> ListStoreFiles()
		 {
			  return _dataSource.listStoreFiles();
		 }

		 public override void Force()
		 {
			  _dataSource.force();
		 }
	}

}