﻿using System.Diagnostics;
using System.IO;
using System.Threading;

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
namespace Neo4Net.Tooling
{

	using BatchImporter = Neo4Net.@unsafe.Impl.Batchimport.BatchImporter;
	using InputIterator = Neo4Net.@unsafe.Impl.Batchimport.InputIterator;
	using Input = Neo4Net.@unsafe.Impl.Batchimport.input.Input;
	using InputChunk = Neo4Net.@unsafe.Impl.Batchimport.input.InputChunk;
	using InputEntity = Neo4Net.@unsafe.Impl.Batchimport.input.InputEntity;
	using RandomEntityDataGenerator = Neo4Net.@unsafe.Impl.Batchimport.input.RandomEntityDataGenerator;
	using Configuration = Neo4Net.@unsafe.Impl.Batchimport.input.csv.Configuration;
	using Neo4Net.@unsafe.Impl.Batchimport.input.csv;
	using Header = Neo4Net.@unsafe.Impl.Batchimport.input.csv.Header;
	using StringDeserialization = Neo4Net.@unsafe.Impl.Batchimport.input.csv.StringDeserialization;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.ByteUnit.mebiBytes;

	public class CsvOutput : BatchImporter
	{
		 private interface Deserializer
		 {
			  string Apply( InputEntity IEntity, Deserialization<string> deserialization, Header header );
		 }

		 private readonly File _targetDirectory;
		 private readonly Header _nodeHeader;
		 private readonly Header _relationshipHeader;
		 private Configuration _config;
		 private readonly Deserialization<string> _deserialization;

		 public CsvOutput( File targetDirectory, Header nodeHeader, Header relationshipHeader, Configuration config )
		 {
			  this._targetDirectory = targetDirectory;
			  Debug.Assert( targetDirectory.Directory );
			  this._nodeHeader = nodeHeader;
			  this._relationshipHeader = relationshipHeader;
			  this._config = config;
			  this._deserialization = new StringDeserialization( config );
			  targetDirectory.mkdirs();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void doImport(org.Neo4Net.unsafe.impl.batchimport.input.Input input) throws java.io.IOException
		 public override void DoImport( Input input )
		 {
			  Consume( "nodes", input.Nodes().GetEnumerator(), _nodeHeader, RandomEntityDataGenerator.convert );
			  Consume( "relationships", input.Relationships().GetEnumerator(), _relationshipHeader, RandomEntityDataGenerator.convert );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void consume(String name, org.Neo4Net.unsafe.impl.batchimport.InputIterator entities, org.Neo4Net.unsafe.impl.batchimport.input.csv.Header header, Deserializer deserializer) throws java.io.IOException
		 private void Consume( string name, InputIterator entities, Header header, Deserializer deserializer )
		 {
			  using ( PrintStream @out = File( name + "header.csv" ) )
			  {
					Serialize( @out, header );
			  }

			  try
			  {
					int threads = Runtime.Runtime.availableProcessors();
					ExecutorService executor = Executors.newFixedThreadPool( threads );
					for ( int i = 0; i < threads; i++ )
					{
						 int id = i;
						 executor.submit((Callable<Void>)() =>
						 {
						 StringDeserialization deserialization = new StringDeserialization( _config );
						 using ( PrintStream @out = File( name + "-" + id + ".csv" ), InputChunk chunk = entities.NewChunk() )
						 {
							 InputEntity IEntity = new InputEntity();
							 while ( entities.Next( chunk ) )
							 {
								 while ( chunk.next( IEntity ) )
								 {
									 @out.println( deserializer.Apply( IEntity, deserialization, header ) );
								 }
							 }
						 }
						 return null;
						 });
					}
					executor.shutdown();
					executor.awaitTermination( 10, TimeUnit.MINUTES );
			  }
			  catch ( InterruptedException e )
			  {
					Thread.CurrentThread.Interrupt();
					throw new IOException( e );
			  }
		 }

		 private void Serialize( PrintStream @out, Header header )
		 {
			  _deserialization.clear();
			  foreach ( Header.Entry entry in header.Entries() )
			  {
					_deserialization.handle( entry, entry.ToString() );
			  }
			  @out.println( _deserialization.materialize() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.PrintStream file(String name) throws java.io.IOException
		 private PrintStream File( string name )
		 {
			  return new PrintStream( new BufferedOutputStream( new FileStream( _targetDirectory, name, FileMode.Create, FileAccess.Write ), ( int ) mebiBytes( 1 ) ) );
		 }
	}

}