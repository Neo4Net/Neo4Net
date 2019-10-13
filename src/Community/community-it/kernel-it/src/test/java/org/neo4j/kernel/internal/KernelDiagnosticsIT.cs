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
namespace Neo4Net.Kernel.@internal
{
	using MutableLong = org.apache.commons.lang3.mutable.MutableLong;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Label = Neo4Net.Graphdb.Label;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using Format = Neo4Net.Helpers.Format;
	using DatabaseFile = Neo4Net.Io.layout.DatabaseFile;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using StoreType = Neo4Net.Kernel.impl.store.StoreType;
	using Logger = Neo4Net.Logging.Logger;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class KernelDiagnosticsIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory directory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory Directory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIncludeNativeIndexFilesInTotalMappedSize()
		 public virtual void ShouldIncludeNativeIndexFilesInTotalMappedSize()
		 {
			  int i = 0;
			  foreach ( GraphDatabaseSettings.SchemaIndex schemaIndex in GraphDatabaseSettings.SchemaIndex.values() )
			  {
					// given
					File dbDir = new File( Directory.databaseDir(), (i++).ToString() );
					CreateIndexInIsolatedDbInstance( dbDir, schemaIndex );

					// when
					KernelDiagnostics.StoreFiles files = new KernelDiagnostics.StoreFiles( DatabaseLayout.of( dbDir ) );
					SizeCapture capture = new SizeCapture( this );
					Files.dump( capture );
					assertNotNull( capture.Size );

					// then
					long expected = ManuallyCountTotalMappedFileSize( dbDir );
					assertEquals( Format.bytes( expected ), capture.Size );
			  }
		 }

		 private void CreateIndexInIsolatedDbInstance( File storeDir, GraphDatabaseSettings.SchemaIndex index )
		 {
			  GraphDatabaseService db = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(storeDir).setConfig(GraphDatabaseSettings.default_schema_provider, index.providerName()).newGraphDatabase();
			  try
			  {
					Label label = Label.label( "Label-" + index.providerName() );
					string key = "key";
					using ( Transaction tx = Db.beginTx() )
					{
						 for ( int i = 0; i < 100; i++ )
						 {
							  Db.createNode( label ).setProperty( key, i );
						 }
						 tx.Success();
					}
					using ( Transaction tx = Db.beginTx() )
					{
						 Db.schema().indexFor(label).on(key).create();
						 tx.Success();
					}
					using ( Transaction tx = Db.beginTx() )
					{
						 Db.schema().awaitIndexesOnline(1, MINUTES);
						 tx.Success();
					}
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

		 private long ManuallyCountTotalMappedFileSize( File dbDir )
		 {
			  MutableLong result = new MutableLong();
			  NativeIndexFileFilter nativeIndexFilter = new NativeIndexFileFilter( dbDir );
			  ManuallyCountTotalMappedFileSize( dbDir, result, nativeIndexFilter );
			  return result.Value;
		 }

		 private void ManuallyCountTotalMappedFileSize( File dir, MutableLong result, NativeIndexFileFilter nativeIndexFilter )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  ISet<string> storeFiles = Stream.of( StoreType.values() ).filter(StoreType::isRecordStore).map(type => type.DatabaseFile.Name).collect(Collectors.toSet());
			  foreach ( File file in dir.listFiles() )
			  {
					if ( file.Directory )
					{
						 ManuallyCountTotalMappedFileSize( file, result, nativeIndexFilter );
					}
					else if ( storeFiles.Contains( file.Name ) || file.Name.Equals( DatabaseFile.LABEL_SCAN_STORE.Name ) || nativeIndexFilter.Accept( file ) )
					{
						 result.add( file.length() );
					}
			  }
		 }

		 private class SizeCapture : Logger
		 {
			 private readonly KernelDiagnosticsIT _outerInstance;

			 public SizeCapture( KernelDiagnosticsIT outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal string Size;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void log(@Nonnull String message)
			  public override void Log( string message )
			  {
					if ( message.Contains( "Total size of mapped files" ) )
					{
						 int beginPos = message.LastIndexOf( ": ", StringComparison.Ordinal );
						 assertTrue( beginPos != -1 );
						 Size = message.Substring( beginPos + 2 );
					}
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void log(@Nonnull String message, @Nonnull Throwable throwable)
			  public override void Log( string message, Exception throwable )
			  {
					throw new System.NotSupportedException();
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void log(@Nonnull String format, @Nullable Object... arguments)
			  public override void Log( string format, params object[] arguments )
			  {
					Log( format( format, arguments ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void bulk(@Nonnull Consumer<org.neo4j.logging.Logger> consumer)
			  public override void Bulk( Consumer<Logger> consumer )
			  {
					throw new System.NotSupportedException();
			  }
		 }
	}

}