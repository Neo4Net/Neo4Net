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

	using CharReadable = Org.Neo4j.Csv.Reader.CharReadable;
	using CharSeeker = Org.Neo4j.Csv.Reader.CharSeeker;
	using Chunker = Org.Neo4j.Csv.Reader.Chunker;
	using Extractors = Org.Neo4j.Csv.Reader.Extractors;
	using Source_Chunk = Org.Neo4j.Csv.Reader.Source_Chunk;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.csv.reader.CharSeekers.charSeeker;

	/// <summary>
	/// <seealso cref="Chunker"/> which parses a chunk of entities when calling <seealso cref="nextChunk(Chunk)"/>,
	/// injecting them into <seealso cref="EagerCsvInputChunk"/>, which simply hands them out one by one.
	/// </summary>
	public class EagerParserChunker : Chunker
	{
		 private readonly CharSeeker _seeker;
		 private readonly CsvInputParser _parser;
		 private readonly int _chunkSize;
		 private readonly Decorator _decorator;

		 public EagerParserChunker( CharReadable reader, IdType idType, Header header, Collector badCollector, Extractors extractors, int chunkSize, Configuration config, Decorator decorator )
		 {
			  this._chunkSize = chunkSize;
			  this._decorator = decorator;
			  this._seeker = charSeeker( reader, config, true );
			  this._parser = new CsvInputParser( _seeker, config.delimiter(), idType, header, badCollector, extractors );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean nextChunk(org.neo4j.csv.reader.Source_Chunk chunk) throws java.io.IOException
		 public override bool NextChunk( Source_Chunk chunk )
		 {
			  InputEntityArray entities = new InputEntityArray( _chunkSize );
			  InputEntityVisitor decorated = _decorator.apply( entities );
			  int cursor = 0;
			  for ( ; cursor < _chunkSize && _parser.next( decorated ); cursor++ )
			  { // just loop through and parse
			  }

			  if ( cursor > 0 )
			  {
					( ( EagerCsvInputChunk )chunk ).Initialize( entities.ToArray() );
					return true;
			  }
			  return false;
		 }

		 public override long Position()
		 {
			  return _seeker.position();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  _parser.Dispose();
		 }

		 public override Source_Chunk NewChunk()
		 {
			  throw new System.NotSupportedException();
		 }
	}

}