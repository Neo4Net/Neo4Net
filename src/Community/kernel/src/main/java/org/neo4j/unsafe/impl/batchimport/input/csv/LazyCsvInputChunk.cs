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
namespace Neo4Net.@unsafe.Impl.Batchimport.input.csv
{

	using Chunker = Neo4Net.Csv.Reader.Chunker;
	using Extractors = Neo4Net.Csv.Reader.Extractors;
	using Source_Chunk = Neo4Net.Csv.Reader.Source_Chunk;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.input.csv.CsvInputIterator.seeker;

	/// <summary>
	/// <seealso cref="InputChunk"/> parsing next entry on each call to <seealso cref="next(InputEntityVisitor)"/>.
	/// </summary>
	public class LazyCsvInputChunk : CsvInputChunk
	{
		 private readonly IdType _idType;
		 private readonly int _delimiter;
		 private readonly Collector _badCollector;
		 private readonly Source_Chunk _processingChunk;
		 private readonly Configuration _config;
		 private readonly Decorator _decorator;
		 private readonly Header _header;
		 private readonly Extractors _extractors;

		 // Set in #fillFrom
		 private CsvInputParser _parser;

		 // Set as #next is called
		 private InputEntityVisitor _previousVisitor;
		 private InputEntityVisitor _visitor;

		 public LazyCsvInputChunk( IdType idType, int delimiter, Collector badCollector, Extractors extractors, Source_Chunk processingChunk, Configuration config, Decorator decorator, Header header )
		 {
			  this._idType = idType;
			  this._badCollector = badCollector;
			  this._extractors = extractors;
			  this._delimiter = delimiter;
			  this._processingChunk = processingChunk;
			  this._config = config;
			  this._decorator = decorator;
			  this._header = header;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean fillFrom(org.neo4j.csv.reader.Chunker chunker) throws java.io.IOException
		 public override bool FillFrom( Chunker chunker )
		 {
			  if ( chunker.NextChunk( _processingChunk ) )
			  {
					CloseCurrentParser();
					this._visitor = null;
					this._parser = new CsvInputParser( seeker( _processingChunk, _config ), _delimiter, _idType, _header.clone(), _badCollector, _extractors );
					return _header.entries().Length != 0;
			  }
			  return false;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void closeCurrentParser() throws java.io.IOException
		 private void CloseCurrentParser()
		 {
			  if ( _parser != null )
			  {
					_parser.Dispose();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean next(org.neo4j.unsafe.impl.batchimport.input.InputEntityVisitor nakedVisitor) throws java.io.IOException
		 public override bool Next( InputEntityVisitor nakedVisitor )
		 {
			  if ( _visitor == null || nakedVisitor != _previousVisitor )
			  {
					DecorateVisitor( nakedVisitor );
			  }

			  return _parser.next( _visitor );
		 }

		 private void DecorateVisitor( InputEntityVisitor nakedVisitor )
		 {
			  _visitor = _decorator.apply( nakedVisitor );
			  _previousVisitor = nakedVisitor;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  CloseCurrentParser();
		 }
	}

}