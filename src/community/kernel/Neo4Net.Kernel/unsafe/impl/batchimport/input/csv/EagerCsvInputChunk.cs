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

	using Chunker = Neo4Net.Csv.Reader.Chunker;
	using Source = Neo4Net.Csv.Reader.Source;

	internal class EagerCsvInputChunk : CsvInputChunk, Neo4Net.Csv.Reader.Source_Chunk
	{
		 private InputEntity[] _entities;
		 private int _cursor;

		 internal virtual void Initialize( InputEntity[] entities )
		 {
			  this._entities = entities;
			  this._cursor = 0;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean next(org.neo4j.unsafe.impl.batchimport.input.InputEntityVisitor visitor) throws java.io.IOException
		 public override bool Next( InputEntityVisitor visitor )
		 {
			  if ( _cursor < _entities.Length )
			  {
					_entities[_cursor++].replayOnto( visitor );
					return true;
			  }
			  return false;
		 }

		 public override void Close()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean fillFrom(org.neo4j.csv.reader.Chunker chunker) throws java.io.IOException
		 public override bool FillFrom( Chunker chunker )
		 {
			  return chunker.NextChunk( this );
		 }

		 public override char[] Data()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override int Length()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override int MaxFieldSize()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override string SourceDescription()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override int StartPosition()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override int BackPosition()
		 {
			  throw new System.NotSupportedException();
		 }
	}

}