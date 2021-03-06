﻿/*
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
namespace Org.Neo4j.@unsafe.Impl.Batchimport.input.csv
{

	using BufferedCharSeeker = Org.Neo4j.Csv.Reader.BufferedCharSeeker;
	using CharReadable = Org.Neo4j.Csv.Reader.CharReadable;
	using ChunkImpl = Org.Neo4j.Csv.Reader.CharReadableChunker.ChunkImpl;
	using CharSeeker = Org.Neo4j.Csv.Reader.CharSeeker;
	using Chunker = Org.Neo4j.Csv.Reader.Chunker;
	using ClosestNewLineChunker = Org.Neo4j.Csv.Reader.ClosestNewLineChunker;
	using Extractors = Org.Neo4j.Csv.Reader.Extractors;
	using Readables = Org.Neo4j.Csv.Reader.Readables;
	using Source = Org.Neo4j.Csv.Reader.Source;
	using Source_Chunk = Org.Neo4j.Csv.Reader.Source_Chunk;
	using SourceTraceability = Org.Neo4j.Csv.Reader.SourceTraceability;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.copyOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.input.csv.CsvGroupInputIterator.extractors;

	/// <summary>
	/// Iterates over one stream of data, where all data items conform to the same <seealso cref="Header"/>.
	/// Typically created from <seealso cref="CsvGroupInputIterator"/>.
	/// </summary>
	internal class CsvInputIterator : SourceTraceability, System.IDisposable
	{
		 private readonly CharReadable _stream;
		 private readonly Chunker _chunker;
		 private readonly int _groupId;
		 private readonly Decorator _decorator;
		 private readonly System.Func<CsvInputChunk> _realInputChunkSupplier;

		 internal CsvInputIterator( CharReadable stream, Decorator decorator, Header header, Configuration config, IdType idType, Collector badCollector, Extractors extractors, int groupId )
		 {
			  this._stream = stream;
			  this._decorator = decorator;
			  this._groupId = groupId;
			  if ( config.multilineFields() )
			  {
					// If we're expecting multi-line fields then there's no way to arbitrarily chunk the underlying data source
					// and find record delimiters with certainty. This is why we opt for a chunker that does parsing inside
					// the call that normally just hands out an arbitrary amount of characters to parse outside and in parallel.
					// This chunker is single-threaded, as it was previously too and keeps the functionality of multi-line fields.
					this._chunker = new EagerParserChunker( stream, idType, header, badCollector, extractors, 1_000, config, decorator );
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
					this._realInputChunkSupplier = EagerCsvInputChunk::new;
			  }
			  else
			  {
					this._chunker = new ClosestNewLineChunker( stream, config.bufferSize() );
					this._realInputChunkSupplier = () => new LazyCsvInputChunk(idType, config.delimiter(), badCollector, extractors(config), _chunker.newChunk(), config, decorator, header);
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: CsvInputIterator(org.neo4j.csv.reader.CharReadable stream, Decorator decorator, Header.Factory headerFactory, IdType idType, Configuration config, org.neo4j.unsafe.impl.batchimport.input.Groups groups, org.neo4j.unsafe.impl.batchimport.input.Collector badCollector, org.neo4j.csv.reader.Extractors extractors, int groupId) throws java.io.IOException
		 internal CsvInputIterator( CharReadable stream, Decorator decorator, Header.Factory headerFactory, IdType idType, Configuration config, Groups groups, Collector badCollector, Extractors extractors, int groupId ) : this( stream, decorator, ExtractHeader( stream, headerFactory, idType, config, groups ), config, idType, badCollector, extractors, groupId )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static Header extractHeader(org.neo4j.csv.reader.CharReadable stream, Header.Factory headerFactory, IdType idType, Configuration config, org.neo4j.unsafe.impl.batchimport.input.Groups groups) throws java.io.IOException
		 internal static Header ExtractHeader( CharReadable stream, Header.Factory headerFactory, IdType idType, Configuration config, Groups groups )
		 {
			  if ( !headerFactory.Defined )
			  {
					char[] firstLineBuffer = Readables.extractFirstLineFrom( stream );
					// make the chunk slightly bigger than the header to not have the seeker think that it's reading
					// a value bigger than its max buffer size
					ChunkImpl firstChunk = new ChunkImpl( copyOf( firstLineBuffer, firstLineBuffer.Length + 1 ) );
					firstChunk.Initialize( firstLineBuffer.Length, stream.SourceDescription() );
					CharSeeker firstSeeker = Seeker( firstChunk, config );
					return headerFactory.Create( firstSeeker, config, idType, groups );
			  }

			  return headerFactory.Create( null, null, null, null );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean next(CsvInputChunkProxy proxy) throws java.io.IOException
		 public virtual bool Next( CsvInputChunkProxy proxy )
		 {
			  proxy.EnsureInstantiated( _realInputChunkSupplier, _groupId );
			  return proxy.FillFrom( _chunker );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  _chunker.Dispose();
			  _decorator.close();
		 }

		 public override string SourceDescription()
		 {
			  return _stream.sourceDescription();
		 }

		 public override long Position()
		 {
			  return _chunker.position();
		 }

		 internal static CharSeeker Seeker( Source_Chunk chunk, Configuration config )
		 {
			  return new BufferedCharSeeker( Source.singleChunk( chunk ), config );
		 }
	}

}