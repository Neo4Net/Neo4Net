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
namespace Neo4Net.@unsafe.Impl.Batchimport.input.csv
{

	using Extractors = Neo4Net.Csv.Reader.Extractors;
	using MultiReadable = Neo4Net.Csv.Reader.MultiReadable;

	/// <summary>
	/// Iterates over groups of input data, each group containing one or more input files. A whole group conforms has each its own header.
	/// </summary>
	public class CsvGroupInputIterator : InputIterator
	{
		 private readonly IEnumerator<DataFactory> _source;
		 private readonly Header.Factory _headerFactory;
		 private readonly IdType _idType;
		 private readonly Configuration _config;
		 private readonly Collector _badCollector;
		 private readonly Groups _groups;
		 private CsvInputIterator _current;
		 private int _groupId;

		 public CsvGroupInputIterator( IEnumerator<DataFactory> source, Header.Factory headerFactory, IdType idType, Configuration config, Collector badCollector, Groups groups )
		 {
			  this._source = source;
			  this._headerFactory = headerFactory;
			  this._idType = idType;
			  this._config = config;
			  this._badCollector = badCollector;
			  this._groups = groups;
		 }

		 public override CsvInputChunkProxy NewChunk()
		 {
			  return new CsvInputChunkProxy();
		 }

		 internal static Extractors Extractors( Configuration config )
		 {
			  return new Extractors( config.arrayDelimiter(), config.emptyQuotedStringsAsNull() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized boolean next(org.Neo4Net.unsafe.impl.batchimport.input.InputChunk chunk) throws java.io.IOException
		 public override bool Next( InputChunk chunk )
		 {
			 lock ( this )
			 {
				  while ( true )
				  {
						if ( _current == null )
						{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
							 if ( !_source.hasNext() )
							 {
								  return false;
							 }
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
							 Data data = _source.next().create(_config);
							 _current = new CsvInputIterator( new MultiReadable( data.Stream() ), data.Decorator(), _headerFactory, _idType, _config, _groups, _badCollector, Extractors(_config), _groupId++ );
						}
      
						if ( _current.next( ( CsvInputChunkProxy ) chunk ) )
						{
							 return true;
						}
						_current.Dispose();
						_current = null;
				  }
			 }
		 }

		 public override void Close()
		 {
			  try
			  {
					if ( _current != null )
					{
						 _current.Dispose();
					}
					_current = null;
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }
	}

}