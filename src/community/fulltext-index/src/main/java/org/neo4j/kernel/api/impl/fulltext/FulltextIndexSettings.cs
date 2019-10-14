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
namespace Neo4Net.Kernel.Api.Impl.Fulltext
{
	using Analyzer = org.apache.lucene.analysis.Analyzer;


	using AnalyzerProvider = Neo4Net.Graphdb.index.fulltext.AnalyzerProvider;
	using PropertyKeyIdNotFoundKernelException = Neo4Net.Internal.Kernel.Api.exceptions.PropertyKeyIdNotFoundKernelException;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using TokenHolder = Neo4Net.Kernel.impl.core.TokenHolder;
	using TokenNotFoundException = Neo4Net.Kernel.impl.core.TokenNotFoundException;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;

	public class FulltextIndexSettings
	{
		 public const string INDEX_CONFIG_ANALYZER = "analyzer";
		 public const string INDEX_CONFIG_EVENTUALLY_CONSISTENT = "eventually_consistent";
		 private const string INDEX_CONFIG_FILE = "fulltext-index.properties";
		 private const string INDEX_CONFIG_PROPERTY_NAMES = "propertyNames";

		 internal static FulltextIndexDescriptor ReadOrInitializeDescriptor( StoreIndexDescriptor descriptor, string defaultAnalyzerName, TokenHolder propertyKeyTokenHolder, File indexFolder, FileSystemAbstraction fileSystem )
		 {
			  Properties indexConfiguration = new Properties();
			  if ( descriptor.Schema() is FulltextSchemaDescriptor )
			  {
					FulltextSchemaDescriptor schema = ( FulltextSchemaDescriptor ) descriptor.Schema();
					indexConfiguration.putAll( Schema.IndexConfiguration );
			  }
			  LoadPersistedSettings( indexConfiguration, indexFolder, fileSystem );
			  bool eventuallyConsistent = bool.Parse( indexConfiguration.getProperty( INDEX_CONFIG_EVENTUALLY_CONSISTENT ) );
			  string analyzerName = indexConfiguration.getProperty( INDEX_CONFIG_ANALYZER, defaultAnalyzerName );
			  Analyzer analyzer = CreateAnalyzer( analyzerName );
			  IList<string> names = new List<string>();
			  foreach ( int propertyKeyId in descriptor.Schema().PropertyIds )
			  {
					try
					{
						 names.Add( propertyKeyTokenHolder.GetTokenById( propertyKeyId ).name() );
					}
					catch ( TokenNotFoundException e )
					{
						 throw new System.InvalidOperationException( "Property key id not found.", new PropertyKeyIdNotFoundKernelException( propertyKeyId, e ) );
					}
			  }
			  IList<string> propertyNames = Collections.unmodifiableList( names );
			  return new FulltextIndexDescriptor( descriptor, propertyNames, analyzer, analyzerName, eventuallyConsistent );
		 }

		 private static void LoadPersistedSettings( Properties settings, File indexFolder, FileSystemAbstraction fs )
		 {
			  File settingsFile = new File( indexFolder, INDEX_CONFIG_FILE );
			  if ( fs.FileExists( settingsFile ) )
			  {
					try
					{
							using ( Reader reader = fs.OpenAsReader( settingsFile, StandardCharsets.UTF_8 ) )
							{
							 settings.load( reader );
							}
					}
					catch ( IOException e )
					{
						 throw new UncheckedIOException( "Failed to read persisted fulltext index properties: " + settingsFile, e );
					}
			  }
		 }

		 private static Analyzer CreateAnalyzer( string analyzerName )
		 {
			  try
			  {
					AnalyzerProvider provider = AnalyzerProvider.getProviderByName( analyzerName );
					return provider.CreateAnalyzer();
			  }
			  catch ( Exception e )
			  {
					throw new Exception( "Could not create fulltext analyzer: " + analyzerName, e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static void saveFulltextIndexSettings(FulltextIndexDescriptor descriptor, java.io.File indexFolder, org.neo4j.io.fs.FileSystemAbstraction fs) throws java.io.IOException
		 internal static void SaveFulltextIndexSettings( FulltextIndexDescriptor descriptor, File indexFolder, FileSystemAbstraction fs )
		 {
			  File indexConfigFile = new File( indexFolder, INDEX_CONFIG_FILE );
			  Properties settings = new Properties();
			  settings.setProperty( INDEX_CONFIG_EVENTUALLY_CONSISTENT, Convert.ToString( descriptor.EventuallyConsistent ) );
			  settings.setProperty( INDEX_CONFIG_ANALYZER, descriptor.AnalyzerName() );
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  settings.setProperty( INDEX_CONFIG_PROPERTY_NAMES, descriptor.PropertyNames().collect(Collectors.joining(", ", "[", "]")) );
			  settings.setProperty( "_propertyIds", Arrays.ToString( descriptor.Properties() ) );
			  settings.setProperty( "_name", descriptor.Name() );
			  settings.setProperty( "_schema_entityType", descriptor.Schema().entityType().name() );
			  settings.setProperty( "_schema_entityTokenIds", Arrays.ToString( descriptor.Schema().EntityTokenIds ) );
			  using ( StoreChannel channel = fs.Create( indexConfigFile ), Writer writer = fs.OpenAsWriter( indexConfigFile, StandardCharsets.UTF_8, false ) )
			  {
					settings.store( writer, "Auto-generated file. Do not modify!" );
					writer.flush();
					channel.Force( true );
			  }
		 }
	}

}